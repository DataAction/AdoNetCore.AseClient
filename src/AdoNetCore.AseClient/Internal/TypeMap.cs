using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using AdoNetCore.AseClient.Enum;

namespace AdoNetCore.AseClient.Internal
{
    internal static class TypeMap
    {
        private const int VarLongBoundary = 255;

        private static readonly Dictionary<DbType, Func<object, int, TdsDataType>> DbToTdsMap = new Dictionary<DbType, Func<object, int, TdsDataType>>
        {
            {DbType.Boolean, (value, length) => TdsDataType.TDS_BIT},
            {DbType.Byte, (value, length) => value == DBNull.Value ? TdsDataType.TDS_INTN : TdsDataType.TDS_INT1},
            {DbType.SByte, (value, length) => TdsDataType.TDS_INTN},
            {DbType.Int16, (value, length) => value == DBNull.Value ? TdsDataType.TDS_INTN : TdsDataType.TDS_INT2},
            {DbType.UInt16, (value, length) => value == DBNull.Value ? TdsDataType.TDS_UINTN : TdsDataType.TDS_UINT2},
            {DbType.Int32, (value, length) => value == DBNull.Value ? TdsDataType.TDS_INTN : TdsDataType.TDS_INT4},
            {DbType.UInt32, (value, length) => value == DBNull.Value ? TdsDataType.TDS_UINTN : TdsDataType.TDS_UINT4},
            {DbType.Int64, (value, length) => value == DBNull.Value ? TdsDataType.TDS_INTN : TdsDataType.TDS_INT8},
            {DbType.UInt64, (value, length) => value == DBNull.Value ? TdsDataType.TDS_UINTN : TdsDataType.TDS_UINT8},
            {DbType.String, (value, length) => TdsDataType.TDS_LONGBINARY},
            {DbType.StringFixedLength, (value, length) => TdsDataType.TDS_LONGBINARY},
            {DbType.AnsiString, (value, length) => length <= VarLongBoundary ? TdsDataType.TDS_VARCHAR : TdsDataType.TDS_LONGCHAR},
            {DbType.AnsiStringFixedLength, (value, length) => length <= VarLongBoundary ? TdsDataType.TDS_VARCHAR : TdsDataType.TDS_LONGCHAR},
            {DbType.Binary, (value, length) => length <= VarLongBoundary ? TdsDataType.TDS_BINARY : TdsDataType.TDS_LONGBINARY},
            {DbType.Guid, (value, length) => TdsDataType.TDS_BINARY},
            {DbType.Decimal, (value, length) => TdsDataType.TDS_NUMN},
            {DbType.Currency, (value, length) => TdsDataType.TDS_MONEYN},
            {DbType.VarNumeric, (value, length) => TdsDataType.TDS_NUMN},
            {DbType.Single, (value, length) => value == DBNull.Value ? TdsDataType.TDS_FLTN : TdsDataType.TDS_FLT4},
            {DbType.Double, (value, length) => value == DBNull.Value ? TdsDataType.TDS_FLTN : TdsDataType.TDS_FLT8},
            {DbType.DateTime, (value, length) => TdsDataType.TDS_BIGDATETIMEN},
            {DbType.Date, (value, length) => value == DBNull.Value ? TdsDataType.TDS_DATEN : TdsDataType.TDS_DATE},
            {DbType.Time, (value, length) => TdsDataType.TDS_BIGDATETIMEN}
        };

        private static readonly Dictionary<Type, AseDbType> NetTypeToAseDbTypeMap = new Dictionary<Type, AseDbType>
        {
            {typeof(bool), AseDbType.Bit},
            {typeof(byte), AseDbType.TinyInt},
            {typeof(sbyte), AseDbType.SmallInt},
            {typeof(short), AseDbType.SmallInt},
            {typeof(ushort), AseDbType.UnsignedSmallInt},
            {typeof(int), AseDbType.Integer},
            {typeof(uint), AseDbType.UnsignedInt},
            {typeof(long), AseDbType.BigInt},
            {typeof(ulong), AseDbType.UnsignedBigInt},
            {typeof(char), AseDbType.VarChar},
            {typeof(char[]), AseDbType.VarChar},
            {typeof(string), AseDbType.VarChar},
            {typeof(byte[]), AseDbType.VarBinary},
            {typeof(Guid), AseDbType.Binary},
            {typeof(decimal), AseDbType.Numeric},
            {typeof(AseDecimal), AseDbType.Decimal},
            {typeof(float), AseDbType.Real},
            {typeof(double), AseDbType.Double},
            {typeof(DateTime), AseDbType.DateTime},
            {typeof(TimeSpan), AseDbType.DateTime}
        };

