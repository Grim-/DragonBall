using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System.Collections.Generic;
using Verse;

namespace DragonBall
{
    public class QuestNode_CreateRandomReward : QuestNode
    {
        public SlateRef<List<ThingDef>> rewardOptions;
        public SlateRef<IntRange> countRange;

        protected override void RunInt()
        {
            Slate slate = QuestGen.slate;
            QuestPart_RandomReward reward = new QuestPart_RandomReward
            {
                inSignal = QuestGenUtility.HardcodedSignalWithQuestID("site.MapGenerated"),
                possibleRewards = rewardOptions.GetValue(slate),
                countRange = countRange.GetValue(slate),
                siteID = slate.Get<Site>("site").ID  // Store the site ID here
            };
            QuestGen.quest.AddPart(reward);
        }

        protected override bool TestRunInt(Slate slate)
        {
            return !rewardOptions.GetValue(slate).NullOrEmpty();
        }
    }

    public class QuestPart_RandomReward : QuestPart
    {
        public string inSignal;
        public List<ThingDef> possibleRewards;
        public IntRange countRange = new IntRange(1, 1);
        public int siteID; 

        public override void Notify_QuestSignalReceived(Signal signal)
        {
            base.Notify_QuestSignalReceived(signal);
            Log.Message($"Random reward quest signal {signal}");

            if (possibleRewards.NullOrEmpty())
            {
                Log.Message($"possibleRewards is null or empty.");
                return;
            }

            if (signal.tag == inSignal)
            {
                Log.Message($"QuestPart_RandomReward correct signal");

                Site site = Find.WorldObjects.Sites.Find(s => s.ID == siteID);
                if (site?.Map == null)
                {
                    Log.Message($"Site or map is null");
                    return;
                }

                ThingDef chosenReward = possibleRewards.RandomElement();
                int count = countRange.RandomInRange;
                Thing reward = ThingMaker.MakeThing(chosenReward);
                reward.stackCount = count;

                Map map = site.Map;
                IntVec3 dropSpot = DropCellFinder.RandomDropSpot(map);
                GenPlace.TryPlaceThing(reward, dropSpot, map, ThingPlaceMode.Near);
                Log.Message($"Placing {reward.Label} at {dropSpot}");
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref inSignal, "inSignal");
            Scribe_Collections.Look(ref possibleRewards, "possibleRewards", LookMode.Def);
            Scribe_Values.Look(ref countRange, "countRange");
            Scribe_Values.Look(ref siteID, "siteID"); 
        }
    }
}
