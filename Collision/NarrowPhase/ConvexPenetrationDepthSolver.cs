using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public abstract class ConvexPenetrationDepthSolver
    {
        public abstract bool calcPenDepth(SimplexSolverInterface simplexSolver,
            ConvexShape convexA, ConvexShape convexB,
            VIntTransform transA, VIntTransform transB,
            ref VInt3 pa, ref VInt3 pb);
    }
}
