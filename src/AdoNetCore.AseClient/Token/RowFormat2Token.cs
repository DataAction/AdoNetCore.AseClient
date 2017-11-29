using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient.Token
{
    internal class RowFormat2Token : IFormatToken
    {
        public TokenType Type => TokenType.TDS_ROWFMT2;

        public void Write(Stream stream, Encoding enc)
        {
            throw new NotImplementedException();
        }

        public void Read(Stream stream, Encoding enc, IFormatToken previousFormatToken)
        {
            Console.WriteLine($"<- {Type}");
            var remainingLength = stream.ReadUInt();
            using (var ts = new ReadablePartialStream(stream, remainingLength))
            {
                var formats = new List<FormatItem>();
                var columnCount = ts.ReadUShort();

                for (var i = 0; i < columnCount; i++)
                {
                    var format = new FormatItem
                    {
                        ColumnLabel = ts.ReadByteLengthPrefixedString(enc),
                        CatalogName = ts.ReadByteLengthPrefixedString(enc),
                        SchemaName = ts.ReadByteLengthPrefixedString(enc),
                        TableName = ts.ReadByteLengthPrefixedString(enc),
                        ColumnName = ts.ReadByteLengthPrefixedString(enc),
                        RowStatus = (RowStatus) ts.ReadUInt(),
                        UserType = ts.ReadInt(),
                        DataType = (TdsDataType) ts.ReadByte(),
                    };

                    int? length = null;

                    switch (format.DataType)
                    {
                        case TdsDataType.TDS_INT4:
                            break;
                        case TdsDataType.TDS_INTN:
                            format.Length = ts.ReadByte();
                            break;
                        default:
                            throw new InvalidOperationException($"Unsupported data type {format.DataType} (column: {format.ColumnName})");
                    }

                    format.LocaleInfo = ts.ReadByteLengthPrefixedString(enc);

                    Console.WriteLine($"  <- {format.ColumnName}: {format.DataType} (len: {format.Length}) (ut:{format.UserType}) (status:{format.RowStatus}) (loc:{format.LocaleInfo})");

                    formats.Add(format);
                }

                Formats = formats.ToArray();
            }
        }

        public static RowFormat2Token Create(Stream stream, Encoding enc, IFormatToken previous)
        {
            var t = new RowFormat2Token();
            t.Read(stream, enc, previous);
            return t;
        }

        public FormatItem[] Formats { get; set; }
    }
}
