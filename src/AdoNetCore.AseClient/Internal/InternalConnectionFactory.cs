using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
#if NET45
using System.Threading.Tasks;
#endif
using AdoNetCore.AseClient.Interface;

namespace AdoNetCore.AseClient.Internal
{
    internal class InternalConnectionFactory : IInternalConnectionFactory
    {
        private readonly IConnectionParameters _parameters;
        private IPEndPoint _endpoint;

        public InternalConnectionFactory(IConnectionParameters parameters)
        {
            _parameters = parameters;
        }

        public async Task<IInternalConnection> GetNewConnection(CancellationToken token)
        {
            try
            {
                if (_endpoint == null)
                {
                    _endpoint = CreateEndpoint(_parameters.Server, _parameters.Port, token);
                }

                var socket = new Socket(_endpoint.AddressFamily, SocketType.Stream, ProtocolType.IP);

#if NET45
                var connect = Task.Factory.FromAsync(socket.BeginConnect, socket.EndConnect, _endpoint, null);
#else
                var connect = socket.ConnectAsync(_endpoint);
#endif
                connect.Wait(token);
                if (connect.IsCanceled)
                {
                    socket.Dispose();
                    throw new TimeoutException($"Timed out attempting to connect to {_parameters.Server},{_parameters.Port}");
                }

                var connection = new InternalConnection(_parameters, new RegularSocket(socket, new TokenParser()));

                try
                {
                    connection.Login();
                }
                catch (TimeoutException)
                {
                    connection.Dispose();
                    throw;
                }

                return connection;
            }
            catch (OperationCanceledException)
            {
                //todo: cleanup
                throw;
            }
            catch(Exception ex)
            {
                Logger.Instance?.WriteLine($"{nameof(InternalConnectionFactory)} encountered exception: {ex}");
                //todo: cleanup
                throw new OperationCanceledException();
            }
        }

        private static IPEndPoint CreateEndpoint(string server, int port, CancellationToken token)
        {
            return new IPEndPoint(
                IPAddress.TryParse(server, out var ip) ? ip : ResolveAddress(server, token),
                port);
        }

        private static IPAddress ResolveAddress(string server, CancellationToken token)
        {
            var dnsTask = Dns.GetHostEntryAsync(server);

            try
            {
                dnsTask.Wait(token);
            }
            catch (AggregateException ae)
            {
                throw ae.InnerException;
            }

            if (dnsTask.IsCanceled)
            {
                throw new TimeoutException($"Timed out attempting to resolve {server}");
            }

            if (dnsTask.Result.AddressList.Length == 0)
            {
                throw new SocketException(11001); //No such host is known
            }

            return dnsTask.Result.AddressList[0];
        }
    }
}
