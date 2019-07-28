using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Data.Common;
using System.Linq;
using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient
{
    public sealed class AseDataReader : DbDataReader
    {
        private readonly TableResult[] _results;
        private int _currentResult = -1;
        private int _currentRow = -1;
        private readonly CommandBehavior _behavior;

#if SYSTEM_DATA_COMMON_EXTENSIONS
        private readonly AseCommand _command;
        private DataTable _currentSchemaTable;
#endif

        internal AseDataReader(IEnumerable<TableResult> results, AseCommand command, CommandBehavior behavior)
        {
            _results = results.ToArray();
#if SYSTEM_DATA_COMMON_EXTENSIONS
            _command = command;
#endif
            _behavior = behavior;
            NextResult();
        }

        public override bool GetBoolean(int i)
        {
            var obj = GetNonNullValue(i);

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
            var obj = GetNonNullValue(i);

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

            var cIndex = fieldOffset;
            var bIndex = (long)bufferOffset;

            for (long index3 = 0; index3 < bytesToRead; ++index3)
            {
                buffer[bIndex] = byteArray[cIndex];
                ++bIndex;
                ++cIndex;
            }

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

        public override long GetChars(int i, long fieldOffset, char[] buffer, int bufferOffset, int length)
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
            var bIndex = (long)bufferOffset;

            for (long index3 = 0; index3 < charsToRead; ++index3)
            {
                buffer[bIndex] = charArray[cIndex];
                ++bIndex;
                ++cIndex;
            }
#else
            Array.Copy(charArray, fieldOffset, buffer, bufferOffset, charsToRead);
#endif
            return charsToRead;
        }

        public override string GetDataTypeName(int ordinal)
        {
            if (CurrentResultSet == null || ordinal < 0)
            {
                throw new IndexOutOfRangeException($"Column referenced by index ({ordinal}) does not exist");
            }

            if (ordinal >= CurrentResultSet.Formats.Length)
            {
                throw new AseException("The column specified does not exist.", 30118);
            }

            return CurrentResultSet.Formats[ordinal].GetDataTypeName();
        }

        public override IEnumerator GetEnumerator()
        {
            return new AseDataReaderEnumerator(this);
        }

        public override DateTime GetDateTime(int i)
        {
            var obj = GetNonNullValue(i);

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
            var obj = GetNonNullValue(i);

            switch (obj)
            {
                case TimeSpan ts:
                    return ts;
                case DateTime dt:
                    return dt.TimeOfDay;
                default:
                    throw new InvalidCastException($"Cannot convert from {GetFieldType(i)} to TimeSpan");
            }
        }

        public override decimal GetDecimal(int i)
        {
            var obj = GetNonNullValue(i);

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
            var obj = GetNonNullValue(i);

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
            var obj = GetNonNullValue(i);

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
            var obj = GetNonNullValue(i);

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
            var obj = GetNonNullValue(i);

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
            var obj = GetNonNullValue(i);

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
            var obj = GetNonNullValue(i);

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
            var obj = GetNonNullValue(i);

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
            var obj = GetNonNullValue(i);

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
            var obj = GetNonNullValue(i);

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

        private object GetNonNullValue(int i)
        {
            var obj = GetValue(i);

            if (obj == DBNull.Value || obj == null)
            {
                throw new AseException("Value in column is null", 30014);
            }

            return obj;
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

#if SYSTEM_DATA_COMMON_EXTENSIONS
        public override void Close() { }
#else
        public void Close() { }
#endif

#if SYSTEM_DATA_COMMON_EXTENSIONS
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

            _currentSchemaTable = new SchemaTableBuilder(_command?.Connection, formats).BuildSchemaTable();
        }

#else
        public DataTable GetSchemaTable()
        {
            return null;
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

#if SYSTEM_DATA_COMMON_EXTENSIONS
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
            if (_currentResult < 0 || _currentResult >= _results.Length)
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

        public IList GetList()
        {
            return null; //todo: implement -- populate a DataView with rows from the current record set and return it.
        }

        public bool ContainsListCollection => false;

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
