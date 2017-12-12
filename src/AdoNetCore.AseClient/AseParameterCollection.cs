using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace AdoNetCore.AseClient
{
    /// <summary>
    /// Represents a collection of parameters associated with an <see cref="AseCommand" />. 
    /// This class cannot be inherited.
    /// </summary>
    public sealed class AseParameterCollection : IDataParameterCollection
    {
        private readonly List<AseParameter> _parameters;
        internal bool HasSendableParameters 
        {
            get 
            {
                for (var i = 0; i < _parameters.Count; i++)
                {
                    if (_parameters[i].CanSendOverTheWire)
                    {
                        return true;
                    }
                }
                return false;
            }    
            
        }
        internal IEnumerable<AseParameter> SendableParameters 
        {
            get 
            {
                for (var i = 0; i < _parameters.Count; i++)
                {
                    if (_parameters[i].CanSendOverTheWire)
                    {
                        yield return _parameters[i];
                    }
                }
            }
        }

        public bool IsFixedSize => ((IList)_parameters).IsFixedSize;

        public bool IsReadOnly => ((IList)_parameters).IsReadOnly;

        /// <summary>
        /// Represents the number of <see cref="AseParameter" /> objects in the collection.
        /// </summary>
        public int Count => _parameters.Count;

        public bool IsSynchronized => ((IList)_parameters).IsSynchronized;

        public object SyncRoot => ((IList)_parameters).SyncRoot;

        public object this[int index] { get => ((IList)_parameters)[index]; set => ((IList)_parameters)[index] = value; }

        public AseParameterCollection() 
        {
            _parameters = new List<AseParameter>();
        }

        /// <summary>
        /// Removes all items from the collection.
        /// </summary>
        public void Clear() 
        {
            _parameters.Clear();
        }
       
        /// <summary>
        /// Determines whether the specified parameter name is in this <see cref="AseParameterCollection" />.
        /// </summary>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <returns><b>true</b> if the <see cref="AseParameterCollection" /> contains the value; otherwise <b>false</b>.</returns>
        public bool Contains(string parameterName)
        {
            return IndexOf(parameterName) >= 0;
        }

        /// <summary>
        /// Determines whether the specified <see cref="AseParameter" /> is in this <see cref="AseParameterCollection" />.
        /// </summary>
        /// <param name="value">The <see cref="AseParameter" />.</param>
        /// <returns><b>true</b> if the <see cref="AseParameterCollection" /> contains the value; otherwise <b>false</b>.</returns>
        public bool Contains(object value)
        {
            return IndexOf(value) >= 0;
        }

        /// <summary>
        /// Gets the <see cref="AseParameter" /> with the specified name.
        /// </summary>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <returns>The <see cref="AseParameter" /> with the specified name.</returns>
        /// <remarks>
        /// <para>The <i>parameterName</i> is used to look up the index value in the underlying <see cref="AseParameterCollection" />. 
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
                this[parameterName] = value as AseParameter;
            }
        }

        /// <summary>
        /// Gets the <see cref="AseParameter" /> with the specified name.
        /// </summary>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <returns>The <see cref="AseParameter" /> with the specified name.</returns>
        /// <remarks>
        /// <para>The <i>parameterName</i> is used to look up the index value in the underlying <see cref="AseParameterCollection" />. 
        /// If the <i>parameterName</i> is not valid, an <see cref="IndexOutOfRangeException" /> will be thrown.</para>
        /// </remarks>
        public AseParameter this[string parameterName]
        {
            get
            {
                var index = IndexOf(parameterName);
                if (index < 0)
                {
                    return null;
                }
                return _parameters[index];
            }
            set
            {
                var index = IndexOf(parameterName);
                if (index < 0)
                {
                    Add(value);
                }
                else
                {
                    _parameters[index] = value;
                }
            }
        }

        /// <summary>
        /// Adds an <see cref="AseParameter" /> to the <see cref="AseParameterCollection" /> given the parameter name and the data type.
        /// </summary>
        /// <param name="parameterName">The name of the parameter to add.</param>
        /// <param name="dbType">The <see cref="AseDbType" /> of the parameter to add.</param>
        /// <returns>A new <see cref="AseParameter" /> object.</returns>
        public AseParameter Add(string parameterName, AseDbType dbType) 
        {
            var parameter = new AseParameter(parameterName, dbType);

            return Add(parameter);
        }
        
        /// <summary>
        /// Adds an <see cref="AseParameter" /> to the <see cref="AseParameterCollection" /> given the parameter name and the data type.
        /// </summary>
        /// <param name="parameterName">The name of the parameter to add.</param>
        /// <param name="dbType">The <see cref="AseDbType" /> of the parameter to add.</param>
        /// <param name="size">The size as <see cref="Int32" />.</param>
        /// <returns>A new <see cref="AseParameter" /> object.</returns>
        /// <remarks>This overload is useful when you are adding a parameter of a variable-length data type such as <b>varchar</b> or <b>binary</b>.</remarks>
        public AseParameter Add(string parameterName, AseDbType dbType, int size) 
        {
            var parameter = new AseParameter(parameterName, dbType, size);
            
            return Add(parameter);
        }

        /// <summary>
        /// Adds an <see cref="AseParameter" /> to the <see cref="AseParameterCollection" /> given the parameter name and the data type.
        /// </summary>
        /// <param name="parameterName">The name of the parameter to add.</param>
        /// <param name="dbType">The <see cref="AseDbType" /> of the parameter to add.</param>
        /// <param name="size">The size as <see cref="Int32" />.</param>
        /// <param name="sourceColumn">The name of the source column.</param>
        /// <returns>A new <see cref="AseParameter" /> object.</returns>
        /// <remarks>This overload is useful when you are adding a parameter of a variable-length data type such as <b>varchar</b> or <b>binary</b>.</remarks>
        public AseParameter Add(string parameterName, AseDbType dbType, int size, string sourceColumn)
        {
            var parameter = new AseParameter(parameterName, dbType, size, sourceColumn);

            return Add(parameter);
        }

        /// <summary>
        /// Adds an <see cref="AseParameter" /> to the <see cref="AseParameterCollection" /> given the parameter name and the data type.
        /// </summary>
        /// <param name="parameterName">The name of the parameter to add.</param>
        /// <param name="dbType">The <see cref="AseDbType" /> of the parameter to add.</param>
        /// <param name="size">The size as <see cref="Int32" />.</param>
        /// <param name="direction">One of the <see cref="ParameterDirection" /> values.</param>
        /// <param name="isNullable"> true if the value of the field can be null; otherwise, false.</param>
        /// <param name="precision">The total number of digits to the left and right of the decimal point to which Value is resolved.</param>
        /// <param name="scale">The total number of decimal places to which Value is resolved.</param>
        /// <param name="sourceColumn">The name of the source column.</param>
        /// <param name="sourceVersion">One of the <see cref="DataRowVersion" /> values.</param>
        /// <param name="value">An object that is the value of the parameter.</param>
        /// <returns>A new <see cref="AseParameter" /> object.</returns>
        /// <remarks>This overload is useful when you are adding a parameter of a variable-length data type such as <b>varchar</b> or <b>binary</b>.</remarks>
        public AseParameter Add(string parameterName, AseDbType dbType, int size, ParameterDirection direction, Boolean isNullable, Byte precision, Byte scale, string sourceColumn, DataRowVersion sourceVersion, object value) 
        {
            var parameter = new AseParameter(
                parameterName, 
                dbType, 
                size, 
                direction,
                isNullable,
                precision,
                scale,
                sourceColumn,
                sourceVersion,
                value
            );

            return Add(parameter);
        }

        /// <summary>
        /// Adds an <see cref="AseParameter" /> to the <see cref="AseParameterCollection" />.
        /// </summary>
        /// <param name="parameter">The <see cref="AseParameter" /> to add.</param>
        /// <returns>A new <see cref="AseParameter" /> object.</returns>
        public AseParameter Add(AseParameter parameter) 
        {
            if(parameter != null)
            {
                _parameters.Add(parameter);
            }
            return parameter;
        }

        /// <summary>
        /// Adds an <see cref="AseParameter" /> to the <see cref="AseParameterCollection" />.
        /// </summary>
        /// <param name="parameter">The <see cref="AseParameter" /> to add.</param>
        /// <returns>A new <see cref="AseParameter" /> object.</returns>
        public AseParameter Add(object parameter) 
        {
            var p = parameter as AseParameter;

            return Add(parameter as AseParameter);
        }

        /// <summary>
        /// Adds a value to the end of the <see cref="AseParameterCollection" />.
        /// </summary>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <param name="value">The value to be added. Use <see cref="DBNull.Value" /> instead of null, to indicate a null value.</param>
        /// <returns>An <see cref="AseParameter" /> object.</returns>
        public AseParameter Add(string parameterName, object value) 
        {
            var parameter = new AseParameter(parameterName, value);
            
            return Add(parameter);
        }

        int IList.Add(object value)
        {
            var p = value as AseParameter;

            if(p != null)
            {
                 return ((IList)_parameters).Add(value);
            }
            return -1;
        }        

        /// <summary>
        /// Gets the location of the specified <see cref="AseParameter" /> with the specified name.
        /// </summary>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <returns>The zero-based location of the specified <see cref="AseParameter" /> with the specified case-sensitive name. 
        /// Returns -1 when the object does not exist in the <see cref="AseParameterCollection" />.</returns>
        public int IndexOf(string parameterName)
        {
            if (string.IsNullOrWhiteSpace(parameterName))
            {
                return -1;
            }

            for (var i = 0; i < _parameters.Count; i++)
            {
                if (string.Equals(parameterName, _parameters[i].ParameterName, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Gets the location of the specified <see cref="AseParameter" />.
        /// </summary>
        /// <param name="value">The <see cref="AseParameter" />.</param>
        /// <returns>The zero-based location of the specified <see cref="AseParameter" />. 
        /// Returns -1 when the object does not exist in the <see cref="AseParameterCollection" />.</returns>
        public int IndexOf(object value)
        {
            var p = value as AseParameter;

            if(p != null) 
            {
                return IndexOf(p.ParameterName);
            }

            return -1;
        }

        /// <summary>
        /// Inserts an <see cref="AseParameter" /> in the collection at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index where the parameter is to be inserted within the collection.</param>
        /// <param name="value">The <see cref="AseParameter" /> object to add to the collection.</param>
        public void Insert(int index, object value)
        {
            ((IList)_parameters).Insert(index, value);
        }

        /// <summary>
        /// Removes the <see cref="AseParameter" /> from the <see cref="AseParameterCollection" />.
        /// </summary>
        /// <param name="value">The <see cref="AseParameter" /> object to remove from the collection.</param>
        public void Remove(object value)
        {
            ((IList)_parameters).Remove(value);
        }

        /// <summary>
        /// Removes the <see cref="AseParameter" /> from the <see cref="AseParameterCollection" /> at the specified parameter name.
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
        /// Removes the <see cref="AseParameter" /> from the <see cref="AseParameterCollection" /> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the parameter to remove.</param>
        public void RemoveAt(int index)
        {
            _parameters.RemoveAt(index);
        }

        /// <summary>
        /// Copies <see cref="AseParameter" /> objects from the <see cref="AseParameterCollection" /> to the specified array.
        /// </summary>
        /// <param name="array">The array into which to copy the AseParameter objects.</param>
        /// <param name="index">The starting index of the array.</param>
        public void CopyTo(Array array, int index)
        {
            ((IList)_parameters).CopyTo(array, index);
        }

        /// <summary>
        /// Enumerates the <see cref="AseParameter" /> objects.
        /// </summary>
        /// <returns>The <see cref="AseParameter" /> objects.</returns>
        public IEnumerator GetEnumerator()
        {
            return ((IList)_parameters).GetEnumerator();
        }
    }
}
