using System.Collections.Generic;
using Verse;

namespace DragonBall
{
    public abstract class BaseWish : IExposable
    {
        public WishDef def;

        public virtual string Label => def.label;
        public virtual string Description => def.description;

        public abstract bool CanBeGranted(Map map, Building_DragonBallAltar altar);
        public abstract void Grant(Map map, Building_DragonBallAltar altar);

        public virtual void ExposeData()
        {
            Scribe_Defs.Look(ref def, "wishDef");
        }

        // This is the key method - each wish type will generate its concrete instances
        public abstract IEnumerable<BaseWish> GenerateWishes(Map map, Building_DragonBallAltar altar);
    }
}
