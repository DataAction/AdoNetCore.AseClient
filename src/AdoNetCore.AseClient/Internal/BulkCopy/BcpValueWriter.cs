using System;
using System.IO;
using System.Text;
using AdoNetCore.AseClient.Enum;

namespace AdoNetCore.AseClient.Internal.BulkCopy
{
    /// <summary>
    /// Writes values in native BCP format for bulk copy operations.
    /// Unlike the regular ValueWriter which uses TDS token format (with length prefixes for nullable types),
    /// BCP native format writes fixed-length types as raw bytes with no prefix, and variable-length types
    /// as raw data whose length is tracked via the offset table.
    /// </summary>
    internal static class BcpValueWriter
    {
        /// <summary>
        /// Writes a fixed-length value in native BCP format (no length prefix).
        /// For null values on fixed-length types, writes zero-filled bytes.
        /// </summary>
        public static void WriteFixedValue(Stream stream, object value, BcpColumnMetadata column, Encoding encoding)
        {
            if (value == null || value == DBNull.Value)
            {
                if (column.DataType == TdsDataType.TDS_CHAR)
                {
                    // Pad with spaces for char columns
                    for (var p = 0; p < column.FixedByteSize; p++)
                    {
                        stream.WriteByte(0x20);
                    }
                }
                else
                {
                    WriteZeroFill(stream, column.FixedByteSize);
                }
                return;
            }

            switch (column.DataType)
            {
                case TdsDataType.TDS_BIT:
                    stream.WriteByte(Convert.ToBoolean(value) ? (byte)1 : (byte)0);
                    break;
                case TdsDataType.TDS_INT1:
                    stream.WriteByte(Convert.ToByte(value));
                    break;
                case TdsDataType.TDS_INT2:
                    stream.WriteShort(Convert.ToInt16(value));
                    break;
                case TdsDataType.TDS_UINT2:
                    stream.WriteUShort(Convert.ToUInt16(value));
                    break;
                case TdsDataType.TDS_INT4:
                    stream.WriteInt(Convert.ToInt32(value));
                    break;
                case TdsDataType.TDS_UINT4:
                    stream.WriteUInt(Convert.ToUInt32(value));
                    break;
                case TdsDataType.TDS_INT8:
                    stream.WriteLong(Convert.ToInt64(value));
                    break;
                case TdsDataType.TDS_UINT8:
                    stream.WriteULong(Convert.ToUInt64(value));
                    break;
                case TdsDataType.TDS_FLT4:
                    stream.WriteFloat(Convert.ToSingle(value));
                    break;
                case TdsDataType.TDS_FLT8:
                    stream.WriteDouble(Convert.ToDouble(value));
                    break;
                case TdsDataType.TDS_DATETIME:
                    stream.WriteIntPartDateTime(Convert.ToDateTime(value));
                    break;
                case TdsDataType.TDS_SHORTDATE:
                    WriteShortDate(stream, Convert.ToDateTime(value));
                    break;
                case TdsDataType.TDS_DATE:
                    stream.WriteDate(Convert.ToDateTime(value));
                    break;
                case TdsDataType.TDS_TIME:
                    WriteTimeValue(stream, value);
                    break;
                case TdsDataType.TDS_MONEY:
                    WriteMoneyRaw(stream, Convert.ToDecimal(value));
                    break;
                case TdsDataType.TDS_SHORTMONEY:
                    WriteShortMoney(stream, Convert.ToDecimal(value));
                    break;
                case TdsDataType.TDS_CHAR:
                    WriteFixedChar(stream, value, column.FixedByteSize, encoding);
                    break;
                case TdsDataType.TDS_BINARY:
                    WriteFixedBinary(stream, value, column.FixedByteSize);
                    break;
                case TdsDataType.TDS_DECN:
                case TdsDataType.TDS_NUMN:
                    WriteFixedDecimal(stream, value, column.FixedByteSize);
                    break;
                default:
                    throw new NotSupportedException($"Unsupported fixed-length BCP type: {column.DataType}");
            }
        }

