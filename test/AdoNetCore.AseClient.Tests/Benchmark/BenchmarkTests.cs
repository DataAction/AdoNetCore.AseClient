using System;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Benchmark
{
#if NET46
    [TestFixture(typeof(SapConnectionProvider))]
#endif
#if NETCORE_OLD || NETCOREAPP2_0 || NET46
    [TestFixture(typeof(CoreFxConnectionProvider))]
#endif
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