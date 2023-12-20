using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using CMS.IO;
using CMS.Base;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace CMS.AzureStorage
{
    /// <summary>
    /// Blob container management.
    /// </summary>
    public static class ContainerInfoProvider
    {
        #region "Constants"

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
        /// Default container name for root of filesystem - for backwards compatibility
        /// </summary>
        public const string CMS_ROOT = "cmsroot";

        /// <summary>
        /// Default container name for root of filesystem
        /// </summary>
        public const string CMS_STORAGE = "cmsstorage";

        #endregion


        #region "Variables"

        private static string mRootContainer = null;       
        private static Dictionary<string, ContainerInfo> containers = new Dictionary<string, ContainerInfo>();
        private static readonly object containersLock = new object();

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets or sets root container name.
        /// </summary>
        public static string GetRootContainer(string path)
        {
            // Path specific container
            AbstractStorageProvider provider = StorageHelper.GetStorageProvider(path);
            if (!string.IsNullOrEmpty(provider.CustomRootPath))
            {
                return provider.CustomRootPath.ToLowerCSafe();
            }

            if (mRootContainer != null)
            {
                return mRootContainer;
            }

            mRootContainer = SettingsHelper.AppSettings["CMSAzureRootContainer"];
            if (string.IsNullOrEmpty(mRootContainer))
            {
                ContainerInfo ci = new ContainerInfo(AccountInfo.CurrentAccount, CMS_ROOT, true, provider.PublicExternalFolderObject);

                try
                {
                    // Check if CMS_ROOT exists
                    ci.BlobContainer.FetchAttributes();

                    // No exception was thrown - it exists - set it as root container
                    mRootContainer = CMS_ROOT;
                }
                catch (StorageException)
                {
                    // CMS_ROOT was not found - set CMS_STORAGE as root container
                    mRootContainer = CMS_STORAGE;
                }
            }

            mRootContainer = mRootContainer.ToLowerInvariant();

            return mRootContainer;
        }


        /// <summary>
        /// Returns whether is container public.
        /// </summary>
        /// <param name="path">Path.</param>
        public static bool IsContainerPublic(string path)
        {
            ContainerInfo container = GetRootContainerInfo(path);
            if (container != null)
            {
                return container.IsPublic;
            }

            return false;
        }


        /// <summary>
        /// Returns container info of root container.
        /// </summary>
        public static ContainerInfo GetRootContainerInfo(string path)
        {
            path = StorageHelper.GetFullFilePhysicalPath(path, null);
            path = path.ToLowerCSafe();

            string containerName = GetRootContainer(path);

            if (containers.Keys.Contains(containerName))
            {
                return containers[containerName];
            }

            lock (containersLock)
            {
                if (containers.Keys.Contains(containerName))
                {
                    return containers[containerName];
                }

                AbstractStorageProvider provider = StorageHelper.GetStorageProvider(path);
                ContainerInfo container = new ContainerInfo(AccountInfo.CurrentAccount, containerName, false, provider.PublicExternalFolderObject);
                containers.Add(containerName, container);

                return container;
            }
        } 


        /// <summary>
        /// Deletes container.
        /// </summary>
        /// <param name="containerInfo">Container info object.</param>
        public static void DeleteContainer(ContainerInfo containerInfo)
        {
            if (containerInfo.BlobContainer == null)
            {
                return;
            }

            containerInfo.BlobContainer.Delete();
            IO.Directory.LogDirectoryOperation(containerInfo.ContainerName, "DeleteContainer", IOProviderName.Azure);
        }


        /// <summary>
        /// Returns content of container.
        /// </summary>
        /// <param name="containerInfo">Container info object.</param>        
        /// <param name="path">Path.</param>
        public static IEnumerable<IListBlobItem> GetContent(ContainerInfo containerInfo, string path)
        {
            return GetContent(containerInfo, path, false);
        }


        /// <summary>
        /// Returns content of container.
        /// </summary>
        /// <param name="containerInfo">Container info object.</param>        
        /// <param name="path">Path.</param>
        /// <param name="useFlatListing">Specifies whether flat listing is used (all blobs with path prefix are used).</param>
        public static IEnumerable<IListBlobItem> GetContent(ContainerInfo containerInfo, string path, bool useFlatListing)
        {
            if (containerInfo.BlobContainer == null)
            {
                return null;
            }

            IO.Directory.LogDirectoryOperation(containerInfo.ContainerName, "ListBlobs", IOProviderName.Azure);

            if (string.IsNullOrEmpty(path))
            {
                return containerInfo.BlobContainer.ListBlobs(useFlatBlobListing: useFlatListing);
            }

            path = Directory.GetBlobPathFromPath(path, containerInfo.CaseSensitive);

            if (string.IsNullOrEmpty(path))
            {
                return containerInfo.BlobContainer.ListBlobs();
            }

            IO.Directory.LogDirectoryOperation(containerInfo.ContainerName, "GetDirectoryReference", IOProviderName.Azure);
            CloudBlobDirectory directory = containerInfo.BlobContainer.GetDirectoryReference(path);
            return directory.ListBlobs(useFlatListing);
        }


        /// <summary>
        /// Returns reference to cloud container (creates it if not exists).
        /// </summary>
        /// <param name="containerInfo">Container info object.</param>
        public static void InitContainerInfo(ContainerInfo containerInfo)
        {
            InitContainerInfo(containerInfo, false);
        }


        /// <summary>
        /// Assigns reference to cloud container to <see cref="ContainerInfo.BlobContainer"/> property of <paramref name="containerInfo"/>.
        /// Creates a new container, if it does not exist.
        /// </summary>
        /// <param name="containerInfo">Container info object.</param>   
        /// <param name="referenceOnly">Sets container reference only. No existence verification, creation or permission setting.</param>
        /// <exception cref="IOException">Thrown when initialization of the container fails.</exception>
        public static void InitContainerInfo(ContainerInfo containerInfo, bool referenceOnly)
        {
            if (containerInfo?.BlobClient == null || string.IsNullOrEmpty(containerInfo.ContainerName) || containerInfo.BlobContainer != null)
            {
                return;
            }

            containerInfo.BlobContainer = containerInfo.BlobClient.GetContainerReference(containerInfo.ContainerName);
            IO.Directory.LogDirectoryOperation(containerInfo.ContainerName, "GetContainerReference", IOProviderName.Azure);

            if (containerInfo.BlobContainer == null || referenceOnly)
            {
                return;
            }

            try
            {
                // Create and get attributes and metadata
                containerInfo.BlobContainer.CreateIfNotExists();
                IO.Directory.LogDirectoryOperation(containerInfo.ContainerName, "CreateContainer", IOProviderName.Azure);

                containerInfo.BlobContainer.FetchAttributes();
                IO.Directory.LogDirectoryOperation(containerInfo.ContainerName, "FetchcContainerAttributes", IOProviderName.Azure);

                // Set permissions to container
                BlobContainerPermissions bcp = new BlobContainerPermissions();
                bcp.PublicAccess = containerInfo.IsPublic ? BlobContainerPublicAccessType.Blob : BlobContainerPublicAccessType.Off;
                containerInfo.BlobContainer.SetPermissions(bcp);

                IO.Directory.LogDirectoryOperation(containerInfo.ContainerName, "SetContainerPermissions", IOProviderName.Azure);
            }
            catch (StorageException ex) when (ex.RequestInformation != null)
            {
                var message = $"Error when getting container. The request failed with the following status: '{ex.RequestInformation.HttpStatusCode} {ex.RequestInformation.HttpStatusMessage}'.";
            
                throw new IOException(message, ex);                            
            }
            catch (Exception ex)
            {
                throw new IOException("Error when getting container (it may be caused by inability to connect to the cloud).", ex);
            }
        }

        #endregion
    }
}
