using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AdoNetCore.AseClient.Internal;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration
{
    [TestFixture]
    public class LoginTests
    {
        private readonly IDictionary<string, string> _connectionStrings = ConnectionStringLoader.Load();

        [TestCase("default")]
        [TestCase("big-packetsize")]
        public void Login_Success(string csName)
        {
            Logger.Enable();
            using (var connection = new AseConnection(_connectionStrings[csName]))
            {
                connection.Open();
                Assert.IsTrue(connection.State == ConnectionState.Open, "Connection state should be Open after calling Open()");
            }
        }

        [Test]
        public void Login_Failure()
        {
            using (var connection = new AseConnection(_connectionStrings["badpass"]))
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

        [TestCase(10, 100)]
        [TestCase(100, 1000)]
        [TestCase(100, 10000)]
        public void Login_Blitz(short size, int threads)
        {
            //need to add some counters so we can see what's going on
            Logger.Disable();
            var parallelism = size * 2;

            var result = Parallel.ForEach(
                Enumerable.Repeat(1, threads),
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = parallelism
                },
                (_, __) =>
                {
                    using (var connection = new AseConnection(_connectionStrings["pooled"] + $";Max Pool Size={size};ConnectionLifetime=1"))
                    {
                        connection.Open();
                    }
                });

            Assert.IsTrue(result.IsCompleted);
        }
    }
}
