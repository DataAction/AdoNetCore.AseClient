using System;
// ReSharper disable InconsistentNaming

namespace AdoNetCore.AseClient.Enum
{
    [Flags]
    internal enum BufferStatus : byte
    {
        /// <summary>
        /// No Status
        /// </summary>
        TDS_BUFSTAT_NONE = 0x00,
        /// <summary>
        /// This is the last buffer in a request or a response.
        /// </summary>
        TDS_BUFSTAT_EOM = 0x01,
        /// <summary>
        /// This is an acknowledgment to the last received attention.
        /// </summary>
        TDS_BUFSTAT_ATTNACK = 0x02,
        /// <summary>
        /// This is an attention request buffer.
        /// </summary>
        TDS_BUFSTAT_ATTN = 0x04,
        /// <summary>
        /// This is an event notification buffer.
        /// </summary>
        TDS_BUFSTAT_EVENT = 0x08,
        /// <summary>
        /// The buffer is encrypted
        /// </summary>
        TDS_BUFSTAT_SEAL = 0x10,
        /// <summary>
        /// The buffer is encrypted (SQL Anywhere CMDSEQ protocol)
        /// </summary>
        TDS_BUFSTAT_ENCRYPT = 0x20
    }
}
