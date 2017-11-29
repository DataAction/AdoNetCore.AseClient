using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace AdoNetCore.AseClient
{
    public sealed class AseDataParameterCollection : List<AseDataParameter>, IDataParameterCollection
    {
        internal bool HasSendableParameters => this.Any(p => p.CanSendOverTheWire);
        internal IEnumerable<AseDataParameter> SendableParameters => this.Where(p => p.CanSendOverTheWire);
       
        public bool Contains(string parameterName)
        {
            return IndexOf(parameterName) >= 0;
        }

        public int IndexOf(string parameterName)
        {
            if (string.IsNullOrWhiteSpace(parameterName))
            {
                return -1;
            }

            for (var i = 0; i < Count; i++)
            {
                if (string.Equals(parameterName, this[i].ParameterName, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return -1;
        }

        public void RemoveAt(string parameterName)
        {
            var index = IndexOf(parameterName);
            if (index < 0)
            {
                return;
            }

            RemoveAt(index);
        }

        public object this[string parameterName]
        {
            get
            {
                var index = IndexOf(parameterName);
                if (index < 0)
                {
                    return null;
                }
                return this[index];
            }
            set
            {
                var index = IndexOf(parameterName);
                if (index < 0)
                {
                    Add((AseDataParameter)value);
                }
                else
                {
                    this[index] = (AseDataParameter)value;
                }
            }
        }
    }
}
