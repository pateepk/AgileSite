using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;

using CMS.Helpers;
using CMS.IO;
using CMS.Base;
using CMS.SiteProvider;

using Microsoft.WindowsAzure.Storage.Blob;

using FileAccess = System.IO.FileAccess;
using FileMode = System.IO.FileMode;
using Path = CMS.IO.Path;
using SeekOrigin = System.IO.SeekOrigin;
using Stream = System.IO.Stream;
using StreamReader = CMS.IO.StreamReader;

namespace CMS.AzureStorage
{
    /// <summary>
    /// Class providing data from Azure blob storage.
    /// </summary>
    public class BlobInfoProvider
    {
        #region "Constants"

        /// <summary>
        /// Name of blob which represents folder.
        /// </summary>
        public const string DIRECTORY_BLOB = "$CMSFolder$";

        /// <summary>
        /// Property name which returns if file is locked for writing.
        /// </summary>
        public const string LOCK = "Lock";

        /// <summary>
        /// Storage key for Azure storage objects.
        /// </summary>
        public const string STORAGE_KEY = "AzureStorage|BlobInfo|";


        /// <summary>
        /// Maximal size (4 MB) of the parts used for multipart upload.
        /// </summary>
        internal const int MAXIMAL_PART_SIZE = 4 * 1024 * 1024;


        /// <summary>
        /// Minimal size of the parts used for multipart upload.
        /// </summary>
        internal const int MINIMAL_PART_SIZE = 1;

        #endregion


        #region "Private variables"

        private static readonly ConcurrentDictionary<string, AutoResetEvent> mBlobEvents = new ConcurrentDictionary<string, AutoResetEvent>();

        #endregion


        #region "Methods"

        /// <summary>
        /// Whether blob exists.
        /// </summary>
        /// <param name="blobInfo">Blob.</param>        
        public static bool BlobExists(BlobInfo blobInfo)
        {
            if (blobInfo?.Blob == null)
            {
                return false;
            }

            return blobInfo.FetchAttributes();
        }


        /// <summary>
        /// Returns reference to the blob.
        /// </summary>
        /// <param name="blobInfo">Blob info object</param>        
        public static void InitBlobInfo(BlobInfo blobInfo)
        {
            if (blobInfo?.Container?.BlobContainer == null || blobInfo.Blob != null || string.IsNullOrEmpty(blobInfo.BlobName))
            {
                return;
            }
           
            if (BlobCacheHelper.IsCloudBlockBlobInstanceSaved(blobInfo))
            {
                blobInfo.Blob = BlobCacheHelper.GetCloudBlockBlob(blobInfo);
            }
            else
            {
                blobInfo.Blob = blobInfo.Container.BlobContainer.GetBlockBlobReference(blobInfo.BlobName);
                BlobCacheHelper.AddCloudBlockBlob(blobInfo);
            }

            var cacheMinutes = ValidationHelper.GetInteger(SettingsHelper.AppSettings["CMSAzureCDNCacheMinutes"], 0);
            cacheMinutes = (cacheMinutes > 0) ? cacheMinutes : CacheHelper.CacheImageMinutes(SiteContext.CurrentSiteName);
            if (cacheMinutes > 0)
            {
                blobInfo.Blob.Properties.CacheControl = "public, max-age=" + cacheMinutes * 60;
            }
        }


        /// <summary>
        /// Returns stream with block blob.
        /// </summary>        
        /// <param name="blobInfo">Blob info object.</param>
        public static Stream GetBlobContent(BlobInfo blobInfo)
        {
            return GetBlobContent(blobInfo, null);
        }


