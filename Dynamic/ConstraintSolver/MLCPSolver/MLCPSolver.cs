namespace MobaGame.Collision
{
    public abstract class MLCPSolver : ConstraintSolver
    {
        protected btMatrixXu m_A;
        protected btVectorXu m_b;
        protected btVectorXu m_x;
        protected btVectorXu m_lo;
        protected btVectorXu m_hi;

        ///when using 'split impulse' we solve two separate (M)LCPs
        protected btVectorXu m_bSplit;
        protected btVectorXu m_xSplit;
        protected btVectorXu m_bSplit1;
        protected btVectorXu m_xSplit2;

        protected btAlignedObjectArray<int> m_limitDependencies;
        protected btConstraintArray m_allConstraintArray;
        protected MLCPSolverInterface m_solver;
        protected int m_fallback;

        protected virtual btScalar solveGroupCacheFriendlySetup(btCollisionObject** bodies, int numBodies, btPersistentManifold** manifoldPtr, int numManifolds,btTypedConstraint** constraints,int numConstraints,const btContactSolverInfo& infoGlobal,btIDebugDraw* debugDrawer);
        protected virtual btScalar solveGroupCacheFriendlyIterations(btCollisionObject** bodies ,int numBodies,btPersistentManifold** manifoldPtr, int numManifolds,btTypedConstraint** constraints,int numConstraints,const btContactSolverInfo& infoGlobal,btIDebugDraw* debugDrawer);
        protected virtual void createMLCP(const btContactSolverInfo& infoGlobal);
        protected virtual void createMLCPFast(const btContactSolverInfo& infoGlobal);

        //return true is it solves the problem successfully
        protected virtual bool solveMLCP(const btContactSolverInfo& infoGlobal);

        public MLCPSolver(MLCPSolverInterface solver)
        {
            setMLCPSolver(solver);
        }

        public void setMLCPSolver(MLCPSolverInterface solver)
        {
            m_solver = solver;
        }

        public int getNumFallbacks()
        {
            return m_fallback;
        }
        public void setNumFallbacks(int num)
        {
            m_fallback = num;
        }

        public virtual ConstraintSolverType	getSolverType()
        {
            return BT_MLCP_SOLVER;
        }

    };
}