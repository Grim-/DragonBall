using SaiyanMod;
using Verse;

namespace DragonBall
{
    // For transformations and other active abilities
    public class ActiveAbilityPowerContributionDef : PowerLevelContributionDef
    {
        public TaranMagicFramework.AbilityDef ability;
        public float multiplier;

        public override float Calculate(Pawn pawn, AbilityClassKI kiClass)
        {
            if (kiClass == null) return 1f;

            var abilityComp = kiClass.GetLearnedAbility(ability);
            if (abilityComp != null && abilityComp.Active)
                return multiplier;
            return 1f; // For multipliers, 1 means no effect
        }
    }
}
