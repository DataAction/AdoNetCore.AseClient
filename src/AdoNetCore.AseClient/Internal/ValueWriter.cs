using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using AdoNetCore.AseClient.Enum;

namespace AdoNetCore.AseClient.Internal
{
    internal static class ValueWriter
    {
        private static readonly Dictionary<Type, Dictionary<Type, Func<object, Encoding, object>>> CastMap = new Dictionary<Type, Dictionary<Type, Func<object, Encoding, object>>>
        {
            {
                typeof(Guid), new Dictionary<Type, Func<object, Encoding, object>>
                {
                    {typeof(byte[]), (o, _) => ((Guid) o).ToByteArray()}
                }
            },
            {
                typeof(DateTime), new Dictionary<Type, Func<object, Encoding, object>>
                {
                    {typeof(TimeSpan), (o, _) => ((DateTime) o).TimeOfDay}
                }
            },
            {
                typeof(TimeSpan), new Dictionary<Type, Func<object, Encoding, object>>
                {
                    {typeof(DateTime), (o, _) => Constants.Sql.RegularDateTime.Epoch + (TimeSpan) o}
                }
            },
            {
                typeof(string), new Dictionary<Type, Func<object, Encoding, object>>
                {
                    {typeof(byte[]), (o, e) => e.GetBytes((string) o)}
                }
            },
            {
                typeof(char[]), new Dictionary<Type, Func<object, Encoding, object>>
                {
                    {typeof(string), (o, e) => new string((char[]) o)}
                }
            }
        };

        private static T Cast<T>(object value, FormatItem format, Encoding enc)
        {
            try
            {
                var tFrom = value.GetType();
                var tTo = typeof(T);

                if (tTo == tFrom)
                {
                    return (T)value;
                }

                return (T)(CastMap.ContainsKey(tFrom) && CastMap[tFrom].ContainsKey(tTo)
                    ? CastMap[tFrom][tTo](value, enc)
                    : Convert.ChangeType(value, tTo));
            }
            catch (InvalidCastException)
            {
                throw new InvalidCastException($"Specified cast is not valid. Parameter: {format.ParameterName}. From: {value.GetType().Name} to: {typeof(T).Name}");
            }
        }

        private static readonly Dictionary<TdsDataType, Action<object, Stream, FormatItem, Encoding>> WriteMap = new Dictionary<TdsDataType, Action<object, Stream, FormatItem, Encoding>>
        {
            { TdsDataType.TDS_BIT, WriteTDS_BIT },
            { TdsDataType.TDS_INT1, WriteTDS_INT1 },
            //no TDS_SINT1, we will transmit as an INTN(2)
            { TdsDataType.TDS_INT2, WriteTDS_INT2 },
            { TdsDataType.TDS_UINT2, WriteTDS_UINT2 },
            { TdsDataType.TDS_INT4, WriteTDS_INT4 },
            { TdsDataType.TDS_UINT4, WriteTDS_UINT4 },
            { TdsDataType.TDS_INT8, WriteTDS_INT8 },
            { TdsDataType.TDS_UINT8, WriteTDS_UINT8 },
            { TdsDataType.TDS_INTN, WriteTDS_INTN },
            { TdsDataType.TDS_UINTN, WriteTDS_UINTN },
            { TdsDataType.TDS_FLT4, WriteTDS_FLT4 },
            { TdsDataType.TDS_FLT8, WriteTDS_FLT8 },
            { TdsDataType.TDS_FLTN, WriteTDS_FLTN },
            { TdsDataType.TDS_VARCHAR, WriteTDS_VARCHAR },
            { TdsDataType.TDS_LONGCHAR, WriteTDS_LONGCHAR },
            { TdsDataType.TDS_VARBINARY, WriteTDS_VARBINARY },
            { TdsDataType.TDS_BINARY, WriteTDS_BINARY },
            { TdsDataType.TDS_LONGBINARY, WriteTDS_LONGBINARY },
            { TdsDataType.TDS_BLOB, WriteTDS_BLOB },
            { TdsDataType.TDS_DECN, WriteTDS_DECN },
            { TdsDataType.TDS_NUMN, WriteTDS_NUMN },
            { TdsDataType.TDS_DATETIME, WriteTDS_DATETIME },
            { TdsDataType.TDS_DATETIMEN, WriteTDS_DATETIMEN },
            { TdsDataType.TDS_BIGDATETIMEN, WriteTDS_BIGDATETIMEN },
            { TdsDataType.TDS_DATE, WriteTDS_DATE },
            { TdsDataType.TDS_DATEN, WriteTDS_DATEN },
            { TdsDataType.TDS_TIME, WriteTDS_TIME },
            { TdsDataType.TDS_TIMEN, WriteTDS_TIMEN },
            { TdsDataType.TDS_MONEYN, WriteTDS_MONEYN },
        };

