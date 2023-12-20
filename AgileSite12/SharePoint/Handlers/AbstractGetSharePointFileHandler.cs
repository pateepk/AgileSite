using System;
using System.Web;

using CMS.Base;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.SiteProvider;
using CMS.Routing.Web;

using IOExceptions = System.IO;

namespace CMS.SharePoint
{
    /// <summary>
    /// Abstract SharePoint handler that serves SharePoint files.
    /// </summary>
    public abstract class AbstractGetSharePointFileHandler : GetFileHandler
    {
        #region "Contsants"

        /// <summary>
        /// Name of the query parameter for cahe minutes setting.
        /// </summary>
        protected const string CACHE_TIME_PARAMETER = "cache";

        /// <summary>
        /// Name of the query parameter for cahe size limit setting.
        /// </summary>
        protected const string CACHE_SIZE_PARAMETER = "cachesizelimit";

        /// <summary>
        /// Name of the query parameter for image width setting.
        /// </summary>
        protected const string IMAGE_WIDTH_PARAMETER = "width";
        
        /// <summary>
        /// Name of the query parameter for image height setting.
        /// </summary>
        protected const string IMAGE_HEIGHT_PARAMETER = "height";

        /// <summary>
        /// Name of the query parameter for image max side size setting.
        /// </summary>
        protected const string IMAGE_MAXSIDESIZE_PARAMETER = "maxsidesize";

        /// <summary>
        /// Mime type that forces browsers to download the file
        /// </summary>
        private const string FORCE_DOWNLOAD_MIMETYPE = "application/octet-stream";

        // Setting keys
        private const string CACHE_TIME_SETTINGKEY = "CMSSharePointCache";
        private const string CACHE_FILESIZE_SETTINGKEY = "CMSSharePointCacheSizeLimit";

        #endregion


        #region "Fields"

        private int? mCacheFileSizeLimit;
        private int mCacheMinutes = -1;
        private bool? mAllowCache;
        private int mWidth = -1;
        private int mHeight = -1;
        private int mMaxSideSize = -1;

        #endregion


        #region "Abstract properties"

        /// <summary>
        /// Gets the cahe item name for currently requested file that will be used when composing the cahe key.
        /// </summary>
        protected abstract string CacheItemName
        {
            get;
        }

        #endregion


        #region "Properties"

