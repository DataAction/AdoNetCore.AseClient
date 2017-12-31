// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedVariable
// ReSharper disable InconsistentNaming

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Newtonsoft.Json;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests
{
    [TestFixture]
    internal sealed class CodeSamples
    {
        private readonly Dictionary<string, string> _connectionStrings = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("ConnectionStrings.json"));

        [SetUp]
        public void SetUp()
        {
            // Use SqlCommandBuilder.
            using (var connnection = new AseConnection(_connectionStrings["default"]))
            {
                connnection.Open();

                using (var command = connnection.CreateCommand())
                {
                    command.CommandText =
@"IF OBJECT_ID('Customer') IS NOT NULL 
BEGIN 
    DROP TABLE Customer
END";
                    command.ExecuteNonQuery();

                    command.CommandText = "CREATE TABLE Customer(FirstName VARCHAR(256) NOT NULL, LastName VARCHAR(256) NOT NULL)";
                    command.ExecuteNonQuery();

                    command.CommandText = "INSERT INTO Customer(FirstName, LastName) VALUES('Barney', 'Rubble')";
                    command.ExecuteNonQuery();

                    command.CommandText =
@"IF OBJECT_ID('GetCustomer') IS NOT NULL 
BEGIN 
    DROP PROCEDURE GetCustomer
END";
                    command.ExecuteNonQuery();

                    command.CommandText =
@"CREATE PROCEDURE GetCustomer
(
    @lastName VARCHAR(256)
)
AS 
BEGIN 
    SELECT 
        FirstName,
        LastName
    FROM
        Customer
    WHERE
        LastName = @lastName
    RETURN 0
END";
                    command.ExecuteNonQuery();
                }
            }
        }

        [TearDown]
        public void TearDown()
        {
            // Use SqlCommandBuilder.
            using (var connnection = new AseConnection(_connectionStrings["default"]))
            {
                connnection.Open();

                using (var command = connnection.CreateCommand())
                {
                    command.CommandText =
@"IF OBJECT_ID('Customer') IS NOT NULL 
BEGIN 
    DROP TABLE Customer
END";
                    command.ExecuteNonQuery();

                    command.CommandText =
@"IF OBJECT_ID('GetCustomer') IS NOT NULL 
BEGIN 
    DROP PROCEDURE GetCustomer
END";
                    command.ExecuteNonQuery();
                }
            }
        }

        [Test]
        public void OpenADatabaseConnection()
        {
            var connectionString = "Data Source=myASEserver;Port=5000;Database=myDataBase;Uid=myUsername;Pwd=myPassword;";

            using (var connection = new AseConnection(_connectionStrings["default"]))
            {
                connection.Open();

                // use the connection...
            }
        }

        [Test]
        public void ExecuteASQStatementAndReadResponseData()
        {
            var connectionString = "Data Source=myASEserver;Port=5000;Database=myDataBase;Uid=myUsername;Pwd=myPassword;";

            using (var connection = new AseConnection(_connectionStrings["default"]))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT FirstName, LastName FROM Customer";

                    using (var reader = command.ExecuteReader())
                    {
                        // Get the results.
                        while (reader.Read())
                        {
                            var firstName = reader.GetString(0);
                            var lastName = reader.GetString(1);

                            // Do something with the data...
                        }
                    }
                }
            }
        }

        [Test]
        public void ExecuteASQLStatementThatReturnsNoResults()
        {
            var connectionString = "Data Source=myASEserver;Port=5000;Database=myDataBase;Uid=myUsername;Pwd=myPassword;";

            using (var connection = new AseConnection(_connectionStrings["default"]))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "INSERT INTO Customer (FirstName, LastName) VALUES ('Fred', 'Flintstone')";

                    var recordsModified = command.ExecuteNonQuery();
                }
            }
        }

        [Test]
        public void ExecuteASQLStatementThatReturnsAScalarValue()
        {
            var connectionString = "Data Source=myASEserver;Port=5000;Database=myDataBase;Uid=myUsername;Pwd=myPassword;";

            using (var connection = new AseConnection(_connectionStrings["default"]))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT COUNT(*) FROM Customer";

                    var result = command.ExecuteScalar();
                }
            }
        }

        [Test, Ignore("This is throwing a NullReferenceException - requires investigation.")]
        public void UseInputOutputAndReturnParametersWithASQLQuery()
        {
            var connectionString = "Data Source=myASEserver;Port=5000;Database=myDataBase;Uid=myUsername;Pwd=myPassword;";

            using (var connection = new AseConnection(_connectionStrings["default"]))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = 
                        "SELECT TOP 1 @firstName = FirstName FROM Customer WHERE LastName = @lastName" +
                        Environment.NewLine +
                        "RETURN 42";

                    command.Parameters.AddWithValue("@lastName", "Flintstone"); // Input parameter.

                    var outputParameter = command.Parameters.Add("@firstName", AseDbType.VarChar);
                    outputParameter.Direction = ParameterDirection.Output;

                    var returnParameter = command.Parameters.Add("@returnValue", AseDbType.Integer);
                    outputParameter.Direction = ParameterDirection.ReturnValue;

                    command.ExecuteNonQuery();

                    Console.WriteLine(outputParameter.Value); // Fred
                    Console.WriteLine(returnParameter.Value); // 42
                }
            }
        }
    }
}
