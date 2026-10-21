using System.Collections.Generic;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Token;

namespace AdoNetCore.AseClient.Internal.Handler
{
    internal class DoneTokenHandler : ITokenHandler
    {
        public TranState? TransactionState { get; private set; }
        /// <summary>
        /// The rows changed by the statements, or -1 when none reported a count that was not a result set's row count.
        /// DbCommand.ExecuteNonQuery: "For UPDATE, INSERT, and DELETE statements, the return value is the number of rows
        /// affected by the command. For all other types of statements, the return value is -1."
        /// </summary>
        public int RowsAffected => _hasCount ? _rowsAffected : -1;
        public bool Canceled { get; private set; }
        
        private int _rowsAffected;
        private bool _hasCount;

        // A done token that ends a result set counts the rows selected, not rows affected, as the data reader also treats it.
        private bool _afterResultSet;

        private static readonly HashSet<TokenType> AllowedTypes = new HashSet<TokenType>
        {
            TokenType.TDS_DONE,
            TokenType.TDS_DONEPROC,
            TokenType.TDS_DONEINPROC,
            TokenType.TDS_ROWFMT,
            TokenType.TDS_ROWFMT2
        };

        public bool CanHandle(TokenType type)
        {
            return AllowedTypes.Contains(type);
        }

        public void Handle(IToken token)
        {
            switch (token)
            {
                case IFormatToken _:
                    _afterResultSet = true;
                    break;
                //TDS_DONE_COUNT - this means that the count we received is meaningful
                case DoneToken t:
                    Logger.Instance?.WriteLine($"{t.Type}: {t.Status}");
                    if (t.Status.HasFlag(DoneToken.DoneStatus.TDS_DONE_INXACT))
                    {
                        Logger.Instance?.WriteLine($"  {t.TransactionState}");
                        TransactionState = t.TransactionState;
                    }
                    if (t.Status.HasFlag(DoneToken.DoneStatus.TDS_DONE_ATTN))
                    {
                        Canceled = true;
                    }
                    OnDone(t.Status.HasFlag(DoneToken.DoneStatus.TDS_DONE_COUNT), t.Count);
                    break;
                case DoneProcToken t:
                    Logger.Instance?.WriteLine($"{t.Type}: {t.Status}");
                    if (t.Status.HasFlag(DoneToken.DoneStatus.TDS_DONE_INXACT))
                    {
                        Logger.Instance?.WriteLine($"  {t.TransactionState}");
                        TransactionState = t.TransactionState;
                    }
                    OnDone(t.Status.HasFlag(DoneToken.DoneStatus.TDS_DONE_COUNT), t.Count);
                    break;
                case DoneInProcToken t:
                    Logger.Instance?.WriteLine($"{t.Type}: {t.Status}");
                    if (t.Status.HasFlag(DoneToken.DoneStatus.TDS_DONE_INXACT))
                    {
                        Logger.Instance?.WriteLine($"  {t.TransactionState}");
                        TransactionState = t.TransactionState;
                    }
                    OnDone(t.Status.HasFlag(DoneToken.DoneStatus.TDS_DONE_COUNT), t.Count);
                    break;
                default:
                    return;
            }
        }

        private void OnDone(bool hasCount, int count)
        {
            var endsResultSet = _afterResultSet;
            _afterResultSet = false;

            if (hasCount && !endsResultSet)
            {
                _rowsAffected += count;
                _hasCount = true;
            }
        }
    }
}
