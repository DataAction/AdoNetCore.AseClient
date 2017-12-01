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
            {DbType.Byte, (value, length) => value == DBNull.Value ? TdsDataType.TDS_INTN : TdsDataType.TDS_INT1 },
            {DbType.Int16, (value, length) => value == DBNull.Value ? TdsDataType.TDS_INTN : TdsDataType.TDS_INT2 },
            {DbType.Int32, (value, length) => value == DBNull.Value ? TdsDataType.TDS_INTN : TdsDataType.TDS_INT4 },
            {DbType.Int64, (value, length) => value == DBNull.Value ? TdsDataType.TDS_INTN : TdsDataType.TDS_INT8 },
            {DbType.String, (value, length) => length <= VarLongBoundary ? TdsDataType.TDS_VARCHAR : TdsDataType.TDS_LONGCHAR},
            {DbType.Binary, (value, length) => length <= VarLongBoundary ? TdsDataType.TDS_BINARY : TdsDataType.TDS_LONGBINARY}
        };

        public static int? GetLength(DbType dbType, object value, Encoding enc)
        {
            if (value == DBNull.Value)
            {
                return null;
            }

            switch (dbType)
            {
                case DbType.String:
                    return enc.GetBytes((string) value).Length;
                case DbType.Binary:
                    return ((byte[]) value).Length;
                default:
                    return null;
            }
        }

        public static int? GetPrecision(DbType dbType, object value)
        {
            if (value == DBNull.Value)
            {
                return null;
            }

            switch (dbType)
            {
                case DbType.Decimal:
                    //???
                    return null;
                default:
                    return null;
            }
        }

        public static int? GetScale(DbType dbType, object value)
        {
            if (value == DBNull.Value)
            {
                return null;
            }

            switch (dbType)
            {
                case DbType.Decimal:
                    //???
                    return null;
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
