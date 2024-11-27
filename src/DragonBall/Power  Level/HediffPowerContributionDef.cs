using SaiyanMod;
using Verse;

namespace DragonBall
{
    public class HediffPowerContributionDef : PowerLevelContributionDef
    {
        public HediffDef hediff;
        public float multiplierPerSeverity = 1f;

        public override float Calculate(Pawn pawn, AbilityClassKI kiClass)
        {
            Hediff hediffInstance = pawn.health.hediffSet.GetFirstHediffOfDef(hediff);
            if (hediffInstance == null)
                return isMultiplier ? 1f : 0f;

            return hediffInstance.Severity * multiplierPerSeverity;
        }
    }
}
