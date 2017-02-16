using System.Collections.Generic;
using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public abstract class CollisionAlgorithm 
    {
        public virtual void init()
        {
        }

        public abstract void destroy();

        public abstract void processCollision(CollisionObject body0, CollisionObject body1, DispatcherInfo dispatchInfo, ManifoldResult resultOut);
    }
}