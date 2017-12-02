using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace AdoNetCore.AseClient
{
    /// <summary>
    /// Represents a collection of parameters associated with an <see cref="AseCommand" />. 
    /// This class cannot be inherited.
    /// </summary>
    public sealed class AseDataParameterCollection : List<AseDataParameter>, IDataParameterCollection
    {
        internal bool HasSendableParameters => this.Any(p => p.CanSendOverTheWire);
        internal IEnumerable<AseDataParameter> SendableParameters => this.Where(p => p.CanSendOverTheWire);
       
        /// <summary>
        /// Determines whether the specified parameter name is in this <see cref="AseDataParameterCollection" />.
        /// </summary>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <returns><b>true</b> if the <see cref="AseDataParameterCollection" /> contains the value; otherwise <b>false</b>.</returns>
        public bool Contains(string parameterName)
        {
            return IndexOf(parameterName) >= 0;
        }

        /// <summary>
        /// Gets the location of the specified <see cref="AseDataParameter" /> with the specified name.
        /// </summary>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <returns>The zero-based location of the specified <see cref="AseDataParameter" /> with the specified case-sensitive name. 
        /// Returns -1 when the object does not exist in the <see cref="AseDataParameterCollection" />.</returns>
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

        /// <summary>
        /// Removes the <see cref="AseDataParameter" /> from the <see cref="AseDataParameterCollection" /> at the specified parameter name.
        /// </summary>
        /// <param name="parameterName">The name of the parameter to remove.</param>
        public void RemoveAt(string parameterName)
        {
            var index = IndexOf(parameterName);
            if (index < 0)
            {
                return;
            }

            RemoveAt(index);
        }

        /// <summary>
        /// Gets the <see cref="AseDataParameter" /> with the specified name.
        /// </summary>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <returns>The <see cref="AseDataParameter" /> with the specified name.</returns>
        /// <remarks>
        /// <para>The <i>parameterName</i> is used to look up the index value in the underlying <see cref="AseDataParameterCollection" />. 
        /// If the <i>parameterName</i> is not valid, an <see cref="IndexOutOfRangeException" /> will be thrown.</para>
        /// </remarks>
        object IDataParameterCollection.this[string parameterName]
        {
            get
            {
                return this[parameterName];
            }
            set
            {
                this[parameterName] = value as AseDataParameter;
            }
        }

        /// <summary>
        /// Gets the <see cref="AseDataParameter" /> with the specified name.
        /// </summary>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <returns>The <see cref="AseDataParameter" /> with the specified name.</returns>
        /// <remarks>
        /// <para>The <i>parameterName</i> is used to look up the index value in the underlying <see cref="AseDataParameterCollection" />. 
        /// If the <i>parameterName</i> is not valid, an <see cref="IndexOutOfRangeException" /> will be thrown.</para>
        /// </remarks>
        public AseDataParameter this[string parameterName]
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
