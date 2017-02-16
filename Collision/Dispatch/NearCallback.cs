namespace MobaGame.Collision
{
    public abstract class NearCallback
    {
        public abstract bool handleCollision(BroadphasePair collisionPair, CollisionDispatcher dispatcher, DispatcherInfo dispatchInfo);
    }
}
