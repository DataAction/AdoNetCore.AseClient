using System.Text;

namespace AdoNetCore.AseClient.Internal
{
    internal class DbEnvironment
    {
        public int HeaderSize { get; } = 8;
        public int PacketSize { get; set; } = 512;
        public Encoding Encoding { get; set; } = Encoding.ASCII;
        public string Database { get; set; }
        public int TextSize { get; set; }
    }
}
