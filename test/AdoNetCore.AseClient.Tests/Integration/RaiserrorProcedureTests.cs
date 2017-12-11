using System.Collections.Generic;
using System.Data;
using System.IO;
using Dapper;
using Newtonsoft.Json;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration
{
    [TestFixture]
    public class RaiserrorProcedureTests
    {
        private readonly Dictionary<string, string> _connectionStrings = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("ConnectionStrings.json"));

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
                Assert.AreEqual("[16] BAD BAD BAD", ex.Message);
                Assert.AreEqual(100000, ex.Errors[0].MessageNumber);
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
