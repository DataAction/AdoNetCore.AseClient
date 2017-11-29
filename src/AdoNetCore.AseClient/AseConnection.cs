using System;
using System.Data;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient
{
    public sealed class AseConnection : IDbConnection
    {
        private IInternalConnection _internal;


        public AseConnection(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public void Dispose()
        {
            Close();
        }

        public IDbTransaction BeginTransaction()
        {
            throw new NotImplementedException();
        }

        public IDbTransaction BeginTransaction(IsolationLevel il)
        {
            throw new NotImplementedException();
        }

        public void ChangeDatabase(string databaseName) => _internal.ChangeDatabase(databaseName);

        public void Close()
        {
            if (State == ConnectionState.Closed)
            {
                return;
            }

            ConnectionPoolManager.Release(ConnectionString, _internal);
            _internal = null;
            State = ConnectionState.Closed;
        }

        public IDbCommand CreateCommand()
        {
            return new AseCommand(this);
        }

        public void Open()
        {
            if (State != ConnectionState.Closed)
            {
                throw new InvalidOperationException("Cannot open a connection which is not closed");
            }

            State = ConnectionState.Connecting;

            _internal = ConnectionPoolManager.Reserve(ConnectionString);

            State = ConnectionState.Open;
        }

        public string ConnectionString { get; set; }

        public int ConnectionTimeout { get; private set; } = 30;

        public string Database => _internal.Database;

        public ConnectionState State { get; private set; }

        internal IInternalConnection InternalConnection => _internal;
    }
}
