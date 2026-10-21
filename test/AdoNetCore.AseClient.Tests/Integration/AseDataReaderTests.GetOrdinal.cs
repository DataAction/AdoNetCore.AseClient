using System;
using System.Data;
using AdoNetCore.AseClient.Tests.ConnectionProvider;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration
{
    public partial class AseDataReaderTests<T> where T : IConnectionProvider
    {
        // IDataRecord.GetOrdinal: "If the index of the named field is not found, an IndexOutOfRangeException is thrown."
        [Test]
        public void GetOrdinal_WithAnUnknownName_ThrowsIndexOutOfRange()
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "select data_field from insert_records_tests";
                    using (var reader = command.ExecuteReader())
                    {
                        Assert.AreEqual(0, reader.GetOrdinal("DATA_FIELD"));
                        Assert.Throws<IndexOutOfRangeException>(() => reader.GetOrdinal("missing"));
                    }
                }
            }
        }
    }
}
