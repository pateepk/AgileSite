using System;
using System.Collections.Generic;
using System.Data;
using System.Web;

using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.DocumentEngine.Internal;
using CMS.Helpers;
using CMS.IO;
using CMS.Localization;
using CMS.Membership;
using CMS.PortalEngine;
using CMS.Routing.Web;
using CMS.SiteProvider;

[assembly: RegisterHttpHandler("CMSPages/GetFile.aspx", typeof(GetAttachmentHandler), Order = 1)]
[assembly: RegisterHttpHandler("getfile/{nodeguid:guid}/{filename}", typeof(GetAttachmentHandler), Order = 2)]
[assembly: RegisterHttpHandler("cms/getfile/{nodeguid:guid}/{filename}", typeof(GetAttachmentHandler), Order = 3)]
[assembly: RegisterHttpHandler("getimage/{guid:guid}/{filename}", typeof(GetAttachmentHandler), Order = 4)]
[assembly: RegisterHttpHandler("getattachment/{guid:guid}/{filename}", typeof(GetAttachmentHandler), Order = 5)]
[assembly: RegisterHttpHandler("cms/getattachment/{guid:guid}/{filename}", typeof(GetAttachmentHandler), Order = 6)]
[assembly: RegisterHttpHandler("getattachment/{*pathandfilename}", typeof(GetAttachmentHandler), Order = 7)]
[assembly: RegisterHttpHandler("cms/getattachment/{*pathandfilename}", typeof(GetAttachmentHandler), Order = 8)]

[assembly: RegisterImplementation(typeof(IGetAttachmentHandler), typeof(GetAttachmentHandler), Priority = CMS.Core.RegistrationPriority.SystemDefault, Lifestyle = Lifestyle.Transient)]

namespace CMS.DocumentEngine
{
    /// <summary>
    /// HTTP handler to get document attachments
    /// </summary>
    internal class GetAttachmentHandler : AdvancedGetFileHandler, IGetAttachmentHandler
    {
        #region "Advanced settings"

        /// <summary>
        /// Sets to false to disable the client caching.
        /// </summary>
        private bool useClientCache = true;


        /// <summary>
        /// Sets to 0 if you do not wish to cache large files.
        /// </summary>
        private int largeFilesCacheMinutes = 1;

        #endregion


        #region "Variables"

        private CMSOutputAttachment outputAttachment;

        private TreeProvider mTreeProvider;

        private TreeNode node;
        private PageInfo pi;

        private int? mVersionHistoryID;
        private bool mIsLatestVersion;
        private Guid guid = Guid.Empty;
        private string mCulture;
        private Guid nodeGuid = Guid.Empty;

        private string aliasPath;
        private string fileName;

        private int latestForDocumentId;
        private int latestForHistoryId;

        private bool allowLatestVersion;

        /// <summary>
        /// View mode of the current page
        /// </summary>
        protected ViewModeEnum mViewMode = ViewModeEnum.Unknown;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets the language for current file.
        /// </summary>
        public string CultureCode
        {
            get
            {
                if (mCulture == null)
                {
                    string culture = QueryHelper.GetString(URLHelper.LanguageParameterName, LocalizationContext.PreferredCultureCode);
                    if (!CultureSiteInfoProvider.IsCultureAllowed(culture, CurrentSiteName))
                    {
                        culture = LocalizationContext.PreferredCultureCode;
                    }

                    mCulture = culture;
                }

                return mCulture;
            }
        }


        /// <summary>
        /// Tree provider.
        /// </summary>
        public TreeProvider TreeProvider
        {
            get
            {
                return mTreeProvider ?? (mTreeProvider = new TreeProvider());
            }
        }


        /// <summary>
        /// Document version history ID.
        /// </summary>
        public int VersionHistoryID
        {
            get
            {
                if (mVersionHistoryID == null)
                {
                    mVersionHistoryID = QueryHelper.GetInteger("versionhistoryid", 0);
                }
                return mVersionHistoryID.Value;
            }
        }


        /// <summary>
        /// Indicates if the file is latest version or comes from version history.
        /// </summary>
        public bool LatestVersion
        {
            get
            {
                return mIsLatestVersion || (VersionHistoryID > 0);
            }
        }


        /// <summary>
        /// Returns true if the process allows cache.
        /// </summary>
        public override bool AllowCache
        {
            get
            {
                if (mAllowCache == null)
                {
                    // By default, cache for the files is disabled outside of the live site
                    mAllowCache = CacheHelper.AlwaysCacheFiles || IsLiveSite;
                }

                return mAllowCache.Value;
            }
            set
            {
                mAllowCache = value;
            }
        }


        /// <summary>
        /// Cache minutes.
        /// </summary>
        public override int CacheMinutes
        {
            get
            {
                // Use 304 revalidation for non-LiveSite mode, for live side mode keep original values
                if (AllowCache && (PortalContext.ViewMode != ViewModeEnum.LiveSite))
                {
                    return 0;
                }
                return base.CacheMinutes;
            }
            set
            {
                base.CacheMinutes = value;
            }
        }


