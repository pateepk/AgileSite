using System;
using System.Globalization;
using System.Reflection;

namespace CMS.IO
{
    /// <summary>
    /// Storage provider
    /// </summary>
    public class StorageProvider : AbstractStorageProvider
    {
        internal const string FILESYSTEM_STORAGE = "CMS.FileSystemStorage";
        internal const string AZURE_STORAGE = "CMS.AzureStorage";
        internal const string AMAZON_STORAGE = "CMS.AmazonStorage";


        /// <summary>
        /// Creates new instance of storage provider for System.IO file system.
        /// </summary>
        /// <param name="isSharedStorage">Indicates whether the storage is shared between web farm servers.</param>
        public static StorageProvider CreateFileSystemStorageProvider(bool isSharedStorage = false)
        {
            return new StorageProvider("", FILESYSTEM_STORAGE, isSharedStorage);
        }


        /// <summary>
        /// Creates new instance of storage provider for files hosted in Azure cloud storage.
        /// </summary>
        public static StorageProvider CreateAzureStorageProvider()
        {
            return new StorageProvider(IOProviderName.Azure, AZURE_STORAGE, true);
        }


        /// <summary>
        /// Creates new instance of storage provider for files hosted in Amazon S3 cloud storage.
        /// </summary>
        public static StorageProvider CreateAmazonStorageProvider()
        {
            return new StorageProvider(IOProviderName.Amazon, AMAZON_STORAGE, true);
        }


        /// <summary>
        /// Creates new instance of custom storage provider.
        /// </summary>
        /// <param name="externalStorageName">External storage name. If used path translation to URL must be set explicitly. Use empty string if storage provider is working under application root.</param>
        /// <param name="providerAssemblyName">Provider assembly name with required implementations.</param>
        /// <param name="isSharedStorage">Indicates whether the storage is shared between web farm servers.</param>
        public StorageProvider(string externalStorageName, string providerAssemblyName, bool isSharedStorage = false)
        {
            ProviderAssemblyName = providerAssemblyName;

            if (String.Equals(providerAssemblyName, StorageProvider.AMAZON_STORAGE, StringComparison.OrdinalIgnoreCase))
            {
                ExternalStorageName = IOProviderName.Amazon;
                IsSharedStorage = true;
            }
            else if (String.Equals(providerAssemblyName, StorageProvider.AZURE_STORAGE, StringComparison.OrdinalIgnoreCase))
            {
                ExternalStorageName = IOProviderName.Azure;
                IsSharedStorage = true;
            }
            else
            {
                ExternalStorageName = externalStorageName;
                IsSharedStorage = isSharedStorage;
            }
        }


        /// <summary>
        /// Creates new directory provider object
        /// </summary>
        protected override AbstractFile CreateFileProviderObject()
        {
            return CreateInstance<AbstractFile>(ProviderAssemblyName, FILE, () => new FileSystemStorage.File());
        }


        /// <summary>
        /// Creates new directory provider object
        /// </summary>
        protected override AbstractDirectory CreateDirectoryProviderObject()
        {
            return CreateInstance<AbstractDirectory>(ProviderAssemblyName, DIRECTORY, () => new FileSystemStorage.Directory());
        }


        /// <summary>
        /// Returns new instance of FileInfo object.
        /// </summary>
        /// <param name="fileName">File name</param>
        public override FileInfo GetFileInfo(string fileName)
        {
            return CreateInstance<FileInfo>(ProviderAssemblyName, FILE_INFO, () => new  FileSystemStorage.FileInfo(fileName), new object[] { fileName });
        }


        /// <summary>
        /// Returns new instance of directory info.
        /// </summary>
        /// <param name="path">Path</param>
        public override DirectoryInfo GetDirectoryInfo(string path)
        {
            return CreateInstance<DirectoryInfo>(ProviderAssemblyName, DIRECTORY_INFO, () => new FileSystemStorage.DirectoryInfo(path),  new object[] { path });
        }


        /// <summary>
        /// Returns new instance of file stream.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="mode">File mode</param>
        public override FileStream GetFileStream(string path, FileMode mode)
        {
            return CreateInstance<FileStream>(ProviderAssemblyName, FILE_STREAM, () => new FileSystemStorage.FileStream(path, mode), new object[] { path, mode });
        }


        /// <summary>
        /// Returns new instance of file stream.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="mode">File mode</param>
        /// <param name="access">File access</param>
        public override FileStream GetFileStream(string path, FileMode mode, FileAccess access)
        {
            return CreateInstance<FileStream>(ProviderAssemblyName, FILE_STREAM, () => new FileSystemStorage.FileStream(path, mode, access), new object[] { path, mode, access });
        }


        /// <summary>
        /// Returns new instance of file stream.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="mode">File mode</param>
        /// <param name="access">File access</param>
        /// <param name="share">Sharing permissions</param>
        public override FileStream GetFileStream(string path, FileMode mode, FileAccess access, FileShare share)
        {
            return CreateInstance<FileStream>(ProviderAssemblyName, FILE_STREAM, () => new FileSystemStorage.FileStream(path, mode, access, share), new object[] { path, mode, access, share });
        }


        /// <summary>
        /// Returns new instance of file stream.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="mode">File mode</param>
        /// <param name="access">File access</param>
        /// <param name="bufferSize">Buffer size</param>
        /// <param name="share">Sharing permissions</param>
        public override FileStream GetFileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize)
        {

            return CreateInstance<FileStream>(ProviderAssemblyName, FILE_STREAM, () => new FileSystemStorage.FileStream(path, mode, access, share, bufferSize), new object[] { path, mode, access, share, bufferSize });
        }


        /// <summary>
        /// Creates an instance of the given class using the provider
        /// </summary>
        /// <param name="assemblyName">Assembly name</param>
        /// <param name="typeName">Type name</param>
        /// <param name="defaultTypeInitializer">Default file system storage object initializer</param>
        /// <param name="parameters">Constructor parameters</param>
        private T CreateInstance<T>(string assemblyName, string typeName, Func<T> defaultTypeInitializer, object[] parameters = null)
            where T : class
        {
            if (FILESYSTEM_STORAGE.Equals(assemblyName, StringComparison.OrdinalIgnoreCase))
            {
                return defaultTypeInitializer();
            }

            T obj;
            var fullName = ProviderAssemblyName + typeName;

            try
            {
                if (parameters == null)
                {
                    // Empty constructor
                    obj = (T)ProviderAssembly.CreateInstance(fullName);
                }
                else
                {
                    // Constructor with parameters
                    obj = (T)ProviderAssembly.CreateInstance(fullName, false, BindingFlags.CreateInstance, null, parameters, CultureInfo.InvariantCulture, null);
                }
            }
            catch (TargetInvocationException ex)
            {
                // Propagate inner exception from constructor
                throw ex.InnerException ?? ex;
            }

            if (obj == null)
            {
                throw new ApplicationException("The class " + fullName + " couldn't be loaded.");
            }

            return obj;
        }
    }
}