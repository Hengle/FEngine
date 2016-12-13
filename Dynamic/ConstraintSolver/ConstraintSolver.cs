﻿using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public enum ConstraintSolverType
    {
        BT_SEQUENTIAL_IMPULSE_SOLVER = 1,
        BT_MLCP_SOLVER = 2,
        BT_NNCG_SOLVER = 4
    };

    public abstract class ConstraintSolver {

        //protected final BulletStack stack = BulletStack.get();

        public void prepareSolve (int numBodies, int numManifolds) {}

        /**
         * Solve a group of constraints.
         */
        public abstract VFixedPoint solveGroup(CollisionObject[] bodies, PersistentManifold[] manifold, TypedConstraint[] constraints, ContactSolverInfo info, Dispatcher dispatcher);

        public void allSolved(ContactSolverInfo info) {}

        /**
         * Clear internal cached data and reset random seed.
         */
        public abstract void reset();

    }

}