        public static void Write(object value, Stream stream, FormatItem format, Encoding enc)
        {
            if (WriteMap.ContainsKey(format.DataType))
            {
                WriteMap[format.DataType](value, stream, format, enc);
            }
            else
            {
                Debug.Assert(false, $"Unsupported data type {format.DataType}");
            }
        }

        private static void WriteTDS_BIT(object value, Stream stream, FormatItem format, Encoding enc)
        {
            switch (value)
            {
                case bool b:
                    stream.WriteBool(b);
                    break;
                default:
                    stream.WriteByte(0);
                    break;
            }
        }

        private static void WriteTDS_INT1(object value, Stream stream, FormatItem format, Encoding enc)
        {
            stream.WriteByte(Cast<byte>(value, format, enc));
        }

        private static void WriteTDS_INT2(object value, Stream stream, FormatItem format, Encoding enc)
        {
            stream.WriteShort(Cast<short>(value, format, enc));
        }

        private static void WriteTDS_UINT2(object value, Stream stream, FormatItem format, Encoding enc)
        {
            stream.WriteUShort(Cast<ushort>(value, format, enc));
        }

        private static void WriteTDS_INT4(object value, Stream stream, FormatItem format, Encoding enc)
        {
            stream.WriteInt(Cast<int>(value, format, enc));
        }

        private static void WriteTDS_UINT4(object value, Stream stream, FormatItem format, Encoding enc)
        {
            stream.WriteUInt(Cast<uint>(value, format, enc));
        }

        private static void WriteTDS_INT8(object value, Stream stream, FormatItem format, Encoding enc)
        {
            stream.WriteLong(Cast<long>(value, format, enc));
        }

        private static void WriteTDS_UINT8(object value, Stream stream, FormatItem format, Encoding enc)
        {
            stream.WriteULong(Cast<ulong>(value, format, enc));
        }

        private static void WriteTDS_INTN(object value, Stream stream, FormatItem format, Encoding enc)
        {
            switch (value)
            {
                case byte b:
                    stream.WriteByte(1);
                    stream.WriteByte(b);
                    break;
                case sbyte sb:
                    stream.WriteByte(2);
                    stream.WriteShort(sb);
                    break;
                case short s:
                    stream.WriteByte(2);
                    stream.WriteShort(s);
                    break;
                case int i:
                    stream.WriteByte(4);
                    stream.WriteInt(i);
                    break;
                case long l:
                    stream.WriteByte(8);
                    stream.WriteLong(l);
                    break;
                //case null:
                default:
                    stream.WriteByte(0);
                    break;
            }
        }

        private static void WriteTDS_UINTN(object value, Stream stream, FormatItem format, Encoding enc)
        {
            switch (value)
            {
                case byte b:
                    stream.WriteByte(1);
                    stream.WriteByte(b);
                    break;
                case ushort s:
                    stream.WriteByte(2);
                    stream.WriteUShort(s);
                    break;
                case uint i:
                    stream.WriteByte(4);
                    stream.WriteUInt(i);
                    break;
                case ulong l:
                    stream.WriteByte(8);
                    stream.WriteULong(l);
                    break;
                //case null:
                default:
                    stream.WriteByte(0);
                    break;
            }
        }

        private static void WriteTDS_FLT4(object value, Stream stream, FormatItem format, Encoding enc)
        {
            stream.WriteFloat(Cast<float>(value, format, enc));
        }

        private static void WriteTDS_FLT8(object value, Stream stream, FormatItem format, Encoding enc)
        {
            stream.WriteDouble(Cast<double>(value, format, enc));
        }

        private static void WriteTDS_FLTN(object value, Stream stream, FormatItem format, Encoding enc)
        {
            switch (value)
            {
                case float f:
                    stream.WriteByte(4);
                    stream.WriteFloat(f);
                    break;
                case double d:
                    stream.WriteByte(8);
                    stream.WriteDouble(d);
                    break;
                default:
                    stream.WriteByte(0);
                    break;
            }
        }

        private static void WriteTDS_VARCHAR(object value, Stream stream, FormatItem format, Encoding enc)
        {
            if (!stream.TryWriteBytePrefixedNull(value))
            {
                stream.WriteBytePrefixedString(Cast<string>(value, format, enc), enc);
            }
        }

        private static void WriteTDS_LONGCHAR(object value, Stream stream, FormatItem format, Encoding enc)
        {
            if (!stream.TryWriteIntPrefixedNull(value))
            {
                stream.WriteIntPrefixedString(Cast<string>(value, format, enc), enc);
            }
        }

        private static void WriteTDS_VARBINARY(object value, Stream stream, FormatItem format, Encoding enc)
        {
            if (!stream.TryWriteBytePrefixedNull(value))
            {
                stream.WriteBytePrefixedByteArray(Cast<byte[]>(value, format, enc));
            }
        }

