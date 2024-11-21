using System.Collections.Generic;
using System.Linq;
using Verse;

namespace DragonBall
{
    public class RelationsWish : BaseWish
    {
        public RelationsWish()
        {

        }

        public RelationsWish(WishDef def) : base(def)
        {

        }

        private RelationsWishDef Def => (RelationsWishDef)def;



        public override bool CanBeGranted(Map map, Building_DragonBallAltar altar, Pawn pawn)
        {
            return pawn != null && Def != null;
        }

        public override IEnumerable<BaseWish> GenerateWishes(Map map, Building_DragonBallAltar altar, Pawn pawn)
        {
            if (Def != null)
            {
                yield return new RelationsWish(Def);
            }
        }

        public override void Grant(Map map, Building_DragonBallAltar altar, Pawn pawn)
        {
            if (pawn?.Faction == null) return;

            List<Pawn> factionPawns = map.mapPawns.FreeColonistsSpawned.ToList();

            foreach (Pawn otherPawn in factionPawns)
            {
                if (otherPawn != pawn && otherPawn.needs?.mood?.thoughts?.memories != null)
                {
                    otherPawn.needs.mood.thoughts.memories.TryGainMemory(Def.thoughtDef, pawn);
                }
            }
        }
    }
}
