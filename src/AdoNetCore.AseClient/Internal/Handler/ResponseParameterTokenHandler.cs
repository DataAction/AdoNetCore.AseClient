using System.Collections.Generic;
using System.Data;
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
        private readonly AseParameterCollection _parameters;

        public ResponseParameterTokenHandler(AseParameterCollection parameters)
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
                    foreach (AseParameter parameter in _parameters)
                    {
                        if (parameter.Direction == ParameterDirection.ReturnValue)
                            parameter.Value = t.Status;
                    }
                    break;

                case ParametersToken t:
                    var dict = new Dictionary<string, ParametersToken.Parameter>();

                    foreach (var p in t.Parameters)
                    {
                        dict[p.Format.ParameterName] = p;
                    }

                    foreach (AseParameter parameter in _parameters)
                    {
                        if (parameter.IsOutput && dict.ContainsKey(parameter.ParameterName))
                            parameter.Value = dict[parameter.ParameterName].Value;
                    }
                    break;
                default:
                    return;
            }
        }
    }
}
