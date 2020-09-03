using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using AdoNetCore.AseClient.Enum;

namespace AdoNetCore.AseClient.Internal
{
    [DebuggerDisplay("[{" + nameof(ColumnName) + ",nq}]")]
    internal class FormatItem
    {
        public string ColumnLabel { get; set; }
        public string CatalogName { get; set; }
        public string SchemaName { get; set; }
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public RowFormatItemStatus RowStatus { get; set; }
        public int UserType { get; set; }
        public TdsDataType DataType { get; set; }
        public int? Length { get; set; }
        /// <summary>
        /// Relates to TDS_NUMN and TDS_DECN
        /// </summary>
        public byte? Precision { get; set; }
        /// <summary>
        /// Relates to TDS_NUMN and TDS_DECN
        /// </summary>
        public byte? Scale { get; set; }
        public string LocaleInfo { get; set; }

        private string _parameterName;
        public string ParameterName
        {
            get => _parameterName;
            set => _parameterName = value == null || value.StartsWith("@") ? value : $"@{value}";
        }
        public bool IsNullable { get; set; }
        public bool IsOutput { get; set; }

        /// <summary>
        /// Get the most appropriate column name to display to the driver user.
        /// Preference is to the Label/Alias (e.g. "some_alias" from "select some_column as some_alias")
        /// If no Label/Alias, then just use whatever value the underlying column name has.
        /// </summary>
        public string DisplayColumnName => string.IsNullOrWhiteSpace(ColumnLabel)
            ? ColumnName
            : ColumnLabel;

        public bool IsDecimalType => DataType == TdsDataType.TDS_DECN ||
                                     DataType == TdsDataType.TDS_NUMN;

        public AseDbType AseDbType { get; set; }
        /// <summary>
        /// Relates to TDS_BLOB
        /// </summary>
        public BlobType BlobType { get; set; }
        /// <summary>
        /// Relates to TDS_BLOB
        /// </summary>
        public byte[] ClassId { get; set; }
        /// <summary>
        /// Relates to TDS_BLOB
        /// </summary>
        public SerializationType SerializationType { get; set; }

        public static FormatItem CreateForParameter(AseParameter parameter, DbEnvironment env, AseCommand command)
        {
            var dbType = parameter.DbType;
            var length = TypeMap.GetFormatLength(dbType, parameter, env.Encoding);

            parameter.AseDbType = TypeMap.InferType(parameter);

            var format = command.FormatItem;
            var parameterName = parameter.ParameterName ?? command.Parameters.IndexOf(parameter).ToString();
            if (!(command.FormatItem != null && command.FormatItem.ParameterName == parameterName &&
                  command.FormatItem.AseDbType == parameter.AseDbType))
            {
                format = new FormatItem
                {
                    AseDbType = parameter.AseDbType,
                    ParameterName = parameter.ParameterName,
                    IsOutput = parameter.IsOutput,
                    IsNullable = parameter.IsNullable,
                    Length = length,
                    DataType = TypeMap.GetTdsDataType(dbType, parameter.SendableValue, length, parameter.ParameterName),
                    UserType = TypeMap.GetUserType(dbType, parameter.SendableValue, length)
                };

                //fixup the FormatItem's BlobType for strings and byte arrays
                if (format.DataType == TdsDataType.TDS_BLOB)
                {
                    switch (parameter.DbType)
                    {
                        case DbType.AnsiString:
                            format.BlobType = BlobType.BLOB_LONGCHAR;
                            break;
                        case DbType.String:
                            format.BlobType = BlobType.BLOB_UNICHAR;
                            // This is far less than ideal but at the time of addressing this issue whereby if the
                            // BlobType is a BLOB_UNICHAR then the UserType would need to be 36 when it
                            // is a stored proc otherwise it would need to be zero (0).
                            //
                            // In the future, we'd need to overhaul how TDS_BLOB is structured especially
                            // around BLOB_UNICHAR and the UserType that it should return in a more consistent way
                            if (command.CommandType != CommandType.StoredProcedure)
                                format.UserType = 0;

                            break;
                        case DbType.Binary:
                            format.BlobType = BlobType.BLOB_LONGBINARY;
                            break;
                    }
                }
            }
            else
            {
                format.DataType = TypeMap.GetTdsDataType(dbType, parameter.SendableValue, length, parameter.ParameterName);
                format.UserType = TypeMap.GetUserType(dbType, parameter.SendableValue, length);
            }

            //fixup the FormatItem's length,scale,precision for decimals
            if (format.IsDecimalType)
            {
                if (parameter.IsOutput)
                {
                    format.Precision = parameter.Precision;
                    format.Scale = parameter.Scale;
                    format.Length = 1;
                }
                else if (parameter.SendableValue == DBNull.Value)
                {
                    format.Precision = 1;
                    format.Scale = 0;
                    format.Length = 1;
                }
                else if (env.UseAseDecimal)
                {
                    var aseDecimal = parameter.SendableValue is AseDecimal d ? d : new AseDecimal(Convert.ToDecimal(parameter.SendableValue));
                    format.Precision = (byte)aseDecimal.Precision;
                    format.Scale = (byte)aseDecimal.Scale;
                    format.Length = aseDecimal.BytesRequired + 1;
                }
                else
                {
                    var sqlDecimal = (SqlDecimal)Convert.ToDecimal(parameter.SendableValue);
                    format.Precision = sqlDecimal.Precision;
                    format.Scale = sqlDecimal.Scale;
                    format.Length = sqlDecimal.BytesRequired + 1;
                }
            }

            return format;
        }

