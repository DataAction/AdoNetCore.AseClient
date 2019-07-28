using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient
{
    /// <summary>
    /// Represents an open connection to an ASE Server database. This class cannot be inherited.
    /// </summary>
    public sealed class AseConnection : DbConnection
#if CLONEABLE_INTERFACE
        , ICloneable
#endif
    {
        private static Func<IConnectionPoolManager> MakeConnectionPoolManager => () => new ConnectionPoolManager();

        private IInternalConnection _internal;
        private string _connectionString;
        private readonly IConnectionPoolManager _connectionPoolManager;
        private ConnectionState _state;
        private bool _isDisposed;
        private AseTransaction _transaction;
        private readonly IEventNotifier _eventNotifier;
        private bool? _namedParameters;

        /// <summary>
        /// Initializes a new instance of the <see cref="AseConnection" /> class.
        /// </summary>
        /// <remarks>
        /// <para>When a new instance of <see cref="AseConnection" /> is created, the read/write properties are set 
        /// to the following initial values unless they are specifically set using their associated keywords in the 
        /// <see cref="ConnectionString" /> property.</para>
        /// <para>
        ///     <list type="table">  
        ///         <listheader>  
        ///             <term>Properties</term> 
        ///             <term>Initial value</term> 
        ///         </listheader>  
        ///         <item>  
        ///             <description><see cref="ConnectionString" /></description>
        ///         </item>  
        ///         <item>  
        ///             <description>empty string ("")</description>
        ///         </item>    
        ///         <item>  
        ///             <description><see cref="ConnectionTimeout" /></description>
        ///         </item>  
        ///         <item>  
        ///             <description>30</description>
        ///         </item>
        ///         <item>  
        ///             <description><see cref="Database" /></description>
        ///         </item>  
        ///         <item>  
        ///             <description>empty string ("")</description>
        ///         </item>
        ///     </list>  
        /// </para>
        /// <para>
        /// </para>
        /// You can change the value for these properties only by using the <see cref="ConnectionString" /> property.
        /// </remarks>
        public AseConnection() : this(string.Empty, new ConnectionPoolManager())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AseConnection" /> class.
        /// </summary>
        /// <param name="connectionString">The connection used to open the ASE Server database.</param>
        /// <remarks>
        /// <para>When a new instance of <see cref="AseConnection" /> is created, the read/write properties are set 
        /// to the following initial values unless they are specifically set using their associated keywords in the 
        /// <see cref="ConnectionString" /> property.</para>
        /// <para>
        ///     <list type="table">  
        ///         <listheader>  
        ///             <term>Properties</term> 
        ///             <term>Initial value</term> 
        ///         </listheader>  
        ///         <item>  
        ///             <description><see cref="ConnectionString" /></description>
        ///         </item>  
        ///         <item>  
        ///             <description>empty string ("")</description>
        ///         </item>    
        ///         <item>  
        ///             <description><see cref="ConnectionTimeout" /></description>
        ///         </item>  
        ///         <item>  
        ///             <description>30</description>
        ///         </item>
        ///         <item>  
        ///             <description><see cref="Database" /></description>
        ///         </item>  
        ///         <item>  
        ///             <description>empty string ("")</description>
        ///         </item>
        ///     </list>  
        /// </para>
        /// <para>
        /// </para>
        /// You can change the value for these properties only by using the <see cref="ConnectionString" /> property.
        /// </remarks>
        public AseConnection(string connectionString) : this(connectionString, MakeConnectionPoolManager())
        {
        }

        internal AseConnection(string connectionString, IConnectionPoolManager connectionPoolManager)
        {
            ConnectionString = connectionString;
            InternalConnectionTimeout = 15; // Default to 15s as per the SAP AseClient http://infocenter.sybase.com/help/topic/com.sybase.infocenter.dc20066.1570100/doc/html/san1364409555258.html
            _connectionPoolManager = connectionPoolManager;
            _isDisposed = false;
            _eventNotifier = new EventNotifier(this);
        }

        /// <summary>
        /// Releases all resources used by the <see cref="AseConnection" />.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (_isDisposed)
            {
                return;
            }

            if (_transaction != null && !_transaction.IsDisposed)
            {
                _transaction.Dispose(); // Will also rollback the transaction
            }

            _transaction = null;

            try
            {
                Close();

                // Kill listening references that might keep this object from being garbage collected.
                _eventNotifier.ClearAll();
            }
            finally
            {
                _isDisposed = true;
            }
        }

        /// <summary>		
        /// Starts a database transaction.		
        /// </summary>		
        /// <param name="isolationLevel">The isolation level under which the transaction should run.</param>		
        /// <returns>An object representing the new transaction.</returns>		
        /// <remarks>		
        /// <para>This command maps to the SQL Server implementation of BEGIN TRANSACTION.</para>		
        /// <para>You must explicitly commit or roll back the transaction using the <see cref="AseTransaction.Commit" /> 		
        /// or <see cref="AseTransaction.Rollback" /> method. To make sure that the .NET Framework Data Provider for ASE 		
        /// transaction management model performs correctly, avoid using other transaction management models, such as the 		
        /// one provided by ASE.</para>		
        /// <para>If you do not specify an isolation level, the default isolation level is used. To specify an isolation 		
        /// level with the <see cref="BeginTransaction" /> method, use the overload that takes the iso parameter 		
        /// (<see cref="BeginTransaction(IsolationLevel)" />).</para>		
        /// </remarks>
        public new AseTransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified)
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(AseConnection));
            }

            Open();
            _transaction = new AseTransaction(this, isolationLevel);
            _transaction.Begin();
            return _transaction;
        }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            return BeginTransaction(isolationLevel);
        }

        /// <summary>
        /// Changes the current database for an open <see cref="AseConnection" />.
        /// </summary>
        /// <param name="databaseName">The name of the database to use instead of the current database.</param>
        /// <remarks>
        /// The value supplied in the <i>database</i> parameter must be a valid database name. The <i>database</i> parameter 
        /// cannot contain a null value, an empty string, or a string with only blank characters.
        /// </remarks>
        public override void ChangeDatabase(string databaseName)
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(AseConnection));
            }

            if (_internal != null && State == ConnectionState.Open)
            {
                _internal.ChangeDatabase(databaseName);
            }
            else
            {
                throw new InvalidOperationException("The Database cannot be changed unless the connection is open.");
            }
        }

        /// <summary>
        /// TODO - document this once transactions are supported.
        /// </summary>
        public override void Close()
        {
            if (State == ConnectionState.Closed)
            {
                return;
            }

            _connectionPoolManager.Release(_connectionString, _internal);
            _internal = null;
            InternalState = ConnectionState.Closed;
        }

        /// <summary>
        /// Creates and returns an <see cref="AseCommand" /> object associated with the <see cref="AseConnection" />.
        /// </summary>
        /// <returns>An <see cref="AseCommand" /> object.</returns>
        public new AseCommand CreateCommand()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(AseConnection));
            }

            var aseCommand = new AseCommand(this);

            return aseCommand;
        }

        protected override DbCommand CreateDbCommand()
        {
            return CreateCommand();
        }

        /// <summary>
        /// Opens a database connection with the property settings specified by the <see cref="ConnectionString" />.
        /// </summary>
        /// <remarks>
        /// The <see cref="AseConnection" /> draws an open connection from the connection pool if one is available. 
        /// Otherwise, it establishes a new connection to an instance of ASE.
        /// </remarks>
        public override void Open()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(AseConnection));
            }

            if (State == ConnectionState.Open)
            {
                return; //already open
            }

            if (State != ConnectionState.Closed)
            {
                throw new InvalidOperationException("Cannot open a connection which is not closed");
            }

            InternalState = ConnectionState.Connecting;

            var parameters = ConnectionParameters.Parse(_connectionString);

            _internal = _connectionPoolManager.Reserve(_connectionString, parameters, _eventNotifier);

            InternalConnectionTimeout = parameters.LoginTimeout;

            InternalState = ConnectionState.Open;
        }

        /// <summary>
        /// Gets or sets the string used to open a connection to an ASE database.
        /// </summary>
        public override string ConnectionString
        {
            get => _connectionString;
            set
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException(nameof(AseConnection));
                }

                if (State == ConnectionState.Closed)
                {
                    _connectionString = value;
                }
                else
                {
                    throw new InvalidOperationException("The connection string cannot be modified unless it is closed.");
                }
            }
        }

        /// <summary>
        /// Gets the time to wait while trying to establish a connection before terminating the attempt and generating an error.
        /// </summary>
        /// <remarks>
        /// You can set the amount of time a connection waits to time out by using the <b>LoginTimeOut</b> keyword in the connection 
        /// string. A value of 0 indicates no limit, and should be avoided in a <see cref="ConnectionString" /> because an attempt to 
        /// connect waits indefinitely.
        /// </remarks>
        public override int ConnectionTimeout => InternalConnectionTimeout;
        private int InternalConnectionTimeout { get; set; }

        /// <summary>
        /// Gets the name of the current database or the database to be used after a connection is opened.
        /// </summary>
        /// <remarks>
        /// The Database property updates dynamically. If you change the current database using a Transact-SQL 
        /// statement or the <see cref="ChangeDatabase(string)" /> method, an informational message is sent and 
        /// the property is updated automatically.
        /// </remarks>
        public override string Database => _internal?.Database;

        /// <summary>
        /// Gets the name of the current server
        /// </summary>
        public override string DataSource => _internal?.DataSource;

        /// <summary>
        /// Gets the version of the current server
        /// </summary>
        public override string ServerVersion => _internal?.ServerVersion;

        private string CanonicalServerVersion
        {
            get
            {
                var serverVersion = ServerVersion;

                if(string.IsNullOrWhiteSpace(serverVersion))
                {
                    return serverVersion;
                }

                var parts = serverVersion.Split('.');
                for (var i = 0; i < parts.Length; i++)
                {
                    parts[i] = parts[i].PadLeft(3, '0');
                }

                return string.Join(".", parts);
            }
        }


