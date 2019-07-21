using System;
using System.IO;
using System.Net.Sockets;
using AdoNetCore.AseClient.Enum;

namespace AdoNetCore.AseClient.Internal
{
    /// <summary>
    /// The TokenReceiveStream is a read-only stream that can read token data from packets coming from the network. 
    /// </summary>
    /// <remarks>
    /// <para>The design goal of the token stream is to support reading from the network in a lazy fashion so that the
    /// ASE server can return info messages as they occur, rather than once all data has been received.
    /// This is particularly important for large data sets, or long running server commands.</para>
    /// <para>The lifecycle of the <see cref="TokenReceiveStream"/> should be one request or one response.</para>
    /// </remarks>
    internal sealed class TokenReceiveStream : Stream
    {
        /// <summary>
        /// The network stream to read data from.
        /// </summary>
        private readonly DbEnvironment _environment;

#if ENABLE_ARRAY_POOL
        /// <summary>
        /// The <see cref="System.Buffers.ArrayPool{T}"/> to use for efficient buffer allocation.
        /// </summary>
        private readonly System.Buffers.ArrayPool<byte> _arrayPool;
#endif
        /// <summary>
        /// Whether or not this has been disposed.
        /// </summary>
        private bool _isDisposed;

        /// <summary>
        /// A buffer to store header information from the TDS packets. This data is read and discarded by the <see cref="TokenReceiveStream"/>.
        /// </summary>
        private readonly byte[] _headerReadBuffer;

        /// <summary>
        /// A buffer to data that has been read by the <see cref="TokenReceiveStream"/>, but not returned to the client.
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
        /// The length of the <see cref="_headerReadBuffer"/>.
        /// </summary>
        private readonly int _headerReadBufferLength;

#if ENABLE_ARRAY_POOL
        public TokenReceiveStream(Stream innerStream, DbEnvironment environment, System.Buffers.ArrayPool<byte> arrayPool)
#else
        public TokenReceiveStream(Stream innerStream, DbEnvironment environment)
#endif
        {
            _innerStream = innerStream;
            _environment = environment;
            _headerReadBufferLength = environment.HeaderSize;
#if ENABLE_ARRAY_POOL
            _arrayPool = arrayPool;
            _headerReadBuffer = arrayPool.Rent(_headerReadBufferLength);
            _bodyReadBuffer = arrayPool.Rent(_environment.PacketSize - _headerReadBufferLength);
#else
            _headerReadBuffer = new byte[_headerReadBufferLength];
            _bodyReadBuffer = new byte[_environment.PacketSize - _headerReadBufferLength];
#endif
            _bodyReadBufferPosition = 0;
            _bodyReadBufferLength = 0;
            IsCancelled = false;
            _isDisposed = false;
            _readBufferHasBytes = false;
            _innerStreamHasBytes = true;
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
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
                BufferBytes(_headerReadBuffer, 0, _headerReadBufferLength);

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
                BufferBytes(_headerReadBuffer, 0, _headerReadBufferLength);

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
            throw new NotSupportedException();
        }

        protected override void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                _isDisposed = true;

                if (!disposing)
                {
                    _innerStream.Dispose();

                    BurnPackets(); // Considering the case where the socket lives longer than the stream, and the stream is disposed without reading all of the data.
#if ENABLE_ARRAY_POOL
                    _arrayPool.Return(_bodyReadBuffer, true);
                    _arrayPool.Return(_headerReadBuffer, true);
#endif
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
    }
}
