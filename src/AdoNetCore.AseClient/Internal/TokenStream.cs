using System;
using System.IO;
using System.Net.Sockets;
using AdoNetCore.AseClient.Enum;

namespace AdoNetCore.AseClient.Internal
{
    /// <summary>
    /// The TokenStream is a read-only stream that can read token data from packets coming from the network.
    /// </summary>
    internal sealed class TokenStream : Stream
    {
        /// <summary>
        /// The network stream to read data from.
        /// </summary>
        private readonly NetworkStream _networkStream;
        private readonly DbEnvironment _environment;
        private readonly byte[] _headerBuffer;
        private readonly byte[] _bodyBuffer;
        private int _bodyBufferPosition;
        private int _bodyBufferLength;
        private bool _isEnd;
        private bool _isCancelled;
        private bool _isDisposed;

        /// <summary>
        /// Reading is supported.
        /// </summary>
        public override bool CanRead => true;

        /// <summary>
        /// Seeking is not supported.
        /// </summary>
        public override bool CanSeek => false;

        /// <summary>
        /// Writing is not supported.
        /// </summary>
        public override bool CanWrite => false;

        /// <summary>
        /// Length is not supported.
        /// </summary>
        public override long Length => throw new NotImplementedException();

        /// <summary>
        /// Position is not supported.
        /// </summary>
        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public TokenStream(NetworkStream networkStream, DbEnvironment environment)
        {
            _networkStream = networkStream;
            _environment = environment;
            _headerBuffer = new byte[_environment.HeaderSize];
            _bodyBuffer = new byte[_environment.PacketSize - environment.HeaderSize];
            _bodyBufferPosition = 0;
            _bodyBufferLength = 0;
            _isEnd = false;
            _isCancelled = false;
            _isDisposed = false;
        }


        public override void Flush()
        {
            // Do nothing.
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
            if (_isCancelled || _isEnd)
            {
                return 0;
            }

            int bytesWrittenToBuffer = 0;


            // If we have some data in the bodyBuffer, we should use that up first.
            if (_bodyBufferPosition > 0)
            {
                var bufferedBytes = _bodyBufferLength - _bodyBufferPosition;

                // if this chunk is less than the amount requested, add it all.
                if (bufferedBytes <= count)
                {
                    Array.Copy(_bodyBuffer, _bodyBufferPosition, buffer, 0, bufferedBytes);
                    bytesWrittenToBuffer += bufferedBytes;

                    // Nothing left in the buffer
                    _bodyBufferPosition = 0;
                    _bodyBufferLength = 0;
                }
                // else add part of it and save the rest.
                else
                {
                    Array.Copy(_bodyBuffer, _bodyBufferPosition, buffer, bytesWrittenToBuffer, count);
                    bytesWrittenToBuffer += count;

                    _bodyBufferPosition += count;
                }
            }

            // If we need more data, let's read if from the network until the buffer is full.
            while (bytesWrittenToBuffer < count)
            {
                int received = _networkStream.Read(_headerBuffer, 0, _environment.HeaderSize);

                if (received != _environment.HeaderSize)
                {
                    _isEnd = true;
                    throw new IOException("Failed to read the packet header.");
                }

                var bufferStatus = (BufferStatus)_headerBuffer[1];

                //" If TDS_BUFSTAT_ATTNACK not also TDS_BUFSTAT_EOM, continue reading packets until TDS_BUFSTAT_EOM."
                _isCancelled = bufferStatus.HasFlag(BufferStatus.TDS_BUFSTAT_ATTNACK) || bufferStatus.HasFlag(BufferStatus.TDS_BUFSTAT_ATTN);

                var length = _headerBuffer[2] << 8 | _headerBuffer[3];

                // If there is no more data, stop there.
                if (bufferStatus.HasFlag(BufferStatus.TDS_BUFSTAT_EOM) || length == _environment.HeaderSize)
                {
                    _isEnd = true;
                    break;
                }

                _bodyBufferLength = length - _environment.HeaderSize;
                _bodyBufferPosition = 0;

                received = _networkStream.Read(_bodyBuffer, 0, _bodyBufferLength);

                if (received != _bodyBufferLength)
                {
                    _isEnd = true;
                    throw new IOException("Failed to read the packet body.");
                }

                var bufferedBytes = _bodyBufferLength - _bodyBufferPosition;

                // if this chunk is less than the amount requested, add it all, and then loop.
                if (bufferedBytes <= count - bytesWrittenToBuffer)
                {
                    Array.Copy(_bodyBuffer, 0, buffer, bytesWrittenToBuffer, _bodyBufferLength);
                    bytesWrittenToBuffer += _bodyBufferLength;

                    // Nothing left in the buffer
                    _bodyBufferPosition = 0;
                    _bodyBufferLength = 0;
                }
                // else add part of it and save the rest.
                else
                {
                    _bodyBufferPosition = count - bytesWrittenToBuffer;

                    Array.Copy(_bodyBuffer, 0, buffer, bytesWrittenToBuffer, _bodyBufferPosition);
                    bytesWrittenToBuffer += _bodyBufferPosition;
                }
            }

            // If cancelled, burn
            if (_isCancelled && !_isEnd)
            {
                BurnPackets();

                throw new TokenStreamCancelledException();
            }

            return bytesWrittenToBuffer;
        }

        private void BurnPackets()
        {
            while (!_isEnd)
            {
                int received = _networkStream.Read(_headerBuffer, 0, _environment.HeaderSize);

                if (received != _environment.HeaderSize)
                {
                    _isEnd = true; // TODO - should I terminate here or try to read more, or throw?
                    break;
                }

                var bufferStatus = (BufferStatus) _headerBuffer[1];

                var length = _headerBuffer[2] << 8 | _headerBuffer[3];

                // If there is no more data, stop there.
                if (bufferStatus.HasFlag(BufferStatus.TDS_BUFSTAT_EOM) || length == _environment.HeaderSize)
                {
                    _isEnd = true; 
                    break;
                }

                _bodyBufferLength = length - _environment.HeaderSize;
                _bodyBufferPosition = 0;

                received = _networkStream.Read(_bodyBuffer, 0, _bodyBufferLength);

                if (received != _bodyBufferLength)
                {
                    _isEnd = true; // TODO - should I terminate here or try to read more or throw?
                    break;
                }
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
                    BurnPackets(); // TODO - is this dumb to clean this up in the Dispose method? Considering the case where the socket lives longer than the stream, and the stream is disposed without reading all of the data.

                    _networkStream.Dispose();
                }
            }

            base.Dispose(disposing);
        }
    }
}
