using MobaGame.FixedMath;
using System.Collections.Generic;

namespace MobaGame.Collision
{

    public interface Collidable<T>: Transformable, Shiftable where T : Fixture
    {
        UUID getId();

        AABB createAABB();

        AABB createAABB(VIntTransform paramTransform);

        Collidable<T> addFixture(T paramT);

        T addFixture(Convex paramConvex);

        T getFixture(int paramInt);

        bool containsFixture(T paramT);

        T getFixture(VInt3 paramVector2);

        List<T> getFixtures(VInt3 paramVector2);

        bool removeFixture(T paramT);

        T removeFixture(int paramInt);

        List<T> removeAllFixtures();

        T removeFixture(VInt3 paramVector2);

        List<T> removeFixtures(VInt3 paramVector2);

        int getFixtureCount();

        List<T> getFixtures();

        bool contains(VInt3 paramVector2);

        VInt3 getLocalCenter();

        VInt3 getWorldCenter();

        VInt3 getLocalPoint(VInt3 paramVector2);

        VInt3 getWorldPoint(VInt3 paramVector2);

        VInt3 getLocalVector(VInt3 paramVector2);

        VInt3 getWorldVector(VInt3 paramVector2);

        VFixedPoint getRotationDiscRadius();

        VIntTransform getTransform();

        void setTransform(VIntTransform paramTransform);

        void rotateAboutCenter(VFixedPoint paramDouble);

        void translateToOrigin();
    }
}
