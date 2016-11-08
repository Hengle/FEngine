namespace MobaGame.Collision
{
    public abstract class OverlappingPairCallback
    {
        public abstract BroadphasePair addOverlappingPair(BroadphaseProxy proxy0, BroadphaseProxy proxy1);

        public abstract Object removeOverlappingPair(BroadphaseProxy proxy0, BroadphaseProxy proxy1, Dispatcher dispatcher);

        public abstract void removeOverlappingPairsContainingProxy(BroadphaseProxy proxy0, Dispatcher dispatcher);
    }
}
