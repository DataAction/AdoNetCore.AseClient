using System;
using System.Collections.Generic;
using System.Data;
using AdoNetCore.AseClient.Enum;

namespace AdoNetCore.AseClient.Internal
{
    internal static class TypeMap
    {
        private static readonly Dictionary<DbType, TdsDataType> DbToTdsMap = new Dictionary<DbType, TdsDataType>
        {
            {DbType.Byte, TdsDataType.TDS_INT1},
            {DbType.Int16, TdsDataType.TDS_INT2},
            {DbType.Int32, TdsDataType.TDS_INT4},
            {DbType.Int64, TdsDataType.TDS_INT8},
            {DbType.String, TdsDataType.TDS_VARCHAR}
        };

        public static int? GetLength(DbType dbType, object value)
        {
            switch (dbType)
            {
                case DbType.String:
                    return ((string)value).Length;
                default:
                    return null;
            }
        }

        public static TdsDataType GetTdsDataType(DbType dbType, int? length)
        {
            if (!DbToTdsMap.ContainsKey(dbType))
            {
                throw new InvalidOperationException($"Unsupported data type {dbType}");
            }

            length = length ?? 0;

            switch (dbType)
            {
                case DbType.String:
                    return length <= 255
                        ? TdsDataType.TDS_VARCHAR
                        : TdsDataType.TDS_LONGCHAR;
                default:
                    return DbToTdsMap[dbType];
            }
        }
    }
}
