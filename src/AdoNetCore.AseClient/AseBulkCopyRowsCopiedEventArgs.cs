using System;

namespace AdoNetCore.AseClient
{
    /// <summary>
    /// Provides data for the <see cref="AseBulkCopy.AseRowsCopied"/> event.
    /// </summary>
    public class AseBulkCopyRowsCopiedEventArgs : EventArgs
    {
        /// <summary>
        /// Creates an instance of <see cref="AseBulkCopyRowsCopiedEventArgs"/>.
        /// </summary>
        /// <param name="rowsCopied">The number of rows copied so far.</param>
        public AseBulkCopyRowsCopiedEventArgs(long rowsCopied)
        {
            RowsCopied = rowsCopied;
        }

        /// <summary>
        /// The number of rows copied during the current bulk copy operation.
        /// </summary>
        public long RowsCopied { get; }

        /// <summary>
        /// Set to true to abort the bulk copy operation.
        /// </summary>
        public bool Abort { get; set; }
    }

    /// <summary>
    /// Delegate for the <see cref="AseBulkCopy.AseRowsCopied"/> event.
    /// </summary>
    public delegate void AseBulkCopyRowsCopiedEventHandler(object sender, AseBulkCopyRowsCopiedEventArgs e);
}
