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
        public override string Message => Errors.Count == 0 ? string.Empty : Errors[0].Message;

        /// <summary>
        /// Constructor function for an <see cref="AseException" /> instance.
        /// </summary>
        /// <param name="message">A message describing the error.</param>
        public AseException(string message) : base(message)
        {
            Errors = new AseErrorCollection(new AseError
            {
                Message = message
            });
        }

        /// <summary>
        /// Constructor function for an <see cref="AseException" /> instance.
        /// </summary>
        /// <param name="message">A message describing the error.</param>
        /// <param name="errorCode">The error code identifying the error.</param>
        public AseException(string message, int errorCode) : base(message)
        {
            Errors = new AseErrorCollection(new AseError
            {
                Message = message,
                MessageNumber = errorCode
            });
        }

        /// <summary>
        /// Constructor function for an <see cref="AseException" /> instance.
        /// </summary>
        /// <param name="errors">Error details</param>
        public AseException(params AseError[] errors)
        {
            Errors = new AseErrorCollection(errors);
        }

        /// <summary>
        /// Constructor function for an <see cref="AseException" /> instance.
        /// </summary>
        /// <param name="message">A message describing the error.</param>
        /// <param name="inner">A deeper error that happened in the context of this error.</param>
        public AseException(string message, Exception inner) : base(message, inner)
        {
            Errors = new AseErrorCollection(new AseError
            {
                Message = message
            });
        } // TODO - construct an AseErrorCollection

        /// <summary>
        /// Constructor function for an <see cref="AseException" /> instance.
        /// </summary>
        /// <param name="message">A message describing the error.</param>
        /// <param name="errorCode">The error code identifying the error.</param>
        /// <param name="inner">A deeper error that happened in the context of this error.</param>
        public AseException(string message, int errorCode, Exception inner) : base(message, inner)
        {
            Errors = new AseErrorCollection(new AseError
            {
                Message = message,
                MessageNumber = errorCode
            });
        }

        /// <summary>
        /// Constructor function for an <see cref="AseException" /> instance.
        /// </summary>
        /// <param name="inner">A deeper error that happened in the context of this error.</param>
        /// <param name="errors">Error details</param>
        public AseException(Exception inner, params AseError[] errors) : base("", inner)
        {
            Errors = new AseErrorCollection(errors);
        }

#if NETCOREAPP2_0
        private AseException(SerializationInfo info, StreamingContext context) : base(info, context) {
            Errors = new AseErrorCollection(new AseError
            {
                Message = ""
            });
        }
#endif
    }
}
