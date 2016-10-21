using MobaGame.FixedMath;
using System.Collections.Generic;

namespace MobaGame.Collision
{
    public class GJK: NarrowphaseDetector, DistanceDetector, RaycastDetector
    {
        private static VInt3 ORIGIN = VInt3.zero;
        public static readonly int DEFAULT_MAX_ITERATIONS = 30;
        protected int maxIterations = DEFAULT_MAX_ITERATIONS;
        public static readonly VFixedPoint DEFAULT_DISTANCE_EPSILON = VFixedPoint.Create(0.01f);
        protected VFixedPoint distanceEpsilon = DEFAULT_DISTANCE_EPSILON;

        public bool detect(Convex convex1, VIntTransform transform1, Convex convex2, VIntTransform transform2, Penetration penetration)
        {
            List<VInt3> simplex = new List<VInt3>(4);

            MinkowskiSum ms = new MinkowskiSum(convex1, transform1, convex2, transform2);

            VInt3 d = getInitialDirection(convex1, transform1, convex2, transform2);
            if (!distance(convex1, transform1, convex2, transform2, new Separation()))
            {
                //this.minkowskiPenetrationSolver.getPenetration(simplex, ms, penetration);
                return true;
            }
            return false;
        }

        public bool detect(Convex convex1, VIntTransform transform1, Convex convex2, VIntTransform transform2)
        {
            List<VInt3> simplex = new List<VInt3>(3);
            VInt3 d = getInitialDirection(convex1, transform1, convex2, transform2);
            return distance(convex1, transform1, convex2, transform2, new Separation());
        }

        protected VInt3 getInitialDirection(Convex convex1, VIntTransform transform1, Convex convex2, VIntTransform transform2)
        {
            VInt3 c1 = transform1.TransformPoint(convex1.getCenter());
            VInt3 c2 = transform2.TransformPoint(convex2.getCenter());

            return c2 - c1;
        }

        public bool distance(Convex convex1, VIntTransform transform1, Convex convex2, VIntTransform transform2, Separation separation)
        {
            int size = 0;
            MinkowskiSumPoint[] Q = new MinkowskiSumPoint[4];

            VInt3 v = getInitialDirection(convex1, transform1, convex2, transform2);
            if (v.sqrMagnitude == VFixedPoint.Zero)
            {
                v = VInt3.forward;
            }

            VFixedPoint eps2 = VFixedPoint.Create(0.01f);

            VFixedPoint epsRel = VFixedPoint.Create(0.000225f);//1.5%.

            VFixedPoint sDist = VFixedPoint.MaxValue;
            VFixedPoint minDist = sDist;

            bool notTerminated = true;
            bool Con = true;
            VInt3 prevV = v;

            do
            {
                minDist = sDist;
                prevV = v;

                MinkowskiSumPoint support = new MinkowskiSumPoint(convex1.getFarthestPoint(-v, transform1), convex2.getFarthestPoint(v, transform2));
                VFixedPoint signDist = VInt3.Dot(support.point, v);
                VFixedPoint tmp0 = sDist - signDist;

                Q[size] = support;
                if (epsRel * sDist > tmp0)
                {
                    getClosestPoint(Q, v, size, separation);
                    separation.distance = v.magnitude;
                    separation.normal = v.Normalize() * -1;
                    return false;
                }

                size++;
                v = DoSimplex(Q, support.point, ref size);
                sDist = v.sqrMagnitude;
                Con = minDist > sDist;
                notTerminated = sDist > eps2 && Con;
            }
            while (notTerminated);

            getClosestPoint(Q, v, size, separation);
            separation.distance = Con ? sDist : minDist;
            separation.normal = v.Normalize() * -1;
            return true;
        }

