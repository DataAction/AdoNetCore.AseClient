using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Packet;
using AdoNetCore.AseClient.Token;

namespace AdoNetCore.AseClient.Internal
{
    public class InternalConnection : IInternalConnection
    {
        private readonly ConnectionParameters _parameters;
        private readonly ISocket _socket;
        private readonly int _headerSize = 8;
        private readonly Encoding _encoding = Encoding.ASCII;

        private int _packetSize = 512; //512's the default but the server can send a new packet size if it so desires
        
        public InternalConnection(ConnectionParameters parameters, ISocket socket)
        {
            _parameters = parameters;
            _socket = socket;
        }

        public void Connect()
        {
            //socket is established already
            //login
            var loginPacket = new LoginPacket(_parameters.ClientHostName, _parameters.Username, _parameters.Password, _parameters.ProcessId, _parameters.ApplicationName, _parameters.Server, "us_english", _parameters.Charset, "ADO.NET", _packetSize, new CapabilityToken());
            _socket.SendPacket(loginPacket, _packetSize, _headerSize, _encoding);
            var tokens = _socket.ReceiveTokens(_packetSize, _headerSize, _encoding);

            var loginAck = tokens.OfType<LoginAckToken>().FirstOrDefault();

            if (loginAck == null || loginAck.Status == LoginAckToken.LoginStatus.TDS_LOG_FAIL)
            {
                throw new InvalidOperationException("No login ack found");
            }

            if (loginAck.Status == LoginAckToken.LoginStatus.TDS_LOG_NEGOTIATE)
            {
                Console.WriteLine($"Login negotiation required");
            }

            Console.WriteLine($"Login success");

            ProcessResponseTokens(tokens);
        }

        /// <summary>
        /// Process tokens to find any relevant to the connection (environmental notifications)
        /// </summary>
        private void ProcessResponseTokens(IEnumerable<IToken> tokens)
        {
            foreach (var change in tokens
                .Where(t => t.Type == TokenType.TDS_ENVCHANGE)
                .Cast<EnvironmentChangeToken>()
                .SelectMany(t => t.Changes))
            {
                Console.WriteLine($"Environment value change. {change.Type}: {change.OldValue} -> {change.NewValue}");
                switch (change.Type)
                {
                    case EnvironmentChangeToken.ChangeType.TDS_ENV_DB:
                        Database = change.NewValue;
                        break;
                    case EnvironmentChangeToken.ChangeType.TDS_ENV_PACKSIZE:
                        //todo: confirm this doesn't break anything
                        if (int.TryParse(change.NewValue, out int newPackSize))
                        {
                            _packetSize = newPackSize;
                        }
                        break;
                }
            }
        }

        public void ChangeDatabase(string databaseName)
        {
            if (string.Equals(databaseName, Database, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            Database = databaseName;

            //Send a USE X command
            throw new NotImplementedException();
        }

        public string Database { get; private set; }

        public void Dispose()
        {
            _socket.Dispose();
        }
    }
}
