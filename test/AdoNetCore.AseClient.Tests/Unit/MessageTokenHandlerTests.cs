using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Internal.Handler;
using AdoNetCore.AseClient.Token;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Unit
{
    [TestFixture]
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
    }
}
