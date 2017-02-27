namespace MobaGame.Collision
{
    public abstract class CollisionConfiguration
    {
        public abstract CollisionAlgorithm getCollisionAlgorithmCreateFunc(BroadphaseNativeType proxyType0, BroadphaseNativeType proxyType1);

        public abstract RaytestAlgorithm getRaytestAlgorithm(BroadphaseNativeType proxyType);

        public abstract SweepAlgorithm getSweepAlgorithmCreateFunc(BroadphaseNativeType proxyType0, BroadphaseNativeType proxyType1);
    }
}
