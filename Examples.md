## Code samples
### Open a database connection
```C#
var connectionString = "Data Source=myASEserver;Port=5000;Database=myDataBase;Uid=myUsername;Pwd=myPassword;";

using(var connection = new AseConnection(connectionString))
{
    connection.Open();

    // use the connection...
}
```

### Execute a SQL statement and read response data
```C#
var connectionString = "Data Source=myASEserver;Port=5000;Database=myDataBase;Uid=myUsername;Pwd=myPassword;";

using (var connection = new AseConnection(connectionString))
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
```

### Execute a SQL statement that returns no results
```C#
var connectionString = "Data Source=myASEserver;Port=5000;Database=myDataBase;Uid=myUsername;Pwd=myPassword;";

using (var connection = new AseConnection(connectionString))
{
    connection.Open();

    using (var command = connection.CreateCommand())
    {
        command.CommandText = "INSERT INTO Customer (FirstName, LastName) VALUES ('Fred', 'Flintstone')";

        var recordsModified = command.ExecuteNonQuery();
    }
}
```

### Execute a SQL statement that returns a scalar value
```C#
var connectionString = "Data Source=myASEserver;Port=5000;Database=myDataBase;Uid=myUsername;Pwd=myPassword;";

using (var connection = new AseConnection(connectionString))
{
    connection.Open();

    using (var command = connection.CreateCommand())
    {
        command.CommandText = "SELECT COUNT(*) FROM Customer";

        var result = command.ExecuteScalar();
    }
}
```

### Use input parameters with a SQL query
Note: ASE only allows `Output`, `InputOutput`, and `ReturnValue` parameters with stored procedures
```C#
var connectionString = "Data Source=myASEserver;Port=5000;Database=myDataBase;Uid=myUsername;Pwd=myPassword;";

using (var connection = new AseConnection(connectionString)
{
    connection.Open();

    using (var command = connection.CreateCommand())
    {
        command.CommandText = "SELECT TOP 1 FirstName FROM Customer WHERE LastName = @lastName";

        command.Parameters.AddWithValue("@lastName", "Rubble");

        var result = command.ExecuteScalar();
    }
}
```

### Execute a stored procedure and read response data
```C#
var connectionString = "Data Source=myASEserver;Port=5000;Database=myDataBase;Uid=myUsername;Pwd=myPassword;";

using (var connection = new AseConnection(connectionString)
{
    connection.Open();

    using (var command = connection.CreateCommand())
    {
        command.CommandText = "GetCustomer";
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.AddWithValue("@lastName", "Rubble");

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
```

### Execute a stored procedure that returns no results
```C#
var connectionString = "Data Source=myASEserver;Port=5000;Database=myDataBase;Uid=myUsername;Pwd=myPassword;";

using (var connection = new AseConnection(connectionString))
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
```

### Execute a stored procedure that returns a scalar value
```C#
var connectionString = "Data Source=myASEserver;Port=5000;Database=myDataBase;Uid=myUsername;Pwd=myPassword;";

using (var connection = new AseConnection(connectionString))
{
    connection.Open();

    using (var command = connection.CreateCommand())
    {
        command.CommandText = "CountCustomer";
        command.CommandType = CommandType.StoredProcedure;

        var result = command.ExecuteScalar();
    }
}
```

### Use input, output, and return parameters with a stored procedure
```C#
var connectionString = "Data Source=myASEserver;Port=5000;Database=myDataBase;Uid=myUsername;Pwd=myPassword;";

using (var connection = new AseConnection(connectionString))
{
    connection.Open();

    using (var command = connection.CreateCommand())
    {
        command.CommandText = "GetCustomerFirstName";
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.AddWithValue("@lastName", "Rubble");

        var outputParameter = command.Parameters.Add("@firstName", AseDbType.VarChar);
        outputParameter.Direction = ParameterDirection.Output;

        var returnParameter = command.Parameters.Add("@returnValue", AseDbType.Integer);
        returnParameter.Direction = ParameterDirection.ReturnValue;

        command.ExecuteNonQuery();

        //Do something with outputParameter.Value and returnParameter.Value...
    }
}
```

### Execute a stored procedure and read response data with [Dapper](https://github.com/StackExchange/Dapper)
```C#
var connectionString = "Data Source=myASEserver;Port=5000;Database=myDataBase;Uid=myUsername;Pwd=myPassword;";

using (var connection = new AseConnection(connectionString))
{
    connection.Open();

    var barneyRubble = connection.Query<Customer>("GetCustomer", new {lastName = "Rubble"}, commandType: CommandType.StoredProcedure).First();

    // Do something with the result...
}
```