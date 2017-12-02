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
    public sealed class AseException : Exception
    {
        /// <summary>
        /// The error code identifying the error.
        /// </summary>
        public int ErrorCode { get; private set; }

        /// <summary>
        /// Constructor function for an <see cref="AseException" /> instance.
        /// </summary>
        public AseException() { }

        /// <summary>
        /// Constructor function for an <see cref="AseException" /> instance.
        /// </summary>
        /// <param name="message">A message describing the error.</param>
        public AseException(string message) : base(message) {}

        /// <summary>
        /// Constructor function for an <see cref="AseException" /> instance.
        /// </summary>
        /// <param name="message">A message describing the error.</param>
        /// <param name="errorCode">The error code identifying the error.</param>
        public AseException(string message, int errorCode) : base(message)
        {
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Constructor function for an <see cref="AseException" /> instance.
        /// </summary>
        /// <param name="message">A message describing the error.</param>
        /// <param name="inner">A deeper error that happened in the context of this error.</param>
        public AseException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        /// Constructor function for an <see cref="AseException" /> instance.
        /// </summary>
        /// <param name="message">A message describing the error.</param>
        /// <param name="errorCode">The error code identifying the error.</param>
        /// <param name="inner">A deeper error that happened in the context of this error.</param>
        public AseException(string message, int errorCode, Exception inner) : base(message, inner)
        {
            ErrorCode = errorCode;
        }
#if NETCOREAPP2_0
        private AseException(SerializationInfo info, StreamingContext context) : base(info, context) { }
#endif
    }
}
