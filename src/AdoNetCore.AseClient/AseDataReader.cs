using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Data;
using System.Text;
using System.Data.Common;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient
{
    public sealed class AseDataReader : DbDataReader
    {
        private TableResult _currentTable;
        private readonly BlockingCollection<object> _results;
        private int _currentResult = -1;
        private int _currentRow = -1;
        private readonly CommandBehavior _behavior;
        private readonly IInfoMessageEventNotifier _eventNotifier;
        private bool _hasFirst;

#if ENABLE_SYSTEM_DATA_COMMON_EXTENSIONS
        private readonly AseCommand _command;
        private DataTable _currentSchemaTable;

        internal AseDataReader(AseCommand command, CommandBehavior behavior, IInfoMessageEventNotifier eventNotifier) : this(behavior, eventNotifier)
        {
            _command = command;
        }
#endif

        // ReSharper disable once MemberCanBePrivate.Global
        internal AseDataReader(CommandBehavior behavior, IInfoMessageEventNotifier eventNotifier)
        {
            _results = new BlockingCollection<object>();
            _currentTable = null;
            _hasFirst = false;

            _behavior = behavior;
            _eventNotifier = eventNotifier;
        }

        internal void AddResult(TableResult result)
        {
            _results.Add(result);

            // If this is the first data result back, then we should automatically point at it.
            if (!_hasFirst)
            {
                NextResult();
                _hasFirst = true;
            }
        }

        internal void AddResult(MessageResult result)
        {
            _results.Add(result);
        }

        internal void CompleteAdding()
        {
            _results.CompleteAdding();
        }

        public override bool GetBoolean(int i)
        {
            return GetPrimitive(i, (value, provider) => value.ToBoolean(provider));
        }

        public override byte GetByte(int i)
        {
            return GetPrimitive(i, (value, provider) => value.ToByte(provider));
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

#if LONG_ARRAY_COPY_UNAVAILABLE
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
            return GetPrimitive(i, (value, provider) => value.ToChar(provider));
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
            if (_currentTable  == null || ordinal < 0)
            {
                throw new IndexOutOfRangeException($"Column referenced by index ({ordinal}) does not exist");
            }

            if (ordinal >= _currentTable .Formats.Length)
            {
                throw new AseException("The column specified does not exist.", 30118);
            }

            return _currentTable .Formats[ordinal].GetDataTypeName();
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
            return GetPrimitive(i, (value, provider) => value.ToDecimal(provider));
        }

        public override double GetDouble(int i)
        {
            return GetPrimitive(i, (value, provider) => value.ToDouble(provider));
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
            return GetPrimitive(i, (value, provider) => value.ToSingle(provider));
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
            return GetPrimitive(i, (value, provider) => value.ToInt16(provider));
        }

        public override int GetInt32(int i)
        {
            return GetPrimitive(i, (value, provider) => value.ToInt32(provider));
        }

        public override long GetInt64(int i)
        {
            return GetPrimitive(i, (value, provider) => value.ToInt64(provider));
        }

        public ushort GetUInt16(int i)
        {
            return GetPrimitive(i, (value, provider) => value.ToUInt16(provider));
        }

        public uint GetUInt32(int i)
        {
            return GetPrimitive(i, (value, provider) => value.ToUInt32(provider));
        }

        public ulong GetUInt64(int i)
        {
            return GetPrimitive(i, (value, provider) => value.ToUInt64(provider));
        }

        private T GetPrimitive<T>(int i, Func<IConvertible, IFormatProvider, T> convert)
        {
            var obj = GetNonNullValue(i);

            if (obj is T i1)
            {
                return i1;
            }

            if (obj is IConvertible convertible)
            {
                var formatProvider = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;

                return convert(convertible, formatProvider);
            }

            throw new InvalidCastException($"Cannot convert from {GetFieldType(i)} to {nameof(T)}");
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

            var formats = _currentTable?.Formats;

            if (formats == null)
            {
                throw new ArgumentException();
            }

            name = name
                .TrimStart('[')
                .TrimEnd(']'); 

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

        public override int FieldCount => _currentTable?.Formats?.Length ?? 0;

        public override int VisibleFieldCount => FieldCount;

        public override bool HasRows => CurrentRowCount > 0;

        public override object this[int ordinal] => GetValue(ordinal);

        public override object this[string name] => GetValue(GetOrdinal(name));

#if ENABLE_SYSTEM_DATA_COMMON_EXTENSIONS
        public override void Close() { }
#else
        public void Close() { }
#endif

#if ENABLE_SYSTEM_DATA_COMMON_EXTENSIONS
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

            var formats = _currentTable?.Formats;

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

#if ENABLE_SYSTEM_DATA_COMMON_EXTENSIONS
            _currentSchemaTable = null;
#endif

            while (_results.TryTake(out var nextItem, -1))
            {
                if (nextItem is TableResult dataItem)
                {
                    _currentTable = dataItem;
                    
                    _currentRow = -1;
                    return true;
                }
                if (nextItem is MessageResult messageItem)
                {
                    _eventNotifier?.NotifyInfoMessage(messageItem.Errors, messageItem.Message);
                }
            }

            return false;
        }

        /// <summary>
        /// Advance the reader to the next record in the current result set.
        /// </summary>
        /// <returns>true if the reader is pointing at a row of data; false otherwise.</returns>
        public override bool Read()
        {
            if (_currentTable == null)
            {
                return false;
            }

            // If we have read one row of data, jump to the end.
            if ((_behavior & CommandBehavior.SingleRow) == CommandBehavior.SingleRow && _currentRow == 0)
            {
                // Jump to the end
                while (_results.TryTake(out var nextItem, -1))
                {
                    if (nextItem is TableResult dataItem)
                    {
                        _currentTable = dataItem;
                        _currentResult++;
                    }
                    if (nextItem is MessageResult messageItem)
                    {
                        _eventNotifier?.NotifyInfoMessage(messageItem.Errors, messageItem.Message);
                    }
                }

                _currentRow = _currentTable.Rows.Count;
            }
            else
            {
                _currentRow++;
            }

            return _currentTable?.Rows.Count > _currentRow;
        }

        public override int Depth => 0;
        public override bool IsClosed => _currentTable == null;
        public override int RecordsAffected => _currentTable?.Rows.Count ?? 0;

        public IList GetList()
        {
            return null; //todo: implement -- populate a DataView with rows from the current record set and return it.
        }

        public bool ContainsListCollection => false;

        /// <summary>
        /// Confirm that the reader is pointing at a row within a result set
        /// </summary>
        private bool WithinRow => _currentTable != null && _currentRow >= 0 && _currentRow < _currentTable.Rows.Count;

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
            var formats = _currentTable?.Formats;
            return formats != null && i >= 0 && i < formats.Length
                ? formats[i]
                : null;
        }

        /// <summary>
        /// Get the number of rows in the current result set, or 0 if there is no result set.
        /// </summary>
        private int CurrentRowCount => _currentTable?.Rows?.Count ?? 0;

        /// <summary>
        /// Get the current row, or null if there is none
        /// </summary>
        private RowResult CurrentRow => WithinRow ? _currentTable.Rows[_currentRow] : null;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _results?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
