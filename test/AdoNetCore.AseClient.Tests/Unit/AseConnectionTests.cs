using System;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Unit
{
    [TestFixture]
    public class AseConnectionTests
    {
        [Test]
        public void ConstructConnection_WithInvalidParameters_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new AseConnection(""));
        }
    }
}
