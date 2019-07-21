using System.IO;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient.Packet
{
    internal class AttentionPacket : IPacket
    {
        public BufferType Type => BufferType.TDS_BUF_ATTN;
        public BufferStatus Status => BufferStatus.TDS_BUFSTAT_ATTN | BufferStatus.TDS_BUFSTAT_EOM;
        public void Write(Stream stream, DbEnvironment env) { }
    }
}
