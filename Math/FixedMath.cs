using System.Collections;
using System;

namespace MobaGame
{
    namespace FixedMath
    {
        public class FMathException:Exception
        {
            public enum REASON
            {
                SQRT_MINUS,
                DIVIDE_ZERO,
                LN_NON_POSITIVE
            }

            public REASON reason;

            public FMathException(REASON reason)
            {
                this.reason = reason;
            }
        }

        public static class FMath
        {

            #region Meta
            public static readonly VFixedPoint Pi = VFixedPoint.Create(355) / VFixedPoint.Create(113);
            public static readonly VFixedPoint TwoPi = Pi * VFixedPoint.Create(2);
            public static readonly VFixedPoint e = VFixedPoint.Create(2.71828182845f);
            public static readonly VFixedPoint ln2 = VFixedPoint.Create(0.69314718f);
            #endregion

            #region Math

            /// <summary>
            /// Square root.
            /// </summary>
            /// <param name="f1">f1.</param>

            public static VFixedPoint Sqrt(VFixedPoint f1)
            {
                if (f1.ValueBar < 0)
                {
                    if(f1.ValueBar < -16)
                    {
                        throw new FMathException(FMathException.REASON.SQRT_MINUS);
                    }
                    else
                    {
                        return VFixedPoint.Zero;
                    }
                }
                    
                ulong a = (ulong)f1.ValueBar;
                ulong num = 0L;
                ulong num2 = 0L;
                for (int i = 0; i < 0x20; i++)
                {
                    num2 = num2 << 1;
                    num = num << 2;
                    num += a >> 0x3e;
                    a = a << 2;
                    if (num2 < num)
                    {
                        num2 += (ulong)1L;
                        num -= num2;
                        num2 += (ulong)1L;
                    }
                }
                return new VFixedPoint(((long)(num2 >> 1) & 0xffffffffL) << (VFixedPoint.SHIFT_AMOUNT / 2));
            }

            public static VFixedPoint Pow(VFixedPoint f1, int n)
            {
                if(n < 0)
                {
                    return VFixedPoint.One / Pow(f1, -n);
                }

                VFixedPoint result = VFixedPoint.One;
                while(n != 0)
                {
                    if((n & 1) != 0)
                    {
                        result *= f1;
                    }
                    f1 *= f1;
                    n = n >> 1;
                }
                return result;
            }

            public static VFixedPoint Exp(VFixedPoint a)
            {
                if (a < VFixedPoint.Zero)
                {
                    return VFixedPoint.One / Exp(-a);
                }
                int n = (int)a.ToInt;
                a -= VFixedPoint.Create(n);
                VFixedPoint e1 = Pow(e, n);
                VFixedPoint e2 = eee(a);
                return e1 * e2;
            }

            static VFixedPoint eee(VFixedPoint a)
            {
                if (a > VFixedPoint.Create(0.01f))
                {
                    VFixedPoint ee = eee(a / VFixedPoint.Create(2));
                    return ee * ee;
                }

                return VFixedPoint.One + a + a * a / VFixedPoint.Create(2) + Pow(a, 3) / VFixedPoint.Create(6);
            }

            public static VFixedPoint Ln(VFixedPoint a)
            {
                if(a <=VFixedPoint.Zero)
                {
                    throw new FMathException(FMathException.REASON.LN_NON_POSITIVE);
                }


                VFixedPoint n1 = VFixedPoint.Zero;
                while(a >= VFixedPoint.Two || a <= VFixedPoint.Half)
                {
                    if(a >= VFixedPoint.Two)
                    {
                        n1 += ln2;
                        a = VFixedPoint.FromBinary(a.ToBinary() >>  1);
                    }
                    else if(a <= VFixedPoint.Half)
                    {
                        n1 -= ln2;
                        a = VFixedPoint.FromBinary(a.ToBinary() << 1);
                    }
                }

                VFixedPoint t = (a - VFixedPoint.One) / (a + VFixedPoint.One);
                VFixedPoint n2 = (t + Pow(t, 3) / 3 + Pow(t, 5) / 5) * 2;
                return n1 + n2;
            }

            public static VFixedPoint Pow(VFixedPoint a, VFixedPoint b)
            {
                return Exp(b * Ln(a));
            }

            #endregion

            #region Helpful

            public static VFixedPoint Lerp(VFixedPoint from, VFixedPoint to, VFixedPoint t)
            {
                if (t >= VFixedPoint.One)
                    return to;
                else if (t.ValueBar <= 0)
                    return from;
                return (to * t + from * (VFixedPoint.One - t));
            }

