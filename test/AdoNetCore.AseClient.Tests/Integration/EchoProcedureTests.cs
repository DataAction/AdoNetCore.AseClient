using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Newtonsoft.Json;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration
{
    /// <summary>
    /// For benchmarking, copy this to a new project referencing the AdoNet4.AseClient dll. Hopefully we don't perform too poorly by comparison :)
    /// </summary>
    [TestFixture]
    public class EchoProcedureTests
    {
        private readonly Dictionary<string, string> _connectionStrings = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("ConnectionStrings.json"));

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

        [SetUp]
        public void Setup()
        {
            using (var connection = new AseConnection(_connectionStrings["default"]))
            {
                connection.Execute(_createProc);
            }
        }

        [Test]
        public void Echo_Procedure_ShouldExecute()
        {
            ExecuteProcedure(_connectionStrings["pooled"]);
        }

        [TestCase(1000, 20, "pooled")]
        [TestCase(5000, 20, "pooled")]
        //note: if you run a nonpooled test too frequenly,
        //you'll consume all the free ports windows normally makes available (port range ~1000-5000)
        //[TestCase(1000, 20, "nonpooled")]
        public void MultiThreaded_Echo_Procedure_ShouldExecute_WithoutCrosstalk(int threads, int parallelism, string csName)
        {
            var connectionString = _connectionStrings[csName];
            ExecuteProcedure(connectionString);
            var sw = Stopwatch.StartNew();
            var result = Parallel.ForEach(Enumerable.Repeat(1, threads), new ParallelOptions { MaxDegreeOfParallelism = parallelism }, (_, __) => ExecuteProcedure(connectionString));
            sw.Stop();
            Assert.True(result.IsCompleted);
            Console.WriteLine($"Stopwatch reports: {sw.ElapsedMilliseconds} ms");
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
