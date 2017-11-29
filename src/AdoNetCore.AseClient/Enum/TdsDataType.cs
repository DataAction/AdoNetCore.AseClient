
// ReSharper disable InconsistentNaming
namespace AdoNetCore.AseClient.Enum
{
    internal enum TdsDataType : byte
    {
        /// <summary>
        /// Fixed Nullable Converted Description
        /// Yes No No Binary
        /// </summary>
        TDS_BINARY = 0x2D,
        /// <summary>
        /// Fixed Nullable Converted Description
        /// Yes No No Bit
        /// </summary>
        TDS_BIT = 0x32,
        /// <summary>
        /// Fixed Nullable Converted Description
        /// No Yes No Serialized Object
        /// </summary>
        TDS_BLOB = 0x24,
        /// <summary>
        /// Fixed Nullable Converted Description
        /// No Yes Yes Boundary
        /// </summary>
        TDS_BOUNDARY = 0x68,
        /// <summary>
        /// Fixed Nullable Converted Description
        /// Yes No Yes Character
        /// </summary>
        TDS_CHAR = 0x2F,
        /// <summary>
        /// Fixed Nullable Converted Description
        /// Yes No No Date
        /// </summary>
        TDS_DATE = 0x31,
        /// <summary>
        /// Fixed Nullable Converted Description
        /// No Yes Yes Date
        /// </summary>
        TDS_DATEN = 0x7B,
        /// <summary>
        /// Fixed Nullable Converted Description
        /// Yes No Yes Date/time
        /// </summary>
        TDS_DATETIME = 0x3D,
        /// <summary>
        /// Fixed Nullable Converted Description
        /// No Yes Yes Date/time
        /// </summary>
        TDS_DATETIMEN = 0x6F,
        /// <summary>
        /// Fixed Nullable Converted Description
        /// No Yes Yes Decimal
        /// </summary>
        TDS_DECN = 0x6A,
        /// <summary>
        /// Fixed Nullable Converted Description
        /// Yes No Yes Float
        /// </summary>
        TDS_FLT4 = 0x3B,
        /// <summary>
        /// Fixed Nullable Converted Description
        /// Yes No Yes Float
        /// </summary>
        TDS_FLT8 = 0x3E,
        /// <summary>
        /// Fixed Nullable Converted Description
        /// No Yes Yes Float
        /// </summary>
        TDS_FLTN = 0x6D,
        /// <summary>
        /// Fixed Nullable Converted Description
        /// No Yes No Image
        /// </summary>
        TDS_IMAGE = 0x22,
        /// <summary>
        /// Fixed Nullable Converted Description
        /// Yes No No Unsigned Integer
        /// </summary>
        TDS_INT1 = 0x30,
        /// <summary>
        /// Fixed Nullable Converted Description
        /// Yes No Yes Integer
        /// </summary>
        TDS_INT2 = 0x34,
        /// <summary>
        /// Fixed Nullable Converted Description
        /// Yes No Yes Integer
        /// </summary>
        TDS_INT4 = 0x38,
        /// <summary>
        /// Fixed Nullable Converted Description
        /// Yes No Yes Integer
        /// </summary>
        TDS_INT8 = 0xbf,
        /// <summary>
        /// Fixed Nullable Converted Description
        /// Yes No Yes Time Interval
        /// </summary>
        TDS_INTERVAL = 0x2E,
        /// <summary>
        /// Fixed Nullable Converted Description
        /// No Yes Yes Integer
        /// </summary>
        TDS_INTN = 0x26,
        /// <summary>
        /// Fixed Nullable Converted Description
        /// No Yes No Binary
        /// </summary>
        TDS_LONGBINARY = 0xE1,
        /// <summary>
        /// Fixed Nullable Converted Description
        /// No Yes Yes Character
        /// </summary>
        TDS_LONGCHAR = 0xAF,
        /// <summary>
        /// Fixed Nullable Converted Description
        /// Yes No Yes Money
        /// </summary>
        TDS_MONEY = 0x3C,
        /// <summary>
        /// Fixed Nullable Converted Description
        /// No Yes Yes Money
        /// </summary>
        TDS_MONEYN = 0x6E,
        /// <summary>
        /// Fixed Nullable Converted Description
        /// No Yes Yes Numeric
        /// </summary>
        TDS_NUMN = 0x6C,
        /// <summary>
        /// Fixed Nullable Converted Description
        /// No Yes Yes Sensitivity
        /// </summary>
        TDS_SENSITIVITY = 0x67,
        /// <summary>
        /// Fixed Nullable Converted Description
        /// Yes No Yes Date/time
        /// </summary>
        TDS_SHORTDATE = 0x3A,
        /// <summary>
        /// Fixed Nullable Converted Description
        /// Yes No Yes Money
        /// </summary>
        TDS_SHORTMONEY = 0x7A,
        /// <summary>
        /// Fixed Nullable Converted Description
        /// Yes No No Signed Integer
        /// </summary>
        TDS_SINT1 = 0xb0,
        /// <summary>
        /// Fixed Nullable Converted Description
        /// No Yes Yes Text
        /// </summary>
        TDS_TEXT = 0x23,
        /// <summary>
        /// Fixed Nullable Converted Description
        /// Yes No No Time
        /// </summary>
        TDS_TIME = 0x33,
        /// <summary>
        /// Fixed Nullable Converted Description
        /// No Yes Yes Time
        /// </summary>
        TDS_TIMEN = 0x93,
        /// <summary>
        /// Fixed Nullable Converted Description
        /// Yes No Yes Unsigned Integer
        /// </summary>
        TDS_UINT2 = 0x41,
        /// <summary>
        /// Fixed Nullable Converted Description
        /// Yes No Yes Unsigned Integer
        /// </summary>
        TDS_UINT4 = 0x42,
        /// <summary>
        /// Fixed Nullable Converted Description
        /// Yes No Yes Unsigned Integer
        /// </summary>
        TDS_UINT8 = 0x43,
        /// <summary>
        /// Fixed Nullable Converted Description
        /// No Yes Yes Unsigned Integer
        /// </summary>
        TDS_UINTN = 0x44,
        /// <summary>
        /// Fixed Nullable Converted Description
        /// No Yes Yes Unicode UTF-16 Text
        /// </summary>
        TDS_UNITEXT = 0xae,
        /// <summary>
        /// Fixed Nullable Converted Description
        /// No Yes No Binary
        /// </summary>
        TDS_VARBINARY = 0x25,
        /// <summary>
        /// Fixed Nullable Converted Description
        /// No Yes Yes Character
        /// </summary>
        TDS_VARCHAR = 0x27,
        /// <summary>
        /// Fixed Nullable Converted Description
        /// N/A N/A N/A Void (unknown)
        /// </summary>
        TDS_VOID = 0x1f,
        /// <summary>
        /// Fixed Nullable Converted Description
        /// No Yes Yes XML
        /// </summary>
        TDS_XML = 0xA3,
    }
}
