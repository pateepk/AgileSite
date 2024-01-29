using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Caching;
using System.Web.SessionState;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.DeviceProfiles;
using CMS.Helpers;
using CMS.IO;
using CMS.SiteProvider;

namespace CMS.Routing.Web
{
    /// <summary>
    /// Base page for GetFile pages.
    /// </summary>
    public abstract class AdvancedGetFileHandler : IHttpHandler,
        IReadOnlySessionState // When doing some change to IReadOnlySessionState implementation, see FixSessionIdForInProcOptimization (it might become redundant)
    {
        #region "HTTP constants"

        private const string HTTP_HEADER_IF_NONE_MATCH = "If-None-Match";
        private const string HTTP_HEADER_IF_MODIFIED_SINCE = "If-Modified-Since";

        #endregion


        #region "Variables"

        /// <summary>
        /// Indicates whether request is completed
        /// </summary>
        protected bool mCompleted = true;

        /// <summary>
        /// Cache minutes
        /// </summary>
        protected int mCacheMinutes = -1;

        /// <summary>
        /// Client cache minutes
        /// </summary>
        protected int mClientCacheMinutes = -1;

        /// <summary>
        /// Indicates whether client cache should be revalidated
        /// </summary>
        protected bool? mRevalidateClientCache;

        /// <summary>
        /// Width
        /// </summary>
        protected int mWidth = -1;

        /// <summary>
        /// Height
        /// </summary>
        protected int mHeight = -1;

        /// <summary>
        /// Max. side size
        /// </summary>
        protected int mMaxSideSize = -1;

        /// <summary>
        /// Indicates whether resizing should be used for device
        /// </summary>
        protected bool? mResizeToDevice;

        /// <summary>
        /// Regular expression to match the range in HTTP header.
        /// </summary>
        private static Regex mRangeRegExp;


        /// <summary>
        /// List of file extensions for which the resumable downloads are disabled.
        /// </summary>
        private static string mExcludedResumableExtensions;


        /// <summary>
        /// Cache item name for the request.
        /// </summary>
        protected string useCacheItemName = null;


        /// <summary>
        /// If true, the caching is allowed.
        /// </summary>
        protected bool? mAllowCache = null;


        /// <summary>
        /// Indicates if max resize for mobile device is used.
        /// </summary>
        protected bool deviceResizeIsUsed;

        /// <summary>
        /// Indicates if used on live site
        /// </summary>
        private bool? mIsLiveSite;

        private string mWatermark;
        private ImageHelper.ImagePositionEnum? mWatermarkPosition;
        private ResponseDataSender mSender;
        private bool mAcceptRange = true;
        private string mCurrentSiteName;

        #endregion


        #region "Properties"


        /// <summary>
        /// Returns the current context
        /// </summary>
        protected HttpContextBase Context
        {
            get;
            private set;
        }


        /// <summary>
        /// Returns the current response
        /// </summary>
        public HttpResponseBase Response
        {
            get
            {
                return Context.Response;
            }
        }


        /// <summary>
        /// Indicates if handler is reusable.
        /// </summary>
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }


        /// <summary>
        /// If set, watermark image is applied to the image. Name of the watermark image from ~/App_Themes/{theme}/Images/Watermarks
        /// </summary>
        public string Watermark
        {
            get
            {
                return mWatermark ?? (mWatermark = ValidationHelper.GetString(SettingsKeyInfoProvider.GetValue(CurrentSiteName + ".CMSImageWatermark"), ""));
            }
            set
            {
                mWatermark = value;
            }
        }


        /// <summary>
        /// Watermark position.
        /// </summary>
        [DefaultValue(ImageHelper.ImagePositionEnum.BottomRight)]
        public ImageHelper.ImagePositionEnum WatermarkPosition
        {
            get
            {
                if (mWatermarkPosition == null)
                {
                    mWatermarkPosition = ImageHelper.GetPositionEnum(SettingsKeyInfoProvider.GetValue(CurrentSiteName + ".CMSImageWatermarkPosition"));
                }

                return mWatermarkPosition.Value;
            }
            set
            {
                mWatermarkPosition = value;
            }
        }


