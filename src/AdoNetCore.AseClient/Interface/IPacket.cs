using System.IO;
using System.Text;
using AdoNetCore.AseClient.Enum;

namespace AdoNetCore.AseClient.Interface
{
    public interface IPacket
    {
        BufferType Type { get; }
        void Write(Stream stream, Encoding enc);
        void Read(Stream stream, Encoding enc);
    }
}
