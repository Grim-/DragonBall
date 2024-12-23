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

        private TaranMagicFramework.Mote_Animation SparksOverlay;

        private AbilityTierKi_KaioKen Tier => (AbilityTierKi_KaioKen)AbilityTier;


        private int currentTick = 0;
        private int ticks => Tier != null ? Tier.severityTickTime : 1250;

        public override void Start(bool consumeEnergy = true)
        {
            base.Start(consumeEnergy);
            if (SparksOverlay == null && DBDefOf.SR_PowerUpOverlaySuperSaiyanIISparks != null)
            {
                SparksOverlay = MakeAnimation(DBDefOf.SR_PowerUpOverlaySuperSaiyanIISparks);
                SparksOverlay.Attach(this.pawn, Vector3.zero, false);
            }

            currentTick = 0;
        }

        public override void Tick()
        {
            base.Tick();

            if (this.Active && this.pawn.IsHashIntervalTick(ticks))
            {
                Hediff kaiokenStrain = this.pawn.health.GetOrAddHediff(DBDefOf.SR_KaioKenStrainHediff);

                if (kaiokenStrain != null)
                {
                    float severityIncrease = GetTickSeverity();

                    kaiokenStrain.Severity += severityIncrease;
                }
            }
        }


        private float GetTickSeverity()
        {
            return Tier != null ? Tier.severityGainedPerTick : 0.05f;
        }

        public override void End()
        {
            base.End();

            if (SparksOverlay != null)
            {
                SparksOverlay.Destroy();
                SparksOverlay = null;
            }

            currentTick = 0;
        }

        public override void ExposeData()
        {
            base.ExposeData();
        }
    }

    public class AbilityTierKi_KaioKen : AbilityTierKIDef
    {
        public float severityGainedPerTick = 0.01f;
        public int severityTickTime = 1250;
    }


   
}
