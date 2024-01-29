using System;

namespace CMS.IO
{
    /// <summary>
    /// Zip provider loader class
    /// </summary>
    public class ZipProviderLoader : AbstractStorageProvider, IDisposable
    {
        #region "Variables"

        private readonly object initLocker = new object();
        const string NOT_IMPLEMENTED_MESSAGE = "[ZipProviderLoader]: This object only serves as a loader for the ZipStorageProvider object, it doesn't provide any IO access on it's own.";

        #endregion


        #region "Properties"

        /// <summary>
        /// Zip file path
        /// </summary>
        public string FilePath
        {
            get;
            protected set;
        }


        /// <summary>
        /// Zip storage provider
        /// </summary>
        protected ZipStorageProvider ZipProvider 
        { 
            get; 
            set; 
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="zipFilePath">Path to the zip file</param>
        /// <param name="mappedPath">Mapped path</param>
        /// <param name="parentProvider">Parent storage provider</param>
        public ZipProviderLoader(string zipFilePath, string mappedPath, AbstractStorageProvider parentProvider)
        {
            FilePath = zipFilePath;
            MappedPath = mappedPath;
            ParentStorageProvider = parentProvider;
        }

        
        /// <summary>
        /// Disposes the object
        /// </summary>
        public void Dispose()
        {
            if (ZipProvider != null)
            {
                ZipProvider.Dispose();
            }
        }


        /// <summary>
        /// Gets the storage provider based on the given path
        /// </summary>
        /// <param name="path">Input path</param>
        protected override AbstractStorageProvider GetInternalProvider(string path)
        {
            if (ZipProvider == null)
            {
                lock (initLocker)
                {
                    if (ZipProvider == null)
                    {
                        ZipProvider = new ZipStorageProvider(FilePath, MappedPath, ParentStorageProvider);
                    }
                }
            }

            return ZipProvider;
        }

        #endregion


        #region "Not implemented methods"

        /// <summary>
        /// Creates new directory provider object
        /// </summary>
        /// <exception cref="NotImplementedException">Zip provider doesn't provide any IO access on it's own.</exception>
        protected override AbstractFile CreateFileProviderObject()
        {
            throw new NotImplementedException(NOT_IMPLEMENTED_MESSAGE);
        }


        /// <summary>
        /// Creates new directory provider object
        /// </summary>
        /// <exception cref="NotImplementedException">Zip provider doesn't provide any IO access on it's own.</exception>
        protected override AbstractDirectory CreateDirectoryProviderObject()
        {
            throw new NotImplementedException(NOT_IMPLEMENTED_MESSAGE);
        }


        /// <summary>
        /// Returns new instance of FileInfo object.
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <exception cref="NotImplementedException">Zip provider doesn't provide any IO access on it's own.</exception>
        public override FileInfo GetFileInfo(string fileName)
        {
            throw new NotImplementedException(NOT_IMPLEMENTED_MESSAGE);
        }


        /// <summary>
        /// Returns new instance of directory info.
        /// </summary>
        /// <param name="path">Path</param>       
        /// <exception cref="NotImplementedException">Zip provider doesn't provide any IO access on it's own.</exception> 
        public override DirectoryInfo GetDirectoryInfo(string path)
        {
            throw new NotImplementedException(NOT_IMPLEMENTED_MESSAGE);
        }


        /// <summary>
        /// Returns new instance of file stream.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="mode">File mode</param>
        /// <exception cref="NotImplementedException">Zip provider doesn't provide any IO access on it's own.</exception>        
        public override FileStream GetFileStream(string path, FileMode mode)
        {
            throw new NotImplementedException(NOT_IMPLEMENTED_MESSAGE);
        }


        /// <summary>
        /// Returns new instance of file stream.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="mode">File mode</param>    
        /// <param name="access">File access</param>
        /// <exception cref="NotImplementedException">Zip provider doesn't provide any IO access on it's own.</exception>
        public override FileStream GetFileStream(string path, FileMode mode, FileAccess access)
        {
            throw new NotImplementedException(NOT_IMPLEMENTED_MESSAGE);
        }


        /// <summary>
        /// Returns new instance of file stream.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="mode">File mode</param>    
        /// <param name="access">File access</param>        
        /// <param name="share">Sharing permissions</param>
        /// <exception cref="NotImplementedException">Zip provider doesn't provide any IO access on it's own.</exception>
        public override FileStream GetFileStream(string path, FileMode mode, FileAccess access, FileShare share)
        {
            throw new NotImplementedException(NOT_IMPLEMENTED_MESSAGE);
        }


        /// <summary>
        /// Returns new instance of file stream.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="mode">File mode</param>    
        /// <param name="access">File access</param>
        /// <param name="bufferSize">Buffer size</param>
        /// <param name="share">Sharing permissions</param>
        /// <exception cref="NotImplementedException">Zip provider doesn't provide any IO access on it's own.</exception>
        public override FileStream GetFileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize)
        {
            throw new NotImplementedException(NOT_IMPLEMENTED_MESSAGE);
        }

        #endregion
    }
}
