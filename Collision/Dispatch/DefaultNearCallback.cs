namespace MobaGame.Collision
{
    public class DefaultNearCallback: NearCallback
    {
        public override bool handleCollision(BroadphasePair collisionPair, CollisionDispatcher dispatcher)
        {
            CollisionObject colObj0 = collisionPair.pProxy0.clientObject;
            CollisionObject colObj1 = collisionPair.pProxy1.clientObject;

            if (colObj0.isStaticOrKinematicObject() && colObj1.isStaticOrKinematicObject())
                return false;

            ManifoldResult contactPointResult = dispatcher.applyManifold();
            contactPointResult.init(colObj0, colObj1);

            CollisionAlgorithm algorithm = dispatcher.findAlgorithm(colObj0, colObj1);
            // discrete collision detection query
            algorithm(colObj0, colObj1, dispatcher.getDispatchInfo(), contactPointResult);
            if(!contactPointResult.hasContact)
            {
                dispatcher.releaseManifold(contactPointResult);
            }
            return contactPointResult.hasContact;

        }
    }
}
