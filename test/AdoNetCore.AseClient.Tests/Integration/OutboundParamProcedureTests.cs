using System;
using System.Data;
using Dapper;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration
{
    [TestFixture]
    [Category("basic")]
    public class OutboundParamProcedureTests
    {
        private readonly string _createProc = @"CREATE PROCEDURE dbo.sp_test_173(@TotalValue numeric(18,4) = 0 output)
AS
set @TotalValue = 2819.0444";
        private readonly string _dropProc = @"drop procedure [dbo].[sp_test_173]";

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
            using (var connection = new AseConnection(ConnectionStrings.Pooled))
            {
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText = "sp_test_173";
                cmd.CommandType = CommandType.StoredProcedure;
                var total = new AseParameter()
                {
                    ParameterName = "@TotalValue",
                    AseDbType = AseDbType.Decimal,
                    Direction = ParameterDirection.Output,
                    Precision = 18,
                    Scale = 4
                };
                cmd.Parameters.Add(total);
                var result = cmd.ExecuteNonQuery();
                Assert.AreEqual(2819.0444m,Convert.ToDecimal(total.SendableValue));
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
