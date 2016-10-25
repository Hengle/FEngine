using MobaGame.FixedMath;
using System.Collections.Generic;

namespace MobaGame.Collision
{
    public interface BroadphaseDetector<E, T>: Shiftable where E : Collidable<T> where T : Fixture
    {
        void add(E paramE);

        void add(E paramE, T paramT);

        void remove(E paramE);

        bool remove(E paramE, T paramT);

        void update(E paramE);

        void update(E paramE, T paramT);

        AABB getAABB(E paramE);

        AABB getAABB(E paramE, T paramT);

        bool contains(E paramE);

        bool contains(E paramE, T paramT);

        void clear();

        int size();

        List<BroadphasePair<E, T>> detect();

        List<BroadphasePair<E, T>> detect(BroadphaseFilter<E, T> paramBroadphaseFilter);

        List<BroadphaseItem<E, T>> detect(AABB paramAABB);

        List<BroadphaseItem<E, T>> detect(AABB paramAABB, BroadphaseFilter<E, T> paramBroadphaseFilter);

        List<BroadphaseItem<E, T>> raycast(Ray paramRay, VFixedPoint paramDouble);

        List<BroadphaseItem<E, T>> raycast(Ray paramRay, VFixedPoint paramDouble, BroadphaseFilter<E, T> paramBroadphaseFilter);

        bool detect(E paramE1, E paramE2);

        bool detect(Convex paramConvex1, VIntTransform paramTransform1, Convex paramConvex2, VIntTransform paramTransform2);

        VFixedPoint getAABBExpansion();

        void setAABBExpansion(VFixedPoint paramDouble);
    }
}