using Verse;

namespace DragonBall
{
    public class DragonBallFlying : Projectile
    {
        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
  
        }

        public override void Tick()
        {
            base.Tick();

            if (this.Position.DistanceTo(DestinationCell) < 1)
            {
                this.Destroy();
            }
        }
    }
}
