using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public class BoxBoxDetector: DiscreteCollisionDetectorInterface
    {
        BoxShape polyA;
        BoxShape polyB;

        VInt3[] au = new VInt3[3];
        VInt3[] bu = new VInt3[3];
        VFixedPoint[,] AbsR = new VFixedPoint[3,3];
        VFixedPoint[,] R = new VFixedPoint[3, 3];

        public void init(BoxShape box1, BoxShape box2)
        {
            polyA = box1;
            polyB = box2;
        }

        // for all 15 possible separating axes:
        //   * see if the axis separates the boxes. if so, return 0.
        //   * find the depth of the penetration along the separating axis (s2)
        //   * if this is the mininum depth so far, record it.
        // the normal vector will be set to the separating axis with the smallest
        // depth. note: normalR is set to point to a column of R1 or R2 if that is
        // the smallest depth normal so far. otherwise normalR is 0 and normalC is
        // set to a vector relative to body 1. invert_normal is 1 if the sign of
        // the normal should be flipped.
        //UNFINISHED

        public override void getClosestPoints(ClosestPointInput input, Result output)
        {
            VIntTransform transformA = input.transformA;
            VIntTransform transformB = input.transformB;

            VInt3 t = transformA.InverseTransformPoint(transformB.position);

            au[0] = transformA.right;
            au[1] = transformA.up;
            au[2] = transformA.forward;

            bu[0] = transformB.right;
            bu[1] = transformB.up;
            bu[2] = transformB.forward;

            VInt3 A = polyA.getHalfExtentsWithMargin();
            VInt3 B = polyB.getHalfExtentsWithMargin();

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    R[i,j] = VInt3.Dot(au[i], bu[j]);
                    AbsR[i, j] = R[i, j].Abs() + Globals.EPS;
                }
            }
            VFixedPoint ra, rb, T;
            VFixedPoint depth, maxDepth = VFixedPoint.MinValue;
            VInt3 worldNormal = VInt3.zero;

            int code = 0;
            // Test axes L = A0, L = A1, L = A2
            for (int i = 0; i < 3; i++)
            {
                T = t[i];
                ra = A[i];
                rb = B[0] * AbsR[i,0]
                     + B[1] * AbsR[i,1]
                     + B[2] * AbsR[i,2];
                depth = T.Abs() - (ra + rb);
                if (depth > VFixedPoint.Zero)
                {
                    return;
                }
                else if (depth > maxDepth)
                {
                    maxDepth = depth;
                    worldNormal = au[i] * (T < VFixedPoint.Zero ? -1 : 1);
                    code = i + 1;
                }
            }

            // Test axes L = B0, L = B1, L = B2
            for (int i = 0; i < 3; i++)
            {
                T = t[0] * R[0, i] + t[1] * R[1, i] + t[2] * R[2, i];
                ra = A[0] * AbsR[0,i]
                     + polyA.getHalfExtentsWithMargin()[1] * AbsR[1, i]
                     + A[2] * AbsR[2, i];
                rb = B[i];
                depth = T.Abs() - (ra + rb);
                if (depth > VFixedPoint.Zero)
                {
                    return;
                }
                else if (depth > maxDepth)
                {
                    maxDepth = depth;
                    worldNormal = bu[i] * (T < VFixedPoint.Zero ? -1 : 1);
                    code = i + 4;
                }
            }

            // Test axis L = A0 x B0
            T = t[2] * R[1, 0] - t[1] * R[2, 0];
            if (T > Globals.EPS)
            {
                ra = A[1] * AbsR[2, 0] + A[2] * AbsR[1, 0];
                rb = B[1] * AbsR[0, 2] + B[2] * AbsR[0, 1];
                depth = T.Abs() - (ra + rb);
                if (depth > VFixedPoint.Zero)
                {
                    return;
                }
                else if (depth > maxDepth)
                {
                    maxDepth = depth;
                    worldNormal = VInt3.Cross(au[0], bu[0]) * (T < VFixedPoint.Zero ? -1 : 1);
                    code = 7;
                }
            }

            // Test axis L = A0 x B1
            T = t[2] * R[1, 1] - t[1] * R[2, 1];
            if(T > Globals.EPS)
            {
                ra = A[1] * AbsR[2, 1] + A[2] * AbsR[1, 1];
                rb = B[0] * AbsR[0, 2] + B[2] * AbsR[0, 0];
                depth = T.Abs() - (ra + rb);
                if (depth > VFixedPoint.Zero)
                {
                    return;
                }
                else if (depth > maxDepth)
                {
                    maxDepth = depth;
                    worldNormal = VInt3.Cross(au[0], bu[1]) * (T < VFixedPoint.Zero ? -1 : 1);
                    code = 8;
                }
            }

            // Test axis L = A0 x B2
            T = t[2] * R[1, 2] - t[1] * R[2, 2];
            if (T > Globals.EPS)
            {
                ra = A[1] * AbsR[2, 2] + A[2] * AbsR[1, 2];
                rb = B[0] * AbsR[0, 1] + B[1] * AbsR[0, 0];
                depth = T.Abs() - (ra + rb);
                if (depth > VFixedPoint.Zero)
                {
                    return;
                }
                else if (depth > maxDepth)
                {
                    maxDepth = depth;
                    worldNormal = VInt3.Cross(au[0], bu[2]) * (T < VFixedPoint.Zero ? -1 : 1);
                    code = 9;
                }
            }

            // Test axis L = A1 x B0
            T = t[0] * R[2, 0] - t[2] * R[0, 0];
            if (T > Globals.EPS)
            {
                ra = A[0] * AbsR[2, 0] + A[2] * AbsR[0, 0];
                rb = B[1] * AbsR[1, 2] + B[2] * AbsR[1, 1];
                depth = T.Abs() - (ra + rb);
                if (depth > VFixedPoint.Zero)
                {
                    return;
                }
                else if (depth > maxDepth)
                {
                    maxDepth = depth;
                    worldNormal = VInt3.Cross(au[1], bu[0]) * (T < VFixedPoint.Zero ? -1 : 1);
                    code = 10;
                }
            }

            // Test axis L = A1 x B1
            T = t[0] * R[2, 1] - t[2] * R[0, 1];
            if (T > Globals.EPS)
            {
                ra = A[0] * AbsR[2, 1] + A[2] * AbsR[0, 1];
                rb = B[0] * AbsR[1, 2] + B[2] * AbsR[1, 0];
                depth = T.Abs() - (ra + rb);
                if (depth > VFixedPoint.Zero)
                {
                    return;
                }
                else if (depth > maxDepth)
                {
                    maxDepth = depth;
                    worldNormal = VInt3.Cross(au[1], bu[1]) * (T < VFixedPoint.Zero ? -1 : 1);
                    code = 11;
                }
            }

            // Test axis L = A1 x B2
            T = t[0] * R[2, 2] - t[2] * R[0, 2];
            if (T > Globals.EPS)
            {
                ra = A[0] * AbsR[2, 2] + A[2] * AbsR[0, 2];
                rb = B[0] * AbsR[1, 1] + B[1] * AbsR[1, 0];
                depth = T.Abs() - (ra + rb);
                if (depth > VFixedPoint.Zero)
                {
                    return;
                }
                else if (depth > maxDepth)
                {
                    maxDepth = depth;
                    worldNormal = VInt3.Cross(au[1], bu[2]) * (T < VFixedPoint.Zero ? -1 : 1);
                    code = 12;
                }
            }

            // Test axis L = A2 x B0
            T = t[1] * R[0, 0] - t[0] * R[1, 0];
            if (T > Globals.EPS)
            {
                ra = A[0] * AbsR[1, 0] + A[1] * AbsR[0, 0];
                rb = B[1] * AbsR[2, 2] + B[2] * AbsR[2, 1];
                depth = T.Abs() - (ra + rb);
                if (depth > VFixedPoint.Zero)
                {
                    return;
                }
                else if (depth > maxDepth)
                {
                    maxDepth = depth;
                    worldNormal = VInt3.Cross(au[2], bu[0]) * (T < VFixedPoint.Zero ? -1 : 1);
                    code = 13;
                }
            }

            // Test axis L = A2 x B1
            T = t[1] * R[0, 1] - t[0] * R[1, 1];
            if (T > Globals.EPS)
            {
                ra = A[0] * AbsR[1, 1] + A[1] * AbsR[0, 1];
                rb = B[0] * AbsR[2, 2] + B[2] * AbsR[2, 0];
                depth = T.Abs() - (ra + rb);
                if (depth > VFixedPoint.Zero)
                {
                    return;
                }
                else if (depth > maxDepth)
                {
                    maxDepth = depth;
                    worldNormal = VInt3.Cross(au[2], bu[1]) * (T < VFixedPoint.Zero ? -1 : 1);
                    code = 14;
                }
            }

            // Test axis L = A2 x B2
            T = t[1] * R[0, 2] - t[0] * R[1, 2];
            if (T > Globals.EPS)
            {
                ra = A[0] * AbsR[1, 2] + A[1] * AbsR[0, 2];
                rb = B[0] * AbsR[2, 1] + B[1] * AbsR[2, 0];
                depth = T.Abs() - (ra + rb);
                if (depth > VFixedPoint.Zero)
                {
                    return;
                }
                else if (depth > maxDepth)
                {
                    maxDepth = depth;
                    worldNormal = VInt3.Cross(au[2], bu[2]) * (T < VFixedPoint.Zero ? -1 : 1);
                    code = 15;
                }
            }
            output.addContactPoint(worldNormal.Normalize(), maxDepth);
        }
    }
}
