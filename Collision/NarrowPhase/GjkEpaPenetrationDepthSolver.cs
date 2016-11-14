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
            VInt3[] Q = new VInt3[4];
            VInt3[] A = new VInt3[4];
            VInt3[] B = new VInt3[4];
            simplexSolver.getSimplex(A, B, Q);

            int size = simplexSolver.numVertices();
            GjkEpaSolver Epa = new GjkEpaSolver();
            VInt3 normal = new VInt3();
            VFixedPoint dist = VFixedPoint.Zero;
            return Epa.PenetrationDepth(pConvexA, pConvexB, transformA, transformB, Q, A, B, size, ref wWitnessOnA, ref wWitnessOnB, ref normal, ref dist) == 1;

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
