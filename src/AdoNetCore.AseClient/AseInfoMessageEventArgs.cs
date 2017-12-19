using System;

namespace AdoNetCore.AseClient
{
    /// <summary>
    /// The event arguments passed to the InfoMessage event handlers.
    /// </summary>
    public sealed class AseInfoMessageEventArgs : EventArgs
    {
        public AseInfoMessageEventArgs(AseErrorCollection errors, string message)
        {
            Message = message;
            Errors = errors;
        }

        /// <summary>
        /// A collection of the actual error objects returned by the server.
        /// </summary>
        public AseErrorCollection Errors 
        {
            get; 
            private set;
        }

        /// <summary>
        /// The error message.
        /// </summary>
        public string Message 
        {
            get; 
            private set;
        }
    }
}
