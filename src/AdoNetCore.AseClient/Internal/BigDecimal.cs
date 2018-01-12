using System;
using System.Numerics;
using System.Text;

namespace AdoNetCore.AseClient.Internal
{
    /// <summary>
    /// Arbitrary precision decimal.
    /// All operations are exact, except for division. Division never determines more digits than the given precision.
    /// Source: https://gist.github.com/JcBernack/0b4eef59ca97ee931a2f45542b9ff06d
    /// Based on https://stackoverflow.com/a/4524254
    /// Author: Jan Christoph Bernack (contact: jc.bernack at gmail.com)
    /// </summary>
    public struct BigDecimal : IComparable, IComparable<BigDecimal>
    {
        /// <summary>
        /// Specifies whether the significant digits should be truncated to the given precision after each operation.
        /// </summary>
        public static bool AlwaysTruncate = false;

        /// <summary>
        /// Sets the maximum precision of division operations.
        /// If AlwaysTruncate is set to true all operations are affected.
        /// </summary>
        public static int MaxTruncationPrecision = 77;

        /// <summary>
        /// Underlying value, stored as a bigint.
        /// </summary>
        public BigInteger Mantissa { get; set; }

        /// <summary>
        /// Exponent associated with the underlying value.
        /// Should always be 0 or negative
        /// Used to translate the BigInteger back into something with decimal places using formula Mantissa * 10^Exponent.
        /// e.g. 9999 * 10^-3 => 9.999
        /// </summary>
        public int Exponent { get; set; }

        public BigDecimal(BigInteger mantissa, int exponent) : this()
        {
            Mantissa = mantissa;
            Exponent = exponent;
            Normalize();
            if (AlwaysTruncate)
            {
                Truncate();
            }
        }

        /// <summary>
        /// Removes trailing zeros on the mantissa
        /// </summary>
        public void Normalize()
        {
            //from comment
            if (Exponent == 0)
            {
                return;
            }

            if (Mantissa.IsZero)
            {
                Exponent = 0;
            }
            else
            {
                BigInteger remainder = 0;
                while (remainder == 0)
                {
                    var shortened = BigInteger.DivRem(Mantissa, 10, out remainder);
                    if (remainder == 0)
                    {
                        Mantissa = shortened;
                        Exponent++;
                    }
                }
            }
        }

        /// <summary>
        /// Truncate the number to the given precision by removing the least significant digits.
        /// </summary>
        /// <returns>The truncated number</returns>
        public BigDecimal Truncate(int precision)
        {
            // copy this instance (remember it's a struct)
            var shortened = this;
            // save some time because the number of digits is not needed to remove trailing zeros
            shortened.Normalize();
            // remove the least significant digits, as long as the number of digits is higher than the given Precision
            while (NumberOfMantissaDigits(shortened.Mantissa) > precision)
            {
                shortened.Mantissa /= 10;
                shortened.Exponent++;
            }
            // normalize again to make sure there are no trailing zeros left
            shortened.Normalize();
            return shortened;
        }

        public BigDecimal Truncate()
        {
            return Truncate(MaxTruncationPrecision);
        }

        public BigDecimal Floor()
        {
            return Truncate(NumberOfMantissaDigits(Mantissa) + Exponent);
        }

        private static int NumberOfMantissaDigits(BigInteger value)
        {
            //todo: fix. May need to do looping/binary search to figure out the value.
            //Could store a const readonly array of 10^n values (for n=0 to 78) and search based on that
            // do not count the sign
            return (value * value.Sign).ToString().Length;
            // faster version --these don't work for some large numbers due to double precision and rounding.
            // e.g. log10(9999999999999999999) should be 18.99999999999999999995657055180967481723271563569882323263 but ends up being 19, breaking calculations.
            //return (int)Math.Ceiling(BigInteger.Log10(value * value.Sign));
            //return (int) Math.Floor(BigInteger.Log10(value * value.Sign) + 1);
        }

        public int NumberOfDigits()
        {
            if (this == 0)
            {
                return 1;
            }

            var mantissaDigits = NumberOfMantissaDigits(Mantissa);
            mantissaDigits = mantissaDigits == 0
                ? 1
                : mantissaDigits;

            Logger.Instance?.WriteLine($"mantissaDigits: {mantissaDigits}; Exponent: {Exponent}");

            if (mantissaDigits <= Math.Abs(Exponent))
            {
                return Math.Abs(Exponent) + 1; // +1 because of the '0.'
            }

            return mantissaDigits;
        }

        #region Conversions

        public static implicit operator BigDecimal(int value)
        {
            return new BigDecimal(value, 0);
        }

