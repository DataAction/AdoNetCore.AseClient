using System;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient
{
    /// <summary>
    /// Represents a Transact-SQL statement or stored procedure to execute against a SAP ASE database. This class cannot be inherited.
    /// </summary>
    public sealed class AseCommand : DbCommand
#if ENABLE_CLONEABLE_INTERFACE
        , ICloneable
#endif
    {
        private const int DefaultCommandTimeout = 30;
        private AseConnection _connection;
        private AseTransaction _transaction;
        private bool _isDisposed;
        internal readonly AseParameterCollection AseParameters;
        private CommandType _commandType;
        private int _commandTimeout = DefaultCommandTimeout;
        private string _commandText;
        private UpdateRowSource _updatedRowSource;
        private bool? _namedParameters;
        internal FormatItem FormatItem { get; set; }

        /// <summary>
        /// Constructor function for an <see cref="AseCommand"/> instance.
        /// Note: the instance will not be initialised with an AseConnection; before use this must be supplied.
        /// </summary>
        public AseCommand()
        {
            AseParameters = new AseParameterCollection();
        }

        /// <summary>
        /// Constructor function for an <see cref="AseCommand"/> instance.
        /// Note: the instance will not be initialised with an AseConnection; before use this must be supplied.
        /// </summary>
        /// <param name="commandText">The command text to execute</param>
        public AseCommand(string commandText) : this()
        {
            CommandText = commandText;
        }

        /// <summary>
        /// Constructor function for an <see cref="AseCommand"/> instance.
        /// </summary>
        /// <param name="commandText">The command text to execute</param>
        /// <param name="connection">The connection upon which to execute</param>
        public AseCommand(string commandText, AseConnection connection) : this(commandText)
        {
            _connection = connection;
            _transaction = connection.Transaction;
        }

        /// <summary>
        /// Constructor function for an <see cref="AseCommand"/> instance.
        /// </summary>
        /// <param name="commandText">The command text to execute</param>
        /// <param name="connection">The connection upon which to execute</param>
        /// <param name="transaction">The transaction within which to execute</param>
        public AseCommand(string commandText, AseConnection connection, AseTransaction transaction) : this (commandText, connection)
        {
            _transaction = transaction;
        }

        internal AseCommand(AseConnection connection) : this (string.Empty, connection, connection.Transaction)
        { }

        /// <summary>
        /// Tries to cancel the execution of a <see cref="AseCommand" />.
        /// </summary>
        /// <remarks>
        /// <para>If there is nothing to cancel, nothing occurs. However, if there is a command in process, 
        /// and the attempt to cancel fails, no exception is generated.</para>
        /// <para> In some, rare, cases, if you call <see cref="ExecuteReader()" /> then call <see cref="AseDataReader.Close" /> (implicitily or explicitly) 
        /// before calling Cancel, and then call Cancel, the cancel command will not be sent to ASE Server and 
        /// the result set can continue to stream after you call <see cref="AseDataReader.Close" />. To avoid this, make sure that you call 
        /// Cancel before closing the reader or connection.</para>
        /// </remarks>
        public override void Cancel()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(AseCommand));
            }

            Logger.Instance?.WriteLine("Cancel requested");
            _connection.InternalConnection.Cancel();
        }

        /// <summary>
        /// Creates a new instance of a <see cref="AseParameter" /> object.
        /// </summary>
        /// <remarks>
        /// The CreateParameter method is a strongly-typed version of <see cref="IDbCommand.CreateParameter" />.
        /// </remarks>
        public new AseParameter CreateParameter()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(AseCommand));
            }

            return new AseParameter();
        }

        protected override DbParameter CreateDbParameter()
        {
            return CreateParameter();
        }


        /// <summary>
        /// Executes a Transact-SQL statement against the connection and returns the number of rows affected.
        /// </summary>
        /// <returns>The number of rows affected.</returns>
        /// <remarks>
        /// <para>You can use the ExecuteNonQuery to perform catalog operations (for example, querying the structure 
        /// of a database or creating database objects such as tables), or to change the data in a database without 
        /// using a DataSet by executing UPDATE, INSERT, or DELETE statements.</para>
        /// <para>Although the ExecuteNonQuery returns no rows, any output parameters or return values mapped to 
        /// parameters are populated with data.</para>
        /// <para> For UPDATE, INSERT, and DELETE statements, the return value is the number of rows affected by the 
        /// command. When a trigger exists on a table being inserted or updated, the return value includes the number 
        /// of rows affected by both the insert or update operation and the number of rows affected by the trigger or 
        /// triggers. For all other types of statements, the return value is -1. If a rollback occurs, the return value 
        /// is also -1.</para>
        /// </remarks>
        public override int ExecuteNonQuery()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(AseCommand));
            }

            LogExecution(nameof(ExecuteNonQuery));
            return _connection.InternalConnection.ExecuteNonQuery(this, Transaction);
        }

        /// <summary>
        /// Sends the <see cref="CommandText" /> to the <see cref="Connection" /> and builds an <see cref="AseDataReader" />.
        /// </summary>
        /// <param name="behavior">One of the <see cref="System.Data.CommandBehavior" /> values.</param>
        /// <returns>An <see cref="AseDataReader" /> object.</returns>
        /// <remarks>
        /// <para>When the <see cref="CommandType" /> property is set to <b>StoredProcedure</b>, the <see cref="CommandText" /> property should be set to the 
        /// name of the stored procedure. The command executes this stored procedure when you call ExecuteReader.</para>
        /// <para>The ExecuteReader method is a strongly-typed version of <see cref="IDbCommand.ExecuteReader(CommandBehavior)" />.</para>
        /// </remarks>
        public new AseDataReader ExecuteReader(CommandBehavior behavior)
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(AseCommand));
            }

            LogExecution(nameof(ExecuteReader));
            return (AseDataReader)_connection.InternalConnection.ExecuteReader(behavior, this, Transaction);
        }

        /// <summary>		
        /// Sends the <see cref="CommandText" /> to the <see cref="Connection" /> and builds an <see cref="AseDataReader" />.		
        /// </summary>		
        /// <returns>An <see cref="AseDataReader" /> object.</returns>		
        /// <remarks>		
        /// <para>When the <see cref="CommandType" /> property is set to <b>StoredProcedure</b>, the <see cref="CommandText" /> property should be set to the 		
        /// name of the stored procedure. The command executes this stored procedure when you call ExecuteReader.</para>		
        /// <para>The ExecuteReader method is a strongly-typed version of <see cref="IDbCommand.ExecuteReader()" />.</para>		
        /// </remarks>		
        public new AseDataReader ExecuteReader()
        {
            return ExecuteReader(CommandBehavior.Default);
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            return ExecuteReader(behavior);
        }

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the result set returned by the query. 
        /// Additional columns or rows are ignored.
        /// </summary>
        /// <returns>The first column of the first row in the result set, or a null reference if the result set is empty.</returns>
        /// <remarks>
        /// <para>Use the ExecuteScalar method to retrieve a single value (for example, an aggregate value) from a database. 
        /// This requires less code than using the <see cref="ExecuteReader()" /> method, and then performing the operations that you need to 
        /// generate the single value using the data returned by a <see cref="AseDataReader" />.</para>
        /// <para>The ExecuteReader method is a strongly-typed version of <see cref="IDbCommand.ExecuteReader()" />.</para>
        /// </remarks>
        public override object ExecuteScalar()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(AseCommand));
            }

            LogExecution(nameof(ExecuteScalar));
            return _connection.InternalConnection.ExecuteScalar(this, Transaction);
        }

        private void LogExecution(string methodName)
        {
            if (Logger.Instance != null)
            {
                Logger.Instance.WriteLine();
                Logger.Instance.WriteLine($"========== {methodName.PadRight(15, ' ')}==========");
                Logger.Instance.WriteLine($"Transaction set: {Transaction != null}");
            }
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        public override void Prepare()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(AseCommand));
            }

            // Support for prepared statements is not currently implemented. But to make this a drop in replacement for other DB Providers,
            // it's better to treat this call as a no-op, than to throw a NotImplementedException.
        }

        /// <summary>
        /// Gets or sets the Transact-SQL statement, table name or stored procedure to execute at the data source.
        /// </summary>
        public override string CommandText
        {
            get
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException(nameof(AseCommand));
                }

                return _commandText;
            }
            set
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException(nameof(AseCommand));
                }

                _commandText = value;
            }
        }

        /// <summary>
        /// Gets or sets the wait time before terminating the attempt to execute a command and generating an error.
        /// </summary>
        public override int CommandTimeout
        {
            get
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException(nameof(AseCommand));
                }

                return _commandTimeout;
            }
            set
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException(nameof(AseCommand));
                }

                _commandTimeout = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating how the <see cref="CommandText" /> property is to be interpreted.
        /// </summary>
        public override CommandType CommandType
        {
            get
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException(nameof(AseCommand));
                }

                return _commandType;
            }
            set
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException(nameof(AseCommand));
                }

                _commandType = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="AseConnection" /> used by this instance of the AseCommand.
        /// </summary>
        public new AseConnection Connection
        {
            get
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException(nameof(AseCommand));
                }

                return _connection;
            }
            set
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException(nameof(AseCommand));
                }

                _connection = value;
            }
        }

        protected override DbConnection DbConnection
        {
            get => Connection;
            set => Connection = (AseConnection)value;
        }

        public new AseTransaction Transaction
        {
            get
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException(nameof(AseCommand));
                }

                return _transaction;
            }
            set
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException(nameof(AseCommand));
                }

                _transaction = value;
            }
        }

        protected override DbTransaction DbTransaction
        {
            get => _transaction;
            set => _transaction = (AseTransaction)value;
        }

        public override bool DesignTimeVisible { get; set; }

        /// <summary>
        /// Governs the default behavior of the AseCommand objects associated with this connection.
        /// </summary>
        /// <remarks>
        /// When true then the ? syntax is not supported, and a name is expected.
        /// </remarks>
        public bool NamedParameters
        {
            get
            {
                if(_namedParameters.HasValue)
                {
                    return _namedParameters.Value;
                }
                if(_connection != null)
                {
                    return _connection.NamedParameters;
                }

                return true;
            }
            set => _namedParameters = value;
        }

        /// <summary>
        /// Gets the <see cref="AseParameterCollection" /> used by this instance of the AseCommand. 
        /// </summary>
        public new AseParameterCollection Parameters => AseParameters;

        /// <summary>
        /// Gets the <see cref="AseParameterCollection" /> used by this instance of the AseCommand. 
        /// </summary>
        protected override DbParameterCollection DbParameterCollection => AseParameters;

        /// <summary>
        /// Gets or sets how command results are applied to the DataRow when used by the Update method of the DbDataAdapter.
        /// </summary>
        public override UpdateRowSource UpdatedRowSource
        {
            get
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException(nameof(AseCommand));
                }

                return _updatedRowSource;
            }
            set
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException(nameof(AseCommand));
                }

                _updatedRowSource = value;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
        }

        internal bool HasSendableParameters => AseParameters?.HasSendableParameters ?? false;

        internal void CancelIgnoreFailure()
        {
            try
            {
                Cancel();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private Task<T> InternalExecuteAsync<T>(Func<Task<T>> taskFunc, CancellationToken cancellationToken)
        {
            var source = new TaskCompletionSource<T>();
            var registration = new CancellationTokenRegistration();
            if (cancellationToken.CanBeCanceled)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Logger.Instance?.Write($"{nameof(InternalExecuteAsync)} - cancellation already requested");
                    source.SetCanceled();
                    return source.Task;
                }
                registration = cancellationToken.Register(s => ((AseCommand)s).CancelIgnoreFailure(), this);
            }

            try
            {
                // ReSharper disable once MethodSupportsCancellation
                Task.Run(taskFunc)
                    // ReSharper disable once MethodSupportsCancellation
                    .ContinueWith(t =>
                    {
                        registration.Dispose();

                        if (t.IsFaulted)
                        {
                            // Documentation states Exception can't be null if IsFaulted is true
                            // ReSharper disable PossibleNullReferenceException
                            // ReSharper disable AssignNullToNotNullAttribute
                            Logger.Instance?.WriteLine($"{nameof(InternalExecuteAsync)} - task faulted: {t.Exception.InnerException}");
                            source.SetException(t.Exception.InnerException);
                            // ReSharper restore AssignNullToNotNullAttribute
                            // ReSharper restore PossibleNullReferenceException
                        }
                        else
                        {
                            if (t.IsCanceled)
                            {
                                Logger.Instance?.WriteLine($"{nameof(InternalExecuteAsync)} - task canceled");
                                source.SetCanceled();
                            }
                            else
                            {
                                Logger.Instance?.WriteLine($"{nameof(InternalExecuteAsync)} - task completed");
                                source.SetResult(t.Result);
                            }
                        }
                    });
            }
            catch (Exception ex)
            {
                Logger.Instance?.WriteLine($"{nameof(InternalExecuteAsync)} - exception: {ex}");
                source.SetException(ex);
            }

            return source.Task;
        }

        public override Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(AseCommand));
            }

            return InternalExecuteAsync(() => _connection.InternalConnection.ExecuteNonQueryTaskRunnable(this, Transaction), cancellationToken);
        }

        protected override Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(AseCommand));
            }

            return InternalExecuteAsync(() => _connection.InternalConnection.ExecuteReaderTaskRunnable(behavior, this, Transaction), cancellationToken);
        }

        public override Task<object> ExecuteScalarAsync(CancellationToken cancellationToken)
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(AseCommand));
            }

            return ExecuteReaderAsync(cancellationToken)
                // ReSharper disable once MethodSupportsCancellation
                .ContinueWith(task =>
                {
                    var source = new TaskCompletionSource<object>();
                    if (task.IsCanceled)
                    {
                        source.SetCanceled();
                    }
                    else if (task.IsFaulted)
                    {
                        // Documentation states Exception can't be null if IsFaulted is true
                        // ReSharper disable once AssignNullToNotNullAttribute
                        // ReSharper disable once PossibleNullReferenceException
                        source.SetException(task.Exception.InnerException);
                    }
                    else
                    {
                        var reader = task.Result;
                        source.SetResult(reader.Read() ? reader[0] : null);
                    }

                    return source.Task;
                }).Unwrap();
        }

#if ENABLE_CLONEABLE_INTERFACE
        public object Clone()
        {
            var clone = new AseCommand(Connection)
            {
                CommandText = CommandText,
                CommandTimeout = CommandTimeout,
                CommandType = CommandType,
                Transaction = Transaction,
                UpdatedRowSource = UpdatedRowSource
            };

            foreach (ICloneable p in Parameters)
            {
                clone.Parameters.Add((AseParameter)p.Clone());
            }

            return clone;
        }
#endif

        public string GetDataTypeName(int colindex)
        {
            using (var reader = ExecuteReader())
            {
                return reader.GetDataTypeName(colindex);
            }
        }

        public void ResetCommandTimeout()
        {
            CommandTimeout = DefaultCommandTimeout;
        }

        public XmlReader ExecuteXmlReader()
        {
            var result = ExecuteScalar();

            if (result.GetType() != typeof(string))
            {
                throw new AseException("Column type cannot hold xml data.", 30081);
            }

            return XmlReader.Create(new StringReader((string)result));
        }
    }
}
