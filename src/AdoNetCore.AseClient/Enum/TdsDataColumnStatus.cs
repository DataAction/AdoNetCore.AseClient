using System;
// ReSharper disable InconsistentNaming

namespace AdoNetCore.AseClient.Enum
{
    [Flags]
    internal enum TdsDataColumnStatus : byte
    {
        /// <summary>
        /// No Data follows, the value is NULL.
        /// </summary>
        TDS_DATA_COLUMNSTATUS_NO_DATA = 0x01,
        /// <summary>
        /// This data value is corrupted due to Overflow/Underflow.
        /// </summary>
        TDS_DATA_COLUMNSTATUS_CORRUPTED = 0x02,
        /// <summary>
        /// This data value has been truncated or rounded.
        /// </summary>
        TDS_DATA_COLUMNSTATUS_TRUNCATED = 0x04,
    }
}