        /// <summary>
        /// Writes a variable-length value in native BCP format (raw bytes, no length prefix).
        /// The caller is responsible for tracking the offset via the offset table.
        /// Returns the number of bytes written.
        /// </summary>
        public static int WriteVariableValue(Stream stream, object value, BcpColumnMetadata column, Encoding encoding)
        {
            if (value == null || value == DBNull.Value)
            {
                return 0;
            }

            switch (column.DataType)
            {
                case TdsDataType.TDS_INTN:
                    return WriteIntN(stream, value, column.Length);
                case TdsDataType.TDS_UINTN:
                    return WriteUIntN(stream, value, column.Length);
                case TdsDataType.TDS_FLTN:
                    return WriteFloatN(stream, value, column.Length);
                case TdsDataType.TDS_DATETIMEN:
                    return WriteDateTimeN(stream, value, column.Length);
                case TdsDataType.TDS_BIGDATETIMEN:
                    return WriteBigDateTimeN(stream, value);
                case TdsDataType.TDS_DATEN:
                    return WriteDateN(stream, value);
                case TdsDataType.TDS_TIMEN:
                    return WriteTimeN(stream, value);
                case TdsDataType.TDS_MONEYN:
                    return WriteMoneyN(stream, value, column.Length);
                case TdsDataType.TDS_CHAR:
                case TdsDataType.TDS_VARCHAR:
                    return WriteString(stream, value, column.Length, encoding);
                case TdsDataType.TDS_LONGCHAR:
                    return WriteLongString(stream, value, column.Length, encoding);
                case TdsDataType.TDS_BINARY:
                case TdsDataType.TDS_VARBINARY:
                    return WriteBinary(stream, value, column.Length);
                case TdsDataType.TDS_LONGBINARY:
                    return WriteLongBinary(stream, value, column);
                case TdsDataType.TDS_DECN:
                case TdsDataType.TDS_NUMN:
                    return WriteDecimal(stream, value);
                default:
                    throw new NotSupportedException($"Unsupported variable-length BCP type: {column.DataType}");
            }
        }

        private static void WriteZeroFill(Stream stream, int count)
        {
            for (var i = 0; i < count; i++)
            {
                stream.WriteByte(0);
            }
        }

        private static void WriteFixedChar(Stream stream, object value, int fixedSize, Encoding encoding)
        {
            var str = Convert.ToString(value) ?? string.Empty;
            var bytes = encoding.GetBytes(str);
            if (bytes.Length >= fixedSize)
            {
                stream.Write(bytes, 0, fixedSize);
            }
            else
            {
                stream.Write(bytes, 0, bytes.Length);
                // Pad with spaces
                for (var p = bytes.Length; p < fixedSize; p++)
                {
                    stream.WriteByte(0x20);
                }
            }
        }

        private static void WriteFixedBinary(Stream stream, object value, int fixedSize)
        {
            var bytes = value as byte[];
            if (bytes != null)
            {
                if (bytes.Length >= fixedSize)
                {
                    stream.Write(bytes, 0, fixedSize);
                }
                else
                {
                    stream.Write(bytes, 0, bytes.Length);
                    WriteZeroFill(stream, fixedSize - bytes.Length);
                }
            }
            else
            {
                WriteZeroFill(stream, fixedSize);
            }
        }

