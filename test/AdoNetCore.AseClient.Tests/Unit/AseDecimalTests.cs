using System;
using System.Collections.Generic;
using System.Globalization;
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

            Assert.AreEqual(pExpected, aseDecimal.Precision, "Precision");
            Assert.AreEqual(sExpected, aseDecimal.Scale, "Scale");

            Assert.AreEqual(input < 0, aseDecimal.IsNegative, $"IsNegative should be {input < 0}");
            Assert.AreEqual(input >= 0, aseDecimal.IsPositive, $"IsPositive should be {input >= 0}");
        }

        public static IEnumerable<TestCaseData> AseDecimal_ConstructedFromDecimal_CalculatesCorrectPrecisionAndScale_Cases()
        {
            yield return new TestCaseData(0m, 1, 0);
            yield return new TestCaseData(0.1m, 2, 1);
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
            yield return new TestCaseData(1000m, 4, 0);
            yield return new TestCaseData(-1000m, 4, 0);
            yield return new TestCaseData(.0001m, 5, 4);
            yield return new TestCaseData(-.0001m, 5, 4);
            yield return new TestCaseData(1000.0001m, 8, 4);
            yield return new TestCaseData(-1000.0001m, 8, 4);
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

        [TestCaseSource(nameof(AseDecimal_CanParse_StrangerInputs_Cases))]
        public void AseDecimal_CanParse_StrangerInputs(string input, string expected, int pExpected, int sExpected)
        {
            var result = AseDecimal.Parse(input);
            var rs = result.ToString();
            Assert.AreEqual(expected, rs);
            Assert.AreEqual(pExpected, result.Precision);
            Assert.AreEqual(sExpected, result.Scale);
        }

        public static IEnumerable<TestCaseData> AseDecimal_CanParse_StrangerInputs_Cases()
        {
            yield return new TestCaseData("09", "9", 1, 0);
            yield return new TestCaseData("0.90", "0.9", 2, 1);
            yield return new TestCaseData(".90", "0.9", 2, 1);
            yield return new TestCaseData(".9", "0.9", 2, 1);
            yield return new TestCaseData("09.90", "9.9", 2, 1);
            yield return new TestCaseData("090", "90", 2, 0);
            yield return new TestCaseData("-09", "-9", 1, 0);
            yield return new TestCaseData("-0.90", "-0.9", 2, 1);
            yield return new TestCaseData("-.90", "-0.9", 2, 1);
            yield return new TestCaseData("-.9", "-0.9", 2, 1);
            yield return new TestCaseData("-09.90", "-9.9", 2, 1);
            yield return new TestCaseData("-090", "-90", 2, 0);
            yield return new TestCaseData(null, "0", 1, 0);
            yield return new TestCaseData(string.Empty, "0", 1, 0);
        }

        [TestCaseSource(nameof(AseDecimal_CanParse_Cases))]
        public void AseDecimal_CanParse(string input)
        {
            var result = AseDecimal.Parse(input);
            var rs = result.ToString();
            Console.WriteLine($"({input.Length}): {input}");
            Console.WriteLine($"({rs.Length}): {rs}");
            Console.WriteLine($"(mantissa): {result.Backing.Mantissa}");
            Console.WriteLine($"(exponent): {result.Backing.Exponent}");
            Assert.AreEqual(input, rs);
            Assert.AreEqual(input.Replace("-", string.Empty).Replace(".", string.Empty).Length, result.Precision);
        }

        public static IEnumerable<TestCaseData> AseDecimal_CanParse_Cases()
        {
            yield return new TestCaseData("90");
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
            yield return new TestCaseData("99999999999999999999999999999999999999999999999999999999999999999999999999999"); //10^77 - 1
            yield return new TestCaseData("-99999999999999999999999999999999999999999999999999999999999999999999999999999"); //-10^77 + 1
            yield return new TestCaseData("0.99999999999999999999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-0.99999999999999999999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("9.9999999999999999999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-9.9999999999999999999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("99.999999999999999999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-99.999999999999999999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("999.99999999999999999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-999.99999999999999999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("9999.9999999999999999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-9999.9999999999999999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("99999.999999999999999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-99999.999999999999999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("999999.99999999999999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-999999.99999999999999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("9999999.9999999999999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-9999999.9999999999999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("99999999.999999999999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-99999999.999999999999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("999999999.99999999999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-999999999.99999999999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("9999999999.9999999999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-9999999999.9999999999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("99999999999.999999999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-99999999999.999999999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("999999999999.99999999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-999999999999.99999999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("9999999999999.9999999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-9999999999999.9999999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("99999999999999.999999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-99999999999999.999999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("999999999999999.99999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-999999999999999.99999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("9999999999999999.9999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-9999999999999999.9999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("99999999999999999.999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-99999999999999999.999999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("999999999999999999.99999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-999999999999999999.99999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("9999999999999999999.9999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-9999999999999999999.9999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("99999999999999999999.999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-99999999999999999999.999999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("999999999999999999999.99999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-999999999999999999999.99999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("9999999999999999999999.9999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-9999999999999999999999.9999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("99999999999999999999999.999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-99999999999999999999999.999999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("999999999999999999999999.99999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-999999999999999999999999.99999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("9999999999999999999999999.9999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-9999999999999999999999999.9999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("99999999999999999999999999.999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-99999999999999999999999999.999999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("999999999999999999999999999.99999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-999999999999999999999999999.99999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("9999999999999999999999999999.9999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-9999999999999999999999999999.9999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("99999999999999999999999999999.999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-99999999999999999999999999999.999999999999999999999999999999999999999999999999");
            yield return new TestCaseData("999999999999999999999999999999.99999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-999999999999999999999999999999.99999999999999999999999999999999999999999999999");
            yield return new TestCaseData("9999999999999999999999999999999.9999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-9999999999999999999999999999999.9999999999999999999999999999999999999999999999");
            yield return new TestCaseData("99999999999999999999999999999999.999999999999999999999999999999999999999999999");
            yield return new TestCaseData("-99999999999999999999999999999999.999999999999999999999999999999999999999999999");
            yield return new TestCaseData("999999999999999999999999999999999.99999999999999999999999999999999999999999999");
            yield return new TestCaseData("-999999999999999999999999999999999.99999999999999999999999999999999999999999999");
            yield return new TestCaseData("9999999999999999999999999999999999.9999999999999999999999999999999999999999999");
            yield return new TestCaseData("-9999999999999999999999999999999999.9999999999999999999999999999999999999999999");
            yield return new TestCaseData("99999999999999999999999999999999999.999999999999999999999999999999999999999999");
            yield return new TestCaseData("-99999999999999999999999999999999999.999999999999999999999999999999999999999999");
            yield return new TestCaseData("999999999999999999999999999999999999.99999999999999999999999999999999999999999");
            yield return new TestCaseData("-999999999999999999999999999999999999.99999999999999999999999999999999999999999");
            yield return new TestCaseData("9999999999999999999999999999999999999.9999999999999999999999999999999999999999");
            yield return new TestCaseData("-9999999999999999999999999999999999999.9999999999999999999999999999999999999999");
            yield return new TestCaseData("99999999999999999999999999999999999999.999999999999999999999999999999999999999");
            yield return new TestCaseData("-99999999999999999999999999999999999999.999999999999999999999999999999999999999");
            yield return new TestCaseData("999999999999999999999999999999999999999.99999999999999999999999999999999999999");
            yield return new TestCaseData("-999999999999999999999999999999999999999.99999999999999999999999999999999999999");
            yield return new TestCaseData("9999999999999999999999999999999999999999.9999999999999999999999999999999999999");
            yield return new TestCaseData("-9999999999999999999999999999999999999999.9999999999999999999999999999999999999");
            yield return new TestCaseData("99999999999999999999999999999999999999999.999999999999999999999999999999999999");
            yield return new TestCaseData("-99999999999999999999999999999999999999999.999999999999999999999999999999999999");
            yield return new TestCaseData("999999999999999999999999999999999999999999.99999999999999999999999999999999999");
            yield return new TestCaseData("-999999999999999999999999999999999999999999.99999999999999999999999999999999999");
            yield return new TestCaseData("9999999999999999999999999999999999999999999.9999999999999999999999999999999999");
            yield return new TestCaseData("-9999999999999999999999999999999999999999999.9999999999999999999999999999999999");
            yield return new TestCaseData("99999999999999999999999999999999999999999999.999999999999999999999999999999999");
            yield return new TestCaseData("-99999999999999999999999999999999999999999999.999999999999999999999999999999999");
            yield return new TestCaseData("999999999999999999999999999999999999999999999.99999999999999999999999999999999");
            yield return new TestCaseData("-999999999999999999999999999999999999999999999.99999999999999999999999999999999");
            yield return new TestCaseData("9999999999999999999999999999999999999999999999.9999999999999999999999999999999");
            yield return new TestCaseData("-9999999999999999999999999999999999999999999999.9999999999999999999999999999999");
            yield return new TestCaseData("99999999999999999999999999999999999999999999999.999999999999999999999999999999");
            yield return new TestCaseData("-99999999999999999999999999999999999999999999999.999999999999999999999999999999");
            yield return new TestCaseData("999999999999999999999999999999999999999999999999.99999999999999999999999999999");
            yield return new TestCaseData("-999999999999999999999999999999999999999999999999.99999999999999999999999999999");
            yield return new TestCaseData("9999999999999999999999999999999999999999999999999.9999999999999999999999999999");
            yield return new TestCaseData("-9999999999999999999999999999999999999999999999999.9999999999999999999999999999");
            yield return new TestCaseData("99999999999999999999999999999999999999999999999999.999999999999999999999999999");
            yield return new TestCaseData("-99999999999999999999999999999999999999999999999999.999999999999999999999999999");
            yield return new TestCaseData("999999999999999999999999999999999999999999999999999.99999999999999999999999999");
            yield return new TestCaseData("-999999999999999999999999999999999999999999999999999.99999999999999999999999999");
            yield return new TestCaseData("9999999999999999999999999999999999999999999999999999.9999999999999999999999999");
            yield return new TestCaseData("-9999999999999999999999999999999999999999999999999999.9999999999999999999999999");
            yield return new TestCaseData("99999999999999999999999999999999999999999999999999999.999999999999999999999999");
            yield return new TestCaseData("-99999999999999999999999999999999999999999999999999999.999999999999999999999999");
            yield return new TestCaseData("999999999999999999999999999999999999999999999999999999.99999999999999999999999");
            yield return new TestCaseData("-999999999999999999999999999999999999999999999999999999.99999999999999999999999");
            yield return new TestCaseData("9999999999999999999999999999999999999999999999999999999.9999999999999999999999");
            yield return new TestCaseData("-9999999999999999999999999999999999999999999999999999999.9999999999999999999999");
            yield return new TestCaseData("99999999999999999999999999999999999999999999999999999999.999999999999999999999");
            yield return new TestCaseData("-99999999999999999999999999999999999999999999999999999999.999999999999999999999");
            yield return new TestCaseData("999999999999999999999999999999999999999999999999999999999.99999999999999999999");
            yield return new TestCaseData("-999999999999999999999999999999999999999999999999999999999.99999999999999999999");
            yield return new TestCaseData("9999999999999999999999999999999999999999999999999999999999.9999999999999999999");
            yield return new TestCaseData("-9999999999999999999999999999999999999999999999999999999999.9999999999999999999");
            yield return new TestCaseData("99999999999999999999999999999999999999999999999999999999999.999999999999999999");
            yield return new TestCaseData("-99999999999999999999999999999999999999999999999999999999999.999999999999999999");
            yield return new TestCaseData("999999999999999999999999999999999999999999999999999999999999.99999999999999999");
            yield return new TestCaseData("-999999999999999999999999999999999999999999999999999999999999.99999999999999999");
            yield return new TestCaseData("9999999999999999999999999999999999999999999999999999999999999.9999999999999999");
            yield return new TestCaseData("-9999999999999999999999999999999999999999999999999999999999999.9999999999999999");
            yield return new TestCaseData("99999999999999999999999999999999999999999999999999999999999999.999999999999999");
            yield return new TestCaseData("-99999999999999999999999999999999999999999999999999999999999999.999999999999999");
            yield return new TestCaseData("999999999999999999999999999999999999999999999999999999999999999.99999999999999");
            yield return new TestCaseData("-999999999999999999999999999999999999999999999999999999999999999.99999999999999");
            yield return new TestCaseData("9999999999999999999999999999999999999999999999999999999999999999.9999999999999");
            yield return new TestCaseData("-9999999999999999999999999999999999999999999999999999999999999999.9999999999999");
            yield return new TestCaseData("99999999999999999999999999999999999999999999999999999999999999999.999999999999");
            yield return new TestCaseData("-99999999999999999999999999999999999999999999999999999999999999999.999999999999");
            yield return new TestCaseData("999999999999999999999999999999999999999999999999999999999999999999.99999999999");
            yield return new TestCaseData("-999999999999999999999999999999999999999999999999999999999999999999.99999999999");
            yield return new TestCaseData("9999999999999999999999999999999999999999999999999999999999999999999.9999999999");
            yield return new TestCaseData("-9999999999999999999999999999999999999999999999999999999999999999999.9999999999");
            yield return new TestCaseData("99999999999999999999999999999999999999999999999999999999999999999999.999999999");
            yield return new TestCaseData("-99999999999999999999999999999999999999999999999999999999999999999999.999999999");
            yield return new TestCaseData("999999999999999999999999999999999999999999999999999999999999999999999.99999999");
            yield return new TestCaseData("-999999999999999999999999999999999999999999999999999999999999999999999.99999999");
            yield return new TestCaseData("9999999999999999999999999999999999999999999999999999999999999999999999.9999999");
            yield return new TestCaseData("-9999999999999999999999999999999999999999999999999999999999999999999999.9999999");
            yield return new TestCaseData("99999999999999999999999999999999999999999999999999999999999999999999999.999999");
            yield return new TestCaseData("-99999999999999999999999999999999999999999999999999999999999999999999999.999999");
            yield return new TestCaseData("999999999999999999999999999999999999999999999999999999999999999999999999.99999");
            yield return new TestCaseData("-999999999999999999999999999999999999999999999999999999999999999999999999.99999");
            yield return new TestCaseData("9999999999999999999999999999999999999999999999999999999999999999999999999.9999");
            yield return new TestCaseData("-9999999999999999999999999999999999999999999999999999999999999999999999999.9999");
            yield return new TestCaseData("99999999999999999999999999999999999999999999999999999999999999999999999999.999");
            yield return new TestCaseData("-99999999999999999999999999999999999999999999999999999999999999999999999999.999");
            yield return new TestCaseData("999999999999999999999999999999999999999999999999999999999999999999999999999.99");
            yield return new TestCaseData("-999999999999999999999999999999999999999999999999999999999999999999999999999.99");
            yield return new TestCaseData("9999999999999999999999999999999999999999999999999999999999999999999999999999.9");
            yield return new TestCaseData("-9999999999999999999999999999999999999999999999999999999999999999999999999999.9");
        }

        [TestCaseSource(nameof(AseDecimal_ToString_Cases))]
        public void AseDecimal_ToString(decimal input)
        {
            Assert.AreEqual(input.ToString(CultureInfo.InvariantCulture), new AseDecimal(input).ToString());
        }

        public static IEnumerable<TestCaseData> AseDecimal_ToString_Cases()
        {
            yield return new TestCaseData(0m);
            yield return new TestCaseData(1m);
            yield return new TestCaseData(-1m);
            yield return new TestCaseData(0.1m);
            yield return new TestCaseData(-0.1m);
            yield return new TestCaseData(0.0000001m);
            yield return new TestCaseData(-0.0000001m);
            yield return new TestCaseData(9.999999m);
            yield return new TestCaseData(-9.999999m);
            yield return new TestCaseData(99.9m);
            yield return new TestCaseData(-99.9m);
            yield return new TestCaseData(99.99m);
            yield return new TestCaseData(-99.99m);
            yield return new TestCaseData(1000m);
            yield return new TestCaseData(-1000m);
            yield return new TestCaseData(1000.0001m);
            yield return new TestCaseData(-1000.0001m);
        }

        [TestCaseSource(nameof(AseDecimal_GetBytes_Cases))]
        public void AseDecimal_GetBytes(AseDecimal input, byte[] expected)
        {
            Assert.AreEqual(expected, input.BinData);
        }

        public static IEnumerable<TestCaseData> AseDecimal_GetBytes_Cases()
        {
            yield return new TestCaseData(new AseDecimal(1m), new byte[] { 1 });
            yield return new TestCaseData(new AseDecimal(-1m), new byte[] { 1 }); //the sign is handled elsewhere
        }

        [Test]
        public void AseDecimal_Round_NegativePosition_ThrowsAseException()
        {
            var ex = Assert.Throws<AseException>(() => AseDecimal.Round(new AseDecimal(), -1));
            Assert.AreEqual("Invalid value.", ex.Message);
            Assert.AreEqual(30037, ex.Errors[0].MessageNumber);
        }

        [Test]
        public void AseDecimal_RoundFloorEquivalent()
        {
            Assert.AreEqual(AseDecimal.Zero, AseDecimal.Round(new AseDecimal(0.1m), 0));
            Assert.AreEqual(AseDecimal.Zero, AseDecimal.Round(new AseDecimal(0.123m), 0));
            Assert.AreEqual(new AseDecimal(0.1m), AseDecimal.Round(new AseDecimal(0.123m), 1));
        }

        [TestCaseSource(nameof(AseDecimal_Round_Cases))]
        public void AseDecimal_Round(string input, string expected)
        {
            var result = AseDecimal.Round(AseDecimal.Parse(input), 1);
            var parsedExpected = AseDecimal.Parse(expected);

            Assert.AreEqual(parsedExpected, result);
        }

        public static IEnumerable<TestCaseData> AseDecimal_Round_Cases()
        {
            yield return new TestCaseData("5.5111", "5.5");
            yield return new TestCaseData("5.5222", "5.5");
            yield return new TestCaseData("5.5333", "5.5");
            yield return new TestCaseData("5.5444", "5.5");
            yield return new TestCaseData("5.5555", "5.6");
            yield return new TestCaseData("5.5666", "5.6");
            yield return new TestCaseData("5.5777", "5.6");
            yield return new TestCaseData("5.5888", "5.6");
            yield return new TestCaseData("5.5999", "5.6");
            yield return new TestCaseData("4.5111", "4.5");
            yield return new TestCaseData("4.5222", "4.5");
            yield return new TestCaseData("4.5333", "4.5");
            yield return new TestCaseData("4.5444", "4.5");
            yield return new TestCaseData("4.5555", "4.6");
            yield return new TestCaseData("4.5666", "4.6");
            yield return new TestCaseData("4.5777", "4.6");
            yield return new TestCaseData("4.5888", "4.6");
            yield return new TestCaseData("4.5999", "4.6");
            yield return new TestCaseData("-5.5111", "-5.5");
            yield return new TestCaseData("-5.5222", "-5.5");
            yield return new TestCaseData("-5.5333", "-5.5");
            yield return new TestCaseData("-5.5444", "-5.5");
            yield return new TestCaseData("-5.5555", "-5.6");
            yield return new TestCaseData("-5.5666", "-5.6");
            yield return new TestCaseData("-5.5777", "-5.6");
            yield return new TestCaseData("-5.5888", "-5.6");
            yield return new TestCaseData("-5.5999", "-5.6");
            yield return new TestCaseData("-4.5111", "-4.5");
            yield return new TestCaseData("-4.5222", "-4.5");
            yield return new TestCaseData("-4.5333", "-4.5");
            yield return new TestCaseData("-4.5444", "-4.5");
            yield return new TestCaseData("-4.5555", "-4.6");
            yield return new TestCaseData("-4.5666", "-4.6");
            yield return new TestCaseData("-4.5777", "-4.6");
            yield return new TestCaseData("-4.5888", "-4.6");
            yield return new TestCaseData("-4.5999", "-4.6");
        }

        [Test]
        public void AseDecimal_Truncate_NegativePosition_ThrowsAseException()
        {
            var ex = Assert.Throws<AseException>(() => AseDecimal.Truncate(new AseDecimal(), -1));
            Assert.AreEqual("Invalid value.", ex.Message);
            Assert.AreEqual(30037, ex.Errors[0].MessageNumber);
        }

        [TestCaseSource(nameof(AseDecimal_Truncate_Cases))]
        public void AseDecimal_Truncate(string input, string expected)
        {
            var result = AseDecimal.Truncate(AseDecimal.Parse(input), 1);
            var parsedExpected = AseDecimal.Parse(expected);

            Assert.AreEqual(parsedExpected, result);
        }

        public static IEnumerable<TestCaseData> AseDecimal_Truncate_Cases()
        {
            yield return new TestCaseData("0.5", "0.5");
            yield return new TestCaseData("5.5555", "5.5");
            yield return new TestCaseData("5.5444", "5.5");
            yield return new TestCaseData("5.5666", "5.5");
            yield return new TestCaseData("-5.5555", "-5.5");
            yield return new TestCaseData("-5.5444", "-5.5");
            yield return new TestCaseData("-5.5666", "-5.5");
        }

        [Test]
        public void AseDecimal_Truncate_FloorEquivalent()
        {
            Assert.AreEqual(AseDecimal.Zero, AseDecimal.Truncate(AseDecimal.Parse("0.1"), 0));
            Assert.AreEqual(AseDecimal.Zero, AseDecimal.Truncate(AseDecimal.Parse("0.123"), 0));
            Assert.AreEqual(AseDecimal.Parse("0.1"), AseDecimal.Truncate(AseDecimal.Parse("0.123"), 1));
        }

        [TestCaseSource(nameof(AseDecimal_Floor_Cases))]
        public void AseDecimal_Floor(string input, string expected)
        {
            var result = AseDecimal.Floor(AseDecimal.Parse(input));
            var parsedExpected = AseDecimal.Parse(expected);

            Assert.AreEqual(parsedExpected, result);
        }

        public static IEnumerable<TestCaseData> AseDecimal_Floor_Cases()
        {
            yield return new TestCaseData("0.1", "0");
            yield return new TestCaseData("5.5555", "5");
            yield return new TestCaseData("5.5444", "5");
            yield return new TestCaseData("5.5666", "5");
            yield return new TestCaseData("-5.5555", "-6");
            yield return new TestCaseData("-5.5444", "-6");
            yield return new TestCaseData("-5.5666", "-6");
        }

        [TestCaseSource(nameof(AseDecimal_ToAseDecimal_Cases))]
        public void AseDecimal_ToAseDecimal(string input, int p, int s, string expected)
        {
            var result = AseDecimal.Parse(input).ToAseDecimal(p, s);
            var parsedExpected = AseDecimal.Parse(expected);

            Console.WriteLine($"expected: {parsedExpected}, result: {result}");
            Assert.AreEqual(parsedExpected, result);
        }

        public static IEnumerable<TestCaseData> AseDecimal_ToAseDecimal_Cases()
        {
            yield return new TestCaseData("5", 1, 0, "5");
            yield return new TestCaseData("5", 2, 0, "50");
            yield return new TestCaseData("5", 2, 1, "5");

            yield return new TestCaseData("5.1", 2, 1, "5.1");
            yield return new TestCaseData("5.9", 2, 1, "5.9");
            yield return new TestCaseData("5.1", 1, 0, "0"); //?
            yield return new TestCaseData("5.9", 1, 0, "0"); //?

            yield return new TestCaseData("55.9", 3, 1, "55.9");
            yield return new TestCaseData("55.9", 2, 1, "5.5");
            yield return new TestCaseData("55.9", 1, 0, "0"); //?

            yield return new TestCaseData("5", 1, 0, "5");
            yield return new TestCaseData("55", 1, 0, "5");
            yield return new TestCaseData("5.1", 1, 0, "0");
            yield return new TestCaseData("5.5", 1, 0, "0");
            yield return new TestCaseData("5.1", 2, 1, "5.1");
            yield return new TestCaseData("55.1", 1, 0, "0");
            yield return new TestCaseData("55.1", 2, 0, "5");
            yield return new TestCaseData("55.1", 2, 1, "5.5");
            yield return new TestCaseData("56.1", 2, 1, "5.6");
            yield return new TestCaseData("5.5555", 5, 0, "5");
            yield return new TestCaseData("5.5444", 5, 0, "5");
            yield return new TestCaseData("5.5666", 5, 0, "5");
            yield return new TestCaseData("5.5555", 5, 1, "5.5");
            yield return new TestCaseData("5.5444", 5, 1, "5.5");
            yield return new TestCaseData("5.5666", 5, 1, "5.5");
            yield return new TestCaseData("5.5555", 5, 3, "5.555");
            yield return new TestCaseData("5.5444", 5, 3, "5.544");
            yield return new TestCaseData("5.5666", 5, 3, "5.566");
            yield return new TestCaseData("-5.5555", 5, 0, "-5");
            yield return new TestCaseData("-5.5444", 5, 0, "-5");
            yield return new TestCaseData("-5.5666", 5, 0, "-5");
            yield return new TestCaseData("-5.5555", 5, 1, "-5.5");
            yield return new TestCaseData("-5.5444", 5, 1, "-5.5");
            yield return new TestCaseData("-5.5666", 5, 1, "-5.5");
            yield return new TestCaseData("-5.5555", 5, 3, "-5.555");
            yield return new TestCaseData("-5.5444", 5, 3, "-5.544");
            yield return new TestCaseData("-5.5666", 5, 3, "-5.566");
        }

        [TestCaseSource(nameof(AseDecimal_ParseInvalidInput_ThrowsFormatException_Cases))]
        public void AseDecimal_ParseInvalidInput_ThrowsFormatException(string input)
        {
            Assert.Throws<FormatException>(() => AseDecimal.Parse(input));
        }

        public static IEnumerable<TestCaseData> AseDecimal_ParseInvalidInput_ThrowsFormatException_Cases()
        {
            yield return new TestCaseData("-a");
            yield return new TestCaseData("bbb");
        }
    }
}
