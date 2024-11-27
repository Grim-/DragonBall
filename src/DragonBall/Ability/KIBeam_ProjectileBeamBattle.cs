
using RimWorld;
using SaiyanMod;
using System.Collections.Generic;
using System.Linq;
using TaranMagicFramework;
using UnityEngine;
using Verse;

namespace DragonBall
{
    public class KIBeam_ProjectileBeamBattle : KIBeam_Projectile
    {
        private static HashSet<KIBeam_Projectile> ActiveBeams = new HashSet<KIBeam_Projectile>();
        private bool isInBeamBattle = false;
        private KIBeam_Projectile battleOpponent = null;
        private Vector3 lockedPosition;
        private Vector3 originalClashPoint;
        private float battleAdvantage = 0f;
        private const float ADVANTAGE_CHANGE_RATE = 0.1f;
        private const float MINIMUM_SAFE_DISTANCE = 2f;
        private Mote_Animation ClashAnimation = null;
        private Vector3 targetPosition;
        private Vector3 currentPosition;
        private Vector3 currentClashPosition;
        private Vector3 targetClashPosition;
        private const float BEAM_MOVE_SPEED = 0.25f;
        private const float CLASH_MOVE_SPEED = 0.15f;
        private const float BATTLE_MOMENTUM = 0.85f;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            ActiveBeams.Add(this);
            currentPosition = this.DrawPos;
            targetPosition = currentPosition;
            currentClashPosition = currentPosition;
            targetClashPosition = currentPosition;
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            ActiveBeams.Remove(this);
            CleanupClashAnimation();
            base.DeSpawn(mode);
        }

        private void CleanupClashAnimation()
        {
            if (ClashAnimation != null && !ClashAnimation.Destroyed)
            {
                ClashAnimation.expireInTick = 0;
                ClashAnimation.Destroy();
                ClashAnimation = null;
            }
        }

        public override void Tick()
        {
            if (!this.Spawned || base.Map == null || this.Position == null || this.ability?.pawn == null)
            {
                return;
            }

            try
            {
                if (!isInBeamBattle)
                {
                    CheckBeamCollisions();
                    if (this.Spawned && this.Map != null)
                    {
                        try
                        {
                            base.Tick();
                        }
                        catch (System.Exception e)
                        {
                            Log.Error($"Error in base.Tick(): {e.Message}");
                        }
                    }
                }
                else
                {
                    HandleBeamBattle();
                    UpdatePosition();
                    UpdateClashAnimationPosition();
                }
            }
            catch (System.Exception ex)
            {
                Log.Error($"Error in KIBeam_ProjectileBeamBattle.Tick(): {ex.Message}\n{ex.StackTrace}");
                if (!this.Destroyed) this.Destroy();
            }
        }
        private void UpdateClashAnimationPosition()
        {
            if (ClashAnimation != null && ClashAnimation.Spawned && battleOpponent != null)
            {
                Vector3 myPawnPos = this.ability.pawn.DrawPos;
                Vector3 theirPawnPos = battleOpponent.ability.pawn.DrawPos;
                Vector3 battleDirection = (theirPawnPos - myPawnPos).normalized;
                float totalDistance = Vector3.Distance(myPawnPos, theirPawnPos);
                float availableBattleSpace = totalDistance - (MINIMUM_SAFE_DISTANCE * 2);
                Vector3 battleStartPoint = myPawnPos + (battleDirection * MINIMUM_SAFE_DISTANCE);
                float pushDistance = ((battleAdvantage + 1f) / 2f) * availableBattleSpace;

                targetClashPosition = battleStartPoint + (battleDirection * pushDistance);
                currentClashPosition = Vector3.Lerp(currentClashPosition, targetClashPosition, CLASH_MOVE_SPEED);

                ClashAnimation.exactPosition = currentClashPosition;
            }
        }
        private void UpdatePosition()
        {
            if (!this.Spawned || this.Map == null || battleOpponent == null)
            {
                return;
            }

            currentPosition = Vector3.Lerp(currentPosition, targetPosition, BEAM_MOVE_SPEED);
            IntVec3 newPos = currentPosition.ToIntVec3();

            // Verify the new position is valid before setting it
            if (newPos.InBounds(this.Map))
            {
                this.Position = newPos;

                if (ClashAnimation != null && !ClashAnimation.Destroyed)
                {
                    ClashAnimation.Position = newPos;
                }
            }
        }

        private void HandleBeamBattle()
        {
            if (ability?.pawn == null || !this.ability.pawn.TryGetKiAbilityClass(out AbilityClassKI kiClass))
            {
                return;
            }

            if (this.ability.pawn.IsHashIntervalTick(60))
            {
                kiClass.abilityResource.energy -= 10f;

                if (kiClass.abilityResource.energy <= 0 || battleOpponent == null)
                {
                    LoseBeamBattle();
                    return;
                }

                UpdateBattleAdvantage(kiClass);
                CalculateTargetPosition();
            }

            if (Find.TickManager.TicksGame % 30 == 0)
            {
                CreateBeamBattleEffects();
            }
        }

