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

        public DragonBallWishTracker(Game game) : base()
        {
            coloniesWishData = new Dictionary<int, ColonyWishData>();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref coloniesWishData, "coloniesWishData", LookMode.Value, LookMode.Deep);
            if (coloniesWishData == null)
            {
                coloniesWishData = new Dictionary<int, ColonyWishData>();
            }
        }

        public int GetCooldownTicksRemaining(Map map)
        {
            if (map == null) return 0;
            int colonyIndex = map.Tile;

            if (!coloniesWishData.ContainsKey(colonyIndex))
            {
                return 0;
            }

            int timeSinceLastUse = Find.TickManager.TicksGame - coloniesWishData[colonyIndex].LastDragonBallUseTick;
            return System.Math.Max(0, CooldownTicks - timeSinceLastUse);
        }

        public bool IsWishOffCooldown(Map map)
        {
            if (map == null) return false;
            int colonyIndex = map.Tile;

            if (!coloniesWishData.ContainsKey(colonyIndex))
            {
                return true;
            }

            return GetCooldownTicksRemaining(map) <= 0;
        }

        public void ResetCooldowns()
        {
            // Set LastDragonBallUseTick to a time that ensures cooldown has expired
            int resetTime = Find.TickManager.TicksGame - CooldownTicks - 1;
            foreach (var item in coloniesWishData)
            {
                item.Value.LastDragonBallUseTick = resetTime;
            }
        }

        public void ResetWishes()
        {
            foreach (var item in coloniesWishData)
            {
                item.Value.RemainingWishes = MaximumWishesPerGathering;
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
