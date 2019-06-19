using System.Collections.Generic;
using System.Data.Common;
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
            TokenType.TDS_DONEINPROC
        };

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
    }
}
