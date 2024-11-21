using RimWorld;
using SaiyanMod;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace DragonBall
{
    public class DragonBallEffectController
    {
        private readonly Map map;
        private readonly Building_DragonBallAltar altar;
        private readonly List<Thing> dragonBalls;
        private int ticksSinceLastStrike = 0;
        private int currentBallIndex = 0;
        private const int STRIKE_DELAY = 60;
        private EffectStage currentStage = EffectStage.IndividualStrikes;

        private enum EffectStage
        {
            IndividualStrikes,
            FinalStrike,
            Complete
        }

        public DragonBallEffectController(Map map, Building_DragonBallAltar altar)
        {
            this.map = map;
            this.altar = altar;
            this.dragonBalls = altar.GetDragonBallsInProximity().ToList();
        }

        public bool Tick()
        {
            if (currentStage == EffectStage.Complete)
                return true;

            ticksSinceLastStrike++;

            switch (currentStage)
            {
                case EffectStage.IndividualStrikes:
                    HandleIndividualStrikes();
                    break;

                case EffectStage.FinalStrike:
                    HandleFinalStrike();
                    break;
            }

            return currentStage == EffectStage.Complete;
        }

        private void HandleIndividualStrikes()
        {
            if (ticksSinceLastStrike < STRIKE_DELAY)
                return;

            if (currentBallIndex >= dragonBalls.Count)
            {
                currentStage = EffectStage.FinalStrike;
                ticksSinceLastStrike = 0;
                return;
            }
            DoHarmlessStrike(dragonBalls[currentBallIndex].Position, map);
            FleckMaker.ThrowLightningGlow(dragonBalls[currentBallIndex].Position.ToVector3(), map, 2f);
            currentBallIndex++;
            ticksSinceLastStrike = 0;
        }
        private void DoHarmlessStrike(IntVec3 strikeLoc, Map map)
        {
            // Off-map thunder sound
            SoundDefOf.Thunder_OffMap.PlayOneShotOnCamera(map);

            if (!strikeLoc.IsValid)
            {
                strikeLoc = CellFinderLoose.RandomCellWith(sq => sq.Standable(map) && !map.roofGrid.Roofed(sq), map, 1000);
            }

            var boltMesh = LightningBoltMeshPool.RandomBoltMesh;

            if (!strikeLoc.Fogged(map))
            {
                Vector3 loc = strikeLoc.ToVector3Shifted();
                for (int i = 0; i < 4; i++)
                {
                    FleckMaker.ThrowSmoke(loc, map, 1.5f);
                    FleckMaker.ThrowMicroSparks(loc, map);
                    FleckMaker.ThrowLightningGlow(loc, map, 1.5f);
                }
            }

            // On-map thunder sound
            SoundInfo info = SoundInfo.InMap(new TargetInfo(strikeLoc, map, false), MaintenanceType.None);
            SoundDefOf.Thunder_OnMap.PlayOneShot(info);
        }
        private void HandleFinalStrike()
        {
            if (ticksSinceLastStrike < STRIKE_DELAY)
                return;

            foreach (var ball in dragonBalls)
            {
                DoHarmlessStrike(ball.Position, map);
                FleckMaker.ThrowLightningGlow(ball.Position.ToVector3(), map, 2f);
            }

            DoHarmlessStrike(altar.Position, map);

            for (int i = 0; i < 4; i++)
            {
                DoHarmlessStrike(altar.Position, map);
            }

            DoHarmlessStrike(altar.Position, map);

            FleckMaker.ThrowLightningGlow(altar.Position.ToVector3(), map, 3f);
            currentStage = EffectStage.Complete;
        }
    }


    public class IncidentWorker_DragonBallLegendarySaiyanRaid : IncidentWorker_LegendarySuperSaiyanRaid
    {
        public override bool CanFireNowSub(IncidentParms parms)
        {
            // Preserve original raid checks
            if (!base.CanFireNowSub(parms))
            {
                return false;
            }

            return SaiyanMod.SaiyanModClass.settings.enableLegendarySuperSaiyanRaids;
        }

        public override float BaseChanceThisGame
        {
            get
            {
                Map map = Find.CurrentMap;
                if (map == null)
                    return base.BaseChanceThisGame;

                int dragonBallCount = GetDragonBallCount(map);
 
                float baseChance = base.BaseChanceThisGame;
                float multiplier = 1f + (dragonBallCount * 0.4f); 

                return baseChance * multiplier;
            }
        }

        public override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            int dragonBallCount = GetDragonBallCount(map);

            if (dragonBallCount > 0)
            {
                parms.points *= 1f + (dragonBallCount * 0.05f); 
            }


            PawnGenerator_GeneratePawnRelations_Patch.skipRelations = true;
            bool result = base.TryExecuteWorker(parms);
            PawnGenerator_GeneratePawnRelations_Patch.skipRelations = false;

            return result;
        }

        private int GetDragonBallCount(Map map)
        {
            return map.listerThings.AllThings.Where(x => x.def.IsWithinCategory(DBDefOf.DragonBallsCategory)).Count();
        }
    }
}
