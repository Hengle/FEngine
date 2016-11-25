using MobaGame.FixedMath;
using System.Collections.Generic;
using System;

namespace MobaGame.Collision
{
    public class BoxBoxDetector: DiscreteCollisionDetectorInterface
    {
        BoxShape box1;
        BoxShape box2;

        public BoxBoxDetector(BoxShape box1, BoxShape box2)
        {
            this.box1 = box1;
            this.box2 = box2;
        }

        public override void getClosestPoints(ClosestPointInput input, Result output)
        {
            VIntTransform transformA = input.transformA;
            VIntTransform transformB = input.transformB;

            VInt3[] R1 = new VInt3[4];
            VInt3[] R2 = new VInt3[4];

            for (int j = 0; j < 3; j++)
            {
                R1[j] = transformA.getBasis()[j];
                R2[j] = transformB.getBasis()[j];
            }

            VInt3 normal;
            VFixedPoint depth;
            int return_code;
            int maxc = 4;

            return_code = dBoxBox2(
                transformA.position,
                R1,
                box1.getHalfExtentsWithMargin() * VFixedPoint.Two,
                transformB.position,
                R2,
                box2.getHalfExtentsWithMargin() * VFixedPoint.Two,
                out normal, out depth, out return_code, maxc, output);
        }


        // given two boxes (p1,R1,side1) and (p2,R2,side2), collide them together and
        // generate contact points. this returns 0 if there is no contact otherwise
        // it returns the number of contacts generated.
        // `normal' returns the contact normal.
        // `depth' returns the maximum penetration depth along that normal.
        // `return_code' returns a number indicating the type of contact that was
        // detected:
        //        1,2,3 = box 2 intersects with a face of box 1
        //        4,5,6 = box 1 intersects with a face of box 2
        //        7..15 = edge-edge contact
        // `maxc' is the maximum number of contacts allowed to be generated, i.e.
        // the size of the `contact' array.
        // `contact' and `skip' are the contact array information provided to the
        // collision functions. this function only fills in the position and depth
        // fields.
        int dBoxBox2(VInt3 p1, VInt3[] R1, VInt3 side1,
                    VInt3 p2, VInt3[] R2, VInt3 side2,
	                out VInt3 normal, out VFixedPoint depth, out int return_code
                    int maxc, Result output)
        {
            VFixedPoint fudge_factor = VFixedPoint.Create(1.05f);
            VInt3 p , pp, normalC = VInt3.zero;
            VFixedPoint normalR = 0;
            VFixedPoint[] A = new VFixedPoint[3], B = new VFixedPoint[3];
            VFixedPoint R11, R12, R13, R21, R22, R23, R31, R32, R33,
              Q11, Q12, Q13, Q21, Q22, Q23, Q31, Q32, Q33, s, s2, l;
            int i, j, invert_normal, code;

            // get vector from centers of box 1 to box 2, relative to box 1
            p = p2 - p1;
            dMULTIPLY1_331(pp, R1, p);      // get pp = p relative to body 1

            // get side lengths / 2
            A[0] = side1[0] * VFixedPoint.Half;
            A[1] = side1[1] * VFixedPoint.Half;
            A[2] = side1[2] * VFixedPoint.Half;
            B[0] = side2[0] * VFixedPoint.Half;
            B[1] = side2[1] * VFixedPoint.Half;
            B[2] = side2[2] * VFixedPoint.Half;
        }
    }
}
