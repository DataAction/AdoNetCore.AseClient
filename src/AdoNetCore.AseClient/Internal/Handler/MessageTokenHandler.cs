using System;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Token;

namespace AdoNetCore.AseClient.Internal.Handler
{
    internal class MessageTokenHandler : ITokenHandler
    {
        public bool CanHandle(TokenType type)
        {
            return type == TokenType.TDS_EED; //add info and error? don't think we'll be messing with capability bits related to that.
        }

        public void Handle(IToken token)
        {
            switch (token)
            {
                case EedToken t:
                    var msgType = t.Severity > 10
                        ? "ERROR"
                        : "INFO ";

                    var formatted = $"{msgType} [{t.Severity}]: {t.Message}";
                    if (formatted.EndsWith("\n"))
                    {
                        Console.Write(formatted);
                    }
                    else
                    {
                        Console.WriteLine(formatted);
                    }
                    break;
                default:
                    return;
            }
        }
    }
}