using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public interface Shape
    {
        ulong getId();
        VInt3 getCenter();
        VFixedPoint getRadius();
        VFixedPoint getRadius(VInt3 paramVector2);
        void rotateAboutCenter(VFixedPoint paramDouble);
        Interval project(VInt3 paramVector2);
        Interval project(VInt3 paramVector2, VIntTransform paramTransform);
        bool contains(VInt3 paramVector2);
        bool contains(VInt3 paramVector2, VIntTransform paramTransform);
        Mass createMass(VFixedPoint paramDouble);
        AABB createAABB();
        AABB createAABB(VIntTransform paramTransform);
    }
}
