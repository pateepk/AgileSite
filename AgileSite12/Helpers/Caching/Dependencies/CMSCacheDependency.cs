using System;
#if NETFULLFRAMEWORK
using System.Web.Caching;
#endif
using System.Xml.Serialization;
using System.Linq;
using System.Web;

using CMS.Base;
using CMS.IO;

namespace CMS.Helpers
{
    /// <summary>
    /// Establishes a dependency relationship between an item stored in ASP.NET application's cache
    /// and a file, cache key, an array of either or another dependency object. This class monitors
    /// the dependency relationship so that when any of them changes, the cache item will be 
    /// automatically removed.
    /// </summary>
    [Serializable]
    public class CMSCacheDependency
    {
        #region "Variables"
#if NETFULLFRAMEWORK
        [Obsolete]
        private CacheDependency mCacheDependency;
#endif
        private string[] mCacheKeys;

        #endregion


        #region "Properties"

        /// <summary>
        /// The date and time against which to check the last modified date of the objects passed in the filenames and cache keys arrays
        /// </summary>
        public DateTime Start
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether the dependency should include also default dependencies.
        /// </summary>
        public bool IncludeDefaultDependencies
        {
            get;
            set;
        }


        /// <summary>
        /// Gets the cache keys of the dependency.
        /// </summary>
        [XmlArray]
        public string[] CacheKeys
        {
            get
            {
                return mCacheKeys;
            }
            set
            {
                // Convert key names to lower case
                if (value != null)
                {
                    mCacheKeys = value.Select(k => (k != null) ? k.ToLowerInvariant() : null).ToArray();
                }
            }
        }


        /// <summary>
        /// Collection of file full paths used for cache dependency.
        /// </summary>
        /// <remarks>
        /// Paths pointing to the external storage are ignored and not used for cache dependency.
        /// </remarks>
        [XmlArray]
        public string[] FileNames
        {
            get;
            set;
        }

#if NETFULLFRAMEWORK
        /// <summary>
        /// .NET Cache dependency object
        /// </summary>
        [XmlIgnore]
        [Obsolete("Use CMS.AspNet.Platform.Cache.Extension.CreateCacheDependency() instead.")]
        public CacheDependency CacheDependency
        {
            get
            {
                return mCacheDependency ?? (mCacheDependency = CreateCacheDependency());
            }
            protected set
            {
                mCacheDependency = value;
            }
        }


        /// <summary>
        /// Creates <see cref="CacheDependency"/> with available <see cref="FileNames"/> and <see cref="CacheKeys"/>.
        /// </summary>
        /// <remarks>
        /// TMethod handles external storages that does not support file monitoring for cache dependency flushing.
        /// We decided not to support file cache flushing on external storages, because any other workaround would be too expensive.
        /// If you find you need to handle this problem, please ensure that the file is being flushed through a cache key.
        /// </remarks>
        /// <exception cref="HttpException">Thrown when cache dependency cannot be created (e.g. file path is pointing to the not existing file in file system)</exception>
        [Obsolete]
        private CacheDependency CreateCacheDependency()
        {
           
            try
            {
                return new CacheDependency(FileNames, CacheKeys, Start);
            }
            catch
            {
                // Try create cache dependency without files stored in external storage
                if (FileNames != null)
                {
                    var localFiles = FileNames.Where(filename => !StorageHelper.IsExternalStorage(filename)).ToArray();
                    if (localFiles.Length != FileNames.Length)
                    {
                        return new CacheDependency(localFiles, CacheKeys, Start);
                    }
                }

                throw;
            }
        }
#endif

        #endregion


        #region "Methods"

        /// <summary>
        /// Initializes a new instance of the <see cref="CMSCacheDependency"/> class that monitors a file or directory for change.
        /// </summary>
        public CMSCacheDependency()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="CMSCacheDependency"/> class that monitors a file or directory for change.
        /// </summary>
        /// <param name="filenames">Sets <see cref="FileNames"/> collection.</param>
        /// <param name="cachekeys">Sets <see cref="CacheKeys"/> collection.</param>
        /// <param name="start">Sets <see cref="Start"/> date time against which to check the last modified date of the objects passed in the file names and cache key arrays.</param>
        public CMSCacheDependency(string[] filenames, string[] cachekeys, DateTime start)
        {
            FileNames = filenames;
            CacheKeys = cachekeys;
            Start = start;
        }


        /// <summary>
        /// Ensures the dummy keys for this dependency
        /// </summary>
        public void EnsureDummyKeys()
        {
            if (CacheKeys != null)
            {
                // Ensure all cache keys
                foreach (string key in CacheKeys)
                {
                    CacheHelper.EnsureDummyKey(key);
                }
            }
        }

        #endregion
    }
}
