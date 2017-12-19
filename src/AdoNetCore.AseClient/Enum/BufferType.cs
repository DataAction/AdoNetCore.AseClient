using System;
// ReSharper disable InconsistentNaming

namespace AdoNetCore.AseClient.Enum
{
    /// <summary>
    /// Refer: Table 51: Buffer Types
    /// Informs the recipient of what type of buffer/message/packet (terminology?) they're receiving.
    /// The recipient may use this information to expect the sender to send certain tokens.
    /// Most are obsolete in TDS 5.0, so to start we will [Obsolete] most of them.
    /// As we implement the protocol, if we need to remove the [Obsolete], we will.
    /// </summary>
    internal enum BufferType : byte
    {
        /// <summary>
        /// None
        /// </summary>
        TDS_BUF_NONE = 0,
        /// <summary>
        /// The buffer contains a language command. TDS does not specify the syntax of the language command.
        /// </summary>
        [Obsolete]
        TDS_BUF_LANG = 1,
        /// <summary>
        /// The buffer contains a login record
        /// </summary>
        TDS_BUF_LOGIN = 2,
        /// <summary>
        /// The buffer contains a remote procedure call command.
        /// </summary>
        [Obsolete]
        TDS_BUF_RPC = 3,
        /// <summary>
        /// The buffer contains the response to a command.
        /// </summary>
        TDS_BUF_RESPONSE = 4,
        /// <summary>
        /// The buffer contains raw unformatted data.
        /// </summary>
        [Obsolete]
        TDS_BUF_UNFMT = 5,
        /// <summary>
        /// The buffer contains a non-expedited attention request.
        /// Refer: 12. Cancel Protocol
        /// </summary>
        TDS_BUF_ATTN = 6,
        /// <summary>
        /// The buffer contains bulk binary data.
        /// </summary>
        [Obsolete]
        TDS_BUF_BULK = 7,
        /// <summary>
        /// A protocol request to setup another logical channel. This buffer is a header only and does not contain any data.
        /// </summary>
        [Obsolete]
        TDS_BUF_SETUP = 8,
        /// <summary>
        /// A protocol request to close a logical channel. This buffer is a header only and does not contain any data.
        /// </summary>
        [Obsolete]
        TDS_BUF_CLOSE = 9,
        /// <summary>
        /// A resource error was detected while attempting to setup or use a logical channel. This buffer is a header only and does not contain any data.
        /// </summary>
        [Obsolete]
        TDS_BUF_ERROR = 10,
        /// <summary>
        /// A protocol acknowledgment associated with the logical channel windowing protocol. This buffer is a header only and does not contain any data.
        /// </summary>
        [Obsolete]
        TDS_BUF_PROTACK = 11,
        /// <summary>
        /// A protocol request to echo the data contained in the buffer.
        /// </summary>
        [Obsolete] TDS_BUF_ECHO = 12,
        /// <summary>
        /// A protocol request to logout an active logical channel. This buffer is a header only and does not contain any data.
        /// </summary>
        [Obsolete]
        TDS_BUF_LOGOUT = 13,
        /// <summary>
        /// What is this??? (Reference pdf asks this question)
        /// </summary>
        [Obsolete]
        TDS_BUF_ENDPARAM = 14,
        /// <summary>
        /// This packet contains a tokenized TDS request or response.
        /// Note: Introduced in TDS 5.0, this obsoletes a lot of the other TDS_BUF_ enums
        /// </summary>
        TDS_BUF_NORMAL = 15,
        /// <summary>
        /// This packet contains an urgent tokenized TDS request or response.
        /// Refer: Event Notifications
        /// </summary>
        [Obsolete]
        TDS_BUF_URGENT = 16,
        /// <summary>
        /// This packet contains a migration protocol message. Currently these are only TDS_MSG tokens.
        /// Refer: 26. Migration Protocol
        /// </summary>
        [Obsolete]
        TDS_BUF_MIGRATE = 17,
        /// <summary>
        /// SQL Anywhere CMDSEQ protocol
        /// </summary>
        [Obsolete]
        TDS_BUF_CMDSEQ_NORMAL = 24,
        /// <summary>
        /// SQL Anywhere CMDSEQ protocol
        /// </summary>
        [Obsolete]
        TDS_BUF_CMDSEQ_LOGIN = 25,
        /// <summary>
        /// SQL Anywhere CMDSEQ protocol
        /// </summary>
        [Obsolete]
        TDS_BUF_CMDSEQ_LIVENESS = 26,
        /// <summary>
        /// SQL Anywhere CMDSEQ protocol
        /// </summary>
        [Obsolete]
        TDS_BUF_CMDSEQ_RESERVED1 = 27,
        /// <summary>
        /// SQL Anywhere CMDSEQ protocol
        /// </summary>
        [Obsolete]
        TDS_BUF_CMDSEQ_RESEVERD2 = 28,
    }
}
