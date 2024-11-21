using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace DragonBall
{
    public abstract class BaseWish : IExposable
    {
        public WishDef def;

        public virtual string Label => def.label;
        public virtual string Description => def.description;

        protected BaseWish()
        {

        }

        protected BaseWish(WishDef def)
        {
            this.def = def;
        }

        public virtual Texture2D GetIcon()
        {
            if (!string.IsNullOrEmpty(def.iconPath))
            {
                return ContentFinder<Texture2D>.Get(def.iconPath, false);
            }

            Texture2D defaultTex = ContentFinder<Texture2D>.Get("UI/Icons/KiBarrier", false);
            return defaultTex;
        }



        public abstract bool CanBeGranted(Map map, Building_DragonBallAltar altar, Pawn pawn);
        public abstract void Grant(Map map, Building_DragonBallAltar altar, Pawn pawn);

        public virtual void ExposeData()
        {
            Scribe_Defs.Look(ref def, "wishDef");
        }

        public abstract IEnumerable<BaseWish> GenerateWishes(Map map, Building_DragonBallAltar altar, Pawn pawn);
    }
}
