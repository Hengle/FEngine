using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public delegate void RaytestAlgorithm(VInt3 FromPos, VInt3 ToPos,
            CollisionObject collisionObject,
            RayResultCallback resultCallback);


    public class RayResultCallback
    {
        public RayResultCallback()
        {
            Reset();
        } 

        public void Reset()
        {
            closestHitFraction = VFixedPoint.One;
            hasHit = false;
            hitNormalWorld = VInt3.zero;
            hitObject = null; 
        }

        public VFixedPoint closestHitFraction
        {
            get; protected set;
        }

        public VInt3 hitNormalWorld
        {
            get; protected set;
        }

        public CollisionObject hitObject
        {
            get; protected set;
        }
            
        public bool hasHit
        {
            get; protected set;
        }

        public void addSingleResult(CollisionObject collisionObject, VInt3 hitNormalWorld, VFixedPoint hitFraction)
        {
            hasHit = true;
            if(hitFraction < closestHitFraction)
            {
                closestHitFraction = hitFraction;
                this.hitNormalWorld = hitNormalWorld;
                hitObject = collisionObject;
            }
        }
    }
}
