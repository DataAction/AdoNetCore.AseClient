using System;
using System.IO;
using System.Text;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient.Token
{
    internal class DoneInProcToken : DoneProcCommonToken, IToken
    {
        public TokenType Type => TokenType.TDS_DONEINPROC;
        public static DoneInProcToken Create(Stream stream, Encoding enc, IFormatToken previous)
        {
            var t = new DoneInProcToken();
            t.Read(stream, enc, previous);
            return t;
        }
    }

    internal class DoneProcToken : DoneProcCommonToken, IToken
    {
        public TokenType Type => TokenType.TDS_DONEPROC;

        public static DoneProcToken Create(Stream stream, Encoding enc, IFormatToken previous)
        {
            var t = new DoneProcToken();
            t.Read(stream, enc, previous);
            return t;
        }
    }

    internal abstract class DoneProcCommonToken
    {
        [Flags]
        public enum DoneProcStatus : ushort
        {
            /// <summary>
            /// This is the final result for the last command. It indicates that the command has completed successfully.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_DONE_FINAL = 0x0000,
            /// <summary>
            /// This Status indicates that there are more results to follow for the current command.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_DONE_MORE = 0x0001,
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
            /// This Status indicates that the count argument is valid. This bit is used to distinguish between an empty count field and a count field with a value of 0.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_DONE_COUNT = 0x0010,
        }

        public DoneProcStatus Status { get; set; }
        public TranState TransactionState { get; set; }
        public int Count { get; set; }

        public void Write(Stream stream, Encoding enc)
        {
            throw new NotImplementedException();
        }

        public void Read(Stream stream, Encoding enc, IFormatToken previous)
        {
            Status = (DoneProcStatus)stream.ReadUShort();
            TransactionState = (TranState)stream.ReadUShort();
            Count = stream.ReadInt();
            Console.WriteLine($"<- {Status}, {TransactionState}, {Count}");
        }
    }
}
