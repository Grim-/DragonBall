using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace DragonBall
{
    public class ResurrectWish : BaseWish
    {
        public Pawn deadPawn;

        public override string Label => $"Resurrect {deadPawn.LabelShort}";
        public override string Description => $"Bring {deadPawn.LabelShort} back to life";

        public override bool CanBeGranted(Map map, Building_DragonBallAltar altar) => deadPawn?.Dead == true;

        public override void Grant(Map map, Building_DragonBallAltar altar)
        {
            //ResurrectionUtility.Resurrect(deadPawn);
            Messages.Message($"{deadPawn.LabelShort} has been resurrected!", MessageTypeDefOf.PositiveEvent);
        }

        public override IEnumerable<BaseWish> GenerateWishes(Map map, Building_DragonBallAltar altar)
        {
            // Find all dead colonists and generate a wish for each
            return Find.WorldPawns.AllPawnsAlive
                .Where(p => p.Dead && p.Faction == Faction.OfPlayer)
                .Select(pawn => new ResurrectWish
                {
                    def = this.def,
                    deadPawn = pawn
                });
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref deadPawn, "deadPawn");
        }
    }
}
