using Verse;

namespace DragonBall
{
    public class ColonyWishData : IExposable
    {
        public int LastDragonBallUseTick;
        public int RemainingWishes;

        public void ExposeData()
        {
            Scribe_Values.Look(ref LastDragonBallUseTick, "lastDragonBallUseTick");
            Scribe_Values.Look(ref RemainingWishes, "remainingWishes");
        }
    }
}
