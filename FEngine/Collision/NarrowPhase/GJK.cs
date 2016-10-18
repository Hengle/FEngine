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

        protected bool containsOrigin(List<MinkowskiSumPoint> input)
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
            VFixedPoint lambda = VFixedPoint.Zero;

            bool lengthCheck = maxLength > VFixedPoint.Zero;

            Vector2 a = null;
            Vector2 b = null;

            VInt3 start = ray.getStart();

            VInt3 x = start;

            VInt3 r = ray.getDirectionVector();

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
                if (dDotW > 0.0D)
                {
                    double dDotR = d.dot(r);
                    if (dDotR >= 0.0D)
                    {
                        return false;
                    }
                    lambda -= dDotW / dDotR;
                    if ((lengthCheck) && (lambda > maxLength))
                    {
                        return false;
                    }
                    x = r.product(lambda).add(start);

                    n.set(d);
                }
                if (a != null)
                {
                    if (b != null)
                    {
                        Vector2 p1 = Segment.getPointOnSegmentClosestToPoint(x, a, p);
                        Vector2 p2 = Segment.getPointOnSegmentClosestToPoint(x, p, b);
                        if (p1.distanceSquared(x) < p2.distanceSquared(x))
                        {
                            b.set(p);

                            distanceSqrd = p1.distanceSquared(x);
                        }
                        else
                        {
                            a.set(p);

                            distanceSqrd = p2.distanceSquared(x);
                        }
                        Vector2 ab = a.to(b);
                        Vector2 ax = a.to(x);
                        d = Vector2.tripleProduct(ab, ax, ab);
                    }
                    else
                    {
                        b = p;

                        Vector2 ab = a.to(b);
                        Vector2 ax = a.to(x);
                        d = Vector2.tripleProduct(ab, ax, ab);
                    }
                }
                else
                {
                    a = p;
                    d.negate();
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
    }
}