#if DB_PROVIDERFACTORY
        /// <summary>
        /// The DbProviderFactory available for creating child types.
        /// </summary>
        protected override DbProviderFactory DbProviderFactory => AseClientFactory.Instance;
#endif

        /// <summary>
        /// Indicates the state of the <see cref="AseConnection" /> during the most recent network operation 
        /// performed on the connection.
        /// </summary>
        /// <remarks>
        /// Returns a <see cref="System.Data.ConnectionState" /> enumeration indicating the state of the 
        /// <see cref="AseConnection" />. Closing and reopening the connection will refresh the value of State.
        /// </remarks>
        public override ConnectionState State => InternalState;
        private ConnectionState InternalState
        {
            get => _state;
            set
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException(nameof(AseConnection));
                }

                if (_state != value)
                {
                    var oldState = _state;
                    _state = value;

                    _eventNotifier.NotifyStateChange(oldState, _state);
                }
            }
        }

        internal IInternalConnection InternalConnection
        {
            get
            {
                if (State != ConnectionState.Open || _internal == null)
                {
                    throw new InvalidOperationException("Cannot execute on a connection which is not open");
                }
                return _internal;
            }
        }

        /// <summary>
        /// Occurs when Adaptive Server ADO.NET Data Provider sends a warning or an informational message.
        /// </summary>
        /// <remarks>
        /// The event handler receives an argument of type AseInfoMessageEventArgs containing data related to this event. 
        /// The Errors and Message properties provide information specific to this event.
        /// </remarks>
        public event AseInfoMessageEventHandler InfoMessage
        {
            add
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException(nameof(AseConnection));
                }
                _eventNotifier.InfoMessage += value;
            }
            remove
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException(nameof(AseConnection));
                }
                _eventNotifier.InfoMessage -= value;
            }
        }

        /// <summary>
        /// Occurs when the state of the connection changes.
        /// </summary>
        /// <remarks>
        /// The event handler receives an argument of StateChangeEventArgs with data related to this event. Two StateChangeEventArgs properties 
        /// provide information specific to this event: CurrentState and OriginalState.
        /// </remarks>
        public override event StateChangeEventHandler StateChange
        {
            add
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException(nameof(AseConnection));
                }
                _eventNotifier.StateChange += value;
            }
            remove
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException(nameof(AseConnection));
                }
                _eventNotifier.StateChange -= value;
            }
        }

        /// <summary>
        /// Traces database activity within an application for debugging.
        /// </summary>
        /// <remarks>
        /// <para>Use TraceEnter and TraceExit events to hook up your own tracing method. This event is unique to an 
        /// instance of a connection. This allows different connections to be logged to different files. It can ignore 
        /// the event, or you can program it for other tracing. In addition, by using a .NET event, you can set up more 
        /// than one event handler for a single connection object. This enables you to log the event to both a window 
        /// and a file at the same time.</para>
        /// <para>Enable the ENABLETRACING connection property to trace Adaptive Server ADO.NET Data Provider activities. 
        /// It is disabled by default to allow for better performance during normal execution where tracing is not needed. 
        /// When this property is disabled, the TraceEnter and TraceExit events are not triggered, and tracing events are 
        /// not executed. You can configure ENABLETRACING in the connection string using these values: True – triggers the 
        /// TraceEnter and TraceExit events; and False – the default value; Adaptive Server ADO.NET Data Provider ignores 
        /// the TraceEnter and TraceExit events.</para>
        /// </remarks>
        public event TraceEnterEventHandler TraceEnter
        {
            add
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException(nameof(AseConnection));
                }
                _eventNotifier.TraceEnter += value;
            }
            remove
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException(nameof(AseConnection));
                }
                _eventNotifier.TraceEnter -= value;
            }
        }

        /// <summary>
        /// Traces database activity within an application for debugging.
        /// </summary>
        /// <remarks>
        /// <para>Use TraceEnter and TraceExit events to hook up your own tracing method. This event is unique to an 
        /// instance of a connection. This allows different connections to be logged to different files. It can ignore 
        /// the event, or you can program it for other tracing. In addition, by using a .NET event, you can set up more 
        /// than one event handler for a single connection object. This enables you to log the event to both a window 
        /// and a file at the same time.</para>
        /// <para>Enable the ENABLETRACING connection property to trace Adaptive Server ADO.NET Data Provider activities. 
        /// It is disabled by default to allow for better performance during normal execution where tracing is not needed. 
        /// When this property is disabled, the TraceEnter and TraceExit events are not triggered, and tracing events are 
        /// not executed. You can configure ENABLETRACING in the connection string using these values: True – triggers the 
        /// TraceEnter and TraceExit events; and False – the default value; Adaptive Server ADO.NET Data Provider ignores 
        /// the TraceEnter and TraceExit events.</para>
        /// </remarks>
        public event TraceExitEventHandler TraceExit
        {
            add
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException(nameof(AseConnection));
                }
                _eventNotifier.TraceExit += value;
            }
            remove
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException(nameof(AseConnection));
                }
                _eventNotifier.TraceExit -= value;
            }
        }

        /// <summary>
        /// Governs the default behavior of the AseCommand objects associated with this connection.
        /// </summary>
        /// <remarks>
        /// This can be either set by the ConnectionString (NamedParameters='true'/'false') or the user can set it directly through an instance of AseConnection.
        /// </remarks>
        public bool NamedParameters
        {
            get
            {
                if(_namedParameters.HasValue)
                {
                    return _namedParameters.Value;
                }
                if(_internal != null)
                {
                    return _internal.NamedParameters;
                }

                return true;
            }
            set => _namedParameters = value;
        }

#if CLONEABLE_INTERFACE
        public object Clone()
        {
            return new AseConnection(_connectionString, _connectionPoolManager);
        }
#endif

        public void ClearPool()
        {
            _connectionPoolManager.ClearPool(_connectionString);
        }

        public static void ClearPools()
        {
            MakeConnectionPoolManager().ClearPools();
        }

        public bool IsCaseSensitive()
        {
            int result;
            using (var command = CreateCommand())
            {
                command.CommandType = CommandType.Text;
                command.CommandText = "SELECT value FROM master.dbo.sysconfigures WHERE name = 'default sortorder id'";

                result = (int)command.ExecuteScalar();
            }

            switch (result)
            {
                case 39:
                case 42:
                case 44:
                case 46:
                case 48:
                case 52:
                case 53:
                case 54:
                case 56:
                case 57:
                case 59:
                case 64:
                case 70:
                case 71:
                case 73:
                case 74:
                    return false;
                default:
                    return true;
            }
        }

        public IDictionary RetrieveStatistics()
        {
            return _internal?.RetrieveStatistics() ?? Internal.InternalConnection.EmptyStatistics;
        }

        public bool StatisticsEnabled
        {
            get => _internal?.StatisticsEnabled ?? false;
            set
            {
                if (_internal != null)
                {
                    _internal.StatisticsEnabled = value;
                }
            }
        }

        public AseTransaction Transaction
        {
            get
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException(nameof(AseConnection));
                }

                return _transaction;
            }
        }

