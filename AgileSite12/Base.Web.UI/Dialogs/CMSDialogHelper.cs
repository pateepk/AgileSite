using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;

using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.DocumentEngine.Internal;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Localization;
using CMS.Membership;
using CMS.SiteProvider;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Class providing helper methods for CMS dialogs.
    /// </summary>
    public class CMSDialogHelper
    {
        #region "Variables"

        private static Regex mGuidReg;

        #endregion


        #region "Properties"

        /// <summary>
        /// GUID regular expresion.
        /// </summary>
        public static Regex GuidReg
        {
            get
            {
                return mGuidReg ?? (mGuidReg = RegexHelper.GetRegex("[a-f0-9]{8}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{12}"));
            }
            set
            {
                mGuidReg = value;
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Returns hashtable parsed from string.
        /// </summary>
        /// <param name="input">Input string in form key1|value1|key2|value2|</param>
        public static Hashtable GetHashTableFromString(string input)
        {
            Hashtable table = new Hashtable();

            if (!String.IsNullOrEmpty(input))
            {
                string[] selection = input.Split('|');

                try
                {
                    for (int i = 0; i < selection.Length; i += 2)
                    {
                        if (selection[i + 1].Contains("%25"))
                        {
                            string selectionValue = HttpUtility.UrlDecode(selection[i + 1]);

                            if (ValidationHelper.IsURL(selectionValue))
                            {
                                table.Add(selection[i].ToLowerInvariant(), selectionValue.Contains("%25") ? HttpUtility.UrlDecode(selectionValue) : selectionValue);
                            }
                            else
                            {
                                // This is "hack" due to backward compatibility with  encoded percentage 
                                selectionValue = selectionValue.Replace("%25", "%");
                                table.Add(selection[i].ToLowerInvariant(), selectionValue);
                            }
                        }
                        else
                        {
                            table.Add(selection[i].ToLowerInvariant(), HttpUtility.UrlDecode(selection[i + 1]));
                        }
                    }
                }
                catch
                {
                }
            }

            return table;
        }


        /// <summary>
        /// Returns array list from input string.
        /// </summary>
        /// <param name="input">Input string contains values separated with '|' (value1|value2|...)</param>
        public static ArrayList GetListFromString(string input)
        {
            ArrayList list = new ArrayList();

            if (!String.IsNullOrEmpty(input))
            {
                try
                {
                    string[] selection = input.Split('|');
                    foreach (string t in selection)
                    {
                        list.Add(Uri.UnescapeDataString(t));
                    }
                }
                catch
                {
                }
            }

            return list;
        }


        /// <summary>
        /// Gets thumbnail preview image dimensions.
        /// </summary>
        /// <param name="origHeight">Original image width</param>
        /// <param name="origWidth">Original image height</param>
        /// <param name="maxHeight">Maximal image width</param>
        /// <param name="maxWidth">Maximal image height</param>
        public static int[] GetThumbImageDimensions(int origHeight, int origWidth, double maxHeight, double maxWidth)
        {
            int[] result = { origHeight, origWidth };

            if ((origHeight > 0) && (origWidth > 0))
            {
                if ((origHeight > maxHeight) || (origWidth > maxWidth))
                {
                    double shrinkRatio = maxWidth / origWidth;

                    // Get new dimensions
                    int newWidth = (int)maxWidth;
                    int newHeight = GetDimension(shrinkRatio, origHeight);

                    // Ensure max height
                    if (newHeight > maxHeight)
                    {
                        shrinkRatio = maxHeight / newHeight;
                        newHeight = (int)maxHeight;
                        newWidth = GetDimension(shrinkRatio, newWidth);
                    }

                    result = new[] { newHeight, newWidth };
                }
                else
                {
                    // Image dimensions are acceptable
                    result = new[] { origHeight, origWidth };
                }
            }

            return result;
        }


        /// <summary>
        /// Returns URL updated according specified properties.
        /// </summary>
        /// <param name="height">Height of the item</param>
        /// <param name="width">Width of the item</param>
        /// <param name="origWidth">Original width of the file</param>
        /// <param name="origHeight">Original height of the file</param>
        /// <param name="url">URL to update</param>
        /// <param name="sourceType">MediaSource type</param>
        public static string UpdateUrl(int width, int height, int origWidth, int origHeight, string url, MediaSourceEnum sourceType)
        {
            return UpdateUrl(width, height, origWidth, origHeight, url, sourceType, false);
        }


        /// <summary>
        /// Returns URL updated according specified properties.
        /// </summary>
        /// <param name="height">Height of the item</param>
        /// <param name="width">Width of the item</param>
        /// <param name="origWidth">Original width of the file</param>
        /// <param name="origHeight">Original height of the file</param>
        /// <param name="url">URL to update</param>
        /// <param name="sourceType">MediaSource type</param>
        /// <param name="forceSizeToUrl">Indicates whether the URL update should be forced</param>
        public static string UpdateUrl(int width, int height, int origWidth, int origHeight, string url, MediaSourceEnum sourceType, bool forceSizeToUrl)
        {
            if (!String.IsNullOrEmpty(url))
            {
                bool mediaFileDirectUrl = (sourceType == MediaSourceEnum.MediaLibraries) && !SettingsKeyInfoProvider.GetBoolValue(SiteContext.CurrentSiteName + ".CMSMediaUsePermanentURLs");
                bool getFileUrl = ((sourceType == MediaSourceEnum.Attachment) || (sourceType == MediaSourceEnum.Content));

                // Check the url type manually
                if ((url.Contains("getmediafile.aspx")) || (url.Contains("/getmedia/")))
                {
                    Match m = GuidReg.Match(url);
                    if (m.Success)
                    {
                        mediaFileDirectUrl = false;
                    }
                }

                //  Do not modify URL for media library items using direct links
                if (!mediaFileDirectUrl || getFileUrl)
                {
                    // Update WIDTH & HEIGHT if not original
                    if ((width < origWidth) || (height < origHeight) || forceSizeToUrl)
                    {
                        bool isImageExtension = ImageHelper.IsImage(URLHelper.GetQueryValue(url, "ext"));
                        bool useOriginalWidth = isImageExtension && ((origWidth != 0) && (width > origWidth));
                        bool useOriginalHeight = isImageExtension && ((origHeight != 0) && (height > origHeight));

                        url = width > 0 ? URLHelper.UpdateParameterInUrl(url, "width", useOriginalWidth ? origWidth.ToString() : width.ToString()) : URLHelper.RemoveParameterFromUrl(url, "width");
                        url = height > 0 ? URLHelper.UpdateParameterInUrl(url, "height", useOriginalHeight ? origHeight.ToString() : height.ToString()) : URLHelper.RemoveParameterFromUrl(url, "height");
                    }
                    else
                    {
                        // Remove if original size
                        url = URLHelper.RemoveParameterFromUrl(url, "width");
                        url = URLHelper.RemoveParameterFromUrl(url, "height");
                    }
                }
            }
            return url;
        }


        /// <summary>
        /// Indicates whether the file with specified extension is selectable according specified condition.
        /// </summary>
        /// <param name="selectableContent">Selectable content</param>
        /// <param name="ext">Extension to check</param>
        /// <param name="isContentFile">Content file item flag</param>
        public static bool IsItemSelectable(SelectableContentEnum selectableContent, string ext, bool isContentFile = false)
        {
            if (isContentFile && (selectableContent == SelectableContentEnum.AllFiles))
            {
                return true;
            }

            if (!String.IsNullOrEmpty(ext))
            {
                // Media library directory is never selectable
                if (HttpUtility.HtmlDecode(ext.ToLowerInvariant()) == "<dir>")
                {
                    return false;
                }

                MediaTypeEnum type = MediaHelper.GetMediaType(ext);

                switch (selectableContent)
                {
                    case SelectableContentEnum.OnlyImages:
                        return (type == MediaTypeEnum.Image);

                    case SelectableContentEnum.OnlyMedia:
                        return ((type == MediaTypeEnum.AudioVideo) ||
                                (type == MediaTypeEnum.Image));
                }
            }

            return ((selectableContent == SelectableContentEnum.AllContent) ||
                    (selectableContent == SelectableContentEnum.AllFiles));
        }


        /// <summary>
        /// Ensures registration of dialog helper JavaScript.
        /// </summary>
        /// <param name="page">Page to register</param>
        public static void RegisterDialogHelper(Page page)
        {
            if (page != null)
            {
                ScriptHelper.RegisterJQuery(page);
                ScriptHelper.RegisterScriptFile(page, "Dialogs/DialogHelper.js");
            }
        }


        /// <summary>
        /// Returns complete URL according to the dialog configuration.
        /// </summary>
        /// <param name="config">Dialog configuration</param>
        /// <param name="liveSite">Indicates if dialog is opened on the live site</param>        
        public static string GetDialogUrl(DialogConfiguration config, bool liveSite)
        {
            return GetDialogUrl(config, liveSite, true);
        }


        /// <summary>
        /// Returns complete URL according to the dialog configuration.
        /// </summary>
        /// <param name="config">Dialog configuration</param>
        /// <param name="liveSite">Indicates if dialog is opened on the live site</param>    
        /// <param name="encode">If true, the final URL is encoded</param>
        public static string GetDialogUrl(DialogConfiguration config, bool liveSite, bool encode)
        {
            return GetDialogUrl(config, liveSite, encode, null);
        }


        /// <summary>
        /// Returns complete URL according to the dialog configuration.
        /// </summary>
        /// <param name="config">Dialog configuration</param>
        /// <param name="liveSite">Indicates if dialog is opened on the live site</param>    
        /// <param name="encode">If true, the final URL is encoded</param>
        /// <param name="ckBasePath">CK editor base path. If not empty relative url for CK plugin is returned</param>
        public static string GetDialogUrl(DialogConfiguration config, bool liveSite, bool encode, string ckBasePath)
        {
            return GetDialogUrl(config, liveSite, encode, ckBasePath, true);
        }


        /// <summary>
        /// Returns complete URL according to the dialog configuration.
        /// </summary>
        /// <param name="config">Dialog configuration</param>
        /// <param name="liveSite">Indicates if dialog is opened on the live site</param>    
        /// <param name="encode">If true, the final URL is encoded</param>
        /// <param name="ckBasePath">CK editor base path. If not empty relative url for CK plugin is returned</param>
        /// <param name="getAbsolute">Indicates if absolute url should be generated</param>
        public static string GetDialogUrl(DialogConfiguration config, bool liveSite, bool encode, string ckBasePath, bool getAbsolute)
        {
            // Get base URL
            string baseUrl;

            if (liveSite)
            {
                const string liveSelectorsFolder = "/CMSFormControls/LiveSelectors/";

                baseUrl = AuthenticationHelper.IsAuthenticated() ? "/cms/dialogs" + liveSelectorsFolder : liveSelectorsFolder;
            }
            else
            {
                baseUrl = "/CMSFormControls/Selectors/";
            }

            // Get query string
            string queryString = GetDialogQueryString(config, encode);

            string relativeUrl = $"~{baseUrl}InsertImageOrMedia/Default.aspx{queryString}";

            // Get complete URL
            if (!String.IsNullOrEmpty(ckBasePath))
            {
                return UrlResolver.ResolveUrl(relativeUrl);
            }

            if (getAbsolute)
            {
                return URLHelper.GetAbsoluteUrl(relativeUrl);
            }

            return UrlResolver.ResolveUrl(relativeUrl);
        }


        /// <summary>
        /// Returns query string which will be passed to the CMS dialogs (Insert image or media/Insert link).
        /// </summary>
        /// <param name="config">Dialog configuration</param>       
        public static string GetDialogQueryString(DialogConfiguration config)
        {
            return GetDialogQueryString(config, true);
        }


        /// <summary>
        /// Returns query string which will be passed to the CMS dialogs (Insert image or media/Insert link).
        /// </summary>
        /// <param name="config">Dialog configuration</param>       
        /// <param name="encode">If true, the final URL is encoded</param>
        public static string GetDialogQueryString(DialogConfiguration config, bool encode)
        {
            StringBuilder builder = new StringBuilder();

            // Output format            
            switch (config.OutputFormat)
            {
                case OutputFormatEnum.HTMLMedia:
                    builder.Append("?output=html");
                    break;

                case OutputFormatEnum.HTMLLink:
                    builder.Append("?output=html&link=1");
                    break;

                case OutputFormatEnum.BBMedia:
                    builder.Append("?output=bb");
                    break;

                case OutputFormatEnum.BBLink:
                    builder.Append("?output=bb&link=1");
                    break;

                case OutputFormatEnum.NodeGUID:
                    builder.Append("?output=nodeguid");
                    break;

                case OutputFormatEnum.URL:
                    builder.Append("?output=url");
                    break;

                default:
                    builder.AppendFormat("?output={0}", HttpUtility.UrlEncode(config.CustomFormatCode));
                    break;
            }

            // Selectable content
            switch (config.SelectableContent)
            {
                case SelectableContentEnum.OnlyImages:
                    builder.Append("&content=img");
                    break;

                case SelectableContentEnum.OnlyMedia:
                    builder.Append("&content=media");
                    break;

                case SelectableContentEnum.AllFiles:
                    builder.Append("&content=allfiles");
                    break;
            }

            // Selectable page types
            builder.AppendFormat("&pagetypes={0}", config.SelectablePageTypes);

            // Hide email tab for BB code
            if (config.OutputFormat == OutputFormatEnum.BBLink)
            {
                builder.Append("&email_hide=1");
            }

            // Hide other tabs if required
            if ((config.OutputFormat == OutputFormatEnum.HTMLLink) ||
                (config.OutputFormat == OutputFormatEnum.Custom) ||
                (config.OutputFormat != OutputFormatEnum.NodeGUID))
            {
                if (config.HideEmail && (config.OutputFormat != OutputFormatEnum.BBLink))
                {
                    builder.Append("&email_hide=1");
                }
                if (config.HideAnchor)
                {
                    builder.Append("&anchor_hide=1");
                }
                // Hide additional tabs
                if (config.OutputFormat != OutputFormatEnum.NodeGUID)
                {
                    if (config.HideAttachments)
                    {
                        builder.Append("&attachments_hide=1");
                    }
                    if (config.HideLibraries)
                    {
                        builder.Append("&libraries_hide=1");
                    }
                    if (config.HideWeb)
                    {
                        builder.Append("&web_hide=1");
                    }
                }
            }
            if (config.HideContent)
            {
                builder.Append("&content_hide=1");
            }

            // Editing existing document - add Document ID                        
            if (config.AttachmentDocumentID > 0)
            {
                builder.AppendFormat("&documentid={0}", config.AttachmentDocumentID);
            }
            // Creating new document - add Form GUID
            else
            {
                if (config.AttachmentFormGUID != Guid.Empty)
                {
                    builder.AppendFormat("&formguid={0}", config.AttachmentFormGUID);
                }
            }
            // Add node parent ID
            if (config.AttachmentParentID > 0)
            {
                builder.AppendFormat("&parentid={0}", config.AttachmentParentID);
            }
            // Add object identification for attachments
            if ((config.MetaFileObjectID > 0) && (!String.IsNullOrEmpty(config.MetaFileObjectType)) && (!String.IsNullOrEmpty(config.MetaFileCategory)))
            {
                builder.AppendFormat("&objectid={0}&objecttype={1}&objectcategory={2}", config.MetaFileObjectID, HttpUtility.UrlEncode(config.MetaFileObjectType), HttpUtility.UrlEncode(config.MetaFileCategory));
            }

            // Content - available sites
            if (config.ContentSites != AvailableSitesEnum.All)
            {
                builder.AppendFormat("&content_sites={0}", GetAvailableSites(config.ContentSites));
            }

            // Content - selected site
            if (!String.IsNullOrEmpty(config.ContentSelectedSite))
            {
                builder.AppendFormat("&content_site={0}", HttpUtility.UrlEncode(config.ContentSelectedSite));
            }
            // Content - starting path
            if (!String.IsNullOrEmpty(config.ContentStartingPath))
            {
                builder.Append("&content_path=" + HttpUtility.UrlEncode(config.ContentStartingPath));
            }
            // Content - use relative URL - send only true, as resolve URL is false by default
            if (config.ContentUseRelativeUrl)
            {
                builder.AppendFormat("&content_userelativeurl={0}", config.ContentUseRelativeUrl);
            }

            builder.AppendFormat("&content_culture={0}", HttpUtility.UrlEncode(config.Culture));

            // Libraries - available sites
            if (config.LibSites != AvailableSitesEnum.All)
            {
                builder.AppendFormat("&libraries_sites={0}", GetAvailableSites(config.LibSites));
            }

            // Libraries - selected site
            if (!String.IsNullOrEmpty(config.LibSelectedSite))
            {
                builder.AppendFormat("&libraries_site={0}", HttpUtility.UrlEncode(config.LibSelectedSite));
            }

            // Libraries - global libraries
            if (config.LibGlobalLibraries != AvailableLibrariesEnum.All)
            {
                builder.AppendFormat("&libraries_global={0}", GetAvailableLibraries(config.LibGlobalLibraries));

                // Libraries - selected global library
                if ((config.LibGlobalLibraries == AvailableLibrariesEnum.OnlySingleLibrary) && !String.IsNullOrEmpty(config.LibGlobalLibraryName))
                {
                    builder.AppendFormat("&libraries_global_name={0}", HttpUtility.UrlEncode(config.LibGlobalLibraryName));
                }
            }

            // Libraries - group libraries
            if (config.LibGroupLibraries != AvailableLibrariesEnum.All)
            {
                builder.AppendFormat("&libraries_group={0}", GetAvailableLibraries(config.LibGroupLibraries));

                // Libraries - selected group library
                if ((config.LibGroupLibraries == AvailableLibrariesEnum.OnlySingleLibrary) && !String.IsNullOrEmpty(config.LibGroupLibraryName))
                {
                    builder.AppendFormat("&libraries_group_name={0}", HttpUtility.UrlEncode(config.LibGroupLibraryName));
                }
            }

            // Libraries - available groups
            if (config.LibGroups != AvailableGroupsEnum.All)
            {
                builder.AppendFormat("&groups={0}", GetAvailableGroups(config.LibGroups));

                if ((config.LibGroups == AvailableGroupsEnum.OnlySingleGroup) && (config.LibGroupName != ""))
                {
                    // Libraries - selected group
                    builder.AppendFormat("&groups_name={0}", HttpUtility.UrlEncode(config.LibGroupName));
                }
            }

            // Libraries - starting path
            if (!String.IsNullOrEmpty(config.LibStartingPath))
            {
                builder.AppendFormat("&libraries_path={0}", HttpUtility.UrlEncode(config.LibStartingPath));
            }

            // Add image auto resize parameters
            if (config.ResizeToMaxSideSize > 0)
            {
                builder.AppendFormat("&autoresize_maxsidesize={0}", config.ResizeToMaxSideSize);
            }
            else
            {
                if (config.ResizeToWidth > 0)
                {
                    builder.AppendFormat("&autoresize_width={0}", config.ResizeToWidth);
                }
                if (config.ResizeToHeight > 0)
                {
                    builder.AppendFormat("&autoresize_height={0}", config.ResizeToHeight);
                }
            }

            // Editor client id
            if (!String.IsNullOrEmpty(config.EditorClientID))
            {
                builder.AppendFormat("&editor_clientid={0}", HttpUtility.UrlEncode(config.EditorClientID));
            }

            // Check if groups are avaliable
            if (LicenseHelper.CheckFeature(RequestContext.CurrentDomain, FeatureEnum.Groups))
            {
                // Check if current group is specified
                int groupId = ModuleCommands.CommunityGetCurrentGroupID();
                if (groupId > 0)
                {
                    builder.AppendFormat("&groupid={0}", groupId);
                }
            }

            // Get current media library object from media library context
            BaseInfo currentLibrary = (BaseInfo)ModuleManager.GetContextProperty("MediaLibraryContext", "CurrentMediaLibrary");
            if (currentLibrary != null)
            {
                builder.Append("&libraryid=" + currentLibrary.Generalized.ObjectID);
            }

            // Use full URL
            if (config.UseFullURL)
            {
                builder.Append("&fullurl=1");
            }

            // Use simple URL properties (hide size and alt text).
            if (config.UseSimpleURLProperties)
            {
                builder.Append("&simple_url_properties=1");
            }

            // Add additional query parameters
            if (!String.IsNullOrEmpty(config.AdditionalQueryParameters))
            {
                builder.AppendFormat("&{0}", config.AdditionalQueryParameters.TrimStart('&'));
            }

            // Add current site identifier
            builder.AppendFormat("&siteid={0}", SiteContext.CurrentSiteID);

            // Get hash for complete query string
            string hash = QueryHelper.GetHash(builder.ToString());
            string dialogQueryResult = builder.AppendFormat("&hash={0}", hash).ToString();

            // Return complete query string with attached hash
            if (encode)
            {
                // Encode to be used as a part of javascript code (when dialog is registered on the page)
                return ScriptHelper.GetString(dialogQueryResult, false);
            }

            return dialogQueryResult;
        }


        /// <summary>
        /// Returns Media data according to given URL.
        /// </summary>
        /// <param name="url">URL of media object</param>
        /// <param name="siteName">Site name</param>
        public static MediaSource GetMediaData(string url, string siteName)
        {
            if (!String.IsNullOrEmpty(url))
            {
                url = url.Trim().ToLowerInvariant();
                MediaSource mediaSource = new MediaSource();

                if (String.IsNullOrEmpty(siteName))
                {
                    // Check if absolute URL
                    if (url.IndexOf("://", StringComparison.OrdinalIgnoreCase) > -1)
                    {
                        // Get site name from URL
                        siteName = SiteInfoProvider.GetSiteNameFromUrl(url);
                        if (String.IsNullOrEmpty(siteName))
                        {
                            // If URL is absolute and no site found it is web media source
                            return mediaSource;
                        }
                    }
                    else
                    {
                        // Use current site name
                        siteName = SiteContext.CurrentSiteName;
                    }
                }

                // Start event
                using (var handler = DialogHandlers.GetMediaData.StartEvent(url, siteName))
                {
                    if (handler.CanContinue() && !handler.EventArguments.EventHandled)
                    {
                        // If getfile or getdoc type of URL
                        if ((url.Contains("getfile")) || (url.Contains("getdoc")) || ((url.Contains("getfile.aspx")) && url.Contains("nodeguid")))
                        {
                            var m = GuidReg.Match(url);
                            if (m.Success)
                            {
                                Guid guid = new Guid(m.Groups[0].Value);

                                var tp = new TreeProvider();

                                // Get the document
                                var node = tp.SelectSingleNode(guid, LocalizationContext.PreferredCultureCode, siteName);
                                if (node != null)
                                {
                                    mediaSource.NodeAliasPath = node.NodeAliasPath;
                                    mediaSource.DocumentID = node.DocumentID;
                                    mediaSource.NodeID = node.NodeID;
                                    mediaSource.NodeGuid = node.NodeGUID;
                                    mediaSource.FileName = node.NodeName;
                                    mediaSource.SiteID = node.NodeSiteID;
                                    mediaSource.SourceType = MediaSourceEnum.Content;
                                    mediaSource.HistoryID = node.DocumentCheckedOutVersionHistoryID;

                                    string docType = ValidationHelper.GetString(node.GetValue("documenttype"), null);
                                    if (docType != null)
                                    {
                                        mediaSource.Extension = docType;
                                    }

                                    if (node.IsFile())
                                    {
                                        // Get the document
                                        var fileNode = tp.SelectSingleNode(siteName, node.NodeAliasPath, LocalizationContext.PreferredCultureCode, false, SystemDocumentTypes.File, false);
                                        if (fileNode != null)
                                        {
                                            Guid attachmentGuid = ValidationHelper.GetGuid(fileNode.GetValue("FileAttachment"), Guid.Empty);

                                            // Get the attachment
                                            var ai = DocumentHelper.GetAttachment(fileNode, attachmentGuid, false);
                                            if (ai != null)
                                            {
                                                mediaSource.MediaWidth = ai.AttachmentImageWidth;
                                                mediaSource.MediaHeight = ai.AttachmentImageHeight;
                                                mediaSource.FileSize = ai.AttachmentSize;
                                                mediaSource.AttachmentGuid = ai.AttachmentGUID;
                                                mediaSource.Extension = ai.AttachmentExtension;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        mediaSource.FileName = node.GetDocumentName();
                                    }

                                    return mediaSource;
                                }
                            }
                        }

                        // If getattachment type of URL
                        if ((url.Contains("getattachment")) || (url.Contains("cmsgetattachment")) || (url.Contains("getfile.aspx") && url.Contains("guid")))
                        {
                            var m = GuidReg.Match(url);
                            if (m.Success)
                            {
                                Guid guid = new Guid(m.Groups[0].Value);
                                TreeNode tn;

                                // Get attachment by GUID
                                var ai = DocumentHelper.GetAttachment(guid, siteName, false, out tn);
                                if (ai != null)
                                {
                                    mediaSource.AttachmentGuid = ai.AttachmentGUID;
                                    mediaSource.AttachmentID = ai.AttachmentID;
                                    mediaSource.SiteID = ai.AttachmentSiteID;
                                    mediaSource.DocumentID = ai.AttachmentDocumentID;
                                    if (tn != null)
                                    {
                                        mediaSource.NodeID = tn.NodeID;
                                        mediaSource.HistoryID = tn.DocumentCheckedOutVersionHistoryID;
                                    }
                                    mediaSource.SourceType = MediaSourceEnum.DocumentAttachments;
                                    mediaSource.Extension = ai.AttachmentExtension;
                                    mediaSource.MediaWidth = ai.AttachmentImageWidth;
                                    mediaSource.MediaHeight = ai.AttachmentImageHeight;
                                    mediaSource.FileSize = ai.AttachmentSize;

                                    string name = ai.AttachmentName;
                                    int dotIndex = name.LastIndexOf(".", StringComparison.OrdinalIgnoreCase);

                                    // Get file name
                                    mediaSource.FileName = dotIndex > -1 ? ai.AttachmentName.Remove(dotIndex) : ai.AttachmentName;

                                    return mediaSource;
                                }
                            }
                            else
                            {
                                string attachmentUrl = url;
                                int attachmentQueryIndex = attachmentUrl.LastIndexOf("?", StringComparison.OrdinalIgnoreCase);
                                if (attachmentQueryIndex > -1)
                                {
                                    attachmentUrl = attachmentUrl.Remove(attachmentQueryIndex);
                                }
                                int index = attachmentUrl.IndexOf("getattachment/", StringComparison.OrdinalIgnoreCase);
                                if (index > -1)
                                {
                                    string fileName;
                                    string aliasPath = attachmentUrl.Substring(index + 13);
                                    int slashIndex = aliasPath.LastIndexOf("/", StringComparison.OrdinalIgnoreCase);
                                    if (slashIndex > -1)
                                    {
                                        // Get alias path and file name from URL
                                        fileName = aliasPath.Substring(slashIndex + 1);
                                        aliasPath = aliasPath.Remove(slashIndex);
                                    }
                                    else
                                    {
                                        // File name is right behind getattachment/
                                        fileName = aliasPath.Trim('/');
                                        // Root document
                                        aliasPath = "/";
                                    }

                                    string fileExtension = TreePathUtils.GetFilesUrlExtension();
                                    if ((!String.IsNullOrEmpty(fileExtension)) && (fileName.EndsWith(fileExtension, StringComparison.OrdinalIgnoreCase)))
                                    {
                                        fileName = fileName.Substring(0, fileName.Length - fileExtension.Length);
                                    }

                                    if (!String.IsNullOrEmpty(aliasPath) && !String.IsNullOrEmpty(fileName))
                                    {
                                        var tp = new TreeProvider();

                                        // Get node by alias path
                                        var node = tp.SelectSingleNode(siteName, aliasPath, LocalizationContext.PreferredCultureCode, false, null, false);
                                        if (node != null)
                                        {
                                            mediaSource.FileName = node.NodeName;
                                            mediaSource.NodeID = node.NodeID;
                                            mediaSource.NodeGuid = node.NodeGUID;
                                            mediaSource.NodeAliasPath = node.NodeAliasPath;
                                            mediaSource.HistoryID = node.DocumentCheckedOutVersionHistoryID;

                                            // Get attachement with specified file name for node
                                            var ai = DocumentHelper.GetAttachment(node, fileName, false);
                                            if (ai != null)
                                            {
                                                // Fill media source
                                                mediaSource.AttachmentGuid = ai.AttachmentGUID;
                                                mediaSource.AttachmentID = ai.AttachmentID;
                                                mediaSource.SiteID = ai.AttachmentSiteID;
                                                mediaSource.DocumentID = ai.AttachmentDocumentID;
                                                mediaSource.SourceType = MediaSourceEnum.DocumentAttachments;
                                                mediaSource.Extension = ai.AttachmentExtension;
                                                mediaSource.MediaWidth = ai.AttachmentImageWidth;
                                                mediaSource.MediaHeight = ai.AttachmentImageHeight;
                                                mediaSource.FileSize = ai.AttachmentSize;

                                                return mediaSource;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        // If getmetafile type of URL
                        if ((url.Contains("getmetafile")) || (url.Contains("getmetafile.aspx") && url.Contains("guid")))
                        {
                            Match m = GuidReg.Match(url);
                            if (m.Success)
                            {
                                Guid guid = new Guid(m.Groups[0].Value);

                                // Get metafile by GUID and site name
                                MetaFileInfo mi = MetaFileInfoProvider.GetMetaFileInfoWithoutBinary(guid, siteName, true);
                                if (mi != null)
                                {
                                    mediaSource.MetaFileGuid = mi.MetaFileGUID;
                                    mediaSource.MetaFileID = mi.MetaFileID;
                                    mediaSource.SiteID = mi.MetaFileSiteID;
                                    mediaSource.SourceType = MediaSourceEnum.MetaFile;
                                    mediaSource.Extension = mi.MetaFileExtension;
                                    mediaSource.MediaWidth = mi.MetaFileImageWidth;
                                    mediaSource.MediaHeight = mi.MetaFileImageHeight;
                                    mediaSource.FileSize = mi.MetaFileSize;

                                    string name = mi.MetaFileName;
                                    int dotIndex = name.LastIndexOf(".", StringComparison.OrdinalIgnoreCase);

                                    // Get file name
                                    mediaSource.FileName = dotIndex > -1 ? name.Remove(dotIndex) : name;

                                    return mediaSource;
                                }
                            }
                        }

                        // If nothing yet try content URL
                        int queryIndex = url.LastIndexOf("?", StringComparison.OrdinalIgnoreCase);
                        if (queryIndex > -1)
                        {
                            url = url.Remove(queryIndex);
                        }

                        // Remove trailing slash
                        url = url.TrimEnd('/');

                        var currentSite = SiteContext.CurrentSite;
                        TreeNode selectedNode = null;

                        if (currentSite.SiteIsContentOnly && currentSite.SiteName.Equals(siteName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            string virtualUrl = URLHelper.UnResolveUrl(url, SystemContext.ApplicationPath);

                            // Try to get content-only page
                            var altUrl = new AlternativeUrlInfo()
                            {
                                AlternativeUrlSiteID = currentSite.SiteID,
                                AlternativeUrlUrl = AlternativeUrlHelper.NormalizeAlternativeUrl(virtualUrl),
                            };

                            selectedNode = AlternativeUrlHelper.GetConflictingPage(altUrl);
                        }
                        else
                        {
                            // Try get the page info
                            PageInfo pageInfo = null;

                            if (url.IndexOf("://", StringComparison.OrdinalIgnoreCase) < 0)
                            {
                                // Make absolute url for GetPageInfoForUrl
                                url = URLHelper.GetAbsoluteUrl(url);
                            }

                            try
                            {
                                pageInfo = PageInfoProvider.GetPageInfoForUrl(url, LocalizationContext.PreferredCultureCode, "/", false, true, siteName);
                            }
                            catch
                            {
                                // ignored
                            }

                            if (pageInfo != null)
                            {
                                // Get the document node
                                selectedNode =
                                    DocumentHelper.GetDocuments(pageInfo.ClassName)
                                        .WhereEquals("NodeID", pageInfo.NodeID)
                                        .Culture(LocalizationContext.PreferredCultureCode)
                                        .CombineWithAnyCulture()
                                        .FirstOrDefault();
                            }
                        }

                        if (selectedNode != null)
                        {
                            mediaSource.NodeAliasPath = selectedNode.NodeAliasPath;
                            mediaSource.DocumentID = selectedNode.DocumentID;
                            mediaSource.NodeID = selectedNode.NodeID;
                            mediaSource.FileName = selectedNode.NodeName;
                            mediaSource.NodeGuid = selectedNode.NodeGUID;
                            mediaSource.SiteID = selectedNode.NodeSiteID;
                            mediaSource.SourceType = MediaSourceEnum.Content;
                            mediaSource.HistoryID = selectedNode.DocumentCheckedOutVersionHistoryID;

                            string mediaType = ValidationHelper.GetString(selectedNode.GetValue("documenttype"), "");
                            mediaSource.Extension = mediaType;

                            if (selectedNode.IsFile())
                            {
                                Guid attachmentsGuid = ValidationHelper.GetGuid(selectedNode.GetValue("FileAttachment"), Guid.Empty);

                                if (attachmentsGuid != Guid.Empty)
                                {
                                    // Get the attachment
                                    var ai = DocumentHelper.GetAttachment(selectedNode, attachmentsGuid, false);
                                    if (ai != null)
                                    {
                                        mediaSource.AttachmentGuid = ai.AttachmentGUID;
                                        mediaSource.MediaWidth = ai.AttachmentImageWidth;
                                        mediaSource.MediaHeight = ai.AttachmentImageHeight;
                                        mediaSource.FileSize = ai.AttachmentSize;
                                        mediaSource.Extension = ai.AttachmentExtension;
                                    }
                                }
                            }
                            else
                            {
                                mediaSource.FileName = selectedNode.GetDocumentName();
                            }

                            return mediaSource;
                        }

                        return mediaSource;
                    }

                    // Finish event
                    handler.FinishEvent();

                    return handler.EventArguments.MediaSource;
                }
            }

            return null;
        }


        /// <summary>
        /// Converts string into SelectableContentEnum.
        /// </summary>
        /// <param name="content">Source string</param>
        public static SelectableContentEnum GetSelectableContent(string content)
        {
            switch (content.ToLowerInvariant())
            {
                case "img":
                    return SelectableContentEnum.OnlyImages;

                case "media":
                    return SelectableContentEnum.OnlyMedia;

                case "allfiles":
                    return SelectableContentEnum.AllFiles;

                default:
                    return SelectableContentEnum.AllContent;
            }
        }


        /// <summary>
        /// Converts SelectableContentEnum into string.
        /// </summary>
        /// <param name="content">SelectableContentEnum value</param>
        public static string GetSelectableContent(SelectableContentEnum content)
        {
            switch (content)
            {
                case SelectableContentEnum.OnlyImages:
                    return "img";

                case SelectableContentEnum.OnlyMedia:
                    return "media";

                case SelectableContentEnum.AllFiles:
                    return "allfiles";

                default:
                    return "all";
            }
        }


        /// <summary>
        /// Converts string into MediaSourceEnum.
        /// </summary>
        /// <param name="source">Source string</param>
        public static MediaSourceEnum GetMediaSource(string source)
        {
            switch (source.ToLowerInvariant())
            {
                case "docattachments":
                    return MediaSourceEnum.DocumentAttachments;

                case "attachment":
                    return MediaSourceEnum.Attachment;

                case "content":
                    return MediaSourceEnum.Content;

                case "libraries":
                    return MediaSourceEnum.MediaLibraries;

                case "physicalfile":
                    return MediaSourceEnum.PhysicalFile;

                case "metafile":
                    return MediaSourceEnum.MetaFile;

                default:
                    return MediaSourceEnum.Web;
            }
        }


        /// <summary>
        /// Converts MediaSourceEnum into string.
        /// </summary>
        /// <param name="source">MediaSourceEnum value</param>
        public static string GetMediaSource(MediaSourceEnum source)
        {
            switch (source)
            {
                case MediaSourceEnum.Attachment:
                    return "attachment";

                case MediaSourceEnum.DocumentAttachments:
                    return "docattachments";

                case MediaSourceEnum.Content:
                    return "content";

                case MediaSourceEnum.MediaLibraries:
                    return "libraries";

                case MediaSourceEnum.PhysicalFile:
                    return "physicalfile";

                case MediaSourceEnum.MetaFile:
                    return "metafile";

                default:
                    return "web";
            }
        }


        /// <summary>
        /// Returns MediaTypeEnum based on item's extension.
        /// </summary>
        /// <param name="ext">Item's extension</param>
        public static MediaTypeEnum GetMediaTypeByExt(string ext)
        {
            if (!String.IsNullOrEmpty(ext))
            {
                // Image
                if (ImageHelper.IsImage(ext))
                {
                    return MediaTypeEnum.Image;
                }
                // Audio/Video
                else if (MediaHelper.IsAudioVideo(ext))
                {
                    return MediaTypeEnum.AudioVideo;
                }
            }

            return MediaTypeEnum.Unknown;
        }


        /// <summary>
        /// Converts string into MediaTypeEnum.
        /// </summary>
        /// <param name="source">Source string</param>
        public static MediaTypeEnum GetMediaType(string source)
        {
            switch (source.ToLowerInvariant())
            {
                case "image":
                    return MediaTypeEnum.Image;

                case "audiovideo":
                    return MediaTypeEnum.AudioVideo;

                default:
                    return MediaTypeEnum.Unknown;
            }
        }


        /// <summary>
        /// Converts MediaTypeEnum into string.
        /// </summary>
        /// <param name="source">MediaTypeEnum value</param>
        public static string GetMediaType(MediaTypeEnum source)
        {
            switch (source)
            {
                case MediaTypeEnum.Image:
                    return "image";

                case MediaTypeEnum.AudioVideo:
                    return "audiovideo";

                default:
                    return "unknown";
            }
        }


        /// <summary>
        /// Returns dialog view mode enumeration from its string representation.
        /// </summary>
        /// <param name="viewMode">Dialog view mode string representation</param>
        public static DialogViewModeEnum GetDialogViewMode(string viewMode)
        {
            if (viewMode == "thumbnails")
            {
                return DialogViewModeEnum.ThumbnailsView;
            }

            // Return ListViewMode as default mode
            return DialogViewModeEnum.ListView;
        }


        /// <summary>
        /// Returns string representation of specified dialog view mode.
        /// </summary>
        /// <param name="viewMode">Dialog view mode</param>
        public static string GetDialogViewMode(DialogViewModeEnum viewMode)
        {
            if (viewMode == DialogViewModeEnum.ThumbnailsView)
            {
                return "thumbnails";
            }

            // Return ListViewMode as default mode
            return "list";
        }


        /// <summary>
        /// Returns output format enumeration.
        /// </summary>
        /// <param name="output">Output format string code (html/bb/url/nodeguid/[custom])</param>
        /// <param name="link">Indicates if output should be link</param>        
        public static OutputFormatEnum GetOutputFormat(string output, bool link)
        {
            switch (output.ToLowerInvariant())
            {
                case "html":
                    return ((link) ? OutputFormatEnum.HTMLLink : OutputFormatEnum.HTMLMedia);

                case "bb":
                    return ((link) ? OutputFormatEnum.BBLink : OutputFormatEnum.BBMedia);

                case "url":
                    return OutputFormatEnum.URL;

                case "nodeguid":
                    return OutputFormatEnum.NodeGUID;

                default:
                    return OutputFormatEnum.Custom;
            }
        }


        /// <summary>
        /// Gets JavaScript object representing image item being inserted.
        /// </summary>
        /// <param name="properties">Collection of item properties</param>
        public static string GetImageItem(Hashtable properties)
        {
            if (!String.IsNullOrEmpty(properties[DialogParameters.IMG_URL]?.ToString()))
            {
                return GetDialogItem(properties);
            }
            return null;
        }


        /// <summary>
        /// Gets JavaScript object representing audio/video item being inserted.
        /// </summary>
        /// <param name="properties">Collection of item properties</param>
        public static string GetAVItem(Hashtable properties)
        {
            if (!String.IsNullOrEmpty(properties[DialogParameters.AV_URL]?.ToString()))
            {
                return GetDialogItem(properties);
            }
            return null;
        }


        /// <summary>
        /// Gets JavaScript object representing you tube item being inserted.
        /// </summary>
        /// <param name="properties">Collection of item properties</param>
        public static string GetYouTubeItem(Hashtable properties)
        {
            if (!String.IsNullOrEmpty(properties[DialogParameters.YOUTUBE_URL]?.ToString()))
            {
                return GetDialogItem(properties);
            }
            return null;
        }


        /// <summary>
        /// Gets JavaScript object representing link item being inserted.
        /// </summary>
        /// <param name="properties">Collection of item properties</param>
        public static string GetLinkItem(Hashtable properties)
        {
            if (!String.IsNullOrEmpty(properties[DialogParameters.LINK_URL]?.ToString()))
            {
                return GetDialogItem(properties);
            }
            return null;
        }


        /// <summary>
        /// Gets JavaScript object representing anchor item being inserted.
        /// </summary>
        /// <param name="properties">Collection of item properties</param>
        public static string GetAnchorItem(Hashtable properties)
        {
            return GetDialogItem(properties);
        }


        /// <summary>
        /// Gets JavaScript object representing email item being inserted.
        /// </summary>
        /// <param name="properties">Collection of item properties</param>
        public static string GetEmailItem(Hashtable properties)
        {
            if (!String.IsNullOrEmpty(properties[DialogParameters.EMAIL_TO]?.ToString()))
            {
                return GetDialogItem(properties);
            }
            return null;
        }


        /// <summary>
        /// Gets JavaScript object representing url item being inserted.
        /// </summary>
        /// <param name="properties">Collection of item properties</param>
        public static string GetUrlItem(Hashtable properties)
        {
            if (!String.IsNullOrEmpty(properties[DialogParameters.URL_URL]?.ToString()))
            {
                return GetDialogItem(properties);
            }
            return null;
        }


        /// <summary>
        /// Gets JavaScript object representing file system item being inserted.
        /// </summary>
        /// <param name="properties">Collection of item properties</param>
        public static string GetFileSystemItem(Hashtable properties)
        {
            if (!String.IsNullOrEmpty(properties[DialogParameters.ITEM_PATH]?.ToString()))
            {
                return GetDialogItem(properties);
            }
            return null;
        }


        /// <summary>
        /// Returns object javascript for insert into wysiwyg editor.
        /// </summary>
        /// <param name="properties">Properties table</param>
        public static string GetDialogItem(Hashtable properties)
        {
            StringBuilder sb = new StringBuilder();
            if (properties.Count > 0)
            {
                sb.AppendLine("var obj = {};");

                foreach (DictionaryEntry entry in properties)
                {
                    sb.AppendFormat("obj.{0} = {1};\n", entry.Key, ScriptHelper.GetString(entry.Value.ToString()));
                }

                var contentChanged = ValidationHelper.GetBoolean(properties[DialogParameters.CONTENT_CHANGED], true);

                sb.AppendLine(@"
var topWin = GetTop();
if ((topWin.frames['insertFooter']) && (topWin.frames['insertFooter'].InsertSelectedItem))
{ 
    topWin.frames['insertFooter'].InsertSelectedItem(obj);");

                if (contentChanged)
                {
                    sb.AppendLine(@"
    if (wopener.Changed)
    {
        // Mark that content has been changed
        wopener.Changed();
    }");
                }

                sb.AppendLine(@"
    CloseDialog();
}");
            }
            return sb.ToString();
        }


        /// <summary>
        /// Returns true if link url is not for image full size, returns false if link is image full size url.
        /// </summary>
        /// <param name="imgUrl">Image url</param>
        /// <param name="linkUrl">Link url</param>
        /// <param name="originalWidth">Original image width</param>
        /// <param name="originalHeight">Original image height</param>
        /// <param name="target">Link target</param>
        public static bool IsImageLink(string imgUrl, string linkUrl, int originalWidth, int originalHeight, string target)
        {
            if ((!String.IsNullOrEmpty(imgUrl)) || (!String.IsNullOrEmpty(linkUrl)))
            {
                if (!String.IsNullOrEmpty(target) && ((target.ToLowerInvariant() == "_blank") || (target.ToLowerInvariant() == "_self")))
                {
                    bool isNotImageLink = false;

                    // Work only with resolved urls
                    linkUrl = UrlResolver.ResolveUrl(linkUrl);
                    imgUrl = UrlResolver.ResolveUrl(imgUrl);

                    int linkQueryIndex = linkUrl.IndexOf("?", StringComparison.OrdinalIgnoreCase);
                    int urlQueryIndex = imgUrl.IndexOf("?", StringComparison.OrdinalIgnoreCase);

                    if (linkQueryIndex < 0)
                    {
                        if (urlQueryIndex < 0)
                        {
                            isNotImageLink = (imgUrl == linkUrl);
                        }
                        else
                        {
                            isNotImageLink = (imgUrl.Remove(urlQueryIndex) == linkUrl);
                        }
                    }
                    else
                    {
                        bool parseLink = false;
                        if (urlQueryIndex < 0)
                        {
                            if (imgUrl == linkUrl.Remove(linkQueryIndex))
                            {
                                parseLink = true;
                            }
                        }
                        else
                        {
                            if (imgUrl.Remove(urlQueryIndex) == linkUrl.Remove(linkQueryIndex))
                            {
                                parseLink = true;
                            }
                        }
                        if (parseLink)
                        {
                            Regex widthReg = RegexHelper.GetRegex(@"width=(\d+)", true);
                            Regex heightReg = RegexHelper.GetRegex(@"height=(\d+)", true);
                            Match matchWidth = widthReg.Match(linkUrl);
                            Match matchHeight = heightReg.Match(linkUrl);

                            string linkWidth = null;
                            string linkHeight = null;
                            if (matchWidth.Success)
                            {
                                linkWidth = matchWidth.Groups[1].Value;
                            }
                            if (matchHeight.Success)
                            {
                                linkHeight = matchHeight.Groups[1].Value;
                            }

                            if (linkWidth != null)
                            {
                                if (linkHeight != null)
                                {
                                    isNotImageLink = ((linkWidth == originalWidth.ToString()) && (linkHeight == originalHeight.ToString()));
                                }
                                else
                                {
                                    isNotImageLink = (linkWidth == originalWidth.ToString());
                                }
                            }
                            else
                            {
                                if (linkHeight != null)
                                {
                                    isNotImageLink = (linkHeight == originalHeight.ToString());
                                }
                            }
                        }
                    }
                    return !isNotImageLink;
                }
                // No target present
                return true;
            }
            // No image url or link url present
            return false;
        }


        /// <summary>
        /// Gets info message for the specified dialog and source type displayed when no file is selected.
        /// </summary>
        /// <param name="config">Configuration used to identify what dialog is message obtained for</param>
        /// <param name="source">Type of the files message is displayed for</param>
        public static string GetSelectItemMessage(DialogConfiguration config, MediaSourceEnum source)
        {
            if ((config != null) && (config.SelectableContent == SelectableContentEnum.AllContent) && (source == MediaSourceEnum.Content))
            {
                return ResHelper.GetString("dialogs.content.linkselect");
            }

            return ResHelper.GetString("dialogs.properties.empty");
        }


        /// <summary>
        /// Gets info message for the specified dialog and source type displayed when no file exists.
        /// </summary>
        /// <param name="config">Configuration used to identify what dialog is message obtained for</param>
        /// <param name="source">Type of the files message is displayed for</param>
        public static string GetNoItemsMessage(DialogConfiguration config, MediaSourceEnum source)
        {
            if ((config != null) && (config.SelectableContent == SelectableContentEnum.AllContent) && (source == MediaSourceEnum.Content))
            {
                return ResHelper.GetString("dialogs.content.linknodata");
            }

            return ResHelper.GetString("dialogs.view.list.nodata");
        }


        /// <summary>
        /// Escapes special characters for argument.
        /// </summary>
        /// <param name="argument">Argument</param>
        public static string EscapeArgument(object argument)
        {
            return argument?.ToString().Replace("|", "&#124;");
        }


        /// <summary>
        /// Unescapes special characters for argument.
        /// </summary>
        /// <param name="argument">Argument</param>
        public static string UnEscapeArgument(object argument)
        {
            return argument?.ToString().Replace("&#124;", "|");
        }


        /// <summary>
        /// Escapes url parameter special characters.
        /// </summary>
        /// <param name="parameter">Input parameter</param>
        public static string EscapeUrlParameter(string parameter)
        {
            if (!String.IsNullOrEmpty(parameter))
            {
                parameter = parameter.Replace("%", "%25"); //.Replace(" ", "%20");
                parameter = HttpUtility.UrlEncode(parameter);
                parameter = parameter
                    .Replace("%2520", "%20")
                    .Replace("'", "%27")
                    .Replace("&", "%26")
                    .Replace("#", "%23")
                    .Replace("+", "%2B")
                    .Replace("{", "%7B")
                    .Replace("}", "%7D")
                    .Replace("/", "%2F")
                    .Replace("|", "%7C");

                return parameter;
            }
            return null;
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Gets modified dimensions matching specified shrink ratio.
        /// </summary>
        /// <param name="ratio">Ration used to shrink dimension</param>
        /// <param name="dimension">Dimension to shrink</param>
        private static int GetDimension(double ratio, double dimension)
        {
            if ((ratio > 0) && (dimension > 0))
            {
                double result = Math.Floor(dimension * ratio);

                return (int)result;
            }

            return (int)dimension;
        }


        /// <summary>
        /// Converts available libraries enumeration into its string representation.
        /// </summary>
        /// <param name="libraries">Available libraries</param>  
        private static string GetAvailableLibraries(AvailableLibrariesEnum libraries)
        {
            switch (libraries)
            {
                case AvailableLibrariesEnum.None:
                    return "none";

                case AvailableLibrariesEnum.OnlyCurrentLibrary:
                    return "current";

                case AvailableLibrariesEnum.OnlySingleLibrary:
                    return "single";

                default:
                    return "";
            }
        }


        /// <summary>
        /// Converts available groups enumeration to its string representation.
        /// </summary>
        /// <param name="groups">Available groups</param>        
        private static string GetAvailableGroups(AvailableGroupsEnum groups)
        {
            switch (groups)
            {
                case AvailableGroupsEnum.None:
                    return "none";

                case AvailableGroupsEnum.OnlyCurrentGroup:
                    return "current";

                case AvailableGroupsEnum.OnlySingleGroup:
                    return "single";

                case AvailableGroupsEnum.All:
                default:
                    return "";
            }
        }


        /// <summary>
        /// Converts available sites enumeration to its string representation.
        /// </summary>
        /// <param name="sites">Available sites</param>        
        private static string GetAvailableSites(AvailableSitesEnum sites)
        {
            switch (sites)
            {
                case AvailableSitesEnum.All:
                    return "all";

                case AvailableSitesEnum.OnlyCurrentSite:
                    return "current";

                case AvailableSitesEnum.OnlySingleSite:
                    return "single";

                default:
                    return "";
            }
        }

        #endregion
    }
}
