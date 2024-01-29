using System;
using System.Collections.Generic;
using System.Reflection;

using CMS.Core;
using CMS.Base;
using System.Threading;

namespace CMS.IO
{
    /// <summary>
    /// Abstract storage provider
    /// </summary>
    public abstract class AbstractStorageProvider
    {
        #region "Constants"

        /// <summary>
        /// Directory info suffix.
        /// </summary>
        protected const string DIRECTORY_INFO = ".DirectoryInfo";

        /// <summary>
        /// Directory suffix.
        /// </summary>
        protected const string DIRECTORY = ".Directory";

        /// <summary>
        /// File info suffix.
        /// </summary>
        protected const string FILE_INFO = ".FileInfo";

        /// <summary>
        /// File suffix.
        /// </summary>
        protected const string FILE = ".File";

        /// <summary>
        /// File stream suffix.
        /// </summary>
        protected const string FILE_STREAM = ".FileStream";

        /// <summary>
        /// Directory lock suffix.
        /// </summary>
        protected const string DIRECTORY_LOCK = ".DirectoryLock";

        #endregion


        #region "Static variables"

        // Default storage provider
        private static StorageProvider mDefaultProvider;

        // System.IO storage provider
        private static StorageProvider mSystemProvider;

        #endregion


        #region "Private variables"

        // Object for locking this instance context
        private readonly object lockObject = new object();

        private Assembly mProviderAssembly;
        private string mProviderAssemblyName;
        private AbstractDirectory mDirectoryProviderObject;
        private AbstractFile mFileProviderObject;
        private bool? mIsExternalStorage;
        private volatile List<AbstractStorageProvider> mMappedProviders;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Provider name
        /// </summary>
        public virtual string Name
        {
            get
            {
                var name = ExternalStorageName;
                if (String.IsNullOrEmpty(name))
                {
                    name = IOProviderName.FileSystem;
                }

                return name;
            }
        }


        /// <summary>
        /// Returns name of external storage (if current instance running on external storage).
        /// </summary>
        public string ExternalStorageName
        {
            get;
            protected set;
        }


        /// <summary>
        /// Returns whether current instance is running on external storage.
        /// </summary>
        public bool IsExternalStorage
        {
            get
            {
                if (mIsExternalStorage == null)
                {
                    mIsExternalStorage = (!string.IsNullOrEmpty(ExternalStorageName) ||
#pragma warning disable 618
                        !string.IsNullOrEmpty(CustomRootUrl));
#pragma warning restore 618
                }
                return mIsExternalStorage.Value;
            }
        }


        /// <summary>
        /// Returns whether current instance is running on shared storage.
        /// </summary>
        public bool IsSharedStorage
        {
            get;
            protected set;
        }


        /// <summary>
        /// Returns CMSDirectoryProvider object.
        /// </summary>
        public AbstractDirectory DirectoryProviderObject
        {
            get
            {
                if (mDirectoryProviderObject == null)
                {
                    lock (lockObject)
                    {
                        if (mDirectoryProviderObject == null)
                        {
                            // Create new directory provider object
                            mDirectoryProviderObject = CreateDirectoryProviderObject();
                        }
                    }
                }

                return mDirectoryProviderObject;
            }
        }


        /// <summary>
        /// Returns AbstractFile object.
        /// </summary> 
        public AbstractFile FileProviderObject
        {
            get
            {
                if (mFileProviderObject == null)
                {
                    lock (lockObject)
                    {
                        if (mFileProviderObject == null)
                        {
                            // Create new directory provider object
                            mFileProviderObject = CreateFileProviderObject();
                        }
                    }
                }

                return mFileProviderObject;
            }
        }


        /// <summary>
        /// Default storage provider
        /// </summary>
        /// <remarks>
        /// When working with the file system, use <see cref="StorageHelper.GetStorageProvider"/> instead to take remapped paths into consideration.
        /// </remarks>
        public static StorageProvider DefaultProvider
        {
            get
            {
                if (mDefaultProvider == null)
                {
                    LazyInitializer.EnsureInitialized(ref mDefaultProvider, CreateDefaultProvider);
                }
                return mDefaultProvider;
            }
        }


        /// <summary>
        /// System.IO storage provider
        /// </summary>
        public static StorageProvider SystemProvider
        {
            get
            {
                if (mSystemProvider == null)
                {
                    LazyInitializer.EnsureInitialized(ref mSystemProvider, () => StorageProvider.CreateFileSystemStorageProvider());
                }
                return mSystemProvider;
            }
        }


        /// <summary>
        /// Gets or sets Custom path where files should be stored.
        /// </summary>
        public string CustomRootPath
        {
            get;
            set;
        }


