using CMS.Base;
using CMS.Helpers;

using Microsoft.WindowsAzure.Storage.Blob;

namespace CMS.AzureStorage
{
    /// <summary>
    /// Helps to save data about files in blob storage into cache.
    /// </summary>
    /// <remarks>Current implementation caches data for current request only.</remarks>
    internal class BlobCacheHelper : AbstractHelper<BlobCacheHelper>
    {
        // Name of the actions made to blob that needs to be cached
        private const string EXISTS = "Exists";
        private const string OBJECT = "Object";


        #region "Public methods"

        /// <summary>
        /// Removes all data from cache about given <paramref name="blobInfo"/>.
        /// </summary>
        /// <param name="blobInfo">Representation of Azure blob unit. Is used as a key to cache values.</param>
        public static void Remove(BlobInfo blobInfo)
        {
            HelperObject.RemoveInternal(blobInfo);
        }


        /// <summary>
        /// Marks information about existence of blob in blob storage into cache.
        /// </summary>
        /// <param name="blobInfo">Representation of Azure blob unit. Is used as a key to cache values.</param>
        /// <param name="exists">True if blob exists in blob storage, false otherwise.</param>
        public static void MarkBlobExists(BlobInfo blobInfo, bool exists = true)
        {
            HelperObject.MarkBlobExistsInternal(blobInfo, exists);
        }


        /// <summary>
        /// Indicates if information about blob existence in blob storage is cached.
        /// </summary>
        /// <param name="blobInfo">Representation of Azure blob unit. Is used as a key to cache values.</param>
        public static bool IsExistenceOfBlobMarked(BlobInfo blobInfo)
        {
            return HelperObject.IsExistenceOfBlobMarkedInternal(blobInfo);
        }


        /// <summary>
        /// Indicates if blob exists in blob storage, false otherwise.
        /// </summary>
        /// <param name="blobInfo">Representation of Azure blob unit. Is used as a key to cache values.</param>
        /// <returns>
        /// True if blob exists in blob storage. 
        /// False if blob does not exist or the information about existence is not known.
        /// </returns>
        public static bool ExistsInBlobStorage(BlobInfo blobInfo)
        {
            return HelperObject.ExistsInBlobStorageInternal(blobInfo);
        }


        /// <summary>
        /// Adds <see cref="CloudBlockBlob"/> instance for given <paramref name="blobInfo"/> in cache.
        /// </summary>
        /// <param name="blobInfo">Representation of Azure blob unit. Is used as a key to cache values.</param>
        public static void AddCloudBlockBlob(BlobInfo blobInfo)
        {
            HelperObject.AddCloudBlockBlobInternal(blobInfo);
        }


        /// <summary>
        /// Indicates if <see cref="CloudBlockBlob"/> isntance encapsulated by <paramref name="blobInfo"/> is saved in cache.
        /// </summary>
        /// <param name="blobInfo">Representation of Azure blob unit. Is used as a key to cache values.</param>
        public static bool IsCloudBlockBlobInstanceSaved(BlobInfo blobInfo)
        {
            return HelperObject.IsCloudBlockBlobInstanceSavedInternal(blobInfo);
        }


        /// <summary>
        /// Returns <see cref="CloudBlockBlob"/> instance for given <paramref name="blobInfo"/> from cache.
        /// </summary>        
        /// <param name="blobInfo">Representation of Azure blob unit. Is used as a key to cache values.</param>
        public static CloudBlockBlob GetCloudBlockBlob(BlobInfo blobInfo)
        {
            return HelperObject.GetCloudBlockBlobInternal(blobInfo);
        }


        /// <summary>
        /// Adds the specified key and value to the cache.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add.</param>
        public static void AddToCache(string key, object value)
        {
            HelperObject.AddToCacheInternal(key, value);
        }


        /// <summary>
        /// Removes the value with the specified key from the cache.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        public static void RemoveFromCache(string key)
        {
            HelperObject.RemoveFromCacheInternal(key);
        }


        /// <summary>
        /// Determines whether the cache contains the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the cache.</param>
        /// <returns>True if cache contains key.</returns>
        public static bool Contains(string key)
        {
            return HelperObject.ContainsInternal(key);
        }


        /// <summary>
        /// Gets the value associated with the specified key from cache.
        /// </summary>
        /// <param name="key">The key of the value to get</param>
        public static object GetFromCache(string key)
        {
            return HelperObject.GetFromCacheInternal(key);
        }

        #endregion


