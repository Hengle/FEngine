using MobaGame.FixedMath;
using System.Collections.Generic;

namespace MobaGame.Collision
{
    public class DynamicsWorld: CollisionWorld
    {
        public DynamicsWorld(Dispatcher dispatcher, BroadphaseInterface broadphasePairCache):base(dispatcher, broadphasePairCache)
        {

        }

        public override void Tick()
        {
            performDiscreteCollisionDetection();
            List<ManifoldResult> manifolds = dispatcher1.getAllManifolds();
            for(int i = 0; i < manifolds.Count; i++)
            {
                ManifoldResult aresult = manifolds[i];
            }
        }
    }
}
