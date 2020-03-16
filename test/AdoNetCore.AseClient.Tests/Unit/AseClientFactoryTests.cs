using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Unit
{
    [TestFixture]
    [Category("quick")]
    public class AseClientFactoryTests
    {
        [Test]
        public void GetInstance_Singleton_NotNull()
        {
            Assert.IsNotNull(AseClientFactory.Instance);
        }

        [Test]
        public void CreateConnection_NoArgs_NotNull()
        {
            Assert.IsNotNull(AseClientFactory.Instance.CreateConnection());
        }

        [Test]
        public void CreateCommand_NoArgs_NotNull()
        {
            Assert.IsNotNull(AseClientFactory.Instance.CreateCommand());
        }

        [Test]
        public void CreateCommandBuilder_NoArgs_NotNull()
        {
            Assert.IsNotNull(AseClientFactory.Instance.CreateCommandBuilder());
        }

        [Test]
        public void CreateConnectionStringBuilder_NoArgs_NotNull()
        {
            Assert.IsNotNull(AseClientFactory.Instance.CreateConnectionStringBuilder());
        }

        [Test]
        public void CreateDataAdapter_NoArgs_NotNull()
        {
            Assert.IsNotNull(AseClientFactory.Instance.CreateDataAdapter());
        }

        [Test]
        public void CreateParameter_NoArgs_NotNull()
        {
            Assert.IsNotNull(AseClientFactory.Instance.CreateParameter());
        }

        [Test]
        public void CreateDataSourceEnumerator_NoArgs_NotNull()
        {
            Assert.IsNotNull(AseClientFactory.Instance.CreateDataSourceEnumerator());
        }

        [Test]
        public void CanCreateDataSourceEnumerator_NoArgs_False()
        {
            Assert.IsFalse(AseClientFactory.Instance.CanCreateDataSourceEnumerator);
        }
    }
}
