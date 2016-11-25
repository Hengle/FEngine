using MobaGame.FixedMath;
using System.Collections.Generic;
using System;

namespace MobaGame.Collision
{ 
    public class ConvexConvexAlgorithm: CollisionAlgorithm
    {
        protected ObjectPool<ClosestPointInput> pointInputsPool = new ObjectPool<ClosestPointInput>();

        GjkPairDetector gjkPairDetector = new GjkPairDetector();

        public bool ownManifold;
        public PersistentManifold manifoldPtr;
        public bool lowLevelOfDetail;

        public void init(PersistentManifold mf, CollisionAlgorithmConstructionInfo ci, CollisionObject body0, CollisionObject body1, SimplexSolverInterface simplexSolver, ConvexPenetrationDepthSolver pdSolver)
        {
            base.init(ci);
            gjkPairDetector.init(null, null, simplexSolver, pdSolver);
            manifoldPtr = mf;
            ownManifold = false;
            lowLevelOfDetail = false;
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

        public void setLowLevelOfDetail(bool useLowLevel)
        {
            this.lowLevelOfDetail = useLowLevel;
        }

        public override void processCollision(CollisionObject body0, CollisionObject body1, DispatcherInfo dispatchInfo, ManifoldResult resultOut)
        {
            if (manifoldPtr == null)
            {
                manifoldPtr = dispatcher.getNewManifold(body0, body1);
                ownManifold = true;
            }

            resultOut.setPersistentManifold(manifoldPtr);

            ConvexShape min0 = (ConvexShape)body0.getCollisionShape();
            ConvexShape min1 = (ConvexShape)body1.getCollisionShape();

            ClosestPointInput input = pointInputsPool.Get();
            input.init();

            // JAVA NOTE: original: TODO: if (dispatchInfo.m_useContinuous)
            gjkPairDetector.setMinkowskiA(min0);
            gjkPairDetector.setMinkowskiB(min1);
            input.maximumDistanceSquared = min0.getMargin() + min1.getMargin() + manifoldPtr.getContactBreakingThreshold();
            input.maximumDistanceSquared *= input.maximumDistanceSquared;
            //input.m_stackAlloc = dispatchInfo.m_stackAllocator;

            //	input.m_maximumDistanceSquared = btScalar(1e30);

            input.transformA = body0.getWorldTransform();
            input.transformB = body1.getWorldTransform();

            gjkPairDetector.getClosestPoints(input, resultOut);

            pointInputsPool.Release(input);
            //	#endif

            if (ownManifold)
            {
                resultOut.refreshContactPoints();
            }
        }

        //private static bool disableCcd = false;

        public override VFixedPoint calculateTimeOfImpact(CollisionObject body0, CollisionObject body1, DispatcherInfo dispatchInfo, ManifoldResult resultOut)
        {
            return VFixedPoint.One;
        }

        public override void getAllContactManifolds(List<PersistentManifold> manifoldArray)
        {
            // should we use ownManifold to avoid adding duplicates?
            if (manifoldPtr != null && ownManifold)
            {
                manifoldArray.Add(manifoldPtr);
            }
        }

        public PersistentManifold getManifold()
        {
            return manifoldPtr;
        }

        public class CreateFunc : CollisionAlgorithmCreateFunc
        {
            private ObjectPool<ConvexConvexAlgorithm> pool = new ObjectPool<ConvexConvexAlgorithm>();

            public ConvexPenetrationDepthSolver pdSolver;
            public SimplexSolverInterface simplexSolver;

            public CreateFunc(SimplexSolverInterface simplexSolver, ConvexPenetrationDepthSolver pdSolver)
            {
                this.simplexSolver = simplexSolver;
                this.pdSolver = pdSolver;
            }

            public override CollisionAlgorithm createCollisionAlgorithm(CollisionAlgorithmConstructionInfo ci, CollisionObject body0, CollisionObject body1)
            {
                ConvexConvexAlgorithm algo = pool.Get();
                algo.init(ci.manifold, ci, body0, body1, simplexSolver, pdSolver);
                return algo;
            }

            public override void releaseCollisionAlgorithm(CollisionAlgorithm algo)
            {
                pool.Release((ConvexConvexAlgorithm)algo);
            }
        };
    }
}
