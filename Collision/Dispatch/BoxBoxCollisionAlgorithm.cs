using MobaGame.FixedMath;
using System.Collections.Generic;
using System;

namespace MobaGame.Collision
{
    public class BoxBoxCollisionAlgorithm: CollisionAlgorithm
    {
        bool ownManifold;
        PersistentManifold manifoldPtr;
        BoxBoxDetector detector = new BoxBoxDetector();

        public void init(PersistentManifold mf, CollisionAlgorithmConstructionInfo ci, CollisionObject body0, CollisionObject body1)
        {
            ownManifold = false;
            manifoldPtr = mf;
            base.init(ci);

            if (manifoldPtr == null && dispatcher.needsCollision(body0, body1))
            {
                manifoldPtr = dispatcher.getNewManifold(body0, body1);
                ownManifold = true;
            }
        }

        public override void destroy()
        {
            if (ownManifold)
            {
                if (manifoldPtr != null)
                {
                    dispatcher.releaseManifold(manifoldPtr);
                }
                manifoldPtr = null;
            }
        }

        public override void processCollision(CollisionObject body0, CollisionObject body1, DispatcherInfo dispatchInfo, ManifoldResult resultOut)
        {
            if (manifoldPtr == null)
                return;

            BoxShape box0 = (BoxShape)body0.getCollisionShape();
            BoxShape box1 = (BoxShape)body1.getCollisionShape();

            resultOut.setPersistentManifold(manifoldPtr);
            manifoldPtr.clearManifold();

            ClosestPointInput input = new ClosestPointInput();
            input.maximumDistanceSquared = VFixedPoint.LARGE_NUMBER;
            input.transformA = body0.getWorldTransform();
            input.transformB = body1.getWorldTransform();

            detector.getClosestPoints(input, resultOut);

            if (ownManifold)
            {
                resultOut.refreshContactPoints();
            }
        }



        public override VFixedPoint calculateTimeOfImpact(CollisionObject body0, CollisionObject body1, DispatcherInfo dispatchInfo, ManifoldResult resultOut)
        {
            return VFixedPoint.One;
        }

        public override void getAllContactManifolds(List<PersistentManifold> manifoldArray)
        {
            if(manifoldPtr != null && ownManifold)
            {
                manifoldArray.Add(manifoldPtr);
            }
        }

        public class CreateFunc : CollisionAlgorithmCreateFunc
        {
            private ObjectPool<BoxBoxCollisionAlgorithm> pool = new ObjectPool<BoxBoxCollisionAlgorithm>();

            public override CollisionAlgorithm createCollisionAlgorithm(CollisionAlgorithmConstructionInfo ci, CollisionObject body0, CollisionObject body1)
            {
                BoxBoxCollisionAlgorithm algo = pool.Get();
                algo.init(null, ci, body0, body1);
                return algo;
            }

            public override void releaseCollisionAlgorithm(CollisionAlgorithm algo)
            {
                pool.Release((BoxBoxCollisionAlgorithm)algo);
            }
        };
    }
}
