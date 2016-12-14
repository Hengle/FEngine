using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public class DispatcherInfo
    {
        public DispatchFunc dispatchFunc;
        public VFixedPoint timeOfImpact;

        public DispatcherInfo()
        {
            dispatchFunc = DispatchFunc.DISPATCH_DISCRETE;
            timeOfImpact = VFixedPoint.One;
        }
    }
}
