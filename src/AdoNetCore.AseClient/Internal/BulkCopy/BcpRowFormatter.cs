using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using AdoNetCore.AseClient.Enum;

namespace AdoNetCore.AseClient.Internal.BulkCopy
{
    /// <summary>
    /// Constructs full native BCP row buffers for the TDS 5.0 bulk copy protocol.
    ///
    /// Native row format (from FreeTDS tds5_send_record):
    ///   [4-byte DOL padding (if datarows-locked)]
    ///   [1 byte: variable column count (actual non-trailing-null count)]
    ///   [1 byte: row number (always 0)]
    ///   [fixed column data -- packed sequentially, no length prefixes]
    ///   [2 bytes: total row size LE (only if variable columns present)]
    ///   [variable column data -- packed sequentially]
    ///   [offset table]
    ///
    /// The 2-byte row size field and offset table are only present when there
    /// are non-trailing-null variable columns.
    ///
    /// On the wire, each row is preceded by a 2-byte LE length prefix (the total
    /// record size), sent by the caller.
    /// </summary>
    internal class BcpRowFormatter
    {
        private readonly BcpColumnMetadata[] _columns;
        private readonly bool _isDatarowsLocked;
        private readonly Encoding _encoding;
        private readonly int[] _sourceOrdinals;
        private readonly List<int> _fixedColumnIndices;
        private readonly List<int> _variableColumnIndices;

        /// <summary>
        /// Creates a new BcpRowFormatter.
        /// </summary>
        /// <param name="columns">Column metadata from the server's INSERT BULK response.</param>
        /// <param name="isDatarowsLocked">Whether the table uses datarows locking (requires 4-byte padding).</param>
        /// <param name="encoding">Character encoding for string data.</param>
        /// <param name="sourceOrdinals">
        /// Maps server column index to source (IDataReader) ordinal.
        /// sourceOrdinals[i] is the reader ordinal for server column i, or -1 to skip.
        /// </param>
        public BcpRowFormatter(BcpColumnMetadata[] columns, bool isDatarowsLocked, Encoding encoding, int[] sourceOrdinals)
        {
            _columns = columns;
            _isDatarowsLocked = isDatarowsLocked;
            _encoding = encoding;
            _sourceOrdinals = sourceOrdinals;
            _fixedColumnIndices = new List<int>();
            _variableColumnIndices = new List<int>();

            for (var i = 0; i < columns.Length; i++)
            {
                if (columns[i].IsFixedLength)
                {
                    _fixedColumnIndices.Add(i);
                }
                else
                {
                    _variableColumnIndices.Add(i);
                }
            }
        }

        /// <summary>
        /// Formats a single row from an IDataReader into native BCP wire format.
        /// </summary>
        /// <param name="reader">The data reader positioned at the current row.</param>
        /// <returns>The complete row buffer ready to write to the TDS stream.</returns>
        public byte[] FormatRow(IDataReader reader)
        {
            using (var ms = new MemoryStream())
            {
                // DOL padding for datarows-locked tables
                if (_isDatarowsLocked)
                {
                    ms.WriteByte(0);
                    ms.WriteByte(0);
                    ms.WriteByte(0);
                    ms.WriteByte(0);
                }

                // Save position for varcount header (will be updated later)
                var varColsPos = (int)ms.Position;
                ms.WriteByte(0); // varcount placeholder
                ms.WriteByte(0); // row number (always 0)

                // Write fixed columns sequentially (no length prefix)
                for (var i = 0; i < _fixedColumnIndices.Count; i++)
                {
                    var colIdx = _fixedColumnIndices[i];
                    var srcOrd = _sourceOrdinals[colIdx];
                    var value = srcOrd >= 0 ? reader.GetValue(srcOrd) : DBNull.Value;
                    BcpValueWriter.WriteFixedValue(ms, value, _columns[colIdx], _encoding);
                }

                // Variable columns section
                var rowSzPos = (int)ms.Position;
                var varCount = _variableColumnIndices.Count;

                if (varCount > 0)
                {
                    // Reserve 2 bytes for total row size placeholder
                    ms.WriteByte(0);
                    ms.WriteByte(0);

                    // offsets[0] = absolute position of start of variable data
                    var offsets = new int[varCount + 1];
                    offsets[0] = (int)ms.Position;
                    var ncols = 0;

                    for (var i = 0; i < varCount; i++)
                    {
                        var colIdx = _variableColumnIndices[i];
                        var srcOrd = _sourceOrdinals[colIdx];
                        var value = srcOrd >= 0 ? reader.GetValue(srcOrd) : DBNull.Value;
                        var isNull = value == null || value == DBNull.Value;

                        if (!isNull)
                        {
                            var written = BcpValueWriter.WriteVariableValue(ms, value, _columns[colIdx], _encoding);
                            // Empty varchar/char → write single space byte (FreeTDS convention)
                            if (written == 0 && IsStringType(_columns[colIdx].DataType))
                            {
                                ms.WriteByte(0x20);
                            }
                        }

                        offsets[++ncols] = (int)ms.Position;
                    }

                    // Strip trailing null columns (where consecutive offsets are equal)
                    while (ncols > 0 && offsets[ncols] == offsets[ncols - 1])
                    {
                        ncols--;
                    }

                    if (ncols > 0)
                    {
                        // Write offset table
                        if (_isDatarowsLocked)
                        {
                            WriteDolOffsetTable(ms, offsets, ncols);
                        }
                        else
                        {
                            WriteAplOffsetTable(ms, offsets, ncols);
                        }

                        // Fill in total row size at rowSzPos (2-byte LE)
                        var totalRowSize = (int)ms.Position;
                        ms.Position = rowSzPos;
                        ms.WriteByte((byte)(totalRowSize & 0xFF));
                        ms.WriteByte((byte)((totalRowSize >> 8) & 0xFF));

                        // Fill in actual var cols written at varColsPos
                        ms.Position = varColsPos;
                        ms.WriteByte((byte)ncols);

                        // Seek back to end
                        ms.Position = totalRowSize;
                    }
                    else
                    {
                        // All variable columns are trailing nulls - remove 2-byte placeholder
                        ms.SetLength(rowSzPos);
                        ms.Position = rowSzPos;
                    }
                }

                return ms.ToArray();
            }
        }

