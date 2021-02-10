using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Internal;
using System;
using System.IO;

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

        private uint CalculateRemainingLength(Stream stream)
        {
            var len = ClassifyLength(_type);
            if (len == CatchAllLength.Undefined || len == CatchAllLength.Absent)
            {
                // We cannot successfully read undefined / absent length without corrupting the stream
                // Undefined has no real logical mapping
                // Absent are context-sensitive and based on prior tokens in the stream (e.g. TDS_ALTROW is of varied length based on TDS_ALTFMT)
                throw new InvalidOperationException($"Attempted to use catch-all token with token {_type} of {len} length");
            }
            return ReadLength(stream, len);
        }

        /// <summary>
        /// The token's length or the length of its remaining length indicator is encoded in the token's type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static CatchAllLength ClassifyLength(byte type)
        {
            if (type == (byte) TokenType.TDS_CURDECLARE3)
            {
                // Default handling in the spec appears to be incorrect
                // Return the length as defined by the token itself
                return CatchAllLength.Dynamic_4;
            }

            // 5.2.2 Fixed Length - xx11xxxx
            switch (type & 0b0011_1100)
            {
                case 0b0011_1100: return CatchAllLength.Fixed_8; // xx1111xx - 8 bytes
                case 0b0011_1000: return CatchAllLength.Fixed_4; // xx1110xx - 4 bytes
                case 0b0011_0100: return CatchAllLength.Fixed_2; // xx1101xx - 2 bytes
                case 0b0011_0000: return CatchAllLength.Fixed_1; // xx1100xx - 1 byte
            }

            // 5.2.3 Variable Length - any other pattern                
            switch (type & 0b1111_0000)
            {
                case 0b1010_0000: return CatchAllLength.Dynamic_2; // 1010xxxx - ushort
                case 0b1110_0000: return CatchAllLength.Dynamic_2; // 1110xxxx - ushort
                case 0b1000_0000: return CatchAllLength.Dynamic_2; // 1000xxxx - ushort
            }

            switch (type & 0b1111_1100)
            {
                case 0b0010_0000: return CatchAllLength.Dynamic_4; // 001000xx - uint
                case 0b0110_0000: return CatchAllLength.Dynamic_4; // 011000xx - uint
                case 0b0010_0100: return CatchAllLength.Dynamic_1; // 001001xx - byte
                case 0b0010_1000: return CatchAllLength.Dynamic_1; // 001010xx - byte
                case 0b0110_0100: return CatchAllLength.Dynamic_1; // 011001xx - byte
                case 0b0110_1000: return CatchAllLength.Dynamic_1; // 011010xx - byte
            }

            // 5.2.1 Zero Length - 110xxxxx
            if ((type & 0b1110_0000) == 0b1100_0000)
            {
                return CatchAllLength.Absent;
            }
            return CatchAllLength.Undefined;
        }

        internal static uint ReadLength(Stream stream, CatchAllLength length)
        {
            switch (length)
            {
                case CatchAllLength.Undefined: return 0;
                case CatchAllLength.Absent: return 0;
                case CatchAllLength.Fixed_1: return 1;
                case CatchAllLength.Fixed_2: return 2;
                case CatchAllLength.Fixed_4: return 4;
                case CatchAllLength.Fixed_8: return 8;
                case CatchAllLength.Dynamic_1: return (uint)stream.ReadByte();
                case CatchAllLength.Dynamic_2: return stream.ReadUShort();
                case CatchAllLength.Dynamic_4: return stream.ReadUInt();
            }
            throw new NotImplementedException("Unrecognised CatchAllLength " + length);
        }

        internal enum CatchAllLength
        {
            Undefined,
            Absent,
            Fixed_1,
            Fixed_2,
            Fixed_4,
            Fixed_8,
            Dynamic_1,
            Dynamic_2,
            Dynamic_4
        }
    }
}
