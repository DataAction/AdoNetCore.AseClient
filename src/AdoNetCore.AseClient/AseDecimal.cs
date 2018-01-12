using System;
using System.Diagnostics;
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

        internal BigDecimal Backing;

        /// <summary>
        /// Initializes a new instance of the AseDecimal structure.
        /// </summary>
        /// <param name="precision">An Int32 value of the target precision.</param>
        /// <param name="scale">An Int32 value of the target scale.</param>
        public AseDecimal(int precision, int scale) : this(new BigDecimal(BigInteger.Zero, -scale)) { }

        /// <summary>
        /// Initializes a new instance of the AseDecimal structure.
        /// </summary>
        /// <param name="aseDecimal">An AseDecimal structure to initialize the value of the new AseDecimal.</param>
        public AseDecimal(AseDecimal aseDecimal) : this(new BigDecimal(new BigInteger(aseDecimal.Backing.Mantissa.ToByteArray()), aseDecimal.Backing.Exponent)) { }

        /// <summary>
        /// Initializes a new instance of the AseDecimal structure.
        /// </summary>
        /// <param name="precision">An Int32 value of the target precision.</param>
        /// <param name="scale">An Int32 value of the target scale.</param>
        /// <param name="value">The target value array in bytes.</param>
        public AseDecimal(int precision, int scale, byte[] value)
        {
            Debug.Assert(null != value);
            Debug.Assert(value.Length > 0);

            var data = new byte[value.Length - 1];
            Array.Copy(value, 1, data, 0, data.Length);
            var bigInt = new BigInteger(data);

            Backing = value[0] != 0 //ase uses sign byte + big integer bytes, we need to translate to + or - bigInt
                ? new BigDecimal(-bigInt, -scale)
                : new BigDecimal(bigInt, -scale);
        }

        internal AseDecimal(int precision, int scale, bool isPositive, byte[] value)
        {
            Logger.Instance?.Write($"Construct AseDecimal from {HexDump.Dump(value)}");
            var bigInt = new BigInteger(value);
            Backing = isPositive
                ? new BigDecimal(bigInt, -scale)
                : new BigDecimal(-bigInt, -scale);
        }

        internal AseDecimal(BigDecimal backing)
        {
            Backing = backing;
        }

        /// <summary>
        /// Initializes a new instance of the AseDecimal structure.
        /// </summary>
        /// <param name="value">A decimal structure to initialize the value of the new AseDecimal.</param>
        public AseDecimal(decimal value)
        {
            Backing = value;
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
            if (ReferenceEquals(null, value))
            {
                return false;
            }

            return value is AseDecimal aseDecimal &&
                   Backing.Equals(aseDecimal.Backing);
        }

        internal byte[] BinData
        {
            get
            {
                var result = new byte[BytesRequired];
                var mantissaBytes = (Backing.Mantissa * Backing.Mantissa.Sign).ToByteArray();

                Logger.Instance?.WriteLine($"Exponent: {Backing.Exponent}, Mantissa: {Backing.Mantissa}");
                Logger.Instance?.WriteLine($"Expecting {BytesRequired} bytes; Mantissa is {mantissaBytes.Length} bytes");
                Logger.Instance?.WriteLine($"Mantissa bytes:");
                Logger.Instance?.Write(HexDump.Dump(mantissaBytes));

                Array.Copy(mantissaBytes, 0, result, 0, Math.Min(result.Length, mantissaBytes.Length));
                Logger.Instance?.WriteLine($"Result bytes:");
                Logger.Instance?.Write(HexDump.Dump(result));
                return result;
            }
        }

        private static readonly double Log256 = Math.Log10(256);

        /// <summary>
        /// get the number of bytes required to represent the value (caller should add 1 byte to represent sign byte)
        /// </summary>
        public int BytesRequired
        {
            get
            {
                if (IsNull)
                    throw new NullReferenceException();
                return Convert.ToInt32(Math.Ceiling(Precision / Log256));
            }
        }

        /// <summary>
        /// Returns a value indicating whether two instances of AseDecimal are equal.
        /// </summary>
        /// <returns>True if the values are equal.</returns>
        public static bool operator ==(AseDecimal a, AseDecimal b)
        {
            return a.Backing == b.Backing;
        }

        public static bool operator !=(AseDecimal a, AseDecimal b)
        {
            return a.Backing != b.Backing;
        }

        /// <summary>
        /// Returns a value indicating if a is less than or equal to b.
        /// </summary>
        /// <returns>True if a is less than or equal to b.</returns>
        public static bool operator <=(AseDecimal a, AseDecimal b)
        {
            return a.Backing <= b.Backing;
        }

        /// <summary>
        /// Returns a value indicating if a is greater than or equal to b.
        /// </summary>
        /// <returns>True if a is greater than or equal to b.</returns>
        public static bool operator >=(AseDecimal a, AseDecimal b)
        {
            return a.Backing >= b.Backing;
        }

        /// <summary>
        /// Returns a value indicating if a is less than b.
        /// </summary>
        /// <returns>True if a is less than b.</returns>
        public static bool operator <(AseDecimal a, AseDecimal b)
        {
            return a.Backing < b.Backing;
        }

        /// <summary>
        /// Returns a value indicating if a is greater than b.
        /// </summary>
        /// <returns>True if a is greater than b.</returns>
        public static bool operator >(AseDecimal a, AseDecimal b)
        {
            return a.Backing > b.Backing;
        }

        /// <summary>
        /// Parses a string value to an AseDecimal value.
        /// </summary>
        /// <param name="s">The string to be parsed into an AseDecimal value.</param>
        /// <returns>An AseDecimal structure representing the parsed string.</returns>
        public static AseDecimal Parse(string s)
        {
            var dpIndex = s.LastIndexOf('.');
            var scale = dpIndex >= 0 && dpIndex < s.Length
                ? s.Length - dpIndex
                : 0;
            return new AseDecimal(new BigDecimal(BigInteger.Parse(s.Replace(".", string.Empty)), -scale));
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
            return Backing.GetHashCode();
        }

        /// <summary>
        /// Compares the value of this AseDecimal with the value of Object.
        /// </summary>
        public int CompareTo(object value)
        {
            return Backing.CompareTo(value);
        }

        /// <summary>
        /// Compares the value of this AseDecimal with the AseDecimal value.
        /// </summary>
        public int CompareTo(AseDecimal value)
        {
            return Backing.CompareTo(value.Backing);
        }

        public int Precision => Backing.NumberOfDigits();

        public int Scale => Math.Abs(Backing.Exponent);

        /// <summary>
        /// Returns true if this AseDecimal is a null value.
        /// </summary>
        public bool IsNull => false;

        /// <summary>
        /// Returns true if this AseDecimal is a negative value.
        /// </summary>
        public bool IsNegative => Backing.Mantissa.Sign == -1;

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
            return Backing.ToString();
        }

        public sbyte ToSByte()
        {
            return (sbyte)Backing;
        }

        public byte ToByte()
        {
            return (byte)Backing;
        }

        public short ToInt16()
        {
            return (short)Backing;
        }

        public ushort ToUInt16()
        {
            return (ushort)Backing;
        }

        public uint ToUInt32()
        {
            return (uint)Backing;
        }

        public ulong ToUInt64()
        {
            return (ulong)Backing;
        }

        public int ToInt32()
        {
            return (int)Backing;
        }

        public long ToInt64()
        {
            return (long)Backing;
        }

        public float ToSingle()
        {
            return (float)Backing;
        }

        public double ToDouble()
        {
            return (double)Backing;
        }

        public decimal ToDecimal()
        {
            return (decimal)Backing;
        }

        public static AseDecimal Floor(AseDecimal n)
        {
            return new AseDecimal(n.Backing.Floor());
        }

        public static AseDecimal Truncate(AseDecimal n, int position)
        {
            return new AseDecimal(n.Backing.Truncate(position));
        }

        public static AseDecimal Round(AseDecimal n, int position)
        {
            throw new NotImplementedException();
        }
    }
}
