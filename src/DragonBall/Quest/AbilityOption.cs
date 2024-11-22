using Verse;

namespace DragonBall
{
    public class AbilityOption : IExposable
    {
        public TaranMagicFramework.AbilityDef ability;
        public float weight = 1f;

        public void ExposeData()
        {
            Scribe_Defs.Look(ref ability, "ability");
            Scribe_Values.Look(ref weight, "weight");
        }
    }

}
