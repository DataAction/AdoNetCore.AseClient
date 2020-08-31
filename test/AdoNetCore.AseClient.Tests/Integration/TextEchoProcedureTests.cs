using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AdoNetCore.AseClient.Internal;
using Dapper;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration
{
    [TestFixture]
    [Category("extra")]
    public class TextEchoProcedureTests
    {
        //echo some bytes
        private readonly string _createEchoTextProc = @"
create procedure [dbo].[sp_test_echo_text]
  @input text,
  @output text output
as
begin
  set @output = @input
end";

        private readonly string _dropEchoTextProc = @"drop procedure [dbo].[sp_test_echo_text]";

        private readonly string _createTextTable = @"create table [dbo].[test_text_table] (Fragment text not null, CreatedDate datetime default getdate() not null)";

        private readonly string _dropTextTable = @"drop table [dbo].[test_text_table]";

        private readonly string _createEchoTextLocatorProc = @"
create procedure [dbo].[sp_test_echo_text_locator]
  @input text_locator,
  @output int output
as
begin
  set @output = 100
end";
        private readonly string _dropEchoTextLocatorProc = @"drop procedure [dbo].[sp_test_echo_text_locator]";

        private readonly string _createTrueEchoTextLocatorProc = @"
create procedure [dbo].[sp_test_true_echo_text_locator]
  @input text_locator,
  @output text output
as
DECLARE
    @sCommentTL     text_locator

begin
  SELECT @sCommentTL = create_locator(text_locator, @input)
  set @output = @input
  --select @sCommentTL
end";

        private readonly string _dropTrueEchoTextLocatorProc = @"drop procedure [dbo].[sp_test_true_echo_text_locator]";

        private readonly string _createTextTypeColumnProc = @"
create procedure [dbo].[sp_test_text_type_coloumn]
  @text_locator_input text_locator,
  @text_input text,
  @output int output
as
DECLARE
    @sCommentTL     text_locator

begin
  --SELECT @sCommentTL = create_locator(text_locator, @text_input)
  set @output = 10
  --select @sCommentTL
end";

        private readonly string _dropTextTypeColumnProc = @"drop procedure [dbo].[sp_test_text_type_coloumn]";

        private readonly string _createJustTextProc = @"
create procedure [dbo].[sp_just_test_echo_text]
  @input text,
  @output int output
as
begin
  set @output = 10
end";
        private readonly string _dropJustTextProc = @"drop procedure [dbo].[sp_just_test_echo_text]";


        private readonly string _createInsertTextProc = @"
create procedure [dbo].[sp_insert_test_echo_text]
  @input text,
  @output int output
as
begin
  insert into [dbo].[test_text_table] (Fragment) VALUES (@input)
  set @output = 10
end";

        private readonly string _dropInsertTextProc = @"drop procedure [dbo].[sp_insert_test_echo_text]";

        public TextEchoProcedureTests()
        {
            Logger.Enable();
        }

        [OneTimeSetUp]
        public void Setup()
        {
            using (var connection = new AseConnection(ConnectionStrings.Pooled))
            {
                connection.Execute(_createEchoTextProc);
                connection.Execute(_createTextTable);
                connection.Execute(_createEchoTextLocatorProc);
                connection.Execute(_createTrueEchoTextLocatorProc);
                connection.Execute(_createTextTypeColumnProc);
                connection.Execute(_createJustTextProc);
                connection.Execute(_createInsertTextProc);
            }
        }

        [OneTimeTearDown]
        public void Teardown()
        {
            using (var connection = new AseConnection(ConnectionStrings.Pooled))
            {
                connection.Execute(_dropInsertTextProc);
                connection.Execute(_dropEchoTextProc);
                connection.Execute(_dropTextTable);
                connection.Execute(_dropEchoTextLocatorProc);
                connection.Execute(_dropTrueEchoTextLocatorProc);
                connection.Execute(_dropTextTypeColumnProc);
                connection.Execute(_dropJustTextProc);
            }
        }


        [Test, Ignore("Output text type")]
        public void EchoText_Procedure_ShouldExecute()
        {
            using (var connection = new AseConnection(ConnectionStrings.Pooled))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "sp_test_echo_text";
                    command.CommandType = CommandType.StoredProcedure;

                    var expected = new string('x', 16384);

                    var p = command.CreateParameter();
                    p.ParameterName = "@input";
                    p.Value = expected;
                    p.DbType = DbType.String;
                    command.Parameters.Add(p);

                    var pOut = command.CreateParameter();
                    pOut.ParameterName = "@output";
                    pOut.Value = DBNull.Value;
                    pOut.DbType = DbType.String;
                    pOut.Direction = ParameterDirection.Output;
                    command.Parameters.Add(pOut);

                    command.ExecuteNonQuery();

                    Assert.AreEqual(expected, pOut.Value);
                }
            }
        }

        [Test]
        public void TextLocator_Procedure_ShouldExecute()
        {
            using (var connection = new AseConnection(ConnectionStrings.Pooled))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "sp_test_echo_text_locator";
                    command.CommandType = CommandType.StoredProcedure;

                    var expected = $"Shawn test long comment {new string('A',1063840)}";

                    var p = command.CreateParameter();
                    p.ParameterName = "@input";
                    p.Value = expected;
                    p.DbType = DbType.String;
                    command.Parameters.Add(p);

                    var pOut = command.CreateParameter();
                    pOut.ParameterName = "@output";
                    pOut.Value = DBNull.Value;
                    pOut.DbType = DbType.Int32;
                    pOut.Direction = ParameterDirection.Output;
                    command.Parameters.Add(pOut);

                    command.ExecuteNonQuery();

                }
            }
        }

        [Test]
        public void TextTypeColumn_Procedure_ShouldExecute()
        {
            using (var connection = new AseConnection(ConnectionStrings.Pooled))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "sp_test_text_type_coloumn";
                    command.CommandType = CommandType.StoredProcedure;

                    //var expected = Enumerable.Repeat(new byte[] {0xde, 0xad, 0xbe, 0xef}, 64).SelectMany(x => x).Take(536386).ToArray();
                    var text_locator = new string('x', 16384);
                    var just_text = new string('y', 1024);

                    var p = command.CreateParameter();
                    p.ParameterName = "@text_locator_input";
                    p.Value = text_locator;
                    p.DbType = DbType.String;
                    //p.OverrideUserType = 36; //19;
                    command.Parameters.Add(p);

                    p = command.CreateParameter();
                    p.ParameterName = "@text_input";
                    p.Value = just_text;
                    p.DbType = DbType.String;
                    //p.OverrideUserType = 36;
                    command.Parameters.Add(p);

                    var pOut = command.CreateParameter();
                    pOut.ParameterName = "@output";
                    pOut.Value = DBNull.Value;
                    pOut.DbType = DbType.Int32;
                    pOut.Direction = ParameterDirection.Output;
                    command.Parameters.Add(pOut);

                    command.ExecuteNonQuery();

                    //Assert.AreEqual(expected, pOut.Value);
                }
            }
        }

        [Test]
        public void TextType_Proc_ShouldExecute_Ok()
        {
            //AdoNetCore.AseClient.AseException : The token datastream length was not correct. This is an internal protocol error.
            using (var connection = new AseConnection(ConnectionStrings.Pooled))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "sp_test_text_type_coloumn";
                    command.CommandType = CommandType.StoredProcedure;

                    //var expected = Enumerable.Repeat(new byte[] {0xde, 0xad, 0xbe, 0xef}, 64).SelectMany(x => x).Take(536386).ToArray();
                    var text_locator = new string('x', 106384);
                    var just_text = new string('y', 1024);

                    var p = command.CreateParameter();
                    p.ParameterName = "@text_locator_input";
                    p.Value = text_locator;
                    p.DbType = DbType.String;
                    command.Parameters.Add(p);

                    p = command.CreateParameter();
                    p.ParameterName = "@text_input";
                    p.Value = just_text;
                    p.DbType = DbType.String;
                    command.Parameters.Add(p);

                    var pOut = command.CreateParameter();
                    pOut.ParameterName = "@output";
                    pOut.Value = DBNull.Value;
                    pOut.DbType = DbType.Int32;
                    pOut.Direction = ParameterDirection.Output;
                    command.Parameters.Add(pOut);

                    command.ExecuteNonQuery();
                }
            }
        }

        [TestCaseSource(nameof(Insert_Text_Length))]
        public void TextTypeColumn_Proc_ShouldExecute_Ok(int length)
        {
            //AdoNetCore.AseClient.AseException : The token datastream length was not correct. This is an internal protocol error.
            using (var connection = new AseConnection(ConnectionStrings.Pooled))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "sp_just_test_echo_text";
                    command.CommandType = CommandType.StoredProcedure;

                    //var expected = Enumerable.Repeat(new byte[] {0xde, 0xad, 0xbe, 0xef}, 64).SelectMany(x => x).Take(536386).ToArray();
                    //var text_locator = new string('x', 16383);
                    var just_text = new string('y', length);

                    var p = command.CreateParameter();

                    p = command.CreateParameter();
                    p.ParameterName = "@input";
                    p.Value = just_text;
                    p.DbType = DbType.String;
                    command.Parameters.Add(p);

                    var pOut = command.CreateParameter();
                    pOut.ParameterName = "@output";
                    pOut.Value = DBNull.Value;
                    pOut.DbType = DbType.Int32;
                    pOut.Direction = ParameterDirection.Output;
                    command.Parameters.Add(pOut);

                    command.ExecuteNonQuery();
                }
            }
        }


        [Test]
        public void AssertTrue()
        {
            Assert.IsTrue(true);
        }

        [TestCaseSource(nameof(Insert_Text_Length))]
        public void TextColumn_With_Data_Length_MoreThan_16384_ReturnSuccess(int length)
        {
            using (var connection = new AseConnection(ConnectionStrings.Pooled))
            {
                using (var transaction = connection.BeginTransaction())
                {
                    using (var fragmentCommand = connection.CreateCommand())
                    {
                        fragmentCommand.CommandText = "insert into test_text_table (Fragment) VALUES (@fragment)";
                        fragmentCommand.CommandType = CommandType.Text;
                        fragmentCommand.Transaction = transaction;

                        var fragment = new string('x', length);
                        var p = fragmentCommand.CreateParameter();
                        p.ParameterName = @"@fragment";
                        p.Value = fragment;
                        p.DbType = DbType.String;
                        p.Direction = ParameterDirection.Input;

                        fragmentCommand.Parameters.Add(p);

                        var status = fragmentCommand.ExecuteNonQuery();
                        transaction.Commit();
                    }
                }
            }
        }

        //[TestCaseSource(nameof(Insert_Text_Length_GreaterThan_10000000))]
        //sp_configure 'procedure cache size', 24000
        [TestCase(2000)]
        [TestCase(4000)]
        [TestCase(24000)]
        [TestCase(26000)]
        [TestCase(28000)]
        [TestCase(30000)]
        //[TestCase(20001000)]
        //[TestCase(20001500)]
        //[TestCase(20002000)]
        //[TestCase(20002500)]
        //[TestCase(20003000)]
        //[TestCase(20003500)]
        //[TestCase(20004000)]
        //[TestCase(20004500)]
        //[TestCase(20005000)]
        //[TestCase(20005500)]
        //[TestCase(20006000)]
        public void TextColumn_Length_gt_10000000_ReturnError(int length)
        {
            using (var connection = new AseConnection(ConnectionStrings.Pooled))
            {
                using (var transaction = connection.BeginTransaction())
                {
                    using (var fragmentCommand = connection.CreateCommand())
                    {
                        fragmentCommand.CommandText = "insert into test_text_table (Fragment) VALUES (@fragment)";
                        fragmentCommand.CommandType = CommandType.Text;
                        fragmentCommand.Transaction = transaction;

                        var fragment = new string('x', length);
                        var p = fragmentCommand.CreateParameter();
                        p.ParameterName = @"@fragment";
                        p.Value = fragment;
                        p.DbType = DbType.String;
                        p.Direction = ParameterDirection.Input;

                        fragmentCommand.Parameters.Add(p);

                        var status = fragmentCommand.ExecuteNonQuery();
                        transaction.Commit();
                    }
                }
            }
        }


        [TestCaseSource(nameof(Insert_Text_Length))]
        public void TextType_Column_Should_Insert_Via_StoredProc_Ok(int length)
        {
            //AdoNetCore.AseClient.AseException : The token datastream length was not correct. This is an internal protocol error.
            using (var connection = new AseConnection(ConnectionStrings.Pooled))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "sp_insert_test_echo_text";
                    command.CommandType = CommandType.StoredProcedure;

                    //var expected = Enumerable.Repeat(new byte[] {0xde, 0xad, 0xbe, 0xef}, 64).SelectMany(x => x).Take(536386).ToArray();
                    //var text_locator = new string('x', 16383);
                    var just_text = new string('y', length);

                    var p = command.CreateParameter();

                    p = command.CreateParameter();
                    p.ParameterName = "@input";
                    p.Value = just_text;
                    p.DbType = DbType.String;
                    command.Parameters.Add(p);

                    var pOut = command.CreateParameter();
                    pOut.ParameterName = "@output";
                    pOut.Value = DBNull.Value;
                    pOut.DbType = DbType.Int32;
                    pOut.Direction = ParameterDirection.Output;
                    command.Parameters.Add(pOut);

                    command.ExecuteNonQuery();
                }
            }
        }

        [Test]
        public void Insert_Text_ShouldBeValid()
        {
            using (var connection = new AseConnection(ConnectionStrings.Pooled))
            {
                using (var transaction = connection.BeginTransaction())
                {
                    using (var fragmentCommand = connection.CreateCommand())
                    {
                        fragmentCommand.CommandText = "insert into test_text_table (Fragment) VALUES (@fragment)";
                        fragmentCommand.CommandType = CommandType.Text;
                        fragmentCommand.Transaction = transaction;

                        //var bytesSegmentSize = 16382;
                        var bytesSegmentSize = 2048;
                        var data = Enumerable.Repeat((byte) 0x65, 16384).ToArray();
                        //var data = Enumerable.Repeat((byte) 0x65, 6380).ToArray();
                        var totalSegment = GetTotalSegments(data.Length, bytesSegmentSize );

                        var pos = 0;
                        for (var i = 1; i <= totalSegment; i++)
                        {
                            var len = data.Length - pos >= bytesSegmentSize
                                ? bytesSegmentSize
                                : (data.Length - pos);

                            var fragment = new byte[len];
                            Buffer.BlockCopy(data, pos, fragment, 0, len);

                            fragmentCommand.Parameters.Clear();
                            var p = fragmentCommand.CreateParameter();
                            p.ParameterName = @"@fragment";
                            p.Value = Convert.ToBase64String(fragment);
                            p.DbType = DbType.String;
                            p.Direction = ParameterDirection.Input;

                            fragmentCommand.Parameters.Add(p);

                            var status = fragmentCommand.ExecuteNonQuery();
                            pos += len;
                        }

                        var count = connection.Execute("Select Fragment from test_text_table");
                        transaction.Rollback();
                    }
                }
            }
        }

        private static int GetTotalSegments(int attachmentSize, int attachmentSegmentSize)
        {
            var total = attachmentSize / attachmentSegmentSize;
            return (total > 0 && attachmentSize % attachmentSegmentSize == 0) ? total : total + 1;
        }

        public static IEnumerable<int> Insert_Text_Length()
        {
            yield return 4096;
            yield return 4097;
            yield return 8192;
            yield return 8193;
            yield return 10000;
            yield return 16384;
            yield return 16385;
            yield return 100000;
            yield return 1000000;
            yield return 10000000;
            //yield return 100000000;
            //yield return 1000000000;
        }

        public static IEnumerable<int> Insert_Text_Length_GreaterThan_10000000()
        {
            yield return 20000500;
            yield return 20001000;
            yield return 20001500;
            yield return 20002000;
            yield return 20002500;
            yield return 20003000;
            yield return 20003500;
            yield return 20004000;
            yield return 20004500;
            yield return 20005000;
            yield return 20005500;
            yield return 20006000;
        }

    }
}
