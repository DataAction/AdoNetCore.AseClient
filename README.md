## Objectives
* Functional parity (eventually) with the .net 4 client `Sybase.AdoNet4.AseClient`, and to perform not too badly
* **Or** to obsolete this project in lieu of an equivalent or better vendor-provided client.
* Target all versions of .NET Core (1.0, 1.1, 2.0, and 2.1 when it is released)
* Should work with dapper at least as well as the .net 4 client
* There is an exception to the parity intent, **connection strings**: We may not implement *all* existing options, as some may not be relevant to our client
  * When we do implement a connection string option, the option name will be the same as the existing name

## Note:
In theory, since we're implementing TDS 5.0, this client might work with other Sybase-produced databases, but the scope for now is just ASE.

## Suggested dev reference material
* `TDS 5.0 Functional Specification Version 3.8` (http://www.sybase.com/content/1040983/Sybase-tds38-102306.pdf)
  * This spec is fairly complete, but it's got a few ??? entries -- if you can find a newer version of the spec to use, let its existence be known.
* `Sybase.AdoNet4.AseClient` wireshark packet captures
* `jTDS` if the above isn't informative enough (credit to them for figuring this out)

## Flows/design
Roughly the flows will be (names not set in stone):

### Open a connection
`AseConnection` -Connection Request-> `ConnectionPoolManager` -Request-> `ConnectionPool` *"existing connection is grabbed, or new connection is created"*

`AseConnection` <-InternalConnection- `ConnectionPoolManager` <-InternalConnection- `ConnectionPool`

### Send a command and receive any response data
`AseCommand` -ADO.net stuff-> `InternalConnection` -Tokens-> `MemoryStream` -bytes-> `PacketChunkerSocket` *"command gets processed"*

`AseCommand` <-ADO.net stuff- `InternalConnection` <-Tokens- `MemoryStream` <-bytes- `PacketChunkerSocket`

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
