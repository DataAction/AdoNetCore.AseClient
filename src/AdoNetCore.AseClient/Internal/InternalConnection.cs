using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AdoNetCore.AseClient.Enum;
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
            Logger.Instance?.WriteLine();
            Logger.Instance?.WriteLine("----------  Send packet   ----------");
            _socket.SendPacket(packet, _environment);
        }

        private void ReceiveTokens(ITokenHandler[] handlers)
        {
            Logger.Instance?.WriteLine();
            Logger.Instance?.WriteLine("---------- Receive Tokens ----------");
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

        public int ExecuteNonQuery(AseCommand command, AseTransaction transaction)
        {
            SendPacket(new NormalPacket(BuildCommandTokens(command)));

            var doneHandler = new DoneTokenHandler();
            var messageHandler = new MessageTokenHandler();

            ReceiveTokens(new ITokenHandler[]
            {
                new EnvChangeTokenHandler(_environment),
                messageHandler,
                new ResponseParameterTokenHandler(command.AseParameters),
                doneHandler,
            });

            if (transaction != null && doneHandler.TransactionState == TranState.TDS_TRAN_ABORT)
            {
                transaction.MarkAborted();
            }

            messageHandler.AssertNoErrors();

            return doneHandler.RowsAffected;
        }

        public AseDataReader ExecuteReader(CommandBehavior behavior, AseCommand command, AseTransaction transaction)
        {
            SendPacket(new NormalPacket(BuildCommandTokens(command)));

            var doneHandler = new DoneTokenHandler();
            var messageHandler = new MessageTokenHandler();
            var dataReaderHandler = new DataReaderTokenHandler();

            ReceiveTokens(new ITokenHandler[]
            {
                new EnvChangeTokenHandler(_environment),
                messageHandler,
                dataReaderHandler,
                new ResponseParameterTokenHandler(command.AseParameters),
                doneHandler
            });

            if (transaction != null && doneHandler.TransactionState == TranState.TDS_TRAN_ABORT)
            {
                transaction.MarkAborted();
            }

            messageHandler.AssertNoErrors();

            return new AseDataReader(dataReaderHandler.Results().ToArray());
        }

        public object ExecuteScalar(AseCommand command, AseTransaction transaction)
        {
            using (var reader = (IDataReader) ExecuteReader(CommandBehavior.Default, command, transaction))
            {
                if (reader.Read())
                {
                    return reader[0];
                }
            }

            return null;
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

            if (command.HasSendableParameters)
            {
                return new[] { commandToken }.Concat(BuildParameterTokens(command.AseParameters));
            }

            return new[] { commandToken };
        }

        private IToken BuildLanguageToken(AseCommand command)
        {
            return new LanguageToken
            {
                CommandText = command.CommandText,
                HasParameters = command.HasSendableParameters
            };
        }

        private IToken BuildRpcToken(AseCommand command)
        {
            return new DbRpcToken
            {
                ProcedureName = command.CommandText,
                HasParameters = command.HasSendableParameters
            };
        }

        private IToken[] BuildParameterTokens(AseDataParameterCollection parameters)
        {
            var formatItems = new List<FormatItem>();
            var parameterItems = new List<ParametersToken.Parameter>();

            foreach (var parameter in parameters.SendableParameters)
            {
                var length = TypeMap.GetLength(parameter.DbType, parameter, _environment.Encoding);
                var formatItem = new FormatItem
                {
                    ParameterName = parameter.ParameterName,
                    DataType = TypeMap.GetTdsDataType(parameter.DbType, parameter.Value, length),
                    IsOutput = parameter.IsOutput,
                    IsNullable = parameter.IsNullable,
                    Length = length
                };

                if (parameter.DbType == DbType.Decimal && parameter.Value is decimal)
                {
                    var sqlDecimal = (SqlDecimal) (decimal) parameter.Value;
                    formatItem.Precision = sqlDecimal.Precision;
                    formatItem.Scale = sqlDecimal.Scale;
                }

                formatItems.Add(formatItem);
                parameterItems.Add(new ParametersToken.Parameter
                {
                    Format = formatItem,
                    Value = parameter.Value
                });
            }

            return new IToken[]
            {
                new ParameterFormat2Token
                {
                    Formats = formatItems.ToArray()
                },
                new ParametersToken
                {
                    Parameters = parameterItems.ToArray()
                }
            };
        }

        public void Dispose()
        {
            _socket.Dispose();
        }
    }
}
