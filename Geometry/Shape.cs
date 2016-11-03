using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public interface Shape
    {
        UUID getId();
        VInt3 getCenter();
        VFixedPoint getRadius();
        Interval project(VInt3 paramVector2);
        Interval project(VInt3 paramVector2, VIntTransform paramTransform);
        bool contains(VInt3 paramVector2);
        bool contains(VInt3 paramVector2, VIntTransform paramTransform);
        Mass createMass(VFixedPoint paramDouble);
        AABB createAABB();
        AABB createAABB(VIntTransform paramTransform);
    }
}
