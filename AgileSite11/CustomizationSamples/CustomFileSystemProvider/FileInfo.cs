using System;

using CMS.IO;

namespace CustomFileSystemProvider
{
    /// <summary>
    /// Sample of FileInfo class of CMS.IO provider.
    /// </summary>
    public class FileInfo : CMS.IO.FileInfo
    {
        #region "Constructors"

        /// <summary>
        /// Initializes new instance of
        /// </summary>
        /// <param name="filename">File name.</param>
        public FileInfo(string filename)
        {

        }

        #endregion


        #region "Properties"

        /// <summary>
        /// Length of file.
        /// </summary>
        public override long Length
        {
            get
            {
                throw new NotImplementedException();
            }
        }


        /// <summary>
        /// File extension.
        /// </summary>
        public override string Extension
        {
            get
            {
                throw new NotImplementedException();
            }
        }


        /// <summary>
        /// Full name of file (with whole path).
        /// </summary>
        public override string FullName
        {
            get
            {
                throw new NotImplementedException();
            }
        }


        /// <summary>
        /// File name of file (without path).
        /// </summary>
        public override string Name
        {
            get
            {
                throw new NotImplementedException();
            }
        }


        /// <summary>
        /// Last write time to file.
        /// </summary>
        public override DateTime LastWriteTime
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


        /// <summary>
        /// If file exists.
        /// </summary>
        public override bool Exists
        {
            get
            {
                throw new NotImplementedException();
            }
        }


        /// <summary>
        /// Creation date of file.
        /// </summary>
        public override DateTime CreationTime
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


        /// <summary>
        /// Directory of file.
        /// </summary>
        public override CMS.IO.DirectoryInfo Directory
        {
            get
            {
                throw new NotImplementedException();
            }
        }


        /// <summary>
        ///  If is read only.
        /// </summary>
        public override bool IsReadOnly
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


        /// <summary>
        /// File attributes.
        /// </summary>
        public override CMS.IO.FileAttributes Attributes
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


        /// <summary>
        /// Directory name.
        /// </summary>
        public override string DirectoryName
        {
            get
            {
                throw new NotImplementedException();
            }
        }


        /// <summary>
        /// Last access time.
        /// </summary>
        public override DateTime LastAccessTime
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


        #region "Methods"

        /// <summary>
        /// Creates or opens a file for writing UTF-8 encoded text.
        /// </summary> 
        protected override CMS.IO.StreamWriter CreateTextInternal()
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Deletes file.
        /// </summary>
        protected override void DeleteInternal()
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Creates a read-only ICMSFileStream.
        /// </summary> 
        protected override CMS.IO.FileStream OpenReadInternal()
        {
            throw new NotImplementedException();
        }
        

        /// <summary>
        /// Copies current file to destination. 
        /// </summary>
        /// <param name="destFileName">Destination file name.</param>
        /// <param name="overwrite">Indicates if existing file should be overwritten</param>        
        protected override CMS.IO.FileInfo CopyToInternal(string destFileName, bool overwrite)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Moves an existing file to a new file.
        /// </summary>
        /// <param name="destFileName">Destination file name.</param>        
        protected override void MoveToInternal(string destFileName)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Creates a StreamReader with UTF8 encoding that reads from an existing text file.
        /// </summary>  
        protected override StreamReader OpenTextInternal()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
