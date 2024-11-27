using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace DragonBall
{
    public class PowerLevelMapComponent : MapComponent
    {
        public PowerLevelMapComponent(Map map) : base(map)
        {
        }


        public override void MapComponentOnGUI()
        {
            base.MapComponentOnGUI();

            foreach (Pawn pawn in map.mapPawns.FreeColonistsSpawned)
            {
                var scouter = pawn.apparel?.WornApparel
                    ?.FirstOrDefault(x => x is Scouter) as Scouter;

                if (scouter != null)
                {
                    scouter.DrawExtraGUI();
                }
            }
        }

    }
}
