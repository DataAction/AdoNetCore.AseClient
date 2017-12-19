using System;
using System.Collections;

namespace AdoNetCore.AseClient
{
    /// <summary>
    /// <see cref="IEnumerator"/> implementation for a an <see cref="AseDataReader"/>.
    /// </summary>
    internal sealed class AseDataReaderEnumerator : IEnumerator
    {
        private readonly AseDataReader _dataReader;

        public AseDataReaderEnumerator(AseDataReader dataReader)
        {
            this._dataReader = dataReader;
        }

        public void Reset()
        {
            throw new NotSupportedException($"{nameof(AseDataReaderEnumerator)}.{nameof(Reset)}");
        }

        public object Current
        {
            get
            {
                var values = new object[_dataReader.FieldCount];

                _dataReader.GetValues(values);

                return values;
            }
        }

        public bool MoveNext()
        {
            return _dataReader.Read();
        }
    }
}