        public bool raycast(Ray ray, VFixedPoint maxLength, Convex convex, VIntTransform transform, Raycast raycast)
        {
            VFixedPoint lambda = VFixedPoint.Zero;

            bool lengthCheck = maxLength > VFixedPoint.Zero;

            VInt3 a = VInt3.zero;
            bool aInit = false;
            VInt3 b = VInt3.zero;
            bool bInit = false;

            VInt3 start = ray.getStart();

            VInt3 x = start;

            VInt3 r = ray.getDirectionVector();

            VInt3 n = VInt3.forward;

            if (convex.contains(start, transform))
            {
                return false;
            }
            VInt3 c = transform.TransformPoint(convex.getCenter());

            VInt3 d = x - c;

            VFixedPoint distanceSqrd = VFixedPoint.MaxValue;
            int iterations = 0;
            while (distanceSqrd > this.distanceEpsilon)
            {
                VInt3 p = convex.getFarthestPoint(d, transform);

                VInt3 w = x - p;

                VFixedPoint dDotW = VInt3.Dot(d, w);
                if (dDotW > VFixedPoint.Zero)
                {
                    VFixedPoint dDotR = VInt3.Dot(d, r);
                    if (dDotR >= VFixedPoint.Zero)
                    {
                        return false;
                    }
                    lambda -= dDotW / dDotR;
                    if ((lengthCheck) && (lambda > maxLength))
                    {
                        return false;
                    }
                    x = r * lambda + start;

                    n = d;
                }
                if (aInit)
                {
                    if (bInit)
                    {
                        VInt3 p1 = Segment.getPointOnSegmentClosestToPoint(x, a, p);
                        VInt3 p2 = Segment.getPointOnSegmentClosestToPoint(x, p, b);
                        if ((p1 - x).sqrMagnitude < (p2 - x).sqrMagnitude)
                        {
                            b = p;
                            distanceSqrd = (p1 - x).sqrMagnitude;
                        }
                        else
                        {
                            a = p;
                            distanceSqrd = (p2 - x).sqrMagnitude;
                        }
                        VInt3 ab = b - a;
                        VInt3 ax = x - a;
                        d = VInt3.Cross(VInt3.Cross(ab, ax), ab);
                    }
                    else
                    {
                        b = p;
                        bInit = true;
                        VInt3 ab = b - a;
                        VInt3 ax = x - a;
                        d = VInt3.Cross(VInt3.Cross(ab, ax), ab);
                    }
                }
                else
                {
                    a = p;
                    aInit = true;
                    d *= -1;
                }
                if (iterations == this.maxIterations)
                {
                    return false;
                }
                iterations++;
            }
            raycast.point = x;
            raycast.normal = n.Normalize();
            raycast.distance = lambda;

            return true;
        }

        public int getMaxIterations()
        {
            return this.maxIterations;
        }

        public void setMaxIterations(int maxIterations)
        {
            if (maxIterations < 5)
            {
                return;
            }
            this.maxIterations = maxIterations;
        }

        VInt3 CrossAba(VInt3 a, VInt3 b)
        {
            return VInt3.Cross(a, VInt3.Cross(b, a));
        }

        protected bool containsOrigin(MinkowskiSumPoint[] input)
        {
            if (input.Length < 4)
            {
                return false;
            }

            VInt3 a = input[0].point;
            VInt3 b = input[1].point;
            VInt3 c = input[2].point;
            VInt3 d = input[3].point;

            if (Tetrahedron.PointOUtsideOfPlane(ORIGIN, a, b, c, d))
            {
                return false;
            }

            if (Tetrahedron.PointOUtsideOfPlane(ORIGIN, a, b, d, c))
            {
                return false;
            }

            if (Tetrahedron.PointOUtsideOfPlane(ORIGIN, a, c, d, b))
            {
                return false;
            }

            if (Tetrahedron.PointOUtsideOfPlane(ORIGIN, b, c, d, a))
            {
                return false;
            }

            return true;
        }

        protected VInt3 DoSimplex(MinkowskiSumPoint[] Q, VInt3 support, ref int size)
        {
            switch (size)
            {
                case 1:
                    {
                        return support;
                    }
                case 2:
                    {
                        return closestPtPointSegment(Q, ref size);
                    }
                case 3:
                    {
                        return closestPtPointTriangle(Q, ref size);
                    }
                case 4:
                    {
                        return closestPtPointTetrahedron(Q, ref size);
                    }
            }
            return support;
        }

