using System.Data;

namespace AdoNetCore.AseClient.Interface
{
    internal interface IInfoMessageEventNotifier
    {
        void NotifyInfoMessage(AseErrorCollection errors, string message);
    }

    internal interface IResultsMessageEventNotifier
    {
        void NotifyResultSet();
        void NotifyResultRow(IAseDataCallbackReader reader);
    }

    internal interface IStateChangeEventNotifier
    {
        void NotifyStateChange(ConnectionState originalState, ConnectionState currentState);
    }

    internal interface ITraceEnterEventNotifier
    {
        void NotifyTraceEnter(object source, string method, params object[] parameters);
    }

    internal interface ITraceExitEventNotifier
    {
        void NotifyTraceExit(object source, string method, object returnValue);
    }

    internal interface IEventNotifier : IInfoMessageEventNotifier, IStateChangeEventNotifier, ITraceEnterEventNotifier, ITraceExitEventNotifier, IResultsMessageEventNotifier
    {
        event AseInfoMessageEventHandler InfoMessage;
        event StateChangeEventHandler StateChange;
        event TraceEnterEventHandler TraceEnter;
        event TraceExitEventHandler TraceExit;

        ResultSetCallbackHandler ResultSet { set; }
        ResultRowCallbackHandler ResultRow { set; }

        void ClearAll();
        void ClearResultHandlers();
    }
}
