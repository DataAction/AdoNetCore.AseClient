using System;
using System.Collections.Generic;
using System.IO;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Internal;
using AdoNetCore.AseClient.Token;
using NUnit.Framework;
using static AdoNetCore.AseClient.Token.CatchAllToken;

namespace AdoNetCore.AseClient.Tests.Unit
{
    [TestFixture]
    [Category("quick")]
    public class CatchAllTokenTests
    {
        [TestCase("110xxxxx", 0b1100_0000)]
        [TestCase("110xxxxx", 0b1101_1111)]
        public void ZeroLengthToken_NoBytesRead(string _, byte type)
        {
            var t = new CatchAllToken(type);
            using (var ms = new MemoryStream())
            {
                // All zero-length tokens are not actually zero-length.
                // They have a length, but it is derived from prior tokens (eg TDS_ALTROW's length is determined by a prior TDS_ALTFMT token)
                // As such, we cannot process the tokens with Catch-All without corrupting the stream's next-token position
                Assert.Throws<InvalidOperationException>(() => t.Read(ms, new DbEnvironment(), null));
            }
        }

        [TestCase("xx1100xx - 0s", 0b0011_0000, 1)]
        [TestCase("xx1100xx - 1s", 0b1111_0011, 1)]
        [TestCase("xx1101xx - 0s", 0b0011_0100, 2)]
        [TestCase("xx1101xx - 1s", 0b1111_0111, 2)]
        [TestCase("xx1110xx - 0s", 0b0011_1000, 4)]
        [TestCase("xx1110xx - 1s", 0b1111_1011, 4)]
        [TestCase("xx1111xx - 0s", 0b0011_1100, 8)]
        [TestCase("xx1111xx - 1s", 0b1111_1111, 8)]
        public void FixedLengthToken_FixedBytesRead(string _, byte type, int dataLength)
        {
            var t = new CatchAllToken(type);
            using (var ms = new MemoryStream())
            {
                ms.Write(new byte[dataLength]);
                ms.Seek(0, SeekOrigin.Begin);

                t.Read(ms, new DbEnvironment(), null);

                Assert.AreEqual(ms.Length, ms.Position);
            }
        }

        [TestCase("1010xxxx - 0s", 0b1010_0000)]
        [TestCase("1010xxxx - 1s", 0b1010_1111)]
        [TestCase("1110xxxx - 0s", 0b1110_0000)]
        [TestCase("1110xxxx - 1s", 0b1110_1111)]
        [TestCase("1000xxxx - 0s", 0b1000_0000)]
        [TestCase("1000xxxx - 1s", 0b1000_1111)]
        public void VariableLengthToken_UShortLength_VariableBytesRead(string _, byte type)
        {
            ushort dataLength = 10;

            var t = new CatchAllToken(type);
            using (var ms = new MemoryStream())
            {
                ms.WriteUShort(dataLength);
                ms.Write(new byte[dataLength]);
                ms.Seek(0, SeekOrigin.Begin);

                t.Read(ms, new DbEnvironment(), null);

                Assert.AreEqual(ms.Length, ms.Position);
            }
        }

        [TestCase("001000xx - 0s", 0b0010_0000)]
        [TestCase("001000xx - 1s", 0b0010_0011)]
        [TestCase("011000xx - 0s", 0b0110_0000)]
        [TestCase("011000xx - 1s", 0b0110_0011)]
        public void VariableLengthToken_UIntLength_VariableBytesRead(string _, byte type)
        {
            uint dataLength = 10;

            var t = new CatchAllToken(type);
            using (var ms = new MemoryStream())
            {
                ms.WriteUInt(dataLength);
                ms.Write(new byte[dataLength]);
                ms.Seek(0, SeekOrigin.Begin);

                t.Read(ms, new DbEnvironment(), null);

                Assert.AreEqual(ms.Length, ms.Position);
            }
        }