        private static void WriteTDS_BINARY(object value, Stream stream, FormatItem format, Encoding enc)
        {
            if (!stream.TryWriteBytePrefixedNull(value))
            {
                stream.WriteBytePrefixedByteArray(Cast<byte[]>(value, format, enc));
            }
        }

        private static void WriteTDS_LONGBINARY(object value, Stream stream, FormatItem format, Encoding enc)
        {
            if (!stream.TryWriteIntPrefixedNull(value))
            {
                switch (value)
                {
                    case string s:
                        stream.WriteIntPrefixedByteArray(Encoding.Unicode.GetBytes(s));
                        break;
                    case char c:
                        stream.WriteIntPrefixedByteArray(Encoding.Unicode.GetBytes(new[] { c }));
                        break;
                    case byte[] ba:
                        stream.WriteIntPrefixedByteArray(ba);
                        break;
                    case byte b:
                        stream.WriteIntPrefixedByteArray(new[] { b });
                        break;
                    default:
                        stream.WriteInt(0);
                        break;
                }
            }
        }
        private static void WriteTDS_BLOB(object value, Stream stream, FormatItem format, Encoding enc)
        {
            //byte serialization type
            //short-prefixed class id
            //n chunks of data
            //    4-byte datalen (highest-order bit indicates if there are more chunks)
            //    data
            switch (value)
            {
                case string s:
                    stream.WriteByte((byte)SerializationType.SER_DEFAULT);
                    stream.WriteNullableUShortPrefixedByteArray(format.ClassId);
                    stream.WriteBlobSpecificIntPrefixedByteArray(Encoding.Unicode.GetBytes(s));
                    break;
                case byte[] ba:
                    stream.WriteByte((byte)SerializationType.SER_DEFAULT);
                    stream.WriteNullableUShortPrefixedByteArray(format.ClassId);
                    stream.WriteBlobSpecificIntPrefixedByteArray(ba);
                    break;
                default:
                    throw new AseException($"TDS_BLOB support for {value.GetType().Name} not yet implemented");
            }
        }

        private static void WriteTDS_DECN(object value, Stream stream, FormatItem format, Encoding enc)
        {
            if (!stream.TryWriteBytePrefixedNull(value))
            {
                switch (value)
                {
                    case AseDecimal ad:
                        stream.WriteDecimal(ad);
                        break;
                    default:
                        stream.WriteDecimal(Cast<decimal>(value, format, enc));
                        break;
                }
            }
        }

        private static void WriteTDS_NUMN(object value, Stream stream, FormatItem format, Encoding enc)
        {
            if (!stream.TryWriteBytePrefixedNull(value))
            {
                switch (value)
                {
                    case AseDecimal ad:
                        stream.WriteDecimal(ad);
                        break;
                    default:
                        stream.WriteDecimal(Cast<decimal>(value, format, enc));
                        break;
                }
            }
        }

        private static void WriteTDS_DATETIME(object value, Stream stream, FormatItem format, Encoding enc)
        {
            stream.WriteIntPartDateTime(Cast<DateTime>(value, format, enc));
        }

        private static void WriteTDS_DATETIMEN(object value, Stream stream, FormatItem format, Encoding enc)
        {
            if (!stream.TryWriteBytePrefixedNull(value))
            {
                stream.WriteIntPartDateTime(Cast<DateTime>(value, format, enc));
            }
        }

        private static void WriteTDS_BIGDATETIMEN(object value, Stream stream, FormatItem format, Encoding enc)
        {
            if (!stream.TryWriteBytePrefixedNull(value))
            {
                stream.WriteBigDateTime(Cast<DateTime>(value, format, enc));
            }
        }

        private static void WriteTDS_DATE(object value, Stream stream, FormatItem format, Encoding enc)
        {
            stream.WriteDate(Cast<DateTime>(value, format, enc));
        }

        private static void WriteTDS_DATEN(object value, Stream stream, FormatItem format, Encoding enc)
        {
            if (!stream.TryWriteBytePrefixedNull(value))
            {
                stream.WriteDate(Cast<DateTime>(value, format, enc));
            }
        }

        private static void WriteTDS_TIME(object value, Stream stream, FormatItem format, Encoding enc)
        {
            stream.WriteTime(Cast<TimeSpan>(value, format, enc));
        }

        private static void WriteTDS_TIMEN(object value, Stream stream, FormatItem format, Encoding enc)
        {
            if (!stream.TryWriteBytePrefixedNull(value))
            {
                stream.WriteTime(Cast<TimeSpan>(value, format, enc));
            }
        }

        private static void WriteTDS_MONEYN(object value, Stream stream, FormatItem format, Encoding enc)
        {
            if (!stream.TryWriteBytePrefixedNull(value))
            {
                stream.WriteMoney(Cast<decimal>(value, format, enc));
            }
        }
    }
}
