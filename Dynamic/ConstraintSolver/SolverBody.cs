using MobaGame.FixedMath;
using UnityEditor.MemoryProfiler;

namespace MobaGame.Collision
{
    public class SolverBody
    {
        public VInt3 angularVelocity;
        public VFixedPoint angularFactor;
        public VFixedPoint invMass;
        public VFixedPoint friction;
        public RigidBody originalBody;
        public VInt3 linearVelocity;
        public VInt3 centerOfMassPosition;

        public VInt3 pushVelocity;
        public VInt3 turnVelocity;

        public VInt3 getVelocityInLocalPoint(VInt3 rel_pos)
        {
            return linearVelocity + VInt3.Cross(angularVelocity, rel_pos);
        }

        /**
         * Optimization for the iterative solver: avoid calculating constant terms involving inertia, normal, relative position.
         */
        public void internalApplyImpulse(VInt3 linearComponent, VInt3 angularComponent, VFixedPoint impulseMagnitude)
        {
            if (invMass != VFixedPoint.Zero)
            {
                linearVelocity = linearComponent * impulseMagnitude + linearVelocity;
                angularVelocity = angularComponent * impulseMagnitude * angularFactor + angularVelocity;
            }
        }

        public void internalApplyPushImpulse(VInt3 linearComponent, VInt3 angularComponent, VFixedPoint impulseMagnitude)
        {
            if (invMass != VFixedPoint.Zero)
            {
                pushVelocity = linearComponent * impulseMagnitude + pushVelocity;
                turnVelocity = angularComponent * impulseMagnitude * angularFactor + turnVelocity;
            }
        }

        public void writebackVelocity()
        {
            if (invMass != VFixedPoint.Zero)
            {
                originalBody.setLinearVelocity(linearVelocity);
                originalBody.setAngularVelocity(angularVelocity);
                //m_originalBody->setCompanionId(-1);
            }
        }

        public void writebackVelocity(VFixedPoint timeStep)
        {
            if (invMass != VFixedPoint.Zero)
            {
                originalBody.setLinearVelocity(linearVelocity);
                originalBody.setAngularVelocity(angularVelocity);

                // correct the position/orientation based on push/turn recovery
                VIntTransform curTrans = originalBody.getWorldTransform();
                VIntTransform newTransform = TransformUtil.integrateTransform(curTrans, pushVelocity, turnVelocity, timeStep);
                originalBody.setWorldTransform(newTransform);

                //m_originalBody->setCompanionId(-1);
            }
        }

        public void readVelocity()
        {
            if (invMass != VFixedPoint.Zero)
            {
                linearVelocity = originalBody.getLinearVelocity();
                angularVelocity = originalBody.getAngularVelocity();
            }
        }
    }
}