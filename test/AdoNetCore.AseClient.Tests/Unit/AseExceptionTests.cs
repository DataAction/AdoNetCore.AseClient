using System.Data.Common;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Unit
{
    public class AseExceptionTests
    {
        // DbException: "The base class for all exceptions thrown on behalf of the data source."
        [Test]
        public void AseException_IsADbException()
        {
            Assert.IsInstanceOf<DbException>(new AseException("error", 1));
        }
    }
}
