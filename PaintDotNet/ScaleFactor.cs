namespace PaintDotNet
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct ScaleFactor
    {
        private int denominator;
        private int numerator;
        public static readonly ScaleFactor OneToOne;
        public static readonly ScaleFactor MinValue;
        public static readonly ScaleFactor MaxValue;
        private static string percentageFormat;
        private static readonly double[] scales;
        public int Denominator =>
            this.denominator;
        public int Numerator =>
            this.numerator;
        public double Ratio =>
            (((double) this.numerator) / ((double) this.denominator));
        private void Clamp()
        {
            if (this < MinValue)
            {
                this = MinValue;
            }
            else if (this > MaxValue)
            {
                this = MaxValue;
            }
        }

        public static ScaleFactor UseIfValid(int numerator, int denominator, ScaleFactor lastResort)
        {
            if ((numerator > 0) && (denominator > 0))
            {
                return new ScaleFactor(numerator, denominator);
            }
            return lastResort;
        }

        public static ScaleFactor Min(int n1, int d1, int n2, int d2, ScaleFactor lastResort)
        {
            ScaleFactor lhs = UseIfValid(n1, d1, lastResort);
            ScaleFactor rhs = UseIfValid(n2, d2, lastResort);
            return Min(lhs, rhs);
        }

        public static ScaleFactor Max(int n1, int d1, int n2, int d2, ScaleFactor lastResort)
        {
            ScaleFactor lhs = UseIfValid(n1, d1, lastResort);
            ScaleFactor rhs = UseIfValid(n2, d2, lastResort);
            return Max(lhs, rhs);
        }

        public static ScaleFactor Min(ScaleFactor lhs, ScaleFactor rhs)
        {
            if (lhs < rhs)
            {
                return lhs;
            }
            return rhs;
        }

        public static ScaleFactor Max(ScaleFactor lhs, ScaleFactor rhs)
        {
            if (lhs > rhs)
            {
                return lhs;
            }
            return rhs;
        }

        public static bool operator ==(ScaleFactor lhs, ScaleFactor rhs) => 
            ((lhs.numerator * rhs.denominator) == (rhs.numerator * lhs.denominator));

        public static bool operator !=(ScaleFactor lhs, ScaleFactor rhs) => 
            !(lhs == rhs);

        public static bool operator <(ScaleFactor lhs, ScaleFactor rhs) => 
            ((lhs.numerator * rhs.denominator) < (rhs.numerator * lhs.denominator));

        public static bool operator <=(ScaleFactor lhs, ScaleFactor rhs) => 
            ((lhs.numerator * rhs.denominator) <= (rhs.numerator * lhs.denominator));

        public static bool operator >(ScaleFactor lhs, ScaleFactor rhs) => 
            ((lhs.numerator * rhs.denominator) > (rhs.numerator * lhs.denominator));

        public static bool operator >=(ScaleFactor lhs, ScaleFactor rhs) => 
            ((lhs.numerator * rhs.denominator) >= (rhs.numerator * lhs.denominator));

        public override bool Equals(object obj)
        {
            if (obj is ScaleFactor)
            {
                ScaleFactor factor = (ScaleFactor) obj;
                return (this == factor);
            }
            return false;
        }

        public override int GetHashCode() => 
            HashCodeUtil.CombineHashCodes(this.numerator, this.denominator);

        public override string ToString()
        {
            try
            {
                return string.Format(percentageFormat, Math.Round((double) (100.0 * this.Ratio)));
            }
            catch (ArithmeticException)
            {
                return "--";
            }
        }

        public static double[] PresetValues
        {
            get
            {
                double[] array = new double[scales.Length];
                scales.CopyTo(array, 0);
                return array;
            }
        }
        public ScaleFactor GetNextLarger()
        {
            double ratio = this.Ratio + 0.005;
            int length = Array.FindIndex<double>(scales, scale => ratio <= scale);
            if (length == -1)
            {
                length = scales.Length;
            }
            length = Math.Min(length, scales.Length - 1);
            return FromDouble(scales[length]);
        }

        public ScaleFactor GetNextSmaller()
        {
            double ratio = this.Ratio - 0.005;
            int num = Array.FindIndex<double>(scales, scale => ratio <= scale) - 1;
            if (num == -1)
            {
                num = 0;
            }
            num = Math.Max(num, 0);
            return FromDouble(scales[num]);
        }

        private static ScaleFactor Reduce(int numerator, int denominator)
        {
            int num = 2;
            while ((num < denominator) && (num < numerator))
            {
                if (((numerator % num) == 0) && ((denominator % num) == 0))
                {
                    numerator /= num;
                    denominator /= num;
                }
                else
                {
                    num++;
                }
            }
            return new ScaleFactor(numerator, denominator);
        }

        public static ScaleFactor FromDouble(double scalar)
        {
            int numerator = (int) Math.Floor((double) (scalar * 1000.0));
            int denominator = 0x3e8;
            return Reduce(numerator, denominator);
        }

        public ScaleFactor(int numerator, int denominator)
        {
            if (denominator <= 0)
            {
                throw new ArgumentOutOfRangeException("denominator", "must be greater than 0");
            }
            if (numerator < 0)
            {
                throw new ArgumentOutOfRangeException("numerator", "must be greater than 0");
            }
            this.numerator = numerator;
            this.denominator = denominator;
            this.Clamp();
        }

        static ScaleFactor()
        {
            OneToOne = new ScaleFactor(1, 1);
            MinValue = new ScaleFactor(1, 100);
            MaxValue = new ScaleFactor(0x20, 1);
            percentageFormat = PdnResources.GetString2("ScaleFactor.Percentage.Format");
            scales = new double[] { 
                0.01, 0.02, 0.03, 0.04, 0.05, 0.06, 0.08, 0.12, 0.16, 0.25, 0.33, 0.5, 0.66, 1.0, 2.0, 3.0,
                4.0, 5.0, 6.0, 7.0, 8.0, 12.0, 16.0, 24.0, 32.0
            };
        }
    }
}

