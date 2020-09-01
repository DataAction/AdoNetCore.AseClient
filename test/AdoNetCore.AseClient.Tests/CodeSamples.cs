// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedVariable
// ReSharper disable InconsistentNaming

using System;
using System.Data;
using System.Linq;
using Dapper;
using NUnit.Framework;
// ReSharper disable All
#pragma warning disable 219

namespace AdoNetCore.AseClient.Tests
{
    [TestFixture, Explicit("Code samples are only encapsulated in tests so we can run them if we need to. We're not relying on them to test the data provider.")]
    internal sealed class CodeSamples
    {
        [SetUp]
        public void SetUp()
        {
            // Use SqlCommandBuilder.
            using (var connnection = new AseConnection(ConnectionStrings.Default))
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
            using (var connnection = new AseConnection(ConnectionStrings.Default))
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
            using (var connection = new AseConnection(ConnectionStrings.Default))
            {
                connection.Open();

                // use the connection...
            }
        }

        [Test]
        public void ExecuteASQStatementAndReadResponseData()
        {
            using (var connection = new AseConnection(ConnectionStrings.Default))
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
            using (var connection = new AseConnection(ConnectionStrings.Default))
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
            using (var connection = new AseConnection(ConnectionStrings.Default))
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
        public void ExecuteAStoredProcedureAndReadResponseData()
        {
            using (var connection = new AseConnection(ConnectionStrings.Default))
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

            using (var connection = new AseConnection(ConnectionStrings.Default))
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

            using (var connection = new AseConnection(ConnectionStrings.Default))
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
        public void UseInputParameterWithASQLQuery()
        {
            using (var connection = new AseConnection(ConnectionStrings.Default))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "SELECT TOP 1 FirstName FROM Customer WHERE LastName = @lastName";

                    command.Parameters.AddWithValue("@lastName", "Rubble"); // Input parameter.
                    var result = command.ExecuteScalar();
                }
            }
        }

        [Test]
        public void UseInputOutputAndReturnParametersWithAStoredProcedure()
        {
            using (var connection = new AseConnection(ConnectionStrings.Default))
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
                    returnParameter.Direction = ParameterDirection.ReturnValue;

                    command.ExecuteNonQuery();

                    Console.WriteLine(outputParameter.Value); // Fred
                    Console.WriteLine(returnParameter.Value); // 42
                }
            }
        }

        [Test]
        public void ExecuteAStoredProcedureAndReadResponseDataUsingDapper()
        {
            using (var connection = new AseConnection(ConnectionStrings.Default))
            {
                connection.Open();

                var barneyRubble = connection.Query<Customer>("GetCustomer", new {lastName = "Rubble"}, commandType: CommandType.StoredProcedure).First();
                Assert.IsNotNull(barneyRubble.FirstName);
                Assert.IsNotNull(barneyRubble.LastName);
                // Do something with the result...
            }
        }

        [Test]
        public void ExecuteScalar_Error_should_be_captured()
        {
            using (var connection = new AseConnection(ConnectionStrings.Default))
            {
                connection.Open();

                var exception = Assert.Throws<AdoNetCore.AseClient.AseException>(() =>
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT 1/0 As Error";

                        var result = command.ExecuteScalar();
                    }
                });

                Assert.That(exception.Message == "Divide by zero occurred.\n");
            }
        }

        private sealed class Customer
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }
    }
}
