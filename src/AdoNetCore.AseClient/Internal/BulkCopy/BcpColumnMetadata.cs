using AdoNetCore.AseClient.Enum;

namespace AdoNetCore.AseClient.Internal.BulkCopy
{
    /// <summary>
    /// Column metadata parsed from the server's response to an INSERT BULK command.
    /// Used to determine how to format row data in native BCP format.
    /// </summary>
    internal class BcpColumnMetadata
    {
        /// <summary>
        /// Column ordinal (0-based position in the row).
        /// </summary>
        public int Ordinal { get; set; }

        /// <summary>
        /// Column name from the server.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// TDS data type for the column.
        /// </summary>
        public TdsDataType DataType { get; set; }

        /// <summary>
        /// Maximum length for variable-length types, or the fixed size for fixed-length types.
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// Precision for DECN/NUMN types.
        /// </summary>
        public byte Precision { get; set; }

        /// <summary>
        /// Scale for DECN/NUMN types.
        /// </summary>
        public byte Scale { get; set; }

        /// <summary>
        /// User-defined type from server metadata.
        /// </summary>
        public int UserType { get; set; }

        /// <summary>
        /// Row format status flags (nullable, identity, etc.).
        /// </summary>
        public RowFormatItemStatus RowStatus { get; set; }

        /// <summary>
        /// Whether this column allows NULL values.
        /// </summary>
        public bool IsNullable => (RowStatus & RowFormatItemStatus.TDS_ROW_NULLALLOWED) != 0;

        /// <summary>
        /// Whether this column is an identity column.
        /// </summary>
        public bool IsIdentity => (RowStatus & RowFormatItemStatus.TDS_ROW_IDENTITY) != 0;

        /// <summary>
        /// Whether this is a fixed-length data type in native BCP format.
        /// Fixed-length types are packed without length prefixes.
        /// </summary>
        /// <summary>
        /// Whether this column occupies a fixed offset in the native BCP row format.
        /// Determined by the server's INSERT BULK metadata: positive offset = fixed, negative = variable.
        /// </summary>
        public bool IsFixedLength { get; set; }

        /// <summary>
        /// Gets the native byte size for fixed-length data types.
        /// For types like TDS_CHAR, TDS_NUMN, TDS_DECN, the size comes from the Length property.
        /// </summary>
        public int FixedByteSize
        {
            get
            {
                switch (DataType)
                {
                    case TdsDataType.TDS_BIT:
                    case TdsDataType.TDS_INT1:
                        return 1;
                    case TdsDataType.TDS_INT2:
                    case TdsDataType.TDS_UINT2:
                        return 2;
                    case TdsDataType.TDS_INT4:
                    case TdsDataType.TDS_UINT4:
                    case TdsDataType.TDS_FLT4:
                    case TdsDataType.TDS_SHORTDATE:
                    case TdsDataType.TDS_DATE:
                    case TdsDataType.TDS_TIME:
                    case TdsDataType.TDS_SHORTMONEY:
                        return 4;
                    case TdsDataType.TDS_INT8:
                    case TdsDataType.TDS_UINT8:
                    case TdsDataType.TDS_FLT8:
                    case TdsDataType.TDS_DATETIME:
                    case TdsDataType.TDS_MONEY:
                        return 8;
                    // Types whose fixed size depends on column definition
                    case TdsDataType.TDS_CHAR:
                    case TdsDataType.TDS_BINARY:
                    case TdsDataType.TDS_NUMN:
                    case TdsDataType.TDS_DECN:
                        return Length;
                    default:
                        return Length > 0 ? Length : 0;
                }
            }
        }

        /// <summary>
        /// Creates BcpColumnMetadata from a FormatItem received from the server.
        /// </summary>
        public static BcpColumnMetadata FromFormatItem(FormatItem format, int ordinal)
        {
            return new BcpColumnMetadata
            {
                Ordinal = ordinal,
                Name = format.DisplayColumnName,
                DataType = format.DataType,
                Length = format.Length ?? 0,
                Precision = format.Precision ?? 0,
                Scale = format.Scale ?? 0,
                UserType = format.UserType,
                RowStatus = format.RowStatus
            };
        }
    }
}
