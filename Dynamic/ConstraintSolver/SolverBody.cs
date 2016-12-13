using MobaGame.FixedMath;
using UnityEditor.MemoryProfiler;

namespace MobaGame.Collision
{
    public class SolverBody
    {
        public VIntTransform worldTransform;
        public VInt3 deltaLinearVelocity;
        public VInt3 deltaAngularVelocity;
        public VInt3 angularFactor;
        public VInt3 linearFactor;
        public VInt3 invMass;
        public VInt3 pushVelocity;
        public VInt3 turnVelocity;
        public VInt3 linearVelocity;
        public VInt3 angularVelocity;
        public VInt3 externalForceImpulse;
        public VInt3 externalTorqueImpulse;

        public RigidBody originalBody;

        public void setWorldTransform(VIntTransform worldTransform)
        {
            this.worldTransform = worldTransform;
        }

        public VIntTransform getWorldTransform()
        {
            return worldTransform;
        }

        public VInt3 getVelocityInLocalPointNoDelta(VInt3 rel_pos)
        {
            if (originalBody != null)
                return linearVelocity + externalForceImpulse + VInt3.Cross(angularVelocity + externalTorqueImpulse, rel_pos);
            else
                return VInt3.zero;
        }

        public VInt3 getVelocityInLocalPoint(VInt3 rel_pos)
        {
            if (originalBody != null)
                return linearVelocity + deltaLinearVelocity + VInt3.Cross(angularVelocity + deltaAngularVelocity, rel_pos);
            else
                return VInt3.zero;
        }

        public VInt3 getAngularVelocity()
        {
            if (originalBody != null)
                return angularVelocity + deltaAngularVelocity;
            else
                return VInt3.zero;
        }

        public void applyImpulse(VInt3 linearComponent, VInt3 angularComponent, VFixedPoint impulseMagnitude)
        {
            if (originalBody != null)
            {
                deltaLinearVelocity += linearComponent * impulseMagnitude * linearFactor;
                deltaAngularVelocity += angularComponent * angularFactor * impulseMagnitude;
            }
        }

        public void internalApplyPushImpulse(VInt3 linearComponent, VInt3 angularComponent, VFixedPoint impulseMagnitude)
        {
            if (originalBody != null)
            {
                pushVelocity += linearComponent * impulseMagnitude * linearFactor;
                turnVelocity += angularComponent * angularFactor * impulseMagnitude;
            }
        }

        public VInt3 getDeltaLinearVelocity()
	    {
		    return deltaLinearVelocity;
	    }

        public VInt3 getDeltaAngularVelocity()
	    {
		    return deltaAngularVelocity;
	    }

        public VInt3 getPushVelocity()
	    {
		    return pushVelocity;
	    }

        public VInt3 getTurnVelocity()
	    {
		    return turnVelocity;
	    }


        ////////////////////////////////////////////////
        ///some internal methods, don't use them

        public VInt3 internalGetDeltaLinearVelocity()
        {
            return deltaLinearVelocity;
        }

        public VInt3 internalGetDeltaAngularVelocity()
        {
            return deltaAngularVelocity;
        }

        public VInt3 internalGetAngularFactor()
	    {
		    return angularFactor;
	    }

        public VInt3 internalGetInvMass()
	    {
		    return invMass;
	    }

        public void internalSetInvMass(VInt3 invMass)
        {
            this.invMass = invMass;
        }

        public VInt3 internalGetPushVelocity()
        {
            return pushVelocity;
        }

        public VInt3 internalGetTurnVelocity()
        {
            return turnVelocity;
        }

        public VInt3 internalGetVelocityInLocalPointObsolete(VInt3 rel_pos)
        {
            return linearVelocity + deltaLinearVelocity + VInt3.Cross(angularVelocity + deltaAngularVelocity, rel_pos);
        }

        public VInt3 internalGetAngularVelocity()
        {
            return angularVelocity + deltaAngularVelocity;
        }

        public void internalApplyImpulse(VInt3 linearComponent, VInt3 angularComponent,VFixedPoint impulseMagnitude)
	    {
		    if (originalBody != null)
		    {
			    deltaLinearVelocity += linearComponent* impulseMagnitude*linearFactor;
			    deltaAngularVelocity += angularComponent*(angularFactor * impulseMagnitude);
            }
        }
    

        public void writebackVelocity()
        {
            if (originalBody != null)
            {
                linearVelocity += deltaLinearVelocity;
                angularVelocity += deltaAngularVelocity;
            }
        }

        public void writebackVelocityAndTransform(VFixedPoint timeStep, VFixedPoint splitImpulseTurnErp)
        {
            if (originalBody != null)
            {
                linearVelocity += deltaLinearVelocity;
                angularVelocity += deltaAngularVelocity;

                //correct the position/orientation based on push/turn recovery
                
                if (pushVelocity != VInt3.zero &&  turnVelocity != VInt3.zero)
                {
                    worldTransform = TransformUtil.integrateTransform(worldTransform, pushVelocity, turnVelocity * splitImpulseTurnErp, timeStep);
                }
            }
        }
    }
}