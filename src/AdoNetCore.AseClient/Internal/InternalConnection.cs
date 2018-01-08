using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Internal.Handler;
using AdoNetCore.AseClient.Packet;
using AdoNetCore.AseClient.Token;

namespace AdoNetCore.AseClient.Internal
{
    internal class InternalConnection : IInternalConnection
    {
        private readonly IConnectionParameters _parameters;
        private readonly ISocket _socket;
        private readonly DbEnvironment _environment = new DbEnvironment();

        private enum InternalConnectionState
        {
            None,
            Ready,
            Active,
            Canceled,
            Broken
        }

        private InternalConnectionState _state = InternalConnectionState.None;
        private readonly object _stateMutex = new object();

        private void SetState(InternalConnectionState newState)
        {
            lock (_stateMutex)
            {
                if (_state == InternalConnectionState.Broken)
                {
                    throw new ArgumentException("Cannot change internal connection state as it is Broken");
                }
                _state = newState;
            }
        }

        private bool TrySetState(InternalConnectionState newState, Func<InternalConnectionState, bool> predicate)
        {
            lock (_stateMutex)
            {
                if (_state == InternalConnectionState.Broken || !predicate(_state))
                {
                    return false;
                }

                _state = newState;
                return true;
            }
        }

        public InternalConnection(IConnectionParameters parameters, ISocket socket)
        {
            _parameters = parameters;
            _socket = socket;
            _environment.PacketSize = parameters.PacketSize; //server might decide to change the packet size later anyway
        }

        private void SendPacket(IPacket packet)
        {
            Logger.Instance?.WriteLine();
            Logger.Instance?.WriteLine("----------  Send packet   ----------");
            _socket.SendPacket(packet, _environment);
        }

        private void ReceiveTokens(params ITokenHandler[] handlers)
        {
            Logger.Instance?.WriteLine();
            Logger.Instance?.WriteLine("---------- Receive Tokens ----------");
            foreach (var receivedToken in _socket.ReceiveTokens(_environment))
            {
                foreach (var handler in handlers)
                {
                    if (handler != null && handler.CanHandle(receivedToken.Type))
                    {
                        handler.Handle(receivedToken);
                    }
                }
            }
        }

        public void Login()
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

            ReceiveTokens(
                ackHandler,
                new EnvChangeTokenHandler(_environment),
                messageHandler);

            messageHandler.AssertNoErrors();

            if (!ackHandler.ReceivedAck)
            {
                IsDoomed = true;
                throw new InvalidOperationException("No login ack found");
            }

            ServerVersion = ackHandler.Token.ProgramVersion;

            Created = DateTime.UtcNow;
            SetState(InternalConnectionState.Ready);
        }

        public DateTime Created { get; private set; }
        public DateTime LastActive => _socket.LastActive;

        public bool Ping()
        {
            try
            {
                AssertExecutionStart();
                SendPacket(new NormalPacket(OptionCommandToken.CreateGet(OptionCommandToken.OptionType.TDS_OPT_STAT_TIME)));

                var messageHandler = new MessageTokenHandler();

                ReceiveTokens(messageHandler);

                AssertExecutionCompletion();
                messageHandler.AssertNoErrors();

                return true;
            }
            catch (Exception ex)
            {
                Logger.Instance?.WriteLine($"Internal ping resulted in exception: {ex}");
                IsDoomed = true;
                return false;
            }
        }

