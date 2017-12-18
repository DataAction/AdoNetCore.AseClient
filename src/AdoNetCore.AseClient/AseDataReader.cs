using System;
using System.Data;
using System.Text;
using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient
{
    public sealed class AseDataReader : IDataReader
    {
        //todo: needs unit tests, feels a bit flimsy
        private readonly TableResult[] _results;
        private int _currentResult = -1;
        private int _currentRow = -1;

        internal AseDataReader(TableResult[] results)
        {
            _results = results;
            NextResult();
        }

        public bool GetBoolean(int i)
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

        public byte GetByte(int i)
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

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferOffset, int length)
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

        public char GetChar(int i)
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

        public long GetChars(int i, long fieldOffset, char[] buffer, int bufferoffset, int length)
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

        public IDataReader GetData(int i)
        {
            throw new NotImplementedException();
        }

        public string GetDataTypeName(int i)
        {
            throw new NotImplementedException();
        }

        public DateTime GetDateTime(int i)
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

        public decimal GetDecimal(int i)
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

        public double GetDouble(int i)
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

        public Type GetFieldType(int i) => typeof(object);

        public float GetFloat(int i)
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

        public Guid GetGuid(int i)
        {
            throw new NotSupportedException($"{nameof(AseDataReader)}.{nameof(GetGuid)}({nameof(i)})");
        }

        public short GetInt16(int i)
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

        public int GetInt32(int i)
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

        public long GetInt64(int i)
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

        public string GetString(int i)
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

        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        private static void AssertNotDBNull(object obj)
        {
            if (obj == DBNull.Value)
            {
                throw new AseException(new AseError {IsError = true, IsFromClient = true, IsFromServer = false, IsInformation = false, IsWarning = false, Message = "Cannot read DBNull as type."});
            }
        }

        public string GetName(int i)
        {
            if (_currentResult >= 0
                && _currentResult < _results.Length
                && i < _results[_currentResult].Formats.Length)
            {
                return _results[_currentResult].Formats[i].ColumnName;
            }

            throw new ArgumentOutOfRangeException(nameof(i));
        }

        public int GetOrdinal(string name)
        {
            if (!string.IsNullOrEmpty(name) && _currentResult >= 0 && _currentResult < _results.Length)
            {
                name = name?.TrimStart('[').TrimEnd(']');

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
        public object GetValue(int i)
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

        public int GetValues(object[] values)
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

        public bool IsDBNull(int i)
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

        public int FieldCount => _currentResult >= 0 && _currentResult < _results.Length
            ? _results[_currentResult].Formats.Length
            : 0;

        object IDataRecord.this[int i] => GetValue(i);

        object IDataRecord.this[string name] => GetValue(GetOrdinal(name));

        public void Dispose()
        {
            Close();
        }

        public void Close()
        {
        }

        public DataTable GetSchemaTable()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Advances the reader to the next result set.
        /// </summary>
        /// <returns>true if the reader is pointing at a record set; false otherwise.</returns>
        public bool NextResult()
        {
            _currentResult++;

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
        public bool Read()
        {
            if (_currentResult < 0)
            {
                return false;
            }

            _currentRow++;

            return _results[_currentResult].Rows.Count > _currentRow;
        }

        public int Depth => 0;
        public bool IsClosed => _currentResult >= _results.Length;
        public int RecordsAffected => _currentResult >= 0 && _currentResult < _results.Length
            ? _results[_currentResult].Rows.Count
            : 0;
    }
}
