namespace AdoNetCore.AseClient
{
    /// <summary>
    /// Defines the mapping between a column in an <see cref="AseBulkCopy"/> data source and a column in the destination table.
    /// </summary>
    public sealed class AseBulkCopyColumnMapping
    {
        /// <summary>
        /// Creates a mapping using source and destination column ordinals.
        /// </summary>
        public AseBulkCopyColumnMapping(int sourceColumnOrdinal, int destinationOrdinal)
        {
            SourceOrdinal = sourceColumnOrdinal;
            DestinationOrdinal = destinationOrdinal;
            SourceColumn = null;
            DestinationColumn = null;
        }

        /// <summary>
        /// Creates a mapping using source and destination column names.
        /// </summary>
        public AseBulkCopyColumnMapping(string sourceColumn, string destinationColumn)
        {
            SourceColumn = sourceColumn;
            DestinationColumn = destinationColumn;
            SourceOrdinal = -1;
            DestinationOrdinal = -1;
        }

        /// <summary>
        /// Creates a mapping using a source column name and destination ordinal.
        /// </summary>
        public AseBulkCopyColumnMapping(string sourceColumn, int destinationOrdinal)
        {
            SourceColumn = sourceColumn;
            DestinationOrdinal = destinationOrdinal;
            SourceOrdinal = -1;
            DestinationColumn = null;
        }

        /// <summary>
        /// Creates a mapping using a source ordinal and destination column name.
        /// </summary>
        public AseBulkCopyColumnMapping(int sourceColumnOrdinal, string destinationColumn)
        {
            SourceOrdinal = sourceColumnOrdinal;
            DestinationColumn = destinationColumn;
            SourceColumn = null;
            DestinationOrdinal = -1;
        }

        /// <summary>
        /// The name of the column in the data source.
        /// </summary>
        public string SourceColumn { get; set; }

        /// <summary>
        /// The ordinal of the column in the data source.
        /// </summary>
        public int SourceOrdinal { get; set; }

        /// <summary>
        /// The name of the column in the destination table.
        /// </summary>
        public string DestinationColumn { get; set; }

        /// <summary>
        /// The ordinal of the column in the destination table.
        /// </summary>
        public int DestinationOrdinal { get; set; }
    }
}
