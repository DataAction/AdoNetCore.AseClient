using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
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

        public Task<IInternalConnection> GetNewConnection(CancellationToken token, IEventNotifier eventNotifier)
        {
            Logger.Instance?.WriteLine($"{nameof(InternalConnectionFactory)}.{nameof(GetNewConnection)} start");

            var socket = CreateSocket(token);
            var connection = CreateConnection(socket, token); //will dispose socket on fail
            connection.EventNotifier = eventNotifier;

            try
            {
                connection.Login();
                return Task.FromResult((IInternalConnection) connection);
            }
            catch (AseException)
            {
                connection?.Dispose();
                socket?.Dispose();
                throw;
            }
            catch(Exception ex)
            {
                Logger.Instance?.WriteLine($"{nameof(InternalConnectionFactory)}.{nameof(GetNewConnection)} encountered exception: {ex}");
                connection?.Dispose();
                socket?.Dispose();
                throw new OperationCanceledException();
            }
        }

        private Socket CreateSocket(CancellationToken token)
        {
            try
            {
                if (_endpoint == null)
                {
                    _endpoint = CreateEndpoint(_parameters.Server, _parameters.Port, token);
                }

                return new Socket(_endpoint.AddressFamily, SocketType.Stream, ProtocolType.IP);
            }
            catch (OperationCanceledException)
            {
                Logger.Instance?.WriteLine($"{nameof(InternalConnectionFactory)}.{nameof(GetNewConnection)} canceled operation");
                throw;
            }
            catch (Exception)
            {
                throw new AseException("Client unable to establish a connection", 30010);
            }
        }

        private InternalConnection CreateConnection(Socket socket, CancellationToken token)
        {
            try
            {
#if NET_FRAMEWORK
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

                return new InternalConnection(_parameters, new RegularSocket(socket, new TokenParser()));
            }
            catch (OperationCanceledException)
            {
                Logger.Instance?.WriteLine($"{nameof(InternalConnectionFactory)}.{nameof(GetNewConnection)} canceled operation");
                socket?.Dispose();
                throw;
            }
            catch(Exception)
            {
                socket.Dispose();
                throw new AseException($"There is no server listening at {_parameters.Server}:{_parameters.Port}.", 30294);
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
                ExceptionDispatchInfo.Capture(ae.InnerException).Throw();
                throw;
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
