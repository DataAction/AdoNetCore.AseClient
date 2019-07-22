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
#if ENABLE_CLONEABLE_INTERFACE
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

#if ENABLE_DB_PROVIDERFACTORY
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

#if ENABLE_CLONEABLE_INTERFACE
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

#if ENABLE_DB_GETSCHEMA
        /// <summary>
        /// Returns schema information for the data source of this <see cref="AseConnection"/>.
        /// </summary>
        /// <returns>A <see cref="DataTable"/> that contains schema information.</returns>
        public override DataTable GetSchema()
        {
            return GetSchema("MetaDataCollections");
        }

        /// <summary>
        /// Returns schema information for the data source of this <see cref="AseConnection"/> using the specified string for the schema name.
        /// </summary>
        /// <param name="collectionName">The name of the collection to retrieve detailed results for.</param>
        /// <returns>A <see cref="DataTable"/> that contains schema information.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="collectionName"/> is null, or does not represent a supported schema collection.</exception>
        public override DataTable GetSchema(string collectionName)
        {
            var result = new DataTable();

            switch (collectionName?.ToLowerInvariant())
            {
                case "metadatacollections":
                    // https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/common-schema-collections#metadatacollections
                    result.TableName = "MetadataCollections";
                    result.Columns.Add("CollectionName", typeof(string));
                    result.Columns.Add("NumberOfRestrictions", typeof(int));
                    result.Columns.Add("NumberOfIdentifierParts", typeof(int));

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
                    result.LoadDataRow(new object[] { "UserDefinedTypes", 2, 1 }, true);
                    break;
                case "datasourceinformation":
                    // https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/common-schema-collections#datasourceinformation
                    break; // TODO - Reference driver throws AccessViolationException... not sure what the desired behaviour is for this one.
                case "datatypes":
                    // https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/common-schema-collections#datatypes
                    result.TableName = "DataTypes";
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
                    //result.Columns.Add("NativeDataType", typeof(string));

                    result.LoadDataRow(new object[] { "smallint", AseDbType.SmallInt, 5, "smallint", null, "System.Int16", true, true, false, true, true, false, true, true, false, false, null, null, false, null, null, null }, true);
                    result.LoadDataRow(new object[] { "int", AseDbType.Integer, 10, "int", null, "System.Int32", true, true, false, true, true, false, true, true, false, false, null, null, false, null, null, null }, true);
                    result.LoadDataRow(new object[] { "real", AseDbType.Real, 7, "real", null, "System.Single", false, true, false, true, false, false, true, true, false, false, null, null, false, null, null, null }, true);
                    result.LoadDataRow(new object[] { "float", AseDbType.Double, 53, "float({ 0 })", "number of bits used to store the mantissa", "System.Double", false, true, false, true, false, false, true, true, false, false, null, null, false, null, null, null}, true);
                    result.LoadDataRow(new object[] { "money", AseDbType.Money, 19, "money", null, "System.Decimal", false, false, false, true, true, false, true, true, false, false, null, null, false, null, null, null }, true);
                    result.LoadDataRow(new object[] { "smallmoney", AseDbType.SmallMoney, 10, "smallmoney", null, "System.Decimal", false, false, false, true, true, false, true, true, false, false, null, null, false, null, null, null }, true);
                    result.LoadDataRow(new object[] { "bit", AseDbType.Bit, 1, "bit", null, "System.Boolean", false, false, false, true, false, false, true, true, false, null, null, null, false, null, null, null }, true);
                    result.LoadDataRow(new object[] { "tinyint", AseDbType.TinyInt, 3, "tinyint", null, "System.SByte", true, true, false, true, true, false, true, true, false, true, null, null, false, null, null, null }, true);
                    result.LoadDataRow(new object[] { "bigint", AseDbType.BigInt, 19, "bigint", null, "System.Int64", true, true, false, true, true, false, true, true, false, false, null, null, false, null, null, null }, true);
                    result.LoadDataRow(new object[] { "timestamp", AseDbType.TimeStamp, 8, "timestamp", null, "System.Byte[]", false, false, false, true, false, false, false, true, false, null, null, null, true, null, "0x", null }, true);
                    result.LoadDataRow(new object[] { "binary", AseDbType.Binary, 8000, "binary({ 0 })", "length", "System.Byte[]", false, true, false, true, false, false, true, true, false, null, null, null, false, null, "0x", null}, true);
                    result.LoadDataRow(new object[] { "image", AseDbType.Image, 2147483647, "image", null, "System.Byte[]", false, true, false, false, false, true, true, false, false, null, null, null, false, null, "0x", null}, true);
                    result.LoadDataRow(new object[] { "text", AseDbType.Text, 2147483647, "text", null, "System.String", false, true, false, false, false, true, true, false, true, null, null, null, false, null, "', '"}, true);
                    //result.LoadDataRow(new object[] { "ntext", TODO, 1073741823, "ntext", null, "System.String", false, true, false, false, false, true, true, false, true, null, null, null, false, null, "N', '"}, true);
                    result.LoadDataRow(new object[] { "decimal", AseDbType.Decimal, 38, "decimal ({0}, {1})", "precision,scale", "System.Decimal", true, true, false, true, false, false, true, true, false, false, 38, 0, false, null, null, null}, true);
                    result.LoadDataRow(new object[] { "numeric", AseDbType.Numeric, 38, "numeric({ 0}, {1})", "precision,scale", "System.Decimal", true, true, false, true, false, false, true, true, false, false, 38, 0, false, null, null, null}, true);
                    result.LoadDataRow(new object[] { "datetime", AseDbType.DateTime, 23, "datetime", null, "System.DateTime", false, true, false, true, false, false, true, true, true, null, null, null, false, null, "{ts ', '}"}, true);
                    result.LoadDataRow(new object[] { "smalldatetime", AseDbType.SmallDateTime, 16, "smalldatetime", null, "System.DateTime", false, true, false, true, false, false, true, true, true, null, null, null, false, null, "{ts ', '}"}, true);
                    result.LoadDataRow(new object[] { "sql_variant", 23, null, "sql_variant", null, "System.Object", false, true, false, false, false, false, true, true, false, null, null, null, false, false, null, null}, true);
                    result.LoadDataRow(new object[] { "xml", 25, 2147483647, "xml", null, "System.String", false, false, false, false, false, true, true, false, false, null, null, null, false, false, null, null}, true);
                    result.LoadDataRow(new object[] { "varchar", AseDbType.VarChar, 2147483647, "varchar({0})", "max length", "System.String", false, true, false, false, false, false, true, true, true, null, null, null, false, null, "', '"}, true);
                    result.LoadDataRow(new object[] { "char", AseDbType.Char, 2147483647, "char ({0})", "length", "System.String", false, true, false, true, false, false, true, true, true, null, null, null, false, null, "', '"}, true);
                    result.LoadDataRow(new object[] { "nchar", AseDbType.NChar, 1073741823, "nchar({0})", "length", "System.String", false, true, false, true, false, false, true, true, true, null, null, null, false, null, "N', '"}, true);
                    result.LoadDataRow(new object[] { "nvarchar", AseDbType.NVarChar, 1073741823, "nvarchar({0})", "max length", "System.String", false, true, false, false, false, false, true, true, true, null, null, null, false, null, "N', '"}, true);
                    result.LoadDataRow(new object[] { "varbinary", AseDbType.VarBinary, 1073741823, "varbinary({0})", "max length", "System.Byte[]", false, true, false, false, false, false, true, true, false, null, null, null, false, null, "0x", null}, true);
                    //result.LoadDataRow(new object[] { "uniqueidentifier", TODO, 16, "uniqueidentifier", null, "System.Guid", false, true, false, true, false, false, true, true, false, null, null, null, false, null, "', '"}, true);
                    break;
                case "restrictions":
                    // https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/common-schema-collections#restrictions
                    result.TableName = "Restrictions";
                    result.Columns.Add("CollectionName", typeof(string));
                    result.Columns.Add("RestrictionName", typeof(string));
                    result.Columns.Add("ParameterName", typeof(string));
                    result.Columns.Add("RestrictionDefault", typeof(string));
                    result.Columns.Add("RestrictionNumber", typeof(string));

                    result.LoadDataRow(new object[] { "Users", "User_Name", "@Name", "name", 1 }, true);
                    result.LoadDataRow(new object[] { "Databases", "Name", "@Name", "Name", 1 }, true);
                    result.LoadDataRow(new object[] { "Tables", "Catalog", "@Catalog", "TABLE_CATALOG", 1 }, true);
                    result.LoadDataRow(new object[] { "Tables", "Owner", "@Owner", "TABLE_SCHEMA", 2 }, true);
                    result.LoadDataRow(new object[] { "Tables", "Table", "@Name", "TABLE_NAME", 3 }, true);
                    result.LoadDataRow(new object[] { "Tables", "TableType", "@TableType", "TABLE_TYPE", 4 }, true);
                    result.LoadDataRow(new object[] { "Columns", "Catalog", "@Catalog", "TABLE_CATALOG", 1 }, true);
                    result.LoadDataRow(new object[] { "Columns", "Owner", "@Owner", "TABLE_SCHEMA", 2 }, true);
                    result.LoadDataRow(new object[] { "Columns", "Table", "@Table", "TABLE_NAME", 3 }, true);
                    result.LoadDataRow(new object[] { "Columns", "Column", "@Column", "COLUMN_NAME", 4 }, true);
                    result.LoadDataRow(new object[] { "Views", "Catalog", "@Catalog", "TABLE_CATALOG", 1 }, true);
                    result.LoadDataRow(new object[] { "Views", "Owner", "@Owner", "TABLE_SCHEMA", 2 }, true);
                    result.LoadDataRow(new object[] { "Views", "Table", "@Table", "TABLE_NAME", 3 }, true);
                    result.LoadDataRow(new object[] { "ViewColumns", "Catalog", "@Catalog", "VIEW_CATALOG", 1 }, true);
                    result.LoadDataRow(new object[] { "ViewColumns", "Owner", "@Owner", "VIEW_SCHEMA", 2 }, true);
                    result.LoadDataRow(new object[] { "ViewColumns", "Table", "@Table", "VIEW_NAME", 3 }, true);
                    result.LoadDataRow(new object[] { "ViewColumns", "Column", "@Column", "COLUMN_NAME", 4 }, true);
                    result.LoadDataRow(new object[] { "ProcedureParameters", "Catalog", "@Catalog", "SPECIFIC_CATALOG", 1 }, true);
                    result.LoadDataRow(new object[] { "ProcedureParameters", "Owner", "@Owner", "SPECIFIC_SCHEMA", 2 }, true);
                    result.LoadDataRow(new object[] { "ProcedureParameters", "Name", "@Name", "SPECIFIC_NAME", 3 }, true);
                    result.LoadDataRow(new object[] { "ProcedureParameters", "Parameter", "@Parameter", "PARAMETER_NAME", 4 }, true);
                    result.LoadDataRow(new object[] { "Procedures", "Catalog", "@Catalog", "SPECIFIC_CATALOG", 1 }, true);
                    result.LoadDataRow(new object[] { "Procedures", "Owner", "@Owner", "SPECIFIC_SCHEMA", 2 }, true);
                    result.LoadDataRow(new object[] { "Procedures", "Name", "@Name", "SPECIFIC_NAME", 3 }, true);
                    result.LoadDataRow(new object[] { "Procedures", "Type", "@Type", "ROUTINE_TYPE", 4 }, true);
                    result.LoadDataRow(new object[] { "IndexColumns", "Catalog", "@Catalog", "db_name()", 1 }, true);
                    result.LoadDataRow(new object[] { "IndexColumns", "Owner", "@Owner", "user_name()", 2 }, true);
                    result.LoadDataRow(new object[] { "IndexColumns", "Table", "@Table", "o.name", 3 }, true);
                    result.LoadDataRow(new object[] { "IndexColumns", "ConstraintName", "@ConstraintName", "x.name", 4 }, true);
                    result.LoadDataRow(new object[] { "IndexColumns", "Column", "@Column", "c.name", 5 }, true);
                    result.LoadDataRow(new object[] { "Indexes", "Catalog", "@Catalog", "db_name()", 1 }, true);
                    result.LoadDataRow(new object[] { "Indexes", "Owner", "@Owner", "user_name()", 2 }, true);
                    result.LoadDataRow(new object[] { "Indexes", "Table", "@Table", "o.name", 3 }, true);
                    result.LoadDataRow(new object[] { "Indexes", "Name", "@Name", "x.name", 4 }, true);
                    result.LoadDataRow(new object[] { "UserDefinedTypes", "assembly_name", "@AssemblyName", "assemblies.name", 1 }, true);
                    result.LoadDataRow(new object[] { "UserDefinedTypes", "udt_name", "@UDTName", "types.assembly_class", 2 }, true);
                    result.LoadDataRow(new object[] { "ForeignKeys", "Catalog", "@Catalog", "CONSTRAINT_CATALOG", 1 }, true);
                    result.LoadDataRow(new object[] { "ForeignKeys", "Owner", "@Owner", "CONSTRAINT_SCHEMA", 2 }, true);
                    result.LoadDataRow(new object[] { "ForeignKeys", "Table", "@Table", "TABLE_NAME", 3 }, true);
                    result.LoadDataRow(new object[] { "ForeignKeys","Name","@Name","CONSTRAINT_NAME", 4 }, true);

                    break;
                case "reservedwords":
                    result.TableName = "ReservedWords";
                    result.Columns.Add("ReservedWord", typeof(string));
                    result.LoadDataRow(new object[] { "ADD" }, true);
                    result.LoadDataRow(new object[] { "EXCEPT" }, true);
                    result.LoadDataRow(new object[] { "PERCENT" }, true);
                    result.LoadDataRow(new object[] { "ALL" }, true);
                    result.LoadDataRow(new object[] { "EXEC" }, true);
                    result.LoadDataRow(new object[] { "PLAN" }, true);
                    result.LoadDataRow(new object[] { "ALTER" }, true);
                    result.LoadDataRow(new object[] { "EXECUTE" }, true);
                    result.LoadDataRow(new object[] { "PRECISION" }, true);
                    result.LoadDataRow(new object[] { "AND" }, true);
                    result.LoadDataRow(new object[] { "EXISTS" }, true);
                    result.LoadDataRow(new object[] { "PRIMARY" }, true);
                    result.LoadDataRow(new object[] { "ANY" }, true);
                    result.LoadDataRow(new object[] { "EXIT" }, true);
                    result.LoadDataRow(new object[] { "PRINT" }, true);
                    result.LoadDataRow(new object[] { "AS" }, true);
                    result.LoadDataRow(new object[] { "FETCH" }, true);
                    result.LoadDataRow(new object[] { "PROC" }, true);
                    result.LoadDataRow(new object[] { "ASC" }, true);
                    result.LoadDataRow(new object[] { "FILE" }, true);
                    result.LoadDataRow(new object[] { "PROCEDURE" }, true);
                    result.LoadDataRow(new object[] { "AUTHORIZATION" }, true);
                    result.LoadDataRow(new object[] { "FILLFACTOR" }, true);
                    result.LoadDataRow(new object[] { "PUBLIC" }, true);
                    result.LoadDataRow(new object[] { "BACKUP" }, true);
                    result.LoadDataRow(new object[] { "FOR" }, true);
                    result.LoadDataRow(new object[] { "RAISERROR" }, true);
                    result.LoadDataRow(new object[] { "BEGIN" }, true);
                    result.LoadDataRow(new object[] { "FOREIGN" }, true);
                    result.LoadDataRow(new object[] { "READ" }, true);
                    result.LoadDataRow(new object[] { "BETWEEN" }, true);
                    result.LoadDataRow(new object[] { "FREETEXT" }, true);
                    result.LoadDataRow(new object[] { "READTEXT" }, true);
                    result.LoadDataRow(new object[] { "BREAK" }, true);
                    result.LoadDataRow(new object[] { "FREETEXTTABLE" }, true);
                    result.LoadDataRow(new object[] { "RECONFIGURE" }, true);
                    result.LoadDataRow(new object[] { "BROWSE" }, true);
                    result.LoadDataRow(new object[] { "FROM" }, true);
                    result.LoadDataRow(new object[] { "REFERENCES" }, true);
                    result.LoadDataRow(new object[] { "BULK" }, true);
                    result.LoadDataRow(new object[] { "FULL" }, true);
                    result.LoadDataRow(new object[] { "REPLICATION" }, true);
                    result.LoadDataRow(new object[] { "BY" }, true);
                    result.LoadDataRow(new object[] { "FUNCTION" }, true);
                    result.LoadDataRow(new object[] { "RESTORE" }, true);
                    result.LoadDataRow(new object[] { "CASCADE" }, true);
                    result.LoadDataRow(new object[] { "GOTO" }, true);
                    result.LoadDataRow(new object[] { "RESTRICT" }, true);
                    result.LoadDataRow(new object[] { "CASE" }, true);
                    result.LoadDataRow(new object[] { "GRANT" }, true);
                    result.LoadDataRow(new object[] { "RETURN" }, true);
                    result.LoadDataRow(new object[] { "CHECK" }, true);
                    result.LoadDataRow(new object[] { "GROUP" }, true);
                    result.LoadDataRow(new object[] { "REVOKE" }, true);
                    result.LoadDataRow(new object[] { "CHECKPOINT" }, true);
                    result.LoadDataRow(new object[] { "HAVING" }, true);
                    result.LoadDataRow(new object[] { "RIGHT" }, true);
                    result.LoadDataRow(new object[] { "CLOSE" }, true);
                    result.LoadDataRow(new object[] { "HOLDLOCK" }, true);
                    result.LoadDataRow(new object[] { "ROLLBACK" }, true);
                    result.LoadDataRow(new object[] { "CLUSTERED" }, true);
                    result.LoadDataRow(new object[] { "IDENTITY" }, true);
                    result.LoadDataRow(new object[] { "ROWCOUNT" }, true);
                    result.LoadDataRow(new object[] { "COALESCE" }, true);
                    result.LoadDataRow(new object[] { "IDENTITY_INSERT" }, true);
                    result.LoadDataRow(new object[] { "ROWGUIDCOL" }, true);
                    result.LoadDataRow(new object[] { "COLLATE" }, true);
                    result.LoadDataRow(new object[] { "IDENTITYCOL" }, true);
                    result.LoadDataRow(new object[] { "RULE" }, true);
                    result.LoadDataRow(new object[] { "COLUMN" }, true);
                    result.LoadDataRow(new object[] { "IF" }, true);
                    result.LoadDataRow(new object[] { "SAVE" }, true);
                    result.LoadDataRow(new object[] { "COMMIT" }, true);
                    result.LoadDataRow(new object[] { "IN" }, true);
                    result.LoadDataRow(new object[] { "SCHEMA" }, true);
                    result.LoadDataRow(new object[] { "COMPUTE" }, true);
                    result.LoadDataRow(new object[] { "INDEX" }, true);
                    result.LoadDataRow(new object[] { "SELECT" }, true);
                    result.LoadDataRow(new object[] { "CONSTRAINT" }, true);
                    result.LoadDataRow(new object[] { "INNER" }, true);
                    result.LoadDataRow(new object[] { "SESSION_USER" }, true);
                    result.LoadDataRow(new object[] { "CONTAINS" }, true);
                    result.LoadDataRow(new object[] { "INSERT" }, true);
                    result.LoadDataRow(new object[] { "SET" }, true);
                    result.LoadDataRow(new object[] { "CONTAINSTABLE" }, true);
                    result.LoadDataRow(new object[] { "INTERSECT" }, true);
                    result.LoadDataRow(new object[] { "SETUSER" }, true);
                    result.LoadDataRow(new object[] { "CONTINUE" }, true);
                    result.LoadDataRow(new object[] { "INTO" }, true);
                    result.LoadDataRow(new object[] { "SHUTDOWN" }, true);
                    result.LoadDataRow(new object[] { "CONVERT" }, true);
                    result.LoadDataRow(new object[] { "IS" }, true);
                    result.LoadDataRow(new object[] { "SOME" }, true);
                    result.LoadDataRow(new object[] { "CREATE" }, true);
                    result.LoadDataRow(new object[] { "JOIN" }, true);
                    result.LoadDataRow(new object[] { "STATISTICS" }, true);
                    result.LoadDataRow(new object[] { "CROSS" }, true);
                    result.LoadDataRow(new object[] { "KEY" }, true);
                    result.LoadDataRow(new object[] { "SYSTEM_USER" }, true);
                    result.LoadDataRow(new object[] { "CURRENT" }, true);
                    result.LoadDataRow(new object[] { "KILL" }, true);
                    result.LoadDataRow(new object[] { "TABLE" }, true);
                    result.LoadDataRow(new object[] { "CURRENT_DATE" }, true);
                    result.LoadDataRow(new object[] { "LEFT" }, true);
                    result.LoadDataRow(new object[] { "TEXTSIZE" }, true);
                    result.LoadDataRow(new object[] { "CURRENT_TIME" }, true);
                    result.LoadDataRow(new object[] { "LIKE" }, true);
                    result.LoadDataRow(new object[] { "THEN" }, true);
                    result.LoadDataRow(new object[] { "CURRENT_TIMESTAMP" }, true);
                    result.LoadDataRow(new object[] { "LINENO" }, true);
                    result.LoadDataRow(new object[] { "TO" }, true);
                    result.LoadDataRow(new object[] { "CURRENT_USER" }, true);
                    result.LoadDataRow(new object[] { "LOAD" }, true);
                    result.LoadDataRow(new object[] { "TOP" }, true);
                    result.LoadDataRow(new object[] { "CURSOR" }, true);
                    result.LoadDataRow(new object[] { "NATIONAL " }, true);
                    result.LoadDataRow(new object[] { "TRAN" }, true);
                    result.LoadDataRow(new object[] { "DATABASE" }, true);
                    result.LoadDataRow(new object[] { "NOCHECK" }, true);
                    result.LoadDataRow(new object[] { "TRANSACTION" }, true);
                    result.LoadDataRow(new object[] { "DBCC" }, true);
                    result.LoadDataRow(new object[] { "NONCLUSTERED" }, true);
                    result.LoadDataRow(new object[] { "TRIGGER" }, true);
                    result.LoadDataRow(new object[] { "DEALLOCATE" }, true);
                    result.LoadDataRow(new object[] { "NOT" }, true);
                    result.LoadDataRow(new object[] { "TRUNCATE" }, true);
                    result.LoadDataRow(new object[] { "DECLARE" }, true);
                    result.LoadDataRow(new object[] { "NULL" }, true);
                    result.LoadDataRow(new object[] { "TSEQUAL" }, true);
                    result.LoadDataRow(new object[] { "DEFAULT" }, true);
                    result.LoadDataRow(new object[] { "NULLIF" }, true);
                    result.LoadDataRow(new object[] { "UNION" }, true);
                    result.LoadDataRow(new object[] { "DELETE" }, true);
                    result.LoadDataRow(new object[] { "OF" }, true);
                    result.LoadDataRow(new object[] { "UNIQUE" }, true);
                    result.LoadDataRow(new object[] { "DENY" }, true);
                    result.LoadDataRow(new object[] { "OFF" }, true);
                    result.LoadDataRow(new object[] { "UPDATE" }, true);
                    result.LoadDataRow(new object[] { "DESC" }, true);
                    result.LoadDataRow(new object[] { "OFFSETS" }, true);
                    result.LoadDataRow(new object[] { "UPDATETEXT" }, true);
                    result.LoadDataRow(new object[] { "DISK" }, true);
                    result.LoadDataRow(new object[] { "ON" }, true);
                    result.LoadDataRow(new object[] { "USE" }, true);
                    result.LoadDataRow(new object[] { "DISTINCT" }, true);
                    result.LoadDataRow(new object[] { "OPEN" }, true);
                    result.LoadDataRow(new object[] { "USER" }, true);
                    result.LoadDataRow(new object[] { "DISTRIBUTED" }, true);
                    result.LoadDataRow(new object[] { "OPENDATASOURCE" }, true);
                    result.LoadDataRow(new object[] { "VALUES" }, true);
                    result.LoadDataRow(new object[] { "DOUBLE" }, true);
                    result.LoadDataRow(new object[] { "OPENQUERY" }, true);
                    result.LoadDataRow(new object[] { "VARYING" }, true);
                    result.LoadDataRow(new object[] { "DROP" }, true);
                    result.LoadDataRow(new object[] { "OPENROWSET" }, true);
                    result.LoadDataRow(new object[] { "VIEW" }, true);
                    result.LoadDataRow(new object[] { "DUMMY" }, true);
                    result.LoadDataRow(new object[] { "OPENXML" }, true);
                    result.LoadDataRow(new object[] { "WAITFOR" }, true);
                    result.LoadDataRow(new object[] { "DUMP" }, true);
                    result.LoadDataRow(new object[] { "OPTION" }, true);
                    result.LoadDataRow(new object[] { "WHEN" }, true);
                    result.LoadDataRow(new object[] { "ELSE" }, true);
                    result.LoadDataRow(new object[] { "OR" }, true);
                    result.LoadDataRow(new object[] { "WHERE" }, true);
                    result.LoadDataRow(new object[] { "END" }, true);
                    result.LoadDataRow(new object[] { "ORDER" }, true);
                    result.LoadDataRow(new object[] { "WHILE" }, true);
                    result.LoadDataRow(new object[] { "ERRLVL" }, true);
                    result.LoadDataRow(new object[] { "OUTER" }, true);
                    result.LoadDataRow(new object[] { "WITH" }, true);
                    result.LoadDataRow(new object[] { "ESCAPE" }, true);
                    result.LoadDataRow(new object[] { "OVER" }, true);
                    result.LoadDataRow(new object[] { "WRITETEXT" }, true);
                    result.LoadDataRow(new object[] { "ABSOLUTE" }, true);
                    result.LoadDataRow(new object[] { "FOUND" }, true);
                    result.LoadDataRow(new object[] { "PRESERVE" }, true);
                    result.LoadDataRow(new object[] { "ACTION" }, true);
                    result.LoadDataRow(new object[] { "FREE" }, true);
                    result.LoadDataRow(new object[] { "PRIOR" }, true);
                    result.LoadDataRow(new object[] { "ADMIN" }, true);
                    result.LoadDataRow(new object[] { "GENERAL" }, true);
                    result.LoadDataRow(new object[] { "PRIVILEGES" }, true);
                    result.LoadDataRow(new object[] { "AFTER" }, true);
                    result.LoadDataRow(new object[] { "GET" }, true);
                    result.LoadDataRow(new object[] { "READS" }, true);
                    result.LoadDataRow(new object[] { "AGGREGATE" }, true);
                    result.LoadDataRow(new object[] { "GLOBAL" }, true);
                    result.LoadDataRow(new object[] { "REAL" }, true);
                    result.LoadDataRow(new object[] { "ALIAS" }, true);
                    result.LoadDataRow(new object[] { "GO" }, true);
                    result.LoadDataRow(new object[] { "RECURSIVE" }, true);
                    result.LoadDataRow(new object[] { "ALLOCATE" }, true);
                    result.LoadDataRow(new object[] { "GROUPING" }, true);
                    result.LoadDataRow(new object[] { "REF" }, true);
                    result.LoadDataRow(new object[] { "ARE" }, true);
                    result.LoadDataRow(new object[] { "HOST" }, true);
                    result.LoadDataRow(new object[] { "REFERENCING" }, true);
                    result.LoadDataRow(new object[] { "ARRAY" }, true);
                    result.LoadDataRow(new object[] { "HOUR" }, true);
                    result.LoadDataRow(new object[] { "RELATIVE" }, true);
                    result.LoadDataRow(new object[] { "ASSERTION" }, true);
                    result.LoadDataRow(new object[] { "IGNORE" }, true);
                    result.LoadDataRow(new object[] { "RESULT" }, true);
                    result.LoadDataRow(new object[] { "AT" }, true);
                    result.LoadDataRow(new object[] { "IMMEDIATE" }, true);
                    result.LoadDataRow(new object[] { "RETURNS" }, true);
                    result.LoadDataRow(new object[] { "BEFORE" }, true);
                    result.LoadDataRow(new object[] { "INDICATOR" }, true);
                    result.LoadDataRow(new object[] { "ROLE" }, true);
                    result.LoadDataRow(new object[] { "BINARY" }, true);
                    result.LoadDataRow(new object[] { "INITIALIZE" }, true);
                    result.LoadDataRow(new object[] { "ROLLUP" }, true);
                    result.LoadDataRow(new object[] { "BIT" }, true);
                    result.LoadDataRow(new object[] { "INITIALLY" }, true);
                    result.LoadDataRow(new object[] { "ROUTINE" }, true);
                    result.LoadDataRow(new object[] { "BLOB" }, true);
                    result.LoadDataRow(new object[] { "INOUT" }, true);
                    result.LoadDataRow(new object[] { "ROW" }, true);
                    result.LoadDataRow(new object[] { "BOOLEAN" }, true);
                    result.LoadDataRow(new object[] { "INPUT" }, true);
                    result.LoadDataRow(new object[] { "ROWS" }, true);
                    result.LoadDataRow(new object[] { "BOTH" }, true);
                    result.LoadDataRow(new object[] { "INT" }, true);
                    result.LoadDataRow(new object[] { "SAVEPOINT" }, true);
                    result.LoadDataRow(new object[] { "BREADTH" }, true);
                    result.LoadDataRow(new object[] { "INTEGER" }, true);
                    result.LoadDataRow(new object[] { "SCROLL" }, true);
                    result.LoadDataRow(new object[] { "CALL" }, true);
                    result.LoadDataRow(new object[] { "INTERVAL" }, true);
                    result.LoadDataRow(new object[] { "SCOPE" }, true);
                    result.LoadDataRow(new object[] { "CASCADED" }, true);
                    result.LoadDataRow(new object[] { "ISOLATION" }, true);
                    result.LoadDataRow(new object[] { "SEARCH" }, true);
                    result.LoadDataRow(new object[] { "CAST" }, true);
                    result.LoadDataRow(new object[] { "ITERATE" }, true);
                    result.LoadDataRow(new object[] { "SECOND" }, true);
                    result.LoadDataRow(new object[] { "CATALOG" }, true);
                    result.LoadDataRow(new object[] { "LANGUAGE" }, true);
                    result.LoadDataRow(new object[] { "SECTION" }, true);
                    result.LoadDataRow(new object[] { "CHAR" }, true);
                    result.LoadDataRow(new object[] { "LARGE" }, true);
                    result.LoadDataRow(new object[] { "SEQUENCE" }, true);
                    result.LoadDataRow(new object[] { "CHARACTER" }, true);
                    result.LoadDataRow(new object[] { "LAST" }, true);
                    result.LoadDataRow(new object[] { "SESSION" }, true);
                    result.LoadDataRow(new object[] { "CLASS" }, true);
                    result.LoadDataRow(new object[] { "LATERAL" }, true);
                    result.LoadDataRow(new object[] { "SETS" }, true);
                    result.LoadDataRow(new object[] { "CLOB" }, true);
                    result.LoadDataRow(new object[] { "LEADING" }, true);
                    result.LoadDataRow(new object[] { "SIZE" }, true);
                    result.LoadDataRow(new object[] { "COLLATION" }, true);
                    result.LoadDataRow(new object[] { "LESS" }, true);
                    result.LoadDataRow(new object[] { "SMALLINT" }, true);
                    result.LoadDataRow(new object[] { "COMPLETION" }, true);
                    result.LoadDataRow(new object[] { "LEVEL" }, true);
                    result.LoadDataRow(new object[] { "SPACE" }, true);
                    result.LoadDataRow(new object[] { "CONNECT" }, true);
                    result.LoadDataRow(new object[] { "LIMIT" }, true);
                    result.LoadDataRow(new object[] { "SPECIFIC" }, true);
                    result.LoadDataRow(new object[] { "CONNECTION" }, true);
                    result.LoadDataRow(new object[] { "LOCAL" }, true);
                    result.LoadDataRow(new object[] { "SPECIFICTYPE" }, true);
                    result.LoadDataRow(new object[] { "CONSTRAINTS" }, true);
                    result.LoadDataRow(new object[] { "LOCALTIME" }, true);
                    result.LoadDataRow(new object[] { "SQL" }, true);
                    result.LoadDataRow(new object[] { "CONSTRUCTOR" }, true);
                    result.LoadDataRow(new object[] { "LOCALTIMESTAMP" }, true);
                    result.LoadDataRow(new object[] { "SQLEXCEPTION" }, true);
                    result.LoadDataRow(new object[] { "CORRESPONDING" }, true);
                    result.LoadDataRow(new object[] { "LOCATOR" }, true);
                    result.LoadDataRow(new object[] { "SQLSTATE" }, true);
                    result.LoadDataRow(new object[] { "CUBE" }, true);
                    result.LoadDataRow(new object[] { "MAP" }, true);
                    result.LoadDataRow(new object[] { "SQLWARNING" }, true);
                    result.LoadDataRow(new object[] { "CURRENT_PATH" }, true);
                    result.LoadDataRow(new object[] { "MATCH" }, true);
                    result.LoadDataRow(new object[] { "START" }, true);
                    result.LoadDataRow(new object[] { "CURRENT_ROLE" }, true);
                    result.LoadDataRow(new object[] { "MINUTE" }, true);
                    result.LoadDataRow(new object[] { "STATE" }, true);
                    result.LoadDataRow(new object[] { "CYCLE" }, true);
                    result.LoadDataRow(new object[] { "MODIFIES" }, true);
                    result.LoadDataRow(new object[] { "STATEMENT" }, true);
                    result.LoadDataRow(new object[] { "DATA" }, true);
                    result.LoadDataRow(new object[] { "MODIFY" }, true);
                    result.LoadDataRow(new object[] { "STATIC" }, true);
                    result.LoadDataRow(new object[] { "DATE" }, true);
                    result.LoadDataRow(new object[] { "MODULE" }, true);
                    result.LoadDataRow(new object[] { "STRUCTURE" }, true);
                    result.LoadDataRow(new object[] { "DAY" }, true);
                    result.LoadDataRow(new object[] { "MONTH" }, true);
                    result.LoadDataRow(new object[] { "TEMPORARY" }, true);
                    result.LoadDataRow(new object[] { "DEC" }, true);
                    result.LoadDataRow(new object[] { "NAMES" }, true);
                    result.LoadDataRow(new object[] { "TERMINATE" }, true);
                    result.LoadDataRow(new object[] { "DECIMAL" }, true);
                    result.LoadDataRow(new object[] { "NATURAL" }, true);
                    result.LoadDataRow(new object[] { "THAN" }, true);
                    result.LoadDataRow(new object[] { "DEFERRABLE" }, true);
                    result.LoadDataRow(new object[] { "NCHAR" }, true);
                    result.LoadDataRow(new object[] { "TIME" }, true);
                    result.LoadDataRow(new object[] { "DEFERRED" }, true);
                    result.LoadDataRow(new object[] { "NCLOB" }, true);
                    result.LoadDataRow(new object[] { "TIMESTAMP" }, true);
                    result.LoadDataRow(new object[] { "DEPTH" }, true);
                    result.LoadDataRow(new object[] { "NEW" }, true);
                    result.LoadDataRow(new object[] { "TIMEZONE_HOUR" }, true);
                    result.LoadDataRow(new object[] { "DEREF" }, true);
                    result.LoadDataRow(new object[] { "NEXT" }, true);
                    result.LoadDataRow(new object[] { "TIMEZONE_MINUTE" }, true);
                    result.LoadDataRow(new object[] { "DESCRIBE" }, true);
                    result.LoadDataRow(new object[] { "NO" }, true);
                    result.LoadDataRow(new object[] { "TRAILING" }, true);
                    result.LoadDataRow(new object[] { "DESCRIPTOR" }, true);
                    result.LoadDataRow(new object[] { "NONE" }, true);
                    result.LoadDataRow(new object[] { "TRANSLATION" }, true);
                    result.LoadDataRow(new object[] { "DESTROY" }, true);
                    result.LoadDataRow(new object[] { "NUMERIC" }, true);
                    result.LoadDataRow(new object[] { "TREAT" }, true);
                    result.LoadDataRow(new object[] { "DESTRUCTOR" }, true);
                    result.LoadDataRow(new object[] { "OBJECT" }, true);
                    result.LoadDataRow(new object[] { "TRUE" }, true);
                    result.LoadDataRow(new object[] { "DETERMINISTIC" }, true);
                    result.LoadDataRow(new object[] { "OLD" }, true);
                    result.LoadDataRow(new object[] { "UNDER" }, true);
                    result.LoadDataRow(new object[] { "DICTIONARY" }, true);
                    result.LoadDataRow(new object[] { "ONLY" }, true);
                    result.LoadDataRow(new object[] { "UNKNOWN" }, true);
                    result.LoadDataRow(new object[] { "DIAGNOSTICS" }, true);
                    result.LoadDataRow(new object[] { "OPERATION" }, true);
                    result.LoadDataRow(new object[] { "UNNEST" }, true);
                    result.LoadDataRow(new object[] { "DISCONNECT" }, true);
                    result.LoadDataRow(new object[] { "ORDINALITY" }, true);
                    result.LoadDataRow(new object[] { "USAGE" }, true);
                    result.LoadDataRow(new object[] { "DOMAIN" }, true);
                    result.LoadDataRow(new object[] { "OUT" }, true);
                    result.LoadDataRow(new object[] { "USING" }, true);
                    result.LoadDataRow(new object[] { "DYNAMIC" }, true);
                    result.LoadDataRow(new object[] { "OUTPUT" }, true);
                    result.LoadDataRow(new object[] { "VALUE" }, true);
                    result.LoadDataRow(new object[] { "EACH" }, true);
                    result.LoadDataRow(new object[] { "PAD" }, true);
                    result.LoadDataRow(new object[] { "VARCHAR" }, true);
                    result.LoadDataRow(new object[] { "END-EXEC" }, true);
                    result.LoadDataRow(new object[] { "PARAMETER" }, true);
                    result.LoadDataRow(new object[] { "VARIABLE" }, true);
                    result.LoadDataRow(new object[] { "EQUALS" }, true);
                    result.LoadDataRow(new object[] { "PARAMETERS" }, true);
                    result.LoadDataRow(new object[] { "WHENEVER" }, true);
                    result.LoadDataRow(new object[] { "EVERY" }, true);
                    result.LoadDataRow(new object[] { "PARTIAL" }, true);
                    result.LoadDataRow(new object[] { "WITHOUT" }, true);
                    result.LoadDataRow(new object[] { "EXCEPTION" }, true);
                    result.LoadDataRow(new object[] { "PATH" }, true);
                    result.LoadDataRow(new object[] { "WORK" }, true);
                    result.LoadDataRow(new object[] { "EXTERNAL" }, true);
                    result.LoadDataRow(new object[] { "POSTFIX" }, true);
                    result.LoadDataRow(new object[] { "WRITE" }, true);
                    result.LoadDataRow(new object[] { "FALSE" }, true);
                    result.LoadDataRow(new object[] { "PREFIX" }, true);
                    result.LoadDataRow(new object[] { "YEAR" }, true);
                    result.LoadDataRow(new object[] { "FIRST" }, true);
                    result.LoadDataRow(new object[] { "PREORDER" }, true);
                    result.LoadDataRow(new object[] { "ZONE" }, true);
                    result.LoadDataRow(new object[] { "FLOAT" }, true);
                    result.LoadDataRow(new object[] { "PREPARE" }, true);
                    result.LoadDataRow(new object[] { "ADA" }, true);
                    result.LoadDataRow(new object[] { "AVG" }, true);
                    result.LoadDataRow(new object[] { "BIT_LENGTH" }, true);
                    result.LoadDataRow(new object[] { "CHAR_LENGTH" }, true);
                    result.LoadDataRow(new object[] { "CHARACTER_LENGTH" }, true);
                    result.LoadDataRow(new object[] { "COUNT" }, true);
                    result.LoadDataRow(new object[] { "EXTRACT" }, true);
                    result.LoadDataRow(new object[] { "FORTRAN" }, true);
                    result.LoadDataRow(new object[] { "INCLUDE" }, true);
                    result.LoadDataRow(new object[] { "INSENSITIVE" }, true);
                    result.LoadDataRow(new object[] { "LOWER" }, true);
                    result.LoadDataRow(new object[] { "MAX" }, true);
                    result.LoadDataRow(new object[] { "MIN" }, true);
                    result.LoadDataRow(new object[] { "OCTET_LENGTH" }, true);
                    result.LoadDataRow(new object[] { "OVERLAPS" }, true);
                    result.LoadDataRow(new object[] { "PASCAL" }, true);
                    result.LoadDataRow(new object[] { "POSITION" }, true);
                    result.LoadDataRow(new object[] { "SQLCA" }, true);
                    result.LoadDataRow(new object[] { "SQLCODE" }, true);
                    result.LoadDataRow(new object[] { "SQLERROR" }, true);
                    result.LoadDataRow(new object[] { "SUBSTRING" }, true);
                    result.LoadDataRow(new object[] { "SUM" }, true);
                    result.LoadDataRow(new object[] { "TRANSLATE" }, true);
                    result.LoadDataRow(new object[] { "TRIM" }, true);
                    result.LoadDataRow(new object[] { "UPPER" }, true);

                    break;
                case "users":
                    // TODO
                    break;
                case "databases":
                    // TODO
                    break;
                case "tables":
                    // TODO
                    break;
                case "columns":
                    // TODO
                    break;
                case "views":
                    // TODO
                    break;
                case "viewcolumns":
                    // TODO
                    break;
                case "procedureparameters":
                    // TODO
                    break;
                case "procedures":
                    // TODO
                    break;
                case "foreignkeys":
                    // TODO
                    break;
                case "indexcolumns":
                    // TODO
                    break;
                case "indexes":
                    // TODO
                    break;
                case "userdefinedtypes":
                    // TODO
                    break;
                default:
                    throw new ArgumentException("The specified collection name is not supported.", nameof(collectionName));
            }

            return result;
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
            return base.GetSchema(collectionName, restrictionValues);
        }
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
