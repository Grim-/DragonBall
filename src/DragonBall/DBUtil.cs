using LudeonTK;
using SaiyanMod;
using System.Collections.Generic;
using TaranMagicFramework;
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
    }
}
