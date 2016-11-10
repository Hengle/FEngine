using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public abstract class ConvexPenetrationDepthSolver
    {
        public abstract bool calcPenDepth(SimplexSolverInterface simplexSolver,
            ConvexShape convexA, ConvexShape convexB,
            VIntTransform transA, VIntTransform transB,
            VInt3 v, VInt3 pa, VInt3 pb);
    }
}
