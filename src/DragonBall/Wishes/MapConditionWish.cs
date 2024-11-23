using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace DragonBall
{
    public class MapConditionWish : BaseWish
    {
        public MapConditionWish()
        {
        }

        public MapConditionWish(WishDef def) : base(def)
        {
        }

        public override bool CanBeGranted(Map map, Building_DragonBallAltar altar, Pawn pawn)
        {
            // Check if there are any negative map conditions present
            return map.gameConditionManager.ActiveConditions
                .Any(condition => IsNegativeCondition(condition.def));
        }

        public override IEnumerable<BaseWish> GenerateWishes(Map map, Building_DragonBallAltar altar, Pawn pawn)
        {
            yield return new MapConditionWish(def);
        }

        public override void Grant(Map map, Building_DragonBallAltar altar, Pawn pawn)
        {
            var conditionsToRemove = map.gameConditionManager.ActiveConditions
                .Where(condition => IsNegativeCondition(condition.def))
                .ToList();

            foreach (var condition in conditionsToRemove)
            {
                condition.End();
            }

            if (conditionsToRemove.Any())
            {
                Messages.Message("The dragon has cleared all negative conditions from the map!", MessageTypeDefOf.PositiveEvent);
            }
        }

        private bool IsNegativeCondition(GameConditionDef def)
        {
            return def == GameConditionDefOf.ToxicFallout ||
                   def == GameConditionDefOf.VolcanicWinter ||
                   def == GameConditionDefOf.PsychicDrone ||
                   def == GameConditionDefOf.Eclipse ||
                   def == GameConditionDefOf.Flashstorm ||
                   def == GameConditionDefOf.HeatWave ||
                   def == GameConditionDefOf.ColdSnap ||
                   def == GameConditionDefOf.ToxicFallout ||
                   def == GameConditionDefOf.WeatherController;
        }
    }
}
