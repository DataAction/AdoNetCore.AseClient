using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Internal.BulkCopy;
using AdoNetCore.AseClient.Internal.Handler;
using AdoNetCore.AseClient.Packet;
using AdoNetCore.AseClient.Token;

namespace AdoNetCore.AseClient.Internal
{
    internal class InternalConnection : IInternalConnection
    {
        private readonly IConnectionParameters _parameters;
        private readonly Stream _networkStream;
        private readonly ITokenReader _reader;
        private readonly DbEnvironment _environment;
        private readonly object _sendMutex;
        private bool _statisticsEnabled;

#if ENABLE_ARRAY_POOL
        /// <summary>
        /// The <see cref="System.Buffers.ArrayPool{T}"/> to use for efficient buffer allocation.
        /// </summary>
        private readonly System.Buffers.ArrayPool<byte> _arrayPool;
#endif
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

#if ENABLE_ARRAY_POOL
        public InternalConnection(IConnectionParameters parameters, Stream networkStream, ITokenReader reader, DbEnvironment environment, System.Buffers.ArrayPool<byte> arrayPool)
#else
        public InternalConnection(IConnectionParameters parameters, Stream networkStream, ITokenReader reader, DbEnvironment environment)
#endif
        {
            _parameters = parameters;
            _networkStream = networkStream;
            _reader = reader;
            _environment = environment;
            _environment.PacketSize = parameters.PacketSize; //server might decide to change the packet size later anyway
            _environment.UseAseDecimal = parameters.UseAseDecimal;
            _sendMutex = new object();
#if ENABLE_ARRAY_POOL
            _arrayPool = arrayPool;
#endif
        }

        private void SendPacket(IPacket packet)
        {
            Logger.Instance?.WriteLine();
            Logger.Instance?.WriteLine("----------  Send packet   ----------");
            try
            {
                lock (_sendMutex)
                {
#if ENABLE_ARRAY_POOL
                    using (var tokenStream = new TokenSendStream(_networkStream, _environment, _arrayPool))
#else
                    using (var tokenStream = new TokenSendStream(_networkStream, _environment))
#endif
                    {
                        // ReSharper disable once InconsistentlySynchronizedField
                        tokenStream.SetBufferType(packet.Type, packet.Status);

                        packet.Write(tokenStream, _environment);

                        tokenStream.Flush();
                    }
                }
            }
            catch
            {
                IsDoomed = true;
                throw;
            }
            finally
            {
                LastActive = DateTime.UtcNow;
            }
        }