        public static FormatItem ReadForRow(Stream stream, Encoding enc, TokenType srcTokenType)
        {
            FormatItem format;
            switch (srcTokenType)
            {
                case TokenType.TDS_ROWFMT:
                    format = new FormatItem
                    {
                        ColumnName = stream.ReadByteLengthPrefixedString(enc),
                        RowStatus = (RowFormatItemStatus)stream.ReadByte()
                    };
                    break;
                case TokenType.TDS_ROWFMT2:
                    format = new FormatItem
                    {
                        ColumnLabel = stream.ReadByteLengthPrefixedString(enc),
                        CatalogName = stream.ReadByteLengthPrefixedString(enc),
                        SchemaName = stream.ReadByteLengthPrefixedString(enc),
                        TableName = stream.ReadByteLengthPrefixedString(enc),
                        ColumnName = stream.ReadByteLengthPrefixedString(enc),
                        RowStatus = (RowFormatItemStatus)stream.ReadUInt()
                    };
                    break;
                default:
                    throw new ArgumentException($"Unexpected token type: {srcTokenType}.", nameof(srcTokenType));
            }

            ReadTypeInfo(format, stream, enc);

            Logger.Instance?.WriteLine($"  <- {format.ColumnName}: {format.DataType} (len: {format.Length}) (ut:{format.UserType}) (status:{format.RowStatus}) (loc:{format.LocaleInfo}) format names available: ColumnLabel [{format.ColumnLabel}], ColumnName [{format.ColumnName}], CatalogName [{format.CatalogName}], ParameterName [{format.ParameterName}], SchemaName [{format.SchemaName}], TableName [{format.TableName}]");

            return format;
        }

        public static FormatItem ReadForParameter(Stream stream, Encoding enc, TokenType srcTokenType)
        {
            var format = new FormatItem
            {
                ParameterName = stream.ReadByteLengthPrefixedString(enc),
            };
            var status = (ParameterFormatItemStatus)(srcTokenType == TokenType.TDS_PARAMFMT
                ? (uint)stream.ReadByte()
                : stream.ReadUInt());
            format.IsOutput = status.HasFlag(ParameterFormatItemStatus.TDS_PARAM_RETURN);
            format.IsNullable = status.HasFlag(ParameterFormatItemStatus.TDS_PARAM_NULLALLOWED);

            ReadTypeInfo(format, stream, enc);

            Logger.Instance?.WriteLine($"  <- {format.ParameterName}: {format.DataType} (len: {format.Length}) (ut:{format.UserType})");

            return format;
        }

