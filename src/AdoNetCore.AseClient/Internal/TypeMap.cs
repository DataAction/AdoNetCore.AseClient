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
            { DbType.Int32, TdsDataType.TDS_INT4 }
        };

        public static TdsDataType GetTdsDataType(DbType dbType)
        {
            if (!DbToTdsMap.ContainsKey(dbType))
            {
                throw new InvalidOperationException($"Unsupported data type {dbType}");
            }

            return DbToTdsMap[dbType];
        }
    }
}
