using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public interface RaycastDetector
    {
        bool raycast(Ray paramRay, VFixedPoint paramDouble, Convex paramConvex, VIntTransform paramTransform, Raycast paramRaycast);
    }
}
