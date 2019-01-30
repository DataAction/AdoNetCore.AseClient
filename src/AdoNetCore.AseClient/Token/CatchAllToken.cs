using System;
using System.IO;
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

        public void Write(Stream stream, DbEnvironment env)
        {
            // do nothing
        }

        public void Read(Stream stream, DbEnvironment env, IFormatToken previous, ref bool streamExceeded)
        {
            //var remainingLength = stream.ReadShort();
            var remainingLength = CalculateRemainingLength(Type, stream, ref streamExceeded);
            if (stream.CheckRequiredLength((long)remainingLength, ref streamExceeded) == false)
                return;

            if (remainingLength < long.MaxValue)
                stream.Seek((long)remainingLength, SeekOrigin.Current);
            else
                for (ulong i = 0; i < remainingLength; i++)
                {
                    stream.ReadByte();
                }
        }

        /// <summary>
        /// The token's length or the length of its remaining length indicator is encoded in the token's type.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private ulong CalculateRemainingLength(TokenType type, Stream stream, ref bool streamExceeded)
        {
            var b = (byte)type;

            if ((b & 0b0011_0000) == 0b0011_0000)
            {
                return Convert.ToUInt64(stream.ReadByte(ref streamExceeded));
            }

            if ((b & 0b0011_0100) == 0b0011_0100)
            {
                return stream.ReadUShort(ref streamExceeded);
            }

            if ((b & 0b0011_1000) == 0b0011_1000)
            {
                return stream.ReadUInt(ref streamExceeded);
            }

            if ((b & 0b0011_1100) == 0b0011_1100)
            {
                return stream.ReadULong(ref streamExceeded);
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