        /// <summary>
        /// Number of minutes the file will be cached. Zero means the file won't be cahed.
        /// </summary>
        protected virtual int CacheMinutes
        {
            get
            {
                if (mCacheMinutes < 0)
                {
                    if (QueryHelper.Contains(CACHE_TIME_PARAMETER))
                    {
                        mCacheMinutes = QueryHelper.GetInteger(CACHE_TIME_PARAMETER, 0);
                    }
                    else
                    {
                        mCacheMinutes = SettingsKeyInfoProvider.GetIntValue(CACHE_TIME_SETTINGKEY, SiteContext.CurrentSiteName);
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
        /// Maximum size of the file that will be cached (bytes).
        /// </summary>
        protected virtual int CacheFileSizeLimit
        {
            get
            {
                if (!mCacheFileSizeLimit.HasValue)
                {
                    if (QueryHelper.Contains(CACHE_TIME_PARAMETER))
                    {
                        mCacheFileSizeLimit = QueryHelper.GetInteger(CACHE_SIZE_PARAMETER, 0);
                    }
                    else
                    {
                        mCacheFileSizeLimit = SettingsKeyInfoProvider.GetIntValue(CACHE_FILESIZE_SETTINGKEY, SiteContext.CurrentSiteName);
                    }
                    mCacheFileSizeLimit = mCacheFileSizeLimit.Value * 1024;
                }
                return mCacheFileSizeLimit.Value;
            }
        }


        /// <summary>
        /// Returns true if the process allows cache.
        /// </summary>
        protected virtual bool AllowCache
        {
            get
            {
                if (mAllowCache == null)
                {
                    // By default, cache for the files is disabled outside of the live site
                    mAllowCache = (CacheMinutes > 0) && (ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSAlwaysCacheSharePointFiles"], false) || IsLiveSite);
                }

                return mAllowCache.Value;
            }
            set
            {
                mAllowCache = value;
            }
        }


        /// <summary>
        /// Indicates whether file forced to be saved on client
        /// </summary>
        protected virtual bool ForceDownload
        {
            get
            {
                return false;
            }
        }


        /// <summary>
        /// Image width.
        /// </summary>
        protected virtual int Width
        {
            get
            {
                if (mWidth < 0)
                {
                    mWidth = QueryHelper.GetInteger(IMAGE_WIDTH_PARAMETER, 0);
                }
                return mWidth;
            }
        }


        /// <summary>
        /// Image height.
        /// </summary>
        protected virtual int Height
        {
            get
            {
                if (mHeight < 0)
                {
                    mHeight = QueryHelper.GetInteger(IMAGE_HEIGHT_PARAMETER, 0);
                }
                return mHeight;
            }
        }


        /// <summary>
        /// Image maximum side size.
        /// </summary>
        protected virtual int MaxSideSize
        {
            get
            {
                if (mMaxSideSize < 0)
                {
                    mMaxSideSize = QueryHelper.GetInteger(IMAGE_MAXSIDESIZE_PARAMETER, 0);
                }
                return mMaxSideSize;
            }
        }

        #endregion


        #region "Abstract methods"

        /// <summary>
        /// Gets the requested SharePoint file.
        /// </summary>
        /// <returns>Requested SharePoint file.</returns>
        /// <exception cref="IOExceptions.FileNotFoundException">Thrown when file was not found.</exception>
        /// <exception cref="Exception">Thrown in case of unexpected error.</exception>
        protected abstract ISharePointFile GetSharePointFile();

        #endregion


        #region "Methods"

        /// <summary>
        /// Process get SharePoint file request.
        /// </summary>
        protected override void ProcessRequestInternal(HttpContext context)
        {
            if (!ValidateRequest())
            {
                FileNotFound();

                return;
            }

            try
            {
                ISharePointFile file;
                if (AllowCache)
                {
                    // Try to get data from cache
                    var cacheSettings = new CacheSettings(CacheMinutes, "getsharepointfile", CacheItemName, Width, Height, MaxSideSize);
                    file = CacheHelper.Cache(cs => LoadFile(cs), cacheSettings);
                }
                else
                {
                    // Retrieve file directly
                    file = LoadFile();
                }

                SendFile(file);
            }
            catch (IOExceptions.FileNotFoundException)
            {
                FileNotFound();
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("AbstractGetSharePointFileHandler", "GetFile", ex, SiteContext.CurrentSiteID, "[GetSharePointFileHandler.ProcessRequestInternal]: Error occured when getting SharePoint file.");
                FileNotFound();
            }

            CompleteRequest();
        }


        /// <summary>
        /// Loads requested SharePoint file.
        /// Loaded file will be cached if it satisfies the cache conditions.
        /// </summary>
        /// <param name="cs">Cache settings that are used if file is cached.</param>
        /// <exception cref="IOExceptions.FileNotFoundException">Thrown when file was not found.</exception>
        /// <exception cref="Exception">Thrown in case of unexpected error.</exception>
        protected virtual ISharePointFile LoadFile(CacheSettings cs)
        {
            cs.BoolCondition = false;
            ISharePointFile file = LoadFile();

            // Check if the file satisfies the conditions to be cached.
            if (AllowCache && ((file.IsLengthSupported && file.Length <= CacheFileSizeLimit) || (!file.IsLengthSupported && file.GetContentBytes() != null && file.GetContentBytes().Length <= CacheFileSizeLimit)))
            {
                cs.BoolCondition = true;
            }

            return file;
        }


        /// <summary>
        /// Loads requested SharePoint file. Image is resized when needed.
        /// </summary>
        /// <exception cref="IOExceptions.FileNotFoundException">Thrown when file was not found.</exception>
        /// <exception cref="Exception">Thrown in case of unexpected error.</exception>
        protected virtual ISharePointFile LoadFile()
        {
            ISharePointFile file = GetSharePointFile();

            // If is image
            if (ImageHelper.IsMimeImage(file.MimeType))
            {
                // Resize image if parameters set
                if ((MaxSideSize > 0) || (Width > 0) || (Height > 0))
                {
                    ImageHelper imgHelper = new ImageHelper(file.GetContentBytes());

                    // Check if image should be resized
                    int[] dim = imgHelper.EnsureImageDimensions(Width, Height, MaxSideSize);
                    if ((dim[0] != imgHelper.ImageWidth) || (dim[1] != imgHelper.ImageHeight))
                    {
                        byte[] imgBytes = imgHelper.GetResizedImageData(dim[0], dim[1], ImageHelper.DefaultQuality);
                        file = new SharePointFile(imgBytes)
                        {
                            Title = file.Title,
                            Extension = file.Extension,
                            MimeType = file.MimeType,
                            Name = file.Name,
                            ServerRelativeUrl = file.ServerRelativeUrl,
                            TimeCreated = file.TimeCreated,
                            TimeLastModified = file.TimeLastModified,
                            ETag = file.ETag
                        };
                    }
                }
            }

            return file;
        }


        /// <summary>
        /// Sends the given file within response.
        /// </summary>
        /// <param name="file">File to send</param>
        protected virtual void SendFile(ISharePointFile file)
        {
            if (file == null)
            {
                FileNotFound();

                return;
            }
            string mimeType = file.MimeType;
            if (ForceDownload)
            {
                mimeType = FORCE_DOWNLOAD_MIMETYPE;
            }
            SetResponseHeaders(file.Name, file.Extension, mimeType);
            if (HandleClientCache(file.ETag, file.TimeLastModified.HasValue ? file.TimeLastModified.Value : DateTimeHelper.ZERO_TIME, false))
            {
                // File is in client's cache
                return;
            }

            // Check if file has any content
            if (file.GetContentBytes() == null)
            {
                FileNotFound();

                return;
            }

            // Use output data of the file in memory if present
            WriteBytes(file.GetContentBytes());
        }


        /// <summary>
        /// Validates the request.
        /// Returns true on success.
        /// </summary>
        protected virtual bool ValidateRequest()
        {
            var settings = new HashSettings("")
            {
                Redirect = false,
            };

            return (QueryHelper.ValidateHash("hash", null, settings) && !String.IsNullOrEmpty(SiteContext.CurrentSiteName));
        }

        #endregion
    }
}
