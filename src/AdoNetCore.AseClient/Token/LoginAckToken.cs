using System;
using System.IO;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient.Token
{
    internal class LoginAckToken : IToken
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
                Status = (LoginStatus)ts.ReadByte();
                var versionBuffer = new byte[4];
                ts.Read(versionBuffer, 0, 4);
                TdsVersion = $"{versionBuffer[0]}.{versionBuffer[1]}.{versionBuffer[2]}.{versionBuffer[3]}";

                ProgramName = ts.ReadByteLengthPrefixedString(env.Encoding, ref streamExceeded);

                ts.Read(versionBuffer, 0, 4);
                ProgramVersion = $"{versionBuffer[0]}.{versionBuffer[1]}.{versionBuffer[2]}"; //Sybase driver only reports the first 3 version numbers, eg: 15.0.0
            }
            Logger.Instance?.WriteLine($"<- {Type}: TDS {TdsVersion}, {ProgramName} {ProgramVersion}");
        }

        public static LoginAckToken Create(Stream stream, DbEnvironment env, IFormatToken previous, ref bool streamExceeded)
        {
            var t = new LoginAckToken();
            t.Read(stream, env, previous, ref streamExceeded);
            return t;
        }
    }
}
