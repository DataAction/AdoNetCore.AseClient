using System.Collections.Generic;
using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient.Interface
{
    /// <summary>
    /// Parse out tokens from a bunch of bytes
    /// </summary>
    internal interface ITokenParser
    {
        IEnumerable<IToken> Parse(TokenStream stream, DbEnvironment env);
    }
}
