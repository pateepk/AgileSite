using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Runtime;

using CMS.Helpers;
using CMS.IO;
using CMS.Base;

using FileAccess = System.IO.FileAccess;
using FileMode = System.IO.FileMode;
using FileShare = System.IO.FileShare;
using MemoryStream = System.IO.MemoryStream;
using Path = CMS.IO.Path;
using Stream = System.IO.Stream;
using StreamReader = CMS.IO.StreamReader;

namespace CMS.AmazonStorage
{
    /// <summary>
    /// Performs operations over the S3 objects.
    /// </summary>
    public class S3ObjectInfoProvider : IS3ObjectInfoProvider
    {
        #region "Constants"

        /// <summary>
        /// Property name which returns if file is locked for writing.
        /// </summary>
        public const string LOCK = "Lock";

        /// <summary>
        /// Last write time field name in storage metadata.
        /// </summary>
        public const string LAST_WRITE_TIME = "LastWriteTime";

        /// <summary>
        /// Creation time field name in storage metadata.
        /// </summary>
        public const string CREATION_TIME = "CreationTime";

        /// <summary>
        /// Attributes field name in storage metadata.
        /// </summary>
        public const string ATTRIBUTES = "Attributes";

        /// <summary>
        /// Storage key for Amazon storage directory objects.
        /// </summary>
        private const string STORAGE_KEY = "AmazonStorage|GetObjectList|";

        /// <summary>
        /// Indicates maximum number of items processed by one request.
        /// </summary>
        public const int MAX_OBJECTS_PER_REQUEST = 1000;
        
        /// <summary>
        /// Recommended minimal size (15 MB) of the file in Bytes for which multipart upload to Amazon S3 storage should be used.
        /// </summary>
        private const long RECOMMENDED_SIZE_FOR_MULTIPART_UPLOAD = 15 * 1024 * 1024;

        /// <summary>
        /// Minimal size (5 MB) of the parts used for multipart upload.
        /// </summary>
        internal const long MINIMAL_PART_SIZE = 5 * 1024 * 1024;

        /// <summary>
        /// Maximal size (5 GB) of the parts used for multipart upload.
        /// </summary>
        internal const long MAXIMAL_PART_SIZE = (long)5000 * 1024 * 1024;

        #endregion


        #region "Variables"

        static IS3ObjectInfoProvider mCurrent = null;
        private S3MultiPartUploader mMultiPartUploader = null;
        private static readonly ConcurrentDictionary<string, AutoResetEvent> mS3ObjectEvents = new ConcurrentDictionary<string, AutoResetEvent>();
        
        #endregion


        #region "Private properties"

        /// <summary>
        /// Returns AmazonS3 class from account info.
        /// </summary>
        private AmazonS3Client S3Client
        {
            get
            {
                return AccountInfo.Current.S3Client;
            }
        }


        /// <summary>
        /// Utility for uploading large files in smaller parts to Amazon S3 storage.
        /// </summary>
        private S3MultiPartUploader MultiPartUploader
        {
            get
            {
                if (mMultiPartUploader == null)
                {
                    mMultiPartUploader = new S3MultiPartUploader(S3Client, MINIMAL_PART_SIZE, MAXIMAL_PART_SIZE);
                }

                return mMultiPartUploader;
            }
        }


        /// <summary>
        /// Returns bucket name from account info.
        /// </summary>
        /// <param name="path">Path for which is bucket name returned.</param>
        public static string GetBucketName(string path)
        {
            AbstractStorageProvider provider = StorageHelper.GetStorageProvider(path);
            if (!string.IsNullOrEmpty(provider.CustomRootPath))
            {
                return provider.CustomRootPath;
            }

            return AccountInfo.Current.BucketName;
        }


        /// <summary>
        /// Returns whether is access public.
        /// </summary>
        /// <param name="path">Path to check.</param>
        private bool IsPublicAccess(string path)
        {
            AbstractStorageProvider provider = StorageHelper.GetStorageProvider(path);
            if (provider.PublicExternalFolderObject != null)
            {
                return provider.PublicExternalFolderObject.Value;
            }


            return AmazonHelper.PublicAccess;
        }

        #endregion


        #region "Public properties"

