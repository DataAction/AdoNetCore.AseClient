namespace AdoNetCore.AseClient.Enum
{
    public enum SerializationType : byte
    {
        //todo: figure out what these names might actually be
        /// <summary>
        /// Use the default serialization associated with the specified <see cref="BlobType"/>.
        /// Allowed cases:
        /// <see cref="BlobType.BLOB_LONGCHAR"/> -  Characters are in their native format, the character set of the data is the same as that of all other character data as negotiated on the connection during login.
        /// <see cref="BlobType.BLOB_LONGBINARY"/> - Binary data in its normal form
        /// <see cref="BlobType.BLOB_UNICHAR"/> - This is unichar data with normal UTF-16 encoding with byte-order identical to that of the client
        /// </summary>
        SER_DEFAULT = 0x00,
        /// <summary>
        /// Allowed cases:
        /// <see cref="BlobType.BLOB_FULLY_QUALIFIED_CLASS_NAME"/> - Native Java Serialization
        /// <see cref="BlobType.BLOB_INT32_CLASS_ID"/> - Native Java Serialization
        /// <see cref="BlobType.BLOB_UNICHAR"/> - This is unichar data in its UTF-8 encoding.
        /// </summary>
        SER_SPECIAL1 = 0x01,
        /// <summary>
        /// Allowed cases:
        /// <see cref="BlobType.BLOB_UNICHAR"/> - This is unichar data in SCSU (compressed) encoding
        /// </summary>
        SER_SPECIAL2 = 0x02
    }
}
