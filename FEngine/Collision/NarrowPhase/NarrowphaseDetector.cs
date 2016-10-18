using MobaGame.FixedMath;
using System.Text;

namespace MobaGame.Collision
{
    public interface NarrowphaseDetector
    {
        bool detect(Convex paramConvex1, VIntTransform paramTransform1, Convex paramConvex2, VIntTransform paramTransform2, Penetration paramPenetration);

        bool detect(Convex paramConvex1, VIntTransform paramTransform1, Convex paramConvex2, VIntTransform paramTransform2);
    }
}