        private static readonly Dictionary<DbType, int> FixedFormatLengthMap = new Dictionary<DbType, int>
        {
            //1 byte pos/neg, 16 bytes data
            {DbType.Decimal, 17},
            //1 byte pos/neg, 16 bytes data
            {DbType.VarNumeric, 17},
            {DbType.Boolean, 1},
            {DbType.Byte, 1},
            {DbType.Int16, 2},
            {DbType.UInt16, 2},
            //can't seem to write an sbyte as a single byte, so it'll get encoded in a short
            {DbType.SByte, 2},
            {DbType.Int32, 4},
            {DbType.UInt32, 4},
            {DbType.Int64, 8},
            {DbType.UInt64, 8},
            {DbType.Single, 4},
            {DbType.Double, 8},
            {DbType.DateTime, 8},
            {DbType.Date, 4},
            {DbType.Time, 4},
            {DbType.Guid, 16},
            {DbType.Currency, 8},
        };

        private static readonly Dictionary<DbType, Func<AseParameter, Encoding, object, int>> VariableFormatLengthMap = new Dictionary<DbType, Func<AseParameter, Encoding, object, int>>
        {
            {DbType.String, GetStringFormatLength},
            {DbType.StringFixedLength, GetStringFormatLength},
            {DbType.AnsiString, GetAnsiStringFormatLength},
            {DbType.AnsiStringFixedLength, GetAnsiStringFormatLength},
            {DbType.Binary, GetBinaryFormatLength},
        };

        public static int? GetFormatLength(DbType dbType, AseParameter parameter, Encoding enc)
        {
            if (FixedFormatLengthMap.ContainsKey(dbType))
            {
                return FixedFormatLengthMap[dbType];
            }

            var value = parameter.SendableValue;

            if (VariableFormatLengthMap.ContainsKey(dbType))
            {
                return VariableFormatLengthMap[dbType](parameter, enc, value);
            }

            return null;
        }

        private static int GetStringFormatLength(AseParameter parameter, Encoding enc, object value)
        {
            if (parameter.IsOutput)
            {
                return Math.Max(VarLongBoundary * 2, parameter.Size);
            }

            switch (value)
            {
                case string s:
                    return Encoding.Unicode.GetByteCount(s);
                case char c:
                    return Encoding.Unicode.GetByteCount(new[] { c });
                default:
                    return 0;
            }
        }

        private static int GetAnsiStringFormatLength(AseParameter parameter, Encoding enc, object value)
        {
            if (parameter.IsOutput)
            {
                return Math.Max(VarLongBoundary, parameter.Size);
            }

            switch (value)
            {
                case string s:
                    return enc.GetByteCount(s);
                case char c:
                    return enc.GetByteCount(new[] { c });
                default:
                    return 0;
            }
        }

        private static int GetBinaryFormatLength(AseParameter parameter, Encoding enc, object value)
        {
            if (parameter.IsOutput)
            {
                return Math.Max(VarLongBoundary, parameter.Size);
            }

            switch (value)
            {
                case byte[] ba:
                    return ba.Length;
                case byte _:
                    return 1;
                default:
                    return 0;
            }
        }

        public static AseDbType InferType(AseParameter parameter)
        {
            if (parameter.AseDbType != AseDbType.Unsupported || parameter.Value == null || parameter.Value == DBNull.Value)
            {
                return parameter.AseDbType;
            }

            var netType = parameter.Value.GetType();
            if (NetTypeToAseDbTypeMap.ContainsKey(netType))
            {
                return NetTypeToAseDbTypeMap[netType];
            }

            throw new NotSupportedException($"Unsupported .net type {parameter.Value.GetType()} for parameter '{parameter.ParameterName}'.");
        }

        public static TdsDataType GetTdsDataType(DbType dbType, object value, int? length, string parameterName)
        {
            // If the consumer has explicitly set a type, then rely on that.
            if (DbToTdsMap.TryGetValue(dbType, out var result))
            {
                return result(value, length ?? 0);
            }

            throw new NotSupportedException($"Unsupported data type {dbType} for parameter '{parameterName}'.");
        }

        public static int GetTdsUserType(DbType dbType)
        {
            switch (dbType)
            {
                case DbType.String:
                    return 35;
                case DbType.StringFixedLength:
                    return 34;
                default:
                    return 0;
            }
        }

