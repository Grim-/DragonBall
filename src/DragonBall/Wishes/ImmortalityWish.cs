using RimWorld;
using System.Collections.Generic;
using Verse;

namespace DragonBall
{
    public class ImmortalityWish : BaseWish
    {
        public ImmortalityWish()
        {

        }

        public ImmortalityWish(WishDef def) : base(def)
        {

        }

        public override bool CanBeGranted(Map map, Building_DragonBallAltar altar, Pawn pawn)
        {
            return pawn != null;
        }

        public override IEnumerable<BaseWish> GenerateWishes(Map map, Building_DragonBallAltar altar, Pawn pawn)
        {
            if (def is HediffWishDef hediffWishDef)
            {
                yield return new HediffWish(hediffWishDef);
            }
        }

        public override void Grant(Map map, Building_DragonBallAltar altar, Pawn pawn)
        {
            if (pawn != null)
            {
                if (!pawn.health.hediffSet.HasHediff(DBDefOf.DragonBallAgingSlowed))
                {
                    pawn.health.AddHediff(DBDefOf.DragonBallAgingSlowed);
                }

                // Set biological age to 21
                pawn.ageTracker.AgeBiologicalTicks = (long)(21 * GenDate.TicksPerYear);

                // Set chronological age to match biological age
                pawn.ageTracker.AgeChronologicalTicks = pawn.ageTracker.AgeBiologicalTicks;

                // Force refresh of age-related stats and graphics
                pawn.ageTracker.ResetAgeReversalDemand(Pawn_AgeTracker.AgeReversalReason.ViaTreatment);
            }
        }
    }
}
