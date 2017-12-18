using System;
using System.Data;
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
            return GetValue<bool>(i);
        }

        public byte GetByte(int i)
        {
            return GetValue<byte>(i);
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public char GetChar(int i)
        {
            return GetValue<char>(i);
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
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
            return GetValue<DateTime>(i);
        }

        public decimal GetDecimal(int i)
        {
            return GetValue<decimal>(i);
        }

        public double GetDouble(int i)
        {
            return GetValue<double>(i);
        }

        public Type GetFieldType(int i) => typeof(object);

        public float GetFloat(int i)
        {
            return GetValue<float>(i);
        }

        public Guid GetGuid(int i)
        {
            throw new NotImplementedException();
        }

        public short GetInt16(int i)
        {
            return GetValue<short>(i);
        }

        public int GetInt32(int i)
        {
            return GetValue<int>(i);
        }

        public long GetInt64(int i)
        {
            return GetValue<long>(i);
        }

        public string GetString(int i)
        {
            return GetValue<string>(i);
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

        private T GetValue<T>(int i)
        {
            var value = GetValue(i);

            if (value is T casted)
            {
                return casted;
            }

            throw new InvalidCastException("Specified cast is not valid.");
        }

        public int GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        public bool IsDBNull(int i)
        {
            throw new NotImplementedException();
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
