using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    class GjkEpaPenetrationDepthSolver: ConvexPenetrationDepthSolver
    {
        public override bool calcPenDepth(SimplexSolverInterface simplexSolver,
                                                  ConvexShape pConvexA, ConvexShape pConvexB,
                                                  VIntTransform transformA, VIntTransform transformB,
                                                  ref VInt3 wWitnessOnA, ref VInt3 wWitnessOnB)
        {
            VInt3[] P = null;
            VInt3[] Q = null;
            VInt3[] W = null;
            simplexSolver.getSimplex(P, Q, W);

            VFixedPoint radialmargin = VFixedPoint.Zero;

            Results results = new GjkEpaSolver.Results();
            if (gjkEpaSolver.collide(pConvexA, transformA,
                    pConvexB, transformB,
                    radialmargin, results))
            {
                wWitnessOnA = results.witnesses[0];
                wWitnessOnB = results.witnesses[1];
                return true;
            }

            return false;
        }

        public enum ResultsStatus
        {
            Separated,      /* Shapes doesnt penetrate												*/
            Penetrating,    /* Shapes are penetrating												*/
            GJK_Failed,     /* GJK phase fail, no big issue, shapes are probably just 'touching'	*/
            EPA_Failed,     /* EPA phase fail, bigger problem, need to save parameters, and debug	*/
        }

        public class Results
        {
            public ResultsStatus status;
            public VInt3[] witnesses/*[2]*/ = new VInt3[] { VInt3.zero, VInt3.zero };
            public VInt3 normal = new VInt3();
            public VFixedPoint depth;
            public int epa_iterations;
            public int gjk_iterations;
        }
    }
}
