using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public class SolverConstraint
    {
        public VInt3 relpos1CrossNormal;
        public VInt3 contactNormal1;

        public VInt3 relpos2CrossNormal;
        public VInt3 contactNormal2;

        public VInt3 angularComponentA;
        public VInt3 angularComponentB;

        public VFixedPoint appliedPushImpulse;
        public VFixedPoint appliedImpulse;

        public int solverBodyIdA;
        public int solverBodyIdB;

        public VFixedPoint friction;
        public VFixedPoint restitution;
        public VFixedPoint jacDiagABInv;
        public VFixedPoint penetration;
        public VFixedPoint rhs;
        public VFixedPoint cfm;
        public VFixedPoint lowerLimit;
        public VFixedPoint upperLimit;
        public VFixedPoint rhsPenetration;

        public SolverConstraintType constraintType;
        public int frictionIndex;
        public ManifoldPoint originalContactPoint;
    }
}