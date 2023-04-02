using System;
using System.IO;
using System.Threading;

namespace doob.Middler.Common.StreamHelper
{

    public class AutoStream : Stream
    {

        public override bool CanRead => InnerStream.CanRead;
        public override bool CanSeek => InnerStream.CanSeek;
        public override bool CanWrite => InnerStream.CanWrite;
        public override long Length => InnerStream.Length;
        public override long Position
        {
            get => InnerStream.Position;
            set => InnerStream.Position = value;
        }

        public AutoStreamOptions Options { get; private set; }

        public CancellationToken CancellationToken { get; private set; }

        private Stream InnerStream { get; set; } = new MemoryStream();
        private bool IsFileStream { get; set; }


        public AutoStream(Action<AutoStreamOptionsBuilder> options, CancellationToken cancellationToken = default) : this((AutoStreamOptionsBuilder)options, cancellationToken)
        {

        }

        public AutoStream(AutoStreamOptions options, CancellationToken cancellationToken = default)
        {
            Options = options;
            CancellationToken = cancellationToken;
        }


        public override void Flush()
        {
            InnerStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return InnerStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return InnerStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            InnerStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {

            if (!IsFileStream)
            {
                var allowMemoryBuffer = (Options.MemoryThreshold - count) >= InnerStream.Length;
                if (!allowMemoryBuffer)
                {
                    IsFileStream = true;
                    var tempStream = InnerStream;

                    EnsureFileStream();
                    tempStream.Seek(0, SeekOrigin.Begin);
                    int copyBufferSize = this.GetCopyBufferSize();
                    tempStream.CopyTo(InnerStream);
                    InnerStream.Flush();
                    tempStream.Dispose();
                }
            }

            InnerStream.Write(buffer, offset, count);

        }

        private int GetCopyBufferSize()
        {
            int num = 81920;
            if (this.CanSeek)
            {
                long length = this.Length;
                long position = this.Position;
                if (length <= position)
                {
                    num = 1;
                }
                else
                {
                    long val2 = length - position;
                    if (val2 > 0L)
                        num = (int)Math.Min((long)num, val2);
                }
            }
            return num;
        }


        private void EnsureFileStream()
        {

            var tempFileName = Path.Combine(Options.TempDirectory, $"{Options.FilePrefix}_{Guid.NewGuid():n}.tmp");
            InnerStream = new FileStream(
                tempFileName,
                FileMode.Create,
                FileAccess.ReadWrite,
                FileShare.Delete,
                bufferSize: 1,
                FileOptions.Asynchronous | FileOptions.SequentialScan | FileOptions.DeleteOnClose);

        }


        protected override void Dispose(bool disposing)
        {
            InnerStream.Dispose();
            base.Dispose(disposing);
        }

    }



    public class MemoryFileStream : Stream
    {

        private MemoryStream _memoryStream;
        private FileStream _fileStream;
        private bool _usingMemory;

        public AutoStreamOptions Options { get; private set; }
        public CancellationToken CancellationToken { get; private set; }
        
        public MemoryFileStream(Action<AutoStreamOptionsBuilder> options, CancellationToken cancellationToken = default) : this((AutoStreamOptionsBuilder)options, cancellationToken)
        {

        }

        public MemoryFileStream(AutoStreamOptions options, CancellationToken cancellationToken = default)
        {
            Options = options;
            _memoryStream = new MemoryStream();
            _usingMemory = true;
            CancellationToken = cancellationToken;
        }


        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => true;

        public override long Length => _usingMemory ? _memoryStream.Length : _fileStream.Length;

        public override long Position { get; set; }

        public override void Flush()
        {
            if (_usingMemory)
            {
                _memoryStream.Flush();
            }
            else
            {
                _fileStream.Flush();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int bytesRead;
            if (_usingMemory)
            {
                bytesRead = _memoryStream.Read(buffer, offset, count);
            }
            else
            {
                bytesRead = _fileStream.Read(buffer, offset, count);
            }
            return bytesRead;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long newPosition;
            if (_usingMemory)
            {
                newPosition = _memoryStream.Seek(offset, origin);
            }
            else
            {
                newPosition = _fileStream.Seek(offset, origin);
            }
            return newPosition;
        }

        public override void SetLength(long value)
        {
            if (_usingMemory)
            {
                _memoryStream.SetLength(value);
            }
            else
            {
                _fileStream.SetLength(value);
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (_usingMemory)
            {
                _memoryStream.Write(buffer, offset, count);
                if (_memoryStream.Position > Options.MemoryThreshold)
                {
                    SwitchToFile();
                }
            }
            else
            {
                _fileStream.Write(buffer, offset, count);
            }
        }

        private void SwitchToFile()
        {
            _usingMemory = false;
            var tempFileName = Path.Combine(Options.TempDirectory, $"{Options.FilePrefix}_{Guid.NewGuid():n}.tmp");
            _fileStream = new FileStream(tempFileName, FileMode.Create, FileAccess.Write);
            _memoryStream.WriteTo(_fileStream);
            _memoryStream.Dispose();
            _memoryStream = null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_memoryStream != null)
                {
                    _memoryStream.Dispose();
                }
                if (_fileStream != null)
                {
                    _fileStream.Dispose();
                }
            }
            base.Dispose(disposing);
        }
    }
}
