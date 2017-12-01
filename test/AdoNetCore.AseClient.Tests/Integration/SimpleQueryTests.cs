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
        public void SelectInts_ShouldWork()
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
            var sExpected = new string('1', count);
            using (var connection = new AseConnection(_connectionStrings["default"]))
            {
                Assert.AreEqual(sExpected, connection.ExecuteScalar<string>($"select '{sExpected}'"));
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
    }
}
