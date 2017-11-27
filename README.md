## Objectives
* Our intent is functional parity with the .net 4 client `Sybase.AdoNet4.AseClient`
  * EXCEPT: connection strings. We may not implement *all* options, but where we do, the option name will be the same
  * In theory, since we're implementing TDS 5.0, this client would work with other Sybase-produced databases, but the scope for now is just ASE
* Target all versions of .NET Core (1.0, 1.1, 2.0, 2.1)
* Should work with dapper at least as well as the .net 4 client
  * Hopefully we can improve it

## Suggested dev reference material
* `TDS 5.0 Functional Specification Version 3.8` (The filename is `Sybase-tds38-102306.pdf`)
  * If you can find a newer version of the spec to use, let its existence be known.
* `jTDS` - Java implementation of TDS (5.0, 7.0+)

## Flows/design

Roughly the flows will be (names not set in stone)

### Open a connection
`AseConnection` -Connection Request-> `ConnectionPoolManager` -Request-> `ConnectionPool` *"existing connection is grabbed, or new connection is created"*

`AseConnection` <-InternalConnection- `ConnectionPoolManager` <-InternalConnection- `ConnectionPool`

### Send a command and receive any response data
`AseCommand` -ADO.net stuff-> `InternalConnection` -Token Generation Commands-> `TdsTokenGenerator` -Tokens-> `TokenWriter` -bytes-> `DB` *"command gets processed"*

`AseCommand` <-ADO.net stuff- `InternalConnection` <-.NET stuff- `TdsTokenParser` <-Tokens- `TokenReader` <-bytes- `DB`

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
