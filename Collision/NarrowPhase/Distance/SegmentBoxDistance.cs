using MobaGame.FixedMath;
using System.Collections.Generic;

namespace MobaGame.Collision
{
    public static class SegmentBoxDistance
    {
        public static VFixedPoint distanceSegmentBoxSquared(VInt3 p0, VInt3 p1, VInt3 boxHalfExtent, VIntTransform boxTransform, ref VFixedPoint segmentParam,
                                        ref VInt3 boxParam)
        {
            VFixedPoint lp = VFixedPoint.Zero;
            VInt3 bp = VInt3.zero;
            VFixedPoint sqrDistance = distanceLineBoxSquared(p0, p1 - p0, boxHalfExtent, boxTransform, ref lp, ref bp);

            if (lp >= VFixedPoint.Zero)
            {
                if (lp <= VFixedPoint.One)
                {
                    segmentParam = lp;
                    boxParam = bp;
                    return sqrDistance;
                }
                else
                {
                    segmentParam = VFixedPoint.One;
                    return BoxPointDistance.distancePointBoxSquared(p1, boxTransform, boxHalfExtent, ref boxParam);
                }
            }
            else
            {
                segmentParam = VFixedPoint.Zero;
                return BoxPointDistance.distancePointBoxSquared(p0, boxTransform, boxHalfExtent, ref boxParam);
            }
        }

        public static VFixedPoint distanceLineBoxSquared(VInt3 lineOrigin, VInt3 lineDirection,
                                  VInt3 boxExtent, VIntTransform boxTransform, ref VFixedPoint lineParam, ref VInt3 boxParam)
        {
            // compute coordinates of line in box coordinate system
            VInt3 pnt = boxTransform.InverseTransformPoint(lineOrigin);
            VInt3 dir = boxTransform.InverseTransformDirection(lineDirection);

            // Apply reflections so that direction vector has nonnegative components.
            bool[] reflect = new bool[3];
            for (int i = 0; i < 3; i++)
            {
                if (dir[i] < VFixedPoint.Zero)
                {
                    pnt[i] = -pnt[i];
                    dir[i] = -dir[i];
                    reflect[i] = true;
                }
                else
                {
                    reflect[i] = false;
                }
            }

            VFixedPoint sqrDistance = VFixedPoint.Zero;

            if (dir.x > VFixedPoint.Zero)
            {
                if (dir.y > VFixedPoint.Zero)
                {
                    if (dir.z > VFixedPoint.Zero) caseNoZero(ref pnt, dir, boxExtent, ref lineParam, ref sqrDistance);
                    else case0(0, 1, 2, ref pnt, dir, boxExtent, ref lineParam, ref sqrDistance);
                }
                else
                {
                    if (dir.z > VFixedPoint.Zero) case0(0, 2, 1, ref pnt, dir, boxExtent, ref lineParam, ref sqrDistance);  // (+,0,+)
                    else case00(0, 1, 2, ref pnt, dir, boxExtent, ref lineParam, ref sqrDistance);	// (+,0,0)
                }
            }
            else
            {
                if (dir.y > VFixedPoint.Zero)
                {
                    if (dir.z > VFixedPoint.Zero) case0(1, 2, 0, ref pnt, dir, boxExtent, ref lineParam, ref sqrDistance);  // (0,+,+)
                    else case00(1, 0, 2, ref pnt, dir, boxExtent, ref lineParam, ref sqrDistance);  // (0,+,0)
                }
                else
                {
                    if (dir.z > VFixedPoint.Zero) case00(2, 0, 1, ref pnt, dir, boxExtent, ref lineParam, ref sqrDistance); // (0,0,+)
                    else
                    {
                        case000(ref pnt, boxExtent, ref sqrDistance);                                       // (0,0,0)
                        lineParam = VFixedPoint.Zero;
                    }
                }
            }

            for (int i = 0; i < 3; i++)
            {
                boxParam[i] = pnt[i] * (reflect[i] ? -1 : 1);
            }

            return sqrDistance;
        }