        protected void getClosestPoint(MinkowskiSumPoint[] Q, VInt3 closest, int size, Separation separation)
        {
            switch (size)
            {
                case 1:
                    {
                        separation.point1 = Q[0].supportPoint1;
                        separation.point2 = Q[0].supportPoint2;
                        break;
                    }
                case 2:
                    {
                        VFixedPoint v = VFixedPoint.Zero;
                        BarycentricCoordinates.barycentricCoordinates(closest, Q[0].point, Q[1].point, ref v);
                        separation.point1 = (Q[1].supportPoint1 - Q[0].supportPoint1) * v + Q[0].supportPoint1;
                        separation.point2 = (Q[1].supportPoint2 - Q[0].supportPoint2) * v + Q[0].supportPoint2;

                        break;
                    }
                case 3:
                    {
                        //calculate the Barycentric of closest point p in the mincowsky sum
                        VFixedPoint v = VFixedPoint.Zero;
                        VFixedPoint w = VFixedPoint.Zero;
                        BarycentricCoordinates.barycentricCoordinates(closest, Q[0].point, Q[1].point, Q[2].point, ref v, ref w);

                        separation.point1 = Q[0].supportPoint1 + (Q[1].supportPoint1 - Q[0].supportPoint1) * v + (Q[2].supportPoint1 - Q[0].supportPoint1) * w;
                        separation.point2 = Q[0].supportPoint2 + (Q[1].supportPoint2 - Q[0].supportPoint2) * v + (Q[2].supportPoint2 - Q[0].supportPoint2) * w;
                        break;
                    }
            }
        }

        static VInt3 closestPtPointSegment(MinkowskiSumPoint[] Q, ref int size)
        {
	        VInt3 a = Q[0].point;
            VInt3 b = Q[1].point;

            //Test degenerated case
            VInt3 ab = b - a;
            VFixedPoint denom = VInt3.Dot(ab, ab);
            VInt3 ap = -a;//V3Sub(origin, a);
            VFixedPoint nom = VInt3.Dot(ap, ab);
            bool con = denom <= VFixedPoint.Zero;
	        //TODO - can we get rid of this branch? The problem is size, which isn't a vector!
	        if(con)
	        {
		        size = 1;
		        return Q[0].point;
	        }

            /*	const PxU32 count = BAllEq(con, bTrue);
	            size = 2 - count;*/

            VFixedPoint tValue = FMath.Clamp(nom / denom, VFixedPoint.Zero, VFixedPoint.One);
	        return ab * tValue + a;
        }

        VInt3 closestPtPointTriangle(MinkowskiSumPoint[] Q, ref int size)
	    {
			size = 3;
		
			VFixedPoint eps = FEps();
			MinkowskiSumPoint a = Q[0];
			MinkowskiSumPoint b = Q[1];
			MinkowskiSumPoint c = Q[2];
			VInt3 ab = b.point - a.point;
			VInt3 ac = c.point - a.point;
			VInt3 signArea = VInt3.Cross(ab, ac);//0.5*(abXac)
			VFixedPoint area = VInt3.Dot(signArea, signArea);
			if(area <= eps)
			{
				//degenerate
				size = 2;
				return Segment.getPointOnSegmentClosestToPoint(ORIGIN, Q[0].point, Q[1].point);
			}

			int _size = 0;
			int[] indices= new int[]{0, 1, 2};
			VInt3 closest = closestPtPointTriangleBaryCentric(a.point, b.point, c.point, indices, ref _size);

			if(_size != 3)
			{
                MinkowskiSumPoint q0 = Q[indices[0]]; MinkowskiSumPoint q1 = Q[indices[1]];
				Q[0] = q0; Q[1] = q1;
				size = _size;
			}

			return closest;
		}

