using System;
using System.Linq;
using System.Web.Caching;

using CMS.Helpers;
using CMS.IO;

namespace CMS.AspNet.Platform.Cache.Extension
{
    /// <summary>
    /// Extension methods for <see cref="CMSCacheDependency"/>.
    /// </summary>
    public static class CMSCacheDependencyExtensions
    {
        /// <summary>
        /// Creates instance of <see cref="CacheDependency"/> based on <paramref name="cmsDependency"/> values.
        /// </summary>
        /// <param name="cmsDependency">Cache dependency abstraction object.</param>
        public static CacheDependency CreateCacheDependency(this CMSCacheDependency cmsDependency)
        {
            if (cmsDependency == null)
            {
                throw new ArgumentNullException(nameof(cmsDependency));
            }

            try
            {
                return new CacheDependency(cmsDependency.FileNames, cmsDependency.CacheKeys, cmsDependency.Start);
            }
            catch
            {
                // Try create cache dependency without files stored in external storage
                if (cmsDependency.FileNames != null)
                {
                    var localFiles = cmsDependency.FileNames.Where(filename => !StorageHelper.IsExternalStorage(filename)).ToArray();
                    if (localFiles.Length != cmsDependency.FileNames.Length)
                    {
                        return new CacheDependency(localFiles, cmsDependency.CacheKeys, cmsDependency.Start);
                    }
                }

                throw;
            }
        }
    }
}
