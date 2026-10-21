using System;
using System.Data;
using AdoNetCore.AseClient.Tests.ConnectionProvider;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration
{
    public partial class AseDataReaderTests<T> where T : IConnectionProvider
    {
        // DbDataReader.GetFieldValue: InvalidOperationException when "There is no data ready to be read (for example, the first
        // Read() hasn't been called, or it returned false)"; IDataRecord.GetValue: IndexOutOfRangeException when "The index
        // passed was outside the range of 0 through FieldCount".
        [Test]
        public void GetValue_WithoutACurrentRow_ThrowsInvalidOperation()
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "select data_field from insert_records_tests where data_field = 1";
                    using (var reader = command.ExecuteReader())
                    {
                        Assert.Throws<InvalidOperationException>(() => reader.GetValue(0));
                        Assert.Throws<InvalidOperationException>(() => reader.GetFieldValue<int>(0));

                        Assert.IsTrue(reader.Read());
                        Assert.AreEqual(1, reader.GetFieldValue<int>(0));
                        Assert.Throws<IndexOutOfRangeException>(() => reader.GetValue(1));

                        Assert.IsFalse(reader.Read());
                        Assert.Throws<InvalidOperationException>(() => reader.GetFieldValue<int>(0));
                    }
                }
            }
        }
    }
}
