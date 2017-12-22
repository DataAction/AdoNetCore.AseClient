using System;
using System.Collections;
using System.Data;
using System.Text;
using System.Data.Common;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient
{
    public sealed class AseDataReader : DbDataReader
    {
        private readonly TableResult[] _results;
        private int _currentResult = -1;
        private int _currentRow = -1;
        private DataTable _currentSchemaTable;

        internal AseDataReader(TableResult[] results)
        {
            _results = results;
            NextResult();
        }

        public override bool GetBoolean(int i)
        {
            var obj = GetValue(i);

            AssertNotDBNull(obj);

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

            AssertNotDBNull(obj);

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

            AssertNotDBNull(obj);

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

            AssertNotDBNull(obj);

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

            AssertNotDBNull(obj);

            if (obj is TimeSpan i1)
            {
                return i1;
            }

            throw new InvalidCastException($"Cannot convert from {GetFieldType(i)} to TimeSpan");
        }

        public override decimal GetDecimal(int i)
        {
            var obj = GetValue(i);

            AssertNotDBNull(obj);

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

            AssertNotDBNull(obj);

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

        public override Type GetFieldType(int i) => typeof(object);

        public override float GetFloat(int i)
        {
            var obj = GetValue(i);

            AssertNotDBNull(obj);

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

            AssertNotDBNull(obj);

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

            AssertNotDBNull(obj);

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

            AssertNotDBNull(obj);

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

            AssertNotDBNull(obj);

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

            AssertNotDBNull(obj);

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

            AssertNotDBNull(obj);

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

            AssertNotDBNull(obj);

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

        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        private static void AssertNotDBNull(object obj)
        {
            if (obj == DBNull.Value)
            {
                throw new AseException(new AseError {IsError = true, IsFromClient = true, IsFromServer = false, IsInformation = false, IsWarning = false, Message = "Cannot read DBNull as type."});
            }
        }

        public override string GetName(int i)
        {
            if (_currentResult >= 0
                && _currentResult < _results.Length
                && i < _results[_currentResult].Formats.Length)
            {
                return _results[_currentResult].Formats[i].ColumnName;
            }

            throw new ArgumentOutOfRangeException(nameof(i));
        }

        public override int GetOrdinal(string name)
        {
            if (!string.IsNullOrEmpty(name) && _currentResult >= 0 && _currentResult < _results.Length)
            {
                name = name
                    .TrimStart('[')
                    .TrimEnd(']'); // TODO - this should be unnecessary - we should store the value in canonical form.

                var formats = _results[_currentResult].Formats;
                for (var i = 0; i < formats.Length; i++)
                {
                    if (string.Equals(formats[i].ColumnName?.TrimStart('[').TrimEnd(']'), name, StringComparison.OrdinalIgnoreCase))
                    {
                        return i;
                    }
                }
            }

            throw new ArgumentException();
        }
        public override object GetValue(int i)
        {
            if (IsDBNull(i))
            {
                return DBNull.Value;
            }

            if (_currentResult >= 0
                && _currentResult < _results.Length
                && _currentRow >= 0
                && _currentRow < _results[_currentResult].Rows.Count
                && i >= 0
                && i < _results[_currentResult].Rows[_currentRow].Items.Length)
            {
                return _results[_currentResult].Rows[_currentRow].Items[i];
            }

            throw new ArgumentOutOfRangeException();
        }

        public override int GetValues(object[] values)
        {
            var num = values.Length;

            if (_currentResult >= 0
                && _currentResult < _results.Length
                && _currentRow >= 0
                && _currentRow < _results[_currentResult].Rows.Count)
            {
                var items = _results[_currentResult].Rows[_currentRow].Items;

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

            return 0;
        }

        public override bool IsDBNull(int i)
        {
            if (_currentResult >= 0
                && _currentResult < _results.Length
                && _currentRow >= 0
                && _currentRow < _results[_currentResult].Rows.Count
                && i >= 0
                && i < _results[_currentResult].Rows[_currentRow].Items.Length)
            {
                var value = _results[_currentResult].Rows[_currentRow].Items[i];

                return value == DBNull.Value; // TODO - don't think this is right. Test with a debugger.
            }

            throw new ArgumentOutOfRangeException();
        }

        public override int FieldCount => _currentResult >= 0 && _currentResult < _results.Length
            ? _results[_currentResult].Formats.Length
            : 0;

        public override int VisibleFieldCount => FieldCount;

        public override bool HasRows => _currentResult >= 0 && _currentResult < _results.Length && _results[_currentResult].Rows.Count > 0;

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
            //return;
            if (_currentSchemaTable != null || FieldCount == 0)
            {
                return;
            }

            var table = new DataTable("SchemaTable");
            var columns = table.Columns;

            var columnName = columns.Add("ColumnName", typeof(string));
            var columnOrdinal = columns.Add("ColumnOrdinal", typeof(int));
            var columnSize = columns.Add("ColumnSize", typeof(int));
            var numericPrecision = columns.Add("NumericPrecision", typeof(int));
            var numericScale = columns.Add("NumericScale", typeof(int));
            var isUnique = columns.Add("IsUnique", typeof(bool));
            var isKey = columns.Add("IsKey", typeof(bool));
            var baseServerName = columns.Add("BaseServerName", typeof(string));
            var baseCatalogName = columns.Add("BaseCatalogName", typeof(string));
            var baseColumnName = columns.Add("BaseColumnName", typeof(string));
            var baseSchemaName = columns.Add("BaseSchemaName", typeof(string));
            var baseTableName = columns.Add("BaseTableName", typeof(string));
            var dataType = columns.Add("DataType", typeof(Type));
            var allowDBNull = columns.Add("AllowDBNull", typeof(bool));
            var providerType = columns.Add("ProviderType", typeof(int));
            var isAliased = columns.Add("IsAliased", typeof(bool));
            var isExpression = columns.Add("IsExpression", typeof(bool));
            var isIdentity = columns.Add("IsIdentity", typeof(bool));
            var isAutoIncrement = columns.Add("IsAutoIncrement", typeof(bool));
            var isRowVersion = columns.Add("IsRowVersion", typeof(bool));
            var isHidden = columns.Add("IsHidden", typeof(bool));
            var isLong = columns.Add("IsLong", typeof(bool));
            var isReadOnly = columns.Add("IsReadOnly", typeof(bool));
            columns.Add("ProviderSpecificDataType", typeof(Type));
            var dataTypeName = columns.Add("DataTypeName", typeof(string));
            //do we need these?
            columns.Add("XmlSchemaCollectionDatabase", typeof(string));
            columns.Add("XmlSchemaCollectionOwningSchema", typeof(string));
            columns.Add("XmlSchemaCollectionName", typeof(string));
            columns.Add("UdtAssemblyQualifiedName");
            columns.Add("NonVersionedProviderType", typeof(int));
            columns.Add("IsColumnSet");

            var formats = _results[_currentResult].Formats;
            for(var i = 0; i < formats.Length; i++)
            {
                var column = formats[i];
                var row = table.NewRow();

                row[columnName] = column.ColumnLabel; //todo: ColumnName or ColumnLabel?
                row[columnOrdinal] = i;
                row[columnSize] = column.Length ?? -1;
                row[numericPrecision] = column.Precision ?? -1;
                row[numericScale] = column.Scale ?? -1;
                row[isUnique] = false; //todo: Does TDS provide this?
                row[isKey] = false; //todo: Does TDS provide this?
                row[baseServerName] = string.Empty;
                row[baseCatalogName] = column.CatalogName;
                row[baseColumnName] = column.ColumnName; //todo: ColumnName or ColumnLabel?
                row[baseSchemaName] = column.SchemaName;
                row[baseTableName] = column.TableName;
                row[dataType] = TypeMap.GetNetType(column);
                row[allowDBNull] = column.IsNullable;
                row[providerType] = (DbType)column.DataType;
                row[isAliased] = !string.Equals(column.ColumnLabel, column.ColumnName, StringComparison.OrdinalIgnoreCase); //todo: is this what we're supposed to check?
                row[isExpression] = false; //todo?
                row[isIdentity] = false; //todo?
                row[isAutoIncrement] = false; //todo?
                row[isRowVersion] = false; //todo?
                row[isHidden] = false; //todo?
                row[isLong] = false; //todo?
                row[isReadOnly] = false; //todo?
                row[dataTypeName] = $"{column.DataType}";

                table.Rows.Add(row);
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
            _currentResult++;
            _currentSchemaTable = null;

            if (_results.Length > _currentResult)
            {
                _currentRow = -1;
                return true;
            }
            else
            {
                return false;
            }
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

            _currentRow++;

            return _results[_currentResult].Rows.Count > _currentRow;
        }

        public override int Depth => 0;
        public override bool IsClosed => _currentResult >= _results.Length;
        public override int RecordsAffected => _currentResult >= 0 && _currentResult < _results.Length
            ? _results[_currentResult].Rows.Count
            : 0;
    }
}
