using System;

using CMS.IO;

namespace CMS.FileSystemStorage
{
    /// <summary>
    /// Envelope for System.IO.FileInfo class.
    /// </summary>
    public class FileInfo : IO.FileInfo
    {
        #region "Variables"

        private readonly System.IO.FileInfo systemFileInfo;
        private DirectoryInfo mDirectory;

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes new instance of FileInfo.
        /// </summary>
        /// <param name="filename">File name from which info should be created</param>
        public FileInfo(string filename)
        {
            filename = AbstractStorageProvider.GetTargetPhysicalPath(filename);
            systemFileInfo = new System.IO.FileInfo(filename);
        }


        /// <summary>
        /// Initializes new instance of FileInfo.
        /// </summary>
        /// <param name="info">System file info</param>
        internal FileInfo(System.IO.FileInfo info)
        {
            systemFileInfo = info;
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
                return systemFileInfo.Length;
            }
        }


        /// <summary>
        /// File extension.
        /// </summary>
        public override string Extension
        {
            get
            {
                return systemFileInfo.Extension;
            }
        }


        /// <summary>
        /// Full name of file (with whole path).
        /// </summary>
        public override string FullName
        {
            get
            {
                return systemFileInfo.FullName;
            }
        }


        /// <summary>
        /// File name of file (without path).
        /// </summary>
        public override string Name
        {
            get
            {
                return systemFileInfo.Name;
            }
        }


        /// <summary>
        /// Directory name of file (without path).
        /// </summary>
        public override string DirectoryName
        {
            get
            {
                return systemFileInfo.DirectoryName;
            }
        }


        /// <summary>
        /// Last write time to file.
        /// </summary>
        public override DateTime LastWriteTime
        {
            get
            {
                return systemFileInfo.LastWriteTime;
            }
            set
            {
                systemFileInfo.LastWriteTime = value;
            }
        }


        /// <summary>
        /// If file exists.
        /// </summary>
        public override bool Exists
        {
            get
            {
                return systemFileInfo.Exists;
            }
        }


        /// <summary>
        /// Creation date of file.
        /// </summary>
        public override DateTime CreationTime
        {
            get
            {
                return systemFileInfo.CreationTime;
            }
            set
            {
                systemFileInfo.CreationTime = value;
            }
        }


        /// <summary>
        /// Creation date of file.
        /// </summary>
        public override DateTime LastAccessTime
        {
            get
            {
                return systemFileInfo.LastAccessTime;
            }
            set
            {
                systemFileInfo.LastAccessTime = value;
            }
        }


        /// <summary>
        /// Directory of file.
        /// </summary>
        public override IO.DirectoryInfo Directory
        {
            get
            {
                // Set directory fields
                if (mDirectory == null)
                {
                    System.IO.DirectoryInfo systemDirectory = systemFileInfo.Directory;
                    mDirectory = new DirectoryInfo(systemDirectory);
                }
                return mDirectory;
            }
        }


        /// <summary>
        ///  If is read only.
        /// </summary>
        public override bool IsReadOnly
        {
            get
            {
                return systemFileInfo.IsReadOnly;
            }
            set
            {
                systemFileInfo.IsReadOnly = value;
            }
        }


        /// <summary>
        /// File attributes.
        /// </summary>
        public override FileAttributes Attributes
        {
            get
            {
                return (FileAttributes)systemFileInfo.Attributes;
            }
            set
            {
                systemFileInfo.Attributes = (System.IO.FileAttributes)value;
            }
        }


        /// <summary>
        /// Returns system file info object.
        /// </summary>
        public override System.IO.FileInfo SystemInfo
        {
            get
            {
                return systemFileInfo;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Creates or opens a file for writing UTF-8 encoded text.
        /// </summary> 
        protected override StreamWriter CreateTextInternal()
        {
            System.IO.StreamWriter sw = systemFileInfo.CreateText();

            // Create new IO StreamWriter
            StreamWriter cmsSw = StreamWriter.New(sw);
            return cmsSw;
        }


        /// <summary>
        /// Deletes file.
        /// </summary>
        protected override void DeleteInternal()
        {
            systemFileInfo.Delete();

            StorageSynchronization.LogDeleteFileTask(FullName);
        }


        /// <summary>
        /// Opens file for reading.
        /// </summary> 
        protected override IO.FileStream OpenReadInternal()
        {
            System.IO.FileStream stream = systemFileInfo.OpenRead();

            // Create CMS stream for return
            FileStream cmsStream = new FileStream(stream);
            return cmsStream;
        }


        /// <summary>
        /// Creates stream which reads from file.
        /// </summary> 
        protected override StreamReader OpenTextInternal()
        {
            System.IO.StreamReader systemReader = systemFileInfo.OpenText();

            // Create CMS stream for return
            StreamReader reader = StreamReader.New(systemReader);
            return reader;
        }


        /// <summary>
        /// Copies an existing file to a new file, allowing the overwriting of an existing file.
        /// </summary>
        /// <param name="destFileName">Destination file name</param>
        /// <param name="overwrite">Whether overwriting is allowed</param>
        protected override IO.FileInfo CopyToInternal(string destFileName, bool overwrite)
        {
            destFileName = AbstractStorageProvider.GetTargetPhysicalPath(destFileName);
            System.IO.FileInfo fi = systemFileInfo.CopyTo(destFileName, overwrite);
            FileInfo info = new FileInfo(fi);
            return info;
        }


        /// <summary>
        /// Copies an existing file to a new file, allowing the overwriting of an existing file.
        /// </summary>
        /// <param name="destFileName">Destination file name</param>        
        protected override void MoveToInternal(string destFileName)
        {
            systemFileInfo.MoveTo(destFileName);
        }


        /// <summary>
        /// Converts FileInfo object to string.
        /// </summary>   
        public override string ToString()
        {
            return systemFileInfo.ToString();
        }

        #endregion
    }
}