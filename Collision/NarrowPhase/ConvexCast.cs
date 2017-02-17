using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public abstract class ConvexCast
    {
        public ConvexShape convexA;
        public ConvexShape convexB;
        public abstract bool calcTimeOfImpact(VIntTransform fromA, VIntTransform toA, VIntTransform fromB, VIntTransform toB, CastResult result);        
    }

    public class CastResult
    {
        public VIntTransform hitTransformA;
        public VIntTransform hitTransformB;

        public VInt3 normal;
        public VInt3 hitPoint;
        public VFixedPoint fraction = VFixedPoint.MaxValue; // input and output
        public VFixedPoint allowedPenetration = VFixedPoint.Zero;
    }
}
