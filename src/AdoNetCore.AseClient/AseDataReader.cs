using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient
{
    public sealed class AseDataReader : DbDataReader
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

        public override bool GetBoolean(int i)
        {
            throw new NotImplementedException();
        }

        public override byte GetByte(int i)
        {
            throw new NotImplementedException();
        }

        public override long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public override char GetChar(int i)
        {
            throw new NotImplementedException();
        }

        public override long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public override string GetDataTypeName(int i)
        {
            throw new NotImplementedException();
        }

        public override IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public override DateTime GetDateTime(int i)
        {
            throw new NotImplementedException();
        }

        public override decimal GetDecimal(int i)
        {
            throw new NotImplementedException();
        }

        public override double GetDouble(int i)
        {
            throw new NotImplementedException();
        }

        public override Type GetFieldType(int i) => typeof(object);

        public override float GetFloat(int i)
        {
            throw new NotImplementedException();
        }

        public override Guid GetGuid(int i)
        {
            throw new NotImplementedException();
        }

        public override short GetInt16(int i)
        {
            throw new NotImplementedException();
        }

        public override int GetInt32(int i)
        {
            throw new NotImplementedException();
        }

        public override long GetInt64(int i)
        {
            throw new NotImplementedException();
        }

        public override string GetName(int i)
        {
            if (_currentResult >= 0
                && _currentResult < _results.Length
                && i < _results[_currentResult].Formats.Length)
            {
                return _results[_currentResult].Formats[i].ColumnName;
            }

            throw new ArgumentException();
        }

        public override int GetOrdinal(string name)
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

        public override string GetString(int i)
        {
            throw new NotImplementedException();
        }

        public override object GetValue(int i)
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

        public override int GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        public override bool IsDBNull(int i)
        {
            throw new NotImplementedException();
        }

        public override int FieldCount => _currentResult >= 0 && _currentResult < _results.Length
            ? _results[_currentResult].Formats.Length
            : 0;

        public override bool HasRows => _results.Length > 0;

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
            throw new NotImplementedException();
        }
#else
        public override DataTable GetSchemaTable()
        {
            throw new NotImplementedException();
        }
#endif

        public override bool NextResult()
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

        public override bool Read()
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

        public override int Depth => 0;
        public override bool IsClosed => _currentResult >= _results.Length;
        public override int RecordsAffected => _currentResult >= 0 && _currentResult < _results.Length
            ? _results[_currentResult].Rows.Count
            : 0;
    }
}
