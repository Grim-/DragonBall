using RimWorld;
using RimWorld.QuestGen;
using System.Collections.Generic;
using Verse;

namespace DragonBall
{
    public class QuestWish : BaseWish
    {
        public QuestWishOption questDef;
        private QuestWishDef QuestDef => def as QuestWishDef;

        public override string Label => $"{questDef.questLabel}";

        public override string Description => $"{questDef.questDescription}";

        public override IEnumerable<BaseWish> GenerateWishes(Map map, Building_DragonBallAltar altar, Pawn pawn)
        {
            if (QuestDef?.availableQuests != null)
            {
                foreach (var quest in QuestDef.availableQuests)
                {
                    yield return new QuestWish
                    {
                        def = this.def,
                        questDef = quest
                    };
                }
            }
        }

        public override bool CanBeGranted(Map map, Building_DragonBallAltar altar, Pawn pawn)
        {
            return questDef != null;
        }

        public override void Grant(Map map, Building_DragonBallAltar altar, Pawn pawn)
        {
            Slate slate = new Slate();
            slate.Set("map", map);
            slate.Set("points", StorytellerUtility.DefaultThreatPointsNow(map));

            Quest quest = QuestUtility.GenerateQuestAndMakeAvailable(questDef.quest, slate);
            if (quest != null)
            {
                QuestUtility.SendLetterQuestAvailable(quest);
                Messages.Message($"Your wish has summoned a new quest: {questDef.questLabel}", MessageTypeDefOf.PositiveEvent);
            }
            else
            {
                Messages.Message("The wish could not be granted at this time.", MessageTypeDefOf.RejectInput);
            }
        }
    }
}
