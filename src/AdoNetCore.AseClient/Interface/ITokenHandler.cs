using AdoNetCore.AseClient.Enum;

namespace AdoNetCore.AseClient.Interface
{
    internal interface ITokenHandler
    {
        bool CanHandle(TokenType type);
        void Handle(IToken token);
    }
}
