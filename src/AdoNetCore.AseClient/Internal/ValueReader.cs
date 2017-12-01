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
                case TdsDataType.TDS_INT1:
                    return (byte) stream.ReadByte();
                case TdsDataType.TDS_INT2:
                    return stream.ReadShort();
                case TdsDataType.TDS_INT4:
                    return stream.ReadInt();
                case TdsDataType.TDS_INT8:
                    return stream.ReadLong();
                case TdsDataType.TDS_INTN:
                    switch (stream.ReadByte())
                    {
                        case 1: return (byte) stream.ReadByte();
                        case 2: return stream.ReadShort();
                        case 4: return stream.ReadInt();
                        case 8: return stream.ReadLong();
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
                    var length = stream.ReadByte();
                    if (length == 0)
                    {
                        return null;
                    }
                    var buf = new byte[length];
                    stream.Read(buf, 0, length);
                    var bytestring = string.Join(" ", buf.Select(x => x.ToString("x2")));
                    Console.WriteLine($"l:{length}, p:{format.Precision}, s:{format.Scale}: {bytestring}");
                    return 0m;
                default:
                    Debug.Assert(false, $"Unsupported data type {format.DataType}");
                    break;
            }

            return null;
        }
    }
}
