using System;

using CMS.Helpers;
using CMS.Routing.Web;
using CMS.SharePoint;

using IOExceptions = System.IO;

[assembly: RegisterHttpHandler("CMSPages/GetLocalSharePointFile.ashx", typeof(GetLocalSharePointFileHandler), Order = 1)]

namespace CMS.SharePoint
{
    /// <summary>
    /// Handler that serves SharePoint files from local storage.
    /// </summary>
    internal class GetLocalSharePointFileHandler : AbstractGetSharePointFileHandler
    {
        #region "Contsants"

        // Required parameter names
        private const string FILE_ID_PARAMETER = "fileid";
        private const string FORCE_DOWNLOAD_PARAMETER = "forcedownload";

        #endregion


        #region "Fields"

        private int mFileID;
        private string mCacheItemName;

        #endregion


        #region "Properties"

        /// <summary>
        /// SharePointFileInfo identifier.
        /// </summary>
        protected virtual int FileID
        {
            get
            {
                if (mFileID == 0)
                {
                    mFileID = QueryHelper.GetInteger(FILE_ID_PARAMETER, -1);
                }
                return mFileID;
            }
        }


        /// <summary>
        /// Indicates whether file forced to be saved on client
        /// </summary>
        protected override bool ForceDownload
        {
            get
            {
                return QueryHelper.GetBoolean(FORCE_DOWNLOAD_PARAMETER, false);
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
                    mCacheItemName = "local_" + FileID;
                }

                return mCacheItemName;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns a URL that will return a sharepoint file upon GET request.
        /// </summary>
        /// <param name="localSharePointFileHandlerUrl">Relative or absolute URL of the handler's ASHX file</param>
        /// <param name="sharePointFileID">ID if the sharepoint file to return</param>
        /// <param name="forceDownload">Indicates whether you want to force user to download the file upon GET request.</param>
        /// <returns>Returns a URL that will return a sharepoint file upon GET request.</returns>
        public static string GetSharePointFileUrl(string localSharePointFileHandlerUrl, int sharePointFileID, bool forceDownload = false)
        {
            string queryString = QueryHelper.BuildQueryWithHash(FILE_ID_PARAMETER,sharePointFileID.ToString(), FORCE_DOWNLOAD_PARAMETER, forceDownload.ToString());
            string url = URLHelper.RemoveQuery(localSharePointFileHandlerUrl);
            url = URLHelper.AppendQuery(url, queryString);

            return url;
        }


        /// <summary>
        /// Gets the requested SharePoint file from local storage.
        /// </summary>
        /// <returns>Requested SharePoint file.</returns>
        /// <exception cref="IOExceptions.FileNotFoundException">Thrown when file was not found.</exception>
        /// <exception cref="Exception">Thrown in case of unexpected error.</exception>
        protected override ISharePointFile GetSharePointFile()
        {
            var file = SharePointFileInfoProvider.GetSharePointFileInfo(FileID, true);
            if (file == null)
            {
                throw new IOExceptions.FileNotFoundException(String.Format("The requested SharePoint file with ID '{0}' was not found.", FileID));
            }

            return file.ToSharePointFile();
        }


        /// <summary>
        /// Validates the request.
        /// Returns true on success.
        /// </summary>
        protected override bool ValidateRequest()
        {
            return (base.ValidateRequest() && (FileID > 0));
        }

        #endregion
    }
}
