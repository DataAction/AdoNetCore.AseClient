using System.Data;

namespace AdoNetCore.AseClient
{
    public sealed class AseDataParameter : IDbDataParameter
    {
        private string _parameterName;
        public DbType DbType { get; set; }
        public ParameterDirection Direction { get; set; }
        public bool IsNullable { get; }

        public string ParameterName
        {
            get => _parameterName;
            set => _parameterName = value == null || value.StartsWith("@") ? value : $"@{value}";
        }

        public string SourceColumn { get; set; }
        public DataRowVersion SourceVersion { get; set; }
        public object Value { get; set; }
        public byte Precision { get; set; }
        public byte Scale { get; set; }
        public int Size { get; set; }

        internal bool CanSendOverTheWire => Direction != ParameterDirection.ReturnValue;
        internal bool IsOutput => Direction == ParameterDirection.InputOutput || Direction == ParameterDirection.Output;
    }
}
