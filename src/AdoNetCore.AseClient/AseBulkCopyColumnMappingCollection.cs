using System.Collections;
using System.Collections.Generic;

namespace AdoNetCore.AseClient
{
    /// <summary>
    /// A collection of <see cref="AseBulkCopyColumnMapping"/> objects that define column mappings
    /// between a data source and the destination table.
    /// </summary>
    public sealed class AseBulkCopyColumnMappingCollection : IList<AseBulkCopyColumnMapping>
    {
        private readonly List<AseBulkCopyColumnMapping> _mappings = new List<AseBulkCopyColumnMapping>();

        /// <summary>
        /// Adds a mapping using source and destination column names.
        /// </summary>
        public AseBulkCopyColumnMapping Add(string sourceColumn, string destinationColumn)
        {
            var mapping = new AseBulkCopyColumnMapping(sourceColumn, destinationColumn);
            _mappings.Add(mapping);
            return mapping;
        }

        /// <summary>
        /// Adds a mapping using source and destination column ordinals.
        /// </summary>
        public AseBulkCopyColumnMapping Add(int sourceColumnOrdinal, int destinationOrdinal)
        {
            var mapping = new AseBulkCopyColumnMapping(sourceColumnOrdinal, destinationOrdinal);
            _mappings.Add(mapping);
            return mapping;
        }

        /// <summary>
        /// Adds a mapping using a source column name and destination ordinal.
        /// </summary>
        public AseBulkCopyColumnMapping Add(string sourceColumn, int destinationOrdinal)
        {
            var mapping = new AseBulkCopyColumnMapping(sourceColumn, destinationOrdinal);
            _mappings.Add(mapping);
            return mapping;
        }

        /// <summary>
        /// Adds a mapping using a source ordinal and destination column name.
        /// </summary>
        public AseBulkCopyColumnMapping Add(int sourceColumnOrdinal, string destinationColumn)
        {
            var mapping = new AseBulkCopyColumnMapping(sourceColumnOrdinal, destinationColumn);
            _mappings.Add(mapping);
            return mapping;
        }

        public void Add(AseBulkCopyColumnMapping item)
        {
            _mappings.Add(item);
        }

        public void Clear()
        {
            _mappings.Clear();
        }

        public bool Contains(AseBulkCopyColumnMapping item)
        {
            return _mappings.Contains(item);
        }

        public void CopyTo(AseBulkCopyColumnMapping[] array, int arrayIndex)
        {
            _mappings.CopyTo(array, arrayIndex);
        }

        public bool Remove(AseBulkCopyColumnMapping item)
        {
            return _mappings.Remove(item);
        }

        public int Count => _mappings.Count;

        public bool IsReadOnly => false;

        public int IndexOf(AseBulkCopyColumnMapping item)
        {
            return _mappings.IndexOf(item);
        }

        public void Insert(int index, AseBulkCopyColumnMapping item)
        {
            _mappings.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _mappings.RemoveAt(index);
        }

        public AseBulkCopyColumnMapping this[int index]
        {
            get => _mappings[index];
            set => _mappings[index] = value;
        }

        public IEnumerator<AseBulkCopyColumnMapping> GetEnumerator()
        {
            return _mappings.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
