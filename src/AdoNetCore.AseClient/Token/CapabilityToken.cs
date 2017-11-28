using System.IO;
using System.Text;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient.Token
{
    public class CapabilityToken : IToken
    {
        private byte TDS_CAP_REQUEST = 1;
        private byte TDS_CAP_RESPONSE = 2;

        public TokenType Type => TokenType.TDS_CAPABILITY;
        public void Write(Stream stream, Encoding enc)
        {
            stream.WriteByte((byte)Type);
            stream.WriteShort((short)_capabilityBytes.Length);
            stream.Write(_capabilityBytes);
        }

        public void Read(Stream stream, Encoding enc, IToken previous)
        {
            throw new System.NotImplementedException();
        }

        //from .net 4 client
        private readonly byte[] _capabilityBytes = {
            //cap request
            0x01, 0x0e, 0x01, 0xef, 0xff, 0x69, 0xb7, 0xfd, 0xff, 0xaf, 0x65, 0x41, 0xff, 0xff, 0xff, 0xd6,
            //cap response
            0x02, 0x0e, 0x00, 0x00, 0x00, 0x00, 0x00, 0x88, 0x40, 0x00, 0x01, 0x02, 0x48, 0x00, 0x00, 0x00
        };
    }
}