        /// <summary>
        /// Returns stream with block blob.
        /// </summary>        
        /// <param name="blobInfo">Blob info object.</param>
        public static Stream GetBlobContentWithOptions(BlobInfo blobInfo)
        {
            int downloadBlobTimeoutInMinutes = ValidationHelper.GetInteger(SettingsHelper.AppSettings["CMSDownloadBlobTimeout"], 0);

            if (downloadBlobTimeoutInMinutes == 0)
            {
                return GetBlobContent(blobInfo);
            }
            
            BlobRequestOptions requestOptions = new BlobRequestOptions();
            requestOptions.ServerTimeout = new TimeSpan(0, downloadBlobTimeoutInMinutes, 0);
            return GetBlobContent(blobInfo, requestOptions);
        }
        
        
        /// <summary>
        /// Returns stream with block blob.
        /// </summary>        
        /// <param name="blobInfo">Blob info object.</param>
        /// <param name="requestOptions">Blob request options.</param>
        public static Stream GetBlobContent(BlobInfo blobInfo, BlobRequestOptions requestOptions)
        {
            if (blobInfo.Blob == null)
            {
                return null;
            }

            var blobEvent = mBlobEvents.GetOrAdd(blobInfo.BlobName, new AutoResetEvent(true));
            try
            {
                // First try to get data from cache
                string cachePath = Path.Combine(PathHelper.CachePath, blobInfo.BlobName);
                string eTagFilename = cachePath + ".etag";

                blobEvent.WaitOne();

                if (IO.File.Exists(cachePath))
                {
                    // Get E-tag from file                    
                    string eTag = System.IO.File.ReadAllText(eTagFilename).Trim();

                    // E-tags are the same - we can get file from cache
                    if (eTag == blobInfo.ETag)
                    {
                        blobEvent.Set();

                        System.IO.FileStream fileStream = new System.IO.FileStream(cachePath, FileMode.Open, FileAccess.Read);

                        LogBlobOperation(blobInfo, "GetFromAzureCache");

                        return fileStream;
                    }
                }

                // Create directory for temporary path if needed
                string tempPath = Path.Combine(PathHelper.TempPath, blobInfo.BlobName);
                Directory.CreateDirectoryStructure(tempPath);

                // Get data from blob
                System.IO.FileStream blobContent = new System.IO.FileStream(tempPath, FileMode.Create);

                // Get e-tag before download
                string etag = blobInfo.ETag;
                blobInfo.Blob.DownloadToStream(blobContent, null, requestOptions);
                blobContent.Seek(0, SeekOrigin.Begin);

                LogBlobOperation(blobInfo, "GetFromAzureBlob");

                // Save it to the cache
                Directory.CreateDirectoryStructure(cachePath);

                const int BufferSize = 65536;

                using (System.IO.FileStream fsCache = new System.IO.FileStream(cachePath, FileMode.Create))
                {
                    Byte[] buffer = new Byte[BufferSize];
                    int bytesRead = blobContent.Read(buffer, 0, BufferSize);

                    // Copy data from blob stream to cache
                    while (bytesRead > 0)
                    {
                        fsCache.Write(buffer, 0, bytesRead);
                        bytesRead = blobContent.Read(buffer, 0, BufferSize);
                    }
                }

                // Save E-tag
                System.IO.File.WriteAllText(eTagFilename, etag);

                blobContent.Seek(0, SeekOrigin.Begin);

                return blobContent;
            }
            finally
            {
                blobEvent.Set();
                mBlobEvents.TryRemove(blobInfo.BlobName, out blobEvent);
            }
        }


        /// <summary>
        /// Puts content of file to blob.
        /// </summary>        
        /// <param name="blobInfo">Blob info object.</param>
        /// <param name="pathToFile">Path to file.</param>
        public static void PutFileToBlob(BlobInfo blobInfo, string pathToFile)
        {
            if (blobInfo.Blob == null)
            {
                return;
            }

            ThrowOnLockedBlob(blobInfo);

            blobInfo.Lock();

            EnsureContentType(blobInfo);
            blobInfo.Blob.UploadFromFile(pathToFile);

            LogBlobOperation(blobInfo, "PutFileToBlob");

            blobInfo.UnLock();

            RemoveRequestCache(blobInfo);
        }


        /// <summary>
        /// Puts text to blob.
        /// </summary>        
        /// <param name="blobInfo">Blob.</param>
        /// <param name="text">Text to put.</param>
        public static void PutTextToBlob(BlobInfo blobInfo, string text)
        {
            if (blobInfo.Blob == null)
            {
                return;
            }

            ThrowOnLockedBlob(blobInfo);

            blobInfo.Lock();

            EnsureContentType(blobInfo);
            blobInfo.Blob.UploadText(text); 

            LogBlobOperation(blobInfo, "PutTextToBlob");

            blobInfo.UnLock();

            RemoveRequestCache(blobInfo);
        }


