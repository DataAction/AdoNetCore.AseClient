using System;
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

        #if !NET_FRAMEWORK
        // Test case is only relevant to .net core, where CP850 isn't provided out of the box.
        // Framework has implemented it, so case would always fail.
        [Test]
        public void OpenConnection_WithCharsetCp850_NoEncodingProvider_Throws()
        {
            using (var connection = new AseConnection(ConnectionStrings.Cp850))
            {
                Assert.Throws<AseException>(() => connection.Open());
            }
        }
        #endif
    }
}
