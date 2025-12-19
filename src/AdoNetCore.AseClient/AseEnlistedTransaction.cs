#if ENABLE_SYSTEM_TRANSACTIONS
using System;
using System.Data;
using System.Transactions;
using DataIsolationLevel = System.Data.IsolationLevel;

namespace AdoNetCore.AseClient
{
    /// <summary>
    /// Handles enlistment in a System.Transactions transaction for AseConnection.
    /// This class implements IEnlistmentNotification to participate in the two-phase commit protocol.
    /// </summary>
    internal sealed class AseEnlistedTransaction : IEnlistmentNotification
    {
        private readonly AseConnection _connection;
        private readonly Transaction _systemTransaction;
        private AseTransaction _localTransaction;
        private bool _isCompleted;

        /// <summary>
        /// Gets the System.Transactions.Transaction that this enlistment is associated with.
        /// </summary>
        internal Transaction SystemTransaction => _systemTransaction;

        /// <summary>
        /// Creates a new enlisted transaction wrapper.
        /// </summary>
        /// <param name="connection">The AseConnection to enlist.</param>
        /// <param name="transaction">The System.Transactions.Transaction to enlist in.</param>
        internal AseEnlistedTransaction(AseConnection connection, Transaction transaction)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _systemTransaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
            _isCompleted = false;
        }

        /// <summary>
        /// Begins a local transaction on the connection to participate in the distributed transaction.
        /// </summary>
        internal void Begin()
        {
            if (_connection.State != ConnectionState.Open)
            {
                throw new InvalidOperationException("Connection must be open to enlist in a transaction.");
            }

            // Map System.Transactions isolation level to ADO.NET isolation level
            var isolationLevel = MapIsolationLevel(_systemTransaction.IsolationLevel);
            
            // Start a local transaction that will be committed/rolled back based on the distributed transaction outcome
            _localTransaction = new AseTransaction(_connection, isolationLevel);
            _localTransaction.Begin();

            // Enlist in the System.Transactions transaction
            _systemTransaction.EnlistVolatile(this, EnlistmentOptions.None);
        }

        /// <summary>
        /// Maps System.Transactions.IsolationLevel to System.Data.IsolationLevel.
        /// </summary>
        private static DataIsolationLevel MapIsolationLevel(System.Transactions.IsolationLevel isolationLevel)
        {
            switch (isolationLevel)
            {
                case System.Transactions.IsolationLevel.Serializable:
                    return DataIsolationLevel.Serializable;
                case System.Transactions.IsolationLevel.RepeatableRead:
                    return DataIsolationLevel.RepeatableRead;
                case System.Transactions.IsolationLevel.ReadCommitted:
                    return DataIsolationLevel.ReadCommitted;
                case System.Transactions.IsolationLevel.ReadUncommitted:
                    return DataIsolationLevel.ReadUncommitted;
                case System.Transactions.IsolationLevel.Snapshot:
                    return DataIsolationLevel.Serializable;
                case System.Transactions.IsolationLevel.Chaos:
                    return DataIsolationLevel.ReadUncommitted;
                case System.Transactions.IsolationLevel.Unspecified:
                default:
                    return DataIsolationLevel.ReadCommitted;
            }
        }

        /// <summary>
        /// Called by the transaction manager during phase 1 of the two-phase commit protocol.
        /// </summary>
        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
            if (_isCompleted)
            {
                preparingEnlistment.Prepared();
                return;
            }

            try
            {
                preparingEnlistment.Prepared();
            }
            catch (Exception ex)
            {
                preparingEnlistment.ForceRollback(ex);
            }
        }

        /// <summary>
        /// Called by the transaction manager during phase 2 when the transaction should be committed.
        /// </summary>
        public void Commit(Enlistment enlistment)
        {
            if (_isCompleted)
            {
                enlistment.Done();
                return;
            }

            try
            {
                if (_localTransaction != null && !_localTransaction.IsDisposed)
                {
                    _localTransaction.Commit();
                }
                _isCompleted = true;
                enlistment.Done();
            }
            catch (Exception)
            {
                _isCompleted = true;
                enlistment.Done();
                throw;
            }
            finally
            {
                Cleanup();
            }
        }

        /// <summary>
        /// Called by the transaction manager when the transaction should be rolled back.
        /// </summary>
        public void Rollback(Enlistment enlistment)
        {
            if (_isCompleted)
            {
                enlistment.Done();
                return;
            }

            try
            {
                if (_localTransaction != null && !_localTransaction.IsDisposed)
                {
                    _localTransaction.Rollback();
                }
            }
            catch (Exception)
            {
                // Swallow rollback exceptions
            }
            finally
            {
                _isCompleted = true;
                Cleanup();
                enlistment.Done();
            }
        }

        /// <summary>
        /// Called by the transaction manager when the transaction outcome is in doubt.
        /// </summary>
        public void InDoubt(Enlistment enlistment)
        {
            try
            {
                if (_localTransaction != null && !_localTransaction.IsDisposed)
                {
                    _localTransaction.Rollback();
                }
            }
            catch (Exception)
            {
                // Swallow exceptions during in-doubt handling
            }
            finally
            {
                _isCompleted = true;
                Cleanup();
                enlistment.Done();
            }
        }

        /// <summary>
        /// Cleans up resources after transaction completion.
        /// </summary>
        private void Cleanup()
        {
            if (_localTransaction != null && !_localTransaction.IsDisposed)
            {
                _localTransaction.Dispose();
            }
            _localTransaction = null;
            
            _connection.OnEnlistedTransactionCompleted();
        }
    }
}
#endif
