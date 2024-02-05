using System;
using System.Configuration;

using CMS.Helpers;
using CMS.Base;

using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Auth;

namespace CMS.AzureStorage
{
    /// <summary>
    /// Object which represents azure blob container
    /// </summary>
    public class ContainerInfo
    {
        #region "Public properties"

        /// <summary>
        /// Container name.
        /// </summary>
        public string ContainerName
        {
            get;
            set;
        }


        /// <summary>
        /// Account info.
        /// </summary>
        public AccountInfo Account
        {
            get;
            set;
        }


        /// <summary>
        /// Blob container.
        /// </summary>
        public CloudBlobContainer BlobContainer
        {
            get;
            set;
        }


        /// <summary>
        /// Blob client.
        /// </summary>
        public CloudBlobClient BlobClient
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if this container is public.
        /// </summary>
        public bool IsPublic
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets whether path should be case sensitive or not.
        /// </summary>
        public bool CaseSensitive
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes new instance of container info class.
        /// </summary>
        /// <param name="accountInfo">Account info.</param>
        /// <param name="containerName">Name of container.</param>
        /// <param name="referenceOnly">Sets container reference only. No existence verification, creation or permission setting.</param>
        /// <param name="publicContainer">Flag indicating whether the container is public or not.</param>
        public ContainerInfo(AccountInfo accountInfo, string containerName, bool referenceOnly, bool? publicContainer)
        {
            Account = accountInfo;
            ContainerName = containerName;

            IsPublic = publicContainer ?? ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSAzurePublicContainer"], false);

            StorageCredentials credentials;
            try
            {
                // Create credentials object
                credentials = new StorageCredentials(Account.AccountName, Account.SharedKey);
            }
            catch
            {
                throw new ConfigurationErrorsException("Cannot create StorageCredentialsAccountAndKey object. May be caused by unfilled storage account name or shared key in configuration file.");
            }

            BlobClient = new CloudBlobClient(new Uri(Account.BlobEndpoint), credentials);
            ContainerInfoProvider.InitContainerInfo(this, referenceOnly);
        }


        /// <summary>
        /// Initializes new instance of container info class.
        /// </summary>
        /// <param name="info">Account info.</param>
        /// <param name="containerName">Name of container.</param>
        public ContainerInfo(AccountInfo info, string containerName)
            : this(info, containerName, false, null)
        {
        }

        #endregion
    }
}
