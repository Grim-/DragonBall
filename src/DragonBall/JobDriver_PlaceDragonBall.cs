using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace DragonBall
{
    public class JobDriver_PlaceDragonBall : JobDriver
    {
        protected Thing DragonBall => job.targetA.Thing;
        protected Thing Altar => job.targetB.Thing;

        protected Building_DragonBallAltar AltarBuilding => (Building_DragonBallAltar)Altar;


        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(DragonBall, job, 1, -1, null, errorOnFailed) &&
                   pawn.Reserve(Altar, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            // Go to placement spot
            yield return Toils_Goto.GotoCell(AltarBuilding.GetDragonBallPosition(DragonBall.def), PathEndMode.ClosestTouch)
                .FailOnDespawnedNullOrForbidden(TargetIndex.B);

            // Place dragon ball
            Toil place = new Toil();
            place.initAction = () =>
            {
                if (pawn.inventory.Contains(DragonBall))
                {
                    pawn.inventory.innerContainer.TryDrop(DragonBall, ThingPlaceMode.Direct, out Thing droppedThing);
                }
            };
            place.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return place;
        }
    }
}
