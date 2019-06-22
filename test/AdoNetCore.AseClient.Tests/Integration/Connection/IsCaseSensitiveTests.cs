using System;
using System.Collections.Generic;
using AdoNetCore.AseClient.Internal;
using NUnit.Framework;
// ReSharper disable NotAccessedVariable

namespace AdoNetCore.AseClient.Tests.Integration.Connection
{
    [TestFixture]
    public class IsCaseSensitiveTests
    {
        public IsCaseSensitiveTests()
        {
            Logger.Enable();
        }

        private AseConnection GetConnection()
        {
            return new AseConnection(ConnectionStrings.BigPacketSize);
        }

        [Test]
        public void AseConnection_IsCaseSensitive_Success()
        {
            using (var connection = GetConnection())
            {
                connection.Open();

                connection.IsCaseSensitive(); 

                Assert.Pass(); // Don't need to assert anything - target DBMS could be in any state. As long as it doesn't throw we're good.
            }
        }

        [TestCase(1000)]
        [TestCase(100000)]
        [TestCase(10000000)]
        [Explicit]
        public void SwitchPerformance(int numberOfIterations)
        {
            var random = new Random();

            var falseCount = 0;
            var trueCount = 0;

            for (int i = 0; i < numberOfIterations; i++)
            {
                var value = random.Next(1, 100);

                switch (value)
                {
                    case 39:
                    case 42:
                    case 44:
                    case 46:
                    case 48:
                    case 52:
                    case 53:
                    case 54:
                    case 56:
                    case 57:
                    case 59:
                    case 64:
                    case 70:
                    case 71:
                    case 73:
                    case 74:
                        falseCount++;
                        break;
                    default:
                        trueCount++;
                        break;
                }
            }
        }

        readonly HashSet<int> _hashSet = new HashSet<int>(new [] { 39, 42, 44, 46, 48, 52, 53, 54, 56, 57, 59, 64, 70, 71, 73, 74 });

        [TestCase(1000)]
        [TestCase(100000)]
        [TestCase(10000000)]
        [Explicit]
        public void HashSetPerformance(int numberOfIterations)
        {
            var random = new Random();

            var falseCount = 0;
            var trueCount = 0;

            for (int i = 0; i < numberOfIterations; i++)
            {
                var value = random.Next(1, 100);

                if (_hashSet.Contains(value))
                {
                    falseCount++;
                }
                else
                {
                    trueCount++;
                }
            }
        }

        readonly int[] _orderedArray = new[] { 39, 42, 44, 46, 48, 52, 53, 54, 56, 57, 59, 64, 70, 71, 73, 74 };

        [TestCase(1000)]
        [TestCase(100000)]
        [TestCase(10000000)]
        [Explicit]
        public void BinarySearchPerformance(int numberOfIterations)
        {
            var random = new Random();

            var falseCount = 0;
            var trueCount = 0;

            for (int i = 0; i < numberOfIterations; i++)
            {
                var value = random.Next(1, 100);

                if (Array.BinarySearch(_orderedArray, value) >=0)
                {
                    falseCount++;
                }
                else
                {
                    trueCount++;
                }
            }
        }
    }
}
