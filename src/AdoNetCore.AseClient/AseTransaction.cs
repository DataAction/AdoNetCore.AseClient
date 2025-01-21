using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;

namespace AdoNetCore.AseClient
{
    /// <summary>
    /// Represents a transaction against a SAP ASE database. This class cannot be inherited
    /// </summary>
    public sealed class AseTransaction : DbTransaction
    {
        private static readonly Dictionary<IsolationLevel, int> IsolationLevelMap = new Dictionary<IsolationLevel, int>
        {
            {IsolationLevel.ReadUncommitted, 0},
            {IsolationLevel.ReadCommitted, 1},
            {IsolationLevel.RepeatableRead, 2},
            {IsolationLevel.Serializable, 3}
        };

        private bool _complete;
        private readonly IDbConnection _connection;
        private bool _isDisposed;
        private readonly IsolationLevel _isolationLevel;

        /// <summary>
        /// Constructor function for an <see cref="AseTransaction"/> instance.
        /// </summary>
        /// <param name="connection">The <see cref="AseConnection"/> that initiated this <see cref="AseTransaction"/>.</param>
        /// <param name="isolationLevel">The <see cref="IsolationLevel"/> to apply to the transaction.</param>
        /// <exception cref="ArgumentException">Thrown in the provided <paramref name="isolationLevel"/> is not an isolation level supported by ASE.</exception>
        internal AseTransaction(IDbConnection connection, IsolationLevel isolationLevel = IsolationLevel.Unspecified)
        {
            //default
            if (isolationLevel == IsolationLevel.Unspecified)
            {
                isolationLevel = IsolationLevel.ReadCommitted;
            }

            if (!IsolationLevelMap.ContainsKey(isolationLevel))
            {
                throw new ArgumentException($"Isolation level '{isolationLevel}' is not supported", nameof(isolationLevel));
            }
            _connection = connection;
            _isolationLevel = isolationLevel;
            _complete = false;
            _isDisposed = false;
        }

        /// <summary>
        /// The <see cref="AseConnection"/> that initiated this <see cref="AseTransaction"/>.
        /// </summary>
        public new AseConnection Connection
        {
            get
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException(nameof(AseTransaction));
                }

                return (AseConnection)_connection;
            }
        }

        public AseConnection AseConnection => Connection;

        protected override DbConnection DbConnection => Connection;

        /// <summary>
        /// The <see cref="IsolationLevel"/> to apply to the transaction.
        /// </summary>
        public override IsolationLevel IsolationLevel
        {
            get
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException(nameof(AseTransaction));
                }

                return _isolationLevel;
            }
        }

        internal void Begin()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(AseTransaction));
            }

            using (var command = _connection.CreateCommand())
            {
                command.CommandText = $"SET TRANSACTION ISOLATION LEVEL {IsolationLevelMap[IsolationLevel]}";
                command.CommandType = CommandType.Text;
                command.ExecuteNonQuery();
            }
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = "BEGIN TRANSACTION";
                command.CommandType = CommandType.Text;
                command.Transaction = this;
                command.ExecuteNonQuery();
            }
        }

        internal void MarkAborted()
        {
            Rollback();
        }

        /// <summary>
        /// Commits the transaction.
        /// </summary>
        public override void Commit()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(AseTransaction));
            }

            if (_complete)
            {
                return;
            }

            using (var command = _connection.CreateCommand())
            {
                command.CommandText = "COMMIT TRANSACTION";
                command.CommandType = CommandType.Text;
                command.Transaction = this;
                command.ExecuteNonQuery();
            }

            _complete = true;
        }

        /// <summary>
        /// Disposes of the <see cref="AseTransaction"/>. Will implicitly rolls back the transaction if it has not already been rolled back or committed.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            try
            {
                // Only rollback if the transaction is still open and the connection is open. For sure do not want to
                // attempt to rollback a transaction on a closed or broken connection. The only other state in the
                // ConnectionState that's currently used is Connecting and it doesn't seem appropriate to attempt a
                // rollback from the Connecting state.
                if (!_complete && _connection.State == ConnectionState.Open)
                {
                    ExecuteRollback();
                }
            }
            catch (Exception ex)
            {
                Debug.Assert(false, "Failed to rollback transaction during dispose");
#if NETFRAMEWORK || NETSTANDARD2_0
                Trace.TraceError(ex.ToString());
#endif
            }
            finally
            {
                base.Dispose(disposing);
                _isDisposed = true;
            }
        }

        internal bool IsDisposed => _isDisposed;

        /// <summary>
        /// Rolls back the transaction.
        /// </summary>
        public override void Rollback()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(AseTransaction));
            }

            if (_complete)
            {
                return;
            }

            ExecuteRollback();
        }
        private void ExecuteRollback()
        { 
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = "ROLLBACK TRANSACTION";
                command.CommandType = CommandType.Text;
                command.Transaction = this;
                command.ExecuteNonQuery();
            }

            _complete = true;
        }
    }
}
