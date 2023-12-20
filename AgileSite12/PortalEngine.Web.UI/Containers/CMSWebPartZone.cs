using System;
using System.Collections;
using System.Web.Caching;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.EventLog;
using CMS.Base.Web.UI.DragAndDrop;
using CMS.FormEngine.Web.UI;
using CMS.Globalization;
using CMS.Helpers;
using CMS.IO;
using CMS.MacroEngine;
using CMS.Membership;
using CMS.Modules;
using CMS.SiteProvider;

using TimeZoneInfo = CMS.Globalization.TimeZoneInfo;

namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// Web part zone control.
    /// </summary>
    [ToolboxData("<{0}:CMSWebPartZone runat=server></{0}:CMSWebPartZone>")]
    [DebuggerDisplay("CMSWebPartZone({ID})")]
    public class CMSWebPartZone : CMSWebControl, ICMSVariantsControl, INamingContainer, ITimeZoneManager
    {
        #region "Controls"

        /// <summary>
        /// Update panel.
        /// </summary>
        protected UpdatePanel mUpdatePanel;

        /// <summary>
        /// Zone instance with the settings.
        /// </summary>
        protected WebPartZoneInstance mZoneInstance;

        /// <summary>
        /// Parent page placeholder.
        /// </summary>
        protected CMSPagePlaceholder mPagePlaceholder;

        /// <summary>
        /// Collection of all the web part located within the zone.
        /// </summary>
        protected List<Control> mWebParts = new List<Control>();

        /// <summary>
        /// Zone title.
        /// </summary>
        protected Label lblTitle;

        /// <summary>
        /// Zone container.
        /// </summary>
        protected Panel pnlZone;

        /// <summary>
        /// Zone design mode header container.
        /// </summary>
        protected Panel pnlDesignHeader;

        /// <summary>
        /// Drag and drop extender.
        /// </summary>
        protected DragAndDropExtender extDragDrop;

        /// <summary>
        /// Drop cue panel.
        /// </summary>
        protected Panel pnlCue;


        /// <summary>
        /// Web part container.
        /// </summary>
        protected Control mWebPartContainer;

        /// <summary>
        /// Container for zone variants.
        /// </summary>
        protected Panel mPnlZoneVariants;

        /// <summary>
        /// If this instance is a zone variant then this variable contains the parent zone control (CMSWebPartZone), otherwise is null.
        /// </summary>
        protected Control mParentZoneControl;

        #endregion


        #region "Variables"

        /// <summary>
        /// List of the editable controls located under current placeholder.
        /// </summary>
        protected List<ICMSEditableControl> mCMSEditableControls;

        /// <summary>
        /// List of the property names that should not be resolved with macros.
        /// </summary>
        protected string mNotResolveProperties = ";;";

        /// <summary>
        /// List of the property names that are used in SQL queries and should avoid SQL injection.
        /// </summary>
        protected string mSQLProperties = ";wherecondition;orderby;";

        /// <summary>
        /// True if the zone is orphaned - Not included in layout.
        /// </summary>
        protected bool mOrphaned;

        /// <summary>
        /// Zone view mode.
        /// </summary>
        protected ViewModeEnum mViewMode = ViewModeEnum.Unknown;

        /// <summary>
        /// Zone type.
        /// </summary>
        protected WidgetZoneTypeEnum mWidgetZoneType = WidgetZoneTypeEnum.All;

        /// <summary>
        /// Local web part properties.
        /// </summary>
        protected Hashtable mLocalProperties = new Hashtable();

        /// <summary>
        /// Zone context resolver.
        /// </summary>
        protected MacroResolver mContextResolver;

        /// <summary>
        /// Container info object.
        /// </summary>
        protected WebPartContainerInfo mContainer;

        /// <summary>
        /// Indicates whether the web part has any variants.
        /// </summary>
        private bool? mHasVariants;

        /// <summary>
        /// Indicates whether to render the variant envelope
        /// </summary>
        private bool? mRenderVariantEnvelope;

        /// <summary>
        /// Indicates whether to render the variants
        /// </summary>
        private bool? mRenderVariants;

        /// <summary>
        /// Script to be rendered for current zone
        /// </summary>
        private string mScript;

        /// <summary>
        /// Short client ID.
        /// </summary>
        protected string mShortClientID;

        private bool mAllowModifyWebPartCollection = true;
        private string mWidgetGroup;
        private bool? mWebPartManagementRequired;

        #endregion


        #region "Private properties"

        /// <summary>
        /// Container for zone variants.
        /// </summary>
        protected Panel PnlZoneVariants
        {
            get
            {
                if (mPnlZoneVariants == null)
                {
                    mPnlZoneVariants = new Panel();
                    mPnlZoneVariants.ID = "pnlZoneVariants";
                }
                return mPnlZoneVariants;
            }
        }


        /// <summary>
        /// Indicates whether to render the variant envelope.
        /// </summary>
        private bool RenderVariantEnvelope
        {
            get
            {
                if (!mRenderVariantEnvelope.HasValue)
                {
                    mRenderVariantEnvelope = false;
                    ViewModeEnum viewMode = PortalContext.ViewMode;

                    if ((PortalContext.ViewMode != ViewModeEnum.LiveSite) && (ViewMode != ViewModeEnum.EditNotCurrent) && (ZoneInstance != null))
                    {
                        // Check permissions based on webpart type
                        if ((ZoneInstance.VariantMode == VariantModeEnum.ContentPersonalization)
                            && MembershipContext.AuthenticatedUser.IsAuthorizedPerResource("cms.contentpersonalization", "Read")
                            && PortalContext.ContentPersonalizationEnabled)
                        {
                            // Render the variant envelope only for zones with CP variants in the Edit view mode
                            if (viewMode.IsEdit(true))
                            {
                                mRenderVariantEnvelope = (ZoneInstance.HasVariants && (ZoneInstance.VariantMode == VariantModeEnum.ContentPersonalization));
                            }
                        }
                    }
                }

                return mRenderVariantEnvelope.Value;
            }
        }


        /// <summary>
        /// Indicates whether to render the variants.
        /// </summary>
        private bool RenderVariants
        {
            get
            {
                if (!mRenderVariants.HasValue)
                {
                    mRenderVariants = false;

                    // Check permissions based on webpart type
                    if (ZoneInstance.VariantMode == VariantModeEnum.MVT)
                    {
                        if (MembershipContext.AuthenticatedUser.IsAuthorizedPerResource("cms.mvtest", "Read") && PortalContext.MVTVariantsEnabled)
                        {
                            mRenderVariants = true;
                        }
                    }

                    if (ZoneInstance.VariantMode == VariantModeEnum.ContentPersonalization)
                    {
                        if (MembershipContext.AuthenticatedUser.IsAuthorizedPerResource("cms.contentpersonalization", "Read") && PortalContext.ContentPersonalizationEnabled)
                        {
                            mRenderVariants = true;
                        }
                    }
                }

                return mRenderVariants.Value;
            }
        }


        /// <summary>
        /// Gets the widget group CSS class.
        /// </summary>
        private string WidgetGroup
        {
            get
            {
                if (mWidgetGroup == null)
                {
                    mWidgetGroup = string.Empty;

                    if (WebPartManagementRequired)
                    {
                        switch (WidgetZoneType)
                        {
                            case WidgetZoneTypeEnum.User:
                                mWidgetGroup = "UserWidgets";
                                break;

                            case WidgetZoneTypeEnum.Group:
                                mWidgetGroup = "GroupWidgets";
                                break;

                            case WidgetZoneTypeEnum.Editor:
                                mWidgetGroup = "EditorWidgets";
                                break;

                            case WidgetZoneTypeEnum.Dashboard:
                                mWidgetGroup = "DashboardWidgets";
                                break;
                        }

                        if (!AllowModifyWebPartCollection)
                        {
                            mWidgetGroup += "ModifyNotAllowed";
                        }
                    }
                }

                return mWidgetGroup;
            }
        }

        #endregion


        #region "Basic web part zone properties"

        /// <summary>
        /// Update panel of the web part.
        /// </summary>
        public UpdatePanel UpdatePanel
        {
            get
            {
                return mUpdatePanel;
            }
            internal set
            {
                mUpdatePanel = value;
            }
        }


        /// <summary>
        /// Web part container object.
        /// </summary>
        public WebPartContainerInfo Container
        {
            get
            {
                // Check if the correct container was loaded
                string containerName = ContainerName;
                if ((mContainer == null) || (mContainer.ContainerName.ToLowerCSafe() != containerName.ToLowerCSafe()))
                {
                    // Get the container
                    mContainer = null;
                    if (containerName != "")
                    {
                        mContainer = WebPartContainerInfoProvider.GetWebPartContainerInfo(containerName);
                    }
                }
                return mContainer;
            }
            set
            {
                mContainer = value;
            }
        }


        /// <summary>
        /// Container to render before the control.
        /// </summary>
        public string ContainerBefore
        {
            get
            {
                if (Container != null)
                {
                    return ResolveMacros(Container.ContainerTextBefore);
                }
                return "";
            }
        }


        /// <summary>
        /// Container to render after the control.
        /// </summary>
        public string ContainerAfter
        {
            get
            {
                if (Container != null)
                {
                    return ResolveMacros(Container.ContainerTextAfter);
                }
                return "";
            }
        }


        /// <summary>
        /// Hide container on sub pages.
        /// </summary>
        public virtual bool ContainerHideOnSubPages
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("ContainerHideOnSubPages"), false);
            }
            set
            {
                SetValue("ContainerHideOnSubPages", value);
            }
        }


        /// <summary>
        /// Returns true if the container should be hidden on current page (hide container on subpages in effect).
        /// </summary>
        public virtual bool ContainerHideOnCurrentPage
        {
            get
            {
                // Hide container if whole control should be hidden
                if (HideOnCurrentPage)
                {
                    return true;
                }

                if (mZoneInstance != null)
                {
                    // Check hide container on subpages settings (hide on sub pages, and inherited template or current page info not the last page info)
                    if (!IsVisible || (ContainerHideOnSubPages && IsSubPage()))
                    {
                        return true;
                    }
                }

                return false;
            }
        }


        /// <summary>
        /// Content before.
        /// </summary>
        public virtual string ContentBefore
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ContentBefore"), "");
            }
            set
            {
                SetValue("ContentBefore", value);
            }
        }


        /// <summary>
        /// Content after.
        /// </summary>
        public virtual string ContentAfter
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ContentAfter"), "");
            }
            set
            {
                SetValue("ContentAfter", value);
            }
        }


        /// <summary>
        /// Container title.
        /// </summary>
        public virtual string ContainerTitle
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ContainerTitle"), "");
            }
            set
            {
                SetValue("ContainerTitle", value);
            }
        }


        /// <summary>
        /// Container name.
        /// </summary>
        public virtual string ContainerName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("Container"), "");
            }
            set
            {
                SetValue("Container", value);
            }
        }


        /// <summary>
        /// Returns true if the web part zone is visible.
        /// </summary>
        public virtual bool IsVisible
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("Visible"), true);
            }
            set
            {
                SetValue("Visible", value);
            }
        }


        /// <summary>
        /// Show for document types.
        /// </summary>
        public virtual string ShowForDocumentTypes
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ShowForDocumentTypes"), "");
            }
            set
            {
                SetValue("ShowForDocumentTypes", value);
            }
        }


        /// <summary>
        /// Hide on sub pages.
        /// </summary>
        public virtual bool HideOnSubPages
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("HideOnSubPages"), false);
            }
            set
            {
                SetValue("HideOnSubPages", value);
            }
        }


        /// <summary>
        /// Display to roles.
        /// </summary>
        public virtual string DisplayToRoles
        {
            get
            {
                return ValidationHelper.GetString(GetValue("DisplayToRoles"), "");
            }
            set
            {
                SetValue("DisplayToRoles", value);
            }
        }


        /// <summary>
        /// Use update panel.
        /// </summary>
        public virtual bool UseUpdatePanel
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("UseUpdatePanel"), false);
            }
            set
            {
                SetValue("UseUpdatePanel", value);
            }
        }


        /// <summary>
        /// Zone title.
        /// </summary>
        public virtual string ZoneTitle
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ZoneTitle"), String.Empty);
            }
            set
            {
                SetValue("ZoneTitle", value);
            }
        }


        /// <summary>
        /// Padding
        /// </summary>
        public virtual string Padding
        {
            get
            {
                return ValidationHelper.GetString(GetValue("Padding"), "");
            }
            set
            {
                SetValue("Padding", value);
            }
        }


        /// <summary>
        /// Margin
        /// </summary>
        public virtual string Margin
        {
            get
            {
                return ValidationHelper.GetString(GetValue("Margin"), "");
            }
            set
            {
                SetValue("Margin", value);
            }
        }


        /// <summary>
        /// Align
        /// </summary>
        public virtual string Align
        {
            get
            {
                return ValidationHelper.GetString(GetValue("Align"), "");
            }
            set
            {
                SetValue("Align", value);
            }
        }


        /// <summary>
        /// Width of the web part zone
        /// </summary>
        public virtual string ZoneWidth
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ZoneWidth"), "");
            }
            set
            {
                SetValue("ZoneWidth", value);
            }
        }


        /// <summary>
        /// Height of the web part zone
        /// </summary>
        public virtual string ZoneHeight
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ZoneHeight"), "");
            }
            set
            {
                SetValue("ZoneHeight", value);
            }
        }


        /// <summary>
        /// Web part instance GUID.
        /// </summary>
        public virtual Guid InstanceGUID
        {
            get
            {
                if (ZoneInstance == null)
                {
                    return Guid.Empty;
                }
                else
                {
                    return ZoneInstance.InstanceGUID;
                }
            }
        }


        /// <summary>
        /// Returns the short client ID of the web part.
        /// </summary>
        public virtual string ShortClientID
        {
            get
            {
                if (mShortClientID == null)
                {
                    string shortId;

                    Guid instanceGuid = InstanceGUID;
                    if (instanceGuid != Guid.Empty)
                    {
                        // Build short ID
                        shortId = "wpz_" + instanceGuid.ToString("N");

                        if (IsVariant)
                        {
                            shortId += "_" + ZoneInstance.VariantID;
                        }
                    }
                    else
                    {
                        // Use standard client ID
                        shortId = ClientID;
                    }

                    mShortClientID = shortId;
                }

                return mShortClientID;
            }
        }


        /// <summary>
        /// Returns the client ID of the zone container control
        /// </summary>
        public virtual string ContainerClientID
        {
            get
            {
                if (mWebPartContainer != null)
                {
                    return mWebPartContainer.ClientID;
                }
                else
                {
                    return ShortClientID + "_container";
                }
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
                                control.PageManager = PagePlaceholder;
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


        #region "Public properties"

        /// <summary>
        /// If true, the zone allows adding new web parts
        /// </summary>
        public bool AllowModifyWebPartCollection
        {
            get
            {
                return mAllowModifyWebPartCollection;
            }
            set
            {
                mAllowModifyWebPartCollection = value;
            }
        }


        /// <summary>
        /// Gets or sets the programmatic identifier assigned to the server control.
        /// </summary>
        public override string ID
        {
            get
            {
                if (!String.IsNullOrEmpty(ZoneId))
                {
                    return ZoneId;
                }

                return base.ID;
            }
            set
            {
                base.ID = value;
            }
        }


        /// <summary>
        /// Zone Id used mainly in conditional layouts where zone should be shared in multiple layouts.
        /// </summary>
        public string ZoneId
        {
            get;
            set;
        }


        /// <summary>
        /// If true, then the size of the free layout is adjusted to match the parent size
        /// </summary>
        public bool AdjustFreeLayoutToParent
        {
            get;
            set;
        }


        /// <summary>
        /// Zone layout type
        /// </summary>
        public ZoneLayoutTypeEnum LayoutType
        {
            get;
            set;
        }


        /// <summary>
        /// Conditional layout within which the zone is placed.
        /// </summary>
        public CMSConditionalLayout ConditionalLayout
        {
            get;
            set;
        }


        /// <summary>
        /// True if the zone is generated by web part layout.
        /// </summary>
        public bool LayoutZone
        {
            get;
            set;
        }


        /// <summary>
        /// Returns true if the control should be hidden on current page (hide control on subpages in effect).
        /// </summary>
        public virtual bool HideOnCurrentPage
        {
            get
            {
                // Check for the Design Mode in Visual Studio
                if (Context == null)
                {
                    return false;
                }

                if (mZoneInstance != null)
                {
                    // Hide in safe mode
                    if (PortalContext.IsDesignMode(PagePlaceholder.ViewMode) && PortalHelper.SafeMode)
                    {
                        return true;
                    }

                    // Check hide on subpages settings (hide on sub pages, and inherited template or current page info not the last page info)
                    if (!IsVisible || (HideOnSubPages && IsSubPage()))
                    {
                        return true;
                    }
                }
                else
                {
                    // Outside of portal engine - Hide if not visible
                    if (Visible == false)
                    {
                        return true;
                    }
                }

                // Check display to roles
                if (DisplayToRoles.Trim().Trim(';') != "")
                {
                    // Check global administrator permissions
                    var currentUser = MembershipContext.AuthenticatedUser;

                    bool show = false;
                    string[] roles = DisplayToRoles.Split(';');

                    // Check the roles
                    foreach (string role in roles)
                    {
                        if (((role.ToLowerCSafe() != RoleName.NOTAUTHENTICATED) && currentUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin)) || currentUser.IsInRole(role, SiteContext.CurrentSiteName))
                        {
                            show = true;
                            break;
                        }
                    }

                    if (!show)
                    {
                        return true;
                    }
                }

                PageInfo currentPage = DocumentContext.CurrentPageInfo;
                // Check show for document types condition
                string classNames = ShowForDocumentTypes;
                if (classNames != "")
                {
                    bool classMatch = false;
                    string[] classes = classNames.ToLowerCSafe().Split(';');
                    string currentClass = currentPage.ClassName.ToLowerCSafe(); // PagePlaceholder.PageInfo.ClassName.ToLowerCSafe()
                    foreach (string className in classes)
                    {
                        if (className == currentClass)
                        {
                            classMatch = true;
                            break;
                        }
                    }
                    if (!classMatch)
                    {
                        return true;
                    }
                }

                return false;
            }
        }


        /// <summary>
        /// List of the property names that should not be resolved with macros.
        /// </summary>
        public virtual string NotResolveProperties
        {
            get
            {
                return mNotResolveProperties;
            }
            set
            {
                if (value == null)
                {
                    value = "";
                }
                mNotResolveProperties = ";" + value.ToLowerCSafe().Trim(';') + ";";
            }
        }


        /// <summary>
        /// List of the property names that are used in SQL queries and should avoid SQL injection.
        /// </summary>
        public virtual string SQLProperties
        {
            get
            {
                return mSQLProperties;
            }
            set
            {
                if (value == null)
                {
                    value = "";
                }
                mSQLProperties = ";" + value.ToLowerCSafe().Trim(';') + ";";
            }
        }


        /// <summary>
        /// Web part zone context resolver.
        /// </summary>
        public virtual MacroResolver ContextResolver
        {
            get
            {
                if (mContextResolver == null)
                {
                    mContextResolver = MacroContext.CurrentResolver.CreateChild();
                    if (PagePlaceholder != null)
                    {
                        mContextResolver.Settings.RelatedObject = PagePlaceholder.TemplateInstance;
                    }

                    mContextResolver.SetNamedSourceData("UIContext", UIContextHelper.GetUIContext(this));

                    // Set the handler
                    mContextResolver.OnGetValue += GetValue;
                }
                return mContextResolver;
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
                    return null;
                }

                if (mPagePlaceholder == null)
                {
                    mPagePlaceholder = PortalHelper.FindParentPlaceholder(this);
                    if (mPagePlaceholder == null)
                    {
                        throw new Exception("[CMSWebPartZone.PagePlaceholder]: Parent CMSPagePlaceholder not found.");
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
        /// Parent portal manager.
        /// </summary>
        public virtual CMSPortalManager PortalManager
        {
            get
            {
                // Check for the Design Mode in Visual Studio
                if (Context == null)
                {
                    return null;
                }

                return PagePlaceholder.PortalManager;
            }
            set
            {
                PagePlaceholder.PortalManager = value;
            }
        }


        /// <summary>
        /// List of WebParts contained within the zone.
        /// </summary>
        public virtual List<Control> WebParts
        {
            get
            {
                return mWebParts;
            }
        }


        /// <summary>
        /// Web part zone instance object.
        /// </summary>
        public virtual WebPartZoneInstance ZoneInstance
        {
            get
            {
                if (mZoneInstance == null)
                {
                    mZoneInstance = new WebPartZoneInstance();
                    mZoneInstance.ZoneID = ID;
                }
                return mZoneInstance;
            }
        }


        /// <summary>
        /// Widget zone type.
        /// </summary>
        public virtual WidgetZoneTypeEnum WidgetZoneType
        {
            get
            {
                if (mWidgetZoneType == WidgetZoneTypeEnum.All)
                {
                    if (mZoneInstance == null)
                    {
                        // If no zone instance available, return default zone without widgets
                        return WidgetZoneTypeEnum.None;
                    }
                    else
                    {
                        // Get the zone type from the instance
                        mWidgetZoneType = mZoneInstance.WidgetZoneType;
                    }
                }
                return mWidgetZoneType;
            }
            set
            {
                mWidgetZoneType = value;
            }
        }


        /// <summary>
        /// Web part container control.
        /// </summary>
        public Control WebPartContainer
        {
            get
            {
                if (mWebPartContainer != null)
                {
                    return mWebPartContainer;
                }
                else
                {
                    return this;
                }
            }
            set
            {
                mWebPartContainer = value;
            }
        }


        /// <summary>
        /// Page mode of the current zone.
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

                if (mViewMode == ViewModeEnum.Unknown)
                {
                    mViewMode = GetCurrentViewmode();
                }

                return mViewMode;
            }
            set
            {
                mViewMode = value;
            }
        }


        /// <summary>
        /// Indicates whether this web part is a variant of an existing zone.
        /// </summary>
        public bool IsVariant
        {
            get
            {
                return ZoneInstance.IsVariant;
            }
        }


        /// <summary>
        /// Indicates whether the zone has any variants.
        /// </summary>
        public bool HasVariants
        {
            get
            {
                if (!mHasVariants.HasValue)
                {
                    mHasVariants = (ZoneInstance.ZoneInstanceVariants != null) && (ZoneInstance.ZoneInstanceVariants.Count > 0);
                }

                return mHasVariants.Value;
            }
        }


        /// <summary>
        /// Returns true if the children components have any variants
        /// </summary>
        public virtual bool ChildrenHaveVariants
        {
            get
            {
                foreach (Control ctrl in WebParts)
                {
                    // Get control instance
                    CMSAbstractWebPart part = PortalHelper.GetWebPartControl(ctrl);
                    if ((part != null) && (part.HasVariants || part.ChildrenHaveVariants))
                    {
                        // If web part has variants, return true
                        return true;
                    }
                }

                return false;
            }
        }


        /// <summary>
        /// Returns true if the parent component has any variants
        /// </summary>
        public virtual bool ParentHasVariants
        {
            get
            {
                if (ParentWebPart == null)
                {
                    return false;
                }

                return ParentWebPart.HasVariants || ParentWebPart.ParentHasVariants;
            }
        }


        /// <summary>
        /// Parent portal component
        /// </summary>
        public CMSAbstractLayoutWebPart ParentWebPart
        {
            get;
            set;
        }


        /// <summary>
        /// Gets the parent zone control (CMSWebPartZone) if this instance is a zone variant. Otherwise returns null.
        /// </summary>
        public Control ParentZoneControl
        {
            get
            {
                return mParentZoneControl;
            }
            set
            {
                mParentZoneControl = value;
            }
        }


        /// <summary>
        /// Gets the zone title label.
        /// </summary>
        public Label TitleLabel
        {
            get
            {
                return lblTitle;
            }
        }


        /// <summary>
        /// Returns true if the zone is empty (no web part is visible)
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                // Check all web parts for visibility
                foreach (Control ctrl in WebParts)
                {
                    if ((ctrl is CMSAbstractWebPart webPart) && (webPart.WebPartType == WebPartTypeEnum.DataSource))
                    {
                        continue;
                    }

                    if (ctrl.Visible)
                    {
                        return false;
                    }
                }

                return true;
            }
        }


        /// <summary>
        /// Control to which the zone header should be rendered
        /// </summary>
        public Control HeaderContainer
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the type of the zone. This value is processed only in the UI pages.
        /// </summary>
        public virtual WebPartZoneTypeEnum ZoneType
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether the web part management support is required.
        /// </summary>
        public bool WebPartManagementRequired
        {
            get
            {
                if (!mWebPartManagementRequired.HasValue)
                {
                    var vm = ViewMode;

                    if (PortalContext.IsDesignMode(vm))
                    {
                        mWebPartManagementRequired = true;
                    }
                    else
                    {
                        switch (vm)
                        {
                            case ViewModeEnum.Edit:
                            case ViewModeEnum.EditLive:
                            case ViewModeEnum.GroupWidgets:
                            case ViewModeEnum.UserWidgets:
                            case ViewModeEnum.UserWidgetsDisabled:
                            case ViewModeEnum.DashboardWidgets:
                                mWebPartManagementRequired = true;
                                break;

                            case ViewModeEnum.EditDisabled:
                                mWebPartManagementRequired = (WidgetZoneType == WidgetZoneTypeEnum.Editor);
                                break;

                            default:
                                mWebPartManagementRequired = false;
                                break;
                        }
                    }
                }

                return mWebPartManagementRequired.Value;
            }
        }

        #endregion


        #region "Overridden control methods"

        /// <summary>
        /// Init action.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            if (PortalContext.IsDesignMode(PagePlaceholder.ViewMode))
            {
                try
                {
                    base.OnInit(e);
                }
                catch (Exception ex)
                {
                    EventLogProvider.LogException("Design", "Zone", ex);
                }
            }
            else
            {
                base.OnInit(e);
            }
        }


        /// <summary>
        /// PreRender action.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            // Register the layout script
            if (LayoutType == ZoneLayoutTypeEnum.Free)
            {
                ScriptHelper.RegisterJQuery(Page);
                PortalHelper.RegisterLayoutsScript(Page);

                mScript = String.Format("InitAutoResize('{0}', 'WebPart', {1});", WebPartContainer.ClientID, AdjustFreeLayoutToParent.ToString().ToLowerCSafe());
            }

            if (PortalContext.IsDesignMode(PagePlaceholder.ViewMode))
            {
                try
                {
                    base.OnPreRender(e);
                }
                catch (Exception ex)
                {
                    EventLogProvider.LogException("Design", "Zone", ex);
                }
            }
            else
            {
                base.OnPreRender(e);
            }

            // Register the container
            if (!ContainerHideOnCurrentPage)
            {
                PortalContext.CurrentComponents.RegisterWebPartContainer(Container);
            }
        }


        /// <summary>
        /// Load action.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            if (PortalContext.IsDesignMode(PagePlaceholder.ViewMode))
            {
                try
                {
                    base.OnLoad(e);

                    // Reload the data for variants that have not been loaded yet (i.e. web parts working with data sources)
                    if (IsVariant)
                    {
                        ReloadData();
                    }
                }
                catch (Exception ex)
                {
                    EventLogProvider.LogException("Design", "Zone", ex);
                }
            }
            else
            {
                base.OnLoad(e);
            }
        }


        /// <summary>
        /// Indicates whether the zone should render its header. Based on the view mode and zone type.
        /// </summary>
        internal bool ZoneHeaderVisible
        {
            get
            {
                if (PortalContext.IsDesignMode(ViewMode))
                {
                    // Display zone header in the design mode
                    return true;
                }
                else
                {
                    // Check how the zone should be rendered
                    switch (ViewMode)
                    {
                        case ViewModeEnum.Edit:
                        case ViewModeEnum.EditLive:
                        case ViewModeEnum.GroupWidgets:
                        case ViewModeEnum.UserWidgets:
                        case ViewModeEnum.UserWidgetsDisabled:
                        case ViewModeEnum.DashboardWidgets:
                            // Widget mode rendering = design + hide availability
                            return !HideOnCurrentPage;

                        case ViewModeEnum.EditDisabled:
                            // Widget mode rendering = design + hide availability
                            if (WidgetZoneType == WidgetZoneTypeEnum.Editor)
                            {
                                return !HideOnCurrentPage;
                            }

                            break;
                    }
                }

                return false;
            }
        }


        /// <summary>
        /// Indicates whether the zone should render its content. Based on the visibility settings and view mode.
        /// </summary>
        internal bool ZoneIsVisible
        {
            get
            {
                if (ZoneHeaderVisible)
                {
                    // Zone is visible when it renders its zone header
                    return true;
                }

                // Check how the zone should be rendered
                switch (ViewMode)
                {
                    case ViewModeEnum.EditDisabled:
                        // Widget mode rendering = design + hide availability
                        if (WidgetZoneType != WidgetZoneTypeEnum.Editor)
                        {
                            return !HideOnCurrentPage;
                        }

                        break;

                    default:
                        // Standard rendering
                        return !HideOnCurrentPage;
                }

                return false;
            }
        }

        /// <summary>
        /// Render action.
        /// </summary>
        protected override void Render(HtmlTextWriter writer)
        {
            if (GeneratePreviewAnchor())
            {
                writer.AddAttribute("name", "previewanchor");
                writer.AddAttribute("style", "visibility:hidden");
                writer.RenderBeginTag("a");
                writer.Write("anchor");
                writer.RenderEndTag(); // a
            }

            // Get the current view mode (it could have been changed when changing workflow steps) if it's required
            if ((PortalManager != null) && (PortalManager.ReloadViewMode))
            {
                ViewMode = GetCurrentViewmode();
            }

            // Render the zone
            if (ZoneHeaderVisible)
            {
                RenderZoneWithHeader(writer);
            }
            else if (ZoneIsVisible)
            {
                RenderZoneContent(writer);
            }

            if (!String.IsNullOrEmpty(mScript))
            {
                // Render the zone script
                writer.Write(ScriptHelper.GetScript(mScript));
            }
        }


        /// <summary>
        /// Returns true if preview anchor to be generated
        /// </summary>
        private bool GeneratePreviewAnchor()
        {
            String previewObjectName = QueryHelper.GetString("previewobjectname", String.Empty);
            if (previewObjectName != String.Empty)
            {
                String previewObjectIdentifier = QueryHelper.GetString("previewobjectidentifier", String.Empty);
                if (previewObjectIdentifier != String.Empty)
                {
                    // Compare identifier based on object name
                    switch (previewObjectName.ToLowerCSafe())
                    {
                        case WebPartContainerInfo.OBJECT_TYPE:
                            return ((Container != null) && (Container.ContainerName == previewObjectIdentifier));
                    }
                }
            }
            return false;
        }


        /// <summary>
        /// Renders the zone in the design mode
        /// </summary>
        /// <param name="writer">HTML writer for output</param>
        private void RenderZoneWithHeader(HtmlTextWriter writer)
        {
            bool denyVariants = false;
            string script = null;

            // Render the variant envelope
            if (RenderVariants && HasVariants)
            {
                denyVariants = true;

                // Create envelopes around zone variants
                foreach (CMSWebPartZone zone in PnlZoneVariants.Controls)
                {
                    zone.Controls.AddAt(0, new LiteralControl("<div id=\"Variant_" + VariantModeFunctions.GetVariantModeString(ZoneInstance.VariantMode) + "_" + zone.ZoneInstance.VariantID + "\" class=\"CMSVariant\" >"));
                    zone.Controls.Add(new LiteralControl("</div>"));
                }
            }

            if (LayoutZone && ParentHasVariants)
            {
                // Add the script only when MVP or Content personalization is enabled
                if ((PortalContext.MVTVariantsEnabled || PortalContext.ContentPersonalizationEnabled)
                    && (MembershipContext.AuthenticatedUser.IsAuthorizedPerResource("cms.mvtest", "Read")
                        || MembershipContext.AuthenticatedUser.IsAuthorizedPerResource("cms.contentpersonalization", "Read")))
                {
                    // Do not allow variants under the layout zone
                    script += "zonesWithVariantsDenied.push('#" + ZoneInstance.ZoneID + "');";

                    denyVariants = true;
                }
            }

            if (denyVariants)
            {
                script += "zonesWithVariantsDenied.push('" + ZoneInstance.ZoneID + "');";
            }

            // Add this zone to the list banned for variants
            if (!String.IsNullOrEmpty(script))
            {
                writer.Write(ScriptHelper.GetScript(script));
            }

            // Design mode / edit mode ... controlled rendering to avoid unhandled exceptions
            try
            {
                if (!IsVariant)
                {
                    // Render the start of the container
                    RenderContainerStart(writer);

                    writer.Write(ScriptHelper.GetScript(String.Format(
                        "webPartZoneLocation['{0}_pnlWebParts'] = new zoneProperties('{1}', '{2}', '{3}', '{4}', {5})",
                        ClientID,
                        ZoneInstance.ZoneID,
                        PagePlaceholder.PageInfo.NodeAliasPath,
                        PagePlaceholder.PageInfo.GetUsedPageTemplateId(),
                        WidgetZoneType.ToStringRepresentation(),
                        LayoutZone.ToString().ToLowerCSafe()
                    )));

                    // Border
                    RenderBorderStart(writer);
                }
                else
                {
                    // Set parent zone location info to its variant as well
                    writer.Write(ScriptHelper.GetScript(String.Format(
                        "webPartZoneLocation['{0}_pnlWebParts'] = webPartZoneLocation['{1}_pnlWebParts'];",
                        ClientID,
                        ParentZoneControl.ClientID
                    )));
                }

                // Render the zone content
                RenderZone(writer, false);

                if (!IsVariant)
                {
                    writer.RenderEndTag(); // Border

                    // Clear both
                    writer.AddAttribute("class", "ClearBoth");
                    writer.RenderBeginTag("div");
                    //writer.Write("&nbsp;");

                    writer.RenderEndTag(); // Clear both


                    writer.RenderEndTag(); // Container
                }
            }
            catch (Exception ex)
            {
                writer.Write("[CMSWebPartZone.Render]: " + ex.Message);

                EventLogProvider.LogException("Design", "Zone", ex);
            }
        }


        /// <summary>
        /// Renders the start of the zone border
        /// </summary>
        /// <param name="writer">Writer for the output</param>
        private void RenderBorderStart(HtmlTextWriter writer)
        {
            writer.AddAttribute("id", ClientID + "_border");
            writer.AddAttribute("class", "WebPartZoneBorder");
            switch (ViewMode)
            {
                case ViewModeEnum.Edit:
                case ViewModeEnum.EditLive:
                case ViewModeEnum.GroupWidgets:
                case ViewModeEnum.UserWidgets:
                case ViewModeEnum.UserWidgetsDisabled:
                case ViewModeEnum.DashboardWidgets:
                    if ((PortalManager != null) && (PortalManager.ActivateZoneBorder))
                    {
                        writer.AddAttribute("onmouseover", "ActivateBorder('" + ClientID + "');");
                        writer.AddAttribute("onmouseout", "DeactivateBorder('" + ClientID + "', 200);");
                    }
                    break;
            }

            writer.RenderBeginTag("div"); // border
        }


        /// <summary>
        /// Renders the start of the zone container
        /// </summary>
        /// <param name="writer">Writer for the output</param>
        private void RenderContainerStart(HtmlTextWriter writer)
        {
            // Container
            writer.AddAttribute("id", ShortClientID + "_container");
            string containerClass = "WebPartZone WebPartZone_" + ID;

            if (PortalContext.IsDesignMode(ViewMode))
            {
                containerClass += " StandardZone";
            }
            else
            {
                switch (ViewMode)
                {
                    case ViewModeEnum.UserWidgetsDisabled:
                        containerClass += " UserWidgetZone UserWidgetZoneDisabled";
                        break;

                    case ViewModeEnum.UserWidgets:
                        containerClass += " UserWidgetZone";
                        break;

                    case ViewModeEnum.GroupWidgets:
                        containerClass += " GroupWidgetZone";
                        break;

                    case ViewModeEnum.Edit:
                    case ViewModeEnum.EditLive:
                        containerClass += " EditorWidgetZone";
                        break;

                    case ViewModeEnum.EditDisabled:
                        containerClass += " EditorWidgetZoneDisabled";
                        break;

                    case ViewModeEnum.DashboardWidgets:
                        containerClass += " DashboardWidgetZone";
                        break;
                }
            }

            // Zone CSS class
            if (!String.IsNullOrEmpty(CssClass))
            {
                containerClass = CssClass + " " + containerClass;
            }

            writer.AddAttribute("class", containerClass);

            writer.RenderBeginTag("div"); // container
        }


        /// <summary>
        /// Renders the zone in a standard way
        /// </summary>
        /// <param name="writer">HTML writer for output</param>
        private void RenderZoneContent(HtmlTextWriter writer)
        {
            // Render the variant envelope
            if (RenderVariantEnvelope)
            {
                // Zone border
                writer.AddAttribute("id", ShortClientID + "_border");
                writer.AddAttribute("class", "WebPartZoneBorder");
                writer.AddAttribute("onmouseover", "ActivateBorder('" + ShortClientID + "', this);");
                writer.AddAttribute("onmouseout", "DeactivateBorder('" + ShortClientID + "', 50);");
                writer.RenderBeginTag("div"); // Zone border

                // Zone container
                writer.AddAttribute("id", ShortClientID + "_Container");
                writer.AddAttribute("class", "CPContainer");
                writer.RenderBeginTag("div"); // Zone container

                // Zone menu container
                writer.AddAttribute("id", ShortClientID + "_MenuContainer");
                writer.RenderBeginTag("div"); // Zone menu container

                // Menu Border
                writer.AddAttribute("id", "pnlHeader" + ShortClientID);
                writer.AddAttribute("class", "CPMenuBorder");
                writer.RenderBeginTag("div"); // Menu Border

                // Prepare the parameters
                string parameter = GetContextMenuParameter();

                writer.Write(ScriptHelper.GetScript(String.Format(
                    "webPartZoneLocation['{0}_pnlWebParts'] = new zoneProperties('{1}', '{2}', '{3}', '{4}', {5})",
                    ShortClientID,
                    ZoneInstance.ZoneID,
                    PagePlaceholder.PageInfo.NodeAliasPath,
                    PagePlaceholder.PageInfo.GetUsedPageTemplateId(),
                    WidgetZoneType.ToStringRepresentation(),
                    LayoutZone.ToString().ToLowerCSafe()
                )));

                // Button
                writer.AddAttribute("id", "pnlCPBtn");
                writer.AddAttribute("class", "CPMenuZone");
                writer.AddAttribute("onclick", "currentContextMenuId = '" + ShortClientID + "'; ContextMenu('cpVariantList', parentNode, GetVariantInfoArray(" + parameter + ").join(), true); return false;");
                writer.RenderBeginTag("div"); // Button

                // Image
                writer.AddAttribute("id", "imgVarList");
                writer.AddAttribute("src", GetImageUrl("CMSModules/CMS_PortalEngine/Menu.png"));
                writer.AddAttribute("class", "CPMenuButton");
                writer.RenderBeginTag("img"); // Image
                writer.RenderEndTag();

                writer.RenderEndTag(); // Button
                writer.RenderEndTag(); // Menu Border
                writer.RenderEndTag(); // Zone menu container
            }

            // Render the zone itself
            RenderZone(writer, true);

            // End of the variant envelope
            if (RenderVariantEnvelope)
            {
                writer.RenderEndTag(); // Zone container

                writer.RenderEndTag(); // Zone border
            }
        }


        /// <summary>
        /// Renders the zone
        /// </summary>
        /// <param name="writer">Writer to which the zone will be rendered</param>
        /// <param name="renderEnvelope">If true, the envelope components will be rendered</param>
        private void RenderZone(HtmlTextWriter writer, bool renderEnvelope)
        {
            string after = null;

            if (renderEnvelope)
            {
                // Envelope start
                string before;

                GetEnvelope(false, out before, out after);

                writer.Write(before);
            }

            // Render the child controls (not the envelope)
            RenderChildren(writer);

            if (renderEnvelope)
            {
                // Envelope end
                writer.Write(after);
            }
        }


        /// <summary>
        /// Returns the envelope for the control
        /// </summary>
        /// <param name="webPartManagement">True if the web part management is enabled</param>
        /// <param name="before">Content that should be placed before the control</param>
        /// <param name="after">Content that should be placed after the control</param>
        private void GetEnvelope(bool webPartManagement, out string before, out string after)
        {
            // Envelope before
            StringBuilder beforeSb = new StringBuilder();

            // Envelope after
            StringBuilder afterSb = new StringBuilder();

            bool hideContainer = ContainerHideOnCurrentPage;

            // Render the outer envelope
            string outerEnvelope = GetOuterEnvelope(webPartManagement);
            beforeSb.Append(outerEnvelope);

            // Container before
            if ((mZoneInstance != null) && !hideContainer)
            {
                beforeSb.Append(ContainerBefore);
            }

            // Render the inner envelope
            string innerEnvelope = GetInnerEnvelope();
            beforeSb.Append(innerEnvelope);

            // Content before
            beforeSb.Append(ContentBefore);


            // Handle the layout type
            if (!webPartManagement)
            {
                switch (LayoutType)
                {
                    case ZoneLayoutTypeEnum.Free:
                        // Add the web parts envelope
                        {
                            beforeSb.AppendFormat("<div id=\"{0}\">", WebPartContainer.ClientID);
                            afterSb.Append("</div>");
                        }
                        break;
                }
            }


            // Content after
            afterSb.Append(ContentAfter);

            // End of the inner style
            if (innerEnvelope != null)
            {
                afterSb.Append("</div>");
            }

            // Container after
            if ((mZoneInstance != null) && !hideContainer)
            {
                afterSb.Append(ContainerAfter);
            }

            // End of the outer style
            if (outerEnvelope != null)
            {
                afterSb.Append("</div>");
            }

            before = beforeSb.ToString();
            after = afterSb.ToString();
        }


        /// <summary>
        /// Gets the inner envelope for the zone
        /// </summary>
        private string GetInnerEnvelope()
        {
            string innerStyle = null;

            // Padding
            string padding = Padding;
            if (!String.IsNullOrEmpty(padding))
            {
                innerStyle += "padding: " + padding + ";";
            }

            // Render the inner style
            string innerEnvelope = null;
            if (innerStyle != null)
            {
                innerEnvelope = String.Format("<div style=\"{0}\">", innerStyle);
            }
            return innerEnvelope;
        }


        /// <summary>
        /// Gets the outer envelope for the zone
        /// </summary>
        /// <param name="webPartManagement">True if the web part management is enabled</param>
        private string GetOuterEnvelope(bool webPartManagement)
        {
            string outerStyle = null;

            // Margin
            string margin = Margin;
            if (!String.IsNullOrEmpty(margin))
            {
                outerStyle += "margin:" + margin + ";";
            }

            // Align
            string align = Align;
            if (!String.IsNullOrEmpty(Align))
            {
                outerStyle += "text-align:" + align + ";";
            }

            if (!webPartManagement)
            {
                // Width
                string width = ZoneWidth;
                if (!String.IsNullOrEmpty(width))
                {
                    outerStyle += "min-width:" + width + ";";
                }

                // Height
                string height = ZoneHeight;
                if (!String.IsNullOrEmpty(height))
                {
                    outerStyle += "min-height:" + height + ";";
                }
            }

            // Render the outer style
            string outerEnvelope = null;
            if (outerStyle != null)
            {
                outerEnvelope = String.Format("<div style=\"{0}\">", outerStyle);
            }
            return outerEnvelope;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor.
        /// </summary>
        public CMSWebPartZone()
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        public CMSWebPartZone(bool orphaned)
            : this()
        {
            mOrphaned = orphaned;
        }


        /// <summary>
        /// Creates the container where all web parts will be loaded into. 
        /// Generates Zone header if required.
        /// </summary>
        private void EnsureWebPartsContainer()
        {
            EnsureChildControls();

            Control mainControl = this;

            // Create an update panel around
            if (UseUpdatePanel)
            {
                // Register current control as async postback control
                var sm = ScriptManager.GetCurrent(Page);

                sm.RegisterAsyncPostBackControl(this);

                mUpdatePanel = new CMSUpdatePanel();
                mUpdatePanel.ID = "sys_pnlUpdate";
                mUpdatePanel.UpdateMode = UpdatePanelUpdateMode.Conditional;

                Controls.Add(mUpdatePanel);
                mainControl = mUpdatePanel.ContentTemplateContainer;
                WebPartContainer = mainControl;
            }

            // Get the design mode settings
            ViewModeEnum viewMode = ViewMode;
            bool fullDesignMode = PortalContext.IsDesignMode(viewMode);

            // Define web part management controls
            if (WebPartManagementRequired)
            {
                // Zone panel
                pnlZone = new Panel();
                pnlZone.ID = "pnlZone";

                if (IsVariant)
                {
                    pnlZone.ID += ZoneInstance.VariantID;
                }

                mainControl.Controls.Add(pnlZone);

                // Register the dialog script
                ScriptHelper.RegisterDialogScript(Page);

                // Full header
                LoadFullHeader(viewMode, fullDesignMode);

                // Insert zone variant DIV envelope
                if (HasVariants)
                {
                    pnlZone.Controls.Add(new LiteralControl("<div id=\"Variant_Zone_" + ZoneInstance.ZoneID + "\" >"));
                }

                // Render the envelope
                string before;
                string after;

                GetEnvelope(WebPartManagementRequired, out before, out after);

                if (before != null)
                {
                    pnlZone.Controls.Add(new LiteralControl(before));
                }

                // Add container for web parts
                Panel pnlWebParts = new Panel
                {
                    ID = "pnlWebParts"
                };

                // Add widget group to the container class
                string containerClass = "WebPartZoneContent";
                if (!string.IsNullOrEmpty(WidgetGroup))
                {
                    containerClass += " " + WidgetGroup;
                }
                if (LayoutType == ZoneLayoutTypeEnum.Free)
                {
                    containerClass += " FreeLayout";
                }

                pnlWebParts.CssClass = containerClass;

                // Set width and height
                string height = ZoneHeight;
                if (!String.IsNullOrEmpty(height))
                {
                    pnlWebParts.Height = new Unit(height);
                }

                string width = ZoneWidth;
                if (!String.IsNullOrEmpty(width))
                {
                    pnlWebParts.Width = new Unit(width);
                }


                bool loadVariants = fullDesignMode && RenderVariants && (ZoneInstance.ZoneInstanceVariants != null) && (ZoneInstance.ParentTemplateInstance != null);

                // Load the content for the web part variants
                if (loadVariants)
                {
                    foreach (WebPartZoneInstance instance in ZoneInstance.ZoneInstanceVariants)
                    {
                        CMSWebPartZone zone = new CMSWebPartZone();
                        zone.PagePlaceholder = PagePlaceholder;
                        zone.Page = Page;
                        zone.LoadWebParts(instance);
                        zone.ParentZoneControl = this;
                        PnlZoneVariants.Controls.Add(zone);
                    }

                    pnlZone.Controls.Add(pnlWebParts);
                }
                else
                {
                    pnlZone.Controls.Add(pnlWebParts);
                }

                // Add the end of envelope
                if (after != null)
                {
                    pnlZone.Controls.Add(new LiteralControl(after));
                }

                // Close zone variant DIV envelope
                if (HasVariants)
                {
                    pnlZone.Controls.Add(new LiteralControl("</div>"));
                }

                // Load the content for the web part variants
                if (loadVariants)
                {
                    // Add the variants
                    pnlZone.Controls.Add(PnlZoneVariants);
                }

                WebPartContainer = pnlWebParts;
            }
            else if (!UseUpdatePanel)
            {
                WebPartContainer = this;
            }
        }


        /// <summary>
        /// Finishes setting up the web parts container.
        /// Loads the closing controls of the container.
        /// </summary>
        private void FinalizeWebPartsContainer()
        {
            // Add Drag and Drop extender
            if (WebPartManagementRequired && (ViewMode != ViewModeEnum.UserWidgetsDisabled) && (ViewMode != ViewModeEnum.DesignDisabled))
            {
                SetupDragDrop(WidgetGroup);
            }

            // Close the floating layout
            switch (LayoutType)
            {
                case ZoneLayoutTypeEnum.FloatLeft:
                case ZoneLayoutTypeEnum.FloatRight:
                    WebPartContainer.Controls.Add(new LiteralControl("<div class=\"ClearBoth\"></div>"));
                    break;
            }
        }


        /// <summary>
        /// Sets up the drag and drop extender
        /// </summary>
        /// <param name="widgetGroup">Widget group name</param>
        private void SetupDragDrop(string widgetGroup)
        {
            // Drop cue
            pnlCue = new Panel();
            pnlCue.ID = "pnlCue";

            WebPartContainer.Controls.Add(pnlCue);

            pnlCue.Controls.Add(new LiteralControl("&nbsp;"));

            // Drag and Drop extender
            extDragDrop = new DragAndDropExtender();
            extDragDrop.ID = "extDragDrop";
            extDragDrop.TargetControlID = WebPartContainer.ID;
            extDragDrop.ItemGroup = widgetGroup;
            extDragDrop.DropCueID = pnlCue.ID;
            extDragDrop.DragItemHandleClass = "WebPartHandle";
            extDragDrop.DragItemClass = "WebPart";
            extDragDrop.OnClientDrop = "OnDropWebPart";
            extDragDrop.OnClientBeforeDrop = "OnBeforeDropWebPart";

            // Allow drag and drop highlight if set
            if (PortalManager != null)
            {
                extDragDrop.HighlightDropableAreas = PortalManager.HighlightDropableAreas;
            }

            string cueClass = "WebPartZoneCue";

            // Float settings for cue
            switch (LayoutType)
            {
                case ZoneLayoutTypeEnum.FloatLeft:
                    // Float left
                    {
                        cueClass += " CueLFloat";
                    }
                    break;

                case ZoneLayoutTypeEnum.FloatRight:
                    // Float right
                    {
                        cueClass += " CueRFloat";
                    }
                    break;

                case ZoneLayoutTypeEnum.Free:
                    // Flow layout
                    {
                        // Absolute position
                        cueClass += " CueFree";

                        extDragDrop.FlowLayout = true;
                    }
                    break;
            }

            pnlCue.CssClass = cueClass;

            WebPartContainer.Controls.Add(extDragDrop);
        }


        /// <summary>
        /// Creates script call for new widget.
        /// </summary>
        /// <param name="aliasPath">Zone's alias path</param>
        /// <param name="templateId">Zone's template ID</param>
        /// <param name="zoneType">Zone's type</param>
        private String CreateNewWigdetScript(String aliasPath, int templateId, String zoneType)
        {
            return String.Format("NewWidget({0}); return false;", CreateNewJsZoneObject(aliasPath, templateId, zoneType));
        }


        /// <summary>
        /// Creates JS zone object
        /// </summary>
        /// <param name="aliasPath">Zone's alias path</param>
        /// <param name="templateId">Zone's template ID</param>
        /// <param name="zoneType">Zone's type</param>
        private String CreateNewJsZoneObject(String aliasPath, int templateId, String zoneType)
        {
            return "new zoneProperties('" + ID + "', '" + aliasPath + "', '" + templateId + "', '" + zoneType + "','" + LayoutZone.ToString().ToLowerCSafe() + "')";
        }


        /// <summary>
        /// Creates JS configuration script
        /// </summary>
        /// <param name="aliasPath">Zone's alias path</param>
        /// <param name="templateId">Zone's template ID</param>
        /// <param name="zoneType">Zone's type</param>
        private String CreateConfigureScript(String aliasPath, int templateId, String zoneType)
        {
            return String.Format("ConfigureWebPartZone({0}); return false;", CreateNewJsZoneObject(aliasPath, templateId, zoneType));
        }


        /// <summary>
        /// Loads the full header of the zone
        /// </summary>
        /// <param name="viewMode">View mode</param>
        /// <param name="fullDesignMode">Use full design mode</param>
        private void LoadFullHeader(ViewModeEnum viewMode, bool fullDesignMode)
        {
            // Standard behavior
            if (!IsVariant)
            {
                // Create the header panel
                pnlDesignHeader = new Panel();
                pnlDesignHeader.ID = "pnlHeader";

                if (HeaderContainer != null)
                {
                    HeaderContainer.Controls.Add(pnlDesignHeader);
                }
                else
                {
                    pnlZone.Controls.Add(pnlDesignHeader);
                }

                // Prepare the parameters
                string aliasPath = PagePlaceholder.PageInfo.NodeAliasPath;
                int templateId = PagePlaceholder.PageInfo.GetUsedPageTemplateId();

                string zoneType = WidgetZoneType.ToStringRepresentation();

                // Add the header
                if (fullDesignMode)
                {
                    // Create the header handle
                    pnlDesignHeader.CssClass = mOrphaned ? "WebPartZoneHeaderOrphaned" : "WebPartZoneHeader";
                    pnlDesignHeader.CssClass += " cms-bootstrap";

                    pnlDesignHeader.Attributes.Add("onmouseover", "ActivateBorder('" + ShortClientID + "');");
                    pnlDesignHeader.Attributes.Add("onmouseout", "DeactivateBorder('" + ShortClientID + "', 200);");

                    // Configuration on double click
                    if (viewMode != ViewModeEnum.DesignDisabled)
                    {
                        string configureScript = CreateConfigureScript(aliasPath, templateId, zoneType);
                        pnlDesignHeader.Attributes.Add("ondblclick", configureScript + " return false;");
                    }

                    bool isWidgetZone = (WidgetZoneType != WidgetZoneTypeEnum.None);

                    // Context menu start
                    string parameter = GetContextMenuParameter();

                    if (ViewMode != ViewModeEnum.DesignDisabled)
                    {
                        if (!isWidgetZone)
                        {
                            pnlDesignHeader.Controls.Add(new LiteralControl(ContextMenuContainer.GetStartTag("webPartZoneMenu", parameter)));
                        }
                        else
                        {
                            pnlDesignHeader.Controls.Add(new LiteralControl(ContextMenuContainer.GetStartTag("widgetZoneMenu", parameter)));

                            pnlDesignHeader.CssClass += " WidgetZoneHeader";
                        }
                    }

                    pnlDesignHeader.Controls.Add(new LiteralControl("<table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" class=\"_nodivs\"><tr>"));

                    if (ViewMode != ViewModeEnum.DesignDisabled)
                    {
                        pnlDesignHeader.Controls.Add(new LiteralControl("<td class=\"WebPartZoneLeftAction\">"));

                        // Add the context menu action
                        AddContextMenuAction(pnlDesignHeader, isWidgetZone, parameter);

                        pnlDesignHeader.Controls.Add(new LiteralControl("</td>"));
                    }

                    pnlDesignHeader.Controls.Add(new LiteralControl("<td>"));

                    // Variant slider
                    if (RenderVariants)
                    {
                        if (ZoneInstance.VariantMode == VariantModeEnum.Conflicted)
                        {
                            // Contains both MVT and Content personalization variants => display warning
                            pnlDesignHeader.Controls.Add(new LiteralControl("<div class=\"SliderConflict\" title=\"" + PortalHelper.LocalizeStringForUI("variantslider.viewmodeconflicttooltip") + "\">"
                                + PortalHelper.LocalizeStringForUI("variantslider.viewmodeconflict") + "</div>"));
                        }
                        else if (!IsVariant
                                 && HasVariants
                                 && !ZoneInstance.WebPartsContainVariants)
                        {
                            // Add variation slider
                            Control pnlVariantSlider = Page.LoadUserControl("~/CMSModules/OnlineMarketing/Controls/Content/VariantSlider.ascx");
                            pnlVariantSlider.ID = "pnlVariations_" + ZoneInstance.ZoneID;
                            pnlDesignHeader.Controls.Add(pnlVariantSlider);
                        }
                    }

                    // Zone handle
                    Panel pnlHandle = new Panel();
                    pnlHandle.CssClass = "WebPartZoneHandle";
                    pnlHandle.ID = "pnlHandle";
                    pnlDesignHeader.Controls.Add(pnlHandle);

                    // Title
                    lblTitle = new Label();
                    lblTitle.CssClass = "WebPartZoneTitle";

                    string title = String.IsNullOrEmpty(ZoneTitle) ? ID : HTMLHelper.HTMLEncode(ZoneTitle);

                    if (mOrphaned)
                    {
                        title += PortalHelper.LocalizeStringForUI("WebPartZone.Orphaned");
                    }
                    lblTitle.Text = title;

                    // Add widget type to title
                    if (isWidgetZone)
                    {
                        // Set the image
                        LocalizedLabel lblZoneType = new LocalizedLabel();
                        lblZoneType.ID = "lblZoneType";
                        lblZoneType.ResourceString = "zone.type." + zoneType;
                        lblZoneType.CssClass = "WebPartZoneType";
                        pnlHandle.Controls.Add(lblZoneType);
                    }

                    pnlHandle.Controls.Add(lblTitle);
                    pnlDesignHeader.Controls.Add(new LiteralControl("</td></tr></table>"));

                    // Context menu end
                    if (ViewMode != ViewModeEnum.DesignDisabled)
                    {
                        pnlDesignHeader.Controls.Add(new LiteralControl(ContextMenuContainer.GetEndTag()));
                    }
                }
                else
                {
                    pnlDesignHeader.CssClass = "WebPartZoneHeader cms-bootstrap";

                    // Add actions panel
                    Panel pnlActions = new Panel();
                    pnlActions.ID = "pnlActions";
                    pnlActions.CssClass = "WebPartZoneActions";
                    pnlDesignHeader.Controls.Add(pnlActions);

                    // Add image
                    ImageButton imgAdd = new ImageButton();
                    imgAdd.ID = "imgAdd";
                    imgAdd.ToolTip = PortalHelper.LocalizeStringForUI("WebPartZone.AddWidgetTooltip");
                    imgAdd.AlternateText = PortalHelper.LocalizeStringForUI("WebPartZone.AddWidgetTooltip");
                    imgAdd.CssClass = "WebPartZoneActionButton";

                    // Different image for user and other zone
                    var imagePath = (WidgetZoneType == WidgetZoneTypeEnum.User) ? "CMSModules/CMS_PortalEngine/Widgets/Add.png" : "CMSModules/CMS_PortalEngine/Widgets/AddEditor.png";

                    imgAdd.ImageUrl = GetImageUrl(imagePath);

                    var onClick = (viewMode == ViewModeEnum.UserWidgetsDisabled) ? "CannotModifyUserWidgets(); return false;" : CreateNewWigdetScript(aliasPath, templateId, zoneType);

                    imgAdd.OnClientClick = onClick;

                    // Add configure menu for editor
                    if (WidgetZoneType == WidgetZoneTypeEnum.Editor)
                    {
                        // Context menu start tag
                        pnlActions.Controls.Add(new LiteralControl(ContextMenuContainer.GetStartTag("widgetZoneEditorMenu", GetContextMenuParameter(), false, HtmlTextWriterTag.Div, "WidgetEditorConfigure", null)));

                        // Configure image
                        CMSIcon imgConf = new CMSIcon();
                        imgConf.ID = "imgConf";
                        imgConf.ToolTip = PortalHelper.LocalizeStringForUI("WebPartZone.ConfigWidgetTooltip");
                        imgConf.CssClass = "WebPartZoneActionButton icon-menu";

                        pnlActions.Controls.Add(imgConf);

                        // Context menu end tag
                        pnlActions.Controls.Add(new LiteralControl(ContextMenuContainer.GetEndTag()));
                    }
                    else
                    {
                        pnlActions.Controls.Add(imgAdd);
                    }
                }
            }
        }


        /// <summary>
        /// Adds the context menu action
        /// </summary>
        /// <param name="parent">Parent control</param>
        /// <param name="isWidgetZone">Flag whether the zone is a widget zone</param>
        /// <param name="parameter">Zone parameters</param>
        /// <param name="iconClass">Icon class</param>
        private void AddContextMenuAction(Control parent, bool isWidgetZone, string parameter, string iconClass = "icon-menu")
        {
            string menuType = (isWidgetZone ? "widgetZoneMenu" : "webPartZoneMenu");
            string onClickScript = string.Format("ContextMenu('{0}', parentNode.parentNode.parentNode.parentNode, {1}, true); return false;", menuType, parameter);

            AddIconAction(parent, iconClass, onClickScript, PortalHelper.LocalizeStringForUI("WebPartZone.Menu"));
        }


        /// <summary>
        /// Adds the icon action to the given parent control (context menu control).
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="iconClass">The icon class.</param>
        /// <param name="onClickScript">The on click script.</param>
        /// <param name="tooltip">The tooltip.</param>
        private void AddIconAction(Control parent, string iconClass, string onClickScript, string tooltip)
        {
            // Create menu icon
            CMSIcon menuIcon = new CMSIcon
            {
                CssClass = iconClass
            };

            menuIcon.Attributes.Add("onclick", onClickScript);
            menuIcon.Attributes.Add("aria-hidden", "true");
            menuIcon.ToolTip = tooltip;

            parent.Controls.Add(menuIcon);
        }


        /// <summary>
        /// Gets the parameter string for the zone context menu
        /// </summary>
        private string GetContextMenuParameter()
        {
            PageInfo pi = PagePlaceholder.PageInfo;
            if (pi != null)
            {
                PageTemplateInfo pti = pi.UsedPageTemplateInfo;
                PageTemplateTypeEnum type = (pti != null) ? pti.PageTemplateType : PageTemplateTypeEnum.Unknown;

                string parameter = String.Format(
                    "new zoneProperties('{0}', '{1}', '{2}', '{3}', {4}, {5}, '{6}')",
                    ID,
                    pi.NodeAliasPath,
                    pi.GetUsedPageTemplateId(),
                    WidgetZoneType,
                    LayoutZone.ToString().ToLowerCSafe(),
                    WebPartClipBoardManager.ContainsClipBoardItem(WidgetZoneType, type).ToString().ToLowerCSafe(),
                    WebPartClipBoardManager.GetKey(WidgetZoneType, type, false)
                );

                return parameter;
            }

            return null;
        }


        /// <summary>
        /// Loads the web parts to the control.
        /// </summary>
        /// <param name="zoneInstance">Zone instance with the web parts data</param>
        /// <param name="reloadData">If true, the web part are forced to reload the data</param>
        public void LoadWebParts(WebPartZoneInstance zoneInstance, bool reloadData = true)
        {
            mZoneInstance = zoneInstance;

            WebParts.Clear();

            // Create the web parts container and zone header.
            EnsureWebPartsContainer();

            // Load the web parts
            if (zoneInstance != null)
            {
                EnableViewState = !ValidationHelper.GetBoolean(GetValue("DisableViewState"), false);

                // Load the web parts
                foreach (WebPartInstance part in zoneInstance.WebParts)
                {
                    // Load the web part the regular way
                    LoadWebPart(WebPartContainer, this, part, reloadData);
                }
            }

            // Load closing controls of the container.
            FinalizeWebPartsContainer();
        }


        /// <summary>
        /// Loads the editable regions content.
        /// </summary>
        /// <param name="forceReload">Force the reload of the content</param>
        public void LoadRegionsContent(bool forceReload)
        {
            // Load zone regions content
            foreach (ICMSEditableControl control in CMSEditableControls)
            {
                control.LoadContent(PagePlaceholder.PageInfo, forceReload);
            }
        }


        /// <summary>
        /// Loads the content to the partially cached web part
        /// </summary>
        /// <param name="cachedControl">Partially cached container</param>
        /// <param name="part">Inner web part</param>
        protected void LoadCachedWebPart(PartialCachingControl cachedControl, CMSAbstractWebPart part)
        {
            string controlId = CMSAbstractWebPart.GetCachedWebPartOriginalID(cachedControl.ID);

            // Setup the web part
            part.ID = controlId;

            // If web part is layout, load the layout content immediately
            var layoutWebPart = part as CMSAbstractLayoutWebPart;
            if (layoutWebPart != null)
            {
                layoutWebPart.LoadLayout(true);
            }

            part.LoadContent(ZoneInstance.GetWebPart(controlId));

            // Load the editable content
            var editableControl = part as ICMSEditableControl;
            if (editableControl != null)
            {
                editableControl.LoadContent(PagePlaceholder.PageInfo);
            }

            // Load cached variants
            foreach (CMSWebPartZone zone in PnlZoneVariants.Controls)
            {
                zone.LoadCachedWebPart(cachedControl, part);
            }
        }


        /// <summary>
        /// Loads the web parts content.
        /// </summary>
        /// <param name="reloadData">If true, the web part are forced to reload the data</param>
        public void LoadWebPartsContent(bool reloadData)
        {
            foreach (Control ctrl in WebParts)
            {
                // Get control instance
                CMSAbstractWebPart part = PortalHelper.GetWebPartControl(ctrl);
                if (part != null)
                {
                    // Handle partial caching control
                    var cachingControl = ctrl as PartialCachingControl;
                    if (cachingControl != null)
                    {
                        LoadCachedWebPart(cachingControl, part);
                    }
                    else
                    {
                        // Load the instance content
                        part.LoadContent(part.PartInstance, reloadData);
                    }

                    // Reload data in webparts on some move action
                    if (RequestHelper.IsPostBack() && PortalContext.IsDesignMode(ViewMode) && !RequestHelper.IsAsyncPostback())
                    {
                        part.ReloadData();
                    }
                }
            }

            // Load zone variants content
            foreach (CMSWebPartZone zone in PnlZoneVariants.Controls)
            {
                zone.LoadWebPartsContent(reloadData);
            }
        }


        /// <summary>
        /// Init event handler.
        /// </summary>
        private void CachedWebPart_Init(object sender, EventArgs e)
        {
            PartialCachingControl cachedControl = (PartialCachingControl)sender;

            CMSAbstractWebPart part = PortalHelper.GetWebPartControl(cachedControl);
            if (part != null)
            {
                LoadCachedWebPart(cachedControl, part);

                // Reload the web part
                part.ReloadData();
            }
        }


        /// <summary>
        /// PreRender event handler.
        /// </summary>
        public void cachedControl_PreRender(object sender, EventArgs e)
        {
            PartialCachingControl cachedWebPart = (PartialCachingControl)sender;

            if ((cachedWebPart.CachedControl == null) && !RequestHelper.IsPostBack())
            {
                // Notes:
                //   Fist load (when a web part is being cached) has the "CachedControl" property defined, CMSAbstractWebPart control is located there.
                //   Second (cached) request takes the "PartialCachingControl" from the output cache (keeps only rendered HTML), so the "CachedControl" is NULL then.
                //   Postback request is not being cached.

                // Log the cache operation
                CacheDebug.LogCacheOperation(CacheOperation.GET, "Partial cache (" + CMSAbstractWebPart.GetCachedWebPartOriginalID(cachedWebPart.ID) + "): " + cachedWebPart.ClientID, null, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CMSCacheItemPriority.NotRemovable, true);
            }
        }


        /// <summary>
        /// Saves the page content to the page info.
        /// </summary>
        /// <param name="pageInfo">Page info where to save the content</param>
        public void SaveContent(PageInfo pageInfo)
        {
            foreach (Control ctrl in WebParts)
            {
                // Get control instance
                Control control = PortalHelper.GetWebPartControl(ctrl);

                // Process only editable controls
                var editableControl = control as ICMSEditableControl;
                if (editableControl != null)
                {
                    // Save the content
                    editableControl.SaveContent(pageInfo);
                }
            }
        }


        /// <summary>
        /// Saves the page content to the page info.
        /// </summary>
        public bool Validate()
        {
            bool isValid = true;

            foreach (Control ctrl in WebParts)
            {
                // Get control instance
                Control control = PortalHelper.GetWebPartControl(ctrl);

                // Process only editable controls
                var editableControl = control as ICMSEditableControl;
                if (editableControl != null)
                {
                    isValid = isValid & editableControl.IsValid();
                }
            }

            return isValid;
        }


        /// <summary>
        /// Returns web part with the specified name or null if not found.
        /// </summary>
        /// <param name="webPartName">Web part name</param>
        public CMSAbstractWebPart GetWebPart(string webPartName)
        {
            // Go through all the web parts
            foreach (Control ctrl in WebParts)
            {
                // Get control instance
                CMSAbstractWebPart part = PortalHelper.GetWebPartControl(ctrl);
                if (part != null)
                {
                    if (part.ID.ToLowerCSafe() == webPartName.ToLowerCSafe())
                    {
                        return part;
                    }
                }
            }
            return null;
        }


        /// <summary>
        /// Method that is called when the page content is fully loaded.
        /// </summary>
        public virtual void OnContentLoaded()
        {
            // Go through all the web parts
            var list = WebParts.ToArray();
            foreach (Control ctrl in list)
            {
                // Get control instance
                CMSAbstractWebPart part = PortalHelper.GetWebPartControl(ctrl);
                if (part != null)
                {
                    // Raise OnContentLoaded
                    part.SetContext();
                    part.OnContentLoaded();
                    part.ReleaseContext();
                }
            }

            // Ensure OnContentLoaded for zone variants
            foreach (CMSWebPartZone zone in PnlZoneVariants.Controls)
            {
                zone.OnContentLoaded();
            }
        }


        /// <summary>
        /// Overrides the generation of the SPAN tag with custom tag.
        /// </summary>
        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                if (SettingsKeyInfoProvider.GetValue(PagePlaceholder.SiteName + ".CMSControlElement").ToLowerCSafe().Trim() == "div")
                {
                    return HtmlTextWriterTag.Div;
                }
                else
                {
                    return HtmlTextWriterTag.Span;
                }
            }
        }


        /// <summary>
        /// Returns the list of the field IDs (Client IDs of the inner controls) that should be spell checked.
        /// </summary>
        public List<string> GetSpellCheckFields()
        {
            List<string> result = new List<string>();

            // Collect from the zones
            foreach (Control ctrl in WebParts)
            {
                // Get control instance
                Control control = PortalHelper.GetWebPartControl(ctrl);

                // Process only controls that support spell check
                var editableControl = control as ISpellCheckableControl;
                if (editableControl != null)
                {
                    var fields = editableControl.GetSpellCheckFields();
                    if (fields != null)
                    {
                        result.AddRange(fields);
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// Clears the caches of all the web parts.
        /// </summary>
        public void ClearCache()
        {
            foreach (Control ctrl in WebParts)
            {
                // Get control instance
                CMSAbstractWebPart part = PortalHelper.GetWebPartControl(ctrl);
                if (part != null)
                {
                    part.ClearCache();
                }
            }

            // Clear variants cache
            foreach (CMSWebPartZone zone in PnlZoneVariants.Controls)
            {
                zone.ClearCache();
            }
        }


        /// <summary>
        /// Returns the value of the given webpart property property.
        /// </summary>
        /// <param name="propertyName">Property name</param>
        public virtual object GetValue(string propertyName)
        {
            object result;

            propertyName = propertyName.ToLowerCSafe();

            // If no zone instance, get local value
            if (mZoneInstance == null)
            {
                result = mLocalProperties[propertyName];
            }
            else
            {
                // Get local value first
                object localValue = mLocalProperties[propertyName];

                result = localValue ?? mZoneInstance.GetValue(propertyName);
            }

            // Localize the value
            if (result is string)
            {
                // If allowed to be resolved, resolve the macros
                string pname = ";" + propertyName.ToLowerCSafe() + ";";
                if (mNotResolveProperties.IndexOfCSafe(pname) < 0)
                {
                    // Resolve the macros using web part resolver
                    MacroSettings settings = new MacroSettings();
                    settings.AvoidInjection = (mSQLProperties.IndexOfCSafe(pname) >= 0);
                    settings.Culture = Thread.CurrentThread.CurrentCulture.ToString();

                    result = ResolveMacros((string)result, settings);
                }
            }

            return result;
        }


        /// <summary>
        /// Sets the property value of the control, setting the value affects only local property value.
        /// </summary>
        /// <param name="propertyName">Property name to set</param>
        /// <param name="value">New property value</param>
        public virtual void SetValue(string propertyName, object value)
        {
            mLocalProperties[propertyName.ToLowerCSafe()] = value;
        }


        /// <summary>
        /// Resolves the macros within current WebPart context.
        /// </summary>
        /// <param name="inputText">Input text to resolve</param>
        /// <param name="settings">Macro context object with specific options</param>
        public virtual string ResolveMacros(string inputText, MacroSettings settings = null)
        {
            // Ensure macro context object
            if (settings == null)
            {
                settings = new MacroSettings();
                settings.Culture = Thread.CurrentThread.CurrentCulture.ToString();
            }

            // Set related object
            if (PagePlaceholder != null)
            {
                settings.RelatedObject = PagePlaceholder.TemplateInstance;
            }

            return ContextResolver.ResolveMacros(inputText, settings);
        }


        /// <summary>
        /// Causes reloading the data, override to implement the data reloading procedure.
        /// </summary>
        public virtual void ReloadData()
        {
            // Reload data of all the web parts
            foreach (Control ctrl in WebParts)
            {
                // Get control instance
                CMSAbstractWebPart part = PortalHelper.GetWebPartControl(ctrl);
                if (part != null)
                {
                    part.ReloadData();
                }
            }

            // Reload variants data
            foreach (CMSWebPartZone zone in PnlZoneVariants.Controls)
            {
                zone.ReloadData();
            }
        }


        /// <summary>
        /// Finds the  web part with specific name.
        /// </summary>
        /// <param name="name">Web part name to find</param>
        public virtual CMSAbstractWebPart FindWebPart(string name)
        {
            // Search all web parts
            foreach (Control ctrl in WebParts)
            {
                // Get control instance
                CMSAbstractWebPart part = PortalHelper.GetWebPartControl(ctrl);
                if ((part != null) && part.ID.EqualsCSafe(name, true))
                {
                    return part;
                }
            }

            return null;
        }


        /// <summary>
        /// Finds the  web part with specific type (first web part).
        /// </summary>
        /// <param name="type">Web part type to find</param>
        public virtual CMSAbstractWebPart FindWebPart(Type type)
        {
            // Search all web parts
            foreach (Control ctrl in WebParts)
            {
                // Get control instance
                CMSAbstractWebPart part = PortalHelper.GetWebPartControl(ctrl);
                if ((part != null) && type.IsInstanceOfType(part))
                {
                    return part;
                }
            }

            return null;
        }


        /// <summary>
        /// Gets the current viewmode.
        /// </summary>
        /// <returns></returns>
        private ViewModeEnum GetCurrentViewmode()
        {
            // Get the view mode
            ViewModeEnum viewMode = PagePlaceholder.ViewMode;
            switch (viewMode)
            {
                case ViewModeEnum.Edit:
                case ViewModeEnum.EditDisabled:
                case ViewModeEnum.EditLive:

                    if ((PagePlaceholder.PageInfo == null) || (PagePlaceholder.PageInfo.ChildPageInfo == null))
                    {
                        // Allow edit mode only with editor zone type
                        if (WidgetZoneType != WidgetZoneTypeEnum.Editor)
                        {
                            viewMode = ViewModeEnum.Preview;
                        }
                        // Set disabled user widgets mode for any other than live site mode
                        if (WidgetZoneType == WidgetZoneTypeEnum.User)
                        {
                            viewMode = ViewModeEnum.UserWidgetsDisabled;
                        }
                    }
                    break;

                case ViewModeEnum.LiveSite:
                    // Ensure correct view mode for user/group widgets
                    if ((PagePlaceholder.PageInfo == null) || (PagePlaceholder.PageInfo.ChildPageInfo == null))
                    {
                        // Special mode for the widget zones
                        switch (WidgetZoneType)
                        {
                            case WidgetZoneTypeEnum.User:
                                if (AuthenticationHelper.IsAuthenticated())
                                {
                                    viewMode = ViewModeEnum.UserWidgets;
                                }
                                else
                                {
                                    viewMode = ViewModeEnum.UserWidgetsDisabled;
                                }
                                break;

                            case WidgetZoneTypeEnum.Group:
                                // Check if the group widgets can be enabled (only for the group admin)
                                int groupId = 0;
                                if (PagePlaceholder.PageInfo != null)
                                {
                                    groupId = PagePlaceholder.PageInfo.NodeGroupID;
                                }
                                if (groupId > 0)
                                {
                                    // Check if the user is group admin of the given group
                                    var currentUser = MembershipContext.AuthenticatedUser;
                                    if (currentUser.IsGroupAdministrator(groupId) || currentUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin))
                                    {
                                        viewMode = ViewModeEnum.GroupWidgets;
                                    }
                                }
                                break;
                        }
                    }
                    break;

                case ViewModeEnum.DashboardWidgets:
                    // Special mode for the widget zones
                    if (WidgetZoneType != WidgetZoneTypeEnum.Dashboard)
                    {
                        viewMode = ViewModeEnum.LiveSite;
                    }
                    break;

                case ViewModeEnum.Preview:
                    if ((PagePlaceholder.PageInfo == null) || (PagePlaceholder.PageInfo.ChildPageInfo == null))
                    {
                        // Set disabled user widgets mode for any other than live site mode
                        if (WidgetZoneType == WidgetZoneTypeEnum.User)
                        {
                            viewMode = ViewModeEnum.UserWidgetsDisabled;
                        }
                    }
                    break;
            }

            return viewMode;
        }

        #endregion


        #region "Static methods"

        /// <summary>
        /// Loads the web part to the given zone. Returns the newly created web part.
        /// </summary>
        /// <param name="container">Control container</param>
        /// <param name="zone">Parent zone</param>
        /// <param name="part">Web part instance</param>
        /// <param name="reloadData">Reload the web part data</param>
        /// <param name="isVariant">Indicates whether the web part instance is a variant of another existing web part</param>
        public static Control LoadWebPart(Control container, CMSWebPartZone zone, WebPartInstance part, bool reloadData, bool isVariant = false)
        {
            if (part == null)
            {
                return null;
            }

            Page page = zone.Page;
            Control newControl;

            WebPartInfo wpi = null;
            WebPartLayoutInfo wpli = null;

            // Get original or variant control ID
            string controlId = part.ControlID;
            if (part.CurrentVariantInstance != null)
            {
                controlId = part.CurrentVariantInstance.ControlID;
            }

            // Get unique id, uses custom replacement due to possible collision with simple underscore
            string id = ValidationHelper.GetIdentifier(controlId, "_wuid_");

            try
            {
                bool safeLoad = false;
                bool loadBlank = false;

                // If code present, load through virtual path provider
                var vm = zone.PagePlaceholder.ViewMode;
                if (PortalContext.IsDesignMode(vm))
                {
                    // Design mode - just web part stub
                    if (!PortalHelper.DisplayContentInDesignMode)
                    {
                        loadBlank = true;
                    }
                    else
                    {
                        safeLoad = true;
                    }
                }
                else
                {
                    switch (vm)
                    {
                        // Load empty widgets if error occurred on dashboard page 
                        case ViewModeEnum.DashboardWidgets:
                            if (PortalHelper.SafeMode && part.IsWidget)
                            {
                                safeLoad = true;
                                loadBlank = true;
                            }
                            break;

                        // Regular mode - load the web part
                        default:
                            // Try to catch error for widgets, to be able to remove widget
                            safeLoad = part.IsWidget;
                            if (PortalHelper.SafeMode && ((zone.PagePlaceholder.ViewMode.IsEdit()) || (zone.PagePlaceholder.ViewMode.IsEditLive())))
                            {
                                loadBlank = true;
                            }
                            break;
                    }
                }

                // Get the URL
                string url = GetWebPartUrl(zone, part, ref wpi, ref wpli);

                // Always load layout web parts
                if (loadBlank && !PortalHelper.SafeMode && (wpi != null) && ((WebPartTypeEnum)wpi.WebPartType == WebPartTypeEnum.Layout))
                {
                    loadBlank = false;
                }

                if (loadBlank)
                {
                    newControl = new WebPartBlank();
                }
                else
                {
                    // Load the control
                    newControl = page.LoadUserControl(url);

                    if (newControl != null)
                    {
                        newControl.ID = "urlControl";
                    }

                    if (safeLoad)
                    {
                        // Check the ability for controls collection modification
                        var ltlTest = new LiteralControl();

                        newControl.Controls.Add(ltlTest);
                        newControl.Controls.Remove(ltlTest);
                    }
                }

                // If cached control, setup later binding
                if (newControl is PartialCachingControl)
                {
                    PartialCachingControl cachedControl = (PartialCachingControl)newControl;

                    cachedControl.ID = CMSAbstractWebPart.CreateCachedWebPartID(id);

                    // Handle the cached control events
                    cachedControl.Init += zone.CachedWebPart_Init;
                    cachedControl.PreRender += zone.cachedControl_PreRender;

                    if (!isVariant)
                    {
                        // add the variant to the webpart controls list
                        container.Controls.Add(cachedControl);

                        // add webpart to the zone web parts list
                        zone.WebParts.Add(cachedControl);
                    }
                }
                else
                {
                    // Setup the web part
                    CMSAbstractWebPart newPart = (CMSAbstractWebPart)newControl;
                    newPart.ParentZone = zone;
                    newPart.ID = id;
                    newPart.CurrentLayoutInfo = wpli;
                    newPart.PagePlaceholder = zone.PagePlaceholder;
                    newPart.PartInstance = part;

                    // Set the Skin ID before the control is added to the controls collection
                    if (!string.IsNullOrEmpty(newPart.SkinID))
                    {
                        newPart.SkinID = newPart.SkinID;
                    }

                    if (!isVariant)
                    {
                        // Add to the controls collection
                        container.Controls.Add(newPart);

                        // add webpart to the zone web parts list
                        zone.WebParts.Add(newPart);
                    }

                    // If web part is layout, load the layout content immediately
                    var layoutWebPart = newPart as CMSAbstractLayoutWebPart;
                    if (layoutWebPart != null)
                    {
                        layoutWebPart.LoadLayout(reloadData);
                    }

                    if (!isVariant)
                    {
                        // Ensure the web part variants
                        newPart.EnsureVariants();
                    }
                }

            }
            catch (ThreadAbortException)
            {
                // Do not handle a ThreadAbortException as web part error. It's re-thrown automatically at the end of the catch block.
                return null;
            }
            catch (Exception ex)
            {
                // Load web part to display an error
                var err = new WebPartError();

                err.PagePlaceholder = zone.PagePlaceholder;
                err.PartInstance = part;
                err.ID = ControlsHelper.GetUniqueID(container, id, null);

                // Try remove existing controls
                Control existing = container.FindControl(id);
                if (existing != null)
                {
                    container.Controls.Remove(existing);
                }

                container.Controls.Add(err);

                bool logException = true;

                // Check out why the error occurred
                if (wpi != null)
                {
                    if (!ResourceInfoProvider.IsResourceAvailable(wpi.WebPartResourceID))
                    {
                        // Get the module
                        var ri = ResourceInfoProvider.GetResourceInfo(wpi.WebPartResourceID);
                        if (ri != null)
                        {
                            // Module not available
                            err.ErrorTitle = String.Format(PortalHelper.LocalizeStringForUI("WebPart.NotInstalled"), wpi.WebPartDisplayName, ri.ResourceDisplayName);
                            logException = false;
                        }
                    }
                }

                // Set the specific error title for widgets
                var zoneType = zone.WidgetZoneType;
                if ((zoneType != WidgetZoneTypeEnum.None) && (zoneType != WidgetZoneTypeEnum.All))
                {
                    err.IsWidgetError = true;
                }

                if (logException)
                {
                    // Log the exception to event log
                    err.InnerException = ex;
                    EventLogProvider.LogException("PortalEngine", "LoadWebPart", ex);
                }

                zone.WebParts.Add(err);
                newControl = err;
            }

            return newControl;
        }


        /// <summary>
        /// Returns true if the partial caching is enabled.
        /// </summary>
        /// <param name="siteName">Site name</param>
        private static bool EnablePartialCache(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSEnablePartialCache");
        }


        /// <summary>
        /// Gets the URL for the web part to load.
        /// </summary>
        /// <param name="zone">Zone</param>
        /// <param name="part">Web part instance</param>
        /// <param name="wpi">Returning the web part info</param>
        /// <param name="wpli">Returning the web part layout info</param>
        private static string GetWebPartUrl(CMSWebPartZone zone, WebPartInstance part, ref WebPartInfo wpi, ref WebPartLayoutInfo wpli)
        {
            string url = null;

            if (part.IsWidget)
            {
                // Load widget
                WidgetInfo wi = WidgetInfoProvider.GetWidgetInfo(part.WebPartType);
                if (wi != null)
                {
                    // Get parent web part
                    wpi = WebPartInfoProvider.GetWebPartInfo(wi.WidgetWebPartID);
                    if (wpi != null)
                    {
                        // Set widget layout
                        wpli = WebPartLayoutInfoProvider.GetWebPartLayoutInfo(wi.WidgetLayoutID);

                        if (wpli != null)
                        {
                            // Load specific layout through virtual path
                            url = wpli.Generalized.GetVirtualFileRelativePath(WebPartLayoutInfo.EXTERNAL_COLUMN_CODE, wpli.WebPartLayoutVersionGUID);
                        }
                        else
                        {
                            // Layout not present, load regularly
                            url = WebPartInfoProvider.GetWebPartUrl(wpi, false);
                        }
                    }
                }
            }
            else
            {
                // Get the web part
                wpi = WebPartInfoProvider.GetWebPartInfo(part.WebPartType);
                if (wpi != null)
                {
                    int partialCacheMinutes = 0;

                    // Check if the web part should be partially cached
                    if (zone.PagePlaceholder.ViewMode.IsLiveSite() && EnablePartialCache(SiteContext.CurrentSiteName))
                    {
                        string partialCacheMinutesValue = ValidationHelper.GetString(part.GetValue("PartialCacheMinutes"), String.Empty);
                        partialCacheMinutes = ValidationHelper.GetInteger(partialCacheMinutesValue, 0);

                        if ((partialCacheMinutes == 0) && MacroProcessor.ContainsMacro(partialCacheMinutesValue))
                        {
                            // Try to parse value with global macro resolver
                            partialCacheMinutes = ValidationHelper.GetInteger(MacroResolver.Resolve(partialCacheMinutesValue), 0);
                        }
                    }

                    // Get web part layout
                    string webPartLayout = ValidationHelper.GetString(part.GetValue("WebPartLayout"), "");

                    if ((partialCacheMinutes > 0) && VirtualPathHelper.UsingVirtualPathProvider)
                    {
                        url = WebPartInfoProvider.GetVirtualWebPartUrl(zone.PagePlaceholder.PageInfo, part, partialCacheMinutes, wpi);
                    }
                    else if (!String.IsNullOrEmpty(webPartLayout))
                    {
                        // Get the web part layout
                        wpli = WebPartLayoutInfoProvider.GetWebPartLayoutInfo(part.WebPartType, webPartLayout);
                        if (wpli != null)
                        {
                            // Load specific layout through virtual path
                            url = wpli.Generalized.GetVirtualFileRelativePath(WebPartLayoutInfo.EXTERNAL_COLUMN_CODE, wpli.WebPartLayoutVersionGUID);
                        }
                    }

                    if (url == null)
                    {
                        // Code not present, load regularly
                        url = WebPartInfoProvider.GetWebPartUrl(wpi, false);
                    }
                }
            }

            return url;
        }


        /// <summary>
        /// Gets UI image relative path.
        /// </summary>
        /// <param name="imagePath">Partial image path strating from ~/App_Themes/(Skin_Folder)/Images/ (e.g. '/CMSModules/Membership/module.png')</param>
        private string GetImageUrl(string imagePath)
        {
            return UIHelper.GetImageUrl(Page, imagePath);
        }


        /// <summary>
        /// Checks whether the page displayed in current context is considered a sub page
        /// </summary>
        private bool IsSubPage()
        {
            return ((PagePlaceholder.PageInfo.ChildPageInfo != null) || (PagePlaceholder.PageInfo.GetUsedPageTemplateId() != PagePlaceholder.PageInfo.UsedPageTemplateInfo.PageTemplateId));
        }

        #endregion


        #region "ITimeZoneManager Members"

        /// <summary>
        /// Returns time zone type.
        /// </summary>
        public TimeZoneTypeEnum TimeZoneType
        {
            get
            {
                string timeZoneType = ValidationHelper.GetString(GetValue("TimeZoneType"), String.Empty);
                if (!String.IsNullOrEmpty(timeZoneType))
                {
                    return timeZoneType.ToEnum<TimeZoneTypeEnum>();
                }
                return TimeZoneTypeEnum.Inherit;
            }
        }


        /// <summary>
        /// Returns custom time zone info.
        /// </summary>
        public TimeZoneInfo CustomTimeZone
        {
            get
            {
                string timeZoneName = ValidationHelper.GetString(GetValue("CustomTimeZone"), "");
                if (timeZoneName != "")
                {
                    return TimeZoneInfoProvider.GetTimeZoneInfo(timeZoneName);
                }
                return null;
            }
        }

        #endregion


        #region "Variants methods"

        /// <summary>
        /// Removes the variant from cache.
        /// </summary>
        /// <param name="variantId">The variant id</param>
        public virtual void RemoveVariantFromCache(int variantId)
        {
            if (HasVariants)
            {
                CMSWebPartZone webPartZoneToRemove = null;
                foreach (CMSWebPartZone zone in PnlZoneVariants.Controls)
                {
                    if ((zone.IsVariant)
                        && (zone.ZoneInstance != null)
                        && (zone.ZoneInstance.VariantID == variantId))
                    {
                        webPartZoneToRemove = zone;
                        ZoneInstance.ZoneInstanceVariants.Remove(zone.ZoneInstance);

                        // Set the variant mode to NONE when there are no other variants left
                        if (ZoneInstance.ZoneInstanceVariants.Count == 0)
                        {
                            ZoneInstance.VariantMode = VariantModeEnum.None;
                        }
                        break;
                    }
                }

                if (webPartZoneToRemove != null)
                {
                    PnlZoneVariants.Controls.Remove(webPartZoneToRemove);
                }
            }
        }

        #endregion
    }
}
