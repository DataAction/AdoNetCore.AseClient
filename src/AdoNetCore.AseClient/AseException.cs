using System;
#if NETCOREAPP2_0
using System.Runtime.Serialization;
#endif

namespace AdoNetCore.AseClient
{
#if NETCOREAPP2_0
    [Serializable]
#endif
    public sealed class AseException : Exception
    {
        public int ErrorCode { get; private set; }

        public AseException() { }

        public AseException(string message) : base(message) {}
        public AseException(string message, int errorCode) : base(message)
        {
            ErrorCode = errorCode;
        }

        public AseException(string message, Exception inner) : base(message, inner) { }
        public AseException(string message, int errorCode, Exception inner) : base(message, inner)
        {
            ErrorCode = errorCode;
        }
#if NETCOREAPP2_0
        private AseException(SerializationInfo info, StreamingContext context) : base(info, context) { }
#endif
    }
}
