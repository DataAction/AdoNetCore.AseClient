using System;
using System.Numerics;
using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient
{
    /// <summary>
    /// AseDecimal represents a decimal or numeric number with a maximum precision of 38. AseDecimal implements IComparable interface.
    /// </summary>
    public struct AseDecimal : IComparable
    {
        // ReSharper disable once InconsistentNaming
        /// <summary>
        /// Maximum allowed precision is 38.
        /// </summary>
        public static readonly byte MAXPRECISION = 38;

        // ReSharper disable once InconsistentNaming
        /// <summary>
        /// Maximum allowed scale is 38.
        /// </summary>
        public static readonly byte MAXSCALE = MAXPRECISION;

        /// <summary>
        /// Maximum DataLength is 33 bytes.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public static readonly byte MAXLENGTH = 33;

        /// <summary>
        /// Maximum output precision is 77.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public static readonly byte MAXOUTPUTPRECISION = 77;

        /// <summary>
        /// Maximum output scale is 77.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public static readonly byte MAXOUTPUTSCALE = 77;

        /// <summary>
        /// Minimum value is -99999999999999999999999999999999999999
        /// </summary>
        public static readonly AseDecimal MinValue = Parse("-99999999999999999999999999999999999999");

        /// <summary>
        /// Maximum value is 99999999999999999999999999999999999999
        /// </summary>
        public static readonly AseDecimal MaxValue = Parse("99999999999999999999999999999999999999");

        /// <summary>
        /// Zero is 0
        /// </summary>
        public static readonly AseDecimal Zero = new AseDecimal(0m);

        private BigDecimal _backing;

        /// <summary>
        /// Initializes a new instance of the AseDecimal structure.
        /// </summary>
        /// <param name="precision">An Int32 value of the target precision.</param>
        /// <param name="scale">An Int32 value of the target scale.</param>
        public AseDecimal(int precision, int scale)
        {
            _backing = new BigDecimal
            {
                Exponent = scale
            };
        }

        /// <summary>
        /// Initializes a new instance of the AseDecimal structure.
        /// </summary>
        /// <param name="aseDecimal">An AseDecimal structure to initialize the value of the new AseDecimal.</param>
        public AseDecimal(AseDecimal aseDecimal)
        {
            _backing = new BigDecimal(new BigInteger(aseDecimal._backing.Mantissa.ToByteArray()), aseDecimal._backing.Exponent);
        }

        /// <summary>
        /// Initializes a new instance of the AseDecimal structure.
        /// </summary>
        /// <param name="precision">An Int32 value of the target precision.</param>
        /// <param name="scale">An Int32 value of the target scale.</param>
        /// <param name="value">The target value array in bytes.</param>
        public AseDecimal(int precision, int scale, byte[] value)
        {
            _backing = new BigDecimal(new BigInteger(value), scale);
        }

        private AseDecimal(BigDecimal backing)
        {
            _backing = backing;
        }

        /// <summary>
        /// Initializes a new instance of the AseDecimal structure.
        /// </summary>
        /// <param name="value">A decimal structure to initialize the value of the new AseDecimal.</param>
        public AseDecimal(decimal value)
        {
            // Adapted from https://stackoverflow.com/a/764102/242311
            var bits = decimal.GetBits(value);
            uint bits0, bits1, bits2, bits3;
            unchecked
            {

                bits0 = (uint)bits[0];
                bits1 = (uint)bits[1];
                bits2 = (uint)bits[2];
                bits3 = (uint)bits[3];
            }

            var decimalPlaces = (bits3 >> 16) & 31;
            var isNegative = (bits3 & 0x80000000) == 0x80000000;

            var mantissa = (bits2 * 4294967296m * 4294967296m) +
                           (bits1 * 4294967296m) +
                           bits0;

            _backing = new BigDecimal(new BigInteger(isNegative ? -1 * mantissa : mantissa), (int)decimalPlaces);
        }

        /// <summary>
        /// Checks the sign of the AseDecimal value.
        /// </summary>
        /// <param name="value">The AseDecimal structure whose design is being checked.</param>
        /// <returns>Returns an integer 0 for null, 1 for positive, and -1 for a negative values respectively.</returns>
        public static int Sign(AseDecimal value)
        {
            return value.IsNull
                ? 0
                : value.IsNegative
                    ? -1
                    : 1;
        }

        /// <summary>
        /// Returns a value indicating whether this instance of AseDecimal and the value indicated by the object value are equal.
        /// </summary>
        /// <param name="value">The object being tested for equality.</param>
        public override bool Equals(object value)
        {
            return _backing.Equals(value);
        }

        /// <summary>
        /// Returns a value indicating whether two instances of AseDecimal are equal.
        /// </summary>
        /// <returns>True if the values are equal.</returns>
        public static bool operator ==(AseDecimal a, AseDecimal b)
        {
            return a._backing == b._backing;
        }

        public static bool operator !=(AseDecimal a, AseDecimal b)
        {
            return a._backing != b._backing;
        }

        /// <summary>
        /// Returns a value indicating if a is less than or equal to b.
        /// </summary>
        /// <returns>True if a is less than or equal to b.</returns>
        public static bool operator <=(AseDecimal a, AseDecimal b)
        {
            return a._backing <= b._backing;
        }

        /// <summary>
        /// Returns a value indicating if a is greater than or equal to b.
        /// </summary>
        /// <returns>True if a is greater than or equal to b.</returns>
        public static bool operator >=(AseDecimal a, AseDecimal b)
        {
            return a._backing >= b._backing;
        }

        /// <summary>
        /// Returns a value indicating if a is less than b.
        /// </summary>
        /// <returns>True if a is less than b.</returns>
        public static bool operator <(AseDecimal a, AseDecimal b)
        {
            return a._backing < b._backing;
        }

        /// <summary>
        /// Returns a value indicating if a is greater than b.
        /// </summary>
        /// <returns>True if a is greater than b.</returns>
        public static bool operator >(AseDecimal a, AseDecimal b)
        {
            return a._backing > b._backing;
        }

        /// <summary>
        /// Parses a string value to an AseDecimal value.
        /// </summary>
        /// <param name="s">The string to be parsed into an AseDecimal value.</param>
        /// <returns>An AseDecimal structure representing the parsed string.</returns>
        public static AseDecimal Parse(string s)
        {
            var dpIndex = s.LastIndexOf('.');
            var scale = dpIndex < s.Length ? s.Length - dpIndex : 0;
            return new AseDecimal
            {
                _backing = new BigDecimal(BigInteger.Parse(s.Replace(".", string.Empty)), scale)
            };
        }

        public static bool TryParse(string s, out AseDecimal result)
        {
            try
            {
                result = Parse(s);
                return true;
            }
            catch
            {
                result = new AseDecimal();
                return false;
            }
        }

        /// <summary>
        /// Returns a hashcode.
        /// </summary>
        /// <returns>An integer value representing the hashcode.</returns>
        public override int GetHashCode()
        {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return _backing.GetHashCode();
        }

        /// <summary>
        /// Compares the value of this AseDecimal with the value of Object.
        /// </summary>
        public int CompareTo(object value)
        {
            return _backing.CompareTo(value);
        }

        /// <summary>
        /// Compares the value of this AseDecimal with the AseDecimal value.
        /// </summary>
        public int CompareTo(AseDecimal value)
        {
            return _backing.CompareTo(value._backing);
        }

        public int Precision
        {
            get
            {
                if (_backing == 0)
                {
                    return 1;
                }

                //return (int)Math.Floor(BigInteger.Log10(BigInteger.Abs(_backing.Mantissa)) + 1);
                return (int)Math.Ceiling(BigInteger.Log10(BigInteger.Abs(_backing.Mantissa)));
            }
        }

        public int Scale => _backing.Exponent;

        /// <summary>
        /// Returns true if this AseDecimal is a null value.
        /// </summary>
        public bool IsNull { get { throw new NotImplementedException(); } }

        /// <summary>
        /// Returns true if this AseDecimal is a negative value.
        /// </summary>
        public bool IsNegative => _backing.Mantissa.Sign == -1;

        /// <summary>
        /// Returns true if this AseDecimal is a positive value.
        /// </summary>
        public bool IsPositive => !IsNegative;

        /// <summary>
        /// Converts this AseDecimal to new AseDecimal with specified precision and scale.
        /// </summary>
        /// <param name="outputPrecision">The target precision for the output</param>
        /// <param name="outputScale">The target scale for the output.</param>
        /// <returns>An AseDecimal structure with the specified precision and scale.</returns>
        public AseDecimal ToAseDecimal(int outputPrecision, int outputScale) { throw new NotImplementedException(); }

        /// <summary>
        /// Returns a string representation of this AseDecimal.
        /// </summary>
        /// <returns>A string representation of this AseDecimal.</returns>
        public override string ToString()
        {
            return _backing.Mantissa.ToString();
        }

        public sbyte ToSByte()
        {
            return (sbyte) _backing.Mantissa;
        }

        public byte ToByte() { return (byte)_backing.Mantissa; }

        public short ToInt16() { return (short)_backing.Mantissa; }

        public ushort ToUInt16() { return (ushort)_backing.Mantissa; }

        public uint ToUInt32() { return (uint)_backing.Mantissa; }

        public ulong ToUInt64() { return (ulong)_backing.Mantissa; }

        public int ToInt32() { return (int)_backing.Mantissa; }

        public long ToInt64() { return (long)_backing.Mantissa; }

        public float ToSingle()
        {
            return Convert.ToSingle(ToDouble());
        }

        public double ToDouble()
        {
            if (Scale == 0)
            {
                return (double)_backing.Mantissa;
            }

            return (double) _backing.Mantissa / Math.Pow(1, Scale);
        }

        public static AseDecimal Floor(AseDecimal n)
        {
            return new AseDecimal(n._backing.Floor());
        }

        public static AseDecimal Truncate(AseDecimal n, int position)
        {
            return new AseDecimal(n._backing.Truncate(position));
        }

        public static AseDecimal Round(AseDecimal n, int position)
        {
            throw new NotImplementedException();
        }
    }
}
