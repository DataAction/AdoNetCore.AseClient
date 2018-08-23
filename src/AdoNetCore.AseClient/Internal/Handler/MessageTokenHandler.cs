using System.Collections.Generic;
using System.Text;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Token;

namespace AdoNetCore.AseClient.Internal.Handler
{
    internal class MessageTokenHandler : ITokenHandler
    {
        private readonly List<AseError> _allErrors = new List<AseError>();
        private bool _foundSevereError = false;

        public bool CanHandle(TokenType type)
        {
            return type == TokenType.TDS_EED; //add info and error? don't think we'll be messing with capability bits related to that.
        }

        public void Handle(IToken token)
        {
            switch (token)
            {
                case EedToken t:

                    var isBackupServerMessage = BackupServerErrorInfo.TryParse(t.Message, out BackupServerErrorInfo backupServerErrorInfo);

                    var isSevere = t.Severity > 10 || isBackupServerMessage &&  backupServerErrorInfo.Severity != BackupServerSeverity.Informational;

                    if (isSevere)
                    {
                        this._foundSevereError = true;
                    }
                    
                    _allErrors.Add(new AseError
                    {
                        IsError = true,
                        IsFromServer = true,
                        IsFromBackupServer = backupServerErrorInfo != null,
                        BackupServerErrorInfo = backupServerErrorInfo,
                        Message = t.Message,
                        MessageNumber = t.MessageNumber,
                        ProcName = t.ProcedureName,
                        State = t.State,
                        TranState = (int)t.TransactionStatus,
                        Status = (int)t.Status,
                        Severity = t.Severity,
                        ServerName = t.ServerName,
                        SqlState = Encoding.ASCII.GetString(t.SqlState),
                        IsFromClient = false,
                        IsInformation = false,
                        IsWarning = false,
                        LineNum = t.LineNumber
                    });

                    var msgType = isSevere
                        ? "ERROR"
                        : "INFO ";

                    var formatted = $"{msgType} [{t.Severity}] [L:{t.LineNumber}]: {t.Message}";

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
            if (this._foundSevereError)
            {
                throw new AseException(this._allErrors.ToArray());
            }
        }
    }
}