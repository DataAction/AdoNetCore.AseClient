using System.Linq;
using AdoNetCore.AseClient.Internal;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Unit
{
    [TestFixture]
    public class ConnectionStringTokeniserTests
    {
        [TestCase("Data Source=myASEserver;Port=5000;Database=myDataBase;Uid=myUsername;Pwd=myPassword;", 5)]
        [TestCase(@";Data Source = ""myASEserver"" ;; Port = 5000 ; ; Database = 'myDataBase' ; Uid = ""myUsername"" ; Pwd = myPassword;", 5)]
        [TestCase(@";Data Source = ""myASEserver"" ;; Port = 5000 ; ; Database = 'myDataBase' ; Uid = ""my name has many parts"" ; Pwd = myPassword;", 5)]
        [TestCase(@";Data Source = ""myASEserver"" ;; Port = 5000 ; ; Database = 'myDataBase' ; Uid = ""my name has many parts"" ; Pwd = 'my-password-contains-characters-that *might* confuse-the-parser:""asd;ads=asd!@#$%^&*()';", 5)]
        public void ConnectionStringParser_WithValidConnectionString_GetsTokens(string connectionString,
            int expectedLength)
        {
            // Arrange
            var tokeniser = new ConnectionStringTokeniser();

            // Act
            var tokens = tokeniser
                .Tokenise(connectionString)
                .ToList();

            // Assert
            Assert.AreEqual(expectedLength, tokens.Count);
        }

        [TestCase(@"PropertyName=value", "PropertyName", "value")]
        [TestCase(@" PropertyName = value ", "PropertyName", "value")]
        [TestCase(@" PropertyName = "" value with whitespace """, "PropertyName", " value with whitespace ")]
        [TestCase(@" PropertyName = ' value with whitespace '", "PropertyName", " value with whitespace ")]
        [TestCase(@" PropertyName = 'value with ""double"" quotes in it'", "PropertyName", @"value with ""double"" quotes in it")]
        [TestCase(@" PropertyName = ""value with 'single' quotes in it""", "PropertyName", "value with 'single' quotes in it")]
        [TestCase(@" PropertyName = 'my-password-contains-characters-that *might* confuse-the-parser:""asd;ads=asd!@#$%^&*()' ", "PropertyName", @"my-password-contains-characters-that *might* confuse-the-parser:""asd;ads=asd!@#$%^&*()")]
        [TestCase(@" PropertyName = ""my-password-contains-characters-that *might* confuse-the-parser:'asd;ads=asd!@#$%^&*()"" ", "PropertyName", @"my-password-contains-characters-that *might* confuse-the-parser:'asd;ads=asd!@#$%^&*()")]
        public void ConnectionStringParser_WithValidConnectionString_GetsTokensWithRightValues(string connectionString, string
            expectedPropertyName, string expectedPropertyValue)
        {
            // Arrange
            var connectionStringParser = new ConnectionStringTokeniser();

            // Act
            var tokens = connectionStringParser
                .Tokenise(connectionString)
                .ToList();

            // Assert
            Assert.AreEqual(1, tokens.Count);

            var item = tokens.First();

            Assert.AreEqual(expectedPropertyName, item.PropertyName);
            Assert.AreEqual(expectedPropertyValue, item.PropertyValue);
        }
    }
}
