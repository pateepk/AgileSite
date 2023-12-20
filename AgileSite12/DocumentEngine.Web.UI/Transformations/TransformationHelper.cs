using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.UI;
using System.Xml;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Globalization;
using CMS.Globalization.Web.UI;
using CMS.Helpers;
using CMS.IO;
using CMS.Localization;
using CMS.MacroEngine;
using CMS.Membership;
using CMS.PortalEngine;
using CMS.Search;
using CMS.SiteProvider;
using CMS.Taxonomy;

using CultureInfo = System.Globalization.CultureInfo;
using TimeZoneInfo = CMS.Globalization.TimeZoneInfo;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Base transformation class.
    /// </summary>
    public class TransformationHelper : AbstractHelper<TransformationHelper>
    {
        #region "Variables"

        /// <summary>
        /// Regular expression for CDATA.
        /// </summary>
        protected static Regex mCDATARegExp;

        /// <summary>
        /// If true, Eval() methods in CMSAbstractTransformation class encodes string values.
        /// </summary>
        private static bool? mHTMLEncodeEval;

        #endregion


        #region "Properties"

        /// <summary>
        /// URL regular expression.
        /// </summary>
        public static Regex CDATARegExp
        {
            get
            {
                return mCDATARegExp ?? (mCDATARegExp = RegexHelper.GetRegex("]]>"));
            }
            set
            {
                mCDATARegExp = value;
            }
        }


        /// <summary>
        /// Gets or sets whether Eval() methods in CMSAbstractTransformation class encodes string values.
        /// </summary>
        public static bool HTMLEncodeEval
        {
            get
            {
                if (mHTMLEncodeEval == null)
                {
                    mHTMLEncodeEval = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSHTMLEncodeEval"], false);
                }
                return mHTMLEncodeEval.Value;
            }
            set
            {
                mHTMLEncodeEval = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Loads the transformation template.
        /// Supports the following formats of transformation name:
        ///     [Some text with macros] - Inline transformation
        ///     cms.user.sometransformation - Transformation full name from database
        ///     ~/SomePath/SomeControl.ascx - Path to transformation user control
        /// </summary>
        /// <param name="parent">Parent control</param>
        /// <param name="transformationName">Transformation name</param>
        /// <param name="editButtonsMode">Specify which editing buttons should be displayed</param>
        public static ITemplate LoadTransformation(Control parent, string transformationName, EditModeButtonEnum editButtonsMode = EditModeButtonEnum.None)
        {
            if (string.IsNullOrEmpty(transformationName))
            {
                return null;
            }

            try
            {
                // Handle in-place transformation (text only)
                transformationName = transformationName.Trim();

                if (transformationName.StartsWith("[", StringComparison.Ordinal) && transformationName.EndsWith("]", StringComparison.Ordinal))
                {
                    return new TextTransformationTemplate(transformationName.Substring(1, transformationName.Length - 2));
                }

                // Handle file path transformation
                if (transformationName.StartsWith("~", StringComparison.Ordinal))
                {
                    return LoadTransformationFromPath(parent, transformationName);
                }

                // Get the transformation
                var ti = TransformationInfoProvider.GetLocalizedTransformation(transformationName, LocalizationContext.PreferredCultureCode);
                if (ti == null)
                {
                    throw new TransformationNotFoundException("Transformation '" + transformationName + "' not found.");
                }

                // Register as a component
                PortalContext.CurrentComponents.RegisterTransformation(ti);

                if (ti.TransformationType == TransformationTypeEnum.Ascx)
                {
                    // Edit/delete parameter
                    if (editButtonsMode != EditModeButtonEnum.None)
                    {
                        RequestStockHelper.AddToStorage("EditModeButtonEditDelete", parent.ClientID, true);
                    }

                    // Load transformation
                    ti.EditDeleteButtonsMode = editButtonsMode;
                    string transPath = ti.Generalized.GetVirtualFileRelativePath(TransformationInfo.EXTERNAL_COLUMN_CODE, ti.TransformationVersionGUID);
                    ti.EditDeleteButtonsMode = EditModeButtonEnum.None;

                    // Return lazy loaded ITemplate object
                    return LoadTransformationFromPath(parent, transPath);
                }

                // Text transformations
                return new TextTransformationTemplate(ti.TransformationCode);
            }
            catch (Exception ex)
            {
                // Log the exception
                EventLogProvider.LogException("Controls", "LoadTransformation", ex);

                return new CMSErrorTransformationTemplate(ex);
            }
        }


        /// <summary>
        /// Loads the transformation from the given path
        /// </summary>
        /// <param name="parent">Parent control</param>
        /// <param name="path">Transformation path</param>
        public static ITemplate LoadTransformationFromPath(Control parent, string path)
        {
            return new TempITemplate(parent.Page, path);
        }


        /// <summary>
        /// Resolves the given alias path, applies the path segment to the given format string {0} for level 0.
        /// </summary>
        /// <param name="format">Alias path pattern</param>
        public string ResolveCurrentPath(string format)
        {
            return MacroResolver.ResolveCurrentPath(format);
        }


        /// <summary>
        /// Resolves the macros in the given text
        /// </summary>
        /// <param name="inputText">Input text to resolve</param>
        public string ResolveMacros(string inputText)
        {
            return MacroResolver.Resolve(inputText);
        }


        /// <summary>
        /// Resolves the discussion macros.
        /// </summary>
        /// <param name="inputText">Text to resolve</param>
        public string ResolveDiscussionMacros(string inputText)
        {
            return DiscussionMacroHelper.ResolveDiscussionMacros(inputText);
        }


        /// <summary>
        /// Returns sitemap XML element for specified type (loc, lastmod, changefreq, priority).
        /// </summary>
        /// <param name="dataitem">Current dataitem object (DataRow or DataRowView object)</param>
        /// <param name="type">Specify xml sitemap type (loc, lastmod, changefreq, priority)</param>
        public string GetSitemapItem(object dataitem, string type)
        {
            // Check whether type is defined
            if ((dataitem != null) && !String.IsNullOrEmpty(type))
            {
                // switch by type
                switch (type.ToLowerInvariant())
                {
                    // LOCATION
                    case "loc":
                        // Get mandatory fields from current dataitem
                        int siteId = ValidationHelper.GetInteger(DataHelper.GetDataContainerItem(dataitem, "NodeSiteID"), 0);
                        string siteName = SiteInfoProvider.GetSiteName(siteId);
                        string aliasPath = ValidationHelper.GetString(DataHelper.GetDataContainerItem(dataitem, "NodeAliasPath"), String.Empty);
                        string urlPath = ValidationHelper.GetString(DataHelper.GetDataContainerItem(dataitem, "DocumentUrlPath"), String.Empty);
                        string culture = ValidationHelper.GetString(DataHelper.GetDataContainerItem(dataitem, "DocumentCulture"), null);

                        // URL
                        string item = GetDocumentUrl(siteName, aliasPath, urlPath, (URLHelper.UseLangPrefixForUrls(siteName) ? culture : null));

                        string domain = RequestContext.CurrentDomain;
                        domain = DocumentURLProvider.EnsureDomainPrefix(domain, siteName);

                        // Absolute URL
                        item = GetAbsoluteUrl(item, domain);
                        // Return location XML node
                        return "<loc>" + HTMLHelper.HTMLEncode(item) + "</loc>";


                    // LAST MODIFICATION
                    case "lastmod":
                        // Get last modification date time
                        DateTime dt = ValidationHelper.GetDateTime(DataHelper.GetDataContainerItem(dataitem, "DocumentModifiedWhen"), DateTime.Now);
                        // Return last modification XML node
                        return "<lastmod>" + HTMLHelper.HTMLEncode(dt.ToString("yyyy-MM-dd")) + "</lastmod>";


                    // CHANGE FREQUENCY
                    case "changefreq":

                        // Try get sitemap settings
                        string[] sitemapSetts = ValidationHelper.GetString(DataHelper.GetDataContainerItem(dataitem, "DocumentSitemapSettings"), String.Empty).Split(';');
                        // Check whether change frequency is defined
                        if ((sitemapSetts.Length == 2) && (!String.IsNullOrEmpty(sitemapSetts[0])))
                        {
                            // Return change frequency lastmod XML node
                            return "<changefreq>" + HTMLHelper.HTMLEncode(sitemapSetts[0]) + "</changefreq>";
                        }
                        break;


                    // PRIORITY
                    case "priority":
                        // Try get sitemap settings
                        sitemapSetts = ValidationHelper.GetString(DataHelper.GetDataContainerItem(dataitem, "DocumentSitemapSettings"), String.Empty).Split(';');
                        // Check whether priority is defined
                        if ((sitemapSetts.Length == 2) && (!String.IsNullOrEmpty(sitemapSetts[1])) && (sitemapSetts[1] != "0.5"))
                        {
                            // Return change frequency priority XML node
                            return "<priority>" + HTMLHelper.HTMLEncode(sitemapSetts[1]) + "</priority>";
                        }
                        break;
                }
            }

            return String.Empty;
        }


        /// <summary>
        /// Limits length of the given plain text.
        /// </summary>
        /// <param name="textObj">Plain text to limit (Text containing HTML tags is not supported)</param>
        /// <param name="maxLength">Maximum text length</param>
        /// <param name="padString">Trimming postfix</param>
        public string LimitLength(object textObj, int maxLength, string padString)
        {
            return LimitLength(textObj, maxLength, padString, false);
        }


        /// <summary>
        /// Limits length of the given plain text.
        /// </summary>
        /// <param name="textObj">Plain text to limit (Text containing HTML tags is not supported)</param>
        /// <param name="maxLength">Maximum text length</param>
        /// <param name="padString">Trimming postfix</param>
        /// <param name="wholeWords">If true, the text won't be cut in the middle of the word</param>
        public string LimitLength(object textObj, int maxLength, string padString, bool wholeWords)
        {
            string text = ValidationHelper.GetString(textObj, "");
            return TextHelper.LimitLength(text, maxLength, padString, wholeWords);
        }


        /// <summary>
        /// Limits line length of the given plain text.
        /// </summary>
        /// <param name="textObj">Plain text to limit (Text containing HTML tags is not supported)</param>
        /// <param name="maxLength">Maximum line length</param>
        public string EnsureMaximumLineLength(object textObj, int maxLength)
        {
            string text = ValidationHelper.GetString(textObj, "");
            return TextHelper.EnsureMaximumLineLength(text, maxLength);
        }


        /// <summary>
        /// Returns URL of the attachment.
        /// </summary>
        /// <param name="attachmentGuid">Attachment GUID.</param>
        /// <param name="attachmentFileName">Name of the attachment file.</param>
        /// <param name="variant">Identifier of the attachment variant.</param>
        public string GetAttachmentUrlByGUID(Guid attachmentGuid, string attachmentFileName, string variant = null)
        {
            var safeFileName = URLHelper.GetSafeFileName(attachmentFileName, null);

            var url = AttachmentURLProvider.GetAttachmentUrl(attachmentGuid, safeFileName, null, variant: variant);

            return UrlResolver.ResolveUrl(url);
        }


        /// <summary>
        /// Returns URL of the attachment.
        /// </summary>
        /// <param name="attachmentFileName">Name of the attachment file.</param>
        /// <param name="nodeAliasPath">Attachment document alias path.</param>
        /// <param name="variant">Identifier of the attachment variant definition.</param>
        public string GetAttachmentUrl(string attachmentFileName, string nodeAliasPath, string variant = null)
        {
            var safeFileName = URLHelper.GetSafeFileName(attachmentFileName, null);
            var url = AttachmentURLProvider.GetAttachmentUrl(safeFileName, nodeAliasPath, null, variant);

            return UrlResolver.ResolveUrl(url);
        }


        /// <summary>
        /// Returns URL of the attachment.
        /// </summary>
        /// <param name="attachmentFileName">Name of the attachment file.</param>
        /// <param name="attachmentDocumentId">Attachment document ID.</param>
        /// <param name="variant">Identifier of the attachment variant definition.</param>
        public string GetAttachmentUrlByDocumentId(string attachmentFileName, int attachmentDocumentId, string variant = null)
        {
            var nodeAliasPath = GetCachedNodeAliasPath(attachmentDocumentId);

            return GetAttachmentUrl(attachmentFileName, nodeAliasPath, variant);
        }


        private static string GetCachedNodeAliasPath(int attachmentDocumentId)
        {
            if (attachmentDocumentId == 0)
            {
                return null;
            }

            string key = "Attachment_NodeAliasPath_" + attachmentDocumentId;
            string nodeAliasPath = ValidationHelper.GetString(RequestStockHelper.GetItem(key), null);
            if (nodeAliasPath != null)
            {
                return nodeAliasPath;
            }

            var tree = new TreeProvider();
            var node = tree.SelectSingleDocument(attachmentDocumentId, false, "NodeAliasPath");
            if (node == null)
            {
                return null;
            }

            RequestStockHelper.Add(key, node.NodeAliasPath);

            return node.NodeAliasPath;
        }


        /// <summary>
        /// Returns URL of the currently rendered document.
        /// </summary>
        /// <param name="siteName">Name of the site</param>
        /// <param name="nodeAliasPath">Alias path of the node</param>
        /// <param name="documentUrlPath">Url path of the document</param>
        /// <param name="culture">Culture of the the document</param>
        public string GetDocumentUrl(object siteName, object nodeAliasPath, object documentUrlPath, string culture = null)
        {
            string site = ValidationHelper.GetString(siteName, "");
            if (site == SiteContext.CurrentSiteName)
            {
                site = null;
            }

            // Get culture alias
            string culturePrefix = null;
            if (culture != null)
            {
                var cultureInfo = CultureInfoProvider.GetCultureInfo(culture);
                if (cultureInfo != null)
                {
                    culturePrefix = !String.IsNullOrEmpty(cultureInfo.CultureAlias) ? cultureInfo.CultureAlias : culture;
                }
            }

            return UrlResolver.ResolveUrl(DocumentURLProvider.GetUrl(ValidationHelper.GetString(nodeAliasPath, null), ValidationHelper.GetString(documentUrlPath, null), site, culturePrefix));
        }


        /// <summary>
        /// Trims the site prefix from user name(if any prefix found)
        /// </summary>
        /// <param name="userName">User name</param>
        public string TrimSitePrefix(object userName)
        {
            if (userName != null)
            {
                return UserInfoProvider.TrimSitePrefix(userName.ToString());
            }

            return String.Empty;
        }


        /// <summary>
        /// Returns URL of the given document (for use with document selector).
        /// </summary>
        /// <param name="nodeGuid">Node GUID</param>
        /// <param name="nodeAlias">Node alias</param>
        public string GetDocumentUrl(object nodeGuid, object nodeAlias)
        {
            Guid guid = ValidationHelper.GetGuid(nodeGuid, Guid.Empty);
            if (guid == Guid.Empty)
            {
                return "";
            }
            else
            {
                return DocumentURLProvider.GetPermanentDocUrl(guid, nodeAlias.ToString(), SiteContext.CurrentSiteName);
            }
        }


        /// <summary>
        /// Returns URL of the given document.
        /// </summary>
        /// <param name="documentIdObj">Document ID</param>
        public string GetDocumentUrl(object documentIdObj)
        {
            string key = "DocumentUrl_" + documentIdObj;
            string documentUrl = ValidationHelper.GetString(RequestStockHelper.GetItem(key), string.Empty);
            if (documentUrl == "")
            {
                int documentId = ValidationHelper.GetInteger(documentIdObj, 0);
                if (documentId != 0)
                {
                    TreeNode node = DocumentHelper.GetDocument(documentId, null);
                    if (node != null)
                    {
                        string url = UrlResolver.ResolveUrl(DocumentURLProvider.GetUrl(node));
                        RequestStockHelper.Add(key, url);
                        documentUrl = url;
                    }
                }
            }

            return documentUrl;
        }


        /// <summary>
        /// Gets document CSS class comparing the current document node alias path.
        /// </summary>
        /// <param name="nodeAliasPath">Node alias path</param>
        /// <param name="cssClass">CSS class</param>
        /// <param name="selectedCssClass">Selected CSS class</param>
        /// <returns>Returns selectedCssClass if given node alias path matches the current document node alias path. Otherwise returns cssClass.</returns>
        public string GetDocumentCssClass(string nodeAliasPath, string cssClass, string selectedCssClass)
        {
            if (DocumentContext.CurrentDocument != null)
            {
                return IsCurrentDocument(nodeAliasPath) ? selectedCssClass : cssClass;
            }

            return cssClass;
        }


        /// <summary>
        /// Returns country displayname based on its codename.
        /// </summary>
        /// <param name="countryCode">Code name of country</param>
        /// <param name="appendState">If true, state code is appended to country name</param>
        /// <param name="format">Format for appending state to country. Default format is '{state}, {country}'</param>
        public object GetCountryDisplayName(String countryCode, bool appendState = true, string format = null)
        {
            string countryName = String.Empty;
            string stateName = String.Empty;

            if (!string.IsNullOrEmpty(countryCode))
            {
                string[] country = countryCode.Split(';');

                if (country.Length > 0)
                {
                    CountryInfo ci = CountryInfoProvider.GetCountryInfo(country[0]);
                    if (ci != null)
                    {
                        countryName = ResHelper.LocalizeString(ci.CountryDisplayName);
                        if (appendState && (country.Length > 1))
                        {
                            StateInfo si = StateInfoProvider.GetStateInfo(country[1]);
                            if (si != null)
                            {
                                stateName = ResHelper.LocalizeString(si.StateDisplayName);
                            }
                        }
                    }
                }

                if (!String.IsNullOrEmpty(countryName) && !String.IsNullOrEmpty(stateName))
                {
                    format = format ?? "{0}, {1}";
                    countryName = String.Format(format, countryName, stateName);
                }
            }

            return countryName;
        }


        /// <summary>
        /// Indicates if the document is current document.
        /// </summary>
        /// <param name="nodeAliasPath">Node alias path</param>
        public bool IsCurrentDocument(string nodeAliasPath)
        {
            if (DocumentContext.CurrentDocument != null)
            {
                return (CMSString.Compare(DocumentContext.CurrentDocument.NodeAliasPath, nodeAliasPath, true) == 0);
            }

            return false;
        }


        /// <summary>
        /// Indicates if the document is on selected path.
        /// </summary>
        /// <param name="nodeAliasPath">Node alias path</param>
        public bool IsDocumentOnSelectedPath(string nodeAliasPath)
        {
            if (DocumentContext.CurrentDocument == null)
            {
                return false;
            }

            // Ensure trailing slashes
            nodeAliasPath = nodeAliasPath.TrimEnd('/') + "/";
            var currentNodeAliasPath = DocumentContext.CurrentDocument.NodeAliasPath + "/";

            return currentNodeAliasPath.StartsWith(nodeAliasPath, StringComparison.InvariantCultureIgnoreCase);
        }


        /// <summary>
        /// Indicates if the document is image.
        /// </summary>
        /// <param name="extension">Document extension</param>
        public bool IsImageDocument(string extension)
        {
            return ImageHelper.IsImage(extension);
        }


        /// <summary>
        /// Returns URL of the specified forum post.
        /// </summary>
        /// <param name="forumId">Forum id</param>
        /// <param name="postIdPath">Post id path</param>
        public string GetForumPostUrl(object postIdPath, object forumId)
        {
            string pIdPath = ValidationHelper.GetString(postIdPath, string.Empty);
            int fId = ValidationHelper.GetInteger(forumId, 0);

            return ModuleCommands.ForumsGetPostUrl(pIdPath, fId, true);
        }


        /// <summary>
        /// Returns URL of the specified media file.
        /// </summary>
        /// <param name="fileGUID">File GUID</param>
        /// <param name="fileName">File name</param>
        public string GetMediaFileUrl(object fileGUID, object fileName)
        {
            Guid fGUID = ValidationHelper.GetGuid(fileGUID, Guid.Empty);
            string fName = ValidationHelper.GetString(fileName, String.Empty);

            return ModuleCommands.MediaLibraryGetMediaFileUrl(fGUID, fName);
        }


        /// <summary>
        /// Returns URL of the specified meta file.
        /// </summary>
        /// <param name="fileGUID">Meta file GUID</param>
        /// <param name="fileName">Meta file name</param>
        public string GetMetaFileUrl(object fileGUID, object fileName)
        {
            Guid guid = ValidationHelper.GetGuid(fileGUID, Guid.Empty);
            string name = ValidationHelper.GetString(fileName, String.Empty);

            return UrlResolver.ResolveUrl(MetaFileInfoProvider.GetMetaFileUrl(guid, name));
        }


        /// <summary>
        /// Returns URL of the message board document.
        /// </summary>
        /// <param name="documentIdObj">Document ID</param>
        public string GetMessageBoardUrl(object documentIdObj)
        {
            return GetDocumentUrl(documentIdObj);
        }


        /// <summary>
        /// Returns URL of the blog comment document.
        /// </summary>
        /// <param name="documentIdObj">Document ID</param>
        public string GetBlogCommentUrl(object documentIdObj)
        {
            return GetDocumentUrl(documentIdObj);
        }


        /// <summary>
        /// Returns HTML markup representing icon.
        /// </summary>
        /// <param name="attachmentGuid">Attachment GUID.</param>
        /// <param name="page">Page object.</param>
        /// <param name="iconSize">Icon size for font icon.</param>
        /// <param name="tooltip">Tooltip.</param>
        /// <param name="additionalAttributes">Additional HTML parameters.</param>
        public string GetAttachmentIcon(object attachmentGuid, Page page, FontIconSizeEnum iconSize = FontIconSizeEnum.NotDefined, string tooltip = "", string additionalAttributes = "")
        {
            // Get attachment
            Guid attGuid = ValidationHelper.GetGuid(attachmentGuid, Guid.Empty);
            AttachmentInfo ai = AttachmentInfoProvider.GetAttachmentInfoWithoutBinary(attGuid, SiteContext.CurrentSiteName);
            if (ai != null)
            {
                return GetFileIcon(ai.AttachmentExtension, page, iconSize, tooltip, additionalAttributes);
            }
            return String.Empty;
        }


        /// <summary>
        /// Returns HTML markup representing icon.
        /// </summary>
        /// <param name="fileExtension">File extension.</param>
        /// <param name="page">Page object.</param>
        /// <param name="iconSize">Icon size for font icon.</param>
        /// <param name="tooltip">Tooltip.</param>
        /// <param name="additionalAttributes">Additional HTML parameters.</param>
        public string GetFileIcon(object fileExtension, Page page, FontIconSizeEnum iconSize = FontIconSizeEnum.NotDefined, string tooltip = "", string additionalAttributes = "")
        {
            return UIHelper.GetFileIcon(page, ValidationHelper.GetString(fileExtension, null), iconSize, tooltip, additionalAttributes);
        }


        /// <summary>
        /// Returns font icon class for specified file extension.
        /// </summary>
        /// <param name="extension">File extension</param>
        public string GetFileIconClass(string extension)
        {
            if (string.IsNullOrEmpty(extension))
            {
                return string.Empty;
            }

            return UIHelper.GetFileIconClass(extension);
        }


        /// <summary>
        /// Returns a complete HTML code of the image.
        /// </summary>
        /// <param name="attachmentFileName">Name of the attachment file</param>
        /// <param name="attachmentGuid">Attachment GUID</param>
        /// <param name="imageUrl">Image url</param>
        /// <param name="maxSideSize">Image max. side size</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <param name="alt">Image alternation text</param>
        private string GetImage(object attachmentFileName, object attachmentGuid, object imageUrl, object maxSideSize, object width, object height, object alt)
        {
            int iMaxSideSize = ValidationHelper.GetInteger(maxSideSize, 0);
            int iWidth = ValidationHelper.GetInteger(width, 0);
            int iHeight = ValidationHelper.GetInteger(height, 0);

            var guid = ValidationHelper.GetGuid(attachmentGuid, Guid.Empty);
            var fileName = ValidationHelper.GetString(attachmentFileName, "");
            var sAlt = ValidationHelper.GetString(alt, "");

            // Get image url
            string url = attachmentGuid != null ? GetAttachmentUrlByGUID(guid, fileName) : ValidationHelper.GetString(imageUrl, "");

            // Add max side size
            if (iMaxSideSize > 0)
            {
                url = URLHelper.AddParameterToUrl(url, "maxSideSize", "" + iMaxSideSize);
            }

            // Add width
            if (iWidth > 0)
            {
                url = URLHelper.AddParameterToUrl(url, "width", "" + iWidth);
            }

            // Add height
            if (iHeight > 0)
            {
                url = URLHelper.AddParameterToUrl(url, "height", "" + iHeight);
            }

            return "<img alt=\"" + sAlt + "\" src=\"" + HTMLHelper.EncodeForHtmlAttribute(url) + "\" />";
        }


        /// <summary>
        /// Returns a complete HTML code of the image.
        /// </summary>
        /// <param name="attachmentFileName">Name of the attachment file</param>
        /// <param name="attachmentGuid">Attachment GUID</param>
        /// <param name="maxSideSize">Image max. side size</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <param name="alt">Image alternation text</param>
        public string GetImage(object attachmentFileName, object attachmentGuid, object maxSideSize = null, object width = null, object height = null, object alt = null)
        {
            return GetImage(attachmentFileName, attachmentGuid, null, maxSideSize, width, height, alt);
        }


        /// <summary>
        /// Returns a complete HTML code of the image.
        /// </summary>
        /// <param name="attachmentFileName">Name of the attachment file</param>
        /// <param name="attachmentGuid">Attachment GUID</param>
        /// <param name="maxSideSize">Image max. side size</param>
        public string GetImage(object attachmentFileName, object attachmentGuid, int maxSideSize)
        {
            return GetImage(attachmentFileName, attachmentGuid, null, maxSideSize, null, null, null);
        }


        /// <summary>
        /// Returns a complete HTML code of the image.
        /// </summary>
        /// <param name="attachmentFileName">Name of the attachment file</param>
        /// <param name="attachmentGuid">Attachment GUID</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        public string GetImage(object attachmentFileName, object attachmentGuid, int width, int height)
        {
            return GetImage(attachmentFileName, attachmentGuid, null, null, width, height, null);
        }


        /// <summary>
        /// Returns a complete HTML code of the image that is specified by the given url.
        /// </summary>
        /// <param name="imageUrl">Image url</param>
        /// <param name="maxSideSize">Image max. side size</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <param name="alt">Image alternation text</param>
        public string GetImageByUrl(object imageUrl, object maxSideSize = null, object width = null, object height = null, object alt = null)
        {
            return GetImage(null, null, imageUrl, maxSideSize, width, height, alt);
        }


        /// <summary>
        /// Returns a complete HTML code of the image that is specified by the given url.
        /// </summary>
        /// <param name="imageUrl">Image url</param>
        /// <param name="maxSideSize">Image max. side size</param>
        public string GetImageByUrl(object imageUrl, int maxSideSize)
        {
            return GetImage(null, null, imageUrl, maxSideSize, null, null, null);
        }


        /// <summary>
        /// Returns a complete HTML code of the image that is specified by the given url.
        /// </summary>
        /// <param name="imageUrl">Image url</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        public string GetImageByUrl(object imageUrl, int width, int height)
        {
            return GetImage(null, null, imageUrl, null, width, height, null);
        }


        /// <summary>
        /// Gets the editable image value.
        /// </summary>
        /// <param name="url">Editable image url</param>
        public string GetEditableImageUrl(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                string imgPath = HTMLHelper.StripTags(url, false);
                // Check if content is URL
                if (ValidationHelper.IsURL(imgPath))
                {
                    return imgPath;
                }
            }
            return null;
        }


        /// <summary>
        /// Returns a complete HTML code of the image that is specified by editable image ID.
        /// </summary>
        /// <param name="url">Editable image url</param>
        /// <param name="maxSideSize">Image max. side size</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <param name="alt">Image alternation text</param>
        public string GetEditableImage(string url, object maxSideSize, object width, object height, object alt)
        {
            string imgUrl = GetEditableImageUrl(url);
            return GetImageByUrl(imgUrl, maxSideSize, width, height, alt);
        }


        /// <summary>
        /// Returns a complete HTML code of the link to the currently rendered document.
        /// </summary>
        /// <param name="siteName">Name of the site</param>
        /// <param name="nodeAliasPath">Alias path of the node</param>
        /// <param name="documentUrlPath">Url path of the document</param>
        /// <param name="documentName">Name of the document</param>
        /// <param name="encodeName">If true, the document name is encoded</param>
        public string GetDocumentLink(object siteName, object nodeAliasPath, object documentUrlPath, object documentName, bool encodeName)
        {
            string name = ValidationHelper.GetString(documentName, "");
            if (encodeName)
            {
                name = HTMLHelper.HTMLEncode(name);
            }
            return "<a href=\"" + GetDocumentUrl(siteName, nodeAliasPath, documentUrlPath) + "\">" + name + "</a>";
        }


        /// <summary>
        /// Returns URL for the specified aliasPath and urlPath (preferable).
        /// </summary>
        /// <param name="aliasPath">Alias path</param>
        /// <param name="urlPath">URL Path</param>
        public string GetUrl(object aliasPath, object urlPath)
        {
            return UrlResolver.ResolveUrl(DocumentURLProvider.GetUrl(ValidationHelper.GetString(aliasPath, null), ValidationHelper.GetString(urlPath, null)));
        }


        /// <summary>
        /// Returns URL for the specified aliasPath and urlPath (preferable).
        /// </summary>
        /// <param name="aliasPath">Alias path</param>
        /// <param name="urlPath">URL Path</param>
        /// <param name="siteName">Site name</param>
        public string GetUrl(object aliasPath, object urlPath, object siteName)
        {
            return UrlResolver.ResolveUrl(DocumentURLProvider.GetUrl(ValidationHelper.GetString(aliasPath, null), ValidationHelper.GetString(urlPath, null), ValidationHelper.GetString(siteName, null)));
        }


        /// <summary>
        /// Returns resolved (i.e. absolute) URL of data item (page) that currently being processed. Method reflects page navigation settings.
        /// </summary>
        /// <param name="data">Page data</param>
        /// <param name="resolver">Macro resolver</param>
        public string GetNavigationUrl(object data, MacroResolver resolver = null)
        {
            return DocumentURLProvider.GetNavigationUrl(DataHelper.GetDataContainer(data), resolver);
        }


        /// <summary>
        /// Returns date from the provided date-time value.
        /// </summary>
        /// <param name="dateTime">Date-time</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="cultureCode">Culture code</param>
        public string GetDate(object dateTime, string defaultValue = "", string cultureCode = "")
        {
            if (!DataHelper.IsEmpty(dateTime))
            {
                DateTime date = ValidationHelper.GetDateTime(dateTime, DateTimeHelper.ZERO_TIME);
                if (date != DateTimeHelper.ZERO_TIME)
                {
                    string cultCode = ValidationHelper.GetString(cultureCode, String.Empty);

                    if (String.IsNullOrEmpty(cultCode))
                    {
                        // Return date
                        return date.ToString("d");
                    }
                    else
                    {
                        // Return culture specific date
                        CultureInfo ci = CultureInfo.CreateSpecificCulture(cultCode);
                        return date.ToString("d", ci);
                    }
                }
            }

            // Return default value
            return defaultValue;
        }


        /// <summary>
        /// Returns absolute URL from relative path.
        /// </summary>
        /// <param name="relativeUrl">Relative URL</param>
        public string GetAbsoluteUrl(string relativeUrl)
        {
            return GetAbsoluteUrl(relativeUrl, (SiteInfoIdentifier)null);
        }


        /// <summary>
        /// Returns absolute URL from relative path.
        /// </summary>
        /// <param name="relativeUrl">Relative URL</param>
        /// <param name="site">Site identifier (name or ID)</param>
        public string GetAbsoluteUrl(string relativeUrl, SiteInfoIdentifier site)
        {
            // Get site domain
            string domainName = null;
            if (site != null)
            {
                // Get site domain only for different site that the current site because of possible current site domain alias (obtained from request context).
                var si = SiteInfoProvider.GetSiteInfo(site.ObjectID);
                if ((si != null) && ((si.SiteID != SiteContext.CurrentSiteID) || String.IsNullOrEmpty(RequestContext.FullDomain)))
                {
                    domainName = si.DomainName;
                }
            }

            return GetAbsoluteUrl(relativeUrl, domainName);
        }


        /// <summary>
        /// Returns absolute URL from relative path.
        /// </summary>
        /// <param name="relativeUrl">Relative URL</param>
        /// <param name="domainName">Domain name</param>
        private string GetAbsoluteUrl(string relativeUrl, string domainName)
        {
            return URLHelper.GetAbsoluteUrl(relativeUrl, domainName);
        }


        /// <summary>
        /// Returns nonEmptyResult if value is null or empty, else returns emptyResult.
        /// </summary>
        /// <param name="value">Conditional value</param>
        /// <param name="emptyResult">Empty value result</param>
        /// <param name="nonEmptyResult">Non empty value result</param>
        public object IfEmpty(object value, object emptyResult, object nonEmptyResult)
        {
            if (((value is DateTime) && value.Equals(DateTimeHelper.ZERO_TIME))
                || ((value is Guid) && value.Equals(Guid.Empty))
                || String.IsNullOrEmpty(ValidationHelper.GetString(value, null)))
            {
                return emptyResult;
            }

            return nonEmptyResult;
        }


        /// <summary>
        /// Returns nonEmptyResult if specified data source is null or empty, else returns emptyResult.
        /// </summary>
        /// <param name="value">Conditional value</param>
        /// <param name="emptyResult">Empty value result</param>
        /// <param name="nonEmptyResult">Non empty value result</param>
        public object IfDataSourceIsEmpty(object value, object emptyResult, object nonEmptyResult)
        {
            if (DataHelper.DataSourceIsEmpty(value))
            {
                return emptyResult;
            }
            else
            {
                return nonEmptyResult;
            }
        }


        /// <summary>
        /// Returns isImage value if file is image.
        /// </summary>
        /// <param name="attachmentGuid">Attachment GUID</param>
        /// <param name="isImage">Is image value</param>
        /// <param name="notImage">Is not image value</param>
        public object IfImage(object attachmentGuid, object isImage, object notImage)
        {
            Guid attGuid = ValidationHelper.GetGuid(attachmentGuid, Guid.Empty);

            if (attGuid != Guid.Empty)
            {
                AttachmentInfo ai = AttachmentInfoProvider.GetAttachmentInfoWithoutBinary(attGuid, SiteContext.CurrentSiteName);
                if (ai != null)
                {
                    if (ImageHelper.IsImage(Path.GetExtension(ai.AttachmentName).TrimStart('.')))
                    {
                        return isImage;
                    }
                }
            }

            return notImage;
        }


        /// <summary>
        /// Format date time.
        /// </summary>
        /// <param name="datetime">Date time object</param>
        /// <param name="format">Format string (If not set, the date time is formated due to current culture settings.)</param>
        public object FormatDateTime(object datetime, string format)
        {
            DateTime dt = ValidationHelper.GetDateTime(datetime, DateTimeHelper.ZERO_TIME);
            if (dt != DateTimeHelper.ZERO_TIME)
            {
                return dt.ToString(format);
            }

            return "";
        }


        /// <summary>
        /// Format date without time part.
        /// </summary>
        /// <param name="datetime">Date time object</param>
        public object FormatDate(object datetime)
        {
            DateTime dt = ValidationHelper.GetDateTime(datetime, DateTimeHelper.ZERO_TIME);
            if (dt != DateTimeHelper.ZERO_TIME)
            {
                // Get current culture info
                CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
                return dt.ToString(currentCulture.DateTimeFormat.ShortDatePattern);
            }

            return "";
        }


        /// <summary>
        /// Transformation "if" statement for guid, int, string, double, decimal, boolean, DateTime
        /// The type of compare depends on comparable value (second parameter)
        /// If both values are NULL, method returns false result.
        /// </summary>
        /// <param name="value">First value</param>
        /// <param name="comparableValue">Second value</param>
        /// <param name="falseResult">False result</param>
        /// <param name="trueResult">True result</param>
        public object IfCompare(object value, object comparableValue, object falseResult, object trueResult)
        {
            if ((value != null && comparableValue != null))
            {
                if (comparableValue is Guid)
                {
                    if ((ValidationHelper.GetGuid(value, Guid.NewGuid()) == ((Guid)comparableValue)))
                    {
                        return trueResult;
                    }
                }

                if (comparableValue is int)
                {
                    if ((ValidationHelper.GetInteger(value, 0)) == ((int)comparableValue))
                    {
                        return trueResult;
                    }
                }

                if (comparableValue is string)
                {
                    if ((ValidationHelper.GetString(value, "")) == ((string)comparableValue))
                    {
                        return trueResult;
                    }
                }

                if (comparableValue is bool)
                {
                    if ((ValidationHelper.GetBoolean(value, false)) == ((bool)comparableValue))
                    {
                        return trueResult;
                    }
                }

                if (comparableValue is double)
                {
                    if ((ValidationHelper.GetDouble(value, 0.0)).Equals((double)comparableValue))
                    {
                        return trueResult;
                    }
                }

                if (comparableValue is decimal)
                {
                    if ((ValidationHelper.GetDecimal(value, 0)).Equals((decimal)comparableValue))
                    {
                        return trueResult;
                    }
                }

                if (comparableValue is DateTime)
                {
                    if ((ValidationHelper.GetDateTime(value, DateTimeHelper.ZERO_TIME)) == ((DateTime)comparableValue))
                    {
                        return trueResult;
                    }
                }
            }

            return falseResult;
        }


        /// <summary>
        /// If input value is evaluated as True then 'true result' is returned, otherwise 'false result' is returned.
        /// </summary>
        /// <param name="value">Value to be evaluated</param>
        /// <param name="trueResult">True result</param>\
        /// <param name="falseResult">False result</param>
        public object If(object value, object trueResult, object falseResult)
        {
            bool val = ValidationHelper.GetBoolean(value, false);

            return val ? trueResult : falseResult;
        }


        /// <summary>
        /// Returns encoded text.
        /// </summary>
        /// <param name="text">Text to be encoded</param>
        public string HTMLEncode(string text)
        {
            return HTMLHelper.HTMLEncode(text);
        }


        /// <summary>
        /// Remove all types of discussion macros from text.
        /// </summary>
        /// <param name="inputText">Text containing macros to be removed</param>
        public string RemoveDiscussionMacros(object inputText)
        {
            return DiscussionMacroResolver.RemoveTags(inputText as String);
        }


        /// <summary>
        /// Remove all dynamic controls macros from text.
        /// </summary>
        /// <param name="inputText">Text containing macros to be removed</param>
        public string RemoveDynamicControls(object inputText)
        {
            return ControlsHelper.RemoveDynamicControls(inputText as String);
        }


        /// <summary>
        /// Remove HTML tags from text.
        /// </summary>
        /// <param name="inputText">Text containing tags to be removed</param>
        public string StripTags(object inputText)
        {
            return HTMLHelper.StripTags(inputText as String, false);
        }


        /// <summary>
        /// Returns HTML encoded value if value is string and it should be encoded(it depends on value of CMSHTMLEncodeEval key in configuration file).
        /// </summary>
        /// <param name="value">Value to encode</param>
        public ReturnType EnsureValueHTMLEncode<ReturnType>(object value)
        {
            // Value is string and it should be HTML encoded
            if ((typeof(ReturnType) == typeof(string)) && HTMLEncodeEval)
            {
                value = HTMLEncode(ValidationHelper.GetString(value, string.Empty));
                return (ReturnType)value;
            }

            return ValidationHelper.GetValue<ReturnType>(value);
        }


        /// <summary>
        /// Encodes input string to be used in javascript code and encapsulates it with "'".
        /// </summary>
        /// <param name="input">Input string</param>
        public string JSEncode(string input)
        {
            return ScriptHelper.GetString(input);
        }

        #endregion


        #region "UI image methods"

        /// <summary>
        /// Gets UI image resolved path.
        /// </summary>
        /// <param name="imagePath">Partial image path starting from ~/App_Themes/(Skin_Folder)/Images/ (e.g. '/CMSModules/CMS_MediaLibrary/module.png')</param>
        /// <param name="page">Page object</param>
        public virtual string GetUIImageUrl(string imagePath, Page page)
        {
            return GetUIImageUrl(imagePath, false, page);
        }


        /// <summary>
        /// Gets UI image resolved path.
        /// </summary>
        /// <param name="imagePath">Partial image path starting from ~/App_Themes/(Skin_Folder)/Images/ (e.g. '/CMSModules/CMS_MediaLibrary/module.png')</param>
        /// <param name="isLiveSite">Indicates if URL should be returned for live site</param>
        /// <param name="page">Page object</param>
        public virtual string GetUIImageUrl(string imagePath, bool isLiveSite, Page page)
        {
            return GetUIImageUrl(imagePath, isLiveSite, true, page);
        }


        /// <summary>
        /// Gets UI image path.
        /// </summary>
        /// <param name="imagePath">Partial image path starting from ~/App_Themes/(Skin_Folder)/Images/ (e.g. '/CMSModules/CMS_MediaLibrary/module.png')</param>
        /// <param name="isLiveSite">Indicates if URL should be returned for live site</param>
        /// <param name="ensureDefaultTheme">Indicates if default theme should be always used</param>
        /// <param name="page">Page object</param>
        public virtual string GetUIImageUrl(string imagePath, bool isLiveSite, bool ensureDefaultTheme, Page page)
        {
            return UIHelper.GetImageUrl(page, imagePath, isLiveSite, ensureDefaultTheme);
        }

        #endregion


        #region "Time zones"

        /// <summary>
        /// Returns date time string according to user or current site time zone.
        /// </summary>
        /// <param name="dateTime">Date time</param>
        /// <param name="userName">User name</param>
        public string GetCurrentDateTimeString(object dateTime, string userName)
        {
            UserInfo user = null;
            if (!String.IsNullOrEmpty(userName))
            {
                // Get user according to user name
                user = UserInfoProvider.GetUserInfo(userName);
            }

            // If userName is null, return current site time zone, otherwise return user time zone
            return TimeZoneHelper.ConvertToUserTimeZone(Convert.ToDateTime(dateTime), true, user, SiteContext.CurrentSite);
        }


        /// <summary>
        /// Returns current user date time DateTime according to user time zone.
        /// </summary>
        /// <param name="dateTime">Date time</param>
        public DateTime GetUserDateTime(object dateTime)
        {
            return TimeZoneHelper.ConvertToUserDateTime(Convert.ToDateTime(dateTime), MembershipContext.AuthenticatedUser);
        }


        /// <summary>
        /// Returns site date time according to site time zone.
        /// </summary>
        /// <param name="dateTime">Date time</param>
        public DateTime GetSiteDateTime(object dateTime)
        {
            return TimeZoneHelper.ConvertToSiteDateTime(Convert.ToDateTime(dateTime), SiteContext.CurrentSite);
        }


        /// <summary>
        /// Returns date time with dependence on selected time zone.
        /// </summary>
        /// <param name="dateTime">DateTime to convert (server time zone)</param>
        /// <param name="timeZoneName">Time zone code name</param>
        public DateTime GetCustomDateTime(object dateTime, string timeZoneName)
        {
            TimeZoneInfo tzi = TimeZoneInfoProvider.GetTimeZoneInfo(timeZoneName);
            if (tzi != null)
            {
                return TimeZoneHelper.ConvertTimeZoneDateTime(Convert.ToDateTime(dateTime), TimeZoneHelper.ServerTimeZone, tzi);
            }
            return ValidationHelper.GetDateTime(dateTime, DateTimeHelper.ZERO_TIME);
        }


        /// <summary>
        /// Returns date time with dependence on current ITimeZone manager time zone settings.
        /// </summary>
        /// <param name="control">Control containing ITimeZoneManager</param>
        /// <param name="dateTime">DateTime to convert (server time zone)</param>
        public DateTime GetDateTime(Control control, object dateTime)
        {
            return GetDateTime(control, ValidationHelper.GetDateTime(dateTime, DateTimeHelper.ZERO_TIME));
        }


        /// <summary>
        /// Returns date time with dependence on current ITimeZone manager time zone settings.
        /// </summary>
        /// <param name="control">Control containing ITimeZoneManager</param>
        /// <param name="dateTime">DateTime to convert (server time zone)</param>
        public DateTime GetDateTime(Control control, DateTime dateTime)
        {
            return TimeZoneUIMethods.GetDateTimeForControl(control, dateTime);
        }


        /// <summary>
        /// Returns string representation of date time with dependence on current ITimeZone manager
        /// time zone settings.
        /// </summary>
        /// <param name="control">Control containing ITimeZoneManager</param>
        /// <param name="dateTime">DateTime to convert (server time zone)</param>
        /// <param name="showTooltip">Wraps date in span with tooltip</param>
        public string GetDateTimeString(Control control, object dateTime, bool showTooltip)
        {
            DateTime time = ValidationHelper.GetDateTime(dateTime, DateTimeHelper.ZERO_TIME);
            if (time == DateTimeHelper.ZERO_TIME)
            {
                return String.Empty;
            }

            TimeZoneInfo tzi;
            string result = TimeZoneUIMethods.GetDateTimeForControl(control, time, out tzi).ToString();
            if (tzi != null)
            {
                result += " " + TimeZoneHelper.GetUTCStringOffset(tzi);

                if (showTooltip)
                {
                    result = "<span title=\"" + HTMLEncode(TimeZoneHelper.GetUTCLongStringOffset(tzi)) + "\">" + result + "</span>";
                }
            }
            return result;
        }

        #endregion


        #region "Community"

        /// <summary>
        /// Returns age according to DOB. If DOB is not set, returns unknownAge string.
        /// </summary>
        /// <param name="dateOfBirth">Date of birth</param>
        /// <param name="unknownAge">Text which is returned when no DOB is given</param>
        public string GetAge(object dateOfBirth, string unknownAge)
        {
            DateTime dob = ValidationHelper.GetDateTime(dateOfBirth, DateTimeHelper.ZERO_TIME);
            DateTime today = DateTime.Today;

            if (dob == DateTimeHelper.ZERO_TIME)
            {
                return unknownAge;
            }

            // Get year difference
            int years = today.Year - dob.Year;

            // Birthday hasn't come yet this year
            if ((today.Month < dob.Month) || ((today.Month == dob.Month) && (today.Day < dob.Day)))
            {
                years--;
            }

            return years.ToString();
        }


        /// <summary>
        /// Returns gender of the user.
        /// </summary>
        /// <param name="genderObj">Gender of the user (0/1/2 = N/A / Male / Female)</param>
        public string GetGender(object genderObj)
        {
            UserGenderEnum gender = (UserGenderEnum)ValidationHelper.GetInteger(genderObj, 0);

            switch (gender)
            {
                case UserGenderEnum.Male:
                    return ResHelper.GetString("general.male");

                case UserGenderEnum.Female:
                    return ResHelper.GetString("general.female");

                default:
                    return ResHelper.GetString("general.na");
            }
        }


        /// <summary>
        /// Returns group profile URL.
        /// </summary>
        /// <param name="groupNameObj">Group name object</param>
        public string GetGroupProfileUrl(object groupNameObj)
        {
            return GetGroupProfileUrl(groupNameObj, SiteContext.CurrentSiteName);
        }


        /// <summary>
        /// Returns group profile URL.
        /// </summary>
        /// <param name="groupNameObj">Group name object</param>
        /// <param name="siteName">Name of the site</param>
        public string GetGroupProfileUrl(object groupNameObj, string siteName)
        {
            string groupName = ValidationHelper.GetString(groupNameObj, "");
            return UrlResolver.ResolveUrl(DocumentURLProvider.GetUrl(ModuleCommands.CommunityGetGroupProfilePath(groupName, siteName)));
        }


        /// <summary>
        /// Returns member profile URL.
        /// </summary>
        /// <param name="memberNameObj">Member name object</param>
        public string GetMemberProfileUrl(object memberNameObj)
        {
            return GetMemberProfileUrl(memberNameObj, SiteContext.CurrentSiteName);
        }


        /// <summary>
        /// Returns member profile URL.
        /// </summary>
        /// <param name="memberNameObj">Member name object</param>
        /// <param name="siteName">Name of the site</param>
        public string GetMemberProfileUrl(object memberNameObj, string siteName)
        {
            string memberName = ValidationHelper.GetString(memberNameObj, "");
            return UrlResolver.ResolveUrl(DocumentURLProvider.GetUrl(ModuleCommands.CommunityGetMemberProfilePath(memberName, siteName)));
        }


        /// <summary>
        /// Returns user profile URL.
        /// </summary>
        /// <param name="userNameObj">User name object</param>
        public string GetUserProfileURL(object userNameObj)
        {
            return GetUserProfileURL(userNameObj, SiteContext.CurrentSiteName);
        }


        /// <summary>
        /// Returns user profile URL.
        /// </summary>
        /// <param name="userNameObj">User name object</param>
        /// <param name="siteName">Name of the site</param>
        public string GetUserProfileURL(object userNameObj, string siteName)
        {
            string userName = ValidationHelper.GetString(userNameObj, "");
            string profileUrl = SettingsKeyInfoProvider.GetValue(siteName + ".CMSMemberProfilePath");

            if (!String.IsNullOrEmpty(profileUrl))
            {
                // Resolve well-known wildcards for UserName and CultureCode
                profileUrl = DocumentURLProvider.GetUrl(profileUrl.ToLowerInvariant().Replace("{username}", userName));
                var cultureCode = LocalizationContext.CurrentCulture;
                profileUrl = profileUrl.Replace("{culturecode}", (cultureCode != null) ? cultureCode.CultureCode : String.Empty);
                profileUrl = UrlResolver.ResolveUrl(profileUrl);
            }
            else
            {
                profileUrl = String.Empty;
            }

            return profileUrl;
        }


        /// <summary>
        /// Returns user avatar image.
        /// </summary>
        /// <param name="avatarGuid">Avatar GUID</param>
        /// <param name="userGender">Avatar gender</param>
        /// <param name="maxSideSize">Max side size</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <param name="alt">Image alternate text</param>
        public string GetUserAvatarImageByGUID(object avatarGuid, object userGender, int maxSideSize, int width, int height, object alt)
        {
            return AvatarInfoProvider.GetUserAvatarImageByGUID(avatarGuid, userGender, maxSideSize, width, height, alt);
        }


        /// <summary>
        /// Returns avatar or gravatar image url, if it is not defined returns gender dependent avatar or user default avatar.
        /// </summary>
        /// <param name="avatarID">Avatar ID</param>
        /// <param name="userID">User ID, load gender avatar or user avatar for specified user if avatar given by avatar id doesn't exist</param>
        /// <param name="maxSideSize">Max side size</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        public string GetUserAvatarImageUrl(object avatarID, object userID, int maxSideSize, int width, int height)
        {
            return AvatarInfoProvider.GetUserAvatarImageUrl(avatarID, userID, String.Empty, maxSideSize, width, height);
        }


        /// <summary>
        /// Returns avatar or gravatar image url, if it is not defined returns gender dependent avatar or user default avatar.
        /// </summary>
        /// <param name="avatarID">Avatar ID</param>
        /// <param name="userID">User ID, load gender avatar or user avatar for specified user if avatar given by avatar id doesn't exist</param>
        /// <param name="userEmail">User e-mail</param>
        /// <param name="maxSideSize">Max side size</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        public string GetUserAvatarImageUrl(object avatarID, object userID, object userEmail, int maxSideSize, int width, int height)
        {
            return AvatarInfoProvider.GetUserAvatarImageUrl(avatarID, userID, userEmail, maxSideSize, width, height);
        }


        /// <summary>
        /// Returns avatar image tag, if avatar is not defined returns gender depend avatar or user default avatar if is defined.
        /// </summary>
        /// <param name="userID">User ID, load gender avatar for specified user if avatar by avatar id doesn't exist</param>
        /// <param name="maxSideSize">Max side size</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <param name="alt">Image alternate text</param>
        public string GetUserAvatarImageForUser(object userID, int maxSideSize, int width, int height, object alt)
        {
            return AvatarInfoProvider.GetUserAvatarImageForUser(userID, maxSideSize, width, height, alt);
        }


        /// <summary>
        /// Returns avatar image tag, if avatar is not defined returns gender depend avatar or user default avatar if is defined.
        /// </summary>
        /// <param name="avatarID">Avatar ID</param>
        /// <param name="userID">User ID, load gender avatar for specified user if avatar by avatar id doesn't exist</param>
        /// <param name="maxSideSize">Max side size</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <param name="alt">Image alternate text</param>
        public string GetUserAvatarImage(object avatarID, object userID, int maxSideSize, int width, int height, object alt)
        {
            return AvatarInfoProvider.GetUserAvatarImage(avatarID, userID, maxSideSize, width, height, alt);
        }


        /// <summary>
        /// Returns group avatar image tag, if avatar is not defined returns default group if is defined.
        /// </summary>
        /// <param name="avatarGuid">Avatar GUID</param>
        /// <param name="maxSideSize">Max side size</param>
        /// <param name="alt">Image alternate text</param>
        public string GetGroupAvatarImage(object avatarGuid, int maxSideSize, object alt)
        {
            return GetGroupAvatarImageByGUID(avatarGuid, maxSideSize, 0, 0, alt);
        }


        /// <summary>
        /// Returns group avatar image tag, if avatar is not defined returns default group if is defined.
        /// </summary>
        /// <param name="avatarGuid">Avatar GUID</param>
        /// <param name="maxSideSize">Max side size</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <param name="alt">Image alternate text</param>
        public string GetGroupAvatarImageByGUID(object avatarGuid, int maxSideSize, int width, int height, object alt)
        {
            Guid guid = ValidationHelper.GetGuid(avatarGuid, Guid.Empty);

            if (guid == Guid.Empty)
            {
                AvatarInfo ai = AvatarInfoProvider.GetDefaultAvatar(DefaultAvatarTypeEnum.Group);
                if (ai != null)
                {
                    guid = ai.AvatarGUID;
                }
            }

            if (guid != Guid.Empty)
            {
                string url = UrlResolver.ResolveUrl("~/CMSPages/GetAvatar.aspx");
                url = URLHelper.AddParameterToUrl(url, "avatarguid", guid.ToString());

                // Max side size
                if (maxSideSize > 0)
                {
                    url = URLHelper.AddParameterToUrl(url, "maxsidesize", maxSideSize.ToString());
                }

                // Width
                if (width > 0)
                {
                    url = URLHelper.AddParameterToUrl(url, "width", width.ToString());
                }

                // Height
                if (height > 0)
                {
                    url = URLHelper.AddParameterToUrl(url, "height", height.ToString());
                }

                // Encode url to be used in html attribute
                url = HTMLHelper.EncodeForHtmlAttribute(url);
                // Encode alternate text to be used in html attribute
                string altText = HTMLHelper.EncodeForHtmlAttribute(DataHelper.GetNotEmpty(alt, "Avatar"));

                return "<img alt=\"" + altText + "\" src=\"" + url + "\" />";
            }

            return "";
        }


        /// <summary>
        /// Returns group avatar image tag, if avatar is not defined returns default group if is defined.
        /// </summary>
        /// <param name="avatarID">Avatar ID</param>
        /// <param name="maxSideSize">Max side size</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <param name="alt">Image alternate text</param>
        public string GetGroupAvatarImage(object avatarID, int maxSideSize, int width, int height, object alt)
        {
            int avatId = ValidationHelper.GetInteger(avatarID, 0);
            AvatarInfo ai = null;

            // Try to get user defined avatar
            if (avatId > 0)
            {
                string key = "AvatarInfo_" + avatId;
                ai = RequestStockHelper.GetItem(key) as AvatarInfo;
                if (ai == null)
                {
                    ai = AvatarInfoProvider.GetAvatarInfoWithoutBinary(avatId);
                    if (ai != null)
                    {
                        RequestStockHelper.Add(key, ai);
                    }
                }
            }

            // Try to get user default avatar
            if (ai == null)
            {
                ai = AvatarInfoProvider.GetDefaultAvatar(DefaultAvatarTypeEnum.Group);
            }

            // If exists avatar info, generate img tag
            if (ai != null)
            {
                string url = UrlResolver.ResolveUrl("~/CMSPages/GetAvatar.aspx");
                url = URLHelper.AddParameterToUrl(url, "avatarguid", ai.AvatarGUID.ToString());

                // Max side size
                if (maxSideSize > 0)
                {
                    url = URLHelper.AddParameterToUrl(url, "maxsidesize", maxSideSize.ToString());
                }

                // Width
                if (width > 0)
                {
                    url = URLHelper.AddParameterToUrl(url, "width", width.ToString());
                }

                // Height
                if (height > 0)
                {
                    url = URLHelper.AddParameterToUrl(url, "height", height.ToString());
                }

                // Encode url to be used in html attribute
                url = HTMLHelper.EncodeForHtmlAttribute(url);
                // Encode alternate text to be used in html attribute
                string altText = HTMLHelper.EncodeForHtmlAttribute(DataHelper.GetNotEmpty(alt, "Avatar"));

                return "<img alt=\"" + altText + "\" src=\"" + url + "\" />";
            }

            return "";
        }


        /// <summary>
        /// Returns badge image tag.
        /// </summary>
        /// <param name="badgeId">Badge ID</param>
        public string GetBadgeImage(int badgeId)
        {
            BadgeInfo bi = BadgeInfoProvider.GetBadgeInfo(badgeId);
            if (bi != null)
            {
                return string.Format("<img alt=\"{0}\" src=\"{1}\" />", HTMLHelper.HTMLEncode(bi.BadgeDisplayName), HTMLHelper.HTMLEncode(UIHelper.GetImageUrl(null, bi.BadgeImageURL)));
            }
            return String.Empty;
        }


        /// <summary>
        /// Returns badge name.
        /// </summary>
        /// <param name="badgeId">Badge ID</param>
        public string GetBadgeName(int badgeId)
        {
            BadgeInfo bi = BadgeInfoProvider.GetBadgeInfo(badgeId);
            if (bi != null)
            {
                return bi.BadgeName;
            }
            return String.Empty;
        }


        /// <summary>
        /// Returns user full name.
        /// </summary>
        /// <param name="userId">User ID</param>
        public string GetUserFullName(int userId)
        {
            if (userId > 0)
            {
                string key = "TransfUserFullName_" + userId;

                // Store full name to the request to minimize the DB access
                if (RequestStockHelper.Contains(key))
                {
                    return ValidationHelper.GetString(RequestStockHelper.GetItem(key), "");
                }
                else
                {
                    UserInfo user = UserInfoProvider.GetUsers().Where("UserID = " + userId).Columns("FullName").TopN(1).FirstOrDefault();
                    if (user != null)
                    {
                        string result = HTMLHelper.HTMLEncode(user.FullName);
                        RequestStockHelper.Add(key, result);

                        return result;
                    }
                }
            }
            return "";
        }

        #endregion


        #region "SharePoint"

        /// <summary>
        /// Gets URL for accessing file on SharePoint server.
        /// </summary>
        /// <param name="connectionName">Code name of SharePoint connection to be used.</param>
        /// <param name="fileRef">SharePoint server path to file.</param>
        /// <param name="cacheMinutes">How long should the file be cached (after accessing the URL). Blank uses the Settings value, 0 means no cache.</param>
        /// <param name="cacheFileSize">The maximum size of file the will be cached. Blank uses the Settings value.</param>
        /// <param name="width">Maximum width of the image the handler should return.</param>
        /// <param name="height">Maximum height of the image the handler should return.</param>
        /// <param name="maxSideSize">Maximum side size of the image the handler should return.</param>
        /// <returns>URL on which the file can be accessed.</returns>
        public string GetSharePointFileUrl(string connectionName, string fileRef, int? cacheMinutes = null, int? cacheFileSize = null, int? width = null, int? height = null, int? maxSideSize = null)
        {
            const string pageUrl = "~/CMSPages/GetSharePointFile.ashx";
            string queryString = QueryHelper.BuildQueryWithHash("connectionname", connectionName,
                "fileref", fileRef,
                "cacheminutes", cacheMinutes?.ToString(),
                "cachefilesize", cacheFileSize?.ToString(),
                "width", width?.ToString(),
                "height", height?.ToString(),
                "maxSideSize", maxSideSize?.ToString());

            return UrlResolver.ResolveUrl(pageUrl + queryString);
        }

        #endregion


        #region "SmartSearch"

        /// <summary>
        /// Returns URL to given search result item.
        /// </summary>
        /// <param name="resultItem">Search result item for which to return an URL.</param>
        /// <param name="noImageUrl">URL to image which should be displayed if image is not defined</param>
        /// <param name="maxSideSize">Max. side size</param>
        public string GetSearchImageUrl(SearchResultItem resultItem, string noImageUrl, int maxSideSize)
        {
            string result;

            if (SearchHelper.CUSTOM_SEARCH_INDEX.Equals(resultItem.Type, StringComparison.OrdinalIgnoreCase))
            {
                // Custom search index - Use direct URL
                result = resultItem.Image;
            }
            else
            {
                // Let indexer decide
                result = SearchIndexers.GetIndexer(resultItem.Type).GetSearchImageUrl(resultItem);
            }

            // If image is not defined try set no image url
            if (String.IsNullOrEmpty(result))
            {
                result = UIHelper.GetImageUrl(null, noImageUrl);
            }

            if (!String.IsNullOrEmpty(result))
            {
                // Setup parameters
                result = URLHelper.UpdateParameterInUrl(result, "requiredtype", "image");
                result = URLHelper.UpdateParameterInUrl(result, "defaultfilepath", AdministrationUrlHelper.GetImagePath(null, noImageUrl));
                if (maxSideSize > 0)
                {
                    result = URLHelper.UpdateParameterInUrl(result, "maxsidesize", maxSideSize.ToString());
                }

                // Get absolute URL for other than current site
                int siteId = ValidationHelper.GetInteger(GetSearchValue(resultItem, "NodeSiteID"), 0);
                if ((siteId > 0) && (siteId != SiteContext.CurrentSiteID))
                {
                    SiteInfo si = SiteInfoProvider.GetSiteInfo(siteId);
                    if (si != null)
                    {
                        result = URLHelper.GetAbsoluteUrl(result, si.DomainName);
                    }
                }
                else
                {
                    // Resolve the URL
                    result = UrlResolver.ResolveUrl(result);
                }
            }

            return result;
        }


        /// <summary>
        /// Highlight input text with dependence on current search keywords.
        /// </summary>
        /// <param name="resultItem">Search result to return highlighted text for.</param>
        /// <param name="text">Input text</param>
        /// <param name="startTag">Start highlight tag</param>
        /// <param name="endTag">End tag</param>
        public string SearchHighlight(SearchResultItem resultItem, string text, string startTag, string endTag)
        {
            return SearchHelper.Highlight(resultItem, text, startTag, endTag);
        }


        /// <summary>
        /// Returns content parsed as XML if required and removes dynamic controls.
        /// </summary>
        /// <param name="resultItem">Search result to return content for.</param>
        /// <param name="content">Content</param>
        public string GetSearchedContent(SearchResultItem resultItem, string content)
        {
            // Switch by result type
            switch (resultItem.Type.ToLowerInvariant())
            {
                // Document
                case TreeNode.OBJECT_TYPE:

                    // Try to parse content as XML
                    if ((content != null) && (content.StartsWith("<content>", StringComparison.Ordinal)) && (content.EndsWith("</content>", StringComparison.Ordinal)))
                    {
                        try
                        {
                            XmlDocument xml = new XmlDocument();
                            xml.LoadXml(content);
                            string text = String.Empty;
                            if (xml["content"] != null)
                            {
                                foreach (XmlNode node in xml["content"].ChildNodes)
                                {
                                    if (node != null)
                                    {
                                        // Get inner text, removes CDATA
                                        text += ControlsHelper.RemoveDynamicControls(node.InnerText);
                                        text += " ";
                                    }
                                }
                            }

                            // Remove macros from content text
                            text = MacroProcessor.RemoveMacros(text, String.Empty);

                            // Remove parts of editable image
                            return SearchHelper.ImageContentReplacer.Replace(text, String.Empty);
                        }
                        catch
                        {
                            return String.Empty;
                        }
                    }

                    // Remove macros from content text
                    return MacroProcessor.RemoveMacros(content, String.Empty);

                // Forums
                case PredefinedObjectType.FORUM:
                    if (content != null)
                    {
                        int forumId = ValidationHelper.GetInteger(GetSearchValue(resultItem, "PostForumID"), 0);
                        // Get forum info object
                        GeneralizedInfo fi = ModuleCommands.ForumsGetForumInfo(forumId);
                        // Indicates whether current form allows HTML editor
                        bool isHtml = false;

                        // Set isHtml flag with according to current forum
                        if (fi != null)
                        {
                            isHtml = ValidationHelper.GetBoolean(fi.GetValue("ForumHTMLEditor"), false);
                        }

                        // Remove BB macros and dynamic controls
                        content = RemoveDiscussionMacros(content);
                        content = RemoveDynamicControls(content);

                        // Strip tags if it is HTML post
                        if (isHtml)
                        {
                            content = HTMLHelper.StripTags(content);
                        }

                        // Encode html entities
                        content = HTMLHelper.HTMLEncode(content);
                    }
                    break;
            }

            // Otherwise return unmodified content
            return content;
        }


        /// <summary>
        /// Returns column value for current search result item.
        /// </summary>
        /// <param name="resultItem">Search result to return column value of.</param>
        /// <param name="columnName">Column name</param>
        public object GetSearchValue(SearchResultItem resultItem, string columnName)
        {
            return resultItem.GetSearchValue(columnName);
        }


        /// <summary>
        /// Returns URL for given search result.
        /// </summary>
        /// <param name="resultItem">Search result to return URL for.</param>
        /// <param name="absolute">Indicates whether generated url should be absolute</param>
        /// <param name="addLangParameter">Adds culture specific query parameter to the URL if more than one culture version exists. Default value is true.</param>
        public string SearchResultUrl(SearchResultItem resultItem, bool absolute, bool addLangParameter = true)
        {
            string url = String.Empty;

            // Switch by result type
            switch (resultItem.Type.ToLowerInvariant())
            {
                #region "Document"

                // Documents
                case TreeNode.OBJECT_TYPE:

                    // Get site infos
                    int siteId = ValidationHelper.GetInteger(resultItem.GetSearchValue("NodeSiteID"), 0);
                    string siteName = SiteContext.CurrentSiteName;

                    // Get sitename if site exists
                    SiteInfo si = SiteInfoProvider.GetSiteInfo(siteId);
                    if (si != null)
                    {
                        siteName = si.SiteName;
                    }

                    // Get url path
                    string urlPath = Convert.ToString(resultItem.GetSearchValue("DocumentUrlPath"));

                    // If node is linked document, do not use url path
                    if (ValidationHelper.GetInteger(resultItem.GetSearchValue("NodeLinkedNodeID"), 0) > 0)
                    {
                        urlPath = null;
                    }

                    // Get node alias path
                    string nodeAlias = Convert.ToString(resultItem.GetSearchValue("NodeAliasPath"));

                    // If node alias path is empty => do not generate link
                    if (String.IsNullOrEmpty(nodeAlias))
                    {
                        return String.Empty;
                    }

                    // Check whether current search result is part of a content only site and resolve the node URL accordingly
                    bool nodeIsContentOnly = ValidationHelper.GetBoolean(resultItem.GetSearchValue("NodeIsContentOnly"), false);
                    if (nodeIsContentOnly)
                    {
                        var searchResult = resultItem.ResultData;
                        var node = TreeNode.New(searchResult);
                        url = DocumentURLProvider.GetPresentationUrl(node);
                    }
                    else
                    {
                        url = UrlResolver.ResolveUrl(DocumentURLProvider.GetUrl(nodeAlias, urlPath, siteName));
                    }

                    // Check whether current search results are for all culture or only current (incl. combine with default)
                    if (resultItem.Result.MoreCultures && addLangParameter)
                    {
                        // Get current result item culture
                        string culture = resultItem.GetSearchValue("DocumentCulture") as string;

                        if (!String.IsNullOrEmpty(culture))
                        {
                            // If current result culture is not same as document culture add lang parameter
                            if (!culture.Equals(LocalizationContext.PreferredCultureCode, StringComparison.OrdinalIgnoreCase))
                            {
                                url = URLHelper.AddParameterToUrl(url, URLHelper.LanguageParameterName, culture);
                            }
                        }
                    }

                    // Return absolute URL
                    if (absolute)
                    {
                        url = URLHelper.GetAbsoluteUrl(url);
                    }

                    // Return relative URL
                    return url;

                #endregion


                #region "Forum"

                case PredefinedObjectType.FORUM:

                    int forumId = ValidationHelper.GetInteger(resultItem.GetSearchValue("PostForumID"), 0);
                    if (forumId > 0)
                    {
                        string domain = String.Empty;

                        // Get forum info object
                        GeneralizedInfo fi = ModuleCommands.ForumsGetForumInfo(forumId);
                        if (fi != null)
                        {
                            // Get base url
                            url = ValidationHelper.GetString(fi.GetValue("ForumBaseUrl"), String.Empty);
                            // Get site domain
                            si = SiteInfoProvider.GetSiteInfo(ValidationHelper.GetInteger(fi.GetValue("ForumSiteID"), 0));
                            if (si != null)
                            {
                                domain = si.DomainName;
                            }
                        }

                        // Get post id path
                        string path = ValidationHelper.GetString(resultItem.GetSearchValue("PostIdPath"), String.Empty);

                        int threadId = 0;
                        // Get thread id from path
                        if (!String.IsNullOrEmpty(path))
                        {
                            string trimmedPath = path.TrimStart('/');
                            int pos = trimmedPath.IndexOf("/", StringComparison.Ordinal);
                            if (pos > 0)
                            {
                                threadId = ValidationHelper.GetInteger(trimmedPath.Substring(0, pos), 0);
                            }
                            else
                            {
                                threadId = ValidationHelper.GetInteger(trimmedPath, 0);
                            }
                        }

                        // Complete forum url
                        url = url + "?forumid=" + forumId + "&threadid=" + threadId;

                        // Return absolute URL if it is required
                        if (absolute)
                        {
                            if (!String.IsNullOrEmpty(domain) && domain.Contains("/"))
                            {
                                url = url.TrimStart('~');
                            }
                            else
                            {
                                url = UrlResolver.ResolveUrl(url);
                            }

                            url = URLHelper.GetAbsoluteUrl(url, domain, null, null);
                        }
                        else
                        {
                            url = UrlResolver.ResolveUrl(url);
                        }

                        return url;
                    }
                    break;

                #endregion


                #region "User"

                case UserInfo.OBJECT_TYPE:
                    string userUrl = GetUserProfileURL(resultItem.GetSearchValue("UserName"));

                    if (absolute)
                    {
                        userUrl = URLHelper.GetAbsoluteUrl(userUrl);
                    }

                    return userUrl;

                #endregion
            }

            #region "Custom"

            // The SearchHelper.CUSTOM_SEARCH_INDEX constant's value does not honor the all case lower spelling
            if (SearchHelper.CUSTOM_SEARCH_INDEX.Equals(resultItem.Type, StringComparison.OrdinalIgnoreCase))
            {
                string customUrl = Convert.ToString(resultItem.GetSearchValue(SearchFieldsConstants.CUSTOM_URL));

                if (!String.IsNullOrEmpty(customUrl))
                {
                    // Process urls only for not physical files
                    if (!customUrl.Contains("\\"))
                    {
                        customUrl = UrlResolver.ResolveUrl(customUrl);

                        if (absolute)
                        {
                            customUrl = URLHelper.GetAbsoluteUrl(customUrl);
                        }
                    }
                }

                return customUrl;
            }

            #endregion

            return String.Empty;
        }

        #endregion


        #region "Syndication methods"

        /// <summary>
        /// Returns feed name. If feed name is defined, than returns it. Otherwise returns instance GUID.
        /// </summary>
        /// <param name="feedName">Name of the feed</param>
        /// <param name="instanceGuid">Instance GUID</param>
        public string GetFeedName(object feedName, object instanceGuid)
        {
            string feedNameStr = ValidationHelper.GetString(feedName, "");
            return String.IsNullOrEmpty(feedNameStr) ? ValidationHelper.GetString(instanceGuid, "") : feedNameStr;
        }


        /// <summary>
        /// Escapes CDATA value.
        /// </summary>
        /// <param name="value">Value to wrap</param>
        public virtual object EvalCDATA(object value)
        {
            return EvalCDATA(value, true);
        }


        /// <summary>
        /// Escapes CDATA value.
        /// </summary>
        /// <param name="value">Value to wrap</param>
        /// <param name="encapsulate">Indicates if resulting string will be encapsulated in CDATA section</param>
        public virtual object EvalCDATA(object value, bool encapsulate)
        {
            // Replace possible "]]>" with "]]]]><![CDATA[>" to get well formed XML
            string val = ValidationHelper.GetString(value, "");
            val = CDATARegExp.Replace(val, "]]]]><![CDATA[>");
            StringBuilder sb = new StringBuilder(val);

            // Wrap with classic CDATA section
            if (encapsulate)
            {
                sb.Insert(0, "<![CDATA[");
                sb.Append("]]>");
            }

            return sb.ToString();
        }


        /// <summary>
        /// Returns URL of the currently rendered document with feed parameter.
        /// </summary>
        /// <param name="feedName">Name of the feed</param>
        /// <param name="url">Base url</param>
        public string GetDocumentUrlForFeed(string feedName, string url)
        {
            if (!String.IsNullOrEmpty(feedName))
            {
                url = URLHelper.AddParameterToUrl(url, "feed", feedName);
            }
            return url;
        }


        /// <summary>
        /// Returns URL of the specified forum post with feed parameter.
        /// </summary>
        /// <param name="feedName">Name of the feed</param>
        /// <param name="forumId">Forum id</param>
        /// <param name="postIdPath">Post id path</param>
        public string GetForumPostUrlForFeed(string feedName, object postIdPath, object forumId)
        {
            string pIdPath = ValidationHelper.GetString(postIdPath, string.Empty);
            int fId = ValidationHelper.GetInteger(forumId, 0);

            string url = ModuleCommands.ForumsGetPostUrl(pIdPath, fId, false);
            if (!String.IsNullOrEmpty(feedName))
            {
                url = URLHelper.AddParameterToUrl(url, "feed", feedName);
            }
            return url;
        }


        /// <summary>
        /// Returns URL of the specified media file with feed parameter.
        /// </summary>
        /// <param name="feedName">Name of the feed</param>
        /// <param name="fileGUID">File GUID</param>
        /// <param name="fileName">File name</param>
        public string GetMediaFileUrlForFeed(string feedName, object fileGUID, object fileName)
        {
            string url = GetMediaFileUrl(fileGUID, fileName);
            if (!String.IsNullOrEmpty(feedName))
            {
                url = URLHelper.AddParameterToUrl(url, "feed", feedName);
            }
            return url;
        }


        /// <summary>
        /// Returns URL of the message board document with feed parameter.
        /// </summary>
        /// <param name="feedName">Name of the feed</param>
        /// <param name="documentIdObj">Document ID</param>
        public string GetMessageBoardUrlForFeed(string feedName, object documentIdObj)
        {
            string url = GetDocumentUrl(documentIdObj);
            if (!String.IsNullOrEmpty(feedName))
            {
                url = URLHelper.AddParameterToUrl(url, "feed", feedName);
            }
            return url;
        }


        /// <summary>
        /// Returns URL of the blog comment document with feed parameter.
        /// </summary>
        /// <param name="feedName">Name of the feed</param>
        /// <param name="documentIdObj">Document ID</param>
        public string GetBlogCommentUrlForFeed(string feedName, object documentIdObj)
        {
            string url = GetDocumentUrl(documentIdObj);
            if (!String.IsNullOrEmpty(feedName))
            {
                url = URLHelper.AddParameterToUrl(url, "feed", feedName);
            }
            return url;
        }


        /// <summary>
        /// Gets time according to RFC 3339 for Atom feeds.
        /// </summary>
        /// <param name="control">Control containing ITimeZoneManager</param>
        /// <param name="dateTime">DateTime object to format</param>
        /// <returns>Formatted date</returns>
        public string GetAtomDateTime(Control control, object dateTime)
        {
            return GetDateTime(control, dateTime).ToString("yyyy-MM-dd'T'HH:mm:ss.fffzzzz");
        }


        /// <summary>
        /// Gets time according to RFC 822 for RSS feeds.
        /// </summary>
        /// <param name="control">Control containing ITimeZoneManager</param>
        /// <param name="dateTime">DateTime object to format</param>
        /// <returns>Formatted date</returns>
        public string GetRSSDateTime(Control control, object dateTime)
        {
            return GetDateTime(control, dateTime).ToUniversalTime().ToString("r");
        }

        #endregion


        #region "Booking system"

        /// <summary>
        /// Returns string representation of event time with dependence on current ITimeZone manager
        /// time zone settings.
        /// </summary>
        /// <param name="control">Control containing ITimeZoneManager</param>
        /// <param name="startTime">Event start time</param>
        /// <param name="endTime">Event end time</param>
        /// <param name="isAllDayEvent">Indicates if it is all day event - if yes, result does not contain times</param>
        public string GetEventDateString(Control control, object startTime, object endTime, bool isAllDayEvent)
        {
            string result = string.Empty;
            string gmtShift = string.Empty;

            TimeZoneInfo tzi;
            DateTime start = TimeZoneUIMethods.GetDateTimeForControl(control, ValidationHelper.GetDateTime(startTime, DateTimeHelper.ZERO_TIME), out tzi);
            DateTime end = TimeZoneUIMethods.GetDateTimeForControl(control, ValidationHelper.GetDateTime(endTime, DateTimeHelper.ZERO_TIME));

            if (tzi != null)
            {
                // Get string representation of time zone shift
                gmtShift = TimeZoneHelper.GetUTCStringOffset(tzi);
            }

            if ((start != DateTimeHelper.ZERO_TIME) || (end != DateTimeHelper.ZERO_TIME))
            {
                if ((start != DateTimeHelper.ZERO_TIME) && (end != DateTimeHelper.ZERO_TIME))
                {
                    // Get date string combined from start time and end time
                    if (isAllDayEvent)
                    {
                        if (start.Date.CompareTo(end.Date) == 0)
                        {
                            // All day event with same start and end dates
                            result = start.ToShortDateString();
                        }
                        else
                        {
                            // All day event through multiple days
                            result = string.Format("{0} - {1}", start.ToShortDateString(), end.ToShortDateString());
                        }
                    }
                    else
                    {
                        if (start.Date.CompareTo(end.Date) == 0)
                        {
                            // One-day event with times
                            result = string.Format("{0}, {1} - {2}{3}", start.ToShortDateString(), start.ToShortTimeString(), end.ToShortTimeString(), gmtShift);
                        }
                        else
                        {
                            // Multi-day event with times
                            result = string.Format("{0:g}{2} - {1:g}{2}", start, end, gmtShift);
                        }
                    }
                }
                else if (start == DateTimeHelper.ZERO_TIME)
                {
                    // Get date string from end time
                    if (isAllDayEvent)
                    {
                        result = end.ToShortDateString();
                    }
                    else
                    {
                        result = end.ToString("g") + gmtShift;
                    }
                }
                else if (end == DateTimeHelper.ZERO_TIME)
                {
                    // Get date string from start time
                    if (isAllDayEvent)
                    {
                        result = start.ToShortDateString();
                    }
                    else
                    {
                        result = start.ToString("g") + gmtShift;
                    }
                }
            }

            return result;
        }

        #endregion


        #region "Categories"

        /// <summary>
        /// Appends current category ID to given url.
        /// </summary>
        /// <param name="url">URL to add parameter to</param>
        public string AddCurrentCategoryParameter(object url)
        {
            string strUrl = ValidationHelper.GetString(url, null);

            if (!string.IsNullOrEmpty(strUrl) && (SiteContext.CurrentCategory != null))
            {
                strUrl = URLHelper.AddParameterToUrl(strUrl, "categoryId", TaxonomyContext.CurrentCategory.CategoryID.ToString());
            }

            return strUrl;
        }

        #endregion
    }
}