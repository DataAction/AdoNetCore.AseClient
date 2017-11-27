using System.IO;
using System.Linq;
using System.Text;

namespace AdoNetCore.AseClient.Internal
{
    public static class StreamExtensions
    {
        public static void Write(this Stream stream, byte[] bytes)
        {
            stream.Write(bytes, 0, bytes.Length);
        }

        public static void Write(this Stream stream, short value)
        {
            stream.WriteByte((byte)value);
            stream.WriteByte((byte)(value >> 8));
        }

        public static void WriteRepeatedBytes(this Stream stream, byte value, int repeat)
        {
            stream.Write(Enumerable.Repeat(value, repeat).ToArray());
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
    }
}
