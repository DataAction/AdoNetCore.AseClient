using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.ExceptionServices;
using System.Security.Authentication;
using System.Security.Cryptography;
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
        private readonly RemoteCertificateValidationCallback _userCertificateValidationCallback;

#if ENABLE_ARRAY_POOL
        private readonly System.Buffers.ArrayPool<byte> _arrayPool;
#endif
        private IPEndPoint _endpoint;

#if ENABLE_ARRAY_POOL
        public InternalConnectionFactory(IConnectionParameters parameters, System.Buffers.ArrayPool<byte> arrayPool, RemoteCertificateValidationCallback userCertificateValidationCallback)
#else
        public InternalConnectionFactory(IConnectionParameters parameters, RemoteCertificateValidationCallback userCertificateValidationCallback)
#endif
        {
            _parameters = parameters;
            if (userCertificateValidationCallback == null)
                userCertificateValidationCallback = GetDefaultUserCertificateValidationCallback();

            _userCertificateValidationCallback = userCertificateValidationCallback;

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
                    sslStream = new SslStream(networkStream, false, _userCertificateValidationCallback.Invoke);

                    var authenticate = sslStream.AuthenticateAsClientAsync(_parameters.Server);

                    authenticate.Wait(token);

                    if (authenticate.IsCanceled)
                    {
                        socket.Dispose();
                        networkStream.Dispose();
                        sslStream.Dispose();

                        throw new TimeoutException($"Timed out attempting to connect securely to {_parameters.Server},{_parameters.Port}");
                    }

                    return CreateConnectionInternal(sslStream);
                }

                return CreateConnectionInternal(networkStream);
            }
            catch (Exception ex)
            {
                sslStream?.Dispose();
                networkStream?.Dispose();
                socket?.Dispose();

                if (ex is OperationCanceledException)
                {
                    Logger.Instance?.WriteLine($"{nameof(InternalConnectionFactory)}.{nameof(GetNewConnection)} canceled operation");

                    throw;
                }

                if (ex is AggregateException ae)
                {
                    ex = ae.InnerException;
                }

                if (ex is AseException)
                {
                    ExceptionDispatchInfo.Capture(ex).Throw();
                    throw;
                }

                if (ex is SocketException || ex is TimeoutException)
                {
                    throw new AseException($"There is no server listening at {_parameters.Server}:{_parameters.Port}.", 30294);
                }

                if (ex is AuthenticationException)
                {
                    throw new AseException($"The secure connection could not be established at {_parameters.Server}:{_parameters.Port}.", ex);
                }

                throw new AseException($"Failed to establish a connection at {_parameters.Server}:{_parameters.Port}.", ex);
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


        private RemoteCertificateValidationCallback GetDefaultUserCertificateValidationCallback()
        {
            //object sender, X509Certificate serverCertificate, X509Chain chain, SslPolicyErrors sslPolicyErrors
            return (sender, serverCertificate, chain, sslPolicyErrors) => {
                var certificateChainPolicyErrors = (sslPolicyErrors & SslPolicyErrors.RemoteCertificateChainErrors) == SslPolicyErrors.RemoteCertificateChainErrors;
                var otherPolicyErrors = (sslPolicyErrors & ~SslPolicyErrors.RemoteCertificateChainErrors) != SslPolicyErrors.None;

                // We're not concerned with chain errors as we verify the chain below.
                if (otherPolicyErrors)
                {
                    Logger.Instance?.WriteLine($"{nameof(InternalConnectionFactory)}.{nameof(GetDefaultUserCertificateValidationCallback)} secure connection failed due to policy errors: {sslPolicyErrors}");
                    return false;
                }

                var mergedStatusFlags = X509ChainStatusFlags.NoError;
                foreach (var status in chain.ChainStatus)
                {
                    mergedStatusFlags |= status.Status;
                }

                var trustedCerts = LoadTrustedFile(_parameters.TrustedFile);
                if (trustedCerts == null)
                {
                    Logger.Instance?.WriteLine($"{nameof(InternalConnectionFactory)}.{nameof(GetDefaultUserCertificateValidationCallback)} secure connection failed due to missing TrustedFile parameter.");
                    return false;
                }

#if !(NETCOREAPP1_0 || NETCOREAPP1_1) // these frameworks do not have the following X509Certificate2 constructor...
            // sometimes the chain policy is only a partial chain because it doesn't include a self signed root?
            if ((mergedStatusFlags & X509ChainStatusFlags.PartialChain) == X509ChainStatusFlags.PartialChain)
            {
                // attempt to resolve a partial root by rebuilding the cert chain including the certs from the trusted file
                chain.ChainPolicy.ExtraStore.AddRange(trustedCerts);
                if (chain.Build(new X509Certificate2(serverCertificate)))
                {
                    // Chain validated with extra roots added; accept it
                    return true;
                }
                mergedStatusFlags = X509ChainStatusFlags.NoError;
                foreach (var status in chain.ChainStatus)
                {
                    mergedStatusFlags |= status.Status;
                }
            }
#endif

                var untrustedRootChainStatusFlags = (mergedStatusFlags & X509ChainStatusFlags.UntrustedRoot) == X509ChainStatusFlags.UntrustedRoot;
                var otherChainStatusFlags = (mergedStatusFlags & ~X509ChainStatusFlags.UntrustedRoot) != X509ChainStatusFlags.NoError;

                if (otherChainStatusFlags)
                {
                    Logger.Instance?.WriteLine($"{nameof(InternalConnectionFactory)}.{nameof(GetDefaultUserCertificateValidationCallback)} secure connection failed due to chain status: {mergedStatusFlags}");
                    return false;
                }

                if (!(certificateChainPolicyErrors || untrustedRootChainStatusFlags))
                {
                    //No chain Errors, we will trust the server certificate.
                    return true;
                }

                // If any certificates in the chain are trusted, then we will trust the server certificate.
                // To do this fairly quickly we can check if thumbprints exist in the set of trusted roots.
                var set = new HashSet<string>(trustedCerts.Select(c => c.Thumbprint));

                // the chain is in an array from leaf at 0 to root at [count - 1]
                // looping from end to start should find cases generated according to sybase documentation on the first attempt
                // but it is possible that someone puts an intermediate or even the leaf cert in their trusted file
                for (int i = chain.ChainElements.Count - 1; i >= 0; i--)
                {
                    var potentialTrusted = chain.ChainElements[i].Certificate.Thumbprint;
                    if (set.Contains(potentialTrusted))
                    {
                        return true;
                    }
                }

                Logger.Instance?.WriteLine($"{nameof(InternalConnectionFactory)}.{nameof(GetDefaultUserCertificateValidationCallback)} secure connection failed due to missing root or intermediate certificate in the certificate store, or the TrustedFile.");

                return false;
            };
        }

        private static X509Certificate2[] LoadTrustedFile(string trustedFile)
        {
            // The TrustedFile is a file containing the public keys, in PEM format of the trusted
            // root certificates that this client is willing to accept TLS connections from.
            if (!string.IsNullOrWhiteSpace(trustedFile) && File.Exists(trustedFile))
            {
                try
                {
                    var trustedRootCertificatesPem = File.ReadAllText(trustedFile, Encoding.ASCII);

                    var parser = new PemParser();
                    var rootCertificates =
                        parser.ParseCertificates(trustedRootCertificatesPem).ToArray();

                    return rootCertificates;
                }
                catch (CryptographicException e)
                {
                    throw new AseException("Failed to extract a public key from the TrustedFile.", e);
                }
                catch (Exception e) when (e is PathTooLongException || e is FileNotFoundException || e is DirectoryNotFoundException || e is UnauthorizedAccessException)
                {
                    throw new AseException("Failed to open the TrustedFile.", e);
                }
            }

            return null;
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
                ExceptionDispatchInfo.Capture(ae.InnerException ?? ae).Throw();
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
