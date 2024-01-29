using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.DocumentEngine.Web.UI;
using CMS.Base.Web.UI.ActionsConfig;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Localization;
using CMS.MacroEngine;
using CMS.Membership;
using CMS.Modules;
using CMS.OutputFilter;
using CMS.PortalEngine;
using CMS.PortalEngine.Web.UI;
using CMS.SiteProvider;
using CMS.Synchronization;

using CultureInfo = System.Globalization.CultureInfo;
using TreeNode = CMS.DocumentEngine.TreeNode;

namespace CMS.UIControls
{
    /// <summary>
    /// Base page for all CMS administration UI pages
    /// </summary>
    public abstract class CMSPage : AbstractCMSPage, IAdminPage
    {
        #region "Events"

        /// <summary>
        /// Fires before page PreInit
        /// </summary>
        public static event BeforeEventHandler OnBeforePagePreInit;


        /// <summary>
        /// Fires after page PreInit
        /// </summary>
        public static event EventHandler OnAfterPagePreInit;


        /// <summary>
        /// Fires before page Init
        /// </summary>
        public static event BeforeEventHandler OnBeforePageInit;


        /// <summary>
        /// Fires after page Init
        /// </summary>
        public static event EventHandler OnAfterPageInit;


        /// <summary>
        /// Fires before page Load
        /// </summary>
        public static event BeforeEventHandler OnBeforePageLoad;


        /// <summary>
        /// Fires after page Load
        /// </summary>
        public static event EventHandler OnAfterPageLoad;


        /// <summary>
        /// Fires before page PreRender
        /// </summary>
        public static event BeforeEventHandler OnBeforePagePreRender;


        /// <summary>
        /// Fires after page PreRender
        /// </summary>
        public static event EventHandler OnAfterPagePreRender;


        /// <summary>
        /// Fires before page Render
        /// </summary>
        public static event BeforeRenderEventHandler OnBeforePageRender;


        /// <summary>
        /// Fires after page Render
        /// </summary>
        public static event RenderEventHandler OnAfterPageRender;


        /// <summary>
        /// Tab creation event.
        /// </summary>
        public event UITabs.TabCreatedEventHandler OnTabCreated;


        /// <summary>
        /// Check security event.
        /// </summary>
        public event UITabs.CheckTabSecurityEventHandler OnCheckTabSecurity;

        #endregion


        #region "Static variables and constants"

        /// <summary>
        /// Width of the export object modal window
        /// </summary>
        public const int EXPORT_OBJECT_WIDTH = 750;


        /// <summary>
        /// Height of the export object modal window
        /// </summary>
        public const int EXPORT_OBJECT_HEIGHT = 230;


        /// <summary>
        /// If true, the administration interface is disabled
        /// </summary>
        private static bool? mDisableAdministrationInterface;

        #endregion


        #region "Variables"

        /// <summary>
        /// Generic body class - contains the base body CSS class
        /// </summary>
        protected string mBodyClass = String.Empty;


        private ICMSMasterPage mCurrentMaster;
        private string mRelativePath;
        private Control mPageStatusContainer;
        private CurrentUserInfo mCurrentUser;
        private string mCurrentSiteName;
        private bool mCheckHashValidationAttribute = true;
        private PageTitle mPageTitle;
        private bool? mRequiresDialog;
        private bool mRegisterPageLoadedScript = true;


        // Indicates whether wOpeber is loaded
        private bool mRegisterWOpener = true;


        // Indicates whether base tag should be added
        private bool mAddBaseTag = true;


        // Indicates whether view state of the page should be logged for debugging
        private bool logViewState = true;


        // Indicates whether modal page script is loaded
        private bool modalPageScriptsRegistered;


        // HTML tags that should be rendered into the page's header during pre-render
        private readonly StringBuilder headerTags = new StringBuilder();

        #endregion


        #region "Properties"

        /// <summary>
        /// Indicates if split mode frame should be refreshed always on post-back (even for different cultures). 
        /// </summary>
        protected bool SplitModeAllwaysRefresh
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if page loaded script should be registered. 
        /// </summary>
        protected bool RegisterPageLoadedScript
        {
            get
            {
                return mRegisterPageLoadedScript;
            }
            set
            {
                mRegisterPageLoadedScript = value;
            }
        }


        /// <summary>
        /// Current CMSMasterPage page
        /// </summary>
        public ICMSMasterPage CurrentMaster
        {
            get
            {
                return mCurrentMaster ?? (mCurrentMaster = (ICMSMasterPage)Master);
            }
        }


        /// <summary>
        /// Page title
        /// </summary>
        public PageTitle PageTitle
        {
            get
            {
                return mPageTitle ?? CurrentMaster.Title;
            }
            set
            {
                mPageTitle = value;
            }
        }


        /// <summary>
        /// Page title
        /// </summary>
        public Breadcrumbs PageBreadcrumbs
        {
            get
            {
                return PageTitle.Breadcrumbs;
            }
        }


        /// <summary>
        /// Height of frame with tabs
        /// </summary>
        public static int TabsFrameHeight
        {
            get
            {
                return 96;
            }
        }


        /// <summary>
        /// Height of frame containing tabs only
        /// </summary>
        public static int TabsOnlyHeight
        {
            get
            {
                return 48;
            }
        }


        /// <summary>
        /// Height of frame containing title only
        /// </summary>
        public static int TitleOnlyHeight
        {
            get
            {
                return 48;
            }
        }


        /// <summary>
        /// Height of frame with footer
        /// </summary>
        public static int FooterFrameHeight
        {
            get
            {
                return 64;
            }
        }


        /// <summary>
        /// Returns the object type name
        /// </summary>
        public string TypeName
        {
            get
            {
                return GetType().Name;
            }
        }


        /// <summary>
        /// Page relative path
        /// </summary>
        public string RelativePath
        {
            get
            {
                return mRelativePath ?? (mRelativePath = URLHelper.RemoveApplicationPath(HttpContext.Current.Request.Path));
            }
        }


        /// <summary>
        /// Body class
        /// </summary>
        public string BodyClass
        {
            get
            {
                return mBodyClass;
            }
            set
            {
                mBodyClass = value;
            }
        }


        /// <summary>
        /// Container for the page and script managers
        /// </summary>
        public override Control ManagersContainer
        {
            get
            {
                if (base.ManagersContainer == null)
                {
                    // Take the status container as the default one
                    return PageStatusContainer;
                }

                return base.ManagersContainer;
            }
            set
            {
                base.ManagersContainer = value;
            }
        }


