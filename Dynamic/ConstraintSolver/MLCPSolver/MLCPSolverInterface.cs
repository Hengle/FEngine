namespace MobaGame.Collision
{
    public interface MLCPSolverInterface
    {
        bool solveMLCP(MatrixXu A, VectorXu b, ref VectorXu x, VectorXu lo, VectorXu hi, btAlignedObjectArray<int> limitDependency, int numIterations, bool useSparsity = true)
    }
}