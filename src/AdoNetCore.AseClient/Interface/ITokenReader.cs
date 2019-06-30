using System.Collections.Generic;
using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient.Interface
{
    /// <summary>
    /// Read out tokens from a bunch of bytes
    /// </summary>
    internal interface ITokenReader
    {
        IEnumerable<IToken> Read(TokenReceiveStream stream, DbEnvironment env);
    }
}
