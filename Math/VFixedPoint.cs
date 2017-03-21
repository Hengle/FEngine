using System.Collections;

namespace MobaGame
{
    namespace FixedMath
    {
        [System.Serializable]
        public struct VFixedPoint
        {
            public static readonly int SHIFT_AMOUNT = 16;
            public static readonly VFixedPoint Zero = Create(0);
            public static readonly VFixedPoint One = Create(1);
			public static readonly VFixedPoint Two = Create(2);
            public static readonly VFixedPoint Half = new VFixedPoint(One.ValueBar/2);
            public static readonly VFixedPoint MaxValue = new VFixedPoint(long.MaxValue);
            public static readonly VFixedPoint MinValue = new VFixedPoint(long.MinValue);

            internal long ValueBar;

            public float Value
            {
                get
                {
                    return ValueBar * 1f / (1 << SHIFT_AMOUNT);
                }

                set
                {
                    ValueBar = ((long)(value * (1 << SHIFT_AMOUNT)));
                }
            }


            internal VFixedPoint(long Value)
            {
                ValueBar = Value;
            }

            public static VFixedPoint Create(long value)
            {
                return new VFixedPoint(value << SHIFT_AMOUNT);
            }

            public static VFixedPoint Create(float value)
            {
                return new VFixedPoint((long)(value * (1 << SHIFT_AMOUNT)));
            }

            public long ToInt
            {
                get
                {
                    return ValueBar >> SHIFT_AMOUNT;
                }
            }

            public float ToFloat
            {
                get
                {
                    return ValueBar * 1f / (1 << SHIFT_AMOUNT);
                }
            }

            public long ToBinary()
            {
                return ValueBar;
            }

            public static VFixedPoint FromBinary(long binary)
            {
                return new VFixedPoint(binary);
            }

            public static bool operator <(VFixedPoint a, VFixedPoint b)
            {
                return a.ValueBar < b.ValueBar;
            }

            public static bool operator >(VFixedPoint a, VFixedPoint b)
            {
                return a.ValueBar > b.ValueBar;
            }

            public static bool operator <=(VFixedPoint a, VFixedPoint b)
            {
                return a.ValueBar <= b.ValueBar;
            }

            public static bool operator >=(VFixedPoint a, VFixedPoint b)
            {
                return a.ValueBar >= b.ValueBar;
            }

            public static bool operator ==(VFixedPoint a, VFixedPoint b)
            {
                return a.ValueBar == b.ValueBar;
            }

            public static bool operator !=(VFixedPoint a, VFixedPoint b)
            {
                return a.ValueBar != b.ValueBar;
            }

            public static VFixedPoint operator +(VFixedPoint a, VFixedPoint b)
            {
                return new VFixedPoint(a.ValueBar + b.ValueBar);
            }


            public static VFixedPoint operator -(VFixedPoint a, VFixedPoint b)
            {
                return new VFixedPoint(a.ValueBar - b.ValueBar);
            }

            public static VFixedPoint operator -(VFixedPoint lhs)
            {
                return new VFixedPoint(-lhs.ValueBar);
            }

            public static VFixedPoint operator *(VFixedPoint a, VFixedPoint b)
            {
                return new VFixedPoint((a.ValueBar * b.ValueBar) >> SHIFT_AMOUNT);
            }

            public static VFixedPoint operator *(VFixedPoint a, long b)
            {
                return new VFixedPoint(a.ValueBar * b);
            }

            public static VFixedPoint operator /(VFixedPoint a, VFixedPoint b)
            {
                if (b.ValueBar == 0)
                    throw new FMathException(FMathException.REASON.DIVIDE_ZERO);
                return new VFixedPoint((a.ValueBar << SHIFT_AMOUNT) / b.ValueBar);
            }

            public static VFixedPoint operator /(VFixedPoint a, long b)
            {
                if (b == 0)
                    throw new FMathException(FMathException.REASON.DIVIDE_ZERO);
                return new VFixedPoint(a.ValueBar / b);
            }

            public override bool Equals(object obj)
            {
                return (((obj != null) && (base.GetType() == obj.GetType())) && (this == ((VFixedPoint)obj)));
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public VFixedPoint Abs()
            {
                return ValueBar > 0 ? this : new VFixedPoint(-1 * ValueBar);
            }

            public VFixedPoint Round()
            {
                return Create(((ValueBar + Half.ValueBar - 1) >> SHIFT_AMOUNT));
            }

            public VFixedPoint Ceil()
            {
                return Create(((ValueBar + One.ValueBar - 1) >> SHIFT_AMOUNT));
            }

            public VFixedPoint Floor()
            {
                return Create(((ValueBar) >> SHIFT_AMOUNT));
            }

            public int Sign()
            {
                if (ValueBar > 0)
                    return 1;
                else if (ValueBar == 0)
                    return 0;
                else
                    return -1;
            }

            public override string ToString()
            {
                return ToFloat.ToString();
            }

            public static bool TryParse(string s, out VFixedPoint result)
            {
                string[] NewValues = s.Split('.');
                if (NewValues.Length <= 2)
                {
                    long Whole;
                    if (long.TryParse(NewValues[0], out Whole))
                    {
                        if (NewValues.Length == 1)
                        {
                            result =  Create(Whole);
                            return true;
                        }
                        else
                        {
                            long Numerator;
                            if (long.TryParse(NewValues[1], out Numerator))
                            {
                                int fractionDigits = NewValues[1].Length;
                                long Denominator = 1;
                                for (int i = 0; i < fractionDigits; i++)
                                {
                                    Denominator *= 10;
                                }
                                result = Create(Whole) + Create(Numerator) /Create(Denominator);
                                return true;
                            }
                        }
                    }
                }
                result = Zero;
                return false;
            }
        }
    }

}

