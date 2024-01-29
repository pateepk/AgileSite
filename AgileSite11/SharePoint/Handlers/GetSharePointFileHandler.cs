using System;
using System.Web;

using CMS.Helpers;
using CMS.Routing.Web;
using CMS.SharePoint;
using CMS.SiteProvider;

using IOExceptions = System.IO;

[assembly: RegisterHttpHandler("CMSPages/GetSharePointFile.ashx", typeof(GetSharePointFileHandler), Order = 1)]

namespace CMS.SharePoint
{
    /// <summary>
    /// Handler that serves SharePoint files directly from SharePoint server.
    /// </summary>
    internal class GetSharePointFileHandler : AbstractGetSharePointFileHandler
    {
        #region "Contsants"

        // Required parameter names
        private const string CONNECTION_NAME_PARAMETER = "connectionname";
        private const string FILE_REF_PARAMETER = "fileref";

        #endregion


        #region "Fields"

        private string mFilePath;
        private SharePointConnectionInfo mSharePointConnection;
        private string mCacheItemName;

        #endregion


        #region "Properties"

        /// <summary>
        /// SharePoint file path.
        /// </summary>
        protected virtual string FilePath
        {
            get
            {
                if (mFilePath == null)
                {
                    mFilePath = QueryHelper.GetString(FILE_REF_PARAMETER, String.Empty);
                }
                return mFilePath;
            }
        }


        /// <summary>
        /// SharePoint connection info.
        /// </summary>
        protected virtual SharePointConnectionInfo SharePointConnection
        {
            get
            {
                return mSharePointConnection ?? (mSharePointConnection = SharePointConnectionInfoProvider.GetSharePointConnectionInfo(HttpUtility.UrlDecode(QueryHelper.GetString(CONNECTION_NAME_PARAMETER, String.Empty)), SiteContext.CurrentSiteName));
            }
        }


        /// <summary>
        /// Gets the cahe item name for currently requested file that will be used when composing the cahe key.
        /// </summary>
        protected override string CacheItemName
        {
            get
            {
                if (mCacheItemName == null)
                {
                    mCacheItemName = "sp_" + SharePointConnection.SharePointConnectionID + "_" + FilePath;
                }

                return mCacheItemName;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets the requested SharePoint file from SharePoint server.
        /// </summary>
        /// <returns>Requested SharePoint file.</returns>
        /// <exception cref="IOExceptions.FileNotFoundException">Thrown when file was not found.</exception>
        /// <exception cref="Exception">Thrown in case of unexpected error.</exception>
        protected override ISharePointFile GetSharePointFile()
        {
            var service = GetService(SharePointConnection);

            if (service == null)
            {
                throw new IOExceptions.FileNotFoundException("The requested SharePoint file was not found.");
            }

            return service.GetFile(FilePath);
        }

        
        /// <summary>
        /// Validates the request.
        /// Returns true on success.
        /// </summary>
        protected override bool ValidateRequest()
        {
            return (base.ValidateRequest() && !String.IsNullOrEmpty(FilePath) && (SharePointConnection != null));
        }


        /// <summary>
        /// Gets the necessary service for getting the specified file.
        /// </summary>
        /// <param name="connection">Connection containing information about connection to the SharePoint site on which the file is located.</param>
        /// <exception cref="SharePointServiceFactoryNotSupportedException">Thrown when no service factory for specified SharePoint version found.</exception>
        /// <exception cref="SharePointServiceNotSupportedException">Thrown when IService implementation is not available.</exception>
        /// <exception cref="SharePointConnectionNotSupportedException">Thrown when available service implementation does not support given connectionData.</exception>
        protected virtual ISharePointFileService GetService(SharePointConnectionInfo connection)
        {
            if (connection == null)
            {
                return null;
            }

            ISharePointFileService service = SharePointServices.GetService<ISharePointFileService>(connection.ToSharePointConnectionData());

            return service;
        }

        #endregion
    }
}
