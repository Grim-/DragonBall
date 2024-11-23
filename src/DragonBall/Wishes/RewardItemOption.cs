using RimWorld;
using Verse;

namespace DragonBall
{
    public class RewardItemOption : IExposable
    {
        public ThingDef thing;
        public QualityCategory quality = QualityCategory.Good;
        public IntRange count;

        public void ExposeData()
        {
            Scribe_Defs.Look(ref thing, "itemOption");
            Scribe_Values.Look(ref count, "count");
            Scribe_Values.Look(ref quality, "quality");
        }
    }
}
