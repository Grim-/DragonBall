using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace DragonBall
{
    public class CompUsableSenzuBean : CompUsable
    {
        public override AcceptanceReport CanBeUsedBy(Pawn p, bool forced = false, bool ignoreReserveAndReachable = false)
        {
            return base.CanBeUsedBy(p, forced, ignoreReserveAndReachable);
        }

        public override void UsedBy(Pawn pawn)
        {
            Hediff hediff = HediffMaker.MakeHediff(DBDefOf.DragonBallSenzuHealing, pawn);
            pawn.health.AddHediff(hediff);
            parent.Destroy();
        }
    }
}
