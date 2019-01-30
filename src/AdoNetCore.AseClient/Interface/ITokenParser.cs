using System.Collections.Generic;
using System.IO;
using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient.Interface
{
    /// <summary>
    /// Parse out tokens from a bunch of bytes
    /// </summary>
    internal interface ITokenParser
    {
        long LastStartPosition { get; }

        IEnumerable<IToken> Parse(Stream stream, DbEnvironment env, out bool streamExceeded);
    }
}
