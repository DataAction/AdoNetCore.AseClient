// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

/*
The MIT License (MIT)

Copyright (c) .NET Foundation and Contributors

All rights reserved.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/


//note: This is a cut-down version adapted for use in our driver.
//Would prefer to use the real thing, but it's only available in .net core 2.0
using System;
using System.Diagnostics;
using System.Globalization;
// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global

namespace AdoNetCore.AseClient.Internal
{
    /// <summary>
    /// Represents a fixed precision and scale numeric value between -10<superscript term='38'/>
    /// -1 and 10<superscript term='38'/> -1 to be stored in or retrieved from a database.
    /// </summary>
    internal struct SqlDecimal
    {
        private static class SQLResource
        {
            public const string ArithOverflowMessage = "Arithmatic Overflow";
            public const string InvalidArraySizeMessage = "Length must be 4";
            public const string NullString = "Null string";
            public const string ConversionOverflowMessage = "Conversion overflow";
            public const string DivideByZeroMessage = "Divide by zero";
            public const string InvalidPrecScaleMessage = "Invalid precision or scale";
        }

        // data in CSsNumeric in SQL Server
        // BYTE    m_cbLen;                // # of DWORDs + 1 (1 is for sign)
        // BYTE    m_bPrec;                // precision
        // BYTE    m_bScale;                // scale
        // BYTE    m_bSign;                // NUM_POSITIVE or NUM_NEGATIVE
        // ULONG    m_rgulData [x_culNumeMax];

        // NOTE: If any instance fields change, update SqlTypeWorkarounds type in System.Data.SqlClient.
        internal byte _bStatus;      // bit 0: fNotNull, bit 1: fNegative
        internal byte _bLen;         // number of uints used, = (CSsNumeric.m_cbLen - 1) / 4.
        internal byte _bPrec;
        internal byte _bScale;
        internal uint _data1;
        internal uint _data2;
        internal uint _data3;
        internal uint _data4;

        private static readonly byte s_NUMERIC_MAX_PRECISION = 38;            // Maximum precision of numeric
        public static readonly byte MaxPrecision = s_NUMERIC_MAX_PRECISION;  // max SS precision
        public static readonly byte MaxScale = s_NUMERIC_MAX_PRECISION;      // max SS scale

        private static readonly byte s_bNullMask = 1;    // bit mask for null bit in m_bStatus
        private static readonly byte s_bIsNull = 0;    // is null
        private static readonly byte s_bNotNull = 1;    // is not null
        private static readonly byte s_bReverseNullMask = unchecked((byte)~s_bNullMask);

        private static readonly byte s_bSignMask = 2;    // bit mask for sign bit in m_bStatus
        private static readonly byte s_bPositive = 0;    // is positive
        private static readonly byte s_bNegative = 2;    // is negative
        private static readonly byte s_bReverseSignMask = unchecked((byte)~s_bSignMask);

        private static readonly uint s_uiZero = 0;

        private static readonly int s_cNumeMax = 4;
        private static readonly long s_lInt32Base = ((long)1) << 32;      // 2**32
        private static readonly ulong s_ulInt32Base = ((ulong)1) << 32;     // 2**32
        private static readonly ulong s_ulInt32BaseForMod = s_ulInt32Base - 1;    // 2**32 - 1 (0xFFF...FF)

        internal static readonly ulong s_llMax = long.MaxValue;   // Max of Int64

        private static readonly uint s_ulBase10 = 10;

        private static readonly double s_DUINT_BASE = s_lInt32Base;     // 2**32
        private static readonly double s_DUINT_BASE2 = s_DUINT_BASE * s_DUINT_BASE;  // 2**64
        private static readonly double s_DUINT_BASE3 = s_DUINT_BASE2 * s_DUINT_BASE; // 2**96
        private static readonly double s_DMAX_NUME = 1.0e+38;                  // Max value of numeric
        private static readonly uint s_DBL_DIG = 17;                       // Max decimal digits of double

        //private static readonly byte s_cNumeDivScaleMin = 6;     // Minimum result scale of numeric division

        // Array of multipliers for lAdjust and Ceiling/Floor.
        private static readonly uint[] s_rgulShiftBase = new uint[9] {
            10,
            10 * 10,
            10 * 10 * 10,
            10 * 10 * 10 * 10,
            10 * 10 * 10 * 10 * 10,
            10 * 10 * 10 * 10 * 10 * 10,
            10 * 10 * 10 * 10 * 10 * 10 * 10,
            10 * 10 * 10 * 10 * 10 * 10 * 10 * 10,
            10 * 10 * 10 * 10 * 10 * 10 * 10 * 10 * 10
        };

        #region DecimalHelperTableGenerator
        /*
                // the code below will generate the DecimalHelpers tables
                private static string[] HelperNames = {
                    "DecimalHelpersLo", "DecimalHelpersMid", "DecimalHelpersHi", "DecimalHelpersHiHi",
                };


                private static void DumpDecimalHelperParts(int index)
                {
                    SqlDecimal sqlDecimalValue = 10;    // start precision=2

                    Console.WriteLine("private static readonly UInt32[] {0} = {{", HelperNames[index]);
                    for (int precision = 2; precision <= SqlDecimal.MaxPrecision; precision++){
                        Console.WriteLine("    0x{0,8:x8}, // precision:{1}, value:{2}", sqlDecimalValue.Data[index], precision, sqlDecimalValue.ToString());
                        if (precision < SqlDecimal.MaxPrecision){
                            sqlDecimalValue *= 10;
                        }
                    }
                    sqlDecimalValue = SqlDecimal.MaxValue;
                    int[] data = sqlDecimalValue.Data;
                    UInt32[] udata = { (UInt32)data[0], (UInt32)data[1], (UInt32)data[2], (UInt32)data[3]};
                    bool carry = true;
                    for (int i = 0; i < 4; i++){
                        if (carry){
                            carry = (++udata[i] == 0);
                        }
                    }
                    Console.WriteLine("    0x{0,8:x8}, // precision:{1}+1, value:{2}+1", udata[index], SqlDecimal.MaxPrecision, SqlDecimal.MaxValue.ToString());

                    Console.WriteLine("};");
                    Console.WriteLine();
                }


                public static void CreateDecimalHelperTable()
                {
                    for (int i = 0; i < 4; i++)
                    {
                        DumpDecimalHelperParts(i);
                    }
                }

        */
        #endregion

        #region DecimalHelperTable
        private static readonly uint[] s_decimalHelpersLo = {
            0x0000000a, // precision:2, value:10
            0x00000064, // precision:3, value:100
            0x000003e8, // precision:4, value:1000
            0x00002710, // precision:5, value:10000
            0x000186a0, // precision:6, value:100000
            0x000f4240, // precision:7, value:1000000
            0x00989680, // precision:8, value:10000000
            0x05f5e100, // precision:9, value:100000000
            0x3b9aca00, // precision:10, value:1000000000
            0x540be400, // precision:11, value:10000000000
            0x4876e800, // precision:12, value:100000000000
            0xd4a51000, // precision:13, value:1000000000000
            0x4e72a000, // precision:14, value:10000000000000
            0x107a4000, // precision:15, value:100000000000000
            0xa4c68000, // precision:16, value:1000000000000000
            0x6fc10000, // precision:17, value:10000000000000000
            0x5d8a0000, // precision:18, value:100000000000000000
            0xa7640000, // precision:19, value:1000000000000000000
            0x89e80000, // precision:20, value:10000000000000000000
            0x63100000, // precision:21, value:100000000000000000000
            0xdea00000, // precision:22, value:1000000000000000000000
            0xb2400000, // precision:23, value:10000000000000000000000
            0xf6800000, // precision:24, value:100000000000000000000000
            0xa1000000, // precision:25, value:1000000000000000000000000
            0x4a000000, // precision:26, value:10000000000000000000000000
            0xe4000000, // precision:27, value:100000000000000000000000000
            0xe8000000, // precision:28, value:1000000000000000000000000000
            0x10000000, // precision:29, value:10000000000000000000000000000
            0xa0000000, // precision:30, value:100000000000000000000000000000
            0x40000000, // precision:31, value:1000000000000000000000000000000
            0x80000000, // precision:32, value:10000000000000000000000000000000
            0x00000000, // precision:33, value:100000000000000000000000000000000
            0x00000000, // precision:34, value:1000000000000000000000000000000000
            0x00000000, // precision:35, value:10000000000000000000000000000000000
            0x00000000, // precision:36, value:100000000000000000000000000000000000
            0x00000000, // precision:37, value:1000000000000000000000000000000000000
            0x00000000, // precision:38, value:10000000000000000000000000000000000000
            0x00000000, // precision:38+1, value:99999999999999999999999999999999999999+1
        };

        private static readonly uint[] s_decimalHelpersMid = {
            0x00000000, // precision:2, value:10
            0x00000000, // precision:3, value:100
            0x00000000, // precision:4, value:1000
            0x00000000, // precision:5, value:10000
            0x00000000, // precision:6, value:100000
            0x00000000, // precision:7, value:1000000
            0x00000000, // precision:8, value:10000000
            0x00000000, // precision:9, value:100000000
            0x00000000, // precision:10, value:1000000000
            0x00000002, // precision:11, value:10000000000
            0x00000017, // precision:12, value:100000000000
            0x000000e8, // precision:13, value:1000000000000
            0x00000918, // precision:14, value:10000000000000
            0x00005af3, // precision:15, value:100000000000000
            0x00038d7e, // precision:16, value:1000000000000000
            0x002386f2, // precision:17, value:10000000000000000
            0x01634578, // precision:18, value:100000000000000000
            0x0de0b6b3, // precision:19, value:1000000000000000000
            0x8ac72304, // precision:20, value:10000000000000000000
            0x6bc75e2d, // precision:21, value:100000000000000000000
            0x35c9adc5, // precision:22, value:1000000000000000000000
            0x19e0c9ba, // precision:23, value:10000000000000000000000
            0x02c7e14a, // precision:24, value:100000000000000000000000
            0x1bcecced, // precision:25, value:1000000000000000000000000
            0x16140148, // precision:26, value:10000000000000000000000000
            0xdcc80cd2, // precision:27, value:100000000000000000000000000
            0x9fd0803c, // precision:28, value:1000000000000000000000000000
            0x3e250261, // precision:29, value:10000000000000000000000000000
            0x6d7217ca, // precision:30, value:100000000000000000000000000000
            0x4674edea, // precision:31, value:1000000000000000000000000000000
            0xc0914b26, // precision:32, value:10000000000000000000000000000000
            0x85acef81, // precision:33, value:100000000000000000000000000000000
            0x38c15b0a, // precision:34, value:1000000000000000000000000000000000
            0x378d8e64, // precision:35, value:10000000000000000000000000000000000
            0x2b878fe8, // precision:36, value:100000000000000000000000000000000000
            0xb34b9f10, // precision:37, value:1000000000000000000000000000000000000
            0x00f436a0, // precision:38, value:10000000000000000000000000000000000000
            0x098a2240, // precision:38+1, value:99999999999999999999999999999999999999+1
        };

        private static readonly uint[] s_decimalHelpersHi = {
            0x00000000, // precision:2, value:10
            0x00000000, // precision:3, value:100
            0x00000000, // precision:4, value:1000
            0x00000000, // precision:5, value:10000
            0x00000000, // precision:6, value:100000
            0x00000000, // precision:7, value:1000000
            0x00000000, // precision:8, value:10000000
            0x00000000, // precision:9, value:100000000
            0x00000000, // precision:10, value:1000000000
            0x00000000, // precision:11, value:10000000000
            0x00000000, // precision:12, value:100000000000
            0x00000000, // precision:13, value:1000000000000
            0x00000000, // precision:14, value:10000000000000
            0x00000000, // precision:15, value:100000000000000
            0x00000000, // precision:16, value:1000000000000000
            0x00000000, // precision:17, value:10000000000000000
            0x00000000, // precision:18, value:100000000000000000
            0x00000000, // precision:19, value:1000000000000000000
            0x00000000, // precision:20, value:10000000000000000000
            0x00000005, // precision:21, value:100000000000000000000
            0x00000036, // precision:22, value:1000000000000000000000
            0x0000021e, // precision:23, value:10000000000000000000000
            0x0000152d, // precision:24, value:100000000000000000000000
            0x0000d3c2, // precision:25, value:1000000000000000000000000
            0x00084595, // precision:26, value:10000000000000000000000000
            0x0052b7d2, // precision:27, value:100000000000000000000000000
            0x033b2e3c, // precision:28, value:1000000000000000000000000000
            0x204fce5e, // precision:29, value:10000000000000000000000000000
            0x431e0fae, // precision:30, value:100000000000000000000000000000
            0x9f2c9cd0, // precision:31, value:1000000000000000000000000000000
            0x37be2022, // precision:32, value:10000000000000000000000000000000
            0x2d6d415b, // precision:33, value:100000000000000000000000000000000
            0xc6448d93, // precision:34, value:1000000000000000000000000000000000
            0xbead87c0, // precision:35, value:10000000000000000000000000000000000
            0x72c74d82, // precision:36, value:100000000000000000000000000000000000
            0x7bc90715, // precision:37, value:1000000000000000000000000000000000000
            0xd5da46d9, // precision:38, value:10000000000000000000000000000000000000
            0x5a86c47a, // precision:38+1, value:99999999999999999999999999999999999999+1
        };

        private static readonly uint[] s_decimalHelpersHiHi = {
            0x00000000, // precision:2, value:10
            0x00000000, // precision:3, value:100
            0x00000000, // precision:4, value:1000
            0x00000000, // precision:5, value:10000
            0x00000000, // precision:6, value:100000
            0x00000000, // precision:7, value:1000000
            0x00000000, // precision:8, value:10000000
            0x00000000, // precision:9, value:100000000
            0x00000000, // precision:10, value:1000000000
            0x00000000, // precision:11, value:10000000000
            0x00000000, // precision:12, value:100000000000
            0x00000000, // precision:13, value:1000000000000
            0x00000000, // precision:14, value:10000000000000
            0x00000000, // precision:15, value:100000000000000
            0x00000000, // precision:16, value:1000000000000000
            0x00000000, // precision:17, value:10000000000000000
            0x00000000, // precision:18, value:100000000000000000
            0x00000000, // precision:19, value:1000000000000000000
            0x00000000, // precision:20, value:10000000000000000000
            0x00000000, // precision:21, value:100000000000000000000
            0x00000000, // precision:22, value:1000000000000000000000
            0x00000000, // precision:23, value:10000000000000000000000
            0x00000000, // precision:24, value:100000000000000000000000
            0x00000000, // precision:25, value:1000000000000000000000000
            0x00000000, // precision:26, value:10000000000000000000000000
            0x00000000, // precision:27, value:100000000000000000000000000
            0x00000000, // precision:28, value:1000000000000000000000000000
            0x00000000, // precision:29, value:10000000000000000000000000000
            0x00000001, // precision:30, value:100000000000000000000000000000
            0x0000000c, // precision:31, value:1000000000000000000000000000000
            0x0000007e, // precision:32, value:10000000000000000000000000000000
            0x000004ee, // precision:33, value:100000000000000000000000000000000
            0x0000314d, // precision:34, value:1000000000000000000000000000000000
            0x0001ed09, // precision:35, value:10000000000000000000000000000000000
            0x00134261, // precision:36, value:100000000000000000000000000000000000
            0x00c097ce, // precision:37, value:1000000000000000000000000000000000000
            0x0785ee10, // precision:38, value:10000000000000000000000000000000000000
            0x4b3b4ca8, // precision:38+1, value:99999999999999999999999999999999999999+1
        };
        #endregion

        // note that the algorithm covers a range from -5 to +4 from the initial index
        // at the end of the algorithm the tableindex will point to the greatest value that is
        // less than the current value
        // except (!) if the current value is less than 10 (precision=1). There is no value ins
        // the table that is less than 10. In this case the algorithm terminates earlier.
        //
        // The startindex values have been chosen so that the highest possible index (startindex+5)
        // does not point to a value that has bits in a higher word set
        //
        private const int HelperTableStartIndexLo = 5;
        private const int HelperTableStartIndexMid = 15;
        private const int HelperTableStartIndexHi = 24;
        private const int HelperTableStartIndexHiHi = 33;

        private byte CalculatePrecision()
        {
            int tableIndex;
            byte precision;
            uint[] decimalHelpers;
            uint decimalPart;

            if (_data4 != 0)
            {
                tableIndex = HelperTableStartIndexHiHi;
                decimalHelpers = s_decimalHelpersHiHi;
                decimalPart = _data4;
            }
            else if (_data3 != 0)
            {
                tableIndex = HelperTableStartIndexHi;
                decimalHelpers = s_decimalHelpersHi;
                decimalPart = _data3;
            }
            else if (_data2 != 0)
            {
                tableIndex = HelperTableStartIndexMid;
                decimalHelpers = s_decimalHelpersMid;
                decimalPart = _data2;
            }
            else
            {
                tableIndex = HelperTableStartIndexLo;
                decimalHelpers = s_decimalHelpersLo;
                decimalPart = _data1;
            }


            // this code will move the index no more than -2 -2 -1 (-5) or +2 +1 +1 (+4)
            // from the initial position
            //
            if (decimalPart < decimalHelpers[tableIndex])
            {
                tableIndex -= 2;
                if (decimalPart < decimalHelpers[tableIndex])
                {
                    tableIndex -= 2;
                    if (decimalPart < decimalHelpers[tableIndex])
                    {
                        tableIndex -= 1;
                    }
                    else
                    {
                        tableIndex += 1;
                    }
                }
                else
                {
                    tableIndex += 1;
                }
            }
            else
            {
                tableIndex += 2;
                if (decimalPart < decimalHelpers[tableIndex])
                {
                    tableIndex -= 1;
                }
                else
                {
                    tableIndex += 1;
                }
            }
            if (decimalPart >= decimalHelpers[tableIndex])
            {
                tableIndex += 1;
                if (tableIndex == 37 && decimalPart >= decimalHelpers[tableIndex])
                {
                    // This can happen only if the actual value is greater than 1E+38,
                    // in which case, tableIndex starts at 33 and ends at 37.
                    // Note that in this case, the actual value will be out of SqlDeicmal's range,
                    // and tableIndex is out of the array boudary. We'll throw later in ctors.
                    tableIndex += 1;
                }
            }

            precision = (byte)(tableIndex + 1);
            if (precision > 1)
            {
                // not done yet
                // tableIndex may still be off by one since we didn't look at the lower words
                //
                if (VerifyPrecision((byte)(precision - 1)))
                {
                    precision -= 1;
                }
            }
            Debug.Assert((precision == MaxPrecision + 1) || VerifyPrecision(precision), "Calcualted precision too low?");
            Debug.Assert((precision == 1) || !VerifyPrecision((byte)(precision - 1)), "Calculated precision too high?");

            // adjust the precision
            // This might not be correct but is to our best knowledge
            precision = Math.Max(precision, _bScale);
#if DEBUG
            byte bPrec = _bPrec;   // store current value
            _bPrec = MaxPrecision; // BActualPrec does not work if m_bPrec uninitialized!
            byte bActualPrecision = BActualPrec();
            _bPrec = bPrec;  // restore current value

            Debug.Assert(precision == bActualPrecision, string.Format(null, "CalculatePrecision={0}, BActualPrec={1}. Results must be equal!", precision, bActualPrecision));
#endif
            return precision;
        }

        // VerifyPrecision
        //
        // returns true if the current value is less or equal than the max value of the
        // supplied precision.
        //
        private bool VerifyPrecision(byte precision)
        {
            Debug.Assert(precision > 0, "Precision cannot be less than 1");
            Debug.Assert(precision <= MaxPrecision, "Precision > MaxPrecision");

            int tableIndex = checked((precision - 1));
            if (_data4 < s_decimalHelpersHiHi[tableIndex])
            {
                return true;
            }
            else if (_data4 == s_decimalHelpersHiHi[tableIndex])
            {
                if (_data3 < s_decimalHelpersHi[tableIndex])
                {
                    return true;
                }
                else if (_data3 == s_decimalHelpersHi[tableIndex])
                {
                    if (_data2 < s_decimalHelpersMid[tableIndex])
                    {
                        return true;
                    }
                    else if (_data2 == s_decimalHelpersMid[tableIndex])
                    {
                        if (_data1 < s_decimalHelpersLo[tableIndex])
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }



        // constructor
        // construct a Null
        private SqlDecimal(bool fNull)
        {
            _bLen =
            _bPrec =
            _bScale = 0;
            _bStatus = 0;
            _data1 =
            _data2 =
            _data3 =
            _data4 = s_uiZero;
        }

        public SqlDecimal(decimal value)
        {
            // set the null bit
            _bStatus = s_bNotNull;

            // note: using usafe code we could get the data directly out of the structure like that:
            // UInt32 * pInt = (UInt32*) &value;
            // UInt32 sgnscl = *pInt++;

            // m_data3 = *pInt++; // high part
            // m_data1 = *pInt++; // lo part
            // m_data2 = *pInt++; // mid part

            int[] bits = decimal.GetBits(value);
            uint sgnscl;

            unchecked
            {
                sgnscl = (uint)bits[3];
                _data1 = (uint)bits[0];
                _data2 = (uint)bits[1];
                _data3 = (uint)bits[2];
                _data4 = s_uiZero;
            }

            // set the sign bit
            _bStatus |= ((sgnscl & 0x80000000) == 0x80000000) ? s_bNegative : (byte)0;

            if (_data3 != 0)
                _bLen = 3;
            else if (_data2 != 0)
                _bLen = 2;
            else
                _bLen = 1;

            // Get the scale info from Decimal
            _bScale = (byte)((int)(sgnscl & 0xff0000) >> 16);

            // initialize precision
            _bPrec = 0; // The this object cannot be used before all of its fields are assigned to


            _bPrec = CalculatePrecision();
            // CalculatePrecision adjusts!
            //            if (m_bPrec < m_bScale)
            //                m_bPrec = m_bScale;
        }

        public SqlDecimal(byte bPrecision, byte bScale, bool fPositive, int[] bits)
        {
            CheckValidPrecScale(bPrecision, bScale);
            if (bits == null)
                throw new ArgumentNullException(nameof(bits));
            else if (bits.Length != 4)
                throw new ArgumentException(SQLResource.InvalidArraySizeMessage, nameof(bits));

            _bPrec = bPrecision;
            _bScale = bScale;
            _data1 = (uint)bits[0];
            _data2 = (uint)bits[1];
            _data3 = (uint)bits[2];
            _data4 = (uint)bits[3];
            _bLen = 1;
            for (int i = 3; i >= 0; i--)
            {
                if (bits[i] != 0)
                {
                    _bLen = (byte)(i + 1);
                    break;
                }
            }

            // set the null bit
            _bStatus = s_bNotNull;

            // set the sign bit
            if (!fPositive)
            {
                _bStatus |= s_bNegative;
            }

            // If result is -0, adjust sign to positive.
            if (FZero())
                SetPositive();

            if (bPrecision < CalculatePrecision())
                throw new OverflowException(SQLResource.ArithOverflowMessage);
        }

        public SqlDecimal(double dVal) : this(false)
        {
            // set the null bit
            _bStatus = s_bNotNull;

            // Split double to sign, integer, and fractional parts
            if (dVal < 0)
            {
                dVal = -dVal;
                _bStatus |= s_bNegative;
            }

            // If it will not fit into numeric(NUMERIC_MAX_PRECISION,0), overflow.
            if (dVal >= s_DMAX_NUME)
                throw new OverflowException(SQLResource.ArithOverflowMessage);

            double dInt = Math.Floor(dVal);
            double dFrac = dVal - dInt;

            _bPrec = s_NUMERIC_MAX_PRECISION;
            _bLen = 1;
            if (dInt > 0.0)
            {
                dVal = Math.Floor(dInt / s_DUINT_BASE);
                _data1 = (uint)(dInt - dVal * s_DUINT_BASE);
                dInt = dVal;

                if (dInt > 0.0)
                {
                    dVal = Math.Floor(dInt / s_DUINT_BASE);
                    _data2 = (uint)(dInt - dVal * s_DUINT_BASE);
                    dInt = dVal;
                    _bLen++;

                    if (dInt > 0.0)
                    {
                        dVal = Math.Floor(dInt / s_DUINT_BASE);
                        _data3 = (uint)(dInt - dVal * s_DUINT_BASE);
                        dInt = dVal;
                        _bLen++;

                        if (dInt > 0.0)
                        {
                            dVal = Math.Floor(dInt / s_DUINT_BASE);
                            _data4 = (uint)(dInt - dVal * s_DUINT_BASE);
                            dInt = dVal;
                            _bLen++;
                        }
                    }
                }
            }

            uint ulLen, ulLenDelta;
            uint ulTemp;

            // Get size of the integer part
            ulLen = FZero() ? 0 : (uint)CalculatePrecision();
            Debug.Assert(ulLen <= s_NUMERIC_MAX_PRECISION, "ulLen <= NUMERIC_MAX_PRECISION", "");

            // If we got more than 17 decimal digits, zero lower ones.
            if (ulLen > s_DBL_DIG)
            {
                // Divide number by 10 while there are more then 17 digits
                uint ulWrk = ulLen - s_DBL_DIG;
                do
                {
                    ulTemp = DivByULong(10);
                    ulWrk--;
                }
                while (ulWrk > 0);
                ulWrk = ulLen - s_DBL_DIG;

                // Round, if necessary. # of digits can change. Cannot be overflow.
                if (ulTemp >= 5)
                {
                    AddULong(1);
                    ulLen = CalculatePrecision() + ulWrk;
                }

                // Multiply back
                do
                {
                    MultByULong(10);
                    ulWrk--;
                }
                while (ulWrk > 0);
            }

            _bScale = (byte)(ulLen < s_DBL_DIG ? s_DBL_DIG - ulLen : 0);
            _bPrec = (byte)(ulLen + _bScale);

            // Add meaningful fractional part - max 9 digits per iteration
            if (_bScale > 0)
            {
                ulLen = _bScale;
                do
                {
                    ulLenDelta = (ulLen >= 9) ? 9 : ulLen;

                    dFrac *= s_rgulShiftBase[(int)ulLenDelta - 1];
                    ulLen -= ulLenDelta;
                    MultByULong(s_rgulShiftBase[(int)ulLenDelta - 1]);
                    AddULong((uint)dFrac);
                    dFrac -= Math.Floor(dFrac);
                }
                while (ulLen > 0);
            }

            // Round, if necessary
            if (dFrac >= 0.5)
            {
                AddULong(1);
            }

            if (FZero())
                SetPositive();

            AssertValid();
        }

        // INullable
        public bool IsNull => (_bStatus & s_bNullMask) == s_bIsNull;

        public decimal Value => ToDecimal();

        public bool IsPositive
        {
            get
            {
                if (IsNull)
                    //throw new SqlNullValueException();
                    throw new NullReferenceException();
                return (_bStatus & s_bSignMask) == s_bPositive;
            }
        }

        private void SetPositive()
        {
            Debug.Assert(!IsNull);
            _bStatus = (byte)(_bStatus & s_bReverseSignMask);
        }

        private void SetSignBit(bool fPositive)
        {
            Debug.Assert(!IsNull);
            _bStatus = (byte)(fPositive ? (_bStatus & s_bReverseSignMask) : (_bStatus | s_bNegative));
        }

        public byte Precision
        {
            get
            {
                if (IsNull)
                    //throw new SqlNullValueException();
                    throw new NullReferenceException();
                return _bPrec;
            }
        }

        public byte Scale
        {
            get
            {
                if (IsNull)
                    //throw new SqlNullValueException();
                    throw new NullReferenceException();
                return _bScale;
            }
        }

        public int[] Data
        {
            get
            {
                if (IsNull)
                    //throw new SqlNullValueException();
                    throw new NullReferenceException();

                unchecked
                {
                    return new int[4] { (int)_data1, (int)_data2, (int)_data3, (int)_data4 };
                }
            }
        }

        public byte[] BinData
        {
            get
            {
                if (IsNull)
                    //throw new SqlNullValueException();
                    throw new NullReferenceException();

                int data1 = (int)_data1;
                int data2 = (int)_data2;
                int data3 = (int)_data3;
                int data4 = (int)_data4;
                byte[] rgBinData = new byte[16];
                rgBinData[0] = (byte)(data1 & 0xff);
                data1 >>= 8;
                rgBinData[1] = (byte)(data1 & 0xff);
                data1 >>= 8;
                rgBinData[2] = (byte)(data1 & 0xff);
                data1 >>= 8;
                rgBinData[3] = (byte)(data1 & 0xff);
                rgBinData[4] = (byte)(data2 & 0xff);
                data2 >>= 8;
                rgBinData[5] = (byte)(data2 & 0xff);
                data2 >>= 8;
                rgBinData[6] = (byte)(data2 & 0xff);
                data2 >>= 8;
                rgBinData[7] = (byte)(data2 & 0xff);
                rgBinData[8] = (byte)(data3 & 0xff);
                data3 >>= 8;
                rgBinData[9] = (byte)(data3 & 0xff);
                data3 >>= 8;
                rgBinData[10] = (byte)(data3 & 0xff);
                data3 >>= 8;
                rgBinData[11] = (byte)(data3 & 0xff);
                rgBinData[12] = (byte)(data4 & 0xff);
                data4 >>= 8;
                rgBinData[13] = (byte)(data4 & 0xff);
                data4 >>= 8;
                rgBinData[14] = (byte)(data4 & 0xff);
                data4 >>= 8;
                rgBinData[15] = (byte)(data4 & 0xff);

                return rgBinData;
            }
        }

        public override string ToString()
        {
            if (IsNull)
                return SQLResource.NullString;
            AssertValid();

            // Make local copy of data to avoid modifying input.
            uint[] rgulNumeric = new uint[4] { _data1, _data2, _data3, _data4 };
            int culLen = _bLen;
            char[] pszTmp = new char[s_NUMERIC_MAX_PRECISION + 1];   //Local Character buffer to hold
                                                                     //the decimal digits, from the
                                                                     //lowest significant to highest significant

            int iDigits = 0;//Number of significant digits
            uint ulRem; //Remainder of a division by x_ulBase10, i.e.,least significant digit

            // Build the final numeric string by inserting the sign, reversing
            // the order and inserting the decimal number at the correct position

            //Retrieve each digit from the lowest significant digit
            while (culLen > 1 || rgulNumeric[0] != 0)
            {
                MpDiv1(rgulNumeric, ref culLen, s_ulBase10, out ulRem);
                //modulo x_ulBase10 is the lowest significant digit
                pszTmp[iDigits++] = ChFromDigit(ulRem);
            }

            // if scale of the number has not been
            // reached pad remaining number with zeros.
            while (iDigits <= _bScale)
            {
                pszTmp[iDigits++] = ChFromDigit(0);
            }


            int uiResultLen = 0;
            int iCurChar = 0;
            char[] szResult;

            // Increment the result length if scale > 0 (need to add '.')            
            if (_bScale > 0)
            {
                uiResultLen = 1;
            }

            if (IsPositive)
            {
                szResult = new char[uiResultLen + iDigits];
            }
            else
            {
                // Increment the result length if negative (need to add '-')            
                szResult = new char[uiResultLen + iDigits + 1];
                szResult[iCurChar++] = '-';
            }

            while (iDigits > 0)
            {
                if (iDigits-- == _bScale)
                    szResult[iCurChar++] = '.';

                szResult[iCurChar++] = pszTmp[iDigits];
            }

            AssertValid();

            return new string(szResult);
        }

        private decimal ToDecimal()
        {
            if (IsNull)
                //throw new SqlNullValueException();
                throw new NullReferenceException();

            if ((int)_data4 != 0 || _bScale > 28)
                throw new OverflowException(SQLResource.ConversionOverflowMessage);

            unchecked
            {
                return new decimal((int)_data1, (int)_data2, (int)_data3, !IsPositive, _bScale);
            }
        }

        // Implicit conversion from Decimal to SqlDecimal
        public static implicit operator SqlDecimal(decimal x)
        {
            return new SqlDecimal(x);
        }

        // Explicit conversion from Double to SqlDecimal
        public static explicit operator SqlDecimal(double x)
        {
            return new SqlDecimal(x);
        }

        // Implicit conversion from long to SqlDecimal
        public static implicit operator SqlDecimal(long x)
        {
            return new SqlDecimal(new decimal(x));
        }

        // Explicit conversion from SqlDecimal to Decimal. Throw exception if x is Null.
        public static explicit operator decimal(SqlDecimal x)
        {
            return x.Value;
        }


        // Unary operators
        public static SqlDecimal operator -(SqlDecimal x)
        {
            if (x.IsNull)
                return Null;
            else
            {
                SqlDecimal s = x;
                if (s.FZero())
                    s.SetPositive();
                else
                    s.SetSignBit(!s.IsPositive);
                return s;
            }
        }

        // private methods

        //----------------------------------------------------------------------
        // Is this RE numeric valid?
        [Conditional("DEBUG")]
        private void AssertValid()
        {
            if (IsNull)
                return;

            // Scale,Prec in range
            Debug.Assert(_bScale <= s_NUMERIC_MAX_PRECISION, "m_bScale <= NUMERIC_MAX_PRECISION", "In AssertValid");
            Debug.Assert(_bScale <= _bPrec, "m_bScale <= m_bPrec", "In AssertValid");
            Debug.Assert(_bScale >= 0, "m_bScale >= 0", "In AssertValid");
            Debug.Assert(_bPrec > 0, "m_bPrec > 0", "In AssertValid");

            Debug.Assert(CLenFromPrec(_bPrec) >= _bLen, "CLenFromPrec(m_bPrec) >= m_bLen", "In AssertValid");
            Debug.Assert(_bLen <= s_cNumeMax, "m_bLen <= x_cNumeMax", "In AssertValid");

            uint[] rglData = new uint[4] { _data1, _data2, _data3, _data4 };

            // highest UI4 is non-0 unless value "zero"
            if (rglData[_bLen - 1] == 0)
            {
                Debug.Assert(_bLen == 1, "m_bLen == 1", "In AssertValid");
            }

            // All UI4s from length to end are 0
            for (int iulData = _bLen; iulData < s_cNumeMax; iulData++)
                Debug.Assert(rglData[iulData] == 0, "rglData[iulData] == 0", "In AssertValid");
        }
        /*
                // print the data members
                [System.Diagnostics.Conditional("DEBUG")]
                private void Print() {
                    if (IsNull) {
                        Debug.WriteLine("Numeric: Null");
                        return;
                    }
                    Debug.WriteLine("Numeric: data - " + m_data4.ToString() + ", " + m_data3.ToString() + ", " +
                                      m_data2.ToString() + ", " + m_data1.ToString());
                    Debug.WriteLine("\tlen = " + m_bLen.ToString() + ", Prec = " + m_bPrec.ToString() +
                                      ", Scale = " + m_bScale.ToString() + ", Sign = " + IsPositive.ToString());
                }

                [System.Diagnostics.Conditional("DEBUG")]
                private void Print(String s) {
                    Debug.WriteLine("*** " + s + " ***");
                    if (IsNull) {
                        Debug.WriteLine("Numeric: Null");
                        return;
                    }
                    Debug.WriteLine("Numeric: data - " + m_data4.ToString() + ", " + m_data3.ToString() + ", " +
                                      m_data2.ToString() + ", " + m_data1.ToString());
                    Debug.WriteLine("\tlen = " + m_bLen.ToString() + ", Prec = " + m_bPrec.ToString() +
                                      ", Scale = " + m_bScale.ToString() + ", Sign = " + IsPositive.ToString());
                }
        */
        // Set all extra uints to zero
        private static void ZeroToMaxLen(uint[] rgulData, int cUI4sCur)
        {
            Debug.Assert(rgulData.Length == s_cNumeMax, "rgulData.Length == x_cNumeMax", "Invalid array length");

            switch (cUI4sCur)
            {
                case 1:
                    rgulData[1] = rgulData[2] = rgulData[3] = 0;
                    break;

                case 2:
                    rgulData[2] = rgulData[3] = 0;
                    break;

                case 3:
                    rgulData[3] = 0;
                    break;
            }
        }

        // Set all extra uints to zero
        /*
        private void ZeroToMaxLen(int cUI4sCur) {
            switch (cUI4sCur) {
                case 1:
                    m_data2 = m_data3 = m_data4 = x_uiZero;
                    break;

                case 2:
                    m_data3 = m_data4 = x_uiZero;
                    break;

                case 3:
                    m_data4 = x_uiZero;
                    break;
            }
        }
        */

        //Determine the number of uints needed for a numeric given a precision
        //Precision        Length
        //    0            invalid
        //    1-9            1
        //    10-19        2
        //    20-28        3
        //    29-38        4
        // The array in Shiloh. Listed here for comparison.
        //private static readonly byte[] rgCLenFromPrec = new byte[] {5,5,5,5,5,5,5,5,5,9,9,9,9,9,
        //    9,9,9,9,9,13,13,13,13,13,13,13,13,13,17,17,17,17,17,17,17,17,17,17};
        private static readonly byte[] s_rgCLenFromPrec = new byte[] {1,1,1,1,1,1,1,1,1,2,2,2,2,2,
            2,2,2,2,2,3,3,3,3,3,3,3,3,3,4,4,4,4,4,4,4,4,4,4};

        private static byte CLenFromPrec(byte bPrec)
        {
            Debug.Assert(bPrec <= MaxPrecision && bPrec > 0, "bPrec <= MaxPrecision && bPrec > 0",
                           "Invalid numeric precision");
            return s_rgCLenFromPrec[bPrec - 1];
        }

        // check whether is zero
        private bool FZero()
        {
            return (_data1 == 0) && (_bLen <= 1);
        }

        // Find the case where we overflowed 10**38, but not 2**128
        private bool FGt10_38()
        {
            return _data4 >= 0x4b3b4ca8L && _bLen == 4 &&
            ((_data4 > 0x4b3b4ca8L) || (_data3 > 0x5a86c47aL) ||
             (_data3 == 0x5a86c47aL) && (_data2 >= 0x098a2240L));
        }

        private bool FGt10_38(uint[] rglData)
        {
            Debug.Assert(rglData.Length == 4, "rglData.Length == 4", "Wrong array length: " + rglData.Length.ToString(CultureInfo.InvariantCulture));

            return rglData[3] >= 0x4b3b4ca8L &&
            ((rglData[3] > 0x4b3b4ca8L) || (rglData[2] > 0x5a86c47aL) ||
             (rglData[2] == 0x5a86c47aL) && (rglData[1] >= 0x098a2240L));
        }


        // Powers of ten (used by BGetPrecI4,BGetPrecI8)
        private static readonly uint s_ulT1 = 10;
        private static readonly uint s_ulT2 = 100;
        private static readonly uint s_ulT3 = 1000;
        private static readonly uint s_ulT4 = 10000;
        private static readonly uint s_ulT5 = 100000;
        private static readonly uint s_ulT6 = 1000000;
        private static readonly uint s_ulT7 = 10000000;
        private static readonly uint s_ulT8 = 100000000;
        private static readonly uint s_ulT9 = 1000000000;
        private static readonly ulong s_dwlT10 = 10000000000;
        private static readonly ulong s_dwlT11 = 100000000000;
        private static readonly ulong s_dwlT12 = 1000000000000;
        private static readonly ulong s_dwlT13 = 10000000000000;
        private static readonly ulong s_dwlT14 = 100000000000000;
        private static readonly ulong s_dwlT15 = 1000000000000000;
        private static readonly ulong s_dwlT16 = 10000000000000000;
        private static readonly ulong s_dwlT17 = 100000000000000000;
        private static readonly ulong s_dwlT18 = 1000000000000000000;
        private static readonly ulong s_dwlT19 = 10000000000000000000;

        //------------------------------------------------------------------
        //BGetPrecI4
        //    Return the precision(number of significant digits) of a integer
        private static byte BGetPrecUI4(uint value)
        {
            int ret;

            // Now do the (almost) binary search
            if (value < s_ulT4)
            {
                if (value < s_ulT2)
                    ret = value >= s_ulT1 ? 2 : 1;
                else
                    ret = value >= s_ulT3 ? 4 : 3;
            }
            else if (value < s_ulT8)
            {
                if (value < s_ulT6)
                    ret = value >= s_ulT5 ? 6 : 5;
                else
                    ret = value >= s_ulT7 ? 8 : 7;
            }
            else
                ret = value >= s_ulT9 ? 10 : 9;

            return (byte)ret;
        }

        //------------------------------------------------------------------
        //BGetPrecI8
        //    Return the precision (number of significant digits) of an Int8
        private static byte BGetPrecUI8(uint ulU0, uint ulU1)
        {
            ulong dwlVal = ulU0 + (((ulong)ulU1) << 32);
            return BGetPrecUI8(dwlVal);
        }

        private static byte BGetPrecUI8(ulong dwlVal)
        {
            int ret;

            // Now do the (almost) binary search
            if (dwlVal < s_ulT8)
            {
                uint ulVal = (uint)dwlVal;

                if (ulVal < s_ulT4)
                {
                    if (ulVal < s_ulT2)
                        ret = (ulVal >= s_ulT1) ? 2 : 1;
                    else
                        ret = (ulVal >= s_ulT3) ? 4 : 3;
                }
                else
                {
                    if (ulVal < s_ulT6)
                        ret = (ulVal >= s_ulT5) ? 6 : 5;
                    else
                        ret = (ulVal >= s_ulT7) ? 8 : 7;
                }
            }
            else if (dwlVal < s_dwlT16)
            {
                if (dwlVal < s_dwlT12)
                {
                    if (dwlVal < s_dwlT10)
                        ret = (dwlVal >= s_ulT9) ? 10 : 9;
                    else
                        ret = (dwlVal >= s_dwlT11) ? 12 : 11;
                }
                else
                {
                    if (dwlVal < s_dwlT14)
                        ret = (dwlVal >= s_dwlT13) ? 14 : 13;
                    else
                        ret = (dwlVal >= s_dwlT15) ? 16 : 15;
                }
            }
            else
            {
                if (dwlVal < s_dwlT18)
                    ret = (dwlVal >= s_dwlT17) ? 18 : 17;
                else
                    ret = (dwlVal >= s_dwlT19) ? 20 : 19;
            }

            return (byte)ret;
        }

#if DEBUG
        // This is the old precision routine. It is only used in debug code to verify that both code paths
        // return the same value

        //
        //    BActualPrec()
        //
        //    Determine the actual number of significant digits (precision) of a numeric
        //
        //    Parameters:
        //
        //    Complexity:
        //        For small numerics, use simpler routines = O(n)
        //        Else, max 3 divisions of mp by ULONG, then again simpler routine.
        //
        //    Returns:
        //        a byte containing the actual precision
        //
        private byte BActualPrec()
        {
            if (_bPrec == 0 || _bLen < 1)
                return 0;

            int ciulU = _bLen;
            int Prec;
            uint ulRem;

            if (ciulU == 1)
            {
                Prec = BGetPrecUI4(_data1);
            }
            else if (ciulU == 2)
            {
                Prec = BGetPrecUI8(_data1, _data2);
            }
            else
            {
                uint[] rgulU = new uint[4] { _data1, _data2, _data3, _data4 };
                Prec = 0;
                do
                {
                    MpDiv1(rgulU, ref ciulU, 1000000000, out ulRem);
                    Prec += 9;
                }
                while (ciulU > 2);
                Debug.Assert(Prec == 9 || Prec == 18 || Prec == 27);
                Prec += BGetPrecUI8(rgulU[0], rgulU[1]);
            }

            // If number of significant digits less than scale, return scale
            return (Prec < _bScale ? _bScale : (byte)Prec);
        }
#endif

        //    AddULong()
        //
        //    Add ulAdd to this numeric.  The result will be returned in *this.
        //
        //    Parameters:
        //        this    - IN Operand1 & OUT Result
        //        ulAdd    - IN operand2.
        //
        private void AddULong(uint ulAdd)
        {
            ulong dwlAccum = ulAdd;
            int iData;                  // which UI4 in this we are on
            int iDataMax = _bLen; // # of UI4s in this

            uint[] rguiData = new uint[4] { _data1, _data2, _data3, _data4 };

            // Add, starting at the LS UI4 until out of UI4s or no carry
            iData = 0;
            do
            {
                dwlAccum += rguiData[iData];
                rguiData[iData] = (uint)dwlAccum;       // equivalent to mod x_dwlBaseUI4
                dwlAccum >>= 32;                        // equivalent to dwlAccum /= x_dwlBaseUI4;
                if (0 == dwlAccum)
                {
                    StoreFromWorkingArray(rguiData);
                    return;
                }
                iData++;
            }
            while (iData < iDataMax);

            // There is carry at the end

            Debug.Assert(dwlAccum < s_ulInt32Base, "dwlAccum < x_lInt32Base", "");

            // Either overflowed
            if (iData == s_cNumeMax)
                throw new OverflowException(SQLResource.ArithOverflowMessage);

            // Or need to extend length by 1 UI4
            rguiData[iData] = (uint)dwlAccum;
            _bLen++;

            if (FGt10_38(rguiData))
                throw new OverflowException(SQLResource.ArithOverflowMessage);

            StoreFromWorkingArray(rguiData);
        }

        // multiply by a long integer
        private void MultByULong(uint uiMultiplier)
        {
            int iDataMax = _bLen; // How many UI4s currently in *this

            ulong dwlAccum = 0;       // accumulated sum
            ulong dwlNextAccum = 0;   // accumulation past dwlAccum
            int iData;              // which UI4 in *This we are on.

            uint[] rguiData = new uint[4] { _data1, _data2, _data3, _data4 };

            for (iData = 0; iData < iDataMax; iData++)
            {
                Debug.Assert(dwlAccum < s_ulInt32Base);

                ulong ulTemp = rguiData[iData];
                dwlNextAccum = ulTemp * uiMultiplier;
                dwlAccum += dwlNextAccum;
                if (dwlAccum < dwlNextAccum)        // Overflow of int64 add
                    dwlNextAccum = s_ulInt32Base;   // how much to add to dwlAccum after div x_dwlBaseUI4
                else
                    dwlNextAccum = 0;
                rguiData[iData] = unchecked((uint)dwlAccum); // equivalent to mod x_dwlBaseUI4
                dwlAccum = (dwlAccum >> 32) + dwlNextAccum;  // equivalent to div x_dwlBaseUI4
            }

            // If any carry,
            if (dwlAccum != 0)
            {
                // Either overflowed
                Debug.Assert(dwlAccum < s_ulInt32Base, "dwlAccum < x_dwlBaseUI4", "Integer overflow");
                if (iDataMax == s_cNumeMax)
                    throw new OverflowException(SQLResource.ArithOverflowMessage);

                // Or extend length by one uint
                rguiData[iDataMax] = (uint)dwlAccum;
                _bLen++;
            }

            if (FGt10_38(rguiData))
                throw new OverflowException(SQLResource.ArithOverflowMessage);

            StoreFromWorkingArray(rguiData);
        }


        //    DivByULong()
        //
        //    Divide numeric value by a ULONG.  The result will be returned
        //    in the dividend *this.
        //
        //    Parameters:
        //        this        - IN Dividend & OUT Result
        //        ulDivisor    - IN Divisor
        //    Returns:        - OUT Remainder
        //
        private uint DivByULong(uint iDivisor)
        {
            ulong dwlDivisor = iDivisor;
            ulong dwlAccum = 0;           //Accumulated sum
            uint ulQuotientCur = 0;      // Value of the current UI4 of the quotient
            bool fAllZero = true;    // All of the quotient (so far) has been 0
            int iData;              //Which UI4 currently on

            // Check for zero divisor.
            if (dwlDivisor == 0)
                throw new DivideByZeroException(SQLResource.DivideByZeroMessage);

            // Copy into array, so that we can iterate through the data
            uint[] rguiData = new uint[4] { _data1, _data2, _data3, _data4 };

            // Start from the MS UI4 of quotient, divide by divisor, placing result
            //        in quotient and carrying the remainder.
            // DWORDLONG sufficient accumulator since:
            //        Accum < Divisor <= 2^32 - 1    at start each loop
            //                                    initially,and mod end previous loop
            //        Accum*2^32 < 2^64 - 2^32
            //                                    multiply both side by 2^32 (x_dwlBaseUI4)
            //        Accum*2^32 + m_rgulData < 2^64
            //                                    rglData < 2^32
            for (iData = _bLen; iData > 0; iData--)
            {
                Debug.Assert(dwlAccum < dwlDivisor);
                dwlAccum = (dwlAccum << 32) + rguiData[iData - 1]; // dwlA*x_dwlBaseUI4 + rglData
                Debug.Assert((dwlAccum / dwlDivisor) < s_ulInt32Base);
                //Update dividend to the quotient.
                ulQuotientCur = (uint)(dwlAccum / dwlDivisor);
                rguiData[iData - 1] = ulQuotientCur;
                //Remainder to be carried to the next lower significant byte.
                dwlAccum = dwlAccum % dwlDivisor;

                // While current part of quotient still 0, reduce length
                if (fAllZero && (ulQuotientCur == 0))
                {
                    _bLen--;
                }
                else
                {
                    fAllZero = false;
                }
            }

            StoreFromWorkingArray(rguiData);

            // If result is 0, preserve sign but set length to 5
            if (fAllZero)
                _bLen = 1;

            AssertValid();

            // return the remainder
            Debug.Assert(dwlAccum < s_ulInt32Base);
            return (uint)dwlAccum;
        }

        //    AdjustScale()
        //
        //    Adjust number of digits to the right of the decimal point.
        //    A positive adjustment increases the scale of the numeric value
        //    while a negative adjustment decreases the scale.  When decreasing
        //    the scale for the numeric value, the remainder is checked and
        //    rounded accordingly.
        //
        internal void AdjustScale(int digits, bool fRound)
        {
            Debug.Assert(!IsNull, "!IsNull", "In AdjustScale");

            uint ulRem;                  //Remainder when downshifting
            uint ulShiftBase;            //What to multiply by to effect scale adjust
            bool fNeedRound = false;     //Do we really need to round?
            byte bNewScale, bNewPrec;
            int lAdjust = digits;

            //If downshifting causes truncation of data
            if (lAdjust + _bScale < 0)
                //throw new SqlTruncateException();
                throw new InvalidOperationException("Data could be truncated");

            //If uphifting causes scale overflow
            if (lAdjust + _bScale > s_NUMERIC_MAX_PRECISION)
                throw new OverflowException(SQLResource.ArithOverflowMessage);

            bNewScale = (byte)(lAdjust + _bScale);
            bNewPrec = (byte)(Math.Min(s_NUMERIC_MAX_PRECISION, Math.Max(1, lAdjust + _bPrec)));

            if (lAdjust > 0)
            {
                _bScale = bNewScale;
                _bPrec = bNewPrec;

                while (lAdjust > 0)
                {
                    //if lAdjust>=9, downshift by 10^9 each time, otherwise by the full amount
                    if (lAdjust >= 9)
                    {
                        ulShiftBase = s_rgulShiftBase[8];
                        lAdjust -= 9;
                    }
                    else
                    {
                        ulShiftBase = s_rgulShiftBase[lAdjust - 1];
                        lAdjust = 0;
                    }
                    MultByULong(ulShiftBase);
                }
            }
            else if (lAdjust < 0)
            {
                do
                {
                    if (lAdjust <= -9)
                    {
                        ulShiftBase = s_rgulShiftBase[8];
                        lAdjust += 9;
                    }
                    else
                    {
                        ulShiftBase = s_rgulShiftBase[-lAdjust - 1];
                        lAdjust = 0;
                    }
                    ulRem = DivByULong(ulShiftBase);
                }
                while (lAdjust < 0);

                // Do we really need to round?
                fNeedRound = (ulRem >= ulShiftBase / 2);

                _bScale = bNewScale;
                _bPrec = bNewPrec;
            }

            AssertValid();

            // After adjusting, if the result is 0 and remainder is less than 5,
            // set the sign to be positive and return.
            if (fNeedRound && fRound)
            {
                // If remainder is 5 or above, increment/decrement by 1.
                AddULong(1);
            }
            else if (FZero())
                SetPositive();
        }

        // Adjust scale of a SqlDecimal
        public static SqlDecimal AdjustScale(SqlDecimal n, int digits, bool fRound)
        {
            if (n.IsNull)
                return SqlDecimal.Null;

            SqlDecimal ret = n;
            ret.AdjustScale(digits, fRound);
            return ret;
        }

        //    LAbsCmp()
        //
        //    Compare the absolute value of two numerics without checking scale
        //
        //  Parameters:
        //        this    - IN Operand1
        //        snumOp    - IN Operand2
        //
        //    Returns:
        //        positive    - |this| > |snumOp|
        //        0            - |this| = |snumOp|
        //        negative    - |this| < |snumOp|
        //
        private int LAbsCmp(SqlDecimal snumOp)
        {
            int iData;  //which UI4 we are operating on
            int culOp;  //#of UI4s on operand
            int culThis; //# of UI4s in this

            // If one longer, it is larger
            culOp = snumOp._bLen;
            culThis = _bLen;
            if (culOp != culThis)
                return (culThis > culOp) ? 1 : -1;

            uint[] rglData1 = new uint[4] { _data1, _data2, _data3, _data4 };
            uint[] rglData2 = new uint[4] { snumOp._data1, snumOp._data2, snumOp._data3, snumOp._data4 };

            // Loop through numeric value checking each byte for differences.
            iData = culOp - 1;
            do
            {
                // CONSIDER PERF: Could replace second comparison with
                //        cast to LONGLONG and subtract.  Probably not worth it.
                if (rglData1[iData] != rglData2[iData])
                    return ((rglData1[iData] > rglData2[iData]) ? 1 : -1);
                iData--;
            }
            while (iData >= 0);

            // All UI4s the same, return 0.
            return 0;
        }

        // Move multi-precision number
        private static void MpMove
        (
        uint[] rgulS,      // In    | Source number
        int ciulS,      // In    | # of digits in S
        uint[] rgulD,      // Out    | Destination number
        out int ciulD       // Out    | # of digits in D
        )
        {
            ciulD = ciulS;

            Debug.Assert(rgulS.Length >= ciulS, "rgulS.Length >= ciulS", "Invalid array length");
            Debug.Assert(rgulD.Length >= ciulS, "rgulD.Length >= ciulS", "Invalid array length");

            for (int i = 0; i < ciulS; i++)
                rgulD[i] = rgulS[i];
        }

        // Set multi-precision number to one super-digit
        private static void MpSet
        (
        uint[] rgulD,      // Out    | Number
        out int ciulD,      // Out    | # of digits in D
        uint iulN        // In    | ULONG to set
        )
        {
            ciulD = 1;
            rgulD[0] = iulN;
        }

        // Normalize multi-precision number - remove leading zeroes
        private static void MpNormalize
        (
        uint[] rgulU,      // In   | Number
        ref int ciulU       // InOut| # of digits
        )
        {
            while (ciulU > 1 && rgulU[ciulU - 1] == 0)
                ciulU--;
        }

        // Multi-precision one super-digit multiply in place.
        // D = D * X
        // Length can increase
        private static void MpMul1
        (
        uint[] piulD,      // InOut| D
        ref int ciulD,      // InOut| # of digits in D
        uint iulX        // In    | X
        )
        {
            Debug.Assert(iulX > s_uiZero);
            uint ulCarry = 0;
            int iData;
            ulong dwlAccum;

            for (iData = 0; iData < ciulD; iData++)
            {
                ulong ulTemp = piulD[iData];
                dwlAccum = ulCarry + ulTemp * iulX;
                ulCarry = HI(dwlAccum);
                piulD[iData] = LO(dwlAccum);
            }

            // If overflow occurs, increase precision
            if (ulCarry != 0)
            {
                piulD[iData] = ulCarry;
                ciulD++;
            }
        }

        // Multi-precision one super-digit divide in place.
        // U = U / D,
        // R = U % D
        // Length of U can decrease
        private static void MpDiv1
        (
        uint[] rgulU,      // InOut| U
        ref int ciulU,      // InOut| # of digits in U
        uint iulD,       // In    | D
        out uint iulR        // Out    | R
        )
        {
            Debug.Assert(rgulU.Length == s_cNumeMax);

            uint ulCarry = 0;
            ulong dwlAccum;
            ulong ulD = iulD;
            int idU = ciulU;

            Debug.Assert(iulD != 0, "iulD != 0", "Divided by zero!");
            Debug.Assert(iulD > 0, "iulD > 0", "Invalid data: less than zero");
            Debug.Assert(ciulU > 0, "ciulU > 0", "No data in the array");

            while (idU > 0)
            {
                idU--;
                dwlAccum = (((ulong)ulCarry) << 32) + rgulU[idU];
                rgulU[idU] = (uint)(dwlAccum / ulD);
                ulCarry = (uint)(dwlAccum - rgulU[idU] * ulD);  // (ULONG) (dwlAccum % iulD)
            }
            iulR = ulCarry;
            MpNormalize(rgulU, ref ciulU);
        }

        internal static ulong DWL(uint lo, uint hi)
        {
            return lo + (((ulong)hi) << 32);
        }

        private static uint HI(ulong x)
        {
            return (uint)(x >> 32);
        }

        private static uint LO(ulong x)
        {
            return (uint)x;
        }

        private static void CheckValidPrecScale(byte bPrec, byte bScale)
        {
            if (bPrec < 1 || bPrec > MaxPrecision || bScale < 0 || bScale > MaxScale || bScale > bPrec)
                throw new ArgumentException(SQLResource.InvalidPrecScaleMessage);
        }

        private static char ChFromDigit(uint uiDigit)
        {
            Debug.Assert(uiDigit < 10);
            return (char)(uiDigit + '0');
        }

        // Store data back from rguiData[] to m_data*
        private void StoreFromWorkingArray(uint[] rguiData)
        {
            Debug.Assert(rguiData.Length == 4);
            _data1 = rguiData[0];
            _data2 = rguiData[1];
            _data3 = rguiData[2];
            _data4 = rguiData[3];
        }

        // Truncate to integer

        // For hashing purpose
        public override int GetHashCode()
        {
            if (IsNull)
                return 0;

            SqlDecimal ssnumTemp;
            int lActualPrec;

            // First, "normalize" numeric, so that values with different
            // scale/precision will have the same representation.
            ssnumTemp = this;
            lActualPrec = ssnumTemp.CalculatePrecision();
            ssnumTemp.AdjustScale(s_NUMERIC_MAX_PRECISION - lActualPrec, true);

            // Now evaluate the hash
            int cDwords = ssnumTemp._bLen;
            int ulValue = 0;
            int ulHi;

            // Size of CRC window (hashing bytes, ssstr, sswstr, numeric)
            const int x_cbCrcWindow = 4;
            // const int iShiftVal = (sizeof ulValue) * (8*sizeof(char)) - x_cbCrcWindow;
            const int iShiftVal = 4 * 8 - x_cbCrcWindow;

            int[] rgiData = ssnumTemp.Data;

            for (int i = 0; i < cDwords; i++)
            {
                ulHi = (ulValue >> iShiftVal) & 0xff;
                ulValue <<= x_cbCrcWindow;
                ulValue = ulValue ^ rgiData[i] ^ ulHi;
            }
            return ulValue;
        }

        // These values are defined at last, because they need to call ctors, which use other
        // constant values. Those constants must be defined before callint ctor.

        public static readonly SqlDecimal Null = new SqlDecimal(true);
    } // SqlDecimal
} // namespace System.Data.SqlTypes