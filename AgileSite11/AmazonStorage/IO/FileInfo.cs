using System;

using CMS.Helpers;
using CMS.IO;

namespace CMS.AmazonStorage
{
    /// <summary>
    /// Implementation of FileInfo class for Amazon S3 object.
    /// </summary>
    public class FileInfo : IO.FileInfo
    {
        #region "Variables"

        string mExtension = string.Empty;
        string mFullName = string.Empty;
        string mName = string.Empty;
        bool mExists = false;
        DateTime mLastWriteTime = DateTimeHelper.ZERO_TIME;
        DateTime mCreationTime = DateTimeHelper.ZERO_TIME;
        FileAttributes mAttributes;
        DirectoryInfo mDirectory = null;
        private long mLength = 0;

        IS3ObjectInfo obj;

        bool existsInS3Storage = false;

        #endregion


        #region "Private properties"

        /// <summary>
        /// Returns S3Object provider.
        /// </summary>
        private IS3ObjectInfoProvider Provider
        {
            get
            {
                return S3ObjectFactory.Provider;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes new instance of FileInfo class.
        /// </summary>
        /// <param name="filename">File name.</param>
        public FileInfo(string filename)
        {
            mExtension = Path.GetExtension(filename);
            mFullName = filename;
            mName = Path.GetFileName(filename);
            mExists = IO.File.Exists(filename);
            IsReadOnly = false;
            Attributes = FileAttributes.Normal;


            // Must initialize these objects because of new files
            obj = S3ObjectFactory.GetInfo(filename);

            if (!Provider.ObjectExists(obj))
            {
                if (System.IO.File.Exists(filename))
                {
                    mSystemInfo = new System.IO.FileInfo(filename);
                }
            }
            else
            {
                mExists = true;
                existsInS3Storage = true;
            }

            InitCMSValues();
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
                return mLength;
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
        /// Last write time to file.
        /// </summary>
        public override DateTime LastWriteTime
        {
            get
            {
                return mLastWriteTime;
            }
            set
            {
                mLastWriteTime = value;
                if (existsInS3Storage)
                {
                    obj.SetMetadata(S3ObjectInfoProvider.LAST_WRITE_TIME, S3ObjectInfoProvider.GetDateTimeString(mLastWriteTime));
                }
            }
        }


        /// <summary>
        /// If file exists.
        /// </summary>
        public override bool Exists
        {
            get
            {
                return mExists;
            }
        }


        /// <summary>
        /// Creation date of file.
        /// </summary>
        public override DateTime CreationTime
        {
            get
            {
                return mCreationTime;
            }
            set
            {
                mCreationTime = value;
                if (existsInS3Storage)
                {
                    obj.SetMetadata(S3ObjectInfoProvider.CREATION_TIME, S3ObjectInfoProvider.GetDateTimeString(mCreationTime));
                }
            }
        }


        /// <summary>
        /// Directory of file.
        /// </summary>
        public override IO.DirectoryInfo Directory
        {
            get
            {
                return mDirectory;
            }
        }


        /// <summary>
        ///  If is read only.
        /// </summary>
        public override bool IsReadOnly
        {
            get;
            set;
        }


        /// <summary>
        /// File attributes.
        /// </summary>
        public override FileAttributes Attributes
        {
            get
            {
                return mAttributes;
            }
            set
            {
                mAttributes = value;
                if (existsInS3Storage)
                {
                    obj.SetMetadata(S3ObjectInfoProvider.ATTRIBUTES, ValidationHelper.GetString(ValidationHelper.GetInteger(mAttributes, 0), string.Empty));
                }
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
        /// Last access time.
        /// </summary>
        public override DateTime LastAccessTime
        {
            get
            {
                return S3ObjectInfoProvider.GetStringDateTime(obj.GetMetadata(S3ObjectInfoProvider.LAST_WRITE_TIME));
            }
            set
            {
                if (existsInS3Storage)
                {
                    obj.SetMetadata(S3ObjectInfoProvider.LAST_WRITE_TIME, S3ObjectInfoProvider.GetDateTimeString(value));
                }
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Creates or opens a file for writing UTF-8 encoded text.
        /// </summary> 
        protected override StreamWriter CreateTextInternal()
        {
            //Provider.CreateEmptyObject(obj);
            mExists = true;
            existsInS3Storage = true;

            // Create new file stream
            IO.FileStream fs = IO.FileStream.New(FullName, FileMode.Create);
            StreamWriter sw = StreamWriter.New(fs);

            return sw;
        }


        /// <summary>
        /// Deletes file.
        /// </summary>
        protected override void DeleteInternal()
        {
            Provider.DeleteObject(obj);
            mExists = false;
            existsInS3Storage = false;
        }


        /// <summary>
        /// Creates a read-only ICMSFileStream.
        /// </summary> 
        protected override IO.FileStream OpenReadInternal()
        {
            FileStream stream = new FileStream(FullName, FileMode.Open, FileAccess.Read);
            return stream;
        }


        /// <summary>
        /// Copies current file to destination. 
        /// </summary>
        /// <param name="destFileName">Destination file name.</param>
        /// <param name="overwrite">Specifies whether destination file is overwritten.</param>        
        protected override IO.FileInfo CopyToInternal(string destFileName, bool overwrite)
        {
            IO.File.Copy(FullName, destFileName, overwrite);

            var info = New(destFileName);
            return info;
        }


        /// <summary>
        /// Moves an existing file to a new file.
        /// </summary>
        /// <param name="destFileName">Destination file name.</param>        
        protected override void MoveToInternal(string destFileName)
        {
            IO.File.Move(FullName, destFileName);
        }


        /// <summary>
        /// Creates a StreamReader with UTF8 encoding that reads from an existing text file.
        /// </summary>   
        protected override StreamReader OpenTextInternal()
        {
            return IO.File.OpenText(FullName);
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Sets values from System.IO.FileInfo to this file info
        /// </summary>
        private void InitCMSValues()
        {
            // File system
            if (mSystemInfo != null)
            {
                mExtension = mSystemInfo.Extension;
                mFullName = mSystemInfo.FullName;
                mName = mSystemInfo.Name;
                mExists = mSystemInfo.Exists;
                LastWriteTime = mSystemInfo.LastWriteTime;
                CreationTime = mSystemInfo.CreationTime;
                IsReadOnly = mSystemInfo.IsReadOnly;
                Attributes = (FileAttributes)mSystemInfo.Attributes;

                // Get length of file only if file exist else exception is thrown
                if (mExists)
                {
                    mLength = mSystemInfo.Length;
                }

                // Set directory fields
                if (mDirectory == null)
                {
                    mDirectory = new DirectoryInfo(mSystemInfo.Directory.FullName);
                }

                mDirectory.CreationTime = mSystemInfo.Directory.CreationTime;
                mDirectory.Exists = mSystemInfo.Directory.Exists;
                mDirectory.FullName = mSystemInfo.Directory.FullName;
                mDirectory.LastWriteTime = mSystemInfo.Directory.LastWriteTime;
                mDirectory.Name = mSystemInfo.Directory.Name;

            }
            // S3 storage
            else
            {
                if (mExists)
                {
                    mLastWriteTime = S3ObjectInfoProvider.GetStringDateTime(obj.GetMetadata(S3ObjectInfoProvider.LAST_WRITE_TIME));
                    mCreationTime = S3ObjectInfoProvider.GetStringDateTime(obj.GetMetadata(S3ObjectInfoProvider.CREATION_TIME));
                    mAttributes = (FileAttributes)ValidationHelper.GetInteger(obj.GetMetadata(S3ObjectInfoProvider.CREATION_TIME), ValidationHelper.GetInteger(FileAttributes.Normal, 0));
                    mLength = obj.Length;
                }
                else
                {
                    LastWriteTime = DateTimeHelper.ZERO_TIME;
                    CreationTime = DateTimeHelper.ZERO_TIME;
                    mAttributes = FileAttributes.Normal;
                }

                mDirectory = new DirectoryInfo(Path.GetDirectoryName(FullName));
            }
        }


        /// <summary>
        /// Converts current info to string.
        /// </summary>        
        public override string ToString()
        {
            return FullName;
        }

        #endregion
    }
}