        /// <summary>
        /// Page status container
        /// </summary>
        protected Control PageStatusContainer
        {
            get
            {
                if (mPageStatusContainer == null)
                {
                    // Find status container within the master page
                    if ((CurrentMaster != null) && (CurrentMaster.PageStatusContainer != null))
                    {
                        mPageStatusContainer = CurrentMaster.PageStatusContainer;
                    }
                    else
                    {
                        mPageStatusContainer = Form;
                    }
                }

                return mPageStatusContainer;
            }
            set
            {
                mPageStatusContainer = value;
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates whether base tag with target _self should be added to the header
        /// </summary>
        public bool AddBaseTag
        {
            get
            {
                return mAddBaseTag;
            }
            set
            {
                mAddBaseTag = value;
            }
        }


        /// <summary>
        /// If true, the page registers the wopener JavaScript variable
        /// </summary>
        public bool RegisterWOpener
        {
            get
            {
                return mRegisterWOpener;
            }
            set
            {
                mRegisterWOpener = value;
            }
        }


        /// <summary>
        /// Current user
        /// </summary>
        protected CurrentUserInfo CurrentUser
        {
            get
            {
                // Get the user form context
                return mCurrentUser ?? (mCurrentUser = MembershipContext.AuthenticatedUser);
            }
            set
            {
                mCurrentUser = value;
            }
        }


        /// <summary>
        /// Current site name
        /// </summary>
        public virtual string CurrentSiteName
        {
            get
            {
                // Get the site name from context
                return mCurrentSiteName ?? (mCurrentSiteName = SiteContext.CurrentSiteName ?? String.Empty);
            }
            set
            {
                mCurrentSiteName = value;
            }
        }


        /// <summary>
        /// Special property allowing to treat edited object as non-static property
        /// Usable when you need to store information about edited object and then after post back reload it based on id and object type 
        /// </summary>
        public object PersistentEditedObject
        {
            get
            {
                if ((EditedObject == null) && (EditedObjectID > 0) && !String.IsNullOrEmpty(EditedObjectType))
                {
                    EditedObject = ProviderHelper.GetInfoById(EditedObjectType, EditedObjectID);
                }

                return EditedObject;
            }
            set
            {
                EditedObject = value;

                if (EditedObject is BaseInfo)
                {
                    BaseInfo edObject = EditedObject as BaseInfo;

                    EditedObjectType = edObject.TypeInfo.ObjectType;
                    EditedObjectID = edObject.Generalized.ObjectID;
                }
            }
        }


        /// <summary>
        /// Edited object ID stored in view state to initialize EditedObject property
        /// </summary>
        private int EditedObjectID
        {
            get
            {
                return ValidationHelper.GetInteger(ViewState["EditedObjectID"], 0);
            }
            set
            {
                ViewState["EditedObjectID"] = value;
            }
        }


        /// <summary>
        /// Edited object type stored in view state to initialize EditedObject property
        /// </summary>
        private string EditedObjectType
        {
            get
            {
                return ValidationHelper.GetString(ViewState["EditedObjectType"], null);
            }
            set
            {
                ViewState["EditedObjectType"] = value;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public bool CheckHashValidationAttribute
        {
            get
            {
                return mCheckHashValidationAttribute;
            }
            set
            {
                mCheckHashValidationAttribute = value;
            }
        }

        #endregion


        #region "Static properties"

        /// <summary>
        /// If true, the administration interface is disabled
        /// </summary>
        public bool DisableAdministrationInterface
        {
            get
            {
                if (mDisableAdministrationInterface == null)
                {
                    mDisableAdministrationInterface = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSDisableAdministrationInterface"], false);
                }

                return mDisableAdministrationInterface.Value;
            }
            set
            {
                mDisableAdministrationInterface = value;
            }
        }


        /// <summary>
        /// Object edited by the current page. If set to NULL, redirects to the information page with information that object has been deleted.
        /// </summary>
        public object EditedObject
        {
            get
            {
                return UIContext.EditedObject;
            }
            set
            {
                UIContext.EditedObject = value;
            }
        }


        /// <summary>
        /// Parent of object edited by the current page. If set to NULL, redirects to the information page with information that object has been deleted.
        /// </summary>
        public object EditedObjectParent
        {
            get
            {
                return UIContext.EditedObjectParent;
            }
            set
            {
                UIContext.EditedObject = value;
            }
        }


        /// <summary>
        /// Document edited by the current page. If set to NULL, redirects to the information page with information that document has been deleted.
        /// </summary>
        public static TreeNode EditedDocument
        {
            get
            {
                return DocumentContext.EditedDocument;
            }
            set
            {
                if (value == null)
                {
                    RedirectToInformation("editeddocument.notexists");
                }
                else
                {
                    DocumentContext.EditedDocument = value;
                }
            }
        }


        /// <summary>
        /// Indicates if screen is locked.
        /// </summary>
        public static bool IsScreenLocked
        {
            get
            {
                return (SessionHelper.GetValue("ScreenLockIsLocked") != null) && (bool)SessionHelper.GetValue("ScreenLockIsLocked");
            }
            set
            {
                SessionHelper.SetValue("ScreenLockIsLocked", value);
            }
        }


        /// <summary>
        /// Time of last request (not counting ScreenLock callbacks).
        /// </summary>
        public static DateTime LastRequest
        {
            get
            {
                return (SessionHelper.GetValue("ScreenLockLastRequest") == null) ? DateTime.Now : (DateTime)SessionHelper.GetValue("ScreenLockLastRequest");
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates whether the page is displayed as dialog. 
        /// </summary>
        public bool RequiresDialog
        {
            get
            {
                if (mRequiresDialog == null)
                {
                    mRequiresDialog = (QueryHelper.GetBoolean("dialog", false) && (!String.IsNullOrEmpty(Page.MasterPageFile)) && (!(Page is CMSModalDesignPage)));
                }
                return mRequiresDialog.Value;
            }
            set
            {
                mRequiresDialog = value;
            }
        }

        #endregion


        #region "Page cycle events"

        /// <summary>
        /// PreInit event handler
        /// </summary>
        protected override void OnPreInit(EventArgs e)
        {
            RequestDebug.LogRequestOperation("OnPreInit", null, 1);

            if (!String.IsNullOrEmpty(Theme))
            {
                Theme = URLHelper.CustomTheme;
            }

            // Set dialog master page for dialog mode
            if (RequiresDialog)
            {
                Page.MasterPageFile = ResolveUrl("~/CMSMasterPages/UI/Dialogs/ModalDialogPage.master");
            }

            // Before event
            if ((OnBeforePagePreInit == null) || OnBeforePagePreInit(this, e))
            {
                base.OnPreInit(e);
            }

            // After event
            if (OnAfterPagePreInit != null)
            {
                OnAfterPagePreInit(this, e);
            }
        }


        /// <summary>
        /// Init event handler
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            RequestDebug.LogRequestOperation("OnInit", null, 1);

            // Before event
            if ((OnBeforePageInit == null) || OnBeforePageInit(this, e))
            {
                base.OnInit(e);
            }

            // After event
            if (OnAfterPageInit != null)
            {
                OnAfterPageInit(this, e);
            }
        }


        /// <summary>
        /// Load event handler
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            RequestDebug.LogRequestOperation("OnLoad", null, 1);

            // Before event
            if ((OnBeforePageLoad == null) || OnBeforePageLoad(this, e))
            {
                base.OnLoad(e);

                Response.Cache.SetNoStore();
            }

            // ScreenLock handling
            HandleScreenLock();

            // Register scripts necessary for dialogs
            /*** [FUTURE VERSION HINT] - following call can be moved to dialog master pages and base pages when all dialog pages with no exceptions inherit them ***/
            RegisterDialogHandlingScripts();

            // Register the development mode styles
            if (SystemContext.DevelopmentMode && DatabaseHelper.IsDatabaseAvailable)
            {
                string url = CssLinkHelper.GetThemeCssUrl("Test/" + SystemContext.MachineName, "CMSDesk.css", true);
                CssRegistration.RegisterCssLink(this, url);
            }

            // After event
            if (OnAfterPageLoad != null)
            {
                OnAfterPageLoad(this, e);
            }
        }


        /// <summary>
        /// PreRender event handler
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            RequestDebug.LogRequestOperation("OnPreRender", null, 1);

            // Before event
            if ((OnBeforePagePreRender == null) || OnBeforePagePreRender(this, e))
            {
                base.OnPreRender(e);
            }

            // Log view state
            if (logViewState && ViewStateDebug.DebugCurrentRequest)
            {
                ViewStateDebug.GetViewStates(this);
            }

            AddWAIRequiredTags();

            AddCharsetMetaTag();

            AddFavIconTags();

            RednerHeaderTags();

            // Register scripts handling page loaded event
            if (RegisterPageLoadedScript)
            {
                ScriptHelper.RegisterPageLoadedEvent(this);
            }

            ScriptHelper.RenderAngularModulesScript(this);

            // After event
            if (OnAfterPagePreRender != null)
            {
                OnAfterPagePreRender(this, e);
            }
        }


        /// <summary>
        /// Render event handler
        /// </summary>
        protected override void Render(HtmlTextWriter writer)
        {
            RequestDebug.LogRequestOperation("Render", null, 1);

            // Before event
            if ((OnBeforePageRender == null) || OnBeforePageRender(this, writer))
            {
                base.Render(writer);
            }

            // After event
            if (OnAfterPageRender != null)
            {
                OnAfterPageRender(this, writer);
            }
        }

        #endregion


        #region "Access denied methods"

        /// <summary>
        /// Gets the URL to Access denied page
        /// </summary>
        /// <param name="pageUrl">Access denied page URL</param>
        /// <param name="resourceName">Resource name that failed</param>
        /// <param name="permissionName">Permission name that failed</param>
        /// <param name="uiElementName">UI element name that failed</param>
        /// <param name="message">Custom message</param>
        public static string GetAccessDeniedUrl(string pageUrl, string resourceName, string permissionName, string uiElementName, string message)
        {
            return AdministrationUrlHelper.GetAccessDeniedUrl(resourceName, permissionName, uiElementName, message, pageUrl);
        }


        /// <summary>
        /// Gets the URL for CMS Desk Access denied page
        /// </summary>
        /// <param name="resourceName">Resource name that failed</param>
        /// <param name="uiElementName">UI element name that failed</param>
        public static string GetCMSDeskUIElementAccessDenied(string resourceName, string uiElementName)
        {
            return GetAccessDeniedUrl(AdministrationUrlHelper.ADMIN_ACCESSDENIED_PAGE, resourceName, null, uiElementName, null);
        }


        /// <summary>
        /// Redirects the user to Access denied page
        /// </summary>
        /// <param name="pageUrl">Access denied page URL</param>
        protected static void RedirectToAccessDeniedPage(string pageUrl)
        {
            SecurityDebug.LogSecurityOperation(MembershipContext.AuthenticatedUser.UserName, "RedirectToAccessDenied", null, null, null, SiteContext.CurrentSiteName);


            if (UIContext.Current.IsDialog)
            {
                pageUrl = URLHelper.AddParameterToUrl(pageUrl, "dialog", "1");
            }

            URLHelper.Redirect(pageUrl);
        }


        /// <summary>
        /// Redirects the user to Access denied page
        /// </summary>
        /// <param name="pageUrl">Access denied page URL</param>
        /// <param name="resourceName">Resource name that failed</param>
        /// <param name="permissionName">Permission name that failed</param>
        /// <param name="uiElementName">UI element name that failed</param>
        /// <param name="message">Custom message</param>
        protected static void RedirectToAccessDenied(string pageUrl, string resourceName, string permissionName, string uiElementName, string message = null)
        {
            RedirectToAccessDeniedPage(GetAccessDeniedUrl(pageUrl, resourceName, permissionName, uiElementName, message));
        }


        /// <summary>
        /// Redirects the user to Access denied page
        /// </summary>
        /// <param name="resourceName">Resource name that failed</param>
        /// <param name="permissionName">Permission name that failed</param>
        public static void RedirectToAccessDenied(string resourceName, string permissionName)
        {
            RedirectToAccessDeniedPage(GetAccessDeniedUrl(AdministrationUrlHelper.ADMIN_ACCESSDENIED_PAGE, resourceName, permissionName, null, null));
        }


        /// <summary>
        /// Redirects the user to CMS Desk Access denied page
        /// </summary>
        /// <param name="resourceName">Resource name that failed</param>
        /// <param name="uiElement">Permission name that failed</param>
        public static void RedirectToUIElementAccessDenied(string resourceName, string uiElement)
        {
            RedirectToAccessDeniedPage(GetAccessDeniedUrl(AdministrationUrlHelper.ADMIN_ACCESSDENIED_PAGE, resourceName, String.Empty, uiElement, null));
        }


        /// <summary>
        /// Redirect to access denied page with error text not available on site
        /// </summary>
        /// <param name="resourceName">Missing resource name</param>
        public static void RedirectToAccessDeniedResourceNotAvailableOnSite(string resourceName)
        {
            if (HttpContext.Current != null)
            {
                RedirectToAccessDeniedPage(GetAccessDeniedUrl(AdministrationUrlHelper.ADMIN_ACCESSDENIED_PAGE, resourceName, null, null, String.Format(ResHelper.GetString("cmsdesk.resourcenotavailableonsite"), resourceName)));
            }
        }


        /// <summary>
        /// Redirects the user to Access denied page
        /// </summary>
        /// <param name="message">Displayed access denied message.</param>
        public static void RedirectToAccessDenied(string message)
        {
            if (HttpContext.Current != null)
            {
                RedirectToAccessDeniedPage(GetAccessDeniedUrl(AdministrationUrlHelper.ADMIN_ACCESSDENIED_PAGE, null, null, null, message));
            }
        }


        /// <summary>
        /// Redirects the user to the access denied page with info about resource not being assigned to site.
        /// </summary>     
        /// <param name="resourceName">Name of the resource</param>
        public static void RedirectToResourceNotAvailableOnSite(string resourceName)
        {
            RedirectToAccessDenied(String.Format(ResHelper.GetString("cmsdesk.resourcenotavailableonsite"), resourceName));
        }


        /// <summary>
        /// Redirects the user to the info page which says that the UI of the requested page is not available.
        /// </summary>        
        public static void RedirectToUINotAvailable()
        {
            SecurityDebug.LogSecurityOperation(MembershipContext.AuthenticatedUser.UserName, "RedirectToUINotAvailable", null, null, null, SiteContext.CurrentSiteName);

            RedirectToInformation("uiprofile.uinotavailable");
        }


        /// <summary>
        /// Redirects the user to the info page which displays specified message.
        /// </summary>     
        /// <param name="message">Message which should be displayed.</param>
        public static void RedirectToInformation(string message)
        {
            URLHelper.SeeOther(GetInformationUrl(message));
        }


        /// <summary>
        /// Gets URL for the info page which displays specified message.
        /// </summary>
        /// <param name="message">Message to display</param>
        /// <returns>URL of info page</returns>
        private static string GetInformationUrl(string message)
        {
            return AdministrationUrlHelper.GetInformationUrl(message);
        }

        #endregion


        #region "Permission checking"

        /// <summary>
        /// Checks if the administration interface is enabled, if not, redirects to access denied
        /// </summary>
        public void CheckAdministrationInterface()
        {
            if (DisableAdministrationInterface)
            {
                RedirectToAccessDenied("cms.adminui", null);
            }
        }


        /// <summary>
        /// Checks if the user is global administrator (can access the administration (development) page, or a hash is valid)
        /// </summary>
        protected void ValidateHash()
        {
            QueryHelper.ValidateHash("hash");
        }


        /// <summary>
        /// Checks whether user is global administrator with access to all applications
        /// User without global access is redirected to access denied page.
        /// </summary>        
        public bool CheckGlobalAdministrator()
        {
            // Check global admin
            if (!CurrentUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.GlobalAdmin))
            {
                RedirectToAccessDenied(ResHelper.GetString("accessdeniedtopage.globaladminrequired"));
                return false;
            }

            return true;
        }


        /// <summary>
        /// Checks whether the user is an editor
        /// </summary>
        public bool CheckEditor()
        {
            if (!CurrentUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Editor, CurrentSiteName))
            {
                URLHelper.Redirect(GetAccessDeniedUrl(AdministrationUrlHelper.ADMIN_ACCESSDENIED_PAGE, null, null, null, GetString("CMSDesk.IsNotEditorMsg")));
                return false;
            }
            return true;
        }


        /// <summary>
        /// Checks if current site is valid. Redirects to invalid web site if not.
        /// </summary>
        public bool CheckSite()
        {
            if (String.IsNullOrEmpty(CurrentSiteName))
            {
                URLHelper.Redirect("~/CMSMessages/invalidwebsite.aspx");

                return false;
            }

            return true;
        }


        /// <summary>
        /// Checks the license for the given feature. Redirects to feature not available if not available.
        /// </summary>
        /// <param name="feature">Feature to check</param>
        public bool CheckLicense(FeatureEnum feature)
        {
            // Check the license
            string currentDomain = RequestContext.CurrentDomain;
            if (!String.IsNullOrEmpty(currentDomain))
            {
                return LicenseHelper.CheckFeatureAndRedirect(currentDomain, feature);
            }

            return false;
        }


        /// <summary>
        /// Checks the security of the resource, returns true if the module is assigned to the site
        /// </summary>
        /// <param name="resourceName">Resource name</param>
        /// <param name="siteName">Site name</param>
        public bool CheckResourceSite(string resourceName, string siteName)
        {
            if (!ResourceSiteInfoProvider.IsResourceOnSite(resourceName, siteName))
            {
                RedirectToResourceNotAvailableOnSite(resourceName);

                return false;
            }

            return true;
        }


        /// <summary>
        /// Checks the security of the permissions, returns true if the security check succeeded
        /// </summary>
        /// <param name="resourceName">Resource name</param>
        /// <param name="permission">Permission name</param>
        public bool CheckPermissions(string resourceName, string permission)
        {
            if (!CurrentUser.IsAuthorizedPerResource(resourceName, permission, CurrentSiteName))
            {
                RedirectToAccessDenied(resourceName, permission);

                return false;
            }

            return true;
        }


        /// <summary>
        /// Checks the security of the UI elements, returns true if the security check succeeded
        /// </summary>
        /// <param name="resourceName">Resource name</param>
        /// <param name="uiElements">List of UI elements</param>
        public bool CheckUIElements(string resourceName, params string[] uiElements)
        {
            // Check for the resource items
            if (!CurrentUser.IsAuthorizedPerUIElement(resourceName, uiElements, CurrentSiteName))
            {
                RedirectToUIElementAccessDenied(resourceName, String.Join(";", uiElements));

                return false;
            }

            return true;
        }


        /// <summary>
        /// Sets the page title
        /// </summary>
        /// <param name="text">Title text</param>
        public void SetTitle(string text)
        {
            var master = CurrentMaster;
            if (master == null)
            {
                return;
            }

            var title = master.Title;

            if (text != null)
            {
                title.TitleText = text;
            }
        }


        /// <summary>
        /// Sets the page title
        /// </summary>
        /// <param name="helpTopic">Help topic</param>
        /// <param name="helpName">Help name</param>
        public void SetHelp(string helpTopic, string helpName)
        {
            var master = CurrentMaster;
            if (master == null)
            {
                return;
            }

            var title = master.Title;

            if (helpTopic != null)
            {
                title.HelpTopicName = helpTopic;
            }
            if (helpName != null)
            {
                title.HelpName = helpName;
            }
        }


        /// <summary>
        /// Initializes the array for the tabs data
        /// </summary>
        /// <param name="moduleName">Name of the module from which the UI elements should be loaded as tabs.</param>
        /// <param name="elementName">Name of the UI element whose child UI elements should be loaded as tabs. If not specified, module root UI element is used.</param>
        /// <param name="targetFrame">Name of the target frame.</param>
        public void InitTabs(string moduleName, string elementName, string targetFrame)
        {
            var master = CurrentMaster;
            if (master != null)
            {
                UITabs tabs = master.Tabs;
                tabs.ModuleName = moduleName;
                tabs.ElementName = elementName;
                tabs.UrlTarget = targetFrame;
            }
        }


        /// <summary>
        /// Initializes the array for the tabs data
        /// </summary>
        /// <param name="targetFrame">Target frame name</param>
        public void InitTabs(string targetFrame)
        {
            var master = CurrentMaster;
            if (master == null)
            {
                return;
            }
            UITabs tabs = master.Tabs;

            if (targetFrame != null)
            {
                tabs.UrlTarget = targetFrame;
            }

            EnsureVersionsTab();
        }


        /// <summary>
        /// Adds the versions tab if required by the edited object.
        /// </summary>
        private void EnsureVersionsTab()
        {
            GeneralizedInfo obj = EditedObject as BaseInfo;

            if ((obj != null) && ObjectVersionManager.DisplayVersionsTab(obj))
            {
                var url = @"~\CMSModules\AdminControls\Pages\ObjectVersions.aspx";
                url = URLHelper.AddParameterToUrl(url, "objecttype", obj.TypeInfo.ObjectType);
                url = URLHelper.AddParameterToUrl(url, "objectid", obj.ObjectID.ToString());
                url = URLHelper.AddParameterToUrl(url, "editonlycode", QueryHelper.GetBoolean("editonlycode", false).ToString());
                url = URLHelper.AddParameterToUrl(url, "noreload", QueryHelper.GetBoolean("noreload", false).ToString());
                url = URLHelper.AddParameterToUrl(url, "showfooter", QueryHelper.GetBoolean("showfooter", true).ToString());
                url = URLHelper.AddParameterToUrl(url, "hash", QueryHelper.GetHash(url));

                SetTab(Int32.MaxValue, GetString("objectversioning.tabtitle"), ResolveUrl(url), null);
            }
        }


        /// <summary>
        /// Sets the particular tab.
        /// </summary>
        /// <param name="index">Index of the tab</param>
        /// <param name="text">Text of the tab</param>
        /// <param name="targetUrl">Target URL</param>
        /// <param name="javascript">JavaScript</param>
        public void SetTab(int index, string text, string targetUrl, string javascript)
        {
            var tab = new UITabItem
            {
                Index = index,
                Text = text,
                OnClientClick = javascript,
                RedirectUrl = targetUrl
            };

            SetTab(tab);
        }


        /// <summary>
        /// Sets the tab.
        /// </summary>
        public void SetTab(UITabItem tab)
        {
            var master = CurrentMaster;
            if (master != null)
            {
                var tabs = master.Tabs;
                if (tabs != null)
                {
                    tabs.AddTab(tab);
                }
            }
        }


        /// <summary>
        /// Sets the particular Breadcrumb data
        /// </summary>
        /// <param name="index">Index of the Breadcrumb</param>
        /// <param name="text">Text of the tab</param>
        /// <param name="targetUrl">Target URL</param>
        /// <param name="targetFrame">Target frame</param>
        /// <param name="javascript">OnClientClick JavaScript</param>
        public void SetBreadcrumb(int index, string text, string targetUrl, string targetFrame, string javascript)
        {
            var master = CurrentMaster;
            if (master != null)
            {
                PageTitle title = master.Title;
                if (title != null)
                {
                    BreadcrumbItem breadcrumb = new BreadcrumbItem
                    {
                        Index = index,
                        Text = text,
                        RedirectUrl = targetUrl,
                        Target = targetFrame,
                        OnClientClick = javascript
                    };
                    title.Breadcrumbs.AddBreadcrumb(breadcrumb);
                }
            }
        }


        /// <summary>
        /// Sets the particular action data.
        /// </summary>
        /// <param name="index">Index of the action</param>
        /// <param name="text">Text of the actions</param>
        /// <param name="targetUrl">Target URL</param>
        /// <param name="javascript">Java-script to execute on click</param>
        /// <param name="tooltip">Tooltip of the action</param>
        public void SetAction(int index, string text, string targetUrl, string javascript = null, string tooltip = null)
        {
            var action = new HeaderAction
            {
                Index = index,
                Text = text,
                Tooltip = tooltip,
                RedirectUrl = targetUrl,
                OnClientClick = javascript
            };

            AddHeaderAction(action);
        }


        /// <summary>
        /// Set edited object and ensure redirection to frame set if necessary
        /// </summary>
        /// <param name="editedObj">Edited object</param>
        /// <param name="frameSetUrl">URL of the frameset</param>
        public void SetEditedObject(object editedObj, string frameSetUrl)
        {
            EditedObject = editedObj;

            if ((EditedObject != null) && !String.IsNullOrEmpty(frameSetUrl) && !TabMode)
            {
                BaseInfo obj = EditedObject as BaseInfo;
                // Check if redirect required
                bool redirect = ObjectVersionManager.DisplayVersionsTab(obj);

                // Redirect if required
                if (redirect)
                {
                    string url = frameSetUrl + RequestContext.CurrentQueryString;
                    url = URLHelper.AddParameterToUrl(url, "tabmode", "1");
                    URLHelper.Redirect(url);
                }
            }
        }


        /// <summary>
        /// Checks the permissions of all UI elements hierarchically starting with specified UI element (with optional custom root element).
        /// </summary>
        /// <param name="element">Starting UI element.</param>
        /// <param name="contextResolver">The context resolver for macro in ElementAccessCondition.</param>
        /// <param name="rootElementId">The root element ID. If set the access is not checked over this element.</param>
        /// <returns>
        /// Returns <see cref="ElementAccessCheckResult"/> type with access check result.
        /// If <paramref name="element"/> is null, returned <see cref="ElementAccessCheckResult"/> has status
        /// <see cref="ElementAccessCheckStatus.NoRestrictions"/> and its <see cref="ElementAccessCheckResult.UIElementInfo"/> is null.
        /// </returns>
        protected internal static ElementAccessCheckResult CheckUIElementAccessHierarchicalInternal(UIElementInfo element, MacroResolver contextResolver = null, int rootElementId = 0)
        { 
            if (element != null)
            {
                var cui = MembershipContext.AuthenticatedUser;
                if (!cui.CheckPrivilegeLevel(UserPrivilegeLevelEnum.GlobalAdmin))
                {
                    // Check global application scope for current element
                    if (ApplicationUIHelper.IsAccessibleOnlyByGlobalAdministrator(element, rootElementId))
                    {
                        return new ElementAccessCheckResult(ElementAccessCheckStatus.GlobalApplicationAccessFailed, element);
                    }

                    var readPermissionsCheckedForModules = new HashSet<int>();
                    do
                    {
                        bool readCheckedForCurrentModule = readPermissionsCheckedForModules.Contains(element.ElementResourceID);

                        ElementAccessCheckResult result = ApplicationUIHelper.CheckElementAccess(element, contextResolver, !readCheckedForCurrentModule);
                        if (result.ElementAccessCheckStatus != ElementAccessCheckStatus.NoRestrictions)
                        {
                            return result;
                        }

                        readPermissionsCheckedForModules.Add(element.ElementResourceID);

                        // Do not check elements over custom root element
                        if (element.ElementID == rootElementId)
                        {
                            break;
                        }

                        element = UIElementInfoProvider.GetUIElementInfo(element.ElementParentID);
                    } while (element != null);
                }
            }

            return new ElementAccessCheckResult(ElementAccessCheckStatus.NoRestrictions, element);
        }


        /// <summary>
        /// Checks the permissions of all UI elements hierarchically starting with specified UI element.
        /// Use optional parameter (contextResolver) and macro in ElementAccessCondition will be also checked.
        /// Use two optional parameters (isDialog, rootElementId) if your ui element is dialog.
        /// In case of insufficient permissions appropriate redirection is made.
        /// </summary>
        /// <param name="element">UI element info</param>
        /// <param name="contextResolver">The context resolver for macro in ElementAccessCondition.</param>
        /// <param name="rootElementId">The root element ID.</param>
        /// <param name="redirectToAccessDenied">Indicates whether not fulfillment permissions should cause redirect to access denied</param>
        /// <returns>Returns false if UI current user is not authorized per UI element (requires disabled redirectToAccessDenied parameter)</returns>
        public static bool CheckUIElementAccessHierarchical(UIElementInfo element, MacroResolver contextResolver = null, int rootElementId = 0, bool redirectToAccessDenied = true)
        {
            if (element != null)
            {
                // Get result
                ElementAccessCheckResult result = CheckUIElementAccessHierarchicalInternal(element, contextResolver, rootElementId);

                // If access failed
                if (result.ElementAccessCheckStatus != ElementAccessCheckStatus.NoRestrictions)
                {
                    // return false if redirect is not required
                    if (!redirectToAccessDenied)
                    {
                        return false;
                    }

                    // or use specific access denied page
                    var restrictedElement = result.UIElementInfo;
                    string resourceName = ApplicationUrlHelper.GetResourceName(restrictedElement.ElementResourceID);
                        
                    switch (result.ElementAccessCheckStatus)
                    {
                        // Element condition
                        case ElementAccessCheckStatus.AccessConditionFailed:
                            RedirectToAccessDenied(String.Format(ResHelper.GetString("elementaccessdenied.info"), resourceName, restrictedElement.ElementDisplayName));
                            break;

                        // Automatic read permission
                        case ElementAccessCheckStatus.ReadPermissionFailed:
                            RedirectToAccessDenied(resourceName, "Read");
                            break;

                        // UI element access
                        case ElementAccessCheckStatus.UIElementAccessFailed:
                            RedirectToUIElementAccessDenied(resourceName, restrictedElement.ElementName);
                            break;

                        // Global application access failed
                        case ElementAccessCheckStatus.GlobalApplicationAccessFailed:
                            RedirectToAccessDenied(ResHelper.GetString("accessdeniedtopage.globaladminrequired"));
                            break;

                        // By default the validation failed
                        default:
                            return false;
                    }
                }
            }
            return true;
        }
        

        /// <summary>
        /// Checks the permissions of all UI elements hierarchically starting with specified UI element.
        /// Use optional parameter "contextResolver" and macro in ElementAccessCondition will be also checked.
        /// Use optional parameter "rootElementId" if your UI element is in a dialog. RootElementId variable identifies the UI element which is used in the top frame of the dialog.
        /// In case of insufficient permissions appropriate redirection is made.
        /// </summary>
        /// <param name="resourceName">Resource name.</param>
        /// <param name="elementName">Starting UI element name.</param>
        /// <param name="contextResolver">The context resolver for macro in ElementAccessCondition.</param>
        /// <param name="rootElementId">The root element ID.</param>
        /// <param name="redirectToAccessDenied">Indicates whether not fulfillment permissions should cause redirect to access denied</param>
        /// <returns>Returns false if UI current user is not authorized per UI element (requires disabled redirectToAccessDenied parameter)</returns>
        public static bool CheckUIElementAccessHierarchical(string resourceName, string elementName, MacroResolver contextResolver = null, int rootElementId = 0, bool redirectToAccessDenied = true)
        {
            var element = UIElementInfoProvider.GetUIElementInfo(resourceName, elementName);
            return CheckUIElementAccessHierarchical(element, contextResolver, rootElementId, redirectToAccessDenied);
        }

        #endregion


        #region "Other methods"

        /// <summary>
        /// Raises OnTabCreated event.
        /// </summary>
        /// <param name="element">UI element</param>
        /// <param name="tab">Tab object</param>
        /// <param name="tabIndex">Index of a tab</param>
        /// <returns>Potentially modified tab object</returns>
        public UITabItem RaiseTabCreated(UIElementInfo element, UITabItem tab, int tabIndex)
        {
            if (OnTabCreated != null)
            {
                var e = new TabCreatedEventArgs
                {
                    UIElement = element,
                    Tab = tab,
                    TabIndex = tabIndex
                };

                OnTabCreated(this, e);

                return e.Tab;
            }

            return tab;
        }


        /// <summary>
        /// Raises OnCheckTabSecurity event.
        /// </summary>
        /// <param name="resource">Resource for which the permission is missing</param>
        /// <param name="permission">Permission that is missing</param>
        /// <returns>If true, some permission is missing</returns>
        public bool RaiseCheckTabSecurity(ref string resource, ref string permission)
        {
            return (OnCheckTabSecurity != null) && OnCheckTabSecurity(ref resource, ref permission);
        }


        /// <summary>
        /// Sets the RTL culture to the body class if RTL language
        /// </summary>
        public void SetRTL()
        {
            if (!CultureHelper.IsUICultureRTL())
            {
                return;
            }

            mBodyClass += " RTL";
            mBodyClass = mBodyClass.Trim();
        }


        /// <summary>
        /// Sets the RTL culture for live site to the body class if RTL language
        /// </summary>
        public void SetLiveRTL()
        {
            if (!CultureHelper.IsPreferredCultureRTL())
            {
                return;
            }

            headerTags.AppendLine("<style type=\"text/css\">* { direction: rtl; }</style>");
            mBodyClass += " RTL";
            mBodyClass = mBodyClass.Trim();
        }


        /// <summary>
        /// Sets the browser class to the body class
        /// </summary>
        /// <param name="generateCultureClass">Generate culture class</param>
        public void SetBrowserClass(bool generateCultureClass = true)
        {
            mBodyClass = CMSMasterPage.EnsureBodyClass(mBodyClass, generateCultureClass);
        }


        /// <summary>
        /// Sets current UI culture
        /// </summary>
        public void SetCulture()
        {
            // Set the culture
            CultureInfo ci = CultureHelper.PreferredUICultureInfo;

            var thr = Thread.CurrentThread;

            thr.CurrentCulture = ci;
            thr.CurrentUICulture = ci;

            // Check parameter UILang
            string culture = QueryHelper.GetString("UILang", String.Empty);
            if (!String.IsNullOrEmpty(culture))
            {
                UICulture = culture;

                var cultureInfo = CultureHelper.GetCultureInfo(culture);
                thr.CurrentCulture = cultureInfo;
                thr.CurrentUICulture = cultureInfo;
            }
        }


        /// <summary>
        /// Sets current UI culture for live site
        /// </summary>
        public void SetLiveCulture()
        {
            var preferredCultureCode = LocalizationContext.PreferredCultureCode;
            // Set the culture
            if (!String.IsNullOrEmpty(preferredCultureCode))
            {
                CultureInfo ci = CultureHelper.GetCultureInfo(preferredCultureCode);

                var thr = Thread.CurrentThread;

                thr.CurrentCulture = ci;
                thr.CurrentUICulture = ci;
            }
        }


        /// <summary>
        /// Registers the script to handle the close window operation on ESC key
        /// </summary>
        public void RegisterEscScript()
        {
            if (Form != null)
            {
                // Create the script handler
                const string script = @"
if (wopener)
{
    document.onkeydown = function(e) {
        if(window.event) e = window.event;
        var mKey  = (window.event) ? event.keyCode : e.keyCode;
        var escKey = (e.DOM_VK_ESCAPE) ? e.DOM_VK_ESCAPE : 27;
        if (mKey == escKey) { 
            CloseDialog(); 
        }
    }
}
";

                ScriptHelper.RegisterStartupScript(Page, typeof(string), "EscScript", ScriptHelper.GetScript(script));
            }
        }


        /// <summary>
        /// Registers js script for synchronization scroll bars in split mode.
        /// </summary>
        /// <param name="basePage">Indicates if page is base.</param>
        /// <param name="body">Indicates if event 'scroll' should be bound on document body.</param>
        protected void RegisterSplitModeSync(bool basePage, bool body)
        {
            bool refresh = RequestHelper.IsPostBack() 
                && (SplitModeAllwaysRefresh || String.Equals(CultureHelper.GetOriginalPreferredCulture(), PortalUIHelper.SplitModeCultureCode, StringComparison.OrdinalIgnoreCase));
            RegisterSplitModeSync(basePage, body, refresh);
        }


        /// <summary>
        /// Registers script for centralized dialog closing. (Contains functionality for refreshing opener window.)
        /// </summary>
        protected void RegisterDialogHandlingScripts()
        {
            ScriptHelper.RegisterCloseDialogScript(this);
            ScriptHelper.RegisterGetTopScript(this);
        }


        /// <summary>
        /// Registers java script for synchronization scroll bars in split mode.
        /// </summary>
        /// <param name="basePage">Indicates if page is base.</param>
        /// <param name="body">Indicates if event 'scroll' should be bound on document body.</param>
        /// <param name="refresh">Indicates if the second frame should be refreshed.</param>
        protected void RegisterSplitModeSync(bool basePage, bool body, bool refresh)
        {
            ScriptHelper.RegisterSplitModeSync(Page, basePage, body, refresh);
        }


        /// <summary>
        /// Determines whether current user is authorized to access cms.content
        /// </summary>
        /// <param name="siteName">Site name to check</param>
        public static bool IsUserAuthorizedPerContent(String siteName)
        {
            // Check site availability
            if (!ResourceSiteInfoProvider.IsResourceOnSite("CMS.Content", siteName))
            {
                return false;
            }

            // Check permission for CMS -> Content tab
            CurrentUserInfo user = MembershipContext.AuthenticatedUser;
            if (!user.IsAuthorizedPerUIElement("CMS.Content", "Content", false, siteName))
            {
                return false;
            }

            return DocumentSecurityHelper.IsUserAuthorizedPerContent(siteName, user);
        }


        /// <summary>
        /// Determines whether current user is authorized to access cms.content
        /// </summary>
        public static bool IsUserAuthorizedPerContent()
        {
            return IsUserAuthorizedPerContent(SiteContext.CurrentSiteName);
        }


        /// <summary>
        /// Registers the model dialog script
        /// </summary>
        public void RegisterModalDialogScript()
        {
            ScriptHelper.RegisterDialogScript(Page);
        }


        /// <summary>
        /// Register wopener script and base target tag and resizable frameset for this page
        /// </summary>
        protected void RegisterModalPageScripts()
        {
            if (modalPageScriptsRegistered)
            {
                // Scripts already registered
                return;
            }

            if (RegisterWOpener)
            {
                ScriptHelper.RegisterWOpenerScript(this);
            }

            if (AddBaseTag)
            {
                headerTags.AppendLine("<base target=\"_self\" />");
            }

            if (Form == null)
            {
                RegisterResizableFrameset();
            }

            modalPageScriptsRegistered = true;
        }


        /// <summary>
        /// Registers the tooltip script
        /// </summary>
        public void RegisterTooltipScript()
        {
            ScriptHelper.RegisterTooltip(this);
        }


        /// <summary>
        /// Registers the object export function
        /// </summary>
        public void RegisterExportScript()
        {
            RegisterModalDialogScript();

            string script = String.Format(@"
function OpenExportObject(type, id, siteid) {{
    var siteQuery = 'objectType=' + type + '&objectId=' + id;
    if(siteid) {{
        siteQuery += '&siteId=' + siteid;
    }}

    modalDialog('{0}?' + siteQuery, 'ExportObject', {1}, {2});
}}",
                ResolveUrl("~/CMSModules/ImportExport/Pages/ExportObject.aspx"),
                EXPORT_OBJECT_WIDTH,
                EXPORT_OBJECT_HEIGHT);

            ScriptHelper.RegisterClientScriptBlock(this, typeof(string), "ExportScript", ScriptHelper.GetScript(script));
        }


        /// <summary>
        /// Adds the no-cache tag to the page header
        /// </summary>
        public void AddNoCacheTag()
        {
            headerTags.AppendLine("<meta http-equiv=\"pragma\" content=\"no-cache\" />");
        }


        /// <summary>
        /// Renders header tags previously appended to the <see cref="headerTags"/> builder into the page.
        /// </summary>
        private void RednerHeaderTags()
        {
            if (headerTags.Length <= 0)
            {
                // No tags to render
                return;
            }

            Header.Controls.Add(new LiteralControl(headerTags.ToString()));
        }


        /// <summary>
        /// Registers a CSS for the frame and a script for its proper resizing.
        /// </summary>
        private void RegisterResizableFrameset()
        {
            headerTags.AppendLine("<style type=\"text/css\">");
            headerTags.AppendLine("\thtml { height: 100%; overflow: hidden; }");

            // RTL IE support
            if (CultureHelper.IsUICultureRTL() && BrowserHelper.IsIE())
            {
                headerTags.AppendLine("\t#rowsFrameset { position: absolute; left: 0; top: 0 }");
            }

            headerTags.AppendLine("</style>");

            // Ensure proper frame resizing
            const string script = @"window.onresize = function() { document.getElementById('rowsFrameset').style.width = document.documentElement.clientWidth + 'px'; };";
            headerTags.AppendLine(ScriptHelper.GetScript(script));
        }


        /// <summary>
        /// Adds additional tags to the page header to make page WAI valid (comply with WAI standard)
        /// </summary>
        private void AddWAIRequiredTags()
        {
            headerTags.AppendLine("<meta http-equiv=\"content-style-type\" content=\"text/css\" />");
            headerTags.AppendLine("<meta http-equiv=\"content-script-type\" content=\"text/javascript\" />");
        }


        /// <summary>
        /// Adds the meta tag with charset to the page header
        /// </summary>
        private void AddCharsetMetaTag()
        {
            headerTags.AppendLine("<meta charset=\"utf-8\" />");
        }


        /// <summary>
        /// Adds link tags for icon and shortcut icon to the page header
        /// </summary>
        private void AddFavIconTags()
        {
            // Get icon information to render
            const string FAVICON_URL = @"~/App_Themes/Default/Images/favicon.ico";
            string extension = URLHelper.GetUrlExtension(FAVICON_URL);
            string type = MimeTypeHelper.GetMimetype(extension, null);

            // Prepare tags
            string shortcutIconLink = HTMLHelper.GetLink(FAVICON_URL, type, rel: "shortcut icon", media: null, title: null);
            string iconLink = HTMLHelper.GetLink(FAVICON_URL, type, rel: "icon", media: null, title: null);

            // Render tags
            headerTags.AppendLine(shortcutIconLink);
            headerTags.AppendLine(iconLink);
        }


        /// <summary>
        /// Registers CSS file for dialogs
        /// </summary>
        public void RegisterDialogCSSLink()
        {
            var name = DocumentContext.CurrentDocumentStylesheetName;
            if (String.IsNullOrEmpty(name))
            {
                name = "Default";
            }

            // Register custom CSS
            CssRegistration.RegisterCssLink(Page, name, "Skin.css");
        }


        /// <summary>
        /// Sets the live site dialog body class.
        /// </summary>
        public void SetLiveDialogClass()
        {
            if (!mBodyClass.Contains("LiveSiteDialog"))
            {
                mBodyClass += " LiveSiteDialog";
                mBodyClass = mBodyClass.Trim();
            }
        }


        /// <summary>
        /// Redirects to secured (SSL) page if its set in settings (CMSUseSSLForAdministrationInterface key)
        /// </summary>
        public static void RedirectToSecured()
        {
            // Redirect to https version if needed
            if (SettingsKeyInfoProvider.GetBoolValue("CMSUseSSLForAdministrationInterface"))
            {
                if (!RequestContext.IsSSL)
                {
                    string url = "https" + RequestContext.URL.AbsoluteUri.Substring(RequestContext.CurrentScheme.Length);
                    URLHelper.Redirect(url);
                }
            }
        }


        /// <summary>
        /// Checks the preferred culture and changes it to default if not valid. Returns true if the culture was valid.
        /// </summary>
        public static bool CheckPreferredCulture()
        {
            return CheckPreferredCulture(LocalizationContext.PreferredCultureCode);
        }


        /// <summary>
        /// Checks the preferred culture and changes it to default if not valid. Returns true if the culture was valid.
        /// </summary>
        /// <param name="requiredCulture">Preferred culture code</param>
        /// <returns>Returns true if required culture is valid, false if not</returns>
        public static bool CheckPreferredCulture(string requiredCulture)
        {
            string siteName = SiteContext.CurrentSiteName;
            string defaultCulture = GetDefaultCulture(siteName);

            // Check (and ensure) the proper content culture
            if (!CultureSiteInfoProvider.IsCultureOnSite(requiredCulture, siteName) || (!CultureSiteInfoProvider.LicenseVersionCheck() && (requiredCulture != defaultCulture)))
            {
                LocalizationContext.PreferredCultureCode = defaultCulture;

                return false;
            }

            return true;
        }


        /// <summary>
        /// Returns default culture
        /// </summary>
        /// <param name="siteName">Site name</param>
        private static string GetDefaultCulture(string siteName)
        {
            string culture = "en-us";
            string defaultCulture = CultureHelper.GetDefaultCultureCode(siteName);

            if (CultureSiteInfoProvider.IsCultureOnSite(defaultCulture, siteName))
            {
                culture = defaultCulture;
            }
            else
            {
                DataSet ds = CultureSiteInfoProvider.GetSiteCultures(siteName);
                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    culture = ValidationHelper.GetString(ds.Tables[0].Rows[0]["CultureCode"], "en-us");
                }
            }

            return culture;
        }