        /// <summary>
        /// Returns whether provider has custom root path.
        /// </summary>
        public bool HasCustomRootPath
        {
            get
            {
                return !string.IsNullOrEmpty(CustomRootPath);
            }
        }


        /// <summary>
        /// Gets or sets whether external storage folder object (i.e. Container for WA blob storage) has public access or not.
        /// </summary>
        public bool? PublicExternalFolderObject
        {
            get;
            set;
        }


        /// <summary>
        /// Specifies custom root URL for provider.
        /// </summary>
        [Obsolete("Use custom code to hold this kind of value in your custom provider instead. ")]
        public string CustomRootUrl
        {
            get;
            set;
        }

        #endregion


        #region "Private properties"

        /// <summary>
        /// Parent storage provider
        /// </summary>
        protected AbstractStorageProvider ParentStorageProvider
        {
            get;
            set;
        }


        /// <summary>
        /// Mapped path
        /// </summary>
        protected string MappedPath
        {
            get;
            set;
        }


        /// <summary>
        /// Custom Provider library assembly.
        /// </summary>
        protected string ProviderAssemblyName
        {
            get
            {
                return mProviderAssemblyName;
            }
            set
            {
                mProviderAssemblyName = value;

                if (String.Equals(mProviderAssemblyName, StorageProvider.AMAZON_STORAGE, StringComparison.OrdinalIgnoreCase))
                {
                    ExternalStorageName = IOProviderName.Amazon;
                    IsSharedStorage = true;
                }
                else if (String.Equals(mProviderAssemblyName, StorageProvider.AZURE_STORAGE, StringComparison.OrdinalIgnoreCase))
                {
                    ExternalStorageName = IOProviderName.Azure;
                    IsSharedStorage = true;
                }
            }
        }


