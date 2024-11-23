using System.Collections.Generic;
using Verse;

namespace DragonBall
{
    public class HediffWish : BaseWish
    {
        public HediffWish()
        {

        }

        public HediffWish(WishDef def) : base(def)
        {

        }

        private HediffWishDef Def => (HediffWishDef)def;

        public override bool CanBeGranted(Map map, Building_DragonBallAltar altar, Pawn pawn)
        {
            if (pawn == null)
            {
                return false;
            }

            return !pawn.health.hediffSet.HasHediff(Def.hediff);
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
            if (pawn != null && !pawn.health.hediffSet.HasHediff(Def.hediff))
            {
                pawn.health.AddHediff(Def.hediff);
            }
        }
    }
}
