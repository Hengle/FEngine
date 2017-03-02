using System.Collections.Generic;
using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    class SphereSphereSweepAlgorithm
    {
        static bool quadraticFormula(VFixedPoint a, VFixedPoint b, VFixedPoint c, ref VFixedPoint r1, ref VFixedPoint r2)
        {
            VFixedPoint q = b * b - a * c * 4; 
	        if(q >= VFixedPoint.Zero)
	        {
                VFixedPoint sq = FMath.Sqrt(q);
                VFixedPoint d = VFixedPoint.One / (a * 2);
                r1 = (-b + sq) * d;
		        r2 = (-b - sq) * d;
		        return true;//real roots
	        }
	        else
	        {
		        return false;//complex roots
	        }
        }

        public static void objectQuerySingle(CollisionObject castObject, VInt3 ToPos, CollisionObject collisionObject, List<CastResult> results, VFixedPoint allowedPenetration)
        {
            SphereShape testShape = (SphereShape)castObject.getCollisionShape();
            SphereShape sphereShape = (SphereShape)collisionObject.getCollisionShape();
            VIntTransform collisionObjectTransform = collisionObject.getWorldTransform();

            VFixedPoint d = VFixedPoint.Zero;
            VFixedPoint tmp = VFixedPoint.Zero;
            VInt3 normal = VInt3.zero;
            if(sphereSphereSweep(testShape.getRadius(), castObject.getWorldTransform().position, ToPos, sphereShape.getRadius(), 
                collisionObjectTransform.position, ref d, ref tmp, ref normal))
            {
                CastResult result = new CastResult();
                result.hitObject = collisionObject;
                result.fraction = d;
                result.normal = normal;
                results.Add(result);
            }            
        }

        public static bool sphereSphereSweep(VFixedPoint ra,// radius of sphere A
                                        VInt3 A0,//from position of sphere A
                                        VInt3 A1,//to position of sphere A
                                        VFixedPoint rb,//radius of sphere B
                                        VInt3 B0,//position of sphere B
                                        ref VFixedPoint u0, //normalized time of first collision
                                        ref VFixedPoint u1, //normalized time of second collision
                                        ref VInt3 normal
                                      )
        {
            VInt3 vab = A1 - A0;
            VInt3 AB = B0 - A0;
            VFixedPoint rab = ra + rb;

            VFixedPoint a = vab.sqrMagnitude;
            VFixedPoint b = VInt3.Dot(vab, AB) * 2;

            VFixedPoint c = AB.sqrMagnitude - rab * rab;

            //check if they're currently overlapping
            if(c <= VFixedPoint.Zero || a == VFixedPoint.Zero)
            {
                u0 = VFixedPoint.Zero;
                normal = B0 - A0;
                return true;
            }

            if(quadraticFormula(a, b, c, ref u0, ref u1))
            {
                if(u0 > u1)
                {
                    VFixedPoint tmp = u0;
                    u0 = u1;
                    u1 = tmp;
                }

                if (u1 < VFixedPoint.Zero || u0 > VFixedPoint.Zero) return false;

                normal = (B0 - (A0 + vab * u0)).Normalize();
                return true;
            }

            return false;
        }
    }
}
