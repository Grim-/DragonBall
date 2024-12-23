//using RimWorld;
//using RimWorld.Planet;
//using UnityEngine;
//using Verse;

//namespace DragonBall
//{

//    public static class HyperbolicMapUtility
//    {
//        private static readonly IntVec3 CHAMBER_SIZE = new IntVec3(100, 1, 100);
//        public const int PLATFORM_RADIUS = 25;
//        public const int MINIMUM_VOID_BORDER = 20;

//        public static Map CreateHyperbolicDimension(Map sourceMap)
//        {
//            // Calculate minimum required map size based on platform radius
//            int minimumMapSize = (PLATFORM_RADIUS * 2) + (MINIMUM_VOID_BORDER * 2);

//            // Ensure map size is at least the minimum required
//            IntVec3 mapSize = new IntVec3(
//                Mathf.Max(CHAMBER_SIZE.x, minimumMapSize),
//                1,
//                Mathf.Max(CHAMBER_SIZE.z, minimumMapSize)
//            );

//            var mapParent = (HyperbolicMapParent)WorldObjectMaker.MakeWorldObject(
//                DBDefOf.HyperbolicMapParent
//            );
//            mapParent.Tile = sourceMap.Tile;
//            Find.WorldObjects.Add(mapParent);

//            // Save and modify world seed for generation
//            string originalSeed = Find.World.info.seedString;
//            Find.World.info.seedString = Find.TickManager.TicksAbs.ToString();

//            Map pocketMap = MapGenerator.GenerateMap(
//                CHAMBER_SIZE,
//                mapParent,
//                DBDefOf.KamisLookoutGenerator,
//                null,
//                null,
//                true
//            );

//            // Restore original seed
//            Find.World.info.seedString = originalSeed;

//            return pocketMap;
//        }

//        public static void DestroyHyperbolicDimension(Map map)
//        {
//            // Use the built-in destroy method
//            PocketMapUtility.DestroyPocketMap(map);
//        }

//        // Helper method to check if a position is within the platform area
//        public static bool IsOnPlatform(IntVec3 position, Map map)
//        {
//            IntVec3 center = new IntVec3(map.Size.x / 2, 0, map.Size.z / 2);
//            float distanceFromCenter = Vector3.Distance(position.ToVector3(), center.ToVector3());
//            return distanceFromCenter <= PLATFORM_RADIUS;
//        }
//    }
//}
