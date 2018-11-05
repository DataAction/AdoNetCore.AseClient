using System.Collections.Generic;
using System.Data;
using Dapper;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration.Connection
{
    [TestFixture]
    public class InfoMessageTests
    {
        [Test]
        public void ExecutePrintStatements_OutputsInfoMessages()
        {
            using (var connection = new AseConnection(ConnectionStrings.Pooled))
            {
                connection.Open();

                var infoMessages = new List<AseError>();
                AseInfoMessageEventHandler handlerFunc = (sender, args) =>
                {
                    foreach (AseError message in args.Errors)
                    {
                        infoMessages.Add(message);
                    }
                };

                connection.InfoMessage += handlerFunc;

                connection.Execute(@"print 'AAA'
print 'BBB'", commandType: CommandType.Text);

                connection.InfoMessage -= handlerFunc;

                Assert.IsNotEmpty(infoMessages);
                Assert.AreEqual("AAA", infoMessages[0].Message);
                Assert.AreEqual("BBB", infoMessages[1].Message);
            }
        }

        [Test]
        public void ExecuteRaiseError_OutputsInfoMessages()
        {
            using (var connection = new AseConnection(ConnectionStrings.Pooled))
            {
                connection.Open();

                var infoMessages = new List<AseError>();
                AseInfoMessageEventHandler handlerFunc = (sender, args) =>
                {
                    foreach (AseError message in args.Errors)
                    {
                        infoMessages.Add(message);
                    }
                };

                connection.InfoMessage += handlerFunc;

                try
                {
                    connection.Execute(@"raiserror 17001 'AAA'", commandType: CommandType.Text);
                }
                catch { }

                connection.InfoMessage -= handlerFunc;

                Assert.IsNotEmpty(infoMessages);
                Assert.AreEqual(17001, infoMessages[0].MessageNumber);
                Assert.AreEqual("AAA", infoMessages[0].Message);
            }
        }
    }
}
