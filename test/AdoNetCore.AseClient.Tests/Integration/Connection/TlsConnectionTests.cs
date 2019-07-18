using System.IO;
using Dapper;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration.Connection
{
    [TestFixture]
    public class TlsConnectionTests
    {
        private string _trusted;
        private string _connectionString;

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
            
            // Install the public key into the trusted.txt file.
            var temporaryFile = Path.GetTempFileName();

            _trusted = Path.ChangeExtension(temporaryFile, ".txt");

            File.WriteAllText(_trusted, ConnectionStrings.TlsTrustedText);

            _connectionString = ConnectionStrings.Tls + $";TrustedFile='{_trusted}'";
        }

        [OneTimeTearDown]
        public void TestFixtureTearDown()
        {
            if (File.Exists(_trusted))
            {
                File.Delete(_trusted);
            }
        }

        [Test]
        public void Open_WithTlsConfigured_Works()
        {
            using (var connection = new AseConnection(_connectionString))
            {
                connection.Open();

                var result = connection.QueryFirst<string>("SELECT @@ssl_ciphersuite");

                Assert.IsNotEmpty(result);
            }
        }
    }
}
