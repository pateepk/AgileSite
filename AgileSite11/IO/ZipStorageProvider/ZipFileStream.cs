using System;

namespace CMS.IO.Zip
{
    /// <summary>
    /// Represents read-only stream from the zip file.
    /// </summary>
    public class ZipFileStream : FileStream
    {
        #region "Private variables"

        /// <summary>
        /// Underlying memory stream
        /// </summary>
        private readonly System.IO.Stream mStream;

        /// <summary>
        /// Number of bytes that were read
        /// </summary>
        private int mReadSize = -1;


        private bool mDisposed;

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes new instance and initializes new system file stream.
        /// </summary>
        /// <param name="provider">Zip provider</param>
        /// <param name="path">Path to file.</param>
        public ZipFileStream(ZipStorageProvider provider, string path)
            : base(path)
        {
            mStream = provider.GetFileStream(Path);
        }

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets a value indicating whether the current stream supports reading.
        /// </summary>
        public override bool CanRead
        {
            get
            {
                return mStream.CanRead;
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
                return mStream.CanSeek;
            }
        }


        /// <summary>
        /// Gets a value indicating whether the current stream supports writing.
        /// </summary>
        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }


        /// <summary>
        /// Gets the length in bytes of the stream.
        /// </summary>
        public override long Length
        {
            get
            {
                return mStream.Length;
            }
        }


        /// <summary>
        ///  Gets or sets the position within the current stream.
        /// </summary>
        public override long Position
        {
            get
            {
                return mStream.Position;
            }
            set
            {
                mStream.Position = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Sets the length of the current stream.
        /// </summary>
        /// <param name="value">The desired length of the current stream in bytes.</param>
        public override void SetLength(long value)
        {
            mStream.SetLength(value);
        }


        /// <summary>
        /// Reads data from stream and stores them into array.
        /// </summary>
        /// <param name="array">Array where result is stored</param>
        /// <param name="offset">Offset from beginning of file</param>
        /// <param name="count">Number of characters which are read</param>
        public override int Read(byte[] array, int offset, int count)
        {
            int retval = mStream.Read(array, offset, count);

            if (mReadSize == -1)
            {
                // Log the first read
                LogFileOperation(Path, "Read", count);
                mReadSize = count;
            }
            else
            {
                // Append read size
                mReadSize += count;
            }

            return retval;
        }


        /// <summary>
        /// Sets cursor position at specified position.
        /// </summary>
        /// <param name="offset">Offset from beginning of file</param>
        /// <param name="loc">Reference points for seeking in the stream</param>
        public override long Seek(long offset, System.IO.SeekOrigin loc)
        {
            return mStream.Seek(offset, loc);
        }


        /// <summary>
        /// Writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="offset">Offset</param>
        /// <param name="count">Number of chars</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }


        /// <summary>
        /// Clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        public override void Flush()
        {
            throw new NotSupportedException();
        }


        /// <summary>
        /// Writes byte to stream.
        /// </summary>
        /// <param name="value">Value to write</param>
        public override void WriteByte(byte value)
        {
            throw new NotSupportedException();
        }

        #endregion


        #region IDisposable Members

        /// <summary>
        /// Releases all unmanaged and optionally managed resources.
        /// </summary>
        /// <param name="disposing">When true, managed resources are released.</param>
        protected override void Dispose(bool disposing)
        {
            if (mDisposed)
            {
                return;
            }

            if (disposing)
            {
                if (mReadSize > 0)
                {
                    // Log read end
                    FileDebug.LogReadEnd(mReadSize);
                    mReadSize = -1;
                }

                LogFileOperation(Path, FileDebugOperation.CLOSE, -1);
                mStream.Dispose();
            }

            mDisposed = true;

            base.Dispose(disposing);
        }

        #endregion
    }
}