using RimWorld;
using Verse;

namespace DragonBall
{
    public class CompProperties_SenzuBeanUse : CompProperties_UseEffect
    {
        public bool RestoreParts = true;
        public bool HealInjuries = true;

        public CompProperties_SenzuBeanUse()
        {
            compClass = typeof(SenzuBeanUseEffect);
        }
    }

    public class SenzuBeanUseEffect : CompUseEffect
    {
        CompProperties_SenzuBeanUse Props => (CompProperties_SenzuBeanUse)props;

        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);
            usedBy.health.AddHediff(DBDefOf.DragonBallSenzuHealing);
        }
    }
}
