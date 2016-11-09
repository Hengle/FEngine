using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public interface DistanceDetector
    {
        bool distance(Convex paramConvex1, VIntTransform paramTransform1, Convex paramConvex2, VIntTransform paramTransform2, Separation paramSeparation);
    }
}