        /// <summary>
        /// Refreshes the parent window
        /// </summary>
        public static void RefreshParentWindow()
        {
            ScriptHelper.RegisterClientScriptBlock(PageContext.CurrentPage, typeof(string), "RefreshParent", "parent.window.location.replace('" + URLHelper.RemoveParameterFromUrl(RequestContext.URL.OriginalString, "culture") + "');", true);
        }


        /// <summary>
        /// Adds the script to the page
        /// </summary>
        /// <param name="script">Script to add</param>
        public virtual void AddScript(string script)
        {
            throw new Exception("[CMSPage.AddScript]: This method must be overridden by the inheriting class.");
        }


        /// <summary>
        /// Ensures the script manager on the page.
        /// </summary>
        public override ScriptManager EnsureScriptManager()
        {
            if (ManagersContainer == null)
            {
                // Try to find the placeholder for the manager
                // Search only within <form> element (as the Script Manager cannot exist anywhere else)
                Control formElem = Form ?? ControlsHelper.GetChildControl(this, typeof(HtmlForm));
                ManagersContainer = ControlsHelper.GetChildControl(formElem, typeof(PlaceHolder));
            }

            return base.EnsureScriptManager();
        }


        /// <summary>
        /// Disables all debugging for current page
        /// </summary>
        public void DisableDebugging()
        {
            // Disable the debugging
            DebugHelper.DisableDebug();

            OutputFilterContext.LogCurrentOutputToFile = false;

            logViewState = false;

            // Disable CSS minification
            CssLinkHelper.MinifyCurrentRequest = false;
        }


