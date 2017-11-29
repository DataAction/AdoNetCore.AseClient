using System;
using System.Data;

namespace AdoNetCore.AseClient
{
    public sealed class AseCommand : IDbCommand
    {
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
            throw new NotImplementedException();
        }

        public void Prepare()
        {
            throw new NotImplementedException();
        }

        public string CommandText { get; set; }

        public int CommandTimeout { get; set; }

        public CommandType CommandType { get; set; }

        public IDbConnection Connection { get; set; }

        public IDataParameterCollection Parameters => AseParameters;

        public IDbTransaction Transaction { get; set; }

        public UpdateRowSource UpdatedRowSource { get; set; }

        internal bool HasSendableParameters => AseParameters?.HasSendableParameters ?? false;
    }
}
