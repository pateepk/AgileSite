using System;

using CMS.Helpers;
using CMS.Base;

namespace CMS.AzureStorage
{
    /// <summary>
    /// Class which represents account info for connection to cloud.
    /// </summary>
    public class AccountInfo
    {
        #region "Variables"

        private string mBlobEndpoint = string.Empty;
        private string mTableEndpoint = string.Empty;
        private string mQueueEndpoint = string.Empty;

        private static AccountInfo mCurrentAccount;

        #endregion


        #region "Public static properties"

        /// <summary>
        /// Gets instance of current account info.
        /// </summary>
        public static AccountInfo CurrentAccount
        {
            get
            {
                if (mCurrentAccount == null)
                {
                    mCurrentAccount = new AccountInfo();
                }
                return mCurrentAccount;
            }
        }

        #endregion


        #region "Public properties"

        /// <summary>
        /// Account name.
        /// </summary>
        public string AccountName
        {
            get;
            set;
        }


        /// <summary>
        /// Shared key.
        /// </summary>
        public string SharedKey
        {
            get;
            set;
        }


        /// <summary>
        /// Blob storage endpoint.
        /// </summary>
        public string BlobEndpoint
        {
            get
            {
                if (string.IsNullOrEmpty(mBlobEndpoint))
                {
                    mBlobEndpoint = "http://" + AccountName + ".blob.core.windows.net/";
                }

                return mBlobEndpoint;
            }
            set
            {
                mBlobEndpoint = EnsureEndPoint(value);
            }
        }


        /// <summary>
        /// Table storage endpoint.
        /// </summary>
        public string TableEndpoint
        {
            get
            {
                if (string.IsNullOrEmpty(mTableEndpoint))
                {
                    mTableEndpoint = "http://" + AccountName + ".table.core.windows.net/";
                }

                return mTableEndpoint;
            }
            set
            {
                mTableEndpoint = EnsureEndPoint(value);
            }
        }


        /// <summary>
        /// Queue storage endpoint.
        /// </summary>
        public string QueueEndpoint
        {
            get
            {
                if (string.IsNullOrEmpty(mQueueEndpoint))
                {
                    mQueueEndpoint = "http://" + AccountName + ".queue.core.windows.net/";

                }
                return mQueueEndpoint;
            }
            set
            {
                mQueueEndpoint = EnsureEndPoint(value);
            }
        }


        /// <summary>
        /// Checks if acount information is set.
        /// 
        /// Returns true if account name and shared key config keys are set. False otherwise.
        /// </summary>
        public static bool AccountConfigured
        {
            get
            {
                return (!String.IsNullOrEmpty(SettingsHelper.AppSettings["CMSAzureAccountName"]) && !String.IsNullOrEmpty(SettingsHelper.AppSettings["CMSAzureSharedKey"]));
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Default constructor.
        /// </summary>
        public AccountInfo()
        {
            // Load values from web.config
            AccountName = ValidationHelper.GetString(SettingsHelper.AppSettings["CMSAzureAccountName"], string.Empty);
            SharedKey = ValidationHelper.GetString(SettingsHelper.AppSettings["CMSAzureSharedKey"], string.Empty);
            QueueEndpoint = ValidationHelper.GetString(SettingsHelper.AppSettings["CMSAzureQueueEndPoint"], string.Empty);
            TableEndpoint = ValidationHelper.GetString(SettingsHelper.AppSettings["CMSAzureTableEndPoint"], string.Empty);
            BlobEndpoint = ValidationHelper.GetString(SettingsHelper.AppSettings["CMSAzureBlobEndPoint"], string.Empty);
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Returns endpoint with ending slash.
        /// </summary>
        /// <param name="endpoint">Endpoint.</param>        
        private static string EnsureEndPoint(string endpoint)
        {
            if (string.IsNullOrEmpty(endpoint))
            {
                return endpoint;
            }

            endpoint = URLHelper.AddHTTPToUrl(endpoint);           
            return endpoint.TrimEnd('/') + "/";
        }

        #endregion
    }
}
