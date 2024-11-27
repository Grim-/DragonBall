using SaiyanMod;
using System.Collections.Generic;
using TaranMagicFramework;
using Verse;

namespace DragonBall
{
    public class KIAbility_KaioKen : KIAbility
    {
        public override bool IsInstantAction
        {
            get
            {
                return false;
            }
        }

        public override void Start(bool consumeEnergy = true)
        {
            base.Start(true);
        }

        private void ClearDisallowedPowerUps()
        {
            foreach (TaranMagicFramework.Ability ability in this.abilityClass.LearnedAbilities)
            {
                //if (ability != this && ability.Active)
                //{
                //    //// Check if the ability is a transformation and end it
                //    //bool isTransformation = ability.def == SR_DefOf.SR_SuperSaiyan ||
                //    //                        ability.def == DBDefOf.SR_SuperSaiyan2 ||
                //    //                      ability.def == SR_DefOf.SR_LegendarySuperSaiyan ||
                //    //                      ability.def == SR_DefOf.SR_TrueSuperSaiyan ||
                //    //                      ability.def == SR_DefOf.SR_Awakened ||
                //    //                      ability.def == SR_DefOf.SR_PowerUp;

                //    //if (isTransformation)
                //    //{
                //    //    ability.End();
                //    //    ability.DestroyAllOverlay();
                //    //}
                //}
            }
        }

        public override void Tick()
        {
            base.Tick();

            // End KaioKen if energy is depleted
            if (this.Active && this.abilityResource.energy <= 0f)
            {
                this.End();
            }
        }

        public override TaranMagicFramework.AnimationDef AnimationDef(OverlayProps overlayProps)
        {
            // Return KaioKen specific animation overlay
            return SR_DefOf.SR_AwakenedOverlay;
        }

        public override Mote_Animation MakeAnimation(OverlayProps overlayProps)
        {
            Mote_Animation mote_Animation = base.MakeAnimation(overlayProps);
            if (mote_Animation.def == SR_DefOf.SR_AwakenedOverlay)
            {
                mote_Animation.instanceColor = new UnityEngine.Color(1f, 0f, 0f, 0.8f);
            }

            return mote_Animation;
        }

        public override void RegisterCast()
        {
       
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            Command_ToggleAbility command_ToggleAbility = new Command_ToggleAbility(this);
            command_ToggleAbility.icon = this.AbilityIcon();
            command_ToggleAbility.defaultLabel = this.AbilityLabel();
            command_ToggleAbility.defaultDesc = this.AbilityDescription();

            command_ToggleAbility.toggleAction = delegate ()
            {
                if (this.Active)
                {
                    this.End();
                }
                else
                {
                    this.Start(true);
                }
            };

            command_ToggleAbility.isActive = (() => this.Active);

            string failReason;
            command_ToggleAbility.Disabled = !this.CanBeActivated(this.EnergyCost, out failReason, base.LevelHumanReadable == 3, () => "");
            command_ToggleAbility.disabledReason = failReason;

            yield return command_ToggleAbility;
            yield break;
        }
    }

}
