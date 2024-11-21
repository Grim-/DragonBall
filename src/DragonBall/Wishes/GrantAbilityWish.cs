using SaiyanMod;
using System.Collections.Generic;
using TaranMagicFramework;
using Verse;

namespace DragonBall
{
    public class AbilityWishDef : WishDef
    {
        public TaranMagicFramework.AbilityDef ability;
    }

    public class AbilityWish : PawnTargetWish
    {
        public TaranMagicFramework.AbilityDef ability;

        public AbilityWish()
        {
        }

        public AbilityWish(WishDef def, Pawn target, TaranMagicFramework.AbilityDef ability) : base(def, target)
        {
            this.ability = ability;
            this.Target = target;
        }

        public override string Label => $"{Target.LabelShort} learns {ability.label}.";

        public override string Description => $"{Target.LabelShort} learns {ability.label}.";

        public override bool CanBeGranted(Map map, Building_DragonBallAltar altarr, Pawn pawn)
        {
            if (pawn == null)
            {
                return false;
            }
            else
            {
                var fusedAbilities = pawn.GetComp<CompAbilities>();

                if (fusedAbilities.TryGetKIAbilityClass(out AbilityClassKI fusedKi))
                {
                    return true;
                }
            }
            return false;
        }

        public override IEnumerable<BaseWish> GenerateWishes(Map map, Building_DragonBallAltar altar, Pawn pawn)
        {
            if (def is AbilityWishDef abilityWishDef)
            {
                yield return new AbilityWish(def, pawn, abilityWishDef.ability);
            }
        }

        public override void Grant(Map map, Building_DragonBallAltar altar, Pawn pawn)
        {
            if (pawn != null)
            {
                var fusedAbilities = pawn.GetComp<CompAbilities>();

                if (fusedAbilities.TryGetKIAbilityClass(out AbilityClassKI fusedKi))
                {
                    if (!fusedKi.Learned(ability))
                    {
                        fusedKi.LearnAbility(ability, false, ability.abilityTiers.Count - 1);
                    }
                }
            }
        }
    }
}
