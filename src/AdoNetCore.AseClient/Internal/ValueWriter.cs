using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using AdoNetCore.AseClient.Enum;

namespace AdoNetCore.AseClient.Internal
{
    internal static class ValueWriter
    {
        private static readonly double SqlTicksPerMillisecond = 0.3;
        private static DateTime SqlDateTimeEpoch = new DateTime(1900, 1, 1);

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
                case TdsDataType.TDS_INT2:
                    stream.WriteShort((short)value);
                    break;
                case TdsDataType.TDS_INT4:
                    stream.WriteInt((int)value);
                    break;
                case TdsDataType.TDS_INT8:
                    stream.WriteLong((long)value);
                    break;
                case TdsDataType.TDS_INTN:
                    switch (value)
                    {
                        case byte b:
                            stream.WriteByte(1);
                            stream.WriteByte(b);
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
                        case null:
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
                    var dt = (DateTime)value;
                    var days = (int)(dt - SqlDateTimeEpoch).TotalDays;
                    var sqlTicks = (int)((dt.TimeOfDay - SqlDateTimeEpoch.TimeOfDay).TotalMilliseconds * SqlTicksPerMillisecond);
                    Console.WriteLine($"  -> {dt}: {days}, {sqlTicks}");
                    stream.WriteInt(days);
                    stream.WriteInt(sqlTicks);
                    break;
                default:
                    Debug.Assert(false, $"Unsupported data type {format.DataType}");
                    break;
            }
        }
    }
}
