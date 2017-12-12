using System;
using System.Collections.Concurrent;
using System.Threading;
using AdoNetCore.AseClient.Interface;

namespace AdoNetCore.AseClient.Internal
{
    internal class ConnectionPool : IConnectionPool
    {
        private readonly IConnectionParameters _parameters;
        private readonly IInternalConnectionFactory _connectionFactory;

        private readonly object _mutex = new object();

        private const int ReserveWaitPeriodMs = 5; //TODO: figure out appropriate value

        private readonly ConcurrentQueue<IInternalConnection> _available;

        public int PoolSize { get; private set; }

        public ConnectionPool(IConnectionParameters parameters, IInternalConnectionFactory connectionFactory)
        {
            _parameters = parameters;
            _connectionFactory = connectionFactory;
            _available = new ConcurrentQueue<IInternalConnection>();
            PoolSize = 0;
        }

        public IInternalConnection Reserve()
        {
            var src = new CancellationTokenSource();
            var t = src.Token;
            t.ThrowIfCancellationRequested();
            src.CancelAfter(_parameters.LoginTimeoutMs);

            t.Register(() => Logger.Instance?.WriteLine("token cancelled"));

            if (!_parameters.Pooling)
            {
                return _connectionFactory.GetNewConnection(src.Token);
            }

            var connection = InternalReserve(src.Token);

            if (connection == null)
            {
                throw new TimeoutException("Pool timed out trying to reserve a connection");
            }

            connection.ChangeDatabase(_parameters.Database);
            connection.SetTextSize(_parameters.TextSize);
            return connection;
        }

        private IInternalConnection InternalReserve(CancellationToken token)
        {
            do
            {
                while (_available.TryDequeue(out var connection))
                {
                    if (!_parameters.PingServer || connection.Ping())
                    {
                        return connection;
                    }
                }

                if (!CheckAndIncrementPoolSize())
                {
                    continue;
                }
                
                IInternalConnection newConnection = null;

                try
                {
                    newConnection = _connectionFactory.GetNewConnection(token);
                    if (newConnection == null)
                    {
                        RemoveConnection();
                    }
                    return newConnection;
                }
                catch
                {
                    RemoveConnection(newConnection);
                    throw;
                }
            } while (!token.WaitHandle.WaitOne(ReserveWaitPeriodMs));

            return null;
        }

        private bool CheckAndIncrementPoolSize()
        {
            lock (_mutex)
            {
                if (PoolSize < _parameters.MaxPoolSize)
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
                connection?.Dispose();
                PoolSize--;
            }
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

            if (connection.IsDoomed)
            {
                RemoveConnection(connection);
                return;
            }

            if (_parameters.ConnectionLifetime > 0 && _parameters.ConnectionLifetime < (now - connection.Created).TotalSeconds)
            {
                RemoveConnection(connection);
                return;
            }

            connection.LastActive = now;
            _available.Enqueue(connection);
        }
    }
}
