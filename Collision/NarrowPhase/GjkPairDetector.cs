using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public class GjkPairDetector : DiscreteCollisionDetectorInterface
    {
        // must be above the machine epsilon
        private static readonly VFixedPoint REL_ERROR2 = Globals.EPS2;

        private VInt3 cachedSeparatingAxis;
        private ConvexPenetrationDepthSolver penetrationDepthSolver;
        private SimplexSolverInterface simplexSolver;
        private ConvexShape minkowskiA;
        private ConvexShape minkowskiB;

        // some debugging to fix degeneracy problems
        public int lastUsedMethod;
        public int curIter;
        public int degenerateSimplex;

        public void init(ConvexShape objectA, ConvexShape objectB, SimplexSolverInterface simplexSolver, ConvexPenetrationDepthSolver penetrationDepthSolver)
        {
            this.cachedSeparatingAxis = VInt3.zero;
            this.lastUsedMethod = -1;

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
            VInt3 tmp = new VInt3();

            VFixedPoint distance = VFixedPoint.Zero;
            VInt3 normalInB = VInt3.zero;
            VInt3 pointOnA = new VInt3();
            VInt3 pointOnB = new VInt3();
            VIntTransform localTransA = input.transformA;
            VIntTransform localTransB = input.transformB;
            VFixedPoint marginA = minkowskiA.getMargin();
            VFixedPoint marginB = minkowskiB.getMargin();
            VInt3 positionOffset = (localTransA.position + localTransB.position) / VFixedPoint.Two;
            localTransA.position -= positionOffset;
            localTransB.position -= positionOffset;

            curIter = 0;
            int gGjkMaxIter = 1000; // this is to catch invalid input, perhaps check for #NaN?
            cachedSeparatingAxis = VInt3.up;

            bool isValid = false;
            bool checkSimplex = false;
            bool checkPenetration = true;
            degenerateSimplex = 0;

            lastUsedMethod = -1;

            VFixedPoint squaredDistance = VFixedPoint.MaxValue;
            VFixedPoint delta = VFixedPoint.Zero;

            VFixedPoint margin = marginA + marginB;

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
            VInt3 tmpNormalInB = new VInt3();

            while (true)
            {
                seperatingAxisInA = cachedSeparatingAxis * -1;
                seperatingAxisInA = input.transformA.InverseTransformVector(seperatingAxisInA);

                seperatingAxisInB = cachedSeparatingAxis;
                seperatingAxisInB = input.transformB.InverseTransformVector(seperatingAxisInB);

                pInA = minkowskiA.localGetSupportingVertexWithoutMargin(seperatingAxisInA);
                qInB =  minkowskiB.localGetSupportingVertexWithoutMargin(seperatingAxisInB);

                pWorld = localTransA.TransformPoint(pInA);
                qWorld = localTransB.TransformPoint(qInB);

                w = pWorld - qWorld;

                delta = VInt3.Dot(cachedSeparatingAxis, w);
                if(delta > VFixedPoint.Zero && delta * delta > squaredDistance * input.maximumDistanceSquared)
                {
                    checkPenetration = false;
                    break;
                }

                if(simplexSolver.inSimplex(w))
                {
                    degenerateSimplex = 1;
                    checkSimplex = true;
                    break;
                }

                VFixedPoint f0 = squaredDistance - delta;
                VFixedPoint f1 = squaredDistance * REL_ERROR2;

                if(f0 <= f1)
                {
                    if(f0 <= VFixedPoint.Zero)
                    {
                        degenerateSimplex = 2;
                    }
                    checkSimplex = true;
                    break;
                }

                simplexSolver.addVertex(w, pWorld, qWorld);

                if(!simplexSolver.closest(ref cachedSeparatingAxis))
                {
                    degenerateSimplex = 3;
                    checkSimplex = true;
                    break;
                }

                if(cachedSeparatingAxis.sqrMagnitude < REL_ERROR2)
                {
                    degenerateSimplex = 6;
                    checkSimplex = true;
                    break;
                }

                VFixedPoint previousSquaredDistance = squaredDistance;
                squaredDistance = cachedSeparatingAxis.sqrMagnitude;

                if(previousSquaredDistance - squaredDistance <= REL_ERROR2)
                {
                    simplexSolver.backup_closest(cachedSeparatingAxis);
                    checkSimplex = true;
                    break;
                }

                if(curIter++ < gGjkMaxIter)
                {
                    break;
                }

                bool check = !simplexSolver.fullSimplex();
                if(!check)
                {
                    simplexSolver.backup_closest(cachedSeparatingAxis);
                    break;
                }
            }

            if(checkSimplex)
            {
                simplexSolver.compute_points(ref pointOnA, ref pointOnB);
                normalInB = cachedSeparatingAxis;
                VFixedPoint lenSqr = cachedSeparatingAxis.sqrMagnitude;

                if(lenSqr > REL_ERROR2)
                {
                    VFixedPoint rlen = VFixedPoint.One / FMath.Sqrt(lenSqr);
                    normalInB /= rlen;
                    VFixedPoint s = FMath.Sqrt(squaredDistance);
                    pointOnA -= cachedSeparatingAxis * marginA / s;
                    pointOnB += cachedSeparatingAxis * marginB / s;
                    distance = VFixedPoint.One / rlen - margin;
                    isValid = true;
                    lastUsedMethod = 1;
                }
                else
                {
                    degenerateSimplex = 5;
                    lastUsedMethod = 2;
                }
            }

            if (checkPenetration && (!isValid || distance + margin < Globals.EPS))
            {
                if (penetrationDepthSolver != null)
                {
                    bool isValid2 = penetrationDepthSolver.calcPenDepth(
                            simplexSolver,
                            minkowskiA, minkowskiB,
                            localTransA, localTransB,
                            ref tmpPointOnA, ref tmpPointOnB);

                    if (isValid2)
                    {
                        tmpNormalInB = tmpPointOnB - tmpPointOnA;

                        VFixedPoint lenSqr = tmpNormalInB.sqrMagnitude;
                        if (lenSqr > VFixedPoint.Zero)
                        {
                            tmpNormalInB /= FMath.Sqrt(lenSqr);
                            tmp = tmpPointOnA - tmpPointOnB;
                            VFixedPoint distance2 = -tmp.magnitude;
                            // only replace valid penetrations when the result is deeper (check)
                            if (!isValid || (distance2 < distance))
                            {
                                distance = distance2;
                                pointOnA = tmpPointOnA;
                                pointOnB = tmpPointOnB;
                                normalInB = tmpNormalInB;
                                isValid = true;
                                lastUsedMethod = 3;
                            }
                        }
                        else
                        {
                            //isValid = false;
                            lastUsedMethod = 4;
                        }
                    }
                    else
                    {
                        lastUsedMethod = 5;
                    }
                }
            }
            if (isValid)
            {
                tmp = pointOnB + positionOffset;
                output.addContactPoint(
                        normalInB,
                        tmp,
                        distance);
            }
        }
    }
}
