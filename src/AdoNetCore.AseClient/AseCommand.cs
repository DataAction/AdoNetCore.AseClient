using System;
using System.Data;

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

        public void Cancel()
        {
            //todo: implement
        }

        public IDbDataParameter CreateParameter()
        {
            return new AseDataParameter();
        }

        public int ExecuteNonQuery()
        {
            return _connection.InternalConnection.ExecuteNonQuery(this);
        }

        public IDataReader ExecuteReader()
        {
            return ExecuteReader(CommandBehavior.Default);
        }

        public IDataReader ExecuteReader(CommandBehavior behavior)
        {
            return _connection.InternalConnection.ExecuteReader(behavior, this);
        }

        public object ExecuteScalar()
        {
            return _connection.InternalConnection.ExecuteScalar(this);
        }

        public void Prepare()
        {
            throw new NotImplementedException();
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
        public IDbConnection Connection { get; set; } // TODO - the SqlClient returns a SqlConnection from this method. Might be better to explicitly implement IDbConnection and override with a concrete type for access to non-standard functionality like async.

        /// <summary>
        /// Gets the <see cref="AseDataParameterCollection" /> used by this instance of the AseCommand. 
        /// </summary>
        public IDataParameterCollection Parameters => AseParameters; // TODO - make explicit as above.

        /// <summary>
        /// Gets or sets the <see cref="AseTransaction" /> within which the SqlCommand executes.
        /// </summary>
        public IDbTransaction Transaction { get; set; } // TODO - make explicit as above.

        /// <summary>
        /// Gets or sets how command results are applied to the <see cref="System.Data.DataRow" /> when used by the Update method of the DbDataAdapter.
        /// </summary>
        public UpdateRowSource UpdatedRowSource { get; set; }

        internal bool HasSendableParameters => AseParameters?.HasSendableParameters ?? false;
    }
}
