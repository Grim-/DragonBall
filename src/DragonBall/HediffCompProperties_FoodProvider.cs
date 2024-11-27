using RimWorld;
using Verse;

namespace DragonBall
{
    public class HediffCompProperties_FoodProvider : HediffCompProperties
    {
        public float nutritionAmount = 0.1f;      
        public int ticksBetweenFeeding = 6000;    
        public bool showMote = true;              

        public HediffCompProperties_FoodProvider()
        {
            compClass = typeof(HediffComp_FoodProvider);
        }
    }

    public class HediffComp_FoodProvider : HediffComp
    {
        private int ticksUntilNextFood;

        public HediffCompProperties_FoodProvider Props => (HediffCompProperties_FoodProvider)props;

        public override void CompPostMake()
        {
            base.CompPostMake();
            ResetInterval();
        }

        private void ResetInterval()
        {
            ticksUntilNextFood = Props.ticksBetweenFeeding;
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (!Pawn.Dead)
            {
                ticksUntilNextFood--;

                if (ticksUntilNextFood <= 0)
                {
                    FeedPawn();
                    ResetInterval();
                }
            }
        }

        private void FeedPawn()
        {
            Need_Food foodNeed = Pawn.needs.food;
            if (foodNeed != null && foodNeed.CurLevelPercentage < 0.99f)
            {
                foodNeed.CurLevel += Props.nutritionAmount;

                if (Props.showMote && Pawn.Spawned)
                {
                    MoteMaker.ThrowText(Pawn.DrawPos, Pawn.Map, "+" + Props.nutritionAmount.ToStringPercent() + " food");
                }
            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref ticksUntilNextFood, "ticksUntilNextFood");
        }

        public override string CompLabelInBracketsExtra =>
            $"Next feeding in {ticksUntilNextFood.ToStringTicksToPeriod()}";
    }
}
