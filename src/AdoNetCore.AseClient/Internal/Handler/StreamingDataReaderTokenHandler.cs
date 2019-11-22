using System;
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
        private readonly IInfoMessageEventNotifier _eventNotifier;
        private TableResult _current;
        private bool _hasFirstResultSet;
        private bool _hasSentCurrent;

        public StreamingDataReaderTokenHandler(TaskCompletionSource<DbDataReader> readerSource, AseDataReader dataReader, IInfoMessageEventNotifier eventNotifier)
        {
            _readerSource = readerSource;
            _dataReader = dataReader;
            _eventNotifier = eventNotifier;
            _hasFirstResultSet = false;
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
                        Formats = format.Formats,
                        RecordsAffected = -1
                    };
                    _hasSentCurrent = false;
                    break;
                case RowToken row:
                    _current?.Rows.Add(new RowResult
                    {
                        Items = row.Values

                    });
                    break;
                case DoneToken t:
                    EnsureResultExists(t.Count);
                    ReturnCurrent();
                    break;
                case DoneInProcToken t:
                    EnsureResultExists(t.Count);
                    ReturnCurrent();
                    break;
                case DoneProcToken t:
                    EnsureResultExists(t.Count);
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

                    // if we have not encountered any data yet, then send messages straight out.
                    if (_current == null)
                    {
                        _eventNotifier?.NotifyInfoMessage(new AseErrorCollection(error), error.Message);
                    }
                    // else if we have encountered any data, then add the messages to the data reader so that they are returned with data/message order preserved.
                    else
                    {
                        _current.Messages.Add(new MessageResult {Errors = new AseErrorCollection(error), Message = error.Message});
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

        private void EnsureResultExists(int affectedCount)
        {
            if (_current == null)
            {
                _current = new TableResult()
                {
                    RecordsAffected = affectedCount
                };
            }
        }

        private void ReturnCurrent()
        {
            if (_current != null && !_hasSentCurrent)
            {
                _dataReader.AddResult(_current);
                _hasSentCurrent = true;

                if (!_hasFirstResultSet)
                {
                    _readerSource.SetResult(_dataReader); // Return the AseDataReader once we have a single table of results.
                    _hasFirstResultSet = true;
                }
            }
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
