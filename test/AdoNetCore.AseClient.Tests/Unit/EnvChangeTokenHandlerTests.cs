using AdoNetCore.AseClient.Internal;
using AdoNetCore.AseClient.Internal.Handler;
using AdoNetCore.AseClient.Token;
using NUnit.Framework;
using System.Text;

namespace AdoNetCore.AseClient.Tests.Unit
{
    [TestFixture]
    [Category("quick")]
    public class EnvChangeTokenHandlerTests
    {
        [Test]
        public void AssertHandlesDbEnvChange()
        {
            var environment = new DbEnvironment();            
            var handler = new EnvChangeTokenHandler(environment, string.Empty);
            handler.Handle(new EnvironmentChangeToken()
            {
                Changes = new EnvironmentChangeToken.EnvironmentChange[]
                {
                    new EnvironmentChangeToken.EnvironmentChange()
                    {
                        Type = EnvironmentChangeToken.ChangeType.TDS_ENV_DB,
                        NewValue = "MyDB"
                    }
                }
            });
            Assert.AreEqual("MyDB", environment.Database);
        }

        [Test]
        public void AssertHandlesPacketSizeChange()
        {
            var environment = new DbEnvironment();
            var handler = new EnvChangeTokenHandler(environment, string.Empty);
            handler.Handle(new EnvironmentChangeToken()
            {
                Changes = new EnvironmentChangeToken.EnvironmentChange[]
                {
                    new EnvironmentChangeToken.EnvironmentChange()
                    {
                        Type = EnvironmentChangeToken.ChangeType.TDS_ENV_PACKSIZE,
                        NewValue = "512"
                    }
                }
            });
            Assert.AreEqual(512, environment.PacketSize);
        }

        [TestCase("iso_1", "", "ISO-8859-1")]
        [TestCase("iso 8859-1", "", "ISO-8859-1")]
        [TestCase("ISO88591", "", "ISO-8859-1")]
        [TestCase("ascii_8", "", "ASCII")]
        [TestCase("utf-8", "", "UTF-8")]
        [TestCase("utf8", "", "UTF-8")]
        [TestCase("ISO-8859-1", "", "ISO-8859-1")]
        [TestCase("", "", "ASCII")]
        [TestCase("", "iso_1", "ISO-8859-1")]
        [TestCase("", "ISO-8859-1", "ISO-8859-1")]
        [TestCase("utf8", "ISO-8859-1", "UTF-8")]
        public void AssertHandlesCharsetChange(string tokenNewCharset, string clientRequestedCharset, string expectedEncodingName)
        {
            var environment = new DbEnvironment();
            var handler = new EnvChangeTokenHandler(environment, clientRequestedCharset);
            handler.Handle(new EnvironmentChangeToken()
            {
                Changes = new EnvironmentChangeToken.EnvironmentChange[]
                {
                    new EnvironmentChangeToken.EnvironmentChange()
                    {
                        Type = EnvironmentChangeToken.ChangeType.TDS_ENV_CHARSET,
                        NewValue = tokenNewCharset
                    }
                }
            });
            Encoding expectedEncoding = Encoding.GetEncoding(expectedEncodingName);
            Assert.AreSame(expectedEncoding, environment.Encoding);
        }

        [Test]
        public void AssertUnknownCharset_EmitsAseException()
        {
            var environment = new DbEnvironment();
            var handler = new EnvChangeTokenHandler(environment, string.Empty);
            string unknownCharsetName = "UNKNOWN-CHARSET";
            var exception = Assert.Throws<AseException>(() => handler.Handle(new EnvironmentChangeToken()
            {
                Changes = new EnvironmentChangeToken.EnvironmentChange[]
                {
                    new EnvironmentChangeToken.EnvironmentChange()
                    {
                        Type = EnvironmentChangeToken.ChangeType.TDS_ENV_CHARSET,
                        NewValue = unknownCharsetName
                    }
                }
            }));
            Assert.IsTrue(exception.Message.Contains(unknownCharsetName));
        }

        [Test]
        public void AssertHandlesTextSizeChange()
        {
            var environment = new DbEnvironment();
            var handler = new EnvChangeTokenHandler(environment, string.Empty);
            handler.Handle(OptionCommandToken.CreateSetTextSize(9999));            
            Assert.AreEqual(9999, environment.TextSize);
        }
    }
}
