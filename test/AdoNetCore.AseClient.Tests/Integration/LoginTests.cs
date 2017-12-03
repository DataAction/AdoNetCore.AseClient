using System.Collections.Generic;
using System.Data;
using System.IO;
using Newtonsoft.Json;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration
{
    [TestFixture]
    public class LoginTests
    {
        private readonly Dictionary<string, string> _connectionStrings = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("ConnectionStrings.json"));

        [Test]
        public void Login_Success()
        {
            using (var connection = new AseConnection(_connectionStrings["default"]))
            {
                connection.Open();
                Assert.IsTrue(connection.State == ConnectionState.Open, "Connection state should be Open after calling Open()");
            }
        }

        [Test]
        public void Login_Failure()
        {
            using (var connection = new AseConnection(_connectionStrings["badpass"]))
            {
                Assert.Throws<AseException>(() => connection.Open());
            }
        }

        public void ConnectionSample()
        {
            var connectionString = "Data Source=myASEserver;Port=5000;Database=myDataBase;Uid=myUsername;Pwd=myPassword;";

            using(var connection = new AseConnection(connectionString)) 
            {
                connection.Open();
                
                // use the connection...
            }
        }

        
        public void CommandSample()
        {
            var connectionString = "Data Source=myASEserver;Port=5000;Database=myDataBase;Uid=myUsername;Pwd=myPassword;";

            using(var connection = new AseConnection(connectionString)) 
            {
                connection.Open();
                
                using(var command = connection.CreateCommand()) 
                {
                    command.CommandText = "SELECT FirstName, LastName FROM Customer";
                    
                    using(var reader = command.ExecuteReader())
                    {
                        // Get the results.
                    }
                }
            }
        }
    }
}
