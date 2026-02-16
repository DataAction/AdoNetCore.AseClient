using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Unit.BulkCopy
{
    [TestFixture]
    public class AseBulkCopyColumnMappingTests
    {
        #region Constructor Tests

        [Test]
        public void Constructor_ByOrdinals_SetsProperties()
        {
            var mapping = new AseBulkCopyColumnMapping(0, 1);

            Assert.AreEqual(0, mapping.SourceOrdinal);
            Assert.AreEqual(1, mapping.DestinationOrdinal);
            Assert.IsNull(mapping.SourceColumn);
            Assert.IsNull(mapping.DestinationColumn);
        }

        [Test]
        public void Constructor_ByNames_SetsProperties()
        {
            var mapping = new AseBulkCopyColumnMapping("SourceCol", "DestCol");

            Assert.AreEqual("SourceCol", mapping.SourceColumn);
            Assert.AreEqual("DestCol", mapping.DestinationColumn);
            Assert.AreEqual(-1, mapping.SourceOrdinal);
            Assert.AreEqual(-1, mapping.DestinationOrdinal);
        }

        [Test]
        public void Constructor_NameToOrdinal_SetsProperties()
        {
            var mapping = new AseBulkCopyColumnMapping("SourceCol", 2);

            Assert.AreEqual("SourceCol", mapping.SourceColumn);
            Assert.AreEqual(2, mapping.DestinationOrdinal);
            Assert.AreEqual(-1, mapping.SourceOrdinal);
            Assert.IsNull(mapping.DestinationColumn);
        }

        [Test]
        public void Constructor_OrdinalToName_SetsProperties()
        {
            var mapping = new AseBulkCopyColumnMapping(3, "DestCol");

            Assert.AreEqual(3, mapping.SourceOrdinal);
            Assert.AreEqual("DestCol", mapping.DestinationColumn);
            Assert.IsNull(mapping.SourceColumn);
            Assert.AreEqual(-1, mapping.DestinationOrdinal);
        }

        #endregion

        #region Collection Tests

        [Test]
        public void Collection_AddByNames_ReturnsMapping()
        {
            var collection = new AseBulkCopyColumnMappingCollection();

            var mapping = collection.Add("src", "dest");

            Assert.AreEqual(1, collection.Count);
            Assert.AreEqual("src", mapping.SourceColumn);
            Assert.AreEqual("dest", mapping.DestinationColumn);
        }

        [Test]
        public void Collection_AddByOrdinals_ReturnsMapping()
        {
            var collection = new AseBulkCopyColumnMappingCollection();

            var mapping = collection.Add(0, 1);

            Assert.AreEqual(1, collection.Count);
            Assert.AreEqual(0, mapping.SourceOrdinal);
            Assert.AreEqual(1, mapping.DestinationOrdinal);
        }

        [Test]
        public void Collection_AddMultiple_HasCorrectCount()
        {
            var collection = new AseBulkCopyColumnMappingCollection();

            collection.Add("a", "x");
            collection.Add("b", "y");
            collection.Add("c", "z");

            Assert.AreEqual(3, collection.Count);
        }

        [Test]
        public void Collection_Clear_RemovesAll()
        {
            var collection = new AseBulkCopyColumnMappingCollection();
            collection.Add("a", "x");
            collection.Add("b", "y");

            collection.Clear();

            Assert.AreEqual(0, collection.Count);
        }

        [Test]
        public void Collection_Indexer_ReturnsCorrectMapping()
        {
            var collection = new AseBulkCopyColumnMappingCollection();
            collection.Add("a", "x");
            collection.Add("b", "y");

            Assert.AreEqual("b", collection[1].SourceColumn);
        }

        [Test]
        public void Collection_RemoveAt_RemovesCorrectItem()
        {
            var collection = new AseBulkCopyColumnMappingCollection();
            collection.Add("a", "x");
            collection.Add("b", "y");
            collection.Add("c", "z");

            collection.RemoveAt(1);

            Assert.AreEqual(2, collection.Count);
            Assert.AreEqual("a", collection[0].SourceColumn);
            Assert.AreEqual("c", collection[1].SourceColumn);
        }

        [Test]
        public void Collection_Enumerable_IteratesAll()
        {
            var collection = new AseBulkCopyColumnMappingCollection();
            collection.Add("a", "x");
            collection.Add("b", "y");

            var count = 0;
            foreach (var mapping in collection)
            {
                count++;
            }
            Assert.AreEqual(2, count);
        }

        [Test]
        public void Collection_AddNameToOrdinal()
        {
            var collection = new AseBulkCopyColumnMappingCollection();

            var mapping = collection.Add("src", 5);

            Assert.AreEqual("src", mapping.SourceColumn);
            Assert.AreEqual(5, mapping.DestinationOrdinal);
        }

        [Test]
        public void Collection_AddOrdinalToName()
        {
            var collection = new AseBulkCopyColumnMappingCollection();

            var mapping = collection.Add(3, "dest");

            Assert.AreEqual(3, mapping.SourceOrdinal);
            Assert.AreEqual("dest", mapping.DestinationColumn);
        }

        #endregion
    }
}
