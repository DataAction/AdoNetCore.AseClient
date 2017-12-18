using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Sybase.Data.AseClient;

namespace AdoNetCore.AseClient.Benchmark.SAP
{
    public class AseClientBenchmarks
    {
        private static readonly string AseServer = Environment.MachineName; // NOTE set this for your own benchmarking tests.
        private const int AsePort = 5000;          // NOTE set this for your own benchmarking tests.
        private const string AseDatabase = "pubs2";  // NOTE set this for your own benchmarking tests.
        private const string AseUsername = "username";  // NOTE set this for your own benchmarking tests.
        private const string AsePassword = "password";  // NOTE set this for your own benchmarking tests.

        // This connection string is used for setting up the database. It requires DDL permissions. Adjust accordingly.
        private readonly string _setupConnectionString = $"Data Source={AseServer};Port={AsePort};Database={AseDatabase};Uid={AseUsername};Pwd={AsePassword};";

        private readonly string _unpooledConnectionString = $"Data Source={AseServer};Port={AsePort};Database={AseDatabase};Uid={AseUsername};Pwd={AsePassword};";
        private readonly string _pooledConnectionString = $"Data Source={AseServer};Port={AsePort};Database={AseDatabase};Uid={AseUsername};Pwd={AsePassword};Pooling=true;Min Pool Size=5;Max Pool Size=10;";

        private static readonly Random Random = new Random();

