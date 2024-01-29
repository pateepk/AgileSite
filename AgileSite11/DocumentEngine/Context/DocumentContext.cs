using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using CMS.Base;
using CMS.DataEngine;
using CMS.DeviceProfiles;
using CMS.Helpers;
using CMS.Localization;
using CMS.MacroEngine;
using CMS.Membership;
using CMS.PortalEngine;
using CMS.SiteProvider;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Provides context of document related properties.
    /// </summary>
    [RegisterAllProperties]
    public class DocumentContext : AbstractContext<DocumentContext>
    {
        private static string DOCTYPE = "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">";

        private string mOriginalAliasPath;
        private string mCurrentAliasPath;
        private TreeNode mCurrentDocument;
        private TreeNode mEditedDocument;
        private AttachmentInfo mCurrentAttachment;
        private PageInfo mCurrentPageInfo;
        private PageInfo mOriginalPageInfo;
        private PageInfo mCurrentCultureInvariantPageInfo;
        private List<PageInfo> mCurrentParentPageInfos;
        private string mPageResultUrlPath;
        private string mCurrentKeyWords;
        private string mCurrentDescription;
        private string mCurrentTitle;
        private string mCurrentBodyClass;
        private string mCurrentDocType;
        private MacroResolver mCurrentResolver;


        /// <summary>
        /// Current macro resolver.
        /// </summary>
        public static MacroResolver CurrentResolver
        {
            get
            {
                var c = Current;

                return c.mCurrentResolver ?? (c.mCurrentResolver = GetResolver());
            }
        }


        /// <summary>
        /// Document edited by the current page.
        /// </summary>
        public static TreeNode EditedDocument
        {
            get
            {
                return Current.mEditedDocument;
            }
            set
            {
                Current.mEditedDocument = value;
            }
        }


        /// <summary>
        /// Returns alias path corresponding to current URL.
        /// </summary>
        [RegisterColumn]
        public static string OriginalAliasPath
        {
            get
            {
                var c = Current;
                return c.mOriginalAliasPath ?? (c.mOriginalAliasPath = CurrentAliasPath);
            }
            set
            {
                Current.mOriginalAliasPath = value;
            }
        }


        /// <summary>
        /// Returns the original page info object if site is in test and current page info belongs to variant.
        /// </summary>
        public static PageInfo OriginalPageInfo
        {
            get
            {
                var c = Current;
                return c.mOriginalPageInfo ?? (c.mOriginalPageInfo = CurrentPageInfo);
            }
            set
            {
                Current.mOriginalPageInfo = value;
                ClearOriginalPageInfoDependentProperties();
            }
        }


        /// <summary>
        /// Returns current document aliaspath.
        /// </summary>
        [RegisterColumn]
        public static string CurrentAliasPath
        {
            get
            {
                var c = Current;
                if (c.mCurrentAliasPath != null)
                {
                    return c.mCurrentAliasPath;
                }

                c.mCurrentAliasPath = GetCurrentAliasPath();
                return c.mCurrentAliasPath;
            }
            set
            {
                Current.mCurrentAliasPath = value;
                ClearAliasPathDependentProperties();
            }
        }


        /// <summary>
        /// Returns the current document node.
        /// </summary>
        public static TreeNode CurrentDocument
        {
            get
            {
                var c = Current;
                if (c.mCurrentDocument != null)
                {
                    return c.mCurrentDocument;
                }

                var pageInfo = GetDocumentPageInfo();
                if (pageInfo == null)
                {
                    return null;
                }

                c.mCurrentDocument = GetCachedCurrentDocument(pageInfo);

                return c.mCurrentDocument;
            }
            set
            {
                Current.mCurrentDocument = value;
                ClearDocumentDependentProperties();
            }
        }


        /// <summary>
        /// Returns parent of the current document node.
        /// </summary>
        public static TreeNode CurrentDocumentParent
        {
            get
            {
                var document = CurrentDocument;
                return document != null ? document.Parent : null;
            }
        }


        /// <summary>
        /// Returns the current document culture.
        /// </summary>
        public static CultureInfo CurrentDocumentCulture
        {
            get
            {
                var pageInfo = GetDocumentPageInfo();
                return pageInfo == null ? null : CultureInfoProvider.GetCultureInfo(pageInfo.DocumentCulture);
            }
        }


        /// <summary>
        /// Returns the current document owner.
        /// </summary>
        public static UserInfo CurrentDocumentOwner
        {
            get
            {
                var pageInfo = GetDocumentPageInfo();
                return pageInfo == null ? null : UserInfoProvider.GetUserInfo(pageInfo.NodeOwner);
            }
        }


        /// <summary>
        /// Returns the current document CSS stylesheet.
        /// </summary>
        public static CssStylesheetInfo CurrentDocumentStylesheet
        {
            get
            {
                var pageInfo = GetDocumentPageInfo();
                return GetCurrentStylesheet(pageInfo);
            }
        }


        /// <summary>
        /// Returns the current document CSS stylesheet name.
        /// </summary>
        public static string CurrentDocumentStylesheetName
        {
            get
            {
                var stylesheet = CurrentDocumentStylesheet;
                return stylesheet != null ? stylesheet.StylesheetName : null;
            }
        }


        /// <summary>
        /// Returns the current document attachment (only for file document type).
        /// </summary>
        public static AttachmentInfo CurrentAttachment
        {
            get
            {
                var pageInfo = GetDocumentPageInfo();
                if (pageInfo == null)
                {
                    return null;
                }

                if (!pageInfo.ClassName.EqualsCSafe(SystemDocumentTypes.File, true))
                {
                    return null;
                }

                var c = Current;
                if (c.mCurrentAttachment != null)
                {
                    return c.mCurrentAttachment;
                }

                c.mCurrentAttachment = GetCurrentAttachment();

                return c.mCurrentAttachment;
            }
        }


        /// <summary>
        /// Gets or sets the ordered list of parent page info objects, which are used on current page. These page info should be used as read-only.
        /// </summary>
        public static List<PageInfo> CurrentParentPageInfos
        {
            get
            {
                var c = Current;
                if (c.mCurrentParentPageInfos != null)
                {
                    return c.mCurrentParentPageInfos;
                }

                c.mCurrentParentPageInfos = new List<PageInfo>();

                return c.mCurrentParentPageInfos;
            }
        }


        /// <summary>
        /// Returns the current page info object.
        /// </summary>
        public static PageInfo CurrentPageInfo
        {
            get
            {
                var c = Current;
                if (c.mCurrentPageInfo != null)
                {
                    return c.mCurrentPageInfo;
                }

                c.mCurrentPageInfo = GetCurrentPageInfo();

                return c.mCurrentPageInfo;
            }
            set
            {
                Current.mCurrentPageInfo = value;
                ClearPageInfoDependentProperties();
            }
        }


        /// <summary>
        /// Returns the current culture invariant page info object.
        /// </summary>
        public static PageInfo CurrentCultureInvariantPageInfo
        {
            get
            {
                var c = Current;
                if (c.mCurrentCultureInvariantPageInfo != null)
                {
                    return c.mCurrentCultureInvariantPageInfo;
                }

                c.mCurrentCultureInvariantPageInfo = GetCurrentInvariantPageInfo();

                return c.mCurrentCultureInvariantPageInfo;
            }
        }


        /// <summary>
        /// Current page template
        /// </summary>
        public static PageTemplateInfo CurrentTemplate
        {
            get
            {
                var pageInfo = CurrentPageInfo;
                return pageInfo != null ? pageInfo.UsedPageTemplateInfo : null;
            }
        }


        /// <summary>
        /// Current page keywords.
        /// </summary>
        [RegisterColumn]
        public static string CurrentKeyWords
        {
            get
            {
                var c = Current;
                if (c.mCurrentKeyWords != null)
                {
                    return c.mCurrentKeyWords;
                }

                var keywords = string.Empty;
                var pageInfo = GetDocumentPageInfo();
                if (pageInfo != null)
                {
                    keywords = GetCurrentKeyWords(pageInfo);
                }

                c.mCurrentKeyWords = keywords;
                return c.mCurrentKeyWords;
            }
            set
            {
                Current.mCurrentKeyWords = value;
            }
        }


        /// <summary>
        /// Current page description.
        /// </summary>
        [RegisterColumn]
        public static string CurrentDescription
        {
            get
            {
                var c = Current;
                if (c.mCurrentDescription != null)
                {
                    return c.mCurrentDescription;
                }

                var description = string.Empty;
                var pageInfo = GetDocumentPageInfo();
                if (pageInfo != null)
                {
                    description = GetCurrentDescription(pageInfo);
                }

                c.mCurrentDescription = description;
                return c.mCurrentDescription;
            }
            set
            {
                Current.mCurrentDescription = value;
            }
        }


        /// <summary>
        /// Current page title with resolved macros.
        /// </summary>
        [RegisterColumn]
        public static string CurrentTitle
        {
            get
            {
                var c = Current;
                if (c.mCurrentTitle != null)
                {
                    return c.mCurrentTitle;
                }

                // Get original Page info instead of current - if there is no original info current page info is taken.
                var title = string.Empty;
                var currentPage = OriginalPageInfo;
                if (currentPage != null)
                {
                    title = currentPage.GetResolvedPageTitle();
                }

                c.mCurrentTitle = title;
                return c.mCurrentTitle;
            }
            set
            {
                Current.mCurrentTitle = value;
            }
        }


        /// <summary>
        /// Current page body class.
        /// </summary>
        [RegisterColumn]
        public static string CurrentBodyClass
        {
            get
            {
                var c = Current;
                if (c.mCurrentBodyClass != null)
                {
                    return c.mCurrentBodyClass;
                }

                c.mCurrentBodyClass = GetCurrentBodyClass(PortalContext.ViewMode);
                return c.mCurrentBodyClass;
            }
            set
            {
                Current.mCurrentBodyClass = value;
            }
        }


        /// <summary>
        /// Current page document type.
        /// </summary>
        [RegisterColumn]
        public static string CurrentDocType
        {
            get
            {
                var c = Current;
                if (c.mCurrentDocType != null)
                {
                    return c.mCurrentDocType;
                }

                var pageInfo = GetDocumentPageInfo();
                var docType = GetCurrentDocType(pageInfo);

                c.mCurrentDocType = docType;
                return c.mCurrentDocType;
            }
            set
            {
                Current.mCurrentDocType = value;
            }
        }


        /// <summary>
        /// Gets or sets the URL path of the current request.
        /// </summary>
        protected internal static string PageResultUrlPath
        {
            get
            {
                return Current.mPageResultUrlPath;
            }
            set
            {
                Current.mPageResultUrlPath = value;
            }
        }


        private static string GetDirectionCssClass(ViewModeEnum viewMode)
        {
            if (viewMode != ViewModeEnum.DashboardWidgets)
            {
                return CultureHelper.IsPreferredCultureRTL() ? "RTL" : "LTR";
            }

            return CultureHelper.IsUICultureRTL() ? "RTL" : "LTR";
        }


        private static string GetContentBodyCssClass()
        {
            return "ContentBody";
        }


        private static string GetViewModeCssClass(ViewModeEnum viewMode)
        {
            switch (viewMode)
            {
                case ViewModeEnum.Design:
                    return "DesignMode";

                case ViewModeEnum.Edit:
                    return "EditMode";

                case ViewModeEnum.DashboardWidgets:
                    return "DashboardMode";
            }

            return null;
        }


        private static string GetCultureCssClass(ViewModeEnum viewMode)
        {
            return viewMode == ViewModeEnum.DashboardWidgets ? GetUICultureClass() : GetCultureClass();
        }


        /// <summary>
        /// Gets the culture specific CSS class name.
        /// </summary>
        public static string GetCultureClass()
        {
            return ValidationHelper.GetIdentifier(LocalizationContext.PreferredCultureCode, "").ToUpperCSafe();
        }


        /// <summary>
        /// Gets the UI culture specific CSS class name.
        /// </summary>
        public static string GetUICultureClass()
        {
            return ValidationHelper.GetIdentifier(LocalizationContext.PreferredUICultureCode, "").ToUpperCSafe();
        }


        private static MacroResolver GetResolver()
        {
            var resolver = MacroContext.CurrentResolver.CreateChild();

            // Assign current thread culture to correct resolving of localization macros
            resolver.Culture = Thread.CurrentThread.CurrentCulture.ToString();

            return resolver;
        }


        /// <summary>
        /// Adds the default output cache dependencies to the request output.
        /// </summary>
        public static void AddDefaultOutputCacheDependencies()
        {
            PageInfo currentPage = CurrentPageInfo;
            if (currentPage != null)
            {
                CacheHelper.AddOutputCacheDependencies(currentPage.GetResponseCacheDependencies());
            }
        }


        private static void ClearAliasPathDependentProperties()
        {
            var c = Current;
            c.mCurrentPageInfo = null;
            c.mCurrentCultureInvariantPageInfo = null;

            ClearPageInfoDependentProperties();
        }


        private static void ClearPageInfoDependentProperties()
        {
            var c = Current;
            c.mCurrentDocType = null;
            c.mCurrentDescription = null;
            c.mCurrentKeyWords = null;
            c.mCurrentDocument = null;

            ClearOriginalPageInfoDependentProperties();
            ClearDocumentDependentProperties();
        }


        private static void ClearOriginalPageInfoDependentProperties()
        {
            var c = Current;
            c.mCurrentTitle = null;
        }


        private static void ClearDocumentDependentProperties()
        {
            var c = Current;
            c.mCurrentAttachment = null;
        }


        private static string GetCurrentAliasPath()
        {
            var aliasPath = QueryHelper.GetString(URLHelper.AliasPathParameterName, null);
            if (string.IsNullOrEmpty(aliasPath))
            {
                return aliasPath;
            }

            if ((aliasPath == "/") && PortalContext.ViewMode.IsLiveSite())
            {
                return GetDefaultAliasPathForLiveSite();
            }

            return aliasPath;
        }


        private static string GetDefaultAliasPathForLiveSite()
        {
            var aliasPath = SettingsKeyInfoProvider.GetValue(SiteContext.CurrentSiteName + ".CMSDefaultAliasPath");
            if (aliasPath == "")
            {
                aliasPath = "/";
            }

            return aliasPath;
        }


        private static TreeNode GetCachedCurrentDocument(PageInfo pageInfo)
        {
            var siteName = SiteContext.CurrentSiteName;
            var viewMode = PortalContext.ViewMode;
            var cacheMinutes = GetCurrentDocumentCacheMinutes(siteName, viewMode);
            var tree = new TreeProvider();

            TreeNode node = null;
            using (var cs = new CachedSection<TreeNode>(ref node, cacheMinutes, true, null, "currentdocument", siteName, pageInfo.NodeAliasPath, CacheHelper.GetCultureCacheKey(pageInfo.DocumentCulture)))
            {
                if (cs.LoadData)
                {
                    var aliasPath = pageInfo.NodeAliasPath;
                    var className = pageInfo.ClassName;
                    node = tree.SelectSingleNode(pageInfo.NodeID, pageInfo.DocumentCulture, className);
                    if (!viewMode.IsLiveSite())
                    {
                        node = DocumentHelper.GetDocument(node, tree);
                    }

                    if (cs.Cached)
                    {
                        cs.CacheDependency = CacheHelper.GetCacheDependency(DocumentDependencyCacheKeysBuilder.GetDependencyCacheKeys(siteName, className, aliasPath, pageInfo.NodeGroupID));
                    }

                    cs.Data = node;
                }
            }

            return node;
        }


        private static CssStylesheetInfo GetCurrentStylesheet(PageInfo pageInfo)
        {
            CssStylesheetInfo sheet = null;
            const string currentThemeKey = CookieName.CurrentTheme;

            if (pageInfo == null)
            {
                var currentTheme = CookieHelper.GetValue(currentThemeKey);
                if (!string.IsNullOrEmpty(currentTheme))
                {
                    sheet = CssStylesheetInfoProvider.GetCssStylesheetInfo(currentTheme);
                }
            }
            else
            {
                if (pageInfo.DocumentStylesheetID > 0)
                {
                    sheet = CssStylesheetInfoProvider.GetCssStylesheetInfo(pageInfo.DocumentStylesheetID);
                }

                if (sheet == null)
                {
                    sheet = PortalContext.CurrentSiteStylesheet;
                }

                if ((sheet != null) && (CMSString.Compare(CookieHelper.GetValue(currentThemeKey), sheet.StylesheetName, true) != 0))
                {
                    CookieHelper.SetValue(currentThemeKey, sheet.StylesheetName, DateTime.Now.AddDays(1));
                }
            }

            return sheet;
        }


        private static PageInfo GetCurrentPageInfo()
        {
            var siteName = SiteContext.CurrentSiteName;
            if (string.IsNullOrEmpty(siteName))
            {
                return null;
            }

            var aliasPath = CurrentAliasPath;
            if (string.IsNullOrEmpty(aliasPath))
            {
                return null;
            }

            return PageInfoProvider.GetPageInfo(siteName, aliasPath, LocalizationContext.PreferredCultureCode, null, SiteInfoProvider.CombineWithDefaultCulture(siteName));
        }


        private static PageInfo GetCurrentInvariantPageInfo()
        {
            var siteName = SiteContext.CurrentSiteName;
            if (string.IsNullOrEmpty(siteName))
            {
                return null;
            }

            var pageInfo = GetCurrentPageInfo();
            if (pageInfo != null)
            {
                return pageInfo;
            }

            var currentUrl = RequestContext.URL.GetLeftPart(UriPartial.Path);
            if (!String.IsNullOrEmpty(currentUrl))
            {
                // Get page info for different culture based on current URL (non-existing page in current culture)
                return PageInfoProvider.GetPageInfoForUrl(currentUrl, TreeProvider.ALL_CULTURES, null, true, true, siteName);
            }

            return null;
        }


        private static AttachmentInfo GetCurrentAttachment()
        {
            var currentDocument = CurrentDocument;
            if (currentDocument == null)
            {
                return null;
            }

            var guid = ValidationHelper.GetGuid(currentDocument.GetValue("FileAttachment"), Guid.Empty);
            return AttachmentInfoProvider.GetAttachmentInfoWithoutBinary(guid, SiteContext.CurrentSiteName);
        }


        private static string GetCurrentKeyWords(PageInfo pageInfo)
        {
            var keywords = SettingsKeyInfoProvider.GetValue(SiteContext.CurrentSiteName + ".CMSPageKeyWordsPrefix");
            keywords += pageInfo.DocumentPageKeyWords;

            return CurrentResolver.ResolveMacros(keywords);
        }


        private static string GetCurrentDescription(PageInfo pageInfo)
        {
            var description = SettingsKeyInfoProvider.GetValue(SiteContext.CurrentSiteName + ".CMSPageDescriptionPrefix");
            description += pageInfo.DocumentPageDescription;

            return CurrentResolver.ResolveMacros(description);
        }


        private static string GetCurrentBodyClass(ViewModeEnum viewMode)
        {
            var classes = new List<string>
            {
                GetDirectionCssClass(viewMode),
                GetViewModeCssClass(viewMode),
                BrowserHelper.GetBrowserClass(),
                GetCultureCssClass(viewMode),
                DeviceContext.GetDeviceProfilesClass(),
                GetContentBodyCssClass()
            };

            return classes.Where(c => !string.IsNullOrEmpty(c)).ToArray().Join(" ");
        }


        private static string GetCurrentDocType(PageInfo pageInfo)
        {
            var docType = string.Empty;
            if (pageInfo != null)
            {
                docType = pageInfo.NodeDocType.Trim();
            }

            if (string.IsNullOrEmpty(docType))
            {
                docType = DOCTYPE;
            }

            return docType;
        }


        private static int GetCurrentDocumentCacheMinutes(string siteName, ViewModeEnum viewMode)
        {
            return !viewMode.IsLiveSite() ? 0 : CacheHelper.CacheMinutes(siteName);
        }


        private static PageInfo GetDocumentPageInfo()
        {
            var pageInfo = CurrentPageInfo;
            if ((pageInfo == null) || !pageInfo.IsDocument)
            {
                return null;
            }

            return pageInfo;
        }
    }
}
