using System.IO;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient.Token
{
    internal class CatchAllToken : IToken
    {
        public TokenType Type => (TokenType) _type;
        private readonly byte _type;

        public CatchAllToken(byte type)
        {
            _type = type;
        }

        public void Write(Stream stream, DbEnvironment env)
        {
            // do nothing
        }

        public void Read(Stream stream, DbEnvironment env, IFormatToken previous)
        {
            var remainingLength = CalculateRemainingLength(stream);

            for (uint i = 0; i < remainingLength; i++)
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
        private uint CalculateRemainingLength(Stream stream)
        {
            // 5.2.2 Fixed Length - xx11xxxx
            // xx1111xx - 8 bytes
            if ((_type & 0b0011_1100) == 0b0011_1100)
            {
                return 8;
            }

            // xx1110xx - 4 bytes
            if ((_type & 0b0011_1000) == 0b0011_1000)
            {
                return 4;
            }

            // xx1101xx - 2 bytes
            if ((_type & 0b0011_0100) == 0b0011_0100)
            {
                return 2;
            }

            // xx1100xx - 1 byte
            if ((_type & 0b0011_0000) == 0b0011_0000)
            {
                return 1;
            }

            // 5.2.3 Variable Length - any other pattern
            // 1010xxxx - ushort
            if ((_type & 0b1010_0000) == 0b1010_0000)
            {
                return stream.ReadUShort();
            }

            // 1110xxxx - ushort
            if ((_type & 0b1110_0000) == 0b1110_0000)
            {
                return stream.ReadUShort();
            }

            // 1000xxxx - ushort
            if ((_type & 0b1000_0000) == 0b1000_0000)
            {
                return stream.ReadUShort();
            }

            // 001000xx - uint
            if ((_type & 0b0010_0000) == 0b0010_0000)
            {
                return stream.ReadUInt();
            }

            // 011000xx - uint
            if ((_type & 0b0110_0000) == 0b0110_0000)
            {
                return stream.ReadUInt();
            }

            // 001001xx - byte
            if ((_type & 0b0010_0100) == 0b0010_0100)
            {
                return (uint) stream.ReadByte();
            }

            // 001010xx - byte
            if ((_type & 0b0010_1000) == 0b0010_1000)
            {
                return (uint) stream.ReadByte();
            }

            // 011001xx - byte
            if ((_type & 0b0110_0100) == 0b0110_0100)
            {
                return (uint) stream.ReadByte();
            }

            // 011010xx - byte
            if ((_type & 0b0110_1000) == 0b0110_1000)
            {
                return (uint) stream.ReadByte();
            }

            // 5.2.1 Zero Length - 110xxxxx
            return 0;
        }
    }
}
