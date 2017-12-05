using System;
using System.IO;
using System.Linq;
using System.Text;
using AdoNetCore.AseClient.Token;

namespace AdoNetCore.AseClient.Internal
{
    internal static class StreamWriteExtensions
    {
        public static void Write(this Stream stream, byte[] bytes)
        {
            stream.Write(bytes, 0, bytes.Length);
        }

        public static void WriteBool(this Stream stream, bool value)
        {
            stream.WriteByte((byte)(value ? 1 : 0));
        }

        public static void WriteShort(this Stream stream, short value)
        {
            stream.Write(BitConverter.GetBytes(value));
        }

        public static void WriteUShort(this Stream stream, ushort value)
        {
            stream.Write(BitConverter.GetBytes(value));
        }

        public static void WriteRepeatedBytes(this Stream stream, byte value, int repeat)
        {
            stream.Write(Enumerable.Repeat(value, repeat).ToArray());
        }

        public static void WriteInt(this Stream stream, int value)
        {
            stream.Write(BitConverter.GetBytes(value));
        }

        public static void WriteUInt(this Stream stream, uint value)
        {
            stream.Write(BitConverter.GetBytes(value));
        }

        public static void WriteLong(this Stream stream, long value)
        {
            stream.Write(BitConverter.GetBytes(value));
        }

        public static void WriteULong(this Stream stream, ulong value)
        {
            stream.Write(BitConverter.GetBytes(value));
        }

        public static void WriteFloat(this Stream stream, float value)
        {
            stream.Write(BitConverter.GetBytes(value));
        }

        public static void WriteDouble(this Stream stream, double value)
        {
            stream.Write(BitConverter.GetBytes(value));
        }

        /// <summary>
        /// Encode and write-out the string, up to the maximum length. Pad any remaining bytes. Append string length byte.
        /// If lengthModifier is supplied, it will be added to the appended length value.
        /// </summary>
        public static void WritePaddedString(this Stream stream, string value, int maxLength, Encoding enc, int lengthModifier = 0)
        {
            var bytes = enc.GetBytes(value);
            if (bytes.Length <= maxLength)
            {
                stream.Write(bytes);
                stream.WriteRepeatedBytes(0, maxLength - bytes.Length);
                stream.WriteByte((byte)(bytes.Length + lengthModifier));
            }
            else
            {
                stream.Write(bytes, 0, maxLength);
                stream.WriteByte((byte)(maxLength + lengthModifier));
            }
        }

        public static void WriteWeirdPasswordString(this Stream stream, string password, int maxLength, Encoding enc)
        {
            stream.WriteByte(0); //
            stream.WriteByte((byte)password.Length);
            stream.WritePaddedString(password, maxLength, enc, 2); //add two bytes to the appended length value to account for the above two bytes
        }

        public static void WriteBytePrefixedString(this Stream stream, string value, Encoding enc)
        {
            var bytes = enc.GetBytes(value);
            stream.WriteBytePrefixedByteArray(bytes);
        }

        public static void WriteIntPrefixedString(this Stream stream, string value, Encoding enc)
        {
            var bytes = enc.GetBytes(value);
            stream.WriteIntPrefixedByteArray(bytes);
        }

        public static void WriteBytePrefixedByteArray(this Stream stream, byte[] value)
        {
            var len = (byte)value.Length;
            stream.WriteByte(len);
            stream.Write(value, 0, len);
        }

        public static void WriteIntPrefixedByteArray(this Stream stream, byte[] value)
        {
            var len = value.Length;
            stream.WriteInt(len);
            stream.Write(value, 0, len);
        }

        public static bool TryWriteBytePrefixedNull(this Stream stream, object value)
        {
            if (value == DBNull.Value)
            {
                stream.WriteByte(0);
                return true;
            }

            return false;
        }

        public static bool TryWriteIntPrefixedNull(this Stream stream, object value)
        {
            if (value == DBNull.Value)
            {
                stream.WriteInt(0);
                return true;
            }

            return false;
        }

        private static readonly double SqlTicksPerMillisecond = 0.3;
        private static readonly DateTime SqlDateTimeEpoch = new DateTime(1900, 1, 1);
        //Refer: corefx SqlDateTime.FromTimeSpan
        public static void WriteIntPartDateTime(this Stream stream, DateTime value)
        {
            var span = value - SqlDateTimeEpoch;
            var day = span.Days;
            var ticks = span.Ticks - day * TimeSpan.TicksPerDay;
            if (ticks < 0L)
            {
                day--;
                ticks += TimeSpan.TicksPerDay;
            }
            var time = (int)((double)ticks / TimeSpan.TicksPerMillisecond * SqlTicksPerMillisecond + 0.5);
            stream.WriteInt(day);
            stream.WriteInt(time);
        }

        public static void WriteDate(this Stream stream, DateTime value)
        {
            var span = value - SqlDateTimeEpoch;
            var day = span.Days;
            if (span.Ticks - day * TimeSpan.TicksPerDay < 0L)
            {
                day--;
            }
            stream.WriteInt(day);
        }

        public static void WriteTime(this Stream stream, TimeSpan value)
        {
            var time = (int)((double)value.Ticks / TimeSpan.TicksPerMillisecond * SqlTicksPerMillisecond + 0.5);
            stream.WriteInt(time);
        }
    }

