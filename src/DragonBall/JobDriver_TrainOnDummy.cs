using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace DragonBall
{
    // JobDriver
    public class JobDriver_TrainOnDummy : JobDriver
    {
        private const TargetIndex DummyInd = TargetIndex.A;
        private const TargetIndex PawnInd = TargetIndex.B;
        private const int AttackDuration = 80;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.GetTarget(DummyInd), job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            if (TargetPawnB != null && TargetPawnB.drafter.Drafted)
            {
                TargetPawnB.drafter.Drafted = false;
            }
            // Add failure conditions for both the dummy and the pawn
            this.FailOnDestroyedOrNull(DummyInd);              // Dummy is destroyed
            this.FailOnDespawnedOrNull(DummyInd);              // Dummy despawns
            this.FailOnForbidden(DummyInd);                    // Dummy becomes forbidden

            this.FailOnDespawnedOrNull(PawnInd);              // Pawn despawns
            this.FailOnDowned(PawnInd);                        // Pawn goes down
            this.FailOn(() => pawn.Drafted);                   // Pawn gets drafted
            this.FailOn(() => pawn.InMentalState);            // Pawn mental breaks
            this.FailOn(() => !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation));  // Pawn loses manipulation

            var comp = job.GetTarget(DummyInd).Thing.TryGetComp<Comp_TrainingDummy>();

            // Go to the training dummy
            yield return Toils_Goto.GotoThing(DummyInd, PathEndMode.InteractionCell)
                .FailOnDespawnedNullOrForbidden(DummyInd)
                .FailOnDespawnedNullOrForbidden(PawnInd)
                .FailOn(() => !pawn.CanReach(job.GetTarget(DummyInd), PathEndMode.InteractionCell, Danger.Deadly));

            // Training toil
            Toil trainToil = new Toil();
            trainToil.initAction = () =>
            {
                comp.ResetAttackTimer();
                pawn.pather.StopDead();
            };

            trainToil.tickAction = () =>
            {
                // Grant XP
                float xpPerTick = comp.Props.xpPerHour / GenDate.TicksPerHour;
                pawn.skills.GetSkill(SkillDefOf.Melee).Learn(xpPerTick);

                // Perform attack when timer hits
                if (comp.ShouldAttackNow())
                {
                    pawn.Drawer.Notify_MeleeAttackOn(TargetThingA);
                    // Face the target
                    pawn.rotationTracker.FaceTarget(job.GetTarget(DummyInd).Thing);
                }
            };

            trainToil.defaultCompleteMode = ToilCompleteMode.Delay;
            trainToil.defaultDuration = job.expiryInterval;
            trainToil.handlingFacing = true;

            trainToil.WithProgressBar(DummyInd, () =>
                (float)(trainToil.actor.jobs.curDriver.ticksLeftThisToil) / trainToil.defaultDuration);

            trainToil.AddFinishAction(() =>
            {
                if (pawn.Spawned && !pawn.Dead)
                {
                    Messages.Message($"{pawn.LabelShort} finished training session.",
                        MessageTypeDefOf.NeutralEvent);
                }
            });

            // Add all failure conditions to the training toil
            trainToil.FailOnDespawnedNullOrForbidden(DummyInd);
            trainToil.FailOnDespawnedNullOrForbidden(PawnInd);
            trainToil.FailOnCannotTouch(DummyInd, PathEndMode.InteractionCell);
            trainToil.FailOn(() => pawn.Drafted);
            trainToil.FailOn(() => pawn.InMentalState);
            trainToil.FailOn(() => !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation));
            trainToil.FailOn(() => !pawn.CanReach(job.GetTarget(DummyInd), PathEndMode.InteractionCell, Danger.Deadly));

            yield return trainToil;
        }

        public override void ExposeData()
        {
            base.ExposeData();
        }
    }
}
