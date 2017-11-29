using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Internal.Handler;
using AdoNetCore.AseClient.Packet;
using AdoNetCore.AseClient.Token;

namespace AdoNetCore.AseClient.Internal
{
    internal class InternalConnection : IInternalConnection
    {
        private readonly ConnectionParameters _parameters;
        private readonly ISocket _socket;
        private readonly DbEnvironment _environment = new DbEnvironment();

        public InternalConnection(ConnectionParameters parameters, ISocket socket)
        {
            _parameters = parameters;
            _socket = socket;
        }

        private void SendPacket(IPacket packet)
        {
            Console.WriteLine();
            Console.WriteLine("==========  Send packet   ==========");
            _socket.SendPacket(packet, _environment);
        }

        private void ReceiveTokens(ITokenHandler[] handlers)
        {
            Console.WriteLine();
            Console.WriteLine("========== Receive Tokens ==========");
            foreach (var token in _socket.ReceiveTokens(_environment))
            {
                foreach (var handler in handlers)
                {
                    if (handler.CanHandle(token.Type))
                    {
                        handler.Handle(token);
                    }
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
                _environment.PacketSize,
                new CapabilityToken()));

            var ackHandler = new LoginTokenHandler();
            var messageHandler = new MessageTokenHandler();

            ReceiveTokens(new ITokenHandler[]
            {
                ackHandler,
                new EnvChangeTokenHandler(_environment),
                messageHandler,
            });

            messageHandler.AssertNoErrors();

            if (!ackHandler.ReceivedAck)
            {
                throw new InvalidOperationException("No login ack found");
            }

            ChangeDatabase(_parameters.Database);
        }

        public void ChangeDatabase(string databaseName)
        {
            if (string.IsNullOrWhiteSpace(databaseName) || string.Equals(databaseName, Database, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            //turns out, you can't issue an env change token to change the database, it responds saying it doesn't know how to process such a token
            SendPacket(new NormalPacket(new IToken[]
            {
                new LanguageToken
                {
                    HasParameters = false,
                    CommandText = $"USE {databaseName}"
                }
            }));

            var messageHandler = new MessageTokenHandler();

            ReceiveTokens(new ITokenHandler[]
            {
                new EnvChangeTokenHandler(_environment),
                messageHandler
            });

            messageHandler.AssertNoErrors();
        }

        public string Database
        {
            get => _environment.Database;
            private set => _environment.Database = value;
        }

        public int ExecuteNonQuery(AseCommand command)
        {
            SendPacket(new NormalPacket(BuildCommandTokens(command)));

            var doneHandler = new DoneTokenHandler();
            var messageHandler = new MessageTokenHandler();

            ReceiveTokens(new ITokenHandler[]
            {
                new EnvChangeTokenHandler(_environment),
                messageHandler,
                doneHandler
            });

            messageHandler.AssertNoErrors();

            return doneHandler.RowsAffected;
        }

        public AseDataReader ExecuteReader(CommandBehavior behavior, AseCommand command)
        {
            SendPacket(new NormalPacket(BuildCommandTokens(command)));

            var messageHandler = new MessageTokenHandler();

            ReceiveTokens(new ITokenHandler[]
            {
                new EnvChangeTokenHandler(_environment),
                messageHandler
            });

            messageHandler.AssertNoErrors();

            return new AseDataReader(new IToken[0]);
        }

        private IEnumerable<IToken> BuildCommandTokens(AseCommand command)
        {
            if (command.CommandType == CommandType.TableDirect)
            {
                throw new NotImplementedException($"{command.CommandType} is not implemented");
            }

            var commandToken = command.CommandType == CommandType.StoredProcedure
                ? BuildRpcToken(command)
                : BuildLanguageToken(command);

            if (command.Parameters?.Count > 0)
            {
                return new[] { commandToken }.Concat(BuildParameterTokens(command));
            }

            return new[] { commandToken };
        }

        private IToken BuildLanguageToken(AseCommand command)
        {
            return new LanguageToken
            {
                CommandText = command.CommandText,
                HasParameters = command.Parameters?.Count > 0
            };
        }

        private IToken BuildRpcToken(AseCommand command)
        {
            return new DbRpcToken
            {
                ProcedureName = command.CommandText,
                HasParameters = command.Parameters?.Count > 0
            };
        }

        private IEnumerable<IToken> BuildParameterTokens(AseCommand command)
        {
            //todo: implement
            return new IToken[0];
        }

        public void Dispose()
        {
            _socket.Dispose();
        }
    }
}