        [TestCase("001001xx - 0s", 0b0010_0100)]
        [TestCase("001001xx - 1s", 0b0010_0111)]
        [TestCase("001010xx - 0s", 0b0010_1000)]
        [TestCase("001010xx - 1s", 0b0010_1011)]
        [TestCase("011001xx - 0s", 0b0110_0100)]
        [TestCase("011001xx - 1s", 0b0110_0111)]
        [TestCase("011010xx - 0s", 0b0110_1000)]
        [TestCase("011010xx - 1s", 0b0110_1011)]
        public void VariableLengthToken_ByteLength_VariableBytesRead(string _, byte type)
        {
            byte dataLength = 10;

            var t = new CatchAllToken(type);
            using (var ms = new MemoryStream())
            {
                ms.WriteByte(dataLength);
                ms.Write(new byte[dataLength]);
                ms.Seek(0, SeekOrigin.Begin);

                t.Read(ms, new DbEnvironment(), null);

                Assert.AreEqual(ms.Length, ms.Position);
            }
        }

        [TestCase(CatchAllLength.Undefined, 0u, 0u)]
        [TestCase(CatchAllLength.Absent, 0u, 0u)]
        [TestCase(CatchAllLength.Fixed_1, 0u, 1u)]
        [TestCase(CatchAllLength.Fixed_2, 0u, 2u)]
        [TestCase(CatchAllLength.Fixed_4, 0u, 4u)]
        [TestCase(CatchAllLength.Fixed_8, 0u, 8u)]
        [TestCase(CatchAllLength.Dynamic_1, 1u, 0b1101_0110u)]
        [TestCase(CatchAllLength.Dynamic_2, 2u, 0b0111_0101__1101_0110u)]
        [TestCase(CatchAllLength.Dynamic_4, 4u, 0b1111_0000__1011_0010__0111_0101__1101_0110u)]
        public void CatchAllLength_ReadLength(object cal, uint expectedRead, uint expectedLength)
        {
            using (var ms = new MemoryStream())
            {
                ms.Write(new byte[] { 0b1101_0110, 0b0111_0101, 0b1011_0010, 0b1111_0000, 0b1100_1100 });
                ms.Seek(0, SeekOrigin.Begin);
                var actualLen = ReadLength(ms, (CatchAllLength)cal);
                Assert.AreEqual(expectedRead, ms.Position);
                Assert.AreEqual(expectedLength, actualLen);
            }
        }

        [TestCaseSource(nameof(ExhaustiveLengthTestInput))]
        public void ExhaustiveLengthTest(object tokenType, object expectedLength)
        {
            var cl = ClassifyLength((byte) tokenType);
            Assert.AreEqual((CatchAllLength) expectedLength, cl);
        }

        [TestCaseSource(nameof(DefinedTokenLengthTestInput))]
        public void DefinedTokenLengthTest(object tokenType, object expectedLength)
        {
            var cl = ClassifyLength((byte)tokenType);
            Assert.AreEqual((CatchAllLength)expectedLength, cl);
        }

