using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace DragonBall
{
    public class HealAllWish : BaseWish
    {
        public override IEnumerable<BaseWish> GenerateWishes(Map map, Building_DragonBallAltar altar)
        {
            // This type only generates one instance if there are injured pawns
            if (map.mapPawns.FreeColonists.Any(p => p.health.hediffSet.hediffs.Any()))
            {
                yield return new HealAllWish { def = this.def };
            }
        }

        public override bool CanBeGranted(Map map, Building_DragonBallAltar altar) => true;

        public override void Grant(Map map, Building_DragonBallAltar altar)
        {
            foreach (Pawn p in map.mapPawns.FreeColonists)
            {
                p.health.RemoveAllHediffs();
            }
            Messages.Message("All colonists have been healed!", MessageTypeDefOf.PositiveEvent);
        }
    }
}
