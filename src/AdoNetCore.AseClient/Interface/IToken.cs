using System.IO;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient.Interface
{
    internal interface IToken
    {
        TokenType Type { get; }
        void Write(Stream stream, DbEnvironment env);
        void Read(Stream stream, DbEnvironment env, IFormatToken previousFormatToken);
    }
}
