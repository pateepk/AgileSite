using System.Data;
using System.IO;

namespace CMS.IO
{
    /// <summary>
    /// Exposes a stream around a file, supporting both synchronous and asynchronous read and write operations.
    /// </summary>
    public abstract class FileStream : Stream
    {
        #region "Constructors"

        /// <summary>
        /// Creates new instance of FileStream object.
        /// </summary>
        /// <param name="path">File path</param>
        protected FileStream(string path)
        {
            Path = path;
        }

        #endregion


        #region "Methods for creating new instances"

        /// <summary>
        /// Initializes new instance and intializes new system file stream.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="mode">File mode</param>
        public static FileStream New(string path, FileMode mode)
        {
            FileDebug.LogFileOperation(path, FileDebugOperation.OPEN);
            return StorageHelper.GetFileStream(path, mode);
        }


        /// <summary>
        /// Initializes new instance and intializes new system file stream.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="mode">File mode</param>
        /// <param name="access">File access</param>
        public static FileStream New(string path, FileMode mode, FileAccess access)
        {
            FileDebug.LogFileOperation(path, FileDebugOperation.OPEN, -1, "", access.ToString());
            return StorageHelper.GetFileStream(path, mode, access);
        }


        /// <summary>
        /// Initializes new instance and intializes new system file stream.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="mode">File mode</param>
        /// <param name="access">File access</param>
        /// <param name="share">Sharing permissions</param>     
        public static FileStream New(string path, FileMode mode, FileAccess access, FileShare share)
        {
            FileDebug.LogFileOperation(path, FileDebugOperation.OPEN, -1, "", access + ", " + share);
            return StorageHelper.GetFileStream(path, mode, access, share);
        }


        /// <summary>
        /// Initializes new instance and intializes new system file stream.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="mode">File mode</param>
        /// <param name="access">File access</param>
        /// <param name="share">Sharing permissions</param>
        /// <param name="bufferSize">Buffer size</param>
        public static FileStream New(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize)
        {
            FileDebug.LogFileOperation(path, FileDebugOperation.OPEN, -1, "", access + ", " + share);
            return StorageHelper.GetFileStream(path, mode, access, share, bufferSize);
        }

        #endregion


        #region "Properties"

        /// <summary>
        /// Stream path
        /// </summary>
        public string Path
        {
            get;
            protected set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Logs the file operation. Logs the file operation to the file and to current request log for debugging.
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <param name="fileOperation">Operation with file (open, close, read, write)</param>
        /// <param name="size">Size of the read / write operation in bytes</param>
        public virtual DataRow LogFileOperation(string filePath, string fileOperation, int size)
        {
            return FileDebug.LogFileOperation(filePath, fileOperation, size);
        }

        #endregion
    }
}