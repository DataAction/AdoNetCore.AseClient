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
                    return stream.ReadIntPartDateTime();
                case TdsDataType.TDS_SHORTDATE:
                    return stream.ReadShortPartDateTime();
                case TdsDataType.TDS_DATETIMEN:
                    switch (stream.ReadByte())
                    {
                        case 4:
                            return stream.ReadShortPartDateTime();
                        case 8:
                            return stream.ReadIntPartDateTime();
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
