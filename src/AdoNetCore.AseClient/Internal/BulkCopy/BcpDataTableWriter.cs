#if ENABLE_SYSTEM_DATA_COMMON_EXTENSIONS
using System;
using System.Data;

namespace AdoNetCore.AseClient.Internal.BulkCopy
{
    /// <summary>
    /// Efficiently populates a DataTable from a streaming IDataReader,
    /// used for BCP OUT operations.
    /// </summary>
    internal static class BcpDataTableWriter
    {
        /// <summary>
        /// Fills a DataTable from the reader, building schema from reader metadata.
        /// </summary>
        public static void Fill(DataTable table, IDataReader reader, int notifyAfter, Func<long, bool> onNotify, object sender)
        {
            // Build schema from reader
            var fieldCount = reader.FieldCount;
            var columns = new DataColumn[fieldCount];
            for (var i = 0; i < fieldCount; i++)
            {
                var colName = reader.GetName(i);
                var colType = reader.GetFieldType(i);
                var col = new DataColumn(colName, colType ?? typeof(object));
                col.AllowDBNull = true;
                table.Columns.Add(col);
                columns[i] = col;
            }

            // Read rows
            var values = new object[fieldCount];
            long rowCount = 0;

            table.BeginLoadData();
            try
            {
                while (reader.Read())
                {
                    reader.GetValues(values);
                    table.Rows.Add(values);
                    rowCount++;

                    if (notifyAfter > 0 && rowCount % notifyAfter == 0)
                    {
                        if (onNotify != null && onNotify(rowCount))
                        {
                            break; // Abort requested
                        }
                    }
                }
            }
            finally
            {
                table.EndLoadData();
            }
        }
    }
}
#endif
