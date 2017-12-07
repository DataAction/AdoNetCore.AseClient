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
            {DbType.Boolean, (value, length) => TdsDataType.TDS_BIT },
            {DbType.Byte, (value, length) => value == DBNull.Value ? TdsDataType.TDS_INTN : TdsDataType.TDS_INT1 },
            {DbType.SByte, (value, length) => TdsDataType.TDS_INTN },
            {DbType.Int16, (value, length) => value == DBNull.Value ? TdsDataType.TDS_INTN : TdsDataType.TDS_INT2 },
            {DbType.UInt16, (value, length) => value == DBNull.Value ? TdsDataType.TDS_UINTN : TdsDataType.TDS_UINT2 },
            {DbType.Int32, (value, length) => value == DBNull.Value ? TdsDataType.TDS_INTN : TdsDataType.TDS_INT4 },
            {DbType.UInt32, (value, length) => value == DBNull.Value ? TdsDataType.TDS_UINTN : TdsDataType.TDS_UINT4 },
            {DbType.Int64, (value, length) => value == DBNull.Value ? TdsDataType.TDS_INTN : TdsDataType.TDS_INT8 },
            {DbType.UInt64, (value, length) => value == DBNull.Value ? TdsDataType.TDS_UINTN : TdsDataType.TDS_UINT8 },
            {DbType.String, (value, length) => length <= VarLongBoundary ? TdsDataType.TDS_VARCHAR : TdsDataType.TDS_LONGCHAR},
            {DbType.StringFixedLength, (value, length) => length <= VarLongBoundary ? TdsDataType.TDS_VARCHAR : TdsDataType.TDS_LONGCHAR},
            {DbType.Binary, (value, length) => length <= VarLongBoundary ? TdsDataType.TDS_BINARY : TdsDataType.TDS_LONGBINARY},
            {DbType.Decimal, (value, length) => TdsDataType.TDS_DECN },
            {DbType.VarNumeric, (value, length) => TdsDataType.TDS_NUMN },
            {DbType.Single, (value, length) => value == DBNull.Value ? TdsDataType.TDS_FLTN : TdsDataType.TDS_FLT4 },
            {DbType.Double, (value, length) => value == DBNull.Value ? TdsDataType.TDS_FLTN : TdsDataType.TDS_FLT8 },
            {DbType.DateTime, (value, length) => value == DBNull.Value ? TdsDataType.TDS_DATETIMEN : TdsDataType.TDS_DATETIME },
            {DbType.Date, (value, length) => value == DBNull.Value ? TdsDataType.TDS_DATEN : TdsDataType.TDS_DATE },
            {DbType.Time, (value, length) => value == DBNull.Value ? TdsDataType.TDS_TIMEN: TdsDataType.TDS_TIME }
        };

        public static int? GetLength(DbType dbType, AseDataParameter parameter, Encoding enc)
        {
            Logger.Instance?.WriteLine($"{parameter.ParameterName}: Reported size is {parameter.Size}");
            if (parameter.Size > 0)
            {
                return parameter.Size;
            }

            var value = parameter.Value;
            switch (dbType)
            {
                case DbType.String:
                    return value == DBNull.Value ? 0 : enc.GetBytes(value.ToString()).Length;
                case DbType.Binary:
                    return value == DBNull.Value ? 0 : ((byte[])value).Length;
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
                default:
                    return null;
            }
        }

        public static TdsDataType GetTdsDataType(DbType dbType, object value, int? length)
        {
            if (!DbToTdsMap.ContainsKey(dbType))
            {
                throw new InvalidOperationException($"Unsupported data type {dbType}");
            }
            return DbToTdsMap[dbType](value, length ?? 0);
        }
    }
}
