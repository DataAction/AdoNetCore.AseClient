using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient.Interface
{
    internal interface IFormatToken : IToken
    {
        FormatItem[] Formats { get; set; }
    }
}
