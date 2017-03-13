namespace MobaGame.Collision
{
    public class DefaultNearCallback: NearCallback
    {
        public override bool handleCollision(BroadphasePair collisionPair, CollisionDispatcher dispatcher)
        {
            CollisionObject colObj0 = collisionPair.pProxy0.clientObject;
            CollisionObject colObj1 = collisionPair.pProxy1.clientObject;

            if (colObj0.isStaticOrKinematicObject() && colObj1.isStaticOrKinematicObject())
            {
                collisionPair.manifold.clearManifold(); 
                return false;
            }

            PersistentManifold manifold = collisionPair.manifold;
            int oldContacts = manifold.getContactPointsNum();
            manifold.refreshContactPoints(colObj0.getWorldTransform(), colObj1.getWorldTransform());
            if (oldContacts == manifold.getContactPointsNum() && oldContacts > 0)
                return true;

            manifold.clearManifold();
            CollisionAlgorithm algorithm = dispatcher.findAlgorithm(colObj0, colObj1);
            // discrete collision detection query
            algorithm(colObj0, colObj1, dispatcher.getDispatchInfo(), manifold);
            return manifold.getContactPointsNum() > 0;

        }
    }
}
