using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public class GjkPairDetector : DiscreteCollisionDetectorInterface
    {
        // must be above the machine epsilon
        private static readonly VFixedPoint ERROR = VFixedPoint.One / VFixedPoint.Create(1000);

        private VInt3 cachedSeparatingAxis;
        private ConvexPenetrationDepthSolver penetrationDepthSolver;
        private SimplexSolverInterface simplexSolver;
        private ConvexShape minkowskiA;
        private ConvexShape minkowskiB;

        public void init(ConvexShape objectA, ConvexShape objectB, SimplexSolverInterface simplexSolver, ConvexPenetrationDepthSolver penetrationDepthSolver)
        {
            this.cachedSeparatingAxis = VInt3.zero;
            this.penetrationDepthSolver = penetrationDepthSolver;
            this.simplexSolver = simplexSolver;
            this.minkowskiA = objectA;
            this.minkowskiB = objectB;
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
            VIntTransform localTransA = input.transformA;
            VIntTransform localTransB = input.transformB;
            //VInt3 positionOffset = (localTransA.position + localTransB.position) / VFixedPoint.Two;
            //localTransA.position -= positionOffset;
            //localTransB.position -= positionOffset;

            int curIter;
            int gGjkMaxIter = 100; // this is to catch invalid input, perhaps check for #NaN?
            cachedSeparatingAxis = VInt3.up;
            simplexSolver.reset();

            VInt3 seperatingAxisInA = new VInt3();
            VInt3 seperatingAxisInB = new VInt3();

            VInt3 pInA = new VInt3();
            VInt3 qInB = new VInt3();

            VInt3 pWorld = new VInt3();
            VInt3 qWorld = new VInt3();
            VInt3 w = new VInt3();

            VInt3 tmpPointOnA = new VInt3();
            VInt3 tmpPointOnB = new VInt3();

            VFixedPoint sDist = VFixedPoint.MaxValue;

            for (curIter = 0; curIter < gGjkMaxIter; curIter++)
            {
                seperatingAxisInA = cachedSeparatingAxis * -1;
                seperatingAxisInA = input.transformA.InverseTransformVector(seperatingAxisInA);

                seperatingAxisInB = cachedSeparatingAxis;
                seperatingAxisInB = input.transformB.InverseTransformVector(seperatingAxisInB);

                minkowskiA.localGetSupportingVertexWithoutMargin(seperatingAxisInA, ref pInA);
                minkowskiB.localGetSupportingVertexWithoutMargin(seperatingAxisInB, ref qInB);

                pWorld = localTransA.TransformPoint(pInA);
                qWorld = localTransB.TransformPoint(qInB);
                w = pWorld - qWorld;

                simplexSolver.addVertex(w, tmpPointOnA, tmpPointOnB);
                if (!simplexSolver.closest(ref cachedSeparatingAxis))
                {
                    //degenerate condition, touching
                    break;
                }
                else
                {
                    if (cachedSeparatingAxis == VInt3.zero)
                    {
                        //origin is contained in tetrahedron, use epa
                        if (!penetrationDepthSolver.calcPenDepth(
                            simplexSolver,
                            minkowskiA, minkowskiB,
                            localTransA, localTransB,
                            ref tmpPointOnA, ref tmpPointOnB))
                        {
                            //epa failed, degenerated condition
                            break;
                        }

                        VInt3 tmpNormalInB = tmpPointOnA - tmpPointOnB;
                        VFixedPoint lenSqr = tmpNormalInB.sqrMagnitude;
                        if (lenSqr > VFixedPoint.Zero)
                        {
                            VFixedPoint depth = FMath.Sqrt(lenSqr);
                            tmpNormalInB /= depth;
                            output.addContactPoint(
                                tmpNormalInB,
                                tmpPointOnB,
                                -depth);
                        }
                        else
                        {
                            //degenerated condition, touching
                        }
                        break;
                    }
                    else if (sDist - cachedSeparatingAxis.sqrMagnitude < ERROR)
                    {
                        VInt3 pointOnA = new VInt3();
                        VInt3 pointOnB = new VInt3();
                        simplexSolver.compute_points(ref pointOnA, ref pointOnB);
                        if (cachedSeparatingAxis.sqrMagnitude < ERROR)
                        {
                            //degenerated case
                        }
                        else
                        {
                            VFixedPoint distance = cachedSeparatingAxis.magnitude;
                            output.addContactPoint(
                                cachedSeparatingAxis / distance,
                                pointOnB,
                                distance);
                        }

                        break;
                    }
                }

                sDist = cachedSeparatingAxis.sqrMagnitude;
            }
        }
    }
}
