namespace AdoNetCore.AseClient.Benchmark.SAP
{
    /// <summary>
    /// Type for basic benchmark testing.
    /// </summary>
    public sealed class DataItem
    {
        /// <summary>
        /// The ID of the records from the database.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The Name of the records from the database.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The Value of the records from the database.
        /// </summary>
        public int Value { get; set; }
    }
}