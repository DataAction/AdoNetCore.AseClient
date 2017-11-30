using System.Collections.Generic;
using System.Data;
using System.Linq;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Token;

namespace AdoNetCore.AseClient.Internal.Handler
{
    internal class ResponseParameterTokenHandler : ITokenHandler
    {
        private static readonly HashSet<TokenType> AllowedTypes = new HashSet<TokenType>
        {
            TokenType.TDS_RETURNSTATUS,
            TokenType.TDS_PARAMS
        };
        private readonly AseDataParameterCollection _parameters;

        public ResponseParameterTokenHandler(AseDataParameterCollection parameters)
        {
            _parameters = parameters;
        }

        public bool CanHandle(TokenType type)
        {
            return AllowedTypes.Contains(type);
        }

        public void Handle(IToken token)
        {
            switch (token)
            {
                case ReturnStatusToken t:
                    foreach (var parameter in _parameters)
                    {
                        if (parameter.Direction == ParameterDirection.ReturnValue)
                        {
                            parameter.Value = t.Status;
                        }
                    }
                    break;
                case ParametersToken t:
                    var dict = t.Parameters.ToDictionary(p => p.Format.ParameterName);

                    foreach (var parameter in _parameters)
                    {
                        if (parameter.IsOutput && dict.ContainsKey(parameter.ParameterName))
                        {
                            parameter.Value = dict[parameter.ParameterName].Value;
                        }
                    }
                    break;
                default:
                    return;
            }
        }
    }
}
