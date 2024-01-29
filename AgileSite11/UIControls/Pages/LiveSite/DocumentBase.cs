using System;
using System.Text;
using System.Threading;
using System.Web;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Localization;
using CMS.MacroEngine;
using CMS.Membership;
using CMS.OutputFilter;
using CMS.PortalEngine.Web.UI;
using CMS.PortalEngine;
using CMS.SiteProvider;

using CultureInfo = System.Globalization.CultureInfo;

namespace CMS.UIControls
{
    /// <summary>
    /// Base interface for document pages.
    /// </summary>
    public class DocumentBase
    {
        #region "Variables"

        /// <summary>
        /// List of the cache key items.
        /// </summary>
        private static string mCacheItems;

        private SiteInfo mCurrentSite;
        private PageInfo mCurrentPage;
        private CurrentUserInfo mCurrentUser;
        private string mCurrentSiteName;

        private HttpResponse mResponse;
        private AbstractCMSPage mPage;

        private string mXmlNamespace;

        /// <summary>
        /// Generic body class - contains the base body css class.
        /// </summary>
        protected string mBodyClass = "";


        /// <summary>
        /// Other parameters in body tag.
        /// </summary>
        protected string mBodyParameters = "";


        /// <summary>
        /// Top HTML body node for custom HTML code.
        /// </summary>
        protected string mBodyScripts = "";


        /// <summary>
        /// Page CSS file link.
        /// </summary>
        protected string mCssFile;


        /// <summary>
        /// Extended page tags.
        /// </summary>
        protected string mExtendedTags;


        /// <summary>
        /// If false, default layout rendering is disabled.
        /// </summary>
        protected bool renderLayout = true;


        /// <summary>
        /// Page manager.
        /// </summary>
        protected IPageManager mPageManager;


        /// <summary>
        /// CSS stylesheet of current page.
        /// </summary>
        private CssStylesheetInfo mCurrentStylesheet;


        /// <summary>
        /// Indicates whether meta tags should be encoded.
        /// </summary>
        private static bool? mEncodeMetaTagValue;

        /// <summary>
        /// Local instance of macro resolver.
        /// </summary>
        private MacroResolver mLocalResolver;

        #endregion


        #region "Private properties"

        /// <summary>
        /// Indicates whether <see cref="SetCulture"/> method was called.
        /// </summary>
        private bool CultureWasSet
        {
            get;
            set;
        }


        /// <summary>
        /// Local instance of macro resolver.
        /// </summary>
        private MacroResolver LocalResolver
        {
            get
            {
                return mLocalResolver ?? (mLocalResolver = PortalUIHelper.GetControlResolver(Page));
            }
        }

        #endregion


        #region "Public properties"

        /// <summary>
        /// Current site.
        /// </summary>
        public virtual SiteInfo CurrentSite
        {
            get
            {
                return mCurrentSite ?? (mCurrentSite = SiteContext.CurrentSite);
            }
        }


        /// <summary>
        /// Current user.
        /// </summary>
        public virtual CurrentUserInfo CurrentUser
        {
            get
            {
                return mCurrentUser ?? (mCurrentUser = MembershipContext.AuthenticatedUser);
            }
        }


        /// <summary>
        /// Current page.
        /// </summary>
        public virtual PageInfo CurrentPage
        {
            get
            {
                return mCurrentPage ?? (mCurrentPage = DocumentContext.CurrentPageInfo);
            }
        }


        /// <summary>
        /// Current CSS stylesheet.
        /// </summary>
        public virtual CssStylesheetInfo CurrentStylesheet
        {
            get
            {
                return mCurrentStylesheet ?? (mCurrentStylesheet = DocumentContext.CurrentDocumentStylesheet);
            }
        }


        /// <summary>
        /// List of the cache key items.
        /// </summary>
        public static string CacheItems
        {
            get
            {
                return mCacheItems ?? (mCacheItems = ValidationHelper.GetString(SettingsHelper.AppSettings["CMSOutputCacheItems"], "username;sitename;lang;browser"));
            }
            set
            {
                mCacheItems = value;
            }
        }


