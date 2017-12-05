using System.Net.Sockets;

namespace AdoNetCore.AseClient.Internal
{
    public static class SocketExtensions
    {
        public static void EnsureSend(this Socket socket, byte[] buffer, int offset, int count)
        {
            var remainingBytes = count;
            var totalSentBytes = 0;
            do
            {
                var sentBytes = socket.Send(buffer, offset + totalSentBytes, remainingBytes, SocketFlags.None);
                remainingBytes -= sentBytes;
                totalSentBytes += sentBytes;
            } while (remainingBytes > 0);
        }

        public static void EnsureReceive(this Socket socket, byte[] buffer, int offset, int count)
        {
            var remainingBytes = count;
            var totalReceivedBytes = 0;
            do
            {
                var receivedBytes = socket.Receive(buffer, offset + totalReceivedBytes, remainingBytes, SocketFlags.None);
                remainingBytes -= receivedBytes;
                totalReceivedBytes += receivedBytes;
            } while (remainingBytes > 0);
        }
    }
}
