using System.Data;
using AdoNetCore.AseClient.Interface;

namespace AdoNetCore.AseClient.Internal
{
    internal class EventNotifier : IEventNotifier
    {
        private AseConnection _aseConnection;

        internal EventNotifier(AseConnection aseConnection)
        {
            _aseConnection = aseConnection;
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
            add => InfoMessageInternal += value;
            remove => InfoMessageInternal -= value;
        }

        private event AseInfoMessageEventHandler InfoMessageInternal;

        public void NotifyInfoMessage(AseErrorCollection errors, string message)
        {
            if (_aseConnection == null)
                return;

            var infoMessage = InfoMessageInternal;
            infoMessage?.Invoke(_aseConnection, new AseInfoMessageEventArgs(errors, message));
        }

        /// <summary>
        /// Occurs when the state of the connection changes.
        /// </summary>
        /// <remarks>
        /// The event handler receives an argument of StateChangeEventArgs with data related to this event. Two StateChangeEventArgs properties 
        /// provide information specific to this event: CurrentState and OriginalState.
        /// </remarks>
        public event StateChangeEventHandler StateChange
        {
            add => StateChangeInternal += value;
            remove => StateChangeInternal -= value;
        }

        private event StateChangeEventHandler StateChangeInternal;

        public void NotifyStateChange(ConnectionState originalState, ConnectionState currentState)
        {
            if (_aseConnection == null)
                return;

            var stateChange = StateChangeInternal;
            stateChange?.Invoke(_aseConnection, new StateChangeEventArgs(originalState, currentState));
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
            add => TraceEnterInternal += value;
            remove => TraceEnterInternal -= value;
        }

        private event TraceEnterEventHandler TraceEnterInternal;

        public void NotifyTraceEnter(object source, string method, params object[] parameters)
        {
            if (_aseConnection == null)
                return;

            var traceEnter = TraceEnterInternal;
            traceEnter?.Invoke(_aseConnection, source, method, parameters);
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
            add => TraceExitInternal += value;
            remove => TraceExitInternal -= value;
        }

        private event TraceExitEventHandler TraceExitInternal;

        public void NotifyTraceExit(object source, string method, object returnValue)
        {
            if (_aseConnection == null)
                return;

            var traceExit = TraceExitInternal;
            traceExit?.Invoke(_aseConnection, source, method, returnValue);
        }

        /// <summary>
        /// Callback methods for new result sets and rows.
        /// </summary>
        public ResultSetCallbackHandler ResultSet { private get; set; }
        public void NotifyResultSet()
        {
            if (_aseConnection == null)
                return;

            ResultSet?.Invoke();
        }

        public ResultRowCallbackHandler ResultRow { private get; set; }
        public void NotifyResultRow(IAseDataCallbackReader reader)
        {
            if (_aseConnection == null)
                return;

            ResultRow?.Invoke(reader);
        }

        public void ClearAll()
        {
            StateChangeInternal = null;
            InfoMessageInternal = null;
            TraceEnterInternal = null;
            TraceExitInternal = null;
            ClearResultHandlers();
            _aseConnection = null;
        }

        public void ClearResultHandlers()
        {
            ResultSet = null;
            ResultRow = null;
        }
    }
}
