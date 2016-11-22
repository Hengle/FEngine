﻿using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public class SubsimplexConvexCast: ConvexCast
    {
        private static readonly int MAX_ITERATIONS = 32;

        private SimplexSolverInterface simplexSolver;
        private ConvexShape convexA;
        private ConvexShape convexB;

        public SubsimplexConvexCast(ConvexShape shapeA, ConvexShape shapeB, SimplexSolverInterface simplexSolver)
        {
            this.convexA = shapeA;
            this.convexB = shapeB;
            this.simplexSolver = simplexSolver;
        }

        public override bool calcTimeOfImpact(VIntTransform fromA, VIntTransform toA, VIntTransform fromB, VIntTransform toB, CastResult result)
        {	
		    simplexSolver.reset();

            VInt3 linVelA = toA.position - fromA.position;
		    VInt3 linVelB = toB.position - fromB.position;
		
		    VFixedPoint lambda = VFixedPoint.Zero;

            VIntTransform interpolatedTransA = fromA;
            VIntTransform interpolatedTransB = fromB;

            // take relative motion
            VInt3 r = linVelA - linVelB;
            VInt3 tmp = -r;
            tmp = fromA.InverseTransformVector(tmp);
            VInt3 supVertexA = convexA.localGetSupportingVertex(tmp);
		    fromA.TransformPoint(supVertexA);
		
		    tmp = fromB.InverseTransformVector(r);
            VInt3 supVertexB = convexB.localGetSupportingVertex(tmp);
		    fromB.TransformPoint(supVertexB);
            VInt3 v = supVertexA - supVertexB;
		
		    int maxIter = MAX_ITERATIONS;

            VInt3 n = VInt3.zero;

		    VFixedPoint lastLambda = lambda;

            VFixedPoint dist2 = v.sqrMagnitude;
            VFixedPoint epsilon = Globals.EPS;

            VInt3 w;
            VFixedPoint VdotR;

		    while ((dist2 > epsilon) && (maxIter--) != 0)
            {
			    tmp = -v;
                tmp = fromA.InverseTransformVector(tmp);
                supVertexA = convexA.localGetSupportingVertex(tmp);
                fromA.TransformPoint(supVertexA);

                tmp = fromB.InverseTransformVector(r);
                supVertexB = convexB.localGetSupportingVertex(tmp);
                fromB.TransformPoint(supVertexB);
                w = supVertexA - supVertexB;

			    VFixedPoint VdotW = VInt3.Dot(v, w);

			    if (lambda > VFixedPoint.One)
                {
				    return false;
			    }
			
			    if (VdotW > VFixedPoint.Zero)
                {
				    VdotR = VInt3.Dot(v, r);

				    if (VdotR > VFixedPoint.Zero)
                    {
					    return false;
				    }
				    else
                    {
					    lambda = lambda - VdotW / VdotR;

                        // interpolate to next lambda
                        interpolatedTransA.position = fromA.position * (VFixedPoint.One - lambda) + toA.position * lambda;
                        interpolatedTransB.position = fromB.position * (VFixedPoint.One - lambda) + toB.position * lambda;
                        w = supVertexA - supVertexB;
					    lastLambda = lambda;
					    n = v;
				    }
                }
                simplexSolver.addVertex(w, supVertexA , supVertexB);
			    if (simplexSolver.closest(ref v))
                {
				    dist2 = v.sqrMagnitude;
			    }
			    else
                {
				    dist2 = VFixedPoint.Zero;
			    }
		    }
	
		    // don't report a time of impact when moving 'away' from the hitnormal
		    result.fraction = lambda;
		    if (n.sqrMagnitude >VFixedPoint.Zero)
            {
                result.normal = result.normal.Normalize();
            }
		    else
            {
			    result.normal = VInt3.zero;
		    }

		    // don't report time of impact for motion away from the contact normal (or causes minor penetration)
		    if (VInt3.Dot(result.normal, r) >= -result.allowedPenetration)
			    return false;

		    VInt3 hitA = VInt3.zero;
		    VInt3 hitB = VInt3.zero;
		    simplexSolver.compute_points(ref hitA,ref hitB);
		    result.hitPoint = hitB;
		    return true;
	    }
    }
}
