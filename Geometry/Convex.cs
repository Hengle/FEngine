using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public interface Convex: Shape
    {
        VInt3[] getAxes(VInt3[] paramArrayOfVector2, VIntTransform paramTransform);
        VInt3[] getFoci(VIntTransform paramTransform);
        VInt3 getFarthestPoint(VInt3 paramVector2, VIntTransform paramTransform);
    }
}
