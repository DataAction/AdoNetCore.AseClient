using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Internal.Handler;
using AdoNetCore.AseClient.Token;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Unit
{
    [TestFixture]
    [Category("quick")]
    public class MessageTokenHandlerTests
    {
        [Test]
        public void AssertNoErrors_EmitsAseException_WithErrorsInOrderOfSeverity()
        {
            var handler = new MessageTokenHandler();
            handler.Handle(new EedToken
            {
                Severity = 11,
                ServerName = "",
                LineNumber = 1,
                Message = "Least Severe",
                MessageNumber = 20000,
                ProcedureName = "",
                SqlState = new byte[0],
                State = 1,
                Status = EedToken.EedStatus.TDS_EED_INFO,
                TransactionStatus = TranState.TDS_NOT_IN_TRAN
            });
            handler.Handle(new EedToken
            {
                Severity = 21,
                ServerName = "",
                LineNumber = 1,
                Message = "Most Severe",
                MessageNumber = 20000,
                ProcedureName = "",
                SqlState = new byte[0],
                State = 1,
                Status = EedToken.EedStatus.TDS_EED_INFO,
                TransactionStatus = TranState.TDS_NOT_IN_TRAN
            });
            var ex = Assert.Throws<AseException>(() => handler.AssertNoErrors());
            Assert.AreEqual("Most Severe", ex.Message);
        }

        [Test]
        public void AssertCanDetermineBackupErrors()
        {
            var handler = new MessageTokenHandler();
            handler.Handle(new EedToken
            {
                Severity = 1,
                ServerName = "AZ3_BS",
                LineNumber = 0,
                Message = "Backup Server: 4.172.1.4: The value of 'allocated pages threshold' has been set to 40%.\n", // This is severity 1, i.e. informational
                MessageNumber = 417201,
                ProcedureName = "bs_optimize",
                SqlState = new byte[0],
                State = 40,
                Status = EedToken.EedStatus.TDS_NO_EED,
                TransactionStatus = TranState.TDS_NOT_IN_TRAN
            });
            handler.Handle(new EedToken
            {
                Severity = 2, // This is not >10, but severity "2" in the backup server's message, so must be treated as error. 
                ServerName = "AZ3_BS",
                LineNumber = 0,
                Message = "Backup Server: 4.141.2.40: [11] The 'open' call failed for database/archive device while working on stripe device '/doesnotexist/foo' with error number 2 (No such file or directory). Refer to your operating system documentation for further details.\0",
                MessageNumber = 414102,
                ProcedureName = "bs_write_header",
                SqlState = new byte[0],
                State = 40,
                Status = EedToken.EedStatus.TDS_NO_EED,
                TransactionStatus = TranState.TDS_NOT_IN_TRAN
            });
            handler.Handle(new EedToken
            {
                Severity = 16,
                ServerName = "AZ3",
                LineNumber = 1,
                Message = "Error encountered by Backup Server.  Please refer to Backup Server messages for details.\n",
                MessageNumber = 8009,
                ProcedureName = "",
                SqlState = new byte[0],
                State = 1,
                Status = EedToken.EedStatus.TDS_EED_INFO,
                TransactionStatus = TranState.TDS_NOT_IN_TRAN
            });
            var ex = Assert.Throws<AseException>(() => handler.AssertNoErrors());
            Assert.AreEqual(3, ex.Errors.Count);
        }
    }
}
