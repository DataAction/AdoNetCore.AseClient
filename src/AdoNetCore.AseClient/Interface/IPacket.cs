using System.IO;
using System.Text;
using AdoNetCore.AseClient.Enum;

namespace AdoNetCore.AseClient.Interface
{
    internal interface IPacket
    {
        BufferType Type { get; }
        /// <summary>
        /// Status flags to send over the wire
        /// If no flags to send, should equal BufferStatus.TDS_BUFSTAT_NONE
        /// </summary>
        BufferStatus Status { get; }
        /// <summary>
        /// Write this packet to a stream, do not worry about packet chunking, that'll happen later
        /// </summary>
        /// <param name="stream">Stream to write to</param>
        /// <param name="enc">Encoding instance to use for encoding</param>
        void Write(Stream stream, Encoding enc);
    }
}
