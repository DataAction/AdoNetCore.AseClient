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
                case TdsDataType.TDS_VARCHAR:
                    stream.WriteBytePrefixedString((string)value, enc);
                    break;
                case TdsDataType.TDS_LONGCHAR:
                    stream.WriteIntPrefixedString((string)value, enc);
                    break;
                default:
                    Debug.Assert(false, $"Unsupported data type {format.DataType}");
                    break;
            }
        }
    }
}
