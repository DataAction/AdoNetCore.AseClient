using System.Collections.Generic;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Token;

namespace AdoNetCore.AseClient.Internal.Handler
{
    internal class LoginTokenHandler : ITokenHandler
    {
        public bool ReceivedAck { get; private set; }
        public LoginAckToken.LoginStatus LoginStatus { get; private set; }
        public LoginAckToken Token { get; private set; }
        public MessageToken Message { get; private set; }
        public ParameterFormatCommonToken Format { get; private set; }
        public ParametersToken Parameters { get; private set; }

        private readonly HashSet<TokenType> _tokens = new HashSet<TokenType>
        {
            TokenType.TDS_LOGINACK,
            TokenType.TDS_MSG,
            TokenType.TDS_PARAMFMT,
            TokenType.TDS_PARAMFMT2,
            TokenType.TDS_PARAMS,
        };

        public bool CanHandle(TokenType type)
        {
            return _tokens.Contains(type);
        }

        public void Handle(IToken token)
        {
            switch (token)
            {
                case LoginAckToken t:
                    ReceivedAck = true;
                    Token = t;
                    LoginStatus = t.Status;

                    if (t.Status == LoginAckToken.LoginStatus.TDS_LOG_FAIL)
                    {
                        Logger.Instance?.WriteLine($"Login failed");
                    }

                    if (t.Status == LoginAckToken.LoginStatus.TDS_LOG_NEGOTIATE)
                    {
                        Logger.Instance?.WriteLine($"Login negotiation required");
                    }

                    if (t.Status == LoginAckToken.LoginStatus.TDS_LOG_SUCCEED)
                    {
                        Logger.Instance?.WriteLine($"Login success");
                    }
                    break;
                case MessageToken msg:
                    Message = msg;
                    break;
                case ParameterFormatToken pf:
                    Format = pf;
                    break;
                case ParameterFormat2Token pf2:
                    Format = pf2;
                    break;
                case ParametersToken p:
                    Parameters = p;
                    break;
                default:
                    return;
            }
        }
    }
}
