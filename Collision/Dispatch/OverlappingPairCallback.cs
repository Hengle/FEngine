namespace MobaGame.Collision
{
    public abstract class OverlappingPairCallback
    {
        public abstract BroadphasePair addOverlappingPair(BroadphaseProxy proxy0, BroadphaseProxy proxy1);

        public abstract void removeOverlappingPair(BroadphaseProxy proxy0, BroadphaseProxy proxy1);

        public abstract void removeOverlappingPairsContainingProxy(BroadphaseProxy proxy0);
    }
}