        /// <summary>
        /// Returns current instance of S3ObjectInfo provider.
        /// </summary>
        public static IS3ObjectInfoProvider Current
        {
            get
            {
                if (mCurrent == null)
                {
                    mCurrent = new S3ObjectInfoProvider();
                }

                return mCurrent;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates instance of S3ObjectInfoProvider. Default constructor is private, object is singleton.
        /// </summary>
        private S3ObjectInfoProvider()
        {
        }

        #endregion


        #region "Provider methods - IS3ObjectInfoProvider members"

        /// <summary>
        /// Returns list with objects from given bucket and under given path. 
        /// </summary>        
        /// <param name="path">Path.</param>
        /// <param name="type">Specifies which objects are returned (files, directories, both).</param>
        /// <param name="useFlatListing">Whether flat listing is used (all files from all subdirectories all in the result).</param>
        /// <param name="lower">Specifies whether path should be lowered inside method.</param>
        /// <param name="useCache">Indicates if results should be primary taken from cache to get better performance</param>
        /// <remarks>
        /// In order to allow to distinguish between files and directories, directories are listed with a trailing backslash.
        /// </remarks>
        public List<string> GetObjectsList(string path, ObjectTypeEnum type, bool useFlatListing = false, bool lower = true, bool useCache = true)
        {
            string bucketName = GetBucketName(path);
            if (string.IsNullOrEmpty(bucketName))
            {
                return null;
            }

            // Prepare request
            ListObjectsRequest request = new ListObjectsRequest();

            request.BucketName = bucketName;

            if (!string.IsNullOrEmpty(path))
            {
                request.Prefix = PathHelper.GetObjectKeyFromPath(path).TrimEnd('/') + "/";
            }

            if (!useFlatListing)
            {
                request.Delimiter = "/";
            }

            HashSet<string> objects = new HashSet<string>();

            // Create cache key
            string cacheKey = request.Prefix + "|" + type.ToString("F") + "|" + useFlatListing + "|" + lower;

            // Try to take form cache first
            if (useCache && RequestStockHelper.Contains(STORAGE_KEY, cacheKey, false))
            {
                IO.Directory.LogDirectoryOperation(path, "GetObjectListFromCache", IOProviderName.Amazon);
                objects = RequestStockHelper.GetItem(STORAGE_KEY, cacheKey, false) as HashSet<string>;
                return (objects != null) ? objects.ToList() : new List<string>();
            }

            bool moreObjects;
            do
            {
                ListObjectsResponse response = S3Client.ListObjects(request);
                if ((type == ObjectTypeEnum.Directories) && !useFlatListing)
                {
                    // List directories - only if not flat listing is used
                    foreach (string folder in response.CommonPrefixes)
                    {
                        objects.Add(path + "\\" + Path.GetFileName(folder.TrimEnd('/')) + '\\');
                    }
                }
                else
                {
                    bool allowDirectories = (type == ObjectTypeEnum.FilesAndDirectories) || (type == ObjectTypeEnum.Directories);
                    bool allowFiles = (type == ObjectTypeEnum.FilesAndDirectories) || (type == ObjectTypeEnum.Files);
                
                    // List files (objects) and directory if flat listing is used
                    foreach (S3Object entry in response.S3Objects)
                    {
                        bool isDirectory = entry.Key.EndsWith("/", StringComparison.Ordinal);
                        if ((isDirectory && allowDirectories) || (!isDirectory && allowFiles))
                        {
                            objects.Add(PathHelper.GetPathFromObjectKey(entry.Key, true, isDirectory, lower));
                        }
                    }
                }
                
                // Mark for continuation
                if (response.IsTruncated)
                {
                    request.Marker = response.NextMarker;
                }
                
                moreObjects = response.IsTruncated;
            } while (moreObjects);

            // Remove source object from collection
            objects.Remove(PathHelper.GetPathFromObjectKey(request.Prefix, true, true, lower));

            IO.Directory.LogDirectoryOperation(path, "ListObjects", IOProviderName.Amazon);

            // Add to cache
            RequestStockHelper.AddToStorage(STORAGE_KEY, cacheKey, objects, false);

            return objects.ToList();
        }


        /// <summary>
        /// Returns whether object exists.
        /// </summary>
        /// <param name="obj">Object info.</param>
        public bool ObjectExists(IS3ObjectInfo obj)
        {
            return obj.Exists();
        }


        /// <summary>
        /// Returns object content as a stream.
        /// </summary>
        /// <param name="obj">Object info.</param>
        /// <param name="fileMode">File mode.</param>
        /// <param name="fileAccess">File access.</param>
        /// <param name="fileShare">Sharing permissions.</param>
        /// <param name="bufferSize">Buffer size.</param>
        public Stream GetObjectContent(IS3ObjectInfo obj, FileMode fileMode = FileMode.Open, FileAccess fileAccess = FileAccess.Read, FileShare fileShare = FileShare.ReadWrite, int bufferSize = 0x1000)
        {
            if (!ObjectExists(obj))
            {
                return null;
            }

            var s3ObjectEvent = mS3ObjectEvents.GetOrAdd(obj.Key, new AutoResetEvent(true));
            try
            {
                string tempPath = Path.Combine(PathHelper.TempPath, PathHelper.GetPathFromObjectKey(obj.Key, false));
                Directory.CreateDiskDirectoryStructure(tempPath);

                // First try to get data from cache
                string cachePath = Path.Combine(PathHelper.CachePath, PathHelper.GetPathFromObjectKey(obj.Key, false));
                string eTagFilename = cachePath + ".etag";

                s3ObjectEvent.WaitOne();

                if (IO.File.Exists(cachePath))
                {
                    // Get E-tag from file                    
                    string eTag = System.IO.File.ReadAllText(eTagFilename).Trim();

                    // E-tags are the same - we can get file from cache
                    if (eTag == obj.ETag)
                    {
                        s3ObjectEvent.Set();

                        System.IO.FileStream fs = new System.IO.FileStream(cachePath, fileMode, fileAccess, fileShare, bufferSize);

                        // Log operation
                        FileDebug.LogFileOperation(PathHelper.GetPathFromObjectKey(obj.Key, true), "GetObjectFromCache", IOProviderName.Amazon);

                        return fs;
                    }
                }

                // Download object
                GetObjectRequest request = new GetObjectRequest();
                request.BucketName = obj.GetBucketName();
                request.Key = obj.Key;

                // Save it to the temp
                using (GetObjectResponse response = S3Client.GetObject(request))
                {
                    response.WriteResponseStreamToFile(tempPath);
                }

                // Log operation
                FileDebug.LogFileOperation(PathHelper.GetPathFromObjectKey(obj.Key, true), "GetObjectFromS3Storage", IOProviderName.Amazon);

                // Copy to the cache
                Directory.CreateDiskDirectoryStructure(cachePath);
                System.IO.File.Copy(tempPath, cachePath, true);

                // Save e-tag to the cache
                System.IO.File.WriteAllText(eTagFilename, obj.ETag);

                // It's needed, because append is not supported in used amazon API
                if ((fileMode == FileMode.Append) && (fileAccess != FileAccess.Read))
                {
                    fileMode = FileMode.Open;
                    fileAccess = FileAccess.ReadWrite;
                }

                // Return temp file stream
                return new System.IO.FileStream(tempPath, fileMode, fileAccess, fileShare, bufferSize);
            }
            finally
            {
                s3ObjectEvent.Set();
                mS3ObjectEvents.TryRemove(obj.Key, out s3ObjectEvent);
            }
        }


        /// <summary>
        /// Puts local file to Amazon S3 storage.
        /// </summary>
        /// <remarks>
        /// For uploading a file from different file system other than local file system to Amazon S3 storage,
        /// <see cref="PutDataFromStreamToObject(IS3ObjectInfo, Stream)"/> method should be used.
        /// </remarks>
        /// <param name="obj">Object info.</param>
        /// <param name="pathToSource">Path to local file.</param>
        public void PutFileToObject(IS3ObjectInfo obj, string pathToSource)
        {
            if (!obj.IsLocked)
            {
                obj.Lock();
                
                var bucket = obj.GetBucketName();
                var fileLength = new System.IO.FileInfo(pathToSource).Length;
                
                if (fileLength >= RECOMMENDED_SIZE_FOR_MULTIPART_UPLOAD)
                {
                    CompleteMultipartUploadResponse response = MultiPartUploader.UploadFromFilePath(obj.Key, bucket, pathToSource);
                    SetS3ObjectMetadaFromResponse(obj, response, fileLength);
                }
                else
                {
                    PutObjectRequest request = CreatePutRequest(obj.Key, bucket);
                    request.FilePath = pathToSource;
                
                    PutObjectResponse response = S3Client.PutObject(request);
                    SetS3ObjectMetadaFromResponse(obj, response, fileLength);
                }

                // Log operation
                FileDebug.LogFileOperation(PathHelper.GetPathFromObjectKey(obj.Key, true), "PutFileToObject", IOProviderName.Amazon);

                obj.UnLock();

                RemoveRequestCache(obj.Key);
            }
            else
            {
                throw new Exception("[IS3ObjectInfoProvider.PutFileToObject]: Couldn't upload object " + obj.Key + " because it is used by another process.");
            }
        }


        /// <summary>
        /// Puts data from stream to Amazon S3 storage.
        /// </summary>
        /// <param name="obj">Object info.</param>
        /// <param name="stream">Stream to upload.</param>
        public void PutDataFromStreamToObject(IS3ObjectInfo obj, Stream stream)
        {
            if (!obj.IsLocked)
            {
                obj.Lock();
                var bucket = obj.GetBucketName();
                var length = stream.Length;

                if (length > RECOMMENDED_SIZE_FOR_MULTIPART_UPLOAD)
                {
                    CompleteMultipartUploadResponse response = MultiPartUploader.UploadFromStream(obj.Key, bucket, stream);
                    SetS3ObjectMetadaFromResponse(obj, response, length);
                }
                else
                {
                    PutObjectRequest request = CreatePutRequest(obj.Key, bucket);
                    request.InputStream = stream;

                    PutObjectResponse response = S3Client.PutObject(request);
                    SetS3ObjectMetadaFromResponse(obj, response, length);
                }

                // Log operation
                FileDebug.LogFileOperation(PathHelper.GetPathFromObjectKey(obj.Key, true), "PutStreamToObject", IOProviderName.Amazon);

                obj.UnLock();

                RemoveRequestCache(obj.Key);
            }
            else
            {
                throw new Exception("[IS3ObjectInfoProvider.PutDataFromStreamToObject]: Couldn't upload object " + obj.Key + " because it is used by another process.");
            }
        }


        /// <summary>
        /// Puts text to Amazon S3 storage object.
        /// </summary>
        /// <param name="obj">Object info.</param>
        /// <param name="content">Content to add.</param>
        public void PutTextToObject(IS3ObjectInfo obj, string content)
        {
            if (!obj.IsLocked)
            {
                string path = PathHelper.GetPathFromObjectKey(obj.Key, true);

                obj.Lock();
                PutObjectRequest request = CreatePutRequest(obj.Key, GetBucketName(path));
                request.ContentBody = content;

                var response = S3Client.PutObject(request);
                SetS3ObjectMetadaFromResponse(obj, response);

                // Log operation
                FileDebug.LogFileOperation(PathHelper.GetPathFromObjectKey(obj.Key, true), "PutTextToObject", IOProviderName.Amazon);

                obj.UnLock();

                RemoveRequestCache(obj.Key);
            }
            else
            {
                throw new Exception("[IS3ObjectInfoProvider.PutTextToObject]: Couldn't upload object " + obj.Key + " because it is used by another process.");
            }
        }


        /// <summary>
        /// Appends text to Amazon S3 storage object.
        /// </summary>
        /// <param name="obj">Object info.</param>
        /// <param name="content">Content to append.</param>
        public void AppendTextToObject(IS3ObjectInfo obj, string content)
        {
            if (ObjectExists(obj))
            {
                if (!obj.IsLocked)
                {
                    obj.Lock();

                    // Get original content
                    Stream stream = GetObjectContent(obj);

                    string fileContent = null;

                    // Put content of stream to path
                    using (StreamReader reader = StreamReader.New(stream))
                    {
                        fileContent = reader.ReadToEnd();
                    }

                    PutObjectRequest request = CreatePutRequest(obj.Key, obj.GetBucketName());
                    request.ContentBody = fileContent + content;

                    var response = S3Client.PutObject(request);
                    SetS3ObjectMetadaFromResponse(obj, response);

                    // Log operation
                    FileDebug.LogFileOperation(PathHelper.GetPathFromObjectKey(obj.Key, true), "AppendTextToObject", IOProviderName.Amazon);

                    obj.UnLock();

                    RemoveRequestCache(obj.Key);
                }
                else
                {
                    throw new Exception("[IS3ObjectInfoProvider.AppendTextToObject]: Couldn't upload object " + obj.Key + " because it is used by another process.");
                }
            }
            else
            {
                PutTextToObject(obj, content);
            }
        }


        /// <summary>
        /// Deletes object from Amazon S3 storage.
        /// </summary>
        /// <param name="obj">Object info.</param>
        public void DeleteObject(IS3ObjectInfo obj)
        {
            DeleteObjectRequest request = new DeleteObjectRequest
            {
                BucketName = obj.GetBucketName(),
                Key = obj.Key
            };

            DeleteObjectResponse response = S3Client.DeleteObject(request);

            obj.DeleteMetadataFile();

            // Log operation
            FileDebug.LogFileOperation(PathHelper.GetPathFromObjectKey(obj.Key, true), "DeleteObject", IOProviderName.Amazon);

            RemoveRequestCache(obj.Key);

            try
            {
                RemoveFromTemp(obj);
                RemoveFromCache(obj);
            }
            catch (IOException)
            {
                // Operation is not crucial, in case that something went wrong (e.g. concurrent access, etc.) during the cleanup, suppress the exception
            }
        }


        /// <summary>
        /// Copies object to another.
        /// </summary>
        /// <param name="sourceObject">Source object info.</param>
        /// <param name="destObject">Destination object info.</param>
        public void CopyObjects(IS3ObjectInfo sourceObject, IS3ObjectInfo destObject)
        {
            string destPath = PathHelper.GetPathFromObjectKey(destObject.Key, true);

            CopyObjectRequest request = new CopyObjectRequest
            {
                SourceBucket = sourceObject.GetBucketName(),
                DestinationBucket = GetBucketName(destPath),
                SourceKey = sourceObject.Key,
                DestinationKey = destObject.Key,
            };

            if (IsPublicAccess(destPath))
            {
                request.CannedACL = S3CannedACL.PublicRead;
            }

            CopyObjectResponse response = S3Client.CopyObject(request);
            destObject.ETag = response.ETag;
            destObject.Length = ValidationHelper.GetLong(response.ContentLength, 0);

            // Log operation
            FileDebug.LogFileOperation(PathHelper.GetPathFromObjectKey(sourceObject.Key, true) + "|" + PathHelper.GetPathFromObjectKey(destObject.Key, true), "CopyObjects", IOProviderName.Amazon);

            RemoveRequestCache(destObject.Key);
        }


        /// <summary>
        /// Creates empty object.
        /// </summary>
        /// <param name="obj">Object info.</param>
        public void CreateEmptyObject(IS3ObjectInfo obj)
        {
            string path = PathHelper.GetPathFromObjectKey(obj.Key, true);

            PutObjectRequest request = CreatePutRequest(obj.Key, GetBucketName(path));
            request.InputStream = new MemoryStream();

            var response = S3Client.PutObject(request);
            SetS3ObjectMetadaFromResponse(obj, response);

            // Log operation
            FileDebug.LogFileOperation(PathHelper.GetPathFromObjectKey(obj.Key, true), "CreateEmptyObject", IOProviderName.Amazon);

            RemoveRequestCache(obj.Key);
        }

        #endregion


        #region "Factory methods - IIS3ObjectInfoProvider members"

        /// <summary>
        /// Returns new instance of IS3ObjectInfo.
        /// </summary>
        /// <param name="path">Path with file name.</param>        
        public IS3ObjectInfo GetInfo(string path)
        {
            return new S3ObjectInfo(path);
        }


        /// <summary>
        /// Initializes new instance of S3 object info with specified bucket name.
        /// </summary>        
        /// <param name="path">Path with file name.</param>
        /// <param name="key">Specifies that given path is already object key.</param>
        public IS3ObjectInfo GetInfo(string path, bool key)
        {
            return new S3ObjectInfo(path, key);
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Removes cached object's items from request cache.
        /// </summary>
        /// <param name="objectKey">Object key.</param>
        internal static void RemoveRequestCache(string objectKey)
        {
            var storageKey = S3ObjectInfo.STORAGE_KEY;

            RequestStockHelper.Remove(storageKey, objectKey + "|Exists", false);
            RequestStockHelper.Remove(storageKey, objectKey + "|Length", false);
            RequestStockHelper.Remove(storageKey, objectKey + "|Metadata", false);
            RequestStockHelper.Remove(storageKey, objectKey + "|ETag", false);

            // Remove all cached directories
            RequestStockHelper.DropStorage(STORAGE_KEY, false);
        }


        /// <summary>
        /// Remove S3 file from temporary local storage.
        /// </summary>
        /// <param name="obj">S3 object to be removed from temporary local storage</param>
        private static void RemoveFromTemp(IS3ObjectInfo obj)
        {
            string tempPath = Path.Combine(PathHelper.TempPath, PathHelper.GetPathFromObjectKey(obj.Key, false));
            DeleteFileFromLocalPath(tempPath);
        }


        /// <summary>
        /// Remove S3 file from cache local storage.
        /// </summary>
        /// <param name="obj">S3 object to be removed from cache local storage</param>
        private static void RemoveFromCache(IS3ObjectInfo obj)
        {
            string cachePath = Path.Combine(PathHelper.CachePath, PathHelper.GetPathFromObjectKey(obj.Key, false));
            DeleteFileFromLocalPath(cachePath);
            DeleteFileFromLocalPath(cachePath + ".etag");
        }


        /// <summary>
        /// Delete file from local filesystem
        /// </summary>
        /// <param name="path">Path to the file to be deleted</param>
        private static void DeleteFileFromLocalPath(string path)
        {
            if (!string.IsNullOrEmpty(path) && System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
        }


        /// <summary>
        /// Creates request for uploading data to Amazon S3 storage.
        /// </summary>
        /// <param name="key">Unique identifier for an object within a bucket.</param>
        /// <param name="bucket">Existing Amazon S3 bucket.</param>
        private static PutObjectRequest CreatePutRequest(string key, string bucket)
        {
            var request =  new PutObjectRequest
            {
                BucketName = bucket,
                Key = key,
            };

            if (AmazonHelper.PublicAccess)
            {
                request.CannedACL = S3CannedACL.PublicRead;
            }

            return request;
        }


        /// <summary>
        /// Sets metadata from response acquired from Amazon S3 storage to S3ObjectInfo.
        /// </summary>
        /// <param name="obj">Representation of the file on Amazon S3 storage.</param>
        /// <param name="eTag">ETag assigned to file uploaded to Amazon S3 storage.</param>
        /// <param name="response">Response acquired from Amazon S3 storage after uploading file.</param>
        /// <param name="length">
        /// Amazon S3 storage does not return length of the uploaded file in <paramref name="response"/>, 
        /// if the file was uploaded via multipart upload. In case of multipart upload is <see cref="IS3ObjectInfo.Length"/>
        /// of the <paramref name="obj"/> set via this parameter.
        /// </param>
        private void SetS3ObjectMetadaFromResponse(IS3ObjectInfo obj, string eTag, AmazonWebServiceResponse response, long length)
        {
            var responseLength = ValidationHelper.GetLong(response.ContentLength, 0);
            if (responseLength == 0)
            {
                responseLength = length;
            }

            obj.ETag = eTag;
            obj.Length = responseLength;
        }


        /// <summary>
        /// Sets metadata from response acquired from Amazon S3 storage to S3ObjectInfo.
        /// </summary>
        /// <param name="obj">Representation of the file on Amazon S3 storage.</param>
        /// <param name="response">Response acquired from Amazon S3 storage after uploading file.</param>
        /// <param name="length">
        /// Amazon S3 storage does not return length of the uploaded file in <paramref name="response"/>, 
        /// if the file was uploaded via multipart upload. In case of multipart upload is <see cref="IS3ObjectInfo.Length"/>
        /// of the <paramref name="obj"/> set via this parameter.
        /// </param>
        private void SetS3ObjectMetadaFromResponse(IS3ObjectInfo obj, PutObjectResponse response, long length = 0)
        {
            SetS3ObjectMetadaFromResponse(obj, response.ETag, response, length);
        }


        /// <summary>
        /// Sets metadata from response acquired from Amazon S3 storage to S3ObjectInfo.
        /// </summary>
        /// <param name="obj">Representation of the file on Amazon S3 storage.</param>
        /// <param name="response">Response acquired from Amazon S3 storage after uploading file.</param>
        /// <param name="length">
        /// Amazon S3 storage does not return length of the uploaded file in <paramref name="response"/>, 
        /// if the file was uploaded via multipart upload. In case of multipart upload is <see cref="IS3ObjectInfo.Length"/>
        /// of the <paramref name="obj"/> set via this parameter.
        /// </param>
        private void SetS3ObjectMetadaFromResponse(IS3ObjectInfo obj, CompleteMultipartUploadResponse response, long length = 0)
        {
            SetS3ObjectMetadaFromResponse(obj, response.ETag, response, length);
        }

        #endregion


        #region "Conversion methods"

        /// <summary>
        /// Returns date time as a string type in english culture.
        /// </summary>
        /// <param name="datetime">Date time.</param>
        public static string GetDateTimeString(DateTime datetime)
        {
            return ValidationHelper.GetString(datetime, string.Empty, CultureHelper.EnglishCulture);
        }


        /// <summary>
        /// Returns date time as a DateTime type converted using english culture.
        /// </summary>
        /// <param name="datetime">String date time.</param>        
        public static DateTime GetStringDateTime(string datetime)
        {
            return ValidationHelper.GetDateTime(datetime, DateTimeHelper.ZERO_TIME, CultureHelper.EnglishCulture);
        }

        #endregion
    }
}
