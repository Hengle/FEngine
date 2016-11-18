using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace MobaGame
{
    namespace FixedMath
    {
        [Serializable]
        public struct VInt3
        {
            public VFixedPoint x;
            public VFixedPoint y;
            public VFixedPoint z;
            public static readonly VInt3 zero;
            public static readonly VInt3 one;
            public static readonly VInt3 forward;
            public static readonly VInt3 up;
            public static readonly VInt3 right;

			public VInt3(Vector3 vec3)
			{
				x = VFixedPoint.Create (vec3.x);
				y = VFixedPoint.Create (vec3.y);
				z = VFixedPoint.Create (vec3.z);
			}

            VInt3(int _x, int _y, int _z)
            {
                this.x = VFixedPoint.Create(_x);
                this.y = VFixedPoint.Create(_y);
                this.z = VFixedPoint.Create(_z);
            }

            public VInt3(VFixedPoint _x, VFixedPoint _y, VFixedPoint _z)
            {
                this.x = _x;
                this.y = _y;
                this.z = _z;
            }

            static VInt3()
            {
                zero = new VInt3(0, 0, 0);
                one = new VInt3(1, 1, 1);
                forward = new VInt3(0, 0, 1);
                up = new VInt3(0, 1, 0);
                right = new VInt3(1, 0, 0);
            }
            
            public static VFixedPoint Dot(VInt3 lhs, VInt3 rhs)
            {
                return ((lhs.x * rhs.x) + (lhs.y * rhs.y)) + (lhs.z * rhs.z);
            }

            public static VInt3 Cross(VInt3 lhs, VInt3 rhs)
            {
                return new VInt3((lhs.y * rhs.z) - (lhs.z * rhs.y), (lhs.z * rhs.x) - (lhs.x * rhs.z), (lhs.x * rhs.y) - (lhs.y * rhs.x));
            }

            public VInt3 Normalize()
            {
                VFixedPoint amagnitude = magnitude;
                if (amagnitude == VFixedPoint.Zero)
                    return VInt3.zero;
                VFixedPoint ax = x/amagnitude;
                VFixedPoint ay = y/amagnitude;
                VFixedPoint az = z/amagnitude;
                return new VInt3(ax, ay, az);
            }

            public VFixedPoint sqrMagnitude
            {
                get
                {
                    return ((x * x) + (y * y)) + (z * z);
                }
            }
            public VFixedPoint magnitude
            {
                get
                {
                    return FMath.Sqrt(((x * x) + (y * y)) + (z * z));
                }
            }

            public VInt3 AddV3X(VFixedPoint value)
            {
                return new VInt3(x + value, y, z);
            }

            public VInt3 AddV3Y(VFixedPoint value)
            {
                return new VInt3(x, y + value, z);
            }

            public VInt3 AddV3Z(VFixedPoint value)
            {
                return new VInt3(x, y, z + value);
            }

            public VInt3 SetV3X(VFixedPoint value)
            {
                return new VInt3(value, y, z);
            }

            public VInt3 SetV3Y(VFixedPoint value)
            {
                return new VInt3(x, value, z);
            }

            public VInt3 SetV3Z(VFixedPoint value)
            {
                return new VInt3(x, y, value);
            }

            public VFixedPoint this[int index]
            {
                get
                {
                    switch(index)
                    {
                        case 0:
                            return x;
                        case 1:
                            return y;
                        case 2:
                            return z;
                    }
                    throw new Exception("Access violation");
                }

                set
                {
                    switch (index)
                    {
                        case 0:
                            x = value;
                            break;
                        case 1:
                            y = value;
                            break;
                        case 2:
                            z = value;
                            break;
                    }
                }
            }

            public override string ToString()
            {
                object[] objArray1 = new object[] { "( ", this.x, ", ", this.y, ", ", this.z, ")" };
                return string.Concat(objArray1);
            }

            public override bool Equals(object o)
            {
                if (o == null)
                {
                    return false;
                }
                VInt3 num = (VInt3)o;
                return (((this.x == num.x) && (this.y == num.y)) && (this.z == num.z));
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public static VInt3 Lerp(VInt3 a, VInt3 b, VFixedPoint f)
            {
                return new VInt3(a.x * (VFixedPoint.One - f) + b.x * f, a.y * (VFixedPoint.One - f) + b.y * f, a.z * (VFixedPoint.One - f) + b.z * f);
            }

            public VFixedPoint XZSqrDistance(VInt3 rhs)
            {
                VFixedPoint num = this.x - rhs.x;
                VFixedPoint num2 = this.z - rhs.z;
                return ((num * num) + (num2 * num2));
            }

            public bool IsEqualXZ(VInt3 rhs)
            {
                return ((this.x == rhs.x) && (this.z == rhs.z));
            }

            public bool IsEqualXZ(ref VInt3 rhs)
            {
                return ((this.x == rhs.x) && (this.z == rhs.z));
            }

            public static bool operator ==(VInt3 lhs, VInt3 rhs)
            {
                return (((lhs.x == rhs.x) && (lhs.y == rhs.y)) && (lhs.z == rhs.z));
            }

            public static bool operator !=(VInt3 lhs, VInt3 rhs)
            {
                return (((lhs.x != rhs.x) || (lhs.y != rhs.y)) || (lhs.z != rhs.z));
            }

            public static VInt3 operator -(VInt3 lhs, VInt3 rhs)
            {
                lhs.x -= rhs.x;
                lhs.y -= rhs.y;
                lhs.z -= rhs.z;
                return lhs;
            }

            public static VInt3 operator -(VInt3 lhs)
            {
                lhs.x = -lhs.x;
                lhs.y = -lhs.y;
                lhs.z = -lhs.z;
                return lhs;
            }

            public static VInt3 operator +(VInt3 lhs, VInt3 rhs)
            {
                lhs.x += rhs.x;
                lhs.y += rhs.y;
                lhs.z += rhs.z;
                return lhs;
            }

            public static VInt3 operator *(VInt3 lhs, VFixedPoint rhs)
            {
                lhs.x *= rhs;
                lhs.y *= rhs;
                lhs.z *= rhs;
                return lhs;
            }

            public static VInt3 operator *(VInt3 lhs, VInt3 rhs)
            {
                lhs.x *= rhs.x;
                lhs.y *= rhs.y;
                lhs.z *= rhs.z;
                return lhs;
            }

            public static VInt3 operator *(VInt3 lhs, long rhs)
            {
                lhs.x *= rhs;
                lhs.y *= rhs;
                lhs.z *= rhs;
                return lhs;
            }

            public static VInt3 operator /(VInt3 lhs, VFixedPoint rhs)
            {
                lhs.x /= rhs;
                lhs.y /= rhs;
                lhs.z /= rhs;
                return lhs;
            }

            public static implicit operator string(VInt3 ob)
            {
                return ob.ToString();
            }

            public static VFixedPoint Distance(VInt3 a, VInt3 b)
            {
                return (a - b).magnitude;
            }

            public static VFixedPoint Angle(VInt3 a, VInt3 b)
            {
                return FMath.Trig.acos(Dot(a.Normalize(), b.Normalize()));
            }
            public static VFixedPoint SinAngle(VInt3 from, VInt3 to)
            {
                return Cross(from.Normalize(), to.Normalize()).magnitude;
            }
        }

    }
}