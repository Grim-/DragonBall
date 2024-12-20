﻿using RimWorld;
using Verse;

namespace DragonBall
{
    public class ItemOption : IExposable
    {
        public ThingDef thing;
        public QualityCategory quality = QualityCategory.Good;
        public int count;
        public void ExposeData()
        {
            Scribe_Defs.Look(ref thing, "itemOption");
            Scribe_Values.Look(ref quality, "quality");
            Scribe_Values.Look(ref count, "count");
        }
    }
}
