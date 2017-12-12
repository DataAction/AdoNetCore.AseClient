# AdoNetCore.AseClient - a .NET Core DB Provider for SAP ASE

SAP (formerly Sybase) has supported accessing the ASE database management system from ADO.NET for many years. Unfortunately SAP has not yet made a driver available to support .NET Core, so this project enables product teams that are dependent upon ASE to keep moving their application stack forwards.

The current .NET 4 version of SAP's AseClient driver is a .NET Framework managed wrapper around SAP's unmanged [ADO DB provider](https://en.wikipedia.org/wiki/ActiveX_Data_Objects) and is dependent upon [COM](https://en.wikipedia.org/wiki/Component_Object_Model). COM is a Windows-only technology and will never be available to .NET Core, making it difficult to port the existing driver.

Under the hood, ASE (and Microsoft Sql Server for that matter) relies on an application-layer protocol called [Tabular Data Stream](https://en.wikipedia.org/wiki/Tabular_Data_Stream) to transfer data between the database server and the client application. ASE uses TDS 5.0.

This project provides a .NET Core native implementation of the TDS 5.0 protocol via an ADO.NET DB Provider, making SAP ASE accessible from .NET Core applications hosted on Windows, Linux, Docker and also serverless platforms like [AWS Lambda](https://aws.amazon.com/lambda/).

## Objectives
* Functional parity with the `Sybase.AdoNet4.AseClient` provided by SAP. Ideally, our driver will be a drop in replacement for the SAP AseClient. The following types are supported:
    * AseCommand - in progress
    * AseConnection - in progress
    * AseParameter
    * AseParameterCollection
    * AseDataReader - in progress
    * AseDbType
    * AseError
    * AseErrorCollection
    * AseException - in progress
    * AseInfoMessageEventArgs
    * AseInfoMessageEventHandler 
    * TraceEnterEventHandler
    * TraceExitEventHandler
* The following features are not yet supported:
   * `Bulk Copy` - no reason this can't be supported, just hasn't been a priority thus far. As such the following types have not yet been implemented:
        * [AseBulkCopy](http://infocenter.sybase.com/help/topic/com.sybase.infocenter.dc20066.1570100/doc/html/san1364409524288.html)
        * [AseBulkCopyColumnMapping](http://infocenter.sybase.com/help/topic/com.sybase.infocenter.dc20066.1570100/doc/html/san1364409528570.html)
        * [AseBulkCopyColumnMappingCollection](http://infocenter.sybase.com/help/topic/com.sybase.infocenter.dc20066.1570100/doc/html/san1364409530992.html)
        * [AseBulkCopyOptions](http://infocenter.sybase.com/help/topic/com.sybase.infocenter.dc20066.1570100/doc/html/san1364409533851.html)
        * [AseRowsCopiedEventArgs](http://infocenter.sybase.com/help/topic/com.sybase.infocenter.dc20066.1570100/doc/html/san1364409610666.html)
        * [AseRowsCopiedEventHandler](http://infocenter.sybase.com/help/topic/com.sybase.infocenter.dc20066.1570100/doc/html/san1364409612103.html)
    * `Failover` - no reason this can't be supported, just hasn't been a priority thus far. As such the following types have not yet been implemented:
        * [AseFailoverException](http://infocenter.sybase.com/help/topic/com.sybase.infocenter.dc20066.1570100/doc/html/san1364409597900.html)
* The following types are not yet supported:
    * [AseClientFactory](http://infocenter.sybase.com/help/topic/com.sybase.infocenter.dc20066.1570100/doc/html/san1364409534226.html) - waiting on .NET Core 2.1 for this type to be supported.
    * [AseClientPermission](http://infocenter.sybase.com/help/topic/com.sybase.infocenter.dc20066.1570100/doc/html/san1364409538585.html) - TODO - depends on .NET Core 2.0.
    * [AseClientPermissionAttribute](http://infocenter.sybase.com/help/topic/com.sybase.infocenter.dc20066.1570100/doc/html/san1364409539304.html) - TODO - depends on .NET Core 2.0.
    * [AseCommandBuilder](http://infocenter.sybase.com/help/topic/com.sybase.infocenter.dc20066.1570100/doc/html/san1364409546398.html) - TODO - depends on .NET Core 2.0.
    * [AseConnectionPool](http://infocenter.sybase.com/help/topic/com.sybase.infocenter.dc20066.1570100/doc/html/san1364409558524.html) - TODO - where is this exposed?
    * [AseConnectionPoolManager](http://infocenter.sybase.com/help/topic/com.sybase.infocenter.dc20066.1570100/doc/html/san1364409559633.html) - TODO - where is this exposed?
    * [AseDataAdapter](http://infocenter.sybase.com/help/topic/com.sybase.infocenter.dc20066.1570100/doc/html/san1364409561039.html) - TODO - depends on .NET Core 2.0.
    * [AseDecimal](http://infocenter.sybase.com/help/topic/com.sybase.infocenter.dc20066.1570100/doc/html/san1364409584368.html) - TODO - is the parallel type to SqlDecimal?
    * [AseRowUpdatedEventArgs](http://infocenter.sybase.com/help/topic/com.sybase.infocenter.dc20066.1570100/doc/html/san1364409612447.html) - TODO - depends on .NET Core 2.0.
    * [AseRowUpdatingEventArgs](http://infocenter.sybase.com/help/topic/com.sybase.infocenter.dc20066.1570100/doc/html/san1364409615713.html)  - TODO - depends on .NET Core 2.0.
    * [AseRowUpdatedEventHandler](http://infocenter.sybase.com/help/topic/com.sybase.infocenter.dc20066.1570100/doc/html/san1364409618604.html)  - TODO - depends on .NET Core 2.0.
    * [AseRowUpdatingEventHandler](http://infocenter.sybase.com/help/topic/com.sybase.infocenter.dc20066.1570100/doc/html/san1364409618979.html)  - TODO - depends on .NET Core 2.0.

* Performance equivalent to or better than that of `Sybase.AdoNet4.AseClient` provided by SAP. This should be possible as we are eliminating the COM and OLE DB layers from this driver and .NET Core is fast.
* Target all versions of .NET Core (1.0, 1.1, 2.0, and 2.1 when it is released)
* Should work with [Dapper](https://github.com/StackExchange/Dapper) at least as well as the .NET 4 client

## Performance benchmarks
We've spent considerable time optimising this driver so that it performs as well as possible. We have benchmarked the .NET Core AseClient against the SAP AseClient in the following ways:

### TODO - test methodology
Look at a tool like: https://github.com/dotnet/BenchmarkDotNet
http://benchmarkdotnet.org/Overview.htm
https://www.nuget.org/packages/BenchmarkDotNet.Core/

### TODO - test results
![Long story short, our speed is a stampede](http://via.placeholder.com/1024x496?text=Run%20Forrest,%20run! "Some sick chart")

## Connection strings
[connectionstrings.com](https://www.connectionstrings.com/sybase-adaptive/) lists the following connection string properties for the ASE ADO.NET Data Provider. In keeping with our objective of being a drop-in replacement for the SAP AseClient, we aim to use identical connection string syntax to the SAP AseClient, however our support for the various properties will be limited. Our support is as follows:

| Property                          | Support   | Notes
| --------------------------------- |:---------:| -----
| `AlternateServers`                | X
| `ApplicationName`                 | &#10003;
| `BufferCacheSize`                 | TODO
| `Charset`                         | &#10003;
| `ClientHostName`                  | &#10003;
| `ClientHostProc`                  | &#10003;
| `CodePageType`                    | TODO
| `Connection Lifetime`             | TODO
| `ConnectionIdleTimeout`           | TODO
| `CumulativeRecordCount`           | TODO
| `Database`                        | &#10003;
| `Data Source`                     | &#10003;
| `DistributedTransactionProtocol`  | X
| `DSURL`                           | TODO | Consider [ini-parser](https://www.nuget.org/packages/ini-parser/)
| `EnableBulkLoad`                  | X
| `EnableServerPacketSize`          | ? | May not be supported any more by capability bits
| `Encryption`                      | X
| `EncryptPassword`                 | ?
| `Enlist`                          | X
| `FetchArraySize`                  | TODO
| `HASession`                       | X
| `LoginTimeOut`                    | TODO
| `Max Pool Size`                   | &#10003;
| `Min Pool Size`                   | TODO
| `PacketSize`                      | &#10003; | The server can decide to change this value
| `Ping Server`                     | TODO
| `Pooling`                         | &#10003;
| `Port`                            | &#10003;
| `Pwd`                             | &#10003;
| `RestrictMaximum PacketSize`      | ? | May not be supported any more by capability bits
| `Secondary Data Source`           | X
| `Secondary Server Port`           | X
| `TextSize`                        | &#10003;
| `TightlyCoupledTransaction`       | X
| `TrustedFile`                     | X
| `Uid`                             | &#10003;
| `UseAseDecimal`                   | TODO
| `UseCursor`                       | X

Notes on SAP AseClient connection string support from the [online docs](http://infocenter.sybase.com/help/topic/com.sybase.infocenter.dc20066.1570100/doc/html/san1364409554914.html):
* The ConnectionString is designed to match the ODBC connection string format as closely as possible.
* You can set the ConnectionString property only when the connection is closed. Many of the connection string values have corresponding read-only properties. When the connection string is set, all of these properties are updated. However, if an error is detected, none of the properties are updated. AseConnection properties return only those settings contained in the ConnectionString.
* If you reset the ConnectionString on a closed connection, all connection string values and related properties are reset, including the password.
* When the property is set, a preliminary validation of the connection string is performed. When an application calls the Open method, the connection string is fully validated. A runtime exception is generated if the connection string contains invalid or unsupported properties.
* Values can be delimited by single or double quotes. Either single or double quotes can be used within a connection string by using the other delimiter. For example, name="value's" or name= 'value"s', but not name='value's' or name= ""value"".

  Blank characters are ignored unless they are placed within a value or within quotes.

  Keyword-value pairs must be separated by a semicolon. If a semicolon is part of a value, it must also be delimited by quotes.

  Escape sequences are not supported, and the value type is irrelevant.

  Names are not case sensitive. If a property name occurs more than once in the connection string, the value associated with the last occurrence is used.
* Use caution when constructing a connection string based on user input, such as when retrieving a user ID and password from a dialog box, and appending it to the connection string. The application should not allow a user to embed extra connection string parameters in these values.


## Supported types
### Types supported when sending requests to the database

| DbType                  | Send      | .NET Type(s) | Notes
| ----------------------- |:---------:| ------------ | -----
| `AnsiString`            | &#10003;  | `string`
| `AnsiStringFixedLength` | &#10003;  | `string`
| `Binary`                | &#10003;  | `byte[]`
| `Boolean`               | &#10003;  | `bool`
| `Byte`                  | &#10003;  | `byte`
| `Currency`              | &#10003;  | `decimal` | Sent as decimal type; may change to send as `TDS_MONEY`, which is shorter
| `Date`                  | &#10003;  | `DateTime` | Time component is ignored
| `DateTime`              | &#10003;  | `DateTime`
| `DateTime2`             | X         | | ASE does not support a `DateTime2` type. Use `DateTime` instead
| `DateTimeOffset`        | X         | | ASE does not support a `DateTimeOffset` type. Use `DateTime` instead
| `Decimal`               | &#10003;  | `decimal`
| `Double`                | &#10003;  | `double`
| `Guid`                  | X         | | ASE does not support GUID or UUID types. Call `.ToByteArray()` and use `DbType.Binary` instead
| `Int16`                 | &#10003;  | `short`
| `Int32`                 | &#10003;  | `int`
| `Int64`                 | &#10003;  | `long`
| `Object`                | X         | | ASE does not support an `Object` type
| `SByte`                 | &#10003;  | `sbyte` | Sent as int16
| `Single`                | &#10003;  | `float`
| `String`                | &#10003;  | `string` | UTF-16 encoded, sent to server as binary with usertype `35`
| `StringFixedLength`     | &#10003;  | `string` | UTF-16 encoded, sent to server as binary with usertype `34`
| `Time`                  | &#10003;  | `TimeSpan`
| `UInt16`                | &#10003;  | `ushort`
| `UInt32`                | &#10003;  | `uint`
| `UInt64`                | &#10003;  | `ulong`
| `VarNumeric`            | &#10003;  | `decimal`
| `Xml`                   | X         | | ASE does not support an `Xml` type

### Types supported when reading responses from the database

| ASE Type            | Receive   | .NET Type(s) | Notes
| ------------------- |:---------:| ------------ | -----
| `bigdatetime`       | ?         | `DateTime` | Investigate: can we enable this type on our test server?
| `bigint`            | &#10003;  | `long`
| `bigtime`           | ?         | `TimeSpan` | Investigate: can we enable this type on our test server?
| `binary`            | &#10003;  | `byte[]`
| `bit`               | &#10003;  | `bool`
| `char`              | &#10003;  | `string`
| `date`              | &#10003;  | `DateTime`
| `datetime`          | &#10003;  | `DateTime`
| `decimal`           | &#10003;  | `decimal`
| `double precision`  | &#10003;  | `double`
| `float`             | &#10003;  | `float`
| `image`             | &#10003;  | `byte[]`
| `int`               | &#10003;  | `int`
| `money`             | &#10003;  | `decimal`
| `nchar`             | &#10003;  | `string`
| `numeric`           | &#10003;  | `decimal`
| `nvarchar`          | &#10003;  | `string`
| `smalldatetime`     | &#10003;  | `DateTime`
| `smallint`          | &#10003;  | `short`
| `smallmoney`        | &#10003;  | `decimal`
| `time`              | &#10003;  | `TimeSpan`
| `tinyint`           | &#10003;  | `byte`
| `unichar`           | &#10003;  | `string` | Server sends as binary with usertype `34`
| `univarchar`        | &#10003;  | `string` | Server sends as binary with usertype `35`
| `unsigned bigint`   | &#10003;  | `ulong`
| `unsigned int`      | &#10003;  | `uint`
| `unsigned smallint` | &#10003;  | `usmallint`
| `varchar`           | &#10003;  | `string`
| `text`              | &#10003;  | `string`
| `unitext`           | &#10003;  | `string`
| `varbinary`         | &#10003;  | `byte[]`

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
using(var command = connection.CreateCommand())
{
    command.CommandText = "SELECT FirstName, LastName FROM Customer";

    using(var reader = command.ExecuteReader())
    {
        // Get the results.
    }
}
```

### Execute a SQL statement that returns no results
```C#
using(var command = connection.CreateCommand())
{
    command.CommandText = "INSERT INTO Customer (FirstName, LastName) VALUES ('Fred', 'Flintstone')";

    var recordsModified = command.ExecuteNonQuery();
}
```

### Execute a SQL statement that returns a scalar value
```C#
using(var command = connection.CreateCommand())
{
    command.CommandText = "SELECT COUNT(*) FROM Customer";

    var result = command.ExecuteScalar();
}
```

### Use input, output, and return parameters with a SQL query
```C#
// TODO 
```

### Execute a stored procedure and read response data
```C#
// TODO 
```

### Execute a stored procedure that returns no results
```C#
// TODO 
```

### Execute a stored procedure that returns a scalar value
```C#
// TODO 
```

### Use input, output, and return parameters with a stored procedure
```C#
// TODO 
```

### Dapper examples for all of the above
```C#
// TODO 
```