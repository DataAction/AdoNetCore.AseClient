using System;
using System.Text;
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

        private void SendPacket(IPacket packet)
        {
            Console.WriteLine();
            Console.WriteLine("==========  Send packet   ==========");
            _socket.SendPacket(packet, _packetSize, _headerSize, _encoding);
        }

        private void ReceiveTokens(Action<IToken>[] handlers)
        {
            Console.WriteLine();
            Console.WriteLine("========== Receive Tokens ==========");
            foreach (var token in _socket.ReceiveTokens(_packetSize, _headerSize, _encoding))
            {
                foreach (var handler in handlers)
                {
                    handler(token);
                }
            }
        }

        public void Connect()
        {
            //socket is established already
            //login
            SendPacket(new LoginPacket(
                _parameters.ClientHostName,
                _parameters.Username,
                _parameters.Password,
                _parameters.ProcessId,
                _parameters.ApplicationName,
                _parameters.Server,
                "us_english",
                _parameters.Charset,
                "ADO.NET",
                _packetSize,
                new CapabilityToken()));

            ReceiveTokens(new Action<IToken>[]
            {
                ProcessLoginAckToken,
                ProcessEnvChangeToken,
                ProcessEedToken
            });

            if (!_loginTokenReceived)
            {
                throw new InvalidOperationException("No login ack found");
            }

            ChangeDatabase(_parameters.Database);
        }

        private bool _loginTokenReceived;
        private void ProcessLoginAckToken(IToken token)
        {
            switch (token)
            {
                case LoginAckToken t:
                    _loginTokenReceived = true;
                    if (t.Status == LoginAckToken.LoginStatus.TDS_LOG_FAIL)
                    {
                        throw new AseException("Login failed.");
                    }

                    if (t.Status == LoginAckToken.LoginStatus.TDS_LOG_NEGOTIATE)
                    {
                        Console.WriteLine($"Login negotiation required");
                    }

                    if (t.Status == LoginAckToken.LoginStatus.TDS_LOG_SUCCEED)
                    {
                        Console.WriteLine($"Login success");
                    }
                    break;
                default:
                    return;
            }
        }

        private void ProcessEnvChangeToken(IToken token)
        {
            switch (token)
            {
                case EnvironmentChangeToken t:
                    foreach (var change in t.Changes)
                    {
                        Console.WriteLine($"{t.Type}: {change.Type} - {change.OldValue} -> {change.NewValue}");
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
                    break;
                default:
                    return;
            }
        }

        private void ProcessEedToken(IToken token)
        {
            switch (token)
            {
                case EedToken t:
                    var msgType = t.Severity > 10
                        ? "ERROR"
                        : "INFO ";

                    var formatted = $"{msgType} [{t.Severity}]: {t.Message}";
                    if (formatted.EndsWith("\n"))
                    {
                        Console.Write(formatted);
                    }
                    else
                    {
                        Console.WriteLine(formatted);
                    }
                    break;
                default:
                    return;
            }
        }


        public void ChangeDatabase(string databaseName)
        {
            if (string.IsNullOrWhiteSpace(databaseName) || string.Equals(databaseName, Database, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            //turns out, you can't issue an env change token to change the database, it responds saying it doesn't know how to process such a token
            SendPacket(new NormalPacket(new LanguageToken
            {
                HasParameters = false,
                CommandText = $"USE {databaseName}"
            }));

            ReceiveTokens(new Action<IToken>[]
            {
                ProcessEnvChangeToken,
                ProcessEedToken
            });
        }

        public string Database { get; private set; }
        public int ExecuteNonQuery(AseCommand command)
        {
            SendPacket(new NormalPacket(new LanguageToken
            {
                CommandText = command.CommandText,
                HasParameters = command.Parameters?.Count > 0
            }));

            rowsAffected = 0;

            ReceiveTokens(new Action<IToken>[]
            {
                ProcessEnvChangeToken,
                ProcessEedToken,
                ProcessDoneToken
            });

            return rowsAffected;
        }

        private int rowsAffected;
        public void ProcessDoneToken(IToken token)
        {
            switch (token)
            {
                case DoneToken t:
                    Console.WriteLine($"{t.Type}: {t.Status}");
                    rowsAffected = t.Count;
                    break;
                default:
                    return;
            }
        }

        public void Dispose()
        {
            _socket.Dispose();
        }
    }
}
