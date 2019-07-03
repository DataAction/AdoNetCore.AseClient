using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AdoNetCore.AseClient.Interface;

namespace AdoNetCore.AseClient.Internal
{
    internal class InternalConnectionFactory : IInternalConnectionFactory
    {
        private readonly IConnectionParameters _parameters;
#if ENABLE_ARRAY_POOL
        private readonly System.Buffers.ArrayPool<byte> _arrayPool;
#endif
        private IPEndPoint _endpoint;

#if ENABLE_ARRAY_POOL
        public InternalConnectionFactory(IConnectionParameters parameters, System.Buffers.ArrayPool<byte> arrayPool)
#else
        public InternalConnectionFactory(IConnectionParameters parameters)
#endif
        {
            _parameters = parameters;
#if ENABLE_ARRAY_POOL
            _arrayPool = arrayPool;
#endif
        }

        public Task<IInternalConnection> GetNewConnection(CancellationToken token, IInfoMessageEventNotifier eventNotifier)
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
                connection.Dispose();
                socket?.Dispose();
                throw;
            }
            catch(Exception ex)
            {
                Logger.Instance?.WriteLine($"{nameof(InternalConnectionFactory)}.{nameof(GetNewConnection)} encountered exception: {ex}");
                connection.Dispose();
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
            NetworkStream networkStream = null;
            SslStream sslStream = null;
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

                networkStream = new NetworkStream(socket, true);

                if (_parameters.Encryption)
                {
                    sslStream = new SslStream(networkStream, false, UserCertificateValidationCallback);

                    var authenticate = sslStream.AuthenticateAsClientAsync(_parameters.Server);

                    authenticate.Wait(token);

                    if(authenticate.IsCanceled)
                    {
                        socket.Dispose();
                        networkStream.Dispose();
                        sslStream.Dispose();

                        throw new TimeoutException($"Timed out attempting to connect to {_parameters.Server},{_parameters.Port}"); // TODO - what error should we raise for a TLS failure?
                    }

                    return CreateConnectionInternal(sslStream);
                }

                return CreateConnectionInternal(networkStream);
            }
            catch (OperationCanceledException)
            {
                Logger.Instance?.WriteLine($"{nameof(InternalConnectionFactory)}.{nameof(GetNewConnection)} canceled operation");
                sslStream?.Dispose();
                networkStream?.Dispose();
                socket?.Dispose();
                throw;
            }
            catch(Exception)
            {
                sslStream?.Dispose();
                networkStream?.Dispose();
                socket?.Dispose();
                throw new AseException($"There is no server listening at {_parameters.Server}:{_parameters.Port}.", 30294);
            }
        }

        private InternalConnection CreateConnectionInternal(Stream networkStream)
        {
            var environment = new DbEnvironment();
            var reader = new TokenReader();

#if ENABLE_ARRAY_POOL
            return new InternalConnection(_parameters, networkStream, reader, environment, _arrayPool);
#else   
                return new InternalConnection(_parameters, networkStream, reader, environment);
#endif
        }

        private bool UserCertificateValidationCallback(object sender, X509Certificate serverCertificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors != SslPolicyErrors.None)
            {
                return false;
            }

            if (!Array.TrueForAll(chain.ChainStatus, status => status.Status == X509ChainStatusFlags.NoError))
            {
                return false;
            }

            IDictionary<string, X509Certificate> rootCertificates;

            // The TrustedFile is a file containing the public keys, in PEM format of the trusted
            // root certificates that this client is willing to accept TLS connections from.
            if (!string.IsNullOrWhiteSpace(_parameters.TrustedFile) && File.Exists(_parameters.TrustedFile))
            {
                var trustedRootCertificatesPem = File.ReadAllText(_parameters.TrustedFile, Encoding.ASCII); // TODO - what about access denied errors? How should these be returned?

                var parser = new PemParser();
                rootCertificates =
                    parser.ParseCertificates(trustedRootCertificatesPem)
                        .ToDictionary(GetCertificateKey);
            }
            // We're going a bit beyond the SAP AseClient here, as that does not support the X509Store.
            else
            {
                using (var rootStore = new X509Store(StoreName.Root, StoreLocation.CurrentUser)) // This should also find certificates installed under StoreLocation.LocalMachine.
                {
                    rootCertificates = rootStore.Certificates
                        .OfType<X509Certificate>()
                        .ToDictionary(GetCertificateKey);
                }
            }

            // If the server certificate itself is the root, weird, but ok.
            if (rootCertificates.ContainsKey(GetCertificateKey(serverCertificate)))
            {
                return true;
            }

            // If any certificates in the chain are trusted, then we will trust the server certificate.
            foreach (var chainElement in chain.ChainElements)
            {
                var key = GetCertificateKey(chainElement.Certificate);

                if (rootCertificates.ContainsKey(key))
                {
                    return true;
                }
            }

            return false;
        }

        private static string GetCertificateKey(X509Certificate certificate)
        {
            return $"{certificate.Issuer}|{Convert.ToBase64String(certificate.GetSerialNumber())}";
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
