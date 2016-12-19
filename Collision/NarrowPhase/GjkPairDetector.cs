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

            while (iterations < maxIterations)
            {

                VInt3 seperatingAxisInA = input.transformA.InverseTransformVector(cachedSeparatingAxis);
                VInt3 seperatingAxisInB = input.transformB.InverseTransformVector(-cachedSeparatingAxis);

                VInt3 pInA = minkowskiA.localGetSupportingVertexWithoutMargin(seperatingAxisInA);
                VInt3 qInB = minkowskiB.localGetSupportingVertexWithoutMargin(seperatingAxisInB);

                VInt3 pWorld = localTransA.TransformPoint(pInA);
                VInt3 qWorld = localTransB.TransformPoint(qInB);

                VInt3 w = pWorld - qWorld;
                simplexSolver.addVertex(w, pWorld, qWorld);

                if(VInt3.Dot(w, cachedSeparatingAxis) <= VFixedPoint.Zero)
                {
                    return;
                }

                bool result = simplexSolver.compute_points(out pointOnA, out pointOnB);
                normalInB = (pointOnA - pointOnB);
				if (result)
                {
                    if(normalInB.sqrMagnitude < Globals.EPS2 && simplexSolver.numVertices() == 3)
                    {
                        VInt3[] aBuf = new VInt3[4];
                        VInt3[] bBuf = new VInt3[4];
                        VInt3[] Q = new VInt3[4];
                        simplexSolver.getSimplex(aBuf, bBuf, Q);
                        normalInB = VInt3.Cross(Q[1] - Q[0], Q[2] - Q[0]);
                        if (VInt3.Dot(normalInB, localTransA.position - localTransB.position) < VFixedPoint.Zero)
                            normalInB *= -1;
                        output.addContactPoint(normalInB.Normalize(), pointOnB, VFixedPoint.Zero);
                    }
					else
                    {

                    }
                    return;
                }
                cachedSeparatingAxis = -normalInB;
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
