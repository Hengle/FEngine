namespace MobaGame.Collision
{
    public abstract class CollisionAlgorithmCreateFunc
    {
        public bool swapped;

        public abstract CollisionAlgorithm createCollisionAlgorithm(CollisionAlgorithmConstructionInfo ci, CollisionObject body0, CollisionObject body1);

        public abstract void releaseCollisionAlgorithm(CollisionAlgorithm algo);
    }
}
