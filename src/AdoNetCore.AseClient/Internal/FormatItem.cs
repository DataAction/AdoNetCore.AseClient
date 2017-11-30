using System;
using System.IO;
using System.Text;
using AdoNetCore.AseClient.Enum;

namespace AdoNetCore.AseClient.Internal
{
    internal class FormatItem
    {
        public string ColumnLabel { get; set; }
        public string CatalogName { get; set; }
        public string SchemaName { get; set; }
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public RowFormatItemStatus RowStatus { get; set; }
        public int UserType { get; set; }
        public TdsDataType DataType { get; set; }
        public int? Length { get; set; }
        public int? Precision { get; set; }
        public int? Scale { get; set; }
        public string LocaleInfo { get; set; }

        private string _parameterName { get; set; }
        public string ParameterName
        {
            get => _parameterName;
            set => _parameterName = value == null || value.StartsWith("@") ? value : $"@{value}";
        }
        public bool IsNullable { get; set; }
        public bool IsOutput { get; set; }

        /// <summary>
        /// Relates to TDS_BLOB
        /// </summary>
        public string ClassId { get; set; }

        public static FormatItem ReadForRow(Stream stream, Encoding enc, TokenType srcTokenType)
        {
            //todo make use of srcTokenType
            var format = new FormatItem
            {
                ColumnLabel = stream.ReadByteLengthPrefixedString(enc),
                CatalogName = stream.ReadByteLengthPrefixedString(enc),
                SchemaName = stream.ReadByteLengthPrefixedString(enc),
                TableName = stream.ReadByteLengthPrefixedString(enc),
                ColumnName = stream.ReadByteLengthPrefixedString(enc),
                RowStatus = (RowFormatItemStatus) (srcTokenType == TokenType.TDS_ROWFMT
                    ? (uint) stream.ReadByte()
                    : stream.ReadUInt())
            };

            ReadTypeInfo(format, stream, enc);

            Console.WriteLine($"  <- {format.ColumnName}: {format.DataType} (len: {format.Length}) (ut:{format.UserType}) (status:{format.RowStatus}) (loc:{format.LocaleInfo})");

            return format;
        }

        public static FormatItem ReadForParameter(Stream stream, Encoding enc, TokenType srcTokenType)
        {
            var format = new FormatItem
            {
                ParameterName = stream.ReadByteLengthPrefixedString(enc),
            };
            var status = (ParameterFormatItemStatus) (srcTokenType == TokenType.TDS_PARAMFMT
                ? (uint)stream.ReadByte()
                : stream.ReadUInt());
            format.IsOutput = status.HasFlag(ParameterFormatItemStatus.TDS_PARAM_RETURN);
            format.IsNullable = status.HasFlag(ParameterFormatItemStatus.TDS_PARAM_NULLALLOWED);

            ReadTypeInfo(format, stream, enc);

            Console.WriteLine($"  <- {format.ParameterName}: {format.DataType} (len: {format.Length}) (ut:{format.UserType})");

            return format;
        }

        private static void ReadTypeInfo(FormatItem format, Stream stream, Encoding enc)
        {
            format.UserType = stream.ReadInt();
            format.DataType = (TdsDataType)stream.ReadByte();

            switch (format.DataType)
            {
                case TdsDataType.TDS_INT4:
                    break;
                case TdsDataType.TDS_INTN:
                    format.Length = stream.ReadByte();
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported data type {format.DataType} (column: {format.ColumnName})");
            }

            format.LocaleInfo = stream.ReadByteLengthPrefixedString(enc);
            //ClassId stuff?
        }

        public void WriteForParameter(Stream stream, Encoding enc, TokenType srcTokenType)
        {
            Console.WriteLine($"  -> {ParameterName}: {DataType}");
            if (string.IsNullOrWhiteSpace(ParameterName) || string.Equals("@", ParameterName))
            {
                stream.WriteByte(0);
            }
            else
            {
                stream.WriteBytePrefixedString(ParameterName, enc);
            }

            var nullableStatus = IsNullable ? ParameterFormatItemStatus.TDS_PARAM_NULLALLOWED : 0x00;
            var outputStatus = IsOutput ? ParameterFormatItemStatus.TDS_PARAM_RETURN : 0x00;
            var status = nullableStatus | outputStatus;
            if (srcTokenType == TokenType.TDS_PARAMFMT)
            {
                stream.WriteByte((byte)status);
            }
            else
            {
                stream.WriteUInt((uint)status);
            }

            stream.WriteInt(0); //we don't currently do anything with user types
            stream.WriteByte((byte)DataType);

            switch (DataType)
            {
                case TdsDataType.TDS_INT4:
                    //int4 is fixed-length so don't write anything
                    break;
                default:
                    throw new InvalidOperationException($"{DataType} not yet supported");
            }

            //locale
            stream.WriteByte(0);
        }
    }
}
