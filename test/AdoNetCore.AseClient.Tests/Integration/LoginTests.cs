using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using AdoNetCore.AseClient.Internal;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration
{
    [TestFixture]
    [Category("basic")]
    public class LoginTests
    {
        [TestCaseSource(nameof(Login_Success_Cases))]
        public void Login_Success(string cs)
        {
            Logger.Enable();
            using (var connection = new AseConnection(cs))
            {
                connection.Open();
                Assert.IsTrue(connection.State == ConnectionState.Open, "Connection state should be Open after calling Open()");
            }
        }

        public static IEnumerable<TestCaseData> Login_Success_Cases()
        {
            yield return new TestCaseData(ConnectionStrings.Default);
            yield return new TestCaseData(ConnectionStrings.BigPacketSize);
        }

        [Test]
        public void Login_Failure()
        {
            using (var connection = new AseConnection(ConnectionStrings.BadPass))
            {
                Assert.Throws<AseException>(() => connection.Open());
            }
        }

        [Test]
        public void CannotResolveServer_Failure()
        {
            using (var connection = new AseConnection("Data Source=myASEServer;Port=5000;Database=mydb;Uid=x;Pwd=y;"))
            {
                Assert.Throws<AseException>(() => connection.Open());
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
                        using (var connection = new AseConnection(cs))
                        {
                            connection.Open();
                        }
                    });
            }
            catch(AggregateException ae)
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
