using System;

namespace CMS.IO
{
    /// <summary>
    /// Provides instance methods for the creation, copying, deletion, moving, and opening of files, and aids in the creation of FileStream  objects.
    /// </summary>
    public abstract class FileInfo
    {
        #region "Variables"

        /// <summary>
        /// System.IO file info object corresponding to this file
        /// </summary>
        protected System.IO.FileInfo mSystemInfo;

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates new instance of FileInfo object.
        /// </summary>
        protected FileInfo()
        {
        }

        #endregion


        #region "Methods for creating new instances"

        /// <summary>
        /// Initializes new instance of file info object.
        /// </summary>
        /// <param name="filename">File name</param>
        public static FileInfo New(string filename)
        {
            return StorageHelper.GetFileInfo(filename);
        }

        #endregion


        #region "Properties"

        /// <summary>
        /// Length of file.
        /// </summary>
        public abstract long Length
        {
            get;
        }


        /// <summary>
        /// File extension.
        /// </summary>
        public abstract string Extension
        {
            get;
        }


        /// <summary>
        /// Full name of file (with whole path).
        /// </summary>
        public abstract string FullName
        {
            get;
        }


        /// <summary>
        /// File name of file (without path).
        /// </summary>
        public abstract string Name
        {
            get;
        }


        /// <summary>
        /// Last write time to file.
        /// </summary>
        public abstract DateTime LastWriteTime
        {
            get;
            set;
        }


        /// <summary>
        /// If file exists.
        /// </summary>
        public abstract bool Exists
        {
            get;
        }


        /// <summary>
        /// Creation date of file.
        /// </summary>
        public abstract DateTime CreationTime
        {
            get;
            set;
        }


        /// <summary>
        /// Last access time of file.
        /// </summary>
        public abstract DateTime LastAccessTime
        {
            get;
            set;
        }


        /// <summary>
        /// Directory of file.
        /// </summary>
        public abstract DirectoryInfo Directory
        {
            get;
        }


        /// <summary>
        ///  If is read only.
        /// </summary>
        public abstract bool IsReadOnly
        {
            get;
            set;
        }


        /// <summary>
        /// File attributes.
        /// </summary>
        public abstract FileAttributes Attributes
        {
            get;
            set;
        }


        /// <summary>
        /// Directory name of file (without path).
        /// </summary>
        public abstract string DirectoryName
        {
            get;
        }


        /// <summary>
        /// System.IO file info object corresponding to this file
        /// </summary>
        public virtual System.IO.FileInfo SystemInfo
        {
            get
            {
                if (mSystemInfo == null)
                {
                    return new System.IO.FileInfo(FullName);
                }

                return mSystemInfo;
            }
        }

        #endregion

        
        #region "Public methods"

        /// <summary>
        /// Creates or opens a file for writing UTF-8 encoded text.
        /// </summary> 
        public StreamWriter CreateText()
        {
            return CreateTextInternal();
        }


        /// <summary>
        /// Deletes file.
        /// </summary>
        public void Delete()
        {
            using (var h = IOEvents.DeleteFile.StartEvent(FullName))
            {
                DeleteInternal();

                FileDebug.LogFileOperation(FullName, FileDebugOperation.DELETE);

                h.FinishEvent();
            }
        }


        /// <summary>
        /// Creates a read-only ICMSFileStream.
        /// </summary> 
        public IO.FileStream OpenRead()
        {
            return OpenReadInternal();
        }


        /// <summary>
        /// Copies current file to destination. 
        /// </summary>
        /// <param name="destFileName">Destination file name.</param>
        public IO.FileInfo CopyTo(string destFileName)
        {
            return CopyTo(destFileName, false);
        }


        /// <summary>
        /// Copies current file to destination. 
        /// </summary>
        /// <param name="destFileName">Destination file name.</param>
        /// <param name="overwrite">Indicates if existing file should be overwritten</param>        
        public IO.FileInfo CopyTo(string destFileName, bool overwrite)
        {
            return CopyToInternal(destFileName, overwrite);
        }


        /// <summary>
        /// Moves an existing file to a new file.
        /// </summary>
        /// <param name="destFileName">Destination file name.</param>        
        public void MoveTo(string destFileName)
        {
            MoveToInternal(destFileName);

            FileDebug.LogFileOperation(FullName + ";" + destFileName, FileDebugOperation.MOVE);
        }


        /// <summary>
        /// Creates a StreamReader with UTF8 encoding that reads from an existing text file.
        /// </summary>   
        public StreamReader OpenText()
        {
            return OpenTextInternal();
        }

        #endregion


        #region "Internal methods"
        
        /// <summary>
        /// Creates or opens a file for writing UTF-8 encoded text.
        /// </summary> 
        protected abstract StreamWriter CreateTextInternal();
        

        /// <summary>
        /// Deletes file.
        /// </summary>
        protected abstract void DeleteInternal();


        /// <summary>
        /// Creates a read-only FileStream.
        /// </summary> 
        protected abstract FileStream OpenReadInternal();


        /// <summary>
        /// Creates a StreamReader with UTF8 encoding that reads from an existing text file.
        /// </summary>        
        protected abstract StreamReader OpenTextInternal();
        

        /// <summary>
        /// Copies an existing file to a new file, allowing the overwriting of an existing file.
        /// </summary>
        /// <param name="destFileName">Destination file name</param>
        /// <param name="overwrite">Whether overwriting is allowed</param>
        protected abstract FileInfo CopyToInternal(string destFileName, bool overwrite);
        

        /// <summary>
        /// Moves an existing file to a new file.
        /// </summary>
        /// <param name="destFileName">Destination file name</param>        
        protected abstract void MoveToInternal(string destFileName);
        
        #endregion
    }
}