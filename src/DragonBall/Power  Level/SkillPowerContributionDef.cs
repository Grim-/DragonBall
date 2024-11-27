using RimWorld;
using SaiyanMod;
using Verse;

namespace DragonBall
{
    // For skills like melee, shooting, etc
    public class SkillPowerContributionDef : PowerLevelContributionDef
    {
        public SkillDef skill;
        public float multiplier;

        public override float Calculate(Pawn pawn, AbilityClassKI kiClass)
        {
            return pawn.skills != null ? pawn.skills.GetSkill(skill).Level * multiplier : 1;
        }
    }
}
