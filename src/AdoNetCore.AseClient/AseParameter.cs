using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient
{
    /// <summary>
    /// Represents a parameter to an <see cref="AseCommand" />. This class cannot be inherited.
    /// </summary>
    public sealed class AseParameter : DbParameter
#if ENABLE_CLONEABLE_INTERFACE
        , ICloneable
#endif
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string _parameterName;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int _parameterIndex;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private AseDbType _type;

        /// <summary>
        /// Constructor function for an <see cref="AseParameter" />instance.
        /// </summary>
        public AseParameter()
        {
            Direction = ParameterDirection.Input;
            SourceVersion = DataRowVersion.Default;
            IsNullable = false;
        }

        /// <summary>
        /// Constructor function for an <see cref="AseParameter" />instance.
        /// </summary>
        /// <param name="parameterName">The name of the parameter to add.</param>
        /// <param name="value">An object that is the value of the parameter.</param>
        public AseParameter(string parameterName, object value) : this()
        {
            Value = value ?? throw new ArgumentNullException($"Value for parameter '{parameterName}' cannot be null! If you meant to insert NULL, use DbNull.Value instead.");
            ParameterName = parameterName;
        }

        /// <summary>
        /// Constructor function for an <see cref="AseParameter" />instance.
        /// </summary>
        /// <param name="parameterName">The name of the parameter to add.</param>
        /// <param name="dbType">The <see cref="AseDbType" /> of the parameter to add.</param>
        public AseParameter(string parameterName, AseDbType dbType) : this()
        {
            ParameterName = parameterName;
            AseDbType = dbType;

            if (dbType == AseDbType.Money)
            {
                Precision = 20;
                Scale = 4;
            }
            else
            {
                if (dbType != AseDbType.SmallMoney)
                {
                    return;
                }
                Precision = 12;
                Scale = 4;
            }
        }

        /// <summary>
        /// Constructor function for an <see cref="AseParameter" />instance.
        /// </summary>
        /// <param name="parameterName">The name of the parameter to add.</param>
        /// <param name="dbType">The <see cref="AseDbType" /> of the parameter to add.</param>
        /// <param name="size">The size as <see cref="int" />.</param>
        public AseParameter(string parameterName, AseDbType dbType, int size) : this()
        {
            ParameterName = parameterName;
            AseDbType = dbType;
            Size = size;
        }

        /// <summary>
        /// Constructor function for an <see cref="AseParameter" />instance.
        /// </summary>
        /// <param name="parameterName">The name of the parameter to add.</param>
        /// <param name="dbType">The <see cref="AseDbType" /> of the parameter to add.</param>
        /// <param name="size">The size as <see cref="int" />.</param>
        /// <param name="sourceColumn">The name of the source column.</param>
        public AseParameter(string parameterName, AseDbType dbType, int size, string sourceColumn) : this()
        {
            ParameterName = parameterName;
            AseDbType = dbType;
            Size = size;
            SourceColumn = sourceColumn;
        }

        /// <summary>
        /// Constructor function for an <see cref="AseParameter" />instance.
        /// </summary>
        /// <param name="parameterName">The name of the parameter to add.</param>
        /// <param name="dbType">The <see cref="AseDbType" /> of the parameter to add.</param>
        /// <param name="size">The size as <see cref="int" />.</param>
        /// <param name="direction">One of the <see cref="ParameterDirection" /> values.</param>
        /// <param name="isNullable"> true if the value of the field can be null; otherwise, false.</param>
        /// <param name="precision">The total number of digits to the left and right of the decimal point to which Value is resolved.</param>
        /// <param name="scale">The total number of decimal places to which Value is resolved.</param>
        /// <param name="sourceColumn">The name of the source column.</param>
        /// <param name="sourceVersion">One of the <see cref="DataRowVersion" /> values.</param>
        /// <param name="value">An object that is the value of the parameter.</param>
        public AseParameter(string parameterName, AseDbType dbType, int size, ParameterDirection direction, bool isNullable, byte precision, byte scale,
            string sourceColumn, DataRowVersion sourceVersion, object value) : this()
        {
            ParameterName = parameterName;
            AseDbType = dbType;
            Size = size;
            Direction = direction;
            IsNullable = isNullable;
            Precision = precision;
            Scale = scale;
            SourceColumn = sourceColumn;
            SourceVersion = sourceVersion;
            Value = value;
        }
        
        /// <summary>
        /// Constructor function for an <see cref="AseParameter" />instance.
        /// </summary>
        /// <param name="parameterName">The name of the parameter to add.</param>
        /// <param name="dbType">The <see cref="AseDbType" /> of the parameter to add.</param>
        /// <param name="size">The size as <see cref="int" />.</param>
        /// <param name="direction">One of the <see cref="ParameterDirection" /> values.</param>
        /// <param name="isNullable"> true if the value of the field can be null; otherwise, false.</param>
        /// <param name="precision">The total number of digits to the left and right of the decimal point to which Value is resolved.</param>
        /// <param name="scale">The total number of decimal places to which Value is resolved.</param>
        /// <param name="sourceColumn">The name of the source column.</param>
        /// <param name="sourceVersion">One of the <see cref="DataRowVersion" /> values.</param>
        /// <param name="value">An object that is the value of the parameter.</param>
        public AseParameter(string parameterName, AseDbType dbType, int size, ParameterDirection direction, bool isNullable, int precision, int scale,
            string sourceColumn, DataRowVersion sourceVersion, object value) : this()
        {
            ParameterName = parameterName;
            AseDbType = dbType;
            Size = size;
            Direction = direction;
            IsNullable = isNullable;
            Precision = (byte)precision;
            Scale = (byte)scale;
            SourceColumn = sourceColumn;
            SourceVersion = sourceVersion;
            Value = value;
        }

        /// <summary>
        /// Constructor function for an <see cref="AseParameter" />instance.
        /// </summary>
        /// <param name="parameterName">The name of the parameter to add.</param>
        /// <param name="dbType">The <see cref="AseDbType" /> of the parameter to add.</param>
        /// <param name="size">The size as <see cref="int" />.</param>
        /// <param name="isNullable"> true if the value of the field can be null; otherwise, false.</param>
        /// <param name="precision">The total number of digits to the left and right of the decimal point to which Value is resolved.</param>
        /// <param name="scale">The total number of decimal places to which Value is resolved.</param>
        public AseParameter(string parameterName, AseDbType dbType, int size, bool isNullable, int precision, int scale) : this()
        {
            ParameterName = parameterName;
            AseDbType = dbType;
            Size = size;
            IsNullable = isNullable;
            Precision = (byte)precision;
            Scale = (byte)scale;
        }

        /// <summary>
        /// Constructor function for an <see cref="AseParameter" />instance.
        /// </summary>
        /// <param name="parameterIndex">The index of the parameter to add.</param>
        /// <param name="dbType">The <see cref="AseDbType" /> of the parameter to add.</param>
        public AseParameter(int parameterIndex, AseDbType dbType) : this()
        {
            ParameterIndex = parameterIndex;
            AseDbType = dbType;
        }

        /// <summary>
        /// Constructor function for an <see cref="AseParameter" />instance.
        /// </summary>
        /// <param name="parameterIndex">The index of the parameter to add.</param>
        /// <param name="value">An object that is the value of the parameter.</param>
        public AseParameter(int parameterIndex, object value) : this()
        {
            ParameterIndex = parameterIndex;
            Value = value;
        }

        /// <summary>
        /// Constructor function for an <see cref="AseParameter" />instance.
        /// </summary>
        /// <param name="parameterIndex">The index of the parameter to add.</param>
        /// <param name="dbType">The <see cref="AseDbType" /> of the parameter to add.</param>
        /// <param name="size">The size as <see cref="int" />.</param>
        public AseParameter(int parameterIndex, AseDbType dbType, int size) : this()
        {
            ParameterIndex = parameterIndex;
            AseDbType = dbType;
            Size = size;
        }

        /// <summary>
        /// Constructor function for an <see cref="AseParameter" />instance.
        /// </summary>
        /// <param name="parameterIndex">The index of the parameter to add.</param>
        /// <param name="dbType">The <see cref="AseDbType" /> of the parameter to add.</param>
        /// <param name="size">The size as <see cref="int" />.</param>
        /// <param name="sourceColumn">The name of the source column.</param>
        public AseParameter(int parameterIndex, AseDbType dbType, int size, string sourceColumn) : this()
        {
            ParameterIndex = parameterIndex;
            AseDbType = dbType;
            Size = size;
            SourceColumn = sourceColumn;
        }

        /// <summary>
        /// Constructor function for an <see cref="AseParameter" />instance.
        /// </summary>
        /// <param name="parameterIndex">The index of the parameter to add.</param>
        /// <param name="dbType">The <see cref="AseDbType" /> of the parameter to add.</param>
        /// <param name="size">The size as <see cref="int" />.</param>
        /// <param name="direction">One of the <see cref="ParameterDirection" /> values.</param>
        /// <param name="isNullable"> true if the value of the field can be null; otherwise, false.</param>
        /// <param name="precision">The total number of digits to the left and right of the decimal point to which Value is resolved.</param>
        /// <param name="scale">The total number of decimal places to which Value is resolved.</param>
        /// <param name="sourceColumn">The name of the source column.</param>
        /// <param name="sourceVersion">One of the <see cref="DataRowVersion" /> values.</param>
        /// <param name="value">An object that is the value of the parameter.</param>
        public AseParameter(int parameterIndex, AseDbType dbType, int size, ParameterDirection direction, bool isNullable, byte precision, byte scale, string sourceColumn, DataRowVersion sourceVersion, object value) : this()
        {
            ParameterIndex = parameterIndex;
            AseDbType = dbType;
            Size = size;
            Direction = direction;
            IsNullable = isNullable;
            Precision = precision;
            Scale = scale;
            SourceColumn = sourceColumn;
            SourceVersion = sourceVersion;
            Value = value;
        }

        /// <summary>
        /// Constructor function for an <see cref="AseParameter" />instance.
        /// </summary>
        /// <param name="parameterIndex">The index of the parameter to add.</param>
        /// <param name="dbType">The <see cref="AseDbType" /> of the parameter to add.</param>
        /// <param name="size">The size as <see cref="int" />.</param>
        /// <param name="direction">One of the <see cref="ParameterDirection" /> values.</param>
        /// <param name="isNullable"> true if the value of the field can be null; otherwise, false.</param>
        /// <param name="precision">The total number of digits to the left and right of the decimal point to which Value is resolved.</param>
        /// <param name="scale">The total number of decimal places to which Value is resolved.</param>
        /// <param name="sourceColumn">The name of the source column.</param>
        /// <param name="sourceVersion">One of the <see cref="DataRowVersion" /> values.</param>
        /// <param name="value">An object that is the value of the parameter.</param>
        public AseParameter(int parameterIndex, AseDbType dbType, int size, ParameterDirection direction, bool isNullable, int precision, int scale, string sourceColumn, DataRowVersion sourceVersion, object value) : this()
        {
            ParameterIndex = parameterIndex;
            AseDbType = dbType;
            Size = size;
            Direction = direction;
            IsNullable = isNullable;
            Precision = (byte)precision;
            Scale = (byte)scale;
            SourceColumn = sourceColumn;
            SourceVersion = sourceVersion;
            Value = value;
        }

        public override void ResetDbType()
        {
            DbType = default(DbType);
        }

        /// <summary>
        /// Gets or sets the <see cref="AseDbType" /> of the parameter.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public override DbType DbType
        {
            get => TypeMap.GetDbType(_type);
            set => AseDbType = TypeMap.GetAseDbType(value);
        }

        /// <summary>
        /// Gets or sets the <see cref="AseDbType" /> of the parameter.
        /// </summary>
#if ENABLE_SYSTEM_DATA_COMMON_EXTENSIONS
        [DbProviderSpecificTypeProperty(true)]
#endif
        public AseDbType AseDbType
        {
            get => _type;
            set {
                _type = TypeMap.CleanupAseDbType(value);
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the parameter is input-only, output-only, 
        /// bidirectional, or a stored procedure return value parameter.
        /// </summary>
        /// <remarks>
        /// <para>If the <see cref="ParameterDirection" /> is output, and execution of the associated <see cref="AseCommand" /> does 
        /// not return a value, the <see cref="AseParameter" /> contains a null value.</para>
        /// <para><b>Output</b>, <b>InputOut</b>, and <b>ReturnValue</b> parameters returned by calling <see cref="AseCommand.ExecuteReader()" /> cannot 
        /// be accessed until you close the <see cref="AseDataReader" />.</para>
        /// </remarks>
        public override ParameterDirection Direction { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the parameter accepts null values. IsNullable is not used to validate the 
        /// parameter’s value and will not prevent sending or receiving a null value when executing a command.
        /// </summary>
        /// <remarks>
        /// <para>Null values are handled using the <see cref="System.DBNull" /> class.</para>
        /// </remarks>
        public override bool IsNullable { get; set; }

        /// <summary>
        /// Gets or sets the name of the <see cref="AseParameter" />.
        /// </summary>
        /// <remarks>
        /// <para>The ParameterName is specified in the form @paramname. You must set ParameterName before executing an 
        /// <see cref="AseCommand" /> that relies on parameters.</para>
        /// </remarks>
        public override string ParameterName
        {
            get => _parameterName;
            set => _parameterName = value == null || value.StartsWith("@") ? value : $"@{value}";
        }

        /// <summary>
        /// Gets or sets the index of the <see cref="AseParameter" />.
        /// </summary>
        public int ParameterIndex
        {
            get => _parameterIndex;
            set => _parameterIndex = value;
        }

        /// <summary>
        /// Not supported yet. .NET Core 2.0 dependency.
        /// </summary>
        public override string SourceColumn { get; set; }

        public override bool SourceColumnNullMapping { get; set; }

#if ENABLE_SYSTEM_DATA_COMMON_EXTENSIONS
        public override DataRowVersion SourceVersion { get; set; }
#else
        public DataRowVersion SourceVersion { get; set; }
#endif
        /// <summary>
        /// Gets or sets the value of the parameter.
        /// </summary>
        /// <remarks>
        /// <para>For input parameters, the value is bound to the <see cref="AseCommand" /> that is sent to the server. For output 
        /// and return value parameters, the value is set on completion of the <see cref="AseCommand" /> and after the <see cref="AseDataReader" /> 
        /// is closed.</para>
        /// <para>This property can be set to null or <see cref="System.DBNull.Value" />. Use <see cref="System.DBNull.Value" /> to send a NULL 
        /// value as the value of the parameter. Use null or do not set Value to use the default value for the parameter.</para>
        /// <para>An exception is thrown if non-Unicode XML data is passed as a string.</para>
        /// <para>If the application specifies the database type, the bound value is converted to that type when the provider sends the 
        /// data to the server. The provider tries to convert any type of value if it supports the <see cref="System.IConvertible" /> 
        /// interface. Conversion errors may result if the specified type is not compatible with the value.</para>
        /// <para>The <see cref="AseDbType" /> property can be inferred by setting the Value.</para>
        /// </remarks>
        public override object Value { get; set; }

        /// <summary>
        /// Gets or sets the number of digits used to represent the <see cref="Value" /> property.
        /// </summary>
        /// <remarks>
        /// <para>The Precision property is used by parameters that have a <see cref="AseDbType" /> of <see cref="decimal" />.</para>
        /// <para>You do not need to specify values for the <see cref="Precision" /> and <see cref="Scale" /> properties for input parameters, as they can be 
        /// inferred from the parameter value. Precision and Scale are required for output parameters and for scenarios where 
        /// you need to specify complete metadata for a parameter without indicating a value, such as specifying a null value 
        /// with a specific precision and scale.</para>
        /// <para>Use of this property to coerce data passed to the database is not supported. To round, truncate, or otherwise coerce data before 
        /// passing it to the database, use the <see cref="System.Math" /> class that is part of the System namespace prior to assigning a value to 
        /// the parameter's <see cref="Value" /> property.</para>
        /// </remarks>
        public override byte Precision { get; set; }

        /// <summary>
        /// Gets or sets the number of decimal places to which <see cref="Value" /> is resolved.
        /// </summary>
        /// <remarks>
        /// <para>The Scale property is used by parameters that have a <see cref="AseDbType" /> of <see cref="decimal" />.</para>
        /// <para>Data may be truncated if the Scale property is not explicitly specified and the data on the server does not fit 
        /// in scale 0 (the default).</para>
        /// <para>You do not need to specify values for the <see cref="Precision" /> and <see cref="Scale" /> properties for input parameters, as they can be 
        /// inferred from the parameter value. Precision and Scale are required for output parameters and for scenarios where 
        /// you need to specify complete metadata for a parameter without indicating a value, such as specifying a null value 
        /// with a specific precision and scale.</para>
        /// <para>Use of this property to coerce data passed to the database is not supported. To round, truncate, or otherwise coerce data before 
        /// passing it to the database, use the <see cref="System.Math" /> class that is part of the System namespace prior to assigning a value to 
        /// the parameter's <see cref="Value" /> property.</para>
        /// </remarks>
        public override byte Scale { get; set; }

        /// <summary>
        /// Gets or sets the maximum size, in bytes, of the data within the column.
        /// </summary>
        /// <remarks>
        /// <para>Return values are not affected by this property; return parameters from stored procedures are always fixed-size integers.</para>
        /// <para>For output parameters with a variable length type (nvarchar, for example), the size of the parameter defines the size of the 
        /// buffer holding the output parameter. The output parameter can be truncated to a size specified with Size. For character types, the 
        /// size specified with Size is in characters.</para>
        /// <para>The Size property is used for binary and string types. For parameters of type <see cref="string" />, Size means length in Unicode 
        /// characters. For parameters of type <see cref="System.Data.DbType.Xml" />, Size is ignored.</para>
        /// <para>For nonstring data types and ANSI string data, the Size property refers to the number of bytes. For Unicode string data, Size refers 
        /// to the number of characters. The count for strings does not include the terminating character.</para>
        /// <para>For variable-length data types, Size describes the maximum amount of data to transmit to the server. For example, for a Unicode 
        /// string value, Size could be used to limit the amount of data sent to the server to the first one hundred characters.</para>
        /// <para>If not explicitly set, the size is inferred from the actual size of the specified parameter value.</para>
        /// <para>If the fractional part of the parameter value is greater than the size, then the value will be truncated to match the size.</para>
        /// <para>For fixed length data types, the value of Size is ignored. It can be retrieved for informational purposes, and returns the 
        /// maximum amount of bytes the provider uses when transmitting the value of the parameter to the server. </para>
        /// </remarks>
        public override int Size { get; set; }

        /// <summary>
        /// Indicates if this parameter can be sent to the server (normal:true),
        /// or if it's an abstraction over return values (ParameterDirection.ReturnValue:false)
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal bool CanSendOverTheWire => Direction != ParameterDirection.ReturnValue;

        /// <summary>
        /// Indicates if this parameter will have an output value
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal bool IsOutput => Direction == ParameterDirection.InputOutput || Direction == ParameterDirection.Output;

        public override string ToString()
        {
            return ParameterName ?? ParameterIndex.ToString();
        }

#if ENABLE_CLONEABLE_INTERFACE
        public object Clone()
        {
            var clone = new AseParameter
            {
                _type = AseDbType,
                Direction = Direction,
                IsNullable = IsNullable,
                ParameterName = ParameterName,
                ParameterIndex = ParameterIndex,
                Precision = Precision,
                Scale = Scale,
                Size = Size,
                SourceColumn = SourceColumn,
                SourceVersion = SourceVersion,
                Value = Value
            };
            return clone;
        }
#endif
        internal object SendableValue => Value.AsSendableValue(_type);
    }
}
