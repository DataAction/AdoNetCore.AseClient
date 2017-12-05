using System;
using System.IO;
using System.Text;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient.Token
{
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
            Logger.Instance?.WriteLine($"<- {Status}, {TransactionState}, {Count}");
        }
    }
}