using MobaGame.FixedMath;
using System.Collections.Generic;

namespace MobaGame.Collision
{
    public interface BroadphaseFilter<E , T> where E: Collidable<T> where T : Fixture
    {
        bool isAllowed(E paramE1, T paramT1, E paramE2, T paramT2);

        bool isAllowed(AABB paramAABB, E paramE, T paramT);

        bool isAllowed(Ray paramRay, VFixedPoint paramDouble, E paramE, T paramT);
    }
}