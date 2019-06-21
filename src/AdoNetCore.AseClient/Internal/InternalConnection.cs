using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using System.Text;
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
        private bool _statisticsEnabled;

        private enum InternalConnectionState
        {
            // ReSharper disable InconsistentNaming
            None,
            Ready,
            Active,
            Canceled,
            Broken
            // ReSharper restore InconsistentNaming
        }

        private InternalConnectionState _state = InternalConnectionState.None;
        private readonly object _stateMutex = new object();

        private void SetState(InternalConnectionState newState)
        {
            lock (_stateMutex)
            {
                //if the connection's state is broken, it can't get any more broken!
                if (_state == newState)
                {
                    return;
                }

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
            _environment.UseAseDecimal = parameters.UseAseDecimal;
        }

        private void SendPacket(IPacket packet)
        {
            Logger.Instance?.WriteLine();
            Logger.Instance?.WriteLine("----------  Send packet   ----------");
            try
            {
                _socket.SendPacket(packet, _environment);
            }
            catch
            {
                IsDoomed = true;
                throw;
            }
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
            SendPacket(
                new LoginPacket(
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
                    new CapabilityToken(),
                    _parameters.EncryptPassword));

            var ackHandler = new LoginTokenHandler();
            var envChangeTokenHandler = new EnvChangeTokenHandler(_environment, _parameters.Charset);
            var messageHandler = new MessageTokenHandler(EventNotifier);

            ReceiveTokens(
                ackHandler,
                envChangeTokenHandler,
                messageHandler);

            messageHandler.AssertNoErrors();

            if (!ackHandler.ReceivedAck)
            {
                IsDoomed = true;
                throw new InvalidOperationException("No login ack found");
            }

            if (ackHandler.LoginStatus == LoginAckToken.LoginStatus.TDS_LOG_NEGOTIATE)
            {
                NegotiatePassword(ackHandler.Message.MessageId, ackHandler.Parameters.Parameters, _parameters.Password);
            }
            else if (ackHandler.LoginStatus != LoginAckToken.LoginStatus.TDS_LOG_SUCCEED)
            {
                throw new AseException("Login failed.\n", 4002); //just in case the server doesn't respond with an appropriate EED token
            }

            ServerVersion = ackHandler.Token.ProgramVersion;

            Created = DateTime.UtcNow;
            SetState(InternalConnectionState.Ready);
        }

        private void NegotiatePassword(MessageToken.MsgId scheme, ParametersToken.Parameter[] parameters, string password)
        {
            try
            {
                switch (scheme)
                {
                    case MessageToken.MsgId.TDS_MSG_SEC_ENCRYPT3:
                        DoEncrypt3Scheme(parameters, password);
                        break;
                    default:
                        throw new NotSupportedException($"Server requested unsupported password encryption scheme");
                }
            }
            catch (CryptographicException ex)
            {
                //todo: expand on exception cases
                Logger.Instance?.WriteLine($"{nameof(CryptographicException)} - {ex}");
                throw new AseException("Password encryption failed");
            }
        }

        private void DoEncrypt3Scheme(ParametersToken.Parameter[] parameters, string password)
        {
            var encryptedPassword = Encryption.EncryptPassword3((int) parameters[0].Value, (byte[]) parameters[1].Value, (byte[]) parameters[2].Value, Encoding.ASCII.GetBytes(password));
            SendPacket(new NormalPacket(Encryption.BuildEncrypt3Tokens(encryptedPassword)));

            // 5. Expect an ack
            var ackHandler = new LoginTokenHandler();
            var envChangeTokenHandler = new EnvChangeTokenHandler(_environment, _parameters.Charset);
            var messageHandler = new MessageTokenHandler(EventNotifier);

            ReceiveTokens(
                ackHandler,
                envChangeTokenHandler,
                messageHandler);

            messageHandler.AssertNoErrors();

            if (ackHandler.LoginStatus != LoginAckToken.LoginStatus.TDS_LOG_SUCCEED)
            {
                throw new AseException("Login failed.\n", 4002); //just in case the server doesn't respond with an appropriate EED token
            }
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

            var messageHandler = new MessageTokenHandler(EventNotifier);
            var envChangeTokenHandler = new EnvChangeTokenHandler(_environment, _parameters.Charset);

            ReceiveTokens(
                envChangeTokenHandler,
                messageHandler);

            AssertExecutionCompletion();

            messageHandler.AssertNoErrors();
        }

        public string Database => _environment.Database;
        public string DataSource => $"{_parameters.Server},{_parameters.Port}";
        public string ServerVersion { get; private set; }

        private void InternalExecuteQueryAsync(AseCommand command, AseTransaction transaction, TaskCompletionSource<DbDataReader> readerSource, CommandBehavior behavior)
        {
            AssertExecutionStart();

            try
            {
                SendPacket(new NormalPacket(BuildCommandTokens(command, behavior)));

                var envChangeTokenHandler = new EnvChangeTokenHandler(_environment, _parameters.Charset);
                var doneHandler = new DoneTokenHandler();
                var messageHandler = new MessageTokenHandler(EventNotifier);
                var dataReaderHandler = new DataReaderTokenHandler();
                var responseParameterTokenHandler = new ResponseParameterTokenHandler(command.AseParameters);

                ReceiveTokens(
                    envChangeTokenHandler,
                    doneHandler,
                    messageHandler,
                    dataReaderHandler,
                    responseParameterTokenHandler);
                
                AssertExecutionCompletion(doneHandler);

                if (transaction != null && doneHandler.TransactionState == TranState.TDS_TRAN_ABORT)
                {
                    transaction.MarkAborted();
                }

                messageHandler.AssertNoErrors();

                if (doneHandler.Canceled)
                {
                    readerSource.TrySetCanceled(); // If we have already begun returning data, then this will get lost.
                }
                else
                {
                    readerSource.TrySetResult(new AseDataReader(dataReaderHandler.Results(), command, behavior));
                }
            }
            catch (Exception ex)
            {
                readerSource.TrySetException(ex); // If we have already begun returning data, then this will get lost.
            }
        }

        private void InternalExecuteNonQueryAsync(AseCommand command, AseTransaction transaction, TaskCompletionSource<int> rowsAffectedSource)
        {
            AssertExecutionStart();

            try
            {
                SendPacket(new NormalPacket(BuildCommandTokens(command, CommandBehavior.Default)));

                var envChangeTokenHandler = new EnvChangeTokenHandler(_environment, _parameters.Charset);
                var messageHandler = new MessageTokenHandler(EventNotifier);
                var responseParameterTokenHandler = new ResponseParameterTokenHandler(command.AseParameters);
                var doneHandler = new DoneTokenHandler();


                ReceiveTokens(
                    envChangeTokenHandler,
                    messageHandler,
                    responseParameterTokenHandler,
                    doneHandler);

                AssertExecutionCompletion(doneHandler);

                if (transaction != null && doneHandler.TransactionState == TranState.TDS_TRAN_ABORT)
                {
                    transaction.MarkAborted();
                }

                messageHandler.AssertNoErrors();

                if (doneHandler.Canceled)
                {
                    rowsAffectedSource.TrySetCanceled();
                }
                else
                {
                    rowsAffectedSource.TrySetResult(doneHandler.RowsAffected);
                }
            }
            catch (Exception ex)
            {
                rowsAffectedSource.TrySetException(ex);
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
            InternalExecuteNonQueryAsync(command, transaction, rowsAffectedSource);
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
            InternalExecuteQueryAsync(command, transaction, readerSource, behavior);
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

        public void SetTextSize(int textSize)
        {
            //todo: may need to remove this, user scripts could change the textsize value
            if (_environment.TextSize == textSize)
            {
                return;
            }

            SendPacket(new NormalPacket(OptionCommandToken.CreateSetTextSize(textSize)));

            var envChangeTokenHandler = new EnvChangeTokenHandler(_environment, _parameters.Charset);
            var messageHandler = new MessageTokenHandler(EventNotifier);
            var dataReaderHandler = new DataReaderTokenHandler();
            var doneHandler = new DoneTokenHandler();

            ReceiveTokens(
                envChangeTokenHandler,
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
                var formatItem = FormatItem.CreateForParameter(parameter, _environment);
                formatItems.Add(formatItem);
                parameterItems.Add(new ParametersToken.Parameter
                {
                    Format = formatItem,
                    Value = parameter.SendableValue
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

        public static readonly IDictionary EmptyStatistics = new ReadOnlyDictionary<string, long>(new Dictionary<string, long>());

        public bool StatisticsEnabled
        {
            get => _statisticsEnabled;
            set => _statisticsEnabled = value;
        }

        public IDictionary RetrieveStatistics()
        {
            if (!_statisticsEnabled)
            {
                return EmptyStatistics;
            }

            //todo: implement
            /*"BuffersReceived"
            "BuffersSent"
            "BytesReceived"
            "BytesSent"
            "ConnectionTime"
            "CursorOpens"
            "ExecutionTime"
            "IduCount"
            "IduRows"
            "NetworkServerTime"
            "PreparedExecs"
            "Prepares"
            "SelectCount"
            "SelectRows"
            "ServerRoundtrips"
            "SumResultSets"
            "Transactions"
            "UnpreparedExecs"*/

            return EmptyStatistics;
        }

        public IInfoMessageEventNotifier EventNotifier { get; set; }
    }
}
