using System;
using System.Collections.Generic;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;

namespace AdoNetCore.AseClient.Internal.Handler
{
    internal class DataReaderTokenHandler : ITokenHandler
    {
        private static HashSet<TokenType> acceptedTypes = new HashSet<TokenType>();
        public bool CanHandle(TokenType type)
        {
            return acceptedTypes.Contains(type);
        }

        public void Handle(IToken token)
        {
            throw new NotImplementedException();
        }
    }
}
