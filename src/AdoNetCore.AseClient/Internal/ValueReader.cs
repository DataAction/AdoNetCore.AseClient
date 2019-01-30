using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using AdoNetCore.AseClient.Enum;

namespace AdoNetCore.AseClient.Internal
{
    internal static class ValueReader
    {
        public static object Read(Stream stream, FormatItem format, DbEnvironment env, ref bool streamExceeded)
        {
            return ReadInternal(stream, format, env, ref streamExceeded) ?? DBNull.Value;
        }

        private delegate object ReadMapMethodDelegate(Stream stream, FormatItem format, DbEnvironment env, ref bool streamExceeded);
        private static readonly Dictionary<TdsDataType, ReadMapMethodDelegate> ReadMap = new Dictionary<TdsDataType, ReadMapMethodDelegate>
        {
            {TdsDataType.TDS_BIT, ReadTDS_BIT},
            {TdsDataType.TDS_INT1, ReadTDS_INT1},
            {TdsDataType.TDS_SINT1, ReadTDS_SINT1},
            {TdsDataType.TDS_INT2, ReadTDS_INT2},
            {TdsDataType.TDS_UINT2, ReadTDS_UINT2},
            {TdsDataType.TDS_INT4, ReadTDS_INT4},
            {TdsDataType.TDS_UINT4, ReadTDS_UINT4},
            {TdsDataType.TDS_INT8, ReadTDS_INT8},
            {TdsDataType.TDS_UINT8, ReadTDS_UINT8},
            {TdsDataType.TDS_INTN, ReadTDS_INTN},
            {TdsDataType.TDS_UINTN, ReadTDS_UINTN},
            {TdsDataType.TDS_FLT4, ReadTDS_FLT4},
            {TdsDataType.TDS_FLT8, ReadTDS_FLT8},
            {TdsDataType.TDS_FLTN, ReadTDS_FLTN},
            {TdsDataType.TDS_CHAR, ReadTDS_CHAR},
            {TdsDataType.TDS_VARCHAR, ReadTDS_VARCHAR},
            {TdsDataType.TDS_BOUNDARY, ReadTDS_BOUNDARY},
            {TdsDataType.TDS_SENSITIVITY, ReadTDS_SENSITIVITY},
            {TdsDataType.TDS_BINARY, ReadTDS_BINARY},
            {TdsDataType.TDS_VARBINARY, ReadTDS_VARBINARY},
            {TdsDataType.TDS_LONGCHAR, ReadTDS_LONGCHAR},
            {TdsDataType.TDS_LONGBINARY, ReadTDS_LONGBINARY},
            {TdsDataType.TDS_DECN, ReadTDS_DECN},
            {TdsDataType.TDS_NUMN, ReadTDS_NUMN},
            {TdsDataType.TDS_MONEY, ReadTDS_MONEY},
            {TdsDataType.TDS_SHORTMONEY, ReadTDS_SHORTMONEY},
            {TdsDataType.TDS_MONEYN, ReadTDS_MONEYN},
            {TdsDataType.TDS_DATETIME, ReadTDS_DATETIME},
            {TdsDataType.TDS_SHORTDATE, ReadTDS_SHORTDATE},
            {TdsDataType.TDS_DATETIMEN, ReadTDS_DATETIMEN},
            {TdsDataType.TDS_BIGDATETIMEN, ReadTDS_BIGDATETIMEN},
            {TdsDataType.TDS_DATE, ReadTDS_DATE},
            {TdsDataType.TDS_DATEN, ReadTDS_DATEN},
            {TdsDataType.TDS_TIME, ReadTDS_TIME},
            {TdsDataType.TDS_TIMEN, ReadTDS_TIMEN},
            {TdsDataType.TDS_TEXT, ReadTDS_TEXT},
            {TdsDataType.TDS_XML, ReadTDS_XML},
            {TdsDataType.TDS_IMAGE, ReadTDS_IMAGE},
            {TdsDataType.TDS_UNITEXT, ReadTDS_UNITEXT},
        };

        private static object ReadInternal(Stream stream, FormatItem format, DbEnvironment env, ref bool streamExceeded)
        {
            if (ReadMap.ContainsKey(format.DataType))
            {
                return ReadMap[format.DataType](stream, format, env, ref streamExceeded) ?? DBNull.Value;
            }

            Debug.Assert(false, $"Unsupported data type {format.DataType}");

            return DBNull.Value; // Catch-all.
        }

        private static object ReadTDS_BIT(Stream stream, FormatItem format, DbEnvironment env, ref bool streamExceeded)
        {
            return stream.ReadBool(ref streamExceeded);
        }

