using System;
using System.IO;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient.Token
{
    internal abstract class DoneProcCommonToken
    {
        public DoneToken.DoneStatus Status { get; set; }
        public TranState TransactionState { get; set; }
        public int Count { get; set; }

        public void Write(Stream stream, DbEnvironment env)
        {
            throw new NotImplementedException();
        }

        public void Read(Stream stream, DbEnvironment env, IFormatToken previous)
        {
            Status = (DoneToken.DoneStatus)stream.ReadUShort();
            TransactionState = (TranState)stream.ReadUShort();
            Count = stream.ReadInt();
            Logger.Instance?.WriteLine($"<- {Status}, {TransactionState}, {Count}");
        }
    }
}