        private void UpdateBattleAdvantage(AbilityClassKI kiClass)
        {
            if (battleOpponent == null || kiClass == null)
            {
                return;
            }

            float myKiPercent = kiClass.abilityResource.energy / kiClass.abilityResource.MaxEnergy;
            float theirKiPercent = 0f;

            if (battleOpponent.ability?.pawn != null &&
                battleOpponent.ability.pawn.TryGetKiAbilityClass(out AbilityClassKI theirKiClass))
            {
                theirKiPercent = theirKiClass.abilityResource.energy / theirKiClass.abilityResource.MaxEnergy;
            }

            float previousAdvantage = battleAdvantage;
            float newAdvantageChange = (myKiPercent - theirKiPercent) * ADVANTAGE_CHANGE_RATE;
            battleAdvantage = (previousAdvantage * BATTLE_MOMENTUM) + (newAdvantageChange * (1 - BATTLE_MOMENTUM));
            battleAdvantage = Mathf.Clamp(battleAdvantage, -1f, 1f);

            if (Rand.Value < 0.3f)
            {
                battleAdvantage += Rand.Range(-0.1f, 0.1f);
                battleAdvantage = Mathf.Clamp(battleAdvantage, -1f, 1f);
            }
        }

        private void CalculateTargetPosition()
        {
            if (battleOpponent == null || ability?.pawn == null || battleOpponent.ability?.pawn == null)
            {
                return;
            }

            Vector3 myPawnPos = this.ability.pawn.DrawPos;
            Vector3 theirPawnPos = battleOpponent.ability.pawn.DrawPos;

            float totalDistance = Vector3.Distance(myPawnPos, theirPawnPos);
            float availableBattleSpace = totalDistance - (MINIMUM_SAFE_DISTANCE * 2);

            if (availableBattleSpace <= 0)
            {
                LoseBeamBattle();
                return;
            }

            Vector3 battleDirection = (theirPawnPos - myPawnPos).normalized;
            Vector3 battleStartPoint = myPawnPos + (battleDirection * MINIMUM_SAFE_DISTANCE);
            float pushDistance = ((battleAdvantage + 1f) / 2f) * availableBattleSpace;

            targetPosition = battleStartPoint + (battleDirection * pushDistance);

            if (battleOpponent is KIBeam_ProjectileBeamBattle battleBeam)
            {
                Vector3 opponentOffset = battleBeam.DrawPos - originalClashPoint;
                battleBeam.targetPosition = targetPosition + opponentOffset;
            }
        }

        private void CheckBeamCollisions()
        {
            if (!this.Spawned || this.Map == null)
            {
                return;
            }

            foreach (var otherBeam in ActiveBeams.ToList())
            {
                if (otherBeam == null || otherBeam == this || otherBeam.Launcher == this.launcher)
                    continue;

                try
                {
                    var myCells = GenRadial.RadialCellsAround(this.Position, 1f, true).ToList();
                    var theirCells = GenRadial.RadialCellsAround(otherBeam.Position, 1f, true).ToList();

                    if (myCells.Intersect(theirCells).Any())
                    {
                        StartBeamBattle(otherBeam);
                        break;
                    }
                }
                catch (System.Exception ex)
                {
                    Log.Error($"Error in CheckBeamCollisions: {ex.Message}");
                }
            }
        }
        public Mote_Animation MakeAnimation(TaranMagicFramework.AnimationDef overlay)
        {
            Mote_Animation mote_Animation = ThingMaker.MakeThing(overlay) as Mote_Animation;
            mote_Animation.sourceAbility = this.ability;
            GenSpawn.Spawn(mote_Animation, this.Position, this.ability.pawn.Map);
            return mote_Animation;
        }
        private void StartBeamBattle(KIBeam_Projectile otherBeam)
        {
            if (otherBeam == null || ability?.pawn == null || otherBeam.ability?.pawn == null)
            {
                return;
            }

            float pawnDistance = Vector3.Distance(this.ability.pawn.DrawPos, otherBeam.ability.pawn.DrawPos);
            if (pawnDistance <= (MINIMUM_SAFE_DISTANCE * 2))
            {
                return;
            }

            IntVec3 otherTravelDirection = IntVec3.East;
            if (otherBeam is KIBeam_ProjectileBeamBattle otherbattleBeam)
            {
                otherTravelDirection = otherBeam.Position - otherbattleBeam.destination.ToIntVec3();
            }
            IntVec3 thisTravelDirection = this.Position - this.destination.ToIntVec3();

            float dotProduct = (thisTravelDirection.x * otherTravelDirection.x + thisTravelDirection.z * otherTravelDirection.z) /
                              (Mathf.Sqrt(thisTravelDirection.x * thisTravelDirection.x + thisTravelDirection.z * thisTravelDirection.z) *
                               Mathf.Sqrt(otherTravelDirection.x * otherTravelDirection.x + otherTravelDirection.z * otherTravelDirection.z));

            if (dotProduct > -0.3f)
            {
                return;
            }

            isInBeamBattle = true;
            lockedPosition = this.DrawPos;
            originalClashPoint = GetCollisionPoint(otherBeam).ToVector3();
            battleAdvantage = 0f;
            currentClashPosition = originalClashPoint;
            targetClashPosition = originalClashPoint;

            if (otherBeam is KIBeam_ProjectileBeamBattle battleBeam)
            {
                battleBeam.isInBeamBattle = true;
                battleBeam.lockedPosition = otherBeam.DrawPos;
                battleBeam.battleOpponent = this;
                battleBeam.originalClashPoint = originalClashPoint;
                battleBeam.battleAdvantage = 0f;
                battleBeam.currentClashPosition = originalClashPoint;
                battleBeam.targetClashPosition = originalClashPoint;
            }

            battleOpponent = otherBeam;

            if (ClashAnimation == null && this.Map != null)
            {
                ClashAnimation = MakeAnimation(DBDefOf.BeamClashMeetingPoint);
                if (ClashAnimation != null)
                {
                    ClashAnimation.Position = originalClashPoint.ToIntVec3();
                    ClashAnimation.exactPosition = originalClashPoint;
                }
            }

            CreateBeamBattleEffects();
        }

