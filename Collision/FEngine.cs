using System;
using System.Collections.Generic;

namespace MobaGame.Collision
{
    public class FEngine: IDisposable
    {
        private CollisionWorld collisionWorld;
        private BroadphaseInterface broadPhase;
        private Dispatcher dispatcher;
		private HashedOverlappingPairCache pbp;

        public FEngine()
        {
            pbp = new HashedOverlappingPairCache();
            broadPhase = new DbvtBroadphase(pbp);
            dispatcher = new CollisionDispatcher(new DefaultCollisionConfiguration());
            collisionWorld = new CollisionWorld(dispatcher, broadPhase);
        }

        public void Dispose()
        {
            collisionWorld.destroy();
        }

        public void Tick()
        {
            collisionWorld.performDiscreteCollisionDetection();
        }

        public CollisionWorld GetCollisionWorld()
        {
            return collisionWorld;
        }
    }
}
