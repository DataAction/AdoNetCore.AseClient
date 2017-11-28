
// ReSharper disable InconsistentNaming

namespace AdoNetCore.AseClient.Enum
{
    public enum TranState : short
    {
        /// <summary>
        /// Not currently in a transaction
        /// </summary>
        TDS_NOT_IN_TRAN = 0,
        /// <summary>
        /// Request caused transaction to complete successfully.
        /// </summary>
        TDS_TRAN_SUCCEED = 1,
        /// <summary>
        /// A transaction is still in progress on this dialog.
        /// </summary>
        TDS_TRAN_PROGRESS = 2,
        /// <summary>
        /// Request caused a statement abort to occur.
        /// </summary>
        TDS_STMT_ABORT = 3,
        /// <summary>
        /// Request caused transaction to abort.
        /// </summary>
        TDS_TRAN_ABORT = 4
    }
}
