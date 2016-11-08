namespace MobaGame.Collision
{
    public abstract class NearCallback
    {
        public abstract void handleCollision(BroadphasePair collisionPair, CollisionDispatcher dispatcher, DispatcherInfo dispatchInfo);
    }
}
