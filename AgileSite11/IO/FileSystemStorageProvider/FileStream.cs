using System.Threading;
using System.Threading.Tasks;

using CMS.IO;

using FileAccess = CMS.IO.FileAccess;
using FileMode = CMS.IO.FileMode;
using FileShare = CMS.IO.FileShare;

namespace CMS.FileSystemStorage
{
    /// <summary>
    /// Envelope for FileStream classes (System.IO).
    /// </summary>
    public class FileStream : IO.FileStream
    {
        #region "Private variables"

        private readonly System.IO.FileStream fsStream;
        private int mReadSize = -1;
        private bool mCloseLogged;
        private bool disposed;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets a value indicating whether the current stream supports reading.
        /// </summary>
        public override bool CanRead
        {
            get
            {
                return fsStream.CanRead;
            }
        }


        /// <summary>
        /// Gets a value indicating whether the current stream supports seeking.
        /// </summary>
        /// <returns>True if the stream supports seeking, false otherwise.</returns>
        public override bool CanSeek
        {
            get
            {
                return fsStream.CanSeek;
            }
        }


        /// <summary>
        /// Gets a value indicating whether the current stream supports writing.
        /// </summary>
        public override bool CanWrite
        {
            get
            {
                return fsStream.CanWrite;
            }
        }


        /// <summary>
        /// Gets the length in bytes of the stream.
        /// </summary>
        public override long Length
        {
            get
            {
                return fsStream.Length;
            }
        }


