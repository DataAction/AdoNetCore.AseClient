using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Data.Common;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient
{
    public sealed class AseDataReader : DbDataReader
    {
        private static readonly HashSet<TdsDataType> LongTdsTypes = new HashSet<TdsDataType> {TdsDataType.TDS_BLOB, TdsDataType.TDS_IMAGE, TdsDataType.TDS_LONGBINARY, TdsDataType.TDS_LONGCHAR, TdsDataType.TDS_TEXT, TdsDataType.TDS_UNITEXT};

        private readonly TableResult[] _results;
        private int _currentResult = -1;
        private int _currentRow = -1;
        private readonly AseCommand _command;
        private readonly CommandBehavior _behavior;

#if !NETCORE_OLD
        private DataTable _currentSchemaTable;
#endif

        internal AseDataReader(TableResult[] results, AseCommand command, CommandBehavior behavior)
        {
            _results = results;
            _command = command;
            _behavior = behavior;
            NextResult();
        }

        public override bool GetBoolean(int i)
        {
            var obj = GetValue(i);

            if (obj is bool i1)
            {
                return i1;
            }

            if (!(obj is IConvertible convertible))
            {
                throw new InvalidCastException($"Cannot convert from {GetFieldType(i)} to Boolean");
            }

            return convertible.ToBoolean(System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
        }

        public override byte GetByte(int i)
        {
            var obj = GetValue(i);

            if (obj is byte i1)
            {
                return i1;
            }

            if (!(obj is IConvertible convertible))
            {
                throw new InvalidCastException($"Cannot convert from {GetFieldType(i)} to Byte");
            }

            return convertible.ToByte(System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
        }

        public override long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferOffset, int length)
        {
            if (IsDBNull(i))
            {
                return 0;
            }

            var obj = GetValue(i);

            if (obj == null)
            {
                return 0;
            }

            byte[] byteArray;
            long byteArrayLength;
            if (obj is byte[] bytes)
            {
                byteArray = bytes;
                byteArrayLength = byteArray.Length;
            }
            else
            {
                if (!(obj is string))
                {
                    return 0;
                }

                byteArray = Encoding.Unicode.GetBytes((string)obj);

                if (byteArray == null)
                {
                    return 0;
                }
                byteArrayLength = byteArray.Length;
            }

            if (buffer == null)
            {
                return byteArrayLength;
            }

            // Assume we can read all of the bytes requested.
            var bytesToRead = (long)length;

            // If the number of bytes required plus the position in the field exceed the length of the field
            if (length + fieldOffset >= byteArrayLength)
            {
                bytesToRead = byteArrayLength - fieldOffset; // Shrink the bytes requested.
            }

#if NETCOREAPP1_0 || NETCOREAPP1_1
            var cIndex = fieldOffset;
            var bIndex = (long)bufferOffset;

            for (long index3 = 0; index3 < bytesToRead; ++index3)
            {
                buffer[bIndex] = byteArray[cIndex];
                ++bIndex;
                ++cIndex;
            }
#else
            Array.Copy(byteArray, fieldOffset, buffer, bufferOffset, bytesToRead);
#endif

            return bytesToRead;
        }

        public override char GetChar(int i)
        {
            var obj = GetValue(i);

            if (obj is char i1)
            {
                return i1;
            }

            if (!(obj is IConvertible convertible))
            {
                throw new InvalidCastException($"Cannot convert from {GetFieldType(i)} to Char");
            }

            return convertible.ToChar(System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
        }

        public override long GetChars(int i, long fieldOffset, char[] buffer, int bufferoffset, int length)
        {
            if (IsDBNull(i))
            {
                return 0;
            }

            var obj = GetValue(i);

            if (obj == null)
            {
                return 0;
            }

            char[] charArray;
            long charArrayLength;

            if (obj is char[] c)
            {
                charArray = c;
                charArrayLength = charArray.Length - 1;
            }
            else
            {
                if (!(obj is string))
                {
                    return 0;
                }
                charArray = ((string)obj).ToCharArray();
                charArrayLength = charArray.Length;
            }

            if (buffer == null)
            {
                return charArrayLength;
            }

            // Assume we can read all of the bytes requested.
            var charsToRead = (long)length;

            // If the number of bytes required plus the position in the field exceed the length of the field
            if (length + fieldOffset >= charArrayLength)
            {
                charsToRead = charArrayLength - fieldOffset; // Shrink the bytes requested.
            }

#if NETCOREAPP1_0 || NETCOREAPP1_1
            var cIndex = fieldOffset;
            var bIndex = (long)bufferoffset;

            for (long index3 = 0; index3 < charsToRead; ++index3)
            {
                buffer[bIndex] = charArray[cIndex];
                ++bIndex;
                ++cIndex;
            }
#else
            Array.Copy(charArray, fieldOffset, buffer, bufferoffset, charsToRead);
#endif
            return charsToRead;
        }

        public override string GetDataTypeName(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override IEnumerator GetEnumerator()
        {
            return new AseDataReaderEnumerator(this);
        }

        public override DateTime GetDateTime(int i)
        {
            var obj = GetValue(i);

            if (obj is DateTime i1)
            {
                return i1;
            }

            if (!(obj is IConvertible convertible))
            {
                throw new InvalidCastException($"Cannot convert from {GetFieldType(i)} to DateTime");
            }

            return convertible.ToDateTime(System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat);
        }

        public TimeSpan GetTimeSpan(int i)
        {
            var obj = GetValue(i);

            if (obj is TimeSpan i1)
            {
                return i1;
            }

            throw new InvalidCastException($"Cannot convert from {GetFieldType(i)} to TimeSpan");
        }

        public override decimal GetDecimal(int i)
        {
            var obj = GetValue(i);

            if (obj is decimal i1)
            {
                return i1;
            }

            if (!(obj is IConvertible convertible))
            {
                throw new InvalidCastException($"Cannot convert from {GetFieldType(i)} to Decimal");
            }

            return convertible.ToDecimal(System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
        }

        public override double GetDouble(int i)
        {
            var obj = GetValue(i);

            if (obj is double i1)
            {
                return i1;
            }

            if (!(obj is IConvertible convertible))
            {
                throw new InvalidCastException($"Cannot convert from {GetFieldType(i)} to Double");
            }

            return convertible.ToDouble(System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
        }

        public override Type GetFieldType(int i)
        {
            var format = GetFormat(i);
            return format == null
                ? typeof(object)
                : TypeMap.GetNetType(format, true);
        }

        public override float GetFloat(int i)
        {
            var obj = GetValue(i);

            if (obj is float i1)
            {
                return i1;
            }

            if (!(obj is IConvertible convertible))
            {
                throw new InvalidCastException($"Cannot convert from {GetFieldType(i)} to Float");
            }

            return convertible.ToSingle(System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
        }

        public override Guid GetGuid(int i)
        {
            if (IsDBNull(i))
            {
                return Guid.Empty;
            }

            var obj = GetValue(i);

            if (obj == null)
            {
                return Guid.Empty;
            }

            if (obj is byte[] bytes)
            {
                if (bytes.Length == 16)
                {
                    return new Guid(bytes);
                }
            }

            throw new InvalidCastException($"Cannot convert from {GetFieldType(i)} to Guid");
        }

        public override short GetInt16(int i)
        {
            var obj = GetValue(i);

            if (obj is short i1)
            {
                return i1;
            }

            if (!(obj is IConvertible convertible))
            {
                throw new InvalidCastException($"Cannot convert from {GetFieldType(i)} to Int16");
            }

            return convertible.ToInt16(System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
        }

        public override int GetInt32(int i)
        {
            var obj = GetValue(i);

            if (obj is int i1)
            {
                return i1;
            }

            if (!(obj is IConvertible convertible))
            {
                throw new InvalidCastException($"Cannot convert from {GetFieldType(i)} to Int32");
            }

            return convertible.ToInt32(System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
        }

        public override long GetInt64(int i)
        {
            var obj = GetValue(i);

            if (obj is long i1)
            {
                return i1;
            }

            if (!(obj is IConvertible convertible))
            {
                throw new InvalidCastException($"Cannot convert from {GetFieldType(i)} to Int64");
            }

            return convertible.ToInt64(System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
        }

        public ushort GetUInt16(int i)
        {
            var obj = GetValue(i);

            if (obj is ushort i1)
            {
                return i1;
            }

            if (!(obj is IConvertible convertible))
            {
                throw new InvalidCastException($"Cannot convert from {GetFieldType(i)} to UInt16");
            }

            return convertible.ToUInt16(System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
        }

        public uint GetUInt32(int i)
        {
            var obj = GetValue(i);

            if (obj is uint i1)
            {
                return i1;
            }

            if (!(obj is IConvertible convertible))
            {
                throw new InvalidCastException($"Cannot convert from {GetFieldType(i)} to UInt32");
            }

            return convertible.ToUInt32(System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
        }

        public ulong GetUInt64(int i)
        {
            var obj = GetValue(i);

            if (obj is ulong i1)
            {
                return i1;
            }

            if (!(obj is IConvertible convertible))
            {
                throw new InvalidCastException($"Cannot convert from {GetFieldType(i)} to UInt64");
            }

            return convertible.ToUInt64(System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
        }

        public override string GetString(int i)
        {
            var obj = GetValue(i);

            if (obj is string s)
            {
                return s;
            }

            if (obj is char[] c)
            {
                return new string(c, 0, c.Length - 1);
            }

            if (!(obj is IConvertible convertible))
            {
                throw new InvalidCastException($"Cannot convert from {GetFieldType(i)} to String");
            }

            return convertible.ToString(System.Globalization.CultureInfo.CurrentCulture);
        }

        //public AseDecimal GetAseDecimal(int ordinal)
        //{
        //    throw new NotImplementedException();
        //}

        public override string GetName(int i)
        {
            var format = GetFormat(i);
            if (format == null)
            {
                throw new ArgumentOutOfRangeException(nameof(i));
            }

            return format.DisplayColumnName;
        }

        public override int GetOrdinal(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException();
            }

            var formats = CurrentResultSet?.Formats;

            if (formats == null)
            {
                throw new ArgumentException();
            }

            name = name
                .TrimStart('[')
                .TrimEnd(']'); // TODO - this should be unnecessary - we should store the value in canonical form.

            for (var i = 0; i < formats.Length; i++)
            {
                if (string.Equals(formats[i].DisplayColumnName?.TrimStart('[').TrimEnd(']'), name, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            throw new ArgumentException();
        }

        public override object GetValue(int i)
        {
            if (!ValueExists(i))
            {
                throw new ArgumentOutOfRangeException();
            }

            return IsDBNull(i)
                ? DBNull.Value
                : CurrentRow.Items[i];
        }

        public override int GetValues(object[] values)
        {
            var num = values.Length;
            var items = CurrentRow?.Items;

            if (items == null)
            {
                return 0;
            }

            if (num > items.Length)
            {
                num = items.Length;
            }

            Array.Copy(items, 0, values, 0, num); // TODO - check how DBNull.Value goes back.

            if (num < values.Length)
            {
                Array.Clear(values, num, values.Length - num); // Clear any extra values to avoid confusion.
            }
            return num;

        }

        public override bool IsDBNull(int i)
        {
            if (!ValueExists(i))
            {
                throw new ArgumentOutOfRangeException();
            }

            return CurrentRow.Items[i] == DBNull.Value;
        }

        public override int FieldCount => CurrentResultSet?.Formats?.Length ?? 0;

        public override int VisibleFieldCount => FieldCount;

        public override bool HasRows => CurrentRowCount > 0;

        public override object this[int ordinal] => GetValue(ordinal);

        public override object this[string name] => GetValue(GetOrdinal(name));

#if NETCORE_OLD
        public void Close() { }
#else
        public override void Close() { }
#endif

#if NETCORE_OLD
        public DataTable GetSchemaTable()
        {
            return null;
        }
#else
        public override DataTable GetSchemaTable()
        {
            EnsureSchemaTable();
            return _currentSchemaTable;
        }

        private void EnsureSchemaTable()
        {
            if (_currentSchemaTable != null || FieldCount == 0)
            {
                return;
            }

            var formats = CurrentResultSet?.Formats;

            if (formats == null)
            {
                return;
            }

            var table = new DataTable("SchemaTable");
            var columns = table.Columns;

            var columnName = columns.Add(SchemaTableColumn.ColumnName, typeof(string));
            var columnOrdinal = columns.Add(SchemaTableColumn.ColumnOrdinal, typeof(int));
            var columnSize = columns.Add(SchemaTableColumn.ColumnSize, typeof(int));
            var numericPrecision = columns.Add(SchemaTableColumn.NumericPrecision, typeof(int));
            var numericScale = columns.Add(SchemaTableColumn.NumericScale, typeof(int));
            var isUnique = columns.Add(SchemaTableColumn.IsUnique, typeof(bool));
            var isKey = columns.Add(SchemaTableColumn.IsKey, typeof(bool));
            var baseServerName = columns.Add(SchemaTableOptionalColumn.BaseServerName, typeof(string));
            var baseCatalogName = columns.Add(SchemaTableOptionalColumn.BaseCatalogName, typeof(string));
            var baseColumnName = columns.Add(SchemaTableColumn.BaseColumnName, typeof(string));
            var baseSchemaName = columns.Add(SchemaTableColumn.BaseSchemaName, typeof(string));
            var baseTableName = columns.Add(SchemaTableColumn.BaseTableName, typeof(string));
            var dataType = columns.Add(SchemaTableColumn.DataType, typeof(Type));
            var allowDBNull = columns.Add(SchemaTableColumn.AllowDBNull, typeof(bool));
            var providerType = columns.Add(SchemaTableColumn.ProviderType, typeof(int));
            var isAliased = columns.Add(SchemaTableColumn.IsAliased, typeof(bool));
            var isExpression = columns.Add(SchemaTableColumn.IsExpression, typeof(bool));
            var isIdentity = columns.Add("IsIdentity", typeof(bool));
            var isAutoIncrement = columns.Add(SchemaTableOptionalColumn.IsAutoIncrement, typeof(bool));
            var isRowVersion = columns.Add(SchemaTableOptionalColumn.IsRowVersion, typeof(bool));
            var isHidden = columns.Add(SchemaTableOptionalColumn.IsHidden, typeof(bool));
            var isLong = columns.Add(SchemaTableColumn.IsLong, typeof(bool));
            var isReadOnly = columns.Add(SchemaTableOptionalColumn.IsReadOnly, typeof(bool));
            columns.Add(SchemaTableOptionalColumn.ProviderSpecificDataType, typeof(Type));
            var dataTypeName = columns.Add("DataTypeName", typeof(string));
            //do we need these?
            columns.Add("XmlSchemaCollectionDatabase", typeof(string));
            columns.Add("XmlSchemaCollectionOwningSchema", typeof(string));
            columns.Add("XmlSchemaCollectionName", typeof(string));
            columns.Add("UdtAssemblyQualifiedName");
            columns.Add(SchemaTableColumn.NonVersionedProviderType, typeof(int));
            //means "is column [a sparse] set", not "is column set [to a value]"
            columns.Add("IsColumnSet", typeof(bool));

            string baseCatalogNameValue = null;
            string baseSchemaNameValue = null;
            string baseTableNameValue = null;

            for (var i = 0; i < formats.Length; i++)
            {
                var column = formats[i];
                var row = table.NewRow();
                var aseDbType = TypeMap.GetAseDbType(column);

                row[columnName] = column.DisplayColumnName;
                row[columnOrdinal] = i;
                row[columnSize] = column.Length ?? -1;
                row[numericPrecision] = column.Precision ?? -1;
                row[numericScale] = column.Scale ?? -1;
                row[isUnique] = false; // This gets set below.
                row[isKey] = false; // This gets set below - no idea why TDS_ROW_KEY is never set.
                row[baseServerName] = string.Empty;
                row[baseCatalogName] = column.CatalogName;
                row[baseColumnName] = column.ColumnName;
                row[baseSchemaName] = column.SchemaName;
                row[baseTableName] = column.TableName;
                row[dataType] = TypeMap.GetNetType(column);
                row[allowDBNull] = column.RowStatus.HasFlag(RowFormatItemStatus.TDS_ROW_NULLALLOWED);
                row[providerType] = aseDbType;
                row[isAliased] = !string.IsNullOrWhiteSpace(column.ColumnLabel);
                row[isExpression] = false; // It doesn't seem to matter that this isn't supported. The column gets flagged as TDS_ROW_UPDATABLE|TDS_ROW_NULLALLOWED so it doesn't cause an issue when an insert/update ignores it.
                row[isIdentity] = column.RowStatus.HasFlag(RowFormatItemStatus.TDS_ROW_IDENTITY); 
                row[isAutoIncrement] = column.RowStatus.HasFlag(RowFormatItemStatus.TDS_ROW_IDENTITY);
                row[isRowVersion] = column.RowStatus.HasFlag(RowFormatItemStatus.TDS_ROW_VERSION);
                row[isHidden] = column.RowStatus.HasFlag(RowFormatItemStatus.TDS_ROW_HIDDEN);
                row[isLong] = LongTdsTypes.Contains(column.DataType);
                row[isReadOnly] = !column.RowStatus.HasFlag(RowFormatItemStatus.TDS_ROW_UPDATABLE);
                row[dataTypeName] = $"{aseDbType}";

                table.Rows.Add(row);

                if (string.IsNullOrEmpty(baseTableNameValue))
                {
                    baseTableNameValue = column.TableName;
                    baseSchemaNameValue = column.SchemaName;
                    baseCatalogNameValue = column.CatalogName;
                }
            }

            // Try to load the key info
            //if ((_behavior & CommandBehavior.KeyInfo) == CommandBehavior.KeyInfo) 
            {
                if (_command.Connection == null)
                {
                    throw new InvalidOperationException("Invalid AseCommand.Connection");
                }
                if (_command.Connection.State != ConnectionState.Open)
                {
                    throw new InvalidOperationException("Invalid AseCommand.Connection.ConnectionState");
                }

                using (var command = _command.Connection.CreateCommand())
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
                        using (var keyInfoDataReader = command.ExecuteReader())
                        {
                            while (keyInfoDataReader.Read())
                            {
                                var keyColumnName = keyInfoDataReader["COLUMN_NAME"].ToString();
                                var keySchemaName = keyInfoDataReader["TABLE_SCHEMA"].ToString();
                                var keyCatalogName = keyInfoDataReader["TABLE_CATALOG"].ToString();

                                foreach (DataRow row in table.Rows)
                                {
                                    // Use the base column name in case the column is aliased.
                                    if (
                                        string.Equals(keyColumnName, row[baseColumnName].ToString(),
                                            StringComparison.OrdinalIgnoreCase) &&
                                        string.Equals(keySchemaName, row[baseSchemaName].ToString(),
                                            StringComparison.OrdinalIgnoreCase) &&
                                        string.Equals(keyCatalogName, row[baseCatalogName].ToString(),
                                            StringComparison.OrdinalIgnoreCase))
                                    {
                                        row[isKey] = (bool) keyInfoDataReader["PRIMARY_KEY"];
                                        row[isUnique] = (bool) keyInfoDataReader["UNIQUE"];
                                    }
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

            _currentSchemaTable = table;
        }
#endif

        /// <summary>
        /// Advances the reader to the next result set.
        /// </summary>
        /// <returns>true if the reader is pointing at a record set; false otherwise.</returns>
        public override bool NextResult()
        {
            // If we have read one row of data, jump to the end.
            if ((_behavior & CommandBehavior.SingleRow) == CommandBehavior.SingleRow && _currentRow == 0)
            {
                // Jump to the end
                return false;
            }

            // If we have read one result set
            if ((_behavior & CommandBehavior.SingleResult) == CommandBehavior.SingleResult && _currentResult == 0)
            {
                // Jump to the end
                return false;
            }

            _currentResult++;

#if !NETCORE_OLD
            _currentSchemaTable = null;
#endif

            if (_results.Length > _currentResult)
            {
                _currentRow = -1;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Advance the reader to the next record in the current result set.
        /// </summary>
        /// <returns>true if the reader is pointing at a row of data; false otherwise.</returns>
        public override bool Read()
        {
            if (_currentResult < 0)
            {
                return false;
            }

            // If we have read one row of data, jump to the end.
            if ((_behavior & CommandBehavior.SingleRow) == CommandBehavior.SingleRow && _currentRow == 0)
            {
                // Jump to the end
                _currentResult = _results.Length - 1;
                _currentRow = _results[_currentResult].Rows.Count;
            }
            else
            {
                _currentRow++;
            }

            return _results[_currentResult].Rows.Count > _currentRow;
        }

        public override int Depth => 0;
        public override bool IsClosed => _currentResult >= _results.Length;
        public override int RecordsAffected => _currentResult >= 0 && _currentResult < _results.Length
            ? _results[_currentResult].Rows.Count
            : 0;

        /// <summary>
        /// Confirm that the reader is pointing at a result set
        /// </summary>
        private bool WithinResultSet => _currentResult >= 0 && _currentResult < _results.Length;

        /// <summary>
        /// Confirm that the reader is pointing at a row within a result set
        /// </summary>
        private bool WithinRow => WithinResultSet && _currentRow >= 0 && _currentRow < CurrentResultSet.Rows.Count;

        /// <summary>
        /// Confirm that there is a value at the supplied index (does not confirm whether value is null or set)
        /// </summary>
        private bool ValueExists(int i)
        {
            var cr = CurrentRow;
            return cr != null && i >= 0 && i < cr.Items.Length;
        }

        /// <summary>
        /// From the current result set, get the FormatItem at the specified index.
        /// </summary>
        /// <returns>Returns the specified format item, or null</returns>
        private FormatItem GetFormat(int i)
        {
            var formats = CurrentResultSet?.Formats;
            return formats != null && i >= 0 && i < formats.Length
                ? formats[i]
                : null;
        }

        /// <summary>
        /// Get the number of rows in the current result set, or 0 if there is no result set.
        /// </summary>
        private int CurrentRowCount => CurrentResultSet?.Rows?.Count ?? 0;

        /// <summary>
        /// Get the current result set, or null if there is none
        /// </summary>
        private TableResult CurrentResultSet => WithinResultSet ? _results[_currentResult] : null;

        /// <summary>
        /// Get the current row, or null if there is none
        /// </summary>
        private RowResult CurrentRow => WithinRow ? CurrentResultSet.Rows[_currentRow] : null;
    }
}
