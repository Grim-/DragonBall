﻿using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace DragonBall
{
    public class TraitWish : BaseWish
    {
        public TraitWish()
        {
        }

        public TraitWish(WishDef def) : base(def)
        {

        }

        private TraitWishDef Def => (TraitWishDef)def;


        public override bool CanBeGranted(Map map, Building_DragonBallAltar altar, Pawn pawn)
        {
            return pawn != null && Def != null && Def.trait != null && !pawn.story.traits.HasTrait(Def.trait);
        }

        public override IEnumerable<BaseWish> GenerateWishes(Map map, Building_DragonBallAltar altar, Pawn pawn)
        {
            if (Def != null)
            {
                yield return new TraitWish(def);
            }
        }

        public override void Grant(Map map, Building_DragonBallAltar altar, Pawn pawn)
        {
            if (!pawn.story.traits.HasTrait(Def.trait))
            {
                pawn.story.traits.GainTrait(new Trait(Def.trait));
            }
        }
    }
}