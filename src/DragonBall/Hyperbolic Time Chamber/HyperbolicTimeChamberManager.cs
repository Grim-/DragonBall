//using RimWorld.Planet;
//using System.Collections.Generic;
//using UnityEngine;
//using Verse;

//namespace DragonBall
//{
//    public class HyperbolicTimeChamberManager : GameComponent
//    {
//        private Dictionary<HyperbolicTimeChamberBuilding, HyperbolicMapParent> Map = new Dictionary<HyperbolicTimeChamberBuilding, HyperbolicMapParent>();

//        public HyperbolicTimeChamberManager(Game game) : base()
//        {

//        }


//        public Map CreateHyperbolicDimension(HyperbolicTimeChamberBuilding HyperbolicTimeChamberBuilding, Map sourceMap, IntVec3 CHAMBER_SIZE, int PLATFORM_RADIUS, int MINIMUM_VOID_BORDER)
//        {
//            // Calculate minimum required map size based on platform radius
//            int minimumMapSize = (PLATFORM_RADIUS * 2) + (MINIMUM_VOID_BORDER * 2);

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


//            Find.World.info.seedString = originalSeed;

//            return pocketMap;
//        }

//        public void DestroyHyperbolicDimension(Map map)
//        {
//            PocketMapUtility.DestroyPocketMap(map);
//        }
//    }
//}