#if DB_GETSCHEMA
        /// <summary>
        /// Returns schema information for the data source of this <see cref="AseConnection"/>.
        /// </summary>
        /// <returns>A <see cref="DataTable"/> that contains schema information.</returns>
        public override DataTable GetSchema()
        {
            var result = new DataTable {TableName = "MetaDataCollections"};

            // https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/common-schema-collections#metadatacollections
            result.Columns.Add("CollectionName", typeof(string));
            result.Columns.Add("NumberOfRestrictions", typeof(int));
            result.Columns.Add("NumberOfIdentifierParts", typeof(int));

            result.BeginLoadData();
            result.LoadDataRow(new object[] { "MetaDataCollections", 0, 0 }, true);
            result.LoadDataRow(new object[] { "DataSourceInformation", 0, 0 }, true);
            result.LoadDataRow(new object[] { "DataTypes", 0, 0 }, true);
            result.LoadDataRow(new object[] { "Restrictions", 0, 0 }, true);
            result.LoadDataRow(new object[] { "ReservedWords", 0, 0 }, true);
            result.LoadDataRow(new object[] { "Users", 1, 1 }, true);
            result.LoadDataRow(new object[] { "Databases", 1, 1 }, true);
            result.LoadDataRow(new object[] { "Tables", 4, 3 }, true);
            result.LoadDataRow(new object[] { "Columns", 4, 4 }, true);
            result.LoadDataRow(new object[] { "Views", 3, 3 }, true);
            result.LoadDataRow(new object[] { "ViewColumns", 4, 4 }, true);
            result.LoadDataRow(new object[] { "ProcedureParameters", 4, 1 }, true);
            result.LoadDataRow(new object[] { "Procedures", 4, 3 }, true);
            result.LoadDataRow(new object[] { "ForeignKeys", 4, 3 }, true);
            result.LoadDataRow(new object[] { "IndexColumns", 5, 4 }, true);
            result.LoadDataRow(new object[] { "Indexes", 4, 3 }, true);
            //result.LoadDataRow(new object[] { "UserDefinedTypes", 2, 1 }, true);
            result.EndLoadData();
            
            return result;
        }

        /// <summary>
        /// Returns schema information for the data source of this <see cref="AseConnection"/> using the specified string for the schema name.
        /// </summary>
        /// <param name="collectionName">The name of the collection to retrieve detailed results for.</param>
        /// <returns>A <see cref="DataTable"/> that contains schema information.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="collectionName"/> is null, or does not represent a supported schema collection.</exception>
        public override DataTable GetSchema(string collectionName)
        {
            return GetSchema(collectionName, new string[0]);
        }

        /// <summary>
        /// Returns schema information for the data source of this <see cref="AseConnection"/> using the specified string for the schema name
        /// and the specified string array for the restriction values..
        /// </summary>
        /// <remarks>
        ///     <para>
        ///     The restrictionValues parameter can supply n depth of values, which are specified by the restrictions collection for a
        ///     specific collection. In order to set values on a given restriction, and not set the values of other restrictions, you need
        ///     to set the preceding restrictions to null and then put the appropriate value in for the restriction that you would like to
        ///     specify a value for.
        ///     </para>
        ///     <para>
        ///     An example of this is the "Tables" collection.If the "Tables" collection has three restrictions (database,
        ///     owner, and table name) and you want to get back only the tables associated with the owner "Carl", you must pass in
        ///     the following values at least: null, "Carl". If a restriction value is not passed in, the default values are used
        ///     for that restriction. This is the same mapping as passing in null, which is different from passing in an empty string
        ///     for the parameter value.In that case, the empty string ("") is considered to be the value for the specified parameter.
        ///     </para>
        /// </remarks>
        /// <param name="collectionName">The name of the collection to retrieve detailed results for.</param>
        /// <param name="restrictionValues">Specifies a set of restriction values for the requested schema.</param>
        /// <returns>A <see cref="DataTable"/> that contains schema information.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="collectionName"/> is null, or does not represent a supported schema collection.</exception>
        public override DataTable GetSchema(string collectionName, string[] restrictionValues)
        {
            if (collectionName == null)
            {
                throw new AseException("Wrong CollectionName");
            }

            var metaDataCollections = GetSchema();

            foreach (DataRow row in metaDataCollections.Rows)
            {
                if (!string.Equals(row["CollectionName"] as string, collectionName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (restrictionValues.Length > (int)row["NumberOfRestrictions"])
                {
                    throw new AseException($"More restrictions were provided than the requested schema ('{collectionName}') supports");
                }
            }

            switch (collectionName)
            {
                case "MetaDataCollections":
                    return metaDataCollections;

                case "DataSourceInformation":
                    return GetDataSourceInformationSchema();

                case "DataTypes":
                    return GetDataTypesSchema();

                case "Restrictions":
                    return GetRestrictionsSchema();

                case "ReservedWords":
                    return GetReservedWordsSchema();

                case "Users":
                    return GetUsersSchema(restrictionValues);

                case "Databases":
                    return GetDatabasesSchema(restrictionValues);

                case "Tables":
                    return GetTablesSchema(restrictionValues);

                case "Columns":
                    return GetColumnsSchema(restrictionValues);

                case "Views":
                    return GetViewsSchema(restrictionValues);

                case "ViewColumns":
                    return GetViewColumnsSchema(restrictionValues);

                case "ProcedureParameters":
                    return GetProcedureParametersSchema(restrictionValues);

                case "Procedures":
                    return GetProceduresSchema(restrictionValues);

                case "ForeignKeys":
                    return GetForeignKeysSchema(restrictionValues);

                case "IndexColumns":
                    return GetIndexColumnsSchema(restrictionValues);

                case "Indexes":
                    return GetIndexesSchema(restrictionValues);

                case "UserDefinedTypes":
                    throw new NotImplementedException(); // Consistent with the reference driver.

                default:
                    throw new AseException($"The requested collection ('{collectionName}') is not defined.");
            }
        }

        private DataTable GetIndexesSchema(string[] restrictionValues)
        {
            throw new NotImplementedException();
        }

        private DataTable GetIndexColumnsSchema(string[] restrictionValues)
        {
            throw new NotImplementedException();
        }

        private DataTable GetForeignKeysSchema(string[] restrictionValues)
        {
            Open(); // Ensure the connection is open.

            using (var foreignKeysCommand = CreateCommand())
            {
                const string procName = "sp_oledb_fkeys";

                foreignKeysCommand.NamedParameters = true;
                foreignKeysCommand.CommandType = CommandType.Text;
                foreignKeysCommand.CommandText = $"SELECT CASE WHEN EXISTS(SELECT 1 FROM sybsystemprocs.dbo.sysobjects WHERE name = '{procName}') THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END";

                var exists = (bool)foreignKeysCommand.ExecuteScalar();

                if (!exists)
                {
                    throw new AseException($"Missing system stored procedure '{procName}'.");
                }

                foreignKeysCommand.CommandType = CommandType.StoredProcedure;
                foreignKeysCommand.CommandText = procName;

                if (restrictionValues.Length > 0)
                {
                    foreignKeysCommand.Parameters.Add("@pktable_name", AseDbType.VarChar, byte.MaxValue).Value = restrictionValues[0];
                }
                if (restrictionValues.Length > 1)
                {
                    foreignKeysCommand.Parameters.Add("@pktable_owner", AseDbType.VarChar, byte.MaxValue).Value = restrictionValues[1];
                }
                if (restrictionValues.Length > 2)
                {
                    foreignKeysCommand.Parameters.Add("@pktable_qualifier", AseDbType.VarChar, byte.MaxValue).Value = restrictionValues[2];
                }
                if (restrictionValues.Length > 3)
                {
                    foreignKeysCommand.Parameters.Add("@fktable_name", AseDbType.VarChar, byte.MaxValue).Value = restrictionValues[3];
                }
                if (restrictionValues.Length > 4)
                {
                    foreignKeysCommand.Parameters.Add("@fktable_owner", AseDbType.VarChar, byte.MaxValue).Value = restrictionValues[4];
                }
                if (restrictionValues.Length > 5)
                {
                    foreignKeysCommand.Parameters.Add("@fktable_qualifier", AseDbType.VarChar, byte.MaxValue).Value = restrictionValues[5];
                }
                
                using (var reader = foreignKeysCommand.ExecuteReader())
                {
                    var constraintCatalogOrdinal = reader.GetOrdinal("FK_TABLE_CATALOG");
                    var constraintSchemaOrdinal = reader.GetOrdinal("FK_TABLE_SCHEMA");
                    var constraintNameOrdinal = reader.GetOrdinal("FK_NAME");
                    var tableCatalogOrdinal = reader.GetOrdinal("PK_TABLE_CATALOG");
                    var tableSchemaOrdinal = reader.GetOrdinal("PK_TABLE_SCHEMA");
                    var tableNameOrdinal = reader.GetOrdinal("FK_TABLE_NAME");

                    var result = new DataTable("ForeignKeys");
                    result.Columns.Add("CONSTRAINT_CATALOG", typeof(string));
                    result.Columns.Add("CONSTRAINT_SCHEMA", typeof(string));
                    result.Columns.Add("CONSTRAINT_NAME", typeof(string));
                    result.Columns.Add("TABLE_CATALOG", typeof(string));
                    result.Columns.Add("TABLE_SCHEMA", typeof(string));
                    result.Columns.Add("TABLE_NAME", typeof(string));
                    result.Columns.Add("CONSTRAINT_TYPE", typeof(string));
                    result.Columns.Add("IS_DEFERRABLE", typeof(string));
                    result.Columns.Add("INITIALLY_DEFERRED", typeof(string));
                    result.BeginLoadData();

                    while (reader.Read())
                    {
                        result.Rows.Add(
                            reader.GetString(constraintCatalogOrdinal),
                            reader.GetString(constraintSchemaOrdinal),
                            reader.GetString(constraintNameOrdinal),
                            reader.GetString(tableCatalogOrdinal),
                            reader.GetString(tableSchemaOrdinal),
                            reader.GetString(tableNameOrdinal),
                            "FOREIGN KEY",
                            "NO",
                            "NO");
                    }

                    result.EndLoadData();

                    result.AcceptChanges();

                    return result;
                }
            }
        }

        private DataTable GetProceduresSchema(string[] restrictionValues)
        {
            throw new NotImplementedException();
        }

        private DataTable GetProcedureParametersSchema(string[] restrictionValues)
        {
            throw new NotImplementedException();
        }

        private DataTable GetViewColumnsSchema(string[] restrictionValues)
        {
            throw new NotImplementedException();
        }

        private DataTable GetViewsSchema(string[] restrictionValues)
        {
            Open(); // Ensure the connection is open.

            using (var viewCommand = CreateCommand())
            {
                viewCommand.NamedParameters = true;
                viewCommand.CommandType = CommandType.Text;
                if (restrictionValues.Length == 0)
                {
                    viewCommand.CommandText =
@"SELECT
    DB_NAME() AS TABLE_CATALOG,
    USER_NAME(o.uid) AS TABLE_SCHEMA,
    o.name AS TABLE_NAME,
    'NONE' AS CHECK_OPTION,
    'NO' AS IS_UPDATABLE
FROM
    sysobjects o
WHERE
    o.type IN ('V')
ORDER BY
    TABLE_CATALOG,
    TABLE_SCHEMA,
    TABLE_NAME";
                }
                else if (restrictionValues.Length == 1)
                {
                    viewCommand.CommandText =
@"SELECT
    DB_NAME() AS TABLE_CATALOG,
    USER_NAME(o.uid) AS TABLE_SCHEMA,
    o.name AS TABLE_NAME,
    'NONE' AS CHECK_OPTION,
    'NO' AS IS_UPDATABLE
FROM
    sysobjects o
WHERE
    o.type IN ('V')
    AND DB_NAME() = @databaseName
ORDER BY
    TABLE_CATALOG,
    TABLE_SCHEMA,
    TABLE_NAME";
                    viewCommand.Parameters.Add("@databaseName", restrictionValues[0]);
                }
                else if(restrictionValues.Length == 2)
                {
                    viewCommand.CommandText =
@"SELECT
    DB_NAME() AS TABLE_CATALOG,
    USER_NAME(o.uid) AS TABLE_SCHEMA,
    o.name AS TABLE_NAME,
    'NONE' AS CHECK_OPTION,
    'NO' AS IS_UPDATABLE
FROM
    sysobjects o
WHERE
    o.type IN ('V')
    AND DB_NAME() = @databaseName
    AND USER_NAME() = @schemaName
ORDER BY
    TABLE_CATALOG,
    TABLE_SCHEMA,
    TABLE_NAME";
                    viewCommand.Parameters.Add("@databaseName", restrictionValues[0]);
                    viewCommand.Parameters.Add("@schemaName", restrictionValues[1]);
                }
                else
                {
                    viewCommand.CommandText =
@"SELECT
    DB_NAME() AS TABLE_CATALOG,
    USER_NAME(o.uid) AS TABLE_SCHEMA,
    o.name AS TABLE_NAME,
    'NONE' AS CHECK_OPTION,
    'NO' AS IS_UPDATABLE
FROM
    sysobjects o
WHERE
    o.type IN ('V')
    AND DB_NAME() = @databaseName
    AND USER_NAME() = @schemaName
    AND o.name = @viewName
ORDER BY
    TABLE_CATALOG,
    TABLE_SCHEMA,
    TABLE_NAME";
                    viewCommand.Parameters.Add("@databaseName", restrictionValues[0]);
                    viewCommand.Parameters.Add("@schemaName", restrictionValues[1]);
                    viewCommand.Parameters.Add("@viewName", restrictionValues[2]);
                }

                using (var reader = viewCommand.ExecuteReader())
                {
                    var result = new DataTable("Views");
                    result.BeginLoadData();
                    result.Load(reader, LoadOption.OverwriteChanges);
                    result.EndLoadData();

                    result.AcceptChanges();

                    return result;
                }
            }
        }

        private DataTable GetColumnsSchema(string[] restrictionValues)
        {
            Open(); // Ensure the connection is open.

            using (var columnsCommand = CreateCommand())
            {
                const string procName = "sp_oledb_columns";

                columnsCommand.NamedParameters = true;
                columnsCommand.CommandType = CommandType.Text;
                columnsCommand.CommandText = $"SELECT CASE WHEN EXISTS(SELECT 1 FROM sybsystemprocs.dbo.sysobjects WHERE name = '{procName}') THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END";

                var exists = (bool)columnsCommand.ExecuteScalar();

                if (!exists)
                {
                    throw new AseException($"Missing system stored procedure '{procName}'.");
                }

                columnsCommand.CommandType = CommandType.StoredProcedure;
                columnsCommand.CommandText = procName;

                if (restrictionValues.Length > 0)
                {
                    columnsCommand.Parameters.Add("@table_name", AseDbType.VarChar, 771).Value = restrictionValues[0];
                }
                if (restrictionValues.Length > 1)
                {
                    columnsCommand.Parameters.Add("@table_owner", AseDbType.VarChar, 32).Value = restrictionValues[1];
                }
                if (restrictionValues.Length > 2)
                {
                    columnsCommand.Parameters.Add("@table_qualifier", AseDbType.VarChar, 32).Value = restrictionValues[2];
                }
                if (restrictionValues.Length > 3)
                {
                    columnsCommand.Parameters.Add("@column_name", AseDbType.VarChar, 771).Value = restrictionValues[3];
                }

                columnsCommand.Parameters.Add("@is_ado", AseDbType.Integer).Value = 2;

                using (var reader = columnsCommand.ExecuteReader())
                {
                    var tableCatalogOrdinal = reader.GetOrdinal("TABLE_CATALOG");
                    var tableSchemaOrdinal = reader.GetOrdinal("TABLE_SCHEMA");
                    var tableNameOrdinal = reader.GetOrdinal("TABLE_NAME");
                    var columnNameOrdinal = reader.GetOrdinal("COLUMN_NAME");
                    var columnGuidOrdinal = reader.GetOrdinal("COLUMN_GUID");
                    var columnPropIdOrdinal = reader.GetOrdinal("COLUMN_PROPID");
                    var ordinalPositionOrdinal = reader.GetOrdinal("ORDINAL_POSITION");
                    var columnDefaultOrdinal = reader.GetOrdinal("COLUMN_DEFAULT");
                    var isNullableOrdinal = reader.GetOrdinal("IS_NULLABLE"); 
                    var typeNameOrdinal = reader.GetOrdinal("TYPE_NAME"); 
                    var typeGuidOrdinal = reader.GetOrdinal("TYPE_GUID");
                    var characterMaximumLengthOrdinal = reader.GetOrdinal("CHARACTER_MAXIMUM_LENGTH");
                    var characterOctetLengthOrdinal = reader.GetOrdinal("CHARACTER_OCTET_LENGTH");
                    var numericPrecisionOrdinal = reader.GetOrdinal("NUMERIC_PRECISION");
                    var numericPrecisionRadixOrdinal = reader.GetOrdinal("NUMERIC_PRECISION_RADIX");
                    var numericScaleOrdinal = reader.GetOrdinal("NUMERIC_SCALE"); 
                    var datetimePrecisionOrdinal = reader.GetOrdinal("DATETIME_PRECISION");
                    var characterSetCatalogOrdinal = reader.GetOrdinal("CHARACTER_SET_CATALOG");
                    var characterSetSchemaOrdinal = reader.GetOrdinal("CHARACTER_SET_SCHEMA");
                    var characterSetNameOrdinal = reader.GetOrdinal("CHARACTER_SET_NAME");
                    var collationCatalogOrdinal = reader.GetOrdinal("COLLATION_CATALOG");
                    var collationSchemaOrdinal = reader.GetOrdinal("COLLATION_SCHEMA");
                    var collationNameOrdinal = reader.GetOrdinal("COLLATION_NAME");
                    var domainCatalogOrdinal = reader.GetOrdinal("DOMAIN_CATALOG");
                    var domainSchemaOrdinal = reader.GetOrdinal("DOMAIN_SCHEMA");
                    var domainNameOrdinal = reader.GetOrdinal("DOMAIN_NAME");
                    var descriptionOrdinal = reader.GetOrdinal("DESCRIPTION");

                    var result = new DataTable("Columns");
                    result.Columns.Add("TABLE_CATALOG", typeof(string));
                    result.Columns.Add("TABLE_SCHEMA", typeof(string));
                    result.Columns.Add("TABLE_NAME", typeof(string));
                    result.Columns.Add("COLUMN_NAME", typeof(string));
                    result.Columns.Add("COLUMN_GUID", typeof(string));
                    result.Columns.Add("COLUMN_PROPID", typeof(int));
                    result.Columns.Add("ORDINAL_POSITION", typeof(int));
                    result.Columns.Add("COLUMN_DEFAULT", typeof(string));
                    result.Columns.Add("IS_NULLABLE", typeof(string));
                    result.Columns.Add("DATA_TYPE", typeof(string));
                    result.Columns.Add("TYPE_GUID", typeof(string));
                    result.Columns.Add("CHARACTER_MAXIMUM_LENGTH", typeof(int));
                    result.Columns.Add("CHARACTER_OCTET_LENGTH", typeof(int));
                    result.Columns.Add("NUMERIC_PRECISION", typeof(short));
                    result.Columns.Add("NUMERIC_PRECISION_RADIX", typeof(short));
                    result.Columns.Add("NUMERIC_SCALE", typeof(short));
                    result.Columns.Add("DATETIME_PRECISION", typeof(int));
                    result.Columns.Add("CHARACTER_SET_CATALOG", typeof(string));
                    result.Columns.Add("CHARACTER_SET_SCHEMA", typeof(string));
                    result.Columns.Add("CHARACTER_SET_NAME", typeof(string));
                    result.Columns.Add("COLLATION_CATALOG", typeof(string));
                    result.Columns.Add("COLLATION_SCHEMA", typeof(string));
                    result.Columns.Add("COLLATION_NAME", typeof(string));
                    result.Columns.Add("DOMAIN_CATALOG", typeof(string));
                    result.Columns.Add("DOMAIN_SCHEMA", typeof(string));
                    result.Columns.Add("DOMAIN_NAME", typeof(string));
                    result.Columns.Add("DESCRIPTION", typeof(string));
                    result.BeginLoadData();

                    while (reader.Read())
                    {
                        result.Rows.Add(
                            reader.GetString(tableCatalogOrdinal),
                            reader.IsDBNull(tableSchemaOrdinal) ? (object)DBNull.Value : reader.GetString(tableSchemaOrdinal),
                            reader.IsDBNull(tableNameOrdinal) ? (object)DBNull.Value : reader.GetString(tableNameOrdinal),
                            reader.IsDBNull(columnNameOrdinal) ? (object)DBNull.Value : reader.GetString(columnNameOrdinal),
                            reader.IsDBNull(columnGuidOrdinal) ? (object)DBNull.Value : reader.GetString(columnGuidOrdinal),
                            reader.IsDBNull(columnPropIdOrdinal) ? (object)DBNull.Value : reader.GetInt32(columnPropIdOrdinal),
                            reader.IsDBNull(ordinalPositionOrdinal) ? (object)DBNull.Value : reader.GetInt32(ordinalPositionOrdinal),
                            reader.IsDBNull(columnDefaultOrdinal) ? (object)DBNull.Value : reader.GetString(columnDefaultOrdinal),
                            reader.IsDBNull(isNullableOrdinal) || !reader.GetBoolean(isNullableOrdinal) ? "NO" : "YES",
                            reader.IsDBNull(typeNameOrdinal) ? (object)DBNull.Value : reader.GetString(typeNameOrdinal),
                            reader.IsDBNull(typeGuidOrdinal) ? (object)DBNull.Value : reader.GetString(typeGuidOrdinal),
                            reader.IsDBNull(characterMaximumLengthOrdinal) ? (object)DBNull.Value : reader.GetInt32(characterMaximumLengthOrdinal),
                            reader.IsDBNull(characterOctetLengthOrdinal) ? (object)DBNull.Value : reader.GetInt32(characterOctetLengthOrdinal),
                            reader.IsDBNull(numericPrecisionOrdinal) ? (object)DBNull.Value : reader.GetInt16(numericPrecisionOrdinal),
                            reader.IsDBNull(numericPrecisionRadixOrdinal) ? (object)DBNull.Value : reader.GetInt16(numericPrecisionRadixOrdinal),
                            reader.IsDBNull(numericScaleOrdinal) ? 0 : reader.GetInt16(numericScaleOrdinal),
                            reader.IsDBNull(datetimePrecisionOrdinal) ? (object)DBNull.Value : reader.GetInt32(datetimePrecisionOrdinal),
                            reader.IsDBNull(characterSetCatalogOrdinal) ? (object)DBNull.Value : reader.GetString(characterSetCatalogOrdinal),
                            reader.IsDBNull(characterSetSchemaOrdinal) ? (object)DBNull.Value : reader.GetString(characterSetSchemaOrdinal),
                            reader.IsDBNull(characterSetNameOrdinal) ? (object)DBNull.Value : reader.GetString(characterSetNameOrdinal),
                            reader.IsDBNull(collationCatalogOrdinal) ? (object)DBNull.Value : reader.GetString(collationCatalogOrdinal),
                            reader.IsDBNull(collationSchemaOrdinal) ? (object)DBNull.Value : reader.GetString(collationSchemaOrdinal),
                            reader.IsDBNull(collationNameOrdinal) ? (object)DBNull.Value : reader.GetString(collationNameOrdinal),
                            reader.IsDBNull(domainCatalogOrdinal) ? (object)DBNull.Value : reader.GetString(domainCatalogOrdinal),
                            reader.IsDBNull(domainSchemaOrdinal) ? (object)DBNull.Value : reader.GetString(domainSchemaOrdinal),
                            reader.IsDBNull(domainNameOrdinal) ? (object)DBNull.Value : reader.GetString(domainNameOrdinal),
                            reader.IsDBNull(descriptionOrdinal) ? (object)DBNull.Value : reader.GetString(descriptionOrdinal)
                        );
                    }

                    result.EndLoadData();

                    result.AcceptChanges();

                    return result;
                }
            }
        }

        private DataTable GetTablesSchema(string[] restrictionValues)
        {
            Open(); // Ensure the connection is open.

            using (var tablesCommand = CreateCommand())
            {
                const string procName = "sp_oledb_tables";

                tablesCommand.NamedParameters = true;
                tablesCommand.CommandType = CommandType.Text;
                tablesCommand.CommandText = $"SELECT CASE WHEN EXISTS(SELECT 1 FROM sybsystemprocs.dbo.sysobjects WHERE name = '{procName}') THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END";

                var exists = (bool)tablesCommand.ExecuteScalar();

                if (!exists)
                {
                    throw new AseException($"Missing system stored procedure '{procName}'.");
                }

                tablesCommand.CommandType = CommandType.StoredProcedure;
                tablesCommand.CommandText = procName;

                if (restrictionValues.Length > 0)
                {
                    tablesCommand.Parameters.Add("@table_catalog", AseDbType.VarChar, 32).Value = restrictionValues[0];
                }
                if (restrictionValues.Length > 1)
                {
                    tablesCommand.Parameters.Add("@table_schema", AseDbType.VarChar, 32).Value = restrictionValues[1];
                }
                if (restrictionValues.Length > 2)
                {
                    tablesCommand.Parameters.Add("@table_name", AseDbType.VarChar, 771).Value = restrictionValues[2];
                }
                if (restrictionValues.Length > 3)
                {
                    tablesCommand.Parameters.Add("@table_type", AseDbType.VarChar, 100).Value = restrictionValues[3];
                }

                using (var reader = tablesCommand.ExecuteReader())
                {
                    var result = new DataTable("Tables");
                    result.BeginLoadData();
                    result.Load(reader);

                    var tableTypeOrdinal = result.Columns["Table_Type"].Ordinal;

                    foreach (DataRow row in result.Rows)
                    {
                        var tableType = row[tableTypeOrdinal].ToString();

                        if(string.Equals(tableType, "TABLE", StringComparison.OrdinalIgnoreCase))
                        {
                            row[tableTypeOrdinal] = "BASE TABLE";
                        }
                    }
                    result.EndLoadData();

                    result.AcceptChanges();

                    return result;
                }
            }
        }

        private DataTable GetDatabasesSchema(string[] restrictionValues)
        {
            Open(); // Ensure the connection is open.

            using (var databaseCommand = CreateCommand())
            {
                databaseCommand.NamedParameters = true;
                databaseCommand.CommandType = CommandType.Text;
                if (restrictionValues.Length == 0)
                {
                    databaseCommand.CommandText =
@"SELECT 
    d.name AS database_name, 
    CONVERT(VARCHAR(254), NULL) AS description, 
    d.dbid, 
    d.crdate AS create_date
FROM
    master.dbo.sysdatabases d
ORDER BY
    d.dbid ASC";
                }
                else
                {
                    databaseCommand.CommandText =
@"SELECT 
    d.name AS database_name, 
    CONVERT(VARCHAR(254), NULL) AS description, 
    d.dbid, 
    d.crdate AS create_date
FROM
    master.dbo.sysdatabases d
WHERE d.name = @name
ORDER BY
    d.dbid ASC";
                    databaseCommand.Parameters.Add("@name", restrictionValues[0]);
                }

                using (var reader = databaseCommand.ExecuteReader())
                {
                    var result = new DataTable("Databases");
                    result.BeginLoadData();
                    result.Load(reader, LoadOption.OverwriteChanges);
                    result.EndLoadData();

                    result.AcceptChanges();

                    return result;
                }
            }
        }

        private DataTable GetUsersSchema(string[] restrictionValues)
        {
            Open(); // Ensure the connection is open.

            using (var userCommand = CreateCommand())
            {
                userCommand.NamedParameters = true;
                userCommand.CommandType = CommandType.Text;

                if (restrictionValues.Length == 0)
                {
                    userCommand.CommandText =
@"SELECT
    usr.uid,
    USER_NAME(usr.uid) AS user_name,
    l.accdate AS createdate,
    CONVERT(DATETIME,NULL) AS updatedate
FROM
    sysusers usr
    INNER JOIN master.dbo.syslogins l
        ON usr.suid = l.suid";
                }
                else
                {
                    userCommand.CommandText =
@"SELECT
    usr.uid,
    USER_NAME(usr.uid) AS user_name,
    l.accdate AS createdate,
    CONVERT(DATETIME,NULL) AS updatedate
FROM
    sysusers usr
    INNER JOIN master.dbo.syslogins l
        ON usr.suid = l.suid
WHERE
    USER_NAME(usr.uid) = @name";

                    userCommand.Parameters.Add("@name", restrictionValues[0]);
                }
                
                using (var reader = userCommand.ExecuteReader())
                {
                    var result = new DataTable("Users");
                    result.BeginLoadData();
                    result.Load(reader, LoadOption.OverwriteChanges);
                    result.EndLoadData();

                    result.AcceptChanges();

                    return result;
                }
            }
        }

        private DataTable GetReservedWordsSchema()
        {
            var result = new DataTable("ReservedWords");
            result.Columns.Add("ReservedWord", typeof(string));

            result.BeginLoadData();
            foreach (var word in _reservedWords)
            {
                result.LoadDataRow(new object[] {word}, true);
            }

            result.EndLoadData();

            result.AcceptChanges();

            return result;
        }

        private static DataTable GetRestrictionsSchema()
        {
            // https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/common-schema-collections#restrictions
            var result = new DataTable("Restrictions");
            result.Columns.Add("CollectionName", typeof(string));
            result.Columns.Add("RestrictionName", typeof(string));
            result.Columns.Add("ParameterName", typeof(string));
            result.Columns.Add("RestrictionDefault", typeof(string));
            result.Columns.Add("RestrictionNumber", typeof(int));
            result.BeginLoadData();
            result.LoadDataRow(new object[] {"Users", "User_Name", "@Name", "name", 1}, true);
            result.LoadDataRow(new object[] {"Databases", "Name", "@Name", "Name", 1}, true);
            result.LoadDataRow(new object[] {"Tables", "Catalog", "@Catalog", "TABLE_CATALOG", 1}, true);
            result.LoadDataRow(new object[] {"Tables", "Owner", "@Owner", "TABLE_SCHEMA", 2}, true);
            result.LoadDataRow(new object[] {"Tables", "Table", "@Name", "TABLE_NAME", 3}, true);
            result.LoadDataRow(new object[] {"Tables", "TableType", "@TableType", "TABLE_TYPE", 4}, true);
            result.LoadDataRow(new object[] {"Columns", "Catalog", "@Catalog", "TABLE_CATALOG", 1}, true);
            result.LoadDataRow(new object[] {"Columns", "Owner", "@Owner", "TABLE_SCHEMA", 2}, true);
            result.LoadDataRow(new object[] {"Columns", "Table", "@Table", "TABLE_NAME", 3}, true);
            result.LoadDataRow(new object[] {"Columns", "Column", "@Column", "COLUMN_NAME", 4}, true);
            result.LoadDataRow(new object[] {"Views", "Catalog", "@Catalog", "TABLE_CATALOG", 1}, true);
            result.LoadDataRow(new object[] {"Views", "Owner", "@Owner", "TABLE_SCHEMA", 2}, true);
            result.LoadDataRow(new object[] {"Views", "Table", "@Table", "TABLE_NAME", 3}, true);
            result.LoadDataRow(new object[] {"ViewColumns", "Catalog", "@Catalog", "VIEW_CATALOG", 1}, true);
            result.LoadDataRow(new object[] {"ViewColumns", "Owner", "@Owner", "VIEW_SCHEMA", 2}, true);
            result.LoadDataRow(new object[] {"ViewColumns", "Table", "@Table", "VIEW_NAME", 3}, true);
            result.LoadDataRow(new object[] {"ViewColumns", "Column", "@Column", "COLUMN_NAME", 4}, true);
            result.LoadDataRow(new object[] {"ProcedureParameters", "Catalog", "@Catalog", "SPECIFIC_CATALOG", 1}, true);
            result.LoadDataRow(new object[] {"ProcedureParameters", "Owner", "@Owner", "SPECIFIC_SCHEMA", 2}, true);
            result.LoadDataRow(new object[] {"ProcedureParameters", "Name", "@Name", "SPECIFIC_NAME", 3}, true);
            result.LoadDataRow(new object[] {"ProcedureParameters", "Parameter", "@Parameter", "PARAMETER_NAME", 4}, true);
            result.LoadDataRow(new object[] {"Procedures", "Catalog", "@Catalog", "SPECIFIC_CATALOG", 1}, true);
            result.LoadDataRow(new object[] {"Procedures", "Owner", "@Owner", "SPECIFIC_SCHEMA", 2}, true);
            result.LoadDataRow(new object[] {"Procedures", "Name", "@Name", "SPECIFIC_NAME", 3}, true);
            result.LoadDataRow(new object[] {"Procedures", "Type", "@Type", "ROUTINE_TYPE", 4}, true);
            result.LoadDataRow(new object[] {"IndexColumns", "Catalog", "@Catalog", "db_name()", 1}, true);
            result.LoadDataRow(new object[] {"IndexColumns", "Owner", "@Owner", "user_name()", 2}, true);
            result.LoadDataRow(new object[] {"IndexColumns", "Table", "@Table", "o.name", 3}, true);
            result.LoadDataRow(new object[] {"IndexColumns", "ConstraintName", "@ConstraintName", "x.name", 4}, true);
            result.LoadDataRow(new object[] {"IndexColumns", "Column", "@Column", "c.name", 5}, true);
            result.LoadDataRow(new object[] {"Indexes", "Catalog", "@Catalog", "db_name()", 1}, true);
            result.LoadDataRow(new object[] {"Indexes", "Owner", "@Owner", "user_name()", 2}, true);
            result.LoadDataRow(new object[] {"Indexes", "Table", "@Table", "o.name", 3}, true);
            result.LoadDataRow(new object[] {"Indexes", "Name", "@Name", "x.name", 4}, true);
            //result.LoadDataRow(new object[] {"UserDefinedTypes", "assembly_name", "@AssemblyName", "assemblies.name", 1}, true);
            //result.LoadDataRow(new object[] {"UserDefinedTypes", "udt_name", "@UDTName", "types.assembly_class", 2}, true);
            result.LoadDataRow(new object[] {"ForeignKeys", "Catalog", "@Catalog", "CONSTRAINT_CATALOG", 1}, true);
            result.LoadDataRow(new object[] {"ForeignKeys", "Owner", "@Owner", "CONSTRAINT_SCHEMA", 2}, true);
            result.LoadDataRow(new object[] {"ForeignKeys", "Table", "@Table", "TABLE_NAME", 3}, true);
            result.LoadDataRow(new object[] {"ForeignKeys", "Name", "@Name", "CONSTRAINT_NAME", 4}, true);

            
            result.EndLoadData();

            result.AcceptChanges();

            return result;
        }

        private DataTable GetDataTypesSchema()
        {
            // https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/common-schema-collections#datatypes
            var result = new DataTable("DataTypes");
            result.Columns.Add("TypeName", typeof(string));
            result.Columns.Add("ProviderDbType", typeof(int));
            result.Columns.Add("ColumnSize", typeof(long));
            result.Columns.Add("CreateFormat", typeof(string));
            result.Columns.Add("CreateParameters", typeof(string));
            result.Columns.Add("DataType", typeof(string));
            result.Columns.Add("IsAutoincrementable", typeof(bool));
            result.Columns.Add("IsBestMatch", typeof(bool));
            result.Columns.Add("IsCaseSensitive", typeof(bool));
            result.Columns.Add("IsFixedLength", typeof(bool));
            result.Columns.Add("IsFixedPrecisionScale", typeof(bool));
            result.Columns.Add("IsLong", typeof(bool));
            result.Columns.Add("IsNullable", typeof(bool));
            result.Columns.Add("IsSearchable", typeof(bool));
            result.Columns.Add("IsSearchableWithLike", typeof(bool));
            result.Columns.Add("IsUnsigned", typeof(bool));
            result.Columns.Add("MaximumScale", typeof(short));
            result.Columns.Add("MinimumScale", typeof(short));
            result.Columns.Add("IsConcurrencyType", typeof(string));
            result.Columns.Add("IsLiteralSupported", typeof(bool));
            result.Columns.Add("LiteralPrefix", typeof(string));
            result.Columns.Add("LiteralSuffix", typeof(string));
            //result.Columns.Add("NativeDataType", typeof(string)); // NativeDataType is an OLE DB specific column for exposing the OLE DB type of the data type .

            result.BeginLoadData();
            result.LoadDataTypeRow("bigdatetime", AseDbType.BigDateTime, 30, netType: typeof(DateTime), isFixedLength: true, isNullable: true, isSearchable: true, isSearchableWithLike: true, isLiteralSupported: true, literalPrefix: "'", literalSuffix: "'");
            result.LoadDataTypeRow("bigint", AseDbType.BigInt, 19, netType: typeof(long), isAutoIncrementable: true, isBestMatch: true, isFixedLength: true, isFixedPrecisionScale: true, isNullable: true, isSearchable: true);
            result.LoadDataTypeRow("binary", AseDbType.Binary, 8000, "binary({ 0 })", "length", typeof(byte[]), isFixedLength: true, isNullable: true, isSearchable: true, isLiteralSupported: true, literalPrefix: "0x");
            result.LoadDataTypeRow("bit", AseDbType.Bit, 1, netType: typeof(bool), isBestMatch: true, isFixedLength: true, isNullable: true, isSearchable: true);
            result.LoadDataTypeRow("char", AseDbType.Char, 2147483647, "char ({0})", "length", typeof(string), isCaseSensitive: false, isFixedLength: true, isNullable: true, isSearchable: true, isSearchableWithLike: true, isLiteralSupported: true, literalPrefix: "'", literalSuffix: "'");
            result.LoadDataTypeRow("char", AseDbType.Char, 2147483647, "char ({0})", "length", typeof(char[]), isCaseSensitive: false, isFixedLength: true, isNullable: true, isSearchable: true, isSearchableWithLike: true, isLiteralSupported: true, literalPrefix: "'", literalSuffix: "'");
            result.LoadDataTypeRow("date", AseDbType.Date, 17, netType: typeof(DateTime), isBestMatch: false, isFixedLength: true, isNullable: true, isSearchable: true, isSearchableWithLike: true, isLiteralSupported: true, literalPrefix: "'", literalSuffix: "'");
            result.LoadDataTypeRow("datetime", AseDbType.DateTime, 23, netType: typeof(DateTime), isBestMatch: true, isFixedLength: true, isNullable: true, isSearchable: true, isSearchableWithLike: true, isLiteralSupported: true, literalPrefix: "'", literalSuffix: "'");
            result.LoadDataTypeRow("decimal", AseDbType.Decimal, 38, "decimal ({0}, {1})", "precision,scale", typeof(decimal), isAutoIncrementable: true, isFixedLength: true, isNullable: true, isSearchable: true, maximumScale: 38, minimumScale: 0);
            result.LoadDataTypeRow("decimal", AseDbType.Decimal, 38, "decimal ({0}, {1})", "precision,scale", typeof(AseDecimal), true, true, isFixedLength: true, isNullable: true, isSearchable: true, maximumScale: 38, minimumScale: 0);
            result.LoadDataTypeRow("double precision", AseDbType.Double, 53, netType: typeof(double), isFixedLength: true, isNullable: true, isSearchable: true);
            result.LoadDataTypeRow("float", AseDbType.Double, 53, "float({ 0 })", "number of bits used to store the mantissa", typeof(double), isBestMatch: true, isFixedLength: true, isNullable: true, isSearchable: true);
            result.LoadDataTypeRow("float", AseDbType.Double, 53, "float({ 0 })", "number of bits used to store the mantissa", typeof(float), isBestMatch: true, isFixedLength: true, isNullable: true, isSearchable: true);
            result.LoadDataTypeRow("image", AseDbType.Image, 2147483647, netType: typeof(byte[]), isLong: true, isNullable: true, isLiteralSupported: true, literalPrefix: "0x");
            result.LoadDataTypeRow("int", AseDbType.Integer, 10, netType: typeof(int), isAutoIncrementable: true, isBestMatch: true, isFixedLength: true, isFixedPrecisionScale: true, isNullable: true, isSearchable: true);
            result.LoadDataTypeRow("money", AseDbType.Money, 19, netType: typeof(decimal), isFixedLength: true, isFixedPrecisionScale: true, isNullable: true, isSearchable: true);
            result.LoadDataTypeRow("nchar", AseDbType.NChar, 1073741823, "nchar({0})", "length", typeof(string), isCaseSensitive: false, isFixedLength: true, isNullable: true, isSearchable: true, isSearchableWithLike: true, isLiteralSupported: true, literalPrefix: "N'", literalSuffix: "'");
            result.LoadDataTypeRow("nchar", AseDbType.NChar, 1073741823, "nchar({0})", "length", typeof(char[]), isCaseSensitive: false, isFixedLength: true, isNullable: true, isSearchable: true, isSearchableWithLike: true, isLiteralSupported: true, literalPrefix: "N'", literalSuffix: "'");
            result.LoadDataTypeRow("numeric", AseDbType.Numeric, 38, "numeric({ 0}, {1})", "precision,scale", typeof(decimal), isAutoIncrementable: true, isBestMatch: true, isFixedLength: true, isNullable: true, isSearchable: true, maximumScale: 38, minimumScale: 0);
            result.LoadDataTypeRow("nvarchar", AseDbType.NVarChar, 1073741823, "nvarchar({0})", "max length", typeof(string), isCaseSensitive: false, isNullable: true, isSearchable: true, isSearchableWithLike: true, isLiteralSupported: true, literalPrefix: "N'", literalSuffix: "'");
            result.LoadDataTypeRow("nvarchar", AseDbType.NVarChar, 1073741823, "nvarchar({0})", "max length", typeof(char[]), isCaseSensitive: false, isNullable: true, isSearchable: true, isSearchableWithLike: true, isLiteralSupported: true, literalPrefix: "N'", literalSuffix: "'");
            result.LoadDataTypeRow("real", AseDbType.Real, 7, netType: typeof(float), isBestMatch: true, isFixedLength: true, isNullable: true, isSearchable: true);
            result.LoadDataTypeRow("smalldatetime", AseDbType.SmallDateTime, 16, netType: typeof(DateTime), isFixedLength: true, isNullable: true, isSearchable: true, isSearchableWithLike: true, isLiteralSupported: true, literalPrefix: "'", literalSuffix: "'");
            result.LoadDataTypeRow("smallint", AseDbType.SmallInt, 5,  netType: typeof(short), isAutoIncrementable: true, isBestMatch: true, isFixedLength: true, isFixedPrecisionScale: true, isNullable: true, isSearchable: true);
            result.LoadDataTypeRow("smallint", AseDbType.SmallInt, 5,  netType: typeof(sbyte), isAutoIncrementable: true, isBestMatch: true, isFixedLength: true, isFixedPrecisionScale: true, isNullable: true, isSearchable: true);
            result.LoadDataTypeRow("smallmoney", AseDbType.SmallMoney, 10, netType: typeof(decimal), isFixedLength: true, isFixedPrecisionScale: true, isNullable: true, isSearchable: true);
            result.LoadDataTypeRow("text", AseDbType.Text, 2147483647, netType: typeof(string), isLong: true, isNullable: true, isSearchableWithLike: true, isLiteralSupported: true, literalPrefix: "'",literalSuffix:"'");
            result.LoadDataTypeRow("text", AseDbType.Text, 2147483647, netType: typeof(char[]), isLong: true, isNullable: true, isSearchableWithLike: true, isLiteralSupported: true, literalPrefix: "'",literalSuffix:"'");
            result.LoadDataTypeRow("time", AseDbType.DateTime, 12, netType: typeof(TimeSpan), isBestMatch: true, isFixedLength: true, isNullable: true, isSearchable: true, isSearchableWithLike: true, isLiteralSupported: true, literalPrefix: "'", literalSuffix: "'");
            result.LoadDataTypeRow("timestamp", AseDbType.TimeStamp, 8, netType: typeof(byte[]), isFixedLength: true, isSearchable: true, isConcurrencyType: true, isLiteralSupported: true, literalPrefix: "0x");
            result.LoadDataTypeRow("tinyint", AseDbType.TinyInt, 3, netType: typeof(byte), isAutoIncrementable: true, isBestMatch: true, isFixedLength: true, isFixedPrecisionScale: true, isNullable: true, isSearchable: true, isUnsigned: true);
            result.LoadDataTypeRow("unichar", AseDbType.UniChar, 1073741823, "unichar({0})", "length", typeof(string), isCaseSensitive: false, isFixedLength: true, isNullable: true, isSearchable: true, isSearchableWithLike: true, isLiteralSupported: true, literalPrefix: "'", literalSuffix: "'");
            result.LoadDataTypeRow("unichar", AseDbType.UniChar, 1073741823, "unichar({0})", "length", typeof(char[]), isCaseSensitive: false, isFixedLength: true, isNullable: true, isSearchable: true, isSearchableWithLike: true, isLiteralSupported: true, literalPrefix: "'", literalSuffix: "'");
            result.LoadDataTypeRow("uniqueidentifier", AseDbType.VarBinary, 16, "varbinary({0})", "max length", typeof(Guid), isBestMatch: true, isNullable: true, isSearchable: true, isLiteralSupported: true, literalPrefix: "0x");
            result.LoadDataTypeRow("unitext", AseDbType.Text, 1073741823, netType: typeof(string), isLong: true, isNullable: true, isSearchableWithLike: true, isLiteralSupported: true, literalPrefix: "'", literalSuffix: "'");
            result.LoadDataTypeRow("unitext", AseDbType.Text, 1073741823, netType: typeof(char[]), isLong: true, isNullable: true, isSearchableWithLike: true, isLiteralSupported: true, literalPrefix: "'", literalSuffix: "'");
            result.LoadDataTypeRow("univarchar", AseDbType.NVarChar, 1073741823, "univarchar({0})", "max length", typeof(string), isCaseSensitive: false, isNullable: true, isSearchable: true, isSearchableWithLike: true, isLiteralSupported: true, literalPrefix: "'", literalSuffix: "'");
            result.LoadDataTypeRow("univarchar", AseDbType.NVarChar, 1073741823, "univarchar({0})", "max length", typeof(char[]), isCaseSensitive: false, isNullable: true, isSearchable: true, isSearchableWithLike: true, isLiteralSupported: true, literalPrefix: "'", literalSuffix: "'");
            result.LoadDataTypeRow("unsigned bigint", AseDbType.UnsignedBigInt, 19, netType: typeof(ulong), isAutoIncrementable: true, isBestMatch: true, isFixedLength: true, isFixedPrecisionScale: true, isNullable: true, isSearchable: true, isUnsigned: true);
            result.LoadDataTypeRow("unsigned int", AseDbType.Integer, 10, netType: typeof(uint), isAutoIncrementable: true, isBestMatch: true, isFixedLength: true, isFixedPrecisionScale: true, isNullable: true, isSearchable: true, isUnsigned: true);
            result.LoadDataTypeRow("unsigned smallint", AseDbType.SmallInt, 5, netType: typeof(ushort), isAutoIncrementable: true, isBestMatch: true, isFixedLength: true, isFixedPrecisionScale: true, isNullable: true, isSearchable: true, isUnsigned: true);
            result.LoadDataTypeRow("varbinary", AseDbType.VarBinary, 1073741823, "varbinary({0})", "max length", typeof(byte[]), isBestMatch: true, isNullable: true, isSearchable: true, isLiteralSupported: true, literalPrefix: "0x");
            result.LoadDataTypeRow("varchar", AseDbType.VarChar, 2147483647, "varchar({0})", "max length", typeof(string), isBestMatch: true, isCaseSensitive: false, isNullable: true, isSearchable: true, isSearchableWithLike: true, isLiteralSupported: true, literalPrefix: "'", literalSuffix: "'");
            result.LoadDataTypeRow("varchar", AseDbType.VarChar, 2147483647, "varchar({0})", "max length", typeof(char[]), isBestMatch: true, isCaseSensitive: false, isNullable: true, isSearchable: true, isSearchableWithLike: true, isLiteralSupported: true, literalPrefix: "'", literalSuffix: "'");
            // NOTE: wchar appears to be a SQLAnywhere type.
            // NOTE: wvarchar appears to be a SQLAnywhere type.
            // NOTE: longchar appears to be a Sybase IQ type.
            // NOTE: longvarchar appears to be a Sybase IQ type.
            result.EndLoadData();

            result.AcceptChanges();

            return result;
        }

        private DataTable GetDataSourceInformationSchema()
        {
            Open(); // For ServerVersion.

            // https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/common-schema-collections#datasourceinformation
            var result = new DataTable("DataSourceInformation");
            result.Columns.Add("CompositeIdentifierSeparatorPattern", typeof(string));
            result.Columns.Add("DataSourceProductName", typeof(string));
            result.Columns.Add("DataSourceProductVersion", typeof(string));
            result.Columns.Add("DataSourceProductVersionNormalized", typeof(string));
            result.Columns.Add("GroupByBehavior", typeof(int));
            result.Columns.Add("IdentifierPattern", typeof(string));
            result.Columns.Add("IdentifierCase", typeof(int));
            result.Columns.Add("OrderByColumnsInSelect", typeof(bool));
            result.Columns.Add("ParameterMarkerFormat", typeof(string));
            result.Columns.Add("ParameterMarkerPattern", typeof(string));
            result.Columns.Add("ParameterNameMaxLength", typeof(int));
            result.Columns.Add("ParameterNamePattern", typeof(string));
            result.Columns.Add("QuotedIdentifierPattern", typeof(string));
            result.Columns.Add("QuotedIdentifierCase", typeof(int));
            result.Columns.Add("StatementSeparatorPattern", typeof(string));
            result.Columns.Add("StringLiteralPattern", typeof(string));
            result.Columns.Add("SupportedJoinOperators", typeof(int));
            result.BeginLoadData();
            result.LoadDataRow(new object[]
            {
                "\\.",
                "Adaptive Server Enterprise",
                ServerVersion,
                CanonicalServerVersion,
                GroupByBehavior.MustContainAll,
                "(^\\[\\p{Lo}\\p{Lu}\\p{Ll}_@#][\\p{Lo}\\p{Lu}\\p{Ll}\\p{Nd}@$#_]*$)|(^\\[[^\\]\\0]|\\]\\]+\\]$)|(^\\\"[^\\\"\\0]|\\\"\\\"+\\\"$)",
                IsCaseSensitive() ? IdentifierCase.Sensitive : IdentifierCase.Insensitive,
                false,
                "@{0}",
                "@[\\p{Lo}\\p{Lu}\\p{Ll}\\p{Lm}_@#][\\p{Lo}\\p{Lu}\\p{Ll}\\p{Lm}\\p{Nd}\\uff3f_@#\\$]*(?=\\s+|$)",
                128,
                "^[\\p{Lo}\\p{Lu}\\p{Ll}\\p{Lm}_@#][\\p{Lo}\\p{Lu}\\p{Ll}\\p{Lm}\\p{Nd}\\uff3f_@#\\$]*(?=\\s+|$)",
                "(([^\\[]|\\]\\])*)",
                IdentifierCase.Insensitive,
                " ",
                "'(([^']|'')*)'",
                SupportedJoinOperators.FullOuter | SupportedJoinOperators.Inner | SupportedJoinOperators.LeftOuter |
                SupportedJoinOperators.RightOuter
            }, true);
            result.EndLoadData();

            result.AcceptChanges();

            return result;
        }

        private readonly string[] _reservedWords = {
            "ADD", "EXCEPT", "PERCENT", "ALL", "EXEC", "PLAN", "ALTER", "EXECUTE", "PRECISION", "AND", "EXISTS", "PRIMARY", "ANY", "EXIT", "PRINT", "AS", "FETCH", "PROC", "ASC", "FILE", "PROCEDURE",
            "AUTHORIZATION", "FILLFACTOR", "PUBLIC", "BACKUP", "FOR", "RAISERROR", "BEGIN", "FOREIGN", "READ", "BETWEEN", "FREETEXT", "READTEXT", "BREAK", "FREETEXTTABLE", "RECONFIGURE", "BROWSE", "FROM",
            "REFERENCES", "BULK", "FULL", "REPLICATION", "BY", "FUNCTION", "RESTORE", "CASCADE", "GOTO", "RESTRICT", "CASE", "GRANT", "RETURN", "CHECK", "GROUP", "REVOKE", "CHECKPOINT", "HAVING", "RIGHT",
            "CLOSE", "HOLDLOCK", "ROLLBACK", "CLUSTERED", "IDENTITY", "ROWCOUNT", "COALESCE", "IDENTITY_INSERT", "ROWGUIDCOL", "COLLATE", "IDENTITYCOL", "RULE", "COLUMN", "IF", "SAVE", "COMMIT", "IN",
            "SCHEMA", "COMPUTE", "INDEX", "SELECT", "CONSTRAINT", "INNER", "SESSION_USER", "CONTAINS", "INSERT", "SET", "CONTAINSTABLE", "INTERSECT", "SETUSER", "CONTINUE", "INTO", "SHUTDOWN", "CONVERT",
            "IS", "SOME", "CREATE", "JOIN", "STATISTICS", "CROSS", "KEY", "SYSTEM_USER", "CURRENT", "KILL", "TABLE", "CURRENT_DATE", "LEFT", "TEXTSIZE", "CURRENT_TIME", "LIKE", "THEN", "CURRENT_TIMESTAMP",
            "LINENO", "TO", "CURRENT_USER", "LOAD", "TOP", "CURSOR", "NATIONAL ", "TRAN", "DATABASE", "NOCHECK", "TRANSACTION", "DBCC", "NONCLUSTERED", "TRIGGER", "DEALLOCATE", "NOT", "TRUNCATE",
            "DECLARE", "NULL", "TSEQUAL", "DEFAULT", "NULLIF", "UNION", "DELETE", "OF", "UNIQUE", "DENY", "OFF", "UPDATE", "DESC", "OFFSETS", "UPDATETEXT", "DISK", "ON", "USE", "DISTINCT", "OPEN", "USER",
            "DISTRIBUTED", "OPENDATASOURCE", "VALUES", "DOUBLE", "OPENQUERY", "VARYING", "DROP", "OPENROWSET", "VIEW", "DUMMY", "OPENXML", "WAITFOR", "DUMP", "OPTION", "WHEN", "ELSE", "OR", "WHERE", "END",
            "ORDER", "WHILE", "ERRLVL", "OUTER", "WITH", "ESCAPE", "OVER", "WRITETEXT", "ABSOLUTE", "FOUND", "PRESERVE", "ACTION", "FREE", "PRIOR", "ADMIN", "GENERAL", "PRIVILEGES", "AFTER", "GET",
            "READS", "AGGREGATE", "GLOBAL", "REAL", "ALIAS", "GO", "RECURSIVE", "ALLOCATE", "GROUPING", "REF", "ARE", "HOST", "REFERENCING", "ARRAY", "HOUR", "RELATIVE", "ASSERTION", "IGNORE", "RESULT",
            "AT", "IMMEDIATE", "RETURNS", "BEFORE", "INDICATOR", "ROLE", "BINARY", "INITIALIZE", "ROLLUP", "BIT", "INITIALLY", "ROUTINE", "BLOB", "INOUT", "ROW", "BOOLEAN", "INPUT", "ROWS", "BOTH", "INT",
            "SAVEPOINT", "BREADTH", "INTEGER", "SCROLL", "CALL", "INTERVAL", "SCOPE", "CASCADED", "ISOLATION", "SEARCH", "CAST", "ITERATE", "SECOND", "CATALOG", "LANGUAGE", "SECTION", "CHAR", "LARGE",
            "SEQUENCE", "CHARACTER", "LAST", "SESSION", "CLASS", "LATERAL", "SETS", "CLOB", "LEADING", "SIZE", "COLLATION", "LESS", "SMALLINT", "COMPLETION", "LEVEL", "SPACE", "CONNECT", "LIMIT",
            "SPECIFIC", "CONNECTION", "LOCAL", "SPECIFICTYPE", "CONSTRAINTS", "LOCALTIME", "SQL", "CONSTRUCTOR", "LOCALTIMESTAMP", "SQLEXCEPTION", "CORRESPONDING", "LOCATOR", "SQLSTATE", "CUBE", "MAP",
            "SQLWARNING", "CURRENT_PATH", "MATCH", "START", "CURRENT_ROLE", "MINUTE", "STATE", "CYCLE", "MODIFIES", "STATEMENT", "DATA", "MODIFY", "STATIC", "DATE", "MODULE", "STRUCTURE", "DAY", "MONTH",
            "TEMPORARY", "DEC", "NAMES", "TERMINATE", "DECIMAL", "NATURAL", "THAN", "DEFERRABLE", "NCHAR", "TIME", "DEFERRED", "NCLOB", "TIMESTAMP", "DEPTH", "NEW", "TIMEZONE_HOUR", "DEREF", "NEXT",
            "TIMEZONE_MINUTE", "DESCRIBE", "NO", "TRAILING", "DESCRIPTOR", "NONE", "TRANSLATION", "DESTROY", "NUMERIC", "TREAT", "DESTRUCTOR", "OBJECT", "TRUE", "DETERMINISTIC", "OLD", "UNDER",
            "DICTIONARY", "ONLY", "UNKNOWN", "DIAGNOSTICS", "OPERATION", "UNNEST", "DISCONNECT", "ORDINALITY", "USAGE", "DOMAIN", "OUT", "USING", "DYNAMIC", "OUTPUT", "VALUE", "EACH", "PAD", "VARCHAR",
            "END-EXEC", "PARAMETER", "VARIABLE", "EQUALS", "PARAMETERS", "WHENEVER", "EVERY", "PARTIAL", "WITHOUT", "EXCEPTION", "PATH", "WORK", "EXTERNAL", "POSTFIX", "WRITE", "FALSE", "PREFIX", "YEAR",
            "FIRST", "PREORDER", "ZONE", "FLOAT", "PREPARE", "ADA", "AVG", "BIT_LENGTH", "CHAR_LENGTH", "CHARACTER_LENGTH", "COUNT", "EXTRACT", "FORTRAN", "INCLUDE", "INSENSITIVE", "LOWER", "MAX", "MIN",
            "OCTET_LENGTH", "OVERLAPS", "PASCAL", "POSITION", "SQLCA", "SQLCODE", "SQLERROR", "SUBSTRING", "SUM", "TRANSLATE", "TRIM", "UPPER"
        };
#endif
    }

    /// <summary>
    /// Represents the method that will handle the InfoMessage event of an AseConnection.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The AseInfoMessageEventArgs object that contains the event data.</param>
    public delegate void AseInfoMessageEventHandler(object sender, AseInfoMessageEventArgs e);

    /// <summary>
    /// Traces database activity within an application for debugging.
    /// </summary>
    /// <remarks>
    /// <para>Use TraceEnter and TraceExit events to hook up your own tracing method. This event is unique to an 
    /// instance of a connection. This allows different connections to be logged to different files. It can ignore 
    /// the event, or you can program it for other tracing. In addition, by using a .NET event, you can set up more 
    /// than one event handler for a single connection object. This enables you to log the event to both a window 
    /// and a file at the same time.</para>
    /// <para>Enable the ENABLETRACING connection property to trace Adaptive Server ADO.NET Data Provider activities. 
    /// It is disabled by default to allow for better performance during normal execution where tracing is not needed. 
    /// When this property is disabled, the TraceEnter and TraceExit events are not triggered, and tracing events are 
    /// not executed. You can configure ENABLETRACING in the connection string using these values: True – triggers the 
    /// TraceEnter and TraceExit events; and False – the default value; Adaptive Server ADO.NET Data Provider ignores 
    /// the TraceEnter and TraceExit events.</para>
    /// </remarks>
    public delegate void TraceEnterEventHandler(AseConnection connection, object source, string method, object[] parameters);

    /// <summary>
    /// Traces database activity within an application for debugging.
    /// </summary>
    /// <remarks>
    /// <para>Use TraceEnter and TraceExit events to hook up your own tracing method. This event is unique to an 
    /// instance of a connection. This allows different connections to be logged to different files. It can ignore 
    /// the event, or you can program it for other tracing. In addition, by using a .NET event, you can set up more 
    /// than one event handler for a single connection object. This enables you to log the event to both a window 
    /// and a file at the same time.</para>
    /// <para>Enable the ENABLETRACING connection property to trace Adaptive Server ADO.NET Data Provider activities. 
    /// It is disabled by default to allow for better performance during normal execution where tracing is not needed. 
    /// When this property is disabled, the TraceEnter and TraceExit events are not triggered, and tracing events are 
    /// not executed. You can configure ENABLETRACING in the connection string using these values: True – triggers the 
    /// TraceEnter and TraceExit events; and False – the default value; Adaptive Server ADO.NET Data Provider ignores 
    /// the TraceEnter and TraceExit events.</para>
    /// </remarks>
    public delegate void TraceExitEventHandler(AseConnection connection, object source, string method, object returnValue);
}
