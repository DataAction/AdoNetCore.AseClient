using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;

namespace AdoNetCore.AseClient
{
    /// <summary>
    /// Represents a collection of parameters associated with an <see cref="AseCommand" />. 
    /// This class cannot be inherited.
    /// </summary>
    [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
    public sealed class AseParameterCollection : DbParameterCollection
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly List<AseParameter> _parameters;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal bool HasSendableParameters
        {
            get
            {
                foreach (var p in _parameters)
                {
                    if (p.CanSendOverTheWire)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal IEnumerable<AseParameter> SendableParameters
        {
            get
            {
                foreach (var p in _parameters)
                {
                    if (p.CanSendOverTheWire)
                    {
                        yield return p;
                    }
                }
            }
        }

#if SYSTEM_DATA_COMMON_EXTENSIONS
        public override bool IsFixedSize => ((IList)_parameters).IsFixedSize;
        public override bool IsReadOnly => ((IList)_parameters).IsReadOnly;
        public override bool IsSynchronized => ((IList)_parameters).IsSynchronized;
#else
        public bool IsFixedSize => ((IList)_parameters).IsFixedSize;
        public bool IsReadOnly => ((IList)_parameters).IsReadOnly;
        public bool IsSynchronized => ((IList)_parameters).IsSynchronized;
#endif

        /// <summary>
        /// Represents the number of <see cref="AseParameter" /> objects in the collection.
        /// </summary>
        public override int Count => _parameters.Count;

        public override object SyncRoot => ((IList)_parameters).SyncRoot;

        protected override DbParameter GetParameter(int index)
        {
            return _parameters[index];
        }

        protected override void SetParameter(int index, DbParameter value)
        {
            _parameters[index] = (AseParameter)value;
        }

        public AseParameterCollection()
        {
            _parameters = new List<AseParameter>();
        }

        /// <summary>
        /// Removes all items from the collection.
        /// </summary>
        public override void Clear()
        {
            _parameters.Clear();
        }

        /// <summary>
        /// Determines whether the specified parameter name is in this <see cref="AseParameterCollection" />.
        /// </summary>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <returns><b>true</b> if the <see cref="AseParameterCollection" /> contains the value; otherwise <b>false</b>.</returns>
        public override bool Contains(string parameterName)
        {
            return IndexOf(parameterName) >= 0;
        }

        /// <summary>
        /// Determines whether the specified <see cref="AseParameter" /> is in this <see cref="AseParameterCollection" />.
        /// </summary>
        /// <param name="value">The <see cref="AseParameter" />.</param>
        /// <returns><b>true</b> if the <see cref="AseParameterCollection" /> contains the value; otherwise <b>false</b>.</returns>
        public override bool Contains(object value)
        {
            return IndexOf(value) >= 0;
        }

        /// <summary>
        /// Determines whether the specified <see cref="AseParameter" /> is in this <see cref="AseParameterCollection" />.
        /// </summary>
        /// <param name="value">The <see cref="AseParameter" />.</param>
        /// <returns><b>true</b> if the <see cref="AseParameterCollection" /> contains the value; otherwise <b>false</b>.</returns>
        public bool Contains(AseParameter value)
        {
            return IndexOf(value) >= 0;
        }

        public new AseParameter this[string parameterName]
        {
            get => (AseParameter)GetParameter(parameterName);
            set => SetParameter(parameterName, value);
        }

        public new AseParameter this[int index]
        {
            get => (AseParameter)GetParameter(index);
            set => SetParameter(index, value);
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
        protected override DbParameter GetParameter(string parameterName)
        {
            var index = IndexOf(parameterName);
            if (index < 0)
            {
                return null;
            }
            return _parameters[index];
        }

        /// <summary>
        /// Sets the <see cref="AseParameter" /> with the specified name.
        /// </summary>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <param name="value">The parameter.</param>
        /// <remarks>
        /// <para>The <i>parameterName</i> is used to look up the index value in the underlying <see cref="AseParameterCollection" />. 
        /// If the <i>parameterName</i> is not valid, an <see cref="IndexOutOfRangeException" /> will be thrown.</para>
        /// </remarks>
        protected override void SetParameter(string parameterName, DbParameter value)
        {
            var index = IndexOf(parameterName);
            if (index < 0)
            {
                Add(value);
            }
            else
            {
                _parameters[index] = (AseParameter)value;
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
            if (parameter != null)
            {
                _parameters.Add(parameter);
            }
            return parameter;
        }

        /// <summary>
        /// Adds an <see cref="AseParameter" /> to the <see cref="AseParameterCollection" /> given the parameter name and the data type.
        /// </summary>
        /// <param name="index">The index of the parameter to add.</param>
        /// <param name="dbType">The <see cref="AseDbType" /> of the parameter to add.</param>
        /// <returns>A new <see cref="AseParameter" /> object.</returns>
        public AseParameter Add(int index, AseDbType dbType)
        {
            return Add(new AseParameter(index, dbType));
        }

        /// <summary>
        /// Adds a value to the end of the <see cref="AseParameterCollection" />.
        /// </summary>
        /// <param name="index">The index of the parameter to add.</param>
        /// <param name="parameterValue">The value to be added. Use <see cref="DBNull.Value" /> instead of null, to indicate a null value.</param>
        /// <returns>An <see cref="AseParameter" /> object.</returns>
        public AseParameter Add(int index, object parameterValue)
        {
            return Add(new AseParameter(index, parameterValue));
        }

        /// <summary>
        /// Adds an <see cref="AseParameter" /> to the <see cref="AseParameterCollection" /> given the parameter name and the data type.
        /// </summary>
        /// <param name="index">The index of the parameter to add.</param>
        /// <param name="dbType">The <see cref="AseDbType" /> of the parameter to add.</param>
        /// <param name="size">The size as <see cref="Int32" />.</param>
        /// <returns>A new <see cref="AseParameter" /> object.</returns>
        /// <remarks>This overload is useful when you are adding a parameter of a variable-length data type such as <b>varchar</b> or <b>binary</b>.</remarks>
        public AseParameter Add(int index, AseDbType dbType, int size)
        {
            return Add(new AseParameter(index, dbType, size));
        }

        /// <summary>
        /// Adds an <see cref="AseParameter" /> to the <see cref="AseParameterCollection" /> given the parameter name and the data type.
        /// </summary>
        /// <param name="index">The index of the parameter to add.</param>
        /// <param name="dbType">The <see cref="AseDbType" /> of the parameter to add.</param>
        /// <param name="size">The size as <see cref="Int32" />.</param>
        /// <param name="sourceColumn">The name of the source column.</param>
        /// <returns>A new <see cref="AseParameter" /> object.</returns>
        /// <remarks>This overload is useful when you are adding a parameter of a variable-length data type such as <b>varchar</b> or <b>binary</b>.</remarks>
        public AseParameter Add(int index, AseDbType dbType, int size, string sourceColumn)
        {
            return Add(new AseParameter(index, dbType, size, sourceColumn));
        }

        /// <summary>
        /// Adds an <see cref="Array"/> of <see cref="AseParameter"/> to the <see cref="AseParameterCollection"/>.
        /// </summary>
        /// <param name="values">The items to add.</param>
        public override void AddRange(Array values)
        {
            foreach (var obj in values)
            {
                Add(obj);
            }
        }

        /// <summary>
        /// Adds an array of <see cref="AseParameter"/> to the <see cref="AseParameterCollection"/>.
        /// </summary>
        /// <param name="values">The items to add.</param>
        public void AddRange(AseParameter[] values)
        {
            foreach (var obj in values)
            {
                Add(obj);
            }
        }

        /// <summary>
        /// Adds a value to the end of the <see cref="AseParameterCollection" />.
        /// </summary>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <param name="parameterValue">The value to be added. Use <see cref="DBNull.Value" /> instead of null, to indicate a null value.</param>
        /// <returns>An <see cref="AseParameter" /> object.</returns>
        public AseParameter Add(string parameterName, object parameterValue)
        {
            var parameter = new AseParameter(parameterName, parameterValue);

            return Add(parameter);
        }

        /// <summary>
        /// Adds a value to the end of the <see cref="AseParameterCollection" />.
        /// </summary>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <param name="parameterValue">The value to be added. Use <see cref="DBNull.Value" /> instead of null, to indicate a null value.</param>
        /// <returns>An <see cref="AseParameter" /> object.</returns>
        // ReSharper disable once UnusedMember.Global
        public AseParameter AddWithValue(string parameterName, object parameterValue)
        {
            var parameter = new AseParameter(parameterName, parameterValue);

            return Add(parameter);
        }

        /// <summary>
        /// Adds an <see cref="AseParameter" /> to the <see cref="AseParameterCollection" />.
        /// </summary>
        /// <param name="value">The <see cref="AseParameter" /> to add.</param>
        /// <returns>The index of the <see cref="AseParameter" /> object.</returns>
        public override int Add(object value)
        {
            if (value is AseParameter p)
            {
                return ((IList)_parameters).Add(p);
            }
            return -1;
        }

        /// <summary>
        /// Gets the location of the specified <see cref="AseParameter" /> with the specified name.
        /// </summary>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <returns>The zero-based location of the specified <see cref="AseParameter" /> with the specified case-sensitive name. 
        /// Returns -1 when the object does not exist in the <see cref="AseParameterCollection" />.</returns>
        public override int IndexOf(string parameterName)
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
        public override int IndexOf(object value)
        {
            if (value is AseParameter p)
            {
                for (var i = 0; i < _parameters.Count; i++)
                {
                    if (p == _parameters[i])
                    {
                        return i;
                    }
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
        public int IndexOf(AseParameter value)
        {
            if (value != null)
            {
                for (var i = 0; i < _parameters.Count; i++)
                {
                    if (value == _parameters[i])
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        /// <summary>
        /// Inserts an <see cref="AseParameter" /> in the collection at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index where the parameter is to be inserted within the collection.</param>
        /// <param name="parameter">The <see cref="AseParameter" /> object to add to the collection.</param>
        public override void Insert(int index, object parameter)
        {
            ((IList)_parameters).Insert(index, parameter);
        }

        /// <summary>
        /// Inserts an <see cref="AseParameter" /> in the collection at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index where the parameter is to be inserted within the collection.</param>
        /// <param name="parameter">The <see cref="AseParameter" /> object to add to the collection.</param>
        public void Insert(int index, AseParameter parameter)
        {
            ((IList)_parameters).Insert(index, parameter);
        }

        /// <summary>
        /// Removes the <see cref="AseParameter" /> from the <see cref="AseParameterCollection" />.
        /// </summary>
        /// <param name="value">The <see cref="AseParameter" /> object to remove from the collection.</param>
        public override void Remove(object value)
        {
            ((IList)_parameters).Remove(value);
        }

        /// <summary>
        /// Removes the <see cref="AseParameter" /> from the <see cref="AseParameterCollection" />.
        /// </summary>
        /// <param name="value">The <see cref="AseParameter" /> object to remove from the collection.</param>
        public void Remove(AseParameter value)
        {
            ((IList)_parameters).Remove(value);
        }

        /// <summary>
        /// Removes the <see cref="AseParameter" /> from the <see cref="AseParameterCollection" /> at the specified parameter name.
        /// </summary>
        /// <param name="parameterName">The name of the parameter to remove.</param>
        public override void RemoveAt(string parameterName)
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
        public override void RemoveAt(int index)
        {
            _parameters.RemoveAt(index);
        }

        /// <summary>
        /// Copies <see cref="AseParameter" /> objects from the <see cref="AseParameterCollection" /> to the specified array.
        /// </summary>
        /// <param name="array">The array into which to copy the AseParameter objects.</param>
        /// <param name="index">The starting index of the array.</param>
        public override void CopyTo(Array array, int index)
        {
            if (array != null)
            {
                ((IList)_parameters).CopyTo(array, index);
            }
        }

        /// <summary>
        /// Copies <see cref="AseParameter" /> objects from the <see cref="AseParameterCollection" /> to the specified array.
        /// </summary>
        /// <param name="array">The array into which to copy the AseParameter objects.</param>
        /// <param name="index">The starting index of the array.</param>
        public void CopyTo(AseParameter[] array, int index)
        {
            ((IList)_parameters).CopyTo(array, index);
        }

        /// <summary>
        /// Enumerates the <see cref="AseParameter" /> objects.
        /// </summary>
        /// <returns>The <see cref="AseParameter" /> objects.</returns>
        public override IEnumerator GetEnumerator()
        {
            return ((IList)_parameters).GetEnumerator();
        }
    }
}
