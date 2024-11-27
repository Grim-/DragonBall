using RimWorld.Planet;
using SaiyanMod;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace DragonBall
{
    public class KIAbilityKaioKen : KIAbility_Toggleable
    {
        protected Color TransformedColor = Color.red;
        protected Color oldColor;
        //private void TryChangeHair()
        //{
        //    var newHair = DefDatabase<HairDef>.GetNamedSilentFail(pawn.story.hairDef.defName + HairPostfix);
        //    oldColor = pawn.story.hairColor;
        //    if (newHair != null)
        //    {
        //        pawn.story.hairDef = newHair;
        //    }
        //    pawn.story.hairColor = TransformedColor;
        //    pawn.ResolveAllGraphicsSafely();
        //}

        private TaranMagicFramework.Mote_Animation SparksOverlay;

        public override void Start(bool consumeEnergy = true)
        {
            base.Start(consumeEnergy);


            if (SparksOverlay == null && DBDefOf.SR_PowerUpOverlaySuperSaiyanIISparks != null)
            {
                SparksOverlay = MakeAnimation(DBDefOf.SR_PowerUpOverlaySuperSaiyanIISparks);
                SparksOverlay.Attach(this.pawn, Vector3.zero, false);
            }




            Hediff kaiokenStrain = this.pawn.health.GetOrAddHediff(DBDefOf.SR_KaioKenStrainHediff);

            if (kaiokenStrain != null)
            {
                float severityIncrease = 0;

                if (this.def == DBDefOf.SR_KaioKen)
                {
                    severityIncrease = 0.25f;
                }
                else if (this.def == DBDefOf.SR_KaioKenX3)
                {
                    severityIncrease = 0.35f;
                }
                else if(this.def == DBDefOf.SR_KaioKenX10)
                {
                    severityIncrease = 0.5f;
                }

                kaiokenStrain.Severity += severityIncrease;
            }

        }

        public override void End()
        {
            base.End();

            if (SparksOverlay != null)
            {
                SparksOverlay.Destroy();
                SparksOverlay = null;
            }
        }
    }


}
