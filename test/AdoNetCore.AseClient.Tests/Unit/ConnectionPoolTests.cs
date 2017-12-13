using System;
using System.Threading;
using System.Threading.Tasks;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Internal;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Unit
{
    [TestFixture]
    public class ConnectionPoolTests
    {
        public ConnectionPoolTests()
        {
            Logger.Enable();
        }

        [Test]
        public void UponCancellationTokenTriggering_ConnectionEstablishmentFails_ThrowsTimeoutException()
        {
            var pool = new ConnectionPool(new TestConnectionParameters(), new SlowConnectionFactory());

            Assert.Throws<AseException>(() => pool.Reserve());
            Assert.AreEqual(0, pool.PoolSize);
        }

        [Test]
        public void WhenMinPoolSizeIsSet_NewPoolFillsToMin()
        {
            var parameters = new TestConnectionParameters
            {
                MinPoolSize = 10
            };
            var pool = new ConnectionPool(parameters, new ImmediateConnectionFactory(parameters));

            Task.Delay(1000).Wait();

            Assert.AreEqual(10, pool.PoolSize);
        }

        [Test]
        public void NewOpenCall_TimesOut_ShouldThrow()
        {
            var parameters = new TestConnectionParameters
            {
                MaxPoolSize = 1,
                LoginTimeout = 1
            };
            var pool = new ConnectionPool(parameters, new ImmediateConnectionFactory(parameters));

            pool.Reserve();
            Assert.Throws<AseException>(() => pool.Reserve());
        }

        private class ImmediateConnectionFactory : IInternalConnectionFactory
        {
            private readonly TestConnectionParameters _parameters;

            public ImmediateConnectionFactory(TestConnectionParameters parameters)
            {
                _parameters = parameters;
            }

            public async Task<IInternalConnection> GetNewConnection(CancellationToken token)
            {
                return new InternalConnection(_parameters, null);
            }
        }

        private class SlowConnectionFactory : IInternalConnectionFactory
        {
            public async Task<IInternalConnection> GetNewConnection(CancellationToken token)
            {
                token.WaitHandle.WaitOne();
                throw new TimeoutException($"Timed out attempting to create new connection");
            }
        }

        private class TestConnectionParameters : IConnectionParameters
        {
            public string Server { get; }
            public int Port { get; }
            public string Database { get; }
            public string Username { get; }
            public string Password { get; }
            public string ProcessId { get; }
            public string ApplicationName { get; }
            public string ClientHostName { get; }
            public string ClientHostProc { get; }
            public string Charset { get; }
            public bool Pooling { get; } = true;
            public short MaxPoolSize { get; set; } = 100;
            public short MinPoolSize { get; set; }
            public int LoginTimeout { get; set; }
            public short ConnectionIdleTimeout { get; }
            public short ConnectionLifetime { get; }
            public bool PingServer { get; }
            public ushort PacketSize { get; }
            public int TextSize { get; }
        }
    }
}