        #region "Internal Methods"

        /// <summary>
        /// Removes all data from cache about given <paramref name="blobInfo"/>.
        /// </summary>
        /// <param name="blobInfo">Representation of Azure blob unit. Is used as a key to cache values.</param>
        protected virtual void RemoveInternal(BlobInfo blobInfo)
        {
            RemoveFromCache(blobInfo.GetCacheKey(EXISTS));
            RemoveFromCache(blobInfo.GetCacheKey(OBJECT));
        }


        /// <summary>
        /// Indicates if blob exists in blob storage, false otherwise.
        /// </summary>
        /// <param name="blobInfo">Representation of Azure blob unit. Is used as a key to cache values.</param>
        /// <returns>
        /// True if blob exists in blob storage. 
        /// False if blob does not exist or the information about existence is not known.
        /// </returns>
        protected virtual bool ExistsInBlobStorageInternal(BlobInfo blobInfo)
        {
            return ValidationHelper.GetBoolean(GetFromCache(blobInfo.GetCacheKey(EXISTS)), false);
        }


        /// <summary>
        /// Marks information about existence of blob in blob storage into cache.
        /// </summary>
        /// <param name="blobInfo">Representation of Azure blob unit. Is used as a key to cache values.</param>
        /// <param name="exists">True if blob exists in blob storage, false otherwise.</param>
        protected virtual void MarkBlobExistsInternal(BlobInfo blobInfo, bool exists)
        {
            AddToCache(blobInfo.GetCacheKey(EXISTS), exists);
        }


        /// <summary>
        /// Indicates if information about blob existence in blob storage is cached.
        /// </summary>
        /// <param name="blobInfo">Representation of Azure blob unit. Is used as a key to cache values.</param>
        protected virtual bool IsExistenceOfBlobMarkedInternal(BlobInfo blobInfo)
        {
            return Contains(blobInfo.GetCacheKey(EXISTS));
        }


        /// <summary>
        /// Adds <see cref="CloudBlockBlob"/> instance for given <paramref name="blobInfo"/> in cache.
        /// </summary>
        /// <param name="blobInfo">Representation of Azure blob unit. Is used as a key to cache values.</param>
        protected virtual void AddCloudBlockBlobInternal(BlobInfo blobInfo)
        {
            AddToCache(blobInfo.GetCacheKey(OBJECT), blobInfo.Blob);
        }


        /// <summary>
        /// Returns <see cref="CloudBlockBlob"/> instance for given <paramref name="blobInfo"/> from cache.
        /// </summary>        
        /// <param name="blobInfo">Representation of Azure blob unit. Is used as a key to cache values.</param>
        protected virtual CloudBlockBlob GetCloudBlockBlobInternal(BlobInfo blobInfo)
        {
            return GetFromCache(blobInfo.GetCacheKey(OBJECT)) as CloudBlockBlob;
        }


        /// <summary>
        /// Indicates if <see cref="CloudBlockBlob"/> instance encapsulated by <paramref name="blobInfo"/> is saved in cache.
        /// </summary>
        /// <param name="blobInfo">Representation of Azure blob unit. Is used as a key to cache values.</param>
        protected virtual bool IsCloudBlockBlobInstanceSavedInternal(BlobInfo blobInfo)
        {
            return GetCloudBlockBlob(blobInfo) != null;
        }


        /// <summary>
        /// Adds the specified key and value to the cache.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add.</param>
        protected virtual void AddToCacheInternal(string key, object value)
        {
            RequestStockHelper.AddToStorage(BlobInfoProvider.STORAGE_KEY, key, value);
        }


        /// <summary>
        /// Removes the value with the specified key from the cache.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        protected virtual void RemoveFromCacheInternal(string key)
        {
            RequestStockHelper.Remove(BlobInfoProvider.STORAGE_KEY, key);
        }


        /// <summary>
        /// Determines whether the cache contains the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the cache.</param>
        /// <returns>True if cache contains key.</returns>
        protected virtual bool ContainsInternal(string key)
        {
            return RequestStockHelper.Contains(BlobInfoProvider.STORAGE_KEY, key);
        }


        /// <summary>
        /// Gets the value associated with the specified key from cache.
        /// </summary>
        /// <param name="key">The key of the value to get</param>
        protected virtual object GetFromCacheInternal(string key)
        {
            return RequestStockHelper.GetItem(BlobInfoProvider.STORAGE_KEY, key);
        }

        #endregion
    }
}
