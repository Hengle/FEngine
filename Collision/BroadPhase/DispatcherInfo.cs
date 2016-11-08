using System.Collections.Generic;

namespace MobaGame.Collision
{
    public class DispatcherInfo
    {
        public float timeStep;
        public int stepCount;
        public DispatchFunc dispatchFunc;
        public float timeOfImpact;
        public bool useContinuous;
        //public IDebugDraw debugDraw;
        public bool useEpa = true;
        public float allowedCcdPenetration = 0.04f;
        //btStackAlloc*	m_stackAllocator;

        public DispatcherInfo()
        {
            dispatchFunc = DispatchFunc.DISPATCH_DISCRETE;
            timeOfImpact = 1f;
        }
    }
}
