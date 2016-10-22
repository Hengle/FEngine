using MobaGame.FixedMath;
using System.Collections.Generic;
using System;

namespace MobaGame.Collision
{
    public class Epa : MinkowskiPenetrationSolver
    {

    	public static readonly int DEFAULT_MAX_ITERATIONS = 100;

		/** The default {@link Epa} distance epsilon in meters; near 1E-8 */
		public static readonly VFixedPoint DEFAULT_DISTANCE_EPSILON = ;
		
		/** The maximum number of {@link Epa} iterations */
		protected int maxIterations = Epa.DEFAULT_MAX_ITERATIONS;

		/** The {@link Epa} distance epsilon in meters */
		protected VFixedPoint distanceEpsilon = Epa.DEFAULT_DISTANCE_EPSILON;

        public void getPenetration(List<VInt3> simplex, MinkowskiSum MinkowskiSum, Penetration Penetration)
        {
            ExpandingSimplex smplx = new ExpandingSimplex(simplex);
			ExpandingSimplexEdge edge = null;
			Vector2 point = null;
			for (int i = 0; i < this.maxIterations; i++) {
				// get the closest edge to the origin
				edge = smplx.getClosestEdge();
				// get a new support point in the direction of the edge normal
				point = minkowskiSum.getSupportPoint(edge.normal);
				
				// see if the new point is significantly past the edge
				double projection = point.dot(edge.normal);
				if ((projection - edge.distance) < this.distanceEpsilon) {
					// then the new point we just made is not far enough
					// in the direction of n so we can stop now and
					// return n as the direction and the projection
					// as the depth since this is the closest found
					// edge and it cannot increase any more
					penetration.normal = edge.normal;
					penetration.depth = projection;
					return;
				}
				
				// lastly add the point to the simplex
				// this breaks the edge we just found to be closest into two edges
				// from a -> b to a -> newPoint -> b
				smplx.expand(point);
			}
			// if we made it here then we know that we hit the maximum number of iterations
			// this is really a catch all termination case
			// set the normal and depth equal to the last edge we created
			penetration.normal = edge.normal;
			penetration.depth = point.dot(edge.normal);
        }

        /**
		 * Returns the maximum number of iterations the algorithm will perform before exiting.
		 * @return int
		 * @see #setMaxIterations(int)
		 */
		public int getMaxIterations() 
		{
			return this.maxIterations;
		}

		/**
		 * Sets the maximum number of iterations the algorithm will perform before exiting.
		 * @param maxIterations the maximum number of iterations in the range [5, &infin;]
		 * @throws IllegalArgumentException if maxIterations is less than 5
		 */
		public void setMaxIterations(int maxIterations) 
		{
			if (maxIterations < 5) 
				return;
			this.maxIterations = maxIterations;
		}

		/**
		 * Returns the distance epsilon.
		 * @return double
		 * @see #setDistanceEpsilon(double)
		 */
		public VFixedPoint getDistanceEpsilon() 
		{
			return this.distanceEpsilon;
		}

		/**
		 * The minimum distance between two iterations of the algorithm.
		 * <p>
		 * The distance epsilon is used to determine when the algorithm is close enough to the
		 * edge of the minkowski sum to conclude that it can no longer expand.  This is primarily
		 * used when one of the {@link Convex} {@link Shape}s in question has a curved shape.
		 * @param distanceEpsilon the distance epsilon in the range (0, &infin;]
		 * @throws IllegalArgumentException if distanceEpsilon is less than or equal to zero
		 */
		public void setDistanceEpsilon(VFixedPoint distanceEpsilon) 
		{
			if (distanceEpsilon <= 0) 
				return;
			this.distanceEpsilon = distanceEpsilon;
		}

    }
}
