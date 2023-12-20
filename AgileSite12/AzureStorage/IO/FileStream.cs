using System;
using System.IO;
using System.Collections.Generic;

using CMS.Core;
using CMS.Core.Internal;
using CMS.Helpers;
using CMS.IO;

using FileAccess = CMS.IO.FileAccess;
using FileMode = CMS.IO.FileMode;
using FileShare = CMS.IO.FileShare;

namespace CMS.AzureStorage
{
    /// <summary>
    /// Implementation of file stream for Microsoft Azure.
    /// </summary>
    public class FileStream : IO.FileStream, IMultiPartUploadStream
    {
        #region "Private variables"

        private readonly IDateTimeNowService mDateTimeNowService;

        private readonly string mPath;

        private readonly FileMode mFileMode = FileMode.Open;
        private readonly FileAccess mFileAccess = FileAccess.ReadWrite;
        private readonly FileShare mFileShare = FileShare.Read;
        private bool mIsMultiPartMode;

        private readonly int mBufferSize;

        // Represents file that exists on external storage and on local file system
        private System.IO.FileStream mTempFileStream;

        // Objects representing blob and container in the cloud
        private BlobInfo mBlob;

        // Represents file that exists only on local file system
        private System.IO.FileStream mFileStream;
        private BlobMultiPartUploader mBlobMultiPartUploader;

        private int mReadSize = -1;

        private bool disposed;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Gets a value indicating whether the current stream supports reading.
        /// </summary>
        public override bool CanRead
        {
            get
            {
                if (mFileStream != null)
                {
                    return mFileStream.CanRead;
                }

                return mTempFileStream.CanRead;
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
                if (mFileStream != null)
                {
                    return mFileStream.CanSeek;
                }

                return mTempFileStream.CanSeek;
            }
        }


        /// <summary>
        /// Gets a value indicating whether the current stream supports writing.
        /// </summary>
        public override bool CanWrite
        {
            get
            {
                if (mFileStream != null)
                {
                    return mFileStream.CanWrite;
                }

                return mTempFileStream.CanWrite;
            }
        }


        /// <summary>
        /// Length of stream.
        /// </summary>
        public override long Length
        {
            get
            {
                if (mFileStream != null)
                {
                    return mFileStream.Length;
                }

                return mTempFileStream.Length;
            }
        }


        /// <summary>
        /// Gets or sets position of current stream.
        /// </summary>
        public override long Position
        {
            get
            {
                if (mFileStream != null)
                {
                    return mFileStream.Position;
                }
                return mTempFileStream.Position;
            }
            set
            {
                if (mFileStream != null)
                {
                    mFileStream.Position = value;
                }
                else
                {
                    mTempFileStream.Position = value;
                }
            }
        }


        /// <summary>
        /// Returns minimal size of the part used in multipart upload process to Azure blob storage.
        /// </summary>
        public long MinimalPartSize => BlobInfoProvider.MINIMAL_PART_SIZE;


        /// <summary>
        /// Maximal size of the part used in multipart upload process to Azure blob storage.
        /// </summary>
        public long MaximalPartSize => BlobInfoProvider.MAXIMAL_PART_SIZE;


