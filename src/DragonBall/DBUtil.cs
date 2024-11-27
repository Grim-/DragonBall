using LudeonTK;
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
    }

    [StaticConstructorOnStartup]
    public class DragonBall : Mod
    {
        private TournamentTracker tracker;
        private DragonBallModSettings settings;

        public DragonBall(ModContentPack content) : base(content)
        {
            settings = GetSettings<DragonBallModSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            if (tracker == null)
            {
                tracker = Current.Game.GetComponent<TournamentTracker>();
            }

            if (tracker == null)
            {
                return;
            }

            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            if (listingStandard.ButtonText("View Tournament History") && Current.Game != null)
            {
                tracker.OpenHistoryWindow();
            }

            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Dragon Ball Addon";
        }
    }

    public class DragonBallModSettings : ModSettings
    {
        public override void ExposeData()
        {
            base.ExposeData();
        }
    }
}
