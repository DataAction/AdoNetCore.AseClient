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
            {DbType.Decimal, (value, length) => TdsDataType.TDS_DECN},
            {DbType.Currency, (value, length) => TdsDataType.TDS_MONEYN},
            {DbType.VarNumeric, (value, length) => TdsDataType.TDS_NUMN},
            {DbType.Single, (value, length) => value == DBNull.Value ? TdsDataType.TDS_FLTN : TdsDataType.TDS_FLT4},
            {DbType.Double, (value, length) => value == DBNull.Value ? TdsDataType.TDS_FLTN : TdsDataType.TDS_FLT8},
            {DbType.DateTime, (value, length) => value == DBNull.Value ? TdsDataType.TDS_DATETIMEN : TdsDataType.TDS_DATETIME},
            {DbType.Date, (value, length) => value == DBNull.Value ? TdsDataType.TDS_DATEN : TdsDataType.TDS_DATE},
            {DbType.Time, (value, length) => value == DBNull.Value ? TdsDataType.TDS_TIMEN : TdsDataType.TDS_TIME}
        };

        private static readonly Dictionary<Type, Func<object, int, TdsDataType>> NetTypeToTdsMap = new Dictionary<Type, Func<object, int, TdsDataType>>
        {
            {typeof(bool), (value, length) => TdsDataType.TDS_BIT},
            {typeof(byte), (value, length) => value == DBNull.Value ? TdsDataType.TDS_INTN : TdsDataType.TDS_INT1},
            {typeof(sbyte), (value, length) => TdsDataType.TDS_INTN},
            {typeof(short), (value, length) => value == DBNull.Value ? TdsDataType.TDS_INTN : TdsDataType.TDS_INT2},
            {typeof(ushort), (value, length) => value == DBNull.Value ? TdsDataType.TDS_UINTN : TdsDataType.TDS_UINT2},
            {typeof(int), (value, length) => value == DBNull.Value ? TdsDataType.TDS_INTN : TdsDataType.TDS_INT4},
            {typeof(uint), (value, length) => value == DBNull.Value ? TdsDataType.TDS_UINTN : TdsDataType.TDS_UINT4},
            {typeof(long), (value, length) => value == DBNull.Value ? TdsDataType.TDS_INTN : TdsDataType.TDS_INT8},
            {typeof(ulong), (value, length) => value == DBNull.Value ? TdsDataType.TDS_UINTN : TdsDataType.TDS_UINT8},
            {typeof(char), (value, length) => TdsDataType.TDS_CHAR},
            {typeof(char[]), (value, length) => TdsDataType.TDS_CHAR},
            {typeof(string), (value, length) => TdsDataType.TDS_LONGBINARY},
            {typeof(byte[]), (value, length) => length <= VarLongBoundary ? TdsDataType.TDS_BINARY : TdsDataType.TDS_LONGBINARY},
            {typeof(Guid), (value, length) => TdsDataType.TDS_BINARY},
            {typeof(decimal), (value, length) => TdsDataType.TDS_DECN},
            {typeof(float), (value, length) => value == DBNull.Value ? TdsDataType.TDS_FLTN : TdsDataType.TDS_FLT4},
            {typeof(double), (value, length) => value == DBNull.Value ? TdsDataType.TDS_FLTN : TdsDataType.TDS_FLT8},
            {typeof(DateTime), (value, length) => value == DBNull.Value ? TdsDataType.TDS_DATETIMEN : TdsDataType.TDS_DATETIME}
        };

        public static int? GetFormatLength(DbType dbType, AseParameter parameter, Encoding enc)
        {
            if (parameter.Size > 0)
            {
                return parameter.Size;
            }

            var value = parameter.Value;
            switch (dbType)
            {
                case DbType.String:
                case DbType.StringFixedLength:
                    switch (value)
                    {
                        case string s:
                            return Encoding.Unicode.GetByteCount(s);
                        case char c:
                            return Encoding.Unicode.GetByteCount(new[] { c });
                        default:
                            return 0;
                    }
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                    switch (value)
                    {
                        case string s:
                            return enc.GetByteCount(s);
                        case char c:
                            return enc.GetByteCount(new[] { c });
                        default:
                            return 0;
                    }
                case DbType.Binary:
                    switch (value)
                    {
                        case byte[] ba:
                            return ba.Length;
                        case byte _:
                            return 1;
                        default:
                            return 0;
                    }
                case DbType.Decimal:
                case DbType.VarNumeric:
                    return 17; //1 byte pos/neg, 16 bytes data
                case DbType.Boolean:
                case DbType.Byte:
                    return 1;
                case DbType.Int16:
                case DbType.UInt16:
                case DbType.SByte://can't seem to write an sbyte as a single byte, so it'll get encoded in a short
                    return 2;
                case DbType.Int32:
                case DbType.UInt32:
                    return 4;
                case DbType.Int64:
                case DbType.UInt64:
                    return 8;
                case DbType.Single:
                    return 4;
                case DbType.Double:
                    return 8;
                case DbType.DateTime:
                    return 8;
                case DbType.Date:
                    return 4;
                case DbType.Time:
                    return 4;
                case DbType.Guid:
                    return 16;
                case DbType.Currency:
                    return 8;
                default:
                    return null;
            }
        }

        public static TdsDataType GetTdsDataType(DbType dbType, bool dbTypeIsKnown, object value, int? length)
        {
            // If the consumer has explicitly set a type, then rely on that.
            if (dbTypeIsKnown && DbToTdsMap.TryGetValue(dbType, out var result))
            {
                return result(value, length ?? 0);
            }

            // If that is not set, then we should try to infer the type;
            if (NetTypeToTdsMap.TryGetValue(value.GetType(), out result))
            {
                return result(value, length ?? 0);
            }

            throw new NotSupportedException($"Unsupported data type {dbType}");
        }

        private static readonly Dictionary<TdsDataType, Func<FormatItem, Type>> TdsToNetMap = new Dictionary<TdsDataType, Func<FormatItem, Type>>
        {
            {TdsDataType.TDS_BINARY, f => typeof(byte[])},
            {TdsDataType.TDS_BIT, f => typeof(bool)},
            {TdsDataType.TDS_BLOB, f => typeof(byte[])},
            //{TdsDataType.TDS_BOUNDARY, f => typeof()},
            {TdsDataType.TDS_CHAR, f => typeof(string)},
            {TdsDataType.TDS_DATE, f => typeof(DateTime)},
            {TdsDataType.TDS_DATEN, f => typeof(DateTime?)},
            {TdsDataType.TDS_DATETIME, f => typeof(DateTime)},
            {TdsDataType.TDS_DATETIMEN, f => typeof(DateTime?)},
            {TdsDataType.TDS_DECN, f => typeof(decimal?)},
            {TdsDataType.TDS_FLT4, f => typeof(float)},
            {TdsDataType.TDS_FLT8, f => typeof(double)},
            {
                TdsDataType.TDS_FLTN, f => f.Length == 8
                    ? typeof(double?)
                    : typeof(float?)
            },
            {TdsDataType.TDS_IMAGE, f => typeof(byte[])},
            {TdsDataType.TDS_INT1, f => typeof(byte)},
            {TdsDataType.TDS_INT2, f => typeof(short)},
            {TdsDataType.TDS_INT4, f => typeof(int)},
            {TdsDataType.TDS_INT8, f => typeof(long)},
            //{TdsDataType.TDS_INTERVAL, f => typeof()},
            {
                TdsDataType.TDS_INTN, f => f.Length == 8
                    ? typeof(long?)
                    : f.Length == 4
                        ? typeof(int?)
                        : f.Length == 2
                            ? typeof(short?)
                            : typeof(byte?)
            },
            {
                TdsDataType.TDS_LONGBINARY, f => f.UserType == 34 || f.UserType == 35
                    ? typeof(string)
                    : typeof(byte[])
            },
            {TdsDataType.TDS_LONGCHAR, f => typeof(string)},
            {TdsDataType.TDS_MONEY, f => typeof(decimal)},
            {TdsDataType.TDS_MONEYN, f => typeof(decimal?)},
            {TdsDataType.TDS_NUMN, f => typeof(decimal?)},
            //{TdsDataType.TDS_SENSITIVITY,f =>  typeof()},
            {TdsDataType.TDS_SHORTDATE, f => typeof(DateTime)},
            {TdsDataType.TDS_SHORTMONEY, f => typeof(decimal)},
            {TdsDataType.TDS_SINT1, f => typeof(sbyte)},
            {TdsDataType.TDS_TEXT, f => typeof(string)},
            {TdsDataType.TDS_TIME, f => typeof(TimeSpan)},
            {TdsDataType.TDS_TIMEN, f => typeof(TimeSpan?)},
            {TdsDataType.TDS_UINT2, f => typeof(ushort)},
            {TdsDataType.TDS_UINT4, f => typeof(uint)},
            {TdsDataType.TDS_UINT8, f => typeof(ulong)},
            {
                TdsDataType.TDS_UINTN, f => f.Length == 8
                    ? typeof(ulong?)
                    : f.Length == 4
                        ? typeof(uint?)
                        : f.Length == 2
                            ? typeof(ushort?)
                            : typeof(byte?)
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
            {TdsDataType.TDS_LONGCHAR, f => AseDbType.LongVarChar},
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
    }
}