        /// <summary>
        /// Instance for uploading large files in smaller parts to Azure blob storage.
        /// </summary>
        internal BlobMultiPartUploader BlobMultiPartUploader
        {
            get
            {
                if (mBlobMultiPartUploader == null)
                {
                    mBlobMultiPartUploader = new BlobMultiPartUploader(mBlob, BlobInfoProvider.MAXIMAL_PART_SIZE);
                }

                return mBlobMultiPartUploader;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes new instance and initializes new system file stream.
        /// </summary>
        /// <param name="path">Path to file.</param>
        /// <param name="mode">File mode.</param>
        public FileStream(string path, FileMode mode)
            : this(path, mode, mode == FileMode.Append ? FileAccess.Write : FileAccess.ReadWrite)
        {
        }


        /// <summary>
        /// Initializes new instance and initializes new system file stream.
        /// </summary>
        /// <param name="path">Path to file.</param>
        /// <param name="mode">File mode.</param>
        /// <param name="access">File access.</param>
        public FileStream(string path, FileMode mode, FileAccess access)
            : this(path, mode, access, FileShare.Read)
        {
        }


        /// <summary>
        /// Initializes new instance and initializes new system file stream.
        /// </summary>
        /// <param name="path">Path to file.</param>
        /// <param name="mode">File mode.</param>
        /// <param name="access">File access.</param>       
        /// <param name="share">Sharing permissions.</param>
        public FileStream(string path, FileMode mode, FileAccess access, FileShare share)
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
        public FileStream(string path, FileMode mode, FileAccess access, FileShare share, int bSize)
            : this(path, mode, access, share, bSize, Service.Resolve<IDateTimeNowService>())
        {
        }


        internal FileStream(string path, FileMode mode, IDateTimeNowService dateTimeNowService)
            : this(path, mode, mode == FileMode.Append ? FileAccess.Write : FileAccess.ReadWrite, dateTimeNowService)
        {
        }


        internal FileStream(string path, FileMode mode, FileAccess access, IDateTimeNowService dateTimeNowService)
            : this(path, mode, access, FileShare.Read, 0x1000, dateTimeNowService)
        {
        }


        internal FileStream(string path, FileMode mode, FileAccess access, FileShare share, int bSize, IDateTimeNowService dateTimeNowService)
            : base(path)
        {
            mDateTimeNowService = dateTimeNowService;
            mPath = path;
            mFileMode = mode;
            mFileAccess = access;
            mFileShare = share;
            mBufferSize = bSize;

            InitFileStream();
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Reads data from stream and stores them into array.
        /// </summary>
        /// <param name="array">Array where result is stored.</param>
        /// <param name="offset">Offset from begining of file.</param>
        /// <param name="count">Number of characters which are read.</param>
        public override int Read(byte[] array, int offset, int count)
        {
            if (mReadSize == -1)
            {
                // Log the first read
                LogFileOperation(mPath, FileDebugOperation.READ, count);
                mReadSize = count;
            }
            else
            {
                // Append read size
                mReadSize += count;
            }

            if (mFileStream != null)
            {
                return mFileStream.Read(array, offset, count);
            }

            return mTempFileStream.Read(array, offset, count);
        }


        /// <summary>
        /// Closes current stream.
        /// </summary>
        public override void Close()
        {
            // Do action only for filesystem - in blob after every action is file closed
            if (mFileStream != null)
            {
                mFileStream.Close();
            }
            else
            {
                Dispose(true);
                mTempFileStream.Close();
            }

            if (mReadSize > 0)
            {
                // Log read end
                FileDebug.LogReadEnd(mReadSize);
                mReadSize = -1;
            }

            LogFileOperation(mPath, FileDebugOperation.CLOSE, -1);
        }


        /// <summary>
        /// Clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        public override void Flush()
        {
            // Do action for filesystem.
            if (mFileStream != null && mFileStream.CanWrite)
            {
                mFileStream.Flush();
            }
            else
            {
                // Put data back to the storage only if stream was open for writing
                if ((mFileAccess != FileAccess.Read || mFileMode != FileMode.Open) && (mTempFileStream != null && mTempFileStream.CanWrite))
                {
                    mTempFileStream.Flush();
                }
            }
        }


        /// <summary>
        /// Writes sequence of bytes to stream.
        /// </summary>
        /// <param name="buffer">Buffer.</param>
        /// <param name="offset">Offset.</param>
        /// <param name="count">Count.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (mFileStream != null)
            {
                mFileStream.Write(buffer, offset, count);
            }
            else
            {
                mTempFileStream.Write(buffer, offset, count);
            }

            LogFileOperation(mPath, FileDebugOperation.WRITE, count);
        }


        /// <summary>
        /// Sets the position within the current stream to the specified value.
        /// </summary>
        /// <param name="offset">Offset</param>
        /// <param name="loc">Location</param>
        public override long Seek(long offset, SeekOrigin loc)
        {
            if (mFileStream != null)
            {
                return mFileStream.Seek(offset, loc);
            }

            return mTempFileStream.Seek(offset, loc);
        }


        /// <summary>
        /// Set length to stream.
        /// </summary>
        /// <param name="value">Value to set.</param>
        public override void SetLength(long value)
        {
            mTempFileStream.SetLength(value);
        }


        /// <summary>
        /// Writes byte to the stream.
        /// </summary>
        /// <param name="value">Value to write.</param>
        public override void WriteByte(byte value)
        {
            if (mFileStream != null)
            {
                mFileStream.WriteByte(value);
            }

            mTempFileStream.WriteByte(value);

            LogFileOperation(mPath, FileDebugOperation.WRITE, 1);
        }

        #endregion


        #region IDisposable Members

        /// <summary>
        /// Releases all unmanaged and optionally managed resources.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                if (mFileStream != null)
                {
                    mFileStream.Dispose();
                }
                else
                {
                    try
                    {
                        Flush();

                        // Put data back to the storage only if stream was open for writing
                        if ((mFileAccess != FileAccess.Read) && (mFileMode != FileMode.Open) && mTempFileStream.CanWrite)
                        {
                            if (!mIsMultiPartMode)
                            {
                                mTempFileStream.Seek(0, SeekOrigin.Begin);
                                BlobInfoProvider.PutDataFromStreamToBlob(mBlob, mTempFileStream);
                                SetLastWriteTimeAndCreationTimeToBlob();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        EventLog.EventLogProvider.LogException(IOProviderName.Azure, "DISPOSESTREAM", e);
                    }
                    finally
                    {
                        mTempFileStream.Dispose();

                        if (mReadSize > 0)
                        {
                            // Log read end
                            FileDebug.LogReadEnd(mReadSize);
                            mReadSize = -1;
                        }

                        LogFileOperation(mPath, FileDebugOperation.CLOSE, -1);
                    }
                }
            }

            disposed = true;

            base.Dispose(disposing);
        }

        #endregion


        #region "IMultiPartStream"
        
        /// <summary>
        /// Inits multipart upload to Azure blob storage.
        /// </summary>
        /// <returns>
        /// Upload ID, unique identifier for one multipart upload to Azure blob storage.
        /// Returned upload ID is needed for each subsequent multipart upload operation.
        /// </returns>
        public string InitMultiPartUpload()
        {
            mIsMultiPartMode = true;

            return BlobMultiPartUploader.InitMultiPartUpload();
        }


        /// <summary>
        /// Uploads data inside a stream in multiple parts to Azure blob storage.
        /// </summary>
        /// <param name="uploadSessionId">Unique identifier for one multipart upload. Can be obtained by <see cref="InitMultiPartUpload()"/> method.</param>
        /// <param name="nextPartNumber">Number that defines position of the data obtained by the stream in the whole multipart upload process.</param>
        /// <returns>Unique identifiers of the uploaded parts.</returns>
        public IEnumerable<string> UploadStreamContentAsMultiPart(string uploadSessionId, int nextPartNumber)
        {
            mIsMultiPartMode = true;

            if (mBlob.IsLocked)
            {
                throw new Exception("Couldn't upload content of the blob '" + mBlob.BlobName + "' because it is used by another process.");
            }

            mBlob.Lock();
            var blockIds = BlobMultiPartUploader.MultiPartUploadFromStream(uploadSessionId, nextPartNumber, this);
            mBlob.UnLock();

            BlobInfoProvider.RemoveRequestCache(mBlob);

            return blockIds;
        }


        /// <summary>
        /// Completes multiple part upload process.
        /// Sends final request to Azure blob storage to merge all parts already sent.
        /// </summary>
        /// <param name="uploadSessionId">Unique identifier for one multipart upload. Can be obtained by <see cref="InitMultiPartUpload()"/> method.</param>
        /// <param name="partIdentifiers">Identifiers of the parts already sent to Azure blob storage, obtained by calling <see cref="UploadStreamContentAsMultiPart(string, int)"/></param>
        /// <returns>
        /// ETag of the uploaded file.
        /// </returns>
        public string CompleteMultiPartUploadProcess(string uploadSessionId, IEnumerable<string> partIdentifiers)
        {
            if (mBlob.IsLocked)
            {
                throw new Exception("Couldn't complete upload of the blob '" + mBlob.BlobName + "' because it is used by another process.");
            }

            mBlob.Lock();
            var eTag = BlobMultiPartUploader.CompleteMultiPartUploadProcess(uploadSessionId, partIdentifiers);
            mBlob.UnLock();

            SetLastWriteTimeAndCreationTimeToBlob();
            BlobInfoProvider.RemoveRequestCache(mBlob);

            return eTag;
        }


        /// <summary>
        /// Aborts multipart upload, so Azure blob storage can delete uploaded parts.
        /// </summary>        
        ///  <param name="uploadSessionId">Unique identifier for one multipart upload. Can be obtained by <see cref="InitMultiPartUpload()"/> method.</param>
        public void AbortMultiPartUpload(string uploadSessionId)
        {
            // Parts already uploaded to azure storage are deleted automatically after 7 days
            BlobInfoProvider.RemoveRequestCache(mBlob);
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Initializes the stream
        /// </summary>
        protected void InitFileStream()
        {
            // Create temp directory for file
            string tempPath = IO.Path.Combine(PathHelper.TempPath, IO.Path.EnsureBackslashes(Directory.GetBlobPathFromPath(mPath)));

            Directory.CreateDirectoryStructure(tempPath);

            mBlob = new BlobInfo(mPath);

            bool blobExists = BlobInfoProvider.BlobExists(mBlob);

            // Open from temp/cache, if new file is created don't load data from existing file
            if ((mFileMode != FileMode.Create) && blobExists)
            {
                // GetBlobContent internally works with temporary file stream
                mTempFileStream = (System.IO.FileStream)BlobInfoProvider.GetBlobContentWithOptions(mBlob);

                // If append set position correctly to end of stream
                if ((System.IO.FileMode)mFileMode == System.IO.FileMode.Append)
                {
                    mTempFileStream.Position = mTempFileStream.Length;
                }
            }
            else if ((mFileMode == FileMode.CreateNew) && blobExists)
            {
                throw new Exception("Cannot create a new file, the file is already exist.");
            }
            else if (System.IO.File.Exists(mPath))
            {
                // If the file does not exist on Azure Blob storage but exists in local file system, try to work with the file in local file system.
                // This happens when the file was created in local file system and then file system was mapped to Azure Blob storage.
                // Working with file like that can cause exceptions and unpredictable behavior and file should be deleted from local file system
                // and recreated on external storage.
                mFileStream = new System.IO.FileStream(mPath, (System.IO.FileMode)mFileMode, (System.IO.FileAccess)mFileAccess, (System.IO.FileShare)mFileShare, mBufferSize);
            }

            // File doesn't exist in blob or in file system - create new temp file
            if ((mTempFileStream == null) && (mFileStream == null))
            {
                try
                {
                    // File doesn't exist so new temp file must be created (if some temp old exist it must be deleted) and access must be for read and write because of uploading data to blob
                    mTempFileStream = new System.IO.FileStream(tempPath, System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite, (System.IO.FileShare)mFileShare, mBufferSize);
                }
                catch (FileNotFoundException)
                {
                    // In case that file exist in file system and it is open for reading
                }
            }
        }


        /// <summary>
        /// Sets last write time and creation time to blob.
        /// </summary>
        private void SetLastWriteTimeAndCreationTimeToBlob()
        {
            // Set date times
            var nowTime = mDateTimeNowService.GetDateTimeNow();
            string now = ValidationHelper.GetString(nowTime, "");
            if (mBlob.GetMetadata(ContainerInfoProvider.CREATION_TIME) == null)
            {
                mBlob.SetMetadata(ContainerInfoProvider.CREATION_TIME, now, false);
            }

            mBlob.SetMetadata(ContainerInfoProvider.LAST_WRITE_TIME, now);
        }

        #endregion
    }
}
