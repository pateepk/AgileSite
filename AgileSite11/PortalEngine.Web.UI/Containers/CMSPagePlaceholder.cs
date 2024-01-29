using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.DeviceProfiles;
using CMS.DocumentEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.Membership;
using CMS.SiteProvider;
using CMS.Synchronization;

namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// Page content container.
    /// </summary>
    [ToolboxData("<{0}:CMSPagePlaceholder runat=server></{0}:CMSPagePlaceholder>")]
    [ParseChildren(true)]
    [DebuggerDisplay("CMSPagePlaceholder({ID})")]
    public class CMSPagePlaceholder : CMSPlaceHolder, ICMSPortalControl, INamingContainer, IPageManager
    {
        #region "Variables"

        /// <summary>
        /// Parent page layout.
        /// </summary>
        protected CMSAbstractLayout mPageLayout;

        /// <summary>
        /// Placeholder to supply window to the next page level when layout not defined.
        /// </summary>
        protected CMSPagePlaceholder mNoLayoutPlaceholder = null;

        /// <summary>
        /// Parent portal manager.
        /// </summary>
        protected CMSPortalManager mPortalManager;

        /// <summary>
        /// Parent page placeholder.
        /// </summary>
        protected CMSPagePlaceholder mPagePlaceholder;

        /// <summary>
        /// Child page placeholders.
        /// </summary>
        protected SafeDictionary<string, CMSPagePlaceholder> mChildPagePlaceholders;

        /// <summary>
        /// Sibling page placeholders.
        /// </summary>
        protected SafeDictionary<string, CMSPagePlaceholder> mSiblingPagePlaceholders;

        /// <summary>
        /// ArrayList of the editable controls located under current placeholder.
        /// </summary>
        protected List<ICMSEditableControl> mCMSEditableControls;

        /// <summary>
        /// Design mode header container.
        /// </summary>
        protected Panel pnlDesignHeader;

        /// <summary>
        /// Design mode content container.
        /// </summary>
        protected Panel pnlDesignContent = null;

        /// <summary>
        /// Control title.
        /// </summary>
        protected Label lblTitle;

        /// <summary>
        /// Overall container.
        /// </summary>
        protected Panel pnlPlaceholder;

        /// <summary>
        /// Placeholder level layout info.
        /// </summary>
        protected LayoutInfo mLayoutInfo;

        /// <summary>
        /// Placeholder level page template layout info.
        /// </summary>
        protected PageTemplateInfo mPageTemplateInfo;

        /// <summary>
        /// Indicates whether user is authorized to read the content when in live site mode.
        /// </summary>
        protected bool? mIsAuthorized;

        /// <summary>
        /// If true, placeholder is enabled in design mode.
        /// </summary>
        protected bool mDesignEnabled = true;

        /// <summary>
        /// If true, placeholder allows the design mode actions.
        /// </summary>
        protected bool mAllowDesignModeActions = true;

        /// <summary>
        /// If set, the placeholder has overridden ViewMode.
        /// </summary>
        protected ViewModeEnum mViewMode = ViewModeEnum.Unknown;

        /// <summary>
        /// Page level of the Placeholder.
        /// </summary>
        protected int mPageLevel = -1;

        private int mCacheMinutes = -1;
        private bool mCheckPermissions = true;
        private string mPagePlaceholderID;
        private string deviceInfoMessage = String.Empty;

        private string mSiteName;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Page placeholder ID
        /// </summary>
        public string PagePlaceholderID
        {
            get
            {
                if (mPagePlaceholderID == null)
                {
                    return this.ID;
                }

                return mPagePlaceholderID;
            }
            set
            {
                this.mPagePlaceholderID = value;
            }
        }


        /// <summary>
        /// CSS class for the envelope
        /// </summary>
        public string CssClass
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the page placeholder displays header in design mode
        /// </summary>
        public bool DisplayHeader
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the placeholder hides the page template name
        /// </summary>
        public bool HidePageTemplateName
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the placeholder is the main (root) placeholder on the page.
        /// </summary>
        public bool Root
        {
            get;
            set;
        }


        /// <summary>
        /// Parent portal manager.
        /// </summary>
        public virtual CMSPortalManager PortalManager
        {
            get
            {
                // Check for the Design Mode in Visual Studio
                if (Context == null)
                {
                    return mPortalManager;
                }

                return mPortalManager ?? (mPortalManager = (CMSPortalManager)PortalContext.CurrentPageManager);
            }
            set
            {
                mPortalManager = value;
            }
        }


        /// <summary>
        /// Returns the table of all the inner placeholders.
        /// </summary>
        public Hashtable ChildPagePlaceholders
        {
            get
            {
                if (mChildPagePlaceholders == null)
                {
                    // Collect the placeholders
                    mChildPagePlaceholders = ControlsHelper.GetControlsHashtable(PortalHelper.CollectPlaceholders(this), p => p.PagePlaceholderID);

                    // Init parent placeholders in children
                    if (mChildPagePlaceholders != null)
                    {
                        foreach (CMSPagePlaceholder placeholder in mChildPagePlaceholders.Values)
                        {
                            placeholder.PagePlaceholder = this;
                        }
                    }
                }
                return mChildPagePlaceholders;
            }
        }


        /// <summary>
        /// Returns the table of all the sibling page placeholders (page placeholders on the same level).
        /// </summary>
        public SafeDictionary<string, CMSPagePlaceholder> SiblingPagePlaceholders
        {
            get
            {
                if (mSiblingPagePlaceholders == null)
                {
                    mSiblingPagePlaceholders = new SafeDictionary<string, CMSPagePlaceholder>();

                    if (PageLevel <= 0)
                    {
                        // Get all placeholders on the root level
                        foreach (CMSPagePlaceholder sibling in PortalManager.CMSPagePlaceholders)
                        {
                            mSiblingPagePlaceholders[sibling.PagePlaceholderID.ToLowerCSafe()] = sibling;
                        }
                    }
                    else
                    {
                        // Get all child placeholders from all siblings of parent
                        foreach (CMSPagePlaceholder parentSibling in PagePlaceholder.SiblingPagePlaceholders.Values)
                        {
                            foreach (CMSPagePlaceholder sibling in parentSibling.ChildPagePlaceholders.Values)
                            {
                                mSiblingPagePlaceholders[sibling.PagePlaceholderID.ToLowerCSafe()] = sibling;
                            }
                        }
                    }
                }

                return mSiblingPagePlaceholders;
            }
        }


        /// <summary>
        /// Returns the layout control.
        /// </summary>
        public virtual CMSAbstractLayout Layout
        {
            get
            {
                return mPageLayout;
            }
        }


        /// <summary>
        /// Returns the layout information.
        /// </summary>
        public virtual LayoutInfo LayoutInfo
        {
            get
            {
                return mLayoutInfo;
            }
        }


        /// <summary>
        /// Web part zones collection.
        /// </summary>
        public virtual List<CMSWebPartZone> WebPartZones
        {
            get
            {
                if (mPageLayout != null)
                {
                    return mPageLayout.WebPartZonesList;
                }
                else
                {
                    return null;
                }
            }
        }


        /// <summary>
        /// Returns page template instance structure.
        /// </summary>
        public virtual PageTemplateInstance TemplateInstance
        {
            get
            {
                if (mPageLayout != null)
                {
                    return mPageLayout.TemplateInstance;
                }
                else
                {
                    return null;
                }
            }
        }


        /// <summary>
        /// Parent page placeholder.
        /// </summary>
        public virtual CMSPagePlaceholder PagePlaceholder
        {
            get
            {
                // Check for the Design Mode in Visual Studio
                if (Context == null)
                {
                    return mPagePlaceholder;
                }

                if (mPagePlaceholder == null)
                {
                    // Find the parent page placeholder
                    mPagePlaceholder = PortalHelper.FindParentPlaceholder(this);
                    if (mPagePlaceholder == null)
                    {
                        throw new Exception("[CMSPagePlaceholder.PagePlaceholder]: Parent CMSPagePlaceholder not found.");
                    }
                }
                return mPagePlaceholder;
            }
            set
            {
                mPagePlaceholder = value;
            }
        }


        /// <summary>
        /// Page info.
        /// </summary>
        public virtual PageInfo PageInfo
        {
            get;
            set;
        }


        /// <summary>
        /// Returns the page template information.
        /// </summary>
        public virtual PageTemplateInfo PageTemplateInfo
        {
            get
            {
                return mPageTemplateInfo;
            }
        }


        /// <summary>
        /// Page mode of the current placeholder page.
        /// </summary>
        public virtual ViewModeEnum ViewMode
        {
            get
            {
                // Check for the Design Mode in Visual Studio
                if (Context == null)
                {
                    return ViewModeEnum.Unknown;
                }

                // Use overridden view mode
                if (mViewMode != ViewModeEnum.Unknown)
                {
                    return mViewMode;
                }

                // Get the necessary objects
                PageInfo pi = PageInfo;
                CMSPortalManager manager = PortalManager;

                // Evaluate the automatic view mode
                switch (manager.ViewMode)
                {
                    case ViewModeEnum.Design:
                        // Design modes
                        if (!DesignEnabled || UsingDefaultPageTemplate)
                        {
                            // Preview mode if design not enabled
                            return ViewModeEnum.Preview;
                        }

                        if (pi != null)
                        {
                            if (pi.ChildPageInfo != null)
                            {
                                // Preview mode if not the current document
                                return ViewModeEnum.Preview;
                            }

                            if ((pi.UsedPageTemplateInfo != null) && (pi.UsedPageTemplateInfo.IsReusable && !MembershipContext.AuthenticatedUser.IsAuthorizedPerUIElement("CMS.Design", "Design.ModifySharedTemplates")))
                            {
                                // Disabled design if not allowed to edit current template
                                return ViewModeEnum.DesignDisabled;
                            }
                        }
                        break;

                    case ViewModeEnum.Edit:
                    case ViewModeEnum.EditLive:
                    case ViewModeEnum.EditDisabled:
                        // Edit modes
                        if ((pi != null) && (PageInfo.ChildPageInfo != null))
                        {
                            return ViewModeEnum.EditNotCurrent;
                        }
                        break;
                }

                return manager.ViewMode;
            }
            set
            {
                // Set the overridden view mode
                mViewMode = value;
            }
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
        /// Site code name.
        /// </summary>
        public virtual string SiteName
        {
            get
            {
                if (mSiteName == null)
                {
                    mSiteName = SiteContext.CurrentSiteName;
                }
                return mSiteName;
            }
            set
            {
                mSiteName = value;
            }
        }


        /// <summary>
        /// If true, placeholder checks access permissions for the editable web parts content.
        /// </summary>
        public bool CheckPermissions
        {
            get
            {
                return mCheckPermissions;
            }
            set
            {
                mCheckPermissions = value;
            }
        }


        /// <summary>
        /// Returns true if the user is authorized for the document.
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

                    if ((PageInfo != null) && (CheckPermissions || !liveSite))
                    {
                        // Try to get value from cache
                        UserInfo currentUser = MembershipContext.AuthenticatedUser;

                        int cacheMinutes = CacheMinutes;
                        if (!liveSite)
                        {
                            cacheMinutes = 0;
                        }

                        // Try to get data from cache
                        using (var cs = new CachedSection<bool>(ref result, cacheMinutes, true, null, "pageplaceholderauthorized", CacheHelper.GetBaseCacheKey(true, false), SiteContext.CurrentSiteName, PageInfo.NodeAliasPath))
                        {
                            if (cs.LoadData)
                            {
                                // Check if authorized
                                result = !(MembershipContext.AuthenticatedUser.IsAuthorizedPerDocument(PageInfo.NodeID, PageInfo.ClassName, NodePermissionsEnum.Read) == AuthorizationResultEnum.Denied);

                                // Save to the cache
                                if (cs.Cached)
                                {
                                    // Prepare the dependencies (depending on document data / user: roles)
                                    string[] dependencies = new string[]
                                                                {
                                                                    "node|" + SiteName.ToLowerCSafe() + "|" + PageInfo.NodeAliasPath.ToLowerCSafe(),
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


        /// <summary>
        /// If true, placeholder is enabled in design mode.
        /// </summary>
        public bool DesignEnabled
        {
            get
            {
                return mDesignEnabled;
            }
            set
            {
                mDesignEnabled = value;
            }
        }


        /// <summary>
        /// If true, placeholder is using default page template.
        /// </summary>
        public bool UsingDefaultPageTemplate
        {
            get;
            set;
        }

        /// <summary>
        /// If true, placeholder is using default document.
        /// </summary>
        public bool UsingDefaultDocument
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether the placeholder is using either default document or default page template.
        /// </summary>
        public bool UsingDefaultPage
        {
            get
            {
                return UsingDefaultDocument || UsingDefaultPageTemplate;
            }
        }


        /// <summary>
        /// If true, placeholder is using external content (does not load the layout if available and uses the page info if set).
        /// </summary>
        public bool HasExternalContent
        {
            get;
            set;
        }


        /// <summary>
        /// If true, placeholder allows the design mode actions.
        /// </summary>
        public bool AllowDesignModeActions
        {
            get
            {
                return mAllowDesignModeActions;
            }
            set
            {
                mAllowDesignModeActions = value;
            }
        }


        /// <summary>
        /// Page level.
        /// </summary>
        public int PageLevel
        {
            get
            {
                if (mPageLevel < 0)
                {
                    // Ensure the parent placeholder
                    if (mPagePlaceholder == null)
                    {
                        mPagePlaceholder = PortalHelper.FindParentPlaceholder(this);
                    }
                    // Initialize the level
                    if (mPagePlaceholder == null)
                    {
                        mPageLevel = 0;
                    }
                    else
                    {
                        mPageLevel = PagePlaceholder.PageLevel + 1;
                    }
                }
                return mPageLevel;
            }
            set
            {
                mPageLevel = value;
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
                    mCMSEditableControls = PortalHelper.CollectEditableControls(this, true, true);

                    // Set the page manager
                    if (mCMSEditableControls != null)
                    {
                        foreach (ICMSEditableControl control in mCMSEditableControls)
                        {
                            // If editable control, set the page manager
                            if (control is CMSAbstractEditableControl)
                            {
                                control.PageManager = this;
                            }
                        }
                    }
                    else
                    {
                        mCMSEditableControls = new List<ICMSEditableControl>();
                    }
                }
                return mCMSEditableControls;
            }
        }

        #endregion


        #region "Template properties"

        /// <summary>
        /// Layout template, if set, the given layout is used.
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty), TemplateContainer(typeof(EmptyLayout)), DefaultValue((string)null), Browsable(false)]
        public virtual ITemplate LayoutTemplate
        {
            get;
            set;
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Constructor, initializes the parent portal manager.
        /// </summary>
        /// <param name="parentManager">Parent portal manager</param>
        public CMSPagePlaceholder(CMSPortalManager parentManager)
        {
            mPortalManager = parentManager;

            DisplayHeader = true;
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        public CMSPagePlaceholder()
            : this(null)
        {
            if (Root)
            {
                PortalManager.SetMainPagePlaceholder(this);
            }
        }


        /// <summary>
        /// Databinding event.
        /// </summary>
        protected override void OnDataBinding(EventArgs e)
        {
            EnsureChildControls();
            base.OnDataBinding(e);
        }


        /// <summary>
        /// Initializes the child controls.
        /// </summary>
        protected override void CreateChildControls()
        {
            Controls.Clear();

            base.CreateChildControls();

            this.SetShortID();

            pnlPlaceholder = new Panel();
            pnlPlaceholder.ID = "pnlPage";
            pnlPlaceholder.CssClass = "PagePlaceholder";

            Controls.Add(pnlPlaceholder);
        }


        /// <summary>
        /// Reload the page controls.
        /// </summary>
        public void ReloadControls()
        {
            EnsureChildControls();

            pnlPlaceholder.Controls.Clear();

            // Prepare the content display control
            Control ctrlContent = null;
            if (mPageLayout != null)
            {
                ctrlContent = mPageLayout;
            }
            else
            {
                ctrlContent = mNoLayoutPlaceholder;
            }

            Control contentContainer = pnlPlaceholder;

            // Build the control structure
            switch (ViewMode)
            {
                case ViewModeEnum.Design:
                case ViewModeEnum.DesignDisabled:
                    {
                        // Create the control header
                        if (DisplayHeader)
                        {
                            CreateHeader();
                        }

                        // Create an extra layer for content of the placeholder
                        Panel pnlContent = new Panel();
                        pnlContent.ID = "c";
                        pnlContent.CssClass = "PagePlaceholderContent";

                        contentContainer.Controls.Add(pnlContent);
                        contentContainer = pnlContent;
                    }
                    break;

                default:
                    // Add just the content control
                    {
                        pnlPlaceholder.CssClass = "PagePlaceholder";
                    }
                    break;
            }

            // Add the content
            contentContainer.Controls.Add(ctrlContent);
        }


        /// <summary>
        /// Creates the control header
        /// </summary>
        private void CreateHeader()
        {
            // Use UI culture for strings
            string culture = MembershipContext.AuthenticatedUser.PreferredUICultureCode;

            pnlPlaceholder.Controls.Add(new LiteralControl("<div class=\"PagePlaceholderHeaderOut cms-bootstrap\">"));

            // Add the header
            pnlDesignHeader = new Panel();
            pnlDesignHeader.CssClass = "PagePlaceholderHeader";
            pnlPlaceholder.Controls.Add(pnlDesignHeader);

            pnlPlaceholder.Controls.Add(new LiteralControl("</div>"));

            string parameter = "'" + PageInfo.NodeAliasPath + "'";

            if (AllowDesignModeActions)
            {
                pnlDesignHeader.Controls.Add(new LiteralControl(ContextMenuContainer.GetStartTag("pagePlaceholderMenu", parameter)));
            }

            pnlDesignHeader.Controls.Add(new LiteralControl("<table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" class=\"_nodivs\"><tr><td class=\"PagePlaceholderLeftAction\">"));

            // Menu image
            if (AllowDesignModeActions)
            {
                pnlDesignHeader.Controls.Add(new LiteralControl(""));

                // Create menu icon
                CMSIcon menuIcon = new CMSIcon()
                {
                    CssClass = "icon-menu"
                };
                menuIcon.Attributes.Add("onclick", string.Format("ContextMenu('pagePlaceholderMenu', this.parentNode.parentNode.parentNode.parentNode, {0}, true); return false;", parameter));
                menuIcon.Attributes.Add("aria-hidden", "true");
                menuIcon.ToolTip = ResHelper.GetString("PagePlaceholder.Menu");

                pnlDesignHeader.Controls.Add(menuIcon);

                if (PortalManager.PlaceholderMenuControl != null)
                {
                    // Set the placeholder menu control
                    PortalManager.PlaceholderMenuControl.PagePlaceholder = this;
                }

                pnlDesignHeader.Controls.Add(new LiteralControl(""));
            }

            pnlDesignHeader.Controls.Add(new LiteralControl("</td>"));

            // Display the device icon in the header if the current device profile is set
            DeviceProfileInfo deviceProfile = DeviceContext.CurrentDeviceProfile;
            if ((deviceProfile != null) && (PageInfo != null) && (PageInfo.UsedPageTemplateInfo != null))
            {
                PageTemplateDeviceLayoutInfo deviceLayout = PageTemplateDeviceLayoutInfoProvider.GetTemplateDeviceLayoutInfo(PageInfo.UsedPageTemplateInfo.PageTemplateId, deviceProfile.ProfileID);
                bool isCustomLayout = (deviceLayout != null);
                bool isLayoutAutoMapping = false;

                if (SettingsKeyInfoProvider.GetBoolValue(SiteContext.CurrentSiteName + ".CMSLayoutMappingEnable"))
                {
                    // Check layout auto mapping
                    if (!isCustomLayout && PageInfo.UsedPageTemplateInfo.LayoutID > 0)
                    {
                        isLayoutAutoMapping = (DeviceProfileLayoutInfoProvider.GetTargetLayoutInfo(deviceProfile.ProfileID, PageInfo.UsedPageTemplateInfo.LayoutID) != null);
                    }
                }

                // Display the icon when there is a specific layout defined
                if (isCustomLayout || isLayoutAutoMapping)
                {
                    string tooltip = String.Format(ResHelper.GetString(isCustomLayout ? "devicelayout.placeholder.tooltip" : "devicelayout.automapping.tooltip"), deviceProfile.ProfileDisplayName);
                    pnlDesignHeader.Controls.Add(new LiteralControl("<td class=\"PagePlaceholderDevice\"><i class=\"icon-monitor-smartphone\" aria-hidden=\"true\" \" title=\"" + HTMLHelper.HTMLEncode(tooltip) + "\"></i></td>"));
                }
            }

            pnlDesignHeader.Controls.Add(new LiteralControl("<td>"));

            // Title
            lblTitle = new Label();
            lblTitle.CssClass = "PagePlaceholderTitle";
            lblTitle.Text = GetTitle(culture);

            pnlDesignHeader.Controls.Add(lblTitle);
            pnlDesignHeader.Controls.Add(new LiteralControl("</td></tr></table>"));

            if (AllowDesignModeActions)
            {
                pnlDesignHeader.Controls.Add(new LiteralControl(ContextMenuContainer.GetEndTag()));

                RegisterEditLayoutScript(this, PageInfo.UsedPageTemplateInfo.PageTemplateId, PageInfo.NodeAliasPath, LayoutInfo);
                
                // Register edit on double click on page placeholder header
                if ((ViewMode != ViewModeEnum.DesignDisabled) && MembershipContext.AuthenticatedUser.IsAuthorizedPerUIElement("CMS.Design", "Design.EditLayout") && (LayoutTemplate == null))
                {
                    pnlDesignHeader.Attributes.Add("ondblclick", "return EditLayout();");
                }
            }
        }


        /// <summary>
        /// Registers edit layout script for specified control
        /// </summary>
        /// <param name="ctrl">Control</param>
        /// <param name="templateId">Page template id</param>
        /// <param name="aliasPath">Node alias path</param>
        /// <param name="li">Layout info</param>
        public static void RegisterEditLayoutScript(Control ctrl, int templateId, string aliasPath, LayoutInfo li)
        {
            ScriptHelper.RegisterWOpenerScript(ctrl.Page);

            String layoutURL = UrlResolver.ResolveUrl(String.Format("~/CMSModules/PortalEngine/UI/PageLayouts/PageLayout_Edit.aspx?dialog=1&templateid={0}&aliaspath={1}", templateId, aliasPath));
            if (li != null)
            {
                layoutURL = URLHelper.AddParameterToUrl(layoutURL, "layoutid", li.LayoutId.ToString());
            }

            string deviceName = QueryHelper.GetString("devicename", string.Empty);
            if (!string.IsNullOrEmpty(deviceName))
            {
                layoutURL = URLHelper.AddParameterToUrl(layoutURL, "devicename", HttpUtility.UrlEncode(deviceName));
                layoutURL = URLHelper.AddParameterToUrl(layoutURL, ObjectLifeTimeFunctions.OBJECT_LIFE_TIME_KEY, "request");
            }

            // Register function for edit layout for context menu
            ScriptHelper.RegisterClientScriptBlock(ctrl, typeof(String), "EditLayoutScript", ScriptHelper.GetScript("function EditLayout(){modalDialog('" + layoutURL + "','EditLayout','85%','85%'); }"));
        }


        /// <summary>
        /// Gets the page placeholder title
        /// </summary>
        /// <param name="culture">Current UI culture</param>
        private string GetTitle(string culture)
        {
            string title = null;

            if (HidePageTemplateName)
            {
                // Only document name path
                title = HTMLHelper.HTMLEncode(PageInfo.DocumentNamePath);
            }
            else
            {
                // Full title
                string templateName = null;

                if (PageInfo.UsedPageTemplateInfo != null)
                {
                    if (PageInfo.UsedPageTemplateInfo.IsReusable)
                    {
                        templateName = ResHelper.LocalizeString(PageInfo.UsedPageTemplateInfo.DisplayName);
                    }
                    else
                    {
                        templateName = ResHelper.GetString("PageTemplateInfo.AdHoc", culture);
                    }
                }
                else
                {
                    templateName = ResHelper.GetString("PageTemplateInfo.None", culture);
                }

                title += String.Format(ResHelper.GetString("PagePlaceholder.TitleInfo", culture), HTMLHelper.HTMLEncode(PageInfo.DocumentNamePath), HTMLHelper.HTMLEncode(templateName));
            }
            return title;
        }


        /// <summary>
        /// Loads the editable regions content.
        /// </summary>
        public void LoadRegionsContent(bool loadChildren = true)
        {
            // Load layout regions content
            if (mPageLayout != null)
            {
                foreach (ICMSEditableControl control in CMSEditableControls)
                {
                    control.LoadContent(PageInfo);
                }
            }

            if (loadChildren)
            {
                // Load the child placeholders content
                if (ChildPagePlaceholders != null)
                {
                    foreach (CMSPagePlaceholder placeholder in ChildPagePlaceholders.Values)
                    {
                        // Load the content
                        if (!placeholder.UsingDefaultPage)
                        {
                            placeholder.LoadRegionsContent();
                        }
                    }
                }
                else if (mNoLayoutPlaceholder != null)
                {
                    mNoLayoutPlaceholder.LoadRegionsContent();
                }
            }
        }


        /// <summary>
        /// Load the page content to the placeholder.
        /// </summary>
        /// <param name="pageInfo">Page info with the page content</param>
        /// <param name="reloadData">Reload the data</param>
        public void LoadContent(PageInfo pageInfo, bool reloadData = true)
        {
            // Ensure the page info
            if (!HasExternalContent || (PageInfo == null))
            {
                PageInfo = pageInfo;
            }

            bool reloadLayout = !HasExternalContent;

            // Reload layout also when not loaded yet
            if (mPageLayout == null)
            {
                reloadLayout = true;
            }

            // Reset the layout based components
            if (reloadLayout)
            {
                mChildPagePlaceholders = null;
                mPageLayout = null;
            }

            if (PageInfo != null)
            {
                ViewModeEnum viewMode = ViewMode;
                bool isDesignMode = PortalContext.IsDesignMode(viewMode);

                DebugHelper.SetContext("PagePlaceholder_" + ID);

                // Get the template info
                PageTemplateInfo templateInfo = PageInfo.UsedPageTemplateInfo;
                if ((templateInfo != null) && ((templateInfo.PageTemplateId > 0) || !templateInfo.IsPortal))
                {
                    if (isDesignMode
                        && SynchronizationHelper.UseCheckinCheckout
                        && !templateInfo.Generalized.IsCheckedOutByUser(MembershipContext.AuthenticatedUser))
                    {
                        ViewMode = ViewModeEnum.DesignDisabled;
                    }

                    bool loadContent = false;

                    if (LayoutTemplate != null)
                    {
                        // Load the layout template
                        LoadLayout(null);

                        LayoutTemplate.InstantiateIn(mPageLayout);

                        loadContent = true;
                    }
                    else
                    {
                        // Load the layout
                        int layoutId = templateInfo.LayoutID;
                        bool isDeviceLayout = false;
                        string deviceLayoutCode = String.Empty;

                        // Get list of device profiles with dependence on current view mode
                        List<DeviceProfileInfo> deviceProfiles = DeviceContext.CurrentDeviceProfiles;

                        // Device layout info
                        PageTemplateDeviceLayoutInfo ptdi = null;
                        DeviceProfileInfo currentDeviceProfile = null;

                        if ((deviceProfiles != null) && (deviceProfiles.Count > 0))
                        {
                            deviceInfoMessage = "notDefined";
                            bool targetLayoutFound = false;

                            // Check if automatic layout mapping for mobile devices is enabled
                            bool layoutMappingEnabled = SettingsKeyInfoProvider.GetBoolValue(SiteContext.CurrentSiteName + ".CMSLayoutMappingEnable");

                            // Loop thru all device profiles
                            foreach (DeviceProfileInfo dpi in deviceProfiles)
                            {
                                ptdi = PageTemplateDeviceLayoutInfoProvider.GetTemplateDeviceLayoutInfo(templateInfo.PageTemplateId, dpi.ProfileID);
                                // Try custom mapping
                                if (ptdi != null)
                                {
                                    isDeviceLayout = true;
                                    currentDeviceProfile = dpi;

                                    // Shared layout
                                    if (ptdi.LayoutID > 0)
                                    {
                                        layoutId = ptdi.LayoutID;
                                    }
                                    // Custom layout
                                    else
                                    {
                                        layoutId = 0;
                                        deviceLayoutCode = ptdi.LayoutCode;
                                    }
                                    deviceInfoMessage = "custom";
                                    // Do not process other device profiles
                                    break;
                                }
                                // Try auto mapping
                                else if (layoutMappingEnabled && !targetLayoutFound && (layoutId > 0))
                                {
                                    int targetLayoutId = DeviceProfileLayoutInfoProvider.GetTargetLayoutId(dpi.ProfileID, layoutId);
                                    if (targetLayoutId > 0)
                                    {
                                        layoutId = targetLayoutId;

                                        // Set flag to true, so target layout won't be overwritten in next loop
                                        // There is no break, because device mapping based on template has higher priority even if it is lower in list (so it will rewrite layoutId in next loop)
                                        targetLayoutFound = true;
                                        deviceInfoMessage = "mapped";
                                    }
                                }
                            }
                        }

                        if (layoutId > 0)
                        {
                            // Get shared layout
                            mLayoutInfo = LayoutInfoProvider.GetLayoutInfo(layoutId);
                            if (mLayoutInfo != null)
                            {
                                // Register the page template in current components
                                PortalContext.CurrentComponents.RegisterLayout(mLayoutInfo);

                                // Insert the layout
                                if (reloadLayout)
                                {
                                    if (mLayoutInfo.LayoutType == LayoutTypeEnum.Ascx)
                                    {
                                        // Load the layout
                                        string url = mLayoutInfo.Generalized.GetVirtualFileRelativePath(LayoutInfo.EXTERNAL_COLUMN_CODE, mLayoutInfo.LayoutVersionGUID);

                                        // Load the layout
                                        LoadLayout(url);
                                    }
                                    else
                                    {
                                        // Load the text layout
                                        LoadTextLayout(mLayoutInfo.LayoutCode);
                                    }
                                }

                                loadContent = true;
                            }
                            else
                            {
                                throw new Exception("[CMSPagePlaceholder.LoadContent]: Layout for template '" + templateInfo.DisplayName + "' not found");
                            }
                        }
                        else
                        {
                            // Get layout from PageTemplateLayout
                            if (templateInfo.PageTemplateId > 0)
                            {
                                mPageTemplateInfo = templateInfo;
                            }

                            if (!isDeviceLayout)
                            {
                                // Register the page template in current components
                                PortalContext.CurrentComponents.RegisterPageTemplate(templateInfo);
                            }
                            else
                            {
                                // Register device layout in current components
                                PortalContext.CurrentComponents.RegisterDeviceLayout(ptdi);
                            }

                            // Insert the layout
                            if (reloadLayout)
                            {
                                if (templateInfo.PageTemplateLayoutType == LayoutTypeEnum.Ascx)
                                {
                                    // Prepare the layout URL
                                    string url = null;
                                    // Check whether page template info exists
                                    if (templateInfo.PageTemplateId > 0)
                                    {
                                        if (!isDeviceLayout)
                                        {
                                            url = templateInfo.Generalized.GetVirtualFileRelativePath(PageTemplateInfo.EXTERNAL_COLUMN_CODE, templateInfo.PageTemplateVersionGUID);
                                        }
                                        // Device layout
                                        else
                                        {
                                            PageTemplateDeviceLayoutInfo di = PageTemplateDeviceLayoutInfoProvider.GetTemplateDeviceLayoutInfo(templateInfo.PageTemplateId, currentDeviceProfile.ProfileID);
                                            if (di != null)
                                            {
                                                url = di.Generalized.GetVirtualFileRelativePath(PageTemplateDeviceLayoutInfo.EXTERNAL_COLUMN_CODE, di.LayoutVersionGUID);
                                            }
                                        }
                                    }

                                    // Load the layout
                                    LoadLayout(url);
                                }
                                else
                                {
                                    string layoutCode = deviceLayoutCode;
                                    if (!isDeviceLayout)
                                    {
                                        layoutCode = templateInfo.PageTemplateLayout;
                                    }

                                    // Load the text layout
                                    LoadTextLayout(layoutCode);
                                }
                            }

                            loadContent = true;
                        }

                        // Device info message
                        if ((!String.IsNullOrEmpty(deviceInfoMessage)) && (ViewMode == ViewModeEnum.Design))
                        {
                            PortalManager.AdditionalDesignMessage = ResHelper.GetString("designDeviceInfo." + deviceInfoMessage);
                        }
                    }

                    // Reload the controls
                    ReloadControls();

                    DocumentContext.CurrentParentPageInfos.Add(pageInfo);

                    if (loadContent)
                    {
                        // Load the layout content
                        if (reloadLayout)
                        {
                            mPageLayout.LoadContent(pageInfo, reloadData, isDesignMode);
                        }

                        bool defaultLoaded = false;
                        CMSPagePlaceholder firstPlaceholder = null;

                        // Setup the child ChildPagePlaceholders
                        if (pageInfo.ChildPageInfo != null)
                        {
                            foreach (CMSPagePlaceholder placeholder in ChildPagePlaceholders.Values)
                            {
                                placeholder.PortalManager = PortalManager;

                                // Load the content
                                if (!placeholder.UsingDefaultPage)
                                {
                                    if (!defaultLoaded)
                                    {
                                        placeholder.LoadContent(pageInfo.ChildPageInfo, reloadData);
                                        if (placeholder.LayoutTemplate == null)
                                        {
                                            defaultLoaded = true;
                                        }
                                    }
                                }
                            }
                        }

                        mPageLayout.RelocateContent();

                        // Reload controls of placeholders with external content
                        foreach (CMSPagePlaceholder placeholder in ChildPagePlaceholders.Values)
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
                }

                DebugHelper.ReleaseContext();
            }
        }


        /// <summary>
        /// Loads the orphaned zones to the.
        /// </summary>
        /// <param name="reloadData">Reload the web parts data</param>
        public void LoadOrphanedZones(bool reloadData)
        {
            if (PageInfo != null)
            {
                bool designMode = PortalContext.IsDesignMode(ViewMode, false);
                if (designMode && (mPageLayout != null))
                {
                    // Load the orphaned zones in design mode
                    mPageLayout.LoadOrphanedZones(reloadData);
                }
            }
        }


        /// <summary>
        /// Loads the given layout.
        /// </summary>
        /// <param name="url">Layout URL to load</param>
        public CMSAbstractLayout LoadLayout(string url)
        {
            try
            {
                // Load the layout control
                if (String.IsNullOrEmpty(url))
                {
                    mPageLayout = new EmptyLayout();
                }
                else
                {
                    mPageLayout = Page.LoadUserControl(url) as CMSAbstractLayout;

                    if (mPageLayout == null)
                    {
                        mPageLayout = new EmptyLayout();
                    }
                }

                mPageLayout.ID = "lt";
                mPageLayout.PagePlaceholder = this;
                mPageLayout.PortalManager = PortalManager;

                return mPageLayout;
            }
            catch (Exception ex)
            {
                // Create layout to display error
                LayoutError errorLayout = new LayoutError();

                errorLayout.InnerException = ex;
                mPageLayout = errorLayout;

                EventLogProvider.LogException("PortalEngine", "LOADLAYOUT", ex);
            }

            return null;
        }


        /// <summary>
        /// Loads the given text layout.
        /// </summary>
        /// <param name="code">Layout code to load</param>
        public CMSAbstractLayout LoadTextLayout(string code)
        {
            try
            {
                // Load the layout control
                mPageLayout = new TextLayout(code);

                mPageLayout.ID = "lt";
                mPageLayout.PagePlaceholder = this;
                mPageLayout.PortalManager = PortalManager;

                return mPageLayout;
            }
            catch (Exception ex)
            {
                // Create layout to display error
                LayoutError errorLayout = new LayoutError();
                errorLayout.InnerException = ex;
                mPageLayout = errorLayout;

                EventLogProvider.LogException("PortalEngine", "LOADTEXTLAYOUT", ex);
            }

            return null;
        }


        /// <summary>
        /// Saves the page content to the page info.
        /// </summary>
        /// <param name="pageInfo">Page info where to save the content</param>
        public void SaveContent(PageInfo pageInfo)
        {
            // Do not save if the placeholder is not loaded
            if (PageInfo == null)
            {
                return;
            }

            // Save all editable controls in current level
            if (PageInfo.ChildPageInfo == null)
            {
                foreach (ICMSEditableControl control in CMSEditableControls)
                {
                    control.SaveContent(pageInfo);
                }
            }
            else
            {
                // Save child placeholders
                if (mPageLayout != null)
                {
                    // Save all child placeholders content
                    foreach (CMSPagePlaceholder page in ChildPagePlaceholders.Values)
                    {
                        page.SaveContent(pageInfo);
                    }
                }
                else if (mNoLayoutPlaceholder != null)
                {
                    // Save content through the default page placeholder
                    mNoLayoutPlaceholder.SaveContent(pageInfo);
                }
            }
        }


        /// <summary>
        /// Returns true if entered data is valid. If data is invalid, it returns false and displays an error message.
        /// </summary>
        public bool Validate()
        {
            bool isValid = true;

            if (mPageLayout != null)
            {
                if (PageInfo.ChildPageInfo == null)
                {
                    isValid = isValid & mPageLayout.Validate();
                }
                foreach (CMSPagePlaceholder page in ChildPagePlaceholders.Values)
                {
                    isValid = isValid & page.Validate();
                }
            }
            else if (mNoLayoutPlaceholder != null)
            {
                isValid = isValid & mNoLayoutPlaceholder.Validate();
            }

            return isValid;
        }


        /// <summary>
        /// Init event handler
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            EnsureChildControls();
            base.OnInit(e);
            PortalManager.RegisterPagePlaceHolder(this);
        }


        /// <summary>
        /// Load event handler.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Load the content of editable regions
            if (PageInfo != null)
            {
                LoadRegionsContent(false);
            }
        }


        /// <summary>
        /// Renders the control.
        /// </summary>
        protected override void Render(HtmlTextWriter writer)
        {
            bool uiCultureRTL = CultureHelper.IsUICultureRTL();

            #region "Web part toolbar - Right position fix"

            // The Web part toolbar needs to add a padding to the page to ensure it will not lay over other page elements.
            bool renderToolbarLaout = (Root && PortalContext.IsDesignMode(PortalContext.ViewMode));

            if (renderToolbarLaout)
            {
                bool docCultureRTL = CultureHelper.IsPreferredCultureRTL();

                // WebPartToolbar - right position
                string className = "WPTTable";

                writer.AddAttribute("class", className);
                writer.RenderBeginTag("div");

                writer.AddAttribute("class", "WPTTableRow");
                writer.RenderBeginTag("div");

                if (uiCultureRTL)
                {
                    // RTL culture - render the Web part toolbar padding on the left side
                    RenderWPTCell(writer);
                }

                writer.AddAttribute("class", "WPTTableCell WPTTableCellContent");
                writer.RenderBeginTag("div");
            }

            #endregion

            switch (ViewMode)
            {
                case ViewModeEnum.Design:
                case ViewModeEnum.DesignDisabled:
                    {
                        if (!String.IsNullOrEmpty(this.CssClass))
                        {
                            writer.AddAttribute("class", this.CssClass);
                            writer.RenderBeginTag("div");

                            BaseRender(writer);

                            writer.RenderEndTag();
                        }
                        else
                        {
                            BaseRender(writer);
                        }
                    }
                    break;

                default:
                    // Default, render just the content
                    if (pnlPlaceholder != null)
                    {
                        foreach (Control child in pnlPlaceholder.Controls)
                        {
                            child.RenderControl(writer);
                        }
                    }
                    break;
            }

            #region "Enclose opened div tags for web part toolbar"

            if (renderToolbarLaout)
            {
                // Close wptTableCell
                writer.RenderEndTag();

                if (!uiCultureRTL)
                {
                    // RTL culture - render the Web part toolbar padding on the right side
                    RenderWPTCell(writer);
                }

                // Close wptTableRow
                writer.RenderEndTag();
                // Close wptTable
                writer.RenderEndTag();
            }

            #endregion
        }


        /// <summary>
        /// Renders a table cell which will ensure a correct padding of the content when the Web part toolbar is used.
        /// </summary>
        /// <param name="writer">The writer.</param>
        private void RenderWPTCell(HtmlTextWriter writer)
        {
            // This table cell will be under the toolbar and will have its min-width set (the same width as the web part toolbar) to ensure that all the page elements will be visible and accessible
            writer.AddAttribute("class", "WPTTableCell WPTTableCellPadding");
            writer.RenderBeginTag("div");
            writer.RenderEndTag();
        }


        /// <summary>
        /// Provides the base rendering of the controls
        /// </summary>
        /// <param name="writer">HTML writer for the output</param>
        private void BaseRender(HtmlTextWriter writer)
        {
            base.Render(writer);

            // Clear both
            writer.AddAttribute("class", "ClearBoth");
            writer.RenderBeginTag("div");
            //writer.Write("&nbsp;");
            writer.RenderEndTag();
        }


        /// <summary>
        /// Load ViewState event handler.
        /// </summary>
        protected override void LoadViewState(object savedState)
        {
            base.LoadViewState(savedState);
            LoadContent(PageInfo);
        }


        /// <summary>
        /// Returns the array list of the field IDs (Client IDs of the inner controls) that should be spell checked.
        /// </summary>
        public List<string> GetSpellCheckFields()
        {
            // Check the inner placeholders for HTML editors
            if (mPageLayout != null)
            {
                List<string> result = new List<string>();

                // Get from the layout
                List<string> fields = mPageLayout.GetSpellCheckFields();
                if (fields != null)
                {
                    result.AddRange(fields);
                }

                // Get from the child placeholders
                if (ChildPagePlaceholders != null)
                {
                    foreach (CMSPagePlaceholder placeholder in ChildPagePlaceholders.Values)
                    {
                        fields = placeholder.GetSpellCheckFields();
                        if (fields != null)
                        {
                            result.AddRange(fields);
                        }
                    }
                }

                return result;
            }
            else if (mNoLayoutPlaceholder != null)
            {
                return mNoLayoutPlaceholder.GetSpellCheckFields();
            }

            return null;
        }


        /// <summary>
        /// Clears the cache of the placeholder.
        /// </summary>
        public void ClearCache()
        {
            // Clear the cache in current layout
            if (mPageLayout != null)
            {
                mPageLayout.ClearCache();
            }

            // Clear the cache of the child placeholders
            foreach (DictionaryEntry entry in ChildPagePlaceholders)
            {
                CMSPagePlaceholder placeholder = entry.Value as CMSPagePlaceholder;
                if (placeholder != null)
                {
                    placeholder.ClearCache();
                }
            }
        }


        /// <summary>
        /// Causes reloading the data, override to implement the data reloading procedure.
        /// </summary>
        public virtual void ReloadData()
        {
            // Clear the cache in current layout
            if (mPageLayout != null)
            {
                mPageLayout.ReloadData();
            }

            // Clear the cache of the child placeholders
            foreach (DictionaryEntry entry in ChildPagePlaceholders)
            {
                CMSPagePlaceholder placeholder = entry.Value as CMSPagePlaceholder;
                if (placeholder != null)
                {
                    placeholder.ReloadData();
                }
            }
        }


        /// <summary>
        /// Returns true if the web part management support is required.
        /// </summary>
        public bool RequiresWebPartManagement()
        {
            // Clear the cache in current layout
            if (mPageLayout != null)
            {
                if (mPageLayout.RequiresWebPartManagement())
                {
                    return true;
                }
            }

            // Clear the cache of the child placeholders
            foreach (DictionaryEntry entry in ChildPagePlaceholders)
            {
                CMSPagePlaceholder placeholder = entry.Value as CMSPagePlaceholder;
                if (placeholder != null)
                {
                    if (placeholder.RequiresWebPartManagement())
                    {
                        return true;
                    }
                }
            }

            return false;
        }


        /// <summary>
        /// Finds the  web part with specific name.
        /// </summary>
        /// <param name="name">Web part name to find</param>
        public virtual CMSAbstractWebPart FindWebPart(string name)
        {
            if (mPageLayout != null)
            {
                return mPageLayout.FindWebPart(name);
            }

            return null;
        }


        /// <summary>
        /// Finds the  web part with specific type (first web part).
        /// </summary>
        /// <param name="type">Web part type to find</param>
        public virtual CMSAbstractWebPart FindWebPart(Type type)
        {
            if (mPageLayout != null)
            {
                return mPageLayout.FindWebPart(type);
            }

            return null;
        }


        /// <summary>
        /// Finds all web parts of specified type.
        /// </summary>
        /// <param name="type">Type to find</param>
        public virtual ArrayList FindAllWebParts(Type type)
        {
            if (mPageLayout != null)
            {
                return mPageLayout.FindAllWebParts(type);
            }

            return null;
        }


        /// <summary>
        /// Finds the zone by its ID.
        /// </summary>
        /// <param name="zoneId">Zone ID</param>
        public CMSWebPartZone FindZone(string zoneId)
        {
            if (mPageLayout != null)
            {
                return mPageLayout.FindZone(zoneId);
            }

            return null;
        }


        /// <summary>
        /// Gets UI image relative path.
        /// </summary>
        /// <param name="imagePath">Partial image path starting from ~/App_Themes/(Skin_Folder)/Images/ (e.g. '/CMSModules/Membership/module.png')</param>
        /// <param name="isLiveSite">Indicates if URL should be returned for live site</param>
        /// <param name="ensureDefaultTheme">Indicates if default theme should be ensured</param>
        public string GetImageUrl(string imagePath, bool isLiveSite = false, bool ensureDefaultTheme = false)
        {
            return UIHelper.GetImageUrl(null, imagePath, isLiveSite, ensureDefaultTheme);
        }

        #endregion
    }
}