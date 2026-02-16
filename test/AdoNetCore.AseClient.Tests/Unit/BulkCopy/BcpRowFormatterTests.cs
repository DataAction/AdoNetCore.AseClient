#if ENABLE_SYSTEM_DATA_COMMON_EXTENSIONS
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Internal.BulkCopy;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Unit.BulkCopy
{
    [TestFixture]
    public class BcpRowFormatterTests
    {
        #region Helper Methods

        private static BcpColumnMetadata MakeCol(int ordinal, string name, TdsDataType type, int length = 0,
            RowFormatItemStatus status = 0, bool isFixed = false)
        {
            return new BcpColumnMetadata
            {
                Ordinal = ordinal,
                Name = name,
                DataType = type,
                Length = length,
                RowStatus = status,
                IsFixedLength = isFixed
            };
        }

        private static DataTable CreateDataTable(params DataColumn[] columns)
        {
            var table = new DataTable();
            foreach (var col in columns)
            {
                table.Columns.Add(col);
            }
            return table;
        }

        #endregion

        #region Basic Row Format

        [Test]
        public void FormatRow_AllFixedColumns_CorrectLayout()
        {
            // Two fixed columns: INT4, INT2
            var columns = new[]
            {
                MakeCol(0, "col1", TdsDataType.TDS_INT4, isFixed: true),
                MakeCol(1, "col2", TdsDataType.TDS_INT2, isFixed: true)
            };

            var sourceOrdinals = new[] { 0, 1 };
            var formatter = new BcpRowFormatter(columns, false, Encoding.ASCII, sourceOrdinals);

            var dt = CreateDataTable(
                new DataColumn("col1", typeof(int)),
                new DataColumn("col2", typeof(short)));
            dt.Rows.Add(42, (short)7);
            var reader = dt.CreateDataReader();
            reader.Read();

            var row = formatter.FormatRow(reader);

            // Expected: [varCount=0] [rowNum=0] [INT4 data] [INT2 data]
            // No row size field or offset table since no variable columns
            Assert.AreEqual(8, row.Length);
            using (var ms = new MemoryStream(row))
            {
                Assert.AreEqual(0, ms.ReadByte()); // varCount = 0
                Assert.AreEqual(0, ms.ReadByte()); // rowNum = 0

                var intBuf = new byte[4];
                ms.Read(intBuf, 0, 4);
                Assert.AreEqual(42, BitConverter.ToInt32(intBuf, 0));

                var shortBuf = new byte[2];
                ms.Read(shortBuf, 0, 2);
                Assert.AreEqual(7, BitConverter.ToInt16(shortBuf, 0));

                Assert.AreEqual(ms.Length, ms.Position); // No more data
            }
        }

        [Test]
        public void FormatRow_AllVariableColumns_HasOffsetTable()
        {
            // Two variable columns: VARCHAR(50), INTN(4)
            var columns = new[]
            {
                MakeCol(0, "name", TdsDataType.TDS_VARCHAR, 50),
                MakeCol(1, "age", TdsDataType.TDS_INTN, 4)
            };

            var sourceOrdinals = new[] { 0, 1 };
            var formatter = new BcpRowFormatter(columns, false, Encoding.ASCII, sourceOrdinals);

            var dt = CreateDataTable(
                new DataColumn("name", typeof(string)),
                new DataColumn("age", typeof(int)));
            dt.Rows.Add("Bob", 25);
            var reader = dt.CreateDataReader();
            reader.Read();

            var row = formatter.FormatRow(reader);

            // Native BCP format:
            //   [varCount=2][rowNum=0] (pos 0-1)
            //   [2-byte row_size LE]   (pos 2-3)
            //   ["Bob" = 3 bytes]      (pos 4-6)
            //   [INT4=25 = 4 bytes]    (pos 7-10)
            //   [offset table]         (pos 11-14)
            //     offsets: [0]=4, [1]=7, [2]=11
            //     table: [count=3] [11] [7] [4]
            //   Total = 15 bytes, row_size = 15
            Assert.AreEqual(15, row.Length);
            using (var ms = new MemoryStream(row))
            {
                Assert.AreEqual(2, ms.ReadByte()); // varCount = 2
                Assert.AreEqual(0, ms.ReadByte()); // rowNum = 0

                // 2-byte row size
                var szBuf = new byte[2];
                ms.Read(szBuf, 0, 2);
                Assert.AreEqual(15, BitConverter.ToUInt16(szBuf, 0));

                // Variable data: "Bob"
                var nameBuf = new byte[3];
                ms.Read(nameBuf, 0, 3);
                Assert.AreEqual("Bob", Encoding.ASCII.GetString(nameBuf));

                // Variable data: INT4 = 25
                var intBuf = new byte[4];
                ms.Read(intBuf, 0, 4);
                Assert.AreEqual(25, BitConverter.ToInt32(intBuf, 0));

                // Offset table: [count=3] [off2=11] [off1=7] [off0=4]
                Assert.AreEqual(3, ms.ReadByte()); // count (ncols+1)
                Assert.AreEqual(11, ms.ReadByte()); // offsets[2] (end of age)
                Assert.AreEqual(7, ms.ReadByte());  // offsets[1] (end of name)
                Assert.AreEqual(4, ms.ReadByte());  // offsets[0] (start of var data)
            }
        }

        [Test]
        public void FormatRow_MixedFixedAndVariable()
        {
            // col0: INT4 (fixed), col1: VARCHAR(50) (variable)
            var columns = new[]
            {
                MakeCol(0, "id", TdsDataType.TDS_INT4, isFixed: true),
                MakeCol(1, "name", TdsDataType.TDS_VARCHAR, 50)
            };

            var sourceOrdinals = new[] { 0, 1 };
            var formatter = new BcpRowFormatter(columns, false, Encoding.ASCII, sourceOrdinals);

            var dt = CreateDataTable(
                new DataColumn("id", typeof(int)),
                new DataColumn("name", typeof(string)));
            dt.Rows.Add(1, "Alice");
            var reader = dt.CreateDataReader();
            reader.Read();

            var row = formatter.FormatRow(reader);

            // Format:
            //   [varCount=1][rowNum=0] (pos 0-1)
            //   [INT4=1]              (pos 2-5, fixed)
            //   [2-byte row_size LE]  (pos 6-7)
            //   ["Alice" = 5 bytes]   (pos 8-12)
            //   [offset table]        (pos 13-15)
            //     offsets: [0]=8, [1]=13
            //     table: [count=2] [13] [8]
            //   Total = 16, row_size = 16
            Assert.AreEqual(16, row.Length);
            using (var ms = new MemoryStream(row))
            {
                Assert.AreEqual(1, ms.ReadByte()); // varCount = 1
                Assert.AreEqual(0, ms.ReadByte()); // rowNum = 0

                // Fixed column: INT4 = 1
                var intBuf = new byte[4];
                ms.Read(intBuf, 0, 4);
                Assert.AreEqual(1, BitConverter.ToInt32(intBuf, 0));

                // 2-byte row size
                var szBuf = new byte[2];
                ms.Read(szBuf, 0, 2);
                Assert.AreEqual(16, BitConverter.ToUInt16(szBuf, 0));

                // Variable column: "Alice" = 5 bytes
                var strBuf = new byte[5];
                ms.Read(strBuf, 0, 5);
                Assert.AreEqual("Alice", Encoding.ASCII.GetString(strBuf));

                // Offset table: [count=2] [off1=13] [off0=8]
                Assert.AreEqual(2, ms.ReadByte()); // count (ncols+1)
                Assert.AreEqual(13, ms.ReadByte()); // offsets[1] (end of "Alice")
                Assert.AreEqual(8, ms.ReadByte());   // offsets[0] (start of var data)
            }
        }

        #endregion

        #region DOL Padding

        [Test]
        public void FormatRow_DatarowsLocked_Has4BytePadding()
        {
            var columns = new[]
            {
                MakeCol(0, "id", TdsDataType.TDS_INT4, isFixed: true)
            };

            var sourceOrdinals = new[] { 0 };
            var formatter = new BcpRowFormatter(columns, true, Encoding.ASCII, sourceOrdinals);

            var dt = CreateDataTable(new DataColumn("id", typeof(int)));
            dt.Rows.Add(99);
            var reader = dt.CreateDataReader();
            reader.Read();

            var row = formatter.FormatRow(reader);

            // Expected: [0,0,0,0] (DOL padding) [varCount=0] [rowNum=0] [INT4 data]
            Assert.AreEqual(10, row.Length);
            Assert.AreEqual(0, row[0]);
            Assert.AreEqual(0, row[1]);
            Assert.AreEqual(0, row[2]);
            Assert.AreEqual(0, row[3]);
            Assert.AreEqual(0, row[4]); // varCount
            Assert.AreEqual(0, row[5]); // rowNum
            Assert.AreEqual(99, BitConverter.ToInt32(row, 6));
        }

        [Test]
        public void FormatRow_NotDatarowsLocked_NoPadding()
        {
            var columns = new[]
            {
                MakeCol(0, "id", TdsDataType.TDS_INT4, isFixed: true)
            };

            var sourceOrdinals = new[] { 0 };
            var formatter = new BcpRowFormatter(columns, false, Encoding.ASCII, sourceOrdinals);

            var dt = CreateDataTable(new DataColumn("id", typeof(int)));
            dt.Rows.Add(99);
            var reader = dt.CreateDataReader();
            reader.Read();

            var row = formatter.FormatRow(reader);

            // Expected: [varCount=0] [rowNum=0] [INT4 data] - total 6 bytes
            Assert.AreEqual(6, row.Length);
            Assert.AreEqual(0, row[0]); // varCount
            Assert.AreEqual(0, row[1]); // rowNum
            Assert.AreEqual(99, BitConverter.ToInt32(row, 2));
        }

        #endregion

        #region Null Handling

        [Test]
        public void FormatRow_NullFixedColumn_ZeroFilled()
        {
            var columns = new[]
            {
                MakeCol(0, "id", TdsDataType.TDS_INT4, isFixed: true,
                    status: RowFormatItemStatus.TDS_ROW_NULLALLOWED)
            };

            var sourceOrdinals = new[] { 0 };
            var formatter = new BcpRowFormatter(columns, false, Encoding.ASCII, sourceOrdinals);

            var dt = CreateDataTable(new DataColumn("id", typeof(object)));
            dt.Rows.Add(DBNull.Value);
            var reader = dt.CreateDataReader();
            reader.Read();

            var row = formatter.FormatRow(reader);

            // Fixed null: zero-filled bytes
            Assert.AreEqual(6, row.Length);
            Assert.AreEqual(0, BitConverter.ToInt32(row, 2));
        }

        [Test]
        public void FormatRow_NullVariableColumn_SameOffset()
        {
            // Two variable columns, first is null
            var columns = new[]
            {
                MakeCol(0, "name", TdsDataType.TDS_VARCHAR, 50,
                    RowFormatItemStatus.TDS_ROW_NULLALLOWED),
                MakeCol(1, "age", TdsDataType.TDS_INTN, 4)
            };

            var sourceOrdinals = new[] { 0, 1 };
            var formatter = new BcpRowFormatter(columns, false, Encoding.ASCII, sourceOrdinals);

            var dt = CreateDataTable(
                new DataColumn("name", typeof(object)),
                new DataColumn("age", typeof(int)));
            dt.Rows.Add(DBNull.Value, 30);
            var reader = dt.CreateDataReader();
            reader.Read();

            var row = formatter.FormatRow(reader);

            // Format:
            //   [varCount=2][rowNum=0] (pos 0-1)
            //   [2-byte row_size LE]   (pos 2-3)
            //   [0 bytes for null]     (nothing)
            //   [INT4=30]              (pos 4-7)
            //   [offset table]         (pos 8-11)
            //     offsets: [0]=4, [1]=4 (null=same), [2]=8
            //     table: [count=3] [8] [4] [4]
            //   Total = 12, row_size = 12
            Assert.AreEqual(12, row.Length);
            using (var ms = new MemoryStream(row))
            {
                Assert.AreEqual(2, ms.ReadByte()); // varCount = 2
                Assert.AreEqual(0, ms.ReadByte()); // rowNum = 0

                // 2-byte row size
                var szBuf = new byte[2];
                ms.Read(szBuf, 0, 2);
                Assert.AreEqual(12, BitConverter.ToUInt16(szBuf, 0));

                // Variable data: null writes 0 bytes, INT4 writes 4 bytes
                var intBuf = new byte[4];
                ms.Read(intBuf, 0, 4);
                Assert.AreEqual(30, BitConverter.ToInt32(intBuf, 0));

                // Offset table: [count=3] [off2=8] [off1=4] [off0=4]
                Assert.AreEqual(3, ms.ReadByte()); // count (ncols+1)
                Assert.AreEqual(8, ms.ReadByte()); // offsets[2] (end of age)
                Assert.AreEqual(4, ms.ReadByte()); // offsets[1] (null = same as offsets[0])
                Assert.AreEqual(4, ms.ReadByte()); // offsets[0] (start of var data)
            }
        }

        #endregion

        #region Column Mapping Resolution

        [Test]
        public void ResolveColumnMappings_NoMappings_MapsByOrdinal()
        {
            var columns = new[]
            {
                MakeCol(0, "id", TdsDataType.TDS_INT4, isFixed: true),
                MakeCol(1, "name", TdsDataType.TDS_VARCHAR, 50),
                MakeCol(2, "value", TdsDataType.TDS_FLT8, isFixed: true)
            };

            var dt = CreateDataTable(
                new DataColumn("id", typeof(int)),
                new DataColumn("name", typeof(string)),
                new DataColumn("value", typeof(double)));
            dt.Rows.Add(1, "test", 3.14);
            var reader = dt.CreateDataReader();

            var result = BcpRowFormatter.ResolveColumnMappings(columns, reader, null);

            Assert.AreEqual(3, result.Length);
            Assert.AreEqual(0, result[0]);
            Assert.AreEqual(1, result[1]);
            Assert.AreEqual(2, result[2]);
        }

        [Test]
        public void ResolveColumnMappings_SkipsIdentityColumns()
        {
            var columns = new[]
            {
                MakeCol(0, "id", TdsDataType.TDS_INT4, isFixed: true,
                    status: RowFormatItemStatus.TDS_ROW_IDENTITY),
                MakeCol(1, "name", TdsDataType.TDS_VARCHAR, 50)
            };

            var dt = CreateDataTable(
                new DataColumn("name", typeof(string)));
            dt.Rows.Add("test");
            var reader = dt.CreateDataReader();

            var result = BcpRowFormatter.ResolveColumnMappings(columns, reader, null);

            Assert.AreEqual(2, result.Length);
            Assert.AreEqual(-1, result[0]); // identity skipped
            Assert.AreEqual(0, result[1]);  // maps to reader col 0
        }

        [Test]
        public void ResolveColumnMappings_ExplicitMappings_ByName()
        {
            var columns = new[]
            {
                MakeCol(0, "id", TdsDataType.TDS_INT4, isFixed: true),
                MakeCol(1, "name", TdsDataType.TDS_VARCHAR, 50)
            };

            var dt = CreateDataTable(
                new DataColumn("src_name", typeof(string)),
                new DataColumn("src_id", typeof(int)));
            dt.Rows.Add("test", 1);
            var reader = dt.CreateDataReader();

            var mappings = new AseBulkCopyColumnMappingCollection();
            mappings.Add("src_id", "id");
            mappings.Add("src_name", "name");

            var result = BcpRowFormatter.ResolveColumnMappings(columns, reader, mappings);

            Assert.AreEqual(2, result.Length);
            Assert.AreEqual(1, result[0]); // "id" -> reader col 1 (src_id)
            Assert.AreEqual(0, result[1]); // "name" -> reader col 0 (src_name)
        }

        [Test]
        public void ResolveColumnMappings_ExplicitMappings_ByOrdinal()
        {
            var columns = new[]
            {
                MakeCol(0, "id", TdsDataType.TDS_INT4, isFixed: true),
                MakeCol(1, "name", TdsDataType.TDS_VARCHAR, 50)
            };

            var dt = CreateDataTable(
                new DataColumn("a", typeof(string)),
                new DataColumn("b", typeof(int)));
            dt.Rows.Add("test", 1);
            var reader = dt.CreateDataReader();

            var mappings = new AseBulkCopyColumnMappingCollection();
            mappings.Add(1, 0); // reader col 1 -> server col 0
            mappings.Add(0, 1); // reader col 0 -> server col 1

            var result = BcpRowFormatter.ResolveColumnMappings(columns, reader, mappings);

            Assert.AreEqual(2, result.Length);
            Assert.AreEqual(1, result[0]);
            Assert.AreEqual(0, result[1]);
        }

        [Test]
        public void ResolveColumnMappings_UnmappedColumns_GetMinusOne()
        {
            var columns = new[]
            {
                MakeCol(0, "id", TdsDataType.TDS_INT4, isFixed: true),
                MakeCol(1, "name", TdsDataType.TDS_VARCHAR, 50),
                MakeCol(2, "extra", TdsDataType.TDS_INT4, isFixed: true)
            };

            var dt = CreateDataTable(
                new DataColumn("src_name", typeof(string)));
            dt.Rows.Add("test");
            var reader = dt.CreateDataReader();

            var mappings = new AseBulkCopyColumnMappingCollection();
            mappings.Add("src_name", "name");

            var result = BcpRowFormatter.ResolveColumnMappings(columns, reader, mappings);

            Assert.AreEqual(-1, result[0]); // unmapped
            Assert.AreEqual(0, result[1]);  // mapped
            Assert.AreEqual(-1, result[2]); // unmapped
        }

        #endregion

        #region FormatRow with object[] overload

        [Test]
        public void FormatRow_ObjectArray_Works()
        {
            var columns = new[]
            {
                MakeCol(0, "id", TdsDataType.TDS_INT4, isFixed: true),
                MakeCol(1, "name", TdsDataType.TDS_VARCHAR, 50)
            };

            var sourceOrdinals = new[] { 0, 1 };
            var formatter = new BcpRowFormatter(columns, false, Encoding.ASCII, sourceOrdinals);

            var values = new object[] { 42, "Hello" };
            var row = formatter.FormatRow(values);

            // Same format as MixedFixedAndVariable
            Assert.AreEqual(16, row.Length);
            using (var ms = new MemoryStream(row))
            {
                Assert.AreEqual(1, ms.ReadByte()); // varCount = 1
                Assert.AreEqual(0, ms.ReadByte()); // rowNum = 0

                // Fixed: INT4 = 42
                var intBuf = new byte[4];
                ms.Read(intBuf, 0, 4);
                Assert.AreEqual(42, BitConverter.ToInt32(intBuf, 0));

                // 2-byte row size
                var szBuf = new byte[2];
                ms.Read(szBuf, 0, 2);
                Assert.AreEqual(16, BitConverter.ToUInt16(szBuf, 0));

                // Variable: "Hello" = 5 bytes
                var strBuf = new byte[5];
                ms.Read(strBuf, 0, 5);
                Assert.AreEqual("Hello", Encoding.ASCII.GetString(strBuf));

                // Offset table: [count=2] [off1=13] [off0=8]
                Assert.AreEqual(2, ms.ReadByte()); // count
                Assert.AreEqual(13, ms.ReadByte()); // offsets[1]
                Assert.AreEqual(8, ms.ReadByte());  // offsets[0]
            }
        }

        #endregion
    }
}
#endif
