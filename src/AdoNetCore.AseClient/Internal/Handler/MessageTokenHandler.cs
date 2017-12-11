using System.Collections.Generic;
using System.Text;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Token;

namespace AdoNetCore.AseClient.Internal.Handler
{
    internal class MessageTokenHandler : ITokenHandler
    {
        private readonly List<EedToken> _errorTokens = new List<EedToken>();
        public bool CanHandle(TokenType type)
        {
            return type == TokenType.TDS_EED; //add info and error? don't think we'll be messing with capability bits related to that.
        }

        public void Handle(IToken token)
        {
            switch (token)
            {
                case EedToken t:
                    var isSevere = t.Severity > 10;
                    var msgType = isSevere
                        ? "ERROR"
                        : "INFO ";

                    var formatted = $"{msgType} [{t.Severity}] [L:{t.LineNumber}]: {t.Message}";

                    if (isSevere)
                    {
                        _errorTokens.Add(t);
                    }

                    if (formatted.EndsWith("\n"))
                    {
                        Logger.Instance?.Write(formatted);
                    }
                    else
                    {
                        Logger.Instance?.WriteLine(formatted);
                    }
                    break;
                default:
                    return;
            }
        }

        public void AssertNoErrors()
        {
            if (_errorTokens.Count > 0)
            {
                var errorList = new List<AseError>();
                foreach (var error in _errorTokens)
                {
                    errorList.Add(new AseError
                    {
                        IsError = true,
                        IsFromServer = true,
                        Message = error.Message,
                        MessageNumber = error.MessageNumber,
                        ProcName = error.ProcedureName,
                        State = error.State,
                        TranState = (int)error.TransactionStatus,
                        Status = (int)error.Status,
                        Severity = error.Severity,
                        ServerName = error.ServerName,
                        SqlState = Encoding.ASCII.GetString(error.SqlState),
                        IsFromClient = false,
                        IsInformation = false,
                        IsWarning = false,
                        LineNum = error.LineNumber
                    });
                }
                throw new AseException(errorList.ToArray());
            }
        }
    }
}