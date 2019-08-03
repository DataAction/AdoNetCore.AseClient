using System.Collections.Generic;

namespace AdoNetCore.AseClient.Internal
{
    internal sealed class TableResult
    {
        public FormatItem[] Formats { get; set; }
        public List<RowResult> Rows { get; } = new List<RowResult>();
    }
}
