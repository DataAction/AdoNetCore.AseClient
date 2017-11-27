using AdoNetCore.AseClient.Enum;

namespace AdoNetCore.AseClient.Internal
{
    public class LoginPacket
    {
        public BufferType Type => BufferType.TDS_BUF_LOGIN;
        public string Hostname { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
