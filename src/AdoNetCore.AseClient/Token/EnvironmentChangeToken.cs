using System.Collections.Generic;
using System.IO;
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

            public void Write (Stream stream, Encoding enc)
            {
                stream.WriteByte((byte)Type);
                stream.WriteBytePrefixedString(NewValue, enc);
                stream.WriteBytePrefixedString(OldValue, enc);
            }
        }

        public EnvironmentChange[] Changes { get; set; }

        public EnvironmentChangeToken()
        {
            Changes = new EnvironmentChange[0];
        }

        public TokenType Type => TokenType.TDS_ENVCHANGE;

        public void Write(Stream stream, DbEnvironment env)
        {
            Logger.Instance?.WriteLine($"Write {Type}");
            stream.WriteByte((byte)Type);
            //we can't write directly to the stream because we need to know the length up-front
            using (var ms = new MemoryStream())
            {
                foreach (var c in Changes)
                {
                    c.Write(ms, env.Encoding);
                }

                ms.Seek(0, SeekOrigin.Begin);
                stream.WriteShort((short)ms.Length);
                ms.CopyTo(stream);
            }
        }

        public void Read(Stream stream, DbEnvironment env, IFormatToken previous)
        {
            var remainingLength = stream.ReadShort();
            using (var ts = new ReadablePartialStream(stream, remainingLength))
            {
                var changes = new List<EnvironmentChange>();

                while (ts.Position < ts.Length)
                {
                    var change = new EnvironmentChange
                    {
                        Type = (ChangeType)ts.ReadByte(),
                        NewValue = ts.ReadByteLengthPrefixedString(env.Encoding),
                        OldValue = ts.ReadByteLengthPrefixedString(env.Encoding)
                    };
                    changes.Add(change);
                }
                Changes = changes.ToArray();
            }
        }

        public static EnvironmentChangeToken Create(Stream stream, DbEnvironment env, IFormatToken previous)
        {
            var t = new EnvironmentChangeToken();
            t.Read(stream, env, previous);
            return t;
        }
    }
}
