using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient.Token
{
    public class EnvironmentChangeToken : IToken
    {
        public enum ChangeType : byte
        {
            // ReSharper disable once InconsistentNaming
            TDS_ENV_DB = 1,
            // ReSharper disable once InconsistentNaming
            TDS_ENV_LANG = 2,
            // ReSharper disable once InconsistentNaming
            TDS_ENV_CHARSET = 3,
            // ReSharper disable once InconsistentNaming
            TDS_ENV_PACKSIZE = 4
        }

        public class EnvironmentChange
        {
            public ChangeType Type { get; set; }
            public string NewValue { get; set; }
            public string OldValue { get; set; }
        }

        public EnvironmentChange[] Changes { get; set; }

        public EnvironmentChangeToken()
        {
            Changes = new EnvironmentChange[0];
        }

        public TokenType Type => TokenType.TDS_ENVCHANGE;

        public void Write(Stream stream, Encoding enc)
        {
            throw new NotImplementedException();
        }

        public void Read(Stream stream, Encoding enc, IToken previous)
        {
            var remainingLength = stream.ReadShort();
            var ts = new ReadablePartialStream(stream, remainingLength);

            var changes = new List<EnvironmentChange>();

            while (ts.Position < ts.Length)
            {
                var change = new EnvironmentChange
                {
                    Type = (ChangeType)ts.ReadByte(),
                    NewValue = ts.ReadByteLengthPrefixedString(enc),
                    OldValue = ts.ReadByteLengthPrefixedString(enc)
                };
                changes.Add(change);
            }

            Changes = changes.ToArray();
        }

        public static EnvironmentChangeToken Create(Stream stream, Encoding enc, IToken previous)
        {
            var t = new EnvironmentChangeToken();
            t.Read(stream, enc, previous);
            return t;
        }
    }
}
