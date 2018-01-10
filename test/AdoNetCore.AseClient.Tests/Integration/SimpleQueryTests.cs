using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AdoNetCore.AseClient.Internal;
using Dapper;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration
{
    [TestFixture]
    public class SimpleQueryTests
    {
        private readonly IDictionary<string, string> _connectionStrings = ConnectionStringLoader.Load();

        private IDbConnection GetConnection()
        {
            return GetConnection("big-packetsize"); //"default", "pooled", "big-packetsize"
        }

        private IDbConnection GetConnection(string csName)
        {
            Internal.Logger.Enable();
            return new AseConnection(_connectionStrings[csName]);
        }

        [TestCase("null", null)]
        [TestCase("255", 255)]
        [TestCase("0", 0)]
        public void SelectByte_Literal_ShouldWork(string input, byte? expected)
        {
            using (var connection = GetConnection())
            {
                Assert.AreEqual(expected, connection.ExecuteScalar<byte?>($"select convert(tinyint, {input})"));
            }
        }

        [TestCase(null)]
        [TestCase(255)]
        [TestCase(0)]
        public void SelectByte_Parameter_ShouldWork(byte? expected)
        {
            using (var connection = GetConnection())
            {
                Assert.AreEqual(expected, connection.ExecuteScalar<byte?>("select @expected", new { expected }));
            }
        }

        [TestCase("null", null)]
        [TestCase("127", 127)]
        [TestCase("-128", -128)]
        public void SelectSByte_Literal_ShouldWork(string input, sbyte? expected)
        {
            using (var connection = GetConnection())
            {
                Assert.AreEqual(expected, connection.ExecuteScalar<sbyte?>($"select {input}"));
            }
        }

        [TestCase(null)]
        [TestCase(127)]
        [TestCase(-128)]
        public void SelectSByte_Parameter_ShouldWork(sbyte? expected)
        {
            using (var connection = GetConnection())
            {
                Assert.AreEqual(expected, connection.ExecuteScalar<sbyte?>("select @expected", new { expected }));
            }
        }

        [TestCase("null", null)]
        [TestCase("32767", 32767)]
        [TestCase("-32768", -32768)]
        public void SelectShort_Literal_ShouldWork(string input, short? expected)
        {
            using (var connection = GetConnection())
            {
                Assert.AreEqual(expected, connection.ExecuteScalar<short?>($"select convert(smallint, {input})"));
            }
        }

        [TestCase(null)]
        [TestCase(32767)]
        [TestCase(-32768)]
        public void SelectShort_Parameter_ShouldWork(short? expected)
        {
            using (var connection = GetConnection())
            {
                Assert.AreEqual(expected, connection.ExecuteScalar<short?>("select @expected", new { expected }));
            }
        }

        [TestCase("null", null)]
        [TestCase("65535", (ushort)65535)]
        [TestCase("0", (ushort)0)]
        public void SelectUShort_Literal_ShouldWork(string input, ushort? expected)
        {
            using (var connection = GetConnection())
            {
                Assert.AreEqual(expected, connection.ExecuteScalar<ushort?>($"select convert(unsigned smallint, {input})"));
            }
        }

        [TestCase(null)]
        [TestCase((ushort)65535)]
        [TestCase((ushort)0)]
        public void SelectUShort_Parameter_ShouldWork(ushort? expected)
        {
            using (var connection = GetConnection())
            {
                Assert.AreEqual(expected, connection.ExecuteScalar<ushort?>("select @expected", new { expected }));
            }
        }

        [TestCase("null", null)]
        [TestCase("2147483647", 2147483647)]
        [TestCase("-2147483648", -2147483648)]
        public void SelectInt_Literal_ShouldWork(string input, int? expected)
        {
            using (var connection = GetConnection())
            {
                Assert.AreEqual(expected, connection.ExecuteScalar<int?>($"select convert(int, {input})"));
            }
        }

        [TestCase(null)]
        [TestCase(2147483647)]
        [TestCase(-2147483648)]
        public void SelectInt_Parameter_ShouldWork(int? expected)
        {
            using (var connection = GetConnection())
            {
                Assert.AreEqual(expected, connection.ExecuteScalar<int?>("select @expected", new { expected }));
            }
        }

        [TestCase("null", null)]
        [TestCase("4294967295", 4294967295)]
        [TestCase("0", (uint)0)]
        public void SelectUInt_Literal_ShouldWork(string input, uint? expected)
        {
            using (var connection = GetConnection())
            {
                Assert.AreEqual(expected, connection.ExecuteScalar<uint?>($"select convert(unsigned int, {input})"));
            }
        }

        [TestCase(null)]
        [TestCase(4294967295)]
        [TestCase((uint)0)]
        public void SelectUInt_Parameter_ShouldWork(uint? expected)
        {
            using (var connection = GetConnection())
            {
                Assert.AreEqual(expected, connection.ExecuteScalar<uint?>("select @expected", new { expected }));
            }
        }

        [TestCase("null", null)]
        [TestCase("9223372036854775807", 9223372036854775807)]
        [TestCase("-9223372036854775808", -9223372036854775808)]
        public void SelectLong_Literal_ShouldWork(string input, long? expected)
        {
            using (var connection = GetConnection())
            {
                Assert.AreEqual(expected, connection.ExecuteScalar<long?>($"select convert(bigint, {input})"));
            }
        }

        [TestCase(null)]
        [TestCase(9223372036854775807)]
        [TestCase(-9223372036854775808)]
        public void SelectLong_Parameter_ShouldWork(long? expected)
        {
            using (var connection = GetConnection())
            {
                Assert.AreEqual(expected, connection.ExecuteScalar<long?>("select @expected", new { expected }));
            }
        }

        [TestCase("null", null)]
        [TestCase("18446744073709551615", 18446744073709551615)]
        [TestCase("0", (ulong)0)]
        public void SelectULong_Literal_ShouldWork(string input, ulong? expected)
        {
            using (var connection = GetConnection())
            {
                Assert.AreEqual(expected, connection.ExecuteScalar<ulong?>($"select convert(unsigned bigint, {input})"));
            }
        }

        [TestCase(null)]
        [TestCase(18446744073709551615)]
        [TestCase((ulong)0)]
        public void SelectULong_Parameter_ShouldWork(ulong? expected)
        {
            using (var connection = GetConnection())
            {
                Assert.AreEqual(expected, connection.ExecuteScalar<ulong?>("select @expected", new { expected }));
            }
        }

        [Test]
        public void SelectShortString_Literal_ShouldWork()
        {
            using (var connection = GetConnection())
            {
                var expected = Guid.NewGuid().ToString();
                Assert.AreEqual(expected, connection.ExecuteScalar<string>($"select '{expected}'"));
            }
        }

        [Test]
        public void SelectShortString_Parameter_ShouldWork()
        {
            using (var connection = GetConnection())
            {
                var expected = Guid.NewGuid().ToString();
                Assert.AreEqual(expected, connection.ExecuteScalar<string>("select @expected", new { expected }));
            }
        }

        [Test]
        public void SelectShortStringFixedLength_Parameter_ShouldWork()
        {
            using (var connection = GetConnection())
            {
                var expected = Guid.NewGuid().ToString();
                var p = new DynamicParameters();
                p.Add("@expected", expected, DbType.StringFixedLength);
                Assert.AreEqual(expected, connection.ExecuteScalar<string>("select @expected", p));
            }
        }

        [Test]
        public void SelectCharStringFixedLength_Parameter_ShouldWork()
        {
            using (var connection = GetConnection())
            {
                var expected = 'X';
                var p = new DynamicParameters();
                p.Add("@expected", expected, DbType.StringFixedLength);
                Assert.AreEqual(expected, connection.ExecuteScalar<char>("select @expected", p));
            }
        }

        [Test]
        public void SelectShortAnsiStringFixedLength_Parameter_ShouldWork()
        {
            using (var connection = GetConnection())
            {
                var expected = Guid.NewGuid().ToString();
                var p = new DynamicParameters();
                p.Add("@expected", expected, DbType.AnsiStringFixedLength);
                Assert.AreEqual(expected, connection.ExecuteScalar<string>("select @expected", p));
            }
        }

        [Test]
        public void SelectNullShortString_Literal_ShouldWork()
        {
            using (var connection = GetConnection())
            {
                Assert.AreEqual(null, connection.ExecuteScalar<string>($"select convert(varchar(255), null)"));
            }
        }

        [Test]
        public void SelectNullShortString_Parameter_ShouldWork()
        {
            using (var connection = GetConnection())
            {
                Assert.AreEqual(null, connection.ExecuteScalar<string>("select convert(varchar(255), @expected)", new { expected = (string)null }));
            }
        }

        [TestCase(255)]
        [TestCase(256)]
        [TestCase(1000)]
        [TestCase(2000)]
        [TestCase(4000)]
        [TestCase(8000)]
        //TDS_TEXT
        [TestCase(16000)]
        [TestCase(32000)]
        [TestCase(64000)]
        [TestCase(128000)]
        public void SelectLongString_Literal_ShouldWork(int count)
        {
            var expected = new string('1', count);
            using (var connection = GetConnection("big-textsize"))
            {
                Assert.AreEqual(expected, connection.ExecuteScalar<string>($"select '{expected}'"));
            }
        }

        [TestCase(255)]
        [TestCase(256)]
        [TestCase(1024)]
        [TestCase(2048)]
        [TestCase(4096)]
        [TestCase(8192)]
        public void SelectLongString_Parameter_ShouldWork(int count)
        {
            var expected = new string('1', count);
            using (var connection = GetConnection("big-textsize"))
            {
                Assert.AreEqual(expected, connection.ExecuteScalar<string>("select @expected", new { expected }));
            }
        }

        [TestCase(255)]
        [TestCase(256)]
        [TestCase(1024)]
        [TestCase(2048)]
        [TestCase(4096)]
        [TestCase(8192)]
        [TestCase(16384)]
        public void SelectLongAnsiString_Parameter_ShouldWork(int count)
        {
            var expected = new string('1', count);
            using (var connection = GetConnection())
            {
                var p = new DynamicParameters();
                p.Add("@expected", expected, DbType.AnsiString);
                Assert.AreEqual(expected, connection.ExecuteScalar<string>("select @expected", p));
            }
        }


        [Test]
        public void SelectNullLongString_ShouldWork()
        {
            using (var connection = GetConnection())
            {
                Assert.AreEqual(null, connection.ExecuteScalar<string>("select convert(varchar(1000), @expected)", new { expected = (string)null }));
            }
        }

        [TestCase("null", null)]
        [TestCase("''", "          ")]
        [TestCase("' '", "          ")]
        [TestCase("'asdf'", "asdf      ")]
        [TestCase("'asdfasdfasdf'", "asdfasdfas")]
        public void SelectFixedLengthString_Literal_ShouldWork(string input, string expected)
        {
            using (var connection = GetConnection())
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
            using (var connection = GetConnection())
            {
                Assert.AreEqual(expected, connection.ExecuteScalar<string>("select convert(char(10), @input)", new { input }));
            }
        }

        [Test]
        public void SelectShortBinary_ShouldWork()
        {
            using (var connection = GetConnection())
            {
                var expected = Guid.NewGuid().ToByteArray();
                Assert.AreEqual(expected, connection.ExecuteScalar<byte[]>("select @expected", new { expected }));
            }
        }

        [Test]
        public void SelectNullShortBinary_ShouldWork()
        {
            using (var connection = GetConnection())
            {
                Assert.AreEqual(null, connection.ExecuteScalar<byte[]>("select @expected", new { expected = (byte[])null }));
            }
        }

        [Test]
        public void SelectLongBinary_ShouldWork()
        {
            using (var connection = GetConnection())
            {
                var expected = Enumerable.Repeat<Func<byte[]>>(() => Guid.NewGuid().ToByteArray(), 100).SelectMany(f => f()).ToArray();
                Assert.AreEqual(expected, connection.ExecuteScalar<byte[]>("select @expected", new { expected }));
            }
        }

        [Test]
        public void SelectNullLongBinary_ShouldWork()
        {
            using (var connection = GetConnection())
            {
                Assert.AreEqual(null, connection.ExecuteScalar<byte[]>("select convert(binary(1000), @expected)", new { expected = (byte[])null }));
            }
        }

        [TestCaseSource(nameof(SelectDecimal_Literal_ShouldWork_Cases))]
        public void SelectDecimal_Literal_ShouldWork(string literal, int precision, int scale, decimal? expected)
        {
            using (var connection = GetConnection())
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
            using (var connection = GetConnection())
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
            using (var connection = GetConnection())
            {
                Assert.AreEqual(expected, connection.ExecuteScalar<decimal?>("select @expected", new { expected }));
            }
        }

        /*[TestCaseSource(nameof(SelectAseDecimal_Parameter_ShouldWork_Cases))]
        public void SelectAseDecimal_Parameter_ShouldWork(AseDecimal expected)
        {
            using (var connection = GetConnection("asedecimal-on"))
            using (var command = connection.CreateCommand())
            {
                var pExpected = command.CreateParameter();
                pExpected.DbType = DbType.Decimal;
                pExpected.ParameterName = "@expected";
                pExpected.Value = expected;

                command.Parameters.Add(pExpected);

                command.CommandType = CommandType.Text;
                command.CommandText = "select @expected";

                connection.Open();
                var result = (AseDecimal)command.ExecuteScalar();
                Assert.AreEqual(expected, result);
            }
        }

        public static IEnumerable<TestCaseData> SelectAseDecimal_Parameter_ShouldWork_Cases()
        {
            yield return new TestCaseData(AseDecimal.Parse("99999999999999999999999999999999999999")); //10^38 - 1
            yield return new TestCaseData(AseDecimal.Parse("-99999999999999999999999999999999999999")); //-10^38 + 1
            //yield return new TestCaseData(AseDecimal.Parse("-100000000000000000000000000000000000000")); //-10^38: should throw IndexOutOfRangeException when trying to bind the parameter
        }


        [TestCaseSource(nameof(SelectAseDecimal_Literal_ShouldWork_Cases))]
        public void SelectAseDecimal_Literal_ShouldWork(string input, AseDecimal expected)
        {
            using (var connection = GetConnection("asedecimal-on"))
            using (var command = connection.CreateCommand())
            {
                command.CommandType = CommandType.Text;
                command.CommandText = $"select {input}";

                connection.Open();
                var result = (AseDecimal)command.ExecuteScalar();
                Assert.AreEqual(expected, result);
            }
        }

        public static IEnumerable<TestCaseData> SelectAseDecimal_Literal_ShouldWork_Cases()
        {
            //10^77 - 1
            yield return new TestCaseData(AseDecimal.Parse("99999999999999999999999999999999999999999999999999999999999999999999999999999"), "99999999999999999999999999999999999999999999999999999999999999999999999999999");
            //-10^77 + 1
            yield return new TestCaseData(AseDecimal.Parse("-99999999999999999999999999999999999999999999999999999999999999999999999999999"), "-99999999999999999999999999999999999999999999999999999999999999999999999999999");
        }*/

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
            yield return new TestCaseData(79228162514264337593543950335m); //max .net decimal value
            yield return new TestCaseData(-79228162514264337593543950335m); //min .net decimal value
            //yield return new TestCaseData(AseDecimal.Parse(""))
        }

        [TestCaseSource(nameof(SelectDecimal_OtherTypedParameter_ShouldWork_Cases))]
        public void SelectDecimal_OtherTypedParameter_ShouldWork(object expected)
        {
            using (var connection = GetConnection())
            {
                var p = new DynamicParameters();
                p.Add("@expected", expected, DbType.Decimal);
                Assert.AreEqual(Convert.ToDecimal(expected), connection.ExecuteScalar<decimal?>("select @expected", p));
            }
        }

        public static IEnumerable<TestCaseData> SelectDecimal_OtherTypedParameter_ShouldWork_Cases()
        {
            yield return new TestCaseData((byte)0);
            yield return new TestCaseData((short)0);
            yield return new TestCaseData(0);
            yield return new TestCaseData((long)0);
            yield return new TestCaseData((ushort)0);
            yield return new TestCaseData((uint)0);
            yield return new TestCaseData((ulong)0);
        }

        [TestCaseSource(nameof(SelectVarNumeric_Parameter_ShouldWork_Cases))]
        public void SelectVarNumeric_Parameter_ShouldWork(decimal expected)
        {
            using (var connection = GetConnection())
            {
                var p = new DynamicParameters();
                p.Add("@expected", expected, DbType.VarNumeric);
                Assert.AreEqual(expected, connection.ExecuteScalar<decimal?>("select @expected", p));
            }
        }

        public static IEnumerable<TestCaseData> SelectVarNumeric_Parameter_ShouldWork_Cases()
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

        [TestCaseSource(nameof(SelectMoney_Literal_ShouldWork_Cases))]
        public void SelectMoney_Literal_ShouldWork(string input, decimal? expected)
        {
            using (var connection = GetConnection())
            {
                Assert.AreEqual(expected, connection.ExecuteScalar<decimal?>($"select convert(money, {input})"));
            }
        }

        public static IEnumerable<TestCaseData> SelectMoney_Literal_ShouldWork_Cases()
        {
            //range: 922,337,203,685,477.5807 to -922,337,203,685,477.5808 equivalent to decimal(19,4)
            //looks like the range of long.max to long.min / 10000
            yield return new TestCaseData("null", null);
            yield return new TestCaseData("0", 0m);
            yield return new TestCaseData("0.9999", 0.9999m);
            yield return new TestCaseData("1", 1m);
            yield return new TestCaseData("-1", -1m);
            yield return new TestCaseData("1.1111", 1.1111m);
            yield return new TestCaseData("922337203685477.5807", 922337203685477.5807m);
            yield return new TestCaseData("-922337203685477.5808", -922337203685477.5808m);
        }

        [TestCaseSource(nameof(SelectSmallMoney_Literal_ShouldWork_Cases))]
        public void SelectSmallMoney_Literal_ShouldWork(string input, decimal? expected)
        {
            using (var connection = GetConnection())
            {
                Assert.AreEqual(expected, connection.ExecuteScalar<decimal?>($"select convert(smallmoney, {input})"));
            }
        }

        public static IEnumerable<TestCaseData> SelectSmallMoney_Literal_ShouldWork_Cases()
        {
            //range: 214,748.3647 to -214,748.3648 equivalent to decimal(10,4)
            //looks like the range of int.max to int.min / 10000
            yield return new TestCaseData("null", null);
            yield return new TestCaseData("0", 0m);
            yield return new TestCaseData("0.9999", 0.9999m);
            yield return new TestCaseData("1", 1m);
            yield return new TestCaseData("-1", -1m);
            yield return new TestCaseData("214748.3647", 214748.3647m);
            yield return new TestCaseData("-214748.3648", -214748.3648m);
        }

        [TestCaseSource(nameof(SelectMoney_Parameter_ShouldWork_Cases))]
        public void SelectMoney_CurrencyParameter_ShouldWork(decimal? expected)
        {
            using (var connection = GetConnection())
            {
                var p = new DynamicParameters();
                p.Add("@expected", expected, DbType.Currency);
                Assert.AreEqual(expected, connection.ExecuteScalar<decimal?>("select convert(money, @expected)", p));
            }
        }

        [TestCaseSource(nameof(SelectMoney_Parameter_ShouldWork_Cases))]
        public void SelectMoney_DecmalParameter_ShouldWork(decimal? expected)
        {
            using (var connection = GetConnection())
            {
                var p = new DynamicParameters();
                p.Add("@expected", expected, DbType.Decimal);
                Assert.AreEqual(expected, connection.ExecuteScalar<decimal?>("select convert(money, @expected)", p));
            }
        }

        public static IEnumerable<TestCaseData> SelectMoney_Parameter_ShouldWork_Cases()
        {
            yield return new TestCaseData(null);
            yield return new TestCaseData(0m);
            yield return new TestCaseData(1m);
            yield return new TestCaseData(1.1111m);
            yield return new TestCaseData(922337203685477.5807m); //max money value
            yield return new TestCaseData(-922337203685477.5808m); //min money value
        }

        [TestCase("null", null)]
        [TestCase("0", 0.0f)]
        [TestCase("1.25", 1.25f)]
        [TestCase("-1.25", -1.25f)]
        [TestCase("123456789.5", 123456789.5f)]
        [TestCase("-123456789.5", -123456789.5f)]
        public void SelectFloat_Literal_Shouldwork(string input, float? expected)
        {
            using (var connection = GetConnection())
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
            using (var connection = GetConnection())
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
            using (var connection = GetConnection())
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
            using (var connection = GetConnection())
            {
                Assert.AreEqual(expected, connection.ExecuteScalar<double?>("select @expected", new { expected }));
            }
        }

        [Test]
        public void SelectBool_Literal_ShouldWork()
        {
            using (var connection = GetConnection())
            {
                Assert.AreEqual(true, connection.ExecuteScalar<bool>("select convert(bit, 1)"));
                Assert.AreEqual(false, connection.ExecuteScalar<bool>("select convert(bit, 0)"));
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void SelectBool_Parameter_ShouldWork(bool expected)
        {
            using (var connection = GetConnection())
            {
                Assert.AreEqual(expected, connection.ExecuteScalar<bool>("select @expected", new { expected }));
            }
        }

        [Test]
        public void SelectNullBool_Parameter_ShouldWork()
        {
            using (var connection = GetConnection())
            {
                var p = new DynamicParameters();
                p.Add("@expected", DBNull.Value, DbType.Boolean);
                Assert.AreEqual(false, connection.ExecuteScalar<bool>("select @expected", p));
            }
        }

        [TestCaseSource(nameof(SelectDateTime_Literal_ShouldWork_Cases))]
        public void SelectDateTime_Literal_ShouldWork(string input, DateTime expected)
        {
            using (var connection = GetConnection())
            {
                Assert.AreEqual(expected, connection.ExecuteScalar<DateTime>($"select convert(datetime, {input})"));
            }
        }

        public static IEnumerable<TestCaseData> SelectDateTime_Literal_ShouldWork_Cases()
        {
            //46 2e ff ff 00 00 00 00; p1: -53690, p2: 0
            yield return new TestCaseData("'1753-01-01 00:00:00'", new DateTime(1753, 1, 1, 0, 0, 0));
            //46 2e ff ff d4 80 8b 01; p1: -53690, p2: 25919700
            yield return new TestCaseData("'1753-01-01 23:59:59'", new DateTime(1753, 1, 1, 23, 59, 59));
            //46 2e ff ff ff 81 8b 01; p1: -53690, p2: 25919999
            yield return new TestCaseData("'1753-01-01 23:59:59.997'", new DateTime(1753, 1, 1, 23, 59, 59, 997));
            //b2 2f ff ff 00 00 00 00; p1: -53326, p2: 0
            yield return new TestCaseData("'1753-12-31 00:00:00'", new DateTime(1753, 12, 31, 0, 0, 0));
            //b2 2f ff ff d4 80 8b 01; p1: -53326, p2: 25919700
            yield return new TestCaseData("'1753-12-31 23:59:59'", new DateTime(1753, 12, 31, 23, 59, 59));
            //b2 2f ff ff ff 81 8b 01; p1: -53326, p2: 25919999
            yield return new TestCaseData("'1753-12-31 23:59:59.997'", new DateTime(1753, 12, 31, 23, 59, 59, 997));

            //00 00 00 00 00 00 00 00; p1: 0, p2: 0
            yield return new TestCaseData("'1900-01-01 00:00:00'", new DateTime(1900, 1, 1, 0, 0, 0));
            //00 00 00 00 d4 80 8b 01; p1: 0, p2: 25919700
            yield return new TestCaseData("'1900-01-01 23:59:59'", new DateTime(1900, 1, 1, 23, 59, 59));
            //p2: 25919999
            yield return new TestCaseData("'1900-01-01 23:59:59.997'", new DateTime(1900, 1, 1, 23, 59, 59, 997));
            //6c 01 00 00 00 00 00 00; p1: 364, p2: 0
            yield return new TestCaseData("'1900-12-31 00:00:00'", new DateTime(1900, 12, 31, 0, 0, 0));
            //6c 01 00 00 d4 80 8b 01; p1: 364, p2: 25919700
            yield return new TestCaseData("'1900-12-31 23:59:59'", new DateTime(1900, 12, 31, 23, 59, 59));
            //p2: 25919999
            yield return new TestCaseData("'1900-12-31 23:59:59.997'", new DateTime(1900, 12, 31, 23, 59, 59, 997));

            //13 23 2d 00 00 00 00 00; p1: 2958099, p2: 0
            //https://www.wolframalpha.com/input/?i=2958099+days+before+9999-01-01 = 1900-01-01
            yield return new TestCaseData("'9999-01-01 00:00:00'", new DateTime(9999, 01, 01, 0, 0, 0));
            //13 23 2d 00 d4 80 8b 01; p1: 2958099, p2: 25919700
            yield return new TestCaseData("'9999-01-01 23:59:59'", new DateTime(9999, 01, 01, 23, 59, 59));
            //p2: 25919999
            yield return new TestCaseData("'9999-01-01 23:59:59.997'", new DateTime(9999, 01, 01, 23, 59, 59, 997));
            //7f 24 2d 00 00 00 00 00; p1: 2958463, p2: 0
            yield return new TestCaseData("'9999-12-31 00:00:00'", new DateTime(9999, 12, 31, 0, 0, 0));
            //7f 24 2d 00 d4 80 8b 01; p1: 2958463, p2: 25919700
            yield return new TestCaseData("'9999-12-31 23:59:59'", new DateTime(9999, 12, 31, 23, 59, 59));
            //p2: 25919999
            yield return new TestCaseData("'9999-12-31 23:59:59.997'", new DateTime(9999, 12, 31, 23, 59, 59, 997));
        }

        [TestCaseSource(nameof(SelectDateTime_Parameter_ShouldWork_Cases))]
        public void SelectDateTime_Parameter_ShouldWork(string _, DateTime? expected)
        {
            using (var connection = GetConnection())
            {
                Assert.AreEqual(expected, connection.ExecuteScalar<DateTime?>("select @expected", new { expected }));
            }
        }

        [Test]
        public void SelectDateTime_Parameter_Now_ShouldWork()
        {
            using (var connection = GetConnection())
            {
                var now = DateTime.Now;
                var expected = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, now.Kind);
                Assert.AreEqual(expected, connection.ExecuteScalar<DateTime?>("select @expected", new { expected }));
            }
        }

        public static IEnumerable<TestCaseData> SelectDateTime_Parameter_ShouldWork_Cases()
        {
            yield return new TestCaseData("null", null);
            yield return new TestCaseData("1753_1", new DateTime(1753, 1, 1, 0, 0, 0));
            yield return new TestCaseData("1753_2", new DateTime(1753, 1, 1, 23, 59, 59));
            yield return new TestCaseData("1753_3", new DateTime(1753, 1, 1, 23, 59, 59, 997));
            yield return new TestCaseData("1753_4", new DateTime(1753, 12, 31, 0, 0, 0));
            yield return new TestCaseData("1753_5", new DateTime(1753, 12, 31, 23, 59, 59));
            yield return new TestCaseData("1753_6", new DateTime(1753, 12, 31, 23, 59, 59, 997));
            yield return new TestCaseData("1900_1", new DateTime(1900, 1, 1, 0, 0, 0));
            yield return new TestCaseData("1900_2", new DateTime(1900, 1, 1, 23, 59, 59));
            yield return new TestCaseData("1900_3", new DateTime(1900, 1, 1, 23, 59, 59, 997));
            yield return new TestCaseData("1900_4", new DateTime(1900, 12, 31, 0, 0, 0));
            yield return new TestCaseData("1900_5", new DateTime(1900, 12, 31, 23, 59, 59));
            yield return new TestCaseData("1900_6", new DateTime(1900, 12, 31, 23, 59, 59, 997));
            yield return new TestCaseData("9999_1", new DateTime(9999, 01, 01, 0, 0, 0));
            yield return new TestCaseData("9999_2", new DateTime(9999, 01, 01, 23, 59, 59));
            yield return new TestCaseData("9999_3", new DateTime(9999, 01, 01, 23, 59, 59, 997));
            yield return new TestCaseData("9999_4", new DateTime(9999, 12, 31, 0, 0, 0));
            yield return new TestCaseData("9999_5", new DateTime(9999, 12, 31, 23, 59, 59));
            yield return new TestCaseData("9999_6", new DateTime(9999, 12, 31, 23, 59, 59, 997));
        }

        [TestCaseSource(nameof(SelectSmallDateTime_Literal_ShouldWork_Cases))]
        public void SelectSmallDateTime_Literal_ShouldWork(string input, DateTime expected)
        {
            using (var connection = GetConnection())
            {
                Assert.AreEqual(expected, connection.ExecuteScalar<DateTime>($"select convert(smalldatetime, {input})"));
            }
        }

        public static IEnumerable<TestCaseData> SelectSmallDateTime_Literal_ShouldWork_Cases()
        {
            //00 00 00 00, p1: 0, p2: 0
            yield return new TestCaseData("'1900-01-01 00:00:00'", new DateTime(1900, 1, 1, 0, 0, 0));
            //00 00 9f 05, p1: 0, p2: 1439
            yield return new TestCaseData("'1900-01-01 23:59:00'", new DateTime(1900, 1, 1, 23, 59, 00));
            //6c 01 00 00, p1: 364, p2: 0
            yield return new TestCaseData("'1900-12-31 00:00:00'", new DateTime(1900, 12, 31, 0, 0, 0));
            //6c 01 9f 05, p1: 364, p2: 1439
            yield return new TestCaseData("'1900-12-31 23:59:00'", new DateTime(1900, 12, 31, 23, 59, 00));

            //ff ff 00 00, p1: 65535, p2: 0
            yield return new TestCaseData("'2079-06-06 00:00:00'", new DateTime(2079, 06, 06, 0, 0, 0));
            //ff ff 9f 05, p1: 65535, p2: 1439
            yield return new TestCaseData("'2079-06-06 23:59:00'", new DateTime(2079, 06, 06, 23, 59, 00));
            //[ushort:days since 1900-01-01][ushort:minutes since 00:00]
        }

        [Test]
        public void SelectSmallDateTime_Parameter_Now_ShouldWork()
        {
            using (var connection = GetConnection())
            {
                var now = DateTime.Now;
                var expected = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, now.Kind);
                Assert.AreEqual(expected, connection.ExecuteScalar<DateTime?>("select convert(smalldatetime, @expected)", new { expected }));
            }
        }

        [TestCaseSource(nameof(SelectDate_Literal_ShouldWork_Cases))]
        public void SelectDate_Literal_ShouldWork(string input, DateTime? expected)
        {
            using (var connection = GetConnection())
            {
                Assert.AreEqual(expected, connection.ExecuteScalar<DateTime?>($"select convert(date, {input})"));
            }
        }

        public static IEnumerable<TestCaseData> SelectDate_Literal_ShouldWork_Cases()
        {
            yield return new TestCaseData("null", null);
            //range is 0001-01-01 to 9999-12-31
            //a5 6a f5 ff, i1: -693595, s1: 27301, s2: -11
            yield return new TestCaseData("'0001-01-01'", new DateTime(0001, 1, 1));
            //7f 24 2d 00, i1: 2958463, s1: 9343, s2: 45
            yield return new TestCaseData("'9999-12-31'", new DateTime(9999, 12, 31));
        }

        [TestCaseSource(nameof(SelectDate_Parameter_ShouldWork_Cases))]
        public void SelectDate_Parameter_ShouldWork(DateTime? expected)
        {
            using (var connection = GetConnection())
            {
                var p = new DynamicParameters();
                p.Add("@expected", expected, DbType.Date);
                Assert.AreEqual(expected, connection.ExecuteScalar<DateTime?>("select @expected", p));
            }
        }

        public static IEnumerable<TestCaseData> SelectDate_Parameter_ShouldWork_Cases()
        {
            yield return new TestCaseData(null);
            yield return new TestCaseData(new DateTime(0001, 1, 1));
            yield return new TestCaseData(new DateTime(9999, 12, 31));
        }

        [TestCaseSource(nameof(SelectTime_Literal_ShouldWork_Cases))]
        public void SelectTime_Literal_ShouldWork(string input, TimeSpan? expected)
        {
            using (var connection = GetConnection())
            {
                Assert.AreEqual(expected, connection.ExecuteScalar<TimeSpan?>($"select convert(time, {input})"));
            }
        }

        public static IEnumerable<TestCaseData> SelectTime_Literal_ShouldWork_Cases()
        {
            yield return new TestCaseData("null", null);
            //range is 00:00:00.000 to 23:59:59.999
            //
            yield return new TestCaseData("'00:00:00'", new TimeSpan(0, 0, 0, 0, 0));
            //
            yield return new TestCaseData("'23:59:59.997'", new TimeSpan(0, 23, 59, 59, 997));
        }

        [TestCaseSource(nameof(SelectTime_Parameter_ShouldWork_Cases))]
        public void SelectTime_Parameter_ShouldWork(TimeSpan? expected)
        {
            using (var connection = GetConnection())
            {
                var p = new DynamicParameters();
                p.Add("@expected", expected, DbType.Time);
                Assert.AreEqual(expected, connection.ExecuteScalar<TimeSpan?>("select @expected", p));
            }
        }

        public static IEnumerable<TestCaseData> SelectTime_Parameter_ShouldWork_Cases()
        {
            yield return new TestCaseData(null);
            yield return new TestCaseData(new TimeSpan(0, 0, 0, 0, 0));
            yield return new TestCaseData(new TimeSpan(0, 23, 59, 59, 997));
        }

        [Test]
        public void SelectGuid_Parameter_ShouldWork()
        {
            var expected = Guid.NewGuid();

            using (var connection = GetConnection())
            using (var command = connection.CreateCommand())
            {
                connection.Open();

                command.CommandText = "select @expected";
                command.CommandType = CommandType.Text;

                var p = command.CreateParameter();
                p.ParameterName = "@expected";
                p.DbType = DbType.Guid;
                p.Value = expected;
                command.Parameters.Add(p);

                using (var reader = command.ExecuteReader())
                {
                    Assert.IsTrue(reader.Read());
                    Assert.AreEqual(expected, reader.GetGuid(0));
                }
            }
        }
    }
}
