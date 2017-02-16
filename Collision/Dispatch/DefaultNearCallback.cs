namespace MobaGame.Collision
{
    public class DefaultNearCallback: NearCallback
    {
        public override bool handleCollision(BroadphasePair collisionPair, CollisionDispatcher dispatcher, DispatcherInfo dispatchInfo)
        {
            CollisionObject colObj0 = collisionPair.pProxy0.clientObject;
            CollisionObject colObj1 = collisionPair.pProxy1.clientObject;

            ManifoldResult contactPointResult = dispatcher.applyManifold();
            contactPointResult.init(colObj0, colObj1);

            if (dispatcher.needsCollision(colObj0, colObj1))
            {
                CollisionAlgorithm algorithm = dispatcher.findAlgorithm(colObj0, colObj1);
                // discrete collision detection query
                algorithm.processCollision(colObj0, colObj1, dispatchInfo, contactPointResult);
                if(contactPointResult.manifoldPoint == null)
                {
                    dispatcher.releaseManifold(contactPointResult);
                }
                return contactPointResult.manifoldPoint != null;
            }
            else
            {
                dispatcher.releaseManifold(contactPointResult);
                return false;
            }
        }
    }
}
