using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration.Connection
{
    [TestFixture]
    [Category("basic")]
    public class CharsetTests
    {
        private class TestEncodingProvider : EncodingProvider
        {
            public override Encoding GetEncoding(int codepage)
            {
                return null;
            }

            public override Encoding GetEncoding(string name)
            {
                if (string.Equals("cp850", name, StringComparison.OrdinalIgnoreCase))
                {
                    return Encoding.GetEncoding(850);
                }

                return null;
            }
        }

        [Test]
        public void OpenConnection_WithCharsetCp850_PlusEncodingProvider_Succeeds()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding.RegisterProvider(new TestEncodingProvider());

            using (var connection = new AseConnection(ConnectionStrings.Cp850))
            {
                connection.Open();
            }
        }

        [Test]
        public void OpenConnection_WithCharsetCp850_NoEncodingProvider_Throws()
        {
            using (var connection = new AseConnection(ConnectionStrings.Cp850))
            {
                Assert.Throws<AseException>(() => connection.Open());
            }
        }
    }
}
