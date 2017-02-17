using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public class GjkConvexCast : ConvexCast
    {
        protected readonly ObjectPool<ClosestPointInput> pointInputsPool = new ObjectPool<ClosestPointInput>();

        private readonly SimplexSolverInterface simplexSolver;

        private readonly GjkPairDetector gjk = new GjkPairDetector();

        private static readonly int MAX_ITERATIONS = 32;

        public GjkConvexCast(SimplexSolverInterface simplexSolver)
        {
            this.simplexSolver = simplexSolver;
        }

        public override bool calcTimeOfImpact(VIntTransform fromA, VIntTransform toA, VIntTransform fromB, VIntTransform toB, CastResult result)
        {
            VInt3 linvelA = toA.position - fromA.position;
            VInt3 linvelB = toB.position - fromB.position;

            VFixedPoint radius = Globals.EPS;
            VFixedPoint lambda = VFixedPoint.Zero;

            int maxIter = MAX_ITERATIONS;

            VInt3 n = VInt3.zero;
		    bool hasResult;
            VInt3 c = new VInt3();
		    VInt3 r = linvelB - linvelA;

            VFixedPoint lastLambda = lambda;
            int numIter = 0;

            PointCollector pointCollector = new PointCollector();

            gjk.init(simplexSolver, null);
            ClosestPointInput input = pointInputsPool.Get();
            input.init();
            try
            {
                input.transformA = fromA;
                input.transformB = fromB;
                gjk.getClosestPoints(input, pointCollector);

                hasResult = pointCollector.hasResult;
                c = pointCollector.pointInWorld;

                if(hasResult)
                {
                    VFixedPoint dist = pointCollector.distance;
                    n = pointCollector.normalOnBInWorld;

                    while(dist > radius)
                    {
                        numIter++;
                        if(numIter > maxIter)
                        {
                            return false;
                        }

                        VFixedPoint projectedLinearVelocity = VInt3.Dot(r, n);
                        VFixedPoint dLambda = dist / projectedLinearVelocity;
                        lambda = lambda - dLambda;

                        if (lambda > VFixedPoint.One)
                        {
                            return false;
                        }
                        if (lambda < VFixedPoint.Zero)
                        {
                            return false;                   // todo: next check with relative epsilon
                        }

                        if (lambda <= lastLambda)
                        {
                            return false;
                        }
                        lastLambda = lambda;

                        // interpolate to next lambda
                        input.transformA.position = fromA.position * (VFixedPoint.One - lambda) + toA.position * lambda;
                        input.transformB.position = fromB.position * (VFixedPoint.One - lambda) + toB.position * lambda;

                        gjk.getClosestPoints(input, pointCollector);
                        if (pointCollector.hasResult)
                        {
                            if (pointCollector.distance < VFixedPoint.Zero)
                            {
                                result.fraction = lastLambda;
                                n = pointCollector.normalOnBInWorld;
                                result.normal = n;
                                result.hitPoint = pointCollector.pointInWorld;
                                return true;
                            }
                            c = pointCollector.pointInWorld;
                            n = pointCollector.normalOnBInWorld;
                            dist = pointCollector.distance;
                        }
                        else
                        {
                            return false;
                        }

                    }

                    // is n normalized?
                    // don't report time of impact for motion away from the contact normal (or causes minor penetration)
                    if (VInt3.Dot(n, r) >= -result.allowedPenetration)
                    {
                        return false;
                    }
                    result.fraction = lambda;
                    result.normal = n;
                    result.hitPoint = c;
                    return true;
                
                }
                else
                {
                    return false;
                }
            }
            finally
            {
                pointInputsPool.Release(input);
            }
        }
    }
}
