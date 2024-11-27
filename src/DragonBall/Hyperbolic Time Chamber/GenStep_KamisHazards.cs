using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace DragonBall
{
    public class GenStep_KamisHazards : CircularPlatformGenStep
    {
        public override int SeedPart => 7654321;
        private const float HAZARD_DENSITY = 0.05f;

        private static readonly List<ThingDef> possibleHazards = new List<ThingDef>
        {
            ThingDefOf.TrapSpike,
            ThingDefOf.SteamGeyser,
        };

        private static readonly List<FleckDef> possibleFlecks = new List<FleckDef>
        {
            FleckDefOf.Smoke,
            FleckDefOf.AirPuff
        };

        public override void Generate(Map map, GenStepParams parms)
        {
            HyperbolicMapParent hyperbolicMap = map.Parent as HyperbolicMapParent;
            if (hyperbolicMap == null) return;

            IntVec3 center = GetMapCenter(map);
            List<IntVec3> validPositions = GetValidPlatformPositions(map, center);

            int hazardCount = Mathf.RoundToInt(validPositions.Count * HAZARD_DENSITY);

            for (int i = 0; i < hazardCount && validPositions.Count > 0; i++)
            {
                IntVec3 position = validPositions.RandomElement();
                validPositions.Remove(position);

                if (Rand.Value > 0.5f)
                {
                    ThingDef hazardDef = possibleHazards.RandomElement();
                    Thing hazard = ThingMaker.MakeThing(hazardDef, hazardDef.stuffProps != null ? ThingDefOf.Steel : null);
                    GenSpawn.Spawn(hazard, position, map, Rot4.Random);
                }
                else
                {
                    FleckDef fleckDef = possibleFlecks.RandomElement();
                    map.flecks.CreateFleck(new FleckCreationData
                    {
                        def = fleckDef,
                        spawnPosition = position.ToVector3Shifted(),
                        scale = Rand.Range(0.5f, 1.5f),
                        rotationRate = Rand.Range(-30f, 30f)
                    });
                }
            }
        }
    }
}
