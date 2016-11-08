namespace MobaGame.Collision
{
    public abstract class CollisionConfiguration
    {
        public abstract CollisionAlgorithmCreateFunc getCollisionAlgorithmCreateFunc(BroadphaseNativeType proxyType0, BroadphaseNativeType proxyType1);
    }
}
