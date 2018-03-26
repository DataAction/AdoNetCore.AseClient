using System.Data;
using Dapper;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration
{
    [TestFixture]
    [Category("basic")]
    public class SimpleProcedureTests
    {
        private readonly string _createProc = @"create procedure [dbo].[sp_test_simple] as begin return end";
        private readonly string _dropProc = @"drop procedure [dbo].[sp_test_simple]";

        [SetUp]
        public void Setup()
        {
            using (var connection = new AseConnection(ConnectionStrings.Default))
            {
                connection.Execute(_createProc);
            }
        }

        [Test]
        public void Simple_Procedure_ShouldExecute()
        {
            using (var connection = new AseConnection(ConnectionStrings.Default))
            {
                connection.Execute("sp_test_simple", commandType: CommandType.StoredProcedure);
            }
        }

        [TearDown]
        public void Teardown()
        {
            using (var connection = new AseConnection(ConnectionStrings.Default))
            {
                connection.Execute(_dropProc);
            }
        }
    }
}