        private static void WriteFixedDecimal(Stream stream, object value, int fixedSize)
        {
            if (value is AseDecimal ad)
            {
                stream.WriteByte(ad.IsPositive ? (byte)0 : (byte)1);
                var data = ad.BinData;
                var dataLen = fixedSize - 1;
                var buf = new byte[dataLen];
                var iData = 0;
                for (var iBuf = buf.Length - 1; iBuf >= 0 && iData < data.Length; iBuf--, iData++)
                {
                    buf[iBuf] = data[iData];
                }
                stream.Write(buf, 0, buf.Length);
            }
            else
            {
                var dec = Convert.ToDecimal(value);
                var sqlDec = (SqlDecimal)dec;
                stream.WriteByte(sqlDec.IsPositive ? (byte)0 : (byte)1);
                var data = sqlDec.BinData;
                var dataLen = fixedSize - 1;
                var buf = new byte[dataLen];
                var iData = 0;
                for (var iBuf = buf.Length - 1; iBuf >= 0 && iData < data.Length; iBuf--, iData++)
                {
                    buf[iBuf] = data[iData];
                }
                stream.Write(buf, 0, buf.Length);
            }
        }

        private static void WriteShortDate(Stream stream, DateTime value)
        {
            var span = value - Constants.Sql.RegularDateTime.Epoch;
            var days = (ushort)span.Days;
            var minutes = (ushort)(span.Hours * 60 + span.Minutes);
            stream.WriteUShort(days);
            stream.WriteUShort(minutes);
        }

        private static void WriteTimeValue(Stream stream, object value)
        {
            TimeSpan ts;
            if (value is TimeSpan span)
            {
                ts = span;
            }
            else
            {
                ts = Convert.ToDateTime(value).TimeOfDay;
            }
            stream.WriteTime(ts);
        }

        private static void WriteMoneyRaw(Stream stream, decimal value)
        {
            // Money is stored as 8 bytes: high 4 bytes then low 4 bytes of a scaled int64
            var scaled = Convert.ToInt64(decimal.Truncate(value * 10000m));
            var buf = BitConverter.GetBytes(scaled);
            // Write high 4 bytes first, then low 4 bytes
            stream.Write(buf, 4, 4);
            stream.Write(buf, 0, 4);
        }

        private static void WriteShortMoney(Stream stream, decimal value)
        {
            var scaled = Convert.ToInt32(decimal.Truncate(value * 10000m));
            stream.WriteInt(scaled);
        }

        private static int WriteIntN(Stream stream, object value, int maxLength)
        {
            switch (maxLength)
            {
                case 1:
                    stream.WriteByte(Convert.ToByte(value));
                    return 1;
                case 2:
                    stream.WriteShort(Convert.ToInt16(value));
                    return 2;
                case 4:
                    stream.WriteInt(Convert.ToInt32(value));
                    return 4;
                case 8:
                    stream.WriteLong(Convert.ToInt64(value));
                    return 8;
                default:
                    stream.WriteInt(Convert.ToInt32(value));
                    return 4;
            }
        }

        private static int WriteUIntN(Stream stream, object value, int maxLength)
        {
            switch (maxLength)
            {
                case 1:
                    stream.WriteByte(Convert.ToByte(value));
                    return 1;
                case 2:
                    stream.WriteUShort(Convert.ToUInt16(value));
                    return 2;
                case 4:
                    stream.WriteUInt(Convert.ToUInt32(value));
                    return 4;
                case 8:
                    stream.WriteULong(Convert.ToUInt64(value));
                    return 8;
                default:
                    stream.WriteUInt(Convert.ToUInt32(value));
                    return 4;
            }
        }

        private static int WriteFloatN(Stream stream, object value, int maxLength)
        {
            if (maxLength == 4)
            {
                stream.WriteFloat(Convert.ToSingle(value));
                return 4;
            }
            stream.WriteDouble(Convert.ToDouble(value));
            return 8;
        }

        private static int WriteDateTimeN(Stream stream, object value, int maxLength)
        {
            var dt = Convert.ToDateTime(value);
            if (maxLength == 4)
            {
                WriteShortDate(stream, dt);
                return 4;
            }
            stream.WriteIntPartDateTime(dt);
            return 8;
        }

