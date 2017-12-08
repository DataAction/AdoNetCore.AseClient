# AdoNetCore.AseClient - a .NET Core DB Provider for SAP ASE

Let's face it, accessing SAP (formerly Sybase) ASE from ADO.NET isn't great. The current .NET 4 version of the vendor's AseClient driver is a .NET Framework managed wrapper around SAP's unmanged [ADO DB provider](https://en.wikipedia.org/wiki/ActiveX_Data_Objects) and is dependent upon [COM](https://en.wikipedia.org/wiki/Component_Object_Model). COM is a Windows-only technology and will never be available to .NET Core.

Under the hood, ASE (and Microsoft Sql Server for that matter) use an application layer protocol called [Tabular Data Stream](https://en.wikipedia.org/wiki/Tabular_Data_Stream) to transfer data between the database server and the client application. ASE uses TDS 5.0.

This project provides a .NET Core native implementation of the TDS 5.0 protocol via an ADO.NET DB Provider, making SAP ASE accessible from .NET Core applications hosted on Windows, Linux, Docker and also serverless platforms like [AWS Lambda](https://aws.amazon.com/lambda/).

## Objectives
* Functional parity (eventually) with the `Sybase.AdoNet4.AseClient` provided by SAP. The following types will be supported:
    * AseClientFactory	- TODO
    * AseCommand - in progress
    * AseConnection - in progress
    * AseDataParameter - in progress
    * AseDataParameterCollection - in progress
    * AseDataReader - in progress
    * AseException - in progress
* Performance equivalent to or better than that of `Sybase.AdoNet4.AseClient` provided by SAP. This should be possible as we are eliminating the COM and OLE DB layers from this driver.
* Target all versions of .NET Core (1.0, 1.1, 2.0, and 2.1 when it is released)
* Should work with [Dapper](https://github.com/StackExchange/Dapper) at least as well as the .NET 4 client

## Note:
In theory, since we're implementing TDS 5.0, this client might work with other Sybase-produced databases, but the scope for now is just ASE.

## Suggested dev reference material
* [TDS 5.0 Functional Specification Version 3.8](http://www.sybase.com/content/1040983/Sybase-tds38-102306.pdf)
  * This spec is fairly complete, but it's got a few ??? entries -- if you can find a newer version of the spec to use, let its existence be known.
* `Sybase.AdoNet4.AseClient` wireshark packet captures
* `jTDS` if the above isn't informative enough (credit to them for figuring this out)

## Connection strings
[connectionstrings.com](https://www.connectionstrings.com/sybase-adaptive/) lists the following connection string properties for the ASE ADO.NET Data Provider. We aim to use identical connection string syntax to the SAP client, however our support for the various properties will be limited. Our support is as follows:

| Property                          | Support
| --------------------------------- |:---------:
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
| `DSURL`                           | TODO
| `EnableBulkLoad`                  | ?
| `EnableServerPacketSize`          | TODO
| `Encryption`                      | X
| `EncryptPassword`                 | ?
| `Enlist`                          | X
| `FetchArraySize`                  | TODO
| `HASession`                       | X
| `LoginTimeOut`                    | TODO
| `Max Pool Size`                   | &#10003;
| `Min Pool Size`                   | TODO
| `PacketSize`                      | TODO
| `Ping Server`                     | TODO
| `Pooling`                         | &#10003;
| `Port`                            | &#10003;
| `Pwd`                             | &#10003;
| `RestrictMaximum PacketSize`      | TODO
| `Secondary Data Source`           | X
| `Secondary Server Port`           | X
| `TextSize`                        | TODO
| `TightlyCoupledTransaction`       | X
| `TrustedFile`                     | X
| `Uid`                             | &#10003;
| `UseAseDecimal`                   | TODO
| `UseCursor`                       | ?

## Supported types
Send support:

| DbType                  | Send      | .NET Type(s) | Notes
| ----------------------- |:---------:| ------------ | -----
| `AnsiString`            | &#10003;  | `string`
| `Binary`                | &#10003;  | `byte[]`
| `Byte`                  | &#10003;  | `byte`
| `Boolean`               | &#10003;  | `bool`
| `Currency`              | &#10003;  | `decimal` | Sent as decimal type; may change to send as `TDS_MONEY`, which is shorter
| `Date`                  | &#10003;  | `DateTime` | Time component is ignored
| `DateTime`              | &#10003;  | `DateTime`
| `Decimal`               | &#10003;  | `decimal`
| `Double`                | &#10003;  | `double`
| `Guid`                  | X         | | Call `.ToByteArray()` and use `DbType.Binary` instead
| `Int16`                 | &#10003;  | `short`
| `Int32`                 | &#10003;  | `int`
| `Int64`                 | &#10003;  | `long`
| `Object`                | X         | | User should select a more appropriate type
| `SByte`                 | &#10003;  | `sbyte` | Sent as int16
| `Single`                | &#10003;  | `float`
| `String`                | &#10003;  | `string` | UTF-16 encoded, sent to server as binary with usertype `35`
| `Time`                  | &#10003;  | `TimeSpan`
| `UInt16`                | &#10003;  | `ushort`
| `UInt32`                | &#10003;  | `uint`
| `UInt64`                | &#10003;  | `ulong`
| `VarNumeric`            | &#10003;  | `decimal`
| `AnsiStringFixedLength` | &#10003;  | `string`
| `StringFixedLength`     | &#10003;  | `string` | UTF-16 encoded, sent to server as binary with usertype `34`
| `Xml`                   | X         | | User should select a more appropriate type
| `DateTime2`             | X         | | Use `DateTime` instead
| `DateTimeOffset`        | X         | | Use `DateTime` instead

Receive support:

| Type                | Receive   | .NET Type(s) | Notes
| ------------------- |:---------:| ------------ | -----
| `bigint`            | &#10003;  | `long`
| `int`               | &#10003;  | `int`
| `smallint`          | &#10003;  | `short`
| `tinyint`           | &#10003;  | `byte`
| `unsigned bigint`   | &#10003;  | `ulong`
| `unsigned int`      | &#10003;  | `uint`
| `unsigned smallint` | &#10003;  | `usmallint`
| `numeric`           | &#10003;  | `decimal`
| `decimal`           | &#10003;  | `decimal`
| `float`             | &#10003;  | `float`
| `double precision`  | &#10003;  | `double`
| `smallmoney`        | &#10003;  | `decimal`
| `money`             | &#10003;  | `decimal`
| `smalldatetime`     | &#10003;  | `DateTime`
| `datetime`          | &#10003;  | `DateTime`
| `date`              | &#10003;  | `DateTime`
| `time`              | &#10003;  | `TimeSpan`
| `bigdatetime`       | ?         | `DateTime` | Investigate: can we enable this type on our test server?
| `bigtime`           | ?         | `TimeSpan` | Investigate: can we enable this type on our test server?
| `char`              | &#10003;  | `string`
| `varchar`           | &#10003;  | `string`
| `unichar`           | &#10003;  | `string` | Server sends as binary with usertype `34`
| `univarchar`        | &#10003;  | `string` | Server sends as binary with usertype `35`
| `nchar`             | &#10003;  | `string`
| `nvarchar`          | &#10003;  | `string`
| `text`              | &#10003;  | `string`
| `unitext`           | &#10003;  | `string`
| `binary`            | &#10003;  | `byte[]`
| `varbinary`         | &#10003;  | `byte[]`
| `image`             | &#10003;  | `byte[]`
| `bit`               | &#10003;  | `bool`

## Flows/design
Roughly the flows will be (names not set in stone):

### Open a connection
`AseConnection` -Connection Request-> `ConnectionPoolManager` -Request-> `ConnectionPool` *"existing connection is grabbed, or new connection is created"*

`AseConnection` <-InternalConnection- `ConnectionPoolManager` <-InternalConnection- `ConnectionPool`

A database connection can be opened as follows:
```C#
var connectionString = "Data Source=myASEserver;Port=5000;Database=myDataBase;Uid=myUsername;Pwd=myPassword;";

using(var connection = new AseConnection(connectionString))
{
    connection.Open();

    // use the connection...
}
```

### Send a command and receive any response data
`AseCommand` -ADO.net stuff-> `InternalConnection` -Tokens-> `MemoryStream` -bytes-> `PacketChunkerSocket` *"command gets processed"*

`AseCommand` <-ADO.net stuff- `InternalConnection` <-Tokens- `MemoryStream` <-bytes- `PacketChunkerSocket`

A command can be executed in several different ways.

Retrieve multiple records using ADO.NET:
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

Update a single records using ADO.NET:
```C#
using(var command = connection.CreateCommand())
{
    command.CommandText = "INSERT INTO Customer (FirstName, LastName) VALUES ('Fred', 'Flintstone')";

    var recordsModified = command.ExecuteNonQuery();
}
```

### Release the connection (dispose)
`AseConnection` -Connection-> `ConnectionPoolManager` -Connection-> `ConnectionPool` *"connection released"*

## Plan
In general, for reasons of unit-testing, please create and implement interfaces.

* Setup project structure / files
* Implement ADO.net interfaces
* Connection string parsing
* Structure internal connections / pool management
  * Be wary of `USE DATABASE` calls, these are expensive and not necessary if the connection is already using the desired database.
* Introduce tokens and types by implementing:
  * Login / capability negotiation
  * Simple sql command call (`create procedure ...`)
  * Stored procedure call (`TDS_DBRPC`)
  * Simple queries (`select 1 as x...` with different types)
  * More advanced queries and procedure calls (i.e. with more parameters/types)

## Design points
SqlClient conventions
* SqlParameter vs AseDataParameter?
* SqlException has an Errors collection containing SqlError objects. It is derived from DbException.
* SqlDbType - we have no equivalent...
* We should avoid depending on Linq.
* Discuss the use of explicit interfaces as a general design principal.
* In general worth comparing each type for equivalence with the SqlClient for best practice.
* In general worth comparing each type for equivalence with the old AseClient for completeness of a drop in replacement.
* Async support would be sick.
