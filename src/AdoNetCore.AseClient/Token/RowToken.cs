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
        public object[] DataItems { get; set; }

        public void Write(Stream stream, Encoding enc)
        {
            throw new NotImplementedException();
        }

        public void Read(Stream stream, Encoding enc, IFormatToken previousFormatToken)
        {
            Console.WriteLine($"<- {Type}");
            var dataItems = new List<object>();
            foreach (var format in previousFormatToken.Formats)
            {
                //read length, can be 0 (for fixed-length types), 1 or 4 bytes (type dependent)
                switch (format.DataType)
                {
                    case TdsDataType.TDS_INTN:
                        var length = stream.ReadByte();
                        if (length == 4)
                        {
                            dataItems.Add(stream.ReadInt());
                        }
                        else
                        {
                            dataItems.Add(null);
                        }
                        break;
                    default:
                        throw new InvalidOperationException($"Unsupported data type {format.DataType}");
                }
            }
            DataItems = dataItems.ToArray();
        }

        public static RowToken Create(Stream stream, Encoding enc, IFormatToken previousFormatToken)
        {
            var t = new RowToken();
            t.Read(stream, enc, previousFormatToken);
            return t;
        }
    }
}
