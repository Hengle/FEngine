using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public class SolverConstraint
    {
        public VInt3 relpos1CrossNormal;
        public VInt3 contactNormal;

        public VInt3 relpos2CrossNormal;
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

        public SolverConstraintType constraintType;
        public int frictionIndex;
        public ManifoldPoint originalContactPoint;
    }
}