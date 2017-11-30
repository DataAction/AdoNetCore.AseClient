using System.Collections.Generic;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Token;

namespace AdoNetCore.AseClient.Internal.Handler
{
    internal class DataReaderTokenHandler : ITokenHandler
    {
        private static readonly HashSet<TokenType> AcceptedTypes = new HashSet<TokenType>
        {
            TokenType.TDS_ROWFMT2,
            TokenType.TDS_ROWFMT,
            TokenType.TDS_ROW
        };
        private readonly List<IToken> _tokens = new List<IToken>();

        public IEnumerable<TableResult> Results()
        {
            TableResult current = null;
            foreach (var token in _tokens)
            {
                switch (token)
                {
                    case IFormatToken format:
                        if (current != null)
                        {
                            yield return current;
                        }
                        current = new TableResult
                        {
                            Formats = format.Formats
                        };
                        break;
                    case RowToken row:
                        current?.Rows.Add(new RowResult
                        {
                            Items = row.DataItems
                        });
                        break;
                }
            }

            if (current != null)
            {
                yield return current;
            }
        }

        public bool CanHandle(TokenType type)
        {
            return AcceptedTypes.Contains(type);
        }

        public void Handle(IToken token)
        {
            _tokens.Add(token);
        }
    }
}
