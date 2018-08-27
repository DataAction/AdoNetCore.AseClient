using System;
using System.Data;
using AdoNetCore.AseClient.Internal;
using Dapper;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration
{
    [TestFixture]
    [Category("basic")]
    public class MultiTypeEchoProcedureTests
    {
        private readonly string _createProc = @"
create procedure [dbo].[sp_multitype_output]
    @echoChar char(1) = null output,
    @inBinary binary(16),
    @echoBinary binary(16) = null output
as begin
    set @echoChar = 'X'
    set @echoBinary = @inBinary
end";
        private readonly string _dropProc = @"drop procedure [dbo].[sp_multitype_output]";

        [SetUp]
        public void Setup()
        {
            using (var connection = new AseConnection(ConnectionStrings.Pooled))
            {
                connection.Execute(_createProc);
            }
        }

        [Test]
        public void Simple_Procedure_ShouldExecute()
        {
            Logger.Enable();
            using (var connection = new AseConnection(ConnectionStrings.Pooled))
            {
                var p = new DynamicParameters();
                p.Add("@echoChar", null, DbType.String, ParameterDirection.Output, 2);
                var inBinary = Guid.NewGuid().ToByteArray();
                p.Add("@inBinary", inBinary);
                p.Add("@echoBinary", null, DbType.Binary, ParameterDirection.Output, 16);
                connection.Execute("sp_multitype_output", p, commandType: CommandType.StoredProcedure);

                Assert.AreEqual("X", p.Get<string>("@echoChar"));
                Assert.AreEqual(inBinary, p.Get<byte[]>("@echoBinary"));
            }
        }

        [TearDown]
        public void Teardown()
        {
            using (var connection = new AseConnection(ConnectionStrings.Pooled))
            {
                connection.Execute(_dropProc);
            }
        }
    }
}