        private static object ReadTDS_INT1(Stream stream, FormatItem format, DbEnvironment env, ref bool streamExceeded)
        {
            return (byte)stream.ReadByte(ref streamExceeded);
        }

        private static object ReadTDS_SINT1(Stream stream, FormatItem format, DbEnvironment env, ref bool streamExceeded)
        {
            return (sbyte)stream.ReadByte(ref streamExceeded);
        }

        private static object ReadTDS_INT2(Stream stream, FormatItem format, DbEnvironment env, ref bool streamExceeded)
        {
            return stream.ReadShort(ref streamExceeded);
        }

        private static object ReadTDS_UINT2(Stream stream, FormatItem format, DbEnvironment env, ref bool streamExceeded)
        {
            return stream.ReadUShort(ref streamExceeded);
        }

        private static object ReadTDS_INT4(Stream stream, FormatItem format, DbEnvironment env, ref bool streamExceeded)
        {
            return stream.ReadInt(ref streamExceeded);
        }

        private static object ReadTDS_UINT4(Stream stream, FormatItem format, DbEnvironment env, ref bool streamExceeded) { return stream.ReadUInt(ref streamExceeded); }

        private static object ReadTDS_INT8(Stream stream, FormatItem format, DbEnvironment env, ref bool streamExceeded) { return stream.ReadLong(ref streamExceeded); }

        private static object ReadTDS_UINT8(Stream stream, FormatItem format, DbEnvironment env, ref bool streamExceeded) { return stream.ReadULong(ref streamExceeded); }

        private static object ReadTDS_INTN(Stream stream, FormatItem format, DbEnvironment env, ref bool streamExceeded)
        {
            switch (stream.ReadByte(ref streamExceeded))
            {
                case 0: return DBNull.Value;
                case 1:
                    if (stream.CheckRequiredLength(1, ref streamExceeded) == false)
                        return (sbyte)0;
                    return (byte)stream.ReadByte(); //both INTN(1) and UINTN(1) are an INT1. Never an SINT1.
                case 2: return stream.ReadShort(ref streamExceeded);
                case 4: return stream.ReadInt(ref streamExceeded);
                case 8: return stream.ReadLong(ref streamExceeded);
            }

            return DBNull.Value;
        }

        private static object ReadTDS_UINTN(Stream stream, FormatItem format, DbEnvironment env, ref bool streamExceeded)
        {
            switch (stream.ReadByte(ref streamExceeded))
            {
                case 0: return DBNull.Value;
                case 1: return (byte)stream.ReadByte(ref streamExceeded);
                case 2: return stream.ReadUShort(ref streamExceeded);
                case 4: return stream.ReadUInt(ref streamExceeded);
                case 8: return stream.ReadULong(ref streamExceeded);
            }

            return DBNull.Value;
        }

        private static object ReadTDS_FLT4(Stream stream, FormatItem format, DbEnvironment env, ref bool streamExceeded) { return stream.ReadFloat(ref streamExceeded); }

        private static object ReadTDS_FLT8(Stream stream, FormatItem format, DbEnvironment env, ref bool streamExceeded) { return stream.ReadDouble(ref streamExceeded); }

        private static object ReadTDS_FLTN(Stream stream, FormatItem format, DbEnvironment env, ref bool streamExceeded)
        {
            switch (stream.ReadByte(ref streamExceeded))
            {
                case 0: return DBNull.Value;
                case 4: return stream.ReadFloat(ref streamExceeded);
                case 8: return stream.ReadDouble(ref streamExceeded);
            }

            return DBNull.Value;
        }

        private static object ReadTDS_CHAR(Stream stream, FormatItem format, DbEnvironment env, ref bool streamExceeded)
        {
            return stream.ReadNullableByteLengthPrefixedString(env.Encoding, ref streamExceeded);
        }

        private static object ReadTDS_VARCHAR(Stream stream, FormatItem format, DbEnvironment env, ref bool streamExceeded)
        {
            return stream.ReadNullableByteLengthPrefixedString(env.Encoding, ref streamExceeded);
        }

        private static object ReadTDS_BOUNDARY(Stream stream, FormatItem format, DbEnvironment env, ref bool streamExceeded)
        {
            return stream.ReadNullableByteLengthPrefixedString(env.Encoding, ref streamExceeded);
        }

        private static object ReadTDS_SENSITIVITY(Stream stream, FormatItem format, DbEnvironment env, ref bool streamExceeded)
        {
            return stream.ReadNullableByteLengthPrefixedString(env.Encoding, ref streamExceeded);
        }

        private static object ReadTDS_BINARY(Stream stream, FormatItem format, DbEnvironment env, ref bool streamExceeded)
        {
            return stream.ReadNullableByteLengthPrefixedByteArray(ref streamExceeded);
        }