        /// <summary>
        /// Formats a single row from an array of values into native BCP wire format.
        /// </summary>
        /// <param name="values">Column values indexed by server column ordinal.</param>
        /// <returns>The complete row buffer ready to write to the TDS stream.</returns>
        public byte[] FormatRow(object[] values)
        {
            using (var ms = new MemoryStream())
            {
                // DOL padding for datarows-locked tables
                if (_isDatarowsLocked)
                {
                    ms.WriteByte(0);
                    ms.WriteByte(0);
                    ms.WriteByte(0);
                    ms.WriteByte(0);
                }

                // Save position for varcount header
                var varColsPos = (int)ms.Position;
                ms.WriteByte(0); // varcount placeholder
                ms.WriteByte(0); // row number (always 0)

                // Write fixed columns sequentially
                for (var i = 0; i < _fixedColumnIndices.Count; i++)
                {
                    var colIdx = _fixedColumnIndices[i];
                    var srcOrd = _sourceOrdinals[colIdx];
                    var value = srcOrd >= 0 && srcOrd < values.Length ? values[srcOrd] : DBNull.Value;
                    BcpValueWriter.WriteFixedValue(ms, value, _columns[colIdx], _encoding);
                }

                // Variable columns section
                var rowSzPos = (int)ms.Position;
                var varCount = _variableColumnIndices.Count;

                if (varCount > 0)
                {
                    // Reserve 2 bytes for total row size placeholder
                    ms.WriteByte(0);
                    ms.WriteByte(0);

                    var offsets = new int[varCount + 1];
                    offsets[0] = (int)ms.Position;
                    var ncols = 0;

                    for (var i = 0; i < varCount; i++)
                    {
                        var colIdx = _variableColumnIndices[i];
                        var srcOrd = _sourceOrdinals[colIdx];
                        var value = srcOrd >= 0 && srcOrd < values.Length ? values[srcOrd] : DBNull.Value;
                        var isNull = value == null || value == DBNull.Value;

                        if (!isNull)
                        {
                            var written = BcpValueWriter.WriteVariableValue(ms, value, _columns[colIdx], _encoding);
                            if (written == 0 && IsStringType(_columns[colIdx].DataType))
                            {
                                ms.WriteByte(0x20);
                            }
                        }

                        offsets[++ncols] = (int)ms.Position;
                    }

                    while (ncols > 0 && offsets[ncols] == offsets[ncols - 1])
                    {
                        ncols--;
                    }

                    if (ncols > 0)
                    {
                        if (_isDatarowsLocked)
                        {
                            WriteDolOffsetTable(ms, offsets, ncols);
                        }
                        else
                        {
                            WriteAplOffsetTable(ms, offsets, ncols);
                        }

                        var totalRowSize = (int)ms.Position;
                        ms.Position = rowSzPos;
                        ms.WriteByte((byte)(totalRowSize & 0xFF));
                        ms.WriteByte((byte)((totalRowSize >> 8) & 0xFF));

                        ms.Position = varColsPos;
                        ms.WriteByte((byte)ncols);

                        ms.Position = totalRowSize;
                    }
                    else
                    {
                        ms.SetLength(rowSzPos);
                        ms.Position = rowSzPos;
                    }
                }

                return ms.ToArray();
            }
        }

        private static bool IsStringType(TdsDataType type)
        {
            return type == TdsDataType.TDS_CHAR
                || type == TdsDataType.TDS_VARCHAR
                || type == TdsDataType.TDS_LONGCHAR;
        }

