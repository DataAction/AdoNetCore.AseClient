using System;
using System.Collections.Generic;
using System.Text;
using Dapper;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration
{
    [TestFixture]
    [Category("extra")]
    public class TextAllocatorProcedureTests
    {
        private readonly string _createTextLocatorProc = @"
create procedure [dbo].[sp_test_true_echo_text_locator]
  @input text_locator,
  @output int output
as
begin
  set @output = 100
end";

        private readonly string _dropTextLocatorProc = @"drop procedure [dbo].[sp_test_true_echo_text_locator]";

        [SetUp]
        public void Setup()
        {
            using (var connection = new AseConnection(ConnectionStrings.Pooled))
            {
                //connection.Execute(_dropTextLocatorProc);

                connection.Execute(_createTextLocatorProc);
            }
        }

        [Test]
        public void AssertTrue()
        {
            Assert.IsTrue(true);
        }

        [TearDown]
        public void Teardown()
        {
            using (var connection = new AseConnection(ConnectionStrings.Pooled))
            {
                connection.Execute(_dropTextLocatorProc);
            }
        }

    }
}
