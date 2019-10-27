using System;
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
        //[TestCase("UserDefinedTypes")]
        public void AseConnection_GetSchemaForCollection_ReturnsResults(string collectionName)
        {
            using (var connection = new AseConnection(ConnectionStrings.Default))
            {
                var schema = connection.GetSchema(collectionName);
                
                Assert.IsNotNull(schema);
                Assert.AreEqual(collectionName, schema.TableName);
                Assert.IsNotEmpty(schema.Rows);

                // TODO - assert MetaDataCollections, DataSourceInformation, DataTypes, Restrictions, and ReservedWords schema matches reference driver.
            }
        }

        // TODO - generate test data.
        [TestCase("Indexes")]
        [TestCase("Indexes", "tableName", "tableOwner", "tableQualifier", "indexName")]
        public void AseConnection_GetSchemaForIndexesCollection_ReturnsResults(string collectionName, params string[] restrictionValues)
        {
            using (var connection = new AseConnection(ConnectionStrings.Default))
            {
                connection.Open();

                if(restrictionValues != null && restrictionValues.Length > 0)
                {
                    var dbIndex = Array.IndexOf(restrictionValues,  "[current-database]");

                    if (dbIndex >= 0)
                    {
                        restrictionValues[dbIndex] = connection.Database;
                    }
                }

                var schema = connection.GetSchema(collectionName, restrictionValues);
                
                Assert.IsNotNull(schema);
                Assert.AreEqual(collectionName, schema.TableName);
                Assert.IsNotEmpty(schema.Rows);

                // TODO - assert Indexes schema matches reference driver.
            }
        }

        // TODO - generate test data.
        [TestCase("IndexColumns")]
        [TestCase("IndexColumns", "tableName", "tableOwner", "tableQualifier", "indexName", "columnName")]
        public void AseConnection_GetSchemaForIndexColumnsCollection_ReturnsResults(string collectionName, params string[] restrictionValues)
        {
            using (var connection = new AseConnection(ConnectionStrings.Default))
            {
                connection.Open();

                if(restrictionValues != null && restrictionValues.Length > 0)
                {
                    var dbIndex = Array.IndexOf(restrictionValues,  "[current-database]");

                    if (dbIndex >= 0)
                    {
                        restrictionValues[dbIndex] = connection.Database;
                    }
                }

                var schema = connection.GetSchema(collectionName, restrictionValues);
                
                Assert.IsNotNull(schema);
                Assert.AreEqual(collectionName, schema.TableName);
                Assert.IsNotEmpty(schema.Rows);

                // TODO - assert IndexColumns schema matches reference driver.
            }
        }

        // TODO - generate test data.
        [TestCase("ForeignKeys")]
        [TestCase("ForeignKeys", "pktable_name", "pktable_owner", "pktable_qualifier", "fktable_name", "fktable_owner", "fktable_qualifier")]
        public void AseConnection_GetSchemaForForeignKeysCollection_ReturnsResults(string collectionName, params string[] restrictionValues)
        {
            using (var connection = new AseConnection(ConnectionStrings.Default))
            {
                connection.Open();

                if(restrictionValues != null && restrictionValues.Length > 0)
                {
                    var dbIndex = Array.IndexOf(restrictionValues,  "[current-database]");

                    if (dbIndex >= 0)
                    {
                        restrictionValues[dbIndex] = connection.Database;
                    }
                }

                var schema = connection.GetSchema(collectionName, restrictionValues);
                
                Assert.IsNotNull(schema);
                Assert.AreEqual(collectionName, schema.TableName);
                Assert.IsNotEmpty(schema.Rows);

                // TODO - assert ForeignKeys schema matches reference driver.
            }
        }
        [TestCase("Procedures")]
        [TestCase("Procedures", "EchoParameter")]
        [TestCase("Procedures", "EchoParameter", "dbo")]
        [TestCase("Procedures", "EchoParameter", "dbo", "[current-database]")]
        [TestCase("Procedures", "EchoParameter", "dbo", "[current-database]", "P")]
        public void AseConnection_GetSchemaForProceduresCollection_ReturnsResults(string collectionName, params string[] restrictionValues)
        {
            using (var connection = new AseConnection(ConnectionStrings.Default))
            {
                connection.Open();

                if(restrictionValues != null && restrictionValues.Length > 0)
                {
                    var dbIndex = Array.IndexOf(restrictionValues,  "[current-database]");

                    if (dbIndex >= 0)
                    {
                        restrictionValues[dbIndex] = connection.Database;
                    }
                }

                var schema = connection.GetSchema(collectionName, restrictionValues);
                
                Assert.IsNotNull(schema);
                Assert.AreEqual(collectionName, schema.TableName);
                Assert.IsNotEmpty(schema.Rows);

                // TODO - assert Procedures schema matches reference driver.
            }
        }

        [TestCase("ProcedureParameters")]
        [TestCase("ProcedureParameters", "EchoParameter")]
        [TestCase("ProcedureParameters", "EchoParameter", "dbo")]
        [TestCase("ProcedureParameters", "EchoParameter", "dbo", "[current-database]")]
        [TestCase("ProcedureParameters", "EchoParameter", "dbo", "[current-database]", "@parameter1")]
        public void AseConnection_GetSchemaForProcedureParametersCollection_ReturnsResults(string collectionName, params string[] restrictionValues)
        {
            using (var connection = new AseConnection(ConnectionStrings.Default))
            {
                connection.Open();

                if(restrictionValues != null && restrictionValues.Length > 0)
                {
                    var dbIndex = Array.IndexOf(restrictionValues,  "[current-database]");

                    if (dbIndex >= 0)
                    {
                        restrictionValues[dbIndex] = connection.Database;
                    }
                }

                var schema = connection.GetSchema(collectionName, restrictionValues);
                
                Assert.IsNotNull(schema);
                Assert.AreEqual(collectionName, schema.TableName);
                Assert.IsNotEmpty(schema.Rows);

                // TODO - assert ProcedureParameters schema matches reference driver.
            }
        }

        [TestCase("ViewColumns")]
        [TestCase("ViewColumns", "[current-database]")]
        [TestCase("ViewColumns", "[current-database]", "dbo")]
        [TestCase("ViewColumns", "[current-database]", "dbo", "sysquerymetrics")]
        public void AseConnection_GetSchemaForViewColumnsCollection_ReturnsResults(string collectionName, params string[] restrictionValues)
        {
            using (var connection = new AseConnection(ConnectionStrings.Default))
            {
                connection.Open();

                if(restrictionValues != null && restrictionValues.Length > 0)
                {
                    var dbIndex = Array.IndexOf(restrictionValues,  "[current-database]");

                    if (dbIndex >= 0)
                    {
                        restrictionValues[dbIndex] = connection.Database;
                    }
                }

                var schema = connection.GetSchema(collectionName, restrictionValues);
                
                Assert.IsNotNull(schema);
                Assert.AreEqual(collectionName, schema.TableName);
                Assert.IsNotEmpty(schema.Rows);

                // TODO - assert ViewColumns schema matches reference driver.
            }
        }

        [TestCase("Columns")]
        [TestCase("Columns", "syscomments")]
        [TestCase("Columns", "syscomments", "dbo")]
        [TestCase("Columns", "syscomments", "dbo", "[current-database]")]
        [TestCase("Columns", "syscomments", "dbo", "[current-database]", "id")]
        public void AseConnection_GetSchemaForColumnsCollection_ReturnsResults(string collectionName, params string[] restrictionValues)
        {
            using (var connection = new AseConnection(ConnectionStrings.Default))
            {
                connection.Open();

                if(restrictionValues != null && restrictionValues.Length > 0)
                {
                    var dbIndex = Array.IndexOf(restrictionValues,  "[current-database]");

                    if (dbIndex >= 0)
                    {
                        restrictionValues[dbIndex] = connection.Database;
                    }
                }

                var schema = connection.GetSchema(collectionName, restrictionValues);
                
                Assert.IsNotNull(schema);
                Assert.AreEqual(collectionName, schema.TableName);
                Assert.IsNotEmpty(schema.Rows);

                // TODO - assert Columns schema matches reference driver.
            }
        }

        [TestCase("Tables")]
        [TestCase("Tables", "[current-database]")]
        [TestCase("Tables", "[current-database]", "dbo")]
        [TestCase("Tables", "[current-database]", "dbo", "syscomments")]
        [TestCase("Tables", "[current-database]", "dbo", "syscomments", "SYSTEM TABLE")]
        public void AseConnection_GetSchemaForTablesCollection_ReturnsResults(string collectionName, params string[] restrictionValues)
        {
            using (var connection = new AseConnection(ConnectionStrings.Default))
            {
                connection.Open();

                if(restrictionValues != null && restrictionValues.Length > 0)
                {
                    restrictionValues[Array.IndexOf(restrictionValues,  "[current-database]")] = connection.Database;
                }

                var schema = connection.GetSchema(collectionName, restrictionValues);
                
                Assert.IsNotNull(schema);
                Assert.AreEqual(collectionName, schema.TableName);
                Assert.IsNotEmpty(schema.Rows);

                // TODO - assert Tables schema matches reference driver.
            }
        }

        [TestCase("Databases")]
        [TestCase("Databases", "master")]
        public void AseConnection_GetSchemaForDatabasesCollection_ReturnsResults(string collectionName, params string[] restrictionValues)
        {
            using (var connection = new AseConnection(ConnectionStrings.Default))
            {
                connection.Open();

                var schema = connection.GetSchema(collectionName, restrictionValues);
                
                Assert.IsNotNull(schema);
                Assert.AreEqual(collectionName, schema.TableName);
                Assert.IsNotEmpty(schema.Rows);

                // TODO - assert Databases schema matches reference driver.
            }
        }

        [TestCase("Users")]
        [TestCase("Users", "dbo")]
        public void AseConnection_GetSchemaForUsersCollection_ReturnsResults(string collectionName, params string[] restrictionValues)
        {
            using (var connection = new AseConnection(ConnectionStrings.Default))
            {
                connection.Open();

                var schema = connection.GetSchema(collectionName, restrictionValues);
                
                Assert.IsNotNull(schema);
                Assert.AreEqual(collectionName, schema.TableName);
                Assert.IsNotEmpty(schema.Rows);

                // TODO - assert Users schema matches reference driver.
            }
        }

        [TestCase("Views")]
        [TestCase("Views", "[current-database]")]
        [TestCase("Views", "[current-database]", "dbo")]
        [TestCase("Views", "[current-database]", "dbo", "sysquerymetrics")]
        public void AseConnection_GetSchemaForViewsCollection_ReturnsResults(string collectionName, params string[] restrictionValues)
        {
            using (var connection = new AseConnection(ConnectionStrings.Default))
            {
                connection.Open();

                if(restrictionValues != null && restrictionValues.Length > 0)
                {
                    restrictionValues[Array.IndexOf(restrictionValues,  "[current-database]")] = connection.Database;
                }

                var restriction1 = connection.Database;
                var schema = connection.GetSchema(collectionName, new []{restriction1});

                Assert.IsNotNull(schema);
                Assert.AreEqual(collectionName, schema.TableName);
                Assert.IsNotEmpty(schema.Rows);

                // TODO - assert Views schema matches reference driver.
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
                connection.Open();

                Assert.Throws<AseException>(() => connection.GetSchema(collectionName, new[] {restriction1}));
            }
        }

        [Test]
        public void AseConnection_GetSchemaForInvalidCollection_Throws()
        {
            using (var connection = new AseConnection(ConnectionStrings.Default))
            {
                connection.Open();

                Assert.Throws<AseException>(() => connection.GetSchema("an invalid collection"));
            }
        }
#endif
    }
}