        static void caseNoZero(ref VInt3 pnt, VInt3 dir, VInt3 extent, ref VFixedPoint lineParam, ref VFixedPoint sqrDistance)
        {
            VInt3 kPmE = pnt - extent;

            VFixedPoint fProdDxPy = VFixedPoint.Zero, fProdDyPx = VFixedPoint.Zero, fProdDzPx = VFixedPoint.Zero, fProdDxPz = VFixedPoint.Zero, fProdDzPy = VFixedPoint.Zero, fProdDyPz = VFixedPoint.Zero;
            fProdDxPy = dir.x * kPmE.y;
            fProdDyPx = dir.y * kPmE.x;
            if (fProdDxPy >= fProdDyPx)
            {
                fProdDzPx = dir.z * kPmE.x;
                fProdDxPz = dir.x * kPmE.z;
                if (fProdDzPx >= fProdDxPz)
                {
                    // line intersects x = e0
                    face(0, 1, 2, ref pnt, dir, extent, kPmE, ref lineParam, ref sqrDistance);
                }
                else
                {
                    // line intersects z = e2
                    face(2, 0, 1, ref pnt, dir, extent, kPmE, ref lineParam, ref sqrDistance);
                }
            }
            else
            {
                fProdDzPy = dir.z * kPmE.y;
                fProdDyPz = dir.y * kPmE.z;
                if (fProdDzPy >= fProdDyPz)
                {
                    // line intersects y = e1
                    face(1, 2, 0, ref pnt, dir, extent, kPmE, ref lineParam, ref sqrDistance);
                }
                else
                {
                    // line intersects z = e2
                    face(2, 0, 1, ref pnt, dir, extent, kPmE, ref lineParam, ref sqrDistance);
                }
            }
        }

        static void case0(int i0, int i1, int i2, ref VInt3 pnt, VInt3 dir, VInt3 extents, ref VFixedPoint lineParam, ref VFixedPoint sqrDistance)
        {
            VFixedPoint fPmE0 = pnt[i0] - extents[i0];
            VFixedPoint fPmE1 = pnt[i1] - extents[i1];
            VFixedPoint fProd0 = dir[i1] * fPmE0;
            VFixedPoint fProd1 = dir[i0] * fPmE1;
            VFixedPoint fDelta = VFixedPoint.Zero, fInvLSqr = VFixedPoint.Zero, fInv = VFixedPoint.Zero;

            if (fProd0 >= fProd1)
            {
                // line intersects P[i0] = e[i0]
                pnt[i0] = extents[i0];

                VFixedPoint fPpE1 = pnt[i1] + extents[i1];
                fDelta = fProd0 - dir[i0] * fPpE1;

                if (fDelta >= VFixedPoint.Zero)
                {
                    fInvLSqr = VFixedPoint.One / (dir[i0] * dir[i0] + dir[i1] * dir[i1]);
                    sqrDistance += fDelta * fDelta * fInvLSqr;

                    pnt[i1] = -extents[i1];
                    lineParam = -(dir[i0] * fPmE0 + dir[i1] * fPpE1) * fInvLSqr;

                }
                else
                {
                    fInv = VFixedPoint.One / dir[i0];
                    pnt[i1] -= fProd0 * fInv;
                    lineParam = -fPmE0 * fInv;
                }
            }
            else
            {
                // line intersects P[i1] = e[i1]
                pnt[i1] = extents[i1];

                VFixedPoint fPpE0 = pnt[i0] + extents[i0];
                fDelta = fProd1 - dir[i1] * fPpE0;
                if (fDelta >= VFixedPoint.Zero)
                {
                    fInvLSqr = VFixedPoint.One / (dir[i0] * dir[i0] + dir[i1] * dir[i1]);
                    sqrDistance += fDelta * fDelta * fInvLSqr;
                    pnt[i0] = -extents[i0];
                    lineParam = -(dir[i0] * fPpE0 + dir[i1] * fPmE1) * fInvLSqr;
                }
                else
                {
                    fInv = VFixedPoint.One / dir[i1];
                    pnt[i0] -= fProd1 * fInv;
                    lineParam = -fPmE1 * fInv;
                }
            }

            if (pnt[i2] < -extents[i2])
            {
                fDelta = pnt[i2] + extents[i2];
                sqrDistance += fDelta * fDelta;
                pnt[i2] = -extents[i2];
            }
            else if (pnt[i2] > extents[i2])
            {
                fDelta = pnt[i2] - extents[i2];
                sqrDistance += fDelta * fDelta;
                pnt[i2] = extents[i2];
            }
        }

