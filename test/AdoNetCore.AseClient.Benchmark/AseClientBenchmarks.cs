using System.Collections.Generic;
using AdoNetCore.AseClient.Tests.Benchmark;
using AdoNetCore.AseClient.Tests.ConnectionProvider;
using BenchmarkDotNet.Attributes;

namespace AdoNetCore.AseClient.Benchmark
{
    public class AseClientBenchmarks<T> where T : IConnectionProvider
    {
        private readonly Benchmarks<T> _benchmark;

        public AseClientBenchmarks(Benchmarks<T> benchmark)
        {
            _benchmark = benchmark;
            _benchmark.SetUp();
        }

        /// <summary>
        /// In this test we fetch a single row from simple database table without connection pooling enabled.
        /// </summary>
        [Benchmark(Description = "Open a connection (unpooled) and invoke AseCommand.ExecuteReader(...) once and read back one row of data.")]
        public DataItem SingleQueryForSingleRecordWithUnpooledConnection()
        {
            return _benchmark.SingleQueryForSingleRecord(_benchmark.UnpooledConnectionString);
        }

        /// <summary>
        /// In this test we fetch a single row from simple database table with connection pooling enabled.
        /// </summary>

        [Benchmark(Description = "Open a connection (pooled) and invoke AseCommand.ExecuteReader(...) once and read back one row of data.")]
        public DataItem SingleQueryForSingleRecordWithPooledConnection()
        {
            return _benchmark.SingleQueryForSingleRecord(_benchmark.PooledConnectionString);
        }

        /// <summary>
        /// In this test we fetch multiple rows using a single query from simple database table without connection pooling enabled.
        /// </summary>

        [Benchmark(Description = "Open a connection (unpooled) and invoke AseCommand.ExecuteReader(...) once and read back 12 rows of data.")]
        public IEnumerable<DataItem> SingleQueryForMultipleRecordsWithUnpooledConnection()
        {
            return _benchmark.SingleQueryForMultipleRecords(_benchmark.UnpooledConnectionString);
        }

        /// <summary>
        /// In this test we fetch multiple rows using a single query from simple database table with connection pooling enabled.
        /// </summary>
        [Benchmark(Description = "Open a connection (pooled) and invoke AseCommand.ExecuteReader(...) once and read back 12 rows of data.")]
        public IEnumerable<DataItem> SingleQueryForMultipleRecordsWithPooledConnection()
        {
            return _benchmark.SingleQueryForMultipleRecords(_benchmark.PooledConnectionString);
        }

        /// <summary>
        /// In this test we fetch multiple rows using multiple queries from simple database table without connection pooling enabled.
        /// </summary>
        [Benchmark(Description = "Open a connection (unpooled) and invoke AseCommand.ExecuteReader(...) 9 times, and read back 11-12 rows of data each time.")]
        public IEnumerable<DataItem> MultipleQueriesForMultipleRecordsWithUnpooledConnection()
        {
            return _benchmark.MultipleQueriesForMultipleRecords(_benchmark.UnpooledConnectionString);
        }

        /// <summary>
        /// In this test we fetch multiple rows using multiple queries from simple database table with connection pooling enabled.
        /// </summary>
        [Benchmark(Description = "Open a connection (pooled) and invoke AseCommand.ExecuteReader(...) 9 times, and read back 11-12 rows of data each time.")]
        public IEnumerable<DataItem> MultipleQueriesForMultipleRecordsWithPooledConnection()
        {
            return _benchmark.MultipleQueriesForMultipleRecords(_benchmark.PooledConnectionString);
        }

        /// <summary>
        /// In this test we fetch multiple rows using multiple queries from simple database table without connection pooling enabled. 
        /// Then we update the data, and update the database one record at a time. Then we return the results.
        /// </summary>
        [Benchmark(Description = "Open a connection (unpooled) and invoke AseCommand.ExecuteReader(...) once, reading back 56 rows of data. Prepare a new AseCommand and invoke AseCommand.ExecuteNonQuery(...) for each of the 56 rows to update the database.")]
        public IEnumerable<DataItem> UpdateMultipleRecordsWithUnpooledConnection()
        {
            return _benchmark.UpdateMultipleRecords(_benchmark.UnpooledConnectionString);
        }

        /// <summary>
        /// In this test we fetch multiple rows using multiple queries from simple database table with connection pooling enabled. 
        /// Then we update the data, and update the database one record at a time. Then we return the results.
        /// </summary>
        [Benchmark(Description = "Open a connection (pooled) and invoke AseCommand.ExecuteReader(...) once, reading back 56 rows of data. Prepare a new AseCommand and invoke AseCommand.ExecuteNonQuery(...) for each of the 56 rows to update the database.")]
        public IEnumerable<DataItem> UpdateMultipleRecordsWithPooledConnection()
        {
            return _benchmark.UpdateMultipleRecords(_benchmark.PooledConnectionString);
        }
    }
}