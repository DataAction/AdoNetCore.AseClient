using System;
using System.Collections.Generic;
using System.IO;
using AdoNetCore.AseClient.Internal;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Unit
{
    public class StreamReadTests
    {
        [TestCaseSource(nameof(ReadBigTime_Succeeds_Cases))]
        public void ReadBigTime_Succeeds(string _, long microsecondsSinceMidnight, DateTime expected)
        {
            using (var ms = new MemoryStream(BitConverter.GetBytes(microsecondsSinceMidnight)))
            {
                Assert.AreEqual(expected, ms.ReadBigTime());
            }
        }

        public static IEnumerable<TestCaseData> ReadBigTime_Succeeds_Cases()
        {
            var day = new DateTime(1900, 1, 1);
            yield return new TestCaseData("midnight", 0L, day);
            yield return new TestCaseData("one_microsecond", 1L, day.AddTicks(10));
            yield return new TestCaseData("13:45:30.123456", 49530123456L, day.AddHours(13).AddMinutes(45).AddSeconds(30).AddTicks(1234560));
            yield return new TestCaseData("last_microsecond_of_day", 86399999999L, day.AddDays(1).AddTicks(-10));
        }
    }
}
