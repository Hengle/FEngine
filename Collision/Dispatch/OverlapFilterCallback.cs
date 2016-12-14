namespace MobaGame.Collision
{
    public abstract class OverlapFilterCallback
    {
        public abstract bool needBroadphaseCollision(BroadphaseProxy proxy0, BroadphaseProxy proxy1);
    }
}
