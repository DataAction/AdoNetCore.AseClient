using System;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Token;

namespace AdoNetCore.AseClient.Internal.Handler
{
    internal class DoneTokenHandler : ITokenHandler
    {
        public int RowsAffected { get; private set; }

        public bool CanHandle(TokenType type)
        {
            return type == TokenType.TDS_DONE;
        }

        public void Handle(IToken token)
        {
            switch (token)
            {
                case DoneToken t:
                    Console.WriteLine($"{t.Type}: {t.Status}");
                    RowsAffected = t.Count;
                    break;
                default:
                    return;
            }
        }
    }
}