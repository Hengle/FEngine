namespace MobaGame.Collision
{
    public abstract class NearCallback
    {
        public abstract ManifoldResult handleCollision(BroadphasePair collisionPair, CollisionDispatcher dispatcher, DispatcherInfo dispatchInfo);
    }
}
