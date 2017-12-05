using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using AdoNetCore.AseClient.Enum;

namespace AdoNetCore.AseClient.Internal
{
    internal static class ValueWriter
    {
        public static void Write(object value, Stream stream, FormatItem format, Encoding enc)
        {
            switch (format.DataType)
            {
                case TdsDataType.TDS_BIT:
                    stream.WriteBool((bool)value);
                    break;
                case TdsDataType.TDS_INT1:
                    stream.WriteByte((byte)value);
                    break;
                //no TDS_SINT1, we will transmit as an INTN(2)
                case TdsDataType.TDS_INT2:
                    stream.WriteShort((short)value);
                    break;
                case TdsDataType.TDS_UINT2:
                    stream.WriteUShort((ushort)value);
                    break;
                case TdsDataType.TDS_INT4:
                    stream.WriteInt((int)value);
                    break;
                case TdsDataType.TDS_UINT4:
                    stream.WriteUInt((uint)value);
                    break;
                case TdsDataType.TDS_INT8:
                    stream.WriteLong((long)value);
                    break;
                case TdsDataType.TDS_UINT8:
                    stream.WriteULong((ulong)value);
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
                    stream.WriteFloat((float)value);
                    break;
                case TdsDataType.TDS_FLT8:
                    stream.WriteDouble((double)value);
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
                        stream.WriteBytePrefixedString((string)value, enc);
                    }
                    break;
                case TdsDataType.TDS_LONGCHAR:
                    if (!stream.TryWriteIntPrefixedNull(value))
                    {
                        stream.WriteIntPrefixedString((string)value, enc);
                    }
                    break;
                case TdsDataType.TDS_VARBINARY:
                case TdsDataType.TDS_BINARY:
                    if (!stream.TryWriteBytePrefixedNull(value))
                    {
                        stream.WriteBytePrefixedByteArray((byte[])value);
                    }
                    break;
                case TdsDataType.TDS_LONGBINARY:
                    if (!stream.TryWriteIntPrefixedNull(value))
                    {
                        stream.WriteIntPrefixedByteArray((byte[])value);
                    }
                    break;
                case TdsDataType.TDS_DECN:
                    if (!stream.TryWriteBytePrefixedNull(value))
                    {
                        var sqlDecimal = (SqlDecimal)(decimal)value;
                        stream.WriteByte(17);
                        stream.WriteByte(sqlDecimal.IsPositive ? (byte)0 : (byte)1);
                        var data = sqlDecimal.BinData;
                        data = new[]
                        {
                            data[15], data[14], data[13], data[12],
                            data[11], data[10], data[9], data[8],
                            data[7], data[6], data[5], data[4],
                            data[3], data[2], data[1], data[0],
                        };
                        stream.Write(data, 0, 16);
                    }
                    break;
                case TdsDataType.TDS_DATETIME:
                    stream.WriteIntPartDateTime((DateTime)value);
                    break;
                case TdsDataType.TDS_DATETIMEN:
                    if (!stream.TryWriteBytePrefixedNull(value))
                    {
                        stream.WriteIntPartDateTime((DateTime)value);
                    }
                    break;
                case TdsDataType.TDS_DATE:
                    stream.WriteDate((DateTime) value);
                    break;
                case TdsDataType.TDS_DATEN:
                    if (!stream.TryWriteBytePrefixedNull(value))
                    {
                        stream.WriteDate((DateTime)value);
                    }
                    break;
                default:
                    Debug.Assert(false, $"Unsupported data type {format.DataType}");
                    break;
            }
        }
    }
}
