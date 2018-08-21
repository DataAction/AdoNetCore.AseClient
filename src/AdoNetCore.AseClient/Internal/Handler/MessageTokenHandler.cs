using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
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

        internal static Tuple<BackupServerErrorMajor, int, BackupServerSeverity, int> parseBackupServerSeverity(string message)
        {
            // Backup Server error messages are in this form: MMM DD YYY: Backup Server:N.N.N.N: Message Text
            // The four components of a Backup Server error message are major.minor.severity.state:

            var regex = new Regex("^Backup Server: ?(?<major>\\d+)\\.(?<minor>\\d+)\\.(?<severity>\\d+)\\.(?<state>\\d+): (?<message>.*)");
            var groups = regex.Match(message).Groups;
            var major = int.Parse(groups["major"].Value);
            var minor = int.Parse(groups["minor"].Value);
            var severity = int.Parse(groups["severity"].Value);
            var state = int.Parse(groups["state"].Value);

            return new Tuple<BackupServerErrorMajor, int, BackupServerSeverity, int>(
                (BackupServerErrorMajor) major, 
                minor, 
                (BackupServerSeverity)severity, 
                state);
        }

        public void Handle(IToken token)
        {
            switch (token)
            {
                case EedToken t:
                    var isBackupServer = t.Message.StartsWith("Backup Server:");
                    
                    var isSevere = !isBackupServer 
                        ? t.Severity > 10 
                        : parseBackupServerSeverity(t.Message).Item3 != BackupServerSeverity.Informational;

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
                errorList.Sort((a, b) => -1 * a.Severity.CompareTo(b.Severity));
                throw new AseException(errorList.ToArray());
            }
        }
    }
}