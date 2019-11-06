using System;
using System.Collections.Generic;
using System.Data.Common;
using AdoNetCore.AseClient.Internal;
using AdoNetCore.AseClient.Tests.ConnectionProvider;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration
{
    [Category("basic")]
#if NET_FRAMEWORK
    [TestFixture(typeof(SapConnectionProvider)/*, Explicit = true, Reason = "SAP AseClient tests are run for compatibility purposes."*/)]
#endif
    [TestFixture(typeof(CoreFxConnectionProvider))]
    public class RecordsAffectedTests<T> where T : IConnectionProvider
    {
        private DbConnection GetConnection()
        {
            return Activator.CreateInstance<T>().GetConnection(ConnectionStrings.Pooled);
        }

        [TestCaseSource(nameof(VariousStatements_HaveExpectedRecordsAffectedValue_Cases))]
        public void VariousStatements_HaveExpectedRecordsAffectedValue(string _, string commandText, int expected)
        {
            Logger.Enable();
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = commandText;
                    var reader = command.ExecuteReader();
                    Assert.AreEqual(expected, reader.RecordsAffected);
                    while (reader.Read()) { }
                    Assert.AreEqual(expected, reader.RecordsAffected);
                }
            }
        }

        public static IEnumerable<TestCaseData> VariousStatements_HaveExpectedRecordsAffectedValue_Cases()
        {
            yield return new TestCaseData("select", "select 1", -1);
            yield return new TestCaseData("insert x 2", @"CREATE TABLE #tmpinsert(x int)
INSERT INTO #tmpinsert (x) VALUES (1)
INSERT INTO #tmpinsert (x) VALUES (1)
DROP TABLE #tmpinsert", 2);
            yield return new TestCaseData("insert, delete", @"CREATE TABLE #tmpinsert(x int)
INSERT INTO #tmpinsert (x) VALUES (1)
DELETE FROM #tmpinsert WHERE x = 1
DROP TABLE #tmpinsert", 2);
            yield return new TestCaseData("insert, update", @"CREATE TABLE #tmpinsert(x int)
INSERT INTO #tmpinsert (x) VALUES (1)
UPDATE #tmpinsert SET x = 2
DROP TABLE #tmpinsert", 2);
            yield return new TestCaseData("insert, update, delete", @"CREATE TABLE #tmpinsert(x int)
INSERT INTO #tmpinsert (x) VALUES (1)
UPDATE #tmpinsert SET x = 2
DELETE FROM #tmpinsert WHERE x = 2
DROP TABLE #tmpinsert", 3);
            yield return new TestCaseData("insert, update, delete, select", @"CREATE TABLE #tmpinsert(x int)
INSERT INTO #tmpinsert (x) VALUES (1)
UPDATE #tmpinsert SET x = 2
DELETE FROM #tmpinsert WHERE x = 2
SELECT 1
DROP TABLE #tmpinsert", 3);
        }
    }
}
