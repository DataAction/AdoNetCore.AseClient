using System;

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

        /// <summary>
        /// Initializes a new instance of the AseDecimal structure.
        /// </summary>
        /// <param name="precision">An Int32 value of the target precision.</param>
        /// <param name="scale">An Int32 value of the target scale.</param>
        public AseDecimal(int precision, int scale)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Initializes a new instance of the AseDecimal structure.
        /// </summary>
        /// <param name="aseDecimal">An AseDecimal structure to initialize the value of the new AseDecimal.</param>
        public AseDecimal(AseDecimal aseDecimal)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Initializes a new instance of the AseDecimal structure.
        /// </summary>
        /// <param name="precision">An Int32 value of the target precision.</param>
        /// <param name="scale">An Int32 value of the target scale.</param>
        /// <param name="value">The target value array in bytes.</param>
        public AseDecimal(int precision, int scale, byte[] value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Initializes a new instance of the AseDecimal structure.
        /// </summary>
        /// <param name="value">A decimal structure to initialize the value of the new AseDecimal.</param>
        public AseDecimal(decimal value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks the sign of the AseDecimal value.
        /// </summary>
        /// <param name="value">The AseDecimal structure whose design is being checked.</param>
        /// <returns>Returns an integer 0 for null, 1 for positive, and -1 for a negative values respectively.</returns>
        public static int Sign(AseDecimal value) { throw new NotImplementedException(); }

        /// <summary>
        /// Returns a value indicating whether this instance of AseDecimal and the value indicated by the object value are equal.
        /// </summary>
        /// <param name="value">The object being tested for equality.</param>
        public override bool Equals(object value) { throw new NotImplementedException(); }

        /// <summary>
        /// Returns a value indicating whether two instances of AseDecimal are equal.
        /// </summary>
        /// <returns>True if the values are equal.</returns>
        public static bool operator ==(AseDecimal a, AseDecimal b) { throw new NotImplementedException(); }

        public static bool operator !=(AseDecimal a, AseDecimal b) { throw new NotImplementedException(); }

        /// <summary>
        /// Returns a value indicating if a is less than or equal to b.
        /// </summary>
        /// <returns>True if a is less than or equal to b.</returns>
        public static bool operator <=(AseDecimal a, AseDecimal b) { throw new NotImplementedException(); }

        /// <summary>
        /// Returns a value indicating if a is greater than or equal to b.
        /// </summary>
        /// <returns>True if a is greater than or equal to b.</returns>
        public static bool operator >=(AseDecimal a, AseDecimal b) { throw new NotImplementedException(); }

        /// <summary>
        /// Returns a value indicating if a is less than b.
        /// </summary>
        /// <returns>True if a is less than b.</returns>
        public static bool operator <(AseDecimal a, AseDecimal b) { throw new NotImplementedException(); }

        /// <summary>
        /// Returns a value indicating if a is greater than b.
        /// </summary>
        /// <returns>True if a is greater than b.</returns>
        public static bool operator >(AseDecimal a, AseDecimal b) { throw new NotImplementedException(); }

        /// <summary>
        /// Parses a string value to an AseDecimal value.
        /// </summary>
        /// <param name="s">The string to be parsed into an AseDecimal value.</param>
        /// <returns>An AseDecimal structure representing the parsed string.</returns>
        public static AseDecimal Parse(string s) { throw new NotImplementedException(); }

        public static bool TryParse(string s, out AseDecimal result) { throw new NotImplementedException(); }

        /// <summary>
        /// Returns a hashcode.
        /// </summary>
        /// <returns>An integer value representing the hashcode.</returns>
        public override int GetHashCode() { throw new NotImplementedException(); }

        /// <summary>
        /// Compares the value of this AseDecimal with the value of Object.
        /// </summary>
        public int CompareTo(object value) { throw new NotImplementedException(); }

        /// <summary>
        /// Compares the value of this AseDecimal with the AseDecimal value.
        /// </summary>
        public int CompareTo(AseDecimal value) { throw new NotImplementedException(); }

        public int Precision { get { throw new NotImplementedException(); } }

        public int Scale { get { throw new NotImplementedException(); } }

        /// <summary>
        /// Returns true if this AseDecimal is a null value.
        /// </summary>
        public bool IsNull { get { throw new NotImplementedException(); } }

        /// <summary>
        /// Returns true if this AseDecimal is a negative value.
        /// </summary>
        public bool IsNegative { get { throw new NotImplementedException(); } }

        /// <summary>
        /// Returns true if this AseDecimal is a positive value.
        /// </summary>
        public bool IsPositive { get { throw new NotImplementedException(); } }

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
        public override string ToString() { throw new NotImplementedException(); }

        public sbyte ToSByte() { throw new NotImplementedException(); }

        public byte ToByte() { throw new NotImplementedException(); }

        public short ToInt16() { throw new NotImplementedException(); }

        public ushort ToUInt16() { throw new NotImplementedException(); }

        public uint ToUInt32() { throw new NotImplementedException(); }

        public ulong ToUInt64() { throw new NotImplementedException(); }

        public int ToInt32() { throw new NotImplementedException(); }

        public long ToInt64() { throw new NotImplementedException(); }

        public float ToSingle() { throw new NotImplementedException(); }

        public double ToDouble() { throw new NotImplementedException(); }

        public static AseDecimal Floor(AseDecimal n) { throw new NotImplementedException(); }

        public static AseDecimal Truncate(AseDecimal n, int position) { throw new NotImplementedException(); }

        public static AseDecimal Round(AseDecimal n, int position) { throw new NotImplementedException(); }
    }
}
