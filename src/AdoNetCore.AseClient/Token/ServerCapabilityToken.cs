using System;
using System.IO;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient.Token
{
    internal class ServerCapabilityToken : IToken
    {
        public TokenType Type => TokenType.TDS_CAPABILITY;

        private byte[] _capabilityBytes;

        public void Write(Stream stream, DbEnvironment env)
        {
            throw new NotSupportedException();
        }

        public void Read(Stream stream, DbEnvironment env, IFormatToken previous)
        {
            var remainingLength = stream.ReadUShort();
            _capabilityBytes = new byte[remainingLength];
            stream.Read(_capabilityBytes, 0, remainingLength);
        }

        public static ServerCapabilityToken Create(Stream stream, DbEnvironment env, IFormatToken previous)
        {
            var t = new ServerCapabilityToken();
            t.Read(stream, env, previous);
            return t;
        }
    }
}
