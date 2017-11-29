using System;
using System.IO;
using System.Text;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient.Token
{
    internal class CatchAllToken : IToken
    {
        public TokenType Type { get; private set; }

        public CatchAllToken(TokenType type)
        {
            Type = type;
        }

        public void Write(Stream stream, Encoding enc)
        {

        }

        public void Read(Stream stream, Encoding enc, IFormatToken previous)
        {
            //var remainingLength = stream.ReadShort();
            var remainingLength = CalculateRemainingLength(Type, stream);

            for (ulong i = 0; i < remainingLength; i++)
            {
                stream.ReadByte();
            }
        }


        /// <summary>
        /// The token's length or the length of its remaining length indicator is encoded in the token's type.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="tokenType"></param>
        /// <returns></returns>
        private ulong CalculateRemainingLength(TokenType type, Stream stream)
        {
            var b = (byte)type;

            if ((b & 0b0011_0000) == 0b0011_0000)
            {
                return Convert.ToUInt64(stream.ReadByte());
            }

            if ((b & 0b0011_0100) == 0b0011_0100)
            {
                return stream.ReadUShort();
            }

            if ((b & 0b0011_1000) == 0b0011_1000)
            {
                return stream.ReadUInt();
            }

            if ((b & 0b0011_1100) == 0b0011_1100)
            {
                return stream.ReadULong();
            }

            if ((b & 0b1010_0000) == 0b1010_0000)
            {
                return 2;
            }

            if ((b & 0b1110_0000) == 0b1110_0000)
            {
                return 2;
            }

            if ((b & 0b1000_0000) == 0b1000_0000)
            {
                return 2;
            }

            if ((b & 0b0010_0000) == 0b0010_0000)
            {
                return 4;
            }

            if ((b & 0b0110_0000) == 0b0110_0000)
            {
                return 4;
            }

            if ((b & 0b0010_0100) == 0b0010_0100)
            {
                return 1;
            }

            if ((b & 0b0010_1000) == 0b0010_1000)
            {
                return 1;
            }

            if ((b & 0b0110_0100) == 0b0110_0100)
            {
                return 1;
            }

            if ((b & 0b0110_1000) == 0b0110_1000)
            {
                return 1;
            }

            return 0; //0b1100_0000
        }
    }
}
