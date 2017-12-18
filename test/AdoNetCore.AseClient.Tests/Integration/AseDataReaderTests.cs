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
        // TODO - more tests -money, smallmoney, binary, varbinary, image (blob), unsigned

        private const string AllTypesQuery =
@"SELECT 
    CAST(123 AS INT) AS [INT],
    CAST(NULL AS INT) AS [NULL_INT],
    CAST(123 AS SMALLINT) AS [SMALLINT],
    CAST(NULL AS SMALLINT) AS [NULL_SMALLINT],
    CAST(123 AS BIGINT) AS [BIGINT],
    CAST(NULL AS BIGINT) AS [NULL_BIGINT],
    CAST(123 AS TINYINT) AS [TINYINT],
    CAST(NULL AS TINYINT) AS [NULL_TINYINT],

    CAST(123.45 AS REAL) AS [REAL],
    CAST(NULL AS REAL) AS [NULL_REAL],
    CAST(123.45 AS DOUBLE PRECISION) AS [DOUBLE_PRECISION],
    CAST(NULL AS DOUBLE PRECISION) AS [NULL_DOUBLE_PRECISION],
    CAST(123.45 AS NUMERIC(18,6)) AS [NUMERIC],
    CAST(NULL AS NUMERIC(18,6)) AS [NULL_NUMERIC],

    CAST(1 AS BIT) AS [BIT],

    CAST('Hello world' AS VARCHAR) AS [VARCHAR],
    CAST(NULL AS VARCHAR) AS [NULL_VARCHAR],
    CAST('Hello world' AS CHAR) AS [CHAR],
    CAST(NULL AS CHAR) AS [NULL_CHAR],
    CAST('Hello world' AS UNIVARCHAR) AS [UNIVARCHAR],
    CAST(NULL AS UNIVARCHAR) AS [NULL_UNIVARCHAR],
    CAST('Hello world' AS UNICHAR) AS [UNICHAR],
    CAST(NULL AS UNICHAR) AS [NULL_UNICHAR],
    CAST('Hello world' AS TEXT) AS [TEXT],
    CAST(NULL AS TEXT) AS [NULL_TEXT],
    CAST('Hello world' AS UNITEXT) AS [UNITEXT],
    CAST(NULL AS UNITEXT) AS [NULL_UNITEXT],

    --CAST('Apr 15 1987 10:23:00.000000PM' AS BIGDATETIME) AS [BIGDATETIME],
    --CAST(NULL AS BIGDATETIME) AS [NULL_BIGDATETIME],
    CAST('Apr 15 1987 10:23:00.000PM' AS DATETIME) AS [DATETIME],
    CAST(NULL AS DATETIME) AS [NULL_DATETIME],
    CAST('Apr 15 1987 10:23:00PM' AS SMALLDATETIME) AS [SMALLDATETIME],
    CAST(NULL AS SMALLDATETIME) AS [NULL_SMALLDATETIME],
    CAST('Apr 15 1987' AS DATE) AS [DATE],
    CAST(NULL AS DATE) AS [NULL_DATE],

    --CAST('11:59:59.999999 PM' AS BIGTIME) AS [BIGTIME],
    --CAST(NULL AS BIGTIME) AS [NULL_BIGTIME],
    CAST('23:59:59:997' AS TIME) AS [TIME],
    CAST(NULL AS TIME) AS [NULL_TIME]
