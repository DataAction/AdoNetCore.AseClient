using System;
using System.IO;
using System.Text;
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
        };

        public TokenType Type => TokenType.TDS_EED;

        public EedStatus Status { get; set; }
        public TranState TransactionStatus { get; set; }
        public string Message { get; set; }
        public int Severity { get; set; }

        public void Write(Stream stream, Encoding enc)
        {
            throw new NotImplementedException();
        }

        public void Read(Stream stream, Encoding enc, IFormatToken previous)
        {
            var remainingLength = stream.ReadUShort();

            var ts = new ReadablePartialStream(stream, remainingLength);
            var messageNumber = ts.ReadUInt();
            var state = ts.ReadByte();
            Severity = ts.ReadByte();
            var sqlStateLen = ts.ReadByte();
            var sqlState = new byte[sqlStateLen];
            ts.Read(sqlState, 0, sqlStateLen);
            Status = (EedStatus) ts.ReadByte();
            TransactionStatus = (TranState) ts.ReadUShort();
            Message = ts.ReadShortLengthPrefixedString(enc);
            var serverName = ts.ReadByteLengthPrefixedString(enc);
            var procName = ts.ReadByteLengthPrefixedString(enc);
            var lineNum = ts.ReadUShort();
        }

        public static EedToken Create(Stream stream, Encoding enc, IFormatToken previous)
        {
            var t = new EedToken();
            t.Read(stream, enc, previous);
            return t;
        }
    }
}