        public void ChangeDatabase(string databaseName)
        {
            if (string.IsNullOrWhiteSpace(databaseName) || string.Equals(databaseName, Database, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            AssertExecutionStart();

            //turns out, you can't issue an env change token to change the database, it responds saying it doesn't know how to process such a token
            SendPacket(new NormalPacket(new LanguageToken
            {
                HasParameters = false,
                CommandText = $"USE {databaseName}"
            }));

            var messageHandler = new MessageTokenHandler();

            ReceiveTokens(
                new EnvChangeTokenHandler(_environment),
                messageHandler);

            AssertExecutionCompletion();

            messageHandler.AssertNoErrors();
        }

        public string Database => _environment.Database;
        public string DataSource => $"{_parameters.Server},{_parameters.Port}";
        public string ServerVersion { get; private set; }

        private void InternalExecuteAsync(AseCommand command, AseTransaction transaction, TaskCompletionSource<int> rowsAffectedSource = null, TaskCompletionSource<DbDataReader> readerSource = null, CommandBehavior behavior = CommandBehavior.Default)
        {
            AssertExecutionStart();

            try
            {
                SendPacket(new NormalPacket(BuildCommandTokens(command, behavior)));

                var doneHandler = new DoneTokenHandler();
                var messageHandler = new MessageTokenHandler();
                var dataReaderHandler = readerSource != null ? new DataReaderTokenHandler() : null;

                ReceiveTokens(
                    new EnvChangeTokenHandler(_environment),
                    messageHandler,
                    dataReaderHandler,
                    new ResponseParameterTokenHandler(command.AseParameters),
                    doneHandler);

                AssertExecutionCompletion(doneHandler);

                if (transaction != null && doneHandler.TransactionState == TranState.TDS_TRAN_ABORT)
                {
                    transaction.MarkAborted();
                }

                messageHandler.AssertNoErrors();

                if (doneHandler.Canceled)
                {
                    rowsAffectedSource?.SetCanceled();
                    readerSource?.SetCanceled();
                }
                else
                {
                    rowsAffectedSource?.SetResult(doneHandler.RowsAffected);
                    readerSource?.SetResult(new AseDataReader(dataReaderHandler.Results(), command, behavior));
                }
            }
            catch (Exception ex)
            {
                rowsAffectedSource?.SetException(ex);
                readerSource?.SetException(ex);
            }
        }

        public int ExecuteNonQuery(AseCommand command, AseTransaction transaction)
        {
            try
            {
                var execTask = ExecuteNonQueryTaskRunnable(command, transaction);
                execTask.Wait();
                return execTask.Result;
            }
            catch (AggregateException ae)
            {
                ExceptionDispatchInfo.Capture(ae.InnerException).Throw();
                throw;
            }
        }

        public Task<int> ExecuteNonQueryTaskRunnable(AseCommand command, AseTransaction transaction)
        {
            var rowsAffectedSource = new TaskCompletionSource<int>();
            InternalExecuteAsync(command, transaction, rowsAffectedSource);
            return rowsAffectedSource.Task;
        }

        public DbDataReader ExecuteReader(CommandBehavior behavior, AseCommand command, AseTransaction transaction)
        {
            try
            {
                var readerTask = ExecuteReaderTaskRunnable(behavior, command, transaction);
                readerTask.Wait();
                return readerTask.Result;
            }
            catch (AggregateException ae)
            {
                ExceptionDispatchInfo.Capture(ae.InnerException).Throw();
                throw;
            }
        }

        public Task<DbDataReader> ExecuteReaderTaskRunnable(CommandBehavior behavior, AseCommand command, AseTransaction transaction)
        {
            var readerSource = new TaskCompletionSource<DbDataReader>();
            InternalExecuteAsync(command, transaction, null, readerSource, behavior);
            return readerSource.Task;
        }

        public object ExecuteScalar(AseCommand command, AseTransaction transaction)
        {
            using (var reader = (IDataReader)ExecuteReader(CommandBehavior.SingleRow, command, transaction))
            {
                if (reader.Read())
                {
                    return reader[0];
                }
            }

            return null;
        }

        public void Cancel()
        {
            if (TrySetState(InternalConnectionState.Canceled, s => s == InternalConnectionState.Active))
            {
                Logger.Instance?.WriteLine("Canceling...");
                SendPacket(new AttentionPacket());
            }
            else
            {
                Logger.Instance?.WriteLine("Did not issue cancel packet as connection is not in the Active state");
            }
        }

        private void AssertExecutionStart()
        {
            if (!TrySetState(InternalConnectionState.Active, s => s == InternalConnectionState.Ready))
            {
                IsDoomed = true;
                throw new AseException("Connection entered broken state");
            }
        }

        private void AssertExecutionCompletion(DoneTokenHandler doneHandler = null)
        {
            if (doneHandler?.Canceled == true)
            {
                TrySetState(InternalConnectionState.Ready, s => s == InternalConnectionState.Canceled);
            }

            if (_state == InternalConnectionState.Canceled)
            {
                //we're in a broken state
                IsDoomed = true;
                throw new AseException("Connection entered broken state");
            }

            TrySetState(InternalConnectionState.Ready, s => s == InternalConnectionState.Active);
        }

        public void GetTextSize()
        {
            SendPacket(new NormalPacket(OptionCommandToken.CreateGet(OptionCommandToken.OptionType.TDS_OPT_TEXTSIZE)));

            var doneHandler = new DoneTokenHandler();
            var messageHandler = new MessageTokenHandler();
            var dataReaderHandler = new DataReaderTokenHandler();

            ReceiveTokens(
                new EnvChangeTokenHandler(_environment),
                messageHandler,
                dataReaderHandler,
                doneHandler);

            messageHandler.AssertNoErrors();
        }

        public void SetTextSize(int textSize)
        {
            //todo: may need to remove this, user scripts could change the textsize value
            if (_environment.TextSize == textSize)
            {
                return;
            }

            SendPacket(new NormalPacket(OptionCommandToken.CreateSetTextSize(textSize)));

            var doneHandler = new DoneTokenHandler();
            var messageHandler = new MessageTokenHandler();
            var dataReaderHandler = new DataReaderTokenHandler();

            ReceiveTokens(
                new EnvChangeTokenHandler(_environment),
                messageHandler,
                dataReaderHandler,
                doneHandler);

            messageHandler.AssertNoErrors();

            _environment.TextSize = textSize;
        }

        private bool _isDoomed;
        public bool IsDoomed
        {
            get => _isDoomed;
            set
            {
                if (value)
                {
                    SetState(InternalConnectionState.Broken);
                }
                _isDoomed = _isDoomed || value;
            }
        }

        public bool IsDisposed { get; private set; }

        private IEnumerable<IToken> BuildCommandTokens(AseCommand command, CommandBehavior behavior)
        {
            if (command.CommandType == CommandType.TableDirect)
            {
                throw new NotImplementedException($"{command.CommandType} is not implemented");
            }

            yield return command.CommandType == CommandType.StoredProcedure
                ? BuildRpcToken(command, behavior)
                : BuildLanguageToken(command, behavior);

            foreach (var token in BuildParameterTokens(command.AseParameters, command.NamedParameters))
            {
                yield return token;
            }
        }

        private IToken BuildLanguageToken(AseCommand command, CommandBehavior behavior)
        {
            return new LanguageToken
            {
                CommandText = MakeCommand(command.CommandText, behavior),
                HasParameters = command.HasSendableParameters
            };
        }

        private IToken BuildRpcToken(AseCommand command, CommandBehavior behavior)
        {
            return new DbRpcToken
            {
                ProcedureName = MakeCommand(command.CommandText, behavior),
                HasParameters = command.HasSendableParameters
            };
        }

        private string MakeCommand(string commandText, CommandBehavior behavior)
        {
            var result = commandText;

            if ((behavior & CommandBehavior.SchemaOnly) == CommandBehavior.SchemaOnly)
            {
                result = 
$@"SET FMTONLY ON
{commandText}
SET FMTONLY OFF";
            }

            return result;
        }

        // TODO - if namedParameters is false, then look for ? characters in the command, and bind the parameters by position.
        private IToken[] BuildParameterTokens(AseParameterCollection parameters, bool namedParameters)
        {
            var formatItems = new List<FormatItem>();
            var parameterItems = new List<ParametersToken.Parameter>();

            foreach (var parameter in parameters.SendableParameters)
            {
                var parameterType = parameter.DbType;
                var isDbTypeSetExplicitly = parameter.IsDbTypeSetExplicitly;
                var length = TypeMap.GetFormatLength(parameterType, parameter, _environment.Encoding);
                var formatItem = new FormatItem
                {
                    ParameterName = parameter.ParameterName,
                    DataType = TypeMap.GetTdsDataType(parameterType, isDbTypeSetExplicitly, parameter.Value, length),
                    IsOutput = parameter.IsOutput,
                    IsNullable = parameter.IsNullable,
                    Length = length
                };

                if ((parameterType == DbType.Decimal
                    || parameterType == DbType.VarNumeric
                    || parameterType == DbType.Currency
                ) && parameter.Value is decimal)
                {
                    var sqlDecimal = (SqlDecimal)(decimal)parameter.Value;
                    formatItem.Precision = sqlDecimal.Precision;
                    formatItem.Scale = sqlDecimal.Scale;
                }

                if (parameterType == DbType.String)
                {
                    formatItem.UserType = 35;
                }

                if (parameterType == DbType.StringFixedLength)
                {
                    formatItem.UserType = 34;
                }

                formatItems.Add(formatItem);
                parameterItems.Add(new ParametersToken.Parameter
                {
                    Format = formatItem,
                    Value = parameter.Value ?? DBNull.Value
                });
            }

            if (formatItems.Count == 0)
            {
                return new IToken[0];
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
            if (IsDisposed)
            {
                return;
            }

            IsDisposed = true;
            _socket.Dispose();
        }
    }
}
