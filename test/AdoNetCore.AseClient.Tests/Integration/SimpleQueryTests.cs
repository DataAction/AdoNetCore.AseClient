using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dapper;
using Newtonsoft.Json;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration
{
    [TestFixture]
    public class SimpleQueryTests
    {
        private readonly Dictionary<string, string> _connectionStrings = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("ConnectionStrings.json"));

        private class IntsRow
        {
            public byte? Byte { get; set; }
            public short? Short { get; set; }
            public int? Int { get; set; }
            public long? Long { get; set; }
        }

        [Test]
        public void SelectInts_Literal_ShouldWork()
        {
            using (var connection = new AseConnection(_connectionStrings["default"]))
            {
                var row = connection.Query<IntsRow>("select convert(tinyint, 1) as Byte, convert(smallint, 2) as Short, convert(int, 3) as Int, convert(bigint, 4) as Long").FirstOrDefault();
                Assert.AreEqual(1, row?.Byte);
                Assert.AreEqual(2, row?.Short);
                Assert.AreEqual(3, row?.Int);
                Assert.AreEqual(4, row?.Long);
            }
        }

        [Test]
        public void SelectInts_Parameters_ShouldWork()
        {
            using (var connection = new AseConnection(_connectionStrings["default"]))
            {
                var pByte = (byte)1;
                var pShort = (short)2;
                var pInt = 4;
                var pLong = (long)8;

                var row = connection.Query<IntsRow>("select @pByte as Byte, @pShort as Short, @pInt as Int, @pLong as Long", new { pByte, pShort, pInt, pLong }).FirstOrDefault();

                Assert.AreEqual(pByte, row?.Byte);
                Assert.AreEqual(pShort, row?.Short);
                Assert.AreEqual(pInt, row?.Int);
                Assert.AreEqual(pLong, row?.Long);
            }
        }

        [Test]
        public void SelectNullInts_ShouldWork()
        {
            using (var connection = new AseConnection(_connectionStrings["default"]))
            {
                var row = connection.Query<IntsRow>("select convert(tinyint, null) as Byte, convert(smallint, null) as Short, convert(int, null) as Int, convert(bigint, null) as Long").FirstOrDefault();
                Assert.IsNull(row.Byte);
                Assert.IsNull(row.Short);
                Assert.IsNull(row.Int);
                Assert.IsNull(row.Long);
            }
        }

        [Test]
        public void SelectNullInts_Parameters_ShouldWork()
        {
            using (var connection = new AseConnection(_connectionStrings["default"]))
            {
                byte? pByte = null;
                short? pShort = null;
                int? pInt = null;
                long? pLong = null;

                var row = connection.Query<IntsRow>("select @pByte as Byte, @pShort as Short, @pInt as Int, @pLong as Long", new { pByte, pShort, pInt, pLong }).FirstOrDefault();

                Assert.AreEqual(pByte, row?.Byte);
                Assert.AreEqual(pShort, row?.Short);
                Assert.AreEqual(pInt, row?.Int);
                Assert.AreEqual(pLong, row?.Long);
            }
        }

        [Test]
        public void SelectShortString_Literal_ShouldWork()
        {
            using (var connection = new AseConnection(_connectionStrings["default"]))
            {
                var expected = Guid.NewGuid().ToString();
                Assert.AreEqual(expected, connection.ExecuteScalar<string>($"select '{expected}'"));
            }
        }

        [Test]
        public void SelectShortString_Parameter_ShouldWork()
        {
            using (var connection = new AseConnection(_connectionStrings["default"]))
            {
                var expected = Guid.NewGuid().ToString();
                Assert.AreEqual(expected, connection.ExecuteScalar<string>("select @expected", new { expected }));
            }
        }

        [Test]
        public void SelectNullShortString_Literal_ShouldWork()
        {
            using (var connection = new AseConnection(_connectionStrings["default"]))
            {
                Assert.AreEqual(null, connection.ExecuteScalar<string>($"select convert(varchar(255), null)"));
            }
        }

        [Test]
        public void SelectNullShortString_Parameter_ShouldWork()
        {
            using (var connection = new AseConnection(_connectionStrings["default"]))
            {
                string expected = null;
                Assert.AreEqual(expected, connection.ExecuteScalar<string>("select convert(varchar(255), @expected)", new { expected }));
            }
        }

        [TestCase(255)]
        [TestCase(256)]
        [TestCase(1000)]
        [TestCase(2000)]
        [TestCase(4000)]
        [TestCase(8000)]
        public void SelectLongString_Literal_ShouldWork(int count)
        {
            var expected = new string('1', count);
            using (var connection = new AseConnection(_connectionStrings["default"]))
            {
                Assert.AreEqual(expected, connection.ExecuteScalar<string>($"select '{expected}'"));
            }
        }

        [TestCase(255)]
        [TestCase(256)]
        [TestCase(1000)]
        [TestCase(2000)]
        [TestCase(4000)]
        [TestCase(8000)]
        public void SelectLongString_Parameter_ShouldWork(int count)
        {
            var expected = new string('1', count);
            using (var connection = new AseConnection(_connectionStrings["default"]))
            {
                Assert.AreEqual(expected, connection.ExecuteScalar<string>("select @expected", new { expected }));
            }
        }


        [Test]
        public void SelectNullLongString_ShouldWork()
        {
            using (var connection = new AseConnection(_connectionStrings["default"]))
            {
                string expected = null;
                Assert.AreEqual(expected, connection.ExecuteScalar<string>("select convert(varchar(1000), @expected)", new { expected }));
            }
        }

        [TestCase("null", null)]
        [TestCase("''", "          ")]
        [TestCase("' '", "          ")]
        [TestCase("'asdf'", "asdf      ")]
        [TestCase("'asdfasdfasdf'", "asdfasdfas")]
        public void SelectFixedLengthString_Literal_ShouldWork(string input, string expected)
        {
            using (var connection = new AseConnection(_connectionStrings["default"]))
            {
                Assert.AreEqual(expected, connection.ExecuteScalar<string>($"select convert(char(10), {input})"));
            }
        }

        [TestCase(null, null)]
        //Spec p.188: "A NULL value has a length of 0. There is no way to represent a non-NULL empty string of length 0."
        //in other words, we might think it's an empty string we're sending, but the database will always interpret it as null
        [TestCase("", null)]
        [TestCase(" ", "          ")]
        [TestCase("asdf", "asdf      ")]
        [TestCase("asdfasdfasdf", "asdfasdfas")]
        public void SelectFixedLengthString_Parameter_ShouldWork(string input, string expected)
        {
            using (var connection = new AseConnection(_connectionStrings["default"]))
            {
                Assert.AreEqual(expected, connection.ExecuteScalar<string>("select convert(char(10), @input)", new { input }));
            }
        }

        [Test]
        public void SelectShortBinary_ShouldWork()
        {
            using (var connection = new AseConnection(_connectionStrings["default"]))
            {
                var expected = Guid.NewGuid().ToByteArray();
                Assert.AreEqual(expected, connection.ExecuteScalar<byte[]>("select @expected", new { expected }));
            }
        }

        [Test]
        public void SelectNullShortBinary_ShouldWork()
        {
            using (var connection = new AseConnection(_connectionStrings["default"]))
            {
                byte[] expected = null;
                Assert.AreEqual(expected, connection.ExecuteScalar<byte[]>("select @expected", new { expected }));
            }
        }

        [Test]
        public void SelectLongBinary_ShouldWork()
        {
            using (var connection = new AseConnection(_connectionStrings["default"]))
            {
                var expected = Enumerable.Repeat<Func<byte[]>>(() => Guid.NewGuid().ToByteArray(), 100).SelectMany(f => f()).ToArray();
                Assert.AreEqual(expected, connection.ExecuteScalar<byte[]>("select @expected", new { expected }));
            }
        }

        [Test]
        public void SelectNullLongBinary_ShouldWork()
        {
            using (var connection = new AseConnection(_connectionStrings["default"]))
            {
                byte[] expected = null;
                Assert.AreEqual(expected, connection.ExecuteScalar<byte[]>("select convert(binary(1000), @expected)", new { expected }));
            }
        }

        [TestCaseSource(nameof(SelectDecimal_Literal_ShouldWork_Cases))]
        public void SelectDecimal_Literal_ShouldWork(string literal, int precision, int scale, decimal? expected)
        {
            using (var connection = new AseConnection(_connectionStrings["default"]))
            {
                Assert.AreEqual(expected, connection.ExecuteScalar<decimal?>($@"select convert(decimal({precision},{scale}), {literal})"));
            }
        }

        public static IEnumerable<TestCaseData> SelectDecimal_Literal_ShouldWork_Cases()
        {
            yield return new TestCaseData("null", 10, 0, null);
            yield return new TestCaseData("0", 10, 0, 0m);
            yield return new TestCaseData("1.1", 2, 1, 1.1m);
            yield return new TestCaseData("-1.1", 2, 1, -1.1m);
            yield return new TestCaseData("1.0123456789", 11, 10, 1.0123456789m);
            yield return new TestCaseData("-1.0123456789", 11, 10, -1.0123456789m);
            yield return new TestCaseData("987654321.0123456789", 19, 10, 987654321.0123456789m);
            yield return new TestCaseData("-987654321.0123456789", 19, 10, -987654321.0123456789m);
            yield return new TestCaseData("32109876543210.0123456789", 24, 10, 32109876543210.0123456789m);
            yield return new TestCaseData("-32109876543210.0123456789", 24, 10, -32109876543210.0123456789m);
        }

        [TestCaseSource(nameof(SelectDecimal_Literal_Simple_ShouldWork_Cases))]
        public void SelectDecimal_Literal_Simple_ShouldWork(string literal, decimal? expected)
        {
            using (var connection = new AseConnection(_connectionStrings["default"]))
            {
                Assert.AreEqual(expected, connection.ExecuteScalar<decimal?>($@"select
  convert(decimal(10,0), {literal}),
  convert(decimal(10,1), {literal}),
  convert(decimal(10,2), {literal}),
  convert(decimal(10,3), {literal}),
  convert(decimal(20,0), {literal}),
  convert(decimal(20,1), {literal}),
  convert(decimal(20,2), {literal}),
  convert(decimal(20,3), {literal})"
  ));
            }
        }

        public static IEnumerable<TestCaseData> SelectDecimal_Literal_Simple_ShouldWork_Cases()
        {
            yield return new TestCaseData("null", null);
            /*
l:6, p:10, s:0: 00 00 00 00 00 01 = 1/1
l:6, p:10, s:1: 00 00 00 00 00 0a = 10/10
l:6, p:10, s:2: 00 00 00 00 00 64 = 100/100
l:6, p:10, s:3: 00 00 00 00 03 e8 = 1000/1000
l:10, p:20, s:0: 00 00 00 00 00 00 00 00 00 01
l:10, p:20, s:1: 00 00 00 00 00 00 00 00 00 0a
l:10, p:20, s:2: 00 00 00 00 00 00 00 00 00 64
l:10, p:20, s:3: 00 00 00 00 00 00 00 00 03 e8
*/
            yield return new TestCaseData("1", 1m);
            /*
l:6, p:10, s:0: 01 00 00 00 00 01 = -1/1
l:6, p:10, s:1: 01 00 00 00 00 0a = -10/10
l:6, p:10, s:2: 01 00 00 00 00 64 = -100/100
l:6, p:10, s:3: 01 00 00 00 03 e8 = -1000/1000
l:10, p:20, s:0: 01 00 00 00 00 00 00 00 00 01
l:10, p:20, s:1: 01 00 00 00 00 00 00 00 00 0a
l:10, p:20, s:2: 01 00 00 00 00 00 00 00 00 64
l:10, p:20, s:3: 01 00 00 00 00 00 00 00 03 e8
*/
            yield return new TestCaseData("-1", -1m);
//so, first byte = negative sign
//interpret remaining bytes as int thing
//divide the result by 10^s
        }

        [TestCaseSource(nameof(SelectDecimal_Parameter_ShouldWork_Cases))]
        public void SelectDecimal_Parameter_ShouldWork(decimal expected)
        {
            using (var connection = new AseConnection(_connectionStrings["default"]))
            {
                Assert.AreEqual(expected, connection.ExecuteScalar<decimal?>("select @expected", new { expected }));
            }
        }

        public static IEnumerable<TestCaseData> SelectDecimal_Parameter_ShouldWork_Cases()
        {
            yield return new TestCaseData(null);
            yield return new TestCaseData(0m);
            yield return new TestCaseData(1.1m);
            yield return new TestCaseData(-1.1m);
            yield return new TestCaseData(1.0123456789m);
            yield return new TestCaseData(-1.0123456789m);
            yield return new TestCaseData(987654321.0123456789m);
            yield return new TestCaseData(-987654321.0123456789m);
            yield return new TestCaseData(32109876543210.0123456789m);
            yield return new TestCaseData(-32109876543210.0123456789m);
        }

        [TestCase("null", null)]
        [TestCase("0", 0.0f)]
        [TestCase("1.25", 1.25f)]
        [TestCase("-1.25", -1.25f)]
        [TestCase("123456789.5", 123456789.5f)]
        [TestCase("-123456789.5", -123456789.5f)]
        public void SelectFloat_Literal_Shouldwork(string input, float? expected)
        {
            using (var connection = new AseConnection(_connectionStrings["default"]))
            {
                Assert.AreEqual(expected, connection.ExecuteScalar<float?>($"select convert(float, {input})"));
            }
        }

        [TestCase(null)]
        [TestCase(0.0f)]
        [TestCase(1.25f)]
        [TestCase(-1.25f)]
        [TestCase(123456789.5f)]
        [TestCase(-123456789.5f)]
        public void SelectFloat_Parameter_Shouldwork(float? expected)
        {
            using (var connection = new AseConnection(_connectionStrings["default"]))
            {
                Assert.AreEqual(expected, connection.ExecuteScalar<float?>("select @expected", new { expected }));
            }
        }

        [TestCase("null", null)]
        [TestCase("0.0", 0.0d)]
        [TestCase("1.25", 1.25d)]
        [TestCase("-1.25", -1.25d)]
        [TestCase("123456789.5", 123456789.5d)]
        [TestCase("-123456789.5", -123456789.5d)]
        [TestCase("123456789.1", 123456789.1d)]
        [TestCase("-123456789.1", -123456789.1d)]
        public void SelectDouble_Literal_Shouldwork(string input, double? expected)
        {
            using (var connection = new AseConnection(_connectionStrings["default"]))
            {
                Assert.AreEqual(expected, connection.ExecuteScalar<double?>($"select convert(double precision, {input})"));
            }
        }

        [TestCase(null)]
        [TestCase(0.0d)]
        [TestCase(1.25d)]
        [TestCase(-1.25d)]
        [TestCase(123456789.5d)]
        [TestCase(-123456789.5d)]
        [TestCase(123456789.1d)]
        [TestCase(-123456789.1d)]
        public void SelectDouble_Parameter_Shouldwork(double? expected)
        {
            using (var connection = new AseConnection(_connectionStrings["default"]))
            {
                Assert.AreEqual(expected, connection.ExecuteScalar<double?>("select @expected", new { expected }));
            }
        }
    }
}