        /// <summary>
        /// Appends text to blob.
        /// </summary>        
        /// <param name="blobInfo">Blob.</param>
        /// <param name="text">Text to put.</param>
        /// <param name="encoding">Encoding.</param>
        public static void AppendTextToBlob(BlobInfo blobInfo, string text, Encoding encoding = null)
        {
            if (blobInfo.Blob == null)
            {
                return;
            }

            ThrowOnLockedBlob(blobInfo);

            if (!BlobExists(blobInfo))
            {
                if (encoding == null)
                {
                    PutTextToBlob(blobInfo, text);
                }
                else
                {
                    var bytes = encoding.GetBytes(text);
                    PutByteArrayToBlob(blobInfo, bytes);
                }

                LogBlobOperation(blobInfo, "AppendTextToBlob");
            }
            else
            {
                blobInfo.Lock();

                // Get original content
                var stream = GetBlobContent(blobInfo);
                var fileContent = ReadStream(stream, encoding);

                EnsureContentType(blobInfo);
                blobInfo.Blob.UploadText(fileContent + text, encoding);
                
                LogBlobOperation(blobInfo, "AppendTextToBlob");

                blobInfo.UnLock();

                RemoveRequestCache(blobInfo);
            }
        }


        /// <summary>
        /// Puts byte array to blob.
        /// </summary>       
        /// <param name="blobInfo">Blob.</param>
        /// <param name="buffer">Buffer with data.</param>
        public static void PutByteArrayToBlob(BlobInfo blobInfo, byte[] buffer)
        {
            if ((blobInfo.Blob == null) || (buffer == null))
            {
                return;
            }

            ThrowOnLockedBlob(blobInfo);

            blobInfo.Lock();

            EnsureContentType(blobInfo);
            blobInfo.Blob.UploadFromByteArray(buffer, 0, buffer.Length);

            LogBlobOperation(blobInfo, "PutArrayToBlob");

            blobInfo.UnLock();

            RemoveRequestCache(blobInfo);
        }


        /// <summary>
        /// Puts content of stream to blob.
        /// </summary>       
        /// <param name="blobInfo">Blob.</param>
        /// <param name="stream">Stream to put.</param>
        public static void PutDataFromStreamToBlob(BlobInfo blobInfo, Stream stream)
        {
            if (blobInfo.Blob == null)
            {
                return;
            }

            ThrowOnLockedBlob(blobInfo);

            blobInfo.Lock();
            EnsureContentType(blobInfo);
            blobInfo.Blob.UploadFromStream(stream);

            LogBlobOperation(blobInfo, "PutDataFromStreamToBlob");

            blobInfo.UnLock();

            // Remove items from request cache
            RemoveRequestCache(blobInfo);
        }


        /// <summary>
        ///  Deletes blob. 
        /// </summary>        
        /// <param name="blobInfo">Blob info object.</param>
        public static void DeleteBlob(BlobInfo blobInfo)
        {
            if (blobInfo.Blob != null)
            {
                ThrowOnLockedBlob(blobInfo);

                blobInfo.Blob.Delete();

                // Remove items from request cache and storage locations
                RemoveRequestCache(blobInfo);

                try
                {
                    RemoveFromCache(blobInfo);
                    RemoveFromTemp(blobInfo);
                }
                catch (IOException)
                {
                    // Operation is not crucial, in case that something went wrong (e.g. concurrent access, etc.) during the cleanup, suppress the exception
                }                
            }
        }


        /// <summary>
        /// Copy one blob to another.
        /// </summary>
        /// <param name="sourceBlob">Source blob.</param>
        /// <param name="destBlob">Destination blob.</param>
        /// <param name="async">Indicates if copy operation should be asynchronous</param>
        public static void CopyBlobs(BlobInfo sourceBlob, BlobInfo destBlob, bool async = false)
        {
            destBlob.Blob.StartCopy(sourceBlob.Blob);

            // Log CopyBlob operation
            FileDebug.LogFileOperation(Directory.GetPathFromUri(sourceBlob.Blob.Uri, true) + "|" + Directory.GetPathFromUri(destBlob.Blob.Uri, true), "CopyBlob", IOProviderName.Azure);

            if (!async)
            {
                WaitUntilBlobCopied(destBlob);
            }

            RemoveRequestCache(destBlob);
        }


