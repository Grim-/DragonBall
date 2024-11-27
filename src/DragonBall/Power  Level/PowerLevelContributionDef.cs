using SaiyanMod;
using Verse;

namespace DragonBall
{
    public abstract class PowerLevelContributionDef : Def
    {
        // Whether to add to base power or multiply final result
        public bool isMultiplier = false;

        public abstract float Calculate(Pawn pawn, AbilityClassKI kiClass);
    }
}
