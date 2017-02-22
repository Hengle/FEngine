using MobaGame.FixedMath;
using System.Collections.Generic;
using System;

namespace MobaGame.Collision
{
    class KinematicCharacterController : ActionInterface
    {
        VInt3[] upAxisDirection = new VInt3[] { VInt3.right, VInt3.up, VInt3.forward };

        protected VFixedPoint halfHeight;
        protected CollisionObject me;

        protected VFixedPoint verticalVelocity;
        protected VFixedPoint verticalOffset;
        protected VFixedPoint fallSpeed;
        protected VFixedPoint jumpSpeed;
        protected VFixedPoint maximumJumpHeight;
        protected VFixedPoint maxSlopeRadians;
        protected VFixedPoint maxSlopeCosine;
        protected VFixedPoint gravity;
        protected VFixedPoint turnAngle;
        protected VFixedPoint stepHeight;
        protected VFixedPoint addedMargin;
        protected VInt3 walkDirection;
        protected VInt3 normalizedDirection;
        protected VInt3 currentPosition;
        protected VFixedPoint currentStepOffset;
        protected VInt3 targetPosition;

        List<ManifoldResult> manifolds = new List<ManifoldResult>();

        protected VInt3 touchingNormal;

        protected bool wasOnGround;

        protected VFixedPoint velocityTimeInterval;
        protected int upAxis;

        public KinematicCharacterController(CollisionObject me, VFixedPoint stepHeight): this(me, stepHeight, 1)
        {
            
        }

        public KinematicCharacterController(CollisionObject me, VFixedPoint stepHeight, int upAxis)
        {
            this.upAxis = upAxis;
            this.addedMargin = VFixedPoint.Two / VFixedPoint.Create(100);
            this.me = me;
            this.gravity = VFixedPoint.Create(10); // 1G acceleration
            this.fallSpeed = VFixedPoint.Create(55); // Terminal velocity of a sky diver in m/s.
            this.jumpSpeed = VFixedPoint.Create(10); 
            this.wasOnGround = false;
            maxSlopeRadians = VFixedPoint.Create(50) / VFixedPoint.Create(180) / FMath.Trig.Rag2Deg;
            maxSlopeCosine = FMath.Trig.Cos(maxSlopeRadians);
        }

        public override void updateAction(CollisionWorld collisionWorld, VFixedPoint deltaTimeStep)
        {
            preStep(collisionWorld);
            playerStep(collisionWorld, deltaTimeStep);
        }

        public void setVelocityForTimeInterval(VInt3 velocity, VFixedPoint timeInterval)
        {
            walkDirection = velocity;
            normalizedDirection = walkDirection.Normalize();
		    velocityTimeInterval = timeInterval;
	    }

        public void warp(VInt3 origin)
        {
            VIntTransform transform = me.getWorldTransform();
            transform.position = origin;
            me.setWorldTransform(transform);
        }

        public void preStep(CollisionWorld collisionWorld)
        {
            int numPenetrationLoops = 0;
            while(recoverFromPenetration(collisionWorld))
            {
                numPenetrationLoops++;
                if(numPenetrationLoops > 4)
                {
                    break;
                }
            }
            currentPosition = me.getWorldTransform().position;
            targetPosition = currentPosition;
        }

        public void playerStep(CollisionWorld collisionWorld, VFixedPoint dt)
        {
            wasOnGround = onGround();

            verticalVelocity -= gravity * dt;
            if(verticalVelocity > VFixedPoint.Zero && verticalVelocity > jumpSpeed)
            {
                verticalVelocity = jumpSpeed;
            }

            if(verticalVelocity < VFixedPoint.Zero && verticalVelocity.Abs() > fallSpeed.Abs())
            {
                verticalVelocity = -fallSpeed.Abs();
            }
            verticalOffset = verticalVelocity * dt;

            VIntTransform transform = me.getWorldTransform();
            stepUp(collisionWorld);

            VFixedPoint dtMoving = FMath.Min(dt, velocityTimeInterval);
            velocityTimeInterval -= dt;
            VInt3 move = walkDirection * dtMoving;
            stepForwardAndStrafe(collisionWorld, move);

            stepDown(collisionWorld, dt);

            transform.position = currentPosition;
            me.setWorldTransform(transform);
        }

        public bool canJump()
        {
            return onGround();
        }

        public void jump()
        {
            if (!canJump()) return;

            verticalVelocity = jumpSpeed;
        }

        protected VInt3 computeReflectionDirection(VInt3 direction, VInt3 normal)
        {
            VInt3 result = normal * VInt3.Dot(direction, normal) * -2 + direction;
            return result;
        }

        protected VInt3 parallelComponent(VInt3 direction, VInt3 normal)
        {
            return normal * VInt3.Dot(direction, normal);
        }

        protected VInt3 perpendicularComponent(VInt3 direction, VInt3 normal)
        {
            VInt3 perpendicular = direction - parallelComponent(direction, normal);
            return perpendicular;
        }

        protected bool recoverFromPenetration(CollisionWorld collisionWorld)
        {
            bool penetration = false;
            VFixedPoint maxPen = VFixedPoint.Zero;

            manifolds.Clear();
            collisionWorld.OverlapTest(me, manifolds);
            for(int i = 0; i < manifolds.Count; i++)
            {
                ManifoldResult aresult = manifolds[i];
                int directionSign = aresult.body0 == me ? -1 : 1;
                VFixedPoint pen = aresult.depth;
                if(pen < VFixedPoint.Zero)
                {
                    if(pen < maxPen)
                    {
                        maxPen = pen;
                        touchingNormal = aresult.normalWorldOnB * directionSign;
                    }

                    currentPosition += aresult.normalWorldOnB * directionSign * pen * VFixedPoint.Create(0.2f);
                    penetration = true;
                }
            }

            return penetration;
        }

        protected void stepUp(CollisionWorld collisionWorld)
        {
            
        }

        protected void stepForwardAndStrafe(CollisionWorld collisionWorld, VInt3 walkMove)
        {

        }

        protected void stepDown(CollisionWorld collisionWorld, VFixedPoint dt)
        {

        }

        public bool onGround()
        {
            return verticalVelocity == VFixedPoint.Zero && verticalOffset == VFixedPoint.Zero;
        }
    }
}
