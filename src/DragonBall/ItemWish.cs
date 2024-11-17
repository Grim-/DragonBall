using System.Collections.Generic;
using Verse;

namespace DragonBall
{
    public class ItemWish : BaseWish
    {
        public ThingDef thingDef;
        public int count;

        public override string Label => $"Wish for {count}x {thingDef.label}";

        public override IEnumerable<BaseWish> GenerateWishes(Map map, Building_DragonBallAltar altar)
        {
            // Generate wishes based on def settings
            if (def is ItemWishDef itemWishDef)
            {
                foreach (var option in itemWishDef.itemOptions)
                {
                    yield return new ItemWish
                    {
                        def = this.def,
                        thingDef = option.thing,
                        count = option.count
                    };
                }
            }
        }

        public override bool CanBeGranted(Map map, Building_DragonBallAltar altar) => map != null;

        public override void Grant(Map map, Building_DragonBallAltar altar)
        {
            Thing thing = ThingMaker.MakeThing(thingDef);
            thing.stackCount = count;
            GenPlace.TryPlaceThing(thing, altar.Position, map, ThingPlaceMode.Near);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref thingDef, "thingDef");
            Scribe_Values.Look(ref count, "count");
        }
    }
}
