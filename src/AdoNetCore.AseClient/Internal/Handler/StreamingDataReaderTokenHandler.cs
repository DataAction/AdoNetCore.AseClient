using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Token;

namespace AdoNetCore.AseClient.Internal.Handler
{
    internal class StreamingDataReaderTokenHandler : ITokenHandler
    {
        private static readonly HashSet<TokenType> AcceptedTypes = new HashSet<TokenType>
        {
            TokenType.TDS_ROWFMT2,
            TokenType.TDS_ROWFMT,
            TokenType.TDS_ROW,
            TokenType.TDS_DONE,
            TokenType.TDS_DONEPROC,
            TokenType.TDS_DONEINPROC,
            TokenType.TDS_EED
        };

        private readonly List<AseError> _allErrors = new List<AseError>();
        private bool _foundSevereError;
        private readonly TaskCompletionSource<DbDataReader> _readerSource;
        private readonly AseDataReader _dataReader;
        private TableResult _current;
        private bool _hasFirst;

        public StreamingDataReaderTokenHandler(TaskCompletionSource<DbDataReader> readerSource, AseDataReader dataReader)
        {
            _readerSource = readerSource;
            _dataReader = dataReader;
            _hasFirst = false;
        }

        public bool CanHandle(TokenType type)
        {
            return AcceptedTypes.Contains(type);
        }

        public void Handle(IToken token)
        {
            switch (token)
            {
                case IFormatToken format:
                    ReturnCurrent();
                    _current = new TableResult
                    {
                        Formats = format.Formats
                    };
                    break;
                case RowToken row:
                    _current?.Rows.Add(new RowResult
                    {
                        Items = row.Values
                    });
                    break;
                case DoneToken _:
                case DoneInProcToken _:
                case DoneProcToken _:
                    ReturnCurrent();
                    break;
                case EedToken t:

                    var isSevere = t.Severity > 10;

                    if (isSevere)
                    {
                        _foundSevereError = true;
                    }

                    var error = new AseError
                    {
                        IsError = isSevere,
                        IsFromServer = true,
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
                        IsInformation = !isSevere,
                        IsWarning = false,
                        LineNum = t.LineNumber
                    };

                    _allErrors.Add(error);

                    _dataReader.AddResult(new MessageResult {Errors = new AseErrorCollection(error), Message = error.Message});

                    if (!_hasFirst)
                    {
                        _readerSource.SetResult(_dataReader); // Return the AseDataReader once we have a single table of results.
                        _hasFirst = true;
                    }

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
            }
        }

        private void ReturnCurrent()
        {
            if (_current != null)
            {
                _dataReader.AddResult(_current);

                if (!_hasFirst)
                {
                    _readerSource.SetResult(_dataReader); // Return the AseDataReader once we have a single table of results.
                    _hasFirst = true;
                }
            }

            _current = null;
        }

        public void AssertNoErrors()
        {
            if (_foundSevereError)
            {
                throw new AseException(_allErrors.ToArray());
            }
        }
    }
}
