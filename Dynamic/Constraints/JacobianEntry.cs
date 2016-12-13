using System.Collections.Generic;
using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public class JacobianEntry
    {
        public VInt3 linearJointAxis;
        public VInt3 aJ;
        public VInt3 bJ;
        public VInt3 m_0MinvJt;
        public VInt3 m_1MinvJt;
        public VFixedPoint Adiag;


        /**
         * Constraint between two different rigidbodies.
         */
        public void init(VIntTransform A2world,
                VIntTransform B2world,
                VInt3 rel_pos1, VInt3 rel_pos2,
                VInt3 jointAxis,
                VInt3 inertiaInvA,
                VFixedPoint massInvA,
                VInt3 inertiaInvB,
                VFixedPoint massInvB)
        {
            linearJointAxis = jointAxis;

            aJ = VInt3.Cross(rel_pos1, linearJointAxis);
            aJ = A2world.InverseTransformVector(aJ);

            bJ = VInt3.Cross(rel_pos2 , - linearJointAxis);
            bJ = B2world.InverseTransformVector(bJ);

            m_0MinvJt = aJ * inertiaInvA;
            m_1MinvJt = bJ * inertiaInvB;
            Adiag = massInvA + VInt3.Dot(m_0MinvJt, aJ) + massInvB + VInt3.Dot(m_1MinvJt, bJ);
        }

        /**
         * Angular constraint between two different rigidbodies.
         */
        /*public void init(VInt3 jointAxis,
            VIntTransform world2A,
            VIntTransform world2B,
            VInt3 inertiaInvA,
            VInt3 inertiaInvB)
        {
            linearJointAxis.set(0f, 0f, 0f);

            aJ.set(jointAxis);
            world2A.transform(aJ);

            bJ.set(jointAxis);
            bJ.negate();
            world2B.transform(bJ);

            VectorUtil.mul(m_0MinvJt, inertiaInvA, aJ);
            VectorUtil.mul(m_1MinvJt, inertiaInvB, bJ);
            Adiag = m_0MinvJt.dot(aJ) + m_1MinvJt.dot(bJ);
        }*/

        /**
         * Angular constraint between two different rigidbodies.
         */
        /*public void init(VInt3 axisInA,
            VInt3 axisInB,
            VInt3 inertiaInvA,
            VInt3 inertiaInvB)
        {
            linearJointAxis.set(0f, 0f, 0f);
            aJ.set(axisInA);

            bJ.set(axisInB);
            bJ.negate();

            VectorUtil.mul(m_0MinvJt, inertiaInvA, aJ);
            VectorUtil.mul(m_1MinvJt, inertiaInvB, bJ);
            Adiag = m_0MinvJt.dot(aJ) + m_1MinvJt.dot(bJ);
        }*/

        /**
         * Constraint on one rigidbody.
         */
        /*public void init(
            VIntTransform world2A,
            VInt3 rel_pos1, VInt3 rel_pos2,
            VInt3 jointAxis,
            VInt3 inertiaInvA,
            VFixedPoint massInvA)
        {
            linearJointAxis.set(jointAxis);

            aJ.cross(rel_pos1, jointAxis);
            world2A.transform(aJ);

            bJ.set(jointAxis);
            bJ.negate();
            bJ.cross(rel_pos2, bJ);
            world2A.transform(bJ);

            VectorUtil.mul(m_0MinvJt, inertiaInvA, aJ);
            m_1MinvJt.set(0f, 0f, 0f);
            Adiag = massInvA + m_0MinvJt.dot(aJ);
        }*/

        public VFixedPoint getDiagonal() { return Adiag; }
    }
}
