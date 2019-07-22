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

        [TestCase("MetaDataCollections", Explicit = true)]
        [TestCase("DataSourceInformation", Explicit = true)]
        [TestCase("DataTypes", Explicit = true)]
        [TestCase("Restrictions", Explicit = true)]
        [TestCase("ReservedWords", Explicit = true)]
        [TestCase("Users", Explicit = true)]
        [TestCase("Databases", Explicit = true)]
        [TestCase("Tables", Explicit = true)]
        [TestCase("Columns", Explicit = true)]
        [TestCase("Views", Explicit = true)]
        [TestCase("ViewColumns", Explicit = true)]
        [TestCase("ProcedureParameters", Explicit = true)]
        [TestCase("Procedures", Explicit = true)]
        [TestCase("ForeignKeys", Explicit = true)]
        [TestCase("IndexColumns", Explicit = true)]
        [TestCase("Indexes", Explicit = true)]
        [TestCase("UserDefinedTypes", Explicit = true)]
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
