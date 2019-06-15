using System;
using System.IO;
using System.Net.Sockets;
using AdoNetCore.AseClient.Enum;

namespace AdoNetCore.AseClient.Internal
{
    /// <summary>
    /// The TokenStream is a read-only stream that can read token data from packets coming from the network.
    /// </summary>
    /// Packet1|Packet2|Packet3|...
    /// [header][body]|[header][body]|[header][body]|
    internal sealed class TokenStream : NetworkStream
    {
        /// <summary>
        /// The network stream to read data from.
        /// </summary>
        private readonly DbEnvironment _environment;
        private readonly byte[] _headerBuffer;
        private readonly byte[] _bodyBuffer;
        private int _bodyBufferPosition;
        private int _bodyBufferLength;
        private bool _isDisposed;
        private bool _bufferHasBytes;
        private bool _networkHasBytes;

        public bool IsCancelled { get; private set; }


        public override bool DataAvailable => _networkHasBytes || _bufferHasBytes;

        public TokenStream(Socket socket, DbEnvironment environment) : base(socket, false)
        {
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
            if (_bodyBufferPosition > 0)
            {
                var bufferedBytes = _bodyBufferLength - _bodyBufferPosition;

                // if this chunk is less than the amount requested, add it all.
                if (bufferedBytes <= count)
                {
                    Buffer.BlockCopy(_bodyBuffer, _bodyBufferPosition, buffer, 0, bufferedBytes);
                    bytesWrittenToBuffer += bufferedBytes;

                    // Nothing left in the buffer
                    _bodyBufferPosition = 0;
                    _bodyBufferLength = 0;
                    _bufferHasBytes = false;
                }
                // else add part of it and save the rest.
                else
                {
                    Buffer.BlockCopy(_bodyBuffer, _bodyBufferPosition, buffer, bytesWrittenToBuffer, count);
                    bytesWrittenToBuffer += count;

                    _bodyBufferPosition += count;
                    _bufferHasBytes = true;
                }
            }

            // If we need more data, let's read if from the network until the buffer is full.
            while (bytesWrittenToBuffer < count)
            {
                BufferBytes(_headerBuffer, 0, _environment.HeaderSize);

                var bufferStatus = (BufferStatus)_headerBuffer[1];

                //" If TDS_BUFSTAT_ATTNACK not also TDS_BUFSTAT_EOM, continue reading packets until TDS_BUFSTAT_EOM."
                IsCancelled = bufferStatus.HasFlag(BufferStatus.TDS_BUFSTAT_ATTNACK) || bufferStatus.HasFlag(BufferStatus.TDS_BUFSTAT_ATTN);

                var length = _headerBuffer[2] << 8 | _headerBuffer[3];

                _bodyBufferLength = length - _environment.HeaderSize;
                _bodyBufferPosition = 0;

                BufferBytes(_bodyBuffer, 0, _bodyBufferLength);

                // if this chunk is less than the amount requested, add it all, and then loop.
                if (bytesWrittenToBuffer + _bodyBufferLength < count)
                {
                    Buffer.BlockCopy(_bodyBuffer, 0, buffer, bytesWrittenToBuffer, _bodyBufferLength);
                    bytesWrittenToBuffer += _bodyBufferLength;

                    // Nothing left in the buffer
                    _bodyBufferPosition = 0;
                    _bodyBufferLength = 0;
                    _bufferHasBytes = false;
                }
                // else add part of it and save the rest.
                else
                {
                    var bytesInLastChunk = count - bytesWrittenToBuffer;

                    Buffer.BlockCopy(_bodyBuffer, 0, buffer, bytesWrittenToBuffer, bytesInLastChunk);
                    bytesWrittenToBuffer += bytesInLastChunk;

                    _bodyBufferPosition = bytesInLastChunk;
                    _bufferHasBytes = true;
                }

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

        private void BurnPackets()
        {
            while (_networkHasBytes)
            {
                BufferBytes(_headerBuffer, 0, _environment.HeaderSize);

                var bufferStatus = (BufferStatus) _headerBuffer[1];

                var length = _headerBuffer[2] << 8 | _headerBuffer[3];

                // If there is no more data, stop there.
                if (bufferStatus.HasFlag(BufferStatus.TDS_BUFSTAT_EOM) || length == _environment.HeaderSize)
                {
                    _networkHasBytes = false;
                    break;
                }

                _bodyBufferLength = length - _environment.HeaderSize;
                _bodyBufferPosition = 0;

                BufferBytes(_bodyBuffer, 0, _bodyBufferLength);
            }

            _bufferHasBytes = false;
            _bodyBufferLength = 0;
            _bodyBufferPosition = 0;
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
                    BurnPackets(); // Considering the case where the socket lives longer than the stream, and the stream is disposed without reading all of the data.
                }
            }

            base.Dispose(disposing);
        }

        private void BufferBytes(byte[] buffer, int offset, int count)
        {
            if (count > 0)
            {
                var remainingBytes = count;
                var totalReceivedBytes = 0;
                do
                {
                    var receivedBytes = base.Read(buffer, offset + totalReceivedBytes, remainingBytes);
                    remainingBytes -= receivedBytes;
                    totalReceivedBytes += receivedBytes;
                } while (remainingBytes > 0);
            }
        }
    }
}
