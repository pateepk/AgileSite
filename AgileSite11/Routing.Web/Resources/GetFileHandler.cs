using System;
using System.Net;
using System.Threading;
using System.Web;

using CMS.Core;
using CMS.Helpers;
using CMS.Base;

namespace CMS.Routing.Web
{
    /// <summary>
    /// Base class for GetFile handlers. Handles client caching and range requests.
    /// </summary>
    public abstract class GetFileHandler : IHttpHandler
    {
        #region "HTTP constants"

        private const string HTTP_HEADER_IF_NONE_MATCH = "If-None-Match";
        private const string HTTP_HEADER_IF_MODIFIED_SINCE = "If-Modified-Since";

        #endregion


        #region "Variables"

        private int mClientCacheMinutes = -1;
        private bool? mRevalidateClientCache;
        private bool? mIsLiveSite;
        private static string mExcludedResumableExtensions;

        #endregion


        #region "Properties"

        /// <summary>
        /// Indicates if handler is reusable.
        /// </summary>
        public bool IsReusable => false;


        /// <summary>
        /// Instance of the sender object that is used for writing data to the response.
        /// </summary>
        protected ResponseDataSender Sender
        {
            get;
            set;
        }


        /// <summary>
        /// Client cache minutes.
        /// </summary>
        protected virtual int ClientCacheMinutes
        {
            get
            {
                if (mClientCacheMinutes < 0)
                {
                    mClientCacheMinutes = CacheHelper.ClientCacheMinutes(Service.Resolve<ISiteService>().CurrentSite.SiteName);
                }
                return mClientCacheMinutes;
            }
            set
            {
                mClientCacheMinutes = value;
            }
        }


        /// <summary>
        /// Returns true if client cache is allowed for the current request.
        /// </summary>
        protected virtual bool AllowClientCache
        {
            get
            {
                return CacheHelper.ClientCacheRequested;
            }
        }


        /// <summary>
        /// Indicates if client cache should be revalidated.
        /// </summary>
        protected virtual bool RevalidateClientCache
        {
            get
            {
                if (!mRevalidateClientCache.HasValue)
                {
                    mRevalidateClientCache = CacheHelper.RevalidateClientCache(Service.Resolve<ISiteService>().CurrentSite.SiteName);
                }

                return mRevalidateClientCache.Value;
            }
            set
            {
                mRevalidateClientCache = value;
            }
        }


        /// <summary>
        /// Indicates if live site mode.
        /// </summary>
        protected bool IsLiveSite
        {
            get
            {
                if (mIsLiveSite == null)
                {
                    mIsLiveSite = Service.Resolve<ISiteService>().IsLiveSite;
                }
                return mIsLiveSite.Value;
            }
        }

        /// <summary>
        /// When true, the request is completed, when false, the Request.End is called.
        /// </summary>
        private bool GetFileEndRequest
        {
            get
            {
                return RequestHelper.GetFileEndRequest;
            }
        }


        /// <summary>
        /// List of file extensions for which the resumable downloads are disabled.
        /// </summary>
        private static string ExcludedResumableExtensions
        {
            get
            {
                if (mExcludedResumableExtensions == null)
                {
                    string valueToSet = ValidationHelper.GetString(SettingsHelper.AppSettings["CMSGetFileExcludedResumableExtensions"], string.Empty);
                    valueToSet = ";" + valueToSet.ToLowerInvariant().Trim(';') + ";";
                    mExcludedResumableExtensions = valueToSet;
                }

                return mExcludedResumableExtensions;
            }
        }

        #endregion


        #region "Current request properties"

        /// <summary>
        ///  Current HTTP context.
        /// </summary>
        protected HttpContextBase Context
        {
            get;
            private set;
        }


        /// <summary>
        /// Current HTTP response.
        /// </summary>
        protected HttpResponseBase Response
        {
            get
            {
                return Context.Response;
            }
        }

        #endregion


        #region "Lifecycle methods"

        /// <summary>
        /// Process the request.
        /// </summary>
        /// <param name="context">Current HTTP context.</param>
        public void ProcessRequest(HttpContext context)
        {
            Context = new HttpContextWrapper(context);
            Sender = new ResponseDataSender(Context);
            try
            {
                ProcessRequestInternal(context);
            }
            catch (ThreadAbortException)
            {
                Thread.ResetAbort();
            }
        }


        /// <summary>
        /// Process the request.
        /// </summary>
        /// <param name="context">Current HTTP context.</param>
        protected abstract void ProcessRequestInternal(HttpContext context);

        #endregion


        #region "Protected methods"

        /// <summary>
        /// Responds HTTP code 404 Not Found.
        /// </summary>
        protected virtual void FileNotFound()
        {
            RequestHelper.Respond404();
        }


