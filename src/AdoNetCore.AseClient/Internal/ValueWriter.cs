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
        private static readonly Dictionary<Type, Dictionary<Type, Func<object, object>>> CastMap = new Dictionary<Type, Dictionary<Type, Func<object, object>>>
        {
            {
                typeof(Guid), new Dictionary<Type, Func<object, object>>
                {
                    {typeof(byte[]), o => ((Guid) o).ToByteArray()}
                }
            }
        };

        private static T Cast<T>(object value, FormatItem format)
        {
            try
            {
                var tFrom = value.GetType();
                var tTo = typeof(T);

                if (tTo == tFrom)
                {
                    return (T) value;
                }

                return (T) (CastMap.ContainsKey(tFrom) && CastMap[tFrom].ContainsKey(tTo)
                    ? CastMap[tFrom][tTo](value)
                    : Convert.ChangeType(value, tTo));
            }
            catch (InvalidCastException)
            {
                throw new InvalidCastException($"Specified cast is not valid. Parameter: {format.ParameterName}. From: {value.GetType().Name} to: {typeof(T).Name}");
            }
        }

        public static void Write(object value, Stream stream, FormatItem format, Encoding enc)
        {
            switch (format.DataType)
            {
                case TdsDataType.TDS_BIT:
                    switch (value)
                    {
                        case bool b:
                            stream.WriteBool(b);
                            break;
                        default:
                            stream.WriteByte(0);
                            break;
                    }
                    break;
                case TdsDataType.TDS_INT1:
                    stream.WriteByte(Cast<byte>(value, format));
                    break;
                //no TDS_SINT1, we will transmit as an INTN(2)
                case TdsDataType.TDS_INT2:
                    stream.WriteShort(Cast<short>(value, format));
                    break;
                case TdsDataType.TDS_UINT2:
                    stream.WriteUShort(Cast<ushort>(value, format));
                    break;
                case TdsDataType.TDS_INT4:
                    stream.WriteInt(Cast<int>(value, format));
                    break;
                case TdsDataType.TDS_UINT4:
                    stream.WriteUInt(Cast<uint>(value, format));
                    break;
                case TdsDataType.TDS_INT8:
                    stream.WriteLong(Cast<long>(value, format));
                    break;
                case TdsDataType.TDS_UINT8:
                    stream.WriteULong(Cast<ulong>(value, format));
                    break;
                case TdsDataType.TDS_INTN:
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
                    break;
                case TdsDataType.TDS_UINTN:
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
                    break;
                case TdsDataType.TDS_FLT4:
                    stream.WriteFloat(Cast<float>(value, format));
                    break;
                case TdsDataType.TDS_FLT8:
                    stream.WriteDouble(Cast<double>(value, format));
                    break;
                case TdsDataType.TDS_FLTN:
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
                    break;
                case TdsDataType.TDS_VARCHAR:
                    if (!stream.TryWriteBytePrefixedNull(value))
                    {
                        stream.WriteBytePrefixedString(Cast<string>(value, format), enc);
                    }
                    break;
                case TdsDataType.TDS_LONGCHAR:
                    if (!stream.TryWriteIntPrefixedNull(value))
                    {
                        stream.WriteIntPrefixedString(Cast<string>(value, format), enc);
                    }
                    break;
                case TdsDataType.TDS_VARBINARY:
                case TdsDataType.TDS_BINARY:
                    if (!stream.TryWriteBytePrefixedNull(value))
                    {
                        stream.WriteBytePrefixedByteArray(Cast<byte[]>(value, format));
                    }
                    break;
                case TdsDataType.TDS_LONGBINARY:
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
                    break;
                case TdsDataType.TDS_DECN:
                case TdsDataType.TDS_NUMN:
                    if (!stream.TryWriteBytePrefixedNull(value))
                    {
                        switch (value)
                        {
                            case AseDecimal ad:
                                stream.WriteDecimal(ad);
                                break;
                            default:
                                stream.WriteDecimal(Cast<decimal>(value, format));
                                break;
                        }
                    }
                    break;
                case TdsDataType.TDS_DATETIME:
                    stream.WriteIntPartDateTime(Cast<DateTime>(value, format));
                    break;
                case TdsDataType.TDS_DATETIMEN:
                    if (!stream.TryWriteBytePrefixedNull(value))
                    {
                        stream.WriteIntPartDateTime(Cast<DateTime>(value, format));
                    }
                    break;
                case TdsDataType.TDS_DATE:
                    stream.WriteDate(Cast<DateTime>(value, format));
                    break;
                case TdsDataType.TDS_DATEN:
                    if (!stream.TryWriteBytePrefixedNull(value))
                    {
                        stream.WriteDate(Cast<DateTime>(value, format));
                    }
                    break;
                case TdsDataType.TDS_TIME:
                    stream.WriteTime(Cast<TimeSpan>(value, format));
                    break;
                case TdsDataType.TDS_TIMEN:
                    if (!stream.TryWriteBytePrefixedNull(value))
                    {
                        stream.WriteTime(Cast<TimeSpan>(value, format));
                    }
                    break;
                case TdsDataType.TDS_MONEYN:
                    if (!stream.TryWriteBytePrefixedNull(value))
                    {
                        stream.WriteMoney(Cast<decimal>(value, format));
                    }
                    break;
                default:
                    Debug.Assert(false, $"Unsupported data type {format.DataType}");
                    break;
            }
        }
    }
}