        private static void ReadTypeInfo(FormatItem format, Stream stream, Encoding enc)
        {
            format.UserType = stream.ReadInt();
            format.DataType = (TdsDataType)stream.ReadByte();

            switch (format.DataType)
            {
                case TdsDataType.TDS_INT1:
                case TdsDataType.TDS_INT2:
                case TdsDataType.TDS_UINT2:
                case TdsDataType.TDS_INT4:
                case TdsDataType.TDS_UINT4:
                case TdsDataType.TDS_INT8:
                case TdsDataType.TDS_UINT8:
                case TdsDataType.TDS_FLT4:
                case TdsDataType.TDS_FLT8:
                case TdsDataType.TDS_BIT:
                case TdsDataType.TDS_DATETIME:
                case TdsDataType.TDS_SHORTDATE:
                case TdsDataType.TDS_DATE:
                case TdsDataType.TDS_TIME:
                case TdsDataType.TDS_MONEY:
                case TdsDataType.TDS_SHORTMONEY:
                    break;
                case TdsDataType.TDS_INTN:
                case TdsDataType.TDS_UINTN:
                case TdsDataType.TDS_CHAR:
                case TdsDataType.TDS_VARCHAR:
                case TdsDataType.TDS_BOUNDARY:
                case TdsDataType.TDS_SENSITIVITY:
                case TdsDataType.TDS_BINARY:
                case TdsDataType.TDS_VARBINARY:
                case TdsDataType.TDS_FLTN:
                case TdsDataType.TDS_DATETIMEN:
                case TdsDataType.TDS_DATEN:
                case TdsDataType.TDS_TIMEN:
                case TdsDataType.TDS_MONEYN:
                    format.Length = stream.ReadByte();
                    break;
                case TdsDataType.TDS_LONGCHAR:
                case TdsDataType.TDS_LONGBINARY:
                    format.Length = stream.ReadInt();
                    break;
                case TdsDataType.TDS_BLOB:
                    format.BlobType = (BlobType)stream.ReadByte();
                    format.ClassId = stream.ReadNullableUShortLengthPrefixedByteArray();
                    break;
                case TdsDataType.TDS_DECN:
                case TdsDataType.TDS_NUMN:
                    format.Length = stream.ReadByte();
                    format.Precision = (byte)stream.ReadByte();
                    format.Scale = (byte)stream.ReadByte();
                    break;
                case TdsDataType.TDS_TEXT:
                case TdsDataType.TDS_XML:
                case TdsDataType.TDS_IMAGE:
                case TdsDataType.TDS_UNITEXT:
                    {
                        format.Length = stream.ReadInt();
                        /*var name =*/
                        stream.ReadShortLengthPrefixedString(enc);
                        break;
                    }
                case TdsDataType.TDS_BIGDATETIMEN:
                    format.Length = stream.ReadByte();
                    // don't know what this represents, but when sending/receiving a big datetime we need to send/receive a byte
                    // maybe it represents the resolution to which the number represents (e.g. 6 = microseconds, 3 = milliseconds?)
                    stream.ReadByte();
                    break;
                default:
                    throw new NotSupportedException($"Unsupported data type {format.DataType} (column: {format.DisplayColumnName})");
            }

            format.LocaleInfo = stream.ReadByteLengthPrefixedString(enc);
            //ClassId stuff?
        }

        public void WriteForParameter(Stream stream, Encoding enc, TokenType srcTokenType)
        {
            Logger.Instance?.WriteLine($"  -> {ParameterName}: {DataType} ({Precision ?? Length}{(Scale.HasValue ? "," + Scale : "")}) (ut:{UserType})");
            if (string.IsNullOrWhiteSpace(ParameterName) || string.Equals("@", ParameterName))
            {
                stream.WriteByte(0);
            }
            else
            {
                stream.WriteBytePrefixedString(ParameterName, enc);
            }

            var nullableStatus = IsNullable ? ParameterFormatItemStatus.TDS_PARAM_NULLALLOWED : 0x00;
            var outputStatus = IsOutput ? ParameterFormatItemStatus.TDS_PARAM_RETURN : 0x00;
            var status = nullableStatus | outputStatus;
            if (srcTokenType == TokenType.TDS_PARAMFMT)
            {
                stream.WriteByte((byte)status);
            }
            else
            {
                stream.WriteUInt((uint)status);
            }

            stream.WriteInt(UserType);
            stream.WriteByte((byte)DataType);

            switch (DataType)
            {
                //fixed-length types
                case TdsDataType.TDS_BIT:
                case TdsDataType.TDS_INT1:
                case TdsDataType.TDS_SINT1:
                case TdsDataType.TDS_INT2:
                case TdsDataType.TDS_UINT2:
                case TdsDataType.TDS_INT4:
                case TdsDataType.TDS_UINT4:
                case TdsDataType.TDS_INT8:
                case TdsDataType.TDS_UINT8:
                case TdsDataType.TDS_FLT4:
                case TdsDataType.TDS_FLT8:
                case TdsDataType.TDS_DATETIME:
                case TdsDataType.TDS_DATE:
                case TdsDataType.TDS_TIME:
                case TdsDataType.TDS_MONEY:
                case TdsDataType.TDS_SHORTMONEY:
                    break;
                case TdsDataType.TDS_VARCHAR:
                case TdsDataType.TDS_VARBINARY:
                case TdsDataType.TDS_BINARY:
                case TdsDataType.TDS_INTN:
                case TdsDataType.TDS_UINTN:
                case TdsDataType.TDS_FLTN:
                case TdsDataType.TDS_DATETIMEN:
                case TdsDataType.TDS_DATEN:
                case TdsDataType.TDS_TIMEN:
                case TdsDataType.TDS_MONEYN:
                    stream.WriteByte((byte)(Length ?? 0));
                    break;
                case TdsDataType.TDS_LONGCHAR:
                case TdsDataType.TDS_LONGBINARY:
                    stream.WriteUInt((uint)(Length ?? 0));
                    break;
                case TdsDataType.TDS_BLOB:
                    //according to spec, length isn't specified as part of the format token, but as part of the params token
                    stream.WriteByte((byte)BlobType);
                    stream.WriteNullableUShortPrefixedByteArray(ClassId);
                    break;
                case TdsDataType.TDS_DECN:
                case TdsDataType.TDS_NUMN:
                    stream.WriteByte((byte)(Length ?? 1));
                    stream.WriteByte(Precision ?? 1);
                    stream.WriteByte(Scale ?? 0);
                    break;
                case TdsDataType.TDS_BIGDATETIMEN:
                    stream.WriteByte((byte)(Length ?? 8));
                    // don't know what this represents, but when sending/receiving a big datetime we need to send/receive a byte
                    // maybe it represents the resolution to which the number represents (e.g. 6 = microseconds, 3 = milliseconds?)
                    stream.WriteByte(6);
                    break;
                default:
                    throw new NotSupportedException($"{DataType} not yet supported");
            }

            //locale
            stream.WriteByte(0);
        }

