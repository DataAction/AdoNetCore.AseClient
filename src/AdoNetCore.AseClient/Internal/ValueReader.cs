using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using AdoNetCore.AseClient.Enum;

namespace AdoNetCore.AseClient.Internal
{
    internal static class ValueReader
    {
        private static readonly double SqlTicksPerMillisecond = 0.3;

        public static object Read(Stream stream, FormatItem format, Encoding enc)
        {
            switch (format.DataType)
            {
                case TdsDataType.TDS_BIT:
                    return stream.ReadBool();
                case TdsDataType.TDS_INT1:
                    return (byte)stream.ReadByte();
                case TdsDataType.TDS_INT2:
                    return stream.ReadShort();
                case TdsDataType.TDS_INT4:
                    return stream.ReadInt();
                case TdsDataType.TDS_INT8:
                    return stream.ReadLong();
                case TdsDataType.TDS_INTN:
                    switch (stream.ReadByte())
                    {
                        case 1: return (byte)stream.ReadByte();
                        case 2: return stream.ReadShort();
                        case 4: return stream.ReadInt();
                        case 8: return stream.ReadLong();
                    }
                    break;
                case TdsDataType.TDS_FLT4:
                    return stream.ReadFloat();
                case TdsDataType.TDS_FLT8:
                    return stream.ReadDouble();
                case TdsDataType.TDS_FLTN:
                    switch (stream.ReadByte())
                    {
                        case 4: return stream.ReadFloat();
                        case 8: return stream.ReadDouble();
                    }
                    break;
                case TdsDataType.TDS_CHAR:
                case TdsDataType.TDS_VARCHAR:
                    return stream.ReadNullableByteLengthPrefixedString(enc);
                case TdsDataType.TDS_BINARY:
                case TdsDataType.TDS_VARBINARY:
                    return stream.ReadNullableByteLengthPrefixedByteArray();
                case TdsDataType.TDS_LONGCHAR:
                    return stream.ReadNullableIntLengthPrefixedString(enc);
                case TdsDataType.TDS_LONGBINARY:
                    return stream.ReadNullableIntLengthPrefixedByteArray();
                case TdsDataType.TDS_DECN:
                    {
                        var length = stream.ReadByte();
                        if (length == 0)
                        {
                            return null;
                        }
                        var isPositive = stream.ReadByte() == 0;
                        var buffer = new byte[]
                        {
                        0, 0, 0, 0,
                        0, 0, 0, 0,
                        0, 0, 0, 0,
                        0, 0, 0, 0
                        };
                        var remainingLength = length - 1;
                        stream.Read(buffer, 16 - remainingLength, remainingLength);
                        buffer = buffer.Reverse().ToArray();
                        var bits = new[]
                        {
                            BitConverter.ToInt32(buffer, 0),
                            BitConverter.ToInt32(buffer, 4),
                            BitConverter.ToInt32(buffer, 8),
                            BitConverter.ToInt32(buffer, 12)
                        };

                        return (decimal)new SqlDecimal(format.Precision ?? 0, format.Scale ?? 0, isPositive, bits);
                    }
                case TdsDataType.TDS_DATETIME:
                    {
                        //var buffer = new byte[8];
                        //stream.Read(buffer, 0, 8);
                        //var hex = string.Join(" ", buffer.Select(b => b.ToString("x2")));
                        //var p1 = BitConverter.ToInt32(buffer, 0);
                        //var p2 = BitConverter.ToInt32(buffer, 4);
                        //Console.WriteLine($"Date: b: {hex}; p1: {p1}, p2: {p2}");
                        var days = stream.ReadInt();
                        var sqlTicks = stream.ReadInt();
                        return new DateTime(1900, 01, 01).AddDays(days).AddMilliseconds(sqlTicks / SqlTicksPerMillisecond);
                    }
                case TdsDataType.TDS_DATETIMEN:
                    switch (stream.ReadByte())
                    {
                        case 4:
                            break;
                        case 8:
                            var days = stream.ReadInt();
                            var sqlTicks = stream.ReadInt();
                            return new DateTime(1900, 01, 01).AddDays(days).AddMilliseconds(sqlTicks / SqlTicksPerMillisecond);
                    }
                    break;
                default:
                    Debug.Assert(false, $"Unsupported data type {format.DataType}");
                    break;
            }

            return null;
        }
    }
}
