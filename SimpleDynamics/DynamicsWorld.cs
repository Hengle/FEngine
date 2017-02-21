using MobaGame.FixedMath;
using System.Collections.Generic;

namespace MobaGame.Collision
{
    public class DynamicsWorld: CollisionWorld
    {
        int iteration = 4;
        public DynamicsWorld(Dispatcher dispatcher, BroadphaseInterface broadphasePairCache):base(dispatcher, broadphasePairCache)
        {

        }

        public override void Tick(VFixedPoint dt)
        {
            performDiscreteCollisionDetection();
            List<ManifoldResult> manifolds = dispatcher1.getAllManifolds();

            for (int i = 0; i < collisionObjects.Count; i++)
            {
                if(!collisionObjects[i].isStaticOrKinematicObject())
                {
                    collisionObjects[i].LinearVel += Globals.g * dt;
                }
            }

            for (int i = 0; i < manifolds.Count; i++)
            {
                ManifoldResult aresult = manifolds[i];
                aresult.PreStep(dt);
            }
           
            for(int iter = 0; iter < iteration; iter++)
            {
                for (int i = 0; i < manifolds.Count; i++)
                {
                    ManifoldResult aresult = manifolds[i];
                    aresult.ApplyImpulse(dt);
                }
            }

            for(int i = 0; i < collisionObjects.Count; i++)
            {
                collisionObjects[i].IntegrateVelocity(dt);
            }
        }
    }
}
