using MobaGame.FixedMath;
using System.Collections.Generic;

namespace MobaGame.Collision
{ 
    public class ConvexConvexAlgorithm: CollisionAlgorithm
    {
        protected ObjectPool<ClosestPointInput> pointInputsPool = new ObjectPool<ClosestPointInput>();

        GjkPairDetector gjkPairDetector;

        public bool ownManifold;
        public bool lowLevelOfDetail;

        public ConvexConvexAlgorithm(SimplexSolverInterface simplexSolver, ConvexPenetrationDepthSolver pdSolver):base()
        {
            gjkPairDetector = new GjkPairDetector();
            gjkPairDetector.init(simplexSolver, pdSolver);
        }

        public override void init(CollisionAlgorithmConstructionInfo ci)
        {
            base.init(ci);
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
    }
}
