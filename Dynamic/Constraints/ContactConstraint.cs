using System.Collections.Generic;
using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public class ContactConstraint
    {
        public static readonly ContactSolverFunc resolveSingleCollision = 
            (RigidBody body1, RigidBody body2, ManifoldPoint contactPoint, ContactSolverInfo info) => ResolveSingleCollision(body1, body2, contactPoint, info);

        public static readonly ContactSolverFunc resolveSingleFriction = 
            (RigidBody body1, RigidBody body2, ManifoldPoint contactPoint, ContactSolverInfo info) => RresolveSingleFriction(body1, body2, contactPoint, info);
            
        public static readonly ContactSolverFunc resolveSingleCollisionCombined = 
            (RigidBody body1, RigidBody body2, ManifoldPoint contactPoint, ContactSolverInfo info) => ResolveSingleCollisionCombined(body1, body2, contactPoint, info);

        static ObjectPool<JacobianEntry> jacobiansPool = new ObjectPool<JacobianEntry>();

        /**
	     * Response between two dynamic objects with friction.
         */
	    public static VFixedPoint ResolveSingleCollision(
                RigidBody body1,
                RigidBody body2,
                ManifoldPoint contactPoint,
                ContactSolverInfo solverInfo)
        {
		    VInt3 pos1_ = contactPoint.getPositionWorldOnA();
		    VInt3 pos2_ = contactPoint.getPositionWorldOnB();
		    VInt3 normal = contactPoint.normalWorldOnB;

            // constant over all iterations
            VInt3 rel_pos1 = pos1_ - body1.getCenterOfMassPosition();
            VInt3 rel_pos2 = pos2_ - body2.getCenterOfMassPosition();

            VInt3 vel1 = body1.getVelocityInLocalPoint(rel_pos1);
		    VInt3 vel2 = body2.getVelocityInLocalPoint(rel_pos2);
		    VInt3 vel = vel1 - vel2;

		    VFixedPoint rel_vel = VInt3.Dot(normal, vel);

            VFixedPoint Kfps = VFixedPoint.One / solverInfo.timeStep;

            VFixedPoint Kerp = solverInfo.erp;
            VFixedPoint Kcor = Kerp * Kfps;

            ConstraintPersistentData cpd = contactPoint.userPersistentData;

            VFixedPoint distance = cpd.penetration;
            VFixedPoint positionalError = Kcor * -distance;
            VFixedPoint velocityError = cpd.restitution - rel_vel; // * damping;

            VFixedPoint penetrationImpulse = positionalError * cpd.jacDiagABInv;

            VFixedPoint velocityImpulse = velocityError * cpd.jacDiagABInv;

            VFixedPoint normalImpulse = penetrationImpulse + velocityImpulse;

            // See Erin Catto's GDC 2006 paper: Clamp the accumulated impulse
            VFixedPoint oldNormalImpulse = cpd.appliedImpulse;
            VFixedPoint sum = oldNormalImpulse + normalImpulse;
            cpd.appliedImpulse = 0f > sum? 0f : sum;

		    normalImpulse = cpd.appliedImpulse - oldNormalImpulse;

		    Vector3f tmp = Stack.alloc(Vector3f.class);
		    if (body1.getInvMass() != 0f) {
			    tmp.scale(body1.getInvMass(), contactPoint.normalWorldOnB);
			    body1.internalApplyImpulse(tmp, cpd.angularComponentA, normalImpulse);
		    }
		    if (body2.getInvMass() != 0f) {
			    tmp.scale(body2.getInvMass(), contactPoint.normalWorldOnB);
			    body2.internalApplyImpulse(tmp, cpd.angularComponentB, -normalImpulse);
		    }

		    return normalImpulse;
	    }
    }
}
