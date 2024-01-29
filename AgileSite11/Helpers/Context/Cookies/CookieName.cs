using CMS.Base;

namespace CMS.Helpers
{
    /// <summary>
    /// System cookie name constants
    /// </summary>
    public static class CookieName
    {
        #region "System cookies"

        /// <summary>
        /// Cookie level
        /// </summary>
        public const string CookieLevel = "CMSCookieLevel";

        #endregion


        #region "Essential cookies"

        /// <summary>
        /// ASP.NET session ID cookie
        /// </summary>
        public const string ASPNETSessionID = "ASP.NET_SessionId";

        /// <summary>
        /// Remembered windows user that is logged in
        /// </summary>
        public const string WindowsUser = "CMSWindowsUser";

        /// <summary>
        /// Preferred content culture
        /// </summary>
        public const string PreferredCulture = "CMSPreferredCulture";
        
        /// <summary>
        /// Redirection flag for the mobile device
        /// </summary>
        public const string MobileRedirected = "CMSMobileRedirected";

        /// <summary>
        /// Current theme
        /// </summary>
        public const string CurrentTheme = "CMSCurrentTheme";

        /// <summary>
        /// Live ID authentication
        /// </summary>
        public const string LiveID = "webauthtoken";
                
        /// <summary>
        /// Shopping cart
        /// </summary>
        public const string ShoppingCart = "CMSShoppingCart";

        /// <summary>
        /// Forum post answer flag to prevent multiple votes
        /// </summary>
        public const string ForumPostAnswer = "CMSForumPostAnswer";

        /// <summary>
        /// Voted polls to prevent multiple votes
        /// </summary>
        public const string VotedPolls = "CMSVotedPolls";
                
        /// <summary>
        /// Rated documents to prevent multiple votes
        /// </summary>
        public const string RatedDocuments = "CMSRatedDocuments";

        /// <summary>
        /// Login token for the chat module
        /// </summary>
        public const string ChatLoggedInToken = "ChatLoggedInToken";

        /// <summary>
        /// Login token for the chat module (support chat)
        /// </summary>
        public const string ChatSupportLoggedInToken = "ChatSupportLoggedInToken";

        /// <summary>
        /// Body CSS class cookie
        /// </summary>
        public const string BodyClass = "CMSBodyClass";

        /// <summary>
        /// Current device profile name
        /// </summary>
        public const string ShowDesktopVersion = "CMSShowDesktopVersion";

        /// <summary>
        /// CSRF cookie used to store CSRF token
        /// </summary>
        public const string CsrfCookie = "CMSCsrfCookie";

        
        #endregion


        #region "Editor cookies"
        
        /// <summary>
        /// Preferred UI culture
        /// </summary>
        public const string PreferredUICulture = "CMSPreferredUICulture";

        /// <summary>
        /// View mode
        /// </summary>
        public const string ViewMode = "CMSViewMode";

        /// <summary>
        /// Spell check user words
        /// </summary>
        public const string SpellCheckUserWords = "CMSUserWords";

        /// <summary>
        /// Macro designer tab
        /// </summary>
        public const string MacroDesignerTab = "CMSMacroDesignerTab";

        /// <summary>
        /// Web part toolbar minimized
        /// </summary>
        public const string WebPartToolbarMinimized = "CMSWebPartToolbarMinimized";

        /// <summary>
        /// Web part toolbar category
        /// </summary>
        public const string WebPartToolbarCategory = "CMSWebPartToolbarCategory";

        /// <summary>
        /// Session token
        /// </summary>
        public const string SessionToken = "CMSSessionToken";
        
        /// <summary>
        /// Preview state for transformation/layout/css preview
        /// </summary>
        public const string PreviewState = "CMSPreviewState";

        /// <summary>
        /// State of the compare mode
        /// </summary>
        public const string SplitMode = "CMSSplitMode";

        /// <summary>
        /// Settings for UniGraph control
        /// </summary>
        public const string UniGraph = "CMSUniGraph";

        /// <summary>
        /// Properties tab selection
        /// </summary>
        public const string PropertyTab = "CMSPropertyTab";

        /// <summary>
        /// View tab selection
        /// </summary>
        public const string ViewTab = "CMSViewTab";

