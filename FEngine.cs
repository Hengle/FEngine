using System;
using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public class FEngine: IDisposable
    {
        private CollisionWorld dynamicWorld;
        private BroadphaseInterface broadPhase;
        private Dispatcher dispatcher;
		private HashedOverlappingPairCache pbp;

        public FEngine()
        {
            pbp = new HashedOverlappingPairCache();
            broadPhase = new DbvtBroadphase(pbp);
            dispatcher = new CollisionDispatcher(new DefaultCollisionConfiguration());
            dynamicWorld = new DynamicsWorld(dispatcher, broadPhase);
        }

        public void Dispose()
        {
            dynamicWorld.destroy();
        }

        public void Tick(VFixedPoint dt)
        {
            dynamicWorld.Tick(dt);
        }

        public CollisionWorld GetCollisionWorld()
        {
            return dynamicWorld;
        }
    }
}