        private static int WriteBigDateTimeN(Stream stream, object value)
        {
            var dt = Convert.ToDateTime(value);
            var timeSinceEpoch = dt - Constants.Sql.BigDateTime.Epoch;
            var msSinceEpoch = timeSinceEpoch.Ticks / TimeSpan.TicksPerMillisecond;
            var usSinceEpoch = msSinceEpoch * 1000;
            var usSinceYearZero = usSinceEpoch + Constants.Sql.BigDateTime.EpochMicroSeconds;
            stream.WriteLong(usSinceYearZero);
            return 8;
        }

        private static int WriteDateN(Stream stream, object value)
        {
            stream.WriteDate(Convert.ToDateTime(value));
            return 4;
        }

        private static int WriteTimeN(Stream stream, object value)
        {
            TimeSpan ts;
            if (value is TimeSpan span)
            {
                ts = span;
            }
            else
            {
                ts = Convert.ToDateTime(value).TimeOfDay;
            }
            stream.WriteTime(ts);
            return 4;
        }

        private static int WriteMoneyN(Stream stream, object value, int maxLength)
        {
            var d = Convert.ToDecimal(value);
            if (maxLength == 4)
            {
                WriteShortMoney(stream, d);
                return 4;
            }
            WriteMoneyRaw(stream, d);
            return 8;
        }

        private static int WriteString(Stream stream, object value, int maxLength, Encoding encoding)
        {
            var str = Convert.ToString(value);
            var bytes = encoding.GetBytes(str);
            var len = Math.Min(bytes.Length, maxLength);
            stream.Write(bytes, 0, len);
            return len;
        }

        private static int WriteLongString(Stream stream, object value, int maxLength, Encoding encoding)
        {
            var str = Convert.ToString(value);
            var bytes = encoding.GetBytes(str);
            var len = Math.Min(bytes.Length, maxLength);
            stream.Write(bytes, 0, len);
            return len;
        }

        private static int WriteBinary(Stream stream, object value, int maxLength)
        {
            var bytes = (byte[])value;
            var len = Math.Min(bytes.Length, maxLength);
            stream.Write(bytes, 0, len);
            return len;
        }

        private static int WriteLongBinary(Stream stream, object value, BcpColumnMetadata column)
        {
            if (value is string s)
            {
                // unichar/univarchar types
                if (column.UserType == 34 || column.UserType == 35)
                {
                    var bytes = Encoding.Unicode.GetBytes(s);
                    var len = Math.Min(bytes.Length, column.Length);
                    stream.Write(bytes, 0, len);
                    return len;
                }
            }

            var ba = value as byte[];
            if (ba != null)
            {
                var len = Math.Min(ba.Length, column.Length);
                stream.Write(ba, 0, len);
                return len;
            }

            return 0;
        }

        private static int WriteDecimal(Stream stream, object value)
        {
            if (value is AseDecimal ad)
            {
                var requiredBytes = ad.BytesRequired;
                stream.WriteByte(ad.IsPositive ? (byte)0 : (byte)1);
                var data = ad.BinData;
                var buf = new byte[requiredBytes];
                var iData = 0;
                for (var iBuf = buf.Length - 1; iBuf >= 0; iBuf--, iData++)
                {
                    buf[iBuf] = data[iData];
                }
                stream.Write(buf, 0, buf.Length);
                return 1 + requiredBytes;
            }
            else
            {
                var dec = Convert.ToDecimal(value);
                var sqlDec = (SqlDecimal)dec;
                var requiredBytes = sqlDec.BytesRequired;
                stream.WriteByte(sqlDec.IsPositive ? (byte)0 : (byte)1);
                var data = sqlDec.BinData;
                var buf = new byte[requiredBytes];
                var iData = 0;
                for (var iBuf = buf.Length - 1; iBuf >= 0; iBuf--, iData++)
                {
                    buf[iBuf] = data[iData];
                }
                stream.Write(buf, 0, buf.Length);
                return 1 + requiredBytes;
            }
        }
    }
}
