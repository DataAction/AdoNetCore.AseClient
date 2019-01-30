using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using AdoNetCore.AseClient.Interface;

namespace AdoNetCore.AseClient.Internal
{
    internal class ConnectionPool : IConnectionPool
    {
        //concurrency-related members
        private readonly object _mutex = new object();
        private readonly BlockingCollection<IInternalConnection> _available;

        /// <summary>
        /// The number of connections in the pool.
        /// </summary>
        public int PoolSize { get; private set; }

        //regular members
        private readonly IConnectionParameters _parameters;
        private readonly IInternalConnectionFactory _connectionFactory;

        /// <summary>
        /// The number of connections available in the pool.
        /// </summary>
        public int Available => _available.Count;

        public ConnectionPool(IConnectionParameters parameters, IInternalConnectionFactory connectionFactory)
        {
            _parameters = parameters;
            _connectionFactory = connectionFactory;
            _available = new BlockingCollection<IInternalConnection>();

            PoolSize = 0;

            if (_parameters.MinPoolSize > 0)
            {
                Task.Run(TryFillPoolToMinSize);
                Logger.Instance?.WriteLine("Pool fill task started");
            }
        }

        public IInternalConnection Reserve(IEventNotifier eventNotifier)
        {
            try
            {
                using (var src = new CancellationTokenSource())
                {
                    var t = src.Token;
                    src.CancelAfter(TimeSpan.FromSeconds(_parameters.LoginTimeout));

                    var task = _parameters.Pooling
                        ? ReservePooledConnection(t, eventNotifier)
                        : _connectionFactory.GetNewConnection(t, eventNotifier);

                    task.Wait(t);
                    var connection = task.Result;

                    if (connection == null)
                    {
                        throw new OperationCanceledException();
                    }

                    connection.ChangeDatabase(_parameters.Database);
                    connection.SetTextSize(_parameters.TextSize);
                    return connection;
                }
            }
            catch (AggregateException ae)
            {
                if (ae.InnerException is OperationCanceledException)
                {
                    throw GetTimedOutAseException(_parameters.Pooling);
                }

                if (ae.InnerException is AseException)
                {
                    ExceptionDispatchInfo.Capture(ae.InnerException).Throw();
                    throw;
                }

                throw new AseException(ae.InnerException);
            }
            catch (OperationCanceledException)
            {
                throw GetTimedOutAseException(_parameters.Pooling);
            }
            catch (AseException)
            {
                throw;
            }
            catch (SocketException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new AseException(ex);
            }
        }

        private static AseException GetTimedOutAseException(bool poolingEnabled)
        {
            return poolingEnabled
                ? new AseException("Pool timed out trying to reserve a connection")
                : new AseException("Timed out trying to establish a connection");
        }

        private async Task<IInternalConnection> ReservePooledConnection(CancellationToken cancellationToken, IEventNotifier eventNotifier)
        {
            return FetchIdlePooledConnection(eventNotifier)
                   ?? (CheckAndIncrementPoolSize()
                       //there's room in the pool! create new connection and return it
                       ? await CreateNewPooledConnection(cancellationToken, eventNotifier)
                       //pool's full, wait for something to release a connection
                       : WaitForPooledConnection(cancellationToken, eventNotifier));
        }

        private IInternalConnection FetchIdlePooledConnection(IEventNotifier eventNotifier)
        {
            var now = DateTime.UtcNow;
            while (_available.TryTake(out var connection))
            {
                if (ShouldRemoveAndReplace(connection, now))
                {
                    RemoveAndReplace(connection);
                    continue;
                }

                if (_parameters.PingServer && !connection.Ping())
                {
                    RemoveAndReplace(connection);
                    continue;
                }

                Logger.Instance?.WriteLine($"{nameof(FetchIdlePooledConnection)} returned idle connection");
                connection.EventNotifier = eventNotifier;
                return connection;
            }

            Logger.Instance?.WriteLine($"{nameof(FetchIdlePooledConnection)} found no idle connection");
            return null;
        }

