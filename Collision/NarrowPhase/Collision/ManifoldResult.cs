using System;
using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public class ManifoldResult : DiscreteCollisionDetectorInterface.Result
    {
        // we need this for compounds
        public CollisionObject body0;
        public CollisionObject body1;
        public VInt3 normalWorldOnB;
        public VFixedPoint depth;
        //for reduce calculation
        VFixedPoint InvK;
        VFixedPoint Bias;
        
        public ManifoldResult()
        {

        }

        public void init(CollisionObject body0, CollisionObject body1)
        {
            this.body0 = body0;
            this.body1 = body1;
            hasContact = false;
        }

        public override void addContactPoint(VInt3 normalOnBInWorld, VFixedPoint depth)
        {
            normalWorldOnB = normalOnBInWorld;
            this.depth = depth;
            hasContact = true;
        }

        public void PreStep(VFixedPoint dt)
        {
            InvK = VFixedPoint.One / (body0.InvMass + body1.InvMass);
            Bias = Globals.BIAS_FACTOR / dt * FMath.Min(VFixedPoint.Zero, depth + Globals.ALLOWD_PENETRATION);
        }

        public void ApplyImpulse(VFixedPoint dt)
        {
            VInt3 relVel = body0.LinearVel - body1.LinearVel;
            VFixedPoint vn = VInt3.Dot(relVel, normalWorldOnB);
            VFixedPoint dPn = FMath.Max(InvK * (-vn + Bias), VFixedPoint.Zero);
            VInt3 Pn = normalWorldOnB * dPn;

            body0.ApplyImpulse(Pn);
            body1.ApplyImpulse(-Pn);

            VInt3 vt = relVel - normalWorldOnB * vn;
            VFixedPoint vtMagnitude = vt.magnitude;
            VFixedPoint dPt = InvK * vtMagnitude;
            VFixedPoint maxPt = Globals.FRICTION * dPn;
            dPt = FMath.Max(FMath.Min(dPt, maxPt), -maxPt);
            VInt3 Pt = vt / vtMagnitude * dPt;

            body0.ApplyImpulse(-Pt);
            body1.ApplyImpulse(Pt);
        }
    }
}
