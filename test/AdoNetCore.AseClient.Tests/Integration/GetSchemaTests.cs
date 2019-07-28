using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration
{
    [TestFixture]
    [Category("basic")]
    public class GetSchemaTests
    {
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

#if DB_GETSCHEMA

        [Test]
        public void AseConnection_GetSchema_ReturnsResults()
        {
            using (var connection = new AseConnection(ConnectionStrings.Default))
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
        //[TestCase("UserDefinedTypes")]
        public void AseConnection_GetSchemaForCollection_ReturnsResults(string collectionName)
        {
            using (var connection = new AseConnection(ConnectionStrings.Default))
            {
                var schema = connection.GetSchema(collectionName);
                
                Assert.IsNotNull(schema);
                Assert.AreEqual(collectionName, schema.TableName);
                Assert.IsNotEmpty(schema.Rows);
            }
        }

        [TestCase("Users", "dbo")]
        [TestCase("Databases", "master")]
        //[TestCase("Tables")]
        //[TestCase("Columns")]
        //[TestCase("ViewColumns")]
        //[TestCase("ProcedureParameters")]
        //[TestCase("Procedures")]
        //[TestCase("ForeignKeys")]
        //[TestCase("IndexColumns")]
        //[TestCase("Indexes")]
        public void AseConnection_GetSchemaForCollection_ReturnsResults(string collectionName, string restriction1)
        {
            using (var connection = new AseConnection(ConnectionStrings.Default))
            {
                var schema = connection.GetSchema(collectionName, new []{restriction1});

                Assert.IsNotNull(schema);
                Assert.AreEqual(collectionName, schema.TableName);
                Assert.IsNotEmpty(schema.Rows);
            }
        }

        [TestCase("Views")]
        public void AseConnection_GetSchemaForViewsCollection_ReturnsResults(string collectionName)
        {
            using (var connection = new AseConnection(ConnectionStrings.Default))
            {
                connection.Open();

                var restriction1 = connection.Database;
                var schema = connection.GetSchema(collectionName, new []{restriction1});

                Assert.IsNotNull(schema);
                Assert.AreEqual(collectionName, schema.TableName);
                Assert.IsNotEmpty(schema.Rows);
            }
        }

        [TestCase("Views", "dbo")]
        public void AseConnection_GetSchemaForViewsCollection_ReturnsResults(string collectionName, string restriction2)
        {
            using (var connection = new AseConnection(ConnectionStrings.Default))
            {
                connection.Open();

                var restriction1 = connection.Database;
                var schema = connection.GetSchema(collectionName, new[] { restriction1, restriction2 });

                Assert.IsNotNull(schema);
                Assert.AreEqual(collectionName, schema.TableName);
                Assert.IsNotEmpty(schema.Rows);
            }
        }

        [TestCase("Views", "dbo", "sysquerymetrics")]
        public void AseConnection_GetSchemaForViewsCollection_ReturnsResults(string collectionName, string restriction2, string restriction3)
        {
            using (var connection = new AseConnection(ConnectionStrings.Default))
            {
                connection.Open();

                var restriction1 = connection.Database;
                var schema = connection.GetSchema(collectionName, new[] { restriction1, restriction2, restriction3 });

                Assert.IsNotNull(schema);
                Assert.AreEqual(collectionName, schema.TableName);
                Assert.IsNotEmpty(schema.Rows);
            }
        }

        [TestCase("MetaDataCollections", "a restriction")]
        [TestCase("DataSourceInformation", "a restriction")]
        [TestCase("DataTypes", "a restriction")]
        [TestCase("Restrictions", "a restriction")]
        [TestCase("ReservedWords", "a restriction")]
        public void AseConnection_GetSchemaForCollectionWithInvalidRestriction_Throws(string collectionName, string restriction1)
        {
            using (var connection = new AseConnection(ConnectionStrings.Default))
            {
                Assert.Throws<AseException>(() => connection.GetSchema(collectionName, new[] {restriction1}));
            }
        }

        [Test]
        public void AseConnection_GetSchemaForInvalidCollection_Throws()
        {
            using (var connection = new AseConnection(ConnectionStrings.Default))
            {
                Assert.Throws<AseException>(() => connection.GetSchema("an invalid collection"));
            }
        }
#endif
    }
}
