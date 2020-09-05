#if ENABLE_SYSTEM_DATA_COMMON_EXTENSIONS
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using AdoNetCore.AseClient.Enum;

namespace AdoNetCore.AseClient.Internal
{
    internal class SchemaTableBuilder
    {
        private static readonly HashSet<TdsDataType> LongTdsTypes = new HashSet<TdsDataType>
        {
            TdsDataType.TDS_BLOB,
            TdsDataType.TDS_IMAGE,
            TdsDataType.TDS_LONGBINARY,
            TdsDataType.TDS_LONGCHAR,
            TdsDataType.TDS_TEXT,
            TdsDataType.TDS_UNITEXT
        };

        private readonly AseConnection _connection;
        private readonly FormatItem[] _formats;

        public SchemaTableBuilder(AseConnection connection, FormatItem[] formats)
        {
            _connection = connection;
            _formats = formats;
        }

        public DataTable BuildSchemaTable()
        {
            var table = new DataTable("SchemaTable");
            InitTableStructure(table);
            var fillResults = FillTableFromFormats(table);
            TryLoadKeyInfo(table, fillResults.BaseTableNameValue, fillResults.BaseSchemaNameValue, fillResults.BaseCatalogNameValue);
            return table;
        }

        private void InitTableStructure(DataTable table)
        {
            var columns = table.Columns;

            columns.Add(SchemaTableColumn.ColumnName, typeof(string));
            columns.Add(SchemaTableColumn.ColumnOrdinal, typeof(int));
            columns.Add(SchemaTableColumn.ColumnSize, typeof(int));
            columns.Add(SchemaTableColumn.NumericPrecision, typeof(int));
            columns.Add(SchemaTableColumn.NumericScale, typeof(int));
            columns.Add(SchemaTableColumn.IsUnique, typeof(bool));
            columns.Add(SchemaTableColumn.IsKey, typeof(bool));
            columns.Add(SchemaTableOptionalColumn.BaseServerName, typeof(string));
            columns.Add(SchemaTableOptionalColumn.BaseCatalogName, typeof(string));
            columns.Add(SchemaTableColumn.BaseColumnName, typeof(string));
            columns.Add(SchemaTableColumn.BaseSchemaName, typeof(string));
            columns.Add(SchemaTableColumn.BaseTableName, typeof(string));
            columns.Add(SchemaTableColumn.DataType, typeof(Type));
            columns.Add(SchemaTableColumn.AllowDBNull, typeof(bool));
            columns.Add(SchemaTableColumn.ProviderType, typeof(int));
            columns.Add(SchemaTableColumn.IsAliased, typeof(bool));
            columns.Add(SchemaTableColumn.IsExpression, typeof(bool));
            columns.Add(SchemaTableExtraColumn.IsIdentity, typeof(bool));
            columns.Add(SchemaTableOptionalColumn.IsAutoIncrement, typeof(bool));
            columns.Add(SchemaTableOptionalColumn.IsRowVersion, typeof(bool));
            columns.Add(SchemaTableOptionalColumn.IsHidden, typeof(bool));
            columns.Add(SchemaTableColumn.IsLong, typeof(bool));
            columns.Add(SchemaTableOptionalColumn.IsReadOnly, typeof(bool));
            columns.Add(SchemaTableOptionalColumn.ProviderSpecificDataType, typeof(Type));
            columns.Add(SchemaTableExtraColumn.DataTypeName, typeof(string));
            //do we need these?
            columns.Add(SchemaTableExtraColumn.XmlSchemaCollectionDatabase, typeof(string));
            columns.Add(SchemaTableExtraColumn.XmlSchemaCollectionOwningSchema, typeof(string));
            columns.Add(SchemaTableExtraColumn.XmlSchemaCollectionName, typeof(string));
            columns.Add(SchemaTableExtraColumn.UdtAssemblyQualifiedName);
            columns.Add(SchemaTableColumn.NonVersionedProviderType, typeof(int));
            //means "is column [a sparse] set", not "is column set [to a value]"
            columns.Add(SchemaTableExtraColumn.IsColumnSet, typeof(bool));
        }

