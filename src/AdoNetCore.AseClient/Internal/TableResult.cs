using System.Collections.Generic;

namespace AdoNetCore.AseClient.Internal
{
    internal class TableResult
    {
        public FormatItem[] Formats { get; set; }
        public List<RowResult> Rows { get; } = new List<RowResult>();
    }

    internal class RowResult
    {
        public object[] Items { get; set; }
    }
}
