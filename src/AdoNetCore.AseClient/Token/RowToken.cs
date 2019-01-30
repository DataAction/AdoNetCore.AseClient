using System;
using System.Collections.Generic;
using System.IO;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient.Token
{
    /// <summary>
    /// Refer: p. 303 TDS_ROW
    /// </summary>
    internal class RowToken : IToken
    {
        public TokenType Type => TokenType.TDS_ROW;
        public object[] Values { get; set; }

        public void Write(Stream stream, DbEnvironment env)
        {
            throw new NotImplementedException();
        }

        public void Read(Stream stream, DbEnvironment env, IFormatToken previousFormatToken, ref bool streamExceeded)
        {
            Logger.Instance?.WriteLine($"<- {Type}");
            var values = new List<object>();
            foreach (var format in previousFormatToken.Formats)
            {
                values.Add(ValueReader.Read(stream, format, env, ref streamExceeded));
                if (streamExceeded)
                    return;
            }
            Values = values.ToArray();
        }

        public static RowToken Create(Stream stream, DbEnvironment env, IFormatToken previous, ref bool streamExceeded)
        {
            var t = new RowToken();
            t.Read(stream, env, previous, ref streamExceeded);
            return t;
        }
    }
}
