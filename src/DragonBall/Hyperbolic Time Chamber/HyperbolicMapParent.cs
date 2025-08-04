using RimWorld.Planet;
using Verse;

namespace DragonBall
{
    public class HyperbolicMapParent : PocketMapParent
    {
        public float timeMultiplier = 365f; // 1 day outside = 1 year inside
        private int ticksInChamber = 0;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref timeMultiplier, "timeMultiplier", 365f);
            Scribe_Values.Look(ref ticksInChamber, "ticksInChamber", 0);
        }

        protected override void Tick()
        {
            base.Tick();
            //ticksInChamber++;

            //// Here we could implement special time dilation effects
            //if (ticksInChamber % 2500 == 0) // Every game day
            //{
            //    // Apply training effects
            //}
        }
    }
}
