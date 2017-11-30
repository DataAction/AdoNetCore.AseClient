using System;
// ReSharper disable InconsistentNaming

namespace AdoNetCore.AseClient.Enum
{
    [Flags]
    internal enum ParameterFormatItemStatus : byte
    {
        /// <summary>
        /// This is a return parameter. It is like a parameter passed by reference.
        /// </summary>
        TDS_PARAM_RETURN = 0x01,
        /// <summary>
        /// This parameter will have a columnstatus byte in its corresponding TDS_PARAM token. Note that it will be a protocol error for this bit to be set when the TDS_DATA_COLUMNSTATUS capability bit is off.
        /// </summary>
        TDS_PARAM_COLUMNSTATUS = 0x08,
        /// <summary>
        /// This parameter can be NULL
        /// </summary>
        TDS_PARAM_NULLALLOWED = 0x20
    }
}
