using RimWorld;
using Verse;

namespace DragonBall
{
    public class GenStep_KamisPlatform : CircularPlatformGenStep
    {
        public override int SeedPart => 1234567;

        public override void Generate(Map map, GenStepParams parms)
        {
            HyperbolicMapParent hyperbolicMap = map.Parent as HyperbolicMapParent;
            if (hyperbolicMap == null) return;

            IntVec3 center = GetMapCenter(map);

            // Create the platform and surrounding area
            ProcessCircularArea(map, center, (current, isInside) => {
                TerrainDef platformTerrain = isInside
                    ? DefDatabase<TerrainDef>.GetNamed("KamiTile")
                    : DefDatabase<TerrainDef>.GetNamed("KamiSkyTile");

                map.terrainGrid.SetTerrain(current, platformTerrain);

                if (!isInside)
                {
                    map.roofGrid.SetRoof(current, RoofDefOf.RoofRockThick);
                }
            });

            // Place edge decorations
            foreach (IntVec3 current in map.AllCells)
            {
                if (IsOnPlatformEdge(current, center))
                {
                    Thing decoration = ThingMaker.MakeThing(ThingDefOf.Column);
                    GenSpawn.Spawn(decoration, current, map);
                }
            }
        }
    }
}
