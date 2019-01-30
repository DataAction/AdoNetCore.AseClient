using System;
using System.IO;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient.Token
{
    internal class EedToken : IToken
    {
        public enum EedStatus
        {
            // ReSharper disable once InconsistentNaming
            TDS_NO_EED = 0x00,
            // ReSharper disable once InconsistentNaming
            TDS_EED_FOLLOWS = 0x01,
            // ReSharper disable once InconsistentNaming
            TDS_EED_INFO = 0x02
        }

        public TokenType Type => TokenType.TDS_EED;

        public EedStatus Status { get; set; }
        public TranState TransactionStatus { get; set; }
        public string Message { get; set; }
        public int MessageNumber { get; set; }
        public int Severity { get; set; }
        public string ServerName { get; set; }
        public string ProcedureName { get; set; }
        public int LineNumber { get; set; }
        public int State { get; set; }
        public byte[] SqlState { get; set; }

        public void Write(Stream stream, DbEnvironment env)
        {
            throw new NotImplementedException();
        }

        public void Read(Stream stream, DbEnvironment env, IFormatToken previous, ref bool streamExceeded)
        {
            var remainingLength = stream.ReadUShort(ref streamExceeded);
            if (stream.CheckRequiredLength(remainingLength, ref streamExceeded) == false)
                return;

            using (var ts = new ReadablePartialStream(stream, remainingLength))
            {
                MessageNumber = ts.ReadInt(ref streamExceeded);
                State = ts.ReadByte();                  // Already checked remainingLength so can use non-checking version
                Severity = ts.ReadByte();
                var sqlStateLen = ts.ReadByte();
                SqlState = new byte[sqlStateLen];
                ts.Read(SqlState, 0, sqlStateLen);
                Status = (EedStatus)ts.ReadByte();
                TransactionStatus = (TranState)ts.ReadUShort(ref streamExceeded);
                Message = ts.ReadShortLengthPrefixedString(env.Encoding, ref streamExceeded);
                ServerName = ts.ReadByteLengthPrefixedString(env.Encoding, ref streamExceeded);
                ProcedureName = ts.ReadByteLengthPrefixedString(env.Encoding, ref streamExceeded);
                LineNumber = ts.ReadUShort(ref streamExceeded);
            }
        }

        public static EedToken Create(Stream stream, DbEnvironment env, IFormatToken previous, ref bool streamExceeded)
        {
            var t = new EedToken();
            t.Read(stream, env, previous, ref streamExceeded);
            return t;
        }
    }
}
