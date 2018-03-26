#if ENABLE_SYSTEM_DATA_COMMON_EXTENSIONS
using System.Data;
using System.Data.Common;

namespace AdoNetCore.AseClient
{
    /// <summary>
    /// Event Args class for use with the <see cref="AseRowUpdatingEventHandler"/> delegate.
    /// </summary>
    public sealed class AseRowUpdatingEventArgs : RowUpdatingEventArgs
    {
        public AseRowUpdatingEventArgs(DataRow row, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
            : base(row, command, statementType, tableMapping)
        {
        }

        public new AseCommand Command
        {
            get => (AseCommand)base.Command;
            set => BaseCommand = value;
        }
    }
}
#endif