using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using AdoNetCore.AseClient.Interface;

namespace AdoNetCore.AseClient.Internal
{
    internal class ConnectionPool : IConnectionPool
    {
        //concurrency-related members
        private readonly object _mutex = new object();
        private readonly ConcurrentQueue<IInternalConnection> _available;
        private readonly ConcurrentQueue<TaskCompletionSource<IInternalConnection>> _requests;
        public int PoolSize { get; private set; }
        //regular members
        private readonly IConnectionParameters _parameters;
        private readonly IInternalConnectionFactory _connectionFactory;

        public ConnectionPool(IConnectionParameters parameters, IInternalConnectionFactory connectionFactory)
        {
            _parameters = parameters;
            _connectionFactory = connectionFactory;
            _available = new ConcurrentQueue<IInternalConnection>();
            _requests = new ConcurrentQueue<TaskCompletionSource<IInternalConnection>>();

            PoolSize = 0;

            if (_parameters.MinPoolSize > 0)
            {
                Task.Run(TryFillPoolToMinSize);
                Logger.Instance?.WriteLine("Pool fill task started");
            }
        }

        public IInternalConnection Reserve()
        {
            try
            {
                using (var src = new CancellationTokenSource())
                {
                    var t = src.Token;
                    src.CancelAfter(_parameters.LoginTimeoutMs);

                    var task = _parameters.Pooling
                        ? ReservePooledConnection(t)
                        : _connectionFactory.GetNewConnection(t);

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

        private async Task<IInternalConnection> ReservePooledConnection(CancellationToken cancellationToken)
        {
            return FetchIdlePooledConnection()
                   ?? (CheckAndIncrementPoolSize()
                       //there's room in the pool! create new connection and return it
                       ? await CreateNewPooledConnection(cancellationToken)
                       //pool's full, wait for something to release a connection
                       : await WaitForPooledConnection(cancellationToken));
        }

        private IInternalConnection FetchIdlePooledConnection()
        {
            while (_available.TryDequeue(out var connection))
            {
                if (!_parameters.PingServer || connection.Ping())
                {
                    return connection;
                }
            }

            return null;
        }

        private async Task<IInternalConnection> CreateNewPooledConnection(CancellationToken cancellationToken)
        {
            try
            {
                return await _connectionFactory.GetNewConnection(cancellationToken);
            }
            catch
            {
                RemoveConnection();
                throw;
            }
        }

        private async Task<IInternalConnection> WaitForPooledConnection(CancellationToken cancellationToken)
        {
            var src = new TaskCompletionSource<IInternalConnection>();
            _requests.Enqueue(src);

            try
            {
                src.Task.Wait(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                if (src.Task.IsCompleted)
                {
                    return src.Task.Result;
                }
                throw;
            }
            return src.Task.Result;
        }

        private async Task TryFillPoolToMinSize()
        {
            Logger.Instance?.WriteLine("FillPoolToMinSize begin");
            while (CheckAndIncrementPoolSize(true))
            {
                try
                {
                    AddToPool(await CreateNewPooledConnection(new CancellationToken()));
                    Logger.Instance?.WriteLine("FillPoolToMinSize added new internal connection");
                }
                catch(Exception ex)
                {
                    Logger.Instance?.WriteLine($"FillPoolToMinSize exception: {ex}");
                    RemoveConnection();
                }
            }
            Logger.Instance?.WriteLine("FillPoolToMinSize end");
        }

        private async Task TryReplaceConnection()
        {
            Logger.Instance?.WriteLine();
            if (CheckAndIncrementPoolSize(true))
            {
                try
                {
                    AddToPool(await CreateNewPooledConnection(new CancellationToken()));
                    Logger.Instance?.WriteLine("TryReplaceConnection added new internal connection");
                }
                catch (Exception ex)
                {
                    Logger.Instance?.WriteLine($"TryReplaceConnection exception: {ex}");
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
                   || (_parameters.ConnectionLifetime > 0 && _parameters.ConnectionLifetime < (now - connection.Created).TotalSeconds);
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
                return;
            }
            
            connection.LastActive = now;

            AddToPool(connection);
        }

        private void AddToPool(IInternalConnection connection)
        {
            //palm the connection off to an existing request
            TaskCompletionSource<IInternalConnection> src;
            while (_requests.TryDequeue(out src))
            {
                if (src.Task.IsCanceled)
                {
                    continue;
                }

                if (src.TrySetResult(connection))
                {
                    Logger.Instance?.WriteLine("Released connection was palmed-off to existing request");
                    return;
                }
            }

            //no valid requests, park it for later
            _available.Enqueue(connection);
            Logger.Instance?.WriteLine("Released connection was placed in available queue");
        }
    }
}
