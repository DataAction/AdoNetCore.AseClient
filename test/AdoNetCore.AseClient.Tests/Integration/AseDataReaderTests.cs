using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Newtonsoft.Json;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration
{
    [TestFixture]
    public class AseDataReaderTests
    {
        // TODO - more tests.

        private const string AllTypesQuery =
@"SELECT 
    CAST(123 AS INT) AS [IntColumn],
    CAST(NULL AS INT) AS [NullIntColumn],
    CAST(123 AS SMALLINT) AS [ShortColumn],
    CAST(NULL AS SMALLINT) AS [NullShortColumn],
    CAST(123 AS BIGINT) AS [LongColumn],
    CAST(NULL AS BIGINT) AS [NullLongColumn],
    CAST('Hello world' AS VARCHAR) AS [StringColumn],
    CAST(NULL AS VARCHAR) AS [NullStringColumn],
    CAST(1 AS BIT) AS [BooleanColumn],
    CAST(123 AS TINYINT) AS [ByteColumn],
    CAST(NULL AS TINYINT) AS [NullByteColumn],
    CAST(123.45 AS REAL) AS [FloatColumn],
    CAST(NULL AS REAL) AS [NullFloatColumn],
    CAST(123.45 AS DOUBLE PRECISION) AS [DoubleColumn],
    CAST(NULL AS DOUBLE PRECISION) AS [NullDoubleColumn],
    CAST(123.45 AS NUMERIC(18,6)) AS [DecimalColumn],
    CAST(NULL AS NUMERIC(18,6)) AS [NullDecimalColumn]";

        private readonly Dictionary<string, string> _connectionStrings = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("ConnectionStrings.json"));

        [Test]
        public void GetInt32_WithValue_CastSuccessfully()
        {
            GetHelper_WithValue_TCastSuccessfully("IntColumn", (reader, ordinal) => reader.GetInt32(ordinal), 123);
        }

        [Test]
        public void GetInt32_WithNullValue_ThrowsInvalidCastException()
        {
            GetHelper_WithNullValue_ThrowsInvalidCastException("NullIntColumn", (reader, ordinal) => reader.GetInt32(ordinal));
        }

        [Test]
        public void GetInt16_WithValue_CastSuccessfully()
        {
            GetHelper_WithValue_TCastSuccessfully("ShortColumn", (reader, ordinal) => reader.GetInt16(ordinal), 123);
        }

        [Test]
        public void GetInt16_WithNullValue_ThrowsInvalidCastException()
        {
            GetHelper_WithNullValue_ThrowsInvalidCastException("NullShortColumn", (reader, ordinal) => reader.GetInt16(ordinal));
        }

        [Test]
        public void GetInt64_WithValue_CastSuccessfully()
        {
            GetHelper_WithValue_TCastSuccessfully("LongColumn", (reader, ordinal) => reader.GetInt64(ordinal), 123);
        }

        [Test]
        public void GetInt64_WithNullValue_ThrowsInvalidCastException()
        {
            GetHelper_WithNullValue_ThrowsInvalidCastException("NullLongColumn", (reader, ordinal) => reader.GetInt64(ordinal));
        }

        [Test]
        public void GetString_WithValue_CastSuccessfully()
        {
            GetHelper_WithValue_TCastSuccessfully("StringColumn", (reader, ordinal) => reader.GetString(ordinal), "Hello world");
        }

        [Test]
        public void GetString_WithNullValue_ThrowsInvalidCastException()
        {
            GetHelper_WithNullValue_ThrowsInvalidCastException("NullStringColumn", (reader, ordinal) => reader.GetString(ordinal));
        }

        [Test]
        public void GetBoolean_WithValue_CastSuccessfully()
        {
            GetHelper_WithValue_TCastSuccessfully("BooleanColumn", (reader, ordinal) => reader.GetBoolean(ordinal), true);
        }

        [Test]
        public void GetByte_WithValue_CastSuccessfully()
        {
            GetHelper_WithValue_TCastSuccessfully("ByteColumn", (reader, ordinal) => reader.GetByte(ordinal), 123);
        }

        [Test]
        public void GetByte_WithNullValue_ThrowsInvalidCastException()
        {
            GetHelper_WithNullValue_ThrowsInvalidCastException("NullByteColumn", (reader, ordinal) => reader.GetByte(ordinal));
        }

        [Test]
        public void GetFloat_WithValue_CastSuccessfully()
        {
            GetHelper_WithValue_TCastSuccessfully("FloatColumn", (reader, ordinal) => reader.GetFloat(ordinal), 123.45f);
        }

        [Test]
        public void GetFloat_WithNullValue_ThrowsInvalidCastException()
        {
            GetHelper_WithNullValue_ThrowsInvalidCastException("NullFloatColumn", (reader, ordinal) => reader.GetFloat(ordinal));
        }

        [Test]
        public void GetDouble_WithValue_CastSuccessfully()
        {
            GetHelper_WithValue_TCastSuccessfully("DoubleColumn", (reader, ordinal) => reader.GetDouble(ordinal), 123.45d);
        }

        [Test]
        public void GetDouble_WithNullValue_ThrowsInvalidCastException()
        {
            GetHelper_WithNullValue_ThrowsInvalidCastException("NullDoubleColumn", (reader, ordinal) => reader.GetDouble(ordinal));
        }

        [Test]
        public void GetDecimal_WithValue_CastSuccessfully()
        {
            GetHelper_WithValue_TCastSuccessfully("DecimalColumn", (reader, ordinal) => reader.GetDecimal(ordinal), 123.45m);
        }

        [Test]
        public void GetDecimal_WithNullValue_ThrowsInvalidCastException()
        {
            GetHelper_WithNullValue_ThrowsInvalidCastException("NullDecimalColumn", (reader, ordinal) => reader.GetDecimal(ordinal));
        }

        private void GetHelper_WithValue_TCastSuccessfully<T>(string columnName, Func<AseDataReader, int, T> testMethod, T expectedValue)
        {
            using (var connection = new AseConnection(_connectionStrings["default"]))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = AllTypesQuery;

                    using (var reader = command.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        var targetFieldOrdinal = reader.GetOrdinal(columnName);

                        Assert.IsTrue(reader.Read());

                        T value = testMethod(reader, targetFieldOrdinal);
                        Assert.AreEqual(expectedValue, value);

                        Assert.IsFalse(reader.Read());
                        Assert.IsFalse(reader.NextResult());
                    }
                }
            }
        }

        private void GetHelper_WithNullValue_ThrowsInvalidCastException<T>(string columnName, Func<AseDataReader, int, T> testMethod)
        {
            using (var connection = new AseConnection(_connectionStrings["default"]))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = AllTypesQuery;

                    using (var reader = command.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        var targetFieldOrdinal = reader.GetOrdinal(columnName);

                        Assert.IsTrue(reader.Read());

                        Assert.Throws<InvalidCastException>(() => testMethod(reader, targetFieldOrdinal));
                    }
                }
            }
        }
    }
}
