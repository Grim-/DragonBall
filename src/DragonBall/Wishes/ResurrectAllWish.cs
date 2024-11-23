using RimWorld;
using System.Collections.Generic;
using Verse;

namespace DragonBall
{
    public class ResurrectAllWish : PawnTargetWish
    {
        public ResurrectAllWish()
        {
        }

        public ResurrectAllWish(WishDef def, Pawn target) : base(def, target)
        {

        }

        public override string Label => $"Resurrect Everyone";

        public override string Description => $"Bring them all back to life";

        public override bool CanBeGranted(Map map, Building_DragonBallAltar altar, Pawn pawn) => ResurrectWish.GetDeadFactionMapPawns(map).Count > 0;

        public override void Grant(Map map, Building_DragonBallAltar altar, Pawn pawn)
        {
            foreach (var item in ResurrectWish.GetDeadFactionMapPawns(map))
            {
                if (ResurrectionUtility.TryResurrect(item, new ResurrectionParams
                {
                    gettingScarsChance = 0f,
                    canKidnap = false,
                    canTimeoutOrFlee = false,
                    useAvoidGridSmart = true,
                    canSteal = false,
                    invisibleStun = true,
                    restoreMissingParts = true,
                    removeDiedThoughts  = true,                    
                }))
                {
                    Messages.Message($"{Target.LabelShort} has been resurrected!", MessageTypeDefOf.PositiveEvent);
                }
            }

        
          
        }

        public override IEnumerable<BaseWish> GenerateWishes(Map map, Building_DragonBallAltar altar, Pawn pawn)
        {
            return new List<BaseWish>() { new ResurrectAllWish(def, null) };
        }
    }
}
