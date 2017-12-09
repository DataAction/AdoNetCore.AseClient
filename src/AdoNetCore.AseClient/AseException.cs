using System;
#if NETCOREAPP2_0
using System.Runtime.Serialization;
#endif

namespace AdoNetCore.AseClient
{
    /// <summary>
    /// The exception that is thrown when ASE returns a warning or error. This class cannot be inherited.
    /// </summary>
#if NETCOREAPP2_0
    [Serializable]
#endif
    public sealed class AseException : 
#if NETCOREAPP2_0
    System.SystemException
#else 
    System.Exception
#endif
    {
        /// <summary>
        /// A code describing the errors.
        /// </summary>
        public int ErrorCode { get; private set; } // TODO - Don't think this should be here - see http://infocenter.sybase.com/help/topic/com.sybase.infocenter.dc20066.1570100/doc/html/san1364409596853.html

        /// <summary>
        /// The error code identifying the error.
        /// </summary>
        /// <remarks>
        /// The AseErrorCollection class always contains at least one instance of the AseError class.
        /// </remarks>
        public AseErrorCollection Errors 
        {
            get; private set;
        }

        /// <summary>
        /// This method returns the message for the first AseError.
        /// </summary>
        public override string Message 
        { 
            get
            {
                return Errors[0].Message; // Apparently there is always at least one value.
            }
        }

        /// <summary>
        /// Constructor function for an <see cref="AseException" /> instance.
        /// </summary>
        public AseException() { } // TODO - construct an AseErrorCollection

        /// <summary>
        /// Constructor function for an <see cref="AseException" /> instance.
        /// </summary>
        /// <param name="message">A message describing the error.</param>
        public AseException(string message) : base(message) {} // TODO - construct an AseErrorCollection

        /// <summary>
        /// Constructor function for an <see cref="AseException" /> instance.
        /// </summary>
        /// <param name="message">A message describing the error.</param>
        /// <param name="errorCode">The error code identifying the error.</param>
        public AseException(string message, int errorCode) : base(message) // TODO - construct an AseErrorCollection
        {
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Constructor function for an <see cref="AseException" /> instance.
        /// </summary>
        /// <param name="message">A message describing the error.</param>
        /// <param name="inner">A deeper error that happened in the context of this error.</param>
        public AseException(string message, Exception inner) : base(message, inner) { } // TODO - construct an AseErrorCollection

        /// <summary>
        /// Constructor function for an <see cref="AseException" /> instance.
        /// </summary>
        /// <param name="message">A message describing the error.</param>
        /// <param name="errorCode">The error code identifying the error.</param>
        /// <param name="inner">A deeper error that happened in the context of this error.</param>
        public AseException(string message, int errorCode, Exception inner) : base(message, inner) // TODO - construct an AseErrorCollection
        {
            ErrorCode = errorCode;
        }
#if NETCOREAPP2_0
        private AseException(SerializationInfo info, StreamingContext context) : base(info, context) { }
#endif
    }
}