        private static readonly Dictionary<TdsDataType, Func<FormatItem, Type>> TdsToNetMap = new Dictionary<TdsDataType, Func<FormatItem, Type>>
        {
            {TdsDataType.TDS_BINARY, f => typeof(byte[])},
            {TdsDataType.TDS_BIT, f => typeof(bool)},
            {TdsDataType.TDS_BLOB, f => typeof(byte[])},
            //{TdsDataType.TDS_BOUNDARY, f => typeof()},
            {TdsDataType.TDS_CHAR, f => typeof(string)},
            {TdsDataType.TDS_DATE, f => typeof(DateTime)},
            {TdsDataType.TDS_DATEN, f => typeof(DateTime)},
            {TdsDataType.TDS_DATETIME, f => typeof(DateTime)},
            {TdsDataType.TDS_DATETIMEN, f => typeof(DateTime)},
            {TdsDataType.TDS_BIGDATETIMEN, f => typeof(DateTime)},
            {TdsDataType.TDS_DECN, f => typeof(decimal)},
            {TdsDataType.TDS_FLT4, f => typeof(float)},
            {TdsDataType.TDS_FLT8, f => typeof(double)},
            {
                TdsDataType.TDS_FLTN, f => f.Length == 8
                    ? typeof(double)
                    : typeof(float)
            },
            {TdsDataType.TDS_IMAGE, f => typeof(byte[])},
            {TdsDataType.TDS_INT1, f => typeof(byte)},
            {TdsDataType.TDS_INT2, f => typeof(short)},
            {TdsDataType.TDS_INT4, f => typeof(int)},
            {TdsDataType.TDS_INT8, f => typeof(long)},
            //{TdsDataType.TDS_INTERVAL, f => typeof()},
            {
                TdsDataType.TDS_INTN, f => f.Length == 8
                    ? typeof(long)
                    : f.Length == 4
                        ? typeof(int)
                        : f.Length == 2
                            ? typeof(short)
                            : typeof(byte)
            },
            {
                TdsDataType.TDS_LONGBINARY, f => f.UserType == 34 || f.UserType == 35
                    ? typeof(string)
                    : typeof(byte[])
            },
            {TdsDataType.TDS_LONGCHAR, f => typeof(string)},
            {TdsDataType.TDS_MONEY, f => typeof(decimal)},
            {TdsDataType.TDS_MONEYN, f => typeof(decimal)},
            {TdsDataType.TDS_NUMN, f => typeof(decimal)},
            //{TdsDataType.TDS_SENSITIVITY,f =>  typeof()},
            {TdsDataType.TDS_SHORTDATE, f => typeof(DateTime)},
            {TdsDataType.TDS_SHORTMONEY, f => typeof(decimal)},
            {TdsDataType.TDS_SINT1, f => typeof(sbyte)},
            {TdsDataType.TDS_TEXT, f => typeof(string)},
            {TdsDataType.TDS_TIME, f => typeof(DateTime)},
            {TdsDataType.TDS_TIMEN, f => typeof(DateTime)},
            {TdsDataType.TDS_UINT2, f => typeof(ushort)},
            {TdsDataType.TDS_UINT4, f => typeof(uint)},
            {TdsDataType.TDS_UINT8, f => typeof(ulong)},
            {
                TdsDataType.TDS_UINTN, f => f.Length == 8
                    ? typeof(ulong)
                    : f.Length == 4
                        ? typeof(uint)
                        : f.Length == 2
                            ? typeof(ushort)
                            : typeof(byte)
            },
            {TdsDataType.TDS_UNITEXT, f => typeof(string)},
            {TdsDataType.TDS_VARBINARY, f => typeof(byte[])},
            {TdsDataType.TDS_VARCHAR, f => typeof(string)},
            //{TdsDataType.TDS_VOID,f =>  typeof()},
            {TdsDataType.TDS_XML, f => typeof(string)},
        };

        public static Type GetNetType(FormatItem format, bool defaultToObject = false)
        {
            if (!TdsToNetMap.ContainsKey(format.DataType))
            {
                if (defaultToObject)
                {
                    return typeof(object);
                }

                throw new NotSupportedException($"Unsupported dataType {format.DataType}");
            }

            return TdsToNetMap[format.DataType](format);
        }

