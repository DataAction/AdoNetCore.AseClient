using System;
using System.IO;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient.Token
{
    internal class DoneToken : IToken
    {
        [Flags]
        public enum DoneStatus : ushort
        {
            /// <summary>
            /// This is the final result for the last command. It indicates that the command has completed successfully.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_DONE_FINAL =0x0000,
            /// <summary>
            /// This Status indicates that there are more results to follow for the current command.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_DONE_MORE =0x0001,
            /// <summary>
            /// This indicates that an error occurred on the current command.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_DONE_ERROR = 0x0002,
            /// <summary>
            /// There is a transaction in progress for the current request.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_DONE_INXACT = 0x0004,
            /// <summary>
            /// This TDS_DONE is from the results of a stored procedure.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_DONE_PROC = 0x0008,
            /// <summary>
            /// This Status indicates that the count argument is valid. This bit is used to distinguish between an empty count field and a count field with a value of 0
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_DONE_COUNT = 0x0010,
            /// <summary>
            /// This TDS_DONE is acknowledging an attention command.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_DONE_ATTN = 0x0020,
            /// <summary>
            /// This TDS_DONE was generated as part of an event notification.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_DONE_EVENT = 0x0040,
        }

        public TokenType Type => TokenType.TDS_DONE;

        public DoneStatus Status { get; set; }
        public TranState TransactionState { get; set; }
        public int Count { get; set; }

        public void Write(Stream stream, DbEnvironment env)
        {
            Logger.Instance?.WriteLine($"-> {Type}: {Status} ({Count})");
            stream.WriteByte((byte)Type);
            stream.WriteUShort((ushort)Status);
            stream.WriteUShort((ushort)TransactionState);
            stream.WriteInt(Count);
        }

        public void Read(Stream stream, DbEnvironment env, IFormatToken previous)
        {
            Status = (DoneStatus) stream.ReadUShort();
            TransactionState = (TranState) stream.ReadUShort();
            Count = stream.ReadInt();
            Logger.Instance?.WriteLine($"<- {Type}: {Status} ({Count})");
        }

        public static DoneToken Create(Stream stream, DbEnvironment env, IFormatToken previous)
        {
            var t = new DoneToken();
            t.Read(stream, env, previous);
            return t;
        }
    }
}
