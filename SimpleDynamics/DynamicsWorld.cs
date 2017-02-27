using MobaGame.FixedMath;
using System.Collections.Generic;

namespace MobaGame.Collision
{
    public class DynamicsWorld: CollisionWorld
    {
        public List<ActionInterface> actions;

        int iteration = 4;
        public DynamicsWorld(Dispatcher dispatcher, BroadphaseInterface broadphase):base(dispatcher, broadphase)
        {

        }

        public override void Tick(VFixedPoint dt)
        {
            //ResolveContactConstraint(dt);

            //process actions
            for (int i = 0; i < actions.Count; i++)
            {
                actions[i].updateAction(this, dt);
            }
        }

        private void ResolveContactConstraint(VFixedPoint dt)
        {
            updateAabbs();
            //performDiscreteCollisionDetection();

            //resolve contact contraints
            List<ManifoldResult> manifolds = dispatcher1.getAllManifolds();

            for (int i = 0; i < collisionObjects.Count; i++)
            {
                if (!collisionObjects[i].isStaticOrKinematicObject())
                {
                    collisionObjects[i].LinearVel += Globals.g * dt;
                }
            }

            for (int i = 0; i < manifolds.Count; i++)
            {
                ManifoldResult aresult = manifolds[i];
                aresult.PreStep(dt);
            }

            for (int iter = 0; iter < iteration; iter++)
            {
                for (int i = 0; i < manifolds.Count; i++)
                {
                    ManifoldResult aresult = manifolds[i];
                    aresult.ApplyImpulse(dt);
                }
            }

            for (int i = 0; i < collisionObjects.Count; i++)
            {
                collisionObjects[i].IntegrateVelocity(dt);
            }
        }
    }
}