        private static readonly Dictionary<TdsDataType, Func<FormatItem, AseDbType>> TdsToAseMap = new Dictionary<TdsDataType, Func<FormatItem, AseDbType>>
        {
            {TdsDataType.TDS_BINARY, f => AseDbType.Binary},
            {TdsDataType.TDS_BIT, f => AseDbType.Bit},
            {TdsDataType.TDS_BLOB, f => AseDbType.Image},
            //{TdsDataType.TDS_BOUNDARY, f => typeof()},
            {TdsDataType.TDS_CHAR, f => AseDbType.Char},
            {TdsDataType.TDS_DATE, f => AseDbType.Date},
            {TdsDataType.TDS_DATEN, f => AseDbType.Date},
            {TdsDataType.TDS_DATETIME, f => AseDbType.DateTime},
            {TdsDataType.TDS_DATETIMEN, f => AseDbType.DateTime},
            {TdsDataType.TDS_BIGDATETIMEN, f => AseDbType.DateTime},
            {TdsDataType.TDS_DECN, f => AseDbType.Decimal},
            {TdsDataType.TDS_FLT4, f => AseDbType.Real},
            {TdsDataType.TDS_FLT8, f => AseDbType.Double},
            {
                TdsDataType.TDS_FLTN, f => f.Length == 8
                    ? AseDbType.Double
                    : AseDbType.Real
            },
            {TdsDataType.TDS_IMAGE, f => AseDbType.Image},
            {TdsDataType.TDS_INT1, f => AseDbType.TinyInt},
            {TdsDataType.TDS_INT2, f => AseDbType.SmallInt},
            {TdsDataType.TDS_INT4, f => AseDbType.Integer},
            {TdsDataType.TDS_INT8, f =>AseDbType.BigInt},
            //{TdsDataType.TDS_INTERVAL, f => typeof()},
            {
                TdsDataType.TDS_INTN, f => f.Length == 8
                    ? AseDbType.BigInt
                    : f.Length == 4
                        ? AseDbType.Integer
                        : f.Length == 2
                            ? AseDbType.SmallInt
                            : AseDbType.TinyInt
            },
            {
                TdsDataType.TDS_LONGBINARY, f => f.UserType == 34
                                                 ? AseDbType.UniChar : f.UserType == 35
                    ? AseDbType.UniVarChar
                    : AseDbType.Binary
            },
            {TdsDataType.TDS_LONGCHAR, f => AseDbType.VarChar},
            {TdsDataType.TDS_MONEY, f => AseDbType.Money},
            {TdsDataType.TDS_MONEYN, f => f.Length == 8 ? AseDbType.Money : AseDbType.SmallMoney},
            {TdsDataType.TDS_NUMN, f => AseDbType.Numeric},
            //{TdsDataType.TDS_SENSITIVITY,f =>  typeof()},
            {TdsDataType.TDS_SHORTDATE, f => AseDbType.SmallDateTime},
            {TdsDataType.TDS_SHORTMONEY, f => AseDbType.SmallMoney},
            {TdsDataType.TDS_SINT1, f => AseDbType.TinyInt},
            {TdsDataType.TDS_TEXT, f => AseDbType.Text},
            {TdsDataType.TDS_TIME, f => AseDbType.Time},
            {TdsDataType.TDS_TIMEN, f => AseDbType.Time},
            {TdsDataType.TDS_UINT2, f => AseDbType.UnsignedSmallInt},
            {TdsDataType.TDS_UINT4, f => AseDbType.UnsignedInt},
            {TdsDataType.TDS_UINT8, f => AseDbType.UnsignedBigInt},
            {
                TdsDataType.TDS_UINTN, f => f.Length == 8
                    ? AseDbType.UnsignedBigInt
                    : f.Length == 4
                        ? AseDbType.UnsignedInt
                        : f.Length == 2
                            ? AseDbType.UnsignedSmallInt
                            : AseDbType.TinyInt
            },
            {TdsDataType.TDS_UNITEXT, f => AseDbType.Unitext},
            {TdsDataType.TDS_VARBINARY, f => AseDbType.VarBinary},
            {TdsDataType.TDS_VARCHAR, f => AseDbType.VarChar},
            //{TdsDataType.TDS_VOID,f =>  typeof()},
            {TdsDataType.TDS_XML, f => AseDbType.VarChar},
        };

        public static AseDbType GetAseDbType(FormatItem format)
        {
            if (!TdsToAseMap.ContainsKey(format.DataType))
            {
                throw new NotSupportedException($"Unsupported dataType {format.DataType}");
            }

            return TdsToAseMap[format.DataType](format);
        }

