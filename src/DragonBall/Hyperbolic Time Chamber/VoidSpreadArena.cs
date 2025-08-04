using System.Collections.Generic;
using Verse;

namespace DragonBall
{
    public class VoidSpreadArena : ArenaMutator
    {
        private HashSet<IntVec3> voidTiles = new HashSet<IntVec3>();

        public VoidSpreadArena(Map map) : base(map)
        {
            ticksBetweenMutations = 1200;
        }

        protected override void ApplyMutation()
        {
            IntVec3 center = GetMapCenter();
            List<IntVec3> potentialNewVoids = new List<IntVec3>();

            if (voidTiles.Count == 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    IntVec3 randomCell = CellFinder.RandomCell(map);
                    if (IsInsidePlatform(randomCell, center))
                    {
                        potentialNewVoids.Add(randomCell);
                    }
                }
            }
            else
            {
                //foreach (IntVec3 voidTile in voidTiles)
                //{
                //    foreach (IntVec3 adj in GenAdj.RandomAdjacentCellCardinal(voidTile))
                //    {
                //        if (IsInsidePlatform(adj, center) && !voidTiles.Contains(adj))
                //        {
                //            potentialNewVoids.Add(adj);
                //        }
                //    }
                //}
            }

            foreach (IntVec3 cell in potentialNewVoids)
            {
                if (Rand.Value < 0.3f && !voidTiles.Contains(cell))
                {
                    SafelyHandlePawns(cell);
                    map.terrainGrid.SetTerrain(cell, DefDatabase<TerrainDef>.GetNamed("KamiVoidTile"));
                    voidTiles.Add(cell);
                }
            }
        }
    }
}
