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

        public bool CanHandle(TokenType type)
        {
            return type == TokenType.TDS_LOGINACK;
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
                default:
                    return;
            }
        }
    }
}
