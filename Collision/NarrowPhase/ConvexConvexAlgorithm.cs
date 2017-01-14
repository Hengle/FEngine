using MobaGame.FixedMath;
using System.Collections.Generic;

namespace MobaGame.Collision
{ 
    public class ConvexConvexAlgorithm: CollisionAlgorithm
    {
        protected ObjectPool<ClosestPointInput> pointInputsPool = new ObjectPool<ClosestPointInput>();

        GjkPairDetector gjkPairDetector = new GjkPairDetector();

        public bool ownManifold;
        public bool lowLevelOfDetail;

        public void init(CollisionAlgorithmConstructionInfo ci, CollisionObject body0, CollisionObject body1, SimplexSolverInterface simplexSolver, ConvexPenetrationDepthSolver pdSolver)
        {
            base.init(ci);
            gjkPairDetector.init(null, null, simplexSolver, pdSolver);
            ownManifold = false;
            lowLevelOfDetail = false;
        }

        public override void destroy()
        {

        }

        public void setLowLevelOfDetail(bool useLowLevel)
        {
            this.lowLevelOfDetail = useLowLevel;
        }

        public override void processCollision(CollisionObject body0, CollisionObject body1, DispatcherInfo dispatchInfo, ManifoldResult resultOut)
        {
            ConvexShape min0 = (ConvexShape)body0.getCollisionShape();
            ConvexShape min1 = (ConvexShape)body1.getCollisionShape();

            ClosestPointInput input = pointInputsPool.Get();
            input.init();

            gjkPairDetector.setMinkowskiA(min0);
            gjkPairDetector.setMinkowskiB(min1);
            input.maximumDistanceSquared = min0.getMargin() + min1.getMargin();
            input.maximumDistanceSquared *= input.maximumDistanceSquared;

            input.transformA = body0.getWorldTransform();
            input.transformB = body1.getWorldTransform();

            gjkPairDetector.getClosestPoints(input, resultOut);

            pointInputsPool.Release(input);
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
                algo.init(ci, body0, body1, simplexSolver, pdSolver);
                return algo;
            }

            public override void releaseCollisionAlgorithm(CollisionAlgorithm algo)
            {
                pool.Release((ConvexConvexAlgorithm)algo);
            }
        };
    }
}
