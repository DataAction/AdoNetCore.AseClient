using System;
using System.Collections.Generic;
using System.Data;
using AdoNetCore.AseClient.Internal;
using Dapper;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration
{
    [TestFixture]
    public class MultiTypeEchoProcedureTests
    {
        private readonly IDictionary<string, string> _connectionStrings = ConnectionStringLoader.Load();

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
            using (var connection = new AseConnection(_connectionStrings["default"]))
            {
                connection.Execute(_createProc);
            }
        }

        [Test]
        public void Simple_Procedure_ShouldExecute()
        {
            Logger.Enable();
            using (var connection = new AseConnection(_connectionStrings["default"]))
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
            using (var connection = new AseConnection(_connectionStrings["default"]))
            {
                connection.Execute(_dropProc);
            }
        }
    }
}