        private static readonly Dictionary<DbType, AseDbType> DbTypeToAseDbTypeMap = new Dictionary<DbType, AseDbType>
        {
            {DbType.AnsiString, AseDbType.VarChar},
            {DbType.AnsiStringFixedLength, AseDbType.Char},
            {DbType.Binary, AseDbType.Binary},
            {DbType.Boolean, AseDbType.Bit},
            {DbType.Byte, AseDbType.TinyInt},
            {DbType.Currency, AseDbType.Money},
            {DbType.Date, AseDbType.Date},
            {DbType.DateTime, AseDbType.DateTime},
            {DbType.DateTime2, AseDbType.DateTime},
            {DbType.DateTimeOffset, AseDbType.Unsupported},
            {DbType.Decimal, AseDbType.Decimal},
            {DbType.Double, AseDbType.Double},
            {DbType.Guid, AseDbType.Unsupported},
            {DbType.Int16, AseDbType.SmallInt},
            {DbType.Int32, AseDbType.Integer},
            {DbType.Int64, AseDbType.BigInt},
            {DbType.Object, AseDbType.Binary},
            {DbType.SByte, AseDbType.SmallInt}, //Was Unsupported
            {DbType.Single, AseDbType.Real},
            {DbType.String, AseDbType.UniVarChar},
            {DbType.StringFixedLength, AseDbType.UniChar},
            {DbType.Time, AseDbType.Time},
            {DbType.UInt16, AseDbType.UnsignedSmallInt},
            {DbType.UInt32, AseDbType.UnsignedInt},
            {DbType.UInt64, AseDbType.UnsignedBigInt},
            {DbType.VarNumeric, AseDbType.Numeric},
            {DbType.Xml, AseDbType.Unsupported},
        };

        public static AseDbType GetAseDbType(DbType dbType)
        {
            return DbTypeToAseDbTypeMap.ContainsKey(dbType)
                ? DbTypeToAseDbTypeMap[dbType]
                : default(AseDbType);
        }

        private static readonly Dictionary<AseDbType, DbType> AseDbTypeToDbTypeMap = new Dictionary<AseDbType, DbType>
        {
            //{AseDbType.BigDateTime, DbType.DateTime}, //same value as DateTime
            {AseDbType.BigInt, DbType.Int64},
            {AseDbType.Binary, DbType.Binary},
            {AseDbType.Bit, DbType.Boolean},
            {AseDbType.Char, DbType.AnsiStringFixedLength},
            {AseDbType.Date, DbType.Date},
            {AseDbType.DateTime, DbType.DateTime},
            {AseDbType.Decimal, DbType.Decimal},
            {AseDbType.Double, DbType.Double},
            {AseDbType.Image, DbType.Binary},
            {AseDbType.Integer, DbType.Int32},
            {AseDbType.Money, DbType.Currency},
            {AseDbType.NChar, DbType.AnsiStringFixedLength},
            {AseDbType.Numeric, DbType.VarNumeric},
            {AseDbType.NVarChar, DbType.AnsiString},
            {AseDbType.Real, DbType.Single},
            {AseDbType.SmallDateTime, DbType.DateTime},
            {AseDbType.SmallInt, DbType.Int16},
            {AseDbType.SmallMoney, DbType.Currency},
            {AseDbType.Text, DbType.AnsiString},
            {AseDbType.Time, DbType.Time},
            {AseDbType.TimeStamp, DbType.Binary},
            {AseDbType.TinyInt, DbType.Byte},
            {AseDbType.UniChar, DbType.StringFixedLength},
            {AseDbType.Unitext, DbType.String},
            {AseDbType.UniVarChar, DbType.String},
            {AseDbType.UnsignedBigInt, DbType.UInt64},
            {AseDbType.UnsignedInt, DbType.UInt32},
            {AseDbType.UnsignedSmallInt, DbType.UInt16},
            // As per the reference driver, Guid appears to be synonymous with Unsupported.
            // Never fear! If the user explicitly sets the DbType to Guid, and uses a System.Guid Value, it will be supported.
            {AseDbType.Unsupported, DbType.Guid},
            {AseDbType.VarBinary, DbType.Binary},
            {AseDbType.VarChar, DbType.AnsiString}
        };

        public static DbType GetDbType(AseDbType aseDbType)
        {
            return AseDbTypeToDbTypeMap.ContainsKey(aseDbType)
                ? AseDbTypeToDbTypeMap[aseDbType]
                : default(DbType);
        }

        /// <summary>
        /// When a parameter's type is set, this will be called to "clean it up".
        /// In particular, the reference driver treats DateTime and BigDateTime as the same value (and both are transmitted as a TDS_BIGDATETIMEN type), so we should do the same.
        /// </summary>
        public static AseDbType CleanupAseDbType(AseDbType aseDbType)
        {
            // DateTime and BigDateTime are implemented the same way.
            if (aseDbType == AseDbType.BigDateTime)
            {
                return AseDbType.DateTime;
            }

            // it's clean, leave it alone!
            return aseDbType;
        }
    }
}
