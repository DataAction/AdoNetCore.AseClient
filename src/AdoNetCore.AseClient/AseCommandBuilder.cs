#if SYSTEM_DATA_COMMON_EXTENSIONS
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;

namespace AdoNetCore.AseClient
{
    /// <summary>
    /// The <see cref="AseCommandBuilder"/> can be used to derive INSERT, UPDATE, and DELETE <see cref="AseCommand"/> instances from a single SELECT <see cref="AseCommand"/>.
    /// 
    /// This can reduce the amount of SQL maintained, and make it easier to initialise an <see cref="AseDataAdapter"/> for committing changes in a <see cref="DataTable"/> to the source.
    /// </summary>
    public sealed class AseCommandBuilder : DbCommandBuilder
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string _commandText;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private DataTable _schema;

        /// <summary>
        /// A mapping of ASE type names to the AseDbType enumeration.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly IDictionary<string, AseDbType> AseDbTypeNameMap = new Dictionary<string, AseDbType>(StringComparer.OrdinalIgnoreCase)
        {
            {"bigdatetime", AseDbType.BigDateTime },
            {"bigint", AseDbType.BigInt },
            {"binary", AseDbType.Binary },
            {"bit", AseDbType.Bit },
            {"char", AseDbType.Char },
            {"date", AseDbType.Date },
            {"datetime", AseDbType.DateTime },
            {"decimal", AseDbType.Decimal },
            {"double precision", AseDbType.Double },
            {"float", AseDbType.Double },
            {"image", AseDbType.Image },
            {"int", AseDbType.Integer },
            {"money", AseDbType.Money },
            {"nchar", AseDbType.NChar },
            {"numeric", AseDbType.Numeric },
            {"nvarchar", AseDbType.NVarChar },
            {"real", AseDbType.Real },
            {"smalldatetime", AseDbType.SmallDateTime },
            {"smallint", AseDbType.SmallInt },
            {"smallmoney", AseDbType.SmallMoney },
            {"longvarchar", AseDbType.Text },
            {"text", AseDbType.Text },
            {"time", AseDbType.Time },
            {"timestamp", AseDbType.TimeStamp },
            {"tinyint", AseDbType.TinyInt },
            {"unichar", AseDbType.UniChar },
            {"uniqueidentifier", AseDbType.VarBinary },
            {"unitext", AseDbType.Unitext },
            {"univarchar", AseDbType.UniVarChar },
            {"unsigned bigint", AseDbType.UnsignedBigInt},
            {"unsigned int", AseDbType.UnsignedInt },
            {"unsigned smallint", AseDbType.UnsignedSmallInt },
            {"varbinary", AseDbType.VarBinary },
            {"varchar", AseDbType.VarChar },
            {"wchar", AseDbType.UniChar },
            {"wvarchar", AseDbType.UniVarChar },
        };

        /// <summary>
        /// A mapping of ASE type codes to the AseDbType enumeration.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly IDictionary<short, AseDbType> AseDbTypeCodeMap = new Dictionary<short, AseDbType>
        {
            {-2, AseDbType.Binary },
            {-7, AseDbType.Bit },
            {1, AseDbType.Char },
            {91, AseDbType.Date },
            {93, AseDbType.DateTime },
            {3, AseDbType.Decimal },
            {8, AseDbType.Double },
            {7, AseDbType.Real },
            {4, AseDbType.Integer },
            {-4, AseDbType.Image },
            {-200, AseDbType.Money },
            {2, AseDbType.Numeric },
            {-204, AseDbType.NChar },
            {-205, AseDbType.NVarChar },
            {-202, AseDbType.SmallDateTime },
            {5, AseDbType.SmallInt },
            {-201, AseDbType.SmallMoney },
            {-1, AseDbType.Text },
            {92, AseDbType.Time },
            {-203, AseDbType.TimeStamp },
            {-6, AseDbType.TinyInt },
            {-3, AseDbType.VarBinary },
            {12, AseDbType.VarChar },
            {-8, AseDbType.UniChar },
            {-9, AseDbType.UniVarChar },
            {-10, AseDbType.Unitext },
            {-206, AseDbType.UnsignedSmallInt },
            {-207, AseDbType.UnsignedInt },
            {-5, AseDbType.BigInt },
            {-208, AseDbType.UnsignedBigInt},
            {2 | 4, AseDbType.Double} // Integer | Numeric
        };

