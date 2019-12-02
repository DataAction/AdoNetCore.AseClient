using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using AdoNetCore.AseClient.Tests.ConnectionProvider;
using NUnit.Framework;
using Dapper;

namespace AdoNetCore.AseClient.Tests.Integration
{
    [TestFixture]
    public class AseDataReaderQuerySequenceTests
    {
        private const string CleanUpSql = @"IF EXISTS(SELECT 1 FROM sysobjects WHERE name = 'records_tests')
BEGIN
    drop table [dbo].[records_tests]
END";

        private const string select = "select top 1 * from records_tests";
        private const string insert = "insert into records_tests (data_field) values (1)";
        private const string delete = "delete records_tests where data_field > 0";
        private const string update = "update records_tests set data_field = 1";
        //[TestInitialize]
        public void Initialise()
        {
            using (var connection = new AseConnection(ConnectionStrings.Pooled))
            {
                connection.Open();

                RunSetupCommand(connection, CleanUpSql);
                RunSetupCommand(connection, "create table [dbo].[records_tests] (data_field int null)");
                RunSetupCommand(connection, "insert into records_tests (data_field) values (1)");
                RunSetupCommand(connection, "insert into records_tests (data_field) values (2)");
                RunSetupCommand(connection, "insert into records_tests (data_field) values (3)");
                RunSetupCommand(connection, $"create or replace procedure dbo.Test_Select as {select}");
                RunSetupCommand(connection, $"create or replace procedure dbo.Test_Insert as {insert}");
                RunSetupCommand(connection, $"create or replace procedure dbo.Test_Delete as {delete}");
                RunSetupCommand(connection, $"create or replace procedure dbo.Test_Update as {update}");
                RunSetupCommand(connection, $"create or replace procedure dbo.Test_Select_Insert as {select} {insert}");
                RunSetupCommand(connection, $"create or replace procedure dbo.Test_Insert_Select as {insert} {select}");
                RunSetupCommand(connection, $"create or replace procedure dbo.Test_Insert_Delete as {insert} {delete}");
                //RunSetupCommand(connection, "create or replace procedure dbo.test as select top 1 * from ov_task select top 2 * from ov_task");

                connection.Close();
            }
        }

