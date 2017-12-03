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

        /// <summary>
        /// Adds an <see cref="AseDataParameter" /> to the <see cref="AseDataParameterCollection" /> given the parameter name and the data type.
        /// </summary>
        /// <param name="parameterName">The name of the parameter to add.</param>
        /// <param name="dbType">The <see cref="DbType" /> of the parameter to add.</param>
        /// <returns>A new <see cref="AseDataParameter" /> object.</returns>
        public AseDataParameter Add(string parameterName, DbType dbType) 
        {
            var parameter = new AseDataParameter(){ParameterName = parameterName, DbType = dbType, Direction = ParameterDirection.Input};
            
            Add(parameter);

            return parameter;
        }
        
        /// <summary>
        /// Adds an <see cref="AseDataParameter" /> to the <see cref="AseDataParameterCollection" /> given the parameter name and the data type.
        /// </summary>
        /// <param name="parameterName">The name of the parameter to add.</param>
        /// <param name="dbType">The <see cref="DbType" /> of the parameter to add.</param>
        /// <param name="size">The size as <see cref="Int32" />.</param>
        /// <returns>A new <see cref="AseDataParameter" /> object.</returns>
        /// <remarks>This overload is useful when you are adding a parameter of a variable-length data type such as <b>varchar</b> or <b>binary</b>.</remarks>
        public AseDataParameter Add(string parameterName, DbType dbType, int size) 
        {
            var parameter = new AseDataParameter(){ParameterName = parameterName, DbType = dbType, Size = size, Direction = ParameterDirection.Input};
            
            Add(parameter);

            return parameter;
        }

        

        /// <summary>
        /// Adds a value to the end of the <see cref="AseDataParameterCollection" />.
        /// </summary>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <param name="value">The value to be added. Use <see cref="DBNull.Value" /> instead of null, to indicate a null value.</param>
        /// <returns>An <see cref="AseDataParameter" /> object.</returns>
        public AseDataParameter AddWithValue(string parameterName, object value) 
        {
            if(value == null) {
                throw new ArgumentNullException();
            }

            var parameter = new AseDataParameter(){ParameterName = parameterName, Value = value, Direction = ParameterDirection.Input};
            if(value == DBNull.Value) 
            {
                // Do nothing.
            } 
            else if(value is int) 
            {
                parameter.DbType = DbType.Int32;
            } 
            else if(value is short) 
            {
                parameter.DbType = DbType.Int16;
            } 
            else if(value is long) 
            {
                parameter.DbType = DbType.Int64;
            } 
            else if(value is uint) 
            {
                parameter.DbType = DbType.UInt32;
            } 
            else if(value is ushort) 
            {
                parameter.DbType = DbType.UInt16;
            } 
            else if(value is ulong) 
            {
                parameter.DbType = DbType.UInt64;
            } 
            else if(value is byte) 
            {
                parameter.DbType = DbType.Byte;
            } 
            else if(value is sbyte) 
            {
                parameter.DbType = DbType.SByte;
            } 
            else if(value is bool) 
            {
                parameter.DbType = DbType.Boolean;
            } 
            else if(value is float) 
            {
                parameter.DbType = DbType.Single;
            } 
            else if(value is double) 
            {
                parameter.DbType = DbType.Double;
            } 
            else if(value is decimal) 
            {
                parameter.DbType = DbType.Decimal;
            } 
            else if(value is Guid) 
            {
                parameter.DbType = DbType.Binary;
                parameter.Size = 16;
            } 
            else if(value is DateTime) 
            {
                parameter.DbType = DbType.DateTime;
            } 
            else if(value is byte[]) 
            {
                parameter.DbType = DbType.Binary;
            } 
            else if(value is char) 
            {
                parameter.DbType = DbType.StringFixedLength;
                parameter.Size = 1;
            } 
            else if(value is char[]) 
            {
                parameter.DbType = DbType.StringFixedLength;
                parameter.Size = ((char[])value).Length;
            } 
            else if(value is string) 
            {
                parameter.DbType = DbType.String;
            }  
            else // AnsiString, AnsiStringFixedLength, Xml, Date, DateTime2, DateTimeOffset, Time, VarNumeric, Guid, Currency, Object.
            {
                throw new NotSupportedException("Inference of data type not supported, add the parameter explicitly.");
            }
            
            Add(parameter);

            return parameter;
        }
    }
}
