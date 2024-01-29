using System;

namespace CMS.IO.Zip
{
    /// <summary>
    /// FileInfo class for zip files
    /// </summary>
    public class ZipFileInfo : FileInfo
    {
        #region "Variables"

        string mExtension = string.Empty;
        string mFullName = string.Empty;
        string mName = string.Empty;
        
        ZipStorageProvider mProvider = null;

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes new instance of FileInfo.
        /// </summary>
        /// <param name="provider">Parent provider</param>
        /// <param name="filename">File name</param>
        public ZipFileInfo(ZipStorageProvider provider, string filename)
        {
            mExtension = Path.GetExtension(filename);
            mFullName = filename;
            mName = Path.GetFileName(filename);
            
            mProvider = provider;
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
                var entry = mProvider.GetFileEntry(mFullName);
                if (entry == null)
                {
                    return 0;
                }

                return entry.Length;
            }
        }


        /// <summary>
        /// File extension.
        /// </summary>
        public override string Extension
        {
            get
            {
                return mExtension;
            }
        }


        /// <summary>
        /// Full name of file (with whole path).
        /// </summary>
        public override string FullName
        {
            get
            {
                return mFullName;
            }
        }


        /// <summary>
        /// File name of file (without path).
        /// </summary>
        public override string Name
        {
            get
            {
                return mName;
            }
        }


        /// <summary>
        /// Directory name.
        /// </summary>
        public override string DirectoryName
        {
            get
            {
                return Directory.Name;
            }
        }


        /// <summary>
        /// Last write time to file.
        /// </summary>
        public override DateTime LastWriteTime
        {
            get
            {
                return mProvider.ZipFile.LastWriteTime;
            }
            set
            {
                ZipStorageProvider.ThrowReadOnly();
            }
        }


        /// <summary>
        /// If file exists.
        /// </summary>
        public override bool Exists
        {
            get
            {
                return (mProvider.GetFileEntry(mFullName) != null);
            }
        }


        /// <summary>
        /// Creation date of file.
        /// </summary>
        public override DateTime CreationTime
        {
            get
            {
                return mProvider.ZipFile.CreationTime;
            }
            set
            {
                ZipStorageProvider.ThrowReadOnly();
            }
        }


        /// <summary>
        /// Creation date of file.
        /// </summary>
        public override DateTime LastAccessTime
        {
            get
            {
                return mProvider.ZipFile.LastAccessTime;
            }
            set
            {
                ZipStorageProvider.ThrowReadOnly();
            }
        }


        /// <summary>
        /// Directory of file.
        /// </summary>
        public override DirectoryInfo Directory
        {
            get
            {
                var path = Path.GetDirectoryName(FullName);

                return mProvider.GetDirectoryInfo(path);
            }
        }


        /// <summary>
        ///  If is read only.
        /// </summary>
        public override bool IsReadOnly
        {
            get
            {
                return true;
            }
            set
            {
                ZipStorageProvider.ThrowReadOnly();
            }
        }


        /// <summary>
        /// File attributes.
        /// </summary>
        public override FileAttributes Attributes
        {
            get
            {
                return FileAttributes.ReadOnly;
            }
            set
            {
                ZipStorageProvider.ThrowReadOnly();
            }
        }


        /// <summary>
        /// System file info
        /// </summary>
        public override System.IO.FileInfo SystemInfo
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Creates or opens a file for writing UTF-8 encoded text.
        /// </summary> 
        protected override StreamWriter CreateTextInternal()
        {
            ZipStorageProvider.ThrowReadOnly();
            return null;
        }


        /// <summary>
        /// Deletes file.
        /// </summary>
        protected override void DeleteInternal()
        {
            ZipStorageProvider.ThrowReadOnly();
        }


        /// <summary>
        /// Opens file for reading.
        /// </summary> 
        protected override FileStream OpenReadInternal()
        {
            FileStream stream = new ZipFileStream(mProvider, FullName);
            return stream;
        }


        /// <summary>
        /// Copies current file to destination. 
        /// </summary>
        /// <param name="destFileName">Destination file name.</param>
        /// <param name="overwrite">Indicates if existing file should be overwritten</param>        
        protected override FileInfo CopyToInternal(string destFileName, bool overwrite)
        {
            File.Copy(FullName, destFileName, overwrite);

            FileInfo info = New(destFileName);
            return info;
        }


        /// <summary>
        /// Copies an existing file to a new file, allowing the overwriting of an existing file.
        /// </summary>
        /// <param name="destFileName">Destination file name</param>        
        protected override void MoveToInternal(string destFileName)
        {
            ZipStorageProvider.ThrowReadOnly();
        }


        /// <summary>
        /// Converts current info to string.
        /// </summary>        
        public override string ToString()
        {
            return FullName;
        }


        /// <summary>
        /// Creates a StreamReader with UTF8 encoding that reads from an existing text file.
        /// </summary>   
        protected override StreamReader OpenTextInternal()
        {
            return File.OpenText(FullName);
        }

        #endregion
    }
}