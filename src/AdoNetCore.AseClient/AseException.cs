using System;

namespace AdoNetCore.AseClient
{
    public class AseException : Exception
    {
        public int ErrorCode { get; private set; }

        public AseException(int errorCode, string errorMessage) : base(errorMessage)
        {
            ErrorCode = errorCode;
        }
    }
}
