using System;
using System.Linq;
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

            var c1 = pool.Reserve();
            Assert.Throws<AseException>(() => pool.Reserve());

            Assert.AreEqual(1, pool.PoolSize);
            pool.Release(c1);
        }

        /// <summary>
        /// In this scenario, the pool is fully consumed, and then an additional pool's worth of Reserve() calls are applied
        /// This is then repeated for so many runs
        /// </summary>
        [TestCase(1, 1)]
        [TestCase(10, 1)]
        [TestCase(100, 1)]
        [TestCase(1, 3)]
        [TestCase(10, 3)]
        [TestCase(100, 3)]
        [TestCase(1, 6)]
        [TestCase(10, 6)]
        [TestCase(100, 6)]
        [TestCase(1, 10)]
        [TestCase(10, 10)]
        [TestCase(100, 10)]
        public void PoolSpam_Waves(short size, int runs)
        {
            var parameters = new TestConnectionParameters
            {
                MaxPoolSize = size,
                LoginTimeout = 1
            };

            var pool = new ConnectionPool(parameters, new ImmediateConnectionFactory(parameters));

            var connections = Enumerable.Repeat<Func<IInternalConnection>>(() => pool.Reserve(), size).Select(f => f()).ToArray();

            for (var run = 0; run < runs; run++)
            {
                Console.WriteLine($"Run {run + 1} of {runs}");
                var reserveTasks = Enumerable.Repeat<Func<Task<IInternalConnection>>>(() => Task.Run(() => pool.Reserve()), size).Select(f => f()).ToArray();
                var releaseTasks = connections.Select(c => Task.Run(() => pool.Release(c))).ToArray();

                Task.WaitAll(releaseTasks);
                // ReSharper disable once CoVariantArrayConversion
                Task.WaitAll(reserveTasks);

                Assert.IsTrue(reserveTasks.All(t => t.IsCompleted));
                Assert.IsTrue(reserveTasks.All(t => t.Result != null));

                connections = reserveTasks.Select(t => t.Result).ToArray();
            }
        }

        /// <summary>
        /// In this scenario, we throw as many threads as we can at the pool
        /// </summary>
        [TestCase(1, 100)]
        [TestCase(10, 100)]
        [TestCase(100, 1000)]
        [TestCase(100, 10000)]
        [TestCase(100, 100000)]
        [TestCase(100, 1000000)]
        public void PoolSpam_Blitz(short size, int threads)
        {
            Logger.Disable();
            var parallelism = size * 2;
            var parameters = new TestConnectionParameters
            {
                MaxPoolSize = size,
                LoginTimeout = 1
            };

            var pool = new ConnectionPool(parameters, new ImmediateConnectionFactory(parameters));

            var result = Parallel.ForEach(
                Enumerable.Repeat(1, threads),
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = parallelism
                },
                (_, __) =>
                {
                    var c = pool.Reserve();
                    pool.Release(c);
                });

            Assert.IsTrue(result.IsCompleted);
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
