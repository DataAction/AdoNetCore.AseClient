using System;
using System.Collections.Generic;
using AdoNetCore.AseClient.Internal;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Unit
{
    [TestFixture]
    public class AseDecimalTests
    {
        public AseDecimalTests()
        {
            Logger.Enable();
        }

        [TestCaseSource(nameof(AseDecimal_ConstructedFromDecimal_CalculatesCorrectPrecisionAndScale_Cases))]
        public void AseDecimal_ConstructedFromDecimal_CalculatesCorrectPrecisionAndScale(decimal input, int pExpected, int sExpected)
        {
            var aseDecimal = new AseDecimal(input);

            Assert.AreEqual(pExpected, aseDecimal.Precision);
            Assert.AreEqual(sExpected, aseDecimal.Scale);
            
            Assert.AreEqual(input < 0, aseDecimal.IsNegative, $"IsNegative should be {input < 0}");
            Assert.AreEqual(input >= 0, aseDecimal.IsPositive, $"IsPositive should be {input >= 0}");
        }

        public static IEnumerable<TestCaseData> AseDecimal_ConstructedFromDecimal_CalculatesCorrectPrecisionAndScale_Cases()
        {
            yield return new TestCaseData(0m, 1, 0);
            yield return new TestCaseData(9m, 1, 0);
            yield return new TestCaseData(-9m, 1, 0);
            yield return new TestCaseData(9.99m, 3, 2);
            yield return new TestCaseData(-9.99m, 3, 2);
            yield return new TestCaseData(99m, 2, 0);
            yield return new TestCaseData(-99m, 2, 0);
            yield return new TestCaseData(99.9m, 3, 1);
            yield return new TestCaseData(-99.9m, 3, 1);
            yield return new TestCaseData(999m, 3, 0);
            yield return new TestCaseData(-999m, 3, 0);
            yield return new TestCaseData(decimal.MaxValue, 29, 0);
            yield return new TestCaseData(decimal.MinValue, 29, 0);
            yield return new TestCaseData(decimal.MaxValue / 10m, 29, 1);
            yield return new TestCaseData(decimal.MinValue / 10m, 29, 1);
            yield return new TestCaseData(decimal.MaxValue / 100m, 29, 2);
            yield return new TestCaseData(decimal.MinValue / 100m, 29, 2);
            yield return new TestCaseData(decimal.MaxValue / 1000m, 29, 3);
            yield return new TestCaseData(decimal.MinValue / 1000m, 29, 3);
            yield return new TestCaseData(decimal.MaxValue / 10000m, 29, 4);
            yield return new TestCaseData(decimal.MinValue / 10000m, 29, 4);
            yield return new TestCaseData(decimal.MaxValue / 100000m, 29, 5);
            yield return new TestCaseData(decimal.MinValue / 100000m, 29, 5);
            yield return new TestCaseData(decimal.MaxValue / 1000000m, 29, 6);
            yield return new TestCaseData(decimal.MinValue / 1000000m, 29, 6);
            yield return new TestCaseData(decimal.MaxValue / 10000000m, 29, 7);
            yield return new TestCaseData(decimal.MinValue / 10000000m, 29, 7);
            yield return new TestCaseData(decimal.MaxValue / 100000000m, 29, 8);
            yield return new TestCaseData(decimal.MinValue / 100000000m, 29, 8);
            yield return new TestCaseData(decimal.MaxValue / 1000000000m, 29, 9);
            yield return new TestCaseData(decimal.MinValue / 1000000000m, 29, 9);
            yield return new TestCaseData(decimal.MaxValue / 10000000000m, 29, 10);
            yield return new TestCaseData(decimal.MinValue / 10000000000m, 29, 10);
            yield return new TestCaseData(decimal.MaxValue / 100000000000m, 29, 11);
            yield return new TestCaseData(decimal.MinValue / 100000000000m, 29, 11);
            yield return new TestCaseData(decimal.MaxValue / 1000000000000m, 29, 12);
            yield return new TestCaseData(decimal.MinValue / 1000000000000m, 29, 12);
            yield return new TestCaseData(decimal.MaxValue / 10000000000000m, 29, 13);
            yield return new TestCaseData(decimal.MinValue / 10000000000000m, 29, 13);
            yield return new TestCaseData(decimal.MaxValue / 100000000000000m, 29, 14);
            yield return new TestCaseData(decimal.MinValue / 100000000000000m, 29, 14);
            yield return new TestCaseData(decimal.MaxValue / 1000000000000000m, 29, 15);
            yield return new TestCaseData(decimal.MinValue / 1000000000000000m, 29, 15);
            yield return new TestCaseData(decimal.MaxValue / 10000000000000000m, 29, 16);
            yield return new TestCaseData(decimal.MinValue / 10000000000000000m, 29, 16);
            yield return new TestCaseData(decimal.MaxValue / 100000000000000000m, 29, 17);
            yield return new TestCaseData(decimal.MinValue / 100000000000000000m, 29, 17);
            yield return new TestCaseData(decimal.MaxValue / 1000000000000000000m, 29, 18);
            yield return new TestCaseData(decimal.MinValue / 1000000000000000000m, 29, 18);
            yield return new TestCaseData(decimal.MaxValue / 10000000000000000000m, 29, 19);
            yield return new TestCaseData(decimal.MinValue / 10000000000000000000m, 29, 19);
            yield return new TestCaseData(decimal.MaxValue / 100000000000000000000m, 29, 20);
            yield return new TestCaseData(decimal.MinValue / 100000000000000000000m, 29, 20);
            yield return new TestCaseData(decimal.MaxValue / 1000000000000000000000m, 29, 21);
            yield return new TestCaseData(decimal.MinValue / 1000000000000000000000m, 29, 21);
            yield return new TestCaseData(decimal.MaxValue / 10000000000000000000000m, 29, 22);
            yield return new TestCaseData(decimal.MinValue / 10000000000000000000000m, 29, 22);
            yield return new TestCaseData(decimal.MaxValue / 100000000000000000000000m, 29, 23);
            yield return new TestCaseData(decimal.MinValue / 100000000000000000000000m, 29, 23);
            yield return new TestCaseData(decimal.MaxValue / 1000000000000000000000000m, 29, 24);
            yield return new TestCaseData(decimal.MinValue / 1000000000000000000000000m, 29, 24);
            yield return new TestCaseData(decimal.MaxValue / 10000000000000000000000000m, 29, 25);
            yield return new TestCaseData(decimal.MinValue / 10000000000000000000000000m, 29, 25);
            yield return new TestCaseData(decimal.MaxValue / 100000000000000000000000000m, 29, 26);
            yield return new TestCaseData(decimal.MinValue / 100000000000000000000000000m, 29, 26);
            yield return new TestCaseData(decimal.MaxValue / 1000000000000000000000000000m, 29, 27);
            yield return new TestCaseData(decimal.MinValue / 1000000000000000000000000000m, 29, 27);
            yield return new TestCaseData(decimal.MaxValue / 10000000000000000000000000000m, 29, 28);
            yield return new TestCaseData(decimal.MinValue / 10000000000000000000000000000m, 29, 28);
        }

        [TestCaseSource(nameof(AseDecimal_CanParse_Cases))]
        public void AseDecimal_CanParse(string input)
        {
            var result = AseDecimal.Parse(input);
            var rs = result.ToString();
            Console.WriteLine($"({input.Length}): {input}");
            Console.WriteLine($"({rs.Length}): {rs}");
            Assert.AreEqual(input, rs);
            Assert.AreEqual(input.Replace("-", string.Empty).Replace(".", string.Empty).Length, result.Precision);
        }

        public static IEnumerable<TestCaseData> AseDecimal_CanParse_Cases()
        {
            yield return new TestCaseData("9");
            yield return new TestCaseData("-9");
            yield return new TestCaseData("99");
            yield return new TestCaseData("-99");
            yield return new TestCaseData("999");
            yield return new TestCaseData("-999");
            yield return new TestCaseData("9999");
            yield return new TestCaseData("-9999");
            yield return new TestCaseData("99999");
            yield return new TestCaseData("-99999");
            yield return new TestCaseData("999999");
            yield return new TestCaseData("-999999");
            yield return new TestCaseData("9999999");
            yield return new TestCaseData("-9999999");
            yield return new TestCaseData("99999999");
            yield return new TestCaseData("-99999999");
            yield return new TestCaseData("999999999");
            yield return new TestCaseData("-999999999");
            yield return new TestCaseData("9999999999");
            yield return new TestCaseData("-9999999999");
            yield return new TestCaseData("99999999999");
            yield return new TestCaseData("-99999999999");
            yield return new TestCaseData("999999999999");
            yield return new TestCaseData("-999999999999");
            yield return new TestCaseData("9999999999999");
            yield return new TestCaseData("-9999999999999");
            yield return new TestCaseData("99999999999999");
            yield return new TestCaseData("-99999999999999");
            yield return new TestCaseData("999999999999999");
            yield return new TestCaseData("-999999999999999");
            yield return new TestCaseData("9999999999999999");
            yield return new TestCaseData("-9999999999999999");
            yield return new TestCaseData("99999999999999999");
            yield return new TestCaseData("-99999999999999999");
            yield return new TestCaseData("999999999999999999");
            yield return new TestCaseData("-999999999999999999");
            yield return new TestCaseData("9999999999999999999");
            yield return new TestCaseData("-9999999999999999999");
            yield return new TestCaseData("99999999999999999999");
            yield return new TestCaseData("-99999999999999999999");
            yield return new TestCaseData("999999999999999999999");
            yield return new TestCaseData("-999999999999999999999");
            yield return new TestCaseData("9999999999999999999999");
            yield return new TestCaseData("-9999999999999999999999");
            yield return new TestCaseData("99999999999999999999999");
            yield return new TestCaseData("-99999999999999999999999");
            yield return new TestCaseData("999999999999999999999999");
            yield return new TestCaseData("-999999999999999999999999");
            yield return new TestCaseData("9999999999999999999999999");
            yield return new TestCaseData("-9999999999999999999999999");
            yield return new TestCaseData("99999999999999999999999999");
            yield return new TestCaseData("-99999999999999999999999999");
            yield return new TestCaseData("999999999999999999999999999");
            yield return new TestCaseData("-999999999999999999999999999");
            yield return new TestCaseData("9999999999999999999999999999");
            yield return new TestCaseData("-9999999999999999999999999999");
            yield return new TestCaseData("99999999999999999999999999999");
            yield return new TestCaseData("-99999999999999999999999999999");
            yield return new TestCaseData("999999999999999999999999999999");
            yield return new TestCaseData("-999999999999999999999999999999");
            yield return new TestCaseData("9999999999999999999999999999999");
            yield return new TestCaseData("-9999999999999999999999999999999");
            yield return new TestCaseData("99999999999999999999999999999999");
            yield return new TestCaseData("-99999999999999999999999999999999");
            yield return new TestCaseData("999999999999999999999999999999999");
            yield return new TestCaseData("-999999999999999999999999999999999");
            yield return new TestCaseData("9999999999999999999999999999999999");
            yield return new TestCaseData("-9999999999999999999999999999999999");
            yield return new TestCaseData("99999999999999999999999999999999999");
            yield return new TestCaseData("-99999999999999999999999999999999999");
            yield return new TestCaseData("999999999999999999999999999999999999");
            yield return new TestCaseData("-999999999999999999999999999999999999");
            yield return new TestCaseData("9999999999999999999999999999999999999");
            yield return new TestCaseData("-9999999999999999999999999999999999999");
            yield return new TestCaseData("99999999999999999999999999999999999999");
            yield return new TestCaseData("-99999999999999999999999999999999999999");
            yield return new TestCaseData("999999999999999999999999999999999999999");
            yield return new TestCaseData("-999999999999999999999999999999999999999");
            yield return new TestCaseData("9999999999999999999999999999999999999999");
            yield return new TestCaseData("-9999999999999999999999999999999999999999");
            yield return new TestCaseData("99999999999999999999999999999999999999999");
            yield return new TestCaseData("-99999999999999999999999999999999999999999");
            yield return new TestCaseData("999999999999999999999999999999999999999999");
            yield return new TestCaseData("-999999999999999999999999999999999999999999");
            yield return new TestCaseData("9999999999999999999999999999999999999999999");
            yield return new TestCaseData("-9999999999999999999999999999999999999999999");
            yield return new TestCaseData("99999999999999999999999999999999999999999999");
            yield return new TestCaseData("-99999999999999999999999999999999999999999999");
            yield return new TestCaseData("999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-999999999999999999999999999999999999999999999");
            yield return new TestCaseData("9999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-9999999999999999999999999999999999999999999999");
            yield return new TestCaseData("99999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-99999999999999999999999999999999999999999999999");
            yield return new TestCaseData("999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("9999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-9999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("99999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-99999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("9999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-9999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("99999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-99999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("9999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-9999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("99999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-99999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("9999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-9999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("99999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-99999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("9999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-9999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("99999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-99999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("999999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-999999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("9999999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-9999999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("99999999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-99999999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("999999999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-999999999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("9999999999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-9999999999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("99999999999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-99999999999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("999999999999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-999999999999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("9999999999999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-9999999999999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("99999999999999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-99999999999999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("999999999999999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-999999999999999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("9999999999999999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-9999999999999999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("99999999999999999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-99999999999999999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("999999999999999999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-999999999999999999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("9999999999999999999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-9999999999999999999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("99999999999999999999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-99999999999999999999999999999999999999999999999999999999999999999999999999999");
        }
    }
}
