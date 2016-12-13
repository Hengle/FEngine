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

        public SequentialImpulseConstraintSolver()
        {
            for (int i = 0; i < gOrder.Length; i++)
            {
                gOrder[i] = new OrderIndex();
            }

            for (int i = 0; i < MAX_CONTACT_SOLVER_TYPES; i++)
            {
                for (int j = 0; j < MAX_CONTACT_SOLVER_TYPES; j++)
                {
                    contactDispatch[i, j] = ContactConstraint.resolveSingleCollision;
                    frictionDispatch[i, j] = ContactConstraint.resolveSingleFriction;
                }
            }
        }

        public override float solveGroup(List<CollisionObject> bodies, int numBodies, List<PersistentManifold> manifold, int manifold_offset, int numManifolds, List<TypedConstraint> constraints, int constraints_offset, int numConstraints, ContactSolverInfo info, Dispatcher dispatcher)
        {
            throw new NotImplementedException();
        }

        public override void reset()
        {
            throw new NotImplementedException();
        }

        private class OrderIndex
        {
            public int manifoldIndex;
            public int pointIndex;
        }
    }
}