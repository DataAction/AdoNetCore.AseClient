﻿using System;
using System.Data;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient
{
    /// <summary>
    /// Represents an open connection to an ASE Server database. This class cannot be inherited.
    /// </summary>
    public sealed class AseConnection : IDbConnection
    {
        private IInternalConnection _internal;
        private string _connectionString;
        private readonly IConnectionPoolManager _connectionPoolManager;
        private ConnectionState _state;
        private bool _isDisposed;

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
        public AseConnection(string connectionString) : this(connectionString, new ConnectionPoolManager())
        {
        }

        internal AseConnection(string connectionString, IConnectionPoolManager connectionPoolManager)
        {
            ConnectionString = connectionString;
            ConnectionTimeout = 15; // Default to 15s as per the SAP AseClient http://infocenter.sybase.com/help/topic/com.sybase.infocenter.dc20066.1570100/doc/html/san1364409555258.html
            _connectionPoolManager = connectionPoolManager;
            NamedParameters = true;
            _isDisposed = false;
        }

        /// <summary>
        /// Releases all resources used by the <see cref="AseConnection" />.
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(AseConnection));
            }

            try
            {
                Close();

                // Kill listening references that might keep this object from being garbage collected.
                StateChangeInternal = null;
                InfoMessageInternal = null;
                TraceEnterInternal = null;
                TraceExitInternal = null;
            }
            finally
            {
                _isDisposed = true;
            }
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
        /// level with the <see cref="BeginTransaction()" /> method, use the overload that takes the iso parameter 
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
        /// level with the <see cref="BeginTransaction()" /> method, use the overload that takes the iso parameter 
        /// (<see cref="BeginTransaction(IsolationLevel)" />).</para>
        /// </remarks>
        public AseTransaction BeginTransaction()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(AseConnection));
            }

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
        /// level with the <see cref="BeginTransaction()" /> method, use the overload that takes the iso parameter 
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
        /// level with the <see cref="BeginTransaction()" /> method, use the overload that takes the iso parameter 
        /// (<see cref="BeginTransaction(IsolationLevel)" />).</para>
        /// </remarks>
        public AseTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(AseConnection));
            }

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
        public void ChangeDatabase(string databaseName)
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
        public void Close()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(AseConnection));
            }

            if (State == ConnectionState.Closed)
            {
                return;
            }

            _connectionPoolManager.Release(_connectionString, _internal);
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
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(AseConnection));
            }

            var aseCommand = new AseCommand(this) {NamedParameters = NamedParameters};

            return aseCommand;
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

            State = ConnectionState.Connecting;

            var parameters = ConnectionParameters.Parse(_connectionString);

            _internal = _connectionPoolManager.Reserve(_connectionString, parameters);

            ConnectionTimeout = parameters.LoginTimeout;

            State = ConnectionState.Open;
        }

        /// <summary>
        /// Gets or sets the string used to open a connection to an ASE database.
        /// </summary>
        public string ConnectionString
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
        public int ConnectionTimeout { get; private set; }

        /// <summary>
        /// Gets the name of the current database or the database to be used after a connection is opened.
        /// </summary>
        /// <remarks>
        /// The Database property updates dynamically. If you change the current database using a Transact-SQL 
        /// statement or the <see cref="ChangeDatabase(string)" /> method, an informational message is sent and 
        /// the property is updated automatically.
        /// </remarks>
        public string Database => _internal?.Database;

        /// <summary>
        /// Indicates the state of the <see cref="AseConnection" /> during the most recent network operation 
        /// performed on the connection.
        /// </summary>
        /// <remarks>
        /// Returns a <see cref="System.Data.ConnectionState" /> enumeration indicating the state of the 
        /// <see cref="AseConnection" />. Closing and reopening the connection will refresh the value of State.
        /// </remarks>
        public ConnectionState State
        {
            get => _state;
            private set
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException(nameof(AseConnection));
                }

                if (_state != value)
                {
                    var oldState = _state;
                    _state = value;

                    NotifyStateChange(oldState, _state);
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
                InfoMessageInternal += value;
            }
            remove
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException(nameof(AseConnection));
                }
                InfoMessageInternal -= value;
            }
        } 

        private event AseInfoMessageEventHandler InfoMessageInternal; // TODO - implement

        /// <summary>
        /// Occurs when the state of the connection changes.
        /// </summary>
        /// <remarks>
        /// The event handler receives an argument of StateChangeEventArgs with data related to this event. Two StateChangeEventArgs properties 
        /// provide information specific to this event: CurrentState and OriginalState.
        /// </remarks>
        public event StateChangeEventHandler StateChange
        {
            add
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException(nameof(AseConnection));
                }
                StateChangeInternal += value;
            }
            remove
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException(nameof(AseConnection));
                }
                StateChangeInternal -= value;
            }
        }

        private event StateChangeEventHandler StateChangeInternal;

        private void NotifyStateChange(ConnectionState originalState, ConnectionState currentState)
        {
            var stateChange = StateChangeInternal;

            stateChange?.Invoke(this, new StateChangeEventArgs(originalState, currentState));
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
                TraceEnterInternal += value;
            }
            remove
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException(nameof(AseConnection));
                }
                TraceEnterInternal -= value;
            }
        }
        private event TraceEnterEventHandler TraceEnterInternal; // TODO - implement

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
                TraceExitInternal += value;
            }
            remove
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException(nameof(AseConnection));
                }
                TraceExitInternal -= value;
            }
        }
        private event TraceExitEventHandler TraceExitInternal; // TODO - implement

        /// <summary>
        /// Governs the default behavior of the AseCommand objects associated with this connection.
        /// </summary>
        /// <remarks>
        /// This can be either set by the ConnectionString (NamedParameters='true'/'false') or the user can set it directly through an instance of AseConnection.
        /// </remarks>
        public bool NamedParameters // TODO - implement
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Represents the method that will handle the InfoMessage event of an AseConnection.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The AseInfoMessageEventArgs object that contains the event data.</param>
    public delegate void AseInfoMessageEventHandler ( object sender, AseInfoMessageEventArgs e );

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
