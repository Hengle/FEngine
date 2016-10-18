using MobaGame.FixedMath;
using System.Collections.Generic;
using System;

namespace MobaGame.Collision
{
    public abstract class AbstractBroadphaseDetector<E, T> : BroadphaseDetector<E, T> where E : Collidable<T> where T : Fixture 
    {
        protected BroadphaseFilter<E, T> defaultFilter = new DefaultBroadphaseFilter<E, T>();
        protected VFixedPoint expansion = VFixedPoint.Create(2) / VFixedPoint.Create(10);

        public void add(E collidable)
        {
            int size = collidable.getFixtureCount();
            for (int i = 0; i < size; i++)
            {
                T fixture = collidable.getFixture(i);
                add(collidable, fixture);
            }
        }

        public void remove(E collidable)
        {
            int size = collidable.getFixtureCount();
            if (size == 0)
            {
                return;
            }
            for (int i = 0; i < size; i++)
            {
                T fixture = collidable.getFixture(i);
                remove(collidable, fixture);
            }
        }

        public void update(E collidable)
        {
            int size = collidable.getFixtureCount();
            for (int i = 0; i < size; i++)
            {
                T fixture = collidable.getFixture(i);
                update(collidable, fixture);
            }
        }

        public bool detect(E a, E b)
        {
            AABB aAABB = getAABB(a);
            AABB bAABB = getAABB(b);
            if ((aAABB == null) || (bAABB == null))
            {
                return false;
            }
            if (aAABB.overlaps(bAABB))
            {
                return true;
            }
            return false;
        }

        public AABB getAABB(E collidable)
        {
            int size = collidable.getFixtureCount();
            if (size == 0)
            {
                return new AABB(VInt3.zero, VInt3.zero);
            }
            AABB union = getAABB(collidable, collidable.getFixture(0));
            for (int i = 1; i < size; i++)
            {
                AABB aabb = getAABB(collidable, collidable.getFixture(i));
                union.union(aabb);
            }
            return union;
        }

        public bool detect(Convex convex1, VIntTransform transform1, Convex convex2, VIntTransform transform2)
        {
            AABB a = convex1.createAABB(transform1);
            AABB b = convex2.createAABB(transform2);
            if (a.overlaps(b))
            {
                return true;
            }
            return false;
        }

        protected bool raycast(VInt3 start, VFixedPoint length, VFixedPoint invDx, VFixedPoint invDy, VFixedPoint invDz, AABB aabb)
        {
            VFixedPoint tx1 = (aabb.getMinX() - start.x) * invDx;
            VFixedPoint tx2 = (aabb.getMaxX() - start.x) * invDx;

            VFixedPoint tmin = FMath.Min(tx1, tx2);
            VFixedPoint tmax = FMath.Max(tx1, tx2);

            VFixedPoint ty1 = (aabb.getMinY() - start.y) * invDy;
            VFixedPoint ty2 = (aabb.getMaxY() - start.y) * invDy;

            tmin = FMath.Max(tmin, FMath.Min(ty1, ty2));
            tmax = FMath.Min(tmax, FMath.Max(ty1, ty2));

            VFixedPoint tz1 = (aabb.getMinZ() - start.z) * invDz;
            VFixedPoint tz2 = (aabb.getMaxZ() - start.z) * invDz;

            tmin = FMath.Max(tmin, FMath.Min(tz1, tz2));
            tmax = FMath.Min(tmax, FMath.Max(tz1, tz2));

            if (tmax < VFixedPoint.Zero)
            {
                return false;
            }
            if (tmin > length)
            {
                return false;
            }
            return tmax >= tmin;
        }

        public List<BroadphasePair<E, T>> detect()
        {
            return detect(this.defaultFilter);
        }

        public List<BroadphaseItem<E, T>> detect(AABB aabb)
        {
            return detect(aabb, this.defaultFilter);
        }

        public List<BroadphaseItem<E, T>> raycast(Ray ray, VFixedPoint length)
        {
            return raycast(ray, length, this.defaultFilter);
        }

        public VFixedPoint getAABBExpansion()
        {
            return this.expansion;
        }

        public void setAABBExpansion(VFixedPoint expansion)
        {
            this.expansion = expansion;
        }

        public abstract void add(E collidable, T fixture);

        public abstract bool remove(E paramE, T paramT);

        public abstract void update(E paramE, T paramT);

        public abstract AABB getAABB(E paramE, T paramT);

        public abstract bool contains(E paramE);

        public abstract bool contains(E paramE, T paramT);

        public abstract void clear();

        public abstract int size();

        public abstract List<BroadphasePair<E, T>> detect(BroadphaseFilter<E, T> paramBroadphaseFilter);

        public abstract List<BroadphaseItem<E, T>> detect(AABB paramAABB, BroadphaseFilter<E, T> paramBroadphaseFilter);

        public abstract List<BroadphaseItem<E, T>> raycast(Ray paramRay, VFixedPoint paramDouble, BroadphaseFilter<E, T> paramBroadphaseFilter);

        public abstract void shift(VInt3 paramVector2);
    }
}