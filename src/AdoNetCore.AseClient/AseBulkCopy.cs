using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Internal.BulkCopy;

namespace AdoNetCore.AseClient
{
    /// <summary>
    /// Provides bulk copy functionality for efficiently loading large volumes of data into ASE tables
    /// using the TDS 5.0 BCP protocol. Also supports bulk extraction (BCP OUT) via ReadFromServer.
    /// </summary>
    /// <remarks>
    /// Follows the SqlBulkCopy pattern for API familiarity. TEXT/IMAGE/UNITEXT blob types are not
    /// supported in the current implementation.
    /// </remarks>
    public sealed class AseBulkCopy : IDisposable
    {
        private readonly AseConnection _connection;
        private readonly AseTransaction _transaction;
        private readonly bool _ownsConnection;
        private bool _disposed;

        /// <summary>
        /// Creates a new AseBulkCopy using an existing open connection.
        /// </summary>
        public AseBulkCopy(AseConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _transaction = null;
            _ownsConnection = false;
            ColumnMappings = new AseBulkCopyColumnMappingCollection();
        }

        /// <summary>
        /// Creates a new AseBulkCopy using an existing open connection and transaction.
        /// </summary>
        public AseBulkCopy(AseConnection connection, AseTransaction transaction)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _transaction = transaction;
            _ownsConnection = false;
            ColumnMappings = new AseBulkCopyColumnMappingCollection();
        }

        /// <summary>
        /// Creates a new AseBulkCopy using a connection string. A new connection will be opened and owned by this instance.
        /// </summary>
        public AseBulkCopy(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            _connection = new AseConnection(connectionString);
            _connection.Open();
            _transaction = null;
            _ownsConnection = true;
            ColumnMappings = new AseBulkCopyColumnMappingCollection();
        }

        /// <summary>
        /// The name of the destination table on the server.
        /// </summary>
        public string DestinationTableName { get; set; }

        /// <summary>
        /// Number of rows in each batch. At the end of each batch, the rows are sent to the server.
        /// Setting to 0 means all rows are sent in a single batch.
        /// </summary>
        public int BatchSize { get; set; }

        /// <summary>
        /// Defines the number of rows to be processed before generating a notification event.
        /// Setting to 0 disables notifications.
        /// </summary>
        public int NotifyAfter { get; set; }

        /// <summary>
        /// Column mappings between the data source and the destination table.
        /// If empty, columns are mapped by ordinal position.
        /// </summary>
        public AseBulkCopyColumnMappingCollection ColumnMappings { get; }

        /// <summary>
        /// Occurs every time the number of rows specified by <see cref="NotifyAfter"/> have been processed.
        /// </summary>
        public event AseBulkCopyRowsCopiedEventHandler AseRowsCopied;

        /// <summary>
        /// Copies all rows from the supplied IDataReader to the destination table.
        /// </summary>
        public void WriteToServer(IDataReader reader)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));
            ThrowIfDisposed();
            ValidateDestination();

            var internalConnection = _connection.InternalConnection;
            internalConnection.ExecuteBulkCopy(
                DestinationTableName,
                reader,
                ColumnMappings,
                BatchSize,
                NotifyAfter,
                OnRowsCopied,
                _transaction);
        }

#if ENABLE_SYSTEM_DATA_COMMON_EXTENSIONS
        /// <summary>
        /// Copies all rows from the supplied DataTable to the destination table.
        /// </summary>
        public void WriteToServer(DataTable table)
        {
            if (table == null) throw new ArgumentNullException(nameof(table));
            ThrowIfDisposed();
            ValidateDestination();

            using (var reader = table.CreateDataReader())
            {
                WriteToServer(reader);
            }
        }
#endif

        /// <summary>
        /// Asynchronously copies all rows from the supplied IDataReader to the destination table.
        /// </summary>
        public Task WriteToServerAsync(IDataReader reader, CancellationToken cancellationToken)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));
            ThrowIfDisposed();
            ValidateDestination();

            return Task.Run(() => WriteToServer(reader), cancellationToken);
        }

        /// <summary>
        /// Asynchronously copies all rows from the supplied IDataReader to the destination table.
        /// </summary>
        public Task WriteToServerAsync(IDataReader reader)
        {
            return WriteToServerAsync(reader, CancellationToken.None);
        }

#if ENABLE_SYSTEM_DATA_COMMON_EXTENSIONS
        /// <summary>
        /// Asynchronously copies all rows from the supplied DataTable to the destination table.
        /// </summary>
        public Task WriteToServerAsync(DataTable table, CancellationToken cancellationToken)
        {
            if (table == null) throw new ArgumentNullException(nameof(table));
            ThrowIfDisposed();
            ValidateDestination();

            return Task.Run(() => WriteToServer(table), cancellationToken);
        }
#endif

#if ENABLE_SYSTEM_DATA_COMMON_EXTENSIONS
        /// <summary>
        /// Bulk extracts data from the server into a DataTable using the specified query.
        /// </summary>
        public DataTable ReadFromServer(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) throw new ArgumentNullException(nameof(query));
            ThrowIfDisposed();

            var table = new DataTable();

            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = query;
                if (_transaction != null)
                {
                    cmd.Transaction = _transaction;
                }

                using (var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess))
                {
                    BcpDataTableWriter.Fill(table, reader, NotifyAfter, OnRowsCopied, this);
                }
            }

            return table;
        }
#endif

        /// <summary>
        /// Bulk extracts data from the server, invoking the row handler for each row.
        /// </summary>
        public void ReadFromServer(string query, Action<IDataReader> rowHandler)
        {
            if (string.IsNullOrWhiteSpace(query)) throw new ArgumentNullException(nameof(query));
            if (rowHandler == null) throw new ArgumentNullException(nameof(rowHandler));
            ThrowIfDisposed();

            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = query;
                if (_transaction != null)
                {
                    cmd.Transaction = _transaction;
                }

                using (var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess))
                {
                    long rowCount = 0;
                    while (reader.Read())
                    {
                        rowHandler(reader);
                        rowCount++;

                        if (NotifyAfter > 0 && rowCount % NotifyAfter == 0)
                        {
                            if (OnRowsCopied(rowCount))
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }

#if ENABLE_SYSTEM_DATA_COMMON_EXTENSIONS
        /// <summary>
        /// Asynchronously bulk extracts data from the server into a DataTable.
        /// </summary>
        public Task<DataTable> ReadFromServerAsync(string query, CancellationToken cancellationToken)
        {
            return Task.Run(() => ReadFromServer(query), cancellationToken);
        }
#endif

        /// <summary>
        /// Asynchronously bulk extracts data from the server, invoking the row handler for each row.
        /// </summary>
        public Task ReadFromServerAsync(string query, Action<IDataReader> rowHandler, CancellationToken cancellationToken)
        {
            return Task.Run(() => ReadFromServer(query, rowHandler), cancellationToken);
        }

        /// <summary>
        /// Fires the AseRowsCopied event. Returns true if the operation should be aborted.
        /// </summary>
        internal bool OnRowsCopied(long rowsCopied)
        {
            var handler = AseRowsCopied;
            if (handler != null)
            {
                var args = new AseBulkCopyRowsCopiedEventArgs(rowsCopied);
                handler(this, args);
                return args.Abort;
            }
            return false;
        }

        private void ValidateDestination()
        {
            if (string.IsNullOrWhiteSpace(DestinationTableName))
            {
                throw new InvalidOperationException("DestinationTableName must be set before calling WriteToServer.");
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(AseBulkCopy));
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            if (_ownsConnection)
            {
                _connection.Dispose();
            }
        }
    }
}
