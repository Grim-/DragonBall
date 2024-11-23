using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace DragonBall
{
    public class CompProperties_TrainingDummy : CompProperties
    {
        public float xpPerHour = 4000f; // Base XP gain per hour
        public int minTrainingHours = 1;
        public int maxTrainingHours = 8;

        public CompProperties_TrainingDummy()
        {
            compClass = typeof(Comp_TrainingDummy);
        }
    }

    // ThingComp
    public class Comp_TrainingDummy : ThingComp
    {
        private int ticksToNextAttack = 0;
        private static readonly int TicksBetweenAttacks = 150; // About 2.5 seconds

        public CompProperties_TrainingDummy Props => (CompProperties_TrainingDummy)props;

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn pawn)
        {
            if (!pawn.CanReach(parent, PathEndMode.InteractionCell, Danger.None))
            {
                yield return new FloatMenuOption("Cannot reach", null);
                yield break;
            }

            for (int hours = Props.minTrainingHours; hours <= Props.maxTrainingHours; hours++)
            {
                string label = $"Train for {hours} hour{(hours > 1 ? "s" : "")}";
                Action action = () => {
                    Job job = JobMaker.MakeJob(DBDefOf.DragonBallTrainOnDummy, parent);
                    job.SetTarget(TargetIndex.A, this.parent);
                    job.SetTarget(TargetIndex.B, pawn);
                    job.expiryInterval = hours * GenDate.TicksPerHour;
                    pawn.jobs.TryTakeOrderedJob(job);
                };
                yield return new FloatMenuOption(label, action);
            }
        }

        public void ResetAttackTimer()
        {
            ticksToNextAttack = TicksBetweenAttacks;
        }

        public bool ShouldAttackNow()
        {
            ticksToNextAttack--;
            if (ticksToNextAttack <= 0)
            {
                ResetAttackTimer();
                return true;
            }
            return false;
        }

        //public override void CompTick()
        //{
        //    base.CompTick();
        //    if (ticksToNextAttack > 0)
        //        ticksToNextAttack--;
        //}
    }
}
