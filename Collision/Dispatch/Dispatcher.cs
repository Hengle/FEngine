using System.Collections.Generic;

namespace MobaGame.Collision
{
    public abstract class Dispatcher
    {
        public abstract CollisionAlgorithm findAlgorithm(CollisionObject body0, CollisionObject body1);

        public abstract bool needsCollision(CollisionObject body0, CollisionObject body1);

        public abstract bool needsResponse(CollisionObject body0, CollisionObject body1);

        public abstract void dispatchAllCollisionPairs(OverlappingPairCache pairCache, DispatcherInfo dispatchInfo, Dispatcher dispatcher);
    }
}
