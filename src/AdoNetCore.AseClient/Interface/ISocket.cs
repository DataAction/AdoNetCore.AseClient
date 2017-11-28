using System;
using System.Text;

namespace AdoNetCore.AseClient.Interface
{
    public interface ISocket : IDisposable
    {
        int Send(byte[] buffer);

        int Receive(byte[] buffer);

        void SendPacket(IPacket packet, int packetSize, int headerSize, Encoding enc);

        IToken[] ReceiveTokens(int packetSize, int headerSize, Encoding enc);
    }
}
