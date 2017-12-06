using System;
using System.Data;
using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient
{
    /// <summary>
    /// Represents a Transact-SQL statement or stored procedure to execute against a SAP ASE database. This class cannot be inherited.
    /// </summary>
    public sealed class AseCommand : IDbCommand
    {
        // TODO - consider async
        private readonly AseConnection _connection;
        internal readonly AseDataParameterCollection AseParameters;

        public AseCommand(AseConnection connection)
        {
            _connection = connection;
            AseParameters = new AseDataParameterCollection();
        }

        public void Dispose() { }

        /// <summary>
        /// Tries to cancel the execution of a <see cref="AseCommand" />.
        /// </summary>
        /// <remarks>
        /// <para>If there is nothing to cancel, nothing occurs. However, if there is a command in process, 
        /// and the attempt to cancel fails, no exception is generated.</para>
        /// <para> In some, rare, cases, if you call <see cref="ExecuteReader" /> then call <see cref="AseDataReader.Close" /> (implicitily or explicitly) 
        /// before calling Cancel, and then call Cancel, the cancel command will not be sent to ASE Server and 
        /// the result set can continue to stream after you call <see cref="AseDataReader.Close" />. To avoid this, make sure that you call 
        /// Cancel before closing the reader or connection.</para>
        /// </remarks>
        public void Cancel()
        {
            //todo: implement
        }

        /// <summary>
        /// Creates a new instance of a <see cref="AseDataParameter" /> object.
        /// </summary>
        IDbDataParameter IDbCommand.CreateParameter()
        {
            return CreateParameter();
        }

        /// <summary>
        /// Creates a new instance of a <see cref="AseDataParameter" /> object.
        /// </summary>
        /// <remarks>
        /// The CreateParameter method is a strongly-typed version of <see cref="IDbCommand.CreateParameter" />.
        /// </remarks>
        public AseDataParameter CreateParameter()
        {
            return new AseDataParameter();
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
        public int ExecuteNonQuery()
        {
            LogExecution(nameof(ExecuteNonQuery));
            return _connection.InternalConnection.ExecuteNonQuery(this, Transaction);
        }

        /// <summary>
        /// Sends the <see cref="CommandText" /> to the <see cref="Connection" /> and builds an <see cref="AseDataReader" />.
        /// </summary>
        /// <returns>An <see cref="AseDataReader" /> object.</returns>
        /// <remarks>
        /// <para>When the <see cref="CommandType" /> property is set to <b>StoredProcedure</b>, the <see cref="CommandText" /> property should be set to the 
        /// name of the stored procedure. The command executes this stored procedure when you call ExecuteReader.</para>
        /// </remarks>
        IDataReader IDbCommand.ExecuteReader()
        {
            return ExecuteReader();
        }

        /// <summary>
        /// Sends the <see cref="CommandText" /> to the <see cref="Connection" /> and builds an <see cref="AseDataReader" />.
        /// </summary>
        /// <returns>An <see cref="AseDataReader" /> object.</returns>
        /// <remarks>
        /// <para>When the <see cref="CommandType" /> property is set to <b>StoredProcedure</b>, the <see cref="CommandText" /> property should be set to the 
        /// name of the stored procedure. The command executes this stored procedure when you call ExecuteReader.</para>
        /// <para>The ExecuteReader method is a strongly-typed version of <see cref="IDbCommand.ExecuteReader" />.</para>
        /// </remarks>
        public AseDataReader ExecuteReader()
        {
            return ExecuteReader(CommandBehavior.Default);
        }

        /// <summary>
        /// Sends the <see cref="CommandText" /> to the <see cref="Connection" /> and builds an <see cref="AseDataReader" />.
        /// </summary>
        /// <returns>An <see cref="AseDataReader" /> object.</returns>
        /// <remarks>
        /// <para>When the <see cref="CommandType" /> property is set to <b>StoredProcedure</b>, the <see cref="CommandText" /> property should be set to the 
        /// name of the stored procedure. The command executes this stored procedure when you call ExecuteReader.</para>
        /// </remarks>
        IDataReader IDbCommand.ExecuteReader(CommandBehavior behavior)
        {
            return ExecuteReader(behavior);
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
        public AseDataReader ExecuteReader(CommandBehavior behavior)
        {
            LogExecution(nameof(ExecuteReader));
            return _connection.InternalConnection.ExecuteReader(behavior, this, Transaction);
        }

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the result set returned by the query. 
        /// Additional columns or rows are ignored.
        /// </summary>
        /// <returns>The first column of the first row in the result set, or a null reference if the result set is empty.</returns>
        /// <remarks>
        /// <para>Use the ExecuteScalar method to retrieve a single value (for example, an aggregate value) from a database. 
        /// This requires less code than using the <see cref="ExecuteReader" /> method, and then performing the operations that you need to 
        /// generate the single value using the data returned by a <see cref="AseDataReader" />.</para>
        /// <para>The ExecuteReader method is a strongly-typed version of <see cref="IDbCommand.ExecuteReader" />.</para>
        /// </remarks>
        public object ExecuteScalar()
        {
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
        public void Prepare()
        {
            // Support for prepared statements is not currently implemented. But to make this a drop in replacement for other DB Providers,
            // it's better to treat this call as a no-op, than to throw a NotImplementedException.
        }

        /// <summary>
        /// Gets or sets the Transact-SQL statement, table name or stored procedure to execute at the data source.
        /// </summary>
        public string CommandText { get; set; }

        /// <summary>
        /// Gets or sets the wait time before terminating the attempt to execute a command and generating an error.
        /// </summary>
        public int CommandTimeout { get; set; }

        /// <summary>
        /// Gets or sets a value indicating how the <see cref="CommandText" /> property is to be interpreted.
        /// </summary>
        public CommandType CommandType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="AseConnection" /> used by this instance of the AseCommand.
        /// </summary>
        IDbConnection IDbCommand.Connection
        {
            get => Connection;
            set => Connection = value as AseConnection;
        }

        /// <summary>
        /// Gets or sets the <see cref="AseConnection" /> used by this instance of the AseCommand.
        /// </summary>
        public AseConnection Connection { get; set; }

        /// <summary>
        /// Gets the <see cref="AseDataParameterCollection" /> used by this instance of the AseCommand. 
        /// </summary>
        IDataParameterCollection IDbCommand.Parameters
        {
            get
            {
                return Parameters;
            }
        }

        /// <summary>
        /// Gets the <see cref="AseDataParameterCollection" /> used by this instance of the AseCommand. 
        /// </summary>
        public AseDataParameterCollection Parameters => AseParameters;

        /// <summary>
        /// Gets or sets the <see cref="AseTransaction" /> within which the SqlCommand executes.
        /// </summary>
        IDbTransaction IDbCommand.Transaction
        {
            get => Transaction;
            set => Transaction = value as AseTransaction;
        }


        /// <summary>
        /// Gets or sets the <see cref="AseTransaction" /> within which the SqlCommand executes.
        /// </summary>
        private AseTransaction Transaction { get; set; }

        /// <summary>
        /// Gets or sets how command results are applied to the <see cref="System.Data.DataRow" /> when used by the Update method of the DbDataAdapter.
        /// </summary>
        public UpdateRowSource UpdatedRowSource { get; set; }

        internal bool HasSendableParameters => AseParameters?.HasSendableParameters ?? false;
    }
}
