using System;
using System.Data;

namespace AdoNetCore.AseClient
{
    /// <summary>
    /// TODO - implement this.
    /// </summary>
    public sealed class AseTransaction : IDbTransaction
    {
        IDbConnection IDbTransaction.Connection 
        {
            get 
            {
                return Connection;
            }
        }

        public AseConnection Connection { get; private set; }

        public IsolationLevel IsolationLevel { get; set; }

        public void Commit() {}
        public void Dispose() {}
        public void Rollback() {}
    }
}