        VInt3 closestPtPointTriangleBaryCentric(VInt3 a, VInt3 b, VInt3 c, int[] indices, ref int size)
        {
            size = 3;
            VFixedPoint eps = FEps();
            
            VInt3 ab = b - a;
            VInt3 ac = c - a;

            VInt3 n = VInt3.Cross(ab, ac);

            VInt3 bCrossC = VInt3.Cross(b, c);
            VInt3 cCrossA = VInt3.Cross(c, a);
            VInt3 aCrossB = VInt3.Cross(a, b);

            VFixedPoint va = VInt3.Dot(n, bCrossC);//edge region of BC, signed area rbc, u = S(rbc)/S(abc) for a
            VFixedPoint vb = VInt3.Dot(n, cCrossA);//edge region of AC, signed area rac, v = S(rca)/S(abc) for b
            VFixedPoint vc = VInt3.Dot(n, aCrossB);//edge region of AB, signed area rab, w = S(rab)/S(abc) for c

            bool isFacePoints = va >= VFixedPoint.Zero && vb >= VFixedPoint.Zero && vc >= VFixedPoint.Zero;


            //face region
            if(isFacePoints)
            {   
                VFixedPoint nn= VInt3.Dot(n, n);
                VFixedPoint t = VInt3.Dot(n, a) / nn;
                return n * t;
            }

            VInt3 ap = -a;
            VInt3 bp = -b;
            VInt3 cp = -c;

            VFixedPoint d1 = VInt3.Dot(ab, ap); //  snom
            VFixedPoint d2 = VInt3.Dot(ac, ap); //  tnom
            VFixedPoint d3 = VInt3.Dot(ab, bp); // -sdenom
            VFixedPoint d4 = VInt3.Dot(ac, bp); //  unom = d4 - d3
            VFixedPoint d5 = VInt3.Dot(ab, cp); //  udenom = d5 - d6
            VFixedPoint d6 = VInt3.Dot(ac, cp); // -tdenom


            VFixedPoint unom = d4 - d3;
            VFixedPoint udenom = d5 - d6;

            size = 2;
            //check if p in edge region of AB
            bool con30 =  vc <= VFixedPoint.Zero;
            bool con31 = d1 >= VFixedPoint.Zero;
            bool con32 = d3 <= VFixedPoint.Zero;
            bool con3 = con30 && con31 && con32;//edge AB region
            if(con3)
            {
                VFixedPoint toRecipAB = d1 - d3;
                VFixedPoint recipAB = toRecipAB.Abs() >= eps ? VFixedPoint.One / toRecipAB: VFixedPoint.Zero;
                VFixedPoint t = d1 * recipAB;
                return ab * t + a;
            }
        
            //check if p in edge region of BC
            bool con40 = va <= VFixedPoint.Zero;
            bool con41 = d4 >= d3;
            bool con42 = d5 >= d6;
            bool con4 = con40 && con41 && con42; //edge BC region
            if(con4)
            {
                VInt3 bc = c - b;
                VFixedPoint toRecipBC = unom + udenom;
                VFixedPoint recipBC = toRecipBC.Abs() >= eps ? VFixedPoint.One / toRecipBC: VFixedPoint.Zero;
                VFixedPoint t = unom * recipBC;
                indices[0] = indices[1];
                indices[1] = indices[2];
                return bc * t + b;
            }
            
            //check if p in edge region of AC
            bool con50 = vb <= VFixedPoint.Zero;
            bool con51 = d2 >= VFixedPoint.Zero;
            bool con52 = d6 <= VFixedPoint.Zero;
        
            bool con5 = con50 && con51 && con52;//edge AC region
            if(con5)
            {
                VFixedPoint toRecipAC = d2 - d6;
                VFixedPoint recipAC = toRecipAC.Abs() >= eps ? VFixedPoint.One / toRecipAC : VFixedPoint.Zero;
                VFixedPoint t = d2 * recipAC;
                indices[1]=indices[2];
                return ac * t + a;
            }

            size = 1;
            //check if p in vertex region outside a
            bool con00 = d1 <= VFixedPoint.Zero; // snom <= 0
            bool con01 = d2 <= VFixedPoint.Zero; // tnom <= 0
            bool con0 = con00 && con01; // vertex region a
            if(con0)
            {
                return a;
            }

            //check if p in vertex region outside b
            bool con10 = d3 >= VFixedPoint.Zero;
            bool con11 = d3 >= d4;
            bool con1 = con10 && con11; // vertex region b
            if(con1)
            {
                indices[0] = indices[1];
                return b;
            }
            
            //p is in vertex region outside c
            indices[0] = indices[2];
            return c;

        }

