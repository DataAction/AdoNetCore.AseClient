using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Runtime.ExceptionServices;
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
            var pool = new ConnectionPool(parameters, new ImmediateConnectionFactory());

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
            var pool = new ConnectionPool(parameters, new ImmediateConnectionFactory());

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
        public void PoolSpam_Waves(short size, int waves)
        {
            var parameters = new TestConnectionParameters
            {
                MaxPoolSize = size,
                LoginTimeout = 1
            };

            var pool = new ConnectionPool(parameters, new ImmediateConnectionFactory());

            Console.WriteLine($"Wave 0 (primer) of {waves}");
            var connections = Enumerable.Repeat<Func<IInternalConnection>>(() => pool.Reserve(), size).Select(f => f()).ToArray();

            for (var wave = 0; wave < waves; wave++)
            {
                Console.WriteLine($"Wave {wave + 1} of {waves}");

                var closureConnections = connections; //access to modified closure warning
                var reserveTask = Task.Run(() => Enumerable.Repeat<Func<IInternalConnection>>(() => pool.Reserve(), size).AsParallel().Select(f => f()).ToArray());
                var releaseTask = Task.Run(() => Parallel.ForEach(closureConnections, pool.Release));

                try
                {
                    Task.WaitAll(releaseTask, reserveTask);
                }
                catch (AggregateException ae)
                {
                    ExceptionDispatchInfo.Capture(ae.InnerException).Throw();
                    throw;
                }
                
                connections = reserveTask.Result;
            }

            Console.WriteLine("Cleanup");
            Parallel.ForEach(connections, pool.Release);
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

            var pool = new ConnectionPool(parameters, new ImmediateConnectionFactory());

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
            public async Task<IInternalConnection> GetNewConnection(CancellationToken token)
            {
                return await Task.FromResult<IInternalConnection>(new DoNothingInternalConnection());
            }
        }

        private class DoNothingInternalConnection : IInternalConnection
        {
            public void Dispose() { }
            public DateTime Created { get; }
            public DateTime LastActive { get; }

            public bool Ping()
            {
                return true;
            }

            public void ChangeDatabase(string databaseName) { }
            public string Database { get; }
            public string DataSource { get; }
            public string ServerVersion { get; }
            public int ExecuteNonQuery(AseCommand command, AseTransaction transaction)
            {
                return 0;
            }

            public Task<int> ExecuteNonQueryTaskRunnable(AseCommand command, AseTransaction transaction)
            {
                return Task.FromResult(0);
            }

            public DbDataReader ExecuteReader(CommandBehavior behavior, AseCommand command, AseTransaction transaction)
            {
                return null;
            }

            public Task<DbDataReader> ExecuteReaderTaskRunnable(CommandBehavior behavior, AseCommand command, AseTransaction transaction)
            {
                return Task.FromResult<DbDataReader>(null);
            }

            public object ExecuteScalar(AseCommand command, AseTransaction transaction)
            {
                return null;
            }

            public void Cancel() { }
            public void SetTextSize(int textSize) { }
            public bool IsDoomed { get; set; }
            public bool IsDisposed { get; }
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
