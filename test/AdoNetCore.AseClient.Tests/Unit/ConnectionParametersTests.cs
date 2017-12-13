using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using AdoNetCore.AseClient.Internal;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Unit
{
    [TestFixture]
    public class ConnectionParametersTests
    {
        [Test]
        public void ConnectionParameters_WithValidConnectionString_SetsProperties()
        {
            // Arrange
            var connectionString =
                "Data Source=a_server;Port=1234;Database=a_database;Uid=a_user;Pwd=a_password;Charset=a_charset;Pooling=true;Min Pool Size=5;Max Pool Size=10;ApplicationName=an_application;" +
                "ClientHostProc=an_app.exe;ClientHostName=a_clienthost;Ping Server=true; LoginTimeOut=5;ConnectionIdleTimeout=6;ConnectionLifetime=7;PacketSize=9000;TextSize=9001";

            // Act
            var actual = ConnectionParameters.Parse(connectionString);

            // Assert
            Assert.AreEqual("a_server", actual.Server);
            Assert.AreEqual(1234, actual.Port);
            Assert.AreEqual("a_database", actual.Database);
            Assert.AreEqual("a_user", actual.Username);
            Assert.AreEqual("a_password", actual.Password);
            Assert.AreEqual("a_charset", actual.Charset);
            Assert.AreEqual(true, actual.Pooling);
            Assert.AreEqual(5, actual.MinPoolSize);
            Assert.AreEqual(10, actual.MaxPoolSize);
            Assert.AreEqual("an_application", actual.ApplicationName);
            Assert.AreEqual("a_clienthost", actual.ClientHostName);
            Assert.AreEqual("an_app.exe", actual.ClientHostProc);
            Assert.AreNotEqual(0, actual.ProcessId);
            Assert.AreEqual(true, actual.PingServer);
            Assert.AreEqual(5, actual.LoginTimeout);
            Assert.AreEqual(6, actual.ConnectionIdleTimeout);
            Assert.AreEqual(7, actual.ConnectionLifetime);
            Assert.AreEqual(9000, actual.PacketSize);
            Assert.AreEqual(9001, actual.TextSize);
        }

        [TestCase("Data Source=a_server:1234;;Initial Catalog=a_database;Uid=myUsername;", "a_server", 1234)]
        [TestCase("Data Source=a_server,1234;Initial Catalog=a_database;Uid=myUsername;", "a_server", 1234)]
        public void ConnectionParameters_WithCombinedServerAndPort_SetsProperties(string connectionString, string expectedServerName, int expectedPort)
        {
            // Act
            var actual = ConnectionParameters.Parse(connectionString);

            // Assert
            Assert.AreEqual(expectedServerName, actual.Server);
            Assert.AreEqual(expectedPort, actual.Port);
        }

        [TestCase("Data Source=myASEserver;Port=5000;Db=a_database;Uid=myUsername;Pwd=myPassword;", "a_database")]
        [TestCase("Data Source=myASEserver;Port=5000;Database=a_database;Uid=myUsername;Pwd=myPassword;", "a_database")]
        [TestCase("Data Source=myASEserver;Port=5000;Initial Catalog=a_database;Uid=myUsername;Pwd=myPassword;", "a_database")]
        [TestCase("Data Source=myASEserver;Port=5000;initial catalog=a_database;Uid=myUsername;Pwd=myPassword;", "a_database")]
        public void ConnectionParameters_WithAltDatabaseSyntax_SetsProperties(string connectionString, string expectedDatabaseName)
        {
            // Act
            var actual = ConnectionParameters.Parse(connectionString);

            // Assert
            Assert.AreEqual(expectedDatabaseName, actual.Database);
        }

        [TestCase("Data Source=myASEserver;Port=5000;Db=a_database;Uid=a_user;Pwd=myPassword;", "a_user")]
        [TestCase("Data Source=myASEserver;Port=5000;Database=a_database;User Id=a_user;Pwd=myPassword;", "a_user")]
        public void ConnectionParameters_WithAltUserIdSyntax_SetsProperties(string connectionString, string expectedDatabaseName)
        {
            // Act
            var actual = ConnectionParameters.Parse(connectionString);

            // Assert
            Assert.AreEqual(expectedDatabaseName, actual.Username);
        }

        [TestCase("Data Source=myASEserver;Port=5000;Db=a_database;Uid=a_user;Pwd=myPassword;", "myPassword")]
        [TestCase("Data Source=myASEserver;Port=5000;Database=a_database;User Id=a_user;Password=myPassword;", "myPassword")]
        public void ConnectionParameters_WithAltPasswordSyntax_SetsProperties(string connectionString, string expectedDatabaseName)
        {
            // Act
            var actual = ConnectionParameters.Parse(connectionString);

            // Assert
            Assert.AreEqual(expectedDatabaseName, actual.Password);
        }

        [TestCase("Data Source=myASEserver;Port=5000;Db=a_database;Uid=a_user;Pwd=myPassword;ApplicationName=an_application", "an_application")]
        [TestCase("Data Source=myASEserver;Port=5000;Database=a_database;User Id=a_user;Password=myPassword;Application Name=an_application", "an_application")]
        public void ConnectionParameters_WithAltApplicationNameSyntax_SetsProperties(string connectionString, string expectedApplicationName)
        {
            // Act
            var actual = ConnectionParameters.Parse(connectionString);

            // Assert
            Assert.AreEqual(expectedApplicationName, actual.ApplicationName);
        }

        [TestCase("Data Source=myASEserver;Port=5000;Db=a_database;Uid=a_user;Pwd=myPassword;ConnectionLifetime=9000", 9000)]
        [TestCase("Data Source=myASEserver;Port=5000;Database=a_database;User Id=a_user;Password=myPassword;ConnectionLifetime=9000", 9000)]
        public void ConnectionParameters_WithAltApplicationNameSyntax_SetsProperties(string connectionString, int expectedConnectionLifetime)
        {
            // Act
            var actual = ConnectionParameters.Parse(connectionString);

            // Assert
            Assert.AreEqual(expectedConnectionLifetime, actual.ConnectionLifetime);
        }

        // TODO - some tests for the DSUrl property.

        [Test]
        public void ConnectionParameters_WithDSURL_SetsProperties()
        {
            // Arrange
            var path = MakeFile(SqlIniWithSeveralEntries);
            var connectionString = $"DSURL=file://{path}?PIANO_BS;Db=a_database;Uid=a_user;Pwd=myPassword";

            // Act
            var actual = ConnectionParameters.Parse(connectionString);

            // Assert
            Assert.AreEqual("PIANO", actual.Server);
            Assert.AreEqual(5001, actual.Port);
        }


        [Test]
        public void ConnectionParameters_WithDSURLButNoServiceNameAndOneDefinitionInTheFile_SetsProperties()
        {
            // Arrange
            var path = MakeFile(SqlIniWithOneEntry);
            var connectionString = $"DSURL=file://{path};Db=a_database;Uid=a_user;Pwd=myPassword";

            // Act
            var actual = ConnectionParameters.Parse(connectionString);

            // Assert
            Assert.AreEqual("PIANO", actual.Server);
            Assert.AreEqual(5001, actual.Port);
        }

        [Test]
        public void ConnectionParameters_WithDSURLButNoServiceNameAndManyDefinitionsInTheFile_error()
        {
            // Arrange
            var path = MakeFile(SqlIniWithSeveralEntries);
            var connectionString = $"DSURL=file://{path};Db=a_database;Uid=a_user;Pwd=myPassword";

            // Act//Assert
            Assert.Throws<AseException>(() =>
            {
                ConnectionParameters.Parse(connectionString);
            });
        }

        private static string MakeFile(string contents)
        {
            var filename = Path.GetTempFileName();

            File.WriteAllText(filename, contents, Encoding.UTF8);

            return filename;
        }

        private const string SqlIniWithSeveralEntries = 
@"[PIANO_XP]
master=NLWNSCK,PIANO,5003
query=NLWNSCK,PIANO,5003


[PIANO]
master=NLWNSCK,PIANO,5000
query=NLWNSCK,PIANO,5000


[PIANO_BS]
master=NLWNSCK,PIANO,5001
query=NLWNSCK,PIANO,5001


[PIANO_MS]
master=NLWNSCK,PIANO,5002
query=NLWNSCK,PIANO,5002


[PIANO_JSAGENT]
master=NLWNSCK,PIANO,4900
query=NLWNSCK,PIANO,4900


[ws]
master=NLWNSCK,PIANO,8183";

        private const string SqlIniWithOneEntry =
@"[PIANO_BS]
master=NLWNSCK,PIANO,5001
query=NLWNSCK,PIANO,5001";
    }
}
