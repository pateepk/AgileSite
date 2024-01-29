using System;
using System.IO;
using System.Collections.Generic;

using CMS.IO;
using CMS.EventLog;

using FileAccess = CMS.IO.FileAccess;
using FileMode = CMS.IO.FileMode;
using FileShare = CMS.IO.FileShare;

using Amazon.S3.Model;

namespace CMS.AmazonStorage
{
    /// <summary>
    /// Implementation of FileStream class for Amazon simple storage.
    /// </summary>
    public class FileStream : IO.FileStream, IMultiPartUploadStream
    {
        #region "Private variables"

        private string mPath;
        private FileMode fileMode = FileMode.Open;
        private FileAccess fileAccess = FileAccess.ReadWrite;
        private FileShare fileShare = FileShare.Read;
        private int bufferSize = 0;
        private bool mMultiPartUploadMode = false;

        // Represents file that exists on external storage and on local file system
        private System.IO.FileStream fsTemp;

        // Objects representing blob and container in the cloud
        private IS3ObjectInfo obj = null;
        private S3MultiPartUploader mMultiPartUploader;

        // Represents file that exists only on local file system
        private System.IO.FileStream fsStream;

        private int mReadSize = -1;

        private bool disposed;

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


        /// <summary>
        /// Instance for uploading large files in smaller parts to Amazon S3 storage.
        /// </summary>
        private S3MultiPartUploader MultiPartUploader
        {
            get
            {
                if (mMultiPartUploader == null)
                {
                    mMultiPartUploader = new S3MultiPartUploader(AccountInfo.Current.S3Client, MinimalPartSize, MaximalPartSize);
                }

                return mMultiPartUploader;
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
            : base(path)
        {
            mPath = path;
            fileMode = mode;
            fileAccess = access;
            fileShare = share;
            bufferSize = bSize;

            InitFileStream();
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
                if (fsStream != null)
                {
                    return fsStream.CanRead;
                }
                else
                {
                    return fsTemp.CanRead;
                }
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
                if (fsStream != null)
                {
                    return fsStream.CanSeek;
                }
                else
                {
                    return fsTemp.CanSeek;
                }
            }
        }


        /// <summary>
        /// Gets a value indicating whether the current stream supports writing.
        /// </summary>
        public override bool CanWrite
        {
            get
            {
                if (fsStream != null)
                {
                    return fsStream.CanWrite;
                }
                else
                {
                    return fsTemp.CanWrite;
                }
            }
        }


        /// <summary>
        /// Length of stream.
        /// </summary>
        public override long Length
        {
            get
            {
                if (fsStream != null)
                {
                    return fsStream.Length;
                }
                else
                {
                    return fsTemp.Length;
                }
            }
        }


        /// <summary>
        /// Gets or sets position of current stream.
        /// </summary>
        public override long Position
        {
            get
            {
                if (fsStream != null)
                {
                    return fsStream.Position;
                }
                else
                {
                    return fsTemp.Position;
                }
            }
            set
            {
                if (fsStream != null)
                {
                    fsStream.Position = value;
                }
                else
                {
                    fsTemp.Position = value;
                }
            }
        }


        /// <summary>
        /// Returns minimal size of the part used in multipart upload process to Amazon S3 storage.
        /// </summary>
        public long MinimalPartSize
        {
            get
            {
                return S3ObjectInfoProvider.MINIMAL_PART_SIZE;
            }
        }


        /// <summary>
        /// Maximal size of the part used in multipart upload process to Amazon S3 storage.
        /// </summary>
        public long MaximalPartSize
        {
            get
            {
                return S3ObjectInfoProvider.MAXIMAL_PART_SIZE;
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Reads data from stream and stores them into array.
        /// </summary>
        /// <param name="array">Array where result is stored.</param>
        /// <param name="offset">Offset from file begin.</param>
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

            if (fsStream != null)
            {
                return fsStream.Read(array, offset, count);
            }
            else
            {
                return fsTemp.Read(array, offset, count);
            }
        }


        /// <summary>
        /// Closes current stream.
        /// </summary>
        public override void Close()
        {
            // Do action only for filesystem - in S3 storage is file closed after every action.
            if (fsStream != null)
            {
                fsStream.Close();
            }
            else
            {
                Dispose(true);
                fsTemp.Close();
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
            if (fsStream != null && fsStream.CanWrite)
            {
                fsStream.Flush();
            }
            // Put data back to the storage only if stream was open for writing
            else if ((fsTemp != null && fsTemp.CanWrite) && (fileAccess != FileAccess.Read || fileMode != FileMode.Open))
            {
                fsTemp.Flush();
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
            if (fsStream != null)
            {
                throw new Exception(@"Cannot write into file because it exists only in application file system. 
                    This exception typically occurs when file system is mapped to Amazon S3 storage after the file or directory
                    '" + mPath + "' was created in the local file system. To fix this issue move given file to Amazon S3 storage.");
            }
            else
            {
                fsTemp.Write(buffer, offset, count);
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
            if (fsStream != null)
            {
                return fsStream.Seek(offset, loc);
            }
            else
            {
                return fsTemp.Seek(offset, loc);
            }
        }


        /// <summary>
        /// Set length to stream.
        /// </summary>
        /// <param name="value">Value to set.</param>
        public override void SetLength(long value)
        {
            fsTemp.SetLength(value);
        }


        /// <summary>
        /// Writes byte to the stream.
        /// </summary>
        /// <param name="value">Value to write.</param>
        public override void WriteByte(byte value)
        {
            if (fsStream != null)
            {
                throw new Exception(@"Cannot write into file because it exists only in application file system. 
                    This exception typically occurs when file system is mapped to Amazon S3 storage after the file or directory
                    '" + mPath + "' was created in the local file system. To fix this issue move given file to Amazon S3 storage.");
            }
            else
            {
                fsTemp.WriteByte(value);
            }

            LogFileOperation(mPath, FileDebugOperation.WRITE, 1);
        }

        #endregion


        #region "IMultiPartUploadStream"

        /// <summary>
        /// Inits multipart upload for given path.
        /// </summary>
        /// <returns>
        /// Upload ID, unique identifier for one multipart upload to Amazon S3 storage.
        /// Returned upload ID is needed for each subsequent multipart upload operation.
        /// </returns>
        public string InitMultiPartUpload()
        {
            mMultiPartUploadMode = true;
            return MultiPartUploader.InitMultiPartUpload(obj.Key, obj.GetBucketName());
        }


        /// <summary>
        /// Uploads stream's content to Amazon S3 storage as one part of the file in multipart upload process
        /// identified by <paramref name="uploadSessionId"/>.
        /// </summary>
        /// <remarks>
        /// Always returns one ETag in collection. If stream's length is more than 5GB then exception is thrown.
        /// </remarks>
        /// <param name="uploadSessionId">Unique identifier for one multipart upload. Can be obtained by <see cref="InitMultiPartUpload"/>.</param>
        /// <param name="nextPartNumber">Number that defines position of the data obtained by the stream in the whole multipart upload process.</param>
        /// <returns>One unique identifier of the uploaded part in collection.</returns>
        public IEnumerable<string> UploadStreamContentAsMultiPart(string uploadSessionId, int nextPartNumber)
        {
            if (Length > MaximalPartSize)
            {
                throw new Exception("Maximal size of part for upload to Amazon S3 storage is " + MaximalPartSize + " current stream has length " + Length + ".");
            }

            mMultiPartUploadMode = true;

            if (obj.IsLocked)
            {
                throw new Exception("Couldn't upload part of the object " + obj.Key + " because it is used by another process.");
            }

            obj.Lock();
            
            obj.Length += Length;
            var result = new List<string>() { MultiPartUploader.UploadPartFromStream(uploadSessionId, obj.Key, obj.GetBucketName(), nextPartNumber, this) };

            obj.UnLock();

            S3ObjectInfoProvider.RemoveRequestCache(obj.Key);

            return result;
        }


        /// <summary>
        /// Uploads one large file to Amazon S3 storage in smaller parts.
        /// </summary>
        /// <remarks>Stream still needs to be disposed.</remarks>
        /// <param name="uploadSessionId">Unique identifier for one multipart upload. Can be obtained by <see cref="InitMultiPartUpload"/> method.</param>
        /// <param name="partIdentifiers">List of identifiers from Amazon S3 received after uploading each part by <see cref="UploadStreamContentAsMultiPart(string, int)"/> method.</param>
        /// <returns>ETag of the uploaded file.</returns>
        public string CompleteMultiPartUploadProcess(string uploadSessionId, IEnumerable<string> partIdentifiers)
        {
            mMultiPartUploadMode = true;
            List<UploadPartResponse> partResponses = new List<UploadPartResponse>();
            int partNumber = 0;
            
            foreach (var partIdentifier in partIdentifiers)
            {
                partResponses.Add(new UploadPartResponse
                {
                    PartNumber = ++partNumber,
                    ETag = partIdentifier,
                });
            }

            if (obj.IsLocked)
            {
                throw new Exception("Couldn't upload part of the object " + obj.Key + " because it is used by another process.");
            }

            obj.Lock();

            obj.ETag = MultiPartUploader.CompleteMultiPartUploadProcess(obj.Key, obj.GetBucketName(), uploadSessionId, partResponses).ETag;

            obj.UnLock();

            SetLastWriteTimeAndCreationTimeToS3Object();
            S3ObjectInfoProvider.RemoveRequestCache(obj.Key);

            return obj.ETag;
        }


        /// <summary>
        /// Aborts multipart upload to Amazon S3 storage and removes all resources already uploaded.
        /// </summary>
        /// <param name="uploadSessionId">
        /// Unique identifier for multipart upload process to external storage.
        /// Is obtained by <see cref="InitMultiPartUpload()"/>.
        /// </param>
        public void AbortMultiPartUpload(string uploadSessionId)
        {
            S3ObjectInfoProvider.RemoveRequestCache(obj.Key);
            MultiPartUploader.AbortMultiPartUpload(obj.Key, obj.GetBucketName(), uploadSessionId);
        }


        #endregion


        #region "IDisposable Members"

        /// <summary>
        /// Releases all unmanaged and optionally managed resources.
        /// </summary>
        /// <param name="disposing">When true, managed resources are released.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                if (fsStream != null)
                {
                    fsStream.Dispose();
                }
                else
                {
                    try
                    {
                        Flush();

                        // Put data back to the storage only if stream was open for writing
                        if ((fileAccess != FileAccess.Read) && (fileMode != FileMode.Open) && (fsTemp.CanWrite))
                        {
                            // If multipart upload has started to the external storage via IMultiPartStream interface
                            // do not upload any data to Amazon S3 storage
                            if (!mMultiPartUploadMode)
                            {
                                fsTemp.Seek(0, SeekOrigin.Begin);
                                Provider.PutDataFromStreamToObject(obj, fsTemp);
                                SetLastWriteTimeAndCreationTimeToS3Object();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        EventLogProvider.LogException(IOProviderName.Amazon, "STREAMDISPOSE", e);
                    }
                    finally
                    {
                        fsTemp.Dispose();

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


        #region "Private methods"

        /// <summary>
        /// Initializes file stream object.
        /// </summary>
        protected virtual void InitFileStream()
        {
            // Create temp directory for file
            string tempPath = IO.Path.Combine(PathHelper.TempPath, PathHelper.GetRelativePath(mPath));

            Directory.CreateDiskDirectoryStructure(tempPath);

            obj = S3ObjectFactory.GetInfo(mPath);

            // Open from file system
            if (Provider.ObjectExists(obj))
            {
                if ((fileMode == FileMode.CreateNew)) 
                {
                    throw new Exception("Cannot create a new file, the file is already exist.");
                }

                // GetObjectContent internally works with temporary file stream
                fsTemp = (System.IO.FileStream)Provider.GetObjectContent(obj, (System.IO.FileMode)fileMode, (System.IO.FileAccess)fileAccess, (System.IO.FileShare)fileShare, bufferSize);

                // If append set position correctly to end of stream
                if ((System.IO.FileMode)fileMode == System.IO.FileMode.Append)
                {
                    fsTemp.Position = fsTemp.Length;
                }
            }
            else if (System.IO.File.Exists(mPath))
            {
                // If the file does not exist on S3 storage but exists in local file system, try to work with the file in local file system.
                // This happens when the file was created in local file system and then file system was mapped to S3 storage.
                // Working with file like that can cause exceptions and unpredictable behavior and file should be deleted from local file system
                // and recreated on external storage.
                fsStream = new System.IO.FileStream(mPath, (System.IO.FileMode)fileMode, (System.IO.FileAccess)fileAccess, (System.IO.FileShare)fileShare, bufferSize);
            }

            // File doesn't exist in S3 or in file system - create new temp file
            if ((fsTemp == null) && (fsStream == null))
            {
                try
                {
                    // File doesn't exist so new temp file must be created (if some temp old exist it must be deleted) and access must be for read and write because of uploading data to S3
                    fsTemp = new System.IO.FileStream(tempPath, System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite, (System.IO.FileShare)fileShare, bufferSize);
                }
                catch (FileNotFoundException)
                {
                    // In case that file exist in file system and it is open for reading
                }
            }
        }
        

        /// <summary>
        /// Sets last write time and creation time to S3 object.
        /// </summary>
        private void SetLastWriteTimeAndCreationTimeToS3Object()
        {
            string now = S3ObjectInfoProvider.GetDateTimeString(DateTime.Now);
            if (obj.GetMetadata(S3ObjectInfoProvider.CREATION_TIME) == null)
            {
                obj.SetMetadata(S3ObjectInfoProvider.CREATION_TIME, now, false);
            }

            obj.SetMetadata(S3ObjectInfoProvider.LAST_WRITE_TIME, now);
        }

        #endregion
    }
}
