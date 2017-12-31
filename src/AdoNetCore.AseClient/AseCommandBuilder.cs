#if !NETCORE_OLD
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using AdoNetCore.AseClient.Enum;

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
            {"binary", AseDbType.Binary },
            {"bit", AseDbType.Bit },
            {"char", AseDbType.Char },
            {"date", AseDbType.Date },
            {"datetime", AseDbType.DateTime },
            {"decimal", AseDbType.Decimal },
            {"float", AseDbType.Double },
            {"real", AseDbType.Real },
            {"int", AseDbType.Integer },
            {"image", AseDbType.Image },
            {"money", AseDbType.Money },
            {"numeric", AseDbType.Numeric },
            {"nchar", AseDbType.NChar },
            {"nvarchar", AseDbType.NVarChar },
            {"smalldatetime", AseDbType.SmallDateTime },
            {"bigdatetime", AseDbType.DateTime },
            {"smallint", AseDbType.SmallInt },
            {"smallmoney", AseDbType.SmallMoney },
            {"longvarchar", AseDbType.Text },
            {"text", AseDbType.Text },
            {"time", AseDbType.Time },
            {"timestamp", AseDbType.TimeStamp },
            {"tinyint", AseDbType.TinyInt },
            {"varbinary", AseDbType.VarBinary },
            {"varchar", AseDbType.VarChar },
            {"wchar", AseDbType.UniChar },
            {"unichar", AseDbType.UniChar },
            {"wvarchar", AseDbType.UniVarChar },
            {"univarchar", AseDbType.UniVarChar },
            {"unitext", AseDbType.Unitext },
            {"unsigned smallint", AseDbType.UnsignedSmallInt },
            {"unsigned int", AseDbType.UnsignedInt },
            {"bigint", AseDbType.BigInt },
            {"unsigned bigint", AseDbType.UnsignedBigInt}
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
            aseParameter.AseDbType = GetAseDbTypeFromTdsName(row["DataTypeName"].ToString());
            //aseParameter.AseDbType = (AseDbType) row[SchemaTableColumn.ProviderType];
            aseParameter.Precision = (byte) (int) row[SchemaTableColumn.NumericPrecision];
            aseParameter.Scale = (byte) (int) row[SchemaTableColumn.NumericScale];
        }

        private static AseDbType GetAseDbTypeFromTdsName(string tdsName)
        {
            var tdsDataType = System.Enum.Parse(typeof(TdsDataType), tdsName, true);

            switch (tdsDataType)
            {
                case TdsDataType.TDS_BINARY:
                    return AseDbType.Binary;
                case TdsDataType.TDS_BIT:
                    return AseDbType.Bit;
                case TdsDataType.TDS_BLOB:
                    return AseDbType.Image;
                case TdsDataType.TDS_CHAR:
                    return AseDbType.Char;
                case TdsDataType.TDS_DATE:
                case TdsDataType.TDS_DATEN:
                    return AseDbType.Date;
                case TdsDataType.TDS_DATETIME:
                case TdsDataType.TDS_DATETIMEN:
                    return AseDbType.DateTime;
                case TdsDataType.TDS_DECN:
                    return AseDbType.Decimal;
                case TdsDataType.TDS_FLTN:
                case TdsDataType.TDS_FLT4:
                    return AseDbType.Real;
                case TdsDataType.TDS_FLT8:
                    return AseDbType.Double;
                case TdsDataType.TDS_IMAGE:
                    return AseDbType.Image;
                case TdsDataType.TDS_SINT1:
                case TdsDataType.TDS_INT1:
                    return AseDbType.TinyInt;
                case TdsDataType.TDS_INT2:
                    return AseDbType.SmallInt;
                case TdsDataType.TDS_INTN:
                case TdsDataType.TDS_INT4:
                    return AseDbType.Integer;
                case TdsDataType.TDS_INT8:
                    return AseDbType.BigInt;
                case TdsDataType.TDS_LONGBINARY:
                    return AseDbType.Binary;
                case TdsDataType.TDS_LONGCHAR:
                    return AseDbType.LongVarChar;
                case TdsDataType.TDS_MONEY:
                case TdsDataType.TDS_MONEYN:
                    return AseDbType.Money;
                case TdsDataType.TDS_SHORTDATE:
                    return AseDbType.SmallDateTime;
                case TdsDataType.TDS_SHORTMONEY:
                    return AseDbType.SmallMoney;
                case TdsDataType.TDS_TEXT:
                    return AseDbType.Text;
                case TdsDataType.TDS_TIME:
                case TdsDataType.TDS_TIMEN:
                    return AseDbType.Time;
                case TdsDataType.TDS_UINT2:
                    return AseDbType.UnsignedSmallInt;
                case TdsDataType.TDS_UINTN:
                case TdsDataType.TDS_UINT4:
                    return AseDbType.UnsignedInt;
                case TdsDataType.TDS_UINT8:
                    return AseDbType.UnsignedBigInt;
                case TdsDataType.TDS_UNITEXT:
                    return AseDbType.Unitext;
                case TdsDataType.TDS_VARBINARY:
                    return AseDbType.VarBinary;
                case TdsDataType.TDS_XML:
                case TdsDataType.TDS_VARCHAR:
                    return AseDbType.VarChar;
                case TdsDataType.TDS_INTERVAL:
                    return AseDbType.TimeStamp;
                case TdsDataType.TDS_NUMN:
                    return AseDbType.Numeric;
                //case TdsDataType.TDS_SENSITIVITY:
                //    break;
                //case TdsDataType.TDS_VOID:
                //    break;
                //case TdsDataType.TDS_BOUNDARY:
                //    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

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