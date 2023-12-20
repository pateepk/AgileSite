using System.Web;

using CMS.AzureStorage;
using CMS.Base;
using CMS.Helpers;
using CMS.IO;
using CMS.Routing.Web;

[assembly: RegisterHttpHandler("CMSPages/GetAzureFile.aspx", typeof(GetAzureFileHandler), Order = 1)]

namespace CMS.AzureStorage
{
    /// <summary>
    /// HTTP handler to serve Azure files.
    /// </summary>
    internal class GetAzureFileHandler : AdvancedGetFileHandler
    {
        #region "Properties"

        /// <summary>
        /// Gets or sets whether cache is allowed. By default cache is allowed on live site.
        /// </summary>
        public override bool AllowCache
        {
            get
            {
                if (mAllowCache == null)
                {
                    mAllowCache = IsLiveSite;
                }

                return mAllowCache.Value;
            }
            set
            {
                mAllowCache = value;
            }
        }

        #endregion


        #region "Page events"

        /// <summary>
        /// Processes the incoming HTTP request that and returns the specified azure file.
        /// </summary>
        /// <param name="context">An HTTPContext object that provides references to the intrinsic server objects used to service HTTP requests</param>
        protected override void ProcessRequestInternal(HttpContextBase context)
        {
            var hash = QueryHelper.GetString("hash", string.Empty);
            var path = QueryHelper.GetString("path", string.Empty);

            // Validate hash
            var settings = new HashSettings("")
            {
                Redirect = false
            };

            if (ValidationHelper.ValidateHash("?path=" + URLHelper.EscapeSpecialCharacters(path), hash, settings))
            {
                if (path.StartsWithCSafe("~"))
                {
                    path = context.Server.MapPath(path);
                }

                // Get file content from blob
                var blob = new BlobInfo(path);

                // Check if blob exists
                if (BlobInfoProvider.BlobExists(blob))
                {
                    // Clear response.
                    CookieHelper.ClearResponseCookies();
                    Response.Clear();

                    // Set the revalidation
                    SetRevalidation();

                    var eTag = blob.ETag;
                    var lastModified = ValidationHelper.GetDateTime(blob.GetMetadata(ContainerInfoProvider.LAST_WRITE_TIME), DateTimeHelper.ZERO_TIME);

                    var extension = Path.GetExtension(path);
                    var mimeType = MimeTypeHelper.GetMimetype(extension);
                    SetResponseContentType(mimeType);

                    // Client caching - only on the live site
                    if (AllowCache && AllowClientCache && ETagsMatch(eTag, lastModified))
                    {
                        // Set the file time stamps to allow client caching
                        SetTimeStamps(lastModified);

                        RespondNotModified(eTag);
                        return;
                    }

                    SetDisposition(Path.GetFileName(path), Path.GetExtension(path));

                    // Setup Etag property
                    ETag = eTag;

                    if (AllowCache)
                    {
                        // Set the file time stamps to allow client caching
                        SetTimeStamps(lastModified);
                        Response.Cache.SetETag(eTag);
                    }
                    else
                    {
                        SetCacheability();
                    }

                    WriteFile(path, CacheHelper.CacheImageAllowed(CurrentSiteName, (int)blob.Length));

                    CompleteRequest();
                }
                else
                {
                    RequestHelper.Respond404();
                }
            }
            else
            {
                RequestHelper.Respond403();
            }
        }

        #endregion
    }
}