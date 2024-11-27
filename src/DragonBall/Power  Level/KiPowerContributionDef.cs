using SaiyanMod;
using Verse;

namespace DragonBall
{
    public class KiPowerContributionDef : PowerLevelContributionDef
    {
        public float multiplier;

        public override float Calculate(Pawn pawn, AbilityClassKI kiClass)
        {
            return kiClass == null ? 1 : kiClass.abilityResource.MaxEnergy * multiplier;
        }
    }
}
