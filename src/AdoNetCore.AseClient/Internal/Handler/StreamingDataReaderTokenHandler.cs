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
        private bool _hasFormatted;
        private int _runningTotalRecordsAffected;

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
                    _hasFormatted = true;
                    break;
                case RowToken row:
                    _current?.Rows.Add(new RowResult
                    {
                        Items = row.Values
                    });
                    break;
                case DoneToken t:
                    HandleDoneToken(t);
                    break;
                case DoneInProcToken _:
                case DoneProcToken _:
                    ReturnCurrent();
                    break;
                case EedToken t:
                    HandleEedToken(t);
                    break;
            }
        }

        private void HandleDoneToken(DoneToken token)
        {
            if (_hasFormatted)
            {
                ReturnCurrent();
                _hasFormatted = false;
            }
            else if ((token.Status & DoneToken.DoneStatus.TDS_DONE_COUNT) == DoneToken.DoneStatus.TDS_DONE_COUNT)
            {
                _runningTotalRecordsAffected += token.Count;
                _dataReader.SetRecordsAffected(_runningTotalRecordsAffected);
            }
            if ((token.Status & DoneToken.DoneStatus.TDS_DONE_MORE) == DoneToken.DoneStatus.TDS_DONE_FINAL)
            {
                ReturnCurrent();
            }
        }

        private void HandleEedToken(EedToken token)
        {
            var isSevere = token.Severity > 10;

            if (isSevere)
            {
                _foundSevereError = true;
            }

            var error = new AseError
            {
                IsError = isSevere,
                IsFromServer = true,
                Message = token.Message,
                MessageNumber = token.MessageNumber,
                ProcName = token.ProcedureName,
                State = token.State,
                TranState = (int)token.TransactionStatus,
                Status = (int)token.Status,
                Severity = token.Severity,
                ServerName = token.ServerName,
                SqlState = Encoding.ASCII.GetString(token.SqlState),
                IsFromClient = false,
                IsInformation = !isSevere,
                IsWarning = false,
                LineNum = token.LineNumber
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
                _current.Messages.Add(new MessageResult { Errors = new AseErrorCollection(error), Message = error.Message });
            }

            var msgType = isSevere
                ? "ERROR"
                : "INFO ";

            var formatted = $"{msgType} [{token.Severity}] [L:{token.LineNumber}]: {token.Message}";

            if (formatted.EndsWith("\n"))
            {
                Logger.Instance?.Write(formatted);
            }
            else
            {
                Logger.Instance?.WriteLine(formatted);
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
