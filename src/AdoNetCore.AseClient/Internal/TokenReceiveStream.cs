using System;
using System.IO;
using System.Net.Sockets;
using AdoNetCore.AseClient.Enum;

namespace AdoNetCore.AseClient.Internal
{
    /// <summary>
    /// The TokenStream is a read-only stream that can read token data from packets coming from the network. 
    /// </summary>
    /// <remarks>
    /// <para>The design goal of the token stream is to support reading from the network in a lazy fashion so that the
    /// ASE server can return info messages as they occur, rather than once all data has been received.
    /// This is particularly important for large data sets, or long running server commands.</para>
    /// <para>The lifecycle of the <see cref="TokenStream"/> should be one request or one response.</para>
    /// </remarks>
    internal sealed class TokenStream : Stream
    {
        /// <summary>
        /// The network stream to read data from.
        /// </summary>
        private readonly DbEnvironment _environment;

        /// <summary>
        /// Whether or not this has been disposed.
        /// </summary>
        private bool _isDisposed;

        /// <summary>
        /// A buffer to store header information from the TDS packets. This data is read and discarded by the <see cref="TokenStream"/>.
        /// </summary>
        private readonly byte[] _headerReadBuffer;

        /// <summary>
        /// A buffer to data that has been read by the <see cref="TokenStream"/>, but not returned to the client.
        /// </summary>
        private readonly byte[] _bodyReadBuffer;

        /// <summary>
        /// A pointer to the next byte that should be read from the <see cref="_bodyReadBuffer"/>.
        /// </summary>
        private int _bodyReadBufferPosition;

        /// <summary>
        /// The number of data bytes that are in the <see cref="_bodyReadBuffer"/>.
        /// </summary>
        private int _bodyReadBufferLength;

        /// <summary>
        /// Whether or not the <see cref="_bodyReadBuffer"/> has data in it to read.
        /// </summary>
        private bool _readBufferHasBytes;

        /// <summary>
        /// Whether or not the <see cref="_innerStream"/> has data in it to read.
        /// </summary>
        private bool _innerStreamHasBytes;

        public bool IsCancelled { get; private set; }

