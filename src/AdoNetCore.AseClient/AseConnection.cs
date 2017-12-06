using System;
using System.Data;
using System.Runtime.CompilerServices;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Internal;

[assembly: InternalsVisibleTo("AdoNetCore.AseClient.Tests")]

namespace AdoNetCore.AseClient
{
    /// <summary>
    /// Represents an open connection to an ASE Server database. This class cannot be inherited.
    /// </summary>
    public sealed class AseConnection : IDbConnection
    {
        private IInternalConnection _internal;
        private ConnectionParameters _parameters;

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
        public AseConnection() : this(string.Empty)
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
        public AseConnection(string connectionString)
        {
            ConnectionString = connectionString;
            ConnectionTimeout = 30;
        }

        /// <summary>
        /// Releases all resources used by the <see cref="AseConnection" />.
        /// </summary>
        public void Dispose()
        {
            Close();
        }

        /// <summary>
        /// Starts a database transaction.
        /// </summary>
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
        IDbTransaction IDbConnection.BeginTransaction()
        {
            return BeginTransaction();
        }

        /// <summary>
        /// Starts a database transaction.
        /// </summary>
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
        public AseTransaction BeginTransaction()
        {
            Open();
            var t = new AseTransaction(this);
            t.Begin();
            return t;
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
        IDbTransaction IDbConnection.BeginTransaction(IsolationLevel isolationLevel)
        {
            return BeginTransaction(isolationLevel);
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
        public AseTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            Open();
            var t = new AseTransaction(this, isolationLevel);
            t.Begin();
            return t;
        }

        /// <summary>
        /// Changes the current database for an open <see cref="AseConnection" />.
        /// </summary>
        /// <param name="databaseName">The name of the database to use instead of the current database.</param>
        /// <remarks>
        /// The value supplied in the <i>database</i> parameter must be a valid database name. The <i>database</i> parameter 
        /// cannot contain a null value, an empty string, or a string with only blank characters.
        /// </remarks>
        public void ChangeDatabase(string databaseName) => _internal.ChangeDatabase(databaseName);

        /// <summary>
        /// TODO - document this once transactions are supported.
        /// </summary>
        public void Close()
        {
            if (State == ConnectionState.Closed)
            {
                return;
            }

            ConnectionPoolManager.Release(_parameters, _internal);
            _internal = null;
            State = ConnectionState.Closed;
        }

        /// <summary>
        /// Creates and returns an <see cref="AseCommand" /> object associated with the <see cref="AseConnection" />.
        /// </summary>
        /// <returns>An <see cref="AseCommand" /> object.</returns>
        IDbCommand IDbConnection.CreateCommand()
        {
            return CreateCommand();
        }

        /// <summary>
        /// Creates and returns an <see cref="AseCommand" /> object associated with the <see cref="AseConnection" />.
        /// </summary>
        /// <returns>An <see cref="AseCommand" /> object.</returns>
        public AseCommand CreateCommand()
        {
            return new AseCommand(this);
        }

        /// <summary>
        /// Opens a database connection with the property settings specified by the <see cref="ConnectionString" />.
        /// </summary>
        /// <remarks>
        /// The <see cref="AseConnection" /> draws an open connection from the connection pool if one is available. 
        /// Otherwise, it establishes a new connection to an instance of ASE.
        /// </remarks>
        public void Open()
        {
            if (State == ConnectionState.Open)
            {
                return; //already open
            }

            if (State != ConnectionState.Closed)
            {
                throw new InvalidOperationException("Cannot open a connection which is not closed");
            }

            State = ConnectionState.Connecting;

            _internal = ConnectionPoolManager.Reserve(_parameters);

            State = ConnectionState.Open;
        }

        /// <summary>
        /// Gets or sets the string used to open a connection to an ASE database.
        /// </summary>
        // TODO - expand docs.
        public string ConnectionString
        {
            get => _parameters.ConnectionString;
            set => _parameters = ConnectionParameters.Parse(value);
        }

        /// <summary>
        /// Gets the time to wait while trying to establish a connection before terminating the attempt and generating an error.
        /// </summary>
        /// <remarks>
        /// You can set the amount of time a connection waits to time out by using the <b>LoginTimeOut</b> keyword in the connection 
        /// string. A value of 0 indicates no limit, and should be avoided in a <see cref="ConnectionString" /> because an attempt to 
        /// connect waits indefinitely.
        /// </remarks>
        public int ConnectionTimeout { get; private set; }

        /// <summary>
        /// Gets the name of the current database or the database to be used after a connection is opened.
        /// </summary>
        /// <remarks>
        /// The Database property updates dynamically. If you change the current database using a Transact-SQL 
        /// statement or the <see cref="ChangeDatabase(string)" /> method, an informational message is sent and 
        /// the property is updated automatically.
        /// </remarks>
        public string Database => _internal.Database;

        /// <summary>
        /// Indicates the state of the <see cref="AseConnection" /> during the most recent network operation 
        /// performed on the connection.
        /// </summary>
        /// <remarks>
        /// Returns a <see cref="System.Data.ConnectionState" /> enumeration indicating the state of the 
        /// <see cref="AseConnection" />. Closing and reopening the connection will refresh the value of State.
        /// </remarks>
        public ConnectionState State { get; private set; }

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
    }
}
