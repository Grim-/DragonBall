using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace DragonBall
{
    public class JobDriver_GatherDragonBalls : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            // Need to reserve both the dragon ball and the altar
            return pawn.Reserve(TargetA, job, 1, -1, null, errorOnFailed) &&
                   pawn.Reserve(TargetB, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Building_DragonBallAltar altar = (Building_DragonBallAltar)TargetB.Thing;

            this.FailOn(() =>
            {
                return altar.HasDragonBallOfTypeAlready(TargetThingA) || altar == null;

            });

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch)
                .FailOnDespawnedOrNull(TargetIndex.A);

            this.job.count = 1;


            yield return Toils_Haul.StartCarryThing(TargetIndex.A);

            Toil GoTo = Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch)
                .FailOnDespawnedOrNull(TargetIndex.B);

            yield return GoTo;

            Toil Store = Toils_General.Wait(300, TargetIndex.B);

            yield return Store;

            Store.AddFinishAction(() =>
            {
                Thing carriedThing = pawn.carryTracker.CarriedThing;
                pawn.carryTracker.TryDropCarriedThing(altar.GetDragonBallPosition(carriedThing.def), ThingPlaceMode.Direct, out Thing DroppedThing);

                // Find next dragon ball
                Thing nextDragonBall = Map.listerThings.AllThings
                    .Where(t => t.def.thingCategories?.Contains(DBDefOf.DragonBallsCategory) ?? false)
                    .Where(t => !altar.HasDragonBallOfTypeAlready(t))
                    .Where(t => !t.IsForbidden(pawn))
                    .OrderBy(t => t.Position.DistanceTo(pawn.Position))
                    .FirstOrDefault();

                if (nextDragonBall != null)
                {
                    Job nextJob = JobMaker.MakeJob(DBDefOf.GatherDragonBalls, nextDragonBall, altar);
                    pawn.jobs.TryTakeOrderedJob(nextJob, JobTag.DraftedOrder, true);
                }
            });

 
        }

    }
}
