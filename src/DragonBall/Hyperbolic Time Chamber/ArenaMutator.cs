using UnityEngine;
using Verse;

namespace DragonBall
{
    public class ArenaMutator
    {
        protected int ticksBetweenMutations = 2500;
        protected int ticksUntilNextMutation;
        protected Map map;
        protected const int PLATFORM_RADIUS = 25;

        public ArenaMutator(Map map)
        {
            this.map = map;
            this.ticksUntilNextMutation = ticksBetweenMutations;
        }

        protected IntVec3 GetMapCenter() => new IntVec3(map.Size.x / 2, 0, map.Size.z / 2);

        protected bool IsInsidePlatform(IntVec3 pos, IntVec3 center)
        {
            return Vector3.Distance(pos.ToVector3(), center.ToVector3()) <= PLATFORM_RADIUS;
        }

        protected void SafelyHandlePawns(IntVec3 cell)
        {
            //List<Thing> things = cell.GetThingList(map).ToList();
            //foreach (Thing thing in things)
            //{
            //    if (thing is Pawn pawn)
            //    {
            //        if (CellFinder.tr(cell, map, out IntVec3 safeSpot))
            //        {
            //            pawn.Position = safeSpot;
            //            pawn.Notify_Teleported(true, false);
            //        }
            //    }
            //}
        }

        public virtual void Tick()
        {
            ticksUntilNextMutation--;
            if (ticksUntilNextMutation <= 0)
            {
                ApplyMutation();
                ticksUntilNextMutation = ticksBetweenMutations;
            }
        }

        protected virtual void ApplyMutation() { }
    }
}
