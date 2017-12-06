using System.Collections.Generic;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Token;

namespace AdoNetCore.AseClient.Internal.Handler
{
    internal class DoneTokenHandler : ITokenHandler
    {
        public TranState? TransactionState { get; private set; }
        public int RowsAffected { get; private set; }
        
        private static readonly HashSet<TokenType> AllowedTypes = new HashSet<TokenType>
        {
            TokenType.TDS_DONE,
            TokenType.TDS_DONEPROC,
            TokenType.TDS_DONEINPROC
        };

        public bool CanHandle(TokenType type)
        {
            return AllowedTypes.Contains(type);
        }

        public void Handle(IToken token)
        {
            switch (token)
            {
                //TDS_DONE_COUNT - this means that the count we received is meaningful
                case DoneToken t:
                    Logger.Instance?.WriteLine($"{t.Type}: {t.Status}");
                    if (t.Status.HasFlag(DoneToken.DoneStatus.TDS_DONE_INXACT))
                    {
                        Logger.Instance?.WriteLine($"  {t.TransactionState}");
                        TransactionState = t.TransactionState;
                    }
                    if (t.Status.HasFlag(DoneToken.DoneStatus.TDS_DONE_COUNT))
                    {
                        RowsAffected += t.Count;
                    }
                    break;
                case DoneProcToken t:
                    Logger.Instance?.WriteLine($"{t.Type}: {t.Status}");
                    if (t.Status.HasFlag(DoneProcCommonToken.DoneProcStatus.TDS_DONE_INXACT))
                    {
                        Logger.Instance?.WriteLine($"  {t.TransactionState}");
                        TransactionState = t.TransactionState;
                    }
                    if (t.Status.HasFlag(DoneProcCommonToken.DoneProcStatus.TDS_DONE_COUNT))
                    {
                        RowsAffected += t.Count;
                    }
                    break;
                case DoneInProcToken t:
                    Logger.Instance?.WriteLine($"{t.Type}: {t.Status}");
                    if (t.Status.HasFlag(DoneProcCommonToken.DoneProcStatus.TDS_DONE_INXACT))
                    {
                        Logger.Instance?.WriteLine($"  {t.TransactionState}");
                        TransactionState = t.TransactionState;
                    }
                    if (t.Status.HasFlag(DoneProcCommonToken.DoneProcStatus.TDS_DONE_COUNT))
                    {
                        RowsAffected += t.Count;
                    }
                    break;
                default:
                    return;
            }
        }
    }
}