using System.Web;

using CMS.AmazonStorage;
using CMS.Base;
using CMS.Helpers;
using CMS.IO;
using CMS.Routing.Web;

[assembly: RegisterHttpHandler("CMSPages/GetAmazonFile.aspx", typeof(GetAmazonFileHandler), Order = 1)]

namespace CMS.AmazonStorage
{
    /// <summary>
    /// HTTP handler to serve Amazon S3 files.
    /// </summary>
    internal class GetAmazonFileHandler : AdvancedGetFileHandler
    {
        #region "Properties"

        /// <summary>
        /// Returns IS3ObjectInfoProvider instance.
        /// </summary>
        private IS3ObjectInfoProvider Provider
        {
            get
            {
                return S3ObjectFactory.Provider;
            }
        }


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
        /// Processes the handler request
        /// </summary>
        /// <param name="context">Handler context</param>
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

                // Get file content from Amazon S3
                var S3Storage = S3ObjectFactory.GetInfo(path);

                // Check if blob exists
                if (Provider.ObjectExists(S3Storage))
                {
                    // Clear response.
                    CookieHelper.ClearResponseCookies();
                    Response.Clear();

                    // Set the revalidation
                    SetRevalidation();

                    var lastModified = S3ObjectInfoProvider.GetStringDateTime(S3Storage.GetMetadata(S3ObjectInfoProvider.LAST_WRITE_TIME));
                    var eTag = "\"" + lastModified + "\"";

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

                    WriteFile(path, CacheHelper.CacheImageAllowed(CurrentSiteName, (int)S3Storage.Length));

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