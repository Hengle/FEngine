using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public class DefaultNearCallback: NearCallback
    {
        private readonly ManifoldResult contactPointResult = new ManifoldResult();

        public override ManifoldResult handleCollision(BroadphasePair collisionPair, CollisionDispatcher dispatcher, DispatcherInfo dispatchInfo)
        {
            CollisionObject colObj0 = collisionPair.pProxy0.clientObject;
            CollisionObject colObj1 = collisionPair.pProxy1.clientObject;

            contactPointResult.init(colObj0, colObj1);

            if (dispatcher.needsCollision(colObj0, colObj1))
            {
                // dispatcher will keep algorithms persistent in the collision pair
                if (collisionPair.algorithm == null)
                {
                    collisionPair.algorithm = dispatcher.findAlgorithm(colObj0, colObj1);
                }

                if (collisionPair.algorithm != null)
                {
                    //ManifoldResult contactPointResult = new ManifoldResult(colObj0, colObj1);
                    

                    if (dispatchInfo.dispatchFunc == DispatchFunc.DISPATCH_DISCRETE)
                    {
                        // discrete collision detection query
                        collisionPair.algorithm.processCollision(colObj0, colObj1, dispatchInfo, contactPointResult);
                    }
                    else
                    {
                        //continuous version not implemented
                    }
                }
            }
            return contactPointResult;
        }
    }
}
