using SaiyanMod;
using Verse;

namespace DragonBall
{
    // For physical capabilities like moving, manipulation
    public class CapacityPowerContributionDef : PowerLevelContributionDef
    {
        public PawnCapacityDef capacity;
        public float multiplier;

        public override float Calculate(Pawn pawn, AbilityClassKI kiClass)
        {
            return pawn.health.capacities.GetLevel(capacity) * multiplier;
        }
    }
}
