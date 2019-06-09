using System.IO;
using AdoNetCore.AseClient.Internal;
using AdoNetCore.AseClient.Token;
using NUnit.Framework;

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
                t.Read(ms, new DbEnvironment(), null);

                Assert.AreEqual(0, ms.Position);
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
    }
}
