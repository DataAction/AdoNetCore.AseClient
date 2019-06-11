using System;
#if ENABLE_SYSTEMEXCEPTION
using System.Runtime.Serialization;
#endif

namespace AdoNetCore.AseClient.Internal
{

#if ENABLE_SYSTEMEXCEPTION
    [Serializable]
#endif
    public class TokenStreamCancelledException :
#if ENABLE_SYSTEMEXCEPTION
    SystemException
#else
        Exception
#endif
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public TokenStreamCancelledException()
        {
        }

        public TokenStreamCancelledException(string message) : base(message)
        {
        }

        public TokenStreamCancelledException(string message, Exception inner) : base(message, inner)
        {
        }

#if ENABLE_SYSTEMEXCEPTION
        protected TokenStreamCancelledException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }
#endif
    }
}