        /// <summary>
        ///  Gets or sets the position within the current stream.
        /// </summary>
        public override long Position
        {
            get
            {
                return fsStream.Position;
            }
            set
            {
                fsStream.Position = value;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes new instance and intializes new system file stream.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="mode">File mode</param>
        public FileStream(string path, FileMode mode)
            : base(path)
        {
            path = AbstractStorageProvider.GetTargetPhysicalPath(path);
            fsStream = new System.IO.FileStream(path, (System.IO.FileMode)mode);
        }


        /// <summary>
        /// Initializes new instance and intializes new system file stream.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="mode">File mode</param>
        /// <param name="access">File access</param>
        public FileStream(string path, FileMode mode, FileAccess access)
            : base(path)
        {
            path = AbstractStorageProvider.GetTargetPhysicalPath(path);
            fsStream = new System.IO.FileStream(path, (System.IO.FileMode)mode, (System.IO.FileAccess)access);
        }


        /// <summary>
        /// Initializes new instance and intializes new system file stream.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="mode">File mode</param>
        /// <param name="access">File access</param>       
        /// <param name="share">Sharing permissions</param>
        public FileStream(string path, FileMode mode, FileAccess access, FileShare share)
            : base(path)
        {
            path = AbstractStorageProvider.GetTargetPhysicalPath(path);
            fsStream = new System.IO.FileStream(path, (System.IO.FileMode)mode, (System.IO.FileAccess)access, (System.IO.FileShare)share);
        }


        /// <summary>
        /// Initializes new instance and intializes new system file stream.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="mode">File mode</param>
        /// <param name="access">File access</param>
        /// <param name="bufferSize">Buffer size</param>
        /// <param name="share">Sharing permissions</param>
        public FileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize)
            : base(path)
        {
            path = AbstractStorageProvider.GetTargetPhysicalPath(path);
            fsStream = new System.IO.FileStream(path, (System.IO.FileMode)mode, (System.IO.FileAccess)access, (System.IO.FileShare)share, bufferSize);
        }


        /// <summary>
        /// Initializes new instance and intializes new system file stream.
        /// </summary>
        /// <param name="fs">File stream</param>        
        public FileStream(System.IO.FileStream fs)
            : base(null)
        {
            fsStream = fs;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Sets the length of the current stream.
        /// </summary>
        /// <param name="value">The desired length of the current stream in bytes.</param>
        public override void SetLength(long value)
        {
            fsStream.SetLength(value);
        }


        /// <summary>
        /// Reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <param name="array">The buffer to write the data into.</param>
        /// <param name="offset">The byte offset in <paramref name="array"/> at which to begin writing data from the stream.</param>
        /// <param name="count">The maximum number of bytes to read.</param>
        public override int Read(byte[] array, int offset, int count)
        {
            int bytesRead = fsStream.Read(array, offset, count);

            if (mReadSize == -1)
            {
                // Log the first read
                LogFileOperation(fsStream.Name, FileDebugOperation.READ, count);
                mReadSize = count;
            }
            else
            {
                // Append read size
                mReadSize += count;
            }

            return bytesRead;
        }


        /// <summary>
        /// Asynchronously reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">The buffer to write the data into.</param>
        /// <param name="offset">The byte offset in <paramref name="buffer"/> at which to begin writing data from the stream.</param>
        /// <param name="count">The maximum number of bytes to read.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            var bytesRead = await fsStream.ReadAsync(buffer, offset, count, cancellationToken);

            if (mReadSize == -1)
            {
                // Log the first read
                LogFileOperation(fsStream.Name, FileDebugOperation.READ_ASYNC, count);
                mReadSize = count;
            }
            else
            {
                // Append read size
                mReadSize += count;
            }

            return bytesRead;
        }


        /// <summary>
        /// Closes the current stream and releases any resources (such as sockets and file handles) associated with the current stream.
        /// </summary>
        public override void Close()
        {
            CloseInnerStream(false, true);
        }


        /// <summary>
        /// Closes or disposes the inner file stream
        /// </summary>
        /// <param name="dispose">If true, the inner stream is closed, otherwise is disposed</param>
        /// <param name="logOperation">If true, the close operation is logged</param>
        private void CloseInnerStream(bool dispose, bool logOperation)
        {
            bool couldWrite = fsStream.CanWrite;

            // Log close operation
            if (logOperation)
            {
                LogCloseOperation();
            }

            if (dispose)
            {
                fsStream.Dispose();
            }
            else
            {
                fsStream.Close();
            }

            // Log the web farm task
            if (couldWrite && (Path != null))
            {
                StorageSynchronization.LogUpdateFileTask(Path);
            }
        }


        /// <summary>
        /// Logs the stream close operation
        /// </summary>
        private void LogCloseOperation()
        {
            if (mReadSize > 0)
            {
                // Log read end
                FileDebug.LogReadEnd(mReadSize);
                mReadSize = -1;
            }

            LogFileOperation(fsStream.Name, FileDebugOperation.CLOSE, -1);

            mCloseLogged = true;
        }


        /// <summary>
        /// Sets cursor position at specified position.
        /// </summary>
        /// <param name="offset">Offset from beginning of file</param>
        /// <param name="loc">Seek origin</param>
        public override long Seek(long offset, System.IO.SeekOrigin loc)
        {
            return fsStream.Seek(offset, loc);
        }


        /// <summary>
        /// Writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">The buffer to write data from.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> from which to begin copying bytes to the stream.</param>
        /// <param name="count">The maximum number of bytes to write.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            LogFileOperation(fsStream.Name, FileDebugOperation.WRITE, count);

            fsStream.Write(buffer, offset, count);
        }


        /// <summary>
        /// Asynchronously writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">The buffer to write data from.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> from which to begin copying bytes to the stream.</param>
        /// <param name="count">The maximum number of bytes to write.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            LogFileOperation(fsStream.Name, FileDebugOperation.WRITE_ASYNC, count);

            return fsStream.WriteAsync(buffer, offset, count, cancellationToken);
        }


        /// <summary>
        /// Clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        public override void Flush()
        {
            fsStream.Flush();
        }


        /// <summary>
        /// Asynchronously clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            return fsStream.FlushAsync(cancellationToken);
        }


        /// <summary>
        /// Writes byte to stream.
        /// </summary>
        /// <param name="value">Value to write</param>
        public override void WriteByte(byte value)
        {
            fsStream.WriteByte(value);
            LogFileOperation(fsStream.Name, FileDebugOperation.WRITE, 1);
        }

        #endregion


        #region IDisposable Members

        /// <summary>
        /// Releases all unmanaged and optionally managed resources.
        /// </summary>
        /// <param name="disposing">When true, managed resources are released.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                CloseInnerStream(true, !mCloseLogged);
            }

            disposed = true;

            base.Dispose(disposing);
        }

        #endregion
    }
}