        /// <summary>
        /// Custom Provider library assembly.
        /// </summary>
        protected Assembly ProviderAssembly
        {
            get
            {
                if (mProviderAssembly == null)
                {
                    lock (lockObject)
                    {
                        if (mProviderAssembly == null)
                        {
                            mProviderAssembly = ClassHelper.GetAssembly(ProviderAssemblyName);
                        }
                    }
                }

                return mProviderAssembly;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        protected AbstractStorageProvider()
        {
            ExternalStorageName = "";
            ProviderAssemblyName = StorageProvider.FILESYSTEM_STORAGE;
        }


        /// <summary>
        /// Creates the default storage provider
        /// </summary>
        private static StorageProvider CreateDefaultProvider()
        {
            // Get the external storage name
            string externalStorageName = CoreServices.AppSettings["CMSExternalStorageName"];
            if (string.IsNullOrEmpty(externalStorageName))
            {
                // Check if application running on Azure
                if (CoreServices.AppSettings["CMSAzureProject"].ToBoolean(false))
                {
                    externalStorageName = IOProviderName.Azure;
                }
            }

            // Get the assembly name
            string providerAssemblyName = CoreServices.AppSettings["CMSStorageProviderAssembly"].ToString("");
            if (string.IsNullOrEmpty(providerAssemblyName))
            {
                if (String.Equals(externalStorageName, IOProviderName.Azure, StringComparison.OrdinalIgnoreCase))
                {
                    providerAssemblyName = StorageProvider.AZURE_STORAGE;
                }
                else if (String.Equals(externalStorageName, IOProviderName.Amazon, StringComparison.OrdinalIgnoreCase))
                {
                    providerAssemblyName = StorageProvider.AMAZON_STORAGE;
                }
                else
                {
                    providerAssemblyName = StorageProvider.FILESYSTEM_STORAGE;
                    return new StorageProvider(externalStorageName, providerAssemblyName, CoreServices.AppSettings["CMSSharedFileSystem"].ToBoolean(false));
                }
            }

            return new StorageProvider(externalStorageName, providerAssemblyName);
        }


        /// <summary>
        /// Gets the storage provider based on the given <paramref name="path"/>.
        /// </summary>
        /// <param name="path">Input path</param>
        protected virtual AbstractStorageProvider GetInternalProvider(string path)
        {
            return this;
        }


        /// <summary>
        /// Gets the storage provider based on the given <paramref name="path"/>.
        /// </summary>
        /// <param name="path">Input path</param>
        protected virtual AbstractStorageProvider GetStorageProviderInternal(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return DefaultProvider;
            }

            int mappedLength = (MappedPath != null) ? MappedPath.Length : 0;

            // Only search below if path is under the mapped path
            if (path.Length > mappedLength)
            {
                return FindMappedProvider(path) ?? TryZipProviderSafe(path, mappedLength) ?? this;
            }

            return this;
        }


        /// <summary>
        /// Returns a provider ready for use on given <paramref name="path"/>.
        /// </summary>
        /// <param name="path">File system path to obtain the storage provider for.</param>
        internal AbstractStorageProvider GetCustomizedStorageProvider(string path)
        {
            var provider = GetStorageProviderInternal(path);
            return provider.GetInternalProvider(path);
        }


        /// <summary>
        /// Ensures the zip provider for the given path
        /// </summary>
        /// <remarks>
        /// If path is not mapped (start index is 0), mapping is set to the application root 
        /// </remarks>
        /// <param name="path">Path</param>
        /// <param name="startIndex">Start index to analyze zip files</param>
        private AbstractStorageProvider TryZipProviderSafe(string path, int startIndex)
        {
            // If not mapped, treat as mapped to the root of the application so that it doesn't try to expand zip paths on the way to the application root
            if (startIndex == 0)
            {
                var appRoot = SystemContext.WebApplicationPhysicalPath;
                if (path.StartsWith(appRoot, StringComparison.OrdinalIgnoreCase))
                {
                    startIndex = appRoot.Length;
                }
            }

            return TryZipProvider(path, startIndex);
        }


        /// <summary>
        /// Ensures the zip provider for the given path
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="startIndex">Start index to analyze zip files</param>
        private AbstractStorageProvider TryZipProvider(string path, int startIndex)
        {
            // Check for the zip archive on the way
            int zipIndex = path.IndexOf(ZipStorageProvider.ZIP_START, startIndex);
            if (zipIndex >= 0)
            {
                lock (lockObject)
                {
                    int endZipIndex = path.IndexOf(ZipStorageProvider.ZIP_END, zipIndex);

                    if ((endZipIndex >= 0) && (zipIndex < endZipIndex))
                    {
                        // Ensure the zip provider mapped to the zip file
                        string zipPath = path.Substring(0, zipIndex) + path.Substring(zipIndex + 1, endZipIndex - zipIndex - 1);
                        if (File.Exists(zipPath))
                        {
                            string mapPath = path.Substring(0, endZipIndex + 1);

                            var provider = new ZipProviderLoader(zipPath, mapPath, this);

                            // Map the starting path to the zip provider
                            MapStoragePath(provider.MappedPath, provider);

                            return provider.GetStorageProviderInternal(path);
                        }

                        // Zip file not found, treat as directory and try next part of the path
                        return TryZipProvider(path, endZipIndex + 1);
                    }
                }
            }

            return null;
        }


        /// <summary>
        /// Attempts to find a provider mapped to the given path
        /// </summary>
        /// <param name="path">Path to map</param>
        private AbstractStorageProvider FindMappedProvider(string path)
        {
            var mappedProviders = mMappedProviders;
            if (mappedProviders != null)
            {
                path = StorageHelper.GetMappingPath(path);

                // Try to find mapping
                foreach (var provider in mappedProviders)
                {
                    // If path starts with set mapped path, return mapped provider
                    if (path.StartsWith(provider.MappedPath, StringComparison.OrdinalIgnoreCase) || (provider.HasCustomRootPath && path.StartsWith("\\" + provider.CustomRootPath, StringComparison.OrdinalIgnoreCase)))
                    {
                        return provider.GetStorageProviderInternal(path);
                    }
                }
            }

            return null;
        }


        /// <summary>
        /// Converts the external mapped path to the internal path of the provider
        /// </summary>
        /// <param name="path">Path to convert</param>
        protected virtual string GetFullPath(string path)
        {
            return MappedPath + "\\" + path;
        }


        /// <summary>
        /// Removes the mapping to a storage provider
        /// </summary>
        /// <param name="path">Path to unmap</param>
        public AbstractStorageProvider UnMapStoragePath(string path)
        {
            var provider = FindMappedProvider(path);
            if ((provider != null) && (provider.ParentStorageProvider != null))
            {
                provider.ParentStorageProvider.RemoveMappedProvider(path);
                return provider;
            }

            return null;
        }


        /// <summary>
        /// Removes a mapped provider from the mapped collection
        /// </summary>
        /// <param name="path">Provider to remove</param>
        protected void RemoveMappedProvider(string path)
        {
            if (mMappedProviders != null)
            {
                lock (lockObject)
                {
                    var mappedProviders = mMappedProviders;
                    var mappedProvider = mappedProviders.Find(p => p.MappedPath.Equals(path, StringComparison.OrdinalIgnoreCase));
                    if (mappedProvider != null)
                    {
                        var providers = new List<AbstractStorageProvider>(mappedProviders);
                        providers.Remove(mappedProvider);
                        mMappedProviders = providers;
                    }
                }
            }
        }


        /// <summary>
        /// Maps the given storage path to a specific provider
        /// </summary>
        /// <param name="path">Path to map</param>
        /// <param name="provider">Provider to use for the given path and sub paths</param>
        public void MapStoragePath(string path, AbstractStorageProvider provider)
        {
            EnsureMappedProviders();

            lock (lockObject)
            {
                var providers = new List<AbstractStorageProvider>(mMappedProviders);

                provider.MappedPath = path;
                provider.ParentStorageProvider = this;

                providers.Add(provider);

                mMappedProviders = providers;
            }
        }


        /// <summary>
        /// Maps target file system path to virtual path. Remaps only paths inside the web application path structure.
        /// </summary>
        /// <param name="provider">Storage provider</param>
        /// <param name="targetPath">Target path</param>
        public static string GetVirtualPhysicalPath(AbstractStorageProvider provider, string targetPath)
        {
            if (string.IsNullOrEmpty(targetPath))
            {
                return targetPath;
            }

            string finalPath = targetPath;

            if (provider.HasCustomRootPath)
            {
                string webAppPath = SystemContext.WebApplicationPhysicalPath;

                // Absolute path which contains web application path
                if (finalPath.StartsWith(provider.CustomRootPath, StringComparison.OrdinalIgnoreCase))
                {
                    finalPath = webAppPath + finalPath.Substring(provider.CustomRootPath.Length);
                }
            }

            return finalPath;
        }


        /// <summary>
        /// Maps virtual file system path to the target one. Remaps only paths inside the web application path structure.
        /// </summary>
        /// <param name="virtualPath">Virtual path</param>
        public static string GetTargetPhysicalPath(string virtualPath)
        {
            if (string.IsNullOrEmpty(virtualPath))
            {
                return virtualPath;
            }

            // Get proper provider
            var provider = StorageHelper.GetStorageProvider(virtualPath);

            string finalPath = virtualPath;

            if (provider.HasCustomRootPath)
            {
                string webAppPath = SystemContext.WebApplicationPhysicalPath;

                // Absolute path which contains web application path
                if (finalPath.StartsWith(webAppPath, StringComparison.OrdinalIgnoreCase))
                {
                    finalPath = provider.CustomRootPath + finalPath.Substring(webAppPath.Length);
                }
                // Relative path under web application path
                else if (!Path.IsPathRooted(finalPath))
                {
                    finalPath = Path.EnsureEndBackslash(provider.CustomRootPath) + finalPath;
                }
            }

            return finalPath;
        }


        /// <summary>
        /// Ensures the mapped providers list
        /// </summary>
        private void EnsureMappedProviders()
        {
            // Ensure mapped providers list
            if (mMappedProviders == null)
            {
                lock (lockObject)
                {
                    if (mMappedProviders == null)
                    {
                        mMappedProviders = new List<AbstractStorageProvider>();
                    }
                }
            }
        }

        #endregion


        #region "Provider methods (interface to the storage)"

        /// <summary>
        /// Creates new directory provider object
        /// </summary>
        protected abstract AbstractFile CreateFileProviderObject();


        /// <summary>
        /// Creates new directory provider object
        /// </summary>
        protected abstract AbstractDirectory CreateDirectoryProviderObject();


        /// <summary>
        /// Returns new instance of FileInfo object.
        /// </summary>
        /// <param name="fileName">File name</param>
        public abstract FileInfo GetFileInfo(string fileName);


        /// <summary>
        /// Returns new instance of directory info.
        /// </summary>
        /// <param name="path">Path</param>        
        public abstract DirectoryInfo GetDirectoryInfo(string path);


        /// <summary>
        /// Returns new instance of file stream.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="mode">File mode</param>        
        public abstract FileStream GetFileStream(string path, FileMode mode);


        /// <summary>
        /// Returns new instance of file stream.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="mode">File mode</param>    
        /// <param name="access">File access</param>
        public abstract FileStream GetFileStream(string path, FileMode mode, FileAccess access);


        /// <summary>
        /// Returns new instance of file stream.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="mode">File mode</param>    
        /// <param name="access">File access</param>        
        /// <param name="share">Sharing permissions</param>
        public abstract FileStream GetFileStream(string path, FileMode mode, FileAccess access, FileShare share);


        /// <summary>
        /// Returns new instance of file stream.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="mode">File mode</param>    
        /// <param name="access">File access</param>
        /// <param name="bufferSize">Buffer size</param>
        /// <param name="share">Sharing permissions</param>
        public abstract FileStream GetFileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize);

        #endregion
    }
}
