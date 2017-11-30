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
    }
}