        /// <summary>
        /// Validation tab selection
        /// </summary>
        public const string ValidationTab = "CMSValidationTab";

        /// <summary>
        /// General prefix for editor cookies for the proper level
        /// </summary>
        public const string EditorPrefix = "CMSEd";

        /// <summary>
        /// Prefix for AB test result overview selectors
        /// </summary>
        public const string ABSelectorStatePrefix = EditorPrefix + "ABSelectorState";

        /// <summary>
        /// Prefix for variant slider cookies
        /// </summary>
        public const string VariantSliderPositionsPrefix = EditorPrefix + "VariantSliderPositions";

        /// <summary>
        /// Gets the cookie name for the position of the variant slider
        /// </summary>
        /// <param name="templateId">Template ID</param>
        public static string GetVariantSliderPositionsCookieName(int templateId)
        {
            return VariantSliderPositionsPrefix + templateId;
        }

        /// <summary>
        /// Current device profile name
        /// </summary>
        public const string CurrentDeviceProfileName = "CMSCurrentDeviceProfileName";

        /// <summary>
        /// Gets the cookie name for the device rotation state
        /// </summary>
        public const string CurrentDeviceProfileRotate = "CMSCurrentDeviceProfileRotate";
        
        /// <summary>
        /// Cookie key for user impersonation
        /// </summary>
        public const string Impersonation = "CMSImpersonation";

        /// <summary>
        /// Cookie key for the indication whether content of web parts should be displayed in the design mode
        /// </summary>
        public const string DisplayContentInDesignMode = "DisplayContentInDesignMode";

        /// <summary>
        /// Cookie key for the indication whether content of UI web parts should be displayed in the design mode
        /// </summary>
        public const string DisplayContentInUIElementDesignMode = "DisplayContentInUIElementDesignMode";


        /// <summary>
        /// Gets the editor level cookie name
        /// </summary>
        /// <param name="cookieName">Cookie name</param>
        public static string GetEditorCookieName(string cookieName)
        {
            return EditorPrefix + cookieName;
        }

        #endregion


        #region "Visitor cookies"

        /// <summary>
        /// Visitor status
        /// </summary>
        public const string VisitorStatus = "VisitorStatus";
        
        /// <summary>
        /// Visit status
        /// </summary>
        public const string VisitStatus = "VisitStatus";

        /// <summary>
        /// Current visit status
        /// </summary>
        public const string CurrentVisitStatus = "CurrentVisitStatus";

        /// <summary>
        /// Campaign
        /// </summary>
        public const string Campaign = "Campaign";

        /// <summary>
        /// Source
        /// </summary>
        public const string Source = "Source";

        /// <summary>
        /// Content
        /// </summary>
        public const string Content = "Content";

        /// <summary>
        /// URL referrer
        /// </summary>
        public const string UrlReferrer = "UrlReferrer";

        /// <summary>
        /// Current contact
        /// </summary>
        public const string CurrentContact = "CurrentContact";

        /// <summary>
        /// Prefix for MVT tests
        /// </summary>
        public const string MVTPrefix = "CMSMVT";

        /// <summary>
        /// Prefix for pages without MVT test
        /// </summary>
        public const string NoTestMVTPrefix = EditorPrefix + "NoTestMVT";

        /// <summary>
        /// Stores all the web analytics Campaigns, which should be tracked within a JavaScript snippet.
        /// </summary>
        public const string TrackedCampaigns = "TrackedCampaigns";

        /// <summary>
        /// Stores landing page visited flag.
        /// </summary>
        public const string LandingPageLoaded = "CMSLandingPageLoaded";


        /// <summary>
        /// Gets the MVT test cookie name
        /// </summary>
        /// <param name="mvtTestName">MVT test name</param>
        public static string GetMVTCookieName(string mvtTestName)
        {
            return MVTPrefix + mvtTestName.ToLowerCSafe();
        }


        /// <summary>
        /// Gets the cookie name when no MVT test is present
        /// </summary>
        /// <param name="templateId">Template ID</param>
        public static string GetNoMVTCookieName(int templateId)
        {
            return NoTestMVTPrefix + templateId;
        }

        #endregion
    }
}
