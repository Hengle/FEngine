using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public abstract class SimplexSolverInterface
    {
        public abstract void reset();

        public abstract void addVertex(VInt3 w, VInt3 p, VInt3 q);

        public abstract VFixedPoint maxVertex();

        public abstract bool fullSimplex();

        public abstract int getSimplex(VInt3[] pBuf, VInt3[] qBuf, VInt3[] yBuf);

        public abstract bool emptySimplex();

		public abstract bool compute_points(out VInt3 p1, out VInt3 p2);

        public abstract int numVertices();
    }
}