        /// <summary>
        /// Gets or sets sender object that is used for writing data to the response.
        /// </summary>
        protected ResponseDataSender Sender
        {
            get
            {
                return mSender ?? (mSender = new ResponseDataSender(CMSHttpContext.Current));
            }
            set
            {
                mSender = value;
            }
        }


        /// <summary>
        /// Size of data.
        /// </summary>
        public long DataLength
        {
            get
            {
                return Sender.DataLength;
            }
        }


        /// <summary>
        /// Indicates whether it is range request.
        /// </summary>
        public bool IsRangeRequest
        {
            get
            {
                return Sender.IsRangeRequest;
            }
        }


        /// <summary>
        /// Indicates whether ranges are valid.
        /// TRUE: ranges are valid or request is not range request
        /// FALSE: all other cases
        /// </summary>
        public bool AreRangesValid
        {
            get
            {
                return Sender.AreRangesValid;
            }
        }


        /// <summary>
        /// Indicates whether it is multipart range request.
        /// </summary>
        public bool IsMultipart
        {
            get
            {
                return Sender.IsMultipart;
            }
        }


        /// <summary>
        /// 2D Array in format {{START_RANGE,END_RANGE},{START_RANGE, END_RANGE}}.
        /// </summary>
        public long[,] Ranges
        {
            get
            {
                return Sender.Ranges;
            }
        }


        /// <summary>
        /// When true, the request is completed, when false, the Request.End is called.
        /// </summary>
        protected static bool GetFileEndRequest
        {
            get
            {
                return RequestHelper.GetFileEndRequest;
            }
        }


        /// <summary>
        /// Indicates if resumable downloads should be supported for current file.
        /// </summary>
        protected bool AcceptRange
        {
            get
            {
                return mAcceptRange;
            }
            set
            {
                mAcceptRange = value;
            }
        }


        /// <summary>
        /// Indicates whether range requests are enabled (ex. for resumable downloads). If false, the HTTP Handler
        /// ignores the Range HTTP Header and returns the entire contents.
        /// </summary>
        protected static bool AcceptRanges
        {
            get
            {
                return ResponseDataSender.AcceptRanges;
            }
        }


        /// <summary>
        /// List of file extensions for which the resumable downloads are disabled.
        /// </summary>
        protected static string ExcludedResumableExtensions
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


        /// <summary>
        /// Current site name.
        /// </summary>
        public virtual string CurrentSiteName
        {
            get
            {
                return mCurrentSiteName ?? (mCurrentSiteName = SiteContext.CurrentSiteName);
            }
            set
            {
                mCurrentSiteName = value;
            }
        }


        /// <summary>
        /// Current site.
        /// </summary>
        public virtual SiteInfo CurrentSite
        {
            get
            {
                return SiteContext.CurrentSite;
            }
        }


        /// <summary>
        /// Cache item name for current request.
        /// </summary>
        public virtual string CacheItemName
        {
            get
            {
                return CacheHelper.GetCacheItemName(null, "getfile", CurrentSiteName, CacheHelper.BaseCacheKey, RequestContext.RawURL);
            }
        }


        /// <summary>
        /// Cache minutes.
        /// </summary>
        public virtual int CacheMinutes
        {
            get
            {
                if (mCacheMinutes < 0)
                {
                    mCacheMinutes = CacheHelper.CacheImageMinutes(CurrentSiteName);
                    if (!AllowCache)
                    {
                        mCacheMinutes = 0;
                    }
                }
                return mCacheMinutes;
            }
            set
            {
                mCacheMinutes = value;
            }
        }


