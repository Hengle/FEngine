using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public class ConstraintPersistentData
    {
        public VFixedPoint appliedImpulse;
        public VFixedPoint prevAppliedImpulse;
        public VFixedPoint accumulatedTangentImpulse0;
        public VFixedPoint accumulatedTangentImpulse1;

        public VFixedPoint jacDiagABInv;
        public VFixedPoint jacDiagABInvTangent0;
        public VFixedPoint jacDiagABInvTangent1;
        public int persistentLifeTime;
        public VFixedPoint restitution;
        public VFixedPoint friction;
        public VFixedPoint penetration;
        public VInt3 frictionWorldTangential0;
        public VInt3 frictionWorldTangential1;

        public VInt3 frictionAngularComponent0A;
        public VInt3 frictionAngularComponent0B;
        public VInt3 frictionAngularComponent1A;
        public VInt3 frictionAngularComponent1B;

        //some data doesn't need to be persistent over frames: todo: clean/reuse this
        public VInt3 angularComponentA;
        public VInt3 angularComponentB;

        public ContactSolverFunc contactSolverFunc = null;
        public ContactSolverFunc frictionSolverFunc = null;

        public void reset()
        {
            appliedImpulse = VFixedPoint.Zero;
            prevAppliedImpulse = VFixedPoint.Zero;
            accumulatedTangentImpulse0 = VFixedPoint.Zero;
            accumulatedTangentImpulse1 = VFixedPoint.Zero;

            jacDiagABInv = VFixedPoint.Zero;
            jacDiagABInvTangent0 = VFixedPoint.Zero;
            jacDiagABInvTangent1 = VFixedPoint.Zero;
            persistentLifeTime = 0;
            restitution = VFixedPoint.Zero;
            friction = VFixedPoint.Zero;
            penetration = VFixedPoint.Zero;
            frictionWorldTangential0 = VInt3.zero;
            frictionWorldTangential1 = VInt3.zero;

            frictionAngularComponent0A = VInt3.zero;
            frictionAngularComponent0B = VInt3.zero;
            frictionAngularComponent1A = VInt3.zero;
            frictionAngularComponent1B = VInt3.zero;

            angularComponentA = VInt3.zero;
            angularComponentB = VInt3.zero;

            contactSolverFunc = null;
            frictionSolverFunc = null;
        }
    }
}
