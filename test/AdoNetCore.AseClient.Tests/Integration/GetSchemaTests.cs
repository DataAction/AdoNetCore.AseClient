using System;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration
{
    [TestFixture]
    [Category("basic")]
    public class GetSchemaTests
    {
        [Test]
        public void Noop()
        {
            Console.WriteLine("Hello there.");
        }


#if NET_FRAMEWORK
        [Test]
        public void ReferenceConnection_GetSchema_ReturnsResults()
        {
            using(var connection = new Sybase.Data.AseClient.AseConnection(ConnectionStrings.Default))
            {
                var schema = connection.GetSchema();

                Assert.IsNotNull(schema);
            }
        }

        [TestCase("MetaDataCollections")]
        [TestCase("DataSourceInformation")]
        [TestCase("DataTypes")]
        [TestCase("Restrictions")]
        [TestCase("ReservedWords")]
        [TestCase("Users")]
        [TestCase("Databases")]
        [TestCase("Tables")]
        [TestCase("Columns")]
        [TestCase("Views")]
        [TestCase("ViewColumns")]
        [TestCase("ProcedureParameters")]
        [TestCase("Procedures")]
        [TestCase("ForeignKeys")]
        [TestCase("IndexColumns")]
        [TestCase("Indexes")]
        [TestCase("UserDefinedTypes")]
        public void ReferenceConnection_GetSchemaForCollection_ReturnsResults(string collectionName)
        {
            using (var connection = new Sybase.Data.AseClient.AseConnection(ConnectionStrings.Default))
            {
                var schema = connection.GetSchema(collectionName); 

                Assert.IsNotNull(schema);
            }
        }
#endif

#if ENABLE_DB_GETSCHEMA

        [Test]
        public void AseConnection_GetSchema_ReturnsResults()
        {
            using (var connection = new AseConnection(ConnectionStrings.Default))
            {
                var schema = connection.GetSchema();

                Assert.IsNotNull(schema);
            }
        }

#endif

#if ENABLE_DB_PROVIDERFACTORY

        [Test]
        public void AseConnection_GetSchema_ReturnsResults()
        {
            using (var connection = new AseConnection(ConnectionStrings.Default))
            {
                var schema = connection.GetSchema();

                Assert.IsNotNull(schema);
            }
        }

#endif
    }
}