        public static implicit operator BigDecimal(double value)
        {
            var mantissa = (BigInteger)value;
            var exponent = 0;
            double scaleFactor = 1;
            while (Math.Abs(value * scaleFactor - (double)mantissa) > 0)
            {
                exponent -= 1;
                scaleFactor *= 10;
                mantissa = (BigInteger)(value * scaleFactor);
            }
            return new BigDecimal(mantissa, exponent);
        }

        public static implicit operator BigDecimal(decimal value)
        {
            var mantissa = (BigInteger)value;
            var exponent = 0;
            decimal scaleFactor = 1;
            while ((decimal)mantissa != value * scaleFactor)
            {
                exponent -= 1;
                scaleFactor *= 10;
                mantissa = (BigInteger)(value * scaleFactor);
            }
            return new BigDecimal(mantissa, exponent);
        }

        public static explicit operator double(BigDecimal value)
        {
            return (double)value.Mantissa * Math.Pow(10, value.Exponent);
        }

        public static explicit operator float(BigDecimal value)
        {
            return Convert.ToSingle((double)value);
        }

        public static explicit operator decimal(BigDecimal value)
        {
            return (decimal)value.Mantissa * (decimal)Math.Pow(10, value.Exponent);
        }

        public static explicit operator int(BigDecimal value)
        {
            return (int)(value.Mantissa * BigInteger.Pow(10, value.Exponent));
        }

        public static explicit operator uint(BigDecimal value)
        {
            return (uint)(value.Mantissa * BigInteger.Pow(10, value.Exponent));
        }

        public static explicit operator long(BigDecimal value)
        {
            return (long)(value.Mantissa * BigInteger.Pow(10, value.Exponent));
        }

        public static explicit operator ulong(BigDecimal value)
        {
            return (ulong)(value.Mantissa * BigInteger.Pow(10, value.Exponent));
        }

        public static explicit operator short(BigDecimal value)
        {
            return (short)(value.Mantissa * BigInteger.Pow(10, value.Exponent));
        }

        public static explicit operator ushort(BigDecimal value)
        {
            return (ushort)(value.Mantissa * BigInteger.Pow(10, value.Exponent));
        }

        public static explicit operator sbyte(BigDecimal value)
        {
            return (sbyte)(value.Mantissa * BigInteger.Pow(10, value.Exponent));
        }

        public static explicit operator byte(BigDecimal value)
        {
            return (byte)(value.Mantissa * BigInteger.Pow(10, value.Exponent));
        }

        #endregion

        #region Operators

        public static BigDecimal operator +(BigDecimal value)
        {
            return value;
        }

        public static BigDecimal operator -(BigDecimal value)
        {
            value.Mantissa *= -1;
            return value;
        }

        public static BigDecimal operator ++(BigDecimal value)
        {
            return value + 1;
        }

        public static BigDecimal operator --(BigDecimal value)
        {
            return value - 1;
        }

        public static BigDecimal operator +(BigDecimal left, BigDecimal right)
        {
            return Add(left, right);
        }

        public static BigDecimal operator -(BigDecimal left, BigDecimal right)
        {
            return Add(left, -right);
        }

        private static BigDecimal Add(BigDecimal left, BigDecimal right)
        {
            return left.Exponent > right.Exponent
                ? new BigDecimal(AlignExponent(left, right) + right.Mantissa, right.Exponent)
                : new BigDecimal(AlignExponent(right, left) + left.Mantissa, left.Exponent);
        }

        public static BigDecimal operator *(BigDecimal left, BigDecimal right)
        {
            return new BigDecimal(left.Mantissa * right.Mantissa, left.Exponent + right.Exponent);
        }

        public static BigDecimal operator /(BigDecimal dividend, BigDecimal divisor)
        {
            var exponentChange = MaxTruncationPrecision - (NumberOfMantissaDigits(dividend.Mantissa) - NumberOfMantissaDigits(divisor.Mantissa));
            if (exponentChange < 0)
            {
                exponentChange = 0;
            }
            dividend.Mantissa *= BigInteger.Pow(10, exponentChange);
            return new BigDecimal(dividend.Mantissa / divisor.Mantissa, dividend.Exponent - divisor.Exponent - exponentChange);
        }

        public static BigDecimal operator %(BigDecimal left, BigDecimal right)
        {
            return left - right * (left / right).Floor();
        }

        public static bool operator ==(BigDecimal left, BigDecimal right)
        {
            return left.Exponent == right.Exponent && left.Mantissa == right.Mantissa;
        }

