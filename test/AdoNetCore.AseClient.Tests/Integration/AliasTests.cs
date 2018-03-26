using System;
using System.Data;
using System.Linq;
using AdoNetCore.AseClient.Internal;
using Dapper;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration
{
    [TestFixture]
    [Category("quick")]
    public class AliasTests
    {
        private IDbConnection GetConnection()
        {
            Logger.Enable();
            return new AseConnection(ConnectionStrings.Pooled);
        }

        [SetUp]
        public void Setup()
        {
            using (var connection = GetConnection())
            {
                connection.Execute("create table [dbo].[test_aliases] (column_with_table_name int)");
                connection.Execute("insert into [dbo].[test_aliases] (column_with_table_name) values (1)");
            }
        }

        [TestCase]
        public void SelectColumnWithoutAlias_UsesColumnName_InResultSet()
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "select column_with_table_name from [dbo].[test_aliases]";

                    using (var reader = command.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        Assert.AreEqual(0, reader.GetOrdinal("column_with_table_name"));
                        Assert.Throws<ArgumentException>(() => reader.GetOrdinal("aliased_name"));
                    }
                }
            }
        }

        [TestCase]
        public void SelectColumnWithAlias_UsesAliasName_InResultSet()
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "select column_with_table_name as aliased_name from [dbo].[test_aliases]";

                    using (var reader = command.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        Assert.AreEqual(0, reader.GetOrdinal("aliased_name"));
                        Assert.Throws<ArgumentException>(() => reader.GetOrdinal("column_with_table_name"));
                    }
                }
            }
        }

        private class AliasTestsDapperResult
        {
            public int? column_with_table_name { get; set; }
            public int? aliased_name { get; set; }
        }

        [TestCase]
        public void SelectColumnWithoutAlias_UsesColumnName_InDapperResult()
        {
            using (var connection = GetConnection())
            {
                var result = connection.Query<AliasTestsDapperResult>("select column_with_table_name from [dbo].[test_aliases]", commandType: CommandType.Text).FirstOrDefault();
                Assert.IsNotNull(result?.column_with_table_name);
            }
        }

        [TestCase]
        public void SelectColumnWithAlias_UsesAliasName_InDapperResult()
        {
            using (var connection = GetConnection())
            {
                var result = connection.Query<AliasTestsDapperResult>("select column_with_table_name as aliased_name from [dbo].[test_aliases]", commandType: CommandType.Text).FirstOrDefault();
                Assert.IsNotNull(result?.aliased_name);
            }
        }

        [TearDown]
        public void Teardown()
        {
            using (var connection = GetConnection())
            {
                connection.Execute("drop table [dbo].[test_aliases]");
            }
        }
    }
}
