using MobaGame.FixedMath;
using System.Collections.Generic;
using System;

namespace MobaGame.Collision
{
    public class BoxBoxCollisionAlgorithm: CollisionAlgorithm
    {
        BoxBoxDetector detector = new BoxBoxDetector();

        public void init(CollisionAlgorithmConstructionInfo ci, CollisionObject body0, CollisionObject body1)
        {
            base.init(ci);
            detector.init((BoxShape)body0.getCollisionShape(), (BoxShape)body1.getCollisionShape());
        }

        public override void destroy()
        {

        }

        public override void processCollision(CollisionObject body0, CollisionObject body1, DispatcherInfo dispatchInfo, ManifoldResult resultOut)
        {
            ClosestPointInput input = new ClosestPointInput();
            input.maximumDistanceSquared = VFixedPoint.LARGE_NUMBER;
            input.transformA = body0.getWorldTransform();
            input.transformB = body1.getWorldTransform();

            detector.getClosestPoints(input, resultOut);
        }

        public class CreateFunc : CollisionAlgorithmCreateFunc
        {
            private ObjectPool<BoxBoxCollisionAlgorithm> pool = new ObjectPool<BoxBoxCollisionAlgorithm>();

            public override CollisionAlgorithm createCollisionAlgorithm(CollisionAlgorithmConstructionInfo ci, CollisionObject body0, CollisionObject body1)
            {
                BoxBoxCollisionAlgorithm algo = pool.Get();
                algo.init(ci, body0, body1);
                return algo;
            }

            public override void releaseCollisionAlgorithm(CollisionAlgorithm algo)
            {
                pool.Release((BoxBoxCollisionAlgorithm)algo);
            }
        };
    }
}
