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
            if (detect(ms, simplex, d))
            {
                //this.minkowskiPenetrationSolver.getPenetration(simplex, ms, penetration);
                return true;
            }
            return false;
        }

        public bool detect(Convex convex1, VIntTransform transform1, Convex convex2, VIntTransform transform2)
        {
            List<VInt3> simplex = new List<VInt3>(3);

            MinkowskiSum ms = new MinkowskiSum(convex1, transform1, convex2, transform2);

            VInt3 d = getInitialDirection(convex1, transform1, convex2, transform2);

            return detect(ms, simplex, d);
        }

        protected VInt3 getInitialDirection(Convex convex1, VIntTransform transform1, Convex convex2, VIntTransform transform2)
        {
            VInt3 c1 = transform1.TransformPoint(convex1.getCenter());
            VInt3 c2 = transform2.TransformPoint(convex2.getCenter());

            return c2 - c1;
        }

        protected bool detect(MinkowskiSum ms, List<VInt3> simplex, VInt3 d)
        {
            if (d == VInt3.zero)
            {
                d = VInt3.forward;
            }
            simplex.Add(ms.getSupportPoint(d));
            if (VInt3.Dot(simplex[0], d) <= VFixedPoint.Zero)
            {
                return false;
            }
            d *= -1;
            do
            {
                simplex.Add(ms.getSupportPoint(d));
                if (VInt3.Dot(simplex[simplex.Count - 1], d) <= VFixedPoint.Zero)
                {
                    return false;
                }
            } while (!checkSimplex(simplex, ref d));
            return true;
        }

        protected bool checkSimplex(List<VInt3> simplex, ref VInt3 direction)
        {
            VInt3 a = simplex[simplex.Count - 1];

            if (simplex.Count == 1)
            {
                direction = a * -1;
                return false;
            }
            else if (simplex.Count == 2)
            {
                VInt3 b = simplex[0];
                direction = CrossAba(b - a, -a);
                return false;
            }
            else if (simplex.Count == 3)
            {
                VInt3 c = simplex[0];
                VInt3 b = simplex[1];
                VInt3 ao = -a;
                VInt3 ab = b - a;
                VInt3 ac = c - a;
                VInt3 abc = VInt3.Cross(ab, ac);
                VInt3 abp = VInt3.Cross(ab, abc);
                if(VInt3.Dot(abp, ao) > VFixedPoint.Zero)
                {
                    simplex.RemoveAt(0);
                    direction = CrossAba(ab, ao);
                    return false;
                }

                VInt3 acp = VInt3.Cross(abc, ac);
                if (VInt3.Dot(acp, ao) > VFixedPoint.Zero)
                {
                    simplex.RemoveAt(1);
                    direction = CrossAba(ac, ao);
                    return false;
                }

                if (VInt3.Dot(abc, ao) > VFixedPoint.Zero)
                {
                    direction = abc;
                }
                else
                {
                    simplex[1] = c;
                    simplex[0] = b;

                    direction = -abc;
                }

                return false;
            }
            else if (simplex.Count == 4)
            {
                VInt3 ao = -a;

                VInt3 ab = simplex[2] - a;
                VInt3 ac = simplex[1] - a;
                VInt3 ad = simplex[0] - a;

                VInt3 abc = VInt3.Cross(ab, ac);
                VInt3 acd = VInt3.Cross(ac, ad);
                VInt3 adb = VInt3.Cross(ad, ab);

                const int over_abc = 0x1;
                const int over_acd = 0x2;
                const int over_adb = 0x4;

                int plane_tests =
                (VInt3.Dot(abc, ao) > VFixedPoint.Zero ? over_abc : 0) |
                (VInt3.Dot(acd, ao) > VFixedPoint.Zero ? over_acd : 0) |
                (VInt3.Dot(adb, ao) > VFixedPoint.Zero ? over_adb : 0);

                switch (plane_tests)
                {
                    case 0:
                        //behind all three faces, thus inside the tetrahedron - we're done
                        return true;

                    case over_abc:
                        goto check_one_face;

                    case over_acd:
                        //rotate ACD into ABC

                        simplex[2] = simplex[1];
                        simplex[1] = simplex[0];

                        ab = ac;
                        ac = ad;

                        abc = acd;

                        goto check_one_face;

                    case over_adb:
                        //rotate ADB into ABC

                        simplex[1] = simplex[2];
                        simplex[2] = simplex[0];

                        ac = ab;
                        ab = ad;

                        abc = adb;

                        goto check_one_face;

                    case over_abc | over_acd:
                        goto check_two_faces;

                    case over_acd | over_adb:
                        //rotate ACD, ADB into ABC, ACD

                        VInt3 tmp = simplex[2];
                        simplex[2] = simplex[1];
                        simplex[1] = simplex[0];
                        simplex[0] = tmp;

                        tmp = ab;
                        ab = ac;
                        ac = ad;
                        ad = tmp;

                        abc = acd;
                        acd = adb;

                        goto check_two_faces;

                    case over_adb | over_abc:
                        //rotate ADB, ABC into ABC, ACD

                        tmp = simplex[1];
                        simplex[1] = simplex[2];
                        simplex[2] = simplex[0];
                        simplex[0] = tmp;

                        tmp = ac;
                        ac = ab;
                        ab = ad;
                        ad = tmp;

                        acd = abc;
                        abc = adb;

                        goto check_two_faces;

                    default:
                        return true;
                }

                check_one_face:
                if (VInt3.Dot(VInt3.Cross(abc, ac), ao) > VFixedPoint.Zero)
                {
                    //in the region of AC
                    simplex.RemoveAt(2);
                    simplex.RemoveAt(0);
                    direction = CrossAba(ac, ao);
                    return false;
                }

                check_one_face_part_2:
                if (VInt3.Dot(VInt3.Cross(ab, abc), ao) > VFixedPoint.Zero)
                {
                    //in the region of edge AB
                    simplex.RemoveAt(1);
                    simplex.RemoveAt(0);
                    direction = CrossAba(ab, ao);
                    return false;
                }

                //in the region of ABC

                simplex.RemoveAt(0);
                direction = abc;
                return false;

                check_two_faces:
                if (VInt3.Dot(VInt3.Cross(abc, ac), ao) > VFixedPoint.Zero)
                {
                    //the origin is beyond AC from ABC's
                    //perspective, effectively excluding
                    //ACD from consideration

                    //we thus need test only ACD

                    simplex[2] = simplex[1];
                    simplex[1] = simplex[0];

                    ab = ac;
                    ac = ad;

                    abc = acd;

                    goto check_one_face;
                }

                //at this point we know we're either over
                //ABC or over AB - all that's left is the
                //second half of the one-fase test

                goto check_one_face_part_2;
            }
            else
            {
                return false;
            }
        }

        public bool distance(Convex convex1, VIntTransform transform1, Convex convex2, VIntTransform transform2, Separation separation)
        {
            MinkowskiSum ms = new MinkowskiSum(convex1, transform1, convex2, transform2);

            VInt3 c1 = transform1.TransformPoint(convex1.getCenter());
            VInt3 c2 = transform2.TransformPoint(convex2.getCenter());

            VInt3 d = c1 - c2;
            if (d.magnitude == VFixedPoint.Zero)
            {
                return false;
            }

            List<MinkowskiSumPoint> simplex = new List<MinkowskiSumPoint>();
            simplex.Add(ms.getSupportPoints(d));
            d *= -1;
            simplex.Add(ms.getSupportPoints(d));

            d = Segment.getPointOnSegmentClosestToPoint(ORIGIN, simplex[0].point, simplex[1].point);
            if (d.magnitude <= distanceEpsilon)
            {
                return false;
            }
            d *= -1;
            simplex.Add(ms.getSupportPoints(d));

            d = Triangle.getPointOnTriangleClosestToPoint(ORIGIN, simplex[0].point, simplex[1].point, simplex[2].point);
            if (d.magnitude <= distanceEpsilon)
            {
                return false;
            }
            d *= -1;
            simplex.Add(ms.getSupportPoints(d));

            for (int i = 0; i < this.maxIterations; i++)
            {
                if (containsOrigin(simplex))
                {
                    return false;
                }

                VFixedPoint projection = VInt3.Dot(simplex[simplex.Count - 1].point, d);
                if (projection - VInt3.Dot(simplex[0].point, d) < this.distanceEpsilon)
                {
                    separation.distance = d.magnitude;
                    d = d.Normalize();
                    separation.normal = d;
                    findClosestPoints(simplex, separation);

                    return true;
                }

                d = Tetrahedron.getPointOnTetrahedronClosestToPoint(ORIGIN, simplex[0].point, simplex[1].point, simplex[2].point, simplex[3].point);
                d *= -1;
                simplex.Add(ms.getSupportPoints(d));
                simplex.RemoveAt(0);

            }
            separation.distance = d.magnitude;
            d = d.Normalize();
            separation.normal = d;
            findClosestPoints(simplex, separation);

            return true;
        }

        protected void findClosestPoints(List<MinkowskiSumPoint> input, Separation separation)
        {
            VInt3 a = input[0].point;
            VInt3 b = input[1].point;
            VInt3 c = input[2].point;
            VInt3 ab = b - a;
            VInt3 ac = c - a;
            VInt3 ap = ORIGIN - a;

            VFixedPoint d1 = VInt3.Dot(ab, ap);
            VFixedPoint d2 = VInt3.Dot(ac, ap);

            VInt3 bp = ORIGIN - b;
            VFixedPoint d3 = VInt3.Dot(ab, bp);
            VFixedPoint d4 = VInt3.Dot(ac, bp);

            VFixedPoint vc = d1 * d4 - d3 * d2;

            VInt3 cp = ORIGIN - c;
            VFixedPoint d5 = VInt3.Dot(ab, cp);
            VFixedPoint d6 = VInt3.Dot(ac, cp);

            VFixedPoint vb = d5 * d2 - d1 * d6;
            VFixedPoint va = d3 * d6 - d5 * d4;

            VFixedPoint denom = VFixedPoint.One / (va + vb + vc);
            VFixedPoint v = vb * denom;
            VFixedPoint w = vc * denom;
            separation.point1 = input[0].supportPoint1 * (VFixedPoint.One - v - w) + input[1].supportPoint1 * v + input[2].supportPoint1 * w;
            separation.point2 = input[0].supportPoint2 * (VFixedPoint.One - v - w) + input[1].supportPoint2 * v + input[2].supportPoint2 * w;
        }

        protected bool containsOrigin(MinkowskiSumPoint[] input)
        {
            if(input.Count < 4)
            {
                return false;
            }

            VInt3 a = input[0].point;
            VInt3 b = input[1].point;
            VInt3 c = input[2].point;
            VInt3 d = input[3].point;

            if(Tetrahedron.PointOUtsideOfPlane(ORIGIN, a, b, c, d))
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

        public bool raycast(Ray ray, VFixedPoint maxLength, Convex convex, VIntTransform transform, Raycast raycast)
        {

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

		VInt3 closestPtPointTriangle(MinkowskiSumPoint[] Q, ref int size)
	    {
			size = 3;
		
			VFixedPoint eps = FEps();
			MinkowskiSumPoint a = Q[0];
			MinkowskiSumPoint b = Q[1];
			MinkowskiSumPoint c = Q[2];
			VInt3 ab = b - a;
			VInt3 ac = c - a;
			VInt3 signArea = VInt3.Cross(ab, ac);//0.5*(abXac)
			VFixedPoint area = V3Dot(signArea, signArea);
			if(area <= eps)
			{
				//degenerate
				size = 2;
				return Segment.getPointOnSegmentClosestToPoint(ORIGIN, Q[0], Q[1]);
			}

			int _size;
			int[] indices= new int[]{0, 1, 2};
			VInt3 closest = closestPtPointTriangleBaryCentric(a.point, b.point, c.point, indices, _size);

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

            VFixedPoint va = V3Dot(n, bCrossC);//edge region of BC, signed area rbc, u = S(rbc)/S(abc) for a
            VFixedPoint vb = V3Dot(n, cCrossA);//edge region of AC, signed area rac, v = S(rca)/S(abc) for b
            VFixedPoint vc = V3Dot(n, aCrossB);//edge region of AB, signed area rab, w = S(rab)/S(abc) for c

            bool isFacePoints = va >= VFixedPoint.zero && vb >= VFixedPoint.zero && vc >= VFixedPoint.zero;


            //face region
            if(isFacePoints)
            {   
                nn= VInt3.Dot(n, n);
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
            bool con30 =  vc <= VFixedPoint.zero;
            bool con31 = d1 >= VFixedPoint.zero;
            bool con32 = d3 <= VFixedPoint.zero;
            bool con3 = con30 && con31 && con32;//edge AB region
            if(con3)
            {
                VFixedPoint toRecipAB = d1 - d3;
                VFixedPoint recipAB = toRecipAB.Abs() >= eps ? VFixedPoint.One / toRecipAB: VFixedPoint.zero;
                VFixedPoint t = d1 * recipAB;
                return ab * t + a;
            }
        
            //check if p in edge region of BC
            bool con40 = va <= VFixedPoint.zero;
            bool con41 = d4 >= d3;
            bool con42 = d5 >= d6;
            bool con4 = con40 && con41 && con42; //edge BC region
            if(con4)
            {
                VInt3 bc = c - b;
                VFixedPoint toRecipBC = unom + udenom;
                VFixedPoint recipBC = toRecipBC.Abs() >= eps ? VFixedPoint.One / toRecipBC: VFixedPoint.zero;
                VFixedPoint t = unom * recipBC;
                indices[0] = indices[1];
                indices[1] = indices[2];
                return bc * t + b;
            }
            
            //check if p in edge region of AC
            bool con50 = FIsGrtrOrEq(zero, vb);
            bool con51 = FIsGrtrOrEq(d2, zero);
            bool con52 = FIsGrtrOrEq(zero, d6);
        
            bool con5 = BAnd(con50, BAnd(con51, con52));//edge AC region
            if(con5)
            {
                VFixedPoint toRecipAC = d2 - d6;
                VFixedPoint recipAC = toRecipAC.Abs()) >= eps ? VFixedPoint.One / toRecipAC : VFixedPoint.zero;
                VFixedPoint t = FMul(d2, recipAC);
                indices[1]=indices[2];
                return ac * t + a;
            }

            size = 1;
            //check if p in vertex region outside a
            bool con00 = d1 <= VFixedPoint.zero; // snom <= 0
            bool con01 = d2 <= VFixedPoint.zero; // tnom <= 0
            bool con0 = con00 && con01; // vertex region a
            if(con0)
            {
                return a;
            }

            //check if p in vertex region outside b
            bool con10 = d3 >= VFixedPoint.zero;
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
            VInt3 ab = b - a;
            VInt3 ac = c - a;
            VInt3 n = VInt3.Cross(ab, ac).Normalize();
            VFixedPoint signDist = VInt3.Dot(n, V3Sub(d, a));
            if(signDist.Abs() <= eps)
            {
                size = 3;
                return closestPtPointTriangle(Q, ref size);
            }

            VInt3 result = VInt3.zero;
            VFixedPoint bestSqDist = VFixedPoint.MaxValue;
            int[] _indices = new int[]{0, 1, 2};

            if(Tetrahedron.PointOUtsideOfPlane(ORIGIN, a, b, c, d))
            {
                result = closestPtPointTriangleBaryCentric(Q[0], Q[1], Q[2], indices, ref size);
                bestSqDist = V3Dot(result, result);
            }

            if (Tetrahedron.PointOUtsideOfPlane(ORIGIN, a, c, d, b))
            {
                int _size = 3;
                _indices[0] = 0; _indices[1] = 2; _indices[2] = 3; 
                VInt3 q = closestPtPointTriangleBaryCentric(Q[0], Q[2], Q[3],  _indices, ref _size);

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

            if (Tetrahedron.PointOUtsideOfPlane(ORIGIN, a, d, b, c))
            {
                int _size = 3;
            
                _indices[0] = 0; _indices[1] = 3; _indices[2] = 1; 

                VInt3 q = closestPtPointTriangleBaryCentric(Q[0], Q[3], Q[1], _indices, ref _size);
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

            if (Tetrahedron.PointOUtsideOfPlane(ORIGIN, b, c, d, a))
            {
                int _size = 3;
            
                _indices[0] = 1; _indices[1] = 3; _indices[2] = 2; 

                VInt3 q = closestPtPointTriangleBaryCentric(Q[1], Q[3], Q[2], _indices, ref _size);
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


