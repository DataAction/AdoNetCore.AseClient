using System;
using AdoNetCore.AseClient.Tests.ConnectionProvider;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Benchmark
{
#if NET_FRAMEWORK
    [TestFixture(typeof(SapConnectionProvider), Explicit = true)]
#endif
    [TestFixture(typeof(CoreFxConnectionProvider), Explicit = true)]
    [Category("benchmark")]
    public partial class Benchmarks<T> where T : IConnectionProvider
    {
        [SetUp]
        public void SetUp()
        {
            Initialise(Activator.CreateInstance<T>());
        }

        [Test]
        public void Benchmark_SingleQueryForSingleRecordWithUnpooledConnection_ReturnsData()
        {
            // Arrange.

            // Act.
            var result = SingleQueryForSingleRecord(UnpooledConnectionString);

            // Assert.
            Assert.IsNotNull(result);
        }

        [Test]
        public void Benchmark_SingleQueryForSingleRecordWithPooledConnection_ReturnsData()
        {
            // Arrange.

            // Act.
            var result = SingleQueryForSingleRecord(PooledConnectionString);

            // Assert.
            Assert.IsNotNull(result);
        }

        [Test]
        public void Benchmark_SingleQueryForMultipleRecordsWithPooledConnection_ReturnsData()
        {
            // Arrange.

            // Act.
            var result = SingleQueryForMultipleRecords(PooledConnectionString);

            // Assert.
            Assert.IsNotNull(result);
            Assert.IsNotEmpty(result);
        }

        [Test]
        public void Benchmark_SingleQueryForMultipleRecordsWithUnpooledConnection_ReturnsData()
        {
            // Arrange.

            // Act.
            var result = SingleQueryForMultipleRecords(UnpooledConnectionString);

            // Assert.
            Assert.IsNotNull(result);
            Assert.IsNotEmpty(result);
        }

        [Test]
        public void Benchmark_MultipleQueriesForMultipleRecordsWithPooledConnection_ReturnsData()
        {
            // Arrange.

            // Act.
            var result = MultipleQueriesForMultipleRecords(PooledConnectionString);

            // Assert.
            Assert.IsNotNull(result);
            Assert.IsNotEmpty(result);
        }

        [Test]
        public void Benchmark_MultipleQueriesForMultipleRecordsWithUnpooledConnection_ReturnsData()
        {
            // Arrange.

            // Act.
            var result = MultipleQueriesForMultipleRecords(UnpooledConnectionString);

            // Assert.
            Assert.IsNotNull(result);
            Assert.IsNotEmpty(result);
        }

        [Test]
        public void Benchmark_UpdateMultipleRecordsWithPooledConnection_ReturnsData()
        {
            // Arrange.

            // Act.
            var result = UpdateMultipleRecords(PooledConnectionString);

            // Assert.
            Assert.IsNotNull(result);
            Assert.IsNotEmpty(result);
        }

        [Test]
        public void Benchmark_UpdateMultipleRecordsWithUnpooledConnection_ReturnsData()
        {
            // Arrange.

            // Act.
            var result = UpdateMultipleRecords(UnpooledConnectionString);

            // Assert.
            Assert.IsNotNull(result);
            Assert.IsNotEmpty(result);
        }
    }
}
