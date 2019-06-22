using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using AdoNetCore.AseClient.Tests.ConnectionProvider;
using Dapper;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration.Query
{
    [Category("basic")]
#if NET_FRAMEWORK
    [TestFixture(typeof(SapConnectionProvider), Explicit = true,  Reason = "SAP AseClient tests are run for compatibility purposes.")]
#endif
    [TestFixture(typeof(CoreFxConnectionProvider))]
    public class UnsupportedTests<T> where T : IConnectionProvider
    {
        private DbConnection GetConnection()
        {
            return Activator.CreateInstance<T>().GetConnection(ConnectionStrings.Pooled);
        }

        [TestCaseSource(nameof(SelectUnsupportedType_Parameter_Cases))]
        public void SelectUnsupportedType_Parameter_DoesNotThrow(DbType type, object value)
        {
            using (var connection = GetConnection())
            {
                var p = new DynamicParameters();
                p.Add("@expected", value, DbType.Object);
                connection.QuerySingle<byte[]>("select @expected", p);
            }
        }

        public static IEnumerable<TestCaseData> SelectUnsupportedType_Parameter_Cases()
        {
            yield return new TestCaseData(DbType.Object, null);
            yield return new TestCaseData(DbType.Object, new byte[0]);
            yield return new TestCaseData(DbType.Object, new byte[] {1});
            yield return new TestCaseData(DbType.Object, new byte[] {0xff});
            yield return new TestCaseData(DbType.Object, "asdf");
            //yield return new TestCaseData(DbType.Object, Guid.NewGuid()); //we support this, but reference driver does not
        }
    }
}
