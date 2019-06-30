using System;
using System.IO;
using AdoNetCore.AseClient.Enum;

namespace AdoNetCore.AseClient.Internal
{
    /// <summary>
    /// The TokenSendStream is a write-only stream that can send packets to the network. 
    /// </summary>
    internal sealed class TokenSendStream : Stream
    {
        /// <summary>
        /// The network stream to read data from.
        /// </summary>
        private readonly DbEnvironment _environment;

        /// <summary>
        /// Whether or not this has been disposed.
        /// </summary>
        private bool _isDisposed;

        public bool IsCancelled { get; private set; }

        /// <summary>
        /// The stream decorated by this type. Data is ultimately read from and written to the inner stream.
        /// </summary>
        private readonly Stream _innerStream;

        /// <summary>
        /// When writing data, it is buffered first and then written out as chunks during <see cref="Flush"/>.
        /// </summary>
        private readonly MemoryStream _innerWriteBufferStream;

        /// <summary>
        /// The type of write response to send.
        /// </summary>
        private BufferType _writeBufferType;

        /// <summary>
        /// The default status of the packets to send.
        /// </summary>
        private BufferStatus _writeBufferStatus;

        // Debug only.
        private readonly bool _hexDump = false;

        /// <summary>
        /// Constructor function for a <see cref="TokenSendStream"/> instance.
        /// </summary>
        /// <param name="innerStream">The stream being decorated.</param>
        /// <param name="environment">The environment settings.</param>
        public TokenSendStream(Stream innerStream, DbEnvironment environment)
        {
            _innerStream = innerStream;
            _innerWriteBufferStream = new MemoryStream();
            _environment = environment;
            IsCancelled = false;
            _isDisposed = false;
        }

        public override void Flush()
        {
            // Point at the start of the stream for reading.
            _innerWriteBufferStream.Seek(0, SeekOrigin.Begin);

            try
            {
                if (_innerWriteBufferStream.Length == 0)
                {
                    // Add an 8 byte header block for cancellation.
                    var header = new byte[] { (byte)_writeBufferType, 0, 0, 0, 0, 0, 0, 0 };

                    // Set the header bytes to describe what is being transmitted
                    header[1] = (byte)(BufferStatus.TDS_BUFSTAT_EOM | _writeBufferStatus);
                    header[2] = (byte)(header.Length >> 8);
                    header[3] = (byte)header.Length;

                    DumpBytes(header, header.Length);

                    _innerStream.Write(header, 0, header.Length);
                }
                else
                {
                    while (_innerWriteBufferStream.Position < _innerWriteBufferStream.Length)
                    {
                        //split into chunks and send over the wire
                        var buffer = new byte[_environment.PacketSize];

                        // Add an 8 byte header block to each chunk.
                        var header = new byte[] { (byte)_writeBufferType, 0, 0, 0, 0, 0, 0, 0 };
                        Buffer.BlockCopy(header, 0, buffer, 0, header.Length);

                        // Write the body block into the remaining space.
                        var bodyLength = _innerWriteBufferStream.Read(buffer, header.Length, buffer.Length - header.Length);

                        // Set the header bytes to describe what is being transmitted
                        var chunkLength = header.Length + bodyLength;
                        buffer[1] = (byte)((_innerWriteBufferStream.Position >= _innerWriteBufferStream.Length ? BufferStatus.TDS_BUFSTAT_EOM : BufferStatus.TDS_BUFSTAT_NONE) | _writeBufferStatus);
                        buffer[2] = (byte)(chunkLength >> 8);
                        buffer[3] = (byte)chunkLength;

                        DumpBytes(buffer, chunkLength);

                        _innerStream.Write(buffer, 0, chunkLength);
                    }
                }
            }
            finally
            {
                // Clean up if something goes wrong.
                _innerWriteBufferStream.SetLength(0);
                _innerStream.Flush();
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets the type of buffer for write operations.
        /// </summary>
        /// <param name="type">The type of buffer being written.</param>
        /// <param name="status">The default status of the buffer.</param>
        public void SetBufferType(BufferType type, BufferStatus status)
        {
            _writeBufferType = type;
            _writeBufferStatus = status;
        }

        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new NotImplementedException();
        public override long Position
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            // Buffer the data and process it on Flush.
            _innerWriteBufferStream.Write(buffer, offset, count);
        }

        protected override void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                _isDisposed = true;

                if (!disposing)
                {
                    _innerStream.Dispose();
                    _innerWriteBufferStream.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        private void DumpBytes(byte[] bytes, int length)
        {
            if (_hexDump)
            {
                Logger.Instance?.Write(Environment.NewLine);
                Logger.Instance?.Write(HexDump.Dump(bytes, 0, length));
            }
        }
    }
}