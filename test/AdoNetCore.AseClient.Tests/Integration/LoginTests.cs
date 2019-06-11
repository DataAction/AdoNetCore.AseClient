using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using AdoNetCore.AseClient.Internal;
using AdoNetCore.AseClient.Tests.ConnectionProvider;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration
{
    [Category("basic")]
#if NET_FRAMEWORK
    [TestFixture(typeof(SapConnectionProvider))]
#endif
    [TestFixture(typeof(CoreFxConnectionProvider))]
    public class LoginTests<T> where T : IConnectionProvider
    {
        private DbConnection GetConnection(string connectionString)
        {
            return Activator.CreateInstance<T>().GetConnection(connectionString);
        }

        [TestCaseSource(nameof(Login_Success_Cases))]
        public void Login_Success(string cs)
        {
            Logger.Enable();
            using (var connection = GetConnection(cs))
            {
                connection.Open();
                Assert.IsTrue(connection.State == ConnectionState.Open, "Connection state should be Open after calling Open()");
            }
        }

        public static IEnumerable<TestCaseData> Login_Success_Cases()
        {
            yield return new TestCaseData(ConnectionStrings.Pooled);
            yield return new TestCaseData(ConnectionStrings.BigPacketSize);
            yield return new TestCaseData(ConnectionStrings.EncryptPassword1);
            yield return new TestCaseData(ConnectionStrings.EncryptPassword2);
        }

        [Test]
        public void Login_Failure()
        {
            using (var connection = GetConnection(ConnectionStrings.BadPass))
            {
#if NET_FRAMEWORK
                if (typeof(T) == typeof(SapConnectionProvider))
                {
                    var ex = Assert.Throws<Sybase.Data.AseClient.AseException>(() => connection.Open());
                    Assert.AreEqual("Login failed.\n", ex.Message);
                    Assert.AreEqual(4002, ex.Errors[0].MessageNumber);
                }
#endif
                if (typeof(T) == typeof(CoreFxConnectionProvider))
                {
                    var ex = Assert.Throws<AseException>(() => connection.Open());
                    Assert.AreEqual("Login failed.\n", ex.Message);
                    Assert.AreEqual(4002, ex.Errors[0].MessageNumber);
                }
            }
        }

        [TestCaseSource(nameof(Login_Blitz_Cases))]
        public void Login_Blitz(int size, int threads, string cs)
        {
            //need to add some counters so we can see what's going on
            Logger.Disable();
            var parallelism = size * 2;

            ParallelLoopResult result;

            try
            {
                result = Parallel.ForEach(
                    Enumerable.Repeat(1, threads),
                    new ParallelOptions
                    {
                        MaxDegreeOfParallelism = parallelism
                    },
                    (_, __) =>
                    {
                        using (var connection = GetConnection(cs))
                        {
                            connection.Open();
                        }
                    });
            }
            catch (AggregateException ae)
            {
                ExceptionDispatchInfo.Capture(ae.InnerException).Throw();
                throw;
            }

            Assert.IsTrue(result.IsCompleted);
        }

        public static IEnumerable<TestCaseData> Login_Blitz_Cases()
        {
            yield return new TestCaseData(10, 100, ConnectionStrings.Pooled10);
            yield return new TestCaseData(100, 1000, ConnectionStrings.Pooled100);
            yield return new TestCaseData(100, 10000, ConnectionStrings.Pooled100);
        }
    }
}
