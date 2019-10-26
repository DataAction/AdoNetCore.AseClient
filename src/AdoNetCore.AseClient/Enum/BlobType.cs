// ReSharper disable InconsistentNaming
namespace AdoNetCore.AseClient.Enum
{
    public enum BlobType : byte
    {
        /// <summary>
        /// Not set
        /// </summary>
        BLOB_UNSET = 0x00,
        /// <summary>
        /// The fully qualified name of the class (“com.foo.Bar”).
        /// This is a Character String in the negotiated TDS character set currently in use on this connection.
        /// </summary>
        BLOB_FULLY_QUALIFIED_CLASS_NAME = 0x01,
        /// <summary>
        /// 4-byte integer (database ID) 4-byte integer(sysextypes number of this class definition in this database).
        /// Both integers are in the byte-ordering negotiated for this connection.
        /// </summary>
        BLOB_INT32_CLASS_ID = 0x02,
        /// <summary>
        /// This is long character data and has no ClassID associated with it
        /// </summary>
        BLOB_LONGCHAR = 0x03,
        /// <summary>
        ///  This is long binary data and has no ClassID associated with it.
        /// Appears in ribo as BLOB_VARBINARY
        /// </summary>
        BLOB_LONGBINARY = 0x04,
        /// <summary>
        /// This is unichar data with no ClassID associated with it.
        /// Appears in ribo as BLOB_UTF16
        /// </summary>
        BLOB_UNICHAR = 0x05
    }
}
