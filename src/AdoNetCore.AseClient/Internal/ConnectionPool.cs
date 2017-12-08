using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AdoNetCore.AseClient.Interface;

namespace AdoNetCore.AseClient.Internal
{
    internal class ConnectionPool : IConnectionPool
    {
        private class PoolItem
        {
            public IInternalConnection Connection { get; set; }
            public bool Available { get; set; }
            public DateTime Created { get; set; }
            public DateTime LastActive { get; set; }
        }

        private readonly IConnectionParameters _parameters;
        private readonly IInternalConnectionFactory _connectionFactory;

        private readonly object _mutex = new object();

        private const int ReserveWaitPeriodMs = 5; //TODO: figure out appropriate value

        private readonly List<PoolItem> _connections;

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
            _connections = new List<PoolItem>(_parameters.MaxPoolSize);
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

            return connection;
        }

        private IInternalConnection InternalReserve(CancellationToken token)
        {
            var wait = new ManualResetEvent(false);
            IInternalConnection connection = null;

            do
            {
                lock (_mutex)
                {
                    var now = DateTime.UtcNow;
                    var item = _connections.FirstOrDefault(i => i.Available);

                    if (item != null)
                    {
                        //todo: recreate connection if broken
                        item.Available = false;
                        connection = item.Connection;
                        wait.Set();
                    }

                    //determine if we can create new items
                    else if (_connections.Count < _parameters.MaxPoolSize)
                    {
                        var newConnection = _connectionFactory.GetNewConnection(token);

                        if (newConnection != null)
                        {
                            _connections.Add(new PoolItem
                            {
                                Connection = newConnection,
                                Created = now,
                                LastActive = now,
                                Available = false
                            });

                            connection = newConnection;
                        }

                        wait.Set();
                    }
                }
            } while (WaitHandle.WaitAny(new[] { wait, token.WaitHandle }, ReserveWaitPeriodMs) < 0);

            connection?.ChangeDatabase(_parameters.Database);
            return connection;
        }

        public void Release(IInternalConnection connection)
        {
            if (!_parameters.Pooling)
            {
                connection?.Dispose();
                return;
            }

            lock (_mutex)
            {
                var item = _connections.FirstOrDefault(i => i.Connection == connection);
                if (item != null)
                {
                    item.Available = true;
                    item.LastActive = DateTime.UtcNow;
                }
            }
        }
    }
}
