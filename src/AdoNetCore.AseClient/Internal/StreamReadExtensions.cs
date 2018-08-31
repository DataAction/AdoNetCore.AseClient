using System;
using System.IO;
using System.Text;

namespace AdoNetCore.AseClient.Internal
{
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

        public static byte[] ReadByteLengthPrefixedByteArray(this Stream stream)
        {
            var length = stream.ReadByte();

            if (length == 0)
            {
                return new byte[0];
            }

            return stream.ReadByteArray(length);
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

        public static DateTime ReadIntPartDateTime(this Stream stream)
        {
            var days = stream.ReadInt();
            var sqlTicks = stream.ReadInt();
            return Constants.Sql.Epoch.AddDays(days).AddMilliseconds((int)(sqlTicks / Constants.Sql.TicksPerMillisecond));
        }

        public static DateTime ReadBigDateTime(this Stream stream)
        {
            var usSinceYearZero = stream.ReadLong();
            var usSinceEpoch = usSinceYearZero - Constants.Sql.BigEpochMicroSeconds;
            var msSinceEpoch = usSinceEpoch / 1000;
            var timeSinceEpoch = TimeSpan.FromMilliseconds(msSinceEpoch);

            return Constants.Sql.BigEpoch + timeSinceEpoch;
        }

        public static DateTime ReadShortPartDateTime(this Stream stream)
        {
            var p1 = stream.ReadUShort();
            var p2 = stream.ReadUShort();

            return Constants.Sql.Epoch.AddDays(p1).AddMinutes(p2);
        }

        public static DateTime ReadDate(this Stream stream)
        {
            var p1 = stream.ReadInt();
            return Constants.Sql.Epoch.AddDays(p1);
        }

        public static DateTime ReadTime(this Stream stream)
        {
            var sqlTicks = stream.ReadInt();
            return Constants.Sql.Epoch.AddMilliseconds((int)(sqlTicks / Constants.Sql.TicksPerMillisecond));
        }

        public static AseDecimal? ReadAseDecimal(this Stream stream, byte precision, byte scale)
        {
            var length = stream.ReadByte();
            if (length == 0)
            {
                return null;
            }
            var isPositive = stream.ReadByte() == 0;
            var remainingLength = length - 1;
            var buf = new byte[length];

            stream.Read(buf, 1, remainingLength);
            
            Array.Reverse(buf);
            
            return new AseDecimal(precision, scale, isPositive, buf);
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