        private static object ReadTDS_VARBINARY(Stream stream, FormatItem format, DbEnvironment env, ref bool streamExceeded)
        {
            return stream.ReadNullableByteLengthPrefixedByteArray(ref streamExceeded);
        }

        private static object ReadTDS_LONGCHAR(Stream stream, FormatItem format, DbEnvironment env, ref bool streamExceeded)
        {
            return stream.ReadNullableIntLengthPrefixedString(env.Encoding, ref streamExceeded);
        }

        /// <summary>
        /// TDS_LONGBINARY serialization 55 serialized java object or instance (i.e. java object)
        /// TDS_LONGBINARY serialized java class 56 serialized java class (i.e. byte code)
        /// TDS_LONGBINARY smallbinary 59 64K max length binary data (ASA)
        /// TDS_LONGBINARY unichar 34 fixed length UTF-16 encoded data
        /// TDS_LONGBINARY univarchar 35 variable length UTF-16 encoded data
        /// </summary>
        private static object ReadTDS_LONGBINARY(Stream stream, FormatItem format, DbEnvironment env, ref bool streamExceeded)
        {
            //the UserType can affect how we need to interpret the result data
            switch (format.UserType)
            {
                case 34:
                case 35:
                    return stream.ReadNullableIntLengthPrefixedString(Encoding.Unicode, ref streamExceeded);
                default:
                    return stream.ReadNullableIntLengthPrefixedByteArray(ref streamExceeded);
            }
        }

        private static object ReadTDS_DECN(Stream stream, FormatItem format, DbEnvironment env, ref bool streamExceeded)
        {
            var precision = format.Precision ?? 1;
            var scale = format.Scale ?? 0;

            Logger.Instance?.WriteLine($"  <- {format.DisplayColumnName} ({precision}, {scale})");

            var aseDecimal = stream.ReadAseDecimal(precision, scale, ref streamExceeded);

            return aseDecimal.HasValue
                ? env.UseAseDecimal
                    ? (object)aseDecimal.Value
                    : aseDecimal.Value.ToDecimal()
                : DBNull.Value;
        }

        private static object ReadTDS_NUMN(Stream stream, FormatItem format, DbEnvironment env, ref bool streamExceeded)
        {
            var precision = format.Precision ?? 1;
            var scale = format.Scale ?? 0;

            Logger.Instance?.WriteLine($"  <- {format.DisplayColumnName} ({precision}, {scale})");

            var aseDecimal = stream.ReadAseDecimal(precision, scale, ref streamExceeded);

            return aseDecimal.HasValue
                ? env.UseAseDecimal
                    ? (object)aseDecimal.Value
                    : aseDecimal.Value.ToDecimal()
                : DBNull.Value;
        }

        private static object ReadTDS_MONEY(Stream stream, FormatItem format, DbEnvironment env, ref bool streamExceeded)
        {
            return stream.ReadMoney(ref streamExceeded);
        }

        private static object ReadTDS_SHORTMONEY(Stream stream, FormatItem format, DbEnvironment env, ref bool streamExceeded)
        {
            return stream.ReadSmallMoney(ref streamExceeded);
        }

        private static object ReadTDS_MONEYN(Stream stream, FormatItem format, DbEnvironment env, ref bool streamExceeded)
        {
            switch (stream.ReadByte(ref streamExceeded))
            {
                case 0: return DBNull.Value;
                case 4:
                    return stream.ReadSmallMoney(ref streamExceeded);
                case 8:
                    return stream.ReadMoney(ref streamExceeded);
            }

            return DBNull.Value;
        }

        private static object ReadTDS_DATETIME(Stream stream, FormatItem format, DbEnvironment env, ref bool streamExceeded)
        {
            return stream.ReadIntPartDateTime(ref streamExceeded);
        }

        private static object ReadTDS_SHORTDATE(Stream stream, FormatItem format, DbEnvironment env, ref bool streamExceeded)
        {
            return stream.ReadShortPartDateTime(ref streamExceeded);
        }

        private static object ReadTDS_DATETIMEN(Stream stream, FormatItem format, DbEnvironment env, ref bool streamExceeded)
        {
            switch (stream.ReadByte(ref streamExceeded))
            {
                case 0: return DBNull.Value;
                case 4:
                    return stream.ReadShortPartDateTime(ref streamExceeded);
                case 8:
                    return stream.ReadIntPartDateTime(ref streamExceeded);
            }

            return DBNull.Value;
        }

        private static object ReadTDS_BIGDATETIMEN(Stream stream, FormatItem format, DbEnvironment env, ref bool streamExceeded)
        {
            switch (stream.ReadByte(ref streamExceeded))
            {
                case 8: return stream.ReadBigDateTime(ref streamExceeded);
            }

