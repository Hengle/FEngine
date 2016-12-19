﻿using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public class GjkPairDetector : DiscreteCollisionDetectorInterface
    {
        // must be above the machine epsilon
        private static readonly VFixedPoint REL_ERROR2 = Globals.EPS2;

        public static readonly int DEFAULT_MAX_ITERATIONS = 30;
        protected int maxIterations = DEFAULT_MAX_ITERATIONS;

        private VInt3 cachedSeparatingAxis;
        private SimplexSolverInterface simplexSolver;
        private ConvexShape minkowskiA;
        private ConvexShape minkowskiB;

        private ConvexPenetrationDepthSolver penetrationDepthSolver;


        public void init(ConvexShape objectA, ConvexShape objectB, SimplexSolverInterface simplexSolver, ConvexPenetrationDepthSolver penetrationDepthSolver)
        {
            this.cachedSeparatingAxis = VInt3.zero;
            this.penetrationDepthSolver = penetrationDepthSolver;
            this.simplexSolver = simplexSolver;
            minkowskiA = objectA;
            minkowskiB = objectB;
            this.penetrationDepthSolver = penetrationDepthSolver;
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
            this.penetrationDepthSolver = penetrationDepthSolver;
        }

        public override void getClosestPoints(ClosestPointInput input, Result output)
        {
            VInt3 pointOnA;
            VInt3 pointOnB;
            VInt3 normalInB;
            VIntTransform localTransA = input.transformA;
            VIntTransform localTransB = input.transformB;
            VFixedPoint marginA = minkowskiA.getMargin();
            VFixedPoint marginB = minkowskiB.getMargin();
            cachedSeparatingAxis = getInitialDirection(localTransA, localTransB);

            //VFixedPoint margin = marginA + marginB;

            simplexSolver.reset();
            int iterations = 0;
            VFixedPoint sDist = VFixedPoint.MaxValue;
            VInt3 prevClosest;

            while (iterations < maxIterations)
            {
                prevClosest = cachedSeparatingAxis;

                VInt3 seperatingAxisInA = input.transformA.InverseTransformVector(cachedSeparatingAxis);
                VInt3 seperatingAxisInB = input.transformB.InverseTransformVector(-cachedSeparatingAxis);

                VInt3 pInA = minkowskiA.localGetSupportingVertexWithoutMargin(seperatingAxisInA);
                VInt3 qInB = minkowskiB.localGetSupportingVertexWithoutMargin(seperatingAxisInB);

                VInt3 pWorld = localTransA.TransformPoint(pInA);
                VInt3 qWorld = localTransB.TransformPoint(qInB);

                VInt3 w = pWorld - qWorld;
                VFixedPoint signDist = VInt3.Dot(w, cachedSeparatingAxis);

                if (VInt3.Dot(cachedSeparatingAxis, w) <= VFixedPoint.Zero)
                {
                    return;
                }

                simplexSolver.addVertex(w, pWorld, qWorld);
                bool result = simplexSolver.compute_points(out pointOnA, out pointOnB);
                normalInB = (pointOnA - pointOnB);
				if (result)
                {
                    if(normalInB.sqrMagnitude < Globals.EPS2)
                    {
                        output.addContactPoint(prevClosest.Normalize(), pointOnB, -signDist);
                    }
					else
                    {
                        VFixedPoint depth = VFixedPoint.Zero;
                        penetrationDepthSolver.calcPenDepth(simplexSolver, minkowskiA, minkowskiB,
                            localTransA, localTransB, ref pointOnA, ref pointOnB, ref normalInB, ref depth);
                        output.addContactPoint((pointOnA - pointOnB).Normalize(), pointOnB, depth);
                    }
                    return;
                }
                cachedSeparatingAxis = -normalInB;
                sDist = normalInB.sqrMagnitude;
                iterations++;
            }
        }

        protected VInt3 getInitialDirection(VIntTransform transform1, VIntTransform transform2)
        {
            VInt3 c1 = transform1.position;
            VInt3 c2 = transform2.position;
            VInt3 c = c1 - c2;
            return c.sqrMagnitude > Globals.EPS2 ? c : VInt3.forward;
        }
    }
}
