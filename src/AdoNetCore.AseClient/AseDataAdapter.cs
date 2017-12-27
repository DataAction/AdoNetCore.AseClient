#if NETCOREAPP2_0 || NET45 || NET46
using System;
using System.Collections;
using System.Data;
using System.Data.Common;

namespace AdoNetCore.AseClient
{
    public sealed class AseDataAdapter : DbDataAdapter, IDbDataAdapter, IDataAdapter, IDisposable
    {
        private Hashtable _cmdList = new Hashtable();
        internal readonly AseDataAdapter2 oldAdapter;

        public AseDataAdapter()
        {
            oldAdapter = new AseDataAdapter2(this);
            oldAdapter.RowUpdated += oldAdapter_RowUpdated;
            oldAdapter.RowUpdating += oldAdapter_RowUpdating;
        }

        public AseDataAdapter(AseCommand selectCommand)
        {
            oldAdapter = new AseDataAdapter2(this, selectCommand);
            oldAdapter.RowUpdated += oldAdapter_RowUpdated;
            oldAdapter.RowUpdating += oldAdapter_RowUpdating;
        }

        public AseDataAdapter(string selectCommandText, AseConnection selectConnection)
        {
            oldAdapter = new AseDataAdapter2(this, selectCommandText, selectConnection);
            oldAdapter.RowUpdated += oldAdapter_RowUpdated;
            oldAdapter.RowUpdating += oldAdapter_RowUpdating;
        }

        public AseDataAdapter(string selectCommandText, string selectConnectionString)
        {
            oldAdapter = new AseDataAdapter2(this, selectCommandText, selectConnectionString);
            oldAdapter.RowUpdated += oldAdapter_RowUpdated;
            oldAdapter.RowUpdating += oldAdapter_RowUpdating;
        }

        private void oldAdapter_RowUpdated(object obj, AseRowUpdatedEventArgs args)
        {
            OnRowUpdated(args);
        }

        private void oldAdapter_RowUpdating(object obj, AseRowUpdatingEventArgs args)
        {
            OnRowUpdating(args);
        }

        public new AseCommand DeleteCommand
        {
            get => oldAdapter.DeleteCommand;
            set => oldAdapter.DeleteCommand = value;
        }

        IDbCommand IDbDataAdapter.DeleteCommand
        {
            get => DeleteCommand;
            set => DeleteCommand = (AseCommand) value;
        }

        public new AseCommand InsertCommand
        {
            get => oldAdapter.InsertCommand;
            set => oldAdapter.InsertCommand = value;
        }

        IDbCommand IDbDataAdapter.InsertCommand
        {
            get => InsertCommand;
            set => InsertCommand = (AseCommand) value;
        }

        public new AseCommand SelectCommand
        {
            get => oldAdapter.SelectCommand;
            set => oldAdapter.SelectCommand = value;
        }

        IDbCommand IDbDataAdapter.SelectCommand
        {
            get => SelectCommand;
            set => SelectCommand = (AseCommand) value;
        }

        public new AseCommand UpdateCommand
        {
            get => oldAdapter.UpdateCommand;
            set => oldAdapter.UpdateCommand = value;
        }

        IDbCommand IDbDataAdapter.UpdateCommand
        {
            get => UpdateCommand;
            set => UpdateCommand = (AseCommand) value;
        }

        public AseCommandBuilder CommandBuilder
        {
            get => oldAdapter.CommandBuilder;
            set => oldAdapter.CommandBuilder = value;
        }

        public override int UpdateBatchSize { get; set; } = 1;

        protected override int AddToBatch(IDbCommand command)
        {
            int count = _cmdList.Count;

            var aseCommand = (AseCommand) ((ICloneable) command).Clone();

            _cmdList.Add(count, aseCommand);

            return count;
        }

        protected override void ClearBatch()
        {
            _cmdList.Clear();
        }

        protected override int ExecuteBatch()
        {
            var values = _cmdList.Values;
            var num = 0;

            foreach (DbCommand dbCommand in values)
            {
                dbCommand.ExecuteNonQuery();
                ++num;
            }

            return num;
        }

        protected override IDataParameter GetBatchedParameter(int commandIdentifier, int parameterIndex)
        {
            return ((AseCommand) _cmdList[commandIdentifier]).Parameters[parameterIndex];
        }

        protected override void InitializeBatching()
        {
        }

        protected override void TerminateBatching()
        {
        }

        protected override RowUpdatedEventArgs CreateRowUpdatedEvent(DataRow dataRow, IDbCommand command,
            StatementType statementType, DataTableMapping tableMapping)
        {
            return new AseRowUpdatedEventArgs(dataRow, command, statementType, tableMapping);
        }

