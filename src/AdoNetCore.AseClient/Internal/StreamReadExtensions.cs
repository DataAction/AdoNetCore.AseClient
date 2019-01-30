using System;
using System.IO;
using System.Text;

namespace AdoNetCore.AseClient.Internal
{
    internal static class StreamReadExtensions
    {
        public static bool ReadBool(this Stream stream, ref bool streamExceeded)
        {
            return stream.ReadByte(ref streamExceeded) != 0;
        }

        public static int ReadByte(this Stream stream, ref bool streamExceeded)
        {
            if (stream.CheckRequiredLength(1, ref streamExceeded) == false)
                return -1;
            return stream.ReadByte();
        }

        public static short ReadShort(this Stream stream, ref bool streamExceeded)
        {
            if (stream.CheckRequiredLength(2, ref streamExceeded) == false)
                return 0;
            var buf = new byte[2];
            stream.Read(buf, 0, 2);
            return BitConverter.ToInt16(buf, 0);
        }

        public static ushort ReadUShort(this Stream stream, ref bool streamExceeded)
        {
            if (stream.CheckRequiredLength(2, ref streamExceeded) == false)
                return 0;
            var buf = new byte[2];
            stream.Read(buf, 0, 2);
            return BitConverter.ToUInt16(buf, 0);
        }

        public static int ReadInt(this Stream stream, ref bool streamExceeded)
        {
            if (stream.CheckRequiredLength(4, ref streamExceeded) == false)
                return 0;
            var buf = new byte[4];
            stream.Read(buf, 0, 4);
            return BitConverter.ToInt32(buf, 0);
        }

        public static uint ReadUInt(this Stream stream, ref bool streamExceeded)
        {
            if (stream.CheckRequiredLength(4, ref streamExceeded) == false)
                return 0;
            var buf = new byte[4];
            stream.Read(buf, 0, 4);
            return BitConverter.ToUInt32(buf, 0);
        }

        public static long ReadLong(this Stream stream, ref bool streamExceeded)
        {
            if (stream.CheckRequiredLength(8, ref streamExceeded) == false)
                return 0L;
            var buf = new byte[8];
            stream.Read(buf, 0, 8);
            return BitConverter.ToInt64(buf, 0);
        }

        public static ulong ReadULong(this Stream stream, ref bool streamExceeded)
        {
            if (stream.CheckRequiredLength(8, ref streamExceeded) == false)
                return 0;
            var buf = new byte[8];
            stream.Read(buf, 0, 8);
            return BitConverter.ToUInt64(buf, 0);
        }

        public static float ReadFloat(this Stream stream, ref bool streamExceeded)
        {
            if (stream.CheckRequiredLength(4, ref streamExceeded) == false)
                return 0.0f;
            var buf = new byte[4];
            stream.Read(buf, 0, 4);
            return BitConverter.ToSingle(buf, 0);
        }

        public static double ReadDouble(this Stream stream, ref bool streamExceeded)
        {
            if (stream.CheckRequiredLength(8, ref streamExceeded) == false)
                return 0.0;
            var buf = new byte[8];
            stream.Read(buf, 0, 8);
            return BitConverter.ToDouble(buf, 0);
        }

        public static string ReadNullableByteLengthPrefixedString(this Stream stream, Encoding enc, ref bool streamExceeded)
        {
            var length = stream.ReadByte(ref streamExceeded);
            if (length == 0 || streamExceeded)
            {
                return null;
            }

            return stream.ReadString(length, enc, ref streamExceeded);
        }

        public static string ReadNullableIntLengthPrefixedString(this Stream stream, Encoding enc, ref bool streamExceeded)
        {
            var length = stream.ReadInt(ref streamExceeded);
            if (length == 0 || streamExceeded)
            {
                return null;
            }

            return stream.ReadString(length, enc, ref streamExceeded);
        }

        public static string ReadByteLengthPrefixedString(this Stream stream, Encoding enc, ref bool streamExceeded)
        {
            var length = stream.ReadByte(ref streamExceeded);
            return stream.ReadString(length, enc, ref streamExceeded);
        }

        public static string ReadShortLengthPrefixedString(this Stream stream, Encoding enc, ref bool streamExceeded)
        {
            var length = stream.ReadUShort(ref streamExceeded);
            return stream.ReadString(length, enc, ref streamExceeded);
        }

        public static string ReadString(this Stream stream, int length, Encoding enc, ref bool streamExceeded)
        {
            if (length == 0 || streamExceeded)
            {
                return string.Empty;
            }

            if (stream.CheckRequiredLength(length, ref streamExceeded) == false)
                return null;
            var buf = new byte[length];
            stream.Read(buf, 0, length);
            return enc.GetString(buf);
        }

        public static byte[] ReadByteLengthPrefixedByteArray(this Stream stream, ref bool streamExceeded)
        {
            var length = stream.ReadByte(ref streamExceeded);

            if (length == 0 || streamExceeded)
            {
                return new byte[0];
            }

            return stream.ReadByteArray(length, ref streamExceeded);
        }

