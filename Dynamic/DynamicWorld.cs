using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public abstract class DynamicWorld: CollisionWorld
    {
        protected object worldUserInfo;

        protected ContactSolverInfo solverInfo = new ContactSolverInfo();

        public DynamicWorld(Dispatcher dispatcher, BroadphaseInterface broadphasePairCache)
            :base(dispatcher, broadphasePairCache)
        {

        }

        public abstract int stepSimulation(VFixedPoint timeStep, int maxSubSteps, VFixedPoint fixedTimeStep);

        public abstract void addConstraint(TypedConstraint constraint, bool disableCollisionsBetweenLinkedBodies);

        public abstract void removeConstraint(TypedConstraint constraint);

        public abstract void setGravity(VInt3 gravity);

        public abstract VInt3 getGravity();

        public abstract void addRigidBody(RigidBody body);

        public abstract void removeRigidBody(RigidBody body);

        public abstract void setConstraintSolver(ConstraintSolver solver);

        public abstract ConstraintSolver getConstraintSolver();

        public int getNumConstraints()
        {
            return 0;
        }

        public TypedConstraint getConstraint(int index)
        {
            return null;
        }

        public abstract DynamicsWorldType getWorldType();

        public abstract void clearForces();

        public void setWorldUserInfo(object worldUserInfo)
        {
            this.worldUserInfo = worldUserInfo;
        }

        public object getWorldUserInfo()
        {
            return worldUserInfo;
        }

        public ContactSolverInfo getSolverInfo()
        {
            return solverInfo;
        }

    }
}