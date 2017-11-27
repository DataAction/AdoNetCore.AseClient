using System.Net.Sockets;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Token;

namespace AdoNetCore.AseClient.Internal
{
    public class TokenWriter : ITokenWriter
    {
        private readonly Socket _socket;

        public TokenWriter(Socket socket)
        {
            _socket = socket;
        }

        public void Write(params BaseToken[] tokens)
        {
            foreach (var t in tokens)
            {
                switch (t)
                {
                    case CapabilityToken ct:
                        Write(ct);
                        break;
                }
            }
        }

        private void Write(CapabilityToken cap)
        {
            
        }
    }
}