        protected override RowUpdatingEventArgs CreateRowUpdatingEvent(DataRow dataRow, IDbCommand command,
            StatementType statementType, DataTableMapping tableMapping)
        {
            return new AseRowUpdatingEventArgs(dataRow, command, statementType, tableMapping);
        }

        protected override void OnRowUpdated(RowUpdatedEventArgs value)
        {
            if (RowUpdated == null)
            {
                return;
            }

            if (!(value is AseRowUpdatedEventArgs e))
            {
                return;
            }

            RowUpdated(this, e);
        }

        protected override void OnRowUpdating(RowUpdatingEventArgs value)
        {
            if (RowUpdating == null)
            {
                return;
            }

            if (!(value is AseRowUpdatingEventArgs e))
            {
                return;
            }

            RowUpdating(this, e);
        }

        public event AseRowUpdatedEventHandler RowUpdated;

        public event AseRowUpdatingEventHandler RowUpdating;
    }

    internal sealed class AseDataAdapter2
    {
        private readonly AseDataAdapter _thisAdapter;
        private bool _disposed;
        private AseCommand _deleteCmd;
        private AseCommand _insertCmd;
        private AseCommand _selectCmd;
        private AseCommand _updateCmd;
        private AseCommandBuilder _builder;

        internal AseDataAdapter2(AseDataAdapter realAdapter)
        {
            _thisAdapter = realAdapter;
        }

        internal AseDataAdapter2(AseDataAdapter realAdapter, AseCommand selectCommand)
        {
            SelectCommand = selectCommand;
            _thisAdapter = realAdapter;
        }

        internal AseDataAdapter2(AseDataAdapter realAdapter, string selectCommandText, AseConnection selectConnection)
        {
            SelectCommand = new AseCommand(selectConnection) {CommandText = selectCommandText};
            _thisAdapter = realAdapter;
        }

        internal AseDataAdapter2(AseDataAdapter realAdapter, string selectCommandText, string selectConnectionString)
        {
            SelectCommand = new AseCommand(new AseConnection(selectConnectionString)) {CommandText = selectCommandText};
            _thisAdapter = realAdapter;
        }