        /// <summary>
        /// Constructor function for an <see cref="AseCommandBuilder"/> instance.
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public AseCommandBuilder()
        {
            base.QuotePrefix = "[";
            base.QuoteSuffix = "]";
        }

        /// <summary>
        /// Constructor function for an <see cref="AseCommandBuilder"/> instance.
        /// </summary>
        /// <param name="adapter">The <see cref="AseDataAdapter"/> to generate T-SQL statements for.</param>
        public AseCommandBuilder(AseDataAdapter adapter) : this()
        {
            DataAdapter = adapter;
        }

        /// <summary>
        /// Gets the <see cref="AseDataAdapter"/> associated with this <see cref="AseCommandBuilder"/>.
        /// </summary>
        public new AseDataAdapter DataAdapter
        {
            get => (AseDataAdapter) base.DataAdapter;
            set
            {
                base.DataAdapter = value;
                value.CommandBuilder = this;
            }
        }

        /// <summary>
        /// The prefix to use when quoting identifiers.
        /// </summary>
        public override string QuotePrefix
        {
            get => base.QuotePrefix;
            set
            {
                switch (value)
                {
                    case "[":
                    case @"""":
                    case "'":
                        base.QuotePrefix = value;
                        break;
                    default:
                        throw new AseException("QuotePrefix must be one of: [, \", '");
                }
            }
        }

        /// <summary>
        /// The suffix to use when quoting identifiers.
        /// </summary>
        public override string QuoteSuffix
        {
            get => base.QuoteSuffix;
            set
            {
                switch (value)
                {
                    case "]":
                    case @"""":
                    case "'":
                        base.QuoteSuffix = value;
                        break;
                    default:
                        throw new AseException("QuoteSuffix must be one of: ], \", '");
                }
            }
        }

        /// <summary>
        /// The INSERT command generated from the given SELECT <see cref="DataAdapter"/>.
        /// </summary>
        /// <returns>An <see cref="AseCommand"/> for performing INSERTs.</returns>
        public new AseCommand GetInsertCommand()
        {
            return (AseCommand) base.GetInsertCommand();
        }

        /// <summary>
        /// The DELETE command generated from the given SELECT <see cref="DataAdapter"/>.
        /// </summary>
        /// <returns>An <see cref="AseCommand"/> for performing DELETEs.</returns>
        public new AseCommand GetDeleteCommand()
        {
            return (AseCommand) base.GetDeleteCommand();
        }

        /// <summary>
        /// The UPDATE command generated from the given SELECT <see cref="DataAdapter"/>.
        /// </summary>
        /// <returns>An <see cref="AseCommand"/> for performing UPDATEs.</returns>
        public new AseCommand GetUpdateCommand()
        {
            return (AseCommand) base.GetUpdateCommand();
        }

        protected override DataTable GetSchemaTable(DbCommand sourceCommand)
        {
            if (_commandText != sourceCommand.CommandText || _schema == null)
            {
                _commandText = sourceCommand.CommandText;
                _schema = GetSchemaTableWithKeyInfo(sourceCommand);
            }
            return _schema;
        }


        private static DataTable GetSchemaTableWithKeyInfo(DbCommand sourceCommand)
        {
            using (var aseCommand = (AseCommand) ((ICloneable) sourceCommand).Clone())
            {
                using (var schemaReader = aseCommand.ExecuteReader(CommandBehavior.SchemaOnly | CommandBehavior.KeyInfo))
                {
                    var dataTable = schemaReader.GetSchemaTable();

                    if (dataTable != null)
                    {
                        // If there is no primary key on the table, then throw MissingPrimaryKeyException.
                        var isKeyColumn = dataTable.Columns["IsKey"];
                        var hasKey = false;

                        foreach (DataRow columnDescriptorRow in dataTable.Rows)
                        {
                            hasKey |= (bool)columnDescriptorRow[isKeyColumn];

                            if (hasKey)
                            {
                                break;
                            }
                        }

                        if (!hasKey)
                        {
                            throw new MissingPrimaryKeyException("Cannot generate SQL statements if there is no unique column in the source command.");
                        }
                    }

                    return dataTable;
                }
            }
        }