        private FillTableResults FillTableFromFormats(DataTable table)
        {
            var i = 0;
            FillTableResults results = null;

            foreach (var column in _formats)
            {
                //var column = _formats[i];
                var row = table.NewRow();
                var aseDbType = TypeMap.GetAseDbType(column);

                row[SchemaTableColumn.ColumnName] = column.DisplayColumnName;
                row[SchemaTableColumn.ColumnOrdinal] = i;
                row[SchemaTableColumn.ColumnSize] = column.Length ?? -1;
                row[SchemaTableColumn.NumericPrecision] = column.Precision ?? -1;
                row[SchemaTableColumn.NumericScale] = column.Scale ?? -1;
                row[SchemaTableColumn.IsUnique] = false; // This gets set below.
                row[SchemaTableColumn.IsKey] = false;    // This gets set below - no idea why TDS_ROW_KEY is never set.
                row[SchemaTableOptionalColumn.BaseServerName] = string.Empty;
                row[SchemaTableOptionalColumn.BaseCatalogName] = column.CatalogName;
                row[SchemaTableColumn.BaseColumnName] = column.ColumnName;
                row[SchemaTableColumn.BaseSchemaName] = column.SchemaName;
                row[SchemaTableColumn.BaseTableName] = column.TableName;
                row[SchemaTableColumn.DataType] = TypeMap.GetNetType(column);
                row[SchemaTableColumn.AllowDBNull] = column.RowStatus.HasFlag(RowFormatItemStatus.TDS_ROW_NULLALLOWED);
                row[SchemaTableColumn.ProviderType] = aseDbType;
                row[SchemaTableColumn.IsAliased] = !string.IsNullOrWhiteSpace(column.ColumnLabel);
                row[SchemaTableColumn.IsExpression] = false; // It doesn't seem to matter that this isn't supported. The column gets flagged as TDS_ROW_UPDATABLE|TDS_ROW_NULLALLOWED so it doesn't cause an issue when an insert/update ignores it.
                row[SchemaTableExtraColumn.IsIdentity] = column.RowStatus.HasFlag(RowFormatItemStatus.TDS_ROW_IDENTITY);
                row[SchemaTableOptionalColumn.IsAutoIncrement] = column.RowStatus.HasFlag(RowFormatItemStatus.TDS_ROW_IDENTITY);
                row[SchemaTableOptionalColumn.IsRowVersion] = column.RowStatus.HasFlag(RowFormatItemStatus.TDS_ROW_VERSION);
                row[SchemaTableOptionalColumn.IsHidden] = column.RowStatus.HasFlag(RowFormatItemStatus.TDS_ROW_HIDDEN);
                row[SchemaTableColumn.IsLong] = LongTdsTypes.Contains(column.DataType);
                row[SchemaTableOptionalColumn.IsReadOnly] = !column.RowStatus.HasFlag(RowFormatItemStatus.TDS_ROW_UPDATABLE);
                row[SchemaTableExtraColumn.DataTypeName] = $"{aseDbType}";

                table.Rows.Add(row);

                results = results ?? new FillTableResults
                {
                    BaseTableNameValue = column.TableName,
                    BaseSchemaNameValue = column.SchemaName,
                    BaseCatalogNameValue = column.CatalogName
                };

                i++;
            }

            return results;
        }

        private void TryLoadKeyInfo(DataTable table, string baseTableNameValue, string baseSchemaNameValue, string baseCatalogNameValue)
        {
            if (_connection == null)
                throw new InvalidOperationException("Invalid AseCommand.Connection");

            if (_connection.State != ConnectionState.Open)
                throw new InvalidOperationException("Invalid AseCommand.Connection.ConnectionState");

            if (string.IsNullOrWhiteSpace(baseTableNameValue))
                return;

            using (var command = _connection.CreateCommand())
            {
                command.CommandText = $"{baseCatalogNameValue}..sp_oledb_getindexinfo";
                command.CommandType = CommandType.StoredProcedure;

                var tableName = command.CreateParameter();
                tableName.ParameterName = "@table_name";
                tableName.AseDbType = AseDbType.VarChar;
                tableName.Value = baseTableNameValue;
                command.Parameters.Add(tableName);

                var tableOwner = command.CreateParameter();
                tableOwner.ParameterName = "@table_owner";
                tableOwner.AseDbType = AseDbType.VarChar;
                tableOwner.Value = baseSchemaNameValue;
                command.Parameters.Add(tableOwner);

                var tableQualifier = command.CreateParameter();
                tableQualifier.ParameterName = "@table_qualifier";
                tableQualifier.AseDbType = AseDbType.VarChar;
                tableQualifier.Value = baseCatalogNameValue;
                command.Parameters.Add(tableQualifier);

                try
                {
                    var columnMetadata = new Dictionary<string, ColumnMetadata>();
                    using (var keyInfoDataReader = command.ExecuteReader())
                    {
                        while (keyInfoDataReader.Read())
                        {
                            var key = keyInfoDataReader["COLUMN_NAME"].ToString();

                            // Check for duplicate name and if we found any then give it a miss
                            if (columnMetadata.ContainsKey(key))
                            {
                                keyInfoDataReader.Close();
                                return;
                            }

                            columnMetadata.Add(key, new ColumnMetadata
                            {
                                Name = key,
                                Schema = keyInfoDataReader["TABLE_SCHEMA"].ToString(),
                                Catalog = keyInfoDataReader["TABLE_CATALOG"].ToString(),
                                IsKey = (bool) keyInfoDataReader["PRIMARY_KEY"],
                                IsUnique = (bool) keyInfoDataReader["UNIQUE"]
                            });
                        }
                    }

                    foreach (var metadata in columnMetadata.Values)
                    {
                        foreach (DataRow row in table.Rows)
                            if (string.Equals(metadata.Name, row[SchemaTableColumn.BaseColumnName].ToString(),
                                    StringComparison.OrdinalIgnoreCase) &&
                                string.Equals(metadata.Schema, row[SchemaTableColumn.BaseSchemaName].ToString(),
                                    StringComparison.OrdinalIgnoreCase) &&
                                string.Equals(metadata.Catalog, row[SchemaTableOptionalColumn.BaseCatalogName].ToString(),
                                    StringComparison.OrdinalIgnoreCase))
                        {
                            // Use the base column name in case the column is aliased.
                            {
                                row[SchemaTableColumn.IsKey] = metadata.IsKey;
                                row[SchemaTableColumn.IsUnique] = metadata.IsUnique;
                            }
                        }
                    }
                }
                catch
                {
                    // ignored
                }
            }
        }

        private class ColumnMetadata
        {
            public string Name { get; set; }
            public string Schema { get; set; }
            public string Catalog { get; set; }
            public bool IsKey { get; set; }
            public bool IsUnique { get; set; }
        }

        private class FillTableResults
        {
            public string BaseCatalogNameValue { get; set; }
            public string BaseSchemaNameValue { get; set; }
            public string BaseTableNameValue { get; set; }
        }
    }
}
#endif
