using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Collections;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.EventLog;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Localization;
using CMS.Membership;
using CMS.SiteProvider;

using TreeNode = CMS.DocumentEngine.TreeNode;

namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// Portal page manager.
    /// </summary>
    [ToolboxData("<{0}:CMSPortalManager runat=server></{0}:CMSPortalManager>")]
    public class CMSPortalManager : CMSWebControl, INamingContainer, IPageManager
    {
        #region "Constants"

        /// <summary>
        /// Paths to controls in portal engine module.
        /// </summary>
        private const string PORTALENGINE_WIDGETSPATH = "~/CMSModules/Widgets/Controls/";
        private const string PORTALENGINE_WEBPARTSPATH = "~/CMSModules/PortalEngine/Controls/Webparts/";
        private const string PORTALENGINE_LAYOUTPATH = "~/CMSModules/PortalEngine/Controls/Layout/";

        #endregion


        #region "Variables"

        /// <summary>
        /// Collection of the page editable regions.
        /// </summary>
        protected List<ICMSEditableControl> mCMSEditableControls;

        /// <summary>
        /// Collection of all the base level page placeholders.
        /// </summary>
        protected List<CMSPagePlaceholder> mCMSPagePlaceholders;

        /// <summary>
        /// Collection of all the page placeholders.
        /// </summary>
        protected List<CMSPagePlaceholder> mAllPagePlaceholders;

        private List<WebPartZoneInstance> mNotLoadedZones;
        private CMSPagePlaceholder mCurrentPlaceholder;
        private string mSiteName;
        private string mPageAliasPath;
        private ViewModeEnum mViewMode = ViewModeEnum.Unknown;
        private WebPartManagementEnum mRequiresWebPartManagement = WebPartManagementEnum.Unknown;
        private string mUserCulture;
        private CurrentUserInfo mCurrentUser;
        private bool allowRegisterPlaceholder = true;
        private bool? mIsAuthorized;
        private int mCacheMinutes = -1;
        private PageInfo mCurrentPageInfo;
        private bool? mPageChanged;

        #endregion


        #region "Control variables"

        /// <summary>
        /// Page placeholder menu inner control.
        /// </summary>
        protected CMSAbstractPortalUserControl mPlaceholderMenuControl;

        internal HeaderPanel pnlHeader;
        private HierarchyPageInfo mHierarchyPageInfo;
        private WebPartActionManager actionManager;
        private MessagesPlaceHolder mMessagesPlaceholder;
        private ICMSDocumentManager mDocumentManager;
        private IDocumentWizardManager mDocumentWizardManager;
        private NamingContainer contextMenuContainer;

        #endregion


        #region "Internal properties"

        /// <summary>
        /// Gets or sets the document wizard manager
        /// </summary>
        internal IDocumentWizardManager DocumentWizardManager
        {
            get
            {
                return mDocumentWizardManager;
            }
            set
            {
                // Log warning about duplicate wizard managers
                if ((mDocumentWizardManager != null) && (value != null))
                {
                    EventLogProvider.LogEvent(EventType.WARNING, "Portal manager", "SETWIZARDMANAGER", "Page wizard manager was set more than once.");
                }

                mDocumentWizardManager = value;
            }
        }


        /// <summary>
        /// Gets the value that indicates whether view mode should be reloaded for child controls
        /// </summary>
        internal bool ReloadViewMode
        {
            get;
            private set;
        }


        /// <summary>
        /// Current user
        /// </summary>
        internal CurrentUserInfo CurrentUser
        {
            get
            {
                return mCurrentUser ?? (mCurrentUser = MembershipContext.AuthenticatedUser);
            }
        }


        /// <summary>
        /// Gets the preferred UI culture (HTML encoded due to XSS) for current user
        /// </summary>
        internal string UserCulture
        {
            get
            {
                return mUserCulture ?? (mUserCulture = CurrentUser.PreferredUICultureCode);
            }
        }


        /// <summary>
        /// Gets or sets the custom message which should be displayed in design mode
        /// </summary>
        internal string AdditionalDesignMessage
        {
            get;
            set;
        }


        /// <summary>
        /// Messages placeholder
        /// </summary>
        internal MessagesPlaceHolder CurrentMessagesPlaceholder
        {
            get
            {
                return mMessagesPlaceholder;
            }
        }


        /// <summary>
        /// Returns true if the web part management support is required.
        /// </summary>
        internal WebPartManagementEnum RequiresWebPartManagement
        {
            get
            {
                if (mRequiresWebPartManagement == WebPartManagementEnum.Unknown)
                {
                    var vm = ViewMode;

                    if (PortalContext.IsDesignMode(vm, false))
                    {
                        mRequiresWebPartManagement = WebPartManagementEnum.All;
                    }
                    else
                    {
                        // Get the information from subcomponents
                        mRequiresWebPartManagement = WebPartManagementEnum.None;

                        // Search all the placeholders
                        if (CMSPagePlaceholders != null)
                        {
                            foreach (CMSPagePlaceholder placeholder in CMSPagePlaceholders)
                            {
                                if (placeholder.RequiresWebPartManagement())
                                {
                                    mRequiresWebPartManagement = WebPartManagementEnum.Widgets;
                                    break;
                                }
                            }
                        }
                    }
                }

                return mRequiresWebPartManagement;
            }
            set
            {
                mRequiresWebPartManagement = value;
            }
        }


        /// <summary>
        /// Indicates whether current request is requested by action manager in async mode
        /// </summary>
        internal bool IsRequestAsync
        {
            get
            {
                String id = Convert.ToString(HttpContext.Current.Request.Form["__CALLBACKID"]);

                // Return true for action manager action (webpart move,..) or context menu action
                return (((actionManager != null) && id.EqualsCSafe(actionManager.UniqueID))
                    || ((contextMenuContainer != null) && id.StartsWithCSafe(contextMenuContainer.UniqueID + "$", true)));
            }
        }


        /// <summary>
        /// Returns true if the page structure changed.
        /// </summary>
        internal bool PageChanged
        {
            get
            {
                if (mPageChanged == null)
                {
                    // Check for the Design Mode in Visual Studio
                    if (Context == null)
                    {
                        mPageChanged = false;
                    }

                    // Check whether callback is raised by web part action manager
                    mPageChanged = (RequestHelper.IsCallback()
                        && (IsRequestAsync))
                        // or page changed flag is set
                        || ValidationHelper.GetBoolean(HttpContext.Current.Request.Params["pageChanged"], false)
                        // or action manager postback is called
                        || ((actionManager != null) && ValidationHelper.GetString(HttpContext.Current.Request.Form[Page.postEventSourceID], String.Empty).EqualsCSafe(actionManager.UniqueID));
                }
                return mPageChanged.Value;
            }
        }

        #endregion


        #region "Permission properties (ASPX mode)"

        /// <summary>
        /// Allows you to specify whether to check permissions of the current user. If the value is 'false' (default value) no permissions are checked. Otherwise, only nodes for which the user has read permission are displayed.
        /// </summary>
        public bool CheckPermissions
        {
            get;
            set;
        }


        /// <summary>
        /// Number of minutes the retrieved content is cached for. Zero indicates that the content will not be cached.
        /// </summary>
        /// <remarks>
        /// This parameter allows you to set up caching of content so that it's not retrieved from the database each time a user requests the page.
        /// </remarks>
        public virtual int CacheMinutes
        {
            get
            {
                if (mCacheMinutes < 0)
                {
                    if ((HttpContext.Current == null) || (PortalContext.ViewMode != ViewModeEnum.LiveSite))
                    {
                        mCacheMinutes = 0;
                    }
                    else
                    {
                        mCacheMinutes = PageInfoCacheHelper.CacheMinutes(SiteContext.CurrentSiteName);
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
        /// Returns true if the user is authorized for current document.
        /// </summary>
        public bool IsAuthorized
        {
            get
            {
                if (mIsAuthorized == null)
                {
                    bool result = true;

                    // Check permissions
                    bool liveSite = PortalContext.ViewMode.IsLiveSite();

                    if ((CurrentPageInfo != null) && (CheckPermissions || !liveSite))
                    {
                        // Try to get value from cache
                        UserInfo currentUser = MembershipContext.AuthenticatedUser;

                        int cacheMinutes = CacheMinutes;
                        if (!liveSite)
                        {
                            cacheMinutes = 0;
                        }

                        // Try to get data from cache
                        using (var cs = new CachedSection<bool>(ref result, cacheMinutes, true, null, "pagemanagerauthorized", CacheHelper.GetBaseCacheKey(true, false), SiteContext.CurrentSiteName, CurrentPageInfo.NodeAliasPath))
                        {
                            if (cs.LoadData)
                            {
                                // Check if authorized
                                result = MembershipContext.AuthenticatedUser.IsAuthorizedPerDocument(CurrentPageInfo.NodeID, CurrentPageInfo.ClassName, NodePermissionsEnum.Read) != AuthorizationResultEnum.Denied;

                                // Save to the cache
                                if (cs.Cached)
                                {
                                    // Prepare the dependencies (depending on document data / user: roles)
                                    string[] dependencies =
                                    {
                                        "node|" + SiteName.ToLowerCSafe() + "|" + CurrentPageInfo.NodeAliasPath.ToLowerCSafe(),
                                        "user|byid|" + currentUser.UserID
                                    };

                                    cs.CacheDependency = CacheHelper.GetCacheDependency(dependencies);
                                }

                                cs.Data = result;
                            }
                        }
                    }

                    mIsAuthorized = result;
                }

                return mIsAuthorized.Value;
            }
        }

        #endregion


        #region "Public properties"

        /// <summary>
        /// Control's UI Context
        /// </summary>
        public UIContext UIContext
        {
            get
            {
                return UIContextHelper.GetUIContext(this);
            }
        }


        /// <summary>
        /// Hierarchy page info
        /// </summary>
        public HierarchyPageInfo HierarchyPageInfo
        {
            get
            {
                // Do not use TreeProvider from DocumentManager, we do not want to ensure manager on a page when only viewing data
                return mHierarchyPageInfo ?? (mHierarchyPageInfo = new HierarchyPageInfo(CurrentPageInfo, ViewMode));
            }
        }


        /// <summary>
        /// Gets the current edit menu if available
        /// </summary>
        public EditMenu CurrentEditMenu
        {
            get
            {
                return pnlHeader.mEditMenu;
            }
        }


        /// <summary>
        /// Gets the current object edit menu if available
        /// </summary>
        public ObjectEditMenu CurrentObjectEditMenu
        {
            get
            {
                return pnlHeader.mObjectEditMenu;
            }
        }


        /// <summary>
        /// Allows you to specify whether the content of non-existing or not visible regions should be preserved when the content is saved.
        /// </summary>
        [Category("Behavior"), DefaultValue(false), Description("Allows you to specify whether the content of non-existing or not visible regions should be preserved when the content is saved.")]
        public virtual bool PreserveContent
        {
            get;
            set;
        }


        /// <summary>
        /// Tree provider instance used to access data. If no TreeProvider is assigned, a new TreeProvider instance is created.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual TreeProvider TreeProvider
        {
            get
            {
                return new TreeProvider(CurrentUser);
            }
        }


        /// <summary>
        /// Document manager
        /// </summary>
        public ICMSDocumentManager DocumentManager
        {
            get
            {
                if (mDocumentManager == null)
                {
                    ICMSPage page = Page as ICMSPage;
                    if (page != null)
                    {
                        mDocumentManager = page.DocumentManager;

                        // Do not require document manager functionality in the UI view mode
                        if ((mDocumentManager != null) && (ViewMode == ViewModeEnum.UI))
                        {
                            mDocumentManager.StopProcessing = true;
                        }
                    }
                    else
                    {
                        throw new NullReferenceException("[CMSPortalManager.DocumentManager]: Current page must implement ICMSPage interface.");
                    }
                }

                return mDocumentManager;
            }
        }


        /// <summary>
        /// Current site name.
        /// </summary>
        public string SiteName
        {
            get
            {
                // Check for the Design Mode in Visual Studio
                if (Context == null)
                {
                    return mSiteName;
                }

                return mSiteName ?? (mSiteName = SiteContext.CurrentSiteName);
            }
            set
            {
                mSiteName = value;
            }
        }


        /// <summary>
        /// Current site alias path.
        /// </summary>
        public string PageAliasPath
        {
            get
            {
                if ((mPageAliasPath == null) && (HttpContext.Current != null))
                {
                    mPageAliasPath = DataHelper.GetNotEmpty(DocumentContext.CurrentAliasPath, "/");
                }

                return mPageAliasPath;
            }
            set
            {
                mPageAliasPath = value;
            }
        }


        /// <summary>
        /// Current page mode.
        /// </summary>
        public ViewModeEnum ViewMode
        {
            get
            {
                // Get mode from view state
                if (mViewMode == ViewModeEnum.Unknown)
                {
                    mViewMode = PortalContext.ViewMode;

                    if (mViewMode.IsDesign())
                    {
                        var currentUser = CurrentUser;

                        if (!currentUser.IsAuthorizedPerResource("CMS.Design", "Design") || !currentUser.IsAuthorizedPerUIElement("CMS.Design", "Design"))
                        {
                            mViewMode = ViewModeEnum.Preview;
                        }
                    }
                }

                return mViewMode;
            }
            set
            {
                mViewMode = value;
            }
        }


        /// <summary>
        /// Returns the array of the editable controls.
        /// </summary>
        public List<ICMSEditableControl> CMSEditableControls
        {
            get
            {
                if (mCMSEditableControls == null)
                {
                    mCMSEditableControls = PortalHelper.CollectEditableControls(Page);

                    // Set the Page manager
                    if (mCMSEditableControls != null)
                    {
                        foreach (ICMSEditableControl control in mCMSEditableControls)
                        {
                            control.PageManager = this;
                        }
                    }
                }
                return mCMSEditableControls;
            }
        }



        /// <summary>
        /// Returns the collection of all the base level page placeholders.
        /// </summary>
        public List<CMSPagePlaceholder> CMSPagePlaceholders
        {
            get
            {
                return mCMSPagePlaceholders;
            }
        }


        /// <summary>
        /// Returns the collection of all the page placeholders.
        /// </summary>
        public List<CMSPagePlaceholder> AllPagePlaceholders
        {
            get
            {
                return mAllPagePlaceholders ?? (mAllPagePlaceholders = PortalHelper.CollectPlaceholders(Page, true));
            }
        }


        /// <summary>
        /// Returns the Placeholder of the current page.
        /// </summary>
        public CMSPagePlaceholder CurrentPlaceholder
        {
            get
            {
                if (mCurrentPlaceholder == null)
                {
                    string aliasPath = DocumentContext.CurrentAliasPath;

                    // Find current placeholder
                    var placeholders = FindAllPlaceholders(aliasPath);
                    if (placeholders.Count > 0)
                    {
                        mCurrentPlaceholder = placeholders[0];
                    }
                }

                return mCurrentPlaceholder;
            }
        }


        /// <summary>
        /// Droppable areas are highlighted when widget dragged.
        /// </summary>
        public bool HighlightDropableAreas
        {
            get;
            set;
        }


        /// <summary>
        /// If true zone border can be activated (+add widget button).
        /// </summary>
        [DefaultValue(true)]
        public bool ActivateZoneBorder
        {
            get;
            set;
        }


        /// <summary>
        /// Returns currently edited document if it is available in the given context.
        /// </summary>
        public TreeNode CurrentNode
        {
            get
            {
                return DocumentManager.Node;
            }
        }


        /// <summary>
        /// Returns currently processed page info, if it is available in the given context.
        /// </summary>
        public PageInfo CurrentPageInfo
        {
            get
            {
                if (mCurrentPageInfo == null)
                {
                    if (mPageAliasPath == null)
                    {
                        mCurrentPageInfo = DocumentContext.CurrentPageInfo;
                    }
                    else
                    {
                        mCurrentPageInfo = PageInfoProvider.GetPageInfo(SiteName, PageAliasPath, LocalizationContext.PreferredCultureCode, null, SiteInfoProvider.CombineWithDefaultCulture(SiteName));
                    }
                }

                return mCurrentPageInfo;
            }
        }


        /// <summary>
        /// Gets the page template of the current page.
        /// </summary>
        public PageTemplateInfo PageTemplate
        {
            get
            {
                return CurrentPageInfo.TemplateInstance.ParentPageTemplate;
            }
        }


        /// <summary>
        /// Gets a value that indicates if the object locking is used.
        /// </summary>
        public bool UseObjectLocking
        {
            get
            {
                return SettingsKeyInfoProvider.GetBoolValue("CMSUseObjectCheckinCheckout") && LicenseHelper.CheckFeature(RequestContext.CurrentDomain, FeatureEnum.ObjectVersioning);
            }
        }


        /// <summary>
        /// Placeholder menu inner control.
        /// </summary>
        public CMSAbstractPortalUserControl PlaceholderMenuControl
        {
            get
            {
                EnsureChildControls();
                return mPlaceholderMenuControl;
            }
        }


        /// <summary>
        /// Indicates if panel with management controls should be shown
        /// </summary>
        internal bool ShowPanel
        {
            get
            {
                return QueryHelper.GetBoolean("showpanel", false);
            }
        }

        #endregion


        #region "Child controls methods"

        /// <summary>
        /// Register base placeholder 
        /// </summary>
        /// <param name="plc">Placeholder instance</param>
        internal void RegisterPagePlaceHolder(CMSPagePlaceholder plc)
        {
            if (allowRegisterPlaceholder)
            {
                if (mCMSPagePlaceholders == null)
                {
                    mCMSPagePlaceholders = new List<CMSPagePlaceholder>();
                }

                mCMSPagePlaceholders.Add(plc);
            }
        }


        /// <summary>
        /// Child control creation.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            // Check for the Design Mode in Visual Studio and page info is available
            if ((Context == null) || (CurrentPageInfo == null))
            {
                return;
            }

            // Keep current view mode
            ViewModeEnum viewMode = ViewMode;

            // Does not need additional portal controls in UI
            if (viewMode == ViewModeEnum.UI)
            {
                return;
            }

            // Web parts action manager
            actionManager = new WebPartActionManager(this);
            actionManager.ID = "am";
            Controls.Add(actionManager);

            // Create header panel
            pnlHeader = new HeaderPanel(this);
            pnlHeader.EnableViewState = false;
            pnlHeader.Attributes.Add("id", "CMSHeaderDiv");

            if (ShowPanel)
            {
                pnlHeader.Attributes.Add("class", CultureHelper.IsUICultureRTL() ? "RTL" : "LTR");
            }
            Controls.Add(pnlHeader);

            // Add context menus
            CreateContextMenus(viewMode);
        }

        /// <summary>
        /// Creates the content personalization context menu
        /// </summary>
        internal void CreatePersonalizationMenu()
        {
            // Add web part/zone variant context menu for Edit mode (Content personalization only)
            CMSAbstractPortalUserControl mContentPersonalizationVariantsMenuControl = (CMSAbstractPortalUserControl)Page.LoadUserControl("~/CMSModules/OnlineMarketing/Controls/Content/MenuContentPersonalizationVariants.ascx");
            mContentPersonalizationVariantsMenuControl.PortalManager = this;
            mContentPersonalizationVariantsMenuControl.ID = "contextMenu";
            mContentPersonalizationVariantsMenuControl.ShortID = "cpv";

            Controls.Add(mContentPersonalizationVariantsMenuControl);
        }


        /// <summary>
        /// Adds the context menu for widget variants
        /// </summary>
        internal void CreateWidgetVariantMenu()
        {
            // Add widget variant context menu
            ContextMenu menuAddWidgetVariant = new ContextMenu();
            menuAddWidgetVariant.ID = "menuAddWidgetVariant";
            menuAddWidgetVariant.MenuID = "addWidgetVariantMenu";
            menuAddWidgetVariant.VerticalPosition = VerticalPositionEnum.Bottom;
            menuAddWidgetVariant.HorizontalPosition = HorizontalPositionEnum.Left;
            menuAddWidgetVariant.ActiveItemCssClass = "WebPartActiveContextMenu";
            menuAddWidgetVariant.OffsetY = 2;
            Controls.Add(menuAddWidgetVariant);

            // Add the menu
            CMSAbstractPortalUserControl mAddWidgetVariantMenuControl = (CMSAbstractPortalUserControl)Page.LoadUserControl("~/CMSModules/OnlineMarketing/Controls/Content/MenuAddWidgetVariant.ascx");
            mAddWidgetVariantMenuControl.PortalManager = this;
            mAddWidgetVariantMenuControl.ID = "contextMenu";
            mAddWidgetVariantMenuControl.ShortID = "wvm";
            menuAddWidgetVariant.Controls.Add(mAddWidgetVariantMenuControl);
        }



        /// <summary>
        /// Creates the messages placeholder control
        /// </summary>
        internal void CreateMessagesPlaceholder()
        {
            // Info panel
            mMessagesPlaceholder = new MessagesPlaceHolder
            {
                ID = "plcMess",
                ShortID = "pM",
                IsLiveSite = false
            };
            Controls.Add(mMessagesPlaceholder);
        }



        /// <summary>
        /// Creates the context menu controls
        /// </summary>
        /// <param name="viewMode">View mode</param>
        private void CreateContextMenus(ViewModeEnum viewMode)
        {
            // Context menu wrapper container
            contextMenuContainer = new NamingContainer();
            contextMenuContainer.ID = "cm";
            Controls.Add(contextMenuContainer);

            // Add context menus for design mode
            if (PortalContext.IsDesignMode(viewMode, false))
            {
                // Web part menu
                ContextMenu menuWebPart = new ContextMenu();
                menuWebPart.ID = "menuWebPart";
                menuWebPart.MenuID = "webPartMenu";
                menuWebPart.VerticalPosition = VerticalPositionEnum.Bottom;
                menuWebPart.HorizontalPosition = HorizontalPositionEnum.Left;
                menuWebPart.ActiveItemCssClass = "WebPartActiveContextMenu";
                menuWebPart.MouseButton = MouseButtonEnum.Right;
                menuWebPart.OffsetY = 1;
                contextMenuContainer.Controls.Add(menuWebPart);

                // Add the menu
                CMSAbstractPortalUserControl mWebPartMenuControl = (CMSAbstractPortalUserControl)Page.LoadUserControl(PORTALENGINE_WEBPARTSPATH + "WebPartMenu.ascx");
                mWebPartMenuControl.PortalManager = this;
                mWebPartMenuControl.ID = "contextMenu";
                mWebPartMenuControl.ShortID = "wm";
                menuWebPart.Controls.Add(mWebPartMenuControl);

                // Zone menu
                ContextMenu menuZone = new ContextMenu();
                menuZone.ID = "menuZone";
                menuZone.MenuID = "webPartZoneMenu";
                menuZone.VerticalPosition = VerticalPositionEnum.Bottom;
                menuZone.HorizontalPosition = HorizontalPositionEnum.Left;
                menuZone.ActiveItemCssClass = "WebPartZoneActiveContextMenu";
                menuZone.MouseButton = MouseButtonEnum.Right;
                menuZone.OffsetX = -1;
                contextMenuContainer.Controls.Add(menuZone);

                // Add the menu
                CMSAbstractPortalUserControl mZoneMenuControl = (CMSAbstractPortalUserControl)Page.LoadUserControl(PORTALENGINE_LAYOUTPATH + "ZoneMenu.ascx");
                mZoneMenuControl.PortalManager = this;
                mZoneMenuControl.ID = "zoneContextMenu";
                mZoneMenuControl.ShortID = "zm";
                menuZone.Controls.Add(mZoneMenuControl);

                // Widget menu
                ContextMenu menuWidget = new ContextMenu();
                menuWidget.ID = "menuWidget";
                menuWidget.MenuID = "widgetMenu";
                menuWidget.VerticalPosition = VerticalPositionEnum.Bottom;
                menuWidget.HorizontalPosition = HorizontalPositionEnum.Left;
                menuWidget.ActiveItemCssClass = "WebPartActiveContextMenu";
                menuWidget.MouseButton = MouseButtonEnum.Right;
                menuWidget.OffsetY = 1;
                contextMenuContainer.Controls.Add(menuWidget);

                // Add the menu
                CMSAbstractPortalUserControl mWidgetMenuControl = (CMSAbstractPortalUserControl)Page.LoadUserControl(PORTALENGINE_WIDGETSPATH + "WidgetMenu.ascx");
                mWidgetMenuControl.PortalManager = this;
                mWidgetMenuControl.ID = "contextWidgetMenu";
                mWidgetMenuControl.ShortID = "wgm";
                menuWidget.Controls.Add(mWidgetMenuControl);

                // Widget zone menu
                ContextMenu menuWidgetZone = new ContextMenu();
                menuWidgetZone.ID = "menuWidgetZone";
                menuWidgetZone.MenuID = "widgetZoneMenu";
                menuWidgetZone.VerticalPosition = VerticalPositionEnum.Bottom;
                menuWidgetZone.HorizontalPosition = HorizontalPositionEnum.Left;
                menuWidgetZone.ActiveItemCssClass = "WebPartZoneActiveContextMenu";
                menuWidgetZone.MouseButton = MouseButtonEnum.Right;
                menuWidgetZone.OffsetX = -1;
                contextMenuContainer.Controls.Add(menuWidgetZone);

                // Add the menu
                CMSAbstractPortalUserControl mWidgetZoneMenuControl = (CMSAbstractPortalUserControl)Page.LoadUserControl(PORTALENGINE_WIDGETSPATH + "WidgetZoneMenu.ascx");
                mWidgetZoneMenuControl.PortalManager = this;
                mWidgetZoneMenuControl.ID = "widgetzoneContextMenu";
                mWidgetZoneMenuControl.ShortID = "wzm";
                menuWidgetZone.Controls.Add(mWidgetZoneMenuControl);

                // Placeholder menu
                ContextMenu menuPlaceholder = new ContextMenu();
                menuPlaceholder.ID = "menuPlaceholder";
                menuPlaceholder.MenuID = "pagePlaceholderMenu";
                menuPlaceholder.VerticalPosition = VerticalPositionEnum.Bottom;
                menuPlaceholder.HorizontalPosition = HorizontalPositionEnum.Left;
                menuPlaceholder.ActiveItemCssClass = "PagePlaceholderActiveContextMenu";
                menuPlaceholder.MouseButton = MouseButtonEnum.Right;
                menuPlaceholder.OffsetX = -1;
                contextMenuContainer.Controls.Add(menuPlaceholder);

                // Add the menu
                mPlaceholderMenuControl = (CMSAbstractPortalUserControl)Page.LoadUserControl(PORTALENGINE_LAYOUTPATH + "PlaceholderMenu.ascx");
                mPlaceholderMenuControl.PortalManager = this;
                mPlaceholderMenuControl.ID = "PlaceholderContextMenu";
                mPlaceholderMenuControl.ShortID = "pcm";
                menuPlaceholder.Controls.Add(mPlaceholderMenuControl);
            }
            else if ((ViewMode == ViewModeEnum.Edit) || (ViewMode == ViewModeEnum.EditLive))
            {
                // Widget menu
                ContextMenu menuWidgetEditor = new ContextMenu();
                menuWidgetEditor.ID = "menuWidgetEditor";
                menuWidgetEditor.MenuID = "widgetEditorMenu";
                menuWidgetEditor.VerticalPosition = VerticalPositionEnum.Bottom;
                menuWidgetEditor.HorizontalPosition = HorizontalPositionEnum.Left;
                menuWidgetEditor.OffsetY = 1;
                contextMenuContainer.Controls.Add(menuWidgetEditor);

                // Add the menu
                CMSAbstractPortalUserControl mWidgetEditorMenuControl = (CMSAbstractPortalUserControl)Page.LoadUserControl(PORTALENGINE_WIDGETSPATH + "WidgetEditorMenu.ascx");
                mWidgetEditorMenuControl.PortalManager = this;
                mWidgetEditorMenuControl.ID = "contextWidgetEditorMenu";
                mWidgetEditorMenuControl.ShortID = "wgem";
                menuWidgetEditor.Controls.Add(mWidgetEditorMenuControl);

                // Script for opening a new variant dialog window and activating widget border to prevent to widget border from hiding
                // when the user moves his mouse to the 'add widget' context menu.
                string script = @"
function OpenMenuConfWidget(menuPositionEl, targetId) {
    currentContextMenuId = targetId;
    ContextMenu('widgetEditorMenu', menuPositionEl, webPartLocation[targetId + '_container'], true);
}";

                ScriptHelper.RegisterStartupScript(this, typeof(string), "OpenMenuConfWidgetScript", ScriptHelper.GetScript(script));


                // Widget zone menu
                ContextMenu menuWidgetZoneEditor = new ContextMenu();
                menuWidgetZoneEditor.ID = "menuWidgetZoneEditor";
                menuWidgetZoneEditor.MenuID = "widgetZoneEditorMenu";
                menuWidgetZoneEditor.VerticalPosition = VerticalPositionEnum.Bottom;
                menuWidgetZoneEditor.HorizontalPosition = HorizontalPositionEnum.Left;
                menuWidgetZoneEditor.ActiveItemCssClass = "WebPartZoneActiveContextMenu";
                menuWidgetZoneEditor.MouseButton = MouseButtonEnum.Both;
                menuWidgetZoneEditor.OffsetX = -1;
                contextMenuContainer.Controls.Add(menuWidgetZoneEditor);

                // Add the menu
                CMSAbstractPortalUserControl mWidgetZoneEditorMenuControl = (CMSAbstractPortalUserControl)Page.LoadUserControl(PORTALENGINE_WIDGETSPATH + "WidgetZoneEditorMenu.ascx");
                mWidgetZoneEditorMenuControl.PortalManager = this;
                mWidgetZoneEditorMenuControl.ID = "widgetzoneEditorContextMenu";
                mWidgetZoneEditorMenuControl.ShortID = "wzem";
                menuWidgetZoneEditor.Controls.Add(mWidgetZoneEditorMenuControl);
            }
        }

        #endregion


        #region "Page processing methods"

        /// <summary>
        /// Constructor.
        /// </summary>
        public CMSPortalManager()
        {
            ActivateZoneBorder = true;

            PreRender += CMSPortalManager_PreRender;
            Init += CMSPortalManager_Init;

            PortalContext.CurrentPageManager = this;
        }


        /// <summary>
        /// PreRender event handler.
        /// </summary>
        protected void CMSPortalManager_PreRender(object sender, EventArgs e)
        {
            // Check for the Design Mode in Visual Studio
            if (Context == null)
            {
                return;
            }

            var vm = ViewMode;


            // Register save changes script just for specific scope in edit mode
            if (vm.IsOneOf(ViewModeEnum.Edit, ViewModeEnum.EditLive))
            {
                DocumentManager.UseFullFormSaveChanges = false;
            }

            // Ensure js script for split mode
            EnsureSplitModeScript(vm);

            var portalViewMode = PortalContext.ViewMode;

            if (!portalViewMode.IsOneOf(ViewModeEnum.LiveSite, ViewModeEnum.EditLive, ViewModeEnum.UI) || ShowPanel)
            {
                // Ensure edit script on the page
                ScriptHelper.RegisterEditScript(Page);
            }

            // Hide header panel if empty
            if (pnlHeader != null)
            {
                pnlHeader.Visible = (pnlHeader.Controls.Count > 0);
            }

            // Enable viewstate back if page changed viewstate was disabled
            if ((CMSPagePlaceholders != null) && PageChanged)
            {
                foreach (CMSPagePlaceholder placeholder in CMSPagePlaceholders)
                {
                    placeholder.EnableViewState = true;
                }
            }
        }


        /// <summary>
        /// Render action.
        /// </summary>
        protected override void Render(HtmlTextWriter writer)
        {
            // Check for the Design Mode in Visual Studio
            if (Context == null)
            {
                writer.Write("[ CMSPortalManager Control : " + ClientID + " ]");
                return;
            }

            if (ViewMode.IsEdit(true))
            {
                var fields = GetSpellCheckFields();

                string fieldsArray = "";
                if ((fields != null) && (fields.Count > 0))
                {
                    fieldsArray += "[";
                    foreach (string field in fields)
                    {
                        fieldsArray += "'" + field + "',";
                    }
                    fieldsArray = fieldsArray.TrimEnd(',') + "]";
                }

                string script = "";

                if (fieldsArray != "")
                {
                    // Save changes support
                    if (DocumentManager.ConfirmChanges)
                    {
                        script += " var checkChangedFields = " + fieldsArray + ";";
                    }

                    // Spell check support
                    script += " var spellCheckFields = " + fieldsArray + ";";
                }

                writer.WriteLine(ScriptHelper.GetScript(script));
            }

            // Hidden field for the page change notification
            if (RequiresWebPartManagement != WebPartManagementEnum.None)
            {
                writer.WriteLine("<input type=\"hidden\" name=\"pageChanged\" id=\"pageChanged\" value=\"0\" />");
            }

            // Render the contents
            if (ViewMode.IsLiveSite() && !ShowPanel)
            {
                RenderChildren(writer);
            }
            else
            {
                base.Render(writer);
            }
        }


        /// <summary>
        /// Init event handler.
        /// </summary>
        protected void CMSPortalManager_Init(object sender, EventArgs e)
        {
            // Check for the Design Mode in Visual Studio
            if (Context == null)
            {
                return;
            }


            // Register scripts
            if (ViewMode != ViewModeEnum.LiveSite)
            {
                ScriptHelper.RegisterLoader(Page);

                EnsureUIContextUIElement(ViewMode);
            }

            ClearEditorWidgets();
            InitializeDocumentManager();

            PageContext.BeforeInitComplete += PageHelper_InitComplete;

            // Make sure controls are not loaded too late
            EnsureChildControls();

            // Load the correct workflow version of document content and editor widgets.
            LoadDocumentContent();

            // Initialize extenders
            var extensionTarget = pnlHeader as IExtendableHeaderPanel;
            extensionTarget.InitializeExtenders("PortalManager");
        }


        private void ClearEditorWidgets()
        {
            if (!ViewMode.IsEdit() && !ViewMode.IsEditLive())
            {
                return;
            }

            bool keepWidgetsInSession;
            const string CMS_CONTENT_CHANGED_PARAM = "cmscontentchanged";

            if (ViewMode.IsEdit())
            {
                // Page tab: Keep widgets in the session only when there was some manipulation with the widgets or when the request URL is invalid
                //   Valid URLs:
                //        /getdoc/GUID/NodeAlias?cmscontentchanged=false   - first load: valid URL must contain the 'cmscontentchanged' query parameter
                //        /getdoc/GUID/NodeAlias?cmscontentchanged=true    - widgets were modified
                //   Invalid URL:
                //        /getdoc/GUID/NodeAlias      - invalid URL (URL does not contain 'cmscontentchanged' parameter) => do not clear widgets from the session. This will ensure that for example wrong image URL <img src="image.jpg" ../> will not clear widgets from the session.
                keepWidgetsInSession = QueryHelper.GetBoolean(CMS_CONTENT_CHANGED_PARAM, true);
            }
            else
            {
                // On-site editing: Keep widgets in session only when there was some manipulation with the widgets.
                // That is indicated by the 'cmscontentchanged' query parameter of value: TRUE
                keepWidgetsInSession = QueryHelper.GetBoolean(CMS_CONTENT_CHANGED_PARAM, false);
            }

            if (!keepWidgetsInSession && (CurrentPageInfo != null))
            {
                // Clear the editor widgets in the temporary interlayer for the first page load
                PortalContext.ClearEditorWidgets(CurrentPageInfo.DocumentID);
            }
        }


        private void InitializeDocumentManager()
        {
            if (ViewMode.IsLiveSite())
            {
                // Document manager is not required for the live site
                return;
            }

            if (ViewMode.IsEdit() || ViewMode.IsEditLive())
            {
                // Register document manager events
                DocumentManager.OnSaveData += DocumentManager_OnSaveData;
                DocumentManager.OnValidateData += DocumentManager_OnValidateData;
                DocumentManager.OnAfterAction += DocumentManager_OnAfterAction;
                DocumentManager.OnLoadData += DocumentManager_OnLoadData;
            }
            else
            {
                // Do not check save changes script in other modes
                DocumentManager.RegisterSaveChangesScript = false;
            }
        }


        /// <summary>
        /// Supplies correct UI element name and resource name to UIContext if view mode is Edit or Design.
        /// </summary>
        /// <param name="viewMode">View mode</param>
        private void EnsureUIContextUIElement(ViewModeEnum viewMode)
        {
            string resName = null;
            string elemName = null;

            if (viewMode.IsEdit(true))
            {
                resName = "cms.content";
                elemName = "page";
            }
            else if (viewMode.IsDesign(true))
            {
                resName = "cms.design";
                elemName = "design";
            }

            if (!String.IsNullOrEmpty(resName))
            {
                UIContext cntx = UIContext.Current;
                cntx.ResourceName = resName;
                cntx.ElementName = elemName;
            }
        }


        /// <summary>
        /// Init complete event handler
        /// </summary>
        void PageHelper_InitComplete(object sender, EventArgs e)
        {
            // Do not allow other root placeholders
            allowRegisterPlaceholder = false;

            LoadPage(true);

            // Ensure context menu placeholder
            ContextMenuPlaceHolder.EnsurePlaceholder(Page, Parent);
        }


        /// <summary>
        /// Refreshes current page by redirecting it to the same URL.
        /// </summary>
        internal static void RefreshPage()
        {
            HeaderPanel.RefreshPage();
        }


        /// <summary>
        /// Loads the content of the document. Loads the correct workflow version of document content and editor widgets.
        /// </summary>
        private void LoadDocumentContent()
        {
            DebugHelper.SetContext("PortalManager");

            PageInfo currentPageInfo = CurrentPageInfo;
            if (currentPageInfo != null)
            {
                if ((ViewMode != ViewModeEnum.LiveSite) && (ViewMode != ViewModeEnum.UI))
                {
                    // Init manager by NodeID and culture code because of linked documents and security check
                    DocumentManager.NodeID = currentPageInfo.NodeID;
                    DocumentManager.CultureCode = currentPageInfo.DocumentCulture;

                    // Process current page content (document content & editor widgets)
                    ProcessContent();
                }
            }

            DebugHelper.ReleaseContext();
        }


        /// <summary>
        /// Loads the pages structure.
        /// </summary>
        private void LoadPage(bool reloadData)
        {
            // Build the page path through the tree structure
            DebugHelper.SetContext("PortalManager");

            // Load the root placeholders content
            if (CMSPagePlaceholders != null)
            {
                bool defaultLoaded = false;
                CMSPagePlaceholder firstPlaceholder = null;

                foreach (CMSPagePlaceholder placeholder in CMSPagePlaceholders)
                {
                    // Disable ViewState when page structure changed
                    if (PageChanged)
                    {
                        placeholder.EnableViewState = false;
                    }

                    // Load the content
                    placeholder.PortalManager = this;

                    // Load the content
                    if (!placeholder.UsingDefaultPage)
                    {
                        if (!defaultLoaded)
                        {
                            placeholder.LoadContent(HierarchyPageInfo.VisualRootPageInfo, reloadData);
                            if (placeholder.LayoutTemplate == null)
                            {
                                defaultLoaded = true;
                            }
                        }
                    }
                }

                // Reload controls of placeholders with external content
                foreach (CMSPagePlaceholder placeholder in CMSPagePlaceholders)
                {
                    if (placeholder.HasExternalContent)
                    {
                        placeholder.ReloadControls();
                    }

                    if (firstPlaceholder == null)
                    {
                        firstPlaceholder = placeholder;
                    }
                }

                // Load the orphaned zones to the last placeholder
                if (firstPlaceholder != null)
                {
                    firstPlaceholder.LoadOrphanedZones(reloadData);
                }
            }

            if ((CurrentPageInfo != null) && reloadData)
            {
                // Save content of standalone editable controls to the page info
                foreach (ICMSEditableControl control in PortalContext.CurrentEditableControls)
                {
                    control.LoadContent(HierarchyPageInfo.CurrentPageInfo);
                }
            }

            DebugHelper.ReleaseContext();
        }


        /// <summary>
        /// Returns true if current user is allowed to read and modify specified node
        /// </summary>
        /// <param name="node">Current node</param>
        /// <param name="viewMode">Current view mode</param>
        public static bool IsAuthorizedPerDocument(TreeNode node, ViewModeEnum viewMode)
        {
            // Check read permissions
            var currentUser = MembershipContext.AuthenticatedUser;
            if (currentUser.IsAuthorizedPerDocument(node, NodePermissionsEnum.Read) == AuthorizationResultEnum.Denied)
            {
                return false;
            }
            // Check modify permissions
            else if (currentUser.IsAuthorizedPerDocument(node, NodePermissionsEnum.Modify) == AuthorizationResultEnum.Denied)
            {
                if (viewMode != ViewModeEnum.Preview)
                {
                    return false;
                }
            }

            return true;
        }


        /// <summary>
        /// Processes current page content (workflow, authorization, information).
        /// </summary>
        private void ProcessContent()
        {
            ViewMode = ViewModeEnum.Unknown;

            // Get current document content
            if (CurrentNode == null)
            {
                return;
            }

            // Check read permissions
            if (ViewMode.IsOneOf(ViewModeEnum.Edit, ViewModeEnum.Design, ViewModeEnum.EditLive)
                && (!IsAuthorizedPerDocument(CurrentNode, ViewMode) || ((ViewMode != ViewModeEnum.Design) && !DocumentManager.AllowSave)))
            {
                ViewMode = ViewModeEnum.EditDisabled;
            }
            else
            {
                ViewMode = PortalHelper.GetWorkflowViewMode(CurrentNode, DocumentManager, ViewMode);
            }

            // Load the correct version of the document
            CurrentPageInfo.LoadVersion(CurrentNode);

            CurrentPageInfo.LoadContentXml(ValidationHelper.GetString(CurrentNode.GetValue("DocumentContent"), ""));
        }


        /// <summary>
        /// Generate div tag key by default due to HTML validity
        /// </summary>
        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return HtmlTextWriterTag.Div;
            }
        }


        /// <summary>
        /// Returns the list of all the page placeholders using the given alias path.
        /// </summary>
        /// <param name="aliasPath">Alias path</param>
        protected internal List<CMSPagePlaceholder> FindAllPlaceholders(string aliasPath)
        {
            List<CMSPagePlaceholder> result = new List<CMSPagePlaceholder>();

            // Go through all the page placeholders
            if (AllPagePlaceholders != null)
            {
                aliasPath = aliasPath ?? String.Empty;

                foreach (CMSPagePlaceholder placeholder in AllPagePlaceholders)
                {
                    string pageAliasPath = (placeholder.PageInfo != null) ? placeholder.PageInfo.NodeAliasPath : String.Empty;
                    pageAliasPath = pageAliasPath ?? String.Empty;

                    if (String.Equals(pageAliasPath, aliasPath, StringComparison.InvariantCultureIgnoreCase))
                    {
                        result.Add(placeholder);
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// Ensures the list of the zones that were not yet loaded.
        /// </summary>
        /// <param name="zones">List of zones</param>
        public List<WebPartZoneInstance> EnsureNotLoadedZones(List<WebPartZoneInstance> zones)
        {
            return mNotLoadedZones ?? (mNotLoadedZones = new List<WebPartZoneInstance>(zones));
        }


        /// <summary>
        /// Marks the given zone as loaded.
        /// </summary>
        /// <param name="zone">Zone to mark</param>
        public void MarkZoneLoaded(WebPartZoneInstance zone)
        {
            if (mNotLoadedZones != null)
            {
                mNotLoadedZones.Remove(zone);
            }
        }


        /// <summary>
        /// Ensures registering client script for split mode for appropriate view mode.
        /// </summary>
        /// <param name="viewMode">View mode</param>
        private void EnsureSplitModeScript(ViewModeEnum viewMode)
        {
            bool includeScript = false;
            bool refresh = false;

            switch (viewMode)
            {
                case ViewModeEnum.LiveSite:
                    includeScript = QueryHelper.GetBoolean("cmssplitmode", false);
                    break;

                case ViewModeEnum.Design:
                case ViewModeEnum.DesignDisabled:
                case ViewModeEnum.Edit:
                case ViewModeEnum.EditDisabled:
                case ViewModeEnum.Preview:
                    // Design, Edit, EditDisabled or Preview mode
                    if (PortalUIHelper.DisplaySplitMode)
                    {
                        // Indicates if is post back
                        if (RequestHelper.IsPostBack())
                        {
                            // Original and split mode cultures are same
                            refresh = (CMSString.Compare(CultureHelper.GetOriginalPreferredCulture(), PortalUIHelper.SplitModeCultureCode, true) == 0);

                            // Handle design mode
                            if (viewMode == ViewModeEnum.Design && (CurrentPageInfo != null))
                            {
                                // Refresh frames with same page template
                                refresh |= CurrentPageInfo.NodeTemplateForAllCultures;
                            }
                        }
                        includeScript = true;
                    }
                    break;
            }

            // Register js script
            if (includeScript)
            {
                ScriptHelper.RegisterSplitModeSync(Page, true, false, refresh);
            }
        }

        #endregion


        #region "Document actions"

        void DocumentManager_OnValidateData(object sender, DocumentManagerEventArgs e)
        {
            e.IsValid = Validate();
        }


        void DocumentManager_OnSaveData(object sender, DocumentManagerEventArgs e)
        {
            TreeNode node = e.Node;

            CurrentPageInfo.LoadVersion(node);

            // Clear current content if not preserved
            if (!PreserveContent &&
                !PortalContext.EditableControlsHidden &&
                !PortalContext.MVTVariantsEnabled &&
                !PortalContext.ContentPersonalizationVariantsEnabled
                )
            {
                // Gets editable web parts
                var webParts = CurrentPageInfo.EditableWebParts;

                // Check whether exist at least one editable web part
                if ((webParts != null) && (webParts.Count > 0))
                {
                    var tInst = CurrentPageInfo.TemplateInstance;
                    if (tInst != null)
                    {
                        List<string> keysToRemove = new List<string>();

                        // Gets the list of web parts to remove
                        foreach (DictionaryEntry edit in webParts)
                        {
                            string key = Convert.ToString(edit.Key);
                            string mainKey = key.Split(';')[0];

                            if (tInst.GetWebPart(mainKey) == null)
                            {
                                keysToRemove.Add(key);
                            }
                        }

                        // Remove web parts
                        foreach (string key in keysToRemove)
                        {
                            webParts.Remove(key);
                        }
                    }
                }
            }


            // Save content of page placeholders to the page info
            if (CMSPagePlaceholders != null)
            {
                foreach (CMSPagePlaceholder page in CMSPagePlaceholders)
                {
                    page.SaveContent(CurrentPageInfo);
                }
            }

            // Save content of standalone editable controls to the page info
            foreach (ICMSEditableControl control in PortalContext.CurrentEditableControls)
            {
                control.SaveContent(CurrentPageInfo);
            }

            // Update the content XML
            CurrentNode.SetValue("DocumentContent", CurrentPageInfo.DocumentContent);

            if (ViewMode.IsOneOf(ViewModeEnum.Edit, ViewModeEnum.EditLive))
            {
                string editorWidgets = PortalContext.GetEditorWidgets(CurrentPageInfo.DocumentID);

                // Get editor widgets from temporary interlayer
                if (editorWidgets != null)
                {
                    // Save the temporary interlayer (containing editor web parts) to the document
                    CurrentNode.SetValue("DocumentWebParts", editorWidgets);

                    // Clear the interlayer after the document is saved
                    PortalContext.ClearEditorWidgets(CurrentPageInfo.DocumentID);
                }
            }
        }


        void DocumentManager_OnAfterAction(object sender, DocumentManagerEventArgs e)
        {
            switch (e.ActionName)
            {
                case DocumentComponentEvents.UNDO_CHECKOUT:
                case DocumentComponentEvents.CHECKOUT:
                case DocumentComponentEvents.CHECKIN:
                case DocumentComponentEvents.CREATE_VERSION:
                case DocumentComponentEvents.PUBLISH:
                case DocumentComponentEvents.APPROVE:
                case DocumentComponentEvents.REJECT:
                    {
                        // Refresh the page because the ViewMode can be changed after the actions (Edit VS EditDisabled) and the components need to load correct state
                        // And clean information about content changed
                        ScriptHelper.RegisterClientScriptBlock(Page, typeof(string), "RefreshPage", "$cmsj(function() { window.location.replace(window.location.href.replace(/((&)|(\\?))cmscontentchanged=([^&]*)(&)?/gi, '$3$2')); });", true);
                    }
                    break;

                default:
                    ProcessContent();

                    // Remember to reload view mode of the zones and web parts when their Render method starts 
                    if (PortalContext.ViewMode.IsEdit(true))
                    {
                        ReloadViewMode = true;
                    }
                    break;
            }
        }


        void DocumentManager_OnLoadData(object sender, DocumentManagerEventArgs e)
        {
            ProcessContent();
        }

        #endregion


        #region "Action handling"

        /// <summary>
        /// Gets the page info within current structure for specific alias path.
        /// </summary>
        /// <param name="aliasPath">Alias path</param>
        internal PageInfo GetPageInfoForEditing(string aliasPath)
        {
            PageInfo pi = HierarchyPageInfo.VisualRootPageInfo;

            while (pi != null)
            {
                // If alias path matches, return current page info
                if (pi.NodeAliasPath.EqualsCSafe(aliasPath, true))
                {
                    break;
                }
                pi = pi.ChildPageInfo;
            }

            // No result in case the template is not available for editing
            if ((pi == null) || (pi.UsedPageTemplateInfo == null))
            {
                return null;
            }

            return pi;
        }


        /// <summary>
        /// Gets the template instance for editing changes.
        /// </summary>
        /// <param name="pi">Page info</param>
        public static PageTemplateInstance GetTemplateInstanceForEditing(PageInfo pi)
        {
            PageTemplateInstance pti = null;

            if (pi != null)
            {
                // Get personalized view in live site mode
                ViewModeEnum viewMode = PortalContext.ViewMode;
                if (PortalContext.IsDesignMode(viewMode, false))
                {
                    // Design mode
                    PageTemplateInfo ti = pi.UsedPageTemplateInfo;
                    if (ti != null)
                    {
                        pti = ti.TemplateInstance;
                    }
                }
                else
                {
                    switch (viewMode)
                    {
                        case ViewModeEnum.LiveSite:
                            // Try to get personalized template instance for live site mode
                            pti = PersonalizationInfoProvider.GetPersonalizedTemplateInstance(pi, MembershipContext.AuthenticatedUser.UserID);
                            break;

                        case ViewModeEnum.DashboardWidgets:
                            // Try to get personalized template instance for specified dashboard
                            pti = PersonalizationInfoProvider.GetPersonalizedTemplateInstance(pi, MembershipContext.AuthenticatedUser.UserID, PortalContext.DashboardName, PortalContext.DashboardSiteName);
                            break;
                    }
                }

                // Get standard way (document instance)
                if (pti == null)
                {
                    pti = pi.TemplateInstance;
                }

                // Clone template instance to avoid modification of already loaded collection in not-editing mode
                if (pti != null)
                {
                    pti = pti.Clone(true);
                }
            }

            return pti;
        }


        /// <summary>
        /// Saves the changes to the widget zone.
        /// </summary>
        /// <param name="pi">Page info</param>
        /// <param name="instance">Template instance with the new data</param>
        /// <param name="zoneType">Zone type</param>
        /// <param name="viewMode">View mode</param>
        /// <param name="tp">Tree provider</param>
        public static void SaveTemplateChanges(PageInfo pi, PageTemplateInstance instance, WidgetZoneTypeEnum zoneType, ViewModeEnum viewMode, TreeProvider tp)
        {
            // In design mode or if the zone is standard, always save the data to the page template (default data)
            if (PortalContext.IsDesignMode(viewMode, false) || (zoneType == WidgetZoneTypeEnum.None))
            {
                PageTemplateInfo pti = pi.UsedPageTemplateInfo;

                // Save the page template
                pti.TemplateInstance.WebParts = instance.WebParts;
                PageTemplateInfoProvider.SetPageTemplateInfo(pti);
            }
            else if ((viewMode.IsOneOf(ViewModeEnum.Edit, ViewModeEnum.EditLive) && (zoneType == WidgetZoneTypeEnum.Editor)) ||
                    ((viewMode.IsLiveSite()) && (zoneType == WidgetZoneTypeEnum.Group)))
            {
                // Save to the document as editor or group admin changes
                TreeNode node = DocumentHelper.GetDocument(pi.DocumentID, tp);

                if (zoneType == WidgetZoneTypeEnum.Editor)
                {
                    // Store the editor widgets in the temporary interlayer
                    if (viewMode.IsEdit(true) || viewMode.IsEditLive())
                    {
                        PortalContext.SaveEditorWidgets(pi.DocumentID, instance.GetZonesXML(WidgetZoneTypeEnum.Editor));
                    }
                    // or save immediately for group view mode
                    else
                    {
                        node.SetValue("DocumentWebParts", instance.GetZonesXML(WidgetZoneTypeEnum.Editor));
                        DocumentHelper.UpdateDocument(node, tp);
                    }
                }
                else if (zoneType == WidgetZoneTypeEnum.Group)
                {
                    // Extract and set the group web parts
                    node.SetValue("DocumentGroupWebParts", instance.GetZonesXML(WidgetZoneTypeEnum.Group));

                    // Save the document
                    DocumentHelper.UpdateDocument(node, tp);
                }
            }
            else if ((viewMode.IsLiveSite()) && (zoneType == WidgetZoneTypeEnum.User))
            {
                // Save only when the user is authenticated
                if (AuthenticationHelper.IsAuthenticated())
                {
                    // Save to the user personalization
                    int userId = MembershipContext.AuthenticatedUser.UserID;
                    int documentId = pi.DocumentID;

                    // Try to get existing personalization
                    PersonalizationInfo upi = PersonalizationInfoProvider.GetUserPersonalization(userId, documentId);
                    if (upi == null)
                    {
                        // Create new personalization if not found
                        upi = new PersonalizationInfo();
                        upi.PersonalizationUserID = userId;
                        upi.PersonalizationDocumentID = documentId;
                    }

                    // Extract user zones and apply personalization
                    upi.TemplateInstance.WebParts = instance.GetZonesXML(WidgetZoneTypeEnum.User);
                    PersonalizationInfoProvider.SetPersonalizationInfo(upi);
                }
            }
            else if ((viewMode == ViewModeEnum.DashboardWidgets) && (zoneType == WidgetZoneTypeEnum.Dashboard))
            {
                // Save only when the user is authenticated
                if (AuthenticationHelper.IsAuthenticated())
                {
                    // Save to the user personalization
                    int userId = MembershipContext.AuthenticatedUser.UserID;

                    // Try to get existing personalization
                    PersonalizationInfo upi = PersonalizationInfoProvider.GetDashBoardPersonalization(userId, PortalContext.DashboardName, PortalContext.DashboardSiteName);
                    if (upi == null)
                    {
                        // Create new personalization if not found
                        upi = new PersonalizationInfo();
                        upi.PersonalizationUserID = userId;
                        upi.PersonalizationDashboardName = PortalContext.DashboardName;
                        SiteInfo si = SiteInfoProvider.GetSiteInfo(PortalContext.DashboardSiteName);
                        if (si != null)
                        {
                            upi.PersonalizationSiteID = si.SiteID;
                        }
                    }

                    // Extract user zones and apply personalization
                    upi.TemplateInstance.WebParts = instance.GetZonesXML(WidgetZoneTypeEnum.Dashboard);
                    PersonalizationInfoProvider.SetPersonalizationInfo(upi);
                }
            }
        }


        /// <summary>
        /// Saves the changes to the zone
        /// </summary>
        /// <param name="pi">Page info</param>
        /// <param name="pti">Page template instance</param>
        /// <param name="zone">Zone instance</param>
        /// <param name="refresh">Refresh after saving the changes</param>
        internal void SaveZoneChanges(PageInfo pi, PageTemplateInstance pti, WebPartZoneInstance zone, bool refresh)
        {
            if (zone != null)
            {
                if (zone.VariantID > 0)
                {
                    // Save zone variant changes
                    VariantHelper.SetVariantWebParts(zone, zone.VariantID);
                }
                else
                {
                    // Save the changes
                    SaveTemplateChanges(pi, pti, zone.WidgetZoneType, ViewMode, TreeProvider);
                }
            }

            // Reload the contents
            if (refresh)
            {
                RefreshPage();
            }
        }


        /// <summary>
        /// Returns true if entered data is valid. If data is invalid, it returns false and displays an error message.
        /// </summary>
        public bool Validate()
        {
            bool isValid = true;

            if (CurrentPageInfo != null)
            {
                if (CurrentNode != null)
                {
                    // Save current content to the page info
                    if (CMSPagePlaceholders != null)
                    {
                        foreach (CMSPagePlaceholder page in CMSPagePlaceholders)
                        {
                            isValid = isValid & page.Validate();
                        }
                    }

                    // Validate editable region
                    if (isValid && CMSEditableControls != null)
                    {
                        foreach (ICMSEditableControl region in CMSEditableControls)
                        {
                            isValid = isValid & region.IsValid();
                        }
                    }
                }
            }

            return isValid;
        }


        /// <summary>
        /// Returns the list of the field IDs (Client IDs of the inner controls) that should be spell checked.
        /// </summary>
        public List<string> GetSpellCheckFields()
        {
            List<string> result = new List<string>();

            // Save current content to the page info
            if (CMSPagePlaceholders != null)
            {
                foreach (CMSPagePlaceholder page in CMSPagePlaceholders)
                {
                    List<string> fields = page.GetSpellCheckFields();
                    if (fields != null)
                    {
                        result.AddRange(fields);
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// Clears the cached items on a page.
        /// </summary>
        public void ClearCache()
        {
            // Clear cache of all main placeholders
            if (CMSPagePlaceholders != null)
            {
                foreach (CMSPagePlaceholder placeholder in CMSPagePlaceholders)
                {
                    placeholder.ClearCache();
                }
            }
        }


        /// <summary>
        /// Causes reloading the data, override to implement the data reloading procedure.
        /// </summary>
        public void ReloadData()
        {
            // Reload data of all main placeholders
            if (CMSPagePlaceholders != null)
            {
                foreach (CMSPagePlaceholder placeholder in CMSPagePlaceholders)
                {
                    placeholder.ReloadData();
                }
            }
        }


        /// <summary>
        /// Sets the main (root) page placeholder.
        /// </summary>
        /// <param name="placeholder">Placeholder to set</param>
        public void SetMainPagePlaceholder(CMSPagePlaceholder placeholder)
        {
            if (mCMSPagePlaceholders == null)
            {
                // Init the original (root page placeholder)
                List<CMSPagePlaceholder> placeholders = new List<CMSPagePlaceholder>();
                placeholders.Add(placeholder);

                mCMSPagePlaceholders = placeholders;
            }
        }

        #endregion
    }
}
