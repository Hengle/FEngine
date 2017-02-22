namespace MobaGame.Collision
{
    public class DefaultNearCallback: NearCallback
    {
        public override bool handleCollision(BroadphasePair collisionPair, CollisionDispatcher dispatcher)
        {
            CollisionObject colObj0 = collisionPair.pProxy0.clientObject;
            CollisionObject colObj1 = collisionPair.pProxy1.clientObject;

            ManifoldResult contactPointResult = dispatcher.applyManifold();
            contactPointResult.init(colObj0, colObj1);

            if (dispatcher.needsCollision(colObj0, colObj1))
            {
                CollisionAlgorithm algorithm = dispatcher.findAlgorithm(colObj0, colObj1);
                // discrete collision detection query
                algorithm.processCollision(colObj0, colObj1, dispatcher.getDispatchInfo(), contactPointResult);
                if(!contactPointResult.hasContact)
                {
                    dispatcher.releaseManifold(contactPointResult);
                }
                return contactPointResult.hasContact;
            }
            else
            {
                dispatcher.releaseManifold(contactPointResult);
                return false;
            }
        }
    }
}
