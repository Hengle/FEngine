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

    public enum DispatchFunc
    {
        DISPATCH_DISCRETE = 1,
        DISPATCH_CONTINUOUS = 2
    }
}
