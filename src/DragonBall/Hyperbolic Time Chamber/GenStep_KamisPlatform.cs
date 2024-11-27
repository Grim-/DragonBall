using RimWorld;
using UnityEngine;
using Verse;

namespace DragonBall
{
    public class GenStep_KamisPlatform : GenStep
    {
        public override int SeedPart => 1234567;
        private const int PLATFORM_RADIUS = 25;
        private float EDGE_RADIUS = PLATFORM_RADIUS - 1f;

        public override void Generate(Map map, GenStepParams parms)
        {
            HyperbolicMapParent hyperbolicMap = map.Parent as HyperbolicMapParent;
            if (hyperbolicMap == null) return;

            // Get the center of the map
            IntVec3 center = new IntVec3(map.Size.x / 2, 0, map.Size.z / 2);

            // Create the circular platform
            for (int x = 0; x < map.Size.x; x++)
            {
                for (int z = 0; z < map.Size.z; z++)
                {
                    IntVec3 current = new IntVec3(x, 0, z);
                    float distanceFromCenter = Vector3.Distance(current.ToVector3(), center.ToVector3());

                    if (distanceFromCenter <= PLATFORM_RADIUS)
                    {
                        // Inside the platform radius
                        TerrainDef platformTerrain = DefDatabase<TerrainDef>.GetNamed("KamiTile");
                        map.terrainGrid.SetTerrain(current, platformTerrain);


                    }
                    else
                    {

                        TerrainDef platformTerrain = DefDatabase<TerrainDef>.GetNamed("KamiSkyTile");
                        map.terrainGrid.SetTerrain(current, platformTerrain);
                        map.roofGrid.SetRoof(current, RoofDefOf.RoofRockThick);
                    }
                }
            }


            for (int x = 0; x < map.Size.x; x++)
            {
                for (int z = 0; z < map.Size.z; z++)
                {
                    IntVec3 current = new IntVec3(x, 0, z);
                    float distanceFromCenter = Vector3.Distance(current.ToVector3(), center.ToVector3());

                    if (Mathf.Abs(distanceFromCenter - EDGE_RADIUS) < 0.5f)
                    {
                        Thing decoration = ThingMaker.MakeThing(ThingDefOf.Column);
                        GenSpawn.Spawn(decoration, current, map);
                    }
                }
            }

            //place exit


            //place
        }
    }
}
