using System;
using System.Collections.Generic;
using System.IO;
using AdoNetCore.AseClient.Internal;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Unit
{
    public class StreamWriteReadTests
    {
        [TestCaseSource(nameof(WriteReadBigDateTime_Succeeds_Cases))]
        public void WriteReadBigDateTime_Succeeds(string _, DateTime value, byte[] expected)
        {
            using (var ms = new MemoryStream())
            {
                ms.WriteBigDateTime(value);
                ms.Seek(0, SeekOrigin.Begin);
                Assert.AreEqual(BitConverter.ToInt64(expected, 0), BitConverter.ToInt64(ms.ToArray(), 1));
                ms.Seek(1, SeekOrigin.Begin);
                var readBack = ms.ReadBigDateTime();
                Assert.AreEqual(value, readBack);
            }
        }

        public static IEnumerable<TestCaseData> WriteReadBigDateTime_Succeeds_Cases()
        {
            yield return new TestCaseData("0001_1", new DateTime(0001, 01, 01, 0, 0, 0, 0), new byte[] { 0x00, 0x40, 0xEB, 0xA9, 0xC2, 0x1C, 0x00, 0x00 });
            yield return new TestCaseData("0001_2", new DateTime(0001, 01, 01, 0, 0, 0, 1), new byte[] { 0xE8, 0x43, 0xEB, 0xA9, 0xC2, 0x1C, 0x00, 0x00 });
            yield return new TestCaseData("0001_3", new DateTime(0001, 01, 01, 0, 0, 0, 2), new byte[] { 0xD0, 0x47, 0xEB, 0xA9, 0xC2, 0x1C, 0x00, 0x00 });
            yield return new TestCaseData("0001_4", new DateTime(0001, 01, 01, 0, 0, 0, 3), new byte[] { 0xB8, 0x4B, 0xEB, 0xA9, 0xC2, 0x1C, 0x00, 0x00 });
            yield return new TestCaseData("0001_5", new DateTime(0001, 01, 01, 0, 0, 0, 4), new byte[] { 0xA0, 0x4F, 0xEB, 0xA9, 0xC2, 0x1C, 0x00, 0x00 });
            yield return new TestCaseData("0001_6", new DateTime(0001, 01, 01, 0, 0, 0, 5), new byte[] { 0x88, 0x53, 0xEB, 0xA9, 0xC2, 0x1C, 0x00, 0x00 });
            yield return new TestCaseData("1753_1", new DateTime(1753, 1, 1, 0, 0, 0, 0, 0), new byte[] { 0x00, 0xA0, 0x7E, 0xDC, 0xB6, 0x88, 0xC4, 0x00 });
            yield return new TestCaseData("1900_1", new DateTime(1900, 1, 1, 0, 0, 0, 0, 0), new byte[] { 0x00, 0x60, 0x5A, 0x60, 0xB1, 0x03, 0xD5, 0x00 });
            yield return new TestCaseData("1900_2", new DateTime(1900, 1, 1, 23, 59, 59, 999), new byte[] { 0x18, 0xBC, 0x31, 0x7E, 0xC5, 0x03, 0xD5, 0x00 });
            // .Net ticks are 1/10 us, so we must lose the last digit of number of ticks
            yield return new TestCaseData("1900_3", new DateTime(1900, 01, 01).Add(TimeSpan.FromHours(24).Add(TimeSpan.FromTicks(-10))), new byte[] { 0xFF, 0xBF, 0x31, 0x7E, 0xC5, 0x03, 0xD5, 0x00 });
            yield return new TestCaseData("9999_1", new DateTime(9999, 1, 1, 0, 0, 0, 0, 0), new byte[] { 0x00, 0x80, 0x76, 0xE9, 0x1F, 0x04, 0x61, 0x04 });
        }
    }
}
