using System;
using System.IO;
using System.Linq;
using System.Text;
using AdoNetCore.AseClient.Enum;

namespace AdoNetCore.AseClient.Internal
{
    internal static class StreamWriteExtensions
    {
        public static void Write(this Stream stream, byte[] bytes)
        {
            stream.Write(bytes, 0, bytes.Length);
        }

        public static void WriteShort(this Stream stream, short value)
        {
            //todo: as long as BitConverter.IsLittleEndian, this'll work. revisit to support bigendian too?
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
            var len = (byte) bytes.Length;
            stream.WriteByte(len);
            stream.Write(bytes, 0, len);
        }
    }

    internal static class StreamReadExtensions
    {
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
    }
}
