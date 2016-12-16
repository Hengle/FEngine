using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    class GjkEpaPenetrationDepthSolver: ConvexPenetrationDepthSolver
    {
        public override bool calcPenDepth(SimplexSolverInterface simplexSolver,
                                                  ConvexShape pConvexA, ConvexShape pConvexB,
                                                  VIntTransform transformA, VIntTransform transformB,
                                                  ref VInt3 wWitnessOnA, ref VInt3 wWitnessOnB, ref VInt3 normal, ref VFixedPoint depth)
        {
            VInt3[] Q = new VInt3[4];
            VInt3[] A = new VInt3[4];
            VInt3[] B = new VInt3[4];
            simplexSolver.getSimplex(A, B, Q);

            int size = simplexSolver.numVertices();
            GjkEpaSolver Epa = new GjkEpaSolver();
            normal = new VInt3();
            depth = VFixedPoint.Zero;
            return Epa.PenetrationDepth(pConvexA, pConvexB, transformA, transformB, Q, A, B, size, ref wWitnessOnA, ref wWitnessOnB, ref normal, ref depth) == 6;

        }
    }
}
