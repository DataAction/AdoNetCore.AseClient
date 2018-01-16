using System;
using System.IO;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient.Token
{
    internal class ControlToken : IToken
    {
        public TokenType Type => TokenType.TDS_CONTROL;

        public void Write(Stream stream, DbEnvironment env)
        {
            throw new NotImplementedException();
        }

        public void Read(Stream stream, DbEnvironment env, IFormatToken previous)
        {
            Logger.Instance?.WriteLine($"<- {Type}");
            var remainingLength = stream.ReadUShort();
            using (var ts = new ReadablePartialStream(stream, remainingLength))
            {
                for (var i = 0; i < previous.Formats.Length; i++)
                {
                    var customFormatInfo = ts.ReadByteLengthPrefixedString(env.Encoding);
                    if (!string.IsNullOrWhiteSpace(customFormatInfo))
                    {
                        Logger.Instance?.WriteLine($"  <- Fmt: {customFormatInfo}");
                    }
                }
            }
        }

        public static ControlToken Create(Stream stream, DbEnvironment env, IFormatToken previous)
        {
            var t = new ControlToken();
            t.Read(stream, env, previous);
            return t;
        }
    }
}
