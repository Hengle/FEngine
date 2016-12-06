using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public class ContactSolverInfo
    {
        public VFixedPoint tau = VFixedPoint.Create(0.6f);
        public VFixedPoint damping = VFixedPoint.One;
        public VFixedPoint friction = VFixedPoint.Create(0.3f);
        public VFixedPoint timeStep;
        public VFixedPoint restitution = VFixedPoint.Zero;
        public int numIterations = 10;
        public VFixedPoint maxErrorReduction = VFixedPoint.Create(20);
        public VFixedPoint sor = VFixedPoint.Create(1.3f);
        public VFixedPoint erp = VFixedPoint.Create(0.2f); // used as Baumgarte factor
        public VFixedPoint erp2 = VFixedPoint.Create(0.1f); // used in Split Impulse
        public bool splitImpulse = false;
        public VFixedPoint splitImpulsePenetrationThreshold = VFixedPoint.Create(-0.02f);
        public VFixedPoint linearSlop = VFixedPoint.Zero;
        public VFixedPoint warmstartingFactor = VFixedPoint.Create(0.85f);

        public int solverMode = SolverMode.SOLVER_RANDMIZE_ORDER | SolverMode.SOLVER_CACHE_FRIENDLY | SolverMode.SOLVER_USE_WARMSTARTING;

        public ContactSolverInfo() {
        }

        public ContactSolverInfo(ContactSolverInfo g) {
            tau = g.tau;
            damping = g.damping;
            friction = g.friction;
            timeStep = g.timeStep;
            restitution = g.restitution;
            numIterations = g.numIterations;
            maxErrorReduction = g.maxErrorReduction;
            sor = g.sor;
            erp = g.erp;
        }
    }
}