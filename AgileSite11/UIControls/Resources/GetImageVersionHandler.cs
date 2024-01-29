using System;
using System.Web;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Routing.Web;
using CMS.UIControls;

[assembly: RegisterHttpHandler("CMSPages/GetImageVersion.aspx", typeof(GetImageVersionHandler), Order = 1)]

namespace CMS.UIControls
{
    /// <summary>
    /// Image handler for the image editor file versions
    /// </summary>
    internal class GetImageVersionHandler : AdvancedGetFileHandler
    {
        #region "Variables"

        private TempFileInfo tempFile;

        #endregion


        #region "Properties"

        /// <summary>
        /// Returns false - do not allow cache.
        /// </summary>
        public override bool AllowCache
        {
            get
            {
                return false;
            }
            set
            {
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Processes the handler request
        /// </summary>
        /// <param name="context">Handler context</param>
        protected override void ProcessRequestInternal(HttpContextBase context)
        {
            // Validate the hash
            var settings = new HashSettings
            {
                HashSalt = HashValidationSalts.GET_IMAGE_VERSION
            };

            QueryHelper.ValidateHash("hash", null, settings, true);
            DebugHelper.SetContext("GetImageVersion");

            // Get the parameters
            Guid editorGuid = QueryHelper.GetGuid("editorguid", Guid.Empty);
            int num = QueryHelper.GetInteger("versionnumber", -1);

            // Load the temp file info
            if (num >= 0)
            {
                tempFile = TempFileInfoProvider.GetTempFileInfo(editorGuid, num);
            }
            else
            {
                tempFile = TempFileInfoProvider.GetTempFiles().OrderByDescending("FileNumber").TopN(1).FirstObject;
            }

            // Send the data
            SendFile();

            DebugHelper.ReleaseContext();
        }


        /// <summary>
        /// Sends the given file within response.
        /// </summary>
        protected void SendFile()
        {
            // Clear response.
            CookieHelper.ClearResponseCookies();
            Response.Clear();

            Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);

            if (tempFile != null)
            {
                // Prepare etag
                string etag = "\"" + tempFile.FileID + "\"";

                SetResponseContentType(tempFile.FileMimeType);
                string extension = tempFile.FileExtension;
                SetDisposition(tempFile.FileNumber + extension, extension);

                // Setup Etag property
                ETag = etag;

                // Set if resumable downloads should be supported
                AcceptRange = !IsExtensionExcludedFromRanges(extension);

                // Add the file data
                tempFile.Generalized.EnsureBinaryData();
                WriteBytes(tempFile.FileBinary);
            }
            else
            {
                RequestHelper.Respond404();
            }

            CompleteRequest();
        }

        #endregion
    }
}