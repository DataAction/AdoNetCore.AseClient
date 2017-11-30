using System.Diagnostics;
using System.IO;
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
                        case 8: return (byte) stream.ReadLong();
                    }
                    break;
                case TdsDataType.TDS_CHAR:
                case TdsDataType.TDS_VARCHAR:
                    return stream.ReadNullableByteLengthPrefixedString(enc);
                default:
                    Debug.Assert(false, $"Unsupported data type {format.DataType}");
                    break;
            }

            return null;
        }
    }
}