        static void case00(int i0, int i1, int i2, ref VInt3 pnt, VInt3 dir, VInt3 extents, ref VFixedPoint lineParam, ref VFixedPoint sqrDistance)
        {
            VFixedPoint fDelta = VFixedPoint.Zero;

            lineParam = (extents[i0] - pnt[i0]) / dir[i0];

            pnt[i0] = extents[i0];

            if (pnt[i1] < -extents[i1])
            {
                fDelta = pnt[i1] + extents[i1];
                sqrDistance += fDelta * fDelta;
                pnt[i1] = -extents[i1];
            }
            else if (pnt[i1] > extents[i1])
            {
                fDelta = pnt[i1] - extents[i1];
                sqrDistance += fDelta * fDelta;
                pnt[i1] = extents[i1];
            }

            if (pnt[i2] < -extents[i2])
            {
                fDelta = pnt[i2] + extents[i2];
                sqrDistance += fDelta * fDelta;
                pnt[i2] = -extents[i2];
            }
            else if (pnt[i2] > extents[i2])
            {
                fDelta = pnt[i2] - extents[i2];
                sqrDistance += fDelta * fDelta;
                pnt[i2] = extents[i2];
            }
        }

        static void case000(ref VInt3 rkPnt, VInt3 extents, ref VFixedPoint rfSqrDistance)
        {
            VFixedPoint fDelta = VFixedPoint.Zero;

            if (rkPnt.x < -extents.x)
            {
                fDelta = rkPnt.x + extents.x;
                rfSqrDistance += fDelta * fDelta;
                rkPnt.x = -extents.x;
            }
            else if (rkPnt.x > extents.x)
            {
                fDelta = rkPnt.x - extents.x;
                rfSqrDistance += fDelta * fDelta;
                rkPnt.x = extents.x;
            }

            if (rkPnt.y < -extents.y)
            {
                fDelta = rkPnt.y + extents.y;
                rfSqrDistance += fDelta * fDelta;
                rkPnt.y = -extents.y;
            }
            else if (rkPnt.y > extents.y)
            {
                fDelta = rkPnt.y - extents.y;
                rfSqrDistance += fDelta * fDelta;
                rkPnt.y = extents.y;
            }

            if (rkPnt.z < -extents.z)
            {
                fDelta = rkPnt.z + extents.z;
                rfSqrDistance += fDelta * fDelta;
                rkPnt.z = -extents.z;
            }
            else if (rkPnt.z > extents.z)
            {
                fDelta = rkPnt.z - extents.z;
                rfSqrDistance += fDelta * fDelta;
                rkPnt.z = extents.z;
            }
        }

