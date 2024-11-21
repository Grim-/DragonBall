using Verse;

namespace DragonBall
{
    public abstract class PawnTargetWish : BaseWish
    {
        protected Pawn Target;

        protected PawnTargetWish()
        {

        }

        protected PawnTargetWish(WishDef def, Pawn target) : base(def)
        {
            Target = target;
        }
    }
}
