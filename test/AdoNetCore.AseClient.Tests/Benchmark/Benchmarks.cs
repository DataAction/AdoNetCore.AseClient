using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AdoNetCore.AseClient.Tests.ConnectionProvider;

namespace AdoNetCore.AseClient.Tests.Benchmark
{
    public partial class Benchmarks<T> where T : IConnectionProvider
    {
        // This connection string is used for setting up the database. It requires DDL permissions. Adjust accordingly.
        private string _setupConnectionString;
        private T _connectionProvider;

        public string UnpooledConnectionString { get; } =ConnectionStrings.NonPooled;

        public string PooledConnectionString => ConnectionStrings.Pooled10;


        private void Initialise(T connectionProvider)
        {
            _connectionProvider = connectionProvider;
            _setupConnectionString = _setupConnectionString ?? UnpooledConnectionString;

            using (var connection = _connectionProvider.GetConnection(_setupConnectionString))
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

        public DataItem SingleQueryForSingleRecord(string connectionString)
        {
            using (var connection = _connectionProvider.GetConnection(connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "SELECT [Id], [Name], [Value] FROM [Benchmark_Simple_Table] WHERE [Id] = 1";

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

        public IEnumerable<DataItem> SingleQueryForMultipleRecords(string connectionString)
        {
            var results = new List<DataItem>();

            using (var connection = _connectionProvider.GetConnection(connectionString))
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

                            results.Add(new DataItem { Id = id, Name = name, Value = value });
                        }
                    }
                }
            }

            return results;
        }

        public IEnumerable<DataItem> MultipleQueriesForMultipleRecords(string connectionString)
        {
            var results = new List<DataItem>();

            using (var connection = _connectionProvider.GetConnection(connectionString))
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

                                results.Add(new DataItem { Id = id, Name = name, Value = value });
                            }
                        }
                    }
                }
            }

            return results;
        }

        public IEnumerable<DataItem> UpdateMultipleRecords(string connectionString)
        {
            var results = new List<DataItem>();

            using (var connection = _connectionProvider.GetConnection(connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        @"SELECT 
    [Id], 
    [Name], 
    [Value] 
FROM 
    [Benchmark_Simple_Table] 
WHERE 
    [Name] LIKE 'string value 1%'
    OR [Name] LIKE 'string value 3%'
    OR [Name] LIKE 'string value 5%'
    OR [Name] LIKE 'string value 7%'
    OR [Name] LIKE 'string value 9%'";

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

                // Change the data.
                foreach (var item in results)
                {
                    item.Value = 99;
                }

                // Update the database.
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "UPDATE [Benchmark_Simple_Table] SET [Value] = @value WHERE [Id] = @id";
                    command.CommandTimeout = 5; // 5 seconds.

                    var idParameter = command.CreateParameter();
                    idParameter.ParameterName = "@id";
                    idParameter.DbType = DbType.Int32;

                    var valueParameter = command.CreateParameter();
                    valueParameter.ParameterName = "@value";
                    valueParameter.DbType = DbType.Int32;

                    command.Parameters.Add(idParameter);
                    command.Parameters.Add(valueParameter);

                    command.Prepare();

                    foreach (var item in results)
                    {
                        idParameter.Value = item.Id;
                        valueParameter.Value = item.Value;

                        try
                        {
                            command.ExecuteNonQuery();
                        }
                        catch (Exception e)
                        {
                            Console.Error.WriteLine(e.Message);
                        }
                    }
                }
            }

            return results;
        }
    }

    /// <summary>
    /// Type for basic benchmark testing.
    /// </summary>
    public sealed class DataItem
    {
        /// <summary>
        /// The ID of the records from the database.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The Name of the records from the database.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string Name { get; set; }

        /// <summary>
        /// The Value of the records from the database.
        /// </summary>
        public int Value { get; set; }
    }
}
