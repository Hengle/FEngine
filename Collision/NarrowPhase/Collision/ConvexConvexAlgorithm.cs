using MobaGame.FixedMath;
using System.Collections.Generic;

namespace MobaGame.Collision
{ 
    public static class ConvexConvexAlgorithm
    {
        static ObjectPool<ClosestPointInput> pointInputsPool = new ObjectPool<ClosestPointInput>();

        static VoronoiSimplexSolver simplexSolver = new VoronoiSimplexSolver();
        static ConvexPenetrationDepthSolver pdSolver = new EpaSolver();
        static GjkPairDetector gjkPairDetector = new GjkPairDetector(simplexSolver, pdSolver);

        public static void processCollision(CollisionObject body0, CollisionObject body1, DispatcherInfo dispatchInfo, ManifoldResult resultOut)
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
