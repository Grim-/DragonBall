using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace DragonBall
{
    public class JobDriver_DragonBallSummoning : JobDriver
    {
        protected Thing Altar => job.targetA.Thing;
        protected Building_DragonBallAltar AltarBuilding => (Building_DragonBallAltar)Altar;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);

            Toil summoningToil = new Toil();
            summoningToil.initAction = delegate
            {
                job.targetA.Thing.Map.weatherManager.TransitionTo(WeatherDef.Named("Rain"));
            };

            summoningToil.tickAction = delegate
            {

                if (pawn.IsHashIntervalTick(100))
                {
                    FleckMaker.ThrowLightningGlow(pawn.Position.ToVector3(), pawn.Map, 2f);
                }
            };

            summoningToil.defaultCompleteMode = ToilCompleteMode.Delay;
            summoningToil.defaultDuration = 2000;
            summoningToil.WithProgressBar(TargetIndex.A, () =>
                (float)summoningToil.actor.jobs.curDriver.ticksLeftThisToil / summoningToil.defaultDuration);
            yield return summoningToil;


            Toil finishSummoning = new Toil();
            finishSummoning.initAction = delegate
            {
                Find.WindowStack.Add(new Window_WishSelection(Map, AltarBuilding));
            };
            finishSummoning.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return finishSummoning;
        }
    }
}