        /// <summary>
        /// Handles actions needed by ScreenLock feature
        /// </summary>
        private void HandleScreenLock()
        {
            if (DatabaseHelper.IsDatabaseAvailable && DatabaseHelper.IsCorrectDatabaseVersion)
            {
                string[] callbackId = Request.Params.GetValues("__CALLBACKID");
                bool isScreenLockCallback = (callbackId != null) && callbackId[0].Contains("screenLock");

                // ScreenLock - redirect to logon page if screen is locked
                if (!isScreenLockCallback && IsScreenLocked)
                {
                    // Sign out current user
                    AuthenticationHelper.SignOut();
                    IsScreenLocked = false;
                    URLHelper.RefreshCurrentPage();
                }

                // ScreenLock - log last request
                if (!isScreenLockCallback)
                {
                    SecurityHelper.LogScreenLockAction();

                    if (SecurityHelper.IsScreenLockEnabled(SiteContext.CurrentSiteName))
                    {
                        // Make sure that ScreenLock warning is hidden after request
                        string script = @"
                        try
                        {
                            if (window.top && window.top.HideScreenLockWarningAndSync) {
                                window.top.HideScreenLockWarningAndSync(" + SecurityHelper.GetSecondsToShowScreenLockAction(SiteContext.CurrentSiteName) + @");
                            }
                        }
                        catch(error)
                        {
                            // Do nothing - this error is probably caused by cross-domain access
                        }";

                        ScriptHelper.RegisterClientScriptBlock(this, typeof(string), "ScreenLock_Hide", ScriptHelper.GetScript(script));
                    }
                }
            }
        }

        #endregion
    }
}