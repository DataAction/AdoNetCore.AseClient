// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedVariable
// ReSharper disable InconsistentNaming

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using NUnit.Framework;
#pragma warning disable 219

namespace AdoNetCore.AseClient.Tests
{
    [TestFixture]
    internal sealed class CodeSamples
    {
        private readonly IDictionary<string, string> _connectionStrings = ConnectionStringLoader.Load();

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

                    command.CommandText =
@"IF OBJECT_ID('CreateCustomer') IS NOT NULL 
BEGIN 
    DROP PROCEDURE CreateCustomer
END";
                    command.ExecuteNonQuery();

                    command.CommandText =
@"CREATE PROCEDURE CreateCustomer
(
    @firstName VARCHAR(256),
    @lastName VARCHAR(256)
)
AS 
BEGIN 
    INSERT INTO Customer
    (
        FirstName, 
        LastName
    ) 
    VALUES 
    (
        @firstName,
        @lastName
    )

    RETURN 0
END";
                    command.ExecuteNonQuery();


                    command.CommandText =
@"IF OBJECT_ID('CountCustomer') IS NOT NULL 
BEGIN 
    DROP PROCEDURE CountCustomer
END";
                    command.ExecuteNonQuery();

                    command.CommandText =
@"CREATE PROCEDURE CountCustomer
AS 
BEGIN 
    SELECT COUNT(1) FROM Customer

    RETURN 0
END";
                    command.ExecuteNonQuery();

                    command.CommandText =
@"IF OBJECT_ID('GetCustomerFirstName') IS NOT NULL 
BEGIN 
    DROP PROCEDURE GetCustomerFirstName
END";
                    command.ExecuteNonQuery();

                    command.CommandText =
@"CREATE PROCEDURE GetCustomerFirstName
( 
    @lastName VARCHAR(256),
    @firstName VARCHAR(256) OUTPUT
)
AS 
BEGIN 
    SELECT TOP 1 
        @firstName = FirstName 
    FROM 
        Customer 
    WHERE 
        LastName = @lastName

    RETURN 42
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

                    command.CommandText =
@"IF OBJECT_ID('CreateCustomer') IS NOT NULL 
BEGIN 
    DROP PROCEDURE CreateCustomer
END";
                    command.ExecuteNonQuery();

                    command.CommandText =
@"IF OBJECT_ID('CountCustomer') IS NOT NULL 
BEGIN 
    DROP PROCEDURE CountCustomer
END";
                    command.ExecuteNonQuery();

                    command.CommandText =
@"IF OBJECT_ID('GetCustomerFirstName') IS NOT NULL 
BEGIN 
    DROP PROCEDURE GetCustomerFirstName
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

        [Test]
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

                    command.Parameters.AddWithValue("@lastName", "Rubble"); // Input parameter.

                    var outputParameter = command.Parameters.Add("@firstName", AseDbType.VarChar);
                    outputParameter.Direction = ParameterDirection.Output;

                    var returnParameter = command.Parameters.Add("@returnValue", AseDbType.Integer);
                    outputParameter.Direction = ParameterDirection.ReturnValue; 

                    Assert.Fail("BUG - this causes a NullReferenceException - the null value is attempted to be written to the TDS stream. Could be a more general issue with null. But DBNull.Value != null...");

                    command.ExecuteNonQuery();

                    Console.WriteLine(outputParameter.Value); // Fred
                    Console.WriteLine(returnParameter.Value); // 42
                }
            }
        }

        [Test]
        public void ExecuteAStoredProcedureAndReadResponseData()
        {
            var connectionString = "Data Source=myASEserver;Port=5000;Database=myDataBase;Uid=myUsername;Pwd=myPassword;";

            using (var connection = new AseConnection(_connectionStrings["default"]))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "GetCustomer";
                    command.CommandType = CommandType.StoredProcedure;

                    var x = command.Parameters.AddWithValue("@lastName", "Rubble");
                    var y = command.Parameters.AddWithValue("@y", 5);

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
        public void ExecuteAStoredProcedureThatReturnsNoResults()
        {
            var connectionString = "Data Source=myASEserver;Port=5000;Database=myDataBase;Uid=myUsername;Pwd=myPassword;";

            using (var connection = new AseConnection(_connectionStrings["default"]))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "CreateCustomer";
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@firstName", "Fred");
                    command.Parameters.AddWithValue("@lastName", "Flintstone");

                    command.ExecuteNonQuery();
                }
            }
        }


        [Test]
        public void ExecuteAStoredProcedureThatReturnsAScalarValue()
        {
            var connectionString = "Data Source=myASEserver;Port=5000;Database=myDataBase;Uid=myUsername;Pwd=myPassword;";

            using (var connection = new AseConnection(_connectionStrings["default"]))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "CountCustomer";
                    command.CommandType = CommandType.StoredProcedure;

                    var result = command.ExecuteScalar();
                }
            }
        }

        [Test]
        public void UseInputOutputAndReturnParametersWithAStoredProcedure()
        {
            var connectionString = "Data Source=myASEserver;Port=5000;Database=myDataBase;Uid=myUsername;Pwd=myPassword;";

            using (var connection = new AseConnection(_connectionStrings["default"]))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "GetCustomerFirstName";
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@lastName", "Rubble"); // Input parameter.

                    var outputParameter = command.Parameters.Add("@firstName", AseDbType.VarChar);
                    outputParameter.Direction = ParameterDirection.Output;

                    var returnParameter = command.Parameters.Add("@returnValue", AseDbType.Integer);
                    outputParameter.Direction = ParameterDirection.ReturnValue;

                    Assert.Fail("BUG - this causes a NullReferenceException - the null value is attempted to be written to the TDS stream. Could be a more general issue with null. But DBNull.Value != null...");

                    command.ExecuteNonQuery();

                    Console.WriteLine(outputParameter.Value); // Fred
                    Console.WriteLine(returnParameter.Value); // 42
                }
            }
        }

        [Test]
        public void ExecuteAStoredProcedureAndReadResponseDataUsingDapper()
        {
            var connectionString = "Data Source=myASEserver;Port=5000;Database=myDataBase;Uid=myUsername;Pwd=myPassword;";

            using (var connection = new AseConnection(_connectionStrings["default"]))
            {
                connection.Open();

                var barneyRubble = connection.Query<Customer>("GetCustomer", new {lastName = "Rubble"}, commandType: CommandType.StoredProcedure).First();

                // Do something with the result...
            }
        }

        private sealed class Customer
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }
    }
}
