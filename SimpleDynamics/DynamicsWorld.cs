using MobaGame.FixedMath;
using System.Collections.Generic;

namespace MobaGame.Collision
{
    public class DynamicsWorld: CollisionWorld
    {
        public List<ActionInterface> actions;

        int iteration;
        public DynamicsWorld(Dispatcher dispatcher, BroadphaseInterface broadphase):base(dispatcher, broadphase)
        {
            iteration = 4;
        }

        public override void Tick(VFixedPoint dt)
        {
            updateAabbs();
            ResolveContactConstraint(dt);
            //process actions
            for (int i = 0; i < actions.Count; i++)
            {
                actions[i].updateAction(this, dt);
            }
        }

        private void ResolveContactConstraint(VFixedPoint dt)
        {
            //performDiscreteCollisionDetection();
            //resolve contact contraints
            
            for(int i = 0; i < iteration; i++)
            {

            }
        }
    }
}
