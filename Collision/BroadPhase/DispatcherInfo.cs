using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public class DispatcherInfo
    {
        public VFixedPoint timeStep;
        public int stepCount;
        public DispatchFunc dispatchFunc;
        public VFixedPoint timeOfImpact;
        public bool useContinuous;
        //public IDebugDraw debugDraw;
        public bool useEpa = true;
        public VFixedPoint allowedCcdPenetration = VFixedPoint.Create(4) / VFixedPoint.Create(100);
        //btStackAlloc*	m_stackAllocator;

        public DispatcherInfo()
        {
            dispatchFunc = DispatchFunc.DISPATCH_DISCRETE;
            timeOfImpact = VFixedPoint.One;
        }
    }
}
