using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public abstract class ActionInterface
    {
        public abstract void updateAction(CollisionWorld collisionWorld, VFixedPoint deltaTimeStep);
    }
}