        private void RunSetupCommand(AseConnection connection, string query)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = query;
                command.ExecuteReader();
            }
        }

        //[TestCleanup]
        public void CleanUp()
        {
            using (var connection = new AseConnection(ConnectionStrings.Pooled))
            {
                connection.Open();

                RunSetupCommand(connection, "drop table [dbo].[records_tests]");
                RunSetupCommand(connection, "drop procedure dbo.Test_Select");
                RunSetupCommand(connection, "drop procedure dbo.Test_Insert");
                RunSetupCommand(connection, "drop procedure dbo.Test_Delete");
                RunSetupCommand(connection, "drop procedure dbo.Test_Update");
                RunSetupCommand(connection, "drop procedure dbo.Test_Select_Insert");
                RunSetupCommand(connection, "drop procedure dbo.Test_Insert_Select");
                RunSetupCommand(connection, "drop procedure dbo.Test_Insert_Delete");

                connection.Close();
            }
        }

        public class result
        {
            public int rows;
            public int recordsAffected;
        }

        public List<result> RunQuery(string query)
        {
            Initialise();
            List<result> results = new List<result>();
            using (var connection = new AseConnection(ConnectionStrings.Pooled))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = query;

                    using (var reader = command.ExecuteReader())
                    {
                        do
                        {
                            //if ((reader.HasRows || reader.RecordsAffected >= 0))
                            //{
                            var result = new result
                            {
                                recordsAffected = reader.RecordsAffected
                            };

                            while (reader.Read())
                            {
                                if (reader.HasRows)
                                    result.rows++;
                            }

                            results.Add(result);
                            //}
                        } while (reader.NextResult());
                        if (reader.RecordsAffected != results[results.Count - 1].recordsAffected)
                        {
                            results.Add(
                                new result
                                {
                                    recordsAffected = reader.RecordsAffected,
                                    rows = -1
                                });
                        }
                    }
                }
            }
            CleanUp();
            return results;

        }

        [TestCase]
        public void ExecuteReader_Procedure_Select_Insert()
        {
            var result = RunQuery("dbo.Test_Select_Insert");

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(-1, result[0].recordsAffected);
            Assert.AreEqual(1, result[0].rows);
        }

        [TestCase]
        public void ExecuteReader_Procedure_Insert_Select()
        {
            var result = RunQuery("dbo.Test_Insert_Select");

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(-1, result[0].recordsAffected);
            Assert.AreEqual(1, result[0].rows);
        }


        [TestCase]
        public void ExecuteReader_Procedure_Insert_Delete()
        {
            var result = RunQuery("dbo.Test_Insert_Delete");

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(-1, result[0].recordsAffected);
            Assert.AreEqual(0, result[0].rows);
        }

        [TestCase]
        public void ExecuteReader_Procedure_Insert()
        {
            var result = RunQuery("dbo.Test_Insert");

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(-1, result[0].recordsAffected);
            Assert.AreEqual(0, result[0].rows);
        }

        [TestCase]
        public void ExecuteReader_Procedure_Delete()
        {
            var result = RunQuery("dbo.Test_Delete");

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(-1, result[0].recordsAffected);
            Assert.AreEqual(0, result[0].rows);
        }

        [TestCase]
        public void ExecuteReader_Procedure_Update()
        {
            var result = RunQuery("dbo.Test_Update");

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(-1, result[0].recordsAffected);
            Assert.AreEqual(0, result[0].rows);
        }

        [TestCase]
        public void ExecuteReader_Query_Select_Insert()
        {
            var result = RunQuery($"{select} {insert}");

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(-1, result[0].recordsAffected);
            Assert.AreEqual(1, result[0].rows);
            Assert.AreEqual(1, result[1].recordsAffected);
            Assert.AreEqual(-1, result[1].rows);
        }

        [TestCase]
        public void ExecuteReader_Query_Insert_Select()
        {
            var result = RunQuery($"{insert} {select}");

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(1, result[0].recordsAffected);
            Assert.AreEqual(1, result[0].rows);
        }

        [TestCase]
        public void ExecuteReader_Query_Insert_Delete()
        {
            var result = RunQuery($"{insert} {delete}");

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(5, result[0].recordsAffected);
            Assert.AreEqual(0, result[0].rows);
        }

        [TestCase]
        public void ExecuteReader_Query_Insert()
        {
            var result = RunQuery(insert);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(1, result[0].recordsAffected);
            Assert.AreEqual(0, result[0].rows);
        }

        [TestCase]
        public void ExecuteReader_Query_Delete()
        {
            var result = RunQuery(delete);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(3, result[0].recordsAffected);
            Assert.AreEqual(0, result[0].rows);
        }

        [TestCase]
        public void ExecuteReader_Query_Update()
        {
            var result = RunQuery(update);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(3, result[0].recordsAffected);
            Assert.AreEqual(0, result[0].rows);
        }

        [TestCase]
        public void ExecuteReader_Query_Select_Then_Inserts()
        {
            var result = RunQuery($"{select} {insert} {insert} {insert} {insert}");

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(-1, result[0].recordsAffected);
            Assert.AreEqual(1, result[0].rows);
            Assert.AreEqual(4, result[1].recordsAffected);
            Assert.AreEqual(-1, result[1].rows);
        }

        [TestCase]
        public void ExecuteReader_Query_Inserts_Then_Select()
        {
            var result = RunQuery($"{insert} {insert} {select} {insert} {insert}");

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(2, result[0].recordsAffected);
            Assert.AreEqual(1, result[0].rows);
            Assert.AreEqual(4, result[1].recordsAffected);
            Assert.AreEqual(-1, result[1].rows);
        }

        [TestCase]
        public void ExecuteReader_Query_Inserts_Then_Selects()
        {
            var result = RunQuery($"{insert} {insert} {select} {insert} {insert} {select} {insert} {insert} {select} {insert} {insert}");
            //var result = RunQuery($"{select} {select} {select}");

            Assert.AreEqual(4, result.Count);
            Assert.AreEqual(2, result[0].recordsAffected);
            Assert.AreEqual(1, result[0].rows);
            Assert.AreEqual(4, result[1].recordsAffected);
            Assert.AreEqual(1, result[1].rows);
            Assert.AreEqual(6, result[2].recordsAffected);
            Assert.AreEqual(1, result[2].rows);
            Assert.AreEqual(8, result[3].recordsAffected);
            Assert.AreEqual(-1, result[3].rows);
        }
    }
}
