using System.Net.Sockets;
using System.Text;
using AdoNetCore.AseClient.Interface;

namespace AdoNetCore.AseClient.Internal
{
    public class RegularSocket : ISocket
    {
        private readonly Socket _inner;

        public RegularSocket(Socket inner)
        {
            _inner = inner;
        }


        public void Dispose()
        {
            _inner.Dispose();
        }

        public int Send(byte[] buffer)
        {
            return _inner.Send(buffer);
        }

        public int Receive(byte[] buffer)
        {
            return _inner.Receive(buffer);
        }

        public void SendPacket(IPacket packet, int packetSize, int headerSize, Encoding enc)
        {
            //todo: move that memorystream stuff from login to here
        }

        public IToken[] ReceiveTokens(int packetSize, int headerSize, Encoding enc)
        {
            //todo: move that memorystream stuff from login to here
            return null;
        }
    }
}
