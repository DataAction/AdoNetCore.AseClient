using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using AdoNetCore.AseClient.Enum;

namespace AdoNetCore.AseClient.Internal
{
    internal static class ValueReader
    {
        public static object Read(Stream stream, FormatItem format, DbEnvironment env)
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
                        case 0: return DBNull.Value;
                        case 1: return (byte)stream.ReadByte(); //both INTN(1) and UINTN(1) are an INT1. Never an SINT1.
                        case 2: return stream.ReadShort();
                        case 4: return stream.ReadInt();
                        case 8: return stream.ReadLong();
                    }
                    break;
                case TdsDataType.TDS_UINTN:
                    switch (stream.ReadByte())
                    {
                        case 0: return DBNull.Value;
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
                        case 0: return DBNull.Value;
                        case 4: return stream.ReadFloat();
                        case 8: return stream.ReadDouble();
                    }
                    break;
                case TdsDataType.TDS_CHAR:
                case TdsDataType.TDS_VARCHAR:
                case TdsDataType.TDS_BOUNDARY:
                case TdsDataType.TDS_SENSITIVITY:
                    return stream.ReadNullableByteLengthPrefixedString(env.Encoding);
                case TdsDataType.TDS_BINARY:
                case TdsDataType.TDS_VARBINARY:
                    return stream.ReadNullableByteLengthPrefixedByteArray();
                case TdsDataType.TDS_LONGCHAR:
                    return stream.ReadNullableIntLengthPrefixedString(env.Encoding);
                /*
                 * TDS_LONGBINARY serialization 55 serialized java object or instance (i.e. java object)
                 * TDS_LONGBINARY serialized java class 56 serialized java class (i.e. byte code)
                 * TDS_LONGBINARY smallbinary 59 64K max length binary data (ASA)
                 * TDS_LONGBINARY unichar 34 fixed length UTF-16 encoded data
                 * TDS_LONGBINARY univarchar 35 variable length UTF-16 encoded data
                */
                case TdsDataType.TDS_LONGBINARY:
                    {
                        //the UserType can affect how we need to interpret the result data
                        switch (format.UserType)
                        {
                            case 34:
                            case 35:
                                return stream.ReadNullableIntLengthPrefixedString(Encoding.Unicode);
                            default:
                                return stream.ReadNullableIntLengthPrefixedByteArray();
                        }
                    }
                case TdsDataType.TDS_DECN:
                case TdsDataType.TDS_NUMN:
                    {
                        var precision = format.Precision ?? 1;
                        var scale = format.Scale ?? 0;
                        if (env.UseAseDecimal)
                        {
                            var aseDecimal = stream.ReadAseDecimal(precision, scale);

                            return aseDecimal.HasValue
                                ? env.UseAseDecimal
                                    ? (object)aseDecimal.Value
                                    : aseDecimal.Value.ToDecimal()
                                : DBNull.Value;
                        }

                        return (object)stream.ReadDecimal(precision, scale) ?? DBNull.Value;
                    }
                case TdsDataType.TDS_MONEY:
                    return stream.ReadMoney();
                case TdsDataType.TDS_SHORTMONEY:
                    return stream.ReadSmallMoney();
                case TdsDataType.TDS_MONEYN:
                    switch (stream.ReadByte())
                    {
                        case 0: return DBNull.Value;
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
                        case 0: return DBNull.Value;
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
                        case 0: return DBNull.Value;
                        case 4:
                            return stream.ReadDate();
                    }
                    break;
                case TdsDataType.TDS_TIME:
                    return stream.ReadTime();
                case TdsDataType.TDS_TIMEN:
                    switch (stream.ReadByte())
                    {
                        case 0: return DBNull.Value;
                        case 4:
                            return stream.ReadTime();
                    }
                    break;
                case TdsDataType.TDS_TEXT:
                case TdsDataType.TDS_XML:
                    {
                        var textPtrLen = (byte)stream.ReadByte();
                        if (textPtrLen == 0)
                        {
                            return DBNull.Value;
                        }
                        var textPtr = new byte[textPtrLen];
                        stream.Read(textPtr, 0, textPtrLen);
                        stream.ReadULong(); //timestamp
                        return stream.ReadNullableIntLengthPrefixedString(env.Encoding);
                    }
                case TdsDataType.TDS_IMAGE:
                    {
                        var textPtrLen = (byte)stream.ReadByte();
                        if (textPtrLen == 0)
                        {
                            return DBNull.Value;
                        }
                        var textPtr = new byte[textPtrLen];
                        stream.Read(textPtr, 0, textPtrLen);
                        stream.ReadULong(); //timestamp
                        var dataLen = stream.ReadInt();
                        var data = new byte[dataLen];
                        stream.Read(data, 0, dataLen);
                        return data;
                    }
                case TdsDataType.TDS_UNITEXT:
                    {
                        var textPtrLen = (byte)stream.ReadByte();
                        if (textPtrLen == 0)
                        {
                            return DBNull.Value;
                        }
                        var textPtr = new byte[textPtrLen];
                        stream.Read(textPtr, 0, textPtrLen);
                        stream.ReadULong(); //timestamp
                        return stream.ReadNullableIntLengthPrefixedString(Encoding.Unicode);
                    }
                default:
                    Debug.Assert(false, $"Unsupported data type {format.DataType}");
                    break;
            }

            return DBNull.Value; // Catch-all.
        }
    }
}
