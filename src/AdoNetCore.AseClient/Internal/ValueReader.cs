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
        public static object Read(Stream stream, FormatItem format, DbEnvironment env)
        {
            return ReadInternal(stream, format, env) ?? DBNull.Value;
        }

        private static readonly Dictionary<TdsDataType, Func<Stream, FormatItem, DbEnvironment, object>> ReadMap = new Dictionary<TdsDataType, Func<Stream, FormatItem, DbEnvironment, object>>
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

        private static object ReadInternal(Stream stream, FormatItem format, DbEnvironment env)
        {
            if (ReadMap.ContainsKey(format.DataType))
            {
                return ReadMap[format.DataType](stream, format, env) ?? DBNull.Value;
            }

            Debug.Assert(false, $"Unsupported data type {format.DataType}");

            return DBNull.Value; // Catch-all.
        }

        private static object ReadTDS_BIT(Stream stream, FormatItem format, DbEnvironment env)
        {
            return stream.ReadBool();
        }

        private static object ReadTDS_INT1(Stream stream, FormatItem format, DbEnvironment env)
        {
            return (byte)stream.ReadByte();
        }

        private static object ReadTDS_SINT1(Stream stream, FormatItem format, DbEnvironment env)
        {
            return (sbyte)stream.ReadByte();
        }

        private static object ReadTDS_INT2(Stream stream, FormatItem format, DbEnvironment env)
        {
            return stream.ReadShort();
        }

        private static object ReadTDS_UINT2(Stream stream, FormatItem format, DbEnvironment env)
        {
            return stream.ReadUShort();
        }

        private static object ReadTDS_INT4(Stream stream, FormatItem format, DbEnvironment env)
        {
            return stream.ReadInt();
        }

        private static object ReadTDS_UINT4(Stream stream, FormatItem format, DbEnvironment env) { return stream.ReadUInt(); }

        private static object ReadTDS_INT8(Stream stream, FormatItem format, DbEnvironment env) { return stream.ReadLong(); }

        private static object ReadTDS_UINT8(Stream stream, FormatItem format, DbEnvironment env) { return stream.ReadULong(); }

        private static object ReadTDS_INTN(Stream stream, FormatItem format, DbEnvironment env)
        {
            switch (stream.ReadByte())
            {
                case 0: return DBNull.Value;
                case 1: return (byte)stream.ReadByte(); //both INTN(1) and UINTN(1) are an INT1. Never an SINT1.
                case 2: return stream.ReadShort();
                case 4: return stream.ReadInt();
                case 8: return stream.ReadLong();
            }

            return DBNull.Value;
        }

        private static object ReadTDS_UINTN(Stream stream, FormatItem format, DbEnvironment env)
        {
            switch (stream.ReadByte())
            {
                case 0: return DBNull.Value;
                case 1: return (byte)stream.ReadByte();
                case 2: return stream.ReadUShort();
                case 4: return stream.ReadUInt();
                case 8: return stream.ReadULong();
            }

            return DBNull.Value;
        }

        private static object ReadTDS_FLT4(Stream stream, FormatItem format, DbEnvironment env) { return stream.ReadFloat(); }

        private static object ReadTDS_FLT8(Stream stream, FormatItem format, DbEnvironment env) { return stream.ReadDouble(); }

        private static object ReadTDS_FLTN(Stream stream, FormatItem format, DbEnvironment env)
        {
            switch (stream.ReadByte())
            {
                case 0: return DBNull.Value;
                case 4: return stream.ReadFloat();
                case 8: return stream.ReadDouble();
            }

            return DBNull.Value;
        }

        private static object ReadTDS_CHAR(Stream stream, FormatItem format, DbEnvironment env)
        {
            return stream.ReadNullableByteLengthPrefixedString(env.Encoding);
        }

        private static object ReadTDS_VARCHAR(Stream stream, FormatItem format, DbEnvironment env)
        {
            return stream.ReadNullableByteLengthPrefixedString(env.Encoding);
        }

        private static object ReadTDS_BOUNDARY(Stream stream, FormatItem format, DbEnvironment env)
        {
            return stream.ReadNullableByteLengthPrefixedString(env.Encoding);
        }

        private static object ReadTDS_SENSITIVITY(Stream stream, FormatItem format, DbEnvironment env)
        {
            return stream.ReadNullableByteLengthPrefixedString(env.Encoding);
        }

        private static object ReadTDS_BINARY(Stream stream, FormatItem format, DbEnvironment env)
        {
            return stream.ReadNullableByteLengthPrefixedByteArray();
        }

        private static object ReadTDS_VARBINARY(Stream stream, FormatItem format, DbEnvironment env)
        {
            return stream.ReadNullableByteLengthPrefixedByteArray();
        }

        private static object ReadTDS_LONGCHAR(Stream stream, FormatItem format, DbEnvironment env)
        {
            return stream.ReadNullableIntLengthPrefixedString(env.Encoding);
        }

        /// <summary>
        /// TDS_LONGBINARY serialization 55 serialized java object or instance (i.e. java object)
        /// TDS_LONGBINARY serialized java class 56 serialized java class (i.e. byte code)
        /// TDS_LONGBINARY smallbinary 59 64K max length binary data (ASA)
        /// TDS_LONGBINARY unichar 34 fixed length UTF-16 encoded data
        /// TDS_LONGBINARY univarchar 35 variable length UTF-16 encoded data
        /// </summary>
        private static object ReadTDS_LONGBINARY(Stream stream, FormatItem format, DbEnvironment env)
        {
            //the UserType can affect how we need to interpret the result data
            switch (format.UserType)
            {
                case 34:
                case 35:
                    return stream.ReadNullableIntLengthPrefixedString(Encoding.Unicode);
                default:
                    return stream.ReadNullableIntLengthPrefixedByteArray();
            }
        }

        private static object ReadTDS_DECN(Stream stream, FormatItem format, DbEnvironment env)
        {
            var precision = format.Precision ?? 1;
            var scale = format.Scale ?? 0;

            Logger.Instance?.WriteLine($"  <- {format.DisplayColumnName} ({precision}, {scale})");

            var aseDecimal = stream.ReadAseDecimal(precision, scale);

            return aseDecimal.HasValue
                ? env.UseAseDecimal
                    ? (object)aseDecimal.Value
                    : aseDecimal.Value.ToDecimal()
                : DBNull.Value;
        }

        private static object ReadTDS_NUMN(Stream stream, FormatItem format, DbEnvironment env)
        {
            var precision = format.Precision ?? 1;
            var scale = format.Scale ?? 0;

            Logger.Instance?.WriteLine($"  <- {format.DisplayColumnName} ({precision}, {scale})");

            var aseDecimal = stream.ReadAseDecimal(precision, scale);

            return aseDecimal.HasValue
                ? env.UseAseDecimal
                    ? (object)aseDecimal.Value
                    : aseDecimal.Value.ToDecimal()
                : DBNull.Value;
        }

        private static object ReadTDS_MONEY(Stream stream, FormatItem format, DbEnvironment env)
        {
            return stream.ReadMoney();
        }

        private static object ReadTDS_SHORTMONEY(Stream stream, FormatItem format, DbEnvironment env)
        {
            return stream.ReadSmallMoney();
        }

        private static object ReadTDS_MONEYN(Stream stream, FormatItem format, DbEnvironment env)
        {
            switch (stream.ReadByte())
            {
                case 0: return DBNull.Value;
                case 4:
                    return stream.ReadSmallMoney();
                case 8:
                    return stream.ReadMoney();
            }

            return DBNull.Value;
        }

        private static object ReadTDS_DATETIME(Stream stream, FormatItem format, DbEnvironment env)
        {
            return stream.ReadIntPartDateTime();
        }

        private static object ReadTDS_SHORTDATE(Stream stream, FormatItem format, DbEnvironment env)
        {
            return stream.ReadShortPartDateTime();
        }

        private static object ReadTDS_DATETIMEN(Stream stream, FormatItem format, DbEnvironment env)
        {
            switch (stream.ReadByte())
            {
                case 0: return DBNull.Value;
                case 4:
                    return stream.ReadShortPartDateTime();
                case 8:
                    return stream.ReadIntPartDateTime();
            }

            return DBNull.Value;
        }

        private static object ReadTDS_BIGDATETIMEN(Stream stream, FormatItem format, DbEnvironment env)
        {
            switch (stream.ReadByte())
            {
                case 8: return stream.ReadBigDateTime();
            }

            return DBNull.Value;
        }

        private static object ReadTDS_DATE(Stream stream, FormatItem format, DbEnvironment env)
        {
            return stream.ReadDate();
        }

        private static object ReadTDS_DATEN(Stream stream, FormatItem format, DbEnvironment env)
        {
            switch (stream.ReadByte())
            {
                case 0: return DBNull.Value;
                case 4:
                    return stream.ReadDate();
            }

            return DBNull.Value;
        }

        private static object ReadTDS_TIME(Stream stream, FormatItem format, DbEnvironment env)
        {
            return stream.ReadTime();
        }

        private static object ReadTDS_TIMEN(Stream stream, FormatItem format, DbEnvironment env)
        {
            switch (stream.ReadByte())
            {
                case 0: return DBNull.Value;
                case 4:
                    return stream.ReadTime();
            }

            return DBNull.Value;
        }

        private static object ReadTDS_TEXT(Stream stream, FormatItem format, DbEnvironment env)
        {
            var textPtrLen = (byte)stream.ReadByte();
            if (textPtrLen == 0)
            {
                return DBNull.Value;
            }
            var textPtr = new byte[textPtrLen];
            stream.Read(textPtr, 0, textPtrLen);
            stream.ReadULong(); //timestamp
            return stream.ReadNullableIntLengthPrefixedString(env.Encoding);
        }

        private static object ReadTDS_XML(Stream stream, FormatItem format, DbEnvironment env)
        {
            var textPtrLen = (byte)stream.ReadByte();
            if (textPtrLen == 0)
            {
                return DBNull.Value;
            }
            var textPtr = new byte[textPtrLen];
            stream.Read(textPtr, 0, textPtrLen);
            stream.ReadULong(); //timestamp
            return stream.ReadNullableIntLengthPrefixedString(env.Encoding);
        }

        private static object ReadTDS_IMAGE(Stream stream, FormatItem format, DbEnvironment env)
        {
            var textPtrLen = (byte)stream.ReadByte();
            if (textPtrLen == 0)
            {
                return DBNull.Value;
            }
            var textPtr = new byte[textPtrLen];
            stream.Read(textPtr, 0, textPtrLen);
            stream.ReadULong(); //timestamp
            var dataLen = stream.ReadInt();
            if (dataLen == 0)
            {
                return DBNull.Value;
            }
            var data = new byte[dataLen];
            stream.Read(data, 0, dataLen);
            return data;
        }

        private static object ReadTDS_UNITEXT(Stream stream, FormatItem format, DbEnvironment env)
        {
            var textPtrLen = (byte)stream.ReadByte();
            if (textPtrLen == 0)
            {
                return DBNull.Value;
            }
            var textPtr = new byte[textPtrLen];
            stream.Read(textPtr, 0, textPtrLen);
            stream.ReadULong(); //timestamp
            return stream.ReadNullableIntLengthPrefixedString(Encoding.Unicode);
        }
    }
}
