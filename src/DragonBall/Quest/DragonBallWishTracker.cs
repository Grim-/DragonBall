using RimWorld;
using System.Collections.Generic;
using Verse;

namespace DragonBall
{
    public class DragonBallWishTracker : GameComponent
    {
        private Dictionary<int, ColonyWishData> coloniesWishData = new Dictionary<int, ColonyWishData>();

        public int MaximumWishesPerGathering = 3;
        public int CooldownTicks => 1 * GenDate.TicksPerYear;

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


        public int GetCooldownTicksRemaining(Map map)
        {
            if (map == null) return 0;

            int colonyIndex = map.Tile;
            if (coloniesWishData.ContainsKey(colonyIndex))
            {
                return Find.TickManager.TicksGame - coloniesWishData[colonyIndex].LastDragonBallUseTick + CooldownTicks;
            }

            return 0;
        }

        public bool IsWishOffCooldown(Map map)
        {
            if (map == null) return false;

            int colonyIndex = map.Tile;
            EnsureColonyDataExists(colonyIndex);

            return Find.TickManager.TicksGame > coloniesWishData[colonyIndex].LastDragonBallUseTick + CooldownTicks;
        }
        public void ResetWishes()
        {
            foreach (var item in coloniesWishData)
            {
                item.Value.RemainingWishes = 3;
            }
        }

        public void ResetCooldowns()
        {
            foreach (var item in coloniesWishData)
            {
                item.Value.LastDragonBallUseTick = 1;
            }
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