        static void face(int i0, int i1, int i2, ref VInt3 rkPnt, VInt3 rkDir, VInt3 extents, VInt3 rkPmE, ref VFixedPoint pfLParam, ref VFixedPoint rfSqrDistance)
        {
            VInt3 kPpE = VInt3.zero;
            VFixedPoint fLSqr = VFixedPoint.Zero, fInv = VFixedPoint.Zero, fTmp = VFixedPoint.Zero, fParam = VFixedPoint.Zero, fT = VFixedPoint.Zero, fDelta = VFixedPoint.Zero;

            kPpE[i1] = rkPnt[i1] + extents[i1];
            kPpE[i2] = rkPnt[i2] + extents[i2];

            if (rkDir[i0] * kPpE[i1] >= rkDir[i1] * rkPmE[i0])
            {
                if (rkDir[i0] * kPpE[i2] >= rkDir[i2] * rkPmE[i0])
                {
                    rkPnt[i0] = extents[i0];
                    fInv = VFixedPoint.One / rkDir[i0];
                    rkPnt[i1] -= rkDir[i1] * rkPmE[i0] * fInv;
                    rkPnt[i2] -= rkDir[i2] * rkPmE[i0] * fInv;
                    pfLParam = -rkPmE[i0] * fInv;
                }
                else
                {
                    // v[i1] >= -e[i1], v[i2] < -e[i2]
                    fLSqr = rkDir[i0] * rkDir[i0] + rkDir[i2] * rkDir[i2];
                    fTmp = fLSqr * kPpE[i1] - rkDir[i1] * (rkDir[i0] * rkPmE[i0] + rkDir[i2] * kPpE[i2]);
                    if (fTmp <= VFixedPoint.Two * fLSqr * extents[i1])
                    {
                        fT = fTmp / fLSqr;
                        fLSqr += rkDir[i1] * rkDir[i1];
                        fTmp = kPpE[i1] - fT;
                        fDelta = rkDir[i0] * rkPmE[i0] + rkDir[i1] * fTmp + rkDir[i2] * kPpE[i2];
                        fParam = -fDelta / fLSqr;
                        rfSqrDistance += rkPmE[i0] * rkPmE[i0] + fTmp * fTmp + kPpE[i2] * kPpE[i2] + fDelta * fParam;

                        pfLParam = fParam;
                        rkPnt[i0] = extents[i0];
                        rkPnt[i1] = fT - extents[i1];
                        rkPnt[i2] = -extents[i2];
                    }
                    else
                    {
                        fLSqr += rkDir[i1] * rkDir[i1];
                        fDelta = rkDir[i0] * rkPmE[i0] + rkDir[i1] * rkPmE[i1] + rkDir[i2] * kPpE[i2];
                        fParam = -fDelta / fLSqr;
                        rfSqrDistance += rkPmE[i0] * rkPmE[i0] + rkPmE[i1] * rkPmE[i1] + kPpE[i2] * kPpE[i2] + fDelta * fParam;

                        pfLParam = fParam;
                        rkPnt[i0] = extents[i0];
                        rkPnt[i1] = extents[i1];
                        rkPnt[i2] = -extents[i2];
                    }
                }
            }
            else
            {
                if (rkDir[i0] * kPpE[i2] >= rkDir[i2] * rkPmE[i0])
                {
                    // v[i1] < -e[i1], v[i2] >= -e[i2]
                    fLSqr = rkDir[i0] * rkDir[i0] + rkDir[i1] * rkDir[i1];
                    fTmp = fLSqr * kPpE[i2] - rkDir[i2] * (rkDir[i0] * rkPmE[i0] + rkDir[i1] * kPpE[i1]);
                    if (fTmp <= VFixedPoint.Two * fLSqr * extents[i2])
                    {
                        fT = fTmp / fLSqr;
                        fLSqr += rkDir[i2] * rkDir[i2];
                        fTmp = kPpE[i2] - fT;
                        fDelta = rkDir[i0] * rkPmE[i0] + rkDir[i1] * kPpE[i1] + rkDir[i2] * fTmp;
                        fParam = -fDelta / fLSqr;
                        rfSqrDistance += rkPmE[i0] * rkPmE[i0] + kPpE[i1] * kPpE[i1] + fTmp * fTmp + fDelta * fParam;

                        pfLParam = fParam;
                        rkPnt[i0] = extents[i0];
                        rkPnt[i1] = -extents[i1];
                        rkPnt[i2] = fT - extents[i2];
                    }
                    else
                    {
                        fLSqr += rkDir[i2] * rkDir[i2];
                        fDelta = rkDir[i0] * rkPmE[i0] + rkDir[i1] * kPpE[i1] + rkDir[i2] * rkPmE[i2];
                        fParam = -fDelta / fLSqr;
                        rfSqrDistance += rkPmE[i0] * rkPmE[i0] + kPpE[i1] * kPpE[i1] + rkPmE[i2] * rkPmE[i2] + fDelta * fParam;

                        pfLParam = fParam;
                        rkPnt[i0] = extents[i0];
                        rkPnt[i1] = -extents[i1];
                        rkPnt[i2] = extents[i2];
                    }
                }
                else
                {
                    // v[i1] < -e[i1], v[i2] < -e[i2]
                    fLSqr = rkDir[i0] * rkDir[i0] + rkDir[i2] * rkDir[i2];
                    fTmp = fLSqr * kPpE[i1] - rkDir[i1] * (rkDir[i0] * rkPmE[i0] + rkDir[i2] * kPpE[i2]);
                    if (fTmp >= VFixedPoint.Zero)
                    {
                        // v[i1]-edge is closest
                        if (fTmp <= VFixedPoint.Two * fLSqr * extents[i1])
                        {
                            fT = fTmp / fLSqr;
                            fLSqr += rkDir[i1] * rkDir[i1];
                            fTmp = kPpE[i1] - fT;
                            fDelta = rkDir[i0] * rkPmE[i0] + rkDir[i1] * fTmp + rkDir[i2] * kPpE[i2];
                            fParam = -fDelta / fLSqr;
                            rfSqrDistance += rkPmE[i0] * rkPmE[i0] + fTmp * fTmp + kPpE[i2] * kPpE[i2] + fDelta * fParam;

                            pfLParam = fParam;
                            rkPnt[i0] = extents[i0];
                            rkPnt[i1] = fT - extents[i1];
                            rkPnt[i2] = -extents[i2];
                        }
                        else
                        {
                            fLSqr += rkDir[i1] * rkDir[i1];
                            fDelta = rkDir[i0] * rkPmE[i0] + rkDir[i1] * rkPmE[i1] + rkDir[i2] * kPpE[i2];
                            fParam = -fDelta / fLSqr;
                            rfSqrDistance += rkPmE[i0] * rkPmE[i0] + rkPmE[i1] * rkPmE[i1] + kPpE[i2] * kPpE[i2] + fDelta * fParam;

                            pfLParam = fParam;
                            rkPnt[i0] = extents[i0];
                            rkPnt[i1] = extents[i1];
                            rkPnt[i2] = -extents[i2];
                        }
                        return;
                    }

                    fLSqr = rkDir[i0] * rkDir[i0] + rkDir[i1] * rkDir[i1];
                    fTmp = fLSqr * kPpE[i2] - rkDir[i2] * (rkDir[i0] * rkPmE[i0] + rkDir[i1] * kPpE[i1]);

                    if (fTmp >= VFixedPoint.Zero)
                    {
                        // v[i2]-edge is closest
                        if (fTmp <= VFixedPoint.Two * fLSqr * extents[i2])
                        {
                            fT = fTmp / fLSqr;
                            fLSqr += rkDir[i2] * rkDir[i2];
                            fTmp = kPpE[i2] - fT;
                            fDelta = rkDir[i0] * rkPmE[i0] + rkDir[i1] * kPpE[i1] + rkDir[i2] * fTmp;
                            fParam = -fDelta / fLSqr;
                            rfSqrDistance += rkPmE[i0] * rkPmE[i0] + kPpE[i1] * kPpE[i1] + fTmp * fTmp + fDelta * fParam;

                            pfLParam = fParam;
                            rkPnt[i0] = extents[i0];
                            rkPnt[i1] = -extents[i1];
                            rkPnt[i2] = fT - extents[i2];
                        }
                        else
                        {
                            fLSqr += rkDir[i2] * rkDir[i2];
                            fDelta = rkDir[i0] * rkPmE[i0] + rkDir[i1] * kPpE[i1] + rkDir[i2] * rkPmE[i2];
                            fParam = -fDelta / fLSqr;
                            rfSqrDistance += rkPmE[i0] * rkPmE[i0] + kPpE[i1] * kPpE[i1] + rkPmE[i2] * rkPmE[i2] + fDelta * fParam;

                            pfLParam = fParam;
                            rkPnt[i0] = extents[i0];
                            rkPnt[i1] = -extents[i1];
                            rkPnt[i2] = extents[i2];
                        }
                        return;
                    }

                    // (v[i1],v[i2])-corner is closest
                    fLSqr += rkDir[i2] * rkDir[i2];
                    fDelta = rkDir[i0] * rkPmE[i0] + rkDir[i1] * kPpE[i1] + rkDir[i2] * kPpE[i2];
                    fParam = -fDelta / fLSqr;
                    rfSqrDistance += rkPmE[i0] * rkPmE[i0] + kPpE[i1] * kPpE[i1] + kPpE[i2] * kPpE[i2] + fDelta * fParam;

                    pfLParam = fParam;
                    rkPnt[i0] = extents[i0];
                    rkPnt[i1] = -extents[i1];
                    rkPnt[i2] = -extents[i2];
                }
            }
        }
    }
}
