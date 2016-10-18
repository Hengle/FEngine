using UnityEngine;

namespace MobaGame
{
    namespace FixedMath
    {
        public struct VIntQuaternion 
        {
            public VFixedPoint x;
            public VFixedPoint y;
            public VFixedPoint z;
            public VFixedPoint w;

            public static readonly VIntQuaternion identity = new VIntQuaternion(Quaternion.identity);


            //W表示轴向转动， xyz表示转动轴方向
            public VIntQuaternion(VFixedPoint x, VFixedPoint y, VFixedPoint z, VFixedPoint w)
            {
                this.x = x;
                this.y = y;
                this.z = z;
                this.w = w;
            }

            public VIntQuaternion(Quaternion q)
            {
                this.x = VFixedPoint.Create(q.x);
                this.y = VFixedPoint.Create(q.y);
                this.z = VFixedPoint.Create(q.z);
                this.w = VFixedPoint.Create(q.w);
            }

            public VInt3 eulerAngles
            {
                get
                {
                    return new VInt3(
                        FMath.Trig.atan2((w*x - y* z)* 2,  VFixedPoint.One - (x*x + y * y) * 2),
                        FMath.Trig.asin((w*y-x* z) * 2),
                        FMath.Trig.atan2((w * z - x * y) * 2, VFixedPoint.One - (z * z + y * y) * 2)
                    );
                }
            }

            public VFixedPoint mangnitude
            {
                get
                {
                    return FMath.Sqrt(x * x + y * y + z * z + w * w);
                }
            }

            public Quaternion quaternion
            {
                get
                {
                    return new Quaternion(x.ToFloat, y.ToFloat, z.ToFloat, w.ToFloat);
                }
            }

            public VIntQuaternion normalized
            {
                get
                {
                    VFixedPoint length = mangnitude;
                    if (length == VFixedPoint.Zero)
                        return identity;
                    return new VIntQuaternion(x / length, y / length, z / length, w / length);
                }
            }

            public static VIntQuaternion operator +(VIntQuaternion lhs, VIntQuaternion rhs)
            {
                return new VIntQuaternion(lhs.x + rhs.x, lhs.y + rhs.y, lhs.z + rhs.z, lhs.w + rhs.w);
            }

            public static VIntQuaternion operator *(VIntQuaternion lhs, VIntQuaternion rhs)
            {
                VInt3 lvirtual = new VInt3(lhs.x, lhs.y, lhs.z);
                VFixedPoint lreal = lhs.w;
                VInt3 rvirtual = new VInt3(rhs.x, rhs.y, rhs.z);
                VFixedPoint rreal = rhs.w;
                VInt3 virtualPart = VInt3.Cross(lvirtual, rvirtual) + rvirtual * lreal + lvirtual * rreal;
                VFixedPoint realPart = lreal * rreal - VInt3.Dot(lvirtual, rvirtual);
                return new VIntQuaternion(virtualPart.x, virtualPart.y, virtualPart.z, realPart);
            }

            public static VInt3 operator *(VIntQuaternion lhs, VInt3 rhs)
            {
                VIntQuaternion virtualQuaternion = new VIntQuaternion(rhs.x, rhs.y, rhs.z, VFixedPoint.Zero);
                VIntQuaternion anormalized = lhs.normalized;
                VIntQuaternion aconjugated = new VIntQuaternion(-anormalized.x, -anormalized.y, -anormalized.z, anormalized.w);
                VIntQuaternion transformed = anormalized * virtualQuaternion * aconjugated;
                return new VInt3(transformed.x, transformed.y, transformed.z);
            }

            public static VIntQuaternion operator *(VIntQuaternion lhs, VFixedPoint rhs)
            {
                return new VIntQuaternion(lhs.x * rhs, lhs.y * rhs, lhs.z * rhs, lhs.w * rhs);
            }

            public static VIntQuaternion Slerp(VIntQuaternion from, VIntQuaternion to, VFixedPoint t)
            {
                if (t <= VFixedPoint.Zero)
                    return from;
                else if (t >= VFixedPoint.One)
                    return to;
                else
                {
                    VFixedPoint costheta = Dot(from.normalized, to.normalized);
                    VFixedPoint theta = FMath.Trig.acos(costheta);
                    VFixedPoint sintheta = FMath.Trig.Sin(theta);
                    Debug.Log(costheta);
                    Debug.Log(theta);
                    Debug.Log(sintheta);
                    VFixedPoint sinthetat = FMath.Trig.Sin(theta * t);
                    VFixedPoint sinthetaOneMinusT = FMath.Trig.Sin(theta * (VFixedPoint.One - t));
                    return from * (sinthetaOneMinusT / sintheta) + to * (sinthetat / sintheta);
                }
            }

            public static VFixedPoint Dot(VIntQuaternion a, VIntQuaternion b)
            {
                return a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w;
            }

            public static VIntQuaternion AngleAxis(VFixedPoint angle, VInt3 axis)
            {
                axis = axis.Normalize();
                return new VIntQuaternion(FMath.Trig.Sin(angle / 2) * axis.x, FMath.Trig.Sin(angle / 2) * axis.y, FMath.Trig.Sin(angle / 2) * axis.z, FMath.Trig.Cos(angle / 2));
            }

            public static VIntQuaternion FromToRotation(VInt3 from, VInt3 to)
            {
                VInt3 axis = VInt3.Cross(from, to);
                VFixedPoint angle = FMath.Trig.acos(VInt3.Dot(from, to));
                return AngleAxis(angle, axis);
            }

            public override string ToString()
            {
                return ("(" + x.ToString() + ", " + y.ToString() + ", " + z.ToString() + ", " + w.ToString() + ")");
            }
        }
    }
}