        private async Task<IInternalConnection> CreateNewPooledConnection(CancellationToken cancellationToken = new CancellationToken(), IEventNotifier eventNotifier = null)
        {
            try
            {
                Logger.Instance?.WriteLine($"{nameof(CreateNewPooledConnection)} start");
                return await _connectionFactory.GetNewConnection(cancellationToken, eventNotifier);
            }
            catch
            {
                Logger.Instance?.WriteLine($"{nameof(CreateNewPooledConnection)} failed");
                RemoveConnection();
                throw;
            }
        }

        private IInternalConnection WaitForPooledConnection(CancellationToken cancellationToken, IEventNotifier eventNotifier)
        {
            Logger.Instance?.WriteLine($"{nameof(WaitForPooledConnection)} start");
            try
            {
                var conn = _available.Take(cancellationToken);
                Logger.Instance?.WriteLine($"{nameof(WaitForPooledConnection)} found connection");
                conn.EventNotifier = eventNotifier;
                return conn;
            }
            catch (OperationCanceledException)
            {
                Logger.Instance?.WriteLine($"{nameof(WaitForPooledConnection)} timed out waiting for connection");
                throw;
            }
        }

        private async Task TryFillPoolToMinSize()
        {
            Logger.Instance?.WriteLine($"{nameof(TryFillPoolToMinSize)} begin");
            while (CheckAndIncrementPoolSize(true))
            {
                try
                {
                    AddToPool(await CreateNewPooledConnection(/*Since this is an internal optimisation, there's no point supplying a cancellation token or event notifier*/));
                    Logger.Instance?.WriteLine($"{nameof(TryFillPoolToMinSize)} added new internal connection");
                }
                catch(Exception ex)
                {
                    Logger.Instance?.WriteLine($"{nameof(TryFillPoolToMinSize)} exception: {ex}");
                    RemoveConnection();
                }
            }
            Logger.Instance?.WriteLine($"{nameof(TryFillPoolToMinSize)} end");
        }

        private async Task TryReplaceConnection()
        {
            Logger.Instance?.WriteLine();
            if (CheckAndIncrementPoolSize(true))
            {
                try
                {
                    AddToPool(await CreateNewPooledConnection(/*Since this is an internal optimisation, there's no point supplying a cancellation token or event notifier*/));
                    Logger.Instance?.WriteLine($"{nameof(TryReplaceConnection)} added new internal connection");
                }
                catch (Exception ex)
                {
                    Logger.Instance?.WriteLine($"{nameof(TryReplaceConnection)} exception: {ex}");
                    RemoveConnection();
                }
            }
        }

        private bool CheckAndIncrementPoolSize(bool mustBeBelowMin = false)
        {
            lock (_mutex)
            {
                if (PoolSize < (mustBeBelowMin ? _parameters.MinPoolSize : _parameters.MaxPoolSize))
                {
                    PoolSize++;
                    return true;
                }

                return false;
            }
        }

        private void RemoveConnection(IInternalConnection connection = null)
        {
            lock (_mutex)
            {
                try
                {
                    connection?.Dispose();
                }
                finally
                {
                    PoolSize--;
                }
            }
        }

        private void RemoveAndReplace(IInternalConnection connection)
        {
            RemoveConnection(connection);
            Task.Run(TryReplaceConnection);
        }

        private bool ShouldRemoveAndReplace(IInternalConnection connection, DateTime now)
        {
            return connection.IsDoomed
                   || (_parameters.ConnectionLifetime > 0 && _parameters.ConnectionLifetime < (now - connection.Created).TotalSeconds)
                   || (_parameters.ConnectionIdleTimeout > 0 && _parameters.ConnectionIdleTimeout < (now - connection.LastActive).TotalSeconds);
        }

        public void Release(IInternalConnection connection)
        {
            if (!_parameters.Pooling)
            {
                connection?.Dispose();
                return;
            }
            
            if (connection == null)
            {
                return;
            }

            var now = DateTime.UtcNow;
            if (ShouldRemoveAndReplace(connection, now))
            {
                RemoveAndReplace(connection);
                Logger.Instance?.WriteLine("Released connection was removed. Creation of a replacement was tasked.");
                return;
            }
            
            AddToPool(connection);
        }

        private void AddToPool(IInternalConnection connection)
        {
            _available.Add(connection);
            Logger.Instance?.WriteLine("Released connection was placed in available queue");
        }
    }
}
