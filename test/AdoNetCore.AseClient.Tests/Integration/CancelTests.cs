using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using AdoNetCore.AseClient.Internal;
using Newtonsoft.Json;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration
{
    [TestFixture]
    public class CancelTests
    {
        private readonly Dictionary<string, string> _connectionStrings = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("ConnectionStrings.json"));

        [Test]
        public void Async_NoCancel_Succeeds()
        {
            Logger.Enable();
            using (var connection = new AseConnection(_connectionStrings["default"]))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "waitfor delay '00:00:02' select 1";
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQueryAsync(new CancellationToken()).Wait();
                }
            }
        }

        [Test]
        public void Async_Cancel_Succeeds()
        {
            Logger.Enable();
            using (var connection = new AseConnection(_connectionStrings["default"]))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "waitfor delay '00:00:10' select 1";
                    command.CommandType = CommandType.Text;
                    var cts = new CancellationTokenSource();
                    var token = cts.Token;

                    Assert.IsTrue(token.CanBeCanceled);
                    token.Register(() => Logger.Instance.WriteLine($"CANCELATION"));
                    
                    var task = command.ExecuteNonQueryAsync(token);
                    cts.Cancel();
                    task.Wait();
                    Assert.IsTrue(task.IsCanceled);
                }
            }
        }
    }
}