        public static IEnumerable<object[]> ExhaustiveLengthTestInput
        {
            get
            {
                // Values expected by strictly obeying the type/length definitions
                var items = new List<object[]>
                {
                    new object[] { (TokenType) 0b0000_0000, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0000_0001, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0000_0010, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0000_0011, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0000_0100, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0000_0101, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0000_0110, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0000_0111, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0000_1000, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0000_1001, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0000_1010, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0000_1011, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0000_1100, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0000_1101, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0000_1110, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0000_1111, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0001_0000, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0001_0001, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0001_0010, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0001_0011, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0001_0100, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0001_0101, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0001_0110, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0001_0111, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0001_1000, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0001_1001, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0001_1010, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0001_1011, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0001_1100, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0001_1101, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0001_1110, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0001_1111, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0010_0000, CatchAllLength.Dynamic_4 },
                    new object[] { (TokenType) 0b0010_0001, CatchAllLength.Dynamic_4 },
                    new object[] { (TokenType) 0b0010_0010, CatchAllLength.Dynamic_4 },
                    new object[] { (TokenType) 0b0010_0011, CatchAllLength.Dynamic_4 },
                    new object[] { (TokenType) 0b0010_0100, CatchAllLength.Dynamic_1 },
                    new object[] { (TokenType) 0b0010_0101, CatchAllLength.Dynamic_1 },
                    new object[] { (TokenType) 0b0010_0110, CatchAllLength.Dynamic_1 },
                    new object[] { (TokenType) 0b0010_0111, CatchAllLength.Dynamic_1 },
                    new object[] { (TokenType) 0b0010_1000, CatchAllLength.Dynamic_1 },
                    new object[] { (TokenType) 0b0010_1001, CatchAllLength.Dynamic_1 },
                    new object[] { (TokenType) 0b0010_1010, CatchAllLength.Dynamic_1 },
                    new object[] { (TokenType) 0b0010_1011, CatchAllLength.Dynamic_1 },
                    new object[] { (TokenType) 0b0010_1100, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0010_1101, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0010_1110, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0010_1111, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0011_0000, CatchAllLength.Fixed_1 },
                    new object[] { (TokenType) 0b0011_0001, CatchAllLength.Fixed_1 },
                    new object[] { (TokenType) 0b0011_0010, CatchAllLength.Fixed_1 },
                    new object[] { (TokenType) 0b0011_0011, CatchAllLength.Fixed_1 },
                    new object[] { (TokenType) 0b0011_0100, CatchAllLength.Fixed_2 },
                    new object[] { (TokenType) 0b0011_0101, CatchAllLength.Fixed_2 },
                    new object[] { (TokenType) 0b0011_0110, CatchAllLength.Fixed_2 },
                    new object[] { (TokenType) 0b0011_0111, CatchAllLength.Fixed_2 },
                    new object[] { (TokenType) 0b0011_1000, CatchAllLength.Fixed_4 },
                    new object[] { (TokenType) 0b0011_1001, CatchAllLength.Fixed_4 },
                    new object[] { (TokenType) 0b0011_1010, CatchAllLength.Fixed_4 },
                    new object[] { (TokenType) 0b0011_1011, CatchAllLength.Fixed_4 },
                    new object[] { (TokenType) 0b0011_1100, CatchAllLength.Fixed_8 },
                    new object[] { (TokenType) 0b0011_1101, CatchAllLength.Fixed_8 },
                    new object[] { (TokenType) 0b0011_1110, CatchAllLength.Fixed_8 },
                    new object[] { (TokenType) 0b0011_1111, CatchAllLength.Fixed_8 },
                    new object[] { (TokenType) 0b0100_0000, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0100_0001, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0100_0010, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0100_0011, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0100_0100, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0100_0101, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0100_0110, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0100_0111, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0100_1000, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0100_1001, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0100_1010, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0100_1011, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0100_1100, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0100_1101, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0100_1110, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0100_1111, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0101_0000, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0101_0001, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0101_0010, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0101_0011, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0101_0100, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0101_0101, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0101_0110, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0101_0111, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0101_1000, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0101_1001, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0101_1010, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0101_1011, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0101_1100, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0101_1101, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0101_1110, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0101_1111, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0110_0000, CatchAllLength.Dynamic_4 },
                    new object[] { (TokenType) 0b0110_0001, CatchAllLength.Dynamic_4 },
                    new object[] { (TokenType) 0b0110_0010, CatchAllLength.Dynamic_4 },
                    new object[] { (TokenType) 0b0110_0011, CatchAllLength.Dynamic_4 },
                    new object[] { (TokenType) 0b0110_0100, CatchAllLength.Dynamic_1 },
                    new object[] { (TokenType) 0b0110_0101, CatchAllLength.Dynamic_1 },
                    new object[] { (TokenType) 0b0110_0110, CatchAllLength.Dynamic_1 },
                    new object[] { (TokenType) 0b0110_0111, CatchAllLength.Dynamic_1 },
                    new object[] { (TokenType) 0b0110_1000, CatchAllLength.Dynamic_1 },
                    new object[] { (TokenType) 0b0110_1001, CatchAllLength.Dynamic_1 },
                    new object[] { (TokenType) 0b0110_1010, CatchAllLength.Dynamic_1 },
                    new object[] { (TokenType) 0b0110_1011, CatchAllLength.Dynamic_1 },
                    new object[] { (TokenType) 0b0110_1100, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0110_1101, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0110_1110, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0110_1111, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b0111_0000, CatchAllLength.Fixed_1 },
                    new object[] { (TokenType) 0b0111_0001, CatchAllLength.Fixed_1 },
                    new object[] { (TokenType) 0b0111_0010, CatchAllLength.Fixed_1 },
                    new object[] { (TokenType) 0b0111_0011, CatchAllLength.Fixed_1 },
                    new object[] { (TokenType) 0b0111_0100, CatchAllLength.Fixed_2 },
                    new object[] { (TokenType) 0b0111_0101, CatchAllLength.Fixed_2 },
                    new object[] { (TokenType) 0b0111_0110, CatchAllLength.Fixed_2 },
                    new object[] { (TokenType) 0b0111_0111, CatchAllLength.Fixed_2 },
                    new object[] { (TokenType) 0b0111_1000, CatchAllLength.Fixed_4 },
                    new object[] { (TokenType) 0b0111_1001, CatchAllLength.Fixed_4 },
                    new object[] { (TokenType) 0b0111_1010, CatchAllLength.Fixed_4 },
                    new object[] { (TokenType) 0b0111_1011, CatchAllLength.Fixed_4 },
                    new object[] { (TokenType) 0b0111_1100, CatchAllLength.Fixed_8 },
                    new object[] { (TokenType) 0b0111_1101, CatchAllLength.Fixed_8 },
                    new object[] { (TokenType) 0b0111_1110, CatchAllLength.Fixed_8 },
                    new object[] { (TokenType) 0b0111_1111, CatchAllLength.Fixed_8 },
                    new object[] { (TokenType) 0b1000_0000, CatchAllLength.Dynamic_2 },
                    new object[] { (TokenType) 0b1000_0001, CatchAllLength.Dynamic_2 },
                    new object[] { (TokenType) 0b1000_0010, CatchAllLength.Dynamic_2 },
                    new object[] { (TokenType) 0b1000_0011, CatchAllLength.Dynamic_2 },
                    new object[] { (TokenType) 0b1000_0100, CatchAllLength.Dynamic_2 },
                    new object[] { (TokenType) 0b1000_0101, CatchAllLength.Dynamic_2 },
                    new object[] { (TokenType) 0b1000_0110, CatchAllLength.Dynamic_2 },
                    new object[] { (TokenType) 0b1000_0111, CatchAllLength.Dynamic_2 },
                    new object[] { (TokenType) 0b1000_1000, CatchAllLength.Dynamic_2 },
                    new object[] { (TokenType) 0b1000_1001, CatchAllLength.Dynamic_2 },
                    new object[] { (TokenType) 0b1000_1010, CatchAllLength.Dynamic_2 },
                    new object[] { (TokenType) 0b1000_1011, CatchAllLength.Dynamic_2 },
                    new object[] { (TokenType) 0b1000_1100, CatchAllLength.Dynamic_2 },
                    new object[] { (TokenType) 0b1000_1101, CatchAllLength.Dynamic_2 },
                    new object[] { (TokenType) 0b1000_1110, CatchAllLength.Dynamic_2 },
                    new object[] { (TokenType) 0b1000_1111, CatchAllLength.Dynamic_2 },
                    new object[] { (TokenType) 0b1001_0000, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b1001_0001, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b1001_0010, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b1001_0011, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b1001_0100, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b1001_0101, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b1001_0110, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b1001_0111, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b1001_1000, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b1001_1001, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b1001_1010, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b1001_1011, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b1001_1100, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b1001_1101, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b1001_1110, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b1001_1111, CatchAllLength.Undefined },
                    new object[] { (TokenType) 0b1010_0000, CatchAllLength.Dynamic_2 },
                    new object[] { (TokenType) 0b1010_0001, CatchAllLength.Dynamic_2 },
                    new object[] { (TokenType) 0b1010_0010, CatchAllLength.Dynamic_2 },
                    new object[] { (TokenType) 0b1010_0011, CatchAllLength.Dynamic_2 },
                    new object[] { (TokenType) 0b1010_0100, CatchAllLength.Dynamic_2 },
                    new object[] { (TokenType) 0b1010_0101, CatchAllLength.Dynamic_2 },
                    new object[] { (TokenType) 0b1010_0110, CatchAllLength.Dynamic_2 },
                    new object[] { (TokenType) 0b1010_0111, CatchAllLength.Dynamic_2 },
                    new object[] { (TokenType) 0b1010_1000, CatchAllLength.Dynamic_2 },
                    new object[] { (TokenType) 0b1010_1001, CatchAllLength.Dynamic_2 },
                    new object[] { (TokenType) 0b1010_1010, CatchAllLength.Dynamic_2 },
                    new object[] { (TokenType) 0b1010_1011, CatchAllLength.Dynamic_2 },
                    new object[] { (TokenType) 0b1010_1100, CatchAllLength.Dynamic_2 },
                    new object[] { (TokenType) 0b1010_1101, CatchAllLength.Dynamic_2 },
                    new object[] { (TokenType) 0b1010_1110, CatchAllLength.Dynamic_2 },
                    new object[] { (TokenType) 0b1010_1111, CatchAllLength.Dynamic_2 },
                    new object[] { (TokenType) 0b1011_0000, CatchAllLength.Fixed_1 },
                    new object[] { (TokenType) 0b1011_0001, CatchAllLength.Fixed_1 },
                    new object[] { (TokenType) 0b1011_0010, CatchAllLength.Fixed_1 },
                    new object[] { (TokenType) 0b1011_0011, CatchAllLength.Fixed_1 },
                    new object[] { (TokenType) 0b1011_0100, CatchAllLength.Fixed_2 },
                    new object[] { (TokenType) 0b1011_0101, CatchAllLength.Fixed_2 },
                    new object[] { (TokenType) 0b1011_0110, CatchAllLength.Fixed_2 },
                    new object[] { (TokenType) 0b1011_0111, CatchAllLength.Fixed_2 },
                    new object[] { (TokenType) 0b1011_1000, CatchAllLength.Fixed_4 },
                    new object[] { (TokenType) 0b1011_1001, CatchAllLength.Fixed_4 },
                    new object[] { (TokenType) 0b1011_1010, CatchAllLength.Fixed_4 },
                    new object[] { (TokenType) 0b1011_1011, CatchAllLength.Fixed_4 },
                    new object[] { (TokenType) 0b1011_1100, CatchAllLength.Fixed_8 },
                    new object[] { (TokenType) 0b1011_1101, CatchAllLength.Fixed_8 },
                    new object[] { (TokenType) 0b1011_1110, CatchAllLength.Fixed_8 },
                    new object[] { (TokenType) 0b1011_1111, CatchAllLength.Fixed_8 },
                    new object[] { (TokenType) 0b1100_0000, CatchAllLength.Absent },
                    new object[] { (TokenType) 0b1100_0001, CatchAllLength.Absent },
                    new object[] { (TokenType) 0b1100_0010, CatchAllLength.Absent },
                    new object[] { (TokenType) 0b1100_0011, CatchAllLength.Absent },
                    new object[] { (TokenType) 0b1100_0100, CatchAllLength.Absent },
                    new object[] { (TokenType) 0b1100_0101, CatchAllLength.Absent },
                    new object[] { (TokenType) 0b1100_0110, CatchAllLength.Absent },
                    new object[] { (TokenType) 0b1100_0111, CatchAllLength.Absent },
                    new object[] { (TokenType) 0b1100_1000, CatchAllLength.Absent },
                    new object[] { (TokenType) 0b1100_1001, CatchAllLength.Absent },
                    // TDS spec 5.2.3 claims KEY token is a zero length token in pattern 1010xxxx;
                    // But KEY is actually 0xCA (0b1100_1010)
                    new object[] { (TokenType) 0b1100_1010, CatchAllLength.Absent },
                    new object[] { (TokenType) 0b1100_1011, CatchAllLength.Absent },
                    new object[] { (TokenType) 0b1100_1100, CatchAllLength.Absent },
                    new object[] { (TokenType) 0b1100_1101, CatchAllLength.Absent },
                    new object[] { (TokenType) 0b1100_1110, CatchAllLength.Absent },
                    new object[] { (TokenType) 0b1100_1111, CatchAllLength.Absent },
                    new object[] { (TokenType) 0b1101_0000, CatchAllLength.Absent },
                    // TDS spec 5.2.3 claims ROW token is a zero length token in pattern 1110xxxx;
                    // But ROW is actually 0xD1 (0b1101_0001)
                    new object[] { (TokenType) 0b1101_0001, CatchAllLength.Absent },
                    new object[] { (TokenType) 0b1101_0010, CatchAllLength.Absent },
                    // TDS spec 5.2.3 claims ALTROW token is a zero length token in pattern 1110xxxx;
                    // But ALTROW is actually 0xD3 (0b1101_0011)
                    new object[] { (TokenType) 0b1101_0011, CatchAllLength.Absent },
                    new object[] { (TokenType) 0b1101_0100, CatchAllLength.Absent },
                    new object[] { (TokenType) 0b1101_0101, CatchAllLength.Absent },
                    new object[] { (TokenType) 0b1101_0110, CatchAllLength.Absent },
                    // TDS spec 5.2.3 claims PARAMS token is a zero length token in pattern 1110xxxx;
                    // But PARAMS is actually 0xD7 (0b1101_0111)
                    new object[] { (TokenType) 0b1101_0111, CatchAllLength.Absent },
                    new object[] { (TokenType) 0b1101_1000, CatchAllLength.Absent },
                    new object[] { (TokenType) 0b1101_1001, CatchAllLength.Absent },
                    new object[] { (TokenType) 0b1101_1010, CatchAllLength.Absent },
                    new object[] { (TokenType) 0b1101_1011, CatchAllLength.Absent },
                    new object[] { (TokenType) 0b1101_1100, CatchAllLength.Absent },
                    new object[] { (TokenType) 0b1101_1101, CatchAllLength.Absent },
                    new object[] { (TokenType) 0b1101_1110, CatchAllLength.Absent },
                    new object[] { (TokenType) 0b1101_1111, CatchAllLength.Absent },
                    new object[] { (TokenType) 0b1110_0000, CatchAllLength.Dynamic_2 },
                    new object[] { (TokenType) 0b1110_0001, CatchAllLength.Dynamic_2 },
                    new object[] { (TokenType) 0b1110_0010, CatchAllLength.Dynamic_2 },
                    new object[] { (TokenType) 0b1110_0011, CatchAllLength.Dynamic_2 },
                    new object[] { (TokenType) 0b1110_0100, CatchAllLength.Dynamic_2 },
                    new object[] { (TokenType) 0b1110_0101, CatchAllLength.Dynamic_2 },
                    new object[] { (TokenType) 0b1110_0110, CatchAllLength.Dynamic_2 },
                    new object[] { (TokenType) 0b1110_0111, CatchAllLength.Dynamic_2 },
                    new object[] { (TokenType) 0b1110_1000, CatchAllLength.Dynamic_2 },
                    new object[] { (TokenType) 0b1110_1001, CatchAllLength.Dynamic_2 },
                    new object[] { (TokenType) 0b1110_1010, CatchAllLength.Dynamic_2 },
                    new object[] { (TokenType) 0b1110_1011, CatchAllLength.Dynamic_2 },
                    new object[] { (TokenType) 0b1110_1100, CatchAllLength.Dynamic_2 },
                    new object[] { (TokenType) 0b1110_1101, CatchAllLength.Dynamic_2 },
                    new object[] { (TokenType) 0b1110_1110, CatchAllLength.Dynamic_2 },
                    new object[] { (TokenType) 0b1110_1111, CatchAllLength.Dynamic_2 },
                    new object[] { (TokenType) 0b1111_0000, CatchAllLength.Fixed_1 },
                    new object[] { (TokenType) 0b1111_0001, CatchAllLength.Fixed_1 },
                    new object[] { (TokenType) 0b1111_0010, CatchAllLength.Fixed_1 },
                    new object[] { (TokenType) 0b1111_0011, CatchAllLength.Fixed_1 },
                    new object[] { (TokenType) 0b1111_0100, CatchAllLength.Fixed_2 },
                    new object[] { (TokenType) 0b1111_0101, CatchAllLength.Fixed_2 },
                    new object[] { (TokenType) 0b1111_0110, CatchAllLength.Fixed_2 },
                    new object[] { (TokenType) 0b1111_0111, CatchAllLength.Fixed_2 },
                    new object[] { (TokenType) 0b1111_1000, CatchAllLength.Fixed_4 },
                    new object[] { (TokenType) 0b1111_1001, CatchAllLength.Fixed_4 },
                    new object[] { (TokenType) 0b1111_1010, CatchAllLength.Fixed_4 },
                    new object[] { (TokenType) 0b1111_1011, CatchAllLength.Fixed_4 },
                    new object[] { (TokenType) 0b1111_1100, CatchAllLength.Fixed_8 },
                    new object[] { (TokenType) 0b1111_1101, CatchAllLength.Fixed_8 },
                    new object[] { (TokenType) 0b1111_1110, CatchAllLength.Fixed_8 },
                    new object[] { (TokenType) 0b1111_1111, CatchAllLength.Fixed_8 },
                };

                for (int i = 0; i < items.Count; i++)
                {
                    if (i != (byte) items[i][0])
                    {
                        throw new InvalidOperationException("Broken assumption - test data not valid");
                    }
                }

                // Broken value - definition of TDS_CURDECLARE3 conflicts with the packet type length rules.
                // Assume TDS_CURDECLARE3 is correct instead

                items[(byte)TokenType.TDS_CURDECLARE3][1] = CatchAllLength.Dynamic_4;

                return items;
            }
        }

