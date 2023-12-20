using System;

using CMS.Helpers;
using CMS.IO;

using Microsoft.WindowsAzure.Storage;

namespace CMS.AzureStorage
{
    /// <summary>
    /// Envelope for FileInfo classes (System.IO or Azure)
    /// </summary>
    public class FileInfo : IO.FileInfo
    {
        #region "Variables"

        private long mLength;
        private string mExtension;
        private string mFullName;
        private string mName;
        private DateTime mLastWriteTime = DateTimeHelper.ZERO_TIME;
        private bool mExists;
        private DateTime mCreationTime = DateTimeHelper.ZERO_TIME;
        private DirectoryInfo mDirectory;
        private FileAttributes mAttributes;

        private readonly BlobInfo mBlob;

        private bool mExistsInBlobStorage;

        #endregion


        #region "Properties"

        /// <summary>
        /// Length of file.
        /// </summary>
        public override long Length => mLength;


        /// <summary>
        /// File extension.
        /// </summary>
        public override string Extension => mExtension;


        /// <summary>
        /// Full name of file (with whole path).
        /// </summary>
        public override string FullName => mFullName;


        /// <summary>
        /// File name of file (without path).
        /// </summary>
        public override string Name => mName;


        /// <summary>
        /// If file exists.
        /// </summary>
        public override bool Exists => mExists;


        /// <summary>
        /// Directory of file.
        /// </summary>
        public override IO.DirectoryInfo Directory => mDirectory;


        /// <summary>
        /// Directory name.
        /// </summary>
        public override string DirectoryName => Directory.Name;


        /// <summary>
        ///  If is read only.
        /// </summary>
        public override bool IsReadOnly
        {
            get;
            set;
        }


        /// <summary>
        /// Last write time to file.
        /// </summary>
        public override DateTime LastWriteTime
        {
            get
            {
                // Show last modified in UTC if this information is not available from metadata
                if (mLastWriteTime == DateTimeHelper.ZERO_TIME && mExistsInBlobStorage && mBlob.Blob != null)
                {
                    return mBlob.Blob.Properties.LastModified.GetValueOrDefault(DateTimeOffset.UtcNow).UtcDateTime;
                }

                return mLastWriteTime;
            }
            set
            {
                mLastWriteTime = value;
                if (mExistsInBlobStorage)
                {
                    mBlob.SetMetadata(ContainerInfoProvider.LAST_WRITE_TIME, ValidationHelper.GetString(mLastWriteTime, String.Empty));
                }
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
                if (mExistsInBlobStorage)
                {
                    mBlob.SetMetadata(ContainerInfoProvider.CREATION_TIME, ValidationHelper.GetString(mCreationTime, ""));
                }
            }
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
                if (mExistsInBlobStorage)
                {
                    mBlob.SetMetadata(ContainerInfoProvider.ATTRIBUTES, ValidationHelper.GetString(ValidationHelper.GetInteger(mAttributes, 0), ""));
                }
            }
        }


        /// <summary>
        /// Last access time.
        /// </summary>
        public override DateTime LastAccessTime
        {
            get
            {
                return ValidationHelper.GetDateTime(mBlob.GetMetadata(ContainerInfoProvider.LAST_WRITE_TIME), DateTimeHelper.ZERO_TIME);
            }
            set
            {
                if (mExistsInBlobStorage)
                {
                    mBlob.SetMetadata(ContainerInfoProvider.LAST_WRITE_TIME, ValidationHelper.GetString(value, ""));
                }
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes new instance of
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
            var container = ContainerInfoProvider.GetRootContainerInfo(filename);
            mBlob = new BlobInfo(container, filename);

            if (!BlobInfoProvider.BlobExists(mBlob))
            {
                if (System.IO.File.Exists(filename))
                {
                    mSystemInfo = new System.IO.FileInfo(filename);
                }
            }
            else
            {
                mExists = true;
                mExistsInBlobStorage = true;
            }

            InitCMSValues();
        }

        #endregion

        #region "Methods"

        /// <summary>
        /// Creates or opens a file for writing UTF-8 encoded text.
        /// </summary>
        protected override StreamWriter CreateTextInternal()
        {
            BlobInfoProvider.CreateEmptyBlob(mBlob);
            mExists = true;
            mExistsInBlobStorage = true;

            // Create new file stream
            IO.FileStream fs = FileStream.New(FullName, FileMode.Create);
            StreamWriter sw = StreamWriter.New(fs);

            return sw;
        }


        /// <summary>
        /// Deletes file.
        /// </summary>
        protected override void DeleteInternal()
        {
            try
            {
                BlobInfoProvider.DeleteBlob(mBlob);
            }
            catch (StorageException)
            {
                // Skip files that cannot be found.
            }

            mExists = false;
            mExistsInBlobStorage = false;
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
        /// <param name="overwrite">Indicates if existing file should be overwritten</param>
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

            // Blob storage
            else
            {
                if (mExists)
                {
                    mLastWriteTime = ValidationHelper.GetDateTime(mBlob.GetMetadata(ContainerInfoProvider.LAST_WRITE_TIME), DateTimeHelper.ZERO_TIME);
                    mCreationTime = ValidationHelper.GetDateTime(mBlob.GetMetadata(ContainerInfoProvider.CREATION_TIME), DateTimeHelper.ZERO_TIME);
                    mAttributes = (FileAttributes)ValidationHelper.GetInteger(mBlob.GetMetadata(ContainerInfoProvider.CREATION_TIME), ValidationHelper.GetInteger(FileAttributes.Normal, 0));
                    mLength = mBlob.Length;
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