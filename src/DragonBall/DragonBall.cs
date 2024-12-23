using UnityEngine;
using Verse;

namespace DragonBall
{
    [StaticConstructorOnStartup]
    public class DragonBall : Mod
    {
        private TournamentTracker tracker;
        private DragonBallModSettings settings;

        public DragonBall(ModContentPack content) : base(content)
        {
            settings = GetSettings<DragonBallModSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            if (tracker == null)
            {
                tracker = Current.Game.GetComponent<TournamentTracker>();
            }

            if (tracker == null)
            {
                return;
            }

            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            if (listingStandard.ButtonText("View Tournament History") && Current.Game != null)
            {
                tracker.OpenHistoryWindow();
            }

            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Dragon Ball Addon";
        }
    }
}
