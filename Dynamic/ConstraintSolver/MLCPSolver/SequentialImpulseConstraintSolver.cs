using System.Collections.Generic;
using MobaGame.Framework;
using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public delegate VFixedPoint SingleConstraintRowSolver(SolverBody body1, SolverBody body2, SolverConstraint solverConstraint);

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

        protected SingleConstraintRowSolver resolveSingleConstraintRowGeneric;
        protected SingleConstraintRowSolver resolveSingleConstraintRowLowerLimit;

        protected void setupFrictionConstraint(SolverConstraint solverConstraint, VInt3 normalAxis,int solverBodyIdA,int solverBodyIdB,
                                 ManifoldPoint cp,VInt3 rel_pos1, VInt3 rel_pos2,
									CollisionObject colObj0, CollisionObject colObj1, VFixedPoint relaxation,
                                     VFixedPoint desiredVelocity, VFixedPoint cfmSlip);

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
        protected void initSolverBody(SolverBody solverBody, CollisionObject collisionObject, VFixedPoint timeStep);


        protected virtual void solveGroupCacheFriendlySplitImpulseIterations(CollisionObject[] bodies, PersistentManifold[] manifoldPtr, TypedConstraint[] constraints, ContactSolverInfo infoGlobal);
        protected virtual VFixedPoint solveGroupCacheFriendlyFinish(CollisionObject[] bodies, ContactSolverInfo infoGlobal);
	    protected virtual VFixedPoint solveSingleIteration(int iteration, CollisionObject[] bodies, PersistentManifold[] manifoldPtr, TypedConstraint[] constraints, ContactSolverInfo infoGlobal);

        protected virtual VFixedPoint solveGroupCacheFriendlySetup(CollisionObject[] bodies, PersistentManifold[] manifoldPtr, TypedConstraint[] constraints,ContactSolverInfo infoGlobal);
        protected virtual VFixedPoint solveGroupCacheFriendlyIterations(CollisionObject[] bodies, PersistentManifold[] manifoldPtr, TypedConstraint[] constraints,ContactSolverInfo infoGlobal);



        public SequentialImpulseConstraintSolver()
        {
            
        }

        public override float solveGroup(List<CollisionObject> bodies, int numBodies, List<PersistentManifold> manifold, int manifold_offset, int numManifolds, List<TypedConstraint> constraints, int constraints_offset, int numConstraints, ContactSolverInfo info, Dispatcher dispatcher)
        {
            
        }

        public override void reset()
        {
            
        }

        public ulong Rand2()
        {

        }

        public int btRandInt2(int n)
        {

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

        public SingleConstraintRowSolver getActiveConstraintRowSolverGeneric()
        {
            return resolveSingleConstraintRowGeneric;
        }

        public void setConstraintRowSolverGeneric(SingleConstraintRowSolver rowSolver)
        {
            resolveSingleConstraintRowGeneric = rowSolver;
        }

        public  SingleConstraintRowSolver getActiveConstraintRowSolverLowerLimit()
        {
            return resolveSingleConstraintRowLowerLimit;
        }

        public void setConstraintRowSolverLowerLimit(SingleConstraintRowSolver rowSolver)
        {
            resolveSingleConstraintRowLowerLimit = rowSolver;
        }

    }
}