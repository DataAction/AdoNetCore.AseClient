using System;
using System.Security.Cryptography.X509Certificates;
using Dapper;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration.Connection
{
    [TestFixture]
    public class TlsConnection
    {
        private X509Certificate2 _certificate;

        [OneTimeSetUp]
        public void TestFixtureSetup()
        {
            if (string.IsNullOrWhiteSpace(ConnectionStrings.TlsHostname) ||
                string.IsNullOrWhiteSpace(ConnectionStrings.TlsTrustedText))
            {
                Assert.Ignore("TLS tests cannot be run without specific TLS configuration");
            }

            // Check that DNS works for the name of the server.
            var dnsTask = System.Net.Dns.GetHostEntryAsync(ConnectionStrings.TlsHostname);

            dnsTask.Wait(5000);

            if (dnsTask.IsCompleted)
            {
                if (dnsTask.Result?.AddressList.Length < 0)
                {
                    Assert.Ignore($"Failed to resolve the server name {ConnectionStrings.TlsHostname} to an IP address. Perhaps a hosts file entry is required.");
                }
            }
            else if (dnsTask.IsFaulted)
            {
                Assert.Ignore($"Failed to resolve the server name {ConnectionStrings.TlsHostname} to an IP address. Perhaps a hosts file entry is required.");
            }

            // Install the public key into the Trusted Root certificate store.
            string password = null;
            _certificate = new X509Certificate2(ConnectionStrings.TlsTrustedText, password);

            var trustedRootStore = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
            trustedRootStore.Certificates.Add(_certificate);
        }

        [OneTimeTearDown]
        public void TestFixtureTearDown()
        {
            if (_certificate != null)
            {
                var trustedRootStore = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
                trustedRootStore.Certificates.Remove(_certificate);
            }
        }

        private AseConnection GetConnection()
        {
            return new AseConnection(ConnectionStrings.Tls);
        }

        [Test]
        public void Open_WithTlsConfigured_Works()
        {
            using (var connection = GetConnection())
            {
                connection.Open();

                var result = connection.QueryFirst<string>("SELECT @@ssl_ciphersuite");

                Assert.IsNotEmpty(result);
            }
        }
    }
}
