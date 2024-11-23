
using RimWorld;
using SaiyanMod;
using System.Collections.Generic;
using System.Linq;
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

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            ActiveBeams.Add(this);
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            ActiveBeams.Remove(this);
            base.DeSpawn(mode);
        }

        public override void Tick()
        {
            if (this.Spawned && this.Map != null)
            {
                if (!isInBeamBattle)
                {
                    base.Tick();
                    CheckBeamCollisions();
                }
                else
                {
                    // We're in a beam battle - drain Ki and check if we should lose
                    if (this.ability.pawn.TryGetKiAbilityClass(out AbilityClassKI kiClass))
                    {
                        if (this.ability.pawn.IsHashIntervalTick(120))
                        {
                            // Drain Ki faster during beam battle
                            kiClass.abilityResource.energy -= 2f;  // Adjust drain rate as needed

                            if (kiClass.abilityResource.energy <= 0)
                            {
                                // Out of Ki - we lose!
                                LoseBeamBattle();
                            }
                        }
                    }

                    // Lock position during beam battle
                    this.Position = lockedPosition.ToIntVec3();
                    //this.DrawPos = lockedPosition;

                    // Create ongoing collision effects
                    if (Find.TickManager.TicksGame % 120 == 0)  // Adjust frequency as needed
                    {
                        CreateBeamBattleEffects();
                    }
                }
            }
        }

        public override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            // Don't trigger impact if we're in a beam battle
            if (!isInBeamBattle)
            {
                base.Impact(hitThing, blockedByShield);
            }
        }

        private void CheckBeamCollisions()
        {
            foreach (var otherBeam in ActiveBeams)
            {
                if (otherBeam == this || otherBeam.Launcher == this.launcher)
                    continue;

                var myCells = GenRadial.RadialCellsAround(this.Position, 2f, true);
                var theirCells = GenRadial.RadialCellsAround(otherBeam.Position, 2f, true);

                if (myCells.Intersect(theirCells).Any())
                {
                    StartBeamBattle(otherBeam);
                    break;
                }
            }
        }

        private void StartBeamBattle(KIBeam_Projectile otherBeam)
        {
            // Lock both beams in place
            isInBeamBattle = true;
            lockedPosition = this.DrawPos;

            if (otherBeam is KIBeam_ProjectileBeamBattle battleBeam)
            {
                battleBeam.isInBeamBattle = true;
                battleBeam.lockedPosition = otherBeam.DrawPos;
                battleBeam.battleOpponent = this;
            }

            battleOpponent = otherBeam;

            CreateBeamBattleEffects();
        }

        private void CreateBeamBattleEffects()
        {
            IntVec3 collisionPoint = GetCollisionPoint(battleOpponent);
            GenExplosion.DoExplosion(
                center: collisionPoint,
                map: Map,
                radius: 1.9f,  // Smaller radius for continuous effects
                damType: DamageDefOf.Bomb,
                instigator: launcher,
                damAmount: 5,  // Reduced damage for continuous effects
                armorPenetration: 2f,
                explosionSound: null,
                weapon: null,
                projectile: def,
                intendedTarget: null,
                postExplosionSpawnThingDef: null,
                postExplosionSpawnChance: 0,
                postExplosionSpawnThingCount: 0,
                postExplosionGasType: null,
                applyDamageToExplosionCellsNeighbors: false,
                preExplosionSpawnThingDef: null,
                preExplosionSpawnChance: 0,
                preExplosionSpawnThingCount: 0,
                chanceToStartFire: 0f,
                damageFalloff: true,
                direction: null,
                ignoredThings: null,
                affectedAngle: null,
                doVisualEffects: true,
                propagationSpeed: 1f,
                excludeRadius: 0f,
                doSoundEffects: false,  // Disable sound for continuous effects
                postExplosionSpawnThingDefWater: null,
                screenShakeFactor: 0.5f
            );
        }

        private void LoseBeamBattle()
        {
            this.ability.End();
            this.Destroy();
            // If our opponent was also in a beam battle, let them know they won
            if (battleOpponent is KIBeam_ProjectileBeamBattle battleBeam)
            {
                battleBeam.isInBeamBattle = false;
                battleBeam.battleOpponent = null;
            }
        }

        private IntVec3 GetCollisionPoint(KIBeam_Projectile otherBeam)
        {
            var myCells = GenRadial.RadialCellsAround(this.Position, 5f, true);
            var theirCells = GenRadial.RadialCellsAround(otherBeam.Position, 5f, true);
            return myCells.Intersect(theirCells).First();
        }
    }
}
