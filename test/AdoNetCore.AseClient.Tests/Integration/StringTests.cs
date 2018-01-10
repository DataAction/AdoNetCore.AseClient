using System.Data;
using AdoNetCore.AseClient.Internal;
using Dapper;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration
{
    [TestFixture]
    public class StringTests
    {
        private IDbConnection GetConnection()
        {
            Logger.Enable();
            return new AseConnection(ConnectionStrings.Pooled);
        }

        [Test]
        public void CharEncoding_ShouldWork()
        {
            using (var connection = GetConnection())
            {
                var result = connection.ExecuteScalar<string>("select convert(char(2), 'Àa')");
                Assert.AreEqual("Àa", result);
            }
        }

        [Test]
        public void VarcharEncoding_ShouldWork()
        {
            using (var connection = GetConnection())
            {
                var result = connection.ExecuteScalar<string>("select convert(varchar(2), 'Àa')");
                Assert.AreEqual("Àa", result);
            }
        }

        [Test]
        public void NcharEncoding_ShouldWork()
        {
            using (var connection = GetConnection())
            {
                var result = connection.ExecuteScalar<string>("select convert(nchar(2), 'Àa')");
                Assert.AreEqual("Àa", result);
            }
        }

        [Test]
        public void NvarcharEncoding_ShouldWork()
        {
            using (var connection = GetConnection())
            {
                var result = connection.ExecuteScalar<string>("select convert(nvarchar(2), 'Àa')");
                Assert.AreEqual("Àa", result);
            }
        }

        [Test]
        public void TextEncoding_ShouldWork()
        {
            using (var connection = GetConnection())
            {
                var result = connection.ExecuteScalar<string>("select convert(text, 'Àa')");
                Assert.AreEqual("Àa", result);
            }
        }

        [Test]
        public void UnicharEncoding_ShouldWork()
        {
            using (var connection = GetConnection())
            {
                var result = connection.ExecuteScalar<string>("select convert(unichar(2), 'Àa')");
                Assert.AreEqual("Àa", result);
            }
        }

        [Test]
        public void UnivarcharEncoding_ShouldWork()
        {
            using (var connection = GetConnection())
            {
                var result = connection.ExecuteScalar<string>("select convert(univarchar(2), 'Àa')");
                Assert.AreEqual("Àa", result);
            }
        }

        [Test]
        public void UnitextEncoding_ShouldWork()
        {
            using (var connection = GetConnection())
            {
                var result = connection.ExecuteScalar<string>("select convert(unitext, 'Àa')");
                Assert.AreEqual("Àa", result);
            }
        }
    }
}