        public string GetDataTypeName()
        {
            switch (DataType)
            {
                case TdsDataType.TDS_FLT4:
                    return "real";
                case TdsDataType.TDS_FLT8:
                    return "float";
                case TdsDataType.TDS_FLTN:
                    switch (Length)
                    {
                        case 4:
                            return "real";
                        //case 8:
                        default:
                            return "float";
                    }
                case TdsDataType.TDS_BIT:
                    return "bit";
                case TdsDataType.TDS_INT1:
                    return "tinyint";
                case TdsDataType.TDS_INT2:
                    return "smallint";
                case TdsDataType.TDS_INT4:
                    return "int";
                case TdsDataType.TDS_INT8:
                    return "bigint";
                case TdsDataType.TDS_INTN:
                    switch (Length)
                    {
                        case 1:
                            return "tinyint";
                        case 2:
                            return "smallint";
                        case 4:
                            return "int";
                        //case 8:
                        default:
                            return "bigint";
                    }
                case TdsDataType.TDS_UINT2:
                    return "unsigned smallint";
                case TdsDataType.TDS_UINT4:
                    return "unsigned int";
                case TdsDataType.TDS_UINT8:
                    return "unsigned bigint";
                case TdsDataType.TDS_UINTN:
                    switch (Length)
                    {
                        //server cannot return an unsigned tinyint
                        //case 1:
                        //    return "unsigned tinyint";
                        case 2:
                            return "unsigned smallint";
                        case 4:
                            return "unsigned int";
                        //case 8:
                        default:
                            return "unsigned bigint";
                    }
                case TdsDataType.TDS_NUMN:
                    return "numeric";
                case TdsDataType.TDS_DECN:
                    return "decimal";
                case TdsDataType.TDS_MONEY:
                    return "money";
                case TdsDataType.TDS_SHORTMONEY:
                    return "smallmoney";
                case TdsDataType.TDS_MONEYN:
                    switch (Length)
                    {
                        case 4:
                            return "smallmoney";
                        //case 8:
                        default:
                            return "money";
                    }
                case TdsDataType.TDS_DATE:
                case TdsDataType.TDS_DATEN:
                    return "date";
                case TdsDataType.TDS_SHORTDATE:
                    return "smalldatetime";
                case TdsDataType.TDS_DATETIME:
                    return "datetime";
                case TdsDataType.TDS_DATETIMEN:
                case TdsDataType.TDS_BIGDATETIMEN:
                    switch (Length)
                    {
                        case 4:
                            return "smalldatetime";
                        //case 8:
                        default:
                            return "datetime";
                    }
                case TdsDataType.TDS_TIME:
                case TdsDataType.TDS_TIMEN:
                    return "time";
                case TdsDataType.TDS_CHAR:
                    return "char";
                case TdsDataType.TDS_VARCHAR:
                    switch (UserType)
                    {
                        case 25:
                            return "nvarchar";
                        default:
                            return "varchar";
                    }
                case TdsDataType.TDS_LONGCHAR:
                    return "longchar";
                case TdsDataType.TDS_TEXT:
                    return "text";
                case TdsDataType.TDS_UNITEXT:
                    return "unitext";
                case TdsDataType.TDS_BINARY:
                    return "binary";
                case TdsDataType.TDS_IMAGE:
                    return "image";
                case TdsDataType.TDS_VARBINARY:
                    return "varbinary";
                case TdsDataType.TDS_LONGBINARY:
                    switch (UserType)
                    {
                        case 34:
                            return "unichar";
                        case 35:
                            return "univarchar";
                        default:
                            return "binary";
                    }
                case TdsDataType.TDS_BLOB:
                    switch (BlobType)
                    {
                        case BlobType.BLOB_UNICHAR:
                            return "unichar";
                        default:
                            return "blob";
                    }
                default:
                    return string.Empty;
            }
        }
    }
}
