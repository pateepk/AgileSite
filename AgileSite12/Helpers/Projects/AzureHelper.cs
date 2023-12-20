using System;
using System.Text.RegularExpressions;

using CMS.Helpers;

namespace CMS.Base
{
    /// <summary>
    /// Helper methods and properties for Microsoft Azure integration.
    /// </summary>
    public class AzureHelper
    {
        #region "Constants"

        /// <summary>
        /// Timeout for getting message from queue.
        /// </summary>
        public const int QUEUE_TIMEOUT = 1;

        /// <summary>
        /// Path to azure file page.
        /// </summary>
        public const string AZURE_FILE_PAGE = "~/CMSPages/GetAzureFile.aspx";

        /// <summary>
        /// Domain of SQL Azure service.
        /// </summary>
        public const string SQLAZURE_DOMAIN = "database.windows.net";

        /// <summary>
        /// Sleep interval between tries for init of smart search worker role.
        /// </summary>
        public const int SEARCH_INIT_SLEEP = 30000;

        /// <summary>
        /// Sleep interval between starting of task processor.
        /// </summary>
        public const int SEARCH_PROCESS_SLEEP = 30000;

        #endregion


        #region "Variables"

        private static string mBlobUrl = null;

        #endregion


        #region "Public properties"
        
        /// <summary>
        /// Gets or sets current instance id.
        /// </summary>
        public static string CurrentInstanceID
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets internal endpoint of current instance.
        /// </summary>
        public static string CurrentInternalEndpoint
        {
            get;
            set;
        }        


        /// <summary>
        /// Gets or sets URL of blob endpoint. 
        /// </summary>        
        public static string BlobUrl
        {
            get
            {
                if (mBlobUrl == null)
                {
                    string cdn = SettingsHelper.AppSettings["CMSAzureCDNEndpoint"];
                    if (!String.IsNullOrEmpty(cdn))
                    {
                        cdn = EnsureUrl(cdn.TrimEnd('/'));
                        mBlobUrl = cdn;
                    }
                    else
                    {
                        mBlobUrl = SettingsHelper.AppSettings["CMSAzureBlobEndPoint"];
                        if (String.IsNullOrEmpty(mBlobUrl))
                        {
                            mBlobUrl = "https://" + SettingsHelper.AppSettings["CMSAzureAccountName"] + ".blob.core.windows.net/";
                        }
                        else
                        {
                            mBlobUrl = EnsureUrl(mBlobUrl);
                        }
                    }
                }

                return mBlobUrl;
            }
            set
            {
                mBlobUrl = value;
            }
        }


        /// <summary>
        /// Deployment id of instance.
        /// </summary>
        public static string DeploymentID
        {
            get;
            set;
        }


        /// <summary>
        /// Number of Azure instances within WebRole.
        /// </summary>
        public static int NumberOfInstances
        {
            get;
            set;
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Returns download path for given path.
        /// </summary>
        /// <param name="path">Path</param>
        public static string GetDownloadPath(string path)
        {
            if (!String.IsNullOrEmpty(path))
            {
                return AZURE_FILE_PAGE + "?path=" + URLHelper.EscapeSpecialCharacters(path);
            }

            return null;
        }


        /// <summary>
        /// Returns whether SQL server is SQL Azure according to its name.
        /// </summary>
        /// <param name="serverName">Server name to check</param>
        public static bool IsSQLAzureServer(string serverName)
        {
            return serverName.ToLowerCSafe().EndsWithCSafe(SQLAZURE_DOMAIN);
        }


        /// <summary>
        /// Returns true if current domain is Azure staging domain.
        /// </summary>
        /// <param name="domain">Domain to check.</param>
        public static bool IsAzureStagingDomain(string domain)
        {
            domain = domain.ToLowerCSafe();

            // Domain has to be .cloudapp.net
            if (domain.EndsWithCSafe(".cloudapp.net"))
            {
                // Get only third level domain
                domain = domain.Substring(0, domain.IndexOfCSafe('.'));

                // Check if third level is 32 characters long - length of deployment id
                if (domain.Length == 32)
                {
                    // Test if GUID has valid format
                    Regex regex = RegexHelper.GetRegex(@"^[0-9a-f]{32}$");
                    if (regex.IsMatch(domain))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Returns URL ending with slash and starting with http:// if no protocol is given.
        /// </summary>
        /// <param name="url">Input URL.</param>
        private static string EnsureUrl(string url)
        {
            url = url.ToLowerCSafe();

            if (!url.EndsWithCSafe("/"))
            {
                url = url + "/";
            }

            url = URLHelper.AddHTTPToUrl(url);
            return url;
        }

        #endregion
    }
}