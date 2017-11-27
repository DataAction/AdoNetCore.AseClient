using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Token;

namespace AdoNetCore.AseClient.Internal
{
    public class InternalConnection : IInternalConnection
    {
        private readonly ConnectionParameters _parameters;
        private readonly Socket _socket;
        private readonly ITokenParser _tokenParse;

        private int _packetSize = 512;

        public InternalConnection(ConnectionParameters parameters, Socket socket, ITokenParser tokenParse)
        {
            _parameters = parameters;
            _socket = socket;
            _tokenParse = tokenParse;
        }

        public void Connect()
        {
            //TODO: make it work
            //socket is established already
            //login
            var loginPacket = new LoginPacket(_parameters.ClientHostName, _parameters.Username, _parameters.Password, _parameters.ProcessId, _parameters.ApplicationName, _parameters.Server, "us_english", _parameters.Charset, "ADO.NET", new CapabilityToken());

            using (var ms = new MemoryStream())
            {
                loginPacket.Write(ms, Encoding.ASCII);
            }

            //parse response
        }

        public void ChangeDatabase(string databaseName)
        {
            if (string.Equals(databaseName, Database, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            Database = databaseName;

            //Send a USE X command
            throw new System.NotImplementedException();
        }

        public string Database { get; private set; }

        public void Dispose()
        {
            _socket.Dispose();
        }
    }
}
