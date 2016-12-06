using System.Collections.Generic;
using MobaGame.Framework;

namespace MobaGame.Collision
{
    public class SequentialImpulseConstraintSolver: ConstraintSolver
    {
        private static readonly int MAX_CONTACT_SOLVER_TYPES = (int)ContactConstraintEnum.MAX_CONTACT_SOLVER_TYPES;

        private static readonly int SEQUENTIAL_IMPULSE_MAX_SOLVER_POINTS = 16384;
        private OrderIndex[] gOrder = new OrderIndex[SEQUENTIAL_IMPULSE_MAX_SOLVER_POINTS];

        private int totalCpd = 0;

        {
            for (int i=0; i<gOrder.length; i++) {
                gOrder[i] = new OrderIndex();
            }
        }

        ////////////////////////////////////////////////////////////////////////////

        private readonly ObjectPool<SolverBody> bodiesPool = new ObjectPool<SolverBody>();
        private readonly ObjectPool<SolverConstraint> constraintsPool = new ObjectPool<SolverConstraint>();
        private readonly ObjectPool<JacobianEntry> jacobiansPool = new ObjectPool<JacobianEntry>();

        private readonly List<SolverBody> tmpSolverBodyPool = new List<SolverBody>();
        private readonly List<SolverConstraint> tmpSolverConstraintPool = new List<SolverConstraint>();
        private readonly List<SolverConstraint> tmpSolverFrictionConstraintPool = new List<SolverConstraint>();
        private readonly IntArrayList orderTmpConstraintPool = new IntArrayList();
        private readonly IntArrayList orderFrictionConstraintPool = new IntArrayList();

        protected readonly ContactSolverFunc[,] contactDispatch = new ContactSolverFunc[MAX_CONTACT_SOLVER_TYPES, MAX_CONTACT_SOLVER_TYPES];
        protected readonly ContactSolverFunc[,] frictionDispatch = new ContactSolverFunc[MAX_CONTACT_SOLVER_TYPES, MAX_CONTACT_SOLVER_TYPES];

        // btSeed2 is used for re-arranging the constraint rows. improves convergence/quality of friction
        protected long btSeed2 = 0L;
    }
}