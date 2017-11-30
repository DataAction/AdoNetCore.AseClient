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
            throw new NotImplementedException();
        }

        public byte GetByte(int i)
        {
            throw new NotImplementedException();
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public char GetChar(int i)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public decimal GetDecimal(int i)
        {
            throw new NotImplementedException();
        }

        public double GetDouble(int i)
        {
            throw new NotImplementedException();
        }

        public Type GetFieldType(int i) => typeof(object);

        public float GetFloat(int i)
        {
            throw new NotImplementedException();
        }

        public Guid GetGuid(int i)
        {
            throw new NotImplementedException();
        }

        public short GetInt16(int i)
        {
            throw new NotImplementedException();
        }

        public int GetInt32(int i)
        {
            throw new NotImplementedException();
        }

        public long GetInt64(int i)
        {
            throw new NotImplementedException();
        }

        public string GetName(int i)
        {
            if (_currentResult >= 0
                && _currentResult < _results.Length
                && i < _results[_currentResult].Formats.Length)
            {
                return _results[_currentResult].Formats[i].ColumnName;
            }

            throw new ArgumentException();
        }

        public int GetOrdinal(string name)
        {
            if (_currentResult >= 0 && _currentResult < _results.Length)
            {
                var formats = _results[_currentResult].Formats;
                for (var i = 0; i < formats.Length; i++)
                {
                    if (string.Equals(formats[i].ColumnName, name))
                    {
                        return i;
                    }
                }
            }

            throw new ArgumentException();
        }

        public string GetString(int i)
        {
            throw new NotImplementedException();
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

            throw new ArgumentException();
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
        
        public void Dispose() { }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public DataTable GetSchemaTable()
        {
            throw new NotImplementedException();
        }

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

        public bool Read()
        {
            if (_currentResult < 0)
            {
                return false;
            }

            _currentRow++;

            if (_results[_currentResult].Rows.Count > _currentRow)
            {
                return true;
            }

            return false;
        }

        public int Depth => 0;
        public bool IsClosed => _currentResult >= _results.Length;
        public int RecordsAffected => _currentResult >= 0 && _currentResult < _results.Length
            ? _results[_currentResult].Rows.Count
            : 0;
    }
}
