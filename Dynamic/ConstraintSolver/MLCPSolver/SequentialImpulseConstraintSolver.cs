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
        protected readonly IntArrayList orderNonContactConstraintPool = new IntArrayList();
        protected readonly IntArrayList orderFrictionConstraintPool = new IntArrayList();
        protected readonly List<TypedConstraint.ConstraintInfo1> tmpConstraintSizesPool = new List<TypedConstraint.ConstraintInfo1>();
        protected int maxOverrideNumSolverIterations;
        protected int fixedBodyId;

        protected ulong Seed2;

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
            SolverBody solverBodyA = tmpSolverBodyPool[solverBodyIdA];
            SolverBody solverBodyB = tmpSolverBodyPool[solverBodyIdB];

            RigidBody body0 = tmpSolverBodyPool[solverBodyIdA].originalBody;
            RigidBody body1 = tmpSolverBodyPool[solverBodyIdB].originalBody;

            solverConstraint.solverBodyIdA = solverBodyIdA;
            solverConstraint.solverBodyIdB = solverBodyIdB;

            solverConstraint.friction = cp.combinedFriction;
            solverConstraint.originalContactPoint = null;

            solverConstraint.appliedImpulse = VFixedPoint.Zero;
            solverConstraint.appliedPushImpulse = VFixedPoint.Zero;

            if (body0 != null)
            {
                solverConstraint.contactNormal1 = normalAxis;
                VInt3 ftorqueAxis1 = VInt3.Cross(rel_pos1, solverConstraint.contactNormal1);
                solverConstraint.relpos1CrossNormal = ftorqueAxis1;
                solverConstraint.angularComponentA = body0.getInvInertiaTensorWorld()*ftorqueAxis1*body0.getAngularFactor();
            }
            else
            {
                solverConstraint.contactNormal1 = VInt3.zero;
                solverConstraint.relpos1CrossNormal = VInt3.zero;
                solverConstraint.angularComponentA = VInt3.zero;
            }

            if (body1 != null)
            {
                solverConstraint.contactNormal2 = -normalAxis;
                VInt3 ftorqueAxis1 = VInt3.Cross(rel_pos2, solverConstraint.contactNormal2);
                solverConstraint.relpos2CrossNormal = ftorqueAxis1;
                solverConstraint.angularComponentB = body1.getInvInertiaTensorWorld()*ftorqueAxis1*body1.getAngularFactor();
            }
            else
            {
                solverConstraint.contactNormal2 = VInt3.zero;
                solverConstraint.relpos2CrossNormal = VInt3.zero;
                solverConstraint.angularComponentB = VInt3.zero;
            }

            VInt3 vec;
            VFixedPoint denom0 = VFixedPoint.Zero;
            VFixedPoint denom1 = VFixedPoint.Zero;
            if (body0 != null)
            {
                vec = VInt3.Cross(solverConstraint.angularComponentA, rel_pos1);
                denom0 = body0.getInvMass() + VInt3.Dot(normalAxis, vec);
            }
            if (body1 != null)
            {
                vec = VInt3.Cross(-solverConstraint.angularComponentB, rel_pos2);
                denom1 = body1.getInvMass() + VInt3.Dot(normalAxis, vec);
            }
            VFixedPoint denom = relaxation/(denom0+denom1);
            solverConstraint.jacDiagABInv = denom;

            VFixedPoint vel1Dotn = VInt3.Dot(solverConstraint.contactNormal1, body0 != null?solverBodyA.linearVelocity+solverBodyA.externalForceImpulse:VInt3.zero)
                                + VInt3.Dot(solverConstraint.relpos1CrossNormal, body0 != null?solverBodyA.angularVelocity:VInt3.zero);
            VFixedPoint vel2Dotn = VInt3.Dot(solverConstraint.contactNormal2, body1 != null?solverBodyB.linearVelocity+solverBodyB.externalForceImpulse:VInt3.zero)
                                + VInt3.Dot(solverConstraint.relpos2CrossNormal, body1 != null?solverBodyB.angularVelocity:VInt3.zero);

            VFixedPoint rel_vel = vel1Dotn+vel2Dotn;

            VFixedPoint velocityError =  desiredVelocity - rel_vel;
            VFixedPoint	velocityImpulse = velocityError * solverConstraint.jacDiagABInv;
            solverConstraint.rhs = velocityImpulse;
            solverConstraint.cfm = cfmSlip;
            solverConstraint.lowerLimit = -solverConstraint.friction;
            solverConstraint.upperLimit = solverConstraint.friction;
        }

        protected SolverConstraint	addFrictionConstraint(VInt3 normalAxis,int solverBodyIdA,int solverBodyIdB,int frictionIndex, ManifoldPoint cp ,
            VInt3 rel_pos1,VInt3 rel_pos2,CollisionObject colObj0, CollisionObject colObj1, VFixedPoint relaxation, VFixedPoint desiredVelocity, VFixedPoint cfmSlip)
        {
            SolverConstraint solverConstraint = tmpSolverContactFrictionConstraintPool.expandNonInitializing();
            solverConstraint.frictionIndex = frictionIndex;
            setupFrictionConstraint(solverConstraint, normalAxis, solverBodyIdA, solverBodyIdB, cp, rel_pos1, rel_pos2,
                colObj0, colObj1, relaxation, desiredVelocity, cfmSlip);
            return solverConstraint;
        }

        protected void setupContactConstraint(SolverConstraint solverConstraint, int solverBodyIdA, int solverBodyIdB, ManifoldPoint cp,
                                ContactSolverInfo infoGlobal,VFixedPoint relaxation, VFixedPoint rel_pos1, VFixedPoint rel_pos2);

        protected static void applyAnisotropicFriction(CollisionObject colObj, VInt3 frictionDirection, int frictionMode)
        {
            if (colObj != null && colObj.hasAnisotropicFriction(frictionMode))
            {
                // transform to local coordinates
                VInt3 loc_lateral = frictionDirection * colObj->getWorldTransform().getBasis();
                VInt3 friction_scaling = colObj->getAnisotropicFriction();
                //apply anisotropic friction
                loc_lateral *= friction_scaling;
                // ... and transform it back to global coordinates
                frictionDirection = colObj.getWorldTransform() * loc_lateral;
            }

        }
        protected void setFrictionConstraintImpulse(SolverConstraint solverConstraint, int solverBodyIdA, int solverBodyIdB,
                                         ManifoldPoint cp, ContactSolverInfo infoGlobal);


        protected VFixedPoint restitutionCurve(VFixedPoint rel_vel, VFixedPoint restitution)
        {
            return restitution * -rel_vel;
        }

        protected virtual void convertContacts(PersistentManifold[] manifoldPtr, ContactSolverInfo infoGlobal)
        {
            PersistentManifold manifold = null;
            for (int i=0;i<manifoldPtr.Length;i++)
            {
                manifold = manifoldPtr[i];
                convertContact(manifold,infoGlobal);
            }
        }

        protected void convertContact(PersistentManifold manifold, ContactSolverInfo infoGlobal)
        {

        }

        protected void resolveSplitPenetrationImpulseCacheFriendly(SolverBody body1, SolverBody body2, SolverConstraint c)
        {
            if (c.rhsPenetration != VFixedPoint.Zero)
            {
                VFixedPoint deltaImpulse = c.rhsPenetration - c.appliedPushImpulse * c.cfm;
                VFixedPoint deltaVel1Dotn = VInt3.Dot(c.contactNormal1, body1.internalGetPushVelocity())
                                         	 	+ VInt3.Dot(c.relpos1CrossNormal, body1.internalGetTurnVelocity());
                VFixedPoint deltaVel2Dotn = VInt3.Dot(c.contactNormal2, body2.internalGetPushVelocity())
                                         	 	+ VInt3.Dot(c.relpos2CrossNormal, body2.internalGetTurnVelocity());

                deltaImpulse -=	deltaVel1Dotn*c.jacDiagABInv;
                deltaImpulse -=	deltaVel2Dotn*c.jacDiagABInv;
                VFixedPoint sum = c.appliedPushImpulse + deltaImpulse;
                if (sum < c.lowerLimit)
                {
                    deltaImpulse = c.lowerLimit-c.appliedPushImpulse;
                    c.appliedPushImpulse = c.lowerLimit;
                }
                else
                {
                    c.appliedPushImpulse = sum;
                }
                body1.internalApplyPushImpulse(c.contactNormal1*body1.internalGetInvMass(),c.angularComponentA,deltaImpulse);
                body2.internalApplyPushImpulse(c.contactNormal2*body2.internalGetInvMass(),c.angularComponentB,deltaImpulse);
            }
        }

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
            for (int iteration = 0;iteration<infoGlobal.numIterations;iteration++)
            {
                {
                    int numPoolConstraints = tmpSolverContactConstraintPool.Count;
                    for (int j=0;j<numPoolConstraints;j++)
                    {
                        SolverConstraint solveManifold = tmpSolverContactConstraintPool[orderTmpConstraintPool[j]];
                        resolveSplitPenetrationImpulseCacheFriendly(tmpSolverBodyPool[solveManifold.solverBodyIdA],tmpSolverBodyPool[solveManifold.solverBodyIdB],solveManifold);
                    }
                }
            }
        }

        protected virtual VFixedPoint solveGroupCacheFriendlyFinish(CollisionObject[] bodies,
            ContactSolverInfo infoGlobal)
        {
            int numPoolConstraints = tmpSolverContactConstraintPool.Count;

            if ((infoGlobal.solverMode & SolverMode.SOLVER_USE_WARMSTARTING) != 0)
            {
                for (int j=0;j<numPoolConstraints;j++)
                {
                    SolverConstraint solveManifold = tmpSolverContactConstraintPool[j];
                    TypedConstraint pt = solveManifold.originalContactPoint;
                    pt.appliedImpulse = solveManifold.appliedImpulse;
                    pt.appliedImpulseLateral1 = tmpSolverContactFrictionConstraintPool[solveManifold.frictionIndex].appliedImpulse;
                    if ((infoGlobal.solverMode & SolverMode.SOLVER_USE_2_FRICTION_DIRECTIONS) != 0)
                    {
                        pt.appliedImpulseLateral2 = tmpSolverContactFrictionConstraintPool[solveManifold.frictionIndex+1].appliedImpulse;
                    }
                    //do a callback here?
                }
            }

            numPoolConstraints = tmpSolverNonContactConstraintPool.Count;
            for (int j=0;j<numPoolConstraints;j++)
            {
                SolverConstraint solverConstr = tmpSolverNonContactConstraintPool[j];
                TypedConstraint constr = solverConstr.originalContactPoint;
                JointFeedback fb = constr.getJointFeedback();
                if (fb != null)
                {
                    fb.appliedForceBodyA += solverConstr.contactNormal1*solverConstr.appliedImpulse*constr.getRigidBodyA().getLinearFactor()/infoGlobal.timeStep;
                    fb.appliedForceBodyB += solverConstr.contactNormal2*solverConstr.appliedImpulse*constr.getRigidBodyB().getLinearFactor()/infoGlobal.timeStep;
                    fb.appliedTorqueBodyA += solverConstr.relpos1CrossNormal* constr.getRigidBodyA().getAngularFactor()*solverConstr.appliedImpulse/infoGlobal.timeStep;
                    fb.appliedTorqueBodyB += solverConstr.relpos2CrossNormal* constr.getRigidBodyB().getAngularFactor()*solverConstr.appliedImpulse/infoGlobal.timeStep; /*RGM ???? */

                }

                constr.appliedImpulse = solverConstr.appliedImpulse;
                if (btFabs(solverConstr.appliedImpulse)>=constr.getBreakingImpulseThreshold())
                {
                    constr.setEnabled(false);
                }
            }

            for (int i=0;i<tmpSolverBodyPool.Count;i++)
            {
                RigidBody body = tmpSolverBodyPool[i].originalBody;
                if (body != null)
                {
                    if (infoGlobal.splitImpulse)
                        tmpSolverBodyPool[i].writebackVelocityAndTransform(infoGlobal.timeStep, infoGlobal.splitImpulseTurnErp);
                    else
                        tmpSolverBodyPool[i].writebackVelocity();

                    tmpSolverBodyPool[i].originalBody.setLinearVelocity(
                        tmpSolverBodyPool[i].linearVelocity+
                        tmpSolverBodyPool[i].externalForceImpulse);

                    tmpSolverBodyPool[i].originalBody.setAngularVelocity(
                        tmpSolverBodyPool[i].angularVelocity+
                        tmpSolverBodyPool[i].externalTorqueImpulse);

                    if (infoGlobal.splitImpulse)
                        tmpSolverBodyPool[i].originalBody.setWorldTransform(tmpSolverBodyPool[i].worldTransform);

                    tmpSolverBodyPool[i].originalBody.setCompanionId(-1);
                }
            }

            tmpSolverContactConstraintPool.Clear();
            tmpSolverNonContactConstraintPool.Clear();
            tmpSolverContactFrictionConstraintPool.Clear();
            tmpSolverContactRollingFrictionConstraintPool.Clear();

            tmpSolverBodyPool.Clear();
            return VFixedPoint.Zero;
        }

        protected virtual VFixedPoint solveSingleIteration(int iteration, CollisionObject[] bodies,
            PersistentManifold[] manifoldPtr, TypedConstraint[] constraints, ContactSolverInfo infoGlobal)
        {
            ///solve all joint constraints
            for (int j=0;j<tmpSolverNonContactConstraintPool.Count;j++)
            {
                SolverConstraint constraint = tmpSolverNonContactConstraintPool[orderNonContactConstraintPool[j]];
                if (iteration < constraint.overrideNumSolverIterations)
                    resolveSingleConstraintRowGeneric(m_tmpSolverBodyPool[constraint.m_solverBodyIdA],m_tmpSolverBodyPool[constraint.m_solverBodyIdB],constraint);
            }

            if (iteration< infoGlobal.numIterations)
            {
                for (int j=0;j<constraints.Length;j++)
                {
                    if (constraints[j].isEnabled())
                    {
                        int bodyAid = getOrInitSolverBody(constraints[j].getRigidBodyA(),infoGlobal.timeStep);
                        int bodyBid = getOrInitSolverBody(constraints[j].getRigidBodyB(),infoGlobal.timeStep);
                        SolverBody bodyA = tmpSolverBodyPool[bodyAid];
                        SolverBody bodyB = tmpSolverBodyPool[bodyBid];
                        constraints[j].solveConstraintObsolete(bodyA,bodyB,infoGlobal.timeStep);
                    }
                }
                ///solve all contact constraints
                int numPoolConstraints = m_tmpSolverContactConstraintPool.size();
                for (int j=0;j<numPoolConstraints;j++)
                {
                    const btSolverConstraint& solveManifold = m_tmpSolverContactConstraintPool[m_orderTmpConstraintPool[j]];
                    resolveSingleConstraintRowLowerLimit(m_tmpSolverBodyPool[solveManifold.m_solverBodyIdA],m_tmpSolverBodyPool[solveManifold.m_solverBodyIdB],solveManifold);
                }
                ///solve all friction constraints
                int numFrictionPoolConstraints = m_tmpSolverContactFrictionConstraintPool.size();
                for (int j=0;j<numFrictionPoolConstraints;j++)
                {
                    btSolverConstraint& solveManifold = m_tmpSolverContactFrictionConstraintPool[m_orderFrictionConstraintPool[j]];
                    btScalar totalImpulse = m_tmpSolverContactConstraintPool[solveManifold.m_frictionIndex].m_appliedImpulse;

                    if (totalImpulse>btScalar(0))
                    {
                        solveManifold.m_lowerLimit = -(solveManifold.m_friction*totalImpulse);
                        solveManifold.m_upperLimit = solveManifold.m_friction*totalImpulse;

                        resolveSingleConstraintRowGeneric(m_tmpSolverBodyPool[solveManifold.m_solverBodyIdA],m_tmpSolverBodyPool[solveManifold.m_solverBodyIdB],solveManifold);
                    }
                }

                int numRollingFrictionPoolConstraints = m_tmpSolverContactRollingFrictionConstraintPool.size();
                for (int j=0;j<numRollingFrictionPoolConstraints;j++)
                {
                    btSolverConstraint& rollingFrictionConstraint = m_tmpSolverContactRollingFrictionConstraintPool[j];
                    btScalar totalImpulse = m_tmpSolverContactConstraintPool[rollingFrictionConstraint.m_frictionIndex].m_appliedImpulse;
                    if (totalImpulse>btScalar(0))
                    {
                        btScalar rollingFrictionMagnitude = rollingFrictionConstraint.m_friction*totalImpulse;
                        if (rollingFrictionMagnitude>rollingFrictionConstraint.m_friction)
                            rollingFrictionMagnitude = rollingFrictionConstraint.m_friction;

                        rollingFrictionConstraint.m_lowerLimit = -rollingFrictionMagnitude;
                        rollingFrictionConstraint.m_upperLimit = rollingFrictionMagnitude;

                        resolveSingleConstraintRowGeneric(m_tmpSolverBodyPool[rollingFrictionConstraint.m_solverBodyIdA],m_tmpSolverBodyPool[rollingFrictionConstraint.m_solverBodyIdB],rollingFrictionConstraint);
                    }
                }
            }
            return VFixedPoint.Zero;
        }

        protected virtual VFixedPoint solveGroupCacheFriendlySetup(CollisionObject[] bodies,
            PersistentManifold[] manifoldPtr, TypedConstraint[] constraints, ContactSolverInfo infoGlobal)
        {

        }

        protected virtual VFixedPoint solveGroupCacheFriendlyIterations(CollisionObject[] bodies,
            PersistentManifold[] manifoldPtr, TypedConstraint[] constraints,ContactSolverInfo infoGlobal)
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