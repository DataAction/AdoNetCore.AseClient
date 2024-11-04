#if ENABLE_SYSTEM_DATA_COMMON_EXTENSIONS
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using AdoNetCore.AseClient.Internal;
using AdoNetCore.AseClient.Tests.ConnectionProvider;
using Dapper;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration
{
    [TestFixture(typeof(CoreFxConnectionProvider), Explicit = true)]
    public class SchemaTableBuilderTests<T> where T : IConnectionProvider
    {

        private readonly string _creatAcctTypeTable = @"
create table [dbo].[acct_type_table]
(
    acct_type     char(9)           not null,
    status        varchar(19)       not null,
    dep_loan      char(2)        not null
)";

        private readonly string _dropAcctTypeTable = @"drop table [dbo].[acct_type_table]";


        private readonly string _creatAcctTable = @"
create table [dbo].[dp_acct_table]
(
    acct_no               char(12)      not null,
    acct_type             char(3)       not null,
    status                char(19)      not null,
    create_dt             smalldatetime not null
)";

        private readonly string _dropAcctTable = @"drop table [dbo].[dp_acct_table]";


        private readonly string _createGetAcctsProc = @"
CREATE PROCEDURE [dbo].[sp_get_accounts]
AS
begin
     
  select  
    acct_type, 
    status,
    dep_loan as base_type
  from
    [dbo].[acct_type_table]

  select
    acct_no,
    acct_type,
    status,
    create_dt
  from
    [dbo].[dp_acct_table]
end
";

        private readonly string _dropGetAcctsProc = @"drop procedure [dbo].[sp_get_accounts]";


        private readonly string[] _insertAcctTypeSqls =
        {
            "INSERT INTO dp_acct_table (acct_no, acct_type, status, create_dt) VALUES ('02244302', 'SAV', 'Closed', '1998-04-01 00:00:00')",
            "INSERT INTO dp_acct_table (acct_no, acct_type, status, create_dt) VALUES ('03157852', 'SAV', 'Dormant', '1998-04-01 00:00:00')",
            "INSERT INTO dp_acct_table (acct_no, acct_type, status, create_dt) VALUES ('03105053', 'SAV', 'Closed', '1993-09-23 00:00:00')"

        };

        private readonly string[] _insertAcctSqls =
        {
            "INSERT INTO dp_acct_table (acct_no, acct_type, status, create_dt) VALUES ('02244302', 'SAV', 'Closed', '1998-04-01 00:00:00')",
            "INSERT INTO dp_acct_table (acct_no, acct_type, status, create_dt) VALUES ('03157852', 'SAV', 'Dormant', '1998-04-01 00:00:00')",
            "INSERT INTO dp_acct_table(acct_no, acct_type, status, create_dt) VALUES('03105053', 'SAV', 'Closed', '1993-09-23 00:00:00')"
        };


        [SetUp]
        public void Setup()
        {
            using (var connection = new AseConnection(ConnectionStrings.Pooled))
            {
                connection.Execute(_creatAcctTypeTable);
                connection.Execute(_creatAcctTable);
                connection.Execute(_createGetAcctsProc);

                foreach (var sql in _insertAcctTypeSqls)
                {
                    connection.Execute(sql);
                }

                foreach (var sql in _insertAcctSqls)
                {
                    connection.Execute(sql);
                }
            }
        }

        [TearDown]
        public void TearDown()
        {
            using (var connection = new AseConnection(ConnectionStrings.Pooled))
            {
                connection.Execute(_dropGetAcctsProc);
                connection.Execute(_dropAcctTypeTable);
                connection.Execute(_dropAcctTable);
            }

            SchemaTableBuilder.SetCacheExpiration(30);
        }


        [Test]
        public void Test_Query_GetSchema_From_Database_Succeed()
        {
            SchemaTableBuilder.SetCacheExpiration(10);
            using (var connection = new AseConnection(ConnectionStrings.Pooled))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "select * from acct_type_table";
                    using (var reader = command.ExecuteReader())
                    {
                        DataTable res = reader.GetSchemaTable();
                        var cache = SchemaTableBuilder.GetSchemaCache();
                        cache.TryGetValue($"{ConnectionStrings.Database}:dbo:acct_type_table",
                            out SchemaTableBuilder.CachedSchemaInfo cachedEntry);
                        Assert.True(!cachedEntry.IsExpired);
                    }
                }
            }
        }

        [Test]
        public void Test_Query_GetSchema_From_Cache_Succeed()
        {
            SchemaTableBuilder.SetCacheExpiration(1);
            using (var connection = new AseConnection(ConnectionStrings.Pooled))
            {
                connection.Open();
                DateTime cacheTimestamp1, cacheTimestamp2;

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "select * from acct_type_table";
                    using (var reader = command.ExecuteReader())
                    {
                        DataTable res1 = reader.GetSchemaTable();
                        var key1 = $"{ConnectionStrings.Database}:dbo:acct_type_table";
                        cacheTimestamp1 = SchemaTableBuilder.GetSchemaCache(key1).CacheTime;

                        try
                        {
                            Thread.Sleep(20 * 1000);
                        }
                        catch (Exception e)
                        {
                            // ignored
                        }

                        DataTable res2 = reader.GetSchemaTable();
                        cacheTimestamp2 = SchemaTableBuilder.GetSchemaCache(key1).CacheTime;

                        Assert.True(res1.Rows.Count > 0 && res2.Rows.Count > 0);
                        Assert.True(cacheTimestamp1 == cacheTimestamp2);
                    }
                }
            }
        }

        [Test]
        public void Test_Query_GetSchema_From_DB_As_Cache_expired()
        {
            SchemaTableBuilder.SetCacheExpiration(1);
            using (var connection = new AseConnection(ConnectionStrings.Pooled))
            {
                connection.Open();
                SchemaTableBuilder.CachedSchemaInfo cache1, cache2;
                var key1 = $"{ConnectionStrings.Database}:dbo:acct_type_table";

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "select * from acct_type_table";
                    using (var reader = command.ExecuteReader())
                    {
                        DataTable res = reader.GetSchemaTable();
                        cache1 = SchemaTableBuilder.GetSchemaCache(key1);
                        Assert.True(res.Rows.Count > 0 && !cache1.IsExpired);
                    }

                    try
                    {
                        Thread.Sleep(65 * 1000);
                    }
                    catch (Exception e)
                    {
                        // ignored
                    }

                    cache1 = SchemaTableBuilder.GetSchemaCache(key1);
                    Assert.True(cache1.IsExpired);

                    using (var reader = command.ExecuteReader())
                    {
                        DataTable res = reader.GetSchemaTable();
                        cache2 = SchemaTableBuilder.GetSchemaCache(key1);

                        Assert.True(res.Rows.Count > 0 && !cache2.IsExpired);
                    }
                }
            }
        }


        [Test]
        public void Test_Exec_Proc_GetSchema_From_Cache_Succeed()
        {
            SchemaTableBuilder.SetCacheExpiration(1);
            try
            {
                using (var connection = new AseConnection(ConnectionStrings.Pooled))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "sp_get_accounts";
                        command.CommandType = CommandType.StoredProcedure;

                        DateTime? timestamp1, timestamp2;
                        string[] tableNames =
                        {
                            "AcctTypes", "Accts"
                        };

                        using (var reader = command.ExecuteReader())
                        {
                            var results = new DataSet();
                            results.Load(reader, LoadOption.OverwriteChanges, tableNames);
                            var cache = SchemaTableBuilder.GetSchemaCache();
                            Assert.True(cache != null && cache.Count == 2);
                            cache.TryGetValue($"{ConnectionStrings.Database}:dbo:acct_type_table",
                                out SchemaTableBuilder.CachedSchemaInfo aCachedEntry1);
                            timestamp1 = aCachedEntry1?.CacheTime;
                        }

                        //sleep but not expire the cache
                        try
                        {
                            Thread.Sleep(20 * 1000);
                        }
                        catch (Exception e)
                        {
                        }

                        using (var reader = command.ExecuteReader())
                        {
                            var results = new DataSet();
                            results.Load(reader, LoadOption.OverwriteChanges, tableNames);
                            var cache = SchemaTableBuilder.GetSchemaCache();
                            Assert.True(cache != null && cache.Count == 2);
                            cache.TryGetValue($"{ConnectionStrings.Database}:dbo:acct_type_table",
                                out SchemaTableBuilder.CachedSchemaInfo aCachedEntry2);
                            timestamp2 = aCachedEntry2?.CacheTime;
                        }

                        Assert.True(timestamp1 == timestamp2);
                    }
                }
            }
            catch (Exception e)
            {
                // ignored
            }
        }

        [Test]
        public void Test_Exec_Proc_GetSchema_From_DB_As_Cache_Expired_Succeed()
        {
            try
            {
                SchemaTableBuilder.SetCacheExpiration(1);

                using (var connection = new AseConnection(ConnectionStrings.Pooled))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "sp_get_accounts";
                        command.CommandType = CommandType.StoredProcedure;

                        DateTime? timestamp1, timestamp2;
                        string[] tableNames =
                        {
                            "AcctTypes", "Accts"
                        };

                        using (var reader = command.ExecuteReader())
                        {
                            var results = new DataSet();
                            results.Load(reader, LoadOption.OverwriteChanges, tableNames);
                            var cache = SchemaTableBuilder.GetSchemaCache();
                            Assert.True(cache != null && cache.Count == 2);
                            cache.TryGetValue($"{ConnectionStrings.Database}:dbo:acct_type_table",
                                out SchemaTableBuilder.CachedSchemaInfo aCachedEntry1);
                            timestamp1 = aCachedEntry1?.CacheTime;
                        }

                        //sleep but not expire the cache
                        try
                        {
                            Thread.Sleep(60 * 1000);
                        }
                        catch (Exception e)
                        {
                        }

                        using (var reader = command.ExecuteReader())
                        {
                            var results = new DataSet();
                            results.Load(reader, LoadOption.OverwriteChanges, tableNames);
                            var cache = SchemaTableBuilder.GetSchemaCache();
                            Assert.True(cache != null && cache.Count == 2);
                            cache.TryGetValue($"{ConnectionStrings.Database}:dbo:acct_type_table",
                                out SchemaTableBuilder.CachedSchemaInfo aCachedEntry1);
                            timestamp2 = aCachedEntry1?.CacheTime;
                        }

                        Assert.True(timestamp1 < timestamp2);
                    }
                }
            }
            catch (Exception e)
            {
                // ignored
            }
        }

        // /**
        //  * This is an integration test to verify expected dataset is returned.
        //  * To run it, connect to a real ATM database which has CAI procs deployed.
        //  */
        // [Test]
        // public void Procedure_CAI_4419_ShouldExecute()
        // {
        //     try
        //     {
        //         SchemaTableBuilder.SetCacheExpiration(1);
        //         using (var connection = new AseConnection(ConnectionStrings.Pooled))
        //         {
        //             connection.Open();
        //             using (var command = connection.CreateCommand())
        //             {
        //                 command.CommandText = "dsp_cai_get_member";
        //                 command.CommandType = CommandType.StoredProcedure;
        //
        //                 var p = command.CreateParameter();
        //                 p.ParameterName = "@pnMemberNumber";
        //                 p.Value = 16;
        //                 p.DbType = DbType.Int32;
        //                 command.Parameters.Add(p);
        //
        //                 var pOut = command.CreateParameter();
        //                 pOut.ParameterName = "@pbReferenceNumber";
        //                 pOut.Value = Guid.NewGuid().ToByteArray();
        //                 pOut.DbType = DbType.Binary;
        //                 command.Parameters.Add(pOut);
        //
        //                 var results = new System.Data.DataSet();
        //                 string[] tableNames =
        //                 {
        //                     "Member", "UserDefinedValues", "AdditionalFields", "AdditionalFieldValidValues", "Fatca",
        //                     "Addresses", "AddressFormats", "ReferenceNumber", "VerificationDocuments",
        //                     "RestrictedMembers"
        //                 };
        //                 using (var reader = command.ExecuteReader())
        //                 {
        //                     results.Load(reader, System.Data.LoadOption.OverwriteChanges, tableNames);
        //                 }
        //
        //                 Assert.True(results.Tables.Count == 10 && results.Tables["Member"].Columns.Count == 131);
        //             }
        //         }
        //     }
        //     catch (Exception e)
        //     {
        //         // ignored
        //     }

        //}
    }
}
#endif
