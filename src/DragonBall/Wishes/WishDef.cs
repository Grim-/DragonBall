using System;
using System.Collections.Generic;
using Verse;

namespace DragonBall
{
    public class WishDef : Def
    {
        public Type wishClass;
        public int silverAmount;
        public bool requiresTargetPawn;
        public string iconPath;
        public string category = "Default";
        public int wishCost = 1;
    }
}