        public static IEnumerable<object[]> DefinedTokenLengthTestInput
        {
            get
            {
                // Values expected as defined by each token's definition
                var items = new List<object[]>
                {
                    new object[] { TokenType.TDS_ALTFMT, CatchAllLength.Dynamic_2 },
                    new object[] { TokenType.TDS_ALTNAME, CatchAllLength.Dynamic_2 },
                    new object[] { TokenType.TDS_ALTROW, CatchAllLength.Absent },
                    new object[] { TokenType.TDS_CAPABILITY, CatchAllLength.Dynamic_2 },
                    new object[] { TokenType.TDS_COLINFO, CatchAllLength.Dynamic_2 },
                    // new object[] { TokenType.TDS_COLFMT, ? },
                    // new object[] { TokenType.TDS_COLFMTOLD, ? },
                    new object[] { TokenType.TDS_CONTROL, CatchAllLength.Dynamic_2 },
                    new object[] { TokenType.TDS_CURCLOSE, CatchAllLength.Dynamic_2 },
                    new object[] { TokenType.TDS_CURDECLARE, CatchAllLength.Dynamic_2 },
                    new object[] { TokenType.TDS_CURDECLARE2, CatchAllLength.Dynamic_4 },
                    new object[] { TokenType.TDS_CURDECLARE3, CatchAllLength.Dynamic_4 },
                    new object[] { TokenType.TDS_CURDELETE, CatchAllLength.Dynamic_2 },
                    new object[] { TokenType.TDS_CURFETCH, CatchAllLength.Dynamic_2 },
                    new object[] { TokenType.TDS_CURINFO, CatchAllLength.Dynamic_2 },
                    new object[] { TokenType.TDS_CURINFO2, CatchAllLength.Dynamic_2 },
                    new object[] { TokenType.TDS_CURINFO3, CatchAllLength.Dynamic_2 },
                    new object[] { TokenType.TDS_CUROPEN, CatchAllLength.Dynamic_2 },
                    new object[] { TokenType.TDS_CURUPDATE, CatchAllLength.Dynamic_2 },
                    new object[] { TokenType.TDS_DBRPC, CatchAllLength.Dynamic_2 },
                    new object[] { TokenType.TDS_DBRPC2, CatchAllLength.Dynamic_2 },
                    // new object[] { TokenType.TDS_DEBUGCMD, ? },
                    new object[] { TokenType.TDS_DONE, CatchAllLength.Fixed_8 },
                    new object[] { TokenType.TDS_DONEINPROC, CatchAllLength.Fixed_8 },
                    new object[] { TokenType.TDS_DONEPROC, CatchAllLength.Fixed_8 },
                    new object[] { TokenType.TDS_DYNAMIC, CatchAllLength.Dynamic_2 },
                    new object[] { TokenType.TDS_DYNAMIC2, CatchAllLength.Dynamic_4 },
                    new object[] { TokenType.TDS_EED, CatchAllLength.Dynamic_2 },
                    new object[] { TokenType.TDS_ENVCHANGE, CatchAllLength.Dynamic_2 },
                    new object[] { TokenType.TDS_ERROR, CatchAllLength.Dynamic_2 },
                    new object[] { TokenType.TDS_EVENTNOTICE, CatchAllLength.Dynamic_2 },
                    new object[] { TokenType.TDS_INFO, CatchAllLength.Dynamic_2 },
                    new object[] { TokenType.TDS_KEY, CatchAllLength.Absent },
                    new object[] { TokenType.TDS_LANGUAGE, CatchAllLength.Dynamic_4 },
                    new object[] { TokenType.TDS_LOGINACK, CatchAllLength.Dynamic_2 },
                    new object[] { TokenType.TDS_LOGOUT, CatchAllLength.Fixed_1 },
                    new object[] { TokenType.TDS_MSG, CatchAllLength.Dynamic_1 },
                    new object[] { TokenType.TDS_OFFSET, CatchAllLength.Fixed_4 },
                    new object[] { TokenType.TDS_OPTIONCMD, CatchAllLength.Dynamic_2 },
                    // new object[] { TokenType.TDS_OPTIONCMD2, ? },
                    new object[] { TokenType.TDS_ORDERBY, CatchAllLength.Dynamic_2 }, // "#Columns" = Length as it is 1:1 with the number of bytes in the packet
                    new object[] { TokenType.TDS_ORDERBY2, CatchAllLength.Dynamic_4 },
                    new object[] { TokenType.TDS_PARAMFMT, CatchAllLength.Dynamic_2 },
                    new object[] { TokenType.TDS_PARAMFMT2, CatchAllLength.Dynamic_4 },
                    new object[] { TokenType.TDS_PARAMS, CatchAllLength.Absent },
                    // new object[] { TokenType.TDS_PROCID, ? },
                    new object[] { TokenType.TDS_RPC, CatchAllLength.Dynamic_2 },
                    new object[] { TokenType.TDS_RETURNSTATUS, CatchAllLength.Fixed_4 },
                    new object[] { TokenType.TDS_RETURNVALUE, CatchAllLength.Dynamic_2 },
                    new object[] { TokenType.TDS_ROW, CatchAllLength.Absent },
                    new object[] { TokenType.TDS_ROWFMT, CatchAllLength.Dynamic_2 },
                    new object[] { TokenType.TDS_ROWFMT2, CatchAllLength.Dynamic_4 },
                    new object[] { TokenType.TDS_TABNAME, CatchAllLength.Dynamic_2 },
                };

                return items;
            }
        }
    }
}