        /// <summary>
        /// Creates empty blob on the cloud. Don't use this method if you want to add content to blob.
        /// </summary>
        /// <param name="blobInfo">Blob info.</param>
        public static void CreateEmptyBlob(BlobInfo blobInfo)
        {
            EnsureContentType(blobInfo);

            using (var blobStream = blobInfo.Blob.OpenWrite())
            {
                blobStream.Commit();
            }

            RemoveRequestCache(blobInfo);

            LogBlobOperation(blobInfo, "CreateEmptyBlob");
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns cache key for specified action performed on blob.  
        /// </summary>
        /// <param name="blobInfo">Blob to obtain cache key for</param>
        /// <param name="blobAction">Action performed on the blob</param>
        internal static string GetBlobCacheKey(BlobInfo blobInfo, string blobAction)
        {
            if ((blobInfo != null) && !string.IsNullOrEmpty(blobAction))
            {
                return $"{blobInfo.Container?.ContainerName ?? String.Empty}|{blobInfo.BlobName}|{blobAction}";
            }

            throw new Exception("Couldn't get blob cache key for non existing blob or action");
        }


        /// <summary>
        /// Removes cached blob items from cache.
        /// </summary>
        /// <param name="blobInfo">Blob info to clear object cache for.</param>
        internal static void RemoveRequestCache(BlobInfo blobInfo)
        {
            BlobCacheHelper.Remove(blobInfo);
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Logs operation performed on <paramref name="blobInfo"/>.
        /// </summary>
        private static void LogBlobOperation(BlobInfo blobInfo, string operationName)
        {
            FileDebug.LogFileOperation(Directory.GetPathFromUri(blobInfo.Blob.Uri, true), operationName, IOProviderName.Azure);
        }


        /// <summary>
        /// Remove blob file from temporary local storage.
        /// </summary>
        /// <param name="blobInfo">Blob info to be removed from temporary local storage</param>
        private static void RemoveFromTemp(BlobInfo blobInfo)
        {
            string tempPath = Path.Combine(PathHelper.TempPath, Directory.GetPathFromUri(blobInfo.Blob.Uri, false));
            DeleteFileFromLocalPath(tempPath);
        }


        /// <summary>
        /// Remove blob file from cache local storage.
        /// </summary>
        /// <param name="blobInfo">Blob info to be removed from cache local storage</param>
        private static void RemoveFromCache(BlobInfo blobInfo)
        {
            string cachePath = Path.Combine(PathHelper.CachePath , Directory.GetPathFromUri(blobInfo.Blob.Uri,false));
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
        /// Sets correct content type to file.
        /// </summary>
        /// <param name="blobInfo">Blob info.</param>
        private static void EnsureContentType(BlobInfo blobInfo)
        {
            if (blobInfo.Blob != null)
            {
                blobInfo.Blob.Properties.ContentType = MimeTypeHelper.GetMimetype(Path.GetExtension(blobInfo.BlobName));
            }
        }


        /// <summary>
        /// Waits till asynchronous copy operation of blob finish.
        /// </summary>
        /// <param name="targetBlob">Target blob to which data are copied</param>
        private static void WaitUntilBlobCopied(BlobInfo targetBlob)
        {
            CloudBlockBlob target = (CloudBlockBlob)targetBlob.Container.BlobContainer.GetBlobReferenceFromServer(targetBlob.BlobName);
            while (target.CopyState.Status == CopyStatus.Pending)
            {
                target = (CloudBlockBlob)target.Container.GetBlobReferenceFromServer(target.Name);
                Thread.Sleep(1000);
            }
        }


        /// <summary>
        /// Throws <see cref="IOException"/> if the <paramref name="blobInfo"/> is locked.
        /// </summary>
        private static void ThrowOnLockedBlob(BlobInfo blobInfo)
        {
            if (blobInfo.IsLocked)
            {
                throw new IOException($"Couldn't change content of blob {blobInfo.BlobName} because it is used by another process.");
            }
        }


        /// <summary>
        /// Returns contents of <see cref="Stream"/>. Uses appropriate <paramref name="encoding"/> if it is provided.
        /// </summary>
        private static string ReadStream(Stream stream, Encoding encoding)
        {
            string fileContent;
            if (encoding != null)
            {
                using (StreamReader reader = StreamReader.New(stream, encoding))
                {
                    fileContent = reader.ReadToEnd();
                }
            }
            else
            {
                using (StreamReader reader = StreamReader.New(stream))
                {
                    fileContent = reader.ReadToEnd();
                }
            }
            return fileContent;
        }

        #endregion
    }
}