        public AseClientBenchmarks()
        {
            using (var connection = new AseConnection(_setupConnectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.Text;

                    command.CommandText =
@"IF OBJECT_ID('dbo.Benchmark_Simple_Table') IS NOT NULL
BEGIN
    DROP TABLE [dbo].[Benchmark_Simple_Table]
END ";
                    command.ExecuteNonQuery();

                    command.CommandText =
@"CREATE TABLE [dbo].[Benchmark_Simple_Table]
(
    [Id] INT IDENTITY PRIMARY KEY,
    [Name] VARCHAR(16) NOT NULL,
    [Value] INT DEFAULT 1 NOT NULL
)";
                    command.ExecuteNonQuery();

                    for (int i = 1; i <= 100; i++)
                    {
                        command.CommandText =
                            $"INSERT INTO [dbo].[Benchmark_Simple_Table]([Name]) VALUES('string value {i}')";
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        /// <summary>
        /// In this test we fetch a single row from simple database table without connection pooling enabled.
        /// </summary>
        [BenchmarkDotNet.Attributes.Benchmark]
        public DataItem SingleQueryForSingleRecordWithUnpooledConnection()
        {
            return SingleQueryForSingleRecord(_unpooledConnectionString);
        }

        /// <summary>
        /// In this test we fetch a single row from simple database table with connection pooling enabled.
        /// </summary>
        [BenchmarkDotNet.Attributes.Benchmark]
        public DataItem SingleQueryForSingleRecordWithPooledConnection()
        {
            return SingleQueryForSingleRecord(_pooledConnectionString);
        }

        private static DataItem SingleQueryForSingleRecord(string connectionString)
        {
            using (var connection = new AseConnection(connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "SELECT [Id], [Name], [Value] FROM [Benchmark_Simple_Table] WHERE [Name] = 'string value 42'";

                    using (var reader = command.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        var idOrdinal = reader.GetOrdinal("Id");
                        var nameOrdinal = reader.GetOrdinal("Name");
                        var valueOrdinal = reader.GetOrdinal("Value");

                        while (reader.Read())
                        {
                            var id = reader.GetInt32(idOrdinal);
                            var name = reader.GetString(nameOrdinal);
                            var value = reader.GetInt32(valueOrdinal);

                            return new DataItem { Id = id, Name = name, Value = value };
                        }
                    }
                }
            }

            return null;
        }


        /// <summary>
        /// In this test we fetch multiple rows using a single query from simple database table without connection pooling enabled.
        /// </summary>
        [BenchmarkDotNet.Attributes.Benchmark]
        public IEnumerable<DataItem> SingleQueryForMultipleRecordsWithUnpooledConnection()
        {
            return SingleQueryForMultipleRecords(_unpooledConnectionString);
        }

        /// <summary>
        /// In this test we fetch multiple rows using a single query from simple database table with connection pooling enabled.
        /// </summary>
        [BenchmarkDotNet.Attributes.Benchmark]
        public IEnumerable<DataItem> SingleQueryForMultipleRecordsWithPooledConnection()
        {
            return SingleQueryForMultipleRecords(_pooledConnectionString);
        }

        private static IEnumerable<DataItem> SingleQueryForMultipleRecords(string connectionString)
        {
            using (var connection = new AseConnection(connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "SELECT [Id], [Name], [Value] FROM [Benchmark_Simple_Table] WHERE [Name] LIKE 'string value 1%'";

                    using (var reader = command.ExecuteReader())
                    {
                        var idOrdinal = reader.GetOrdinal("Id");
                        var nameOrdinal = reader.GetOrdinal("Name");
                        var valueOrdinal = reader.GetOrdinal("Value");

                        while (reader.Read())
                        {
                            var id = reader.GetInt32(idOrdinal);
                            var name = reader.GetString(nameOrdinal);
                            var value = reader.GetInt32(valueOrdinal);

                            yield return new DataItem { Id = id, Name = name, Value = value };
                        }
                    }
                }
            }
        }




        /// <summary>
        /// In this test we fetch multiple rows using multiple queries from simple database table without connection pooling enabled.
        /// </summary>
        [BenchmarkDotNet.Attributes.Benchmark]
        public IEnumerable<DataItem> MultipleQueriesForMultipleRecordsWithUnpooledConnection()
        {
            return MultipleQueriesForMultipleRecords(_unpooledConnectionString);
        }

        /// <summary>
        /// In this test we fetch multiple rows using multiple queries from simple database table with connection pooling enabled.
        /// </summary>
        [BenchmarkDotNet.Attributes.Benchmark]
        public IEnumerable<DataItem> MultipleQueriesForMultipleRecordsWithPooledConnection()
        {
            return MultipleQueriesForMultipleRecords(_pooledConnectionString);
        }

        private static IEnumerable<DataItem> MultipleQueriesForMultipleRecords(string connectionString)
        {
            using (var connection = new AseConnection(connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    foreach (var randomId in Enumerable.Range(1, 9))
                    {
                        command.CommandText =
                            $"SELECT [Id], [Name], [Value] FROM [Benchmark_Simple_Table] WHERE [Name] LIKE 'string value {randomId}%'";

                        using (var reader = command.ExecuteReader())
                        {
                            var idOrdinal = reader.GetOrdinal("Id");
                            var nameOrdinal = reader.GetOrdinal("Name");
                            var valueOrdinal = reader.GetOrdinal("Value");

                            while (reader.Read())
                            {
                                var id = reader.GetInt32(idOrdinal);
                                var name = reader.GetString(nameOrdinal);
                                var value = reader.GetInt32(valueOrdinal);

                                yield return new DataItem { Id = id, Name = name, Value = value };
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// In this test we fetch multiple rows using multiple queries from simple database table without connection pooling enabled. 
        /// Then we update the data, and update the database one record at a time. Then we return the results.
        /// </summary>
        [BenchmarkDotNet.Attributes.Benchmark]
        public IEnumerable<DataItem> UpdateMultipleRecordsWithUnpooledConnection()
        {
            return UpdateMultipleRecords(_unpooledConnectionString);
        }

        /// <summary>
        /// In this test we fetch multiple rows using multiple queries from simple database table with connection pooling enabled. 
        /// Then we update the data, and update the database one record at a time. Then we return the results.
        /// </summary>
        [BenchmarkDotNet.Attributes.Benchmark]
        public IEnumerable<DataItem> UpdateMultipleRecordsWithPooledConnection()
        {
            return UpdateMultipleRecords(_pooledConnectionString);
        }

        private static IEnumerable<DataItem> UpdateMultipleRecords(string connectionString)
        {
            var results = new List<DataItem>();

            using (var connection = new AseConnection(connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    foreach (var randomId in Enumerable.Range(1, 9).Where(i => i % 2 == 1))
                    {
                        command.CommandText =
                            $"SELECT [Id], [Name], [Value] FROM [Benchmark_Simple_Table] WHERE [Name] LIKE 'string value {randomId}%'";

                        using (var reader = command.ExecuteReader())
                        {
                            var idOrdinal = reader.GetOrdinal("Id");
                            var nameOrdinal = reader.GetOrdinal("Name");
                            var valueOrdinal = reader.GetOrdinal("Value");

                            while (reader.Read())
                            {
                                var id = reader.GetInt32(idOrdinal);
                                var name = reader.GetString(nameOrdinal);
                                var value = reader.GetInt32(valueOrdinal);

                                results.Add(new DataItem { Id = id, Name = name, Value = value });
                            }
                        }
                    }
                }

                // Change the data.
                foreach (var item in results)
                {
                    item.Value = Random.Next(1, 1000);
                }

                // Update the database.
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "UPDATE [Benchmark_Simple_Table] SET [Value] = @value WHERE [Id] = @id";

                    var idParameter = command.Parameters.Add("@id", AseDbType.Integer);
                    var valueParameter = command.Parameters.Add("@value", AseDbType.Integer);

                    command.Prepare();

                    foreach (var item in results)
                    {
                        idParameter.Value = item.Id;
                        valueParameter.Value = item.Value;

                        command.ExecuteNonQuery();
                    }
                }
            }

            return results;
        }
    }
}