        public static bool operator !=(BigDecimal left, BigDecimal right)
        {
            return left.Exponent != right.Exponent || left.Mantissa != right.Mantissa;
        }

        public static bool operator <(BigDecimal left, BigDecimal right)
        {
            return left.Exponent > right.Exponent ? AlignExponent(left, right) < right.Mantissa : left.Mantissa < AlignExponent(right, left);
        }

        public static bool operator >(BigDecimal left, BigDecimal right)
        {
            return left.Exponent > right.Exponent ? AlignExponent(left, right) > right.Mantissa : left.Mantissa > AlignExponent(right, left);
        }

        public static bool operator <=(BigDecimal left, BigDecimal right)
        {
            return left.Exponent > right.Exponent ? AlignExponent(left, right) <= right.Mantissa : left.Mantissa <= AlignExponent(right, left);
        }

        public static bool operator >=(BigDecimal left, BigDecimal right)
        {
            return left.Exponent > right.Exponent ? AlignExponent(left, right) >= right.Mantissa : left.Mantissa >= AlignExponent(right, left);
        }

        /// <summary>
        /// Returns the mantissa of value, aligned to the exponent of reference.
        /// Assumes the exponent of value is larger than of reference.
        /// </summary>
        private static BigInteger AlignExponent(BigDecimal value, BigDecimal reference)
        {
            return value.Mantissa * BigInteger.Pow(10, value.Exponent - reference.Exponent);
        }

        #endregion

        #region Additional mathematical functions

        public static BigDecimal Exp(double exponent)
        {
            var tmp = (BigDecimal)1;
            while (Math.Abs(exponent) > 100)
            {
                var diff = exponent > 0 ? 100 : -100;
                tmp *= Math.Exp(diff);
                exponent -= diff;
            }
            return tmp * Math.Exp(exponent);
        }

        public static BigDecimal Pow(double basis, double exponent)
        {
            var tmp = (BigDecimal)1;
            while (Math.Abs(exponent) > 100)
            {
                var diff = exponent > 0 ? 100 : -100;
                tmp *= Math.Pow(basis, diff);
                exponent -= diff;
            }
            return tmp * Math.Pow(basis, exponent);
        }

        #endregion

        public override string ToString()
        {
            var sb = new StringBuilder((Mantissa * Mantissa.Sign).ToString());

            PadIfNecessary(sb);
            InsertDecimalPoint(sb);
            InsertSign(sb);

            return sb.ToString();
        }

        private void PadIfNecessary(StringBuilder sb)
        {
            Logger.Instance?.WriteLine($"{nameof(PadIfNecessary)}({sb})");
            var expectedDigits = NumberOfDigits();
            var mantissaDigits = sb.Length;
            var paddingRequired = expectedDigits - mantissaDigits;

            Logger.Instance?.WriteLine($"expected digits: {expectedDigits}, mantissa digits: {mantissaDigits}, paddingRequired: {paddingRequired}, total digits: {NumberOfDigits()}");

            if (paddingRequired > 0)
            {
                sb.Insert(0, new string('0', paddingRequired));
            }
        }

        /// <summary>
        /// Once the string is of the correct length (padded with 0s if necessary), then insert the decimal point
        /// </summary>
        private void InsertDecimalPoint(StringBuilder sb)
        {
            Logger.Instance?.WriteLine($"{nameof(InsertDecimalPoint)}({sb})");
            if (Exponent < 0)
            {
                var targetIndex = sb.Length + Exponent;
                if (targetIndex == 0)
                {
                    sb.Insert(targetIndex, "0.");
                }
                else
                {
                    sb.Insert(targetIndex, '.');
                }
            }
        }

        private void InsertSign(StringBuilder sb)
        {
            Logger.Instance?.WriteLine($"{nameof(InsertSign)}({sb})");
            if (Mantissa.Sign < 0)
            {
                sb.Insert(0, '-');
            }
        }

        public bool Equals(BigDecimal other)
        {
            return other.Mantissa.Equals(Mantissa) && other.Exponent == Exponent;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is BigDecimal bigDecimal &&
                   Equals(bigDecimal);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                // ReSharper disable NonReadonlyMemberInGetHashCode
                return (Mantissa.GetHashCode() * 397) ^ Exponent;
                // ReSharper restore NonReadonlyMemberInGetHashCode
            }
        }

        public int CompareTo(object obj)
        {
            if (ReferenceEquals(obj, null) || !(obj is BigDecimal))
            {
                throw new ArgumentException();
            }
            return CompareTo((BigDecimal)obj);
        }

        public int CompareTo(BigDecimal other)
        {
            return this < other ? -1 : (this > other ? 1 : 0);
        }
    }
}