            return DBNull.Value;
        }

        private static object ReadTDS_DATE(Stream stream, FormatItem format, DbEnvironment env, ref bool streamExceeded)
        {
            return stream.ReadDate(ref streamExceeded);
        }

        private static object ReadTDS_DATEN(Stream stream, FormatItem format, DbEnvironment env, ref bool streamExceeded)
        {
            switch (stream.ReadByte(ref streamExceeded))
            {
                case 0: return DBNull.Value;
                case 4:
                    return stream.ReadDate(ref streamExceeded);
            }

            return DBNull.Value;
        }

        private static object ReadTDS_TIME(Stream stream, FormatItem format, DbEnvironment env, ref bool streamExceeded)
        {
            return stream.ReadTime(ref streamExceeded);
        }

        private static object ReadTDS_TIMEN(Stream stream, FormatItem format, DbEnvironment env, ref bool streamExceeded)
        {
            switch (stream.ReadByte(ref streamExceeded))
            {
                case 0: return DBNull.Value;
                case 4:
                    return stream.ReadTime(ref streamExceeded);
            }

            return DBNull.Value;
        }

        private static object ReadTDS_TEXT(Stream stream, FormatItem format, DbEnvironment env, ref bool streamExceeded)
        {
            var textPtrLen = (byte)stream.ReadByte(ref streamExceeded);
            if (textPtrLen <= 0 || streamExceeded)
            {
                return DBNull.Value;
            }
            //var textPtr = new byte[textPtrLen];
            //stream.Read(textPtr, 0, textPtrLen);
            //stream.ReadULong(ref streamExceeded); //timestamp
            if (stream.CheckRequiredLength(textPtrLen + 4, ref streamExceeded) == false)
                return DBNull.Value;
            stream.Seek(textPtrLen + 4, SeekOrigin.Current);
            return stream.ReadNullableIntLengthPrefixedString(env.Encoding, ref streamExceeded);
        }

        private static object ReadTDS_XML(Stream stream, FormatItem format, DbEnvironment env, ref bool streamExceeded)
        {
            var textPtrLen = (byte)stream.ReadByte(ref streamExceeded);
            if (textPtrLen <= 0 || streamExceeded)
            {
                return DBNull.Value;
            }
            //var textPtr = new byte[textPtrLen];
            //stream.Read(textPtr, 0, textPtrLen);
            //stream.ReadULong(ref streamExceeded); //timestamp
            if (stream.CheckRequiredLength(textPtrLen + 4, ref streamExceeded) == false)
                return DBNull.Value;
            stream.Seek(textPtrLen + 4, SeekOrigin.Current);
            return stream.ReadNullableIntLengthPrefixedString(env.Encoding, ref streamExceeded);
        }

        private static object ReadTDS_IMAGE(Stream stream, FormatItem format, DbEnvironment env, ref bool streamExceeded)
        {
            var textPtrLen = (byte)stream.ReadByte(ref streamExceeded);
            if (textPtrLen == 0 || streamExceeded)
            {
                return DBNull.Value;
            }
            //var textPtr = new byte[textPtrLen];
            //stream.Read(textPtr, 0, textPtrLen);
            //stream.ReadULong(ref streamExceeded); //timestamp
            if (stream.CheckRequiredLength(textPtrLen + 4, ref streamExceeded) == false)
                return DBNull.Value;
            stream.Seek(textPtrLen + 4, SeekOrigin.Current);
            var dataLen = stream.ReadInt(ref streamExceeded);
            if (dataLen <= 0)
            {
                return DBNull.Value;
            }
            if (stream.CheckRequiredLength(dataLen, ref streamExceeded) == false)
                return DBNull.Value;
            var data = new byte[dataLen];
            stream.Read(data, 0, dataLen);
            return data;
        }

        private static object ReadTDS_UNITEXT(Stream stream, FormatItem format, DbEnvironment env, ref bool streamExceeded)
        {
            var textPtrLen = (byte)stream.ReadByte(ref streamExceeded);
            if (textPtrLen <= 0 || streamExceeded)
            {
                return DBNull.Value;
            }
            //var textPtr = new byte[textPtrLen];
            //stream.Read(textPtr, 0, textPtrLen);
            //stream.ReadULong(ref streamExceeded); //timestamp
            if (stream.CheckRequiredLength(textPtrLen + 4, ref streamExceeded) == false)
                return DBNull.Value;
            stream.Seek(textPtrLen + 4, SeekOrigin.Current);
            return stream.ReadNullableIntLengthPrefixedString(Encoding.Unicode, ref streamExceeded);
        }
    }
}
