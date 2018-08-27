#if NETCOREAPP2_0
using System;
using System.Data;
using AdoNetCore.AseClient.Internal;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration
{
    [TestFixture]
    [Category("extra")]
    public class DataSetTests
    {
        [Test]
        public void SingleTable_Load_Succeeds()
        {
            Logger.Enable();
            using (var connection = new AseConnection(ConnectionStrings.Pooled))
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = "select * from (select 'asdf' as stringCol, 1 as intCol union select 'derp' as stringCol, 2 as intCol) x";
                command.CommandType = CommandType.Text;

                var ds = new DataSet();
                
                using (var reader = command.ExecuteReader())
                {
                    var tbl = new DataTable("table_1");
                    ds.Tables.Add(tbl);
                    ds.Load(reader,
                        LoadOption.OverwriteChanges,
                        (sender, args) => Console.WriteLine($"FillErrorEventHandler - {args}"),
                        tbl);
                }

                var table1 = ds.Tables["table_1"];
                var stringCol = table1.Columns["stringCol"];
                var intCol = table1.Columns["intCol"];

                var row = table1.Rows[0];

                Assert.AreEqual("asdf", Convert.ToString(row[stringCol]));
                Assert.AreEqual(1, Convert.ToInt32(row[intCol]));
            }
        }
    }
}
#endif
