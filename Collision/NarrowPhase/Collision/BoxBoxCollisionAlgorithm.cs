using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public static class BoxBoxCollisionAlgorithm
    {
        public static void processCollision(CollisionObject body0, CollisionObject body1, DispatcherInfo dispatchInfo, PersistentManifold resultOut)
        {
            BoxShape box0 = (BoxShape)body0.getCollisionShape();
            BoxShape box1 = (BoxShape)body1.getCollisionShape();

            VIntTransform transform0 = body0.getWorldTransform();
            VIntTransform transform1 = body1.getWorldTransform();

            VInt3 box0Extent = box0.getHalfExtent();
            VInt3 box1Extent = box1.getHalfExtent();

            doBoxBoxGenerateContacts(box0Extent, box1Extent, transform0, transform1, resultOut);
        }

        static void doBoxBoxGenerateContacts(VInt3 box0Extent, VInt3 box1Extent, VIntTransform transform0, VIntTransform transform1, PersistentManifold resultOut)
        {
            VFixedPoint ea0 = box0Extent.x, ea1 = box0Extent.y, ea2 = box0Extent.z;
            VFixedPoint eb0 = box1Extent.x, eb1 = box1Extent.y, eb2 = box1Extent.z;

            VIntTransform transform1To0 = transform0.InvTransform(transform1);
            VInt3 position1To0 = transform1To0.position;
            VFixedPoint tx = position1To0.x;
            VFixedPoint ty = position1To0.y;
            VFixedPoint tz = position1To0.z;
            VInt3 col0 = transform1To0.right;
            VInt3 col1 = transform1To0.up;
            VInt3 col2 = transform1To0.forward;

            VInt3 abs1To0Col0 = col0.Abs() + VInt3.one * Globals.EPS;
            VInt3 abs1To0Col1 = col1.Abs() + VInt3.one * Globals.EPS;
            VInt3 abs1To0Col2 = col2.Abs() + VInt3.one * Globals.EPS;

            VInt3[] transBasis = transform1To0.getTransposeBasis();
            VInt3 abs0To1Col0 = transBasis[0].Abs() + VInt3.one * Globals.EPS;
            VInt3 abs0To1Col1 = transBasis[1].Abs() + VInt3.one * Globals.EPS;
            VInt3 abs0To1Col2 = transBasis[2].Abs() + VInt3.one * Globals.EPS;

            VFixedPoint[] sign = new VFixedPoint[6];
            VFixedPoint[] overlap = new VFixedPoint[6];

            VFixedPoint ra = VFixedPoint.Zero, rb = VFixedPoint.Zero, radiusSum = VFixedPoint.Zero;

            //ua0
            {
                sign[0] = tx;

                rb = VInt3.Dot(abs0To1Col0, box1Extent);
                radiusSum = ea0 + rb;
                overlap[0] = radiusSum - sign[0].Abs();
                if (overlap[0] < VFixedPoint.Zero)
                    return;
            }

            //ua1
            {
                sign[1] = ty;

                rb = VInt3.Dot(abs0To1Col1, box1Extent);
                radiusSum = ea1 + rb;
                overlap[1] = radiusSum - sign[1].Abs();
                if (overlap[1] < VFixedPoint.Zero)
                    return;
            }

            //ua2
            {
                sign[1] = tz;

                rb = VInt3.Dot(abs0To1Col2, box1Extent);
                radiusSum = ea2 + rb;
                overlap[2] = radiusSum - sign[2].Abs();
                if (overlap[2] < VFixedPoint.Zero)
                    return;
            }

            //ub0
            {
                sign[3] = VInt3.Dot(position1To0, col0);

                ra = VInt3.Dot(abs1To0Col0, box0Extent);
                radiusSum = ra + eb0;
                overlap[3] = radiusSum - sign[3].Abs();
                if (overlap[3] < VFixedPoint.Zero)
                    return;
            }

            //ub1
            {
                sign[4] = VInt3.Dot(position1To0, col1);

                ra = VInt3.Dot(abs1To0Col1, box0Extent);
                radiusSum = ra + eb1;
                overlap[4] = radiusSum - sign[4].Abs();
                if (overlap[4] < VFixedPoint.Zero)
                    return;
            }

            //ub2
            {
                sign[5] = VInt3.Dot(position1To0, col2);

                ra = VInt3.Dot(abs1To0Col2, box0Extent);
                radiusSum = ra + eb2;
                overlap[5] = radiusSum - sign[5].Abs();
                if (overlap[5] < VFixedPoint.Zero)
                    return;
            }

            //ua0 x ub0
            {
                VFixedPoint absSign = (col0.y * tz - col0.z * ty).Abs();

                VFixedPoint vtemp0 = abs1To0Col0.z * ea1;
                VFixedPoint vtemp1 = abs1To0Col0.y * ea2;
                ra = vtemp0 + vtemp1;

                VFixedPoint vtemp01 = abs0To1Col0.z * eb1;
                VFixedPoint vtemp02 = abs0To1Col0.y * eb2;
                rb = vtemp01 + vtemp02;

                radiusSum = ra + rb;
                if (absSign > radiusSum) return;
            }

            //ua0 x ub1
            {
                VFixedPoint absSign = (col1.y * tz - col1.z * ty).Abs();

                VFixedPoint vtemp0 = abs1To0Col1.z * ea1;
                VFixedPoint vtemp1 = abs1To0Col1.y * ea2;
                ra = vtemp0 + vtemp1;

                VFixedPoint vtemp01 = abs0To1Col0.z * eb0;
                VFixedPoint vtemp02 = abs0To1Col0.x * eb2;
                rb = vtemp01 + vtemp02;

                radiusSum = ra + rb;
                if (absSign > radiusSum) return;
            }

            //ua0 x ub2
            {
                VFixedPoint absSign = (col2.y * tz - col2.z * ty).Abs();

                VFixedPoint vtemp0 = abs1To0Col2.z * ea1;
                VFixedPoint vtemp1 = abs1To0Col2.y * ea2;
                ra = vtemp0 + vtemp1;

                VFixedPoint vtemp01 = abs0To1Col0.y * eb0;
                VFixedPoint vtemp02 = abs0To1Col0.x * eb1;
                rb = vtemp01 + vtemp02;

                radiusSum = ra + rb;
                if (absSign > radiusSum) return;
            }

            //ua1 x ub0
            {
                VFixedPoint absSign = (col0.z * tx - col0.x * tz).Abs();

                VFixedPoint vtemp0 = abs1To0Col0.z * ea0;
                VFixedPoint vtemp1 = abs1To0Col0.x * ea2;
                ra = vtemp0 + vtemp1;

                VFixedPoint vtemp01 = abs0To1Col1.z * eb1;
                VFixedPoint vtemp02 = abs0To1Col1.y * eb2;
                rb = vtemp01 + vtemp02;

                radiusSum = ra + rb;
                if (absSign > radiusSum) return;
            }

            //ua1 x ub1
            {
                VFixedPoint absSign = (col1.z * tx - col1.x * tz).Abs();

                VFixedPoint vtemp0 = abs1To0Col0.z * ea0;
                VFixedPoint vtemp1 = abs1To0Col0.x * ea2;
                ra = vtemp0 + vtemp1;

                VFixedPoint vtemp01 = abs0To1Col1.z * eb0;
                VFixedPoint vtemp02 = abs0To1Col1.x * eb2;
                rb = vtemp01 + vtemp02;

                radiusSum = ra + rb;
                if (absSign > radiusSum) return;
            }

            //ua1 x ub2
            {
                VFixedPoint absSign = (col2.z * tx - col2.x * tz).Abs();

                VFixedPoint vtemp0 = abs1To0Col2.z * ea0;
                VFixedPoint vtemp1 = abs1To0Col2.x * ea2;
                ra = vtemp0 + vtemp1;

                VFixedPoint vtemp01 = abs0To1Col1.y * eb0;
                VFixedPoint vtemp02 = abs0To1Col1.x * eb1;
                rb = vtemp01 + vtemp02;

                radiusSum = ra + rb;
                if (absSign > radiusSum) return;
            }

            //ua2 x ub0
            {
                VFixedPoint absSign = (col0.x * ty - col0.y * tx).Abs();

                VFixedPoint vtemp0 = abs1To0Col0.y * ea0;
                VFixedPoint vtemp1 = abs1To0Col0.x * ea1;
                ra = vtemp0 + vtemp1;

                VFixedPoint vtemp01 = abs0To1Col2.z * eb1;
                VFixedPoint vtemp02 = abs0To1Col2.y * eb2;
                rb = vtemp01 + vtemp02;

                radiusSum = ra + rb;
                if (absSign > radiusSum) return;
            }

            //ua2 x ub1
            {
                VFixedPoint absSign = (col1.x * ty - col1.y * tx).Abs();

                VFixedPoint vtemp0 = abs1To0Col1.y * ea0;
                VFixedPoint vtemp1 = abs1To0Col1.x * ea1;
                ra = vtemp0 + vtemp1;

                VFixedPoint vtemp01 = abs0To1Col2.z * eb0;
                VFixedPoint vtemp02 = abs0To1Col2.x * eb2;
                rb = vtemp01 + vtemp02;

                radiusSum = ra + rb;
                if (absSign > radiusSum) return;
            }

            //ua2 x ub2
            {
                VFixedPoint absSign = (col2.x * ty - col2.y * tx).Abs();

                VFixedPoint vtemp0 = abs1To0Col2.y * ea0;
                VFixedPoint vtemp1 = abs1To0Col2.x * ea1;
                ra = vtemp0 + vtemp1;

                VFixedPoint vtemp01 = abs0To1Col2.y * eb0;
                VFixedPoint vtemp02 = abs0To1Col2.x * eb1;
                rb = vtemp01 + vtemp02;

                radiusSum = ra + rb;
                if (absSign > radiusSum) return;
            }

            VInt3 mtd = VInt3.zero;

            int feature = 0;
            VFixedPoint minOverlap = overlap[0];
            for(int i = 1; i < 6; i++)
            {
                if(minOverlap > overlap[i])
                {
                    feature = i;
                    minOverlap = overlap[i];
                }
            }

            VIntTransform newTransformV = VIntTransform.Identity;
            VInt3 axis00 = transform0.right;
            VInt3 axis01 = transform0.up;
            VInt3 axis02 = transform0.forward;
            VInt3 axis10 = transform1.right;
            VInt3 axis11 = transform1.up;
            VInt3 axis12 = transform1.forward;

            VInt3 incidentFaceNormalInNew = VInt3.zero;
            VInt3[] pts = new VInt3[4];
            switch(feature)
            {
                case 0:
                    {
                        if (sign[0] <= VFixedPoint.Zero)
                        {
                            mtd = axis00;
                            newTransformV = new VIntTransform((transform0.position - axis00 * ea0), -axis02, axis01, axis00);
                        }
                        else
                        {
                            VInt3 nAxis00 = -axis00;
                            mtd = nAxis00;
                            newTransformV = new VIntTransform((transform0.position + axis00 * ea0), axis02, axis01, nAxis00);
                        }
                        VIntTransform transform1ToNew = newTransformV.InvTransform(transform1);
                        VInt3 localNormal = newTransformV.InverseTransformDirection(mtd);
                        getIncidentPolygon(ref pts, incidentFaceNormalInNew, -localNormal, transform1ToNew, box1Extent);
                        calculateContacts(ea2, ea1, pts, incidentFaceNormalInNew, localNormal, Globals.EPS, newTransformV, resultOut, false);
                        break;
                    }
                    
                case 1:
                    {
                        if (sign[1] <= VFixedPoint.Zero)
                        {
                            mtd = axis01;
                            newTransformV = new VIntTransform((transform0.position - axis01 * ea1), axis00, -axis02, axis01);
                        }
                        else
                        {
                            VInt3 nAxis01 = -axis01;
                            mtd = nAxis01;
                            newTransformV = new VIntTransform((transform0.position + axis01 * ea1), axis00, axis02, nAxis01);
                        }
                        VIntTransform transform1ToNew = newTransformV.InvTransform(transform1);
                        VInt3 localNormal = newTransformV.InverseTransformDirection(mtd);
                        getIncidentPolygon(ref pts, incidentFaceNormalInNew, -localNormal, transform1ToNew, box1Extent);
                        calculateContacts(ea0, ea2, pts, incidentFaceNormalInNew, localNormal, Globals.EPS, newTransformV, resultOut, false);
                        break;
                    }

                case 2:
                    {
                        if (sign[2] <= VFixedPoint.Zero)
                        {
                            mtd = axis02;
                            newTransformV = new VIntTransform((transform0.position - axis02 * ea2), axis00, axis01, axis02);
                        }
                        else
                        {
                            VInt3 nAxis02 = -axis02;
                            mtd = nAxis02;
                            newTransformV = new VIntTransform((transform0.position + axis02 * ea2), axis00, -axis01, nAxis02);
                        }
                        VIntTransform transform1ToNew = newTransformV.InvTransform(transform1);
                        VInt3 localNormal = newTransformV.InverseTransformDirection(mtd);
                        getIncidentPolygon(ref pts, incidentFaceNormalInNew, -localNormal, transform1ToNew, box1Extent);
                        calculateContacts(ea0, ea1, pts, incidentFaceNormalInNew, localNormal, Globals.EPS, newTransformV, resultOut, false);
                        break;
                    }

                case 3:
                    {
                        if (sign[3] <= VFixedPoint.Zero)
                        {
                            mtd = axis10;
                            newTransformV = new VIntTransform((transform1.position - axis10 * eb0), axis12, axis11, -axis10);
                        }
                        else
                        {
                            mtd = -axis10;
                            newTransformV = new VIntTransform((transform1.position + axis10 * eb0), -axis12, axis11, axis10);
                        }
                        VIntTransform transform1ToNew = newTransformV.InvTransform(transform0);
                        VInt3 localNormal = newTransformV.InverseTransformDirection(mtd);
                        getIncidentPolygon(ref pts, incidentFaceNormalInNew, localNormal, transform1ToNew, box0Extent);
                        calculateContacts(eb2, eb1, pts, incidentFaceNormalInNew, localNormal, Globals.EPS, newTransformV, resultOut, true);
                        break;
                    }

                case 4:
                    {
                        if (sign[4] <= VFixedPoint.Zero)
                        {
                            mtd = axis11;
                            newTransformV = new VIntTransform((transform1.position - axis11 * eb1), axis10, axis12, -axis11);
                        }
                        else
                        {
                            mtd = -axis11;
                            newTransformV = new VIntTransform((transform1.position + axis11 * eb1), axis10, -axis12, axis11);
                        }
                        VIntTransform transform1ToNew = newTransformV.InvTransform(transform0);
                        VInt3 localNormal = newTransformV.InverseTransformDirection(mtd);
                        getIncidentPolygon(ref pts, incidentFaceNormalInNew, localNormal, transform1ToNew, box0Extent);
                        calculateContacts(eb0, eb2, pts, incidentFaceNormalInNew, localNormal, Globals.EPS, newTransformV, resultOut, true);
                        break;
                    }

                case 5:
                    {
                        if (sign[5] <= VFixedPoint.Zero)
                        {
                            mtd = axis12;
                            newTransformV = new VIntTransform((transform1.position - axis12 * eb2), axis10, -axis11, -axis12);
                        }
                        else
                        {
                            mtd = -axis12;
                            newTransformV = new VIntTransform((transform1.position + axis12 * eb2), axis10, axis11, axis12);
                        }
                        VIntTransform transform1ToNew = newTransformV.InvTransform(transform0);
                        VInt3 localNormal = newTransformV.InverseTransformDirection(mtd);
                        getIncidentPolygon(ref pts, incidentFaceNormalInNew, localNormal, transform1ToNew, box0Extent);
                        calculateContacts(eb0, eb1, pts, incidentFaceNormalInNew, localNormal, Globals.EPS, newTransformV, resultOut, true);
                        break;
                    }
                default:
                    return;
                  
            }
        }

        static void getIncidentPolygon(ref VInt3[] pts, VInt3 faceNormal, VInt3 axis, VIntTransform transf1To0, VInt3 extents)
        {
            VFixedPoint ex = extents.x, ey = extents.y, ez = extents.z;
            VInt3 u0 = transf1To0.right, u1 = transf1To0.up, u2 = transf1To0.forward;
            VFixedPoint d0 = VInt3.Dot(u0, axis), d1 = VInt3.Dot(u1, axis), d2 = VInt3.Dot(u2, axis);
            VFixedPoint absd0 = d0.Abs(), absd1 = d1.Abs(), absd2 = d2.Abs();
            VInt3 r0 = VInt3.zero, r1 = VInt3.zero, r2 = VInt3.zero;

            if(absd0 >= absd1 && absd0 >= absd2)
            {
                bool con = d0 > VFixedPoint.Zero;
                faceNormal = con ? -u0 : u0;
                ex = con ? -ex : ex;
                r0 = u0 * ex;
                r1 = u1 * ey;
                r2 = u2 * ez;

                VInt3 temp0 = transf1To0.position + r0;
                VInt3 temp1 = r1 + r2;
                VInt3 temp2 = r1 - r2;

                pts[0] = temp0 + temp1;
                pts[1] = temp0 + temp2;
                pts[2] = temp0 - temp1;
                pts[3] = temp0 - temp2;
            }
            else if( absd1 >= absd2)
            {
                bool con = d1 > VFixedPoint.Zero;
                faceNormal = con ? -u1 : u1;
                ey = con ? -ey : ey;
                r0 = u0 * ex;
                r1 = u1 * ey;
                r2 = u2 * ez;

                VInt3 temp0 = transf1To0.position + r1;
                VInt3 temp1 = r0 + r2;
                VInt3 temp2 = r0 - r2;

                pts[0] = temp0 + temp1;
                pts[1] = temp0 + temp2;
                pts[2] = temp0 - temp1;
                pts[3] = temp0 - temp2;
            }
            else
            {
                bool con = d2 > VFixedPoint.Zero;
                faceNormal = con ? -u2 : u2;
                ez = con ? -ez : ez;
                r0 = u0 * ex;
                r1 = u1 * ey;
                r2 = u2 * ez;

                VInt3 temp0 = transf1To0.position + r2;
                VInt3 temp1 = r0 + r1;
                VInt3 temp2 = r0 - r1;

                pts[0] = temp0 + temp1;
                pts[1] = temp0 + temp2;
                pts[2] = temp0 - temp1;
                pts[3] = temp0 - temp2;
            }
        }

        static void calculateContacts(VFixedPoint extentX, VFixedPoint extentY, VInt3[] pts, 
            VInt3 incidentfaceNormalInNew, VInt3 localNormal, VFixedPoint contactDist, VIntTransform transformNew, 
            PersistentManifold resultOut, bool flip)
        {
            localNormal = transformNew.TransformDirection(localNormal);

            VFixedPoint nExtentX = -extentX;
            VFixedPoint nExtentY = -extentY;

            bool[] penetration = new bool[4];
            bool[] area = new bool[4];

            VInt3 bmin = new VInt3(VFixedPoint.MaxValue, VFixedPoint.MaxValue, VFixedPoint.MaxValue);
            VInt3 bmax = new VInt3(VFixedPoint.MinValue, VFixedPoint.MinValue, VFixedPoint.MinValue);

            VInt3 bound = new VInt3(extentX, extentY, VFixedPoint.MaxValue);

            for(int i = 0; i < 4; i++)
            {
                bmin = VInt3.Min(bmin, pts[i]);
                bmax = VInt3.Max(bmax, pts[i]);
                VFixedPoint z = -pts[i].z;
                if(contactDist > z)
                {

                    penetration[i] = true;

                    VInt3 absPt = pts[i].Abs();
                    bool con = bound >= absPt;
                    if(con)
                    {
                        area[i] = true;

                        VInt3 localPointA = pts[i]; localPointA.z = VFixedPoint.Zero; localPointA = transformNew.TransformPoint(localPointA);
                        VInt3 localPointB = pts[i];localPointB = transformNew.TransformPoint(localPointB);
                        ManifoldPoint contactPoint = new ManifoldPoint(flip ? localPointB : localPointA, flip ? localPointA : localPointB, localNormal, z);
                        resultOut.addManifoldPoint(contactPoint);
                    }
                    else
                    {
                        area[i] = false;
                    }
                }
                else
                {
                    penetration[i] = false;
                    area[i] = false;
                }
            }

            if (resultOut.getContactPointsNum() == PersistentManifold.MANIFOLD_CACHE_SIZE)
                return;

            {
                {
                    VFixedPoint denom = incidentfaceNormalInNew.z;
                    {
                        VInt3 q0 = new VInt3(extentX, extentY, VFixedPoint.Zero);

                        if(contains(pts, q0, bmin, bmax))
                        {
                            VFixedPoint nom = VInt3.Dot(incidentfaceNormalInNew, pts[0] - q0);
                            VFixedPoint t = nom / denom;
                            VFixedPoint pen = -t;
                            if(contactDist > pen)
                            {
                                VInt3 localPointA = q0; localPointA = transformNew.TransformPoint(localPointA);
                                VInt3 localPointB = q0; localPointB.z = t; localPointB = transformNew.TransformPoint(localPointB);
                                ManifoldPoint contactPoint = new ManifoldPoint(flip ? localPointB : localPointA, flip ? localPointA : localPointB, localNormal, pen);
                                resultOut.addManifoldPoint(contactPoint);
                            }
                        }
                    }

                    {
                        VInt3 q0 = new VInt3(extentX, nExtentY, VFixedPoint.Zero);
                        if(contains(pts, q0, bmin, bmax))
                        {
                            VFixedPoint nom = VInt3.Dot(incidentfaceNormalInNew, pts[0] - q0);
                            VFixedPoint t = nom / denom;
                            VFixedPoint pen = -t;
                            if(contactDist > pen)
                            {
                                VInt3 localPointA = q0; localPointA = transformNew.TransformPoint(localPointA);
                                VInt3 localPointB = q0; localPointB.z = t; localPointB = transformNew.TransformPoint(localPointB);
                                ManifoldPoint contactPoint = new ManifoldPoint(flip ? localPointB : localPointA, flip ? localPointA : localPointB, localNormal, pen);
                                resultOut.addManifoldPoint(contactPoint);
                            }
                        }
                    }

                    {
                        VInt3 q0 = new VInt3(nExtentX, extentY, VFixedPoint.Zero);
                        if (contains(pts, q0, bmin, bmax))
                        {
                            VFixedPoint nom = VInt3.Dot(incidentfaceNormalInNew, pts[0] - q0);
                            VFixedPoint t = nom / denom;
                            VFixedPoint pen = -t;
                            if (contactDist > pen)
                            {
                                VInt3 localPointA = q0; localPointA = transformNew.TransformPoint(localPointA);
                                VInt3 localPointB = q0; localPointB.z = t; localPointB = transformNew.TransformPoint(localPointB);
                                ManifoldPoint contactPoint = new ManifoldPoint(flip ? localPointB : localPointA, flip ? localPointA : localPointB, localNormal, pen);
                                resultOut.addManifoldPoint(contactPoint);
                            }
                        }
                    }

                    {
                        VInt3 q0 = new VInt3(nExtentX, nExtentY, VFixedPoint.Zero);
                        if (contains(pts, q0, bmin, bmax))
                        {
                            VFixedPoint nom = VInt3.Dot(incidentfaceNormalInNew, pts[0] - q0);
                            VFixedPoint t = nom / denom;
                            VFixedPoint pen = -t;
                            if (contactDist > pen)
                            {
                                VInt3 localPointA = q0; localPointA = transformNew.TransformPoint(localPointA);
                                VInt3 localPointB = q0; localPointB.z = t; localPointB = transformNew.TransformPoint(localPointB);
                                ManifoldPoint contactPoint = new ManifoldPoint(flip ? localPointB : localPointA, flip ? localPointA : localPointB, localNormal, pen);
                                resultOut.addManifoldPoint(contactPoint);
                            }
                        }
                    }
                }
            }

            VInt3 ext = new VInt3(extentX, extentY, VFixedPoint.MaxValue);
            VInt3 negExt = new VInt3(nExtentX, nExtentY, -(contactDist + Globals.EPS));

            for(int start = 0, end = 3; start < 4; end = start++)
            {
                VInt3 p0 = pts[start];
                VInt3 p1 = pts[end];

                if (!penetration[start] && !penetration[end])
                    continue;

                bool con0 = penetration[start] && area[start];
                bool con1 = penetration[end] && area[end];
                if (con0 && con1)
                    continue;

                VFixedPoint tmin = VFixedPoint.Zero, tmax = VFixedPoint.Zero;
                if (AabbUtils.RayAabb2(p0, p1, new VInt3[] { ext, negExt }, ref tmin, ref tmax))
                {
                    if (!con0)
                    {
                        VInt3 intersectP = p0 * (VFixedPoint.One - tmin) + p1 * tmin;
                        VInt3 localPointA = intersectP; localPointA.z = VFixedPoint.Zero; localPointA = transformNew.TransformPoint(localPointA);
                        VInt3 localPointB = intersectP; localPointB = transformNew.TransformPoint(localPointB);
                        ManifoldPoint contactPoint = new ManifoldPoint(flip ? localPointB : localPointA, flip ? localPointA : localPointB, localNormal, -intersectP.z);
                        resultOut.addManifoldPoint(contactPoint);
                    }
                    if (!con1)
                    {
                        VInt3 intersectP = p0 * (VFixedPoint.One - tmax) + p1 * tmax;
                        VInt3 localPointA = intersectP; localPointA.z = VFixedPoint.Zero; localPointA = transformNew.TransformPoint(localPointA);
                        VInt3 localPointB = intersectP; localPointB = transformNew.TransformPoint(localPointB);
                        ManifoldPoint contactPoint = new ManifoldPoint(flip ? localPointB : localPointA, flip ? localPointA : localPointB, localNormal, -intersectP.z);
                        resultOut.addManifoldPoint(contactPoint);
                    }
                }
            }
        }

        static bool contains(VInt3[] verts, VInt3 p, VInt3 min, VInt3 max)
        {
            bool con = (min.x > p.x || p.x > max.x) || (min.y > p.y || p.y > max.y);
            if (con)
                return false;

            VFixedPoint tx = p.x, ty = p.y;
            int intersectionPoints = 0;

            for(int i = 0, j = verts.Length - 1; i < verts.Length; j = i++)
            {
                VFixedPoint jy = verts[j].y;
                VFixedPoint iy = verts[i].y;

                VFixedPoint jx = verts[j].x;
                VFixedPoint ix = verts[i].x;

                bool con0 = tx == jx && ty == jy;
                bool con1 = tx == ix && ty == iy;

                if (con0 == con1)
                    return true;

                bool yflag0 = jy > ty;
                bool yflag1 = iy > ty;

                if(yflag0 != yflag1)
                {
                    VFixedPoint jix = ix - jx;
                    VFixedPoint jiy = iy - jy;
                    VFixedPoint jty = ty - jy;
                    VFixedPoint part1 = jty * jix;
                    VFixedPoint part2 = (jx + Globals.EPS) * jiy;
                    VFixedPoint part3 = tx * jiy;

                    bool comp = jiy > VFixedPoint.Zero;
                    VFixedPoint tmp = part1 + part2;
                    VFixedPoint comp1 = comp ? tmp : part3;
                    VFixedPoint comp2 = comp ? part3 : tmp;

                    if(comp1 >= comp2)
                    {
                        if (intersectionPoints == 1)
                            return false;
                        intersectionPoints++;
                    }
                }
            }
            return intersectionPoints > 0;
        }
    }
}
