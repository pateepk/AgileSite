namespace CMS.AzureStorage
{
    /// <summary>
    /// Extension methods for <see cref="BlobInfo"/> to ease work with caching.
    /// </summary>
    internal static class BlobInfoExtensionMethods
    {
        /// <summary>
        /// Creates key for given <paramref name="blobInfo"/> and <paramref name="action"/> to be used 
        /// for saving data in cache.
        /// </summary>
        /// <param name="blobInfo">Representation of Azure blob unit.</param>
        /// <param name="action">Action that was made to <paramref name="blobInfo"/> that needs to be cached.</param>
        internal static string GetCacheKey(this BlobInfo blobInfo, string action)
        {
            return BlobInfoProvider.GetBlobCacheKey(blobInfo, action);
        }
    }
}
