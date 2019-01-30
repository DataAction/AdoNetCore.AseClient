using System;
using System.Collections.Generic;
using System.IO;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient.Token
{
    internal class RowFormat2Token : IFormatToken
    {
        public TokenType Type => TokenType.TDS_ROWFMT2;

        public void Write(Stream stream, DbEnvironment env)
        {
            throw new NotImplementedException();
        }

        public void Read(Stream stream, DbEnvironment env, IFormatToken previousFormatToken, ref bool streamExceeded)
        {
            Logger.Instance?.WriteLine($"<- {Type}");
            var remainingLength = stream.ReadUInt(ref streamExceeded);
            if (stream.CheckRequiredLength(remainingLength, ref streamExceeded) == false)
                return;
            using (var ts = new ReadablePartialStream(stream, remainingLength))
            {
                var formats = new List<FormatItem>();
                var columnCount = ts.ReadUShort(ref streamExceeded);

                for (var i = 0; i < columnCount; i++)
                {
                    formats.Add(FormatItem.ReadForRow(ts, env.Encoding, Type, ref streamExceeded));
                    if (streamExceeded)
                        return;
                }

                Formats = formats.ToArray();
            }
        }

        public static RowFormat2Token Create(Stream stream, DbEnvironment env, IFormatToken previous, ref bool streamExceeded)
        {
            var t = new RowFormat2Token();
            t.Read(stream, env, previous, ref streamExceeded);
            return t;
        }

        public FormatItem[] Formats { get; set; }
    }
}