        /// <summary>
        /// Retrieves parameter information from the stored procedure specified in the <see cref="AseCommand"/> and populates the 
        /// <see cref="AseCommand.Parameters"/> collection of the specified <see cref="AseCommand"/> object.
        /// </summary>
        /// <param name="command">The <see cref="AseCommand"/> referencing the stored procedure from which the parameter information 
        /// is to be derived. The derived parameters are added to the <see cref="AseCommand.Parameters"/> collection of the <see cref="AseCommand"/>.</param>
        /// <exception cref="InvalidOperationException">The command text is not a valid stored procedure name.</exception>
        public static void DeriveParameters(AseCommand command)
        {
            if (command.CommandType != CommandType.StoredProcedure)
            {
                throw new InvalidOperationException("Invalid AseCommand.CommandType. Only CommandType.StoredProcedure is supported");
            }
            if (command.Connection == null)
            {
                throw new InvalidOperationException("Invalid AseCommand.Connection");
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

            string procedureName;
            string parameterMetadataCommandText;
            string owner;

            var commandParts = command.CommandText.Split('.');
            switch (commandParts.Length)
            {
                // procedure_name
                case 1:
                    procedureName = commandParts[0].Trim();
                    owner = null;
                    parameterMetadataCommandText = "sp_sproc_columns";
                    break;
                // owner.procedure_name
                case 2:
                    owner = commandParts[0].Trim();
                    procedureName = commandParts[1].Trim();
                    parameterMetadataCommandText = "sp_sproc_columns";
                    break;
                // database.owner.procedure_name
                case 3:
                    var database = commandParts[0].Trim();

                    owner = commandParts[1].Trim();
                    procedureName = commandParts[2].Trim();
                    parameterMetadataCommandText = database.Length > 0 ? $"{database}..sp_sproc_columns" : "sp_sproc_columns";
                    break;
                default:
                    throw new InvalidOperationException("The command text is not a valid stored procedure name of the form [database].[owner].procedure_name.");
            }

            using (var parameterMetadataCommand = new AseCommand(command.Connection) {CommandText = parameterMetadataCommandText, CommandType = CommandType.StoredProcedure })
            {
                parameterMetadataCommand.Parameters.Add("@procedure_name", AseDbType.VarChar).Value = procedureName;

                if (owner?.Length > 0)
                {
                    parameterMetadataCommand.Parameters.Add("@procedure_owner", AseDbType.VarChar).Value = owner;
                }

                using (var reader = parameterMetadataCommand.ExecuteReader())
                {
                    var isFirstParameter = true;

                    var procedureNameOrdinal = reader.GetOrdinal("column_name");
                    var typeNameOrdinal = reader.GetOrdinal("type_name");
                    var sqlDataTypeOrdinal = reader.GetOrdinal("sql_data_type");
                    var nullableOrdinal = reader.GetOrdinal("nullable");
                    var precisionOrdinal = reader.GetOrdinal("precision");
                    var lengthOrdinal = reader.GetOrdinal("length");
                    var scaleOrdinal = reader.GetOrdinal("scale");

                    while (reader.Read())
                    {
                        var parameterName = reader.GetString(procedureNameOrdinal);

                        if (!parameterName.StartsWith("@"))
                        {
                            parameterName = "@" + parameterName;
                        }

                        var dbType = default(AseDbType);

                        if (!reader.IsDBNull(typeNameOrdinal) && !AseDbTypeNameMap.TryGetValue(reader.GetString(typeNameOrdinal), out dbType))
                        {
                            dbType = AseDbTypeCodeMap[reader.GetInt16(sqlDataTypeOrdinal)];
                        }

                        var isNullable = false;
                        if (!reader.IsDBNull(nullableOrdinal))
                        {
                            isNullable = reader.GetInt16(nullableOrdinal) != 0;
                        }

                        byte precision = 0;
                        byte scale = 0;

                        if (HasPrecisionAndScale(dbType))
                        {
                            if (!reader.IsDBNull(precisionOrdinal))
                            {
                                precision = reader.GetByte(precisionOrdinal);
                            }

                            if (!reader.IsDBNull(scaleOrdinal))
                            {
                                try
                                {
                                    scale = reader.GetByte(scaleOrdinal);
                                }
                                catch (FormatException)
                                {
                                    // Swallow the FormatException if it's because the value is "NULL"
                                    if (!string.Equals(reader.GetString(scaleOrdinal).Trim(), "NULL", StringComparison.OrdinalIgnoreCase))
                                    {
                                        throw;
                                    }
                                }
                            }
                        }

                        var size = 0;
                        if (!reader.IsDBNull(lengthOrdinal))
                        {
                            size = reader.GetInt16(lengthOrdinal);
                        }
                            

                        var parameter = new AseParameter(parameterName, dbType, size) { IsNullable = isNullable, Precision = precision, Scale = scale };

                        if (isFirstParameter)
                        {
                            parameter.Direction = ParameterDirection.ReturnValue;
                            isFirstParameter = false;
                        }
                        else
                        {
                            try
                            {
                                // Get the mode.
                                if (reader.FieldCount > 21 && !reader.IsDBNull(21))
                                {
                                    if (string.Equals(reader.GetString(21), "out", StringComparison.OrdinalIgnoreCase))
                                    {
                                        parameter.Direction = ParameterDirection.Output;
                                    }
                                }
                            }
                            catch (IndexOutOfRangeException) // TODO - this shouldn't happen with the FieldCount check above.
                            {
                            }
                        }
                        command.Parameters.Add(parameter);
                    }
                }
            }
        }

        private static bool HasPrecisionAndScale(AseDbType aseDbType)
        {
            switch (aseDbType)
            {
                case AseDbType.Double:
                case AseDbType.Decimal:
                case AseDbType.Money:
                case AseDbType.Numeric:
                    return true;
                default:
                    return false;
            }
        }

        protected override void ApplyParameterInfo(DbParameter parameter, DataRow row, StatementType statementType, bool whereClause)
        {
            var aseParameter = (AseParameter) parameter;
            aseParameter.AseDbType = (AseDbType) row[SchemaTableColumn.ProviderType];
            aseParameter.Precision = (byte) (int) row[SchemaTableColumn.NumericPrecision];
            aseParameter.Scale = (byte) (int) row[SchemaTableColumn.NumericScale];
        }

        protected override string GetParameterName(string parameterName)
        {
            if (parameterName.Length > 0 && (parameterName[0] == '?' || parameterName[0] == '@'))
            {
                return parameterName;
            }

            return "@" + parameterName;
        }

        protected override string GetParameterName(int parameterOrdinal)
        {
            return _GetParameterName(parameterOrdinal);
        }

        protected override string GetParameterPlaceholder(int parameterOrdinal)
        {
            return _GetParameterName(parameterOrdinal);
        }

        private static string _GetParameterName(int parameterOrdinal)
        {
            if (parameterOrdinal <= 0)
            {
                throw new AseException(new AseError {IsError = true, IsFromClient = true, MessageNumber = 30070});
            }

            return "@p" + parameterOrdinal;
        }

        protected override void SetRowUpdatingHandler(DbDataAdapter adapter)
        {
            if (adapter != base.DataAdapter)
            {
                ((AseDataAdapter) adapter).RowUpdating += RowUpdating;
            }
            else
            {
                ((AseDataAdapter) adapter).RowUpdating -= RowUpdating;
            }
        }

        private void RowUpdating(object sender, AseRowUpdatingEventArgs args)
        {
            RowUpdatingHandler(args);
        }
    }
}
#endif