        /// <summary>
        /// Current response.
        /// </summary>
        public virtual HttpResponse Response
        {
            get
            {
                return mResponse ?? (mResponse = HttpContext.Current.Response);
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
        /// Document page.
        /// </summary>
        public virtual AbstractCMSPage Page
        {
            get
            {
                return mPage;
            }
        }


        /// <summary>
        /// Page manager.
        /// </summary>
        public virtual IPageManager PageManager
        {
            get
            {
                // If not set, try to find
                if (mPageManager == null)
                {
                    // Get current page manager from context
                    mPageManager = PortalContext.CurrentPageManager;
                    if (mPageManager == null)
                    {
                        throw new Exception("[DocumentBase.PageManager]: Page manager control not found.");
                    }
                }

                return mPageManager;
            }
            set
            {
                mPageManager = value;
            }
        }


        /// <summary>
        /// DocType.
        /// </summary>
        public virtual string DocType
        {
            get
            {
                return DocumentContext.CurrentDocType;
            }
            set
            {
                DocumentContext.CurrentDocType = value;
            }
        }


        /// <summary>
        /// Body parameters.
        /// </summary>
        public virtual string BodyParameters
        {
            get
            {
                return mBodyParameters;
            }
            set
            {
                mBodyParameters = value;
            }
        }


        /// <summary>
        /// Top HTML body node for custom HTML code.
        /// </summary>
        public virtual string BodyScripts
        {
            get
            {
                return mBodyScripts;
            }
            set
            {
                mBodyScripts = value;
            }
        }


        /// <summary>
        /// CSS file.
        /// </summary>
        public virtual string CssFile
        {
            get
            {
                return mCssFile;
            }
            set
            {
                mCssFile = value;
            }
        }


        /// <summary>
        /// Extended tags.
        /// </summary>
        public virtual string ExtendedTags
        {
            get
            {
                return mExtendedTags;
            }
            set
            {
                mExtendedTags = value;
            }
        }


        /// <summary>
        /// Body class.
        /// </summary>
        public virtual string BodyClass
        {
            get
            {
                var bodyClass = DocumentContext.CurrentBodyClass;

                // Add bootstrap class if allowed
                PortalUIHelper.EnsureBootstrapBodyClass(ref bodyClass, PortalContext.ViewMode, PageContext.CurrentPage);

                return bodyClass;
            }
            set
            {
                DocumentContext.CurrentBodyClass = value;
            }
        }


        /// <summary>
        /// Additional XML namespace to HTML tag.
        /// </summary>
        public virtual string XmlNamespace
        {
            get
            {
                return mXmlNamespace;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    if ((mXmlNamespace == null) || !mXmlNamespace.Contains(value.Trim()))
                    {
                        mXmlNamespace += " " + value;
                        mXmlNamespace = mXmlNamespace.Trim();
                    }
                }
            }
        }


        /// <summary>
        /// Description.
        /// </summary>
        public virtual string Description
        {
            get
            {
                return DocumentContext.CurrentDescription;
            }
            set
            {
                DocumentContext.CurrentDescription = value;
            }
        }


        /// <summary>
        /// Key words.
        /// </summary>
        public virtual string KeyWords
        {
            get
            {
                return DocumentContext.CurrentKeyWords;
            }
            set
            {
                DocumentContext.CurrentKeyWords = value;
            }
        }


        /// <summary>
        /// Title.
        /// </summary>
        public virtual string Title
        {
            get
            {
                return DocumentContext.CurrentTitle;
            }
            set
            {
                DocumentContext.CurrentTitle = value;
            }
        }


        /// <summary>
        /// Favicon tag.
        /// </summary>
        public virtual string FaviconTag
        {
            get
            {
                // Favicon markup cannot be generated when site is not present
                if (CurrentSite == null)
                {
                    return null;
                }

                var faviconBuilder = new FaviconMarkupBuilder(CurrentSite.SiteName);
                return faviconBuilder.GetCachedFaviconMarkup();
            }
        }


        /// <summary>
        /// Gets the value that indicates whether meta tags value should be encoded.
        /// </summary>
        protected static bool EncodeMetaTagValue
        {
            get
            {
                if (mEncodeMetaTagValue == null)
                {
                    mEncodeMetaTagValue = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSEncodeMetaTagValue"], false);
                }

                return mEncodeMetaTagValue.Value;
            }
        }


        /// <summary>
        /// Indicates whether DocType of current page is equal to HTML5 DocType.
        /// </summary>
        public virtual bool IsHTML5
        {
            get
            {
                return CMSString.Compare(DocType, HTMLHelper.DOCTYPE_HTML5, true) == 0;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns true if the full client caching is allowed (no revalidation requests).
        /// </summary>
        public virtual bool AllowClientCache()
        {
            return CacheHelper.ClientCacheRequested;
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="parentPage">Parent page</param>
        public DocumentBase(AbstractCMSPage parentPage)
        {
            mPage = parentPage;
        }


        /// <summary>
        /// Resolves the given URL.
        /// </summary>
        /// <param name="relativeUrl">URL to resolve</param>
        public virtual string ResolveUrl(string relativeUrl)
        {
            return Page.ResolveUrl(relativeUrl);
        }


        /// <summary>
        /// Sets the current thread culture according to the user culture.
        /// </summary>
        public virtual void SetCulture()
        {
            CultureWasSet = true;

            // Set the culture
            string culture = LocalizationContext.PreferredCultureCode;
            if (!String.IsNullOrEmpty(culture))
            {
                CultureInfo ci = CultureHelper.GetCultureInfo(culture);
                Thread.CurrentThread.CurrentCulture = ci;
                Thread.CurrentThread.CurrentUICulture = ci;
                LocalResolver.Culture = ci.ToString();
            }
        }


        /// <summary>
        /// Sets the page caching.
        /// </summary>
        public virtual void SetCaching()
        {
            if (OutputHelper.SetCaching(CurrentSiteName, CurrentPage, Response))
            {
                Page.UseViewStateUserKey = false;
            }
        }


        /// <summary>
        /// Logs the page hit.
        /// </summary>
        public virtual void LogHit()
        {
            // Log the page view hit in live site mode
            RequestContext.LogPageHit = PortalContext.ViewMode.IsLiveSite() && RequestContext.IsContentPage;
        }


        /// <summary>
        /// Adds the CMS page tags.
        /// </summary>
        public virtual void AddCMSTags()
        {
            if (CurrentPage != null)
            {
                if (PortalContext.ViewMode != ViewModeEnum.LiveSite)
                {
                    if (!RequestHelper.IsAsyncPostback())
                    {
                        string tags = "<div>\n<input type=\"hidden\" name=\"vmode\" id=\"vmode\" value=\"" + ViewModeCode.FromEnum(PortalContext.ViewMode) + "\" />\n</div>";
                        ScriptHelper.RegisterClientScriptBlock(Page, typeof(string), "CMSTags", tags);
                    }
                }
            }
        }


        /// <summary>
        /// Sets the CSS for the page
        /// </summary>
        public virtual void SetCSS()
        {
            // If stylesheet found, get the URL
            if (CurrentStylesheet != null)
            {
                mCssFile = CssLinkHelper.GetStylesheetUrl(CurrentStylesheet.StylesheetName);
            }
        }


        /// <summary>
        /// Sets the CSS theme for the page.
        /// </summary>
        public virtual void SetTheme()
        {
            // If stylesheet found, set the theme
            if (CurrentStylesheet != null)
            {
                // Set the Theme if available
                if (FileHelper.DirectoryExists("~/App_Themes/" + CurrentStylesheet.StylesheetName))
                {
                    Page.Theme = CurrentStylesheet.StylesheetName;
                }
            }
        }


        /// <summary>
        /// Runs the PreInit actions of the page.
        /// </summary>
        public virtual void PreInit()
        {
            // Page initialization
            if ((CurrentSite != null) && (CurrentPage != null))
            {
                ViewModeEnum viewMode = PortalContext.ViewMode;

                PageSecurityHelper.RequestSecurePage(CurrentPage, false, viewMode, CurrentSite.SiteName);
                PageSecurityHelper.CheckSecuredAreas(CurrentSite.SiteName, CurrentPage, false, viewMode);

                // Set the caching
                SetCaching();

                if (!CultureWasSet)
                {
                    SetCulture();
                }

                // Set page theme
                SetTheme();

                if (viewMode != ViewModeEnum.LiveSite)
                {
                    Page.MaintainScrollPositionOnPostBack = true;
                }
            }
        }


        /// <summary>
        /// Runs the Load actions of the page.
        /// </summary>
        public virtual void Load()
        {
            if (CurrentPage != null)
            {
                // Log the hit
                LogHit();

                bool isRtl;

                // For dashboard widgets check UI culture instead of preferred UI culture
                if (PortalContext.ViewMode == ViewModeEnum.DashboardWidgets)
                {
                    isRtl = CultureHelper.IsUICultureRTL();
                }
                else
                {
                    isRtl = CultureHelper.IsPreferredCultureRTL();
                }

                if (isRtl)
                {
                    mExtendedTags += CssHelper.GetStyle("* { direction: rtl; }");
                }

                // Register the Design mode CSS for other than live site modes
                if (PortalContext.ViewMode != ViewModeEnum.LiveSite)
                {
                    CssRegistration.RegisterDesignMode(Page);

                    // Register bootstrap for non-live site modes
                    CssRegistration.RegisterBootstrap(Page);
                    ScriptHelper.RegisterBootstrapScripts(Page);

                    if (SystemContext.DevelopmentMode)
                    {
                        RegisterCSS("Test/" + SystemContext.MachineName, "DesignMode.css");
                    }
                }

                // Document header tags
                mExtendedTags += LocalResolver.ResolveMacros(CurrentPage.NodeHeadTags);

                // Page template header tags
                if (CurrentPage.UsedPageTemplateInfo != null)
                {
                    // Check whether headers should be inherited for current template
                    if (CurrentPage.UsedPageTemplateInfo.PageTemplateInheritParentHeader)
                    {
                        string parentExtendedTags = String.Empty;

                        var hierarchy = DocumentContext.CurrentParentPageInfos;
                        for (int i = hierarchy.Count - 1; i >= 0; i--)
                        {
                            PageInfo parent = hierarchy[i];
                            if (parent == null)
                            {
                                break;
                            }

                            // Do not process current document
                            if (parent.NodeID == CurrentPage.NodeID)
                            {
                                continue;
                            }

                            // Check whether inheritance is allowed for parent item
                            if ((parent.UsedPageTemplateInfo != null) && (parent.UsedPageTemplateInfo.PageTemplateAllowInheritHeader) && (TreePathUtils.IsMenuItemType(parent.ClassName)))
                            {
                                // Add to the parent extended tags
                                parentExtendedTags = LocalResolver.ResolveMacros(parent.UsedPageTemplateInfo.PageTemplateHeader) + parentExtendedTags;

                                // Stop loop if parent item should not inherit other headers
                                if (!parent.UsedPageTemplateInfo.PageTemplateInheritParentHeader)
                                {
                                    break;
                                }
                            }

                            // Get next parent page info
                            parent = parent.ParentPageInfo;
                        }

                        // Add header tags to the final tags
                        mExtendedTags += parentExtendedTags;
                    }

                    if (TreePathUtils.IsMenuItemType(CurrentPage.ClassName))
                    {
                        mExtendedTags += LocalResolver.ResolveMacros(CurrentPage.UsedPageTemplateInfo.PageTemplateHeader);
                    }
                }

                // Body parameters
                if (ValidationHelper.GetString(CurrentPage.NodeBodyElementAttributes, "") != "")
                {
                    mBodyParameters += " " + LocalResolver.ResolveMacros(CurrentPage.NodeBodyElementAttributes);
                }

                // Body scripts
                if (ValidationHelper.GetString(CurrentPage.NodeBodyScripts, "") != "")
                {
                    mBodyScripts += " " + LocalResolver.ResolveMacros(CurrentPage.NodeBodyScripts);
                }
            }

            if (Page.Header != null)
            {
                // Do not encode page title for asynchronous postback
                string title = DocumentContext.CurrentTitle;
                if (RequestHelper.IsAsyncPostback())
                {
                    Page.Title = title;
                }
                else
                {
                    Page.Title = HTMLHelper.HTMLEncode(title);
                }
            }
        }


        /// <summary>
        /// Registers the CSS file
        /// </summary>
        /// <param name="theme">Theme</param>
        /// <param name="file">CSS file name</param>
        private void RegisterCSS(string theme, string file)
        {
            string url = CssLinkHelper.GetPhysicalCssUrl(theme, file);
            if (!String.IsNullOrEmpty(url) && !CssLinkHelper.IsCssLinkRegistered(url))
            {
                mExtendedTags += CssLinkHelper.GetCssFileLink(url);
                CssLinkHelper.SetCssLinkRegistered(url);
            }
        }


        /// <summary>
        /// Runs the PreRender actions of the page.
        /// </summary>
        public virtual void PreRender()
        {
            // Add CMS tags
            AddCMSTags();

            if (PortalContext.ViewMode != ViewModeEnum.DashboardWidgets)
            {
                // Set page CSS stylesheet
                SetCSS();
            }
        }


        /// <summary>
        /// Gets the header tags for current page.
        /// </summary>
        public virtual string GetHeaderTags()
        {
            var tags = new StringBuilder();

            // HTML 5
            bool isHTML5 = (CMSString.Compare(DocType, HTMLHelper.DOCTYPE_HTML5, true) == 0);

            // Generator meta tag
            bool freeLicense = false;

            // Try to get license information for current domain
            LicenseKeyInfo lki = LicenseKeyInfoProvider.GetLicenseKeyInfo(RequestContext.CurrentDomain);
            if (lki != null)
            {
                freeLicense = (lki.Edition == ProductEditionEnum.Free);
            }

            if (freeLicense || ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSRenderGeneratorName"], false))
            {
                string freeLicenseText = null;
                if (freeLicense)
                {
                    freeLicenseText = "FREE LICENSE";
                }

                tags.Append("<meta name=\"generator\" content=\"", ResHelper.GetString("general.cmsname"), " ", CMSVersion.MainVersion, " (build ", CMSVersion.GetVersion(true, true, true, false, false), ") ", freeLicenseText, "\" /> \n");
            }

            // Page description
            string description = Description;
            if (!String.IsNullOrEmpty(description))
            {
                if (EncodeMetaTagValue)
                {
                    tags.Append("<meta name=\"description\" content=\"", HTMLHelper.HTMLEncode(description), "\" /> \n");
                }
                else
                {
                    description = description.Replace('"', ' ');
                    tags.Append("<meta name=\"description\" content=\"", description, "\" /> \n");
                }
            }

            // Add encoding
            if (isHTML5)
            {
                tags.Append("<meta charset=\"UTF-8\" /> \n");
            }
            else
            {
                tags.Append("<meta http-equiv=\"content-type\" content=\"text/html; charset=UTF-8\" /> \n");
            }

            // No cache in live site
            //if (PortalContext.ViewMode != ViewModeEnum.LiveSite)
            //{
            if (isHTML5)
            {
                Response.CacheControl = "no-cache";
            }
            else
            {
                tags.Append("<meta http-equiv=\"pragma\" content=\"no-cache\" /> \n");
            }
            //}

            // WAI standard, but not for HTML 5
            if (!isHTML5)
            {
                tags.Append("<meta http-equiv=\"content-style-type\" content=\"text/css\" /> \n");
                tags.Append("<meta http-equiv=\"content-script-type\" content=\"text/javascript\" /> \n");
            }

            // Page keywords
            string keywords = KeyWords;
            if (!String.IsNullOrEmpty(keywords))
            {
                if (EncodeMetaTagValue)
                {
                    tags.Append("<meta name=\"keywords\" content=\"", HTMLHelper.HTMLEncode(keywords), "\" /> \n");
                }
                else
                {
                    keywords = keywords.Replace('"', ' ');
                    tags.Append("<meta name=\"keywords\" content=\"", keywords, "\" /> \n");
                }
            }

            // Add noindex meta tag for documents excluded from search
            PageInfo pi = DocumentContext.CurrentPageInfo;
            if ((pi != null) && (pi.DocumentSearchExcluded))
            {
                tags.Append("<meta name=\"robots\" content=\"noindex,nofollow\" /> \n");
            }

            string cssFile = CssFile;

            // Page stylesheets
            if (!String.IsNullOrEmpty(cssFile))
            {
                if (!QueryHelper.GetBoolean("clientcache", true))
                {
                    // For IE add unique CSS link - 'clientcache=0' is not sufficient for IE in preview.
                    if (BrowserHelper.IsIE())
                    {
                        cssFile += "&guid=" + Guid.NewGuid();
                    }

                    cssFile += "&clientcache=0";
                }

                tags.Append(CssLinkHelper.GetCssFileLink(cssFile));
            }

            // Additional tags
            tags.Append(mExtendedTags, " \n");

            // Favicon
            tags.Append(FaviconTag);

            return tags.ToString();
        }

        #endregion
    }
}
