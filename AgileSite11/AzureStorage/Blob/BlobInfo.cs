using System;
using System.Net;

using CMS.Base;
using CMS.Helpers;
using CMS.IO;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace CMS.AzureStorage
{
    /// <summary>
    /// Class which represents Azure blob unit.
    /// </summary>
    public class BlobInfo
    {
        #region "Public properties"

        /// <summary>
        /// Blob name.
        /// </summary>
        public string BlobName
        {
            get;
            set;
        }


        /// <summary>
        /// Blob object.
        /// </summary>
        public CloudBlockBlob Blob
        {
            get;
            set;
        }


        /// <summary>
        /// ContainerInfo object.
        /// </summary>
        public ContainerInfo Container
        {
            get;
            set;
        }


        /// <summary>
        /// Returns whether current blob is locked.
        /// </summary>
        public bool IsLocked => ValidationHelper.GetBoolean(GetMetadata(BlobInfoProvider.LOCK), false);


        /// <summary>
        /// Returns E-tag from blob.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Throws <see cref="InvalidOperationException"/> when ETag attribute could not be safely retrieved from blob 
        /// e.g. blob does not exist in blob storage.
        /// </exception>
        public string ETag
        {
            get
            {
                if (!FetchAttributes())
                {
                    throw new InvalidOperationException("ETag of the blob could not be retrieved, because blob may not exist in blob storage.");
                }

                return Blob.Properties.ETag;
            }
        }


        /// <summary>
        /// Returns length of the blob in bytes.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Throws <see cref="InvalidOperationException"/> when Length attribute could not be safely retrieved from blob 
        /// e.g. blob does not exist in blob storage.
        /// </exception>
        public long Length
        {
            get
            {
                if (!FetchAttributes())
                {
                    throw new InvalidOperationException("Length of the blob could not be retrieved, because blob may not exist in blob storage.");
                }

                return Blob.Properties.Length;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes new instance of blob info class.
        /// </summary>
        /// <param name="path">Absolute or relative file system path.</param>
        public BlobInfo(string path)
            : this(ContainerInfoProvider.GetRootContainerInfo(path), path)
        {
        }


        /// <summary>
        /// Initializes new instance of blob info class.
        /// </summary>
        /// <param name="container">Container object.</param>
        /// <param name="path">Absolute or relative file system path.</param>
        public BlobInfo(ContainerInfo container, string path)
            : this(container, path, false)
        {
        }


        /// <summary>
        /// Initializes new instance of blob info class.
        /// </summary>
        /// <param name="containerInfo">Container object.</param>
        /// <param name="path">Absolute or relative file system path or blob URI (in that case set <paramref name="blobUri"/> appropriately).</param>
        /// <param name="blobUri">Indicates whether path is blob URI</param>
        public BlobInfo(ContainerInfo containerInfo, string path, bool blobUri)
        {
            if (containerInfo == null)
            {
                throw new ArgumentException("Parameter must not be null.", nameof(containerInfo));
            }

            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Parameter must not be null or empty.", nameof(path));
            }

            Uri pathUri;
            if (blobUri && !Uri.TryCreate(path, UriKind.Absolute, out pathUri))
            {
                throw new ArgumentException("Invalid combination of blobUri and path parameters. When value of blobUri parameter is true, path must be absolute URI.");
            }

            Container = containerInfo;

            // Get blob name from path so it contains relative path from root of application
            if (blobUri && (Container.BlobContainer != null))
            {
                var containerUri = containerInfo.BlobContainer.Uri.ToString();
                if (path.StartsWithCSafe(containerUri))
                {
                    BlobName = path.Substring(containerUri.Length).Trim('/');
                }
                else
                {
                    throw new ArgumentException("Path doesn't belong to the specified container.");
                }
            }
            else
            {
                BlobName = Directory.GetBlobPathFromPath(path, Container.CaseSensitive);
            }

            BlobInfoProvider.InitBlobInfo(this);
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Sets meta data to blob.
        /// </summary>
        /// <param name="key">MetaData key.</param>
        /// <param name="value">Metadata value.</param>
        public void SetMetadata(string key, string value)
        {
            SetMetadata(key, value, true);
        }


        /// <summary>
        /// Sets meta data to blob.
        /// </summary>
        /// <param name="key">MetaData key.</param>
        /// <param name="value">Metadata value.</param>
        /// <param name="update">Indicates whether data are updated in the cloud.</param>
        public void SetMetadata(string key, string value, bool update)
        {
            SetMetadata(key, value, update, true);
        }


        /// <summary>
        /// Sets meta data to blob.
        /// </summary>
        /// <param name="key">MetaData key.</param>
        /// <param name="value">Metadata value.</param>
        /// <param name="update">Indicates whether data are updated in the cloud.</param>
        /// <param name="log">Indicates whether is operation logged.</param>
        public void SetMetadata(string key, string value, bool update, bool log)
        {
            Blob.Metadata[key] = value;

            if (update)
            {
                // Upload to blob
                Blob.SetMetadata();

                if (log)
                {
                    // Log SetMetadata operation
                    FileDebug.LogFileOperation(Directory.GetPathFromUri(Blob.Uri, true), "SetMetadata", IOProviderName.Azure);
                }
            }
        }


        /// <summary>
        /// Returns blob meta data.  
        /// </summary>
        /// <param name="key">Metadata key.</param>        
        public string GetMetadata(string key)
        {
            if (!FetchAttributes())
            {
                return null;
            }

            // Return metadata if contained
            return Blob.Metadata.ContainsKey(key) ? Blob.Metadata[key] : null;
        }


        /// <summary>
        /// Locks current blob.
        /// </summary>
        public void Lock()
        {
            if (BlobInfoProvider.BlobExists(this))
            {
                // Log Lock operation
                FileDebug.LogFileOperation(Directory.GetPathFromUri(Blob.Uri, true), "Lock", IOProviderName.Azure);

                SetMetadata(BlobInfoProvider.LOCK, true.ToString(), true, false);
            }
        }


        /// <summary>
        /// Unlocks current blob.
        /// </summary>
        public void UnLock()
        {
            if (BlobInfoProvider.BlobExists(this))
            {
                // Log Unlock operation
                FileDebug.LogFileOperation(Directory.GetPathFromUri(Blob.Uri, true), "Unlock", IOProviderName.Azure);

                SetMetadata(BlobInfoProvider.LOCK, false.ToString(), true, false);
            }
        }


        /// <summary>
        /// Acquires attributes of <see cref="Blob"/> saved in blob storage.
        /// Ensures communication with the blob storage only if it's necessary.
        /// </summary>
        /// <returns>
        /// True if blob exists in blob storage and attributes were successfully fetched, false otherwise.
        /// </returns>
        public bool FetchAttributes()
        {
            // If the blob does not exist, attributes can not be fetched
            if (BlobCacheHelper.IsExistenceOfBlobMarked(this))
            {
                return BlobCacheHelper.ExistsInBlobStorage(this);
            }

            try
            {
                Blob.FetchAttributes();
            }
            catch (StorageException e)
            {
                if (e.RequestInformation != null && e.RequestInformation.HttpStatusCode == (int)HttpStatusCode.NotFound)
                {
                    // Blob does not exist, mark that in cache
                    BlobCacheHelper.MarkBlobExists(this, false);

                    return false;
                }

                // If exception is different from 404, then throw that exception
                throw;
            }

            // Attributes were successfully fetched so mark that blob exists and cache the data from server
            BlobCacheHelper.MarkBlobExists(this);
            BlobCacheHelper.AddCloudBlockBlob(this);

            // Log FetchAttributes operation
            FileDebug.LogFileOperation(Directory.GetPathFromUri(Blob.Uri, true), "FetchAttributes", IOProviderName.Azure);

            return true;
        }

        #endregion
    }
}
