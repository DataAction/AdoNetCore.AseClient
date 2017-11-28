using System;
using System.IO;
using System.Linq;
using System.Text;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient.Token
{
    public class LoginAckToken : IToken
    {
        public enum LoginStatus : byte
        {
            /// <summary>
            /// The login request completed successfully.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_LOG_SUCCEED = 5,

            /// <summary>
            /// The login request failed. The client must terminate the dialog and restart to attempt another login request.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_LOG_FAIL = 6,

            /// <summary>
            /// The server is requesting that the client complete a negotiation before completing the login request. The login negotiation is done using the TDS_MSG token.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_LOG_NEGOTIATE = 7
        }

        public TokenType Type => TokenType.TDS_LOGINACK;

        public LoginStatus Status { get; set; }
        public string TdsVersion { get; set; }
        public string ProgramName { get; set; }
        public string ProgramVersion { get; set; }

        public void Write(Stream stream, Encoding enc)
        {
            throw new NotImplementedException();
        }

        public void Read(Stream stream, Encoding enc, IToken previous)
        {
            var remainingLength = stream.ReadUShort();
            var ts = new ReadablePartialStream(stream, remainingLength);

            Status = (LoginStatus)ts.ReadByte();

            var versionBuffer = new byte[4];
            ts.Read(versionBuffer, 0, 4);
            TdsVersion = $"{versionBuffer[0]}.{versionBuffer[1]}.{versionBuffer[2]}.{versionBuffer[3]}";

            ProgramName = ts.ReadByteLengthPrefixedString(enc);

            ts.Read(versionBuffer, 0, 4);
            ProgramVersion = $"{versionBuffer[0]}.{versionBuffer[1]}.{versionBuffer[2]}.{versionBuffer[3]}";
        }

        public static LoginAckToken Create(Stream stream, Encoding enc, IToken previous)
        {
            var t = new LoginAckToken();
            t.Read(stream, enc, previous);
            return t;
        }
    }
}
