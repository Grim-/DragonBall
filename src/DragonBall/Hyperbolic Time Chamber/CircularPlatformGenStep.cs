using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace DragonBall
{
    public abstract class CircularPlatformGenStep : GenStep
    {
        protected const int PLATFORM_RADIUS = 25;
        protected readonly float EDGE_RADIUS = PLATFORM_RADIUS - 1f;

        protected IntVec3 GetMapCenter(Map map) => new IntVec3(map.Size.x / 2, 0, map.Size.z / 2);

        protected bool IsInsidePlatform(IntVec3 pos, IntVec3 center)
        {
            return Vector3.Distance(pos.ToVector3(), center.ToVector3()) <= PLATFORM_RADIUS;
        }

        protected bool IsOnPlatformEdge(IntVec3 pos, IntVec3 center)
        {
            return Mathf.Abs(Vector3.Distance(pos.ToVector3(), center.ToVector3()) - EDGE_RADIUS) < 0.5f;
        }

        protected List<IntVec3> GetValidPlatformPositions(Map map, IntVec3 center, bool requireStandable = true)
        {
            List<IntVec3> validPositions = new List<IntVec3>();

            foreach (IntVec3 current in map.AllCells)
            {
                if (IsInsidePlatform(current, center))
                {
                    if (!requireStandable || (current.Standable(map) && !current.GetThingList(map).Any()))
                    {
                        validPositions.Add(current);
                    }
                }
            }

            return validPositions;
        }

        protected void ProcessCircularArea(Map map, IntVec3 center, System.Action<IntVec3, bool> processor)
        {
            foreach (IntVec3 current in map.AllCells)
            {
                bool isInside = IsInsidePlatform(current, center);
                processor(current, isInside);
            }
        }
    }
}
