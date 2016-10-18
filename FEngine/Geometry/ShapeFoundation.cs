using MobaGame.FixedMath;

namespace MobaGame.Collision
{

    public interface Translatable
    {
        void translate(VFixedPoint x, VFixedPoint y, VFixedPoint z);
        void translate(VInt3 paramVector2);
    }

    public interface Rotatble
    {
        void rotate(VFixedPoint paramDouble, VInt3 paramVector2);
        void rotate(VFixedPoint paramDouble1, VFixedPoint paramDouble2, VFixedPoint paramDouble3);
    }

    public interface Transformable: Translatable, Rotatble
    {
       
    }

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

    public interface Convex: Shape
    {
        VInt3[] getAxes(VInt3[] paramArrayOfVector2, VIntTransform paramTransform);
        VInt3[] getFoci(VIntTransform paramTransform);
        VInt3 getFarthestPoint(VInt3 paramVector2, VIntTransform paramTransform);
    }

    public interface Shiftable
    {
        void shift(VInt3 paramVector2);
    }
}
