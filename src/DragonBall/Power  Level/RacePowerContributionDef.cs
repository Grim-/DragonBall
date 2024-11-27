using SaiyanMod;
using Verse;

namespace DragonBall
{
    public class RacePowerContributionDef : PowerLevelContributionDef
    {
        public ThingDef race;
        public float multiplier = 1f;

        public override float Calculate(Pawn pawn, AbilityClassKI kiClass)
        {
            if (pawn.def == race)
                return multiplier;

            return isMultiplier ? 1f : 0f;
        }
    }
}
