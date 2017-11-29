using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient.Token
{
    internal class EnvironmentChangeToken : IToken
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

            public byte[] GetBytes(Encoding enc)
            {
                var oldValue = enc.GetBytes(OldValue);
                var newValue = enc.GetBytes(NewValue);

                var response = new byte[3 + oldValue.Length + newValue.Length];

                return new[]
                {
                    (byte) Type,
                    (byte) newValue.Length
                }
                .Concat(newValue)
                .Concat(new[]
                    {
                        (byte) oldValue.Length
                    })
                .Concat(oldValue).ToArray();
            }
        }

        public EnvironmentChange[] Changes { get; set; }

        public EnvironmentChangeToken()
        {
            Changes = new EnvironmentChange[0];
        }

        public TokenType Type => TokenType.TDS_ENVCHANGE;

        public void Write(Stream stream, Encoding enc)
        {
            Console.WriteLine($"Write {Type}");
            stream.WriteByte((byte)Type);

            var changeBytes = Changes
                .SelectMany(c => c.GetBytes(enc))
                .ToArray();
            var length = (short) changeBytes.Length;
            stream.WriteShort(length);
            stream.Write(changeBytes, 0, length);
        }

        public void Read(Stream stream, Encoding enc, IFormatToken previous)
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

        public static EnvironmentChangeToken Create(Stream stream, Encoding enc, IFormatToken previous)
        {
            var t = new EnvironmentChangeToken();
            t.Read(stream, enc, previous);
            return t;
        }
    }
}
