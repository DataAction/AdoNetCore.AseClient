using System;
#if ENABLE_SYSTEMEXCEPTION
using System.Runtime.Serialization;
#endif

namespace AdoNetCore.AseClient
{
    /// <summary>
    /// The exception that is thrown when ASE returns a warning or error. This class cannot be inherited.
    /// </summary>
#if ENABLE_SYSTEMEXCEPTION
    [Serializable]
#endif
    public sealed class AseException :
#if ENABLE_SYSTEMEXCEPTION
    SystemException
#else
    Exception
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
            get;
        }

        /// <summary>
        /// This method returns the message of the most severe error.
        /// </summary>
        public override string Message => Errors.MainError == null ? string.Empty : Errors.MainError.Message;

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
        } 

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

#if ENABLE_SYSTEMEXCEPTION
        private AseException(SerializationInfo info, StreamingContext context) : base(info, context) {
            Errors = new AseErrorCollection(new AseError
            {
                Message = ""
            });
        }
#endif
    }
}
