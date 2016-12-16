using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public class GjkPairDetector : DiscreteCollisionDetectorInterface
    {
        // must be above the machine epsilon
        private static readonly VFixedPoint REL_ERROR2 = Globals.EPS2;

        private static VInt3 ORIGIN = VInt3.zero;
        public static readonly int DEFAULT_MAX_ITERATIONS = 30;
        protected int maxIterations = DEFAULT_MAX_ITERATIONS;

        private VInt3 cachedSeparatingAxis;
        private SimplexSolverInterface simplexSolver;
        private ConvexShape minkowskiA;
        private ConvexShape minkowskiB;


        public void init(ConvexShape objectA, ConvexShape objectB, SimplexSolverInterface simplexSolver, ConvexPenetrationDepthSolver penetrationDepthSolver)
        {
            this.cachedSeparatingAxis = VInt3.zero;
            //this.penetrationDepthSolver = penetrationDepthSolver;
            this.simplexSolver = simplexSolver;
            minkowskiA = objectA;
            minkowskiB = objectB;
        }

        public void setMinkowskiA(ConvexShape minkA)
        {
            minkowskiA = minkA;
        }

        public void setMinkowskiB(ConvexShape minkB)
        {
            minkowskiB = minkB;
        }

        public void setCachedSeperatingAxis(VInt3 seperatingAxis)
        {
            cachedSeparatingAxis = seperatingAxis;
        }

        public void setPenetrationDepthSolver(ConvexPenetrationDepthSolver penetrationDepthSolver)
        {
            //this.penetrationDepthSolver = penetrationDepthSolver;
        }

        public override void getClosestPoints(ClosestPointInput input, Result output)
        {
            VFixedPoint distance = VFixedPoint.Zero;
            VInt3 normalInB = VInt3.zero;
            VInt3 pointOnA;
            VInt3 pointOnB;
            VIntTransform localTransA = input.transformA;
            VIntTransform localTransB = input.transformB;
            VFixedPoint marginA = minkowskiA.getMargin();
            VFixedPoint marginB = minkowskiB.getMargin();
            cachedSeparatingAxis = getInitialDirection(localTransA, localTransB);


            VFixedPoint sDist = VFixedPoint.MaxValue;
            VFixedPoint minDist = sDist;

            VFixedPoint margin = marginA + marginB;
            simplexSolver.reset();
            int iterations = 0;
            bool notTerminated = true;

            do
            {
                minDist = sDist;

                VInt3 seperatingAxisInA = input.transformA.InverseTransformVector(-cachedSeparatingAxis);
                VInt3 seperatingAxisInB = input.transformB.InverseTransformVector(cachedSeparatingAxis);

                VInt3 pInA = minkowskiA.localGetSupportingVertexWithoutMargin(seperatingAxisInA);
                VInt3 qInB = minkowskiB.localGetSupportingVertexWithoutMargin(seperatingAxisInB);

                VInt3 pWorld = localTransA.TransformPoint(pInA);
                VInt3 qWorld = localTransB.TransformPoint(qInB);

                VInt3 w = pWorld - qWorld;

                if (containsOrigin(Q))
                {
                    separation.distance = VFixedPoint.Zero;
                    separation.normal = VInt3.zero;
                    return true;
                }

                size++;
                v = DoSimplex(Q, support.point, ref size) * -1;
                sDist = v.sqrMagnitude;
                notTerminated = minDist - sDist > eps2 && iterations < maxIterations;
                iterations++;
            }
            while (notTerminated);


        }

        protected VInt3 getInitialDirection(VIntTransform transform1, VIntTransform transform2)
        {
            VInt3 c1 = transform1.position;
            VInt3 c2 = transform2.position;

            return c2 - c1;
        }
    }
}