        /// <summary>
        /// Completes the request.
        /// </summary>
        protected void CompleteRequest()
        {
            if (GetFileEndRequest)
            {
                RequestHelper.CompleteRequest();
            }
            else
            {
                RequestHelper.EndResponse();
            }
        }


        /// <summary>
        /// Sets response headers.
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="extension">File extension</param>
        /// <param name="mimeType">File MIME type</param>
        protected void SetResponseHeaders(string fileName, string extension, string mimeType)
        {
            CookieHelper.ClearResponseCookies();

            if (extension == null)
            {
                extension = string.Empty;
            }

            HTTPHelper.SetFileDisposition(fileName, extension);
            Response.ContentType = mimeType;

            extension = extension.ToLowerInvariant().TrimStart('.');
            Sender.AcceptRange = !(ExcludedResumableExtensions.Contains(";" + extension + ";") || ExcludedResumableExtensions.Contains(";." + extension + ";"));
        }


        /// <summary>
        /// Sets client cache headers. Responds HTTP code 304 Not Modified and returns true when requested file is in client's cache.
        /// </summary>
        /// <param name="eTag">Entity tag of object to compare against ETag received in request</param>
        /// <param name="lastModified">Timestamp of the last modification to compare against value in request</param>        
        /// <param name="publicCache">Indicates whether response can be cached by public cache systems or not</param>
        /// <returns>Returns true when requested file is in client's cache, false otherwise.</returns>
        protected bool HandleClientCache(string eTag, DateTime lastModified, bool publicCache)
        {
            if (String.IsNullOrEmpty(eTag))
            {
                return false;
            }

            bool useClientCache = (AllowClientCache && (ClientCacheMinutes > 0) && IsLiveSite);
            DateTime expires = useClientCache ? DateTime.Now.AddMinutes(ClientCacheMinutes) : DateTime.Now;
            SetClientCache(eTag, lastModified, expires, publicCache);

            if (useClientCache && ETagsMatch(eTag, lastModified))
            {
                RequestDebug.LogRequestOperation("304NotModified", eTag, 1);
                RespondNotModified();

                return true;
            }

            return false;
        }


        /// <summary>
        /// Streams the byte array to the response.
        /// </summary>
        /// <param name="data">Data to write</param>
        protected void WriteBytes(byte[] data)
        {
            Sender.WriteBytes(data);
        }


        /// <summary>
        /// Responds HTTP code 304 Not Modified.
        /// </summary>
        protected virtual void RespondNotModified()
        {
            Response.StatusCode = (int)HttpStatusCode.NotModified;
            CompleteRequest();
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Sets client cache headers.
        /// </summary>
        /// <param name="eTag">Entity tag of object to compare against ETag received in request</param>
        /// <param name="lastModified">Timestamp of the last modification to compare against value in request</param>        
        /// <param name="expires">Date and time when client cache's entry expires</param>
        /// <param name="publicCache">Indicates whether response can be cached by public cache systems or not</param>
        private void SetClientCache(string eTag, DateTime lastModified, DateTime expires, bool publicCache)
        {
            Response.Cache.SetLastModified(lastModified);
            Response.Cache.SetExpires(expires);
            Response.Cache.SetETag(eTag);
            Response.Cache.SetCacheability(publicCache ? HttpCacheability.Public : HttpCacheability.ServerAndPrivate);
            Response.Cache.SetOmitVaryStar(true);
            Response.Cache.SetRevalidation(RevalidateClientCache ? HttpCacheRevalidation.AllCaches : HttpCacheRevalidation.None);
        }


        /// <summary>
        /// Checks if given and requested ETags match and object has current timestamp.
        /// </summary>
        /// <param name="etag">Entity tag of object to compare against ETag received in request</param>
        /// <param name="lastModified">Timestamp of last modification to compare against value in request</param>        
        private bool ETagsMatch(string etag, DateTime lastModified)
        {
            // Determine the last modified date and etag sent from the browser
            string currentETag = RequestHelper.GetHeader(HTTP_HEADER_IF_NONE_MATCH, null);
            string ifModifiedString = RequestHelper.GetHeader(HTTP_HEADER_IF_MODIFIED_SINCE, null);
            if ((ifModifiedString != null) && (currentETag == etag))
            {
                // IE-browsers send something like this:
                // If-Modified-Since: Wed, 19 Jul 2006 11:19:59 GMT; length=3350
                DateTime ifModified;
                if (DateTime.TryParse(ifModifiedString.Split(";".ToCharArray())[0], out ifModified))
                {
                    // If not changed, let the browser use the cached data
                    if (lastModified.ToUniversalTime() <= ifModified.ToUniversalTime().AddSeconds(1))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        #endregion
    }
}