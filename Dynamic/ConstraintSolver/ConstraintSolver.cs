using System.Collections.Generic;

namespace MobaGame.Collision
{
    public abstract class ConstraintSolver {

        //protected final BulletStack stack = BulletStack.get();

        public void prepareSolve (int numBodies, int numManifolds) {}

        /**
         * Solve a group of constraints.
         */
        public abstract float solveGroup(List<CollisionObject> bodies, int numBodies, List<PersistentManifold> manifold, int manifold_offset, int numManifolds, List<TypedConstraint> constraints, int constraints_offset, int numConstraints, ContactSolverInfo info, Dispatcher dispatcher);

        public void allSolved(ContactSolverInfo info) {}

        /**
         * Clear internal cached data and reset random seed.
         */
        public abstract void reset();

    }

}