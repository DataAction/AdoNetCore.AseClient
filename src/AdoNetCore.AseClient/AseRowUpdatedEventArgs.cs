using System.Data;
using System.Data.Common;

namespace AdoNetCore.AseClient
{
    /// <summary>
    /// Event Args class for use with the <see cref="AseRowUpdatedEventHandler"/> delegate.
    /// </summary>
    public sealed class AseRowUpdatedEventArgs : RowUpdatedEventArgs
    {
        public AseRowUpdatedEventArgs(DataRow row, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
            : base(row, command, statementType, tableMapping)
        {
        }

        public new AseCommand Command => (AseCommand)base.Command;
    }
}
