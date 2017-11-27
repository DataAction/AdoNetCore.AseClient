using System;
using System.Data;

namespace AdoNetCore.AseClient
{
    public class AseCommand : IDbCommand
    {
        private readonly AseConnection _connection;

        public AseCommand(AseConnection connection)
        {
            _connection = connection;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Cancel()
        {
            throw new NotImplementedException();
        }

        public IDbDataParameter CreateParameter()
        {
            throw new NotImplementedException();
        }

        public int ExecuteNonQuery()
        {
            throw new NotImplementedException();
        }

        public IDataReader ExecuteReader()
        {
            throw new NotImplementedException();
        }

        public IDataReader ExecuteReader(CommandBehavior behavior)
        {
            throw new NotImplementedException();
        }

        public object ExecuteScalar()
        {
            throw new NotImplementedException();
        }

        public void Prepare()
        {
            throw new NotImplementedException();
        }

        public string CommandText
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public int CommandTimeout
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public CommandType CommandType
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public IDbConnection Connection
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public IDataParameterCollection Parameters
        {
            get { throw new NotImplementedException(); }
        }

        public IDbTransaction Transaction
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public UpdateRowSource UpdatedRowSource
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
    }
}
