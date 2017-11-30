using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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

        public void Write(Stream stream, Encoding enc)
        {
            throw new NotImplementedException();
        }

        public void Read(Stream stream, Encoding enc, IFormatToken previousFormatToken)
        {
            Console.WriteLine($"<- {Type}");
            var values = new List<object>();
            foreach (var format in previousFormatToken.Formats)
            {
                values.Add(ValueReader.Read(stream, format, enc));
            }
            Values = values.ToArray();
        }

        public static RowToken Create(Stream stream, Encoding enc, IFormatToken previousFormatToken)
        {
            var t = new RowToken();
            t.Read(stream, enc, previousFormatToken);
            return t;
        }
    }
}