        /// <summary>
        /// Cache minutes.
        /// </summary>
        public virtual int ClientCacheMinutes
        {
            get
            {
                if (mClientCacheMinutes < 0)
                {
                    mClientCacheMinutes = CacheHelper.ClientCacheMinutes(CurrentSiteName);
                    if (!AllowCache)
                    {
                        mClientCacheMinutes = 0;
                    }
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
        public virtual bool AllowClientCache
        {
            get
            {
                return CacheHelper.ClientCacheRequested;
            }
        }


        /// <summary>
        /// Cache minutes.
        /// </summary>
        public virtual bool RevalidateClientCache
        {
            get
            {
                if (mRevalidateClientCache == null)
                {
                    mRevalidateClientCache = CacheHelper.RevalidateClientCache(CurrentSiteName);
                }

                return mRevalidateClientCache.Value;
            }
            set
            {
                mRevalidateClientCache = value;
            }
        }


        /// <summary>
        /// Cache item name for the file output data.
        /// </summary>
        public virtual string OutputDataCacheItemName
        {
            get
            {
                return "fileoutputdata|" + CacheHelper.GetCacheItemName(useCacheItemName, GetBaseCacheKey(), SiteContext.CurrentSiteName, RequestContext.RawURL);
            }
        }


        /// <summary>
        /// Image width.
        /// </summary>
        public virtual int Width
        {
            get
            {
                if (mWidth < 0)
                {
                    mWidth = QueryHelper.GetInteger("width", 0);
                }
                return mWidth;
            }
            set
            {
                mWidth = value;
            }
        }


        /// <summary>
        /// Image height.
        /// </summary>
        public virtual int Height
        {
            get
            {
                if (mHeight < 0)
                {
                    mHeight = QueryHelper.GetInteger("height", 0);
                }
                return mHeight;
            }
            set
            {
                mHeight = value;
            }
        }


        /// <summary>
        /// Image maximum side size.
        /// </summary>
        public virtual int MaxSideSize
        {
            get
            {
                if (mMaxSideSize < 0)
                {
                    mMaxSideSize = QueryHelper.GetInteger("maxsidesize", 0);
                }
                return mMaxSideSize;
            }
            set
            {
                mMaxSideSize = value;
            }
        }


        /// <summary>
        /// Indicates if max side size parameter should be changes to device profile dimensions.
        /// </summary>
        public virtual bool ResizeToDevice
        {
            get
            {
                if (!mResizeToDevice.HasValue)
                {
                    ImageResizeEnum resizeEnum = ImageHelper.GetResizeEnum(QueryHelper.GetString("resizemode", "auto"));

                    // Disable device resize if resize mode is not set to auto
                    mResizeToDevice = (resizeEnum == ImageResizeEnum.Auto);
                }

                return mResizeToDevice.Value;
            }
            set
            {
                mResizeToDevice = value;
            }
        }


        /// <summary>
        /// HTTP header entity tag.
        /// </summary>
        public virtual string ETag
        {
            get;
            set;
        }


        /// <summary>
        /// Logs the exceptions caused by the process.
        /// </summary>
        public virtual bool LogExceptions
        {
            get;
            set;
        }


        /// <summary>
        /// Whether to log exception caused by communication problems (e.g. when remote host closes the connection).
        /// Log exceptions has to be set to TRUE.
        /// </summary>
        public virtual bool LogCommunicationExceptions
        {
            get;
            set;
        }


        /// <summary>
        /// The range from HTTP header regular expression.
        /// </summary>
        protected static Regex RangeRegExp
        {
            get
            {
                // Expression groups:                                              (1:rng )
                return mRangeRegExp ?? (mRangeRegExp = RegexHelper.GetRegex("bytes=([0-9]*)-"));
            }
        }


        /// <summary>
        /// Returns true if the process allows cache.
        /// </summary>
        public abstract bool AllowCache
        {
            get;
            set;
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

        #endregion


        #region "Methods"

        /// <summary>
        /// <see cref="AdvancedGetFileHandler"/> is implementation of <see cref="IReadOnlySessionState"/>.
        /// This may cause that session created within request handled by this handler won't be saved due to 
        /// optimization in default <see cref="SessionStateModule"/>. The optimization is trying
        /// to postpone creation of session to the latest request event (<see cref="HttpApplication.ReleaseRequestState"/>.
        ///
        /// <see cref="ResponseDataSender"/> used for data transmition is eagerly flushing response to the client. 
        /// The first flush is sending response headers (in our case without sessionId). In case of not existing session id
        /// in cookies the postponed sessionId creation causes an exception (not affecting current response or CMS system, the
        /// exception is logged to the system log).
        /// 
        /// The optimization is used only if following conditions are satisfied:
        /// <see cref="HttpSessionStateBase.Mode"/> is <see cref="SessionStateMode.InProc"/>
        /// <see cref="ISessionIDManager"/> is <see cref="SessionIDManager"/>
        /// Cookies are used as sessionId storage.
        /// </summary>
        private static void FixSessionIdForInProcOptimization(HttpContextBase context)
        {
            try
            {
                if (context?.Session != null && context.Session.Mode == SessionStateMode.InProc)
                {
#pragma warning disable BH1000 // 'Session.SessionId' should not be used. Use 'SessionHelper.GetSessionID()' instead.
                    // There is not public method for creation of cookie 
                    // so we have to force creation by reading the id from session.
                    var sessionId = context.Session.SessionID;
#pragma warning restore BH1000 // 'Session.SessionId' should not be used. Use 'SessionHelper.GetSessionID()' instead.
                }
            }
            catch
            {
                // General exception catch is used due to possible incomplete implementation of custom session provider.
                // Custom implementation may return an exception for Session, Mode or any other property.
            }
        }


        /// <summary>
        /// Processes the handler request
        /// </summary>
        /// <param name="context">Context</param>
        public void ProcessRequest(HttpContext context)
        {
            Context = new HttpContextWrapper(context);

            DomainPolicy.ApplyForFiles(SiteContext.CurrentSite);

            RequestContext.CurrentStatus = RequestStatusEnum.GetFileHandler;

            ProcessRequestInternal(Context);
        }


        /// <summary>
        /// Processes the handler request
        /// </summary>
        /// <param name="context">Context</param>
        protected abstract void ProcessRequestInternal(HttpContextBase context);


        /// <summary>
        /// Loads the site name from the query string.
        /// </summary>
        protected virtual void LoadSiteName()
        {
            string siteName = QueryHelper.GetString("sitename", string.Empty);
            if (siteName != string.Empty)
            {
                CurrentSiteName = siteName;
            }
        }


        /// <summary>
        /// Gets the cached data for the current request.
        /// </summary>
        /// <returns>Returns null if no data is cached</returns>
        protected virtual byte[] GetCachedOutputData()
        {
            if (!CacheHelper.CacheImageEnabled(CurrentSiteName) || !AllowCache)
            {
                return null;
            }

            // Key is based only on culture, all users have the same data
            string cacheName = OutputDataCacheItemName;
            byte[] data;
            CacheHelper.TryGetItem(cacheName, out data);

            return data;
        }


        /// <summary>
        /// Saves the data for current request to cache.
        /// </summary>
        /// <param name="data">Data to cache</param>
        /// <param name="cd">Cache item dependency</param>
        /// <remarks>If the data is null, nothing is saved to the cache</remarks>
        protected virtual void SaveOutputDataToCache(byte[] data, CMSCacheDependency cd)
        {
            if (data != null)
            {
                if ((CacheMinutes > 0) && CacheHelper.CacheImageAllowed(CurrentSiteName, data.Length))
                {
                    // Key is based only on culture, all users have the same data
                    string cacheName = OutputDataCacheItemName;

                    CacheHelper.Add(cacheName, data, cd, DateTime.Now.AddMinutes(CacheMinutes), Cache.NoSlidingExpiration, CacheHelper.CacheItemPriority);
                }
            }
        }


        /// <summary>
        /// Returns the base cache key (created with all parameters considered to be valid for proper caching).
        /// </summary>
        protected virtual string GetBaseCacheKey()
        {
            HandleDeviceResize();

            string cache = CacheHelper.GetBaseCacheKey(false, true);
            if (deviceResizeIsUsed)
            {
                // Append device profile name to base cache key
                cache = $"{cache}|deviceprofilename|{DeviceContext.CurrentDeviceProfileName}";
            }

            return cache;
        }


        /// <summary>
        /// Changes MaxSideSize to device dimensions if device resizing is enabled.
        /// </summary>
        protected virtual void HandleDeviceResize()
        {
            // Try to match max side size to device profile dimensions
            if (ResizeToDevice)
            {
                MaxSideSize = HandleDeviceDimension(MaxSideSize);
            }
        }


        /// <summary>
        /// Creates the cache dependency from the given keys.
        /// </summary>
        /// <param name="dependencies">Cache keys</param>
        protected virtual CMSCacheDependency GetCacheDependency(ICollection<string> dependencies)
        {
            if (deviceResizeIsUsed)
            {
                dependencies.Add(DeviceProfileInfoProvider.DEVICE_IMAGE_CACHE_KEY);
                dependencies.Add("cms.deviceprofile|byname|" + DeviceContext.CurrentDeviceProfileName);
            }

            return CacheHelper.GetCacheDependency(dependencies);
        }


        /// <summary>
        /// Sets the cache-ability with dependence on connection type
        /// IE browser doesn't support No-Cache if current connection is secured
        /// </summary>
        protected virtual void SetCacheability()
        {
            var cache = HttpContext.Current.Response.Cache;

            cache.SetExpires(DateTime.Now);

            // Set ETag
            cache.SetETag(!String.IsNullOrEmpty(ETag) ? ETag : Guid.NewGuid().ToString());

            cache.SetCacheability(HttpCacheability.Public);

            SetRevalidation();
        }


        /// <summary>
        /// Sets the revalidation of the client caches.
        /// </summary>
        protected virtual void SetRevalidation()
        {
            var cache = HttpContext.Current.Response.Cache;

            cache.SetRevalidation(RevalidateClientCache ? HttpCacheRevalidation.AllCaches : HttpCacheRevalidation.None);
        }


        /// <summary>
        /// Sets the last modified and expires header to the response
        /// </summary>
        /// <param name="lastModified">Last modified date</param>
        /// <param name="publicCache">True if object can be cached by clients and proxies, false if only by clients</param>
        protected virtual void SetTimeStamps(DateTime lastModified, bool publicCache = true)
        {
            DateTime expires = DateTime.Now;

            var cache = HttpContext.Current.Response.Cache;

            // Send last modified header to allow client caching
            cache.SetLastModified(lastModified);

            cache.SetCacheability(publicCache ? HttpCacheability.Public : HttpCacheability.ServerAndPrivate);

            if (AllowClientCache)
            {
                expires = DateTime.Now.AddMinutes(ClientCacheMinutes);
            }

            cache.SetExpires(expires);
        }


        /// <summary>
        /// Sets response header according to file type.
        /// </summary>
        /// <param name="fileName">Name of file (e.g. image.png)</param>
        /// <param name="extension">File extension</param>
        protected virtual void SetDisposition(string fileName, string extension)
        {
            HTTPHelper.SetFileDisposition(fileName, extension);
        }


        /// <summary>
        /// Completes the request.
        /// </summary>
        protected virtual void CompleteRequest()
        {
            if (GetFileEndRequest)
            {
                RequestHelper.CompleteRequest();
            }
            else
            {
                RequestHelper.EndResponse();
            }

            mCompleted = true;
        }


        /// <summary>
        /// Streams the data file to the response.
        /// </summary>
        /// <param name="filepath">File path</param>
        /// <param name="returnOutputData">If true, output data is returned</param>
        /// <returns>Returns streamed binary data if requested by <paramref name="returnOutputData"/>. Binary data are never returned for range requests.</returns>
        protected virtual byte[] WriteFile(string filepath, bool returnOutputData = false)
        {
            FixSessionIdForInProcOptimization(Context);

            SetupSender();
            return Sender.WriteFile(filepath, returnOutputData);
        }


        /// <summary>
        /// Streams the byte array to the response.
        /// </summary>
        /// <param name="data">Data to write</param>
        protected virtual void WriteBytes(byte[] data)
        {
            FixSessionIdForInProcOptimization(Context);

            SetupSender();
            Sender.WriteBytes(data);
        }


        /// <summary>
        /// Parses the range header from the request.
        /// </summary>
        /// <param name="size">Size of data</param>
        /// <param name="currentContext">Current HTTP context</param>
        /// <returns>2D Array in format {{START_RANGE,END_RANGE},{START_RANGE, END_RANGE}}</returns>
        protected virtual long[,] GetRange(long size, HttpContextBase currentContext)
        {
            SetupSender();
            return Sender.GetRange(size, currentContext);
        }


        /// <summary>
        /// Responds with the not modified code.
        /// </summary>
        /// <param name="eTag">Etag for the file</param>
        /// <param name="publicCache">True if object can be cached by clients and proxies, false if only by clients</param>
        protected virtual void RespondNotModified(string eTag, bool publicCache = true)
        {
            HttpResponse response = HttpContext.Current.Response;

            // Set the code to Not modified
            response.StatusCode = (int)HttpStatusCode.NotModified;

            // Set client cache-ability
            response.Cache.SetCacheability(publicCache ? HttpCacheability.Public : HttpCacheability.ServerAndPrivate);

            // Set ETag
            response.Cache.SetETag(eTag);

            // Log request operation
            RequestDebug.LogRequestOperation("304NotModified", eTag, 1);

            CompleteRequest();
        }


        /// <summary>
        /// Responds with 304 Not Modified if ETags match and object has current timestamp.
        /// </summary>
        /// <param name="eTag">Entity tag of object to compare against ETag received in request</param>
        /// <param name="lastModified">Timestamp of last modification to compare against value in request</param>        
        protected virtual bool ETagsMatch(string eTag, DateTime lastModified)
        {
            // Determine the last modified date and ETag sent from the browser
            string currentETag = RequestHelper.GetHeader(HTTP_HEADER_IF_NONE_MATCH, null);
            string ifModifiedString = RequestHelper.GetHeader(HTTP_HEADER_IF_MODIFIED_SINCE, null);
            if ((ifModifiedString != null) && (currentETag == eTag))
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


        /// <summary>
        /// Indicates if file with given extension is excluded from resumable downloads.
        /// </summary>
        /// <param name="extension">File extension</param>
        protected virtual bool IsExtensionExcludedFromRanges(string extension)
        {
            if (extension == null)
            {
                extension = string.Empty;
            }

            extension = extension.ToLowerInvariant().TrimStart('.');
            return ExcludedResumableExtensions.Contains(";" + extension + ";") || ExcludedResumableExtensions.Contains(";." + extension + ";");
        }


        /// <summary>
        /// Handles max side size according to device profile dimensions.
        /// </summary>
        /// <param name="maxSideSize">Max side size</param>
        protected int HandleDeviceDimension(int maxSideSize)
        {
            if (!SettingsKeyInfoProvider.GetBoolValue(SiteContext.CurrentSiteName + ".CMSResizeImagesToDevice"))
            {
                return maxSideSize;
            }

            DeviceProfileInfo profileInfo = DeviceContext.CurrentDeviceProfile;
            if (profileInfo == null)
            {
                return maxSideSize;
            }

            int maxDeviceSize = Math.Max(profileInfo.ProfilePreviewWidth, profileInfo.ProfilePreviewHeight);
            if ((maxDeviceSize > 0) && ((maxSideSize == 0) || (maxDeviceSize < maxSideSize)))
            {
                deviceResizeIsUsed = true;

                return maxDeviceSize;
            }

            return maxSideSize;
        }


        /// <summary>
        /// Setups sender according to current properties's values.
        /// </summary>
        private void SetupSender()
        {
            Sender.AcceptRange = AcceptRange;
            Sender.LogCommunicationExceptions = LogCommunicationExceptions;
            Sender.LogExceptions = LogExceptions;
        }


        /// <summary>
        /// Sets content type of the response based on file MIME type
        /// </summary>
        /// <param name="mimeType">MIME type</param>
        protected void SetResponseContentType(string mimeType)
        {
            Response.ContentType = mimeType;
        }

        #endregion
    }
}