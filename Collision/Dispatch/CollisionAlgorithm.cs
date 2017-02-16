using System.Collections.Generic;
using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public abstract class CollisionAlgorithm 
    {
        protected Dispatcher dispatcher;

        public virtual void init()
        {
        }

        public virtual void init(CollisionAlgorithmConstructionInfo ci)
        {
            dispatcher = ci.dispatcher1;
        }

        public abstract void destroy();

        public abstract void processCollision(CollisionObject body0, CollisionObject body1, DispatcherInfo dispatchInfo, ManifoldResult resultOut);
    }
}