using System;
using System.IO;

namespace AdoNetCore.AseClient.Internal
{
    internal class ReadablePartialStream : Stream
    {
        private readonly Stream _inner;
        private readonly uint _length;
        private uint _position;

        public ReadablePartialStream(Stream inner, uint length)
        {
            _inner = inner;
            _length = length;
        }

        public ReadablePartialStream(Stream inner, int length)
        {
            _inner = inner;
            _length = Convert.ToUInt32(length);
        }

        public override void Flush()
        {
            throw new InvalidOperationException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if ((_length - _position) >= count)
            {
                _position += Convert.ToUInt32(count);
                var bytesRead = _inner.Read(buffer, offset, count);
                if (bytesRead < count)
                {
                    _position = _length;
                }
                return bytesRead;
            }

            throw new EndOfStreamException();
        }

        public override int ReadByte()
        {
            if (_position < _length)
            {
                _position++;
                return _inner.ReadByte();
            }

            throw new EndOfStreamException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new InvalidOperationException();
        }

        public override void SetLength(long value)
        {
            throw new InvalidOperationException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new InvalidOperationException();
        }

        public override bool CanRead => _inner.CanRead;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => _length;

        public override long Position
        {
            get { return _position; }
            set { throw new InvalidOperationException(); }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            //Debug.Assert(_position == Length, "Readable Partial Stream was not fully consumed");
            while (_position < _length)
            {
                ReadByte();
            }
        }
    }
}