        private void ReceiveTokens(params ITokenHandler[] handlers)
        {
            Logger.Instance?.WriteLine();
            Logger.Instance?.WriteLine("---------- Receive Tokens ----------");
            try
            {
#if ENABLE_ARRAY_POOL
                using (var tokenStream = new TokenReceiveStream(_networkStream, _environment, _arrayPool))
#else
                using (var tokenStream = new TokenReceiveStream(_networkStream, _environment))
#endif
                {
                    foreach (var receivedToken in _reader.Read(tokenStream, _environment))
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
            }
            finally
            {
                LastActive = DateTime.UtcNow;
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
                    new ClientCapabilityToken(_parameters.EnableServerPacketSize),
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
                        throw new NotSupportedException("Server requested unsupported password encryption scheme");
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
        public DateTime LastActive { get; private set; }

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

            ReceiveTokens(envChangeTokenHandler, messageHandler);

            AssertExecutionCompletion();

            messageHandler.AssertNoErrors();
        }

        public string Database => _environment.Database;
        public string DataSource => $"{_parameters.Server},{_parameters.Port}";
        public string ServerVersion { get; private set; }

        private void InternalExecuteQueryAsync(AseCommand command, AseTransaction transaction, TaskCompletionSource<DbDataReader> readerSource, CommandBehavior behavior)
        {
            AssertExecutionStart();
#if ENABLE_SYSTEM_DATA_COMMON_EXTENSIONS
            var dataReader = new AseDataReader(command, behavior, EventNotifier);
#else
            var dataReader = new AseDataReader(behavior, EventNotifier);
#endif
            try
            {
                SendPacket(new NormalPacket(BuildCommandTokens(command, behavior)));

                var envChangeTokenHandler = new EnvChangeTokenHandler(_environment, _parameters.Charset);
                var doneHandler = new DoneTokenHandler();
                var dataReaderHandler = new StreamingDataReaderTokenHandler(readerSource, dataReader, EventNotifier);
                var responseParameterTokenHandler = new ResponseParameterTokenHandler(command.AseParameters);

                Logger.Instance?.WriteLine();
                Logger.Instance?.WriteLine("---------- Receive Tokens ----------");
                try
                {
#if ENABLE_ARRAY_POOL
                using (var tokenStream = new TokenReceiveStream(_networkStream, _environment, _arrayPool))
#else
                    using (var tokenStream = new TokenReceiveStream(_networkStream, _environment))
#endif
                    {
                        foreach (var receivedToken in _reader.Read(tokenStream, _environment))
                        {
                            if (envChangeTokenHandler.CanHandle(receivedToken.Type))
                            {
                                envChangeTokenHandler.Handle(receivedToken);
                            }

                            if (doneHandler.CanHandle(receivedToken.Type))
                            {
                                doneHandler.Handle(receivedToken);
                            }

                            if (dataReaderHandler.CanHandle(receivedToken.Type))
                            {
                                dataReaderHandler.Handle(receivedToken);
                            }

                            if (responseParameterTokenHandler.CanHandle(receivedToken.Type))
                            {
                                responseParameterTokenHandler.Handle(receivedToken);
                            }
                        }
                    }
                }
                finally
                {
                    LastActive = DateTime.UtcNow;
                }

                // This tells the data reader to stop waiting for more results.
                dataReader.CompleteAdding();
                
                AssertExecutionCompletion(doneHandler);

                if (transaction != null && doneHandler.TransactionState == TranState.TDS_TRAN_ABORT)
                {
                    transaction.MarkAborted();
                }

                dataReaderHandler.AssertNoErrors();

                if (doneHandler.Canceled)
                {
                    readerSource.TrySetCanceled(); // If we have already begun returning data, then this will get lost.
                }
                else
                {
                    readerSource.TrySetResult(dataReader); // Catchall - covers cases where no data is returned by the server.
                }
            }
            catch (Exception ex)
            {
                // If we have already begun returning data, then this will get lost.
                if (!readerSource.TrySetException(ex)) throw;
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
                if (!rowsAffectedSource.TrySetException(ex)) throw;
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
                ExceptionDispatchInfo.Capture(ae.InnerException ?? ae).Throw();
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
                ExceptionDispatchInfo.Capture(ae.InnerException ?? ae).Throw();
                throw;
            }
        }

        public Task<DbDataReader> ExecuteReaderTaskRunnable(CommandBehavior behavior, AseCommand command, AseTransaction transaction)
        {
            var readerSource = new TaskCompletionSource<DbDataReader>();

            // If SequentialAccess is requested, run on background thread for streaming support.
            // This allows the reader to be returned as soon as first data arrives, while
            // background thread continues receiving remaining data into the BlockingCollection.
            if ((behavior & CommandBehavior.SequentialAccess) == CommandBehavior.SequentialAccess)
            {
                Task.Run(() => InternalExecuteQueryAsync(command, transaction, readerSource, behavior));
            }
            else
            {
                InternalExecuteQueryAsync(command, transaction, readerSource, behavior);
            }

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

        public void SetAnsiNull(bool enabled)
        {
            SendPacket(new NormalPacket(OptionCommandToken.CreateSetAnsiNull(enabled)));

            var envChangeTokenHandler = new EnvChangeTokenHandler(_environment, _parameters.Charset);
            var messageHandler = new MessageTokenHandler(EventNotifier);
            var doneHandler = new DoneTokenHandler();

            ReceiveTokens(
                envChangeTokenHandler,
                messageHandler,
                doneHandler);

            messageHandler.AssertNoErrors();
        }

        public bool NamedParameters
        {
            get;
            set;
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

            foreach (var token in BuildParameterTokens(command))
            {
                yield return token;
            }

        }

        private IToken BuildLanguageToken(AseCommand command, CommandBehavior behavior)
        {
            return new LanguageToken
            {
                CommandText = MakeCommand(command.CommandText, behavior, command.NamedParameters),
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

        private string MakeCommand(string commandText, CommandBehavior behavior, bool namedParameters = true)
        {
            var result = commandText;

            if (!namedParameters)
            {
                try
                {
                    result = result.ToNamedParameters();
                }
                catch (ArgumentException)
                {
                    throw new AseException("Incorrect syntax. Possible mismatched quotes on string literal, or identifier delimiter.");
                }
            }

            if ((behavior & CommandBehavior.SchemaOnly) == CommandBehavior.SchemaOnly)
            {
                result =
$@"SET FMTONLY ON
{result}
SET FMTONLY OFF";
            }

            return result;
        }

        private IToken[] BuildParameterTokens(AseCommand command)
        {
            var formatItems = new List<FormatItem>();
            var parameterItems = new List<ParametersToken.Parameter>();

            foreach (var parameter in command.Parameters.SendableParameters)
            {
                command.FormatItem = FormatItem.CreateForParameter(parameter, _environment, command);

                formatItems.Add(command.FormatItem);
                parameterItems.Add(new ParametersToken.Parameter
                {
                    Format = command.FormatItem,
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

        public void ExecuteBulkCopy(
            string destinationTableName,
            IDataReader reader,
            AseBulkCopyColumnMappingCollection columnMappings,
            int batchSize,
            int notifyAfter,
            Func<long, bool> onNotify,
            AseTransaction transaction)
        {
            AssertExecutionStart();

            try
            {
                // Step 1: Check if table uses datarows locking (affects row format padding)
                var isDatarowsLocked = CheckDatarowsLocking(destinationTableName);

                // Step 2: Send "insert bulk <tablename>" via normal language token
                SendPacket(new NormalPacket(new LanguageToken
                {
                    HasParameters = false,
                    CommandText = "insert bulk " + destinationTableName
                }));

                // Step 3: Receive response - server sends column metadata as a result set
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

                // Step 4: Parse column metadata from the response
                var serverColumns = ParseBulkColumnMetadata(dataReaderHandler);

                if (serverColumns.Length == 0)
                {
                    throw new AseException("Server returned no column metadata for bulk insert.");
                }

                // Step 5: Resolve column mappings
                var sourceOrdinals = BcpRowFormatter.ResolveColumnMappings(serverColumns, reader, columnMappings);

                // Step 6: Create row formatter for native BCP format
                var formatter = new BcpRowFormatter(serverColumns, isDatarowsLocked, _environment.Encoding, sourceOrdinals);

                // Step 7: Send rows in bulk mode
                // TDS 5.0 BCP wire format (from FreeTDS): [2-byte row_size LE] [native row data]
                // No TDS_ROW token prefix for TDS 5.0 BCP.
                long totalRowsCopied = 0;
                var currentBatchRows = 0;
                var effectiveBatchSize = batchSize > 0 ? batchSize : int.MaxValue;

                lock (_sendMutex)
                {
#if ENABLE_ARRAY_POOL
                    using (var tokenStream = new TokenSendStream(_networkStream, _environment, _arrayPool))
#else
                    using (var tokenStream = new TokenSendStream(_networkStream, _environment))
#endif
                    {
                        tokenStream.SetBufferType(BufferType.TDS_BUF_BULK, BufferStatus.TDS_BUFSTAT_NONE);

                        while (reader.Read())
                        {
                            var rowBytes = formatter.FormatRow(reader);

                            // Write 2-byte row length prefix (LE) then native row data
                            var rowLenBytes = BitConverter.GetBytes((short)rowBytes.Length);
                            tokenStream.Write(rowLenBytes, 0, 2);
                            tokenStream.Write(rowBytes, 0, rowBytes.Length);

                            currentBatchRows++;
                            totalRowsCopied++;

                            // Check for batch completion
                            if (currentBatchRows >= effectiveBatchSize)
                            {
                                // Flush current batch
                                tokenStream.Flush();

                                // Receive DONE token for this batch
                                var batchDoneHandler = new DoneTokenHandler();
                                var batchMsgHandler = new MessageTokenHandler(EventNotifier);
                                ReceiveTokens(batchMsgHandler, batchDoneHandler);
                                batchMsgHandler.AssertNoErrors();

                                if (transaction != null && batchDoneHandler.TransactionState == TranState.TDS_TRAN_ABORT)
                                {
                                    transaction.MarkAborted();
                                    throw new AseException("Transaction was aborted during bulk copy.");
                                }

                                currentBatchRows = 0;

                                // Start new bulk stream for next batch
                                tokenStream.SetBufferType(BufferType.TDS_BUF_BULK, BufferStatus.TDS_BUFSTAT_NONE);
                            }

                            // Fire notification
                            if (notifyAfter > 0 && totalRowsCopied % notifyAfter == 0)
                            {
                                if (onNotify != null && onNotify(totalRowsCopied))
                                {
                                    // User requested abort
                                    break;
                                }
                            }
                        }

                        // Flush to send all data (EOM flag in TDS packet signals end of bulk data)
                        tokenStream.Flush();
                    }
                }

                // Receive final DONE token
                var finalDoneHandler = new DoneTokenHandler();
                var finalMsgHandler = new MessageTokenHandler(EventNotifier);
                ReceiveTokens(finalMsgHandler, finalDoneHandler);
                finalMsgHandler.AssertNoErrors();

                if (transaction != null && finalDoneHandler.TransactionState == TranState.TDS_TRAN_ABORT)
                {
                    transaction.MarkAborted();
                }

                AssertExecutionCompletion(finalDoneHandler);
            }
            catch (AseException)
            {
                IsDoomed = true;
                AssertExecutionCompletion();
                throw;
            }
            catch (Exception ex)
            {
                IsDoomed = true;
                AssertExecutionCompletion();
                throw new AseException("Bulk copy failed: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Checks if the target table uses datarows locking (DOL) by querying sysstat2 from sysobjects.
        /// DOL-locked tables require 4-byte padding at the start of each BCP row.
        /// </summary>
        private bool CheckDatarowsLocking(string tableName)
        {
            // Temporarily release execution state to execute the probe query
            TrySetState(InternalConnectionState.Ready, s => s == InternalConnectionState.Active);

            try
            {
                AssertExecutionStart();

                // Query sysobjects to check the locking scheme via sysstat2 bit 0x1000
                // sysstat2 & 0x1000 != 0 means DOL (datarows-locked)
                var probeSql = $"SELECT sysstat2 FROM sysobjects WHERE name = '{tableName.Replace("'", "''")}'";

                SendPacket(new NormalPacket(new LanguageToken
                {
                    HasParameters = false,
                    CommandText = probeSql
                }));

                var envChange = new EnvChangeTokenHandler(_environment, _parameters.Charset);
                var msgHandler = new MessageTokenHandler(EventNotifier);
                var readerHandler = new DataReaderTokenHandler();
                var done = new DoneTokenHandler();

                ReceiveTokens(envChange, msgHandler, readerHandler, done);

                // Don't throw on error - just default to false (allpages locking)
                foreach (var result in readerHandler.Results())
                {
                    if (result.Rows.Count > 0 && result.Rows[0].Items.Length > 0)
                    {
                        var sysstat2 = Convert.ToInt32(result.Rows[0].Items[0]);
                        return (sysstat2 & 0x1000) != 0;
                    }
                }

                return false;
            }
            catch
            {
                // If we can't determine locking, default to allpages (no DOL padding)
                return false;
            }
            finally
            {
                // Restore Active state for the bulk copy operation
                TrySetState(InternalConnectionState.Ready, s => s == InternalConnectionState.Active);
                AssertExecutionStart();
            }
        }

        /// <summary>
        /// Parses column metadata from the server's INSERT BULK response.
        /// The server returns a result set where each ROW describes one table column:
        ///   [0] minlen (INT2), [1] maxlen (INT2), [2] colcnt (INT2),
        ///   [3] name (VARCHAR), [4] colid (INT2), [5] type (INT1),
        ///   [6] length (INT4), [7] status (INT1), [8] offset (INT2),
        ///   [9] dflt (INT1), [10] prec (INT1), [11] scale (INT1)
        /// </summary>
        private static BcpColumnMetadata[] ParseBulkColumnMetadata(DataReaderTokenHandler readerHandler)
        {
            var columns = new List<BcpColumnMetadata>();

            foreach (var result in readerHandler.Results())
            {
                for (var i = 0; i < result.Rows.Count; i++)
                {
                    var row = result.Rows[i].Items;

                    var name = Convert.ToString(row[3])?.Trim() ?? string.Empty;
                    var tdsType = (TdsDataType)Convert.ToByte(row[5]);
                    var length = Convert.ToInt32(row[6]);
                    var status = Convert.ToByte(row[7]);
                    var offset = Convert.ToInt16(row[8]);
                    var prec = Convert.ToByte(row[10]);
                    var scale = Convert.ToByte(row[11]);

                    // Map status bits to RowFormatItemStatus
                    // In the INSERT BULK metadata:
                    //   0x08 = nullable
                    //   0x10 = fixed-offset column (NOT identity)
                    var rowStatus = RowFormatItemStatus.TDS_ROW_UPDATABLE;
                    if ((status & 0x08) != 0)
                    {
                        rowStatus |= RowFormatItemStatus.TDS_ROW_NULLALLOWED;
                    }

                    // Positive offset = fixed position in row; negative = variable column
                    var isFixed = offset >= 0;

                    columns.Add(new BcpColumnMetadata
                    {
                        Ordinal = i,
                        Name = name,
                        DataType = tdsType,
                        Length = length,
                        IsFixedLength = isFixed,
                        Precision = prec,
                        Scale = scale,
                        UserType = 0,
                        RowStatus = rowStatus
                    });
                }
            }

            return columns.ToArray();
        }

        public void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            IsDisposed = true;
            _networkStream.Dispose();
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
