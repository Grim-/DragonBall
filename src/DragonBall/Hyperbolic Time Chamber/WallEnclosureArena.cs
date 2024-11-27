using RimWorld;
using UnityEngine;
using Verse;

namespace DragonBall
{
    // Example implementation: Closing walls
    public class WallEnclosureArena : ArenaMutator
    {
        private int currentLayer = 0;
        private readonly int maxLayers;

        public WallEnclosureArena(Map map, int maxLayers = 5) : base(map)
        {
            this.maxLayers = maxLayers;
            ticksBetweenMutations = 6000;
        }

        protected override void ApplyMutation()
        {
            if (currentLayer >= maxLayers) return;

            IntVec3 center = GetMapCenter();
            float radius = PLATFORM_RADIUS - currentLayer;

            foreach (IntVec3 cell in map.AllCells)
            {
                float distance = Vector3.Distance(cell.ToVector3(), center.ToVector3());
                if (Mathf.Abs(distance - radius) < 0.5f)
                {
                    SafelyHandlePawns(cell);
                    GenSpawn.Spawn(ThingDefOf.Wall, cell, map);
                }
            }

            currentLayer++;
        }
    }
}
