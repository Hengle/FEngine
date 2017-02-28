using System.Collections.Generic;
using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    class SphereTriangleSweepAlgorithm
    {
        static int rayTriSpecial(VInt3 orig, VInt3 dir, VInt3 vert0, VInt3 edge1, VInt3 edge2, ref VFixedPoint t, ref VFixedPoint u, ref VFixedPoint v)
        {
            VInt3 pvec = VInt3.Cross(dir, edge2);
            VFixedPoint det = VInt3.Dot(edge1, pvec);
            //triangle lies in plane of triangle
            if (det > -Globals.EPS && det < Globals.EPS)
            {
                return 0;
            }

            VFixedPoint oneOverDet = VFixedPoint.One / det;

            VInt3 tvec = orig - vert0;

            u = VInt3.Dot(tvec, pvec) * oneOverDet;

            VInt3 qvec = VInt3.Cross(tvec, edge1);

            v = VInt3.Dot(dir, qvec) * oneOverDet;

            if (u < VFixedPoint.Zero || u > VFixedPoint.One) return 1;
            if (v < VFixedPoint.Zero || u + v > VFixedPoint.One) return 1;

            t = VInt3.Dot(edge2, qvec) * oneOverDet;
            return 2;
        }

        // Returns true if sphere can be tested against triangle vertex, false if edge test should be performed
        //
        // Uses a conservative approach to work for "sliver triangles" (long & thin) as well.
        static bool edgeOrVertexTest(VInt3 planeIntersectionPoint, VInt3[] tri, int vertIntersectCandidate, int vert0, int vert1, ref int secondEdgeVert)
        {
            {
                VInt3 edge0 = tri[vertIntersectCandidate] - tri[vert0];
                VFixedPoint edge0LengthSqr = edge0.sqrMagnitude;

                VInt3 diff = planeIntersectionPoint - tri[vert0];

                if (VInt3.Dot(edge0, diff) < edge0LengthSqr)
                {
                    secondEdgeVert = vert0;
                    return false;
                }
            }

            {
                VInt3 edge1 = tri[vertIntersectCandidate] - tri[vert1];
                VFixedPoint edge1LengthSqr = edge1.sqrMagnitude;

                VInt3 diff = planeIntersectionPoint - tri[vert1];

                if (VInt3.Dot(edge1, diff) < edge1LengthSqr)
                {
                    secondEdgeVert = vert1;
                    return false;
                }
            }

            return true;
        }

        static bool testRayVsSphereOrCapsule(ref VFixedPoint impactDistance, bool testSphere, VInt3 center, VFixedPoint radius, VInt3 dir, VFixedPoint length, VInt3[] verts, int e0, int e1)
        {
            if (testSphere)
            {
                VFixedPoint t = VFixedPoint.Zero; VInt3 tmp = VInt3.zero;
                if (SphereRaytestAlgorithm.rayTestSphere(center, dir * length + center, verts[e0], radius, ref tmp, ref t))
                {
                    impactDistance = t;
                    return true;
                }
            }
            else
            {
                VFixedPoint t = VFixedPoint.Zero; VInt3 tmp = VInt3.zero;
                if (CapsuleRaytestAlgorithm.raycastCapsule(center, dir * length + center, verts[e0], verts[e1], radius, ref tmp, ref t))
                {
                    impactDistance = t;
                    return true;
                }
            }

            return false;
        }

        static bool sweepSphereVsTri(VInt3[] triVerts, VInt3 normal, VInt3 center, VFixedPoint radius, VInt3 dir, VFixedPoint length, ref VFixedPoint impactDistance, ref bool directHit, bool testInitialialOverlap)
        {
            directHit = false;
            VInt3 edge10 = triVerts[1] - triVerts[0];
            VInt3 edge20 = triVerts[2] - triVerts[0];

            if (testInitialialOverlap)
            {
                VInt3 cp = Distance.closestPointTriangle2(center, triVerts[0], triVerts[1], triVerts[2], edge10, edge20);
                if ((cp - center).sqrMagnitude <= radius * radius)
                {
                    impactDistance = VFixedPoint.Zero;
                    return true;
                }
            }

            VFixedPoint u = VFixedPoint.Zero, v = VFixedPoint.Zero;
            {
                VInt3 R = normal * radius;
                if (VInt3.Dot(dir, R) >= VFixedPoint.Zero)
                {
                    R *= -1;
                }

                // The first point of the sphere to hit the triangle plane is the point of the sphere nearest to
                // the triangle plane. Hence, we use center - (normal*radius) below.

                // PT: casting against the extruded triangle in direction R is the same as casting from a ray moved by -R
                VFixedPoint t = VFixedPoint.Zero;
                int r = rayTriSpecial(center - R, dir, triVerts[0], edge10, edge20, ref t, ref u, ref v);
                if (r == 0)
                    return false;
                if (r == 2)
                {
                    if (t < VFixedPoint.Zero)
                        return false;
                    impactDistance = t;
                    directHit = true;
                    return true;
                }
            }

            //
            // Let's do some art!
            //
            // The triangle gets divided into the following areas (based on the barycentric coordinates (u,v)):
            //
            //               \   A0    /
            //                 \      /
            //                   \   /
            //                     \/ 0
            //            A02      *      A01
            //   u /              /   \          \ v
            //    *              /      \         *
            //                  /         \						.
            //               2 /            \ 1
            //          ------*--------------*-------
            //               /                 \				.
            //        A2    /        A12         \   A1
            //
            //
            // Based on the area where the computed triangle plane intersection point lies in, a different sweep test will be applied.
            //
            // A) A01, A02, A12  : Test sphere against the corresponding edge
            // B) A0, A1, A2     : Test sphere against the corresponding vertex
            //
            // Unfortunately, B) does not work for long, thin triangles. Hence there is some extra code which does a conservative check and
            // switches to edge tests if necessary.
            //

            bool TestSphere = false;
            int e0 = 0, e1 = 0;
            if (u < VFixedPoint.Zero)
            {
                if (v < VFixedPoint.Zero)
                {
                    e0 = 0;
                    VInt3 intersectPoint = triVerts[1] * u + triVerts[2] * v + triVerts[0] * (VFixedPoint.One - u - v);
                    TestSphere = edgeOrVertexTest(intersectPoint, triVerts, 0, 1, 2, ref e1);
                }
                else if (u + v > VFixedPoint.One)
                {
                    e0 = 2;
                    VInt3 intersectPoint = triVerts[1] * u + triVerts[2] * v + triVerts[0] * (VFixedPoint.One - u - v);
                    TestSphere = edgeOrVertexTest(intersectPoint, triVerts, 2, 0, 1, ref e1);
                }
                else
                {
                    TestSphere = false;
                    e0 = 0; e1 = 2;
                }
            }
            else
            {
                if (v < VFixedPoint.Zero)
                {
                    if (u + v > VFixedPoint.One)
                    {
                        e0 = 1;
                        VInt3 intersectPoint = triVerts[1] * u + triVerts[2] * v + triVerts[0] * (VFixedPoint.One - u - v);
                        TestSphere = edgeOrVertexTest(intersectPoint, triVerts, 1, 0, 2, ref e1);
                    }
                    else
                    {
                        TestSphere = false;
                        e0 = 0;
                        e1 = 1;
                    }
                }
                else
                {
                    TestSphere = false;
                    e0 = 1;
                    e1 = 2;
                }
            }

            return testRayVsSphereOrCapsule(ref impactDistance, TestSphere, center, radius, dir, length, triVerts, e0, e1);
        }

        public static bool sweepSphereTriangles(Triangle[] triangles,
                    VInt3 center, VFixedPoint radius,
                    VInt3 unitDir, VFixedPoint distance,
                    SweepHit h, ref VInt3 triNormalOut,
                    bool testInitialOverlap)
        {
            if(triangles.Length == 0)
            {
                return false;
            }

            VFixedPoint curT = distance;
            VFixedPoint dpc0 = VInt3.Dot(center, unitDir);

            VFixedPoint bestAlignment = VFixedPoint.Two;
            VInt3 bestTriNormal = VInt3.zero;

            for(int i = 0; i < triangles.Length; i++)
            {
                Triangle currentTri = triangles[i];

                if (rejectTriangle(center, unitDir, curT, radius, currentTri.verts, dpc0))
                    continue;

                VInt3 triNormal = currentTri.denormalizedNormal;

                if (VInt3.Dot(triNormal, unitDir) > VFixedPoint.Zero)
                    continue;

                VFixedPoint magnitude = triNormal.magnitude;
                if (magnitude == VFixedPoint.Zero)
                    continue;

                triNormal /= magnitude;

                VFixedPoint currentDistance = VFixedPoint.Zero;
                bool unused = false;
                if (!sweepSphereVsTri(currentTri.verts, triNormal, center, radius, unitDir, distance, ref currentDistance, ref unused, testInitialOverlap))
                    continue;


            }
        }


        static int getInitIndex(List<int> cachedIndex, int nTris)
        {
            int initIndex = 0;
            if(cachedIndex.Count > 0)
            {
                initIndex = cachedIndex[0];
            }
            return initIndex;
        }

        static bool rejectTriangle(VInt3 center, VInt3 unitDir, VFixedPoint curT, VFixedPoint radius, VInt3[] triVerts, VFixedPoint dpc0)
        {
            if (!coarseCullingTri(center, unitDir, curT, radius, triVerts))
                return true;
            if (!cullTriangle(triVerts, unitDir, radius, curT, dpc0))
                return true;
            return false;
        }

        static VFixedPoint squareDistance(VInt3 p0, VInt3 dir, VFixedPoint t, VInt3 point)
        {
            VInt3 diff = point - p0;
            VFixedPoint fT = VInt3.Dot(diff, dir);
            fT = FMath.Min(FMath.Max(fT, VFixedPoint.Zero), t);
            diff -= dir * fT;
            return diff.sqrMagnitude;
        }

        static bool coarseCullingTri(VInt3 center, VInt3 dir, VFixedPoint t, VFixedPoint radius, VInt3[] triVerts)
        {
            VInt3 triCenter = (triVerts[0] + triVerts[1] + triVerts[2]) / VFixedPoint.Create(3);

            // PT: distance between the triangle center and the swept path (an LSS)
            VFixedPoint d = FMath.Sqrt(squareDistance(center, dir, t, triCenter)) - radius - Globals.EPS;
            if (d < VFixedPoint.Zero)
                return true;

            d *= d;

            if (d <= (triCenter - triVerts[0]).sqrMagnitude) return true;
            if (d <= (triCenter - triVerts[1]).sqrMagnitude) return true;
            if (d <= (triCenter - triVerts[2]).sqrMagnitude) return true;

            return false;
        }

        static bool cullTriangle(VInt3[] triVerts, VInt3 dir, VFixedPoint radius, VFixedPoint t, VFixedPoint dpc0)
        {
            // PT: project triangle on axis
            VFixedPoint dp0 = VInt3.Dot(triVerts[0], dir);
            VFixedPoint dp1 = VInt3.Dot(triVerts[1], dir);
            VFixedPoint dp2 = VInt3.Dot(triVerts[2], dir);

            // PT: keep min value = earliest possible impact distance
            VFixedPoint dp = dp0;
            dp = FMath.Min(dp, dp1); dp = FMath.Min(dp, dp2);

            // PT: make sure we keep triangles that are about as close as best current distance
            radius += Globals.EPS;

            // PT: if earliest possible impact distance for this triangle is already larger than
            // sphere's current best known impact distance, we can skip the triangle
            if(dp > dpc0 + t + radius)
            {
                return false;
            }

            // PT: if triangle is fully located before the sphere's initial position, skip it too
            VFixedPoint dpc1 = dpc0 - radius;
            if(dp0 < dpc1 && dp1 < dpc1 && dp2 < dpc1)
            {
                return false;
            }

            return true;
        }
    }
}
