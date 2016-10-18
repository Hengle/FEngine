using MobaGame.FixedMath;
using System.Collections.Generic;

namespace MobaGame.Collision
{
    public class BroadphaseFilterAdapter<E, T> : BroadphaseFilter<E, T> where E :Collidable<T> where T: Fixture
    {
        public virtual bool isAllowed(E collidable1, T fixture1, E collidable2, T fixture2)
        {
            return true;
        }

        public virtual bool isAllowed(AABB aabb, E collidable, T fixture)
        {
            return true;
        }

        public virtual bool isAllowed(Ray ray, VFixedPoint length, E collidable, T fixture)
        {
            return true;
        }
    }
}