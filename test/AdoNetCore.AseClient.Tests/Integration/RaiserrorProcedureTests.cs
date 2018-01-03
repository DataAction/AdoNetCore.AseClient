using System.Collections.Generic;
using System.Data;
using Dapper;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration
{
    [TestFixture]
    public class RaiserrorProcedureTests
    {
        private readonly IDictionary<string, string> _connectionStrings = ConnectionStringLoader.Load();

        private readonly string _createProc = @"
create procedure [dbo].[sp_test_raiseerror]
as
begin
  raiserror 100000 'BAD BAD BAD'
  return null
end";

        private readonly string _dropProc = @"drop procedure [dbo].[sp_test_raiseerror]";

        [SetUp]
        public void Setup()
        {
            using (var connection = new AseConnection(_connectionStrings["default"]))
            {
                connection.Execute(_createProc);
            }
        }

        [Test]
        public void RaiserrorProcedure_ThrowsAseException()
        {
            using (var connection = new AseConnection(_connectionStrings["default"]))
            {
                var ex = Assert.Throws<AseException>(() => connection.Execute("sp_test_raiseerror", commandType: CommandType.StoredProcedure));
                var error = ex.Errors[0];
                Assert.AreEqual("BAD BAD BAD", error.Message);
                Assert.AreEqual(ex.Message, error.Message);
                Assert.AreEqual(16, error.Severity);
                Assert.AreEqual(100000, error.MessageNumber);
                Assert.AreEqual(5, error.LineNum);
                Assert.AreEqual("sp_test_raiseerror", error.ProcName);
            }
        }

        [TearDown]
        public void Teardown()
        {
            using (var connection = new AseConnection(_connectionStrings["default"]))
            {
                connection.Execute(_dropProc);
            }
        }
    }
}
