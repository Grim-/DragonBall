using Verse;

namespace DragonBall
{
    public class ItemOption : IExposable
    {
        public ThingDef thing;
        public int count;
        public void ExposeData()
        {
            Scribe_Defs.Look(ref thing, "itemOption");
            Scribe_Values.Look(ref count, "count");
        }
    }

    public class RewardItemOption : IExposable
    {
        public ThingDef thing;
        public IntRange count;

        public void ExposeData()
        {
            Scribe_Defs.Look(ref thing, "itemOption");
            Scribe_Values.Look(ref count, "count");
        }
    }
}
