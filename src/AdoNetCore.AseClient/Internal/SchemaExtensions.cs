#if DB_GETSCHEMA
using System;
using System.Data;

namespace AdoNetCore.AseClient.Internal
{
    internal static class SchemaExtensions
    {
        public static void LoadDataTypeRow(this DataTable table, string typeName,
            AseDbType providerDbType,
            long columnSize,
            string createFormat = null,
            string createParameters = null,
            Type netType = null,
            bool isAutoIncrementable = false,
            bool isBestMatch = false,
            bool isCaseSensitive = false,
            bool isFixedLength = false,
            bool isFixedPrecisionScale = false,
            bool isLong = false,
            bool isNullable = false,
            bool isSearchable = false,
            bool isSearchableWithLike = false,
            bool? isUnsigned = null,
            short? maximumScale = null,
            short? minimumScale = null,
            bool isConcurrencyType = false,
            bool isLiteralSupported = false,
            string literalPrefix = null,
            string literalSuffix = null)
        {
            table.LoadDataRow(
                new object[]
                {
                    typeName,
                    providerDbType,
                    columnSize,
                    createFormat ?? typeName,
                    createParameters,
                    netType?.FullName ?? string.Empty,
                    isAutoIncrementable,
                    isBestMatch,
                    isCaseSensitive,
                    isFixedLength,
                    isFixedPrecisionScale,
                    isLong,
                    isNullable,
                    isSearchable,
                    isSearchableWithLike,
                    isUnsigned,
                    maximumScale,
                    minimumScale,
                    isConcurrencyType,
                    isLiteralSupported,
                    literalPrefix,
                    literalSuffix
                },
                true);
        }
    }
}
#endif
