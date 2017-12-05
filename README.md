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

| Property      | Support       |
| ------------- |:-------------:| 
| `AlternateServers` | X |
| `ApplicationName` | &#10003; |
| `BufferCacheSize` | TODO |
| `Charset` | &#10003; |
| `ClientHostName` | &#10003; |
| `ClientHostProc` | &#10003; |
| `CodePageType` | TODO |
| `Connection Lifetime` | TODO |
| `ConnectionIdleTimeout` | TODO |
| `CumulativeRecordCount` | TODO |
| `Database` | &#10003; |
| `Data Source` | &#10003; |
| `DistributedTransactionProtocol` | X |
| `DSURL` | TODO |
| `EnableBulkLoad` | ? |
| `EnableServerPacketSize` | TODO |
| `Encryption` | X |
| `EncryptPassword` | ? |
| `Enlist` | X |
| `FetchArraySize` | TODO |
| `HASession` | X |
| `LoginTimeOut` | TODO |
| `Max Pool Size` | TODO |
| `Min Pool Size` | TODO |
| `PacketSize` | TODO |
| `Ping Server` | TODO |
| `Pooling` | &#10003; |
| `Port` | &#10003; |
| `Pwd` | &#10003; |
| `RestrictMaximum PacketSize` | TODO |
| `Secondary Data Source` | X |
| `Secondary Server Port` | X |
| `TextSize` | TODO |
| `TightlyCoupledTransaction` | X |
| `TrustedFile` | X |
| `Uid` | &#10003; |
| `UseAseDecimal` | TODO |
| `UseCursor` | ? |

## Supported types
This is the list of types that ASE can emit.
If the type is marked with &#10003;, then the driver can read it.

For transmission of data to ASE, the driver will select an appropriate type, which will roughly line-up with what's supported below.

| Type    | Support     |
| ------- |:-----------:|
| `TDS_BINARY` | &#10003; |
| `TDS_BIT` | &#10003; |
| `TDS_CHAR` | &#10003; |
| `TDS_DATETIME` | &#10003; |
| `TDS_DATE` | &#10003; |
| `TDS_DATEN` | &#10003; |
| `TDS_SHORTDATE` | &#10003; |
| `TDS_DATETIMN` | &#10003; |
| `TDS_DECN` | &#10003; |
| `TDS_FLT4` | &#10003; |
| `TDS_FLT8` | &#10003; |
| `TDS_FLTN` | &#10003; |
| `TDS_IMAGE` | &#10003; |
| `TDS_INT1` | &#10003; |
| `TDS_INT2` | &#10003; |
| `TDS_INT4` | &#10003; |
| `TDS_INT8` | &#10003; |
| `TDS_INTN` | &#10003; |
| `TDS_INTERVAL` | ? |
| `TDS_SINT1` | &#10003; |
| `TDS_UINT2` | &#10003; |
| `TDS_UINT4` | &#10003; |
| `TDS_UINT8` | &#10003; |
| `TDS_UINTN` | &#10003; |
| `TDS_LONGBINARY` | &#10003; |
| `TDS_LONGCHAR` | &#10003; |
| `TDS_MONEY` | &#10003; |
| `TDS_SHORTMONEY` | &#10003; |
| `TDS_MONEYN` | &#10003; |
| `TDS_NUMN` | &#10003; |
| `TDS_SENSITIVITY` | &#10003; |
| `TDS_TIME` | &#10003; |
| `TDS_TIMEN` | &#10003; |
| `TDS_TEXT` | &#10003; |
| `TDS_UNITEXT` | TODO |
| `TDS_VARBINARY` | &#10003; |
| `TDS_VARCHAR` | &#10003; |
| `TDS_XML` | &#10003; |
| `TDS_BOUNDARY` | &#10003; |
| `TDS_BLOB` | TODO |

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
