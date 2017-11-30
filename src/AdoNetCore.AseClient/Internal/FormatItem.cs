using AdoNetCore.AseClient.Enum;

namespace AdoNetCore.AseClient.Internal
{
    internal class FormatItem
    {
        public string ColumnLabel { get; set; }
        public string CatalogName { get; set; }
        public string SchemaName { get; set; }
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public RowStatus RowStatus { get; set; }
        public int UserType { get; set; }
        public TdsDataType DataType { get; set; }
        public int? Length { get; set; }
        public int? Precision { get; set; }
        public int? Scale { get; set; }
        public string LocaleInfo { get; set; }

        private string _parameterName { get; set; }
        public string ParameterName
        {
            get => _parameterName;
            set => _parameterName = value == null || value.StartsWith("@") ? value : $"@{value}";
        }
        public bool IsNullable { get; set; }
        public bool IsOutput { get; set; }

        /// <summary>
        /// Relates to TDS_BLOB
        /// </summary>
        public string ClassId { get; set; }
    }
}