        private void LoseBeamBattle()
        {
            CleanupClashAnimation();

            if (battleOpponent is KIBeam_ProjectileBeamBattle battleBeam)
            {
                battleBeam.isInBeamBattle = false;
                battleBeam.battleOpponent = null;
            }

            this.ability?.End();
            if (!this.Destroyed) this.Destroy(DestroyMode.KillFinalize);
        }

        private IntVec3 GetCollisionPoint(KIBeam_Projectile otherBeam)
        {
            if (this.Map == null || otherBeam == null)
            {
                return this.Position;
            }
            var myCells = GenRadial.RadialCellsAround(this.Position, 5f, true);
            var theirCells = GenRadial.RadialCellsAround(otherBeam.Position, 5f, true);
            return myCells.Intersect(theirCells).First();
        }
        public override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            if (!isInBeamBattle)
            {
                base.Impact(hitThing, blockedByShield);
            }
        }

        private void CreateBeamBattleEffects()
        {
            if (battleOpponent == null || ability?.pawn == null || battleOpponent.ability?.pawn == null || this.Map == null)
            {
                return;
            }

            Vector3 myPawnPos = this.ability.pawn.DrawPos;
            Vector3 theirPawnPos = battleOpponent.ability.pawn.DrawPos;
            Vector3 battleDirection = (theirPawnPos - myPawnPos).normalized;
            float totalDistance = Vector3.Distance(myPawnPos, theirPawnPos);
            float availableBattleSpace = totalDistance - (MINIMUM_SAFE_DISTANCE * 2);
            Vector3 battleStartPoint = myPawnPos + (battleDirection * MINIMUM_SAFE_DISTANCE);
            float pushDistance = ((battleAdvantage + 1f) / 2f) * availableBattleSpace;
            Vector3 currentClashPoint = battleStartPoint + (battleDirection * pushDistance);

            //GenExplosion.DoExplosion(
            //    center: currentClashPoint.ToIntVec3(),
            //    map: Map,
            //    radius: 1.9f,
            //    damType: DamageDefOf.Bomb,
            //    instigator: launcher,
            //    damAmount: 5,
            //    armorPenetration: 2f,
            //    explosionSound: null,
            //    weapon: null,
            //    projectile: def,
            //    intendedTarget: null,
            //    postExplosionSpawnThingDef: null,
            //    postExplosionSpawnChance: 0,
            //    postExplosionSpawnThingCount: 0,
            //    postExplosionGasType: null,
            //    applyDamageToExplosionCellsNeighbors: false,
            //    preExplosionSpawnThingDef: null,
            //    preExplosionSpawnChance: 0,
            //    preExplosionSpawnThingCount: 0,
            //    chanceToStartFire: 0f,
            //    damageFalloff: true,
            //    direction: null,
            //    ignoredThings: null,
            //    affectedAngle: null,
            //    doVisualEffects: true,
            //    propagationSpeed: 1f,
            //    excludeRadius: 0f,
            //    doSoundEffects: false,
            //    postExplosionSpawnThingDefWater: null,
            //    screenShakeFactor: 0.5f + Mathf.Abs(battleAdvantage) * 0.5f
            //);
        }
    }

}
