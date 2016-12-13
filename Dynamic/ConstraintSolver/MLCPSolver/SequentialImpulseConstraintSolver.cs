using System.Collections.Generic;
using MobaGame.Framework;
using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    //public delegate VFixedPoint SingleConstraintRowSolver(SolverBody body1, SolverBody body2, SolverConstraint solverConstraint);

    public class SequentialImpulseConstraintSolver: ConstraintSolver
    {
        protected readonly List<SolverBody> tmpSolverBodyPool = new List<SolverBody>();
        protected readonly List<SolverConstraint> tmpSolverContactConstraintPool = new List<SolverConstraint>();
        protected readonly List<SolverConstraint> tmpSolverNonContactConstraintPool = new List<SolverConstraint>();
        protected readonly List<SolverConstraint> tmpSolverContactFrictionConstraintPool = new List<SolverConstraint>();
        protected readonly List<SolverConstraint> tmpSolverContactRollingFrictionConstraintPool = new List<SolverConstraint>();

        protected readonly IntArrayList orderTmpConstraintPool = new IntArrayList();
        protected readonly IntArrayList orderNonContactFrictionConstraintPool = new IntArrayList();
        protected readonly IntArrayList orderFrictionConstraintPool = new IntArrayList();
        protected readonly List<TypedConstraint.ConstraintInfo1> tmpConstraintSizesPool = new List<TypedConstraint.ConstraintInfo1>();
        protected int maxOverrideNumSolverIterations;
        protected int fixedBodyId;

        protected VFixedPoint resolveSingleConstraintRowGeneric(SolverBody body1, SolverBody body2, SolverConstraint c)
        {
            VFixedPoint deltaImpulse = c.rhs - c.appliedImpulse * c.cfm;
            VFixedPoint deltaVel1Dotn = VInt3.Dot(c.contactNormal1, body1.internalGetDeltaLinearVelocity()) + VInt3.Dot(c.relpos1CrossNormal, body1.internalGetDeltaAngularVelocity());
            VFixedPoint deltaVel2Dotn = VInt3.Dot(c.contactNormal2, body2.internalGetDeltaLinearVelocity()) + VInt3.Dot(c.relpos2CrossNormal, body2.internalGetDeltaAngularVelocity());

            //	const btScalar delta_rel_vel	=	deltaVel1Dotn-deltaVel2Dotn;
            deltaImpulse -= deltaVel1Dotn * c.jacDiagABInv;
            deltaImpulse -= deltaVel2Dotn * c.jacDiagABInv;

            VFixedPoint sum = c.appliedImpulse + deltaImpulse;
            if (sum < c.lowerLimit)
            {
                deltaImpulse = c.lowerLimit - c.appliedImpulse;
                c.appliedImpulse = c.lowerLimit;
            }
            else if (sum > c.upperLimit)
            {
                deltaImpulse = c.upperLimit - c.appliedImpulse;
                c.appliedImpulse = c.upperLimit;
            }
            else
            {
                c.appliedImpulse = sum;
            }

            body1.internalApplyImpulse(c.contactNormal1 * body1.internalGetInvMass(), c.angularComponentA, deltaImpulse);
            body2.internalApplyImpulse(c.contactNormal2 * body2.internalGetInvMass(), c.angularComponentB, deltaImpulse);

            return deltaImpulse;
        }
        protected VFixedPoint resolveSingleConstraintRowLowerLimit(SolverBody body1, SolverBody body2, SolverConstraint c)
        {
            VFixedPoint deltaImpulse = c.rhs - c.appliedImpulse * c.cfm;
            VFixedPoint deltaVel1Dotn = VInt3.Dot(c.contactNormal1, body1.internalGetDeltaLinearVelocity()) + VInt3.Dot(c.relpos1CrossNormal, body1.internalGetDeltaAngularVelocity());
            VFixedPoint deltaVel2Dotn = VInt3.Dot(c.contactNormal2, body2.internalGetDeltaLinearVelocity()) + VInt3.Dot(c.relpos2CrossNormal, body2.internalGetDeltaAngularVelocity());

            deltaImpulse -= deltaVel1Dotn * c.jacDiagABInv;
            deltaImpulse -= deltaVel2Dotn * c.jacDiagABInv;
            VFixedPoint sum = c.appliedImpulse + deltaImpulse;
            if (sum < c.lowerLimit)
            {
                deltaImpulse = c.lowerLimit - c.appliedImpulse;
                c.appliedImpulse = c.lowerLimit;
            }
            else
            {
                c.appliedImpulse = sum;
            }
            body1.internalApplyImpulse(c.contactNormal1 * body1.internalGetInvMass(), c.angularComponentA, deltaImpulse);
            body2.internalApplyImpulse(c.contactNormal2 * body2.internalGetInvMass(), c.angularComponentB, deltaImpulse);

            return deltaImpulse;
        }

        protected void setupFrictionConstraint(SolverConstraint solverConstraint, VInt3 normalAxis,int solverBodyIdA,int solverBodyIdB,
                                 ManifoldPoint cp,VInt3 rel_pos1, VInt3 rel_pos2,
									CollisionObject colObj0, CollisionObject colObj1, VFixedPoint relaxation,
                                     VFixedPoint desiredVelocity, VFixedPoint cfmSlip)
        {

        }

        protected void setupTorsionalFrictionConstraint(SolverConstraint solverConstraint, VInt3 normalAxis,int solverBodyIdA,int solverBodyIdB,
                                      ManifoldPoint cp,VFixedPoint combinedTorsionalFriction, VInt3 rel_pos1, VInt3 rel_pos2,
									  CollisionObject colObj0, CollisionObject colObj1, VFixedPoint relaxation,
                                     VFixedPoint desiredVelocity, VFixedPoint cfmSlip);

        protected SolverConstraint	addFrictionConstraint(VInt3 normalAxis,int solverBodyIdA,int solverBodyIdB,int frictionIndex, ManifoldPoint cp ,
            VInt3 rel_pos1,VInt3 rel_pos2,CollisionObject colObj0, CollisionObject colObj1, VFixedPoint relaxation, VFixedPoint desiredVelocity, VFixedPoint cfmSlip);
        protected SolverConstraint	addTorsionalFrictionConstraint(VInt3 normalAxis,int solverBodyIdA,int solverBodyIdB,int frictionIndex, 
            ManifoldPoint cp,VFixedPoint torsionalFriction, VFixedPoint rel_pos1, VFixedPoint rel_pos2,CollisionObject colObj0, CollisionObject colObj1,
            VFixedPoint relaxation, VFixedPoint desiredVelocity, VFixedPoint cfmSlip);
        protected void setupContactConstraint(SolverConstraint solverConstraint, int solverBodyIdA, int solverBodyIdB, ManifoldPoint cp,
                                ContactSolverInfo infoGlobal,VFixedPoint relaxation, VFixedPoint rel_pos1, VFixedPoint rel_pos2);
        protected static void applyAnisotropicFriction(CollisionObject colObj, VInt3 frictionDirection, int frictionMode);
        protected void setFrictionConstraintImpulse(SolverConstraint solverConstraint, int solverBodyIdA, int solverBodyIdB,
                                         ManifoldPoint cp, ContactSolverInfo infoGlobal);
        protected ulong Seed2;
        protected VFixedPoint restitutionCurve(VFixedPoint rel_vel, VFixedPoint restitution);
        protected virtual void convertContacts(PersistentManifold[] manifoldPtr, ContactSolverInfo infoGlobal);
        protected void convertContact(PersistentManifold manifold,ContactSolverInfo infoGlobal);
        protected void resolveSplitPenetrationImpulseCacheFriendly(SolverBody bodyA, SolverBody bodyB, SolverConstraint contactConstraint);
        //internal method
        protected int getOrInitSolverBody(CollisionObject body, VFixedPoint timeStep);
        protected void initSolverBody(SolverBody solverBody, RigidBody rb, VFixedPoint timeStep)
        {

            solverBody.deltaLinearVelocity = VInt3.zero;
            solverBody.deltaAngularVelocity = VInt3.zero;
            solverBody.pushVelocity = VInt3.zero;
            solverBody.turnVelocity = VInt3.zero;

            if (rb != null)
            {
                solverBody.worldTransform = rb.getWorldTransform();
                solverBody.internalSetInvMass(new VInt3(rb.getInvMass(), rb.getInvMass(), rb.getInvMass()) * rb.getLinearFactor());
                solverBody.originalBody = rb;
                solverBody.angularFactor = rb.getAngularFactor();
                solverBody.linearFactor = rb.getLinearFactor();
                solverBody.linearVelocity = rb.getLinearVelocity();
                solverBody.angularVelocity = rb.getAngularVelocity();
                solverBody.externalForceImpulse = rb.getTotalForce() * rb.getInvMass() * timeStep;
                solverBody.externalTorqueImpulse = rb.getTotalTorque() * rb.getInvInertiaTensorWorld() * timeStep;

            }
            else
            {
                solverBody.worldTransform = VIntTransform.Identity;
                solverBody.internalSetInvMass(VInt3.zero);
                solverBody.originalBody = null;
                solverBody.angularFactor = VInt3.one;
                solverBody.linearFactor = VInt3.one;
                solverBody.linearVelocity = VInt3.zero;
                solverBody.angularVelocity = VInt3.zero;
                solverBody.externalForceImpulse = VInt3.zero;
                solverBody.externalTorqueImpulse = VInt3.zero;
            }
        }


        protected virtual void solveGroupCacheFriendlySplitImpulseIterations(CollisionObject[] bodies, PersistentManifold[] manifoldPtr, TypedConstraint[] constraints, ContactSolverInfo infoGlobal)
        {

        }
        protected virtual VFixedPoint solveGroupCacheFriendlyFinish(CollisionObject[] bodies, ContactSolverInfo infoGlobal);
	    protected virtual VFixedPoint solveSingleIteration(int iteration, CollisionObject[] bodies, PersistentManifold[] manifoldPtr, TypedConstraint[] constraints, ContactSolverInfo infoGlobal);

        protected virtual VFixedPoint solveGroupCacheFriendlySetup(CollisionObject[] bodies, PersistentManifold[] manifoldPtr, TypedConstraint[] constraints,ContactSolverInfo infoGlobal);
        protected virtual VFixedPoint solveGroupCacheFriendlyIterations(CollisionObject[] bodies, PersistentManifold[] manifoldPtr, TypedConstraint[] constraints,ContactSolverInfo infoGlobal)
        {
            ///this is a special step to resolve penetrations (just for contacts)
            solveGroupCacheFriendlySplitImpulseIterations(bodies, manifoldPtr,  constraints, infoGlobal);

            int maxIterations = maxOverrideNumSolverIterations > infoGlobal.numIterations ? maxOverrideNumSolverIterations : infoGlobal.numIterations;

            for (int iteration = 0; iteration < maxIterations; iteration++)
            {
                solveSingleIteration(iteration, bodies, manifoldPtr, constraints, infoGlobal);
            }
            return VFixedPoint.Zero;
        }

        public SequentialImpulseConstraintSolver()
        {
            Seed2 = 0L;

        }

        public override VFixedPoint solveGroup(CollisionObject[] bodies, PersistentManifold[] manifold, TypedConstraint[] constraints, ContactSolverInfo info, Dispatcher dispatcher)
        {
            solveGroupCacheFriendlySetup(bodies, manifold, constraints, info);

            solveGroupCacheFriendlyIterations(bodies, manifold, constraints, info);

            solveGroupCacheFriendlyFinish(bodies, info);
            return VFixedPoint.Zero;
        }

        public override void reset()
        {
            Seed2 = 0;
        }

        public ulong Rand2()
        {
            Seed2 = (1664525L * Seed2 + 1013904223L) & 0xffffffff;
            return Seed2;
        }

        public int RandInt2(int n)
        {
            // seems good; xor-fold and modulus
            ulong un = (ulong)n;
            ulong r = Rand2();

            // note: probably more aggressive than it needs to be -- might be
            //       able to get away without one or two of the innermost branches.
            if (un <= 0x00010000UL)
            {
                r ^= (r >> 16);
                if (un <= 0x00000100UL)
                {
                    r ^= (r >> 8);
                    if (un <= 0x00000010UL)
                    {
                        r ^= (r >> 4);
                        if (un <= 0x00000004UL)
                        {
                            r ^= (r >> 2);
                            if (un <= 0x00000002UL)
                            {
                                r ^= (r >> 1);
                            }
                        }
                    }
                }
            }

            return (int)(r % un);
        }

        public void setRandSeed(ulong seed)
        {
            Seed2 = seed;
        }

        public ulong getRandSeed()
  	    {
		    return Seed2;
	    }

        public virtual ConstraintSolverType getSolverType()
	    {
		    return ConstraintSolverType.BT_SEQUENTIAL_IMPULSE_SOLVER;
	    }
    }
}