        /// <summary>
        /// Writes APL (allpages-locked) offset table.
        /// From FreeTDS: adjustment bytes (if offsets > 255), then count byte, then
        /// low bytes of offsets in reverse order (offsets[ncols] down to offsets[0]).
        /// Total of ncols+1 offset entries.
        /// </summary>
        private static void WriteAplOffsetTable(Stream ms, int[] offsets, int ncols)
        {
            var pfxTop = offsets[ncols] >> 8;

            if (pfxTop == 0)
            {
                // Simple case: all offsets < 256
                ms.WriteByte((byte)(ncols + 1));
            }
            else
            {
                // Need adjustment table for high bytes
                if (offsets[ncols] / 256 == offsets[ncols - 1] / 256)
                {
                    ms.WriteByte((byte)(ncols + 1));
                }

                // Run-length prefix encoding for high bytes
                var topCopy = pfxTop;
                while (topCopy > 0)
                {
                    var nPfx = 1;
                    for (var i = 0; i <= ncols; i++)
                    {
                        if ((offsets[i] >> 8) < topCopy)
                        {
                            nPfx++;
                        }
                    }
                    ms.WriteByte((byte)nPfx);
                    topCopy--;
                }
            }

            // Write low bytes of all offsets in reverse order
            for (var i = 0; i <= ncols; i++)
            {
                ms.WriteByte((byte)(offsets[ncols - i] & 0xFF));
            }
        }

        /// <summary>
        /// Writes DOL (datarows-locked) offset table.
        /// Each non-fixed column gets a 2-byte absolute offset in reverse order.
        /// Does not include the "one past the end" offset that APL format has.
        /// </summary>
        private static void WriteDolOffsetTable(Stream ms, int[] offsets, int ncols)
        {
            for (var col = ncols - 1; col >= 0; col--)
            {
                ms.WriteByte((byte)(offsets[col] & 0xFF));
                ms.WriteByte((byte)((offsets[col] >> 8) & 0xFF));
            }
        }

        /// <summary>
        /// Resolves column mappings to produce source ordinals.
        /// Returns an array where result[i] is the source reader ordinal for server column i.
        /// </summary>
        public static int[] ResolveColumnMappings(
            BcpColumnMetadata[] serverColumns,
            IDataReader reader,
            AseBulkCopyColumnMappingCollection mappings)
        {
            var sourceOrdinals = new int[serverColumns.Length];

            if (mappings == null || mappings.Count == 0)
            {
                var readerFieldCount = reader.FieldCount;

                // Count non-identity server columns
                var nonIdentityCount = 0;
                for (var i = 0; i < serverColumns.Length; i++)
                {
                    if (!serverColumns[i].IsIdentity)
                    {
                        nonIdentityCount++;
                    }
                }

                if (readerFieldCount == serverColumns.Length)
                {
                    // Reader has same number of fields as server columns (including identity).
                    // Map 1:1 by ordinal — the reader is providing identity values too
                    // (e.g., BCP OUT -> BCP IN pipeline for the same table).
                    for (var i = 0; i < serverColumns.Length; i++)
                    {
                        sourceOrdinals[i] = i;
                    }
                }
                else
                {
                    // Reader has fewer fields — skip identity columns and map sequentially
                    var readerOrdinal = 0;
                    for (var i = 0; i < serverColumns.Length; i++)
                    {
                        if (serverColumns[i].IsIdentity)
                        {
                            sourceOrdinals[i] = -1;
                        }
                        else if (readerOrdinal < readerFieldCount)
                        {
                            sourceOrdinals[i] = readerOrdinal;
                            readerOrdinal++;
                        }
                        else
                        {
                            sourceOrdinals[i] = -1;
                        }
                    }
                }
            }
            else
            {
                // Initialize all to -1 (unmapped)
                for (var i = 0; i < sourceOrdinals.Length; i++)
                {
                    sourceOrdinals[i] = -1;
                }

                // Apply explicit mappings
                foreach (var mapping in mappings)
                {
                    var destIdx = ResolveDestinationOrdinal(serverColumns, mapping);
                    var srcIdx = ResolveSourceOrdinal(reader, mapping);

                    if (destIdx >= 0 && destIdx < serverColumns.Length && srcIdx >= 0)
                    {
                        sourceOrdinals[destIdx] = srcIdx;
                    }
                }
            }

            return sourceOrdinals;
        }

        private static int ResolveDestinationOrdinal(BcpColumnMetadata[] serverColumns, AseBulkCopyColumnMapping mapping)
        {
            if (mapping.DestinationOrdinal >= 0)
            {
                return mapping.DestinationOrdinal;
            }

            if (!string.IsNullOrEmpty(mapping.DestinationColumn))
            {
                for (var i = 0; i < serverColumns.Length; i++)
                {
                    if (string.Equals(serverColumns[i].Name, mapping.DestinationColumn, StringComparison.OrdinalIgnoreCase))
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        private static int ResolveSourceOrdinal(IDataReader reader, AseBulkCopyColumnMapping mapping)
        {
            if (mapping.SourceOrdinal >= 0)
            {
                return mapping.SourceOrdinal;
            }

            if (!string.IsNullOrEmpty(mapping.SourceColumn))
            {
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    if (string.Equals(reader.GetName(i), mapping.SourceColumn, StringComparison.OrdinalIgnoreCase))
                    {
                        return i;
                    }
                }
            }

            return -1;
        }
    }
}
