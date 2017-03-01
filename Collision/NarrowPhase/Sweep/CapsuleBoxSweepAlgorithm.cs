﻿using System.Collections.Generic;
using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    class CapsuleBoxSweepAlgorithm
    {
        public static void objectQuerySingle(CollisionObject castObject, VInt3 FromPos, VInt3 ToPos, CollisionObject collisionObject, List<CastResult> results, VFixedPoint allowedPenetration)
        {
            
        }

        static void OUTPUT_TRI(Triangle[] triangles, ref int index, VInt3 p0, VInt3 p1, VInt3 p2)
        {
            Triangle t = triangles[index];
            t.verts[0] = p0;
            t.verts[1] = p1;
            t.verts[2] = p2;
            index++;
        }

        static void OUTPUT_TRI2(Triangle[] triangles, ref int index, VInt3 p0, VInt3 p1, VInt3 p2, VInt3 d)
        {
            Triangle t = triangles[index];
            t.verts[0] = p0;
            t.verts[1] = p1;
            t.verts[2] = p2;
            VInt3 denormalizedNormal = triangles[index].denormalizedNormal;
            if(VInt3.Dot(denormalizedNormal, d) > VFixedPoint.Zero)
            {
                VInt3 tmp = t.verts[1];
                t.verts[1] = t.verts[2];
                t.verts[2] = tmp;
            }

            index++;
        }

        public static int extrudeMesh(Triangle[] triangles, VInt3 extrudeDir, VInt3 dir, Triangle[] tris)
        {
            int num = 0;
            for(int i = 0; i < triangles.Length; i++)
            {
                Triangle currentTriangle = triangles[i];

                VInt3 denormalizedNormal = currentTriangle.denormalizedNormal;

                bool culled = VInt3.Dot(denormalizedNormal, dir) > VFixedPoint.Zero;
                if (culled) continue;

                VInt3 p0 = currentTriangle.verts[0];
                VInt3 p1 = currentTriangle.verts[1];
                VInt3 p2 = currentTriangle.verts[2];

                VInt3 p0b = p0 + extrudeDir;
                VInt3 p1b = p1 + extrudeDir;
                VInt3 p2b = p2 + extrudeDir;

                p0 -= extrudeDir;
                p1 -= extrudeDir;
                p2 -= extrudeDir;

                if (VInt3.Dot(denormalizedNormal, extrudeDir) >= VFixedPoint.Zero) OUTPUT_TRI(tris, ref num, p0b, p1b, p2b);
                else OUTPUT_TRI(tris, ref num, p0, p1, p2);

                {
                    OUTPUT_TRI2(tris, ref num, p1, p1b, p2b, dir);
                    OUTPUT_TRI2(tris, ref num, p1, p2b, p2, dir);
                }

                {
                    OUTPUT_TRI2(tris, ref num, p0, p2, p2b, dir);
                    OUTPUT_TRI2(tris, ref num, p0, p2b, p0b, dir);
                }

                {
                    OUTPUT_TRI2(tris, ref num, p0b, p1b, p1, dir);
                    OUTPUT_TRI2(tris, ref num, p0b, p1, p0, dir);
                }
            }

            return num;
        }

        
        static VInt3[] boxP;
        static void computeBoxPoints(VInt3 aabbMax, VInt3 aabbMin)
        {
            if(boxP == null)
            {
                boxP = new VInt3[8];
                for (int i = 0; i < 8; i++)
                {
                    boxP[i] = VInt3.zero;
                }
            }

            {
                boxP[0].x = aabbMin.x; boxP[0].y = aabbMin.y;  boxP[0].z = aabbMin.z;
                boxP[1].x = aabbMax.x; boxP[1].y = aabbMin.y; boxP[1].z = aabbMin.z;
                boxP[2].x = aabbMax.x; boxP[2].y = aabbMax.y; boxP[2].z = aabbMin.z;
                boxP[3].x = aabbMin.x; boxP[3].y = aabbMax.y; boxP[3].z = aabbMin.z;
                boxP[4].x = aabbMin.x; boxP[4].y = aabbMin.y; boxP[4].z = aabbMax.z;
                boxP[5].x = aabbMax.x; boxP[5].y = aabbMin.y; boxP[5].z = aabbMax.z;
                boxP[6].x = aabbMax.x; boxP[6].y = aabbMax.y; boxP[6].z = aabbMax.z;
                boxP[7].x = aabbMin.x; boxP[7].y = aabbMax.y; boxP[7].z = aabbMax.z;
            }
        }

        static int[] Indices = new int[]
        {
            0,2,1,  0,3,2,
            1,6,5,  1,2,6,
            5,7,4,  5,6,7,
            4,3,0,  4,7,3,
            3,6,2,  3,7,6,
            5,0,1,  5,4,0
        };

        static Triangle[] boxTris;
        static int extrudeBox(BoxShape localBox, VIntTransform world, VInt3 extrusionDir, Triangle[] tris, VInt3 dir)
        {
            VInt3 aabbMax, aabbMin;
            localBox.getAabb(world, out aabbMin, out aabbMax);
            computeBoxPoints(aabbMax, aabbMin);

            if(boxTris == null)
            {
                boxTris = new Triangle[12];
                for(int i = 0; i < 12; i++)
                {
                    boxTris[i] = new Triangle();
                }
            }

            for(int i = 0; i < 12; i++)
            {
                int VRef0 = Indices[i * 3 + 0];
                int VRef1 = Indices[i * 3 + 1];
                int VRef2 = Indices[i * 3 + 2];

                Triangle atriangle = boxTris[i];

                atriangle.verts[0] = world.TransformPoint(boxP[VRef0]);
                atriangle.verts[0] = world.TransformPoint(boxP[VRef1]);
                atriangle.verts[0]= world.TransformPoint(boxP[VRef2]);
                
            }

            return extrudeMesh(boxTris, extrusionDir, dir, tris);
        }

        public static bool sweepCapsuleBox(VInt3 p0, VInt3 p1, VFixedPoint radius, VInt3 boxHalfExtension, VIntTransform boxTransform, VInt3 dir, VFixedPoint length, ref VFixedPoint fraction, ref VInt3 hitNormal)
        {
            if(Distance.distanceSegmentBoxSquared(p0, p1, radius, boxHalfExtension, boxTransform) < radius * radius)
            {
                fraction = VFixedPoint.Zero;
                hitNormal = -dir;
                return true;
            }  



        }
    }
}