";

        private readonly Dictionary<string, string> _connectionStrings = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("ConnectionStrings.json"));

        [TestCase("INT")]
        [TestCase("BIGINT")]
        [TestCase("TINYINT")]
        [TestCase("REAL")]
        [TestCase("DOUBLE_PRECISION")]
        [TestCase("NUMERIC")]
        public void GetInt32_WithValue_CastSuccessfully(string aseType)
        {
            GetHelper_WithValue_TCastSuccessfully(aseType, (reader, ordinal) => reader.GetInt32(ordinal), 123);
        }

        [TestCase("INT")]
        [TestCase("BIGINT")]
        [TestCase("TINYINT")]
        [TestCase("REAL")]
        [TestCase("DOUBLE_PRECISION")]
        [TestCase("NUMERIC")]
        public void GetInt32_WithNullValue_ThrowsInvalidCastException(string aseType)
        {
            GetHelper_WithNullValue_ThrowsInvalidCastException(aseType, (reader, ordinal) => reader.GetInt32(ordinal));
        }

        [TestCase("INT")]
        [TestCase("BIGINT")]
        [TestCase("TINYINT")]
        [TestCase("REAL")]
        [TestCase("DOUBLE_PRECISION")]
        [TestCase("NUMERIC")]
        public void GetInt16_WithValue_CastSuccessfully(string aseType)
        {
            GetHelper_WithValue_TCastSuccessfully(aseType, (reader, ordinal) => reader.GetInt16(ordinal), 123);
        }

        [TestCase("INT")]
        [TestCase("BIGINT")]
        [TestCase("TINYINT")]
        [TestCase("REAL")]
        [TestCase("DOUBLE_PRECISION")]
        [TestCase("NUMERIC")]
        public void GetInt16_WithNullValue_ThrowsInvalidCastException(string aseType)
        {
            GetHelper_WithNullValue_ThrowsInvalidCastException(aseType, (reader, ordinal) => reader.GetInt16(ordinal));
        }

        [TestCase("INT")]
        [TestCase("BIGINT")]
        [TestCase("TINYINT")]
        [TestCase("REAL")]
        [TestCase("DOUBLE_PRECISION")]
        [TestCase("NUMERIC")]
        public void GetInt64_WithValue_CastSuccessfully(string aseType)
        {
            GetHelper_WithValue_TCastSuccessfully(aseType, (reader, ordinal) => reader.GetInt64(ordinal), 123);
        }

        [TestCase("INT")]
        [TestCase("BIGINT")]
        [TestCase("TINYINT")]
        [TestCase("REAL")]
        [TestCase("DOUBLE_PRECISION")]
        [TestCase("NUMERIC")]
        public void GetInt64_WithNullValue_ThrowsInvalidCastException(string aseType)
        {
            GetHelper_WithNullValue_ThrowsInvalidCastException(aseType, (reader, ordinal) => reader.GetInt64(ordinal));
        }

        [TestCase("VARCHAR", "Hello world")]
        [TestCase("CHAR", "Hello world")]
        [TestCase("UNIVARCHAR", "Hello world")]
        [TestCase("UNICHAR", "Hello world")]
        [TestCase("TEXT", "Hello world")]
        [TestCase("UNITEXT", "Hello world")]
        public void GetString_WithValue_CastSuccessfully(string aseType, string expectedValue)
        {
            GetHelper_WithValue_TCastSuccessfully(aseType, (reader, ordinal) => reader.GetString(ordinal), expectedValue);
        }

        [TestCase("VARCHAR")]
        [TestCase("CHAR")]
        [TestCase("UNIVARCHAR")]
        [TestCase("UNICHAR")]
        [TestCase("TEXT")]
        [TestCase("UNITEXT")]
        public void GetString_WithNullValue_ThrowsInvalidCastException(string aseType)
        {
            GetHelper_WithNullValue_ThrowsInvalidCastException(aseType, (reader, ordinal) => reader.GetString(ordinal));
        }



        [TestCase("DATE", "Apr 15 1987")]
        [TestCase("DATETIME", "Apr 15 1987 10:23:00.000PM")]
        [TestCase("SMALLDATETIME", "Apr 15 1987 10:23:00PM")]
        [TestCase("BIGDATETIME", "Apr 15 1987 10:23:00.000000PM", Ignore = "true", IgnoreReason = "BIGDATETIME is not supported yet")]
        public void GetDateTime_WithValue_CastSuccessfully(string aseType, string expectedDateTime)
        {
            GetHelper_WithValue_TCastSuccessfully(aseType, (reader, ordinal) => reader.GetDateTime(ordinal), DateTime.Parse(expectedDateTime));
        }

        [TestCase("DATE")]
        [TestCase("DATETIME")]
        [TestCase("SMALLDATETIME")]
        [TestCase("BIGDATETIME", Ignore = "true", IgnoreReason = "BIGDATETIME is not supported yet")]
        public void GetDateTime_WithNullValue_ThrowsInvalidCastException(string aseType)
        {
            GetHelper_WithNullValue_ThrowsInvalidCastException(aseType, (reader, ordinal) => reader.GetDateTime(ordinal));
        }
        [TestCase("TIME", "23:59:59.997")]
        [TestCase("BIGTIME", "11:59:59.999999 PM", Ignore = "true", IgnoreReason = "BIGTIME is not supported yet")]
        public void GetTimeSpan_WithValue_CastSuccessfully(string aseType, string expectedTimeSpan)
        {
            GetHelper_WithValue_TCastSuccessfully(aseType, (reader, ordinal) => reader.GetTimeSpan(ordinal), TimeSpan.Parse(expectedTimeSpan));
        }

        [TestCase("TIME")]
        [TestCase("BIGTIME", Ignore = "true", IgnoreReason = "BIGTIME is not supported yet")]
        public void GetTimeSpan_WithNullValue_ThrowsInvalidCastException(string aseType)
        {
            GetHelper_WithNullValue_ThrowsInvalidCastException(aseType, (reader, ordinal) => reader.GetTimeSpan(ordinal));
        }

        [TestCase("BIT")]
        public void GetBoolean_WithValue_CastSuccessfully(string aseType)
        {
            GetHelper_WithValue_TCastSuccessfully(aseType, (reader, ordinal) => reader.GetBoolean(ordinal), true);
        }

        [TestCase("INT")]
        [TestCase("BIGINT")]
        [TestCase("TINYINT")]
        [TestCase("REAL")]
        [TestCase("DOUBLE_PRECISION")]
        [TestCase("NUMERIC")]
        public void GetByte_WithValue_CastSuccessfully(string aseType)
        {
            GetHelper_WithValue_TCastSuccessfully(aseType, (reader, ordinal) => reader.GetByte(ordinal), 123);
        }

        [TestCase("INT")]
        [TestCase("BIGINT")]
        [TestCase("TINYINT")]
        [TestCase("REAL")]
        [TestCase("DOUBLE_PRECISION")]
        [TestCase("NUMERIC")]
        public void GetByte_WithNullValue_ThrowsInvalidCastException(string aseType)
        {
            GetHelper_WithNullValue_ThrowsInvalidCastException(aseType, (reader, ordinal) => reader.GetByte(ordinal));
        }

        [TestCase("INT", 123)]
        [TestCase("BIGINT", 123)]
        [TestCase("TINYINT", 123)]
        [TestCase("REAL", 123.45f)]
        [TestCase("DOUBLE_PRECISION", 123.45f)]
        [TestCase("NUMERIC", 123.45f)]
        public void GetFloat_WithValue_CastSuccessfully(string aseType, float expectedValue)
        {
            GetHelper_WithValue_TCastSuccessfully(aseType, (reader, ordinal) => reader.GetFloat(ordinal), expectedValue);
        }

        [TestCase("INT")]
        [TestCase("BIGINT")]
        [TestCase("TINYINT")]
        [TestCase("REAL")]
        [TestCase("DOUBLE_PRECISION")]
        [TestCase("NUMERIC")]
        public void GetFloat_WithNullValue_ThrowsInvalidCastException(string aseType)
        {
            GetHelper_WithNullValue_ThrowsInvalidCastException(aseType, (reader, ordinal) => reader.GetFloat(ordinal));
        }

        [TestCase("INT", 123d)]
        [TestCase("BIGINT", 123d)]
        [TestCase("TINYINT", 123d)]
        [TestCase("REAL", 123.45d)]
        [TestCase("DOUBLE_PRECISION", 123.45d)]
        [TestCase("NUMERIC", 123.45d)]
        public void GetDouble_WithValue_CastSuccessfully(string aseType, double expectedValue)
        {
            GetHelper_WithValue_TCastSuccessfully(aseType, (reader, ordinal) => reader.GetDouble(ordinal), expectedValue);
        }

        [TestCase("INT")]
        [TestCase("BIGINT")]
        [TestCase("TINYINT")]
        [TestCase("REAL")]
        [TestCase("DOUBLE_PRECISION")]
        [TestCase("NUMERIC")]
        public void GetDouble_WithNullValue_ThrowsInvalidCastException(string aseType)
        {
            GetHelper_WithNullValue_ThrowsInvalidCastException(aseType, (reader, ordinal) => reader.GetDouble(ordinal));
        }

        [TestCase("INT", 123)]
        [TestCase("BIGINT", 123)]
        [TestCase("TINYINT", 123)]
        [TestCase("REAL", 123.45d)]
        [TestCase("DOUBLE_PRECISION", 123.45d)]
        [TestCase("NUMERIC", 123.45d)]
        public void GetDecimal_WithValue_CastSuccessfully(string aseType, decimal expectedValue)
        {
            GetHelper_WithValue_TCastSuccessfully(aseType, (reader, ordinal) => reader.GetDecimal(ordinal), expectedValue);
        }

        [TestCase("INT")]
        [TestCase("BIGINT")]
        [TestCase("TINYINT")]
        [TestCase("REAL")]
        [TestCase("DOUBLE_PRECISION")]
        [TestCase("NUMERIC")]
        public void GetDecimal_WithNullValue_ThrowsInvalidCastException(string aseType)
        {
            GetHelper_WithNullValue_ThrowsInvalidCastException(aseType, (reader, ordinal) => reader.GetDecimal(ordinal));
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

                        if (expectedValue is float || expectedValue is double)
                        {
                            Assert.That(expectedValue, Is.EqualTo(value).Within(0.1));
                        }
                        else if (expectedValue is string)
                        {
                            Assert.AreEqual(expectedValue, (value as string)?.Trim());
                        }
                        else
                        {
                            Assert.AreEqual(expectedValue, value);
                        }

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
                        var targetFieldOrdinal = reader.GetOrdinal($"NULL_{columnName}");

                        Assert.IsTrue(reader.Read());

                        Assert.Throws<InvalidCastException>(() => testMethod(reader, targetFieldOrdinal));
                    }
                }
            }
        }
    }
}
