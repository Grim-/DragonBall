using LudeonTK;
using RimWorld;
using SaiyanMod;
using System.Collections.Generic;
using TaranMagicFramework;
using UnityEngine;
using Verse;

namespace DragonBall
{
    public static class DBUtil
    {
        [DebugAction("The Saiyans - Dragon Ball", "SpawnDragonBallSet", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void SpawnDragonBallSet()
        {
            IntVec3 Origin = UI.MouseCell();

            foreach (var item in DragonBallPositions)
            {
                if (item.Key == null)
                {
                    continue;
                }

                Thing thing = ThingMaker.MakeThing(item.Key);
                GenSpawn.Spawn(thing, Origin + GetDragonBallOffset(thing), Find.CurrentMap);
            }
        }

        [DebugAction("The Saiyans - Dragon Ball", "View Tournament History")]
        private static void OpenTournamentHistory()
        {
            Current.Game.GetComponent<TournamentTracker>().OpenHistoryWindow();
        }

        public static readonly Dictionary<ThingDef, IntVec3> DragonBallPositions = new Dictionary<ThingDef, IntVec3>
        {
            { DBDefOf.DragonBallSevenStar, new IntVec3(0, 0, 0) },     // Center
            { DBDefOf.DragonBallOneStar,   new IntVec3(0, 0, 2) },     // North
            { DBDefOf.DragonBallTwoStar,   new IntVec3(2, 0, 1) },     // Northeast
            { DBDefOf.DragonBallThreeStar, new IntVec3(2, 0, -1) },    // Southeast
            { DBDefOf.DragonBallFourStar,  new IntVec3(0, 0, -2) },    // South
            { DBDefOf.DragonBallFiveStar,  new IntVec3(-2, 0, -1) },   // Southwest
            { DBDefOf.DragonBallSixStar,   new IntVec3(-2, 0, 1) },    // Northwest
        };


        public static IntVec3 GetDragonBallOffset(Thing Thing)
        {
            if (DragonBallPositions.ContainsKey(Thing.def))
            {
                return DragonBallPositions[Thing.def];
            }

            return IntVec3.Zero;
        }

        public static IntVec3 GetDragonBallOffset(ThingDef Thing)
        {
            if (DragonBallPositions.ContainsKey(Thing))
            {
                return DragonBallPositions[Thing];
            }

            return IntVec3.Zero;
        }

        public static void LevelUp(this TaranMagicFramework.Ability ability)
        {
            int currentLevel = ability.level;
            int maxLevel = ability.MaxLevel;
            ability.ChangeLevel(Mathf.Clamp(currentLevel + 1, 0, maxLevel));
        }

        public static void LevelToMax(this TaranMagicFramework.Ability ability)
        {
            int currentLevel = ability.level;
            int maxLevel = ability.MaxLevel;
            ability.ChangeLevel(maxLevel);
        }

        public static bool TryGetKiAbilityClass(this Pawn Pawn, out AbilityClassKI abilityClass)
        {
            abilityClass = null;

            var compAbility = Pawn.GetComp<CompAbilities>();

            if (compAbility == null) return false;

            if (compAbility.TryGetKIAbilityClass(out AbilityClassKI kiClass))
            {
                abilityClass = kiClass;
                return true;
            }

            return false;
        }


        public static void DoSenzuBeanHeal(this Pawn Pawn, bool HealInjuries, bool RestoreParts)
        {

            if (HealInjuries)
            {
                List<Hediff> hediffs = Pawn.health.hediffSet.hediffs;
                for (int i = hediffs.Count - 1; i >= 0; i--)
                {
                    Hediff hediff = hediffs[i]; 
                    if (CanRemoveHediff(hediff))
                    {
                        Pawn.health.RemoveHediff(hediff);
                    }
                }
            }

            if (RestoreParts)
            {
                List<Hediff_MissingPart> missingParts = Pawn.health.hediffSet.GetMissingPartsCommonAncestors();
                for (int i = missingParts.Count - 1; i >= 0; i--)
                {
                    if (IsSaiyanTail(Pawn, missingParts[i].Part))
                        continue;

                    Pawn.health.RestorePart(missingParts[i].Part);
                }
            }

            Hediff bloodLoss = Pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BloodLoss);
            if (bloodLoss != null)
            {
                Pawn.health.RemoveHediff(bloodLoss);
            }
             

            if (Pawn.needs != null)
            {
                if (Pawn.needs.rest != null)
                    Pawn.needs.rest.CurLevel = Pawn.needs.rest.MaxLevel;

                if (Pawn.needs.food != null)
                    Pawn.needs.food.CurLevel = Pawn.needs.food.MaxLevel;
            }

            if (Pawn.TryGetKiAbilityClass(out AbilityClassKI kiClass))
            {
                kiClass.abilityResource.energy = kiClass.abilityResource.MaxEnergy;
            }
        }


        private static bool CanRemoveHediff(Hediff hediff)
        {
            bool IsHediffBadAndNotPermanent = hediff.def.isBad && !hediff.IsPermanent();
            bool IsNonPermanentInjury = hediff is Hediff_Injury injury && injury.def.isBad && !injury.IsPermanent();
            bool IsNotSaiyanMissingTail = hediff.def != SR_DefOf.SR_SaiyanTailInjured;
            return IsHediffBadAndNotPermanent && IsNonPermanentInjury && IsNotSaiyanMissingTail;
        }

        private static bool IsSaiyanTail(Pawn Pawn, BodyPartRecord part)
        {
            return part.def == SR_DefOf.SR_Tail;
        }
    }
}