        /// <summary>
        /// View mode of the current page.
        /// </summary>
        public virtual ViewModeEnum ViewMode
        {
            get
            {
                if (mViewMode == ViewModeEnum.Unknown)
                {
                    mViewMode = PortalContext.ViewMode;
                }

                return mViewMode;
            }
            set
            {
                mViewMode = value;
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
            DebugHelper.SetContext("GetFile");

            // Load the site name
            LoadSiteName();

            // Check the site
            if (CurrentSiteName == "")
            {
                throw new Exception("Site is not running.");
            }

            RaiseProcessRequest();

            ProcessAttachmentCached();

            // Do not cache images in the browser if cache is not allowed
            if (LatestVersion)
            {
                useClientCache = false;
            }

            // Send the data
            SendFile(outputAttachment);

            DebugHelper.ReleaseContext();
        }


        private void ProcessAttachmentCached()
        {
            int cacheMinutes = CacheMinutes;

            // Try to get data from cache
            using (var cs = new CachedSection<CMSOutputAttachment>(ref outputAttachment, cacheMinutes, true, null, "getfile", CurrentSiteName, GetBaseCacheKey(), QueryHelper.GetParameterString()))
            {
                if (cs.LoadData)
                {
                    // Store current value and temporary disable caching
                    bool cached = cs.Cached;
                    cs.Cached = false;

                    // Process the file
                    ProcessAttachment();

                    // Restore cache settings - data were loaded
                    cs.Cached = cached;

                    if (cs.Cached)
                    {
                        SetupCache(cacheMinutes, cs);
                    }

                    cs.Data = outputAttachment;
                }
            }
        }


        private void SetupCache(int cacheMinutes, CachedSection<CMSOutputAttachment> cs)
        {
            var filesLocationType = FileHelper.FilesLocationType(CurrentSiteName);

            // Do not cache if too big file which would be stored in memory
            if ((outputAttachment != null) &&
                (outputAttachment.Attachment != null) &&
                !CacheHelper.CacheImageAllowed(CurrentSiteName, outputAttachment.Attachment.AttachmentSize) &&
                (filesLocationType == FilesLocationTypeEnum.Database))
            {
                cacheMinutes = largeFilesCacheMinutes;
            }

            if (cacheMinutes > 0)
            {
                // Prepare the cache dependency
                CMSCacheDependency cd = null;

                if (outputAttachment != null)
                {
                    var dependencies = new List<string>
                    {
                        "node|" + CurrentSiteName.ToLowerCSafe() + "|" + outputAttachment.AliasPath.ToLowerCSafe()
                    };

                    // Do not cache if too big file which would be stored in memory
                    if (outputAttachment.Attachment != null)
                    {
                        if (!CacheHelper.CacheImageAllowed(CurrentSiteName, outputAttachment.Attachment.AttachmentSize) && (filesLocationType == FilesLocationTypeEnum.Database))
                        {
                            cacheMinutes = largeFilesCacheMinutes;
                        }

                        dependencies.Add("attachment|" + outputAttachment.Attachment.AttachmentGUID.ToString().ToLowerCSafe());
                    }

                    cd = GetCacheDependency(dependencies);
                }

                if (cd == null)
                {
                    cd = GetAttachmentDefaultCacheDependency();
                }

                cs.CacheDependency = cd;
            }

            // Cache the data
            cs.CacheMinutes = cacheMinutes;
        }


        private void RaiseProcessRequest()
        {
            var handler = AttachmentHandlerEvents.ProcessRequest;
            if (!handler.IsBound)
            {
                return;
            }

            var e = new CMSEventArgs();
            handler.StartEvent(e);
        }


        private CMSCacheDependency GetAttachmentDefaultCacheDependency()
        {
            CMSCacheDependency cd = null;

            // Set default dependency
            if (guid != Guid.Empty)
            {
                // By attachment GUID
                cd = CacheHelper.GetCacheDependency(new[]
                {
                    "attachment|" + guid.ToString().ToLowerCSafe()
                });
            }
            else if (nodeGuid != Guid.Empty)
            {
                // By node GUID
                cd = CacheHelper.GetCacheDependency(new[]
                {
                    "nodeguid|" + CurrentSiteName.ToLowerCSafe() + "|" + nodeGuid.ToString().ToLowerCSafe()
                });
            }
            else if (aliasPath != null)
            {
                // By node alias path
                cd = CacheHelper.GetCacheDependency(new[]
                {
                    "node|" + CurrentSiteName.ToLowerCSafe() + "|" + aliasPath.ToLowerCSafe()
                });
            }
            return cd;
        }


        /// <summary>
        /// Sends the given file within response.
        /// </summary>
        /// <param name="attachment">File to send</param>
        protected void SendFile(CMSOutputAttachment attachment)
        {
            // Clear response.
            CookieHelper.ClearResponseCookies();
            Response.Clear();

            // Set the revalidation
            SetRevalidation();

            // Send the file
            if ((attachment != null) && attachment.IsValid)
            {
                // Redirect if the file should be redirected
                if (attachment.RedirectTo != "")
                {
                    RaiseSendFile(attachment);
                    RedirectAttachment(attachment);
                    return;
                }

                // Check authentication if secured file
                var secured = attachment.IsSecured;
                if (secured)
                {
                    PageSecurityHelper.CheckSecured(CurrentSiteName, ViewMode);
                }

                string etag = GetFileETag(attachment);

                // Client caching - only on the live site
                if (useClientCache && AllowCache && AllowClientCache && ETagsMatch(etag, attachment.LastModified))
                {
                    RespondNotModified(attachment, secured, etag);
                    return;
                }

                // If physical file not present, try to load
                if (attachment.PhysicalFile == null)
                {
                    EnsurePhysicalFile(outputAttachment);
                }

                bool cacheOutputData = CheckCacheOutputData(attachment);

                EnsureOutputData(attachment, cacheOutputData);

                // Send the file
                if ((attachment.OutputData != null) || (attachment.PhysicalFile != ""))
                {
                    SetResponseHeaders(attachment, etag);

                    RaiseSendFile(attachment);

                    SendOutputData(attachment, cacheOutputData);
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


        private void SendOutputData(CMSOutputAttachment attachment, bool cacheOutputData)
        {
            // Add the file data
            if ((attachment.PhysicalFile != "") && (attachment.OutputData == null))
            {
                if (!File.Exists(attachment.PhysicalFile))
                {
                    // File doesn't exist
                    RequestHelper.Respond404();
                }
                else
                {
                    // Stream the file from the file system
                    attachment.OutputData = WriteFile(attachment.PhysicalFile, cacheOutputData);
                }
            }
            else
            {
                // Use output data of the file in memory if present
                WriteBytes(attachment.OutputData);
            }
        }


        private void SetResponseHeaders(CMSOutputAttachment attachment, string etag)
        {
            // Set correct response content type
            SetResponseContentType(attachment.MimeType);

            if (attachment.Attachment != null)
            {
                string extension = attachment.Attachment.AttachmentExtension;
                SetDisposition(attachment.Attachment.AttachmentName, extension);

                // Setup Etag property
                ETag = etag;

                // Set if resumable downloads should be supported
                AcceptRange = !IsExtensionExcludedFromRanges(extension);
            }

            if (useClientCache && AllowCache)
            {
                // Set the file time stamps to allow client caching
                SetTimeStamps(attachment.LastModified);

                Response.Cache.SetETag(etag);
            }
            else
            {
                SetCacheability();
            }
        }


        private void EnsureOutputData(CMSOutputAttachment attachment, bool cacheOutputData)
        {
            // Ensure the file data if physical file not present
            if (!attachment.DataLoaded && (attachment.PhysicalFile == ""))
            {
                byte[] cachedData = GetCachedOutputData();
                if (attachment.EnsureData(cachedData))
                {
                    if ((cachedData == null) && cacheOutputData)
                    {
                        SaveOutputDataToCache(attachment.OutputData, GetOutputDataDependency(attachment.Attachment));
                    }
                }
            }
        }


        private bool CheckCacheOutputData(CMSOutputAttachment attachment)
        {
            // If the output data should be cached, return the output data
            bool cacheOutputData = false;
            if (attachment.Attachment != null)
            {
                // Cache data if allowed
                if (!LatestVersion && (CacheMinutes > 0))
                {
                    cacheOutputData = CacheHelper.CacheImageAllowed(CurrentSiteName, attachment.Attachment.AttachmentSize);
                }
            }

            return cacheOutputData;
        }


        private void RespondNotModified(CMSOutputAttachment attachment, bool secured, string etag)
        {
            // Set correct response content type
            SetResponseContentType(attachment.MimeType);

            // Set the file time stamps to allow client caching
            SetTimeStamps(attachment.LastModified, !secured);

            RaiseSendFile(attachment);

            RespondNotModified(etag, !secured);
        }


        private void RedirectAttachment(CMSOutputAttachment attachment)
        {
            if (StorageHelper.IsExternalStorage(attachment.RedirectTo))
            {
                string url = File.GetFileUrl(attachment.RedirectTo, CurrentSiteName);
                if (!string.IsNullOrEmpty(url))
                {
                    URLHelper.RedirectPermanent(url, CurrentSiteName);
                }
            }

            URLHelper.RedirectPermanent(attachment.RedirectTo, CurrentSiteName);
        }


        /// <summary>
        /// Gets the file ETag
        /// </summary>
        /// <param name="attachment">File</param>
        private static string GetFileETag(CMSOutputAttachment attachment)
        {
            // Prepare etag
            string etag = attachment.CultureCode.ToLowerCSafe();
            if (attachment.Attachment != null)
            {
                etag += "|" + attachment.Attachment.AttachmentGUID + "|" + attachment.Attachment.AttachmentLastModified.ToUniversalTime();
            }

            if (attachment.IsSecured)
            {
                // For secured files, add user name to etag
                etag += "|" + RequestContext.UserName;
            }

            etag += "|" + PortalContext.ViewMode;

            // Put etag into ""
            etag = "\"" + etag + "\"";

            return etag;
        }


        /// <summary>
        /// Processes the attachment.
        /// </summary>
        protected void ProcessAttachment()
        {
            outputAttachment = null;

            // If guid given, process the attachment
            guid = QueryHelper.GetGuid("guid", Guid.Empty);
            allowLatestVersion = CheckAllowLatestVersion();

            if (guid != Guid.Empty)
            {
                // Check version
                if (VersionHistoryID > 0)
                {
                    ProcessFile(guid, VersionHistoryID);
                }
                else
                {
                    ProcessFile(guid);
                }
            }
            else
            {
                // Get by node GUID
                nodeGuid = QueryHelper.GetGuid("nodeguid", Guid.Empty);
                if (nodeGuid != Guid.Empty)
                {
                    // If node GUID given, process the file
                    ProcessNode(nodeGuid);
                }
                else
                {
                    // Get by alias path and file name
                    LoadAliasPathAndFileName();

                    if (aliasPath != null)
                    {
                        ProcessNode(aliasPath, fileName);
                    }
                }
            }

            // If chset specified, do not cache
            string chset = QueryHelper.GetString("chset", null);
            if (chset != null)
            {
                mIsLatestVersion = true;
            }
        }


        /// <summary>
        /// Loads the alias path and file name from the request parameters
        /// </summary>
        private void LoadAliasPathAndFileName()
        {
            // Try to get combined path and file name from route
            var pathAndFileName = QueryHelper.GetString("pathandfilename", null);
            if (pathAndFileName != null)
            {
                // Parse the path and file name
                pathAndFileName = pathAndFileName.TrimEnd('/');
                int fileNameIndex = pathAndFileName.LastIndexOf('/');
                if (fileNameIndex >= 0)
                {
                    fileName = pathAndFileName.Substring(fileNameIndex + 1);
                    fileName = RemoveFilesUrlExtension(fileName);

                    aliasPath = "/" + pathAndFileName.Substring(0, fileNameIndex).Trim('/');

                    return;
                }
            }

            // Get individual values from parameters
            aliasPath = QueryHelper.GetString("aliaspath", null);
            fileName = QueryHelper.GetString("filename", null);
        }


        /// <summary>
        /// Removes the friendly file name extension from the given file name
        /// </summary>
        /// <param name="filename">File name</param>
        private string RemoveFilesUrlExtension(string filename)
        {
            // Get the friendly extension
            string friendlyUrlExtension = TreePathUtils.GetFilesUrlExtension();

            // Remove the ASPX extension if not set as a custom
            if (!friendlyUrlExtension.EqualsCSafe(".aspx", true) && filename.EndsWithCSafe(".aspx", true))
            {
                filename = filename.Substring(0, filename.Length - 5);
            }

            // Remove friendly extension from the end
            if ((friendlyUrlExtension != String.Empty) && filename.EndsWithCSafe(friendlyUrlExtension, true))
            {
                filename = filename.Substring(0, filename.Length - friendlyUrlExtension.Length);
            }

            return filename;
        }


        /// <summary>
        /// Processes the specified file and returns the data to the output stream.
        /// </summary>
        /// <param name="attachmentGuid">Attachment guid</param>
        protected void ProcessFile(Guid attachmentGuid)
        {
            byte[] cachedData;
            var requiresData = CheckRequiresData(out cachedData);
            var filesLocationType = FileHelper.FilesLocationType(CurrentSiteName);

            var attachment = GetAttachment(attachmentGuid, requiresData, filesLocationType);

            HandleVariant(ref attachment);

            if (attachment == null)
            {
                return;
            }

            // Temporary attachment is always latest version
            if (attachment.AttachmentFormGUID != Guid.Empty)
            {
                mIsLatestVersion = true;
            }

            // Check if current mimetype is allowed
            if (!CheckRequiredMimeType(attachment))
            {
                return;
            }

            bool checkPublishedFiles = AttachmentInfoProvider.CheckPublishedFiles(CurrentSiteName);
            bool checkFilesPermissions = AttachmentInfoProvider.CheckFilesPermissions(CurrentSiteName);

            // Get the document node
            if ((node == null) && (checkPublishedFiles || checkFilesPermissions))
            {
                LoadNodeByDocumentIdCached(attachment.AttachmentDocumentID);
            }

            var secured = false;

            if ((node != null) && checkFilesPermissions)
            {
                secured = CheckNodeSecurity();
            }

            bool resizeImage = ImageHelper.IsImage(attachment.AttachmentExtension) && AttachmentBinaryHelper.CanResizeImage(attachment, Width, Height, MaxSideSize);

            // If the file should be redirected, redirect the file
            if (!mIsLatestVersion && IsLiveSite && SettingsKeyInfoProvider.GetBoolValue(CurrentSiteName + ".CMSRedirectFilesToDisk"))
            {
                if (filesLocationType != FilesLocationTypeEnum.Database)
                {
                    // If path is valid, redirect
                    var path = GetAttachmentPath(attachment, resizeImage);
                    if (path != null)
                    {
                        // Check if file exists
                        var filePath = Context.Server.MapPath(path);
                        if (File.Exists(filePath))
                        {
                            CreateOutputFromPath(path, attachment, secured);
                        }
                    }
                }
            }

            // Get the data
            if ((outputAttachment == null) || (outputAttachment.Attachment == null))
            {
                outputAttachment = CreateOutputFromAttachment(attachment, null);

                SetOutputAttachmentSize(resizeImage);

                outputAttachment.SiteName = CurrentSiteName;

                // Load the data if required
                if (requiresData)
                {
                    // Try to get the physical file, if not latest version
                    if (!mIsLatestVersion)
                    {
                        EnsurePhysicalFile(outputAttachment);
                    }

                    // Load data if necessary
                    var loadData = string.IsNullOrEmpty(outputAttachment.PhysicalFile);
                    if (loadData)
                    {
                        LoadAttachmentData(attachment);
                    }
                }
                else if (cachedData != null)
                {
                    // Load the cached data if available
                    outputAttachment.OutputData = cachedData;
                }
            }

            if (outputAttachment != null)
            {
                SetOutputData(checkPublishedFiles, secured);
            }
        }


        private static void HandleVariant(ref DocumentAttachment attachment)
        {
            if (attachment == null)
            {
                return;
            }

            // Not permitted to get variant directly, it must be accessed through the main attachment
            if (attachment.IsVariant())
            {
                attachment = null;
            }
            else
            {
                // Get the specific variant
                var variant = QueryHelper.GetString("variant", String.Empty);
                if (!String.IsNullOrEmpty(variant))
                {
                    attachment = attachment.GetVariant(variant);
                }
            }
        }


        private DocumentAttachment GetAttachment(Guid attachmentGuid, bool requiresData, FilesLocationTypeEnum filesLocationType)
        {
            DocumentAttachment attachment;

            if (!IsLiveSite)
            {
                attachment = GetLatestAttachmentVersion(attachmentGuid);
            }
            else
            {
                attachment = (DocumentAttachment)GetPublishedAttachment(attachmentGuid, requiresData, filesLocationType);

                // If attachment not found as published, try to find the latest if allowed
                if (allowLatestVersion && ((attachment == null) || (latestForHistoryId > 0) || (attachment.AttachmentDocumentID == latestForDocumentId)))
                {
                    attachment = GetLatestAttachmentVersion(attachmentGuid);

                    if (attachment == null)
                    {
                        return null;
                    }

                    // If not attachment for the required document, do not return
                    if ((attachment.AttachmentDocumentID != latestForDocumentId) && (latestForHistoryId == 0))
                    {
                        return null;
                    }
                
                    mIsLatestVersion = true;
                }
            }

            return attachment;
        }


        private bool CheckRequiresData(out byte[] cachedData)
        {
            bool requiresData = true;

            // Check if it is necessary to load the file data
            if (useClientCache && IsLiveSite && AllowClientCache)
            {
                // If possibly cached by client, do not load data (may not be sent)
                string ifModifiedString = Context.Request.Headers["If-Modified-Since"];
                if (ifModifiedString != null)
                {
                    requiresData = false;
                }
            }

            // If output data available from cache, do not require loading the data
            cachedData = GetCachedOutputData();
            if (cachedData != null)
            {
                requiresData = false;
            }

            return requiresData;
        }


        private AttachmentInfo GetPublishedAttachment(Guid attachmentGuid, bool requiresData, FilesLocationTypeEnum filesLocationType)
        {
            AttachmentInfo atInfo;

            if (!requiresData || (filesLocationType != FilesLocationTypeEnum.Database))
            {
                // Do not require data from DB - Not necessary or available from file system
                atInfo = AttachmentInfoProvider.GetAttachmentInfoWithoutBinary(attachmentGuid, CurrentSiteName);
            }
            else
            {
                // Require data from DB - Stored in DB
                atInfo = AttachmentInfoProvider.GetAttachmentInfo(attachmentGuid, CurrentSiteName);
            }

            return atInfo;
        }

        private bool CheckNodeSecurity()
        {
            var isSecured = node.IsSecuredNode;
            var secured = isSecured.HasValue && isSecured.Value;

            // Check secured pages
            if (secured)
            {
                PageSecurityHelper.CheckSecuredAreas(CurrentSiteName, false, ViewMode);
            }

            var requireSSL = node.RequiresSSL;
            if (requireSSL.HasValue && (requireSSL.Value == 1))
            {
                PageSecurityHelper.RequestSecurePage(false, requireSSL.Value, ViewMode, CurrentSiteName);
            }

            // Check permissions
            bool checkPermissions = false;
            switch (PageSecurityHelper.CheckPagePermissions(CurrentSiteName))
            {
                case PageLocationEnum.All:
                    checkPermissions = true;
                    break;

                case PageLocationEnum.SecuredAreas:
                    checkPermissions = secured;
                    break;
            }

            // Check the read permission for the page
            if (checkPermissions)
            {
                if (MembershipContext.AuthenticatedUser.IsAuthorizedPerDocument(node, NodePermissionsEnum.Read) == AuthorizationResultEnum.Denied)
                {
                    URLHelper.Redirect(PageSecurityHelper.AccessDeniedPageURL(CurrentSiteName));
                }
            }

            return secured;
        }


        private void CreateOutputFromPath(string path, DocumentAttachment attachment, bool secured)
        {
            outputAttachment = CreateEmptyOutput();
            outputAttachment.IsSecured = secured;
            outputAttachment.RedirectTo = path;
            outputAttachment.Attachment = attachment;
        }


        private string GetAttachmentPath(DocumentAttachment attachment, bool resizeImage)
        {
            string path;

            if (!resizeImage)
            {
                path = AttachmentURLProvider.GetFilePhysicalUrl(CurrentSiteName, attachment.AttachmentGUID.ToString(), attachment.AttachmentExtension);
            }
            else
            {
                int[] newDim = ImageHelper.EnsureImageDimensions(Width, Height, MaxSideSize, attachment.AttachmentImageWidth, attachment.AttachmentImageHeight);
                path = AttachmentURLProvider.GetFilePhysicalUrl(CurrentSiteName, attachment.AttachmentGUID.ToString(), attachment.AttachmentExtension, newDim[0], newDim[1]);
            }

            return path;
        }


        private void LoadAttachmentData(DocumentAttachment attachment)
        {
            if (attachment.AttachmentBinary != null)
            {
                // Load from the attachment
                outputAttachment.LoadData(attachment.AttachmentBinary);
            }
            else
            {
                // Load from the disk
                var data = AttachmentBinaryHelper.GetAttachmentBinary(attachment);
                outputAttachment.LoadData(data);
            }

            // Save data to the cache, if not latest version
            if (!mIsLatestVersion && (CacheMinutes > 0))
            {
                SaveOutputDataToCache(outputAttachment.OutputData, GetOutputDataDependency(outputAttachment.Attachment));
            }
        }


        private void SetOutputAttachmentSize(bool resizeImage)
        {
            outputAttachment.Width = Width;
            outputAttachment.Height = Height;
            outputAttachment.MaxSideSize = MaxSideSize;
            outputAttachment.Resized = resizeImage;
        }


        private void SetOutputData(bool checkPublishedFiles, bool secured)
        {
            outputAttachment.IsSecured = secured;

            if (node == null)
            {
                return;
            }

            // Add node data
            outputAttachment.AliasPath = node.NodeAliasPath;
            outputAttachment.CultureCode = node.DocumentCulture;
            outputAttachment.FileNode = node;

            // Set the file validity
            if (IsLiveSite && !mIsLatestVersion && checkPublishedFiles)
            {
                outputAttachment.ValidFrom = node.DocumentPublishFrom;
                outputAttachment.ValidTo = node.DocumentPublishTo;

                // Set the published flag                   
                outputAttachment.IsPublished = node.IsPublished;
            }
        }


        private DocumentAttachment GetLatestAttachmentVersion(Guid attachmentGuid)
        {
            DocumentAttachment attachment;

            if (node != null)
            {
                attachment = DocumentHelper.GetAttachment(node, attachmentGuid);
            }
            else
            {
                attachment = DocumentHelper.GetAttachment(attachmentGuid, CurrentSiteName);
            }

            return attachment;
        }


        /// <summary>
        /// Checks if attachment mime type is allowed.
        /// </summary>
        /// <param name="attachment">AttachmentInfo</param>
        /// <returns>True if file is allowed, false if not</returns>
        protected bool CheckRequiredMimeType(IAttachment attachment)
        {
            string requiredMimeType = QueryHelper.GetString("requiredtype", String.Empty);

            if (!String.IsNullOrEmpty(requiredMimeType))
            {
                string[] mimetype = attachment.AttachmentMimeType.Split('/');
                string requiredMimeSubType = QueryHelper.GetString("requiredsubtype", String.Empty);

                // Check if  mimetype match these parameters
                if ((mimetype[0] != requiredMimeType) || (!String.IsNullOrEmpty(requiredMimeSubType) && (mimetype[1] != requiredMimeSubType)))
                {
                    // Get default image path
                    string defaultImage = QueryHelper.GetString("defaultfilepath", String.Empty);

                    bool isRelativePath = defaultImage.StartsWithCSafe("~") || defaultImage.StartsWithCSafe("/");

                    // If image path is empty or isn't relative
                    if (String.IsNullOrEmpty(defaultImage) || !isRelativePath)
                    {
                        // 404 Page not found
                        return false;
                    }
                    
                    // Redirect to default image
                    URLHelper.Redirect(AdministrationUrlHelper.ResolveImageUrl(defaultImage));
                }
            }

            return true;
        }


        /// <summary>
        /// Processes the specified document node.
        /// </summary>
        /// <param name="currentAliasPath">Alias path</param>
        /// <param name="currentFileName">File name</param>
        protected void ProcessNode(string currentAliasPath, string currentFileName)
        {
            // Load the document node
            if (node == null)
            {
                string className = currentFileName == null ? SystemDocumentTypes.File : null;

                LoadNodeByAliasPathCached(currentAliasPath, className);
            }

            // Process the document
            ProcessNode(node, null, currentFileName);
        }


        /// <summary>
        /// Processes the specified document node.
        /// </summary>
        /// <param name="currentNodeGuid">Node GUID</param>
        protected void ProcessNode(Guid currentNodeGuid)
        {
            // Load the document node
            string columnName = QueryHelper.GetString("columnName", String.Empty);
            if (node == null)
            {
                LoadNodeByGuidCached(currentNodeGuid, columnName);
            }

            // Process the document node
            ProcessNode(node, columnName, null);
        }


        private string GetNodeClassName(Guid currentNodeGuid, string columnName)
        {
            // Get the document
            string className = null;
            if (columnName == "")
            {
                className = SystemDocumentTypes.File;
            }
            else
            {
                // Other document types
                TreeNode srcNode = TreeProvider.SelectSingleNode(currentNodeGuid, CultureCode, CurrentSiteName);
                if (srcNode != null)
                {
                    className = srcNode.NodeClassName;
                }
            }

            return className;
        }


        /// <summary>
        /// Processes the specified document node.
        /// </summary>
        /// <param name="treeNode">Document node to process</param>
        /// <param name="columnName">Column name</param>
        /// <param name="processedFileName">File name</param>
        protected void ProcessNode(TreeNode treeNode, string columnName, string processedFileName)
        {
            if (treeNode == null)
            {
                return;
            }

            // Check if latest or live site version is required
            bool latest = !IsLiveSite;
            if (allowLatestVersion && ((treeNode.DocumentID == latestForDocumentId) || (treeNode.DocumentCheckedOutVersionHistoryID == latestForHistoryId)))
            {
                latest = true;
            }

            // If not published, return no content
            if (!latest && !treeNode.IsPublished)
            {
                CreateNoContentOutput(treeNode);
            }
            else
            {
                EnsureOriginalNodeSiteName(treeNode);

                // Process the node
                // Get from specific column
                if (String.IsNullOrEmpty(columnName) && String.IsNullOrEmpty(processedFileName) && treeNode.IsFile())
                {
                    columnName = "FileAttachment";
                }

                if (!String.IsNullOrEmpty(columnName))
                {
                    // File document type or specified by column
                    var attachmentGuid = ValidationHelper.GetGuid(treeNode.GetValue(columnName), Guid.Empty);
                    if (attachmentGuid != Guid.Empty)
                    {
                        ProcessFile(attachmentGuid);
                    }
                }
                else
                {
                    // Get by file name
                    if (processedFileName == null)
                    {
                        var attachmentGuid = ValidationHelper.GetGuid(treeNode.GetValue("FileAttachment"), Guid.Empty);
                        if (attachmentGuid != Guid.Empty)
                        {
                            ProcessFile(attachmentGuid);
                        }
                    }
                    else
                    {
                        var attachment = GetAttachmentByFileName(treeNode, processedFileName, latest);
                        if (attachment != null)
                        {
                            ProcessFile(attachment.AttachmentGUID);
                        }
                    }
                }
            }
        }


        private void CreateNoContentOutput(TreeNode treeNode)
        {
            outputAttachment = CreateOutputFromAttachment(null, null);
            outputAttachment.AliasPath = treeNode.NodeAliasPath;
            outputAttachment.CultureCode = treeNode.DocumentCulture;
            if (IsLiveSite && AttachmentInfoProvider.CheckPublishedFiles(CurrentSiteName))
            {
                outputAttachment.IsPublished = treeNode.IsPublished;
            }
            outputAttachment.FileNode = treeNode;
            outputAttachment.Height = Height;
            outputAttachment.Width = Width;
            outputAttachment.MaxSideSize = MaxSideSize;
        }


        private void EnsureOriginalNodeSiteName(TreeNode treeNode)
        {
            // Get valid site name if link
            if (treeNode.IsLink)
            {
                var origNode = TreeProvider.GetOriginalNode(treeNode);
                if (origNode != null)
                {
                    var si = SiteInfoProvider.GetSiteInfo(origNode.NodeSiteID);
                    if (si != null)
                    {
                        CurrentSiteName = si.SiteName;
                    }
                }
            }
        }


        private DocumentAttachment GetAttachmentByFileName(TreeNode treeNode, string processedFileName, bool latest)
        {
            // Other document types, get the attachment by file name
            DocumentAttachment attachment;
            if (latest)
            {
                // Not livesite mode - get latest version
                attachment = DocumentHelper.GetAttachment(treeNode, processedFileName, false);
            }
            else
            {
                // Live site mode, get directly from database
                attachment = (DocumentAttachment)AttachmentInfoProvider.GetAttachmentInfo(treeNode.DocumentID, processedFileName, false);
            }

            return attachment;
        }


        /// <summary>
        /// Processes the specified version of the file and returns the data to the output stream.
        /// </summary>
        /// <param name="attachmentGuid">Attachment GUID</param>
        /// <param name="versionHistoryId">Document version history ID</param>
        protected void ProcessFile(Guid attachmentGuid, int versionHistoryId)
        {
            var attachment = GetFile(attachmentGuid, versionHistoryId);

            HandleVariant(ref attachment);

            if (attachment == null)
            {
                return;
            }

            // If attachment is image, try resize
            var binaryData = attachment.AttachmentBinary;
            if (binaryData != null)
            {
                CreateOutputFromBinaryData(attachment, binaryData);
            }

            // Get the file document
            if (node == null)
            {
                node = TreeProvider.SelectSingleDocument(attachment.AttachmentDocumentID, false);
            }

            if (node == null)
            {
                return;
            }

            CheckPageInfoSecurity();

            // Check the permissions for the document
            if ((MembershipContext.AuthenticatedUser.IsAuthorizedPerDocument(node, NodePermissionsEnum.Read) == AuthorizationResultEnum.Allowed) || (node.NodeOwner == MembershipContext.AuthenticatedUser.UserID))
            {
                CreateOutputFromNode();
            }
            else
            {
                outputAttachment = null;
            }
        }


        private void CreateOutputFromNode()
        {
            if (outputAttachment == null)
            {
                outputAttachment = CreateEmptyOutput();
            }

            outputAttachment.AliasPath = node.NodeAliasPath;
            outputAttachment.CultureCode = node.DocumentCulture;
            if (IsLiveSite && AttachmentInfoProvider.CheckPublishedFiles(CurrentSiteName))
            {
                outputAttachment.IsPublished = node.IsPublished;
            }
            outputAttachment.FileNode = node;
        }


        private void CheckPageInfoSecurity()
        {
            // Check secured area
            if (pi == null)
            {
                pi = PageInfoProvider.GetPageInfo(node.NodeSiteName, node.NodeAliasPath, node.DocumentCulture, node.DocumentUrlPath, false);
            }

            if (pi != null)
            {
                PageSecurityHelper.RequestSecurePage(pi, false, ViewMode, CurrentSiteName);
                PageSecurityHelper.CheckSecuredAreas(CurrentSiteName, pi, false, ViewMode);
            }
        }


        private void CreateOutputFromBinaryData(DocumentAttachment attachment, byte[] binaryData)
        {
            string mimetype = null;

            if (ImageHelper.IsImage(attachment.AttachmentExtension))
            {
                if (AttachmentBinaryHelper.CanResizeImage(attachment, Width, Height, MaxSideSize))
                {
                    // Do not search thumbnail on the disk
                    binaryData = AttachmentBinaryHelper.GetImageThumbnailBinary(attachment, Width, Height, MaxSideSize, false);
                    mimetype = "image/jpeg";
                }
            }

            if (binaryData != null)
            {
                outputAttachment = CreateOutputFromAttachment(attachment, binaryData);
            }
            else
            {
                outputAttachment = CreateEmptyOutput();
            }

            outputAttachment.Height = Height;
            outputAttachment.Width = Width;
            outputAttachment.MaxSideSize = MaxSideSize;
            outputAttachment.MimeType = mimetype;
        }


        /// <summary>
        /// Gets the file from version history.
        /// </summary>
        /// <param name="attachmentGuid">Attachment GUID</param>
        /// <param name="versionHistoryId">Version history ID</param>
        protected DocumentAttachment GetFile(Guid attachmentGuid, int versionHistoryId)
        {
            var vm = VersionManager.GetInstance(TreeProvider);

            // Get the attachment version
            var attachment = (DocumentAttachment)vm.GetAttachmentVersion(versionHistoryId, attachmentGuid);
            if (attachment != null)
            {
                attachment.AttachmentVersionHistoryID = versionHistoryId;
            }

            return attachment;
        }


        /// <summary>
        /// Returns the output data dependency based on the given attachment record.
        /// </summary>
        /// <param name="ai">Attachment object</param>
        protected CMSCacheDependency GetOutputDataDependency(IAttachment ai)
        {
            return 
                (ai == null) ? 
                null : 
                CacheHelper.GetCacheDependency(AttachmentInfoProvider.GetDependencyCacheKeys(ai));
        }


        /// <summary>
        /// Returns the cache dependency for the given document node.
        /// </summary>
        /// <param name="document">Document node</param>
        protected CMSCacheDependency GetNodeDependency(TreeNode document)
        {
            var siteName = CurrentSiteName.ToLowerCSafe();

            return CacheHelper.GetCacheDependency(new[]
            {
                CacheHelper.FILENODE_KEY,
                CacheHelper.FILENODE_KEY + "|" + siteName,
                "node|" + siteName + "|" + document.NodeAliasPath.ToLowerCSafe()
            });
        }


        /// <summary>
        /// Ensures the security settings in the given document node.
        /// </summary>
        /// <param name="document">Document node</param>
        protected void EnsureSecuritySettings(TreeNode document)
        {
            if (AttachmentInfoProvider.CheckFilesPermissions(CurrentSiteName))
            {
                // Load secured values
                document.LoadInheritedValues(new[]
                {
                    "IsSecuredNode",
                    "RequiresSSL"
                });
            }
        }


        /// <summary>
        /// Handles the document caching actions.
        /// </summary>
        /// <param name="cs">Cached section</param>
        /// <param name="document">Document node</param>
        protected void CacheNode(CachedSection<TreeNode> cs, TreeNode document)
        {
            if (document != null)
            {
                // Load the security settings
                EnsureSecuritySettings(document);

                // Save to the cache
                if (cs.Cached)
                {
                    cs.CacheDependency = GetNodeDependency(document);
                }
            }
            else
            {
                // Do not cache in case not cached
                cs.CacheMinutes = 0;
            }

            cs.Data = document;
        }


        private void RaiseSendFile(CMSOutputAttachment attachment)
        {
            // GetRange() parses request header and sets 'IsMultipart' and 'IsRangeRequest' properties
            GetRange(100, Context);

            var handler = AttachmentHandlerEvents.SendFile;
            if (!handler.IsBound)
            {
                return;
            }

            var e = new AttachmentSendFileEventArgs(attachment, CurrentSiteName, IsLiveSite, IsMultipart, IsRangeRequest);

            handler.StartEvent(e);
        }


        /// <summary>
        /// Ensures the physical file.
        /// </summary>
        /// <param name="attachment">Output file</param>
        public bool EnsurePhysicalFile(CMSOutputAttachment attachment)
        {
            if (attachment == null)
            {
                return false;
            }

            var filesLocationType = FileHelper.FilesLocationType(CurrentSiteName);

            // Try to link to file system in case of published attachment
            if (String.IsNullOrEmpty(attachment.Watermark) && 
                (attachment.Attachment != null) && 
                (attachment.Attachment.AttachmentID > 0) && 
                (filesLocationType != FilesLocationTypeEnum.Database))
            {
                var filePath = AttachmentBinaryHelper.EnsurePhysicalFile(attachment.Attachment);
                if (filePath != null)
                {
                    if (attachment.Resized)
                    {
                        // If resized, ensure the thumbnail file
                        if (AttachmentBinaryHelper.GenerateThumbnails(attachment.SiteName))
                        {
                            filePath = AttachmentBinaryHelper.EnsureThumbnailFile(attachment.Attachment, Width, Height, MaxSideSize);
                            if (filePath != null)
                            {
                                // Link to the physical file
                                attachment.PhysicalFile = filePath;
                                return true;
                            }
                        }
                    }
                    else
                    {
                        // Link to the physical file
                        attachment.PhysicalFile = filePath;
                        return false;
                    }
                }
            }

            attachment.PhysicalFile = "";
            return false;
        }


        /// <summary>
        /// Returns true if latest version of the document is allowed.
        /// </summary>
        public bool CheckAllowLatestVersion()
        {
            // Check if latest version is required
            latestForDocumentId = QueryHelper.GetInteger("latestfordocid", 0);
            latestForHistoryId = QueryHelper.GetInteger("latestforhistoryid", 0);

            if ((latestForDocumentId > 0) || (latestForHistoryId > 0))
            {
                // Validate the hash
                string hash = QueryHelper.GetString("hash", "");
                string validate = (latestForDocumentId > 0) ? "d" + latestForDocumentId : "h" + latestForHistoryId;

                // Prevent redirection to "Access denied" page if hash isn't valid because the handler manages the situation itself. 
                if (!String.IsNullOrEmpty(hash) && QueryHelper.ValidateHashString(validate, hash, new HashSettings { Redirect = false }))
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Gets the new output file object.
        /// </summary>
        private CMSOutputAttachment CreateEmptyOutput()
        {
            return new CMSOutputAttachment
            {
                Watermark = Watermark,
                WatermarkPosition = WatermarkPosition
            };
        }


        /// <summary>
        /// Gets the new output file object.
        /// </summary>
        /// <param name="ai">AttachmentInfo</param>
        /// <param name="data">Output file data</param>
        private CMSOutputAttachment CreateOutputFromAttachment(DocumentAttachment ai, byte[] data)
        {
            return new CMSOutputAttachment(ai, data)
            {
                Watermark = Watermark,
                WatermarkPosition = WatermarkPosition
            };
        }

        #endregion


        #region "Node loading"

        private void LoadNodeByDocumentIdCached(int documentId)
        {
            // Try to get data from cache
            using (var cs = new CachedSection<TreeNode>(ref node, CacheMinutes, !allowLatestVersion, null, "getfilenodebydocumentid", documentId))
            {
                if (cs.LoadData)
                {
                    LoadNodeByDocumentID(documentId);

                    // Cache the document
                    CacheNode(cs, node);
                }
            }
        }


        private void LoadNodeByDocumentID(int documentId)
        {
            // Get the document
            node = TreeProvider.SelectSingleDocument(documentId, false);
        }


        private void LoadNodeByAliasPathCached(string currentAliasPath, string className)
        {
            // Try to get data from cache
            using (var cs = new CachedSection<TreeNode>(ref node, CacheMinutes, !allowLatestVersion, null, "getfilenodebyaliaspath|", CurrentSiteName, GetBaseCacheKey(), currentAliasPath))
            {
                if (cs.LoadData)
                {
                    // Get the document
                    bool combineWithDefaultCulture = SettingsKeyInfoProvider.GetBoolValue(CurrentSiteName + ".CMSCombineImagesWithDefaultCulture");
                    string culture = CultureCode;

                    // Get the document
                    LoadNodeByAliasPath(currentAliasPath, className, combineWithDefaultCulture, culture);

                    // Try to find node using the document aliases
                    if (node == null)
                    {
                        LoadNodeByDocumentAlias(currentAliasPath, combineWithDefaultCulture);
                    }

                    // Cache the document
                    CacheNode(cs, node);
                }
            }
        }


        private void LoadNodeByDocumentAlias(string currentAliasPath, bool combineWithDefaultCulture)
        {
            var ds =
                DocumentAliasInfoProvider.GetDocumentAliases()
                    .TopN(1)
                    .Columns("AliasNodeID", "AliasCulture")
                    .WhereEquals("AliasURLPath", currentAliasPath)
                    .OrderByDescending("AliasCulture");

            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                DataRow dr = ds.Tables[0].Rows[0];
                int nodeId = (int)dr["AliasNodeID"];
                string nodeCulture = DataHelper.GetStringValue(dr, "AliasCulture", null);

                LoadNodeByNodeId(nodeId, nodeCulture, combineWithDefaultCulture);
            }
        }
        

        private void LoadNodeByNodeId(int nodeId, string culture, bool combineWithDefaultCulture)
        {
            if (!IsLiveSite)
            {
                node = DocumentHelper.GetDocument(nodeId, culture, combineWithDefaultCulture, TreeProvider);
            }
            else
            {
                node = TreeProvider.SelectSingleNode(nodeId, culture, combineWithDefaultCulture);

                // Documents should be combined with default culture
                if ((node != null) && combineWithDefaultCulture && !node.IsPublished)
                {
                    // Try to find published document in default culture
                    string defaultCulture = CultureHelper.GetDefaultCultureCode(CurrentSiteName);
                    TreeNode cultureNode = TreeProvider.SelectSingleNode(nodeId, defaultCulture, false);
                    if ((cultureNode != null) && cultureNode.IsPublished)
                    {
                        node = cultureNode;
                    }
                }
            }
        }


        private void LoadNodeByAliasPath(string currentAliasPath, string className, bool combineWithDefaultCulture, string culture)
        {
            // Get the document data
            if (!IsLiveSite)
            {
                node = DocumentHelper.GetDocument(CurrentSiteName, currentAliasPath, culture, combineWithDefaultCulture, className, null, null, TreeProvider.ALL_LEVELS, false, null, TreeProvider);
            }
            else
            {
                node = TreeProvider.SelectSingleNode(CurrentSiteName, currentAliasPath, culture, combineWithDefaultCulture, className, null, null, TreeProvider.ALL_LEVELS, false);

                // Documents should be combined with default culture
                if ((node != null) && combineWithDefaultCulture && !node.IsPublished)
                {
                    // Try to find published document in default culture
                    string defaultCulture = CultureHelper.GetDefaultCultureCode(CurrentSiteName);
                    TreeNode cultureNode = TreeProvider.SelectSingleNode(CurrentSiteName, currentAliasPath, defaultCulture, false, className, null, null, TreeProvider.ALL_LEVELS, false);
                    if ((cultureNode != null) && cultureNode.IsPublished)
                    {
                        node = cultureNode;
                    }
                }
            }
        }

        
        private void LoadNodeByGuidCached(Guid currentNodeGuid, string columnName)
        {
            // Try to get data from cache
            using (var cs = new CachedSection<TreeNode>(ref node, CacheMinutes, !allowLatestVersion, null, "getfilenodebyguid|", CurrentSiteName, GetBaseCacheKey(), currentNodeGuid))
            {
                if (cs.LoadData)
                {
                    // Get the document
                    bool combineWithDefaultCulture = SettingsKeyInfoProvider.GetBoolValue(CurrentSiteName + ".CMSCombineImagesWithDefaultCulture");
                    string culture = CultureCode;

                    var className = GetNodeClassName(currentNodeGuid, columnName);

                    LoadNodeByGuid(className, currentNodeGuid, culture, combineWithDefaultCulture);

                    // Cache the document
                    CacheNode(cs, node);
                }
            }
        }


        private void LoadNodeByGuid(string className, Guid currentNodeGuid, string culture, bool combineWithDefaultCulture)
        {
            string where = "NodeGUID = '" + currentNodeGuid + "'";

            // Get the document data
            if (!IsLiveSite || allowLatestVersion)
            {
                node = DocumentHelper.GetDocument(CurrentSiteName, null, culture, combineWithDefaultCulture, className, where, null, TreeProvider.ALL_LEVELS, false, null, TreeProvider);
            }
            else
            {
                node = TreeProvider.SelectSingleNode(CurrentSiteName, null, culture, combineWithDefaultCulture, className, where, null, TreeProvider.ALL_LEVELS, false);

                // Documents should be combined with default culture
                if ((node != null) && combineWithDefaultCulture && !node.IsPublished)
                {
                    // Try to find published document in default culture
                    string defaultCulture = CultureHelper.GetDefaultCultureCode(CurrentSiteName);
                    TreeNode cultureNode = TreeProvider.SelectSingleNode(CurrentSiteName, null, defaultCulture, false, className, where, null, TreeProvider.ALL_LEVELS, false);
                    if ((cultureNode != null) && cultureNode.IsPublished)
                    {
                        node = cultureNode;
                    }
                }
            }
        }

        #endregion
    }
}