    internal static class StreamReadExtensions
    {
        public static bool ReadBool(this Stream stream)
        {
            return stream.ReadByte() != 0;
        }

        public static short ReadShort(this Stream stream)
        {
            var buf = new byte[2];
            stream.Read(buf, 0, 2);
            return BitConverter.ToInt16(buf, 0);
        }
        public static ushort ReadUShort(this Stream stream)
        {
            var buf = new byte[2];
            stream.Read(buf, 0, 2);
            return BitConverter.ToUInt16(buf, 0);
        }

        public static int ReadInt(this Stream stream)
        {
            var buf = new byte[4];
            stream.Read(buf, 0, 4);
            return BitConverter.ToInt32(buf, 0);
        }

        public static uint ReadUInt(this Stream stream)
        {
            var buf = new byte[4];
            stream.Read(buf, 0, 4);
            return BitConverter.ToUInt32(buf, 0);
        }

        public static long ReadLong(this Stream stream)
        {
            var buf = new byte[8];
            stream.Read(buf, 0, 8);
            return BitConverter.ToInt64(buf, 0);
        }

        public static ulong ReadULong(this Stream stream)
        {
            var buf = new byte[8];
            stream.Read(buf, 0, 8);
            return BitConverter.ToUInt64(buf, 0);
        }

        public static float ReadFloat(this Stream stream)
        {
            var buf = new byte[4];
            stream.Read(buf, 0, 4);
            return BitConverter.ToSingle(buf, 0);
        }

        public static double ReadDouble(this Stream stream)
        {
            var buf = new byte[8];
            stream.Read(buf, 0, 8);
            return BitConverter.ToDouble(buf, 0);
        }

        public static string ReadNullableByteLengthPrefixedString(this Stream stream, Encoding enc)
        {
            var length = stream.ReadByte();
            if (length == 0)
            {
                return null;
            }

            return stream.ReadString(length, enc);
        }
        public static string ReadNullableIntLengthPrefixedString(this Stream stream, Encoding enc)
        {
            var length = stream.ReadInt();
            if (length == 0)
            {
                return null;
            }

            return stream.ReadString(length, enc);
        }

        public static string ReadByteLengthPrefixedString(this Stream stream, Encoding enc)
        {
            var length = stream.ReadByte();
            return stream.ReadString(length, enc);
        }

        public static string ReadShortLengthPrefixedString(this Stream stream, Encoding enc)
        {
            var length = stream.ReadUShort();
            return stream.ReadString(length, enc);
        }

        public static string ReadString(this Stream stream, int length, Encoding enc)
        {
            if (length == 0)
            {
                return string.Empty;
            }

            var buf = new byte[length];
            stream.Read(buf, 0, length);
            return enc.GetString(buf);
        }

        public static byte[] ReadNullableByteLengthPrefixedByteArray(this Stream stream)
        {
            var length = stream.ReadByte();

            if (length == 0)
            {
                return null;
            }

            return stream.ReadByteArray(length);
        }

        public static byte[] ReadNullableIntLengthPrefixedByteArray(this Stream stream)
        {
            var length = stream.ReadInt();

            if (length == 0)
            {
                return null;
            }

            return stream.ReadByteArray(length);
        }

        public static byte[] ReadByteArray(this Stream stream, int length)
        {
            if (length == 0)
            {
                return new byte[0];
            }

            var buf = new byte[length];
            stream.Read(buf, 0, length);
            return buf;
        }

        private static readonly double SqlTicksPerMillisecond = 0.3;
        private static readonly DateTime SqlDateTimeEpoch = new DateTime(1900, 1, 1);

        public static DateTime ReadIntPartDateTime(this Stream stream)
        {
            var days = stream.ReadInt();
            var sqlTicks = stream.ReadInt();
            return SqlDateTimeEpoch.AddDays(days).AddMilliseconds(sqlTicks / SqlTicksPerMillisecond);
        }

        public static DateTime ReadShortPartDateTime(this Stream stream)
        {
            var p1 = stream.ReadUShort();
            var p2 = stream.ReadUShort();

            return SqlDateTimeEpoch.AddDays(p1).AddMinutes(p2);
        }

        public static DateTime ReadDate(this Stream stream)
        {
            var p1 = stream.ReadInt();
            return SqlDateTimeEpoch.AddDays(p1);
        }

        public static TimeSpan ReadTime(this Stream stream)
        {
            var sqlTicks = stream.ReadInt();
            return SqlDateTimeEpoch.AddMilliseconds(sqlTicks / SqlTicksPerMillisecond) - SqlDateTimeEpoch;
        }

        public static decimal? ReadDecimal(this Stream stream, byte precision, byte scale)
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

            return (decimal) new SqlDecimal(precision, scale, isPositive, bits);
        }

        public static decimal ReadMoney(this Stream stream)
        {
            var buf = new byte[8];
            stream.Read(buf, 0, 8);
            buf = new[]
            {
                buf[4], buf[5], buf[6], buf[7],
                buf[0], buf[1], buf[2], buf[3]
            };
            return new decimal(BitConverter.ToInt64(buf, 0)) / 10000m;
        }

        public static decimal ReadSmallMoney(this Stream stream)
        {
            var buf = new byte[4];
            stream.Read(buf, 0, 4);
            return new decimal(BitConverter.ToInt32(buf, 0)) / 10000m;
        }
    }
}
