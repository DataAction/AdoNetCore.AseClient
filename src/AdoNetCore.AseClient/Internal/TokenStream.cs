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
    /// The design goal of the token stream is to support reading from the network in a lazy fashion so that the
    /// <see cref="AseDataReader"/> can return results before the client has received all of the data.
    /// This is particularly important for large data sets, or long running server commands</remarks>
    /// Packet1|Packet2|Packet3|...
    /// [header][body]|[header][body]|[header][body]|
    internal sealed class TokenStream : Stream
    {
        /// <summary>
        /// The network stream to read data from.
        /// </summary>
        private readonly DbEnvironment _environment;

        /// <summary>
        /// A buffer to store header information from the TDS packets. This data is read and discarded by the <see cref="TokenStream"/>.
        /// </summary>
        private readonly byte[] _headerBuffer;

        /// <summary>
        /// A buffer to data that has been read by the <see cref="TokenStream"/>, but not returned to the client.
        /// </summary>
        private readonly byte[] _bodyBuffer;

        /// <summary>
        /// A pointer to the next byte that should be read from the <see cref="_bodyBuffer"/>.
        /// </summary>
        private int _bodyBufferPosition;

        /// <summary>
        /// The number of data bytes that are in the <see cref="_bodyBuffer"/>.
        /// </summary>
        private int _bodyBufferLength;
        private bool _isDisposed;
        private bool _bufferHasBytes;
        private bool _networkHasBytes;

        public bool IsCancelled { get; private set; }

        public bool DataAvailable => _networkHasBytes || _bufferHasBytes;

        private readonly Stream _innerStream;

        public TokenStream(Stream innerStream, DbEnvironment environment)
        {
            _innerStream = innerStream;
            _environment = environment;
            _headerBuffer = new byte[_environment.HeaderSize];
            _bodyBuffer = new byte[_environment.PacketSize - environment.HeaderSize];
            _bodyBufferPosition = 0;
            _bodyBufferLength = 0;
            IsCancelled = false;
            _isDisposed = false;
            _bufferHasBytes = false;
            _networkHasBytes = true;
        }

        public override void Flush()
        {
            // Do nothing.
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
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
            if (_bufferHasBytes)
            {
                bytesWrittenToBuffer = GetBufferedBytes(buffer, 0 + offset, count);
            }

            // If we need more data, let's read it from the network until the buffer is full.
            while (bytesWrittenToBuffer < count && _networkHasBytes)
            {
                BufferBytes(_headerBuffer, 0, _environment.HeaderSize);

                var bufferStatus = (BufferStatus)_headerBuffer[1];

                //" If TDS_BUFSTAT_ATTNACK not also TDS_BUFSTAT_EOM, continue reading packets until TDS_BUFSTAT_EOM."
                IsCancelled = bufferStatus.HasFlag(BufferStatus.TDS_BUFSTAT_ATTNACK) || bufferStatus.HasFlag(BufferStatus.TDS_BUFSTAT_ATTN);

                var length = _headerBuffer[2] << 8 | _headerBuffer[3];

                _bodyBufferLength = length - _environment.HeaderSize;
                _bodyBufferPosition = 0;

                BufferBytes(_bodyBuffer, 0, _bodyBufferLength);

                bytesWrittenToBuffer += GetBufferedBytes(buffer, bytesWrittenToBuffer + offset, count - bytesWrittenToBuffer);

                // If there is no more data, stop there.
                if (bufferStatus.HasFlag(BufferStatus.TDS_BUFSTAT_EOM) || length == _environment.HeaderSize)
                {
                    _networkHasBytes = false;
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
            while (_networkHasBytes)
            {
                BufferBytes(_headerBuffer, 0, _environment.HeaderSize);

                var bufferStatus = (BufferStatus) _headerBuffer[1];

                var length = _headerBuffer[2] << 8 | _headerBuffer[3];

                _bodyBufferLength = length - _environment.HeaderSize;
                _bodyBufferPosition = 0;

                BufferBytes(_bodyBuffer, 0, _bodyBufferLength);

                // If there is no more data, stop there.
                if (bufferStatus.HasFlag(BufferStatus.TDS_BUFSTAT_EOM) || length == _environment.HeaderSize)
                {
                    _networkHasBytes = false;
                    break;
                }
            }

            _bufferHasBytes = false;
            _bodyBufferLength = 0;
            _bodyBufferPosition = 0;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
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
            
            var bufferedBytes = _bodyBufferLength - _bodyBufferPosition;

            // if this chunk is less than the amount requested, add it all.
            if (bufferedBytes <= count)
            {
                Buffer.BlockCopy(_bodyBuffer, _bodyBufferPosition, buffer, offset, bufferedBytes);
                written = bufferedBytes;

                // Nothing left in the buffer
                _bodyBufferPosition = 0;
                _bodyBufferLength = 0;
                _bufferHasBytes = false;
            }
            // else add part of it and save the rest.
            else
            {
                Buffer.BlockCopy(_bodyBuffer, _bodyBufferPosition, buffer, offset, count);
                written = count;

                _bodyBufferPosition += count;
                _bufferHasBytes = true;
            }

            return written;
        }
    }
}