        public static byte[] ReadNullableByteLengthPrefixedByteArray(this Stream stream, ref bool streamExceeded)
        {
            var length = stream.ReadByte(ref streamExceeded);

            if (length == 0 || streamExceeded)
            {
                return null;
            }

            return stream.ReadByteArray(length, ref streamExceeded);
        }

        public static byte[] ReadNullableIntLengthPrefixedByteArray(this Stream stream, ref bool streamExceeded)
        {
            var length = stream.ReadInt(ref streamExceeded);

            if (length == 0 || streamExceeded)
            {
                return null;
            }

            return stream.ReadByteArray(length, ref streamExceeded);
        }

        public static byte[] ReadByteArray(this Stream stream, int length, ref bool streamExceeded)
        {
            if (length == 0 || streamExceeded)
            {
                return new byte[0];
            }

            if (stream.CheckRequiredLength(length, ref streamExceeded) == false)
                return null;
            var buf = new byte[length];
            stream.Read(buf, 0, length);
            return buf;
        }

        public static DateTime ReadIntPartDateTime(this Stream stream, ref bool streamExceeded)
        {
            var days = stream.ReadInt(ref streamExceeded);
            var sqlTicks = stream.ReadInt(ref streamExceeded);

            return Constants.Sql.RegularDateTime.Epoch.AddDays(days).AddMilliseconds((int)(sqlTicks / Constants.Sql.RegularDateTime.TicksPerMillisecond));
        }

        public static DateTime ReadBigDateTime(this Stream stream, ref bool streamExceeded)
        {
            var usSinceYearZero = stream.ReadLong(ref streamExceeded);
            if (streamExceeded)
                return DateTime.MinValue;
            var usSinceEpoch = usSinceYearZero - Constants.Sql.BigDateTime.EpochMicroSeconds;
            var msSinceEpoch = usSinceEpoch / 1000;
            var timeSinceEpoch = TimeSpan.FromMilliseconds(msSinceEpoch);

            return Constants.Sql.BigDateTime.Epoch + timeSinceEpoch;
        }

        public static DateTime ReadShortPartDateTime(this Stream stream, ref bool streamExceeded)
        {
            var p1 = stream.ReadUShort(ref streamExceeded);
            var p2 = stream.ReadUShort(ref streamExceeded);

            return Constants.Sql.RegularDateTime.Epoch.AddDays(p1).AddMinutes(p2);
        }

        public static DateTime ReadDate(this Stream stream, ref bool streamExceeded)
        {
            var p1 = stream.ReadInt(ref streamExceeded);
            return Constants.Sql.RegularDateTime.Epoch.AddDays(p1);
        }

        public static DateTime ReadTime(this Stream stream, ref bool streamExceeded)
        {
            var sqlTicks = stream.ReadInt(ref streamExceeded);
            return Constants.Sql.RegularDateTime.Epoch.AddMilliseconds((int)(sqlTicks / Constants.Sql.RegularDateTime.TicksPerMillisecond));
        }

        public static AseDecimal? ReadAseDecimal(this Stream stream, byte precision, byte scale, ref bool streamExceeded)
        {
            // We will read at least 2 bytes in this method so check it once and use the base ReadByte
            if (stream.CheckRequiredLength(2, ref streamExceeded) == false)
                return null;
            var length = stream.ReadByte();
            if (length == 0)
            {
                return null;
            }
            var isPositive = stream.ReadByte() == 0;
            var remainingLength = length - 1;
            var buf = new byte[length];

            if (stream.CheckRequiredLength(remainingLength, ref streamExceeded) == false)
                return null;
            stream.Read(buf, 1, remainingLength);
            
            Array.Reverse(buf);
            
            return new AseDecimal(precision, scale, isPositive, buf);
        }

        public static decimal ReadMoney(this Stream stream, ref bool streamExceeded)
        {
            if (stream.CheckRequiredLength(8, ref streamExceeded) == false)
                return 0m;
            var buf = new byte[8];
            stream.Read(buf, 0, 8);
            buf = new[]
            {
                buf[4], buf[5], buf[6], buf[7],
                buf[0], buf[1], buf[2], buf[3]
            };
            return new decimal(BitConverter.ToInt64(buf, 0)) / 10000m;
        }

        public static decimal ReadSmallMoney(this Stream stream, ref bool streamExceeded)
        {
            if (stream.CheckRequiredLength(4, ref streamExceeded) == false)
                return 0m;
            var buf = new byte[4];
            stream.Read(buf, 0, 4);
            return new decimal(BitConverter.ToInt32(buf, 0)) / 10000m;
        }

        public static bool CheckRequiredLength(this Stream stream, long length, ref bool streamExceeded)
        {
            if (length < 0 || stream.Position + length > stream.Length)
                streamExceeded = true;
            return !streamExceeded;
        }
    }
}
