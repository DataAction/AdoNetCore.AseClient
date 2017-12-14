using System.Collections.Generic;
using System.IO;
using System.Threading;
using Dapper;
using Newtonsoft.Json;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration
{
    [TestFixture]
    public class CancelTests
    {
        private readonly Dictionary<string, string> _connectionStrings = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("ConnectionStrings.json"));

        [Test]
        public void Blah()
        {
            using (var connection = new AseConnection(_connectionStrings["default"]))
            {
                var source = new CancellationTokenSource();
                var task = connection.QueryAsync<int>(new CommandDefinition("select 1", cancellationToken: source.Token));
                source.Cancel();
                task.Wait();
            }
        }
    }
}
