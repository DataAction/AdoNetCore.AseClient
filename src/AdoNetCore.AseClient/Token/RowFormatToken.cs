using System;
using System.IO;
using System.Text;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient.Token
{
    internal class RowFormatToken : IToken
    {
        [Flags]
        public enum RowStatus : byte
        {
            /// <summary>
            /// This is a hidden column.
            /// It was not listed in the target list of the select statement.
            /// Hidden fields are often used to pass key information back to a client.
            /// For example: select a, b from table T where columns b and c are the key columns.
            /// Columns a, b, and c may be returned and c would have a status of TDS_ROW_HIDDEN|TDS_ROW_KEY.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_ROW_HIDDEN = 0x01,
            /// <summary>
            /// This indicates that this column is a key.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_ROW_KEY = 0x02,
            /// <summary>
            /// This column is part of the version key for a row. It is used when updating rows through cursors.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_ROW_VERSION = 0x04,
            /// <summary>
            /// All rows in this column will contain the columnstatus byte. Note that it will be a protocol error to set this bit if the TDS_DATA_COLUMNSTATUS capability bit is off.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_ROW_COLUMNSTATUS = 0x08,
            /// <summary>
            /// This column is updatable.It is used with cursors.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_ROW_UPDATABLE = 0x10,
            /// <summary>
            /// This column allows nulls.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_ROW_NULLALLOWED = 0x20,
            /// <summary>
            /// This column is an identity column.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_ROW_IDENTITY = 0x40,
            /// <summary>
            /// This column has been padded with blank characters.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_ROW_PADCHAR = 0x80
        }
        public TokenType Type => TokenType.TDS_ROWFMT2;

        public void Write(Stream stream, Encoding enc)
        {
            throw new NotImplementedException();
        }

        public void Read(Stream stream, Encoding enc, IToken previous)
        {
            Console.WriteLine($"<- {Type}");
            var remainingLength = stream.ReadUInt();
            var ts = new ReadablePartialStream(stream, remainingLength);
            var columnCount = ts.ReadUShort();

            for (var i = 0; i < columnCount; i++)
            {
                var columnName = ts.ReadByteLengthPrefixedString(enc);
                var status = (RowStatus)ts.ReadByte();
                var userType = ts.ReadInt();
                var dataType = (TdsDataType)ts.ReadByte();

                switch (dataType)
                {
                    case TdsDataType.TDS_INT4:
                        break;
                }

                var localeInfo = ts.ReadByteLengthPrefixedString(enc);

                Console.WriteLine($"  <- {columnName}: {dataType} ({status}) ({localeInfo})");
            }
        }

        public static RowFormatToken Create(Stream stream, Encoding enc, IToken previous)
        {
            var t = new RowFormatToken();
            t.Read(stream, enc, previous);
            return t;
        }
    }
}
