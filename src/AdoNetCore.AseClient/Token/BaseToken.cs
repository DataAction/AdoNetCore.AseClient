using AdoNetCore.AseClient.Enum;

namespace AdoNetCore.AseClient.Token
{
    public abstract class BaseToken
    {
        public abstract TokenType Type { get; }
    }
}
