using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace DragonBall
{
    public class ResurrectWish : PawnTargetWish
    {
        public ResurrectWish()
        {
        }

        public ResurrectWish(WishDef def, Pawn target) : base(def, target)
        {
        }

        public override string Label => $"Resurrect {Target.LabelShort}";

        public override string Description => $"Bring {Target.LabelShort} back to life";

        public override bool CanBeGranted(Map map, Building_DragonBallAltar altar, Pawn pawn) => Target?.Dead == true;

        public override void Grant(Map map, Building_DragonBallAltar altar, Pawn pawn)
        {
            ResurrectionUtility.TryResurrect(Target);
            Messages.Message($"{Target.LabelShort} has been resurrected!", MessageTypeDefOf.PositiveEvent);
        }

        public override IEnumerable<BaseWish> GenerateWishes(Map map, Building_DragonBallAltar altar, Pawn pawn)
        {
            var deadColonists = new HashSet<Pawn>();

            // Find corpses in graves
            var graves = map.listerThings.ThingsInGroup(ThingRequestGroup.Grave);
            var gravePawns = graves
                .OfType<Building_Grave>()
                .Where(g => g.HasCorpse && g.Corpse.InnerPawn?.Faction == Faction.OfPlayer)
                .Select(g => g.Corpse.InnerPawn);
            deadColonists.UnionWith(gravePawns);

            // Find corpses on the ground
            var corpses = map.listerThings.ThingsInGroup(ThingRequestGroup.Corpse);
            var corpsePawns = corpses
                .OfType<Corpse>()
                .Where(c => c.InnerPawn?.Faction == Faction.OfPlayer)
                .Select(c => c.InnerPawn);
            deadColonists.UnionWith(corpsePawns);

            // Create wishes for each dead colonist
            return deadColonists.Select(deadPawn => new ResurrectWish(def, deadPawn));
        }


        public static List<Pawn> GetDeadFactionMapPawns(Map map)
        {
            var deadColonists = new HashSet<Pawn>();

            // Find corpses in graves
            var graves = map.listerThings.ThingsInGroup(ThingRequestGroup.Grave);
            var gravePawns = graves
                .OfType<Building_Grave>()
                .Where(g => g.HasCorpse && g.Corpse.InnerPawn?.Faction == Faction.OfPlayer)
                .Select(g => g.Corpse.InnerPawn);
            deadColonists.UnionWith(gravePawns);

            // Find corpses on the ground
            var corpses = map.listerThings.ThingsInGroup(ThingRequestGroup.Corpse);
            var corpsePawns = corpses
                .OfType<Corpse>()
                .Where(c => c.InnerPawn?.Faction == Faction.OfPlayer)
                .Select(c => c.InnerPawn);
            deadColonists.UnionWith(corpsePawns);

            return deadColonists.ToList();
        }
    }

    //public class CropBoostWishDef : WishDef
    //{
    //    public float growthSpeedFactor = 2f;
    //    public int durationTicks = 60000;
    //}

    //public class CropBoostWish : BaseWish
    //{
    //    private CropBoostWishDef CropDef => def as CropBoostWishDef;

    //    public override string Label => $"Magical Growth ({CropDef.durationTicks.ToStringTicksToPeriod()})";

    //    public override bool CanBeGranted(Map map, Building_DragonBallAltar altar, Pawn pawn)
    //    {
    //        return map?.listerThings.ThingsInGroup(ThingRequestGroup.Plant).Any() ?? false;
    //    }

    //    public override void Grant(Map map, Building_DragonBallAltar altar, Pawn pawn)
    //    {
    //        var plants = map.listerThings.ThingsInGroup(ThingRequestGroup.Plant)
    //            .OfType<Plant>();

    //        foreach (var plant in plants)
    //        {
    //            plant.Growth = plant.Growth; // Refresh growth
    //            plant. *= CropDef.growthSpeedFactor;
    //            FleckMaker.ThrowAirPuffUp(plant.Position.ToVector3(), map);
    //        }

    //        Messages.Message("Plants have been blessed with magical growth!", MessageTypeDefOf.PositiveEvent);
    //    }
    //}
}
