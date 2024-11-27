using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace DragonBall
{
    public class TrapArenaMutator
    {
        private readonly Map map;
        private int ticksBetweenMutations = 2500;
        private int ticksUntilNextMutation;
        private const int PLATFORM_RADIUS = 25;
        private HashSet<IntVec3> trapLocations = new HashSet<IntVec3>();

        public TrapArenaMutator(Map map)
        {
            this.map = map;
            this.ticksUntilNextMutation = ticksBetweenMutations;
        }

        public void Tick()
        {
            ticksUntilNextMutation--;
            if (ticksUntilNextMutation <= 0)
            {
                PlaceTraps();
                ticksUntilNextMutation = ticksBetweenMutations;
            }
        }

        private void PlaceTraps()
        {
            IntVec3 center = new IntVec3(map.Size.x / 2, 0, map.Size.z / 2);
            List<IntVec3> validCells = new List<IntVec3>();

            // Find valid cells for trap placement
            foreach (IntVec3 cell in map.AllCells)
            {
                if (IsValidTrapLocation(cell, center))
                {
                    validCells.Add(cell);
                }
            }

            // Place 1-3 traps
            int trapsToPlace = Rand.RangeInclusive(1, 3);
            for (int i = 0; i < trapsToPlace && validCells.Count > 0; i++)
            {
                IntVec3 selectedCell = validCells.RandomElement();
                validCells.Remove(selectedCell);

                // Move any pawns out of the way
                //List<Thing> things = selectedCell.GetThingList(map).ToList();
                //foreach (Thing thing in things)
                //{
                //    if (thing is Pawn pawn)
                //    {
                //        if (CellFinder.TryFindRandomSpotJustOutsideOf(selectedCell, map, out IntVec3 safeSpot))
                //        {
                //            pawn.Position = safeSpot;
                //            pawn.Notify_Teleported(true, false);
                //        }
                //    }
                //}

                // Place the trap
                Thing trap = ThingMaker.MakeThing(ThingDefOf.TrapSpike, ThingDefOf.Steel);
                GenSpawn.Spawn(trap, selectedCell, map);
                trapLocations.Add(selectedCell);
            }
        }

        private bool IsValidTrapLocation(IntVec3 cell, IntVec3 center)
        {
            return Vector3.Distance(cell.ToVector3(), center.ToVector3()) <= PLATFORM_RADIUS &&
                   cell.Standable(map) &&
                   !cell.GetThingList(map).Any() &&
                   !trapLocations.Contains(cell);
        }
    }
}
