using RimWorld;
using SaiyanMod;
using Verse;

namespace DragonBall
{
    public class PassionPowerContributionDef : PowerLevelContributionDef
    {
        public SkillDef skill;
        public float minorPassionMultiplier = 1.25f;
        public float majorPassionMultiplier = 1.5f;

        public override float Calculate(Pawn pawn, AbilityClassKI kiClass)
        {
            Passion passion = pawn.skills.GetSkill(skill).passion;
            if (passion == Passion.Major)
                return majorPassionMultiplier;
            if (passion == Passion.Minor)
                return minorPassionMultiplier;
            return 1f;
        }
    }
}
