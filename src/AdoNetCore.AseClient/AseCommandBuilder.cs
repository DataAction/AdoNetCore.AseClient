#if !NETCORE_OLD
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace AdoNetCore.AseClient
{
    public sealed class AseCommandBuilder : DbCommandBuilder
    {
        private string _commandText;
        private DataTable _schema;

        /// <summary>
        /// A mapping of ASE type names to the AseDbType enumeration.
        /// </summary>
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
            {93, AseDbType.DateTime },
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

        public new AseDataAdapter DataAdapter
        {
            get => (AseDataAdapter) base.DataAdapter;
            set
            {
                base.DataAdapter = value;
                value.CommandBuilder = this;
            }
        }

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
//                    if (aseDataReader._missingKey != null) // TODO - this depends on the DataSet branch where schema is loaded.
//                    {
//                        throw new MissingPrimaryKeyException(aseDataReader._missingKey);
//                    }
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
            var owner = (string) null;
            var database = (string) null;

            if (commandText.IndexOf('.') >= 0)
            {
                var commandParts = commandText.Split('.');
                switch (commandParts.Length)
                {
                    // owner.object_name
                    case 2:
                        owner = commandParts[0].Trim();
                        commandText = commandParts[1].Trim();
                        break;
                    // database.owner.object_name
                    case 3:
                        database = commandParts[0].Trim();

                        owner = commandParts[1].Trim();

                        commandText = commandParts[2].Trim();

                        if (database.Length > 0)
                        {
                            cmdText = database + ".." + cmdText;
                        }
                        break;
                    case 4:
                        string str4 = commandParts[0].Trim();
                        string str5 = commandParts[1].Trim();

                        owner = commandParts[2].Trim();
                        commandText = commandParts[3].Trim();

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

            using (var aseCommand = new AseCommand(command.Connection) {CommandText = cmdText, CommandType = CommandType.StoredProcedure })
            {
                aseCommand.Parameters.Add("@procedure_name", AseDbType.VarChar).Value = commandText;

                if (owner?.Length > 0)
                {
                    aseCommand.Parameters.Add("@procedure_owner", AseDbType.VarChar).Value = owner;
                }

                try
                {
                    using (var reader = aseCommand.ExecuteReader())
                    {
                        var isFirstParameter = true;

                        while (reader.Read())
                        {
                            var parameterName = reader.GetString(3);

                            if (!parameterName.StartsWith("@"))
                            {
                                parameterName = "@" + parameterName;
                            }

                            var isNullable = false;
                            if (!reader.IsDBNull(11))
                            {
                                isNullable = reader.GetInt16(11) != 0;
                            }

                            byte precision = 0;
                            if (!reader.IsDBNull(7))
                            {
                                precision = reader.GetByte(7);
                            }

                            var size = 0;
                            if (!reader.IsDBNull(8))
                            {
                                size = reader.GetInt16(8);
                            }

                            byte scale = 0;
                            if (!reader.IsDBNull(9))
                            {
                                try
                                {
                                    scale = reader.GetByte(9);
                                }
                                catch (FormatException)
                                {
                                    // Swallow the FormatException if it's because the value is "NULL"
                                    if (!string.Equals(reader.GetString(9).Trim(), "NULL", StringComparison.OrdinalIgnoreCase))
                                    {
                                        throw; 
                                    }
                                }
                            }

                            var dbType = default(AseDbType);

                            if (!reader.IsDBNull(6) && !AseDbTypeNameMap.TryGetValue(reader.GetString(6), out dbType))
                            {
                                dbType = AseDbTypeCodeMap[reader.GetInt16(16)];
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
                catch (Exception ex)
                {
                    throw ex; // TODO - this doesn't look right. 
                }
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
            if (parameterOrdinal <= 0)
            {
                throw new AseException(new AseError { IsError = true, IsFromClient = true, MessageNumber = 30070});
            }
            return "@p" + parameterOrdinal;
        }

        protected override string GetParameterPlaceholder(int parameterOrdinal)
        {
            if (parameterOrdinal <= 0)
            {
                throw new AseException(new AseError { IsError = true, IsFromClient = true, MessageNumber = 30070 });
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