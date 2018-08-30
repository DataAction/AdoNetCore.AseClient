using System;
using System.Collections.Generic;
using System.Data.Common;
using Dapper;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration.Insert
{
    public static class DateTimeTestHelper
    {
        public static IEnumerable<DateTime?> TestValues = new List<DateTime?>
        {
            null,
            new DateTime(1900, 01, 01, 0, 0, 0, 0),
            new DateTime(1900, 01, 01, 0, 44, 33, 876),
            new DateTime(1900, 01, 01, 12, 12, 12),
            new DateTime(1900, 01, 01, 14, 44, 33, 233),
            new DateTime(1900, 01, 01, 22, 44, 33, 0),
            new DateTime(1900, 01, 01, 23, 59, 59, 996),
            new DateTime(1900, 01, 01, 23, 59, 59, 996),
            new DateTime(1900, 01, 01, 9, 44, 33, 886),
            new DateTime(1753, 1, 1, 0, 0, 0),
            new DateTime(1753, 1, 1, 23, 59, 59),
            new DateTime(1753, 1, 1, 23, 59, 59, 996),
            new DateTime(1753, 12, 31, 0, 0, 0),
            new DateTime(1753, 12, 31, 23, 59, 59),
            new DateTime(1753, 12, 31, 23, 59, 59, 996),
            new DateTime(1900, 1, 1, 0, 0, 0),
            new DateTime(1900, 1, 1, 23, 59, 59),
            new DateTime(1900, 1, 1, 23, 59, 59, 996),
            new DateTime(1900, 12, 31, 0, 0, 0),
            new DateTime(1900, 12, 31, 23, 59, 59),
            new DateTime(1900, 12, 31, 23, 59, 59, 996),
            new DateTime(9999, 01, 01, 0, 0, 0),
            new DateTime(9999, 01, 01, 23, 59, 59),
            new DateTime(9999, 01, 01, 23, 59, 59, 996),
            new DateTime(9999, 12, 31, 0, 0, 0),
            new DateTime(9999, 12, 31, 23, 59, 59),
            new DateTime(9999, 12, 31, 23, 59, 59, 996),
            new DateTime(0001, 01, 01),
            new DateTime(2000, 11, 23),
            new DateTime(2123, 11, 23),
            new DateTime(3210, 11, 23),
            new DateTime(9999, 12, 31),
        };

        public static void SetAseDbType(object p, string type)
        {
#if NET_FRAMEWORK
            if (p is Sybase.Data.AseClient.AseParameter aseParameter)
            {
                aseParameter.AseDbType = (Sybase.Data.AseClient.AseDbType)System.Enum.Parse(typeof(Sybase.Data.AseClient.AseDbType), type);
            }
#endif
            if (p is AseParameter coreParameter)
            {
                coreParameter.AseDbType = (AseDbType)System.Enum.Parse(typeof(AseDbType), type);
            }
        }

        public static void Insert_Parameter_VerifyResult(Func<DbConnection> getConnection, string table, string field, DateTime? expected)
        {
            using (var connection = getConnection())
            {
                Assert.AreEqual(expected, connection.QuerySingle<DateTime?>($"select top 1 {field} from [dbo].[{table}]"));
            }
        }
    }
}
