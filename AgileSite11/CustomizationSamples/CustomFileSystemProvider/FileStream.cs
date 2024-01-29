using System;

using SystemIO = System.IO;
namespace CustomFileSystemProvider
{
    /// <summary>
    /// Sample of file stream of CMS.IO provider.
    /// </summary>
    public class FileStream : CMS.IO.FileStream
    {
        #region "Constructors"

        /// <summary>
        /// Initializes new instance and initializes new system file stream.
        /// </summary>
        /// <param name="path">Path to file.</param>
        /// <param name="mode">File mode.</param>
        public FileStream(string path, CMS.IO.FileMode mode)
            : this(path, mode, mode == CMS.IO.FileMode.Append ? CMS.IO.FileAccess.Write : CMS.IO.FileAccess.ReadWrite)
        {
        }


        /// <summary>
        /// Initializes new instance and initializes new system file stream.
        /// </summary>
        /// <param name="path">Path to file.</param>
        /// <param name="mode">File mode.</param>
        /// <param name="access">File access.</param>
        public FileStream(string path, CMS.IO.FileMode mode, CMS.IO.FileAccess access)
            : this(path, mode, access, CMS.IO.FileShare.Read)
        {
        }


        /// <summary>
        /// Initializes new instance and initializes new system file stream.
        /// </summary>
        /// <param name="path">Path to file.</param>
        /// <param name="mode">File mode.</param>
        /// <param name="access">File access.</param>       
        /// <param name="share">Sharing permissions.</param>
        public FileStream(string path, CMS.IO.FileMode mode, CMS.IO.FileAccess access, CMS.IO.FileShare share)
            : this(path, mode, access, share, 0x1000)
        {
        }


        /// <summary>
        /// Initializes new instance and initializes new system file stream.
        /// </summary>
        /// <param name="path">Path to file.</param>
        /// <param name="mode">File mode.</param>
        /// <param name="access">File access.</param>
        /// <param name="bSize">Buffer size.</param>
        /// <param name="share">Sharing permissions.</param>
        public FileStream(string path, CMS.IO.FileMode mode, CMS.IO.FileAccess access, CMS.IO.FileShare share, int bSize)
            : base(path)
        {
        }

        #endregion


        #region "Public properties"

        /// <summary>
        /// Gets a value indicating whether the current stream supports reading.
        /// </summary>
        public override bool CanRead
        {
            get
            {
                throw new NotImplementedException();
            }
        }


        /// <summary>
        /// Gets a value indicating whether the current stream supports seeking.
        /// </summary>
        public override bool CanSeek
        {
            get
            {
                throw new NotImplementedException();
            }
        }


        /// <summary>
        /// Gets a value indicating whether the current stream supports writing.
        /// </summary>
        public override bool CanWrite
        {
            get
            {
                throw new NotImplementedException();
            }
        }


        /// <summary>
        /// Length of stream.
        /// </summary>
        public override long Length
        {
            get
            {
                throw new NotImplementedException();
            }
        }


        /// <summary>
        /// Gets or sets position of current stream.
        /// </summary>
        public override long Position
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }        

        #endregion


        #region "Public methods"

        /// <summary>
        /// Reads data from stream and stores them into array.
        /// </summary>
        /// <param name="array">Array where result is stored.</param>
        /// <param name="offset">Offset from begin of file.</param>
        /// <param name="count">Number of characters which are read.</param>
        public override int Read(byte[] array, int offset, int count)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Closes current stream.
        /// </summary>
        public override void Close()
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        public override void Flush()
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Writes sequence of bytes to stream.
        /// </summary>
        /// <param name="buffer">Buffer.</param>
        /// <param name="offset">Offset.</param>
        /// <param name="count">Count.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Sets the position within the current stream to the specified value.
        /// </summary>
        /// <param name="offset">Offset</param>
        /// <param name="loc">Location</param>
        public override long Seek(long offset, SystemIO.SeekOrigin loc)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Set length to stream.
        /// </summary>
        /// <param name="value">Value to set.</param>
        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Writes byte to the stream.
        /// </summary>
        /// <param name="value">Value to write.</param>
        public override void WriteByte(byte value)
        {
            throw new NotImplementedException();
        }

        #endregion


        #region IDisposable Members

        /// <summary>
        /// Releases all unmanaged and optionally managed resources.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
