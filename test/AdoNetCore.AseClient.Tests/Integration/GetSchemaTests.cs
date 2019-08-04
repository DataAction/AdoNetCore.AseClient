using System.Data;
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

        [TestCase("MetaDataCollections")]
        //[TestCase("DataSourceInformation")]
        [TestCase("DataTypes")]
        [TestCase("Restrictions")]
        [TestCase("ReservedWords")]
        [TestCase("Users")]
        [TestCase("Users", "sa")]
        [TestCase("Databases")]
        [TestCase("Databases", "master")]
        [TestCase("Tables")]
        // TODO - add the other restrictions.
        [TestCase("Columns")]
        [TestCase("Views")]
        [TestCase("ViewColumns")]
        [TestCase("ProcedureParameters")]
        [TestCase("Procedures")]
        [TestCase("ForeignKeys")]
        [TestCase("IndexColumns")]
        [TestCase("Indexes")]
        public void GetSchema_CompareWithReferenceDriver_SchemasMatch(string collectionName, params string[] restrictionValues)
        {
            DataTable expectedSchema;
            DataTable actualSchema;

            // The reference driver doesn't like an empty array - even though you're not meant to pass null...
            if (restrictionValues.Length == 0)
            {
                restrictionValues = null;
            }

            // Fix bugs in the reference driver that we don't want to carry across into this driver.
            if (string.Equals(collectionName, "Users"))
            {
                // Although the user name restriction is supported according to MetaDataCollections and
                // Restrictions - the reference driver throws when a user name is passed.
                restrictionValues = null; 
            }

            using (var connection = new Sybase.Data.AseClient.AseConnection(ConnectionStrings.Default))
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                expectedSchema = connection.GetSchema(collectionName, restrictionValues); 

                Assert.IsNotNull(expectedSchema);
            }

            // Fix bugs in the reference driver that we don't want to carry across into this driver.
            if (string.Equals(collectionName, "MetaDataCollections"))
            {
                expectedSchema.Rows.RemoveAt(expectedSchema.Rows.Count - 1); // The last result is not implemented, so why include it?

                expectedSchema.TableName = collectionName; // It's called "GetSchema" for some reason.
            }

            using (var connection = new AseConnection(ConnectionStrings.Default))
            {
                actualSchema = connection.GetSchema(collectionName, restrictionValues); 

                Assert.IsNotNull(actualSchema);
            }

            Assert.AreEqual(expectedSchema.CaseSensitive, actualSchema.CaseSensitive);
            Assert.AreEqual(expectedSchema.ChildRelations, actualSchema.ChildRelations);
            Assert.AreEqual(expectedSchema.Constraints, actualSchema.Constraints);
            Assert.AreEqual(expectedSchema.DataSet, actualSchema.DataSet);
            //Assert.AreEqual(expectedSchema.DefaultView, actualSchema.DefaultView);
            Assert.AreEqual(expectedSchema.DisplayExpression, actualSchema.DisplayExpression);
            Assert.AreEqual(expectedSchema.ExtendedProperties, actualSchema.ExtendedProperties);
            Assert.AreEqual(expectedSchema.HasErrors, actualSchema.HasErrors);
            Assert.AreEqual(expectedSchema.IsInitialized, actualSchema.IsInitialized);
            Assert.AreEqual(expectedSchema.Locale, actualSchema.Locale);
            Assert.AreEqual(expectedSchema.MinimumCapacity, actualSchema.MinimumCapacity);
            Assert.AreEqual(expectedSchema.Namespace, actualSchema.Namespace);
            Assert.AreEqual(expectedSchema.ParentRelations, actualSchema.ParentRelations);
            Assert.AreEqual(expectedSchema.Prefix, actualSchema.Prefix);
            Assert.AreEqual(expectedSchema.PrimaryKey, actualSchema.PrimaryKey);
            Assert.AreEqual(expectedSchema.RemotingFormat, actualSchema.RemotingFormat);
            //Assert.AreEqual(expectedSchema.Rows, actualSchema.Rows);
            Assert.AreEqual(expectedSchema.TableName, actualSchema.TableName);

            // The reference driver sets these inconsistently. We will be consistent.
            Assert.IsNull(actualSchema.GetChanges());
            Assert.IsNull(actualSchema.Site);
            Assert.IsNull(actualSchema.Container);
            Assert.IsFalse(actualSchema.DesignMode);

            // Columns.
            Assert.AreEqual(expectedSchema.Columns.Count, actualSchema.Columns.Count);
            
            for (var i = 0; i < expectedSchema.Columns.Count; i++)
            {
                var expectedColumn = expectedSchema.Columns[i];
                var actualColumn = actualSchema.Columns[i];

                //Assert.AreEqual(expectedColumn.AllowDBNull, actualColumn.AllowDBNull); // This can be inferred, not an issue if different.
                Assert.AreEqual(expectedColumn.AutoIncrement, actualColumn.AutoIncrement);
                Assert.AreEqual(expectedColumn.AutoIncrementSeed, actualColumn.AutoIncrementSeed);
                Assert.AreEqual(expectedColumn.AutoIncrementStep, actualColumn.AutoIncrementStep);
                Assert.AreEqual(expectedColumn.Caption, actualColumn.Caption);
                Assert.AreEqual(expectedColumn.ColumnMapping, actualColumn.ColumnMapping);
                Assert.AreEqual(expectedColumn.ColumnName, actualColumn.ColumnName);
                Assert.AreEqual(expectedColumn.DataType, actualColumn.DataType);
                Assert.AreEqual(expectedColumn.DateTimeMode, actualColumn.DateTimeMode);
                Assert.AreEqual(expectedColumn.DefaultValue, actualColumn.DefaultValue);
                Assert.AreEqual(expectedColumn.Expression, actualColumn.Expression);
                Assert.AreEqual(expectedColumn.ExtendedProperties, actualColumn.ExtendedProperties);
                //Assert.AreEqual(expectedColumn.MaxLength, actualColumn.MaxLength); // This can be inferred, not an issue if different.
                Assert.AreEqual(expectedColumn.Namespace, actualColumn.Namespace);
                Assert.AreEqual(expectedColumn.Ordinal, actualColumn.Ordinal);
                Assert.AreEqual(expectedColumn.Prefix, actualColumn.Prefix);
                Assert.AreEqual(expectedColumn.ReadOnly, actualColumn.ReadOnly);
                Assert.AreEqual(expectedColumn.Table?.TableName, actualColumn.Table?.TableName);
                Assert.AreEqual(expectedColumn.Unique, actualColumn.Unique);
                Assert.AreEqual(expectedColumn.Ordinal, actualColumn.Ordinal);

                Assert.IsNull(actualColumn.Site);
                Assert.IsNull(actualColumn.Container);
                Assert.IsFalse(actualColumn.DesignMode);
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
