using System;
using System.Collections.Generic;
using System.Web;

using CMS.Base;
using CMS.DocumentEngine;
using CMS.Forums;
using CMS.Helpers;
using CMS.IO;
using CMS.Membership;
using CMS.Routing.Web;
using CMS.SiteProvider;

[assembly: RegisterHttpHandler("CMSPages/GetForumAttachment.aspx", typeof(GetForumAttachmentHandler), Order=1)]

namespace CMS.Forums
{
    /// <summary>
    /// Handles requests for forum attachment files.
    /// </summary>
    internal class GetForumAttachmentHandler : AdvancedGetFileHandler
    {
        #region "Advanced settings"

        /// <summary>
        /// Sets to false to disable the client caching.
        /// </summary>
        protected bool useClientCache = true;


        /// <summary>
        /// Sets to 0 if you do not wish to cache large files.
        /// </summary>
        protected int largeFilesCacheMinutes = 1;

        #endregion


        #region "Variables"

        protected CMSOutputForumAttachment outputFile = null;
        protected Guid fileGuid = Guid.Empty;

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
                    // By default, cache for the forum attachments is always enabled (even outside of the live site)
                    mAllowCache = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSAlwaysCacheForumAttachments"], true) || IsLiveSite;
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
            fileGuid = QueryHelper.GetGuid("fileguid", Guid.Empty);

            DebugHelper.SetContext("GetForumAttachment");

            int cacheMinutes = CacheMinutes;

            // Try to get data from cache
            using (var cs = new CachedSection<CMSOutputForumAttachment>(ref outputFile, cacheMinutes, true, null, "getforumattachment", CurrentSiteName, GetBaseCacheKey(), QueryHelper.GetParameterString()))
            {
                if (cs.LoadData)
                {
                    // Disable caching before check permissions
                    cs.CacheMinutes = 0;

                    // Process the file
                    ProcessFile();

                    // Keep original cache minutes if permissions are ok
                    cs.CacheMinutes = cacheMinutes;

                    // Ensure the cache settings
                    if (cs.Cached)
                    {
                        // Prepare the cache dependency
                        CMSCacheDependency cd = null;
                        if (outputFile != null)
                        {
                            if (outputFile.ForumAttachment != null)
                            {
                                cd = CacheHelper.GetCacheDependency(outputFile.ForumAttachment.Generalized.GetCacheDependencies());
                                var filesLocationType = FileHelper.FilesLocationType(CurrentSiteName);

                                // Do not cache if too big file which would be stored in memory
                                if (!CacheHelper.CacheImageAllowed(CurrentSiteName, outputFile.ForumAttachment.AttachmentFileSize) && (filesLocationType == FilesLocationTypeEnum.Database))
                                {
                                    cacheMinutes = largeFilesCacheMinutes;
                                }
                            }
                        }

                        if ((cd == null) && (CurrentSite != null) && (outputFile != null))
                        {
                            // Get the current forum id
                            int forumId = 0;
                            if (outputFile.ForumAttachment != null)
                            {
                                forumId = outputFile.ForumAttachment.AttachmentForumID;
                            }

                            // Set default dependency based on GUID
                            cd = GetCacheDependency(new List<string>
                            {
                                "forumattachment|" + fileGuid.ToString().ToLowerCSafe() + "|" + CurrentSite.SiteID,
                                "forumattachment|" + forumId
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

            // Get the file
            ForumAttachmentInfo fileInfo = ForumAttachmentInfoProvider.GetForumAttachmentInfoWithoutBinary(fileGuid, SiteContext.CurrentSiteName);
            if (fileInfo == null)
            {
                return;
            }

            // Check forum access
            var forum = ForumInfoProvider.GetForumInfo(fileInfo.AttachmentForumID);
            if ((forum == null) || !ForumInfoProvider.IsAuthorizedPerForum(forum.ForumID, forum.ForumGroupID, "AccessToForum", SecurityHelper.GetSecurityAccessEnum(forum.ForumAccess, 6), MembershipContext.AuthenticatedUser))
            {
                // If attachment is not allowed for current user, redirect to the access denied page
                URLHelper.Redirect(PageSecurityHelper.AccessDeniedPageURL(CurrentSiteName));
            }

            bool resizeImage = (ImageHelper.IsMimeImage(fileInfo.AttachmentMimeType) && ForumAttachmentInfoProvider.CanResizeImage(fileInfo, Width, Height, MaxSideSize));

            // Get the data
            if ((outputFile == null) || (outputFile.ForumAttachment == null))
            {
                outputFile = new CMSOutputForumAttachment(fileInfo, fileInfo.AttachmentBinary);
                outputFile.Width = Width;
                outputFile.Height = Height;
                outputFile.MaxSideSize = MaxSideSize;
                outputFile.Resized = resizeImage;
            }
        }


        /// <summary>
        /// Sends the given file within response.
        /// </summary>
        /// <param name="file">File to send</param>
        protected void SendFile(CMSOutputForumAttachment file)
        {
            // Clear response.
            CookieHelper.ClearResponseCookies();
            Response.Clear();

            // Set the revalidation
            SetRevalidation();

            if (file != null)
            {
                // Redirect if the file should be redirected
                if (file.RedirectTo != "")
                {
                    URLHelper.RedirectPermanent(file.RedirectTo, CurrentSiteName);
                }

                // Prepare etag
                string etag = file.LastModified.ToString();

                // Client caching - only on the live site
                if (useClientCache && AllowCache && ETagsMatch(etag, file.LastModified))
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
                            SaveOutputDataToCache(file.OutputData, GetOutputDataDependency(file.ForumAttachment));
                        }
                    }
                }

                // Send the file
                if ((file.OutputData != null) || (file.PhysicalFile != ""))
                {
                    // Set correct response content type
                    SetResponseContentType(file.MimeType);

                    string extension = file.ForumAttachment.AttachmentFileExtension;
                    SetDisposition(file.ForumAttachment.AttachmentFileName, extension);

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
                            if (file.ForumAttachment != null)
                            {
                                cacheOutputData = CacheHelper.CacheImageAllowed(CurrentSiteName, file.ForumAttachment.AttachmentFileSize);
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
        /// <param name="fi">ForumAttachmentInfo object</param>
        protected CMSCacheDependency GetOutputDataDependency(ForumAttachmentInfo fi)
        {
            if (fi == null)
            {
                return null;
            }

            return CacheHelper.GetCacheDependency(fi.Generalized.GetCacheDependencies());
        }


        /// <summary>
        /// Ensures the physical file.
        /// </summary>
        /// <param name="file">Output file</param>
        public bool EnsurePhysicalFile(CMSOutputForumAttachment file)
        {
            if (file == null)
            {
                return false;
            }

            var filesLocationType = FileHelper.FilesLocationType(CurrentSiteName);

            // Try to link to file system
            if ((file.ForumAttachment != null) && (file.ForumAttachment.AttachmentID > 0) && (filesLocationType != FilesLocationTypeEnum.Database))
            {
                string filePath = ForumAttachmentInfoProvider.EnsureAttachmentPhysicalFile(file.ForumAttachment, CurrentSiteName);
                if (filePath != null)
                {
                    if (file.Resized)
                    {
                        // If resized, ensure the thumbnail file
                        if (ForumAttachmentInfoProvider.GenerateThumbnails(CurrentSiteName))
                        {
                            filePath = ForumAttachmentInfoProvider.EnsureThumbnailFile(file.ForumAttachment, Width, Height, MaxSideSize);
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