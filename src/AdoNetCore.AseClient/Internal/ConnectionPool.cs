using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        private readonly List<IInternalConnection> _connections;

        public int PoolSize
        {
            get
            {
                lock (_mutex)
                {
                    return _connections.Count;
                }
            }
        }

        public ConnectionPool(IConnectionParameters parameters, IInternalConnectionFactory connectionFactory)
        {
            _parameters = parameters;
            _connectionFactory = connectionFactory;
            _connections = new List<IInternalConnection>(_parameters.MaxPoolSize);
            _available = new ConcurrentQueue<IInternalConnection>();
        }

        public IInternalConnection Reserve()
        {
            var src = new CancellationTokenSource(TimeSpan.FromSeconds(_parameters.LoginTimeout));
            var t = src.Token;
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

            return connection;
        }

        private IInternalConnection InternalReserve(CancellationToken token)
        {
            do
            {
                while (_available.TryDequeue(out var connection))
                {
                    if (connection.Ping())
                    {
                        return connection;
                    }
                }
                
                lock (_mutex)
                {
                    //determine if we can create new items
                    if (_connections.Count < _parameters.MaxPoolSize)
                    {
                        var newConnection = _connectionFactory.GetNewConnection(token);

                        if (newConnection != null)
                        {
                            _connections.Add(newConnection);
                            return newConnection;
                        }
                    }
                }
            } while (WaitHandle.WaitAny(new[] { token.WaitHandle }, ReserveWaitPeriodMs) < 0);

            return null;
        }

        public void Release(IInternalConnection connection)
        {
            if (!_parameters.Pooling)
            {
                connection?.Dispose();
                return;
            }

            connection.LastActive = DateTime.UtcNow;
            _available.Enqueue(connection);
        }
    }
}
