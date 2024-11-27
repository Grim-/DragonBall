using SaiyanMod;
using Verse;

namespace DragonBall
{
    public class LevelContributionDef : PowerLevelContributionDef
    {
        public float multiplier = 1.1f;

        public override float Calculate(Pawn pawn, AbilityClassKI kiClass)
        {
            return kiClass == null ? 1 : kiClass.level * multiplier;
        }
    }
}
