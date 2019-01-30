using System.Collections.Generic;
using System.Data;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Token;

namespace AdoNetCore.AseClient.Internal.Handler
{
    internal class DataReaderCallbackTokenHandler : ITokenHandler
    {
        private static readonly HashSet<TokenType> AcceptedTypes = new HashSet<TokenType>
        {
            TokenType.TDS_ROWFMT2,
            TokenType.TDS_ROWFMT,
            TokenType.TDS_ROW
        };

        private readonly IResultsMessageEventNotifier _eventNotifier;
        private readonly IList<TableResult> _results = new List<TableResult>();
        private readonly AseDataReader _reader;
        private int _numResults;

        public DataReaderCallbackTokenHandler(AseCommand command, CommandBehavior behavior, IResultsMessageEventNotifier eventNotifier)
        {
            _eventNotifier = eventNotifier;
            _reader = new AseDataReader(_results, command, behavior);
            // Set up dummy result set in Reader so that processing in Handle is simpler
            _results.Add(new TableResult());
            _results[0].Rows.Add(null);
            _reader.Read();
        }

        public bool CanHandle(TokenType type)
        {
            return AcceptedTypes.Contains(type);
        }

        public void Handle(IToken token)
        {
            // Only read the data into the first row of the first result set. We aren't allowing result set navigation for the callback.
            switch (token)
            {
                case IFormatToken format:
                    var current = new TableResult { Formats = format.Formats };
                    current.Rows.Add(null);

                    // Don't report the first result set
                    if (_numResults++ > 0)
                    {
                        _eventNotifier?.NotifyResultSet();
                    }

                    _results[0] = current;
                    break;
                case RowToken row:
                    _results[0].Rows[0] = new RowResult { Items = row.Values };
                    _eventNotifier?.NotifyResultRow(_reader);
                    break;
            }
        }
    }
}
