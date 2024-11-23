using System.Collections.Generic;
using Verse;

namespace DragonBall
{
    public class DragonBallWishTracker : GameComponent
    {
        // Dictionary to track per-colony dragon ball usage
        private Dictionary<int, ColonyWishData> coloniesWishData = new Dictionary<int, ColonyWishData>();

        // Settings that could be exposed to mod settings later
        public int MaximumWishesPerGathering = 3;
        public int CooldownTicks = 60000;

        public DragonBallWishTracker(Game game)
        {
            coloniesWishData = new Dictionary<int, ColonyWishData>();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref coloniesWishData, "coloniesWishData", LookMode.Value, LookMode.Deep);

            // Initialize if null after loading
            if (coloniesWishData == null)
            {
                coloniesWishData = new Dictionary<int, ColonyWishData>();
            }
        }

        public bool IsWishOffCooldown(Map map)
        {
            if (map == null) return false;

            int colonyIndex = map.Tile;
            EnsureColonyDataExists(colonyIndex);

            return Find.TickManager.TicksGame > coloniesWishData[colonyIndex].LastDragonBallUseTick + CooldownTicks;
        }

        public int GetRemainingWishes(Map map)
        {
            if (map == null) return 0;

            int colonyIndex = map.Tile;
            EnsureColonyDataExists(colonyIndex);

            return coloniesWishData[colonyIndex].RemainingWishes;
        }

        public void UseWish(Map map)
        {
            if (map == null) return;

            int colonyIndex = map.Tile;
            EnsureColonyDataExists(colonyIndex);

            ColonyWishData data = coloniesWishData[colonyIndex];
            data.RemainingWishes--;

            if (data.RemainingWishes <= 0)
            {
                data.LastDragonBallUseTick = Find.TickManager.TicksGame;
                data.RemainingWishes = 0;
            }
        }

        public void InitializeWishes(Map map)
        {
            if (map == null) return;

            int colonyIndex = map.Tile;
            EnsureColonyDataExists(colonyIndex);

            coloniesWishData[colonyIndex].RemainingWishes = MaximumWishesPerGathering;
        }

        private void EnsureColonyDataExists(int colonyIndex)
        {
            if (!coloniesWishData.ContainsKey(colonyIndex))
            {
                coloniesWishData[colonyIndex] = new ColonyWishData();
            }
        }
    }
}
