using MobaGame.FixedMath;
using System.Collections.Generic;

namespace MobaGame.Collision
{
    public abstract class TypedConstraint
    {
        private static RigidBody s_fixed;// = new RigidBody(0, null, null);

        private static RigidBody getFixed()
        {
            if (s_fixed == null)
            {
                s_fixed = new RigidBody(VFixedPoint.Zero, null, VInt3.zero);
            }
            return s_fixed;
        }

        private int userConstraintType = -1;
        private int userConstraintId = -1;

        private TypedConstraintType constraintType;

        protected RigidBody rbA;
        protected RigidBody rbB;
        protected VFixedPoint appliedImpulse;

        public TypedConstraint(TypedConstraintType type, RigidBody rbA): this(type, rbA, getFixed())
        {
            
        }

        public TypedConstraint(TypedConstraintType type, RigidBody rbA, RigidBody rbB)
        {
            appliedImpulse = VFixedPoint.Zero;
            constraintType = type;
            this.rbA = rbA;
            this.rbB = rbB;
            getFixed().setMassProps(VFixedPoint.Zero, VInt3.zero);
        }

        public virtual void buildJacobian()
        {

        }

        public virtual void setupSolverConstraint(List<SolverConstraint> ca, int solverBodyA, int solverBodyB, VFixedPoint timeStep)
        {

        }

        ///internal method used by the constraint solver, don't use them directly
        public abstract ConstraintInfo1 getInfo1();

	    ///internal method used by the constraint solver, don't use them directly
	    public abstract ConstraintInfo2 getInfo2();

        public virtual void solveConstraintObsolete(VFixedPoint timeStep)
        {

        }

        public RigidBody getRigidBodyA()
        {
            return rbA;
        }

        public RigidBody getRigidBodyB()
        {
            return rbB;
        }

        public int getUserConstraintType()
        {
            return userConstraintType;
        }

        public void setUserConstraintType(int userConstraintType)
        {
            this.userConstraintType = userConstraintType;
        }

        public int getUserConstraintId()
        {
            return userConstraintId;
        }

        public int getUid()
        {
            return userConstraintId;
        }

        public void setUserConstraintId(int userConstraintId)
        {
            this.userConstraintId = userConstraintId;
        }

        public VFixedPoint getAppliedImpulse()
        {
            return appliedImpulse;
        }

        public TypedConstraintType getConstraintType()
        {
            return constraintType;
        }

        public class ConstraintInfo1
        {
            public int numConstraintRows;
            public int nub;
        }

        public class ConstraintInfo2
        {
            // integrator parameters: frames per second (1/stepsize), default error
            // reduction parameter (0..1).
            public VFixedPoint fps;
            public VFixedPoint erp;

            // elements to jump from one row to the next in J's
            int rowskip;

            // for the first and second body, pointers to two (linear and angular)
            // n*3 jacobian sub matrices, stored by rows. these matrices will have
            // been initialized to 0 on entry. if the second body is zero then the
            // J2xx pointers may be 0.
            //public VFixedPoint* m_J1linearAxis;
            //public VFixedPoint* m_J1angularAxis;
            //public VFixedPoint* m_J2linearAxis;
            //public VFixedPoint* m_J2angularAxis;

            // right hand sides of the equation J*v = c + cfm * lambda. cfm is the
            // "constraint force mixing" vector. c is set to zero on entry, cfm is
            // set to a constant value (typically very small or zero) value on entry.
            //public VFixedPoint* constraintError;
            //public VFixedPoint* cfm;

		    // lo and hi limits for variables (set to -/+ infinity on entry).
		    //public VFixedPoint* lowerLimit;
            //public VFixedPoint* upperLimit;

		    // number of solver iterations
		    public int numIterations;

            //damping of the velocity
            public VFixedPoint damping;
        }
    }
}