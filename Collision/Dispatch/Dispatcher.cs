using System.Collections.Generic;

namespace MobaGame.Collision
{
    public abstract class Dispatcher
    {
        protected DispatcherInfo dispatchInfo = new DispatcherInfo();

        public DispatcherInfo getDispatchInfo()
        {
            return dispatchInfo;
        }

        public abstract CollisionAlgorithm findAlgorithm(CollisionObject body0, CollisionObject body1);

        public abstract RaytestAlgorithm findAlgorithm(CollisionObject body);

        public abstract bool needsCollision(CollisionObject body0, CollisionObject body1);

        public abstract bool needsCollision(CollisionObject body0, RayResultCallback rayResultCallback);

        public abstract void dispatchAllCollisionPairs(OverlappingPairCache pairCache, Dispatcher dispatcher);

        public abstract ManifoldResult applyManifold();

        public abstract void releaseManifold(ManifoldResult result);

        public abstract void releaseAllManifold();

        public abstract List<ManifoldResult> getAllManifolds();
    }
}