        ~AseDataAdapter2()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }
            if (disposing)
            {
                _deleteCmd?.Dispose();
                _insertCmd?.Dispose();
                _selectCmd?.Dispose();
                _updateCmd?.Dispose();
            }
            _disposed = true;
        }

        internal AseCommand DeleteCommand
        {
            get
            {
                if (_deleteCmd != null)
                    return _deleteCmd;
                if (_builder != null)
                    return _builder.GetDeleteCommand();
                return null;
            }
            set
            {
                _deleteCmd = value;
                if (_deleteCmd == null || _deleteCmd.Connection != null || _selectCmd == null)
                    return;
                _deleteCmd.Connection = _selectCmd.Connection;
            }
        }

        internal AseCommand InsertCommand
        {
            get
            {
                if (_insertCmd != null)
                {
                    return _insertCmd;
                }

                if (_builder != null)
                {
                    return _builder.GetInsertCommand();
                }

                return null;
            }
            set
            {
                _insertCmd = value;
                if (_insertCmd == null || _insertCmd.Connection != null || _selectCmd == null)
                {
                    return;
                }

                _insertCmd.Connection = _selectCmd.Connection;
            }
        }

        internal AseCommand SelectCommand
        {
            get => _selectCmd;
            set => _selectCmd = value;
        }

        internal AseCommand UpdateCommand
        {
            get
            {
                AseCommand aseCommand = null;
                try
                {
                    if (_updateCmd != null)
                    {
                        aseCommand = _updateCmd;
                    }

                    if (_builder != null)
                    {
                        aseCommand = _builder.GetUpdateCommand();
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                return aseCommand;
            }
            set
            {
                _updateCmd = value;
                if (_updateCmd == null || _updateCmd.Connection != null || _selectCmd == null)
                {
                    return;
                }

                _updateCmd.Connection = _selectCmd.Connection;
            }
        }

        internal AseCommandBuilder CommandBuilder
        {
            get => _builder;
            set
            {
                _builder = value;
                if (_builder == null || _builder.DataAdapter == _thisAdapter)
                {
                    return;
                }

                _builder.DataAdapter = _thisAdapter;
            }
        }

        internal RowUpdatedEventArgs CreateRowUpdatedEvent(DataRow dataRow, IDbCommand command,
            StatementType statementType, DataTableMapping tableMapping)
        {
            return new AseRowUpdatedEventArgs(dataRow, command, statementType, tableMapping);
        }

        internal RowUpdatingEventArgs CreateRowUpdatingEvent(DataRow dataRow, IDbCommand command,
            StatementType statementType, DataTableMapping tableMapping)
        {
            return new AseRowUpdatingEventArgs(dataRow, command, statementType, tableMapping);
        }

        internal void OnRowUpdated(RowUpdatedEventArgs value)
        {
            if (RowUpdated == null)
            {
                return;
            }

            if (!(value is AseRowUpdatedEventArgs e))
            {
                return;
            }

            RowUpdated(_thisAdapter, e);
        }

        internal void OnRowUpdating(RowUpdatingEventArgs value)
        {
            if (RowUpdating == null)
            {
                return;
            }

            if (!(value is AseRowUpdatingEventArgs e))
            {
                return;
            }

            RowUpdating(_thisAdapter, e);
        }

        internal event AseRowUpdatedEventHandler RowUpdated;

        internal event AseRowUpdatingEventHandler RowUpdating;
    }

    public sealed class AseCommandBuilder : DbCommandBuilder
    {
        private string _commandText;
        private DataTable _schema;

        public AseCommandBuilder()
        {
            base.QuotePrefix = "[";
            base.QuoteSuffix = "]";
        }

        public AseCommandBuilder(AseDataAdapter adapter)
        {
            base.QuotePrefix = "[";
            base.QuoteSuffix = "]";
            DataAdapter = adapter;
        }

        public new AseDataAdapter DataAdapter
        {
            get { return (AseDataAdapter) base.DataAdapter; }
            set
            {
                base.DataAdapter = value;
                value.CommandBuilder = this;
            }
        }

        public override string QuotePrefix
        {
            get { return base.QuotePrefix; }
            set
            {
                if ("[" != value && "\"" != value && "'" != value)
                    throw new AseException("QuotePrefix must be one of: [, \", '");
                base.QuotePrefix = value;
            }
        }

        public override string QuoteSuffix
        {
            get { return base.QuoteSuffix; }
            set
            {
                if ("]" != value && "\"" != value && "'" != value)
                    throw new AseException("QuoteSuffix must be one of: ], \", '");
                base.QuoteSuffix = value;
            }
        }

        public AseCommand GetInsertCommand()
        {
            return (AseCommand) base.GetInsertCommand();
        }

        public AseCommand GetDeleteCommand()
        {
            return (AseCommand) base.GetDeleteCommand();
        }

        public AseCommand GetUpdateCommand()
        {
            return (AseCommand) base.GetUpdateCommand();
        }

        protected override DataTable GetSchemaTable(DbCommand sourceCommand)
        {
            if (_commandText != sourceCommand.CommandText || _schema == null)
            {
                _commandText = sourceCommand.CommandText;
                _schema = FillSchemaTable(sourceCommand);
            }
            return _schema;
        }

        private DataTable FillSchemaTable(DbCommand sourceCommand)
        {
            DataTable dataTable;
            using (var aseCommand = (AseCommand) ((ICloneable) sourceCommand).Clone())
            {
                using (var aseDataReader =
                    aseCommand.ExecuteReader(CommandBehavior.SchemaOnly | CommandBehavior.KeyInfo))
                {
                    dataTable = aseDataReader.GetSchemaTable();
                    if (aseDataReader._missingKey != null)
                        throw new MissingPrimaryKeyException(aseDataReader._missingKey);
                }
            }
            return dataTable;
        }

        public static void DeriveParameters(AseCommand command)
        {
            if (command.CommandType != CommandType.StoredProcedure)
            {
                throw new InvalidOperationException(
                    "Invalid AseCommand.CommandType. Only CommandType.StoredProcedure is supported");
            }
            if (command.Connection == null)
            {
                throw new InvalidOperationException("Invalid AseCommnad.Connection");
            }
            if (command.CommandText.Length == 0)
            {
                throw new InvalidOperationException("Invalid AseCommand.CommandText");
            }
            if (command.Connection.State != ConnectionState.Open)
            {
                throw new InvalidOperationException("Invalid AseCommand.Connection.ConnectionState");
            }

            command.Parameters.Clear();

            var cmdText = "sp_sproc_columns";
            var commandText = command.CommandText;
            var str2 = (string) null;
            if (commandText.IndexOf('.') >= 0)
            {
                char[] chArray = new char[1] {'.'};
                string[] strArray = commandText.Split(chArray);
                switch (strArray.Length)
                {
                    case 2:
                        str2 = strArray[0].Trim();
                        commandText = strArray[1].Trim();
                        break;
                    case 3:
                        string str3 = strArray[0].Trim();
                        str2 = strArray[1].Trim();
                        commandText = strArray[2].Trim();
                        if (str3.Length > 0)
                        {
                            cmdText = str3 + ".." + cmdText;
                            break;
                        }
                        break;
                    case 4:
                        string str4 = strArray[0].Trim();
                        string str5 = strArray[1].Trim();
                        str2 = strArray[2].Trim();
                        commandText = strArray[3].Trim();
                        if (str4.Length > 0)
                        {
                            cmdText = str4 + "." + str5 + ".." + cmdText;
                            break;
                        }
                        if (str5.Length > 0)
                        {
                            cmdText = str5 + ".." + cmdText;
                            break;
                        }
                        break;
                    default:
                        throw new InvalidOperationException("Invalid AseCommand.CommandText");
                }
            }
            using (var aseCommand = new AseCommand(command.Connection) {CommandText = cmdText})
            {
                aseCommand.CommandType = CommandType.StoredProcedure;
                aseCommand.Parameters.Add("@procedure_name", AseDbType.VarChar).Value = (object) commandText;
                if (str2 != null)
                {
                    if (str2.Length > 0)
                        aseCommand.Parameters.Add("@procedure_owner", AseDbType.VarChar).Value = (object) str2;
                }
                try
                {
                    AseDataReader aseDataReader = aseCommand.ExecuteReader();
                    bool flag = true;
                    while (aseDataReader.Read())
                    {
                        string parameterName = aseDataReader.GetString(3);
                        if (!parameterName.StartsWith("@"))
                            parameterName = "@" + parameterName;
                        bool isNullable = false;
                        if (!aseDataReader.IsDBNull(11))
                            isNullable = (int) aseDataReader.GetInt16(11) != 0;
                        int precision = 0;
                        if (!aseDataReader.IsDBNull(7))
                            precision = (int) aseDataReader.GetInt16(7);
                        int size = 0;
                        if (!aseDataReader.IsDBNull(8))
                            size = (int) aseDataReader.GetInt16(8);
                        int scale = 0;
                        if (!aseDataReader.IsDBNull(9))
                        {
                            try
                            {
                                scale = (int) aseDataReader.GetInt16(9);
                            }
                            catch (FormatException ex)
                            {
                                if (aseDataReader.GetString(9).ToUpper().Trim() != "NULL")
                                    throw ex;
                            }
                        }
                        AseDbType dbType = AseDbType.Unsupported;
                        if (!aseDataReader.IsDBNull(6))
                            dbType = StringToAseDbTypeMap.GetAseDbTypeFromString(aseDataReader.GetString(6));
                        if (dbType == AseDbType.Unsupported)
                            dbType = (AseDbType) aseDataReader.GetInt16(16);
                        if (dbType == (AseDbType.Integer | AseDbType.Numeric))
                            dbType = AseDbType.Double;
                        var parameter = new AseParameter(parameterName, dbType, size, isNullable, precision, scale);
                        if (flag)
                        {
                            parameter.Direction = ParameterDirection.ReturnValue;
                            flag = false;
                        }
                        else
                        {
                            try
                            {
                                if (!aseDataReader.IsDBNull(21))
                                {
                                    if (aseDataReader.GetString(21).CompareTo("out") == 0)
                                        parameter.Direction = ParameterDirection.Output;
                                }
                            }
                            catch (IndexOutOfRangeException)
                            {
                            }
                        }
                        command.Parameters.Add(parameter);
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        protected override void ApplyParameterInfo(DbParameter parameter, DataRow row, StatementType statementType,
            bool whereClause)
        {
            AseParameter aseParameter = (AseParameter) parameter;
            aseParameter.AseDbType = (AseDbType) row[SchemaTableColumn.ProviderType];
            aseParameter.Precision = (byte) (int) row[SchemaTableColumn.NumericPrecision];
            aseParameter.Scale = (byte) (int) row[SchemaTableColumn.NumericScale];
        }

        protected override string GetParameterName(string parameterName)
        {
            if (parameterName.Length > 0 && ((int) parameterName[0] == 63 || (int) parameterName[0] == 64))
                return parameterName;
            return "@" + parameterName;
        }

        protected override string GetParameterName(int parameterOrdinal)
        {
            if (parameterOrdinal <= 0)
            {
                throw new AseException(DriverMsgNumber.ERR_BAD_PARAM_INDEX, (AseConnection) null);
            }
            return "@p" + parameterOrdinal;
        }

        protected override string GetParameterPlaceholder(int parameterOrdinal)
        {
            if (parameterOrdinal <= 0)
            {
                throw new AseException(DriverMsgNumber.ERR_BAD_PARAM_INDEX, (AseConnection)null);
            }

            return "@p" + parameterOrdinal;
        }

        protected override void SetRowUpdatingHandler(DbDataAdapter adapter)
        {
            if (adapter != base.DataAdapter)
                ((AseDataAdapter) adapter).RowUpdating += RowUpdating;
            else
                ((AseDataAdapter) adapter).RowUpdating -= RowUpdating;
        }

        private void RowUpdating(object sender, AseRowUpdatingEventArgs args)
        {
            RowUpdatingHandler((RowUpdatingEventArgs) args);
        }
    }
}
#endif