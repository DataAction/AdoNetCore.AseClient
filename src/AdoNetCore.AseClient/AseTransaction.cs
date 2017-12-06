using System;
using System.Collections.Generic;
using System.Data;

namespace AdoNetCore.AseClient
{
    /// <summary>
    /// Represents a transaction against a SAP ASE database. This class cannot be inherited
    /// </summary>
    public sealed class AseTransaction : IDbTransaction
    {
        private static readonly Dictionary<IsolationLevel, int> IsolationLevelMap = new Dictionary<IsolationLevel, int>
        {
            {IsolationLevel.ReadUncommitted, 0},
            {IsolationLevel.ReadCommitted, 1},
            {IsolationLevel.RepeatableRead, 2},
            {IsolationLevel.Serializable, 3}
        };
        private AseConnection Connection { get; }
        private bool complete = false;

        internal AseTransaction(AseConnection connection, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (!IsolationLevelMap.ContainsKey(isolationLevel))
            {
                throw new ArgumentException("Isolation level is unsupported by ASE", nameof(isolationLevel));
            }
            Connection = connection;
            IsolationLevel = isolationLevel;
        }

        IDbConnection IDbTransaction.Connection => Connection;

        public IsolationLevel IsolationLevel { get; }

        internal void Begin()
        {
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = $"SET TRANSACTION ISOLATION LEVEL {IsolationLevelMap[IsolationLevel]}";
                command.CommandType = CommandType.Text;
                command.ExecuteNonQuery();
            }
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = "BEGIN TRANSACTION";
                command.CommandType = CommandType.Text;
                ((IDbCommand)command).Transaction = this;
                command.ExecuteNonQuery();
            }
        }

        internal void MarkAborted()
        {
            Rollback();
        }

        public void Commit()
        {
            if (complete)
            {
                return;
            }

            using (var command = Connection.CreateCommand())
            {
                command.CommandText = "COMMIT TRANSACTION";
                command.CommandType = CommandType.Text;
                ((IDbCommand) command).Transaction = this;
                command.ExecuteNonQuery();
            }

            complete = true;
        }

        public void Dispose()
        {
            Rollback();
        }

        public void Rollback()
        {
            if (complete)
            {
                return;
            }

            using (var command = Connection.CreateCommand())
            {
                command.CommandText = "ROLLBACK TRANSACTION";
                command.CommandType = CommandType.Text;
                ((IDbCommand)command).Transaction = this;
                command.ExecuteNonQuery();
            }

            complete = true;
        }
    }
}