            public static VFixedPoint Min(VFixedPoint f1, VFixedPoint f2)
            {
                return f1 <= f2 ? f1 : f2;
            }

            public static VFixedPoint Max(VFixedPoint f1, VFixedPoint f2)
            {
                return f1 >= f2 ? f1 : f2;
            }
            public static VFixedPoint Clamp(VFixedPoint f1, VFixedPoint min, VFixedPoint max)
            {
                if (f1 < min) return min;
                if (f1 > max) return max;
                return f1;
            }
            #endregion

            public static class Trig
            {
                public static readonly VFixedPoint Rag2Deg = VFixedPoint.Create(180) / Pi;

                public static VFixedPoint Sin(VFixedPoint theta)
                {
                    int index = SinCosLookupTable.getIndex(theta.ValueBar, 1 << VFixedPoint.SHIFT_AMOUNT);
                    return VFixedPoint.Create(SinCosLookupTable.sin_table[index]) / VFixedPoint.Create(SinCosLookupTable.FACTOR);
                }

                public static VFixedPoint Cos(VFixedPoint theta)
                {
                    return Sin(Pi * VFixedPoint.Half - theta);
                }

                public static VFixedPoint Tan(VFixedPoint theta)
                {
                    return Sin(theta)/Cos(theta);
                }

                public static VFixedPoint acos(VFixedPoint a)
                {
                    int num = (int)a.ValueBar * AcosLookupTable.HALF_COUNT / (1<<VFixedPoint.SHIFT_AMOUNT) + AcosLookupTable.HALF_COUNT;
                    num = Math.Min(Math.Max(num, 0), AcosLookupTable.COUNT);
                    return VFixedPoint.Create(AcosLookupTable.table[num]) / VFixedPoint.Create(0x2710L);
                }

                public static VFixedPoint asin(VFixedPoint a)
                {
                    return Pi / 2 - acos(a);
                }

                /// <summary>
                /// 一个自定义的Sin函数，对应Excel中针对Sin的计算公式
                /// </summary>
                /// <param name="a"></param>
                /// <returns></returns>
                public static VFixedPoint SinSelfDefine(VFixedPoint a)
                {
                    if (a >= Pi)
                    {
                        return VFixedPoint.Create(2);
                    }
                    else if (a > Pi*VFixedPoint.Half)
                    {
                        return VFixedPoint.Create(2) + Sin(a + Pi);
                    }
                    else if (a > VFixedPoint.Zero)
                    {
                        return Sin(a);
                    }
                    else
                    {
                        return VFixedPoint.Zero;
                    }
                }

                /// <summary>
                /// 一个自定的ASin函数，对应Excel中针对Sin的计算公式
                /// </summary>
                /// <param name="a"></param>
                /// <returns></returns>
                public static VFixedPoint AsinSelfDefine(VFixedPoint a)
                {
                    if (a >= VFixedPoint.Create(2))
                    {
                        return Pi;
                    }
                    else if (a > VFixedPoint.One)
                    {
                        return Pi + asin(a - VFixedPoint.Create(2));
                    }
                    else if (a > VFixedPoint.Zero)
                    {
                        return asin(a);
                    }
                    else
                    {
                        return VFixedPoint.Zero;
                    }
                }

                public static VFixedPoint atan2(VFixedPoint y, VFixedPoint x)
                {
                    int num;
                    int num2;
                    if (x < VFixedPoint.Zero)
                    {
                        if (y < VFixedPoint.Zero)
                        {
                            x = -x;
                            y = -y;
                            num2 = 1;
                        }
                        else
                        {
                            x = -x;
                            num2 = -1;
                        }
                        num = -31416;
                    }
                    else
                    {
                        if (y < VFixedPoint.Zero)
                        {
                            y = -y;
                            num2 = -1;
                        }
                        else
                        {
                            num2 = 1;
                        }
                        num = 0;
                    }
                    int dIM = Atan2LookupTable.DIM;
                    long num4 = dIM - 1;
                    long b = (x >= y) ? x.ValueBar : y.ValueBar;
                    int num6 = (int)(x.ValueBar * num4 / b);
                    int num7 = (int)(y.ValueBar * num4 / b);
                    int num8 = Atan2LookupTable.table[(num7 * dIM) + num6];
                    return VFixedPoint.Create((num8 + num) * num2) / VFixedPoint.Create(0x2710L);
                }

                
            }
        }
    }
}