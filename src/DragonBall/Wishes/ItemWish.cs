using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace DragonBall
{
    public class ItemWish : BaseWish
    {
        public ThingDef thingDef;
        public int count;
        public QualityCategory quality;
        public override string Label => $"Wish for {count}x {thingDef.label}";

        public override IEnumerable<BaseWish> GenerateWishes(Map map, Building_DragonBallAltar altar, Pawn Pawn)
        {
            if (def is ItemWishDef itemWishDef)
            {
                foreach (var option in itemWishDef.itemOptions)
                {
                    yield return new ItemWish
                    {
                        def = this.def,
                        thingDef = option.thing,
                        count = option.count,
                        quality = option.quality
                    };
                }
            }
        }

        public override Texture2D GetIcon()
        {
            return thingDef != null && thingDef.uiIcon != null ? thingDef.uiIcon : base.GetIcon();
        }

        public override bool CanBeGranted(Map map, Building_DragonBallAltar altar, Pawn pawn) => map != null;

        public override void Grant(Map map, Building_DragonBallAltar altar, Pawn pawn)
        {
            Thing thing = ThingMaker.MakeThing(thingDef);
            thing.stackCount = count;

            if (CanHaveQuality(thingDef))
            {
                CompQuality compQuality = thing.TryGetComp<CompQuality>();
                if (compQuality != null)
                {
                    compQuality.SetQuality(quality, ArtGenerationContext.Colony);
                }
            }

            GenPlace.TryPlaceThing(thing, altar.Position, map, ThingPlaceMode.Near);
        }
        private bool CanHaveQuality(ThingDef def)
        {
            return def.HasComp(typeof(CompQuality));
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref thingDef, "thingDef");
            Scribe_Values.Look(ref count, "count");
        }
    }
}
