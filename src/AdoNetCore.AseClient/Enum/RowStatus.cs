using System;

// ReSharper disable InconsistentNaming

namespace AdoNetCore.AseClient.Enum
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
        TDS_ROW_HIDDEN = 0x01,
        /// <summary>
        /// This indicates that this column is a key.
        /// </summary>
        TDS_ROW_KEY = 0x02,
        /// <summary>
        /// This column is part of the version key for a row. It is used when updating rows through cursors.
        /// </summary>
        TDS_ROW_VERSION = 0x04,
        /// <summary>
        /// All rows in this column will contain the columnstatus byte. Note that it will be a protocol error to set this bit if the TDS_DATA_COLUMNSTATUS capability bit is off.
        /// </summary>
        TDS_ROW_COLUMNSTATUS = 0x08,
        /// <summary>
        /// This column is updatable.It is used with cursors.
        /// </summary>
        TDS_ROW_UPDATABLE = 0x10,
        /// <summary>
        /// This column allows nulls.
        /// </summary>
        TDS_ROW_NULLALLOWED = 0x20,
        /// <summary>
        /// This column is an identity column.
        /// </summary>
        TDS_ROW_IDENTITY = 0x40,
        /// <summary>
        /// This column has been padded with blank characters.
        /// </summary>
        TDS_ROW_PADCHAR = 0x80
    }
}
