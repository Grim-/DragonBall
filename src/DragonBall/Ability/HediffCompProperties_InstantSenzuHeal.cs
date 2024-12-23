using RimWorld;
using SaiyanMod;
using System.Collections.Generic;
using Verse;

namespace DragonBall
{
    public class HediffCompProperties_InstantSenzuHeal : HediffCompProperties
    {
        public bool RestoreParts = true;
        public bool HealInjuries = true;

        public HediffCompProperties_InstantSenzuHeal()
        {
            compClass = typeof(HediffComp_InstantSenzuHeal);
        }
    }

    public class HediffComp_InstantSenzuHeal : HediffComp
    {
        HediffCompProperties_InstantSenzuHeal Props => (HediffCompProperties_InstantSenzuHeal)props;

        public override void CompPostMake()
        {
            base.CompPostMake();
            DBUtil.DoSenzuBeanHeal(Pawn, Props.HealInjuries, Props.RestoreParts);
        }
    }



}
