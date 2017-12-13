using System;
using System.IO;
using System.Text;

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

        private static void WriteRepeatedBytes(this Stream stream, byte value, int repeat)
        {
            for (var i = 0; i < repeat; i++)
            {
                stream.WriteByte(value);
            }
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
}