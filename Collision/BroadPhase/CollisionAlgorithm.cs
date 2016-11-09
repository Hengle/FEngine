using System.Collections.Generic;

namespace MobaGame.Collision
{
    public abstract class CollisionAlgorithm 
    {
        private CollisionAlgorithmCreateFunc createFunc;

        protected Dispatcher dispatcher;

        public void init()
        {
        }

        public void init(CollisionAlgorithmConstructionInfo ci)
        {
            dispatcher = ci.dispatcher1;
        }

        public abstract void destroy();

        public abstract void processCollision(CollisionObject body0, CollisionObject body1, DispatcherInfo dispatchInfo, ManifoldResult resultOut);

        public abstract float calculateTimeOfImpact(CollisionObject body0, CollisionObject body1, DispatcherInfo dispatchInfo, ManifoldResult resultOut);

        public abstract void getAllContactManifolds(List<PersistentManifold> manifoldArray);

        public void internalSetCreateFunc(CollisionAlgorithmCreateFunc func)
        {
            createFunc = func;
        }

        public CollisionAlgorithmCreateFunc internalGetCreateFunc()
        {
            return createFunc;
        }
    }
}