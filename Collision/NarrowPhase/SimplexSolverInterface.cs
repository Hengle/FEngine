using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public abstract class SimplexSolverInterface
    {
        public abstract void reset();

        public abstract void addVertex(VInt3 w, VInt3 p, VInt3 q);

        public abstract bool closest(ref VInt3 v);

        public abstract VFixedPoint maxVertex();

        public abstract bool fullSimplex();

        public abstract int getSimplex(VInt3[] pBuf, VInt3[] qBuf, VInt3[] yBuf);

        public abstract bool inSimplex(VInt3 w);

        public abstract void backup_closest(VInt3 v);

        public abstract bool emptySimplex();

        public abstract void compute_points(ref VInt3 p1, ref VInt3 p2);

        public abstract int numVertices();
    }
}
