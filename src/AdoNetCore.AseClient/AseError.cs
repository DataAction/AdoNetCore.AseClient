using System;
using AdoNetCore.AseClient.Internal.Handler;

namespace AdoNetCore.AseClient
{
    /// <summary>
    /// Collects information relevant to a warning or error returned by the data source.
    /// </summary>
    public sealed class AseError : IDisposable
    {
        /// <summary>
        /// Number of the error message.
        /// </summary>
        public int MessageNumber 
        {
            get; internal set;
        }

        /// <summary>
        /// A short description of the error.
        /// </summary>
        public string Message
        {
            get; internal set;
        }        

        /// <summary>
        /// The Adaptive Server five-character SQL state following the ANSI SQL standard. If the error can 
        /// be issued from more than one place, the five-character error code identifies the source of the error.
        /// </summary>
        public string SqlState
        {
            get; internal set;
        }

        /// <summary>
        /// Identifies the complete text of the error message.
        /// </summary>
        public override string ToString() 
        {
            return $"AseError:{Message}";
        }

        /// <summary>
        /// The message state. Used as a modifier to the MsgNumber.
        /// </summary>
        public int State 
        {
            get; internal set;
        }        

        /// <summary>
        /// The severity of the message.
        /// </summary>
        public int Severity 
        {
            get; internal set;
        }        

        /// <summary>
        /// The name of the server that is sending the message.
        /// </summary>
        public string ServerName 
        {
            get; internal set;
        }        

        /// <summary>
        /// The name of the stored procedure or remote procedure call (RPC) in which the message occurred.
        /// </summary>
        public string ProcName 
        {
            get; internal set;
        }        

        /// <summary>
        /// The line number in the command batch or the stored procedure that has the error, if applicable.
        /// </summary>
        public int LineNum 
        {
            get; internal set;
        }        

        /// <summary>
        /// Associated with the extended message.
        /// </summary>
        public int Status 
        {
            get; internal set;
        }

        /// <summary>
        /// The current state of any transactions that are active on this dialog.
        /// </summary>
        public int TranState 
        {
            get; internal set;
        }


        /// <summary>
        /// The error message that comes from the Adaptive Server server.
        /// </summary>
        public bool IsFromServer
        {
            get; internal set;
        }

        /// <summary>
        /// The error message that comes from Adaptive Server ADO.NET Data Provider.
        /// </summary>
        public bool IsFromClient 
        {
            get; internal set;
        }

        /// <summary>
        /// The message is considered an error.
        /// </summary>
        public bool IsError
        {
            get; internal set;
        }

        /// <summary>
        /// The message is a warning that things might not be quite right.
        /// </summary>
        public bool IsWarning 
        {
            get; internal set;
        }

        /// <summary>
        /// An informative message, providing information such as the active catalog has changed.
        /// </summary>
        public bool IsInformation 
        {
            get; internal set;
        }

        public void Dispose() { }
    }
}