using System;
using System.Collections.Generic;
using System.Web;

using CMS.Base;
using CMS.Helpers;
using CMS.IO;
using CMS.Membership;
using CMS.Routing.Web;
using CMS.UIControls;

[assembly: RegisterHttpHandler("CMSPages/GetAvatar.aspx", typeof(GetAvatarHandler), Order = 1)]
[assembly: RegisterHttpHandler("getavatar/{avatarguid:guid}/{filename}", typeof(GetAvatarHandler), Order = 2)]

namespace CMS.UIControls
{
    /// <summary>
    /// Handler which provides avatar images.
    /// </summary>
    internal class GetAvatarHandler : AdvancedGetFileHandler
    {
        #region "Advanced settings"

        /// <summary>
        /// Set to false to disable the client caching.
        /// </summary>
        protected bool useClientCache = true;


        /// <summary>
        /// Set to 0 if you do not wish to cache large files.
        /// </summary>
        protected int largeFilesCacheMinutes = 1;

        #endregion


        #region "Variables"

        private CMSOutputAvatar outputFile = null;
        private Guid fileGuid = Guid.Empty;

        #endregion


        #region "Properties"

        /// <summary>
        /// Returns true if the process allows cache.
        /// </summary>
        public override bool AllowCache
        {
            get
            {
                if (mAllowCache == null)
                {
                    // By default, cache for the metafiles is always enabled (even outside of the live site)
                    mAllowCache = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSAlwaysCacheAvatars"], true) || IsLiveSite;
                }

                return mAllowCache.Value;
            }
            set
            {
                mAllowCache = value;
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
            DebugHelper.SetContext("GetAvatar");

            // Prepare the cache key
            fileGuid = QueryHelper.GetGuid("avatarguid", Guid.Empty);

            int cacheMinutes = CacheMinutes;

            // Try to get data from cache
            using (var cs = new CachedSection<CMSOutputAvatar>(ref outputFile, cacheMinutes, true, null, "getavatar", GetBaseCacheKey(), QueryHelper.GetParameterString()))
            {
                if (cs.LoadData)
                {
                    // Process the file
                    ProcessFile();

                    // Ensure the cache settings
                    if (cs.Cached)
                    {
                        // Add cache dependency
                        CMSCacheDependency cd = null;
                        if ((outputFile != null) && (outputFile.Avatar != null))
                        {
                            cd = CacheHelper.GetCacheDependency(outputFile.Avatar.Generalized.GetCacheDependencies());
                            var filesLocationType = FileHelper.FilesLocationType();

                            // Do not cache if too big file which would be stored in memory
                            if (!CacheHelper.CacheImageAllowed(CurrentSiteName, outputFile.Avatar.AvatarFileSize) && (filesLocationType == FilesLocationTypeEnum.Database))
                            {
                                cacheMinutes = largeFilesCacheMinutes;
                            }
                        }

                        if ((cd == null) && (cacheMinutes > 0))
                        {
                            // Set default dependency based on GUID
                            cd = GetCacheDependency(new List<string>
                            {
                                "avatarfile|" + fileGuid.ToString().ToLowerCSafe()
                            });
                        }

                        // Cache the data
                        cs.CacheMinutes = cacheMinutes;
                        cs.CacheDependency = cd;
                    }

                    cs.Data = outputFile;
                }
            }

            // Send the data
            SendFile(outputFile);

            DebugHelper.ReleaseContext();
        }


        /// <summary>
        /// Processes the file.
        /// </summary>
        protected void ProcessFile()
        {
            if (fileGuid == Guid.Empty)
            {
                return;
            }

            // Get the avatar
            var avatar = AvatarInfoProvider.GetAvatarInfoWithoutBinary(fileGuid);
            if (avatar == null)
            {
                return;
            }

            // Get the data
            if ((outputFile == null) || (outputFile.Avatar == null))
            {
                var resizeImage = (ImageHelper.IsImage(avatar.AvatarFileExtension) && AvatarInfoProvider.CanResizeImage(avatar, Width, Height, MaxSideSize));

                outputFile = new CMSOutputAvatar(avatar, null)
                             {
                                 Width = Width,
                                 Height = Height,
                                 MaxSideSize = MaxSideSize,
                                 Resized = resizeImage
                             };
            }
        }


        /// <summary>
        /// Sends the given file within response.
        /// </summary>
        /// <param name="file">File to send</param>
        protected void SendFile(CMSOutputAvatar file)
        {
            // Clear response.
            CookieHelper.ClearResponseCookies();
            Response.Clear();

            // Set the revalidation
            SetRevalidation();

            // Send the file
            if (file != null)
            {
                // Redirect if the file should be redirected
                if (file.RedirectTo != "")
                {
                    URLHelper.RedirectPermanent(file.RedirectTo, CurrentSiteName);
                }

                // Prepare etag
                string etag = file.LastModified.ToString();

                // Client caching
                if (useClientCache && AllowCache && AllowClientCache && ETagsMatch(etag, file.LastModified))
                {
                    // Set correct response content type
                    SetResponseContentType(file.MimeType);

                    SetTimeStamps(file.LastModified);

                    RespondNotModified(etag);
                    return;
                }

                // If physical file not present, try to load
                if (file.PhysicalFile == null)
                {
                    EnsurePhysicalFile(outputFile);
                }

                // Ensure the file data if physical file not present
                if (!file.DataLoaded && (file.PhysicalFile == ""))
                {
                    byte[] cachedData = GetCachedOutputData();
                    if (file.EnsureData(cachedData))
                    {
                        if ((cachedData == null) && (CacheMinutes > 0))
                        {
                            SaveOutputDataToCache(file.OutputData, GetOutputDataDependency(file.Avatar));
                        }
                    }
                }

                // Send the file
                if ((file.OutputData != null) || (file.PhysicalFile != ""))
                {
                    // Set correct response content type
                    SetResponseContentType(file.MimeType);

                    // Prepare response
                    string extension = file.Avatar.AvatarFileExtension;
                    SetDisposition(file.Avatar.AvatarFileName, extension);

                    ETag = etag;
                    // Set if resumable downloads should be supported
                    AcceptRange = !IsExtensionExcludedFromRanges(extension);

                    if (useClientCache && AllowCache)
                    {
                        SetTimeStamps(file.LastModified);

                        Response.Cache.SetETag(etag);
                    }
                    else
                    {
                        SetCacheability();
                    }

                    // Add the file data
                    if ((file.PhysicalFile != "") && (file.OutputData == null))
                    {
                        if (!File.Exists(file.PhysicalFile))
                        {
                            // File doesn't exist
                            RequestHelper.Respond404();
                        }
                        else
                        {
                            // If the output data should be cached, return the output data
                            bool cacheOutputData = false;
                            if (file.Avatar != null)
                            {
                                cacheOutputData = CacheHelper.CacheImageAllowed(CurrentSiteName, file.Avatar.AvatarFileSize);
                            }

                            // Stream the file from the file system
                            file.OutputData = WriteFile(file.PhysicalFile, cacheOutputData);
                        }
                    }
                    else
                    {
                        // Use output data of the file in memory if present
                        WriteBytes(file.OutputData);
                    }
                }
                else
                {
                    RequestHelper.Respond404();
                }
            }
            else
            {
                RequestHelper.Respond404();
            }

            CompleteRequest();
        }


        /// <summary>
        /// Returns the output data dependency based on the given attachment record.
        /// </summary>
        /// <param name="ai">Avatar object</param>
        protected CMSCacheDependency GetOutputDataDependency(AvatarInfo ai)
        {
            if (ai == null)
            {
                return null;
            }

            return CacheHelper.GetCacheDependency(ai.Generalized.GetCacheDependencies());
        }


        /// <summary>
        /// Ensures the physical file.
        /// </summary>
        /// <param name="file">Output file</param>
        public bool EnsurePhysicalFile(CMSOutputAvatar file)
        {
            if (file == null)
            {
                return false;
            }

            var filesLocationType = FileHelper.FilesLocationType();

            // Try to link to file system
            if ((file.Avatar != null) && (file.Avatar.AvatarID > 0) && filesLocationType != FilesLocationTypeEnum.Database)
            {
                string filePath = AvatarInfoProvider.EnsureAvatarPhysicalFile(file.Avatar);
                if (filePath != null)
                {
                    if (file.Resized)
                    {
                        // If resized, ensure the thumbnail file
                        if (AvatarInfoProvider.GenerateThumbnails())
                        {
                            filePath = AvatarInfoProvider.EnsureThumbnailFile(file.Avatar, Width, Height, MaxSideSize);
                            if (filePath != null)
                            {
                                // Link to the physical file
                                file.PhysicalFile = filePath;
                                return true;
                            }
                        }
                    }
                    else
                    {
                        // Link to the physical file
                        file.PhysicalFile = filePath;
                        return false;
                    }
                }
            }

            file.PhysicalFile = "";
            return false;
        }

        #endregion
    }
}