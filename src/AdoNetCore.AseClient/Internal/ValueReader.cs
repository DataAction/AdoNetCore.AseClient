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
                case TdsDataType.TDS_SINT1:
                    return (sbyte)stream.ReadByte();
                case TdsDataType.TDS_INT2:
                    return stream.ReadShort();
                case TdsDataType.TDS_UINT2:
                    return stream.ReadUShort();
                case TdsDataType.TDS_INT4:
                    return stream.ReadInt();
                case TdsDataType.TDS_UINT4:
                    return stream.ReadUInt();
                case TdsDataType.TDS_INT8:
                    return stream.ReadLong();
                case TdsDataType.TDS_UINT8:
                    return stream.ReadULong();
                case TdsDataType.TDS_INTN:
                    switch (stream.ReadByte())
                    {
                        case 1: return (byte)stream.ReadByte(); //both INTN(1) and UINTN(1) are an INT1. Never an SINT1.
                        case 2: return stream.ReadShort();
                        case 4: return stream.ReadInt();
                        case 8: return stream.ReadLong();
                    }
                    break;
                case TdsDataType.TDS_UINTN:
                    switch (stream.ReadByte())
                    {
                        case 1: return (byte)stream.ReadByte();
                        case 2: return stream.ReadUShort();
                        case 4: return stream.ReadUInt();
                        case 8: return stream.ReadULong();
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
                case TdsDataType.TDS_BOUNDARY:
                case TdsDataType.TDS_SENSITIVITY:
                    return stream.ReadNullableByteLengthPrefixedString(enc);
                case TdsDataType.TDS_BINARY:
                case TdsDataType.TDS_VARBINARY:
                    return stream.ReadNullableByteLengthPrefixedByteArray();
                case TdsDataType.TDS_LONGCHAR:
                    return stream.ReadNullableIntLengthPrefixedString(enc);
                case TdsDataType.TDS_LONGBINARY:
                    return stream.ReadNullableIntLengthPrefixedByteArray();
                case TdsDataType.TDS_DECN:
                case TdsDataType.TDS_NUMN:
                    return stream.ReadDecimal(format.Precision ?? 1, format.Scale ?? 0);
                case TdsDataType.TDS_MONEY:
                    return stream.ReadMoney();
                case TdsDataType.TDS_SHORTMONEY:
                    return stream.ReadSmallMoney();
                case TdsDataType.TDS_MONEYN:
                    switch (stream.ReadByte())
                    {
                        case 4:
                            return stream.ReadSmallMoney();
                        case 8:
                            return stream.ReadMoney();
                    }
                    break;
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
                case TdsDataType.TDS_DATE:
                    return stream.ReadDate();
                case TdsDataType.TDS_DATEN:
                    switch (stream.ReadByte())
                    {
                        case 4:
                            return stream.ReadDate();
                    }
                    break;
                case TdsDataType.TDS_TIME:
                    return stream.ReadTime();
                case TdsDataType.TDS_TIMEN:
                    switch (stream.ReadByte())
                    {
                        case 4:
                            return stream.ReadTime();
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