        /// <summary>
        /// Whether or not there is data available to read.
        /// </summary>
        public bool DataAvailable => _innerStreamHasBytes || _readBufferHasBytes;

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
        /// Constructor function for a <see cref="TokenStream"/> instance.
        /// </summary>
        /// <param name="innerStream">The stream being decorated.</param>
        /// <param name="environment">The environment settings.</param>
        public TokenStream(Stream innerStream, DbEnvironment environment)
        {
            _innerStream = innerStream;
            _innerWriteBufferStream = new MemoryStream();
            _environment = environment;
            _headerReadBuffer = new byte[_environment.HeaderSize];
            _bodyReadBuffer = new byte[_environment.PacketSize - environment.HeaderSize];
            _bodyReadBufferPosition = 0;
            _bodyReadBufferLength = 0;
            IsCancelled = false;
            _isDisposed = false;
            _readBufferHasBytes = false;
            _innerStreamHasBytes = true;
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

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new NotImplementedException();
        public override long Position
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public override int ReadByte()
        {
            byte[] buffer = new byte[1];

            var result = Read(buffer, 0, buffer.Length);
            if (result != buffer.Length)
            {
                return -1;
            }

            return buffer[0];
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException("Cannot read from a stream that has been disposed.");
            }
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer), "The buffer parameter cannot be null.");
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), offset, "The offset parameter cannot be less than zero.");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), offset, "The count parameter cannot be less than zero.");
            }
            if (offset + count > buffer.Length)
            {
                throw new ArgumentException("The sum of offset and count is larger than the buffer length.");
            }

            // If there is no more data, then stop.
            if (IsCancelled || !DataAvailable)
            {
                return 0;
            }
            
            var bytesWrittenToBuffer = 0;

            // If we have some data in the bodyBuffer, we should use that up first.
            if (_readBufferHasBytes)
            {
                bytesWrittenToBuffer = GetBufferedBytes(buffer, 0 + offset, count);
            }

            // If we need more data, let's read it from the network until the buffer is full.
            while (bytesWrittenToBuffer < count && _innerStreamHasBytes)
            {
                BufferBytes(_headerReadBuffer, 0, _environment.HeaderSize);

                var bufferStatus = (BufferStatus)_headerReadBuffer[1];

                //" If TDS_BUFSTAT_ATTNACK not also TDS_BUFSTAT_EOM, continue reading packets until TDS_BUFSTAT_EOM."
                IsCancelled = bufferStatus.HasFlag(BufferStatus.TDS_BUFSTAT_ATTNACK) || bufferStatus.HasFlag(BufferStatus.TDS_BUFSTAT_ATTN);

                var length = _headerReadBuffer[2] << 8 | _headerReadBuffer[3];

                _bodyReadBufferLength = length - _environment.HeaderSize;
                _bodyReadBufferPosition = 0;

                BufferBytes(_bodyReadBuffer, 0, _bodyReadBufferLength);

                bytesWrittenToBuffer += GetBufferedBytes(buffer, bytesWrittenToBuffer + offset, count - bytesWrittenToBuffer);

                // If there is no more data, stop there.
                if (bufferStatus.HasFlag(BufferStatus.TDS_BUFSTAT_EOM) || length == _environment.HeaderSize)
                {
                    _innerStreamHasBytes = false;
                    break;
                }
            }

            // If cancelled, burn
            if (IsCancelled)
            {
                BurnPackets();
            }

            return bytesWrittenToBuffer;
        }

        /// <summary>
        /// It is possible that the caller doesn't process all of the data returned to the socket.
        /// This method reads and discards all data, leaving the socket in a ready state for the next request.
        /// </summary>
        private void BurnPackets()
        {
            while (_innerStreamHasBytes)
            {
                BufferBytes(_headerReadBuffer, 0, _environment.HeaderSize);

                var bufferStatus = (BufferStatus) _headerReadBuffer[1];

                var length = _headerReadBuffer[2] << 8 | _headerReadBuffer[3];

                _bodyReadBufferLength = length - _environment.HeaderSize;
                _bodyReadBufferPosition = 0;

                BufferBytes(_bodyReadBuffer, 0, _bodyReadBufferLength);

                // If there is no more data, stop there.
                if (bufferStatus.HasFlag(BufferStatus.TDS_BUFSTAT_EOM) || length == _environment.HeaderSize)
                {
                    _innerStreamHasBytes = false;
                    break;
                }
            }

            _readBufferHasBytes = false;
            _bodyReadBufferLength = 0;
            _bodyReadBufferPosition = 0;
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

                    BurnPackets(); // Considering the case where the socket lives longer than the stream, and the stream is disposed without reading all of the data.
                }
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Ensure that the requested bytes have been received. The base <see cref="NetworkStream"/> will
        /// block until at least one byte is available, but not necessarily wait for all of the requested
        /// bytes.
        /// </summary>
        /// <param name="buffer">The buffer to fill.</param>
        /// <param name="offset">The position within the buffer to write data into.</param>
        /// <param name="count">The number of bytes.</param>
        private void BufferBytes(byte[] buffer, int offset, int count)
        {
            if (count <= 0)
            {
                return;
            }

            var remainingBytes = count;
            var totalReceivedBytes = 0;
            do
            {
                var receivedBytes = _innerStream.Read(buffer, offset + totalReceivedBytes, remainingBytes);
                if (receivedBytes == 0)
                {
                    throw new SocketException((int)SocketError.NotConnected);
                }
                remainingBytes -= receivedBytes;
                totalReceivedBytes += receivedBytes;
            } while (remainingBytes > 0);
        }

        private int GetBufferedBytes(byte[] buffer, int offset, int count)
        {
            int written;
            
            var bufferedBytes = _bodyReadBufferLength - _bodyReadBufferPosition;

            // if this chunk is less than the amount requested, add it all.
            if (bufferedBytes <= count)
            {
                Buffer.BlockCopy(_bodyReadBuffer, _bodyReadBufferPosition, buffer, offset, bufferedBytes);
                written = bufferedBytes;

                // Nothing left in the buffer
                _bodyReadBufferPosition = 0;
                _bodyReadBufferLength = 0;
                _readBufferHasBytes = false;
            }
            // else add part of it and save the rest.
            else
            {
                Buffer.BlockCopy(_bodyReadBuffer, _bodyReadBufferPosition, buffer, offset, count);
                written = count;

                _bodyReadBufferPosition += count;
                _readBufferHasBytes = true;
            }

            return written;
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
