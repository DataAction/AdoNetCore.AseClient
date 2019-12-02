using System.Collections.Generic;

namespace AdoNetCore.AseClient.Internal
{
    internal sealed class TableResult
    {
        public FormatItem[] Formats { get; set; }
        public List<RowResult> Rows { get; } = new List<RowResult>();

        public List<MessageResult> Messages { get; } = new List<MessageResult>();

        public int RecordsAffected { get; set; } = 0;
    }
}
