using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AdoNetCore.AseClient.Internal;
using Dapper;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration
{
    /// <summary>
    /// For benchmarking, copy this to a new project referencing the AdoNet4.AseClient dll. Hopefully we don't perform too poorly by comparison :)
    /// </summary>
    [TestFixture]
    [Category("basic")]
    public class EchoProcedureTests
    {
        private readonly string _createProc = @"
create procedure [dbo].[sp_test_echo]
  @nEchoValue int,
  @nEchoValueReturn int OUTPUT
as
begin
  set @nEchoValueReturn = @nEchoValue
  select @nEchoValue as t1Echo
  select @nEchoValue + 1 as t2Echo
  return @nEchoValue
end";

        private readonly string _dropProc = @"drop procedure [dbo].[sp_test_echo]";

        private readonly string _createEchoCharProc = @"create procedure [dbo].[sp_test_echo_char]
  @input char(1),
  @output char(1) output
as
begin
  set @output = @input
end";

        private readonly string _dropEchoCharProc = @"drop procedure [dbo].[sp_test_echo_char]";

        public EchoProcedureTests()
        {
            Logger.Disable();
        }

        [SetUp]
        public void Setup()
        {
            using (var connection = new AseConnection(ConnectionStrings.Default))
            {
                connection.Execute(_createProc);
                connection.Execute(_createEchoCharProc);
            }
        }

        [Test]
        public void Echo_Procedure_ShouldExecute()
        {
            ExecuteProcedure(ConnectionStrings.Pooled);
        }

        [TestCaseSource(nameof(MultiThreaded_Echo_Procedure_ShouldExecute_WithoutCrosstalk_Cases))]
        public void MultiThreaded_Echo_Procedure_ShouldExecute_WithoutCrosstalk(int threads, int parallelism, string csName)
        {
            var connectionString = ConnectionStrings.Pooled;
            ExecuteProcedure(connectionString);
            var sw = Stopwatch.StartNew();
            var result = Parallel.ForEach(Enumerable.Repeat(1, threads), new ParallelOptions { MaxDegreeOfParallelism = parallelism }, (_, __) => ExecuteProcedure(connectionString));
            sw.Stop();
            Assert.True(result.IsCompleted);
            Console.WriteLine($"Stopwatch reports: {sw.ElapsedMilliseconds} ms");
        }

        public static IEnumerable<TestCaseData> MultiThreaded_Echo_Procedure_ShouldExecute_WithoutCrosstalk_Cases()
        {
            yield return new TestCaseData(1000, 20, ConnectionStrings.Pooled);
            yield return new TestCaseData(5000, 20, ConnectionStrings.Pooled);
            //note: if you run a nonpooled test too frequenly,
            //you'll consume all the free ports windows normally makes available (port range ~1000-5000)
            //yield return new TestCaseData(5000, 20, ConnectionStrings.NonPooled);
        }

        private void ExecuteProcedure(string connectionString)
        {
            using (var connection = new AseConnection(connectionString))
            {
                var expected = Guid.NewGuid().GetHashCode();
                var parameters = new DynamicParameters();
                parameters.Add("@RETURN_VALUE", -1, DbType.Int32, ParameterDirection.ReturnValue);
                parameters.Add("@nEchoValueReturn", -1, DbType.Int32, ParameterDirection.Output);
                parameters.Add("@nEchoValue", expected, DbType.Int32, ParameterDirection.Input);

                using (var multi = connection.QueryMultiple("sp_test_echo", parameters, commandType: CommandType.StoredProcedure))
                {
                    var t1Echo = multi.Read<int>().FirstOrDefault();
                    var t2Echo = multi.Read<int>().FirstOrDefault();

                    Assert.AreEqual(expected, t1Echo);
                    Assert.AreEqual(expected + 1, t2Echo);

                    var rReturn = parameters.Get<int>("@RETURN_VALUE");
                    var rOutput = parameters.Get<int>("@nEchoValueReturn");

                    Assert.AreEqual(expected, rReturn);
                    Assert.AreEqual(expected, rOutput);
                }
            }
        }

        [TestCase(" ", " ")]
        [TestCase("", " ")]
        [TestCase(null, null)]
        public void EchoChar_Procedure_ShouldExecute(object input, object expected)
        {
            using (var connection = new AseConnection(ConnectionStrings.Default))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "sp_test_echo_char";
                    command.CommandType = CommandType.StoredProcedure;

                    var p = command.CreateParameter();
                    p.ParameterName = "@input";
                    p.Value = input ?? DBNull.Value;
                    p.DbType = DbType.AnsiStringFixedLength;
                    command.Parameters.Add(p);

                    var pOut = command.CreateParameter();
                    pOut.ParameterName = "@output";
                    pOut.Value = DBNull.Value;
                    pOut.DbType = DbType.AnsiStringFixedLength;
                    pOut.Direction = ParameterDirection.Output;
                    pOut.Size = 1;
                    command.Parameters.Add(pOut);

                    command.ExecuteNonQuery();

                    Assert.AreEqual(expected ?? DBNull.Value, pOut.Value);
                }
            }
        }

        [TearDown]
        public void Teardown()
        {
            using (var connection = new AseConnection(ConnectionStrings.Default))
            {
                connection.Execute(_dropProc);
                connection.Execute(_dropEchoCharProc);
            }
        }
    }
}