        VInt3 closestPtPointTetrahedron(MinkowskiSumPoint[] Q, ref int size)
        {
            
            VFixedPoint eps = VFixedPoint.Create(1e-4f);
            MinkowskiSumPoint a = Q[0];
            MinkowskiSumPoint b = Q[1];
            MinkowskiSumPoint c = Q[2];  
            MinkowskiSumPoint d = Q[3];

            //degenerated
            VInt3 ab = b.point - a.point;
            VInt3 ac = c.point - a.point;
            VInt3 n = VInt3.Cross(ab, ac).Normalize();
            VFixedPoint signDist = VInt3.Dot(n, d.point - a.point);
            if(signDist.Abs() <= eps)
            {
                size = 3;
                return closestPtPointTriangle(Q, ref size);
            }

            VInt3 result = VInt3.zero;
            VFixedPoint bestSqDist = VFixedPoint.MaxValue;
            int[] indices = new int[] { 0, 1, 2 };
            int[] _indices = new int[]{0, 1, 2};

            if(Tetrahedron.PointOUtsideOfPlane(ORIGIN, a.point, b.point, c.point, d.point))
            {
                result = closestPtPointTriangleBaryCentric(Q[0].point, Q[1].point, Q[2].point, indices, ref size);
                bestSqDist = VInt3.Dot(result, result);
            }

            if (Tetrahedron.PointOUtsideOfPlane(ORIGIN, a.point, c.point, d.point, b.point))
            {
                int _size = 3;
                _indices[0] = 0; _indices[1] = 2; _indices[2] = 3; 
                VInt3 q = closestPtPointTriangleBaryCentric(Q[0].point, Q[2].point, Q[3].point,  _indices, ref _size);

                VFixedPoint sqDist = VInt3.Dot(q, q);
                bool con = bestSqDist >= sqDist;
                if(con)
                {
                    result = q;
                    bestSqDist = sqDist;

                    indices[0] = _indices[0];
                    indices[1] = _indices[1];
                    indices[2] = _indices[2];

                    size = _size;
                }
            }

            if (Tetrahedron.PointOUtsideOfPlane(ORIGIN, a.point, d.point, b.point, c.point))
            {
                int _size = 3;
            
                _indices[0] = 0; _indices[1] = 3; _indices[2] = 1; 

                VInt3 q = closestPtPointTriangleBaryCentric(Q[0].point, Q[3].point, Q[1].point, _indices, ref _size);
                VFixedPoint sqDist = VInt3.Dot(q, q);
                bool con = bestSqDist >= sqDist;
                if(con)
                {
                    result = q;
                    bestSqDist = sqDist;

                    indices[0] = _indices[0];
                    indices[1] = _indices[1];
                    indices[2] = _indices[2];

                    size = _size;
                }
            }

            if (Tetrahedron.PointOUtsideOfPlane(ORIGIN, b.point, c.point, d.point, a.point))
            {
                int _size = 3;
            
                _indices[0] = 1; _indices[1] = 3; _indices[2] = 2; 

                VInt3 q = closestPtPointTriangleBaryCentric(Q[1].point, Q[3].point, Q[2].point, _indices, ref _size);
                VFixedPoint sqDist = VInt3.Dot(q, q);
                bool con = bestSqDist >= sqDist;
                if(con)
                {
                    result = q;
                    bestSqDist = sqDist;

                    indices[0] = _indices[0];
                    indices[1] = _indices[1];
                    indices[2] = _indices[2];

                    size = _size;
                }
            }

            MinkowskiSumPoint q0 = Q[indices[0]]; MinkowskiSumPoint q1 = Q[indices[1]]; MinkowskiSumPoint q2 = Q[indices[2]];
            Q[0] = q0; Q[1] = q1; Q[2] = q2;

            return VInt3.zero;
        }
    }
}


