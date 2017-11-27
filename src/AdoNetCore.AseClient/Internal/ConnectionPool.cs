using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using AdoNetCore.AseClient.Interface;

namespace AdoNetCore.AseClient.Internal
{
    public class ConnectionPool
    {
        private class PoolItem
        {
            public InternalConnection Connection { get; set; }
            public bool Available { get; set; }
        }

        private readonly ConnectionParameters _parameters;

        private IPEndPoint _endpoint;
        private readonly object _mutex = new object();

        private const int MaxPooledConnections = 8; //TODO: make configurable, add in min value
        private const int ReserveWaitPeriodMs = 5; //TODO: figure out appropriate value

        private readonly List<PoolItem> _connections = new List<PoolItem>(MaxPooledConnections);

        public ConnectionPool(string connectionString)
        {
            _parameters = ConnectionParameters.Parse(connectionString);
        }

        public IInternalConnection Reserve()
        {
            if (!_parameters.Pooling)
            {
                return InitialiseNewConnection();
            }

            var wait = new ManualResetEvent(false);
            InternalConnection connection = null;

            do
            {
                lock (_mutex)
                {
                    var item = _connections.FirstOrDefault(i => i.Available);

                    if (item != null)
                    {
                        //todo: recreate connection if broken
                        item.Available = false;
                        connection = item.Connection;
                        wait.Set();
                    }

                    //determine if we can create new items
                    else if (_connections.Count < MaxPooledConnections)
                    {
                        var newConnection = InitialiseNewConnection();
                        _connections.Add(new PoolItem
                        {
                            Connection = newConnection,
                            Available = false
                        });

                        connection = newConnection;
                        wait.Set();
                    }

                    //todo: if we've waited long enough, set wait
                }
            } while (!wait.WaitOne(ReserveWaitPeriodMs));

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
                var item = _connections.First(i => i.Connection == connection);
                item.Available = true;
            }
        }

        private InternalConnection InitialiseNewConnection()
        {
            var socket = new Socket(Endpoint.AddressFamily, SocketType.Stream, ProtocolType.IP);
            socket.Connect(Endpoint);
            var connection = new InternalConnection(_parameters, socket, new TokenParser());
            connection.Connect();
            return connection;
        }

        private EndPoint Endpoint
        {
            get
            {
                if (_endpoint == null)
                {
                    _endpoint = CreateEndpoint(_parameters.Server, _parameters.Port);
                }
                return _endpoint;
            }
        }

        private static IPEndPoint CreateEndpoint(string server, int port)
        {
            return new IPEndPoint(
                IPAddress.TryParse(server, out var ip) ? ip : ResolveAddress(server),
                port);
        }

        private static IPAddress ResolveAddress(string server)
        {
            var dnsTask = Dns.GetHostEntryAsync(server);
            dnsTask.Wait();
            return dnsTask.Result.AddressList.First();
        }
    }
}
