using System.Collections.Generic;
using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public class ContactConstraint
    {
        static ObjectPool<JacobianEntry> jacobiansPool = new ObjectPool<JacobianEntry>();

        /**
	     * Response between two dynamic objects with friction.
         */
	    public static VFixedPoint resolveSingleCollision(
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
            cpd.appliedImpulse = VFixedPoint.Zero > sum? VFixedPoint.Zero : sum;

		    normalImpulse = cpd.appliedImpulse - oldNormalImpulse;

		    if (body1.getInvMass() != VFixedPoint.Zero)
            {
			    body1.internalApplyImpulse(contactPoint.normalWorldOnB * body1.getInvMass(), cpd.angularComponentA, normalImpulse);
		    }
		    if (body2.getInvMass() != VFixedPoint.Zero)
            {
			    body2.internalApplyImpulse(contactPoint.normalWorldOnB * body2.getInvMass(), cpd.angularComponentB, -normalImpulse);
		    }

		    return normalImpulse;
	    }

        /**
	     * velocity + friction<br>
	     * response between two dynamic objects with friction
	     */
        public static VFixedPoint resolveSingleFriction(
            RigidBody body1,
            RigidBody body2,
            ManifoldPoint contactPoint,
            ContactSolverInfo solverInfo)
        {
		    VInt3 pos1 = contactPoint.getPositionWorldOnA();
            VInt3 pos2 = contactPoint.getPositionWorldOnB();

            VInt3 rel_pos1 = pos1 - body1.getCenterOfMassPosition();
            VInt3 rel_pos2 = pos2 - body2.getCenterOfMassPosition();

		    ConstraintPersistentData cpd = (ConstraintPersistentData)contactPoint.userPersistentData;

            VFixedPoint combinedFriction = cpd.friction;

            VFixedPoint limit = cpd.appliedImpulse * combinedFriction;

		    if (cpd.appliedImpulse > VFixedPoint.Zero) //friction
		    {
			    //apply friction in the 2 tangential directions

			    // 1st tangent
			    VInt3 vel1 = body1.getVelocityInLocalPoint(rel_pos1);
                VInt3 vel2 = body2.getVelocityInLocalPoint(rel_pos2);
			    VInt3 vel = vel1 - vel2;

				VFixedPoint vrel = VInt3.Dot(cpd.frictionWorldTangential0, vel);

                // calculate j that moves us to zero relative velocity
                VFixedPoint j1 = -vrel* cpd.jacDiagABInvTangent0;
                VFixedPoint oldTangentImpulse = cpd.accumulatedTangentImpulse0;
                cpd.accumulatedTangentImpulse0 = oldTangentImpulse + j1;

				cpd.accumulatedTangentImpulse0 = FMath.Min(cpd.accumulatedTangentImpulse0, limit);
				cpd.accumulatedTangentImpulse0 = FMath.Max(cpd.accumulatedTangentImpulse0, -limit);
				j1 = cpd.accumulatedTangentImpulse0 - oldTangentImpulse;

				// 2nd tangent
				vrel = VInt3.Dot(cpd.frictionWorldTangential1, vel);

                // calculate j that moves us to zero relative velocity
                VFixedPoint j2 = -vrel* cpd.jacDiagABInvTangent1;
                oldTangentImpulse = cpd.accumulatedTangentImpulse1;
                cpd.accumulatedTangentImpulse1 = oldTangentImpulse + j2;

				cpd.accumulatedTangentImpulse1 = FMath.Min(cpd.accumulatedTangentImpulse1, limit);
				cpd.accumulatedTangentImpulse1 = FMath.Max(cpd.accumulatedTangentImpulse1, -limit);
				j2 = cpd.accumulatedTangentImpulse1 - oldTangentImpulse;

			    if (body1.getInvMass() != VFixedPoint.Zero)
                {
				    body1.internalApplyImpulse(cpd.frictionWorldTangential0 * body1.getInvMass(), cpd.frictionAngularComponent0A, j1);
				    body1.internalApplyImpulse(cpd.frictionWorldTangential1 * body1.getInvMass(), cpd.frictionAngularComponent1A, j2);
			    }
			    if (body2.getInvMass() != VFixedPoint.Zero)
                {
				    body2.internalApplyImpulse(cpd.frictionWorldTangential0 * body2.getInvMass(), cpd.frictionAngularComponent0B, -j1);
				    body2.internalApplyImpulse(cpd.frictionWorldTangential1 * body2.getInvMass(), cpd.frictionAngularComponent1B, -j2);
			    }
		    }
		    return cpd.appliedImpulse;
	    }

        /**
	     * velocity + friction<br>
	     * response between two dynamic objects with friction
	     */
        public static VFixedPoint resolveSingleCollisionCombined(
                RigidBody body1,
                RigidBody body2,
                ManifoldPoint contactPoint,
                ContactSolverInfo solverInfo)
        {		
		    VInt3 pos1 = contactPoint.getPositionWorldOnA();
		    VInt3 pos2 = contactPoint.getPositionWorldOnB();
		    VInt3 normal = contactPoint.normalWorldOnB;

            VInt3 rel_pos1 = pos1 - body1.getCenterOfMassPosition();
            VInt3 rel_pos2 = pos2 - body2.getCenterOfMassPosition();

            VInt3 vel1 = body1.getVelocityInLocalPoint(rel_pos1);
		    VInt3 vel2 = body2.getVelocityInLocalPoint(rel_pos2);
		    VInt3 vel = vel1 - vel2;

		    VFixedPoint rel_vel = VInt3.Dot(normal, vel);

            VFixedPoint Kfps = VFixedPoint.One / solverInfo.timeStep;

            //btScalar damping = solverInfo.m_damping ;
            VFixedPoint Kerp = solverInfo.erp;
            VFixedPoint Kcor = Kerp * Kfps;

            ConstraintPersistentData cpd = (ConstraintPersistentData)contactPoint.userPersistentData;

            VFixedPoint distance = cpd.penetration;
            VFixedPoint positionalError = Kcor * -distance;
            VFixedPoint velocityError = cpd.restitution - rel_vel;// * damping;

            VFixedPoint penetrationImpulse = positionalError * cpd.jacDiagABInv;

            VFixedPoint velocityImpulse = velocityError * cpd.jacDiagABInv;

            VFixedPoint normalImpulse = penetrationImpulse + velocityImpulse;

            // See Erin Catto's GDC 2006 paper: Clamp the accumulated impulse
            VFixedPoint oldNormalImpulse = cpd.appliedImpulse;
            VFixedPoint sum = oldNormalImpulse + normalImpulse;
            cpd.appliedImpulse = VFixedPoint.Zero > sum? VFixedPoint.Zero : sum;

		    normalImpulse = cpd.appliedImpulse - oldNormalImpulse;

		    if (body1.getInvMass() != VFixedPoint.Zero)
            {
			    body1.internalApplyImpulse(contactPoint.normalWorldOnB * body1.getInvMass(), cpd.angularComponentA, normalImpulse);
		    }
		    if (body2.getInvMass() != VFixedPoint.Zero)
            {
			    body2.internalApplyImpulse(contactPoint.normalWorldOnB * body2.getInvMass(), cpd.angularComponentB, -normalImpulse);
		    }

		    
            //friction
            vel1 = body1.getVelocityInLocalPoint(rel_pos1);
            vel2 = body2.getVelocityInLocalPoint(rel_pos2);
			vel = vel1 - vel2;

			rel_vel = VInt3.Dot(normal, vel);

			VInt3 lat_vel = vel - normal * rel_vel;
			VFixedPoint lat_rel_vel = lat_vel.magnitude;

            VFixedPoint combinedFriction = cpd.friction;

			if (cpd.appliedImpulse > VFixedPoint.Zero) {
				if (lat_rel_vel > VFixedPoint.Zero) {
					lat_vel  /= lat_rel_vel;

					VInt3 temp1 = VInt3.Cross(rel_pos1, lat_vel);
					body1.getInvInertiaTensorWorld().transform(temp1);

                    VInt3 temp2 = VInt3.Cross(rel_pos2, lat_vel);
					body2.getInvInertiaTensorWorld().transform(temp2);

                    VInt3 java_tmp1 = VInt3.Cross(temp1, rel_pos1);

					VInt3 java_tmp2 = VInt3.Cross(temp2, rel_pos2);

                    VInt3 tmp = java_tmp1 + java_tmp2;

					VFixedPoint friction_impulse = lat_rel_vel /
                            (body1.getInvMass() + body2.getInvMass() + VInt3.Dot(lat_vel, tmp));
                    VFixedPoint normal_impulse = cpd.appliedImpulse * combinedFriction;

                    friction_impulse = FMath.Min(friction_impulse, normal_impulse);
					friction_impulse = FMath.Max(friction_impulse, -normal_impulse);
					body1.applyImpulse(lat_vel * -friction_impulse, rel_pos1);
					body2.applyImpulse(lat_vel * friction_impulse, rel_pos2);
				}
			}
		    return normalImpulse;
	    }

        public static VFixedPoint resolveSingleFrictionEmpty(
            RigidBody body1,
            RigidBody body2,
            ManifoldPoint contactPoint,
            ContactSolverInfo solverInfo)
        {
            return VFixedPoint.Zero;
        }
    }
}
