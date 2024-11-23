using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace DragonBall
{
    public class JobDriver_DragonBallSummoning : JobDriver
    {
        protected Thing Altar => job.targetA.Thing;
        protected Building_DragonBallAltar AltarBuilding => (Building_DragonBallAltar)Altar;
        private DragonBallEffectController effectController;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOn(() =>
            {
                return AltarBuilding.HasDragonBallOfTypeAlready(TargetThingA) || Altar == null;
            });

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell).FailOnDespawnedNullOrForbidden(TargetIndex.A);

            Toil initializeSummoning = new Toil();
            initializeSummoning.initAction = delegate
            {
                var dragonBalls = AltarBuilding.GetDragonBallsInProximity().ToList();
                job.targetA.Thing.Map.weatherManager.TransitionTo(WeatherDef.Named("RainyThunderstorm"));
                effectController = new DragonBallEffectController(Map, AltarBuilding);
            };

            yield return initializeSummoning;

            // Main summoning sequence
            Toil summoningToil = new Toil();
            summoningToil.tickAction = delegate
            {
                if (effectController.Tick())
                {
                    initializeSummoning.actor.jobs.curDriver.ReadyForNextToil();
                }
            };

            summoningToil.defaultCompleteMode = ToilCompleteMode.Never;
            summoningToil.defaultDuration = 2000;
            summoningToil.WithProgressBar(TargetIndex.A, () =>
                (float)summoningToil.actor.jobs.curDriver.ticksLeftThisToil / summoningToil.defaultDuration);
            yield return summoningToil;

            // Final dramatic effect and wish selection
            Toil dramaticEffectsToil = new Toil();
            dramaticEffectsToil.defaultCompleteMode = ToilCompleteMode.Delay;
            dramaticEffectsToil.defaultDuration = 180;
            dramaticEffectsToil.initAction = delegate
            {
                Map map = pawn.Map;

                foreach (var ball in AltarBuilding.GetDragonBallsInProximity())
                {
                    FleckMaker.ThrowLightningGlow(ball.Position.ToVector3(), map, 3f);
                    DoHarmlessStrike(ball.Position, map);
                    Find.CameraDriver.shaker.DoShake(4f);
                }


                DoHarmlessStrike(Altar.Position, map);
                Find.CameraDriver.shaker.DoShake(4f);
            };


            yield return dramaticEffectsToil;
            Toil showUIToil = new Toil();
            showUIToil.initAction = () =>
            {
                Map map = pawn.Map;
                AltarBuilding.ShowWishUI(map, pawn);
                this.EndJobWith(JobCondition.Succeeded);
            };

            yield return showUIToil;
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
    }
}
