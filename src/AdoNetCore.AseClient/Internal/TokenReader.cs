using System.Net.Sockets;
using AdoNetCore.AseClient.Interface;

namespace AdoNetCore.AseClient.Internal
{
    public class TokenReader : ITokenReader
    {
        private readonly Socket _socket;

        public TokenReader(Socket socket)
        {
            _socket = socket;
        }
    }
}
