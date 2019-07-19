using System;
using Dapper;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Util
{
    public static class CharsetUtility
    {
        public static bool IsCharset(string connectionString, string expectedCharSet)
        {
            using (var connection = new AseConnection(connectionString))
            {
                using (var reader = connection.ExecuteReader("sp_helpsort"))
                {
                    Assert.IsTrue(reader.NextResult());
                    Assert.IsTrue(reader.NextResult());
                    Assert.IsTrue(reader.Read());

                    var infoRow = reader.GetString(0);

                    return infoRow.IndexOf(expectedCharSet, StringComparison.OrdinalIgnoreCase) >= 0;
                }
            }
        }
    }
}
