using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Caching;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.AspNet.Platform.Cache.Extension;
using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.EventLog;
using CMS.FormEngine.Web.UI;
using CMS.Globalization;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.Membership;
using CMS.OutputFilter;
using CMS.SiteProvider;

using TimeZoneInfo = CMS.Globalization.TimeZoneInfo;

namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// Base class for the web part controls.
    /// </summary>
    [DebuggerDisplay("CMSAbstractWebPart({WebPartID})")]
    public abstract partial class CMSAbstractWebPart : AbstractUserControl, ICMSVariantsControl, ITimeZoneManager, IDataControl, ISimpleDataContainer, IExceptionHandler
    {
        #region "Constants"

        /// <summary>
        /// Web part render resolver name
        /// </summary>
        public const string RESOLVER_RENDER = "WebPartRender";

        #endregion


        #region "Variables"

        private bool? mIsVisible;

        /// <summary>
        /// List of properties that should not be resolved.
        /// </summary>
        protected string mNotResolveProperties = ";;";

        /// <summary>
        /// List of SQL properties that should be escaped for SQL injection.
        /// </summary>
        protected string mSQLProperties = ";columns;wherecondition;orderby;";

        /// <summary>
        /// If true, the macros are disabled.
        /// </summary>
        protected bool? mDisableMacros;

        /// <summary>
        /// If true, the script for setting the property if generated.
        /// </summary>
        protected bool mProvideSetPropertyScript ;

        /// <summary>
        /// Flag if the properties of widget that shouldn't be resolved were loaded.
        /// </summary>
        protected bool widgetNotResolvePropertiesLoaded;

        /// <summary>
        /// Script to fire the configuration dialog.
        /// </summary>
        protected string configureScript;

        /// <summary> 
        /// If true, the web part location script was already rendered
        /// </summary>
        protected bool locationRendered;

        /// <summary>
        /// If true, the web part variants were already loaded
        /// </summary>
        protected bool variantsLoaded;

        /// <summary>
        /// Web part error instance
        /// </summary>
        private WebPartError errorWebPart;

        /// <summary>
        /// Short client ID.
        /// </summary>
        protected string mShortClientID;

        /// <summary>
        /// Local view mode.
        /// </summary>
        protected ViewModeEnum mViewMode = ViewModeEnum.Unknown;


        /// <summary>
        /// Web part info.
        /// </summary>
        protected WebPartInfo mPartInfo;

        /// <summary>
        /// Title information.
        /// </summary>
        protected string mTitleInfo;


        /// <summary>
        /// Local web part properties.
        /// </summary>
        protected Hashtable mLocalProperties = new Hashtable();

        /// <summary>
        /// Specifies whether the control is StandAlone or not, if false, the control is located within PortalEngine environment.
        /// </summary>
        protected bool mStandAlone = true;

        /// <summary>
        /// Control page cycle status.
        /// </summary>
        protected PageCycleEnum mPageCycle = PageCycleEnum.Created;

        /// <summary>
        /// Container info object.
        /// </summary>
        protected WebPartContainerInfo mContainer;

        /// <summary>
        /// Web part context resolver.
        /// </summary>
        private MacroResolver mContextResolver;

        private bool mStopProcessing;

        /// <summary>
        /// Custom data connected to the object.
        /// </summary>
        protected object mRelatedData;

        /// <summary>
        /// Indicates whether to render the variants.
        /// </summary>
        private bool? mRenderVariants;

        /// <summary>
        /// Additional css class
        /// </summary>
        private string mAdditionalCssClass = String.Empty;

        /// <summary>
        /// Indicates whether this web part is in edit mode and contains content personalization variants.
        /// According to this setting, the web part context menu will be rendered in the edit mode.
        /// </summary>
        private bool? mCPWebPartInEdit;

        /// <summary>
        /// Web part variants
        /// </summary>
        private List<CMSAbstractWebPart> mVariants;

        /// <summary>
        /// Table of evals used within the control 
        /// </summary>
        private HashSet<string> mLoggedEvals;

        /// <summary>
        /// Number of available columns within the data source
        /// </summary>
        private int mLoggedColsCount;

        /// <summary>
        /// If true, the web part needs the layouts script.
        /// </summary>
        protected bool mNeedsLayoutScript;

        /// <summary>
        /// True if the web part is in design mode.
        /// </summary>
        protected bool? mIsDesign;

        /// <summary>
        /// If true, the web part class is rendered
        /// </summary>
        protected bool mRenderWebPartClass = true;

        /// <summary>
        /// If true, the on-site editing envelope tags will be rendered
        /// </summary>
        private bool renderOnSiteEditSpanTags;

        /// <summary>
        /// Web part instance.
        /// </summary>
        private WebPartInstance mPartInstance;

        /// <summary>
        /// Indicates whether the web part should keep the cached output HTML during a postback request (note: postback request itself is not being cached).
        /// </summary>
        private bool? mPreservePartialCacheOnPostback;

        private UIContext mUIContext;

        #endregion


        #region "Controls variables"

        /// <summary>
        /// Parent web part zone.
        /// </summary>
        protected CMSWebPartZone mParentZone;

        /// <summary>
        /// If true, the parent zone was already searched
        /// </summary>
        protected bool mParentZoneSearched;


        /// <summary>
        /// Parent page placeholder.
        /// </summary>
        protected CMSPagePlaceholder mPagePlaceholder;

        /// <summary>
        /// Parent page manager.
        /// </summary>
        protected IPageManager mPageManager;

        /// <summary>
        /// First level child page placeholders.
        /// </summary>
        protected List<CMSPagePlaceholder> mChildPagePlaceholders;

        /// <summary>
        /// Container control for the design mode header.
        /// </summary>
        protected Control mHeaderContainer;

        /// <summary>
        /// Header control
        /// </summary>
        protected Control mHeaderControl;

        /// <summary>
        /// Web part title label.
        /// </summary>
        protected Label lblWebPartTitle;

        /// <summary>
        /// Web part title container
        /// </summary>
        protected PlaceHolder plcTitleContainer;

        /// <summary>
        /// Placeholder containing a list of variants used for an explicit rendering.
        /// </summary>
        private PlaceHolder mVariantControlsPlaceHolder;

        /// <summary>
        /// Placeholder used for zone variants.
        /// </summary>
        private PlaceHolder plcVariantSlider;

        #endregion


        #region "Private properties"

        /// <summary>
        /// Gets or sets current document wizard manager
        /// </summary>
        protected IDocumentWizardManager DocumentWizardManager
        {
            get
            {
                return PortalManager?.DocumentWizardManager;
            }
            set
            {
                if (PortalManager != null)
                {
                    PortalManager.DocumentWizardManager = value;
                }
            }
        }


        /// <summary>
        /// Placeholder containing a list of variants used for an explicit rendering.
        /// </summary>
        protected PlaceHolder VariantControlsPlaceHolder
        {
            get
            {
                if (mVariantControlsPlaceHolder == null)
                {
                    mVariantControlsPlaceHolder = new PlaceHolder();
                    Controls.Add(mVariantControlsPlaceHolder);
                }

                return mVariantControlsPlaceHolder;
            }
        }


        /// <summary>
        /// Indicates whether this web part is in edit mode and contains content personalization variants.
        /// According to this setting, the web part context menu will be rendered in the edit mode.
        /// </summary>
        protected bool CPWebPartInEdit
        {
            get
            {
                if (!mCPWebPartInEdit.HasValue)
                {
                    mCPWebPartInEdit = false;

                    if ((PartInstance != null)
                        && !IsWidget
                        && PartInstance.HasVariants
                        && ViewMode.IsEdit(true)
                        && (PartInstance.VariantMode == VariantModeEnum.ContentPersonalization))
                    {
                        mCPWebPartInEdit = true;
                    }
                }

                return mCPWebPartInEdit.Value;
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
                    ViewModeEnum viewMode = PortalContext.ViewMode;

                    if ((PortalContext.ViewMode != ViewModeEnum.LiveSite)
                        && (PartInstance?.ParentZone != null)
                        && (PortalContext.MVTVariantsEnabled || PortalContext.ContentPersonalizationEnabled))
                    {
                        // Check permissions based on webpart type
                        CurrentUserInfo cui = MembershipContext.AuthenticatedUser;
                        if ((PartInstance.VariantMode == VariantModeEnum.MVT) && !cui.IsAuthorizedPerResource("cms.mvtest", "Read"))
                        {
                            return mRenderVariants.Value;
                        }

                        if ((PartInstance.VariantMode == VariantModeEnum.ContentPersonalization) && !cui.IsAuthorizedPerResource("cms.contentpersonalization", "Read"))
                        {
                            return mRenderVariants.Value;
                        }

                        // Render the variant envelope only for webparts in the design mode and for editor's widgets in the Edit mode
                        // and also for webparts with CP variants in the Edit view mode
                        switch (viewMode)
                        {
                            case ViewModeEnum.Design:
                            case ViewModeEnum.DesignDisabled:
                                if ((PortalContext.ContentPersonalizationEnabled && (PartInstance.VariantMode == VariantModeEnum.ContentPersonalization))
                                    || (PortalContext.MVTVariantsEnabled && (PartInstance.VariantMode == VariantModeEnum.MVT))
                                    || (PartInstance.VariantMode == VariantModeEnum.Conflicted))
                                {
                                    mRenderVariants = !IsWidget;
                                }
                                break;

                            case ViewModeEnum.Edit:
                            case ViewModeEnum.EditDisabled:
                                if (IsWidget)
                                {
                                    mRenderVariants = (PartInstance.ParentZone.WidgetZoneType == WidgetZoneTypeEnum.Editor);
                                }
                                break;
                        }
                    }
                }

                return mRenderVariants.Value;
            }
        }


        /// <summary>
        /// Indicates whether the web part should keep the cached output HTML during a postback request (note: postback request itself is not being cached).
        /// </summary>
        private bool PreservePartialCacheOnPostback
        {
            get
            {
                if (!mPreservePartialCacheOnPostback.HasValue)
                {
                    mPreservePartialCacheOnPostback = ValidationHelper.GetBoolean(GetValue("PreservePartialCacheOnPostback"), false);
                }

                return mPreservePartialCacheOnPostback.Value;
            }
        }

        #endregion


        #region "Public properties"

        /// <summary>
        /// Control's UI Context
        /// </summary>
        protected UIContext UIContext => mUIContext ?? (mUIContext = UIContextHelper.GetUIContext(this));


        /// <summary>
        /// Control's edited object
        /// </summary>
        protected object EditedObject
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
        /// Web part title container
        /// </summary>
        protected PlaceHolder TitleContainer => plcTitleContainer ?? (plcTitleContainer = new PlaceHolder());


        /// <summary>
        /// Gets or sets the value of the property
        /// </summary>
        /// <param name="propertyName">Property name</param>
        public object this[string propertyName]
        {
            get
            {
                return GetValue(propertyName);
            }
            set
            {
                SetValue(propertyName, value);
            }
        }


        /// <summary>
        /// Prefix for the resource strings which are used for the localization.
        /// </summary>
        public override string ResourcePrefix
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ResourcePrefix"), null);
            }
            set
            {
                SetValue("ResourcePrefix", value);
            }
        }


        /// <summary>
        /// Returns the Web part ID as registered in design mode
        /// </summary>
        public string WebPartID
        {
            get
            {
                if (PartInstance != null)
                {
                    return PartInstance.ControlID;
                }
                return ID;
            }
        }


        /// <summary>
        ///  Gets or sets the additional class which is added to the web part content panel
        /// </summary>
        public string AdditionalCssClass
        {
            get
            {
                // Add empty space before the CSS class if is defined
                if (!String.IsNullOrEmpty(mAdditionalCssClass))
                {
                    return " " + mAdditionalCssClass;
                }
                return mAdditionalCssClass;
            }
            set
            {
                mAdditionalCssClass = value;
            }
        }


        /// <summary>
        /// If true, the web part renders div with web part client ID around it.
        /// </summary>
        public virtual bool RenderEnvelope { get; set; }


        /// <summary>
        /// Custom data connected to the object.
        /// </summary>
        public virtual object RelatedData
        {
            get
            {
                return mRelatedData;
            }
            set
            {
                mRelatedData = value;
            }
        }


        /// <summary>
        /// Control context.
        /// </summary>
        public virtual string ControlContext => "WebPart_" + ID;


        /// <summary>
        /// Parent page manager.
        /// </summary>
        public virtual IPageManager PageManager
        {
            get
            {
                // Check for the Design Mode in Visual Studio
                if (Context == null)
                {
                    return mPageManager;
                }

                if (mPageManager == null)
                {
                    if (mPagePlaceholder != null)
                    {
                        mPageManager = mPagePlaceholder;
                    }
                    else
                    {
                        // Try to find placeholder first
                        mPagePlaceholder = PortalHelper.FindParentPlaceholder(this);
                        mPageManager = mPagePlaceholder ?? PortalHelper.FindPageManager(Page);
                    }

                    // If no manager found, throw exception
                    if (mPageManager == null)
                    {
                        throw new Exception("[CMSAbstractWebPart.PageManager]: Parent PageManager not found.");
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
        /// Returns true if the parent component has any variants
        /// </summary>
        public virtual bool ParentHasVariants
        {
            get
            {
                if (ParentZone == null)
                {
                    return false;
                }

                return ParentZone.HasVariants || ParentZone.ParentHasVariants;
            }
        }


        /// <summary>
        /// Returns true if the children components have any variants
        /// </summary>
        public virtual bool ChildrenHaveVariants => false;


        /// <summary>
        /// Parent zone.
        /// </summary>
        public virtual CMSWebPartZone ParentZone
        {
            get
            {
                if ((mParentZone == null) && !mParentZoneSearched)
                {
                    mParentZoneSearched = true;

                    // Try to find the web part zone as the parent control
                    mParentZone = (CMSWebPartZone)ControlsHelper.GetParentControl(this, typeof(CMSWebPartZone));
                }

                return mParentZone;
            }
            set
            {
                mParentZone = value;
            }
        }
        

        /// <summary>
        /// Parent page placeholder.
        /// </summary>
        public virtual CMSPagePlaceholder PagePlaceholder
        {
            get
            {
                return mPagePlaceholder ?? (mPagePlaceholder = PortalHelper.FindParentPlaceholder(this));
            }
            set
            {
                mPagePlaceholder = value;
            }
        }


        /// <summary>
        /// Returns the table of all the inner placeholders.
        /// </summary>
        public virtual List<CMSPagePlaceholder> ChildPagePlaceholders => mChildPagePlaceholders ?? (mChildPagePlaceholders = PortalHelper.CollectPlaceholders(this));


        /// <summary>
        /// Portal manager for the page.
        /// </summary>
        public virtual CMSPortalManager PortalManager
        {
            get
            {
                return PagePlaceholder?.PortalManager;
            }
            set
            {
                PagePlaceholder.PortalManager = value;
            }
        }
        

        /// <summary>
        /// Web part instance.
        /// </summary>
        public virtual WebPartInstance PartInstance
        {
            get
            {
                if ((mPartInstance == null) && (Parent is PartialCachingControl))
                {
                    // Try to get web part instance object from some of ancestor control if it is not set explicitly
                    CMSWebPartZone zone = ControlsHelper.GetParentControl<CMSWebPartZone>(this);
                    if (zone != null)
                    {
                        WebPartZoneInstance zoneInstance = zone.ZoneInstance;
                        mPartInstance = zoneInstance.GetWebPart(GetCachedWebPartOriginalID(Parent.ID));
                    }
                }

                return mPartInstance;
            }
            set
            {
                mPartInstance = value;
            }
        }


        /// <summary>
        /// Specifies whether the control is StandAlone or not, if false, the control is located within PortalEngine environment.
        /// </summary>
        public virtual bool StandAlone
        {
            get
            {
                if (PartInstance?.ParentZone != null && (ParentZone != null))
                {
                    return false;
                }

                return mStandAlone;
            }
        }


        /// <summary>
        /// Control page cycle status.
        /// </summary>
        public virtual PageCycleEnum PageCycle => mPageCycle;


        /// <summary>
        /// Returns true if the control processing should be stopped.
        /// </summary>
        public override bool StopProcessing
        {
            get
            {
                // Stop processing
                if (mStopProcessing)
                {
                    return true;
                }

                // Do not process if hidden
                if (HideOnCurrentPage)
                {
                    mStopProcessing = true;
                    return true;
                }

                return false;
            }
            set
            {
                mStopProcessing = value;
            }
        }


        /// <summary>
        /// Gets or sets whether the control is visible on the page.
        /// </summary>
        public override bool Visible
        {
            get
            {
                return base.Visible;
            }
            set
            {
                // Hide only virtually in new design mode or when widget in edit mode
                if (PortalContext.IsDesignMode(ViewMode)
                    || ((PortalContext.ViewMode.IsEditLive()) && !HideOnCurrentPage)
                    || (IsWidget && (ParentZone != null) && ParentZone.WebPartManagementRequired))
                {
                    IsVisible = value;
                }
                else
                {
                    base.Visible = value;
                }
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
                mNotResolveProperties = ";" + value.ToLowerInvariant().Trim(';') + ";";
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
                mSQLProperties = ";" + value.ToLowerInvariant().Trim(';') + ";";
            }
        }


        /// <summary>
        /// Page mode of the current web part.
        /// </summary>
        public virtual ViewModeEnum ViewMode
        {
            get
            {
                if (mViewMode != ViewModeEnum.Unknown)
                {
                    return mViewMode;
                }
                return GetCurrentViewmode();
            }
            set
            {
                mViewMode = value;
            }
        }


        /// <summary>
        /// Site name.
        /// </summary>
        public virtual string CurrentSiteName
        {
            get
            {
                if (StandAlone || (PageCycle <= PageCycleEnum.Created))
                {
                    // Not yet initialized, get SiteName from context
                    return SiteContext.CurrentSiteName;
                }
                // Get ViewMode from the page manager
                return PageManager.SiteName;
            }
        }


        /// <summary>
        /// Gets or sets the current layout info
        /// </summary>
        internal WebPartLayoutInfo CurrentLayoutInfo { get; set; }


        /// <summary>
        /// Web part info.
        /// </summary>
        public virtual WebPartInfo PartInfo
        {
            get
            {
                if (mPartInfo == null)
                {
                    if (PartInstance != null)
                    {
                        string webPartName = PartInstance.WebPartType;

                        if (IsWidget)
                        {
                            // If widget, get inherited web part
                            WidgetInfo wi = WidgetInfoProvider.GetWidgetInfo(webPartName);
                            if (wi != null)
                            {
                                mPartInfo = WebPartInfoProvider.GetWebPartInfo(wi.WidgetWebPartID);
                            }
                        }
                        else
                        {
                            // Get web part the standard way
                            mPartInfo = WebPartInfoProvider.GetWebPartInfo(webPartName);
                        }
                    }
                }

                return mPartInfo;
            }
        }


        /// <summary>
        /// Layout type of the parent zone
        /// </summary>
        public ZoneLayoutTypeEnum LayoutType
        {
            get
            {
                if (ParentZone != null)
                {
                    return ParentZone.LayoutType;
                }

                return ZoneLayoutTypeEnum.Standard;
            }
        }


        /// <summary>
        /// Returns the web part type.
        /// </summary>
        public virtual WebPartTypeEnum WebPartType
        {
            get
            {
                if (PartInfo != null)
                {
                    return (WebPartTypeEnum)PartInfo.WebPartType;
                }

                return WebPartTypeEnum.Basic;
            }
        }


        /// <summary>
        /// Returns true if the control should be hidden on current page (hide control on subpages in effect).
        /// </summary>
        public virtual bool HideOnCurrentPage
        {
            get
            {
                if ((ParentZone != null) && ParentZone.HideOnCurrentPage)
                {
                    return true;
                }

                if (PartInstance != null)
                {
                    // Hide in safe mode
                    if (PortalContext.IsDesignMode(ViewMode))
                    {
                        // Never hide layout web parts
                        if (this is CMSAbstractLayoutWebPart)
                        {
                            return false;
                        }

                        // Do not process in old design mode or safe mode
                        if (PortalHelper.SafeMode)
                        {
                            return true;
                        }
                    }

                    bool isVisible = IsVisible;

                    // Do not consider IsVisible property in OnSite editing
                    if (PortalContext.ViewMode.IsEditLive())
                    {
                        // Ensure that the content of this web part will not be rendered when the visibility condition is not fulfilled (only on-site edit span tags will be rendered)
                        if (!isVisible)
                        {
                            StopProcessing = true;
                        }

                        isVisible = true;
                    }

                    // Check hide on subpages settings (hide on sub pages, and inherited template or current page info not the last page info)
                    if (!isVisible || (HideOnSubPages && IsSubPage()))
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
                        if ((!string.Equals(role, RoleName.NOTAUTHENTICATED, StringComparison.InvariantCultureIgnoreCase) && currentUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin)) || currentUser.IsInRole(role, CurrentSiteName))
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

                if (currentPage != null)
                {
                    // Check show for document types condition
                    string classNames = ShowForDocumentTypes;
                    if (classNames != "")
                    {
                        bool classMatch = false;
                        string[] classes = classNames.Split(';');
                        string currentClass = currentPage.ClassName; // PagePlaceholder.PageInfo.ClassName.ToLowerCSafe()
                        foreach (string className in classes)
                        {
                            if (string.Equals(className, currentClass, StringComparison.InvariantCultureIgnoreCase))
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
                }

                return false;
            }
        }


        /// <summary>
        /// Gets the web part title label.
        /// </summary>
        public Label TitleLabel => lblWebPartTitle;


        /// <summary>
        /// Web part instance GUID.
        /// </summary>
        public virtual Guid InstanceGUID
        {
            get
            {
                if (PartInstance == null)
                {
                    return Guid.Empty;
                }
                return PartInstance.InstanceGUID;
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
                        shortId = "wp_" + instanceGuid.ToString("N");

                        if (IsVariant)
                        {
                            shortId += "_" + PartInstance.VariantID;
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
        /// Returns true if the web part is widget.
        /// </summary>
        public virtual bool IsWidget
        {
            get
            {
                if (PartInstance == null)
                {
                    return false;
                }

                return PartInstance.IsWidget;
            }
        }


        /// <summary>
        /// True if the web part is in design mode.
        /// </summary>
        public bool IsDesign
        {
            get
            {
                if (mIsDesign == null)
                {
                    mIsDesign = ParentZone.WebPartManagementRequired && (ViewMode != ViewModeEnum.DashboardWidgets);
                }

                return mIsDesign.Value;
            }
        }


        /// <summary>
        /// Returns true, if the web part represents layout
        /// </summary>
        public bool IsLayout => (WebPartType == WebPartTypeEnum.Layout) || (this is CMSAbstractLayoutWebPart);


        /// <summary>
        /// Title information.
        /// </summary>
        public virtual string TitleInfo
        {
            get
            {
                if (!String.IsNullOrEmpty(WebPartTitle))
                {
                    return WebPartTitle;
                }
                return mTitleInfo;
            }
            set
            {
                mTitleInfo = value;
                if (lblWebPartTitle != null)
                {
                    lblWebPartTitle.Text = mTitleInfo;
                }
            }
        }


        /// <summary>
        /// Content has been loaded.
        /// </summary>
        public virtual bool ContentLoaded { get; set; }


        /// <summary>
        /// Web part context resolver.
        /// </summary>
        public virtual MacroResolver ContextResolver
        {
            get
            {
                if (mContextResolver == null)
                {
                    mContextResolver = PortalUIHelper.GetControlResolver(Page, UIContext);

                    // Set related object
                    if (PagePlaceholder != null)
                    {
                        mContextResolver.Settings.RelatedObject = PagePlaceholder.TemplateInstance;
                    }

                    // Set the handler
                    mContextResolver.OnGetValue += GetValueEventHandler;
                }
                return mContextResolver;
            }
        }


        /// <summary>
        /// True if the web part was removed from current template.
        /// </summary>
        public virtual bool Removed
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["Removed"], false);
            }
            set
            {
                ViewState["Removed"] = value;
            }
        }


        /// <summary>
        /// Enabled.
        /// </summary>
        public virtual bool Enabled
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["Enabled"], true);
            }
            set
            {
                ViewState["Enabled"] = value;
            }
        }


        /// <summary>
        /// Update panel of the web part.
        /// </summary>
        public UpdatePanel UpdatePanel { get; internal set; }


        /// <summary>
        /// Indicates whether this web part is a variant of an existing web part.
        /// </summary>
        public bool IsVariant
        {
            get
            {
                if (PartInstance != null)
                {
                    return PartInstance.IsVariant;
                }

                return false;
            }
        }


        /// <summary>
        /// Indicates whether the web part has any variants.
        /// </summary>
        public bool HasVariants => PartInstance.HasVariants;


        /// <summary>
        /// If true, the header of the web part is hidden in design mode
        /// </summary>
        public bool HideHeader { get; set; }

        #endregion


        #region "Default properties"

        /// <summary>
        /// Width of the web part
        /// </summary>
        public virtual string WebPartWidth
        {
            get
            {
                return ValidationHelper.GetString(GetValue("WebPartWidth"), "");
            }
            set
            {
                SetValue("WebPartWidth", value);
            }
        }


        /// <summary>
        /// Height of the web part
        /// </summary>
        public virtual string WebPartHeight
        {
            get
            {
                return ValidationHelper.GetString(GetValue("WebPartHeight"), "");
            }
            set
            {
                SetValue("WebPartHeight", value);
            }
        }


        /// <summary>
        /// Name of the cache item the control will use.
        /// </summary>
        /// <remarks>
        /// By setting this name dynamically, you can achieve caching based on URL parameter or some other variable - simply put the value of the parameter to the CacheItemName property. If no value is set, the control stores its content to the item named "URL|ControlID".
        /// </remarks>
        public virtual string CacheItemName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("CacheItemName"), "");
            }
            set
            {
                SetValue("CacheItemName", value);
            }
        }


        /// <summary>
        /// Cache dependencies, each cache dependency on a new line.
        /// </summary>
        public virtual string CacheDependencies
        {
            get
            {
                // Evaluate the cache dependencies
                string dep = ValidationHelper.GetString(GetValue("CacheDependencies"), CacheHelper.DEFAULT_CACHE_DEPENDENCIES);

                return CacheHelper.GetCacheDependencies(dep, GetDefaultCacheDependendencies());
            }
            set
            {
                SetValue("CacheDependencies", value);
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
                // Return null if no context
                if (HttpContext.Current == null)
                {
                    return -1;
                }

                // Do not cache in live site mode
                if (PortalContext.ViewMode != ViewModeEnum.LiveSite)
                {
                    return 0;
                }

                // Get the value
                int result = ValidationHelper.GetInteger(GetValue("CacheMinutes"), -1);
                if (result < 0)
                {
                    // If not set, get from the site settings
                    result = SettingsKeyInfoProvider.GetIntValue(CurrentSiteName + ".CMSCacheMinutes");
                }

                return result;
            }
            set
            {
                SetValue("CacheMinutes", value);
            }
        }


        /// <summary>
        /// Web part title.
        /// </summary>
        public virtual string WebPartTitle
        {
            get
            {
                return ValidationHelper.GetString(GetValue("WebPartTitle"), "");
            }
            set
            {
                SetValue("WebPartTitle", value);
            }
        }


        /// <summary>
        /// Widget title.
        /// </summary>
        public virtual string WidgetTitle
        {
            get
            {
                string title = ValidationHelper.GetString(GetValue("WidgetTitle"), "");

                // If title empty, use widget display name
                if (String.IsNullOrEmpty(title) && (PartInstance != null))
                {
                    WidgetInfo wi = WidgetInfoProvider.GetWidgetInfo(PartInstance.WebPartType);
                    if (wi != null)
                    {
                        title = ResHelper.LocalizeString(wi.WidgetDisplayName);
                    }
                }
                return title;
            }
            set
            {
                SetValue("WidgetTitle", value);
            }
        }


        /// <summary>
        /// Web part CSS class.
        /// </summary>
        public virtual string CssClass
        {
            get
            {
                return ValidationHelper.GetString(GetValue("CssClass"), "");
            }
            set
            {
                SetValue("CssClass", value);
            }
        }


        /// <summary>
        /// Allows disabling of the web part viewstate.
        /// </summary>
        public virtual bool DisableViewState
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("DisableViewState"), false);
            }
            set
            {
                SetValue("DisableViewState", value);
                EnableViewState = !value;
            }
        }


        /// <summary>
        /// If true, macros are not resolved in the web part properties.
        /// </summary>
        public virtual bool DisableMacros
        {
            get
            {
                if (mDisableMacros == null)
                {
                    mDisableMacros = (PartInstance == null) || ValidationHelper.GetBoolean(GetValueInternal("DisableMacros", false), false);
                }

                return mDisableMacros.Value;
            }
            set
            {
                mDisableMacros = value;
                SetValue("DisableMacros", value);
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
        /// Skin ID.
        /// </summary>
        public override string SkinID
        {
            get
            {
                return ValidationHelper.GetString(GetValue("SkinID"), "");
            }
            set
            {
                SetValue("SkinID", value);
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
        /// Returns true if the webpart is visible.
        /// </summary>
        public virtual bool IsVisible
        {
            get
            {
                if (mIsVisible == null)
                {
                    mIsVisible = ValidationHelper.GetBoolean(GetValue("Visible"), true);
                }
                return mIsVisible.Value;
            }
            set
            {
                mIsVisible = value;
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
        /// Use update panel. This property is working only in Portal Engine mode. Don't use with ASPX templates.
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

        #endregion


        #region "Basic web part properties"

        /// <summary>
        /// Web part container object.
        /// </summary>
        public virtual WebPartContainerInfo Container
        {
            get
            {
                // Check if the correct container was loaded
                string containerName = ContainerName;
                if (mContainer == null || !string.Equals(mContainer.ContainerName, containerName, StringComparison.InvariantCultureIgnoreCase))
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
        public virtual string ContainerBefore
        {
            get
            {
                if (Container != null)
                {
                    var context = new MacroSettings
                    {
                        AvoidInjection = false,
                        AllowRecursion = !DisableMacros,
                        Culture = Thread.CurrentThread.CurrentCulture.ToString()
                    };

                    return ResolveMacros(Container.ContainerTextBefore, context);
                }
                return "";
            }
        }


        /// <summary>
        /// Container to render after the control.
        /// </summary>
        public virtual string ContainerAfter
        {
            get
            {
                if (Container != null)
                {
                    var context = new MacroSettings
                    {
                        AvoidInjection = false,
                        AllowRecursion = !DisableMacros,
                        Culture = Thread.CurrentThread.CurrentCulture.ToString()
                    };

                    return ResolveMacros(Container.ContainerTextAfter, context);
                }
                return "";
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

                if (PartInstance != null)
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

        #endregion


        #region "Fix XHTML properties"

        /// <summary>
        /// Enable output filter
        /// </summary>
        public bool EnableOutputFilter
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("EnableOutputFilter"), false);
            }
            set
            {
                SetValue("EnableOutputFilter", value);
            }
        }


        /// <summary>
        /// Resolve URLs
        /// </summary>
        public bool OutputResolveURLs
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("OutputResolveURLs"), true);
            }
            set
            {
                SetValue("OutputResolveURLs", value);
            }
        }


        /// <summary>
        /// Fix attributes
        /// </summary>
        public bool OutputFixAttributes
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("OutputFixAttributes"), true);
            }
            set
            {
                SetValue("OutputFixAttributes", value);
            }
        }


        /// <summary>
        /// Fix Javascript
        /// </summary>
        public bool OutputFixJavascript
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("OutputFixJavascript"), true);
            }
            set
            {
                SetValue("OutputFixJavascript", value);
            }
        }


        /// <summary>
        /// Fix lower case
        /// </summary>
        public bool OutputFixLowerCase
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("OutputFixLowerCase"), true);
            }
            set
            {
                SetValue("OutputFixLowerCase", value);
            }
        }


        /// <summary>
        /// Fix self closing tags
        /// </summary>
        public bool OutputFixSelfClose
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("OutputFixSelfClose"), true);
            }
            set
            {
                SetValue("OutputFixSelfClose", value);
            }
        }


        /// <summary>
        /// Fix HTML5
        /// </summary>
        public bool OutputFixHTML5
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("OutputFixHTML5"), true);
            }
            set
            {
                SetValue("OutputFixHTML5", value);
            }
        }


        /// <summary>
        /// Convert TABLE tags to DIV tags
        /// </summary>
        public ConvertTableEnum OutputConvertTablesToDivs
        {
            get
            {
                return HTMLHelper.GetConvertTableEnum(ValidationHelper.GetString(GetValue("OutputConvertTablesToDivs"), ""));
            }
            set
            {
                SetValue("OutputConvertTablesToDivs", value);
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor, initializes the parent portal manager.
        /// </summary>
        /// <param name="parentPlaceholder">Parent page placeholder</param>
        protected CMSAbstractWebPart(CMSPagePlaceholder parentPlaceholder)
        {
            mPagePlaceholder = parentPlaceholder;
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        protected CMSAbstractWebPart()
        {
        }


        /// <summary>
        /// Checks whether the page displayed in current context is considered a sub page
        /// </summary>
        private bool IsSubPage()
        {
            return ((PagePlaceholder.PageInfo.ChildPageInfo != null) || (PagePlaceholder.PageInfo.GetUsedPageTemplateId() != PagePlaceholder.PageInfo.UsedPageTemplateInfo.PageTemplateId));
        }


        /// <summary>
        /// Creates child controls within the web part.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            if (!StandAlone && !HideHeader && (!IsVariant || (mHeaderContainer != null)))
            {
                // Create the web part header
                CreateHeader();
            }

            // Use update panel
            if (UseUpdatePanel)
            {
                // Register current control as async postback control
                ScriptManager sm = ScriptManager.GetCurrent(Page);
                sm.RegisterAsyncPostBackControl(this);

                UpdatePanel = new CMSUpdatePanel();
                // Use "sys_" prefix to ensure no collision with another control id
                UpdatePanel.ID = "sys_pnlUpdate";
                UpdatePanel.UpdateMode = UpdatePanelUpdateMode.Conditional;

                // Add render placeholder container
                CMSUpdatePanelWebPartPlaceHolder plcUp = new CMSUpdatePanelWebPartPlaceHolder();
                plcUp.ID = "plcUp";
                plcUp.WebPart = this;
                UpdatePanel.ContentTemplateContainer.Controls.Add(plcUp);

                // Move the controls to the update panel
                ControlsHelper.MoveControls(this, plcUp, mHeaderControl);
                Controls.Add(UpdatePanel);
            }
        }


        /// <summary>
        /// Adds the header control
        /// </summary>
        /// <param name="parent">Parent control</param>
        /// <param name="header">Header control</param>
        /// <param name="createEnvelope">If true, envelope is created for the header to position it on top of the web part</param>
        private void AddHeaderControl(Control parent, Control header, bool createEnvelope)
        {
            if (createEnvelope)
            {
                var plc = new PlaceHolder();

                string envelopeClass = (IsLayout ? "LayoutWebPartHeaderEnvelope" : "WebPartHeaderEnvelope");

                envelopeClass += " cms-bootstrap";

                plc.Controls.Add(new LiteralControl("<div class=\"" + envelopeClass + "\">"));
                plc.Controls.Add(header);
                plc.Controls.Add(new LiteralControl("</div>"));

                header = plc;
            }

            parent.Controls.AddAt(0, header);

            mHeaderControl = header;
        }


        /// <summary>
        /// Gets the ID of the container encapsulating the web part
        /// </summary>
        protected string GetContainerId()
        {
            return ShortClientID + "_container";
        }


        /// <summary>
        /// Creates the web part header structure
        /// </summary>
        private void CreateHeader()
        {
            // Get the design mode settings
            ViewModeEnum viewMode = ParentZone.ViewMode;

            bool webPartManagement = ParentZone.WebPartManagementRequired;
            bool fullDesignMode = PortalContext.IsDesignMode(viewMode);
            bool isWidget = IsWidget;

            string parameter = $"webPartLocation['{GetContainerId()}']";

            if (webPartManagement)
            {
                // Design panel
                var pnlHeader = new Panel();
                // Use "sys_" prefix to ensure no collision with another control id
                pnlHeader.ID = "sys_pnlHeader";
                //pnlHeader.Attributes.Add("onmousemove", "return false;");
                pnlHeader.Attributes.Add("onmousedown", "return false;");

                // Add the header container
                if (mHeaderContainer == null)
                {
                    AddHeaderControl(this, pnlHeader, true);
                }
                else
                {
                    // Configuration on double click
                    if ((viewMode != ViewModeEnum.DesignDisabled) && !isWidget)
                    {
                        pnlHeader.Attributes.Add("ondblclick", configureScript + " return false;");
                    }

                    AddHeaderControl(mHeaderContainer, pnlHeader, true);
                }

                // Prepare the parameters
                string zoneId = PartInstance.ParentZone.ZoneID;

                var pageInfo = PagePlaceholder.PageInfo;
                string aliasPath = pageInfo.NodeAliasPath;
                string templateId = pageInfo.GetUsedPageTemplateId().ToString();

                string instanceGuid = InstanceGUID.ToString();
                string webPartName = PartInstance.ControlID;

                var configFunc = (isWidget ? "ConfigureWidget" : "ConfigureWebPart");

                configureScript = String.Format(
                    "{6}(new webPartProperties('{0}', '{1}', '{2}', '{3}', '{4}', '{5}'));",
                    HttpUtility.UrlEncode(zoneId),
                    HttpUtility.UrlEncode(webPartName),
                    HttpUtility.UrlEncode(aliasPath),
                    HttpUtility.UrlEncode(instanceGuid),
                    templateId,
                    ShortClientID,
                    configFunc
                );

                bool layout = IsLayout;

                if (fullDesignMode)
                {
                    // Base header class
                    pnlHeader.CssClass = layout ? "LayoutWebPartHeader" : "WebPartHeader";


                    pnlHeader.Attributes.Add("onmouseover", $"ActivateBorder('{ShortClientID}', this);");
                    pnlHeader.Attributes.Add("onmouseout", $"DeactivateBorder('{ShortClientID}', 50);");


                    // Context menu begin
                    if (ViewMode != ViewModeEnum.DesignDisabled)
                    {
                        if (!isWidget)
                        {
                            AddControl(pnlHeader, new LiteralControl(GetWebPartMenuStartTag(parameter)));
                        }
                        else
                        {
                            AddControl(pnlHeader, new LiteralControl(GetWidgetMenuStartTag(parameter)));
                        }
                    }

                    AddControl(pnlHeader, new LiteralControl("<table cellpadding=\"0\" cellspacing=\"0\" class=\"_nodivs\"><tr>"));

                    if (ViewMode != ViewModeEnum.DesignDisabled)
                    {
                        AddControl(pnlHeader, new LiteralControl("<td class=\"WebPartLeftAction\">"));

                        // Create menu icon
                        var menuIcon = new CMSIcon
                        {
                            CssClass = "icon-menu"
                        };

                        string cmFunctionName = (isWidget ? "WG_ContextMenu" : "WP_ContextMenu");
                        string onClickScript = $"return {cmFunctionName}(this, {parameter})";

                        menuIcon.Attributes.Add("onclick", onClickScript);
                        menuIcon.Attributes.Add("aria-hidden", "true");

                        AddControl(pnlHeader, menuIcon);
                        AddControl(pnlHeader, new LiteralControl("</td><td>"));
                    }
                    else
                    {
                        AddControl(pnlHeader, new LiteralControl("<td>"));
                    }

                    // Drag handle
                    Panel pnlHandle = new Panel();
                    pnlHandle.ID = "pnlHandle";

                    pnlHandle.CssClass = "WebPartHandle";

                    AddControl(pnlHeader, pnlHandle);

                    Control headerContainer = pnlHandle;

                    // Get webpart type
                    string iconClass = null;
                    if (!isWidget && (PartInfo != null))
                    {
                        // Web part type icon
                        switch (WebPartType)
                        {
                            case WebPartTypeEnum.DataSource:
                                pnlHeader.CssClass += " WebPartSource";
                                iconClass = "icon-database";
                                break;

                            case WebPartTypeEnum.Filter:
                                pnlHeader.CssClass += " WebPartFilter";
                                iconClass = "icon-funnel";
                                break;

                            case WebPartTypeEnum.Placeholder:
                                pnlHeader.CssClass += " WebPartPlaceholder";
                                iconClass = "icon-placeholder";
                                break;

                            case WebPartTypeEnum.Invisible:
                                pnlHeader.CssClass += " WebPartInvisible";
                                iconClass = "icon-eye-slash";
                                break;

                            case WebPartTypeEnum.Layout:
                                pnlHeader.CssClass += " WebPartLayout";
                                iconClass = "icon-layout";
                                break;
                        }
                    }

                    // Display web part type icon
                    if (iconClass != null)
                    {
                        // Set the image
                        CMSIcon imgType = new CMSIcon();
                        imgType.ID = "imgType";
                        imgType.CssClass = "WebPartTypeIcon " + iconClass;
                        headerContainer.Controls.Add(imgType);
                    }

                    // Display widget type text
                    if (isWidget)
                    {
                        LocalizedLabel lblZoneType = new LocalizedLabel();
                        lblZoneType.ID = "lblZoneType";
                        string zoneType = ParentZone.WidgetZoneType.ToStringRepresentation();
                        lblZoneType.ResourceString = "zone.type." + zoneType;
                        lblZoneType.CssClass = "WidgetType";
                        headerContainer.Controls.Add(lblZoneType);
                    }

                    // Add the variant place holder to the header container
                    if (RenderVariants || (IsVariant && (mHeaderContainer != null)))
                    {
                        plcVariantSlider = new PlaceHolder();
                        headerContainer.Controls.Add(plcVariantSlider);
                    }

                    // Title
                    lblWebPartTitle = new Label();
                    lblWebPartTitle.Text = HTMLHelper.HTMLEncode(TitleInfo);
                    lblWebPartTitle.CssClass = "WebPartTitle";
                    lblWebPartTitle.EnableViewState = false;

                    headerContainer.Controls.Add(TitleContainer);
                    TitleContainer.Controls.AddAt(0, lblWebPartTitle);

                    AddControl(pnlHeader, new LiteralControl("</td></tr></table>"));

                    // Context menu end
                    if (ViewMode != ViewModeEnum.DesignDisabled)
                    {
                        AddControl(pnlHeader, new LiteralControl(ContextMenuContainer.GetEndTag()));
                    }
                }
                else
                {
                    bool isEditorWidget = ((IsWidget && viewMode == ViewModeEnum.Edit) || (viewMode == ViewModeEnum.EditLive));

                    // User widgets mode (header always visible)
                    pnlHeader.CssClass = "WebPartHeader";

                    // Drag handle
                    Panel pnlHandle = new Panel();
                    pnlHandle.ID = "pnlHandle";
                    pnlHandle.CssClass = "WebPartHandle";
                    pnlHandle.Attributes.Add("onmouseover", "this.className = 'WebPartHandle ActionTop';");
                    pnlHandle.Attributes.Add("onmouseout", "this.className = 'WebPartHandle';");

                    AddControl(pnlHeader, pnlHandle);

                    Control headerContainer = pnlHandle;

                    // Hide the the drag button for the user widgets and the dashboard widgets
                    if ((viewMode != ViewModeEnum.UserWidgets)
                        && (viewMode != ViewModeEnum.UserWidgetsDisabled)
                        && (viewMode != ViewModeEnum.DashboardWidgets))
                    {
                        if ((this is CMSAbstractEditableWebPart) && ViewMode.IsEditLive())
                        {
                            // Get dialog witdh
                            var editableWebPart = (CMSAbstractEditableWebPart)this;
                            int dialogWidth = ValidationHelper.GetInteger(editableWebPart.EditDialogWidth, 0);
                            int dialogWidthFromWebPartProperties = ValidationHelper.GetInteger(GetValue("DialogWidth"), 0);
                            if (dialogWidthFromWebPartProperties > 0)
                            {
                                dialogWidth = dialogWidthFromWebPartProperties;
                            }

                            // Create edit dialog URL
                            string editPageUrl = editableWebPart.EditPageUrl;
                            editPageUrl = OnSiteEditHelper.GetEditDialogURL(editPageUrl, PagePlaceholder.PageInfo, PartInstance);

                            // Create icon
                            CMSIcon imgSelect = new CMSIcon
                            {
                                ID = "imgSelect",
                                CssClass = "WebPartActionButton icon-edit"
                            };

                            // Script for icon
                            string script = "modalDialog('" + editPageUrl + "', 'editpage', '" + ((dialogWidth > 0) ? dialogWidth : 1000) + "', '90%');";
                            imgSelect.Attributes["onclick"] = script + "return false";

                            headerContainer.Controls.Add(imgSelect);
                        }

                        // Drag image
                        CMSIcon imgDrag = new CMSIcon();
                        imgDrag.ID = "imgDrag";
                        imgDrag.CssClass = "WebPartActionButton WidgetDrag icon-arrows";
                        headerContainer.Controls.Add(imgDrag);
                    }

                    // Add the variant slider place holder to the header container
                    if (RenderVariants)
                    {
                        plcVariantSlider = new PlaceHolder();
                        headerContainer.Controls.Add(plcVariantSlider);
                    }

                    // In user widgets and dashboard mode, display a header with title and buttons on the right side
                    var displayFullWidthHeader = viewMode.IsOneOf(ViewModeEnum.UserWidgets, ViewModeEnum.UserWidgetsDisabled, ViewModeEnum.DashboardWidgets);

                    PlaceHolder plcActionButtons = new PlaceHolder();
                    plcActionButtons.ID = "pnlAc";

                    // Indicates whether float class was added and should be cleared
                    bool floatAdded = false;

                    // In user widgets and dashboard mode, move the action buttons to the right from the title
                    if (displayFullWidthHeader)
                    {
                        Panel pnlButtons = new Panel();
                        pnlButtons.CssClass = "FloatRight";
                        headerContainer.Controls.Add(pnlButtons);

                        // Wrap the action buttons into a div element 
                        pnlButtons.Controls.Add(plcActionButtons);
                        floatAdded = true;
                    }
                    else
                    {
                        headerContainer.Controls.Add(plcActionButtons);
                    }

                    // Add title to the user/dashboard widget header
                    if (displayFullWidthHeader)
                    {
                        // Title
                        var heading = new LocalizedHeading
                        {
                            Level = 3,
                            Text = HTMLHelper.HTMLEncode(WidgetTitle),
                            EnableViewState = false,
                            CssClass = "FloatLeft"
                        };
                        lblWebPartTitle = heading;
                        headerContainer.Controls.Add(lblWebPartTitle);

                        floatAdded = true;
                    }

                    // Add clearing class for floats
                    if (floatAdded)
                    {
                        headerContainer.Controls.Add(new LiteralControl()
                        {
                            EnableViewState = false,
                            Text = @"<div class=""ClearBoth""></div>"
                        });
                    }

                    // Configure image
                    CMSIcon imgConfigure = new CMSIcon();
                    imgConfigure.ID = "imgConfigure";
                    imgConfigure.CssClass = "icon-cogwheel WebPartActionButton";
                    imgConfigure.ToolTip = PortalHelper.LocalizeStringForUI("Widgets.Configure");

                    if (viewMode == ViewModeEnum.UserWidgetsDisabled)
                    {
                        imgConfigure.Attributes["onclick"] = "CannotModifyUserWidgets(); return false;";
                    }
                    else
                    {
                        if (!isEditorWidget)
                        {
                            imgConfigure.Attributes["onclick"] = configureScript + " return false;";
                        }
                        else
                        {
                            imgConfigure.Attributes["onclick"] = "OpenMenuConfWidget(this, '" + ShortClientID + "'); return false;";
                        }
                    }
                    plcActionButtons.Controls.Add(imgConfigure);

                    if (displayFullWidthHeader)
                    {
                        // Minimize image
                        CMSIcon minimizeIcon = new CMSIcon();
                        minimizeIcon.ID = "iconMinimized";

                        if (PartInstance.Minimized)
                        {
                            minimizeIcon.ToolTip = PortalHelper.LocalizeStringForUI("Widgets.Maximize");
                            minimizeIcon.CssClass = "icon-modal-maximize WebPartActionButton";
                        }
                        else
                        {
                            minimizeIcon.ToolTip = PortalHelper.LocalizeStringForUI("Widgets.Minimize");
                            minimizeIcon.CssClass = "icon-modal-minimize WebPartActionButton";
                        }

                        if (viewMode == ViewModeEnum.UserWidgetsDisabled)
                        {
                            minimizeIcon.Attributes["onclick"] = "CannotModifyUserWidgets(); return false;";
                        }
                        else
                        {
                            minimizeIcon.Attributes["onclick"] = "ToggleMinimizeWidget(this, '" + ShortClientID + "', '" + zoneId + "', '" + webPartName + "', '" + aliasPath + "'); return false;";
                        }

                        plcActionButtons.Controls.Add(minimizeIcon);
                    }

                    if (!isEditorWidget)
                    {
                        // Delete button
                        CMSIcon deleteIcon = new CMSIcon();
                        deleteIcon.ID = "iconDelete";
                        deleteIcon.CssClass = "icon-bin WebPartActionButton";
                        deleteIcon.ToolTip = PortalHelper.LocalizeStringForUI("widgets.remove");

                        if (viewMode == ViewModeEnum.UserWidgetsDisabled)
                        {
                            deleteIcon.Attributes["onclick"] = "CannotModifyUserWidgets(); return false;";
                        }
                        else
                        {
                            deleteIcon.Attributes["onclick"] = "RemoveWidget(new webPartProperties('" + zoneId + "', '" + webPartName + "', '" + aliasPath + "', '" + instanceGuid + "')); return false;";
                        }

                        plcActionButtons.Controls.Add(deleteIcon);
                    }

                }

                // Script to set the properties of the web part
                if (mProvideSetPropertyScript)
                {
                    string script = "function SetWebPartProperty_" + ShortClientID + "(propertyName, value) { SetWebPartProperty('" + zoneId + "', '" + webPartName + "', '" + aliasPath + "', '" + instanceGuid + "', propertyName, value); }";

                    ScriptHelper.RegisterClientScriptBlock(this, typeof(string), "SetWebPartProperty_" + ShortClientID, ScriptHelper.GetScript(script));
                }
            }
            else if (CPWebPartInEdit)
            {
                // Design panel
                var pnlHeader = new Panel();
                pnlHeader.ID = "sys_pnlHeader";
                pnlHeader.CssClass = "CPMenuBorder";

                Panel pnl = new Panel();
                pnl.CssClass = "CPMenuWebPart";
                pnl.Attributes.Add("onclick", "currentContextMenuId = '" + ShortClientID + "'; ContextMenu('cpVariantList', parentNode, GetVariantInfoArray(" + parameter + ").join(), true); return false;");

                // Set the image
                Image imgVariants = new Image();
                imgVariants.ID = "imgVarList";
                imgVariants.ImageUrl = GetImageUrl("CMSModules/CMS_PortalEngine/Menu.png");
                imgVariants.CssClass = "CPMenuButton";
                pnl.Controls.Add(imgVariants);

                AddControl(pnlHeader, pnl);

                AddHeaderControl(this, pnlHeader, true);
            }
        }


        /// <summary>
        /// Adds the given control to the specified parent
        /// </summary>
        /// <param name="parent">Parent control</param>
        /// <param name="ctrl">Control to add</param>
        private static void AddControl(Panel parent, Control ctrl)
        {
            parent.Controls.Add(ctrl);
        }


        /// <summary>
        /// Gets the start tag for the widget context menu
        /// </summary>
        /// <param name="parameter">Menu parameter</param>
        private static string GetWidgetMenuStartTag(string parameter)
        {
            return ContextMenuContainer.GetStartTag("widgetMenu", parameter);
        }


        /// <summary>
        /// Gets the start tag for the web part context menu
        /// </summary>
        /// <param name="parameter">Menu parameter</param>
        private static string GetWebPartMenuStartTag(string parameter)
        {
            return ContextMenuContainer.GetStartTag("webPartMenu", parameter, false, HtmlTextWriterTag.Div, null, null, null, false, "WP_GetMenuContext");
        }


        /// <summary>
        /// Creates ID for partial cached web part. Pre-defined prefix is added to original ID.
        /// </summary>
        /// <param name="webPartId">ID of the web part</param>
        internal static string CreateCachedWebPartID(string webPartId)
        {
            if (webPartId == null)
            {
                return null;
            }

            return ValidationHelper.GetIdentifier("c" + webPartId);
        }


        /// <summary>
        /// Gets original ID from which the cached web part ID was created.
        /// </summary>
        /// <param name="cachedWebPartId">Cached web part ID</param>
        internal static string GetCachedWebPartOriginalID(string cachedWebPartId)
        {
            return ((cachedWebPartId != null) && cachedWebPartId.StartsWith("c", StringComparison.Ordinal)) ? cachedWebPartId.Substring(1) : cachedWebPartId;
        }


        /// <summary>
        /// Init event handler.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            // Set current page cycle (Must be set before EnsureChildControls is called)
            mPageCycle = PageCycleEnum.Initializing;

            EnsureChildControls();

            if (StopProcessing)
            {
                // No actions if processing is stopped
                base.OnInit(e);
            }
            else
            {
                if (!StandAlone && !(Parent is PartialCachingControl))
                {
                    // Load the content if not loaded yet
                    if (!ContentLoaded)
                    {
                        LoadContent(PartInstance, false);
                    }

                    // Handle the default init event
                    if (PortalContext.IsDesignMode(PagePlaceholder.ViewMode))
                    {
                        try
                        {
                            base.OnInit(e);
                        }
                        catch (Exception ex)
                        {
                            EventLogProvider.LogException("Design", "WebPart", ex);
                        }
                    }
                    else
                    {
                        base.OnInit(e);
                    }
                }
                else
                {
                    base.OnInit(e);
                }

                // Load control extender
                var extenderAssemblyName = GetStringValue("ExtenderAssemblyName", null);
                var extenderClassName = GetStringValue("ExtenderClassName", null);
                var webPartRendersContent = !(this is WebPartBlank);

                if (!string.IsNullOrEmpty(extenderAssemblyName) && !string.IsNullOrEmpty(extenderClassName) && webPartRendersContent)
                {
                    try
                    {
                        ControlsHelper.LoadExtender(extenderAssemblyName, extenderClassName, this);
                    }
                    catch (Exception ex)
                    {
                        // Log exception
                        CoreServices.EventLog.LogException("CMSAbstractUIWebpart", "OnInit", ex);

                        // Add error web part
                        errorWebPart = new WebPartError
                        {
                            InnerException = ex,
                        };

                        StopProcessing = true;
                        Controls.Add(errorWebPart);
                    }
                }

                // Handle init complete for wizard events
                PageContext.InitComplete += PageHelper_InitComplete;
            }

            mPageCycle = PageCycleEnum.Initialized;
        }


        /// <summary>
        /// Page.InitComplete
        /// </summary>
        void PageHelper_InitComplete(object sender, EventArgs e)
        {
            if (DocumentWizardManager != null)
            {
                ComponentEvents.RequestEvents.RegisterForComponentEvent<StepEventArgs>("PageWizardManager", ComponentEvents.LOAD_STEP, null, LoadStep);
                ComponentEvents.RequestEvents.RegisterForComponentEvent<StepEventArgs>("PageWizardManager", ComponentEvents.STEP_LOADED, null, StepLoaded);
                ComponentEvents.RequestEvents.RegisterForComponentEvent<StepEventArgs>("PageWizardManager", ComponentEvents.VALIDATE_STEP, null, ValidateStepData);
                ComponentEvents.RequestEvents.RegisterForComponentEvent<StepEventArgs>("PageWizardManager", ComponentEvents.FINISH_STEP, null, SaveStepData);
                ComponentEvents.RequestEvents.RegisterForComponentEvent<StepEventArgs>("PageWizardManager", ComponentEvents.STEP_FINISHED, null, StepFinished);
            }
        }


        #region "Page wizard virtual methods"

        /// <summary>
        /// Allows you to execute additional logic after load step event.
        /// </summary>
        /// <param name="e">The StepEventArgs instance containing the event data.</param>
        /// <param name="sender">Sender</param>
        protected virtual void StepLoaded(object sender, StepEventArgs e)
        {
            // Must be implemented by inherited control
        }


        /// <summary>
        /// Loads the step.
        /// </summary>
        /// <param name="e">The StepEventArgs instance containing the event data.</param>
        /// <param name="sender">Sender</param>
        protected virtual void LoadStep(object sender, StepEventArgs e)
        {
            // Must be implemented by inherited control
        }


        /// <summary>
        /// Validates the wizard step data.
        /// </summary>
        /// <param name="e">The StepEventArgs instance containing the event data.</param>
        /// <param name="sender">Sender</param>
        protected virtual void ValidateStepData(object sender, StepEventArgs e)
        {
            // Must be implemented by inherited control
        }


        /// <summary>
        /// Saves the wizard step data.
        /// </summary>
        /// <param name="e">The StepEventArgs instance containing the event data.</param>
        /// <param name="sender">Sender</param>
        protected virtual void SaveStepData(object sender, StepEventArgs e)
        {
            // Must be implemented by inherited control
        }


        /// <summary>
        /// Allows you to execute additional logic after finish/save step event.
        /// </summary>
        /// <param name="e">The StepEventArgs instance containing the event data.</param>
        /// <param name="sender">Sender</param>
        protected virtual void StepFinished(object sender, StepEventArgs e)
        {
            // Must be implemented by inherited control
        }

        #endregion


        /// <summary>
        /// Ensures the web part variants
        /// </summary>
        public void EnsureVariants()
        {
            if (RenderVariants && !variantsLoaded && HasVariants)
            {
                EnsureChildControls();

                variantsLoaded = true;

                mVariants = new List<CMSAbstractWebPart>();

                // Load the content for the web part variants
                if (PartInstance?.PartInstanceVariants != null)
                {
                    // Loop thru all variants
                    foreach (WebPartInstance instance in PartInstance.PartInstanceVariants)
                    {
                        // Load web part control
                        CMSAbstractWebPart webpart = CMSWebPartZone.LoadWebPart(this, ParentZone, instance, true, true) as CMSAbstractWebPart;

                        if (webpart != null)
                        {
                            // Add to the variants collection
                            VariantControlsPlaceHolder.Controls.Add(webpart);

                            mVariants.Add(webpart);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Load event handler.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            if (StopProcessing)
            {
                // No actions if processing is stopped
                base.OnLoad(e);
            }
            else
            {
                mPageCycle = PageCycleEnum.Loading;
                EnsureChildControls();

                if (!StandAlone)
                {
                    if (PortalContext.IsDesignMode(PagePlaceholder.ViewMode))
                    {
                        try
                        {
                            base.OnLoad(e);
                        }
                        catch (Exception ex)
                        {
                            EventLogProvider.LogException("Design", "WebPart", ex);
                        }
                    }
                    else
                    {
                        base.OnLoad(e);
                    }

                    if ((plcVariantSlider != null) && (PartInstance != null))
                    {
                        // Add Variation slider
                        if (PartInstance.VariantMode == VariantModeEnum.Conflicted)
                        {
                            // Contains both MVT and Content personalization variants => display warning
                            plcVariantSlider.Controls.Add(new LiteralControl("<div class=\"" + ((!IsWidget) ? "SliderConflict" : "SliderWidgetConflict") + "\" title=\"" + PortalHelper.LocalizeStringForUI("variantslider.viewmodeconflicttooltip") + "\">" + PortalHelper.LocalizeStringForUI("variantslider.viewmodeconflict") + "</div>"));
                        }
                        else if ((ParentZone != null) && !ParentZone.HasVariants && !ParentZone.IsVariant)
                        {
                            // Render envelope in case that the header is positioned in a container
                            bool renderEnvelope = (mHeaderContainer != null);
                            if (renderEnvelope)
                            {
                                plcVariantSlider.Controls.Add(new LiteralControl("<div id=\"" + GetVariantID() + "_slider\">"));
                            }

                            // Render the variants only if parent zone doesn't have variants
                            if ((HasVariants || IsWidget) && !IsVariant)
                            {
                                // Render the variant slider
                                Control pnlVariantSlider = Page.LoadUserControl("~/CMSModules/OnlineMarketing/Controls/Content/VariantSlider.ascx");
                                pnlVariantSlider.ID = "var";
                                plcVariantSlider.Controls.Add(pnlVariantSlider);
                            }

                            if (renderEnvelope)
                            {
                                plcVariantSlider.Controls.Add(new LiteralControl("</div>"));
                            }
                        }
                    }
                }
                else
                {
                    base.OnLoad(e);
                }

                // Fire OnContentLoaded (for web part outside the Portal page)
                if (!ContentLoaded)
                {
                    OnContentLoaded();
                }
            }

            mPageCycle = PageCycleEnum.Loaded;
        }


        /// <summary>
        /// PreRender event handler.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            if (StopProcessing)
            {
                // No actions if processing is stopped
                base.OnPreRender(e);
            }
            else
            {
                mPageCycle = PageCycleEnum.PreRendering;

                if (!StandAlone)
                {
                    var viewMode = PagePlaceholder.ViewMode;

                    if (PortalContext.IsDesignMode(viewMode))
                    {
                        try
                        {
                            base.OnPreRender(e);
                        }
                        catch (Exception ex)
                        {
                            EventLogProvider.LogException("Design", "WebPart", ex);
                        }
                    }
                    else
                    {
                        base.OnPreRender(e);
                    }
                }
                else
                {
                    base.OnPreRender(e);
                }

                if (mNeedsLayoutScript)
                {
                    // Register the layout script
                    PortalHelper.RegisterLayoutsScript(Page);
                }

                // Register the container
                if (!ContainerHideOnCurrentPage)
                {
                    PortalContext.CurrentComponents.RegisterWebPartContainer(Container);
                }

                // Register current web part component
                if (PartInfo != null)
                {
                    PortalContext.CurrentComponents.RegisterWebPart(PartInfo);
                }

                // Register current web part layout component 
                if (CurrentLayoutInfo != null)
                {
                    PortalContext.CurrentComponents.RegisterWebPartLayout(CurrentLayoutInfo);
                }
            }

            mPageCycle = PageCycleEnum.PreRendered;
        }


        /// <summary>
        /// Gets the variant ID for rendering
        /// </summary>
        protected string GetVariantID()
        {
            if (IsVariant)
            {
                return "Variant_" + VariantModeFunctions.GetVariantModeString(PartInstance.VariantMode) + "_" + PartInstance.VariantID;
            }
            return "Variant_WP_" + PartInstance.InstanceGUID.ToString("N");
        }


        /// <summary>
        /// Gets the script defining the web part location
        /// </summary>
        private string GetLocationScript()
        {
            PageInfo pi = PagePlaceholder.PageInfo;
            if (pi != null)
            {
                WebPartZoneInstance parentZone = PartInstance.ParentZone;
                PageTemplateInfo pti = pi.UsedPageTemplateInfo;
                PageTemplateTypeEnum pageTemplateType = (pti != null) ? pti.PageTemplateType : PageTemplateTypeEnum.Unknown;
                WidgetZoneTypeEnum zoneType = parentZone.WidgetZoneType;

                string script = ScriptHelper.GetScript(String.Format(
                    "webPartLocation['{7}'] = new webPartProperties('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', {6}, {8}, '{9}')",
                    parentZone.ZoneID,
                    PartInstance.ControlID,
                    pi.NodeAliasPath,
                    (InstanceGUID != Guid.Empty ? InstanceGUID.ToString() : ""),
                    pi.GetUsedPageTemplateId(),
                    ShortClientID,
                    PartInstance.VariantID,
                    GetContainerId(),
                    WebPartClipBoardManager.ContainsClipBoardItem(zoneType, pageTemplateType).ToString().ToLowerInvariant(),
                    WebPartClipBoardManager.GetKey(zoneType, pageTemplateType, false)
                ));

                return script;
            }

            return null;
        }


        /// <summary>
        /// Load the content to the web part.
        /// </summary>
        /// <param name="partInstance">Web part instance with initialization data</param>
        /// <param name="reloadData">Reload control data</param>
        public void LoadContent(WebPartInstance partInstance, bool reloadData = true)
        {
            PartInstance = partInstance;

            if (partInstance != null)
            {
                // Setups the web part partial cache when enabled
                SetupPartialCache(partInstance);

                // Set the control cycle status
                if (mPageCycle < PageCycleEnum.ParentInitializing)
                {
                    mPageCycle = PageCycleEnum.ParentInitializing;
                }

                // Get original or variant control id
                string controlId = partInstance.ControlID;
                if (partInstance.CurrentVariantInstance != null)
                {
                    controlId = partInstance.CurrentVariantInstance.ControlID;
                }

                // Get unique id, uses custom replacement due to possible collision with simple underscore
                string id = ValidationHelper.GetIdentifier(controlId, "_wuid_");

                // Set the title information
                if (string.IsNullOrEmpty(lblWebPartTitle?.Text))
                {
                    TitleInfo = id;
                }

                ID = id;

                EnableViewState = !DisableViewState;

                mStandAlone = false;

                // Ensure visible state
                Visible = (IsVisible & !HideOnCurrentPage);

                // Load web part variants
                ExecuteForAllVariants(webPart => webPart.LoadContent(webPart.PartInstance, reloadData));
            }
        }


        /// <summary>
        /// Setups the partial cache when enabled.
        /// </summary>
        private void SetupPartialCache(WebPartInstance partInstance)
        {
            var cachedControl = Parent as PartialCachingControl;

            if (cachedControl == null)
            {
                return;
            }

            bool varyBysNotConfigured = cachedControl.CachePolicy.VaryByParams[PartialCacheHelper.VARY_BY_NOT_CONFIGURED];
            if (varyBysNotConfigured)
            {
                // Always setup correct VaryBy... settings, even if the output cache will be disabled.
                // When the cache is disabled, it must be disabled for the correct VaryBy... settings to make sure
                // that the cache is not accidentally cleared for the same web part (placed in master page) rendered on another page.
                SetVaryByCustom(cachedControl);
                SetVaryByParams(cachedControl);
            }

            if (!CanUsePartialCache())
            {
                // Setting up a cache dependency that expires immediately will disable the partial cache
                cachedControl.Dependency = new CacheDependency(null, new[] { "notcache" }, DateTime.MinValue);

                // Make sure that this cached web part (GET variant) is cleared in all the web farm instances after postback
                CacheHelper.TouchKey(GetPostbackExpiracyCacheKey());

                CacheDebug.LogCacheOperation(CacheOperation.REMOVE, "Partial cache (" + partInstance.ControlID + ")", null, null, DateTime.MinValue, Cache.NoSlidingExpiration, CMSCacheItemPriority.NotRemovable, true);
            }
            else
            {
                int cacheMinutes = ValidationHelper.GetInteger(GetValue("PartialCacheMinutes"), 0);
                if (cacheMinutes > 0)
                {
                    if (!PreservePartialCacheOnPostback)
                    {
                        // Create a unique cache dummy key which will be used for web farm cache synchronization if the web part partial cache should be cleared after a postback.
                        // This "expiracy key" is then added to the web part's cache dependency.
                        DateTime expires = DateTime.Now.AddMinutes(cacheMinutes);
                        CacheHelper.EnsureKey(GetPostbackExpiracyCacheKey(), expires);
                    }

                    cachedControl.CachePolicy.Duration = new TimeSpan(0, cacheMinutes, 0);

                    var cacheDependencies = GetPartialCacheDependencies();
                    if (cacheDependencies != null)
                    {
                        cachedControl.Dependency = cacheDependencies.CreateCacheDependency();
                    }

                    CacheDebug.LogCacheOperation(CacheOperation.ADD, "Partial cache (" + partInstance.ControlID + ")", null, cacheDependencies, DateTime.Now.AddMinutes(cacheMinutes), Cache.NoSlidingExpiration, CMSCacheItemPriority.NotRemovable, true);
                }
            }
        }


        private bool CanUsePartialCache()
        {
            // Use partial cache only on the live site - test the live site view mode explicitly
            //    to make sure that partial cache will be disabled even if specified in the custom layout using the <%@ OutputCache %> directive

            // Cache POST variant of the control only if "preserve on postback" is enabled

            return ViewMode.IsLiveSite() && (!RequestHelper.IsPostBack() || PreservePartialCacheOnPostback);
        }


        private CMSCacheDependency GetPartialCacheDependencies()
        {
            string dependencies = ValidationHelper.GetString(GetValue("PartialCacheDependencies"), CacheHelper.NO_CACHE_DEPENDENCIES);
            string defaultDependencies = GetDefaultCacheDependendencies();

            // If the current web part is editable, add default dependency to current document to make sure that the cached web part is cleared when the document content is re-saved.
            if (this is CMSAbstractEditableWebPart)
            {
                PageInfo currentPage = DocumentContext.CurrentPageInfo;
                if (currentPage != null)
                {
                    defaultDependencies += "\ndocumentid|" + currentPage.DocumentID;
                }
            }

            StringBuilder dep = new StringBuilder();
            dep.Append(CacheHelper.GetCacheDependencies(dependencies, defaultDependencies));
            dep.Append("\n", CacheHelper.PARTIAL_KEY, "\ntemplate|", ParentZone.ZoneInstance.ParentTemplateInstance.ParentPageTemplate.PageTemplateId);

            if (!PreservePartialCacheOnPostback)
            {
                // Append a dependency to the web part's expiration key.
                // This key is being touched by the postback requests.
                dep.Append("\n", GetPostbackExpiracyCacheKey());
            }

            return CacheHelper.GetCacheDependency(dep.ToString());
        }


        /// <summary>
        /// Get a unique cache dummy key which will be used for web farm cache synchronization if the web part partial cache should be cleared after postback.
        /// </summary>
        private string GetPostbackExpiracyCacheKey()
        {
            string cacheItems = PartialCacheItemsProvider.GetEnabledCacheItems();
            var viewMode = new ViewModeOnDemand();
            var siteName = new SiteNameOnDemand();
            string currentAliasPath = DocumentContext.CurrentAliasPath;
            string webPartAliasPath = PagePlaceholder.PageInfo?.NodeAliasPath;

            // Use just one expiracy key for all query parameters variations
            // Unique web part identification consist of 1) alias path on which the web part is located and 2) its web part control ID 
            // Include CurrentAliasPath to distinguish possibly different content on child pages (i.e. Repeater with an empty Path parameter uses a current path on child pages)
            return CacheHelper.GetCacheItemName(null, "controlexpiracy", OutputHelper.GetContextCacheString(cacheItems, viewMode, siteName), currentAliasPath, webPartAliasPath, WebPartID);
        }


        /// <summary>
        /// Sets the VaryByCustom according to the web part settings
        /// </summary>
        private void SetVaryByCustom(PartialCachingControl cachedControl)
        {
            string varyByCustom = "control;" + PartialCacheItemsProvider.GetEnabledCacheItems();

            if (PreservePartialCacheOnPostback)
            {
                // Add a "flag" to make sure that postback request does not create a new cache object when evaluating VaryByCustom parameters
                // when the "PreservePartialCacheOnPostback" is enabled for this web part
                varyByCustom += ";preserveonpostback";
            }

            cachedControl.CachePolicy.SetVaryByCustom(varyByCustom);
        }


        /// <summary>
        /// Sets the VaryByParam according to the web part settings
        /// </summary>
        private void SetVaryByParams(PartialCachingControl cachedControl)
        {
            if (PreservePartialCacheOnPostback)
            {
                // Disable all form POST values to ensure using the same cache object for POST requests.
                // Drawback: query string parameters (except "aliaspath") are ignored as well.
                // "Aliaspath" must be specified because portal engine rewrites the current URL to the "PortalTemplate.aspx?aliaspath=..." page.
                cachedControl.CachePolicy.VaryByParams["aliaspath"] = true;
            }
            else
            {
                // Make sure that web part output HTML will be cached independently for different query string values
                cachedControl.CachePolicy.VaryByParams["*"] = true;
            }
        }


        /// <summary>
        /// Executes the given action for all nested web parts
        /// </summary>
        /// <param name="action">Action to execute</param>
        protected void ExecuteForAllVariants(Action<CMSAbstractWebPart> action)
        {
            // Process web part variants
            if (mVariants == null)
            {
                return;
            }

            foreach (var part in mVariants)
            {
                action(part);
            }
        }


        /// <summary>
        /// Method that is called when the page content is loaded, override to implement the control initialization after the content has been loaded.
        /// </summary>
        public virtual void OnContentLoaded()
        {
            ContentLoaded = true;

            ExecuteForAllVariants(webPart => webPart.OnContentLoaded());
        }


        /// <summary>
        /// Causes reloading the data, override to implement the data reloading procedure.
        /// </summary>
        public virtual void ReloadData()
        {
            ContentLoaded = true;

            ExecuteForAllVariants(webPart => webPart.ReloadData());
        }


        /// <summary>
        /// Causes clearing the control data cache, override to implement the data reloading procedure.
        /// </summary>
        public virtual void ClearCache()
        {
            ExecuteForAllVariants(webPart => webPart.ClearCache());
        }


        /// <summary>
        /// Removes the variant from cache.
        /// </summary>
        /// <param name="variantId">The variant id</param>
        public virtual void RemoveVariantFromCache(int variantId)
        {
            if (HasVariants)
            {
                CMSAbstractWebPart webPartToRemove = null;
                foreach (CMSAbstractWebPart webpart in VariantControlsPlaceHolder.Controls)
                {
                    if ((webpart.IsVariant)
                        && (webpart.PartInstance != null)
                        && (webpart.PartInstance.VariantID == variantId))
                    {
                        webPartToRemove = webpart;
                        PartInstance.PartInstanceVariants.Remove(webpart.PartInstance);

                        // Set the variant mode to NONE when there are no other variants left
                        if (PartInstance.PartInstanceVariants.Count == 0)
                        {
                            PartInstance.VariantMode = VariantModeEnum.None;
                            PartInstance.ParentZone.VariantMode = VariantModeEnum.None;
                        }

                        WebPartInstance wpi = PartInstance.ParentZone.ParentTemplateInstance.GetWebPart(webPartToRemove.InstanceGUID);
                        wpi?.PartInstanceVariants?.RemoveAll(variant => variant.VariantID.Equals(variantId));
                        break;
                    }
                }

                if (webPartToRemove != null)
                {
                    Controls.Remove(webPartToRemove);
                }
            }
        }


        /// <summary>
        /// Sets the web part context.
        /// </summary>
        public virtual void SetContext()
        {
            DebugHelper.SetContext(ControlContext);
        }


        /// <summary>
        /// Releases the web part context.
        /// </summary>
        public virtual void ReleaseContext()
        {
            DebugHelper.ReleaseContext();
        }


        /// <summary>
        /// Gets the default cache dependencies for the data source.
        /// </summary>
        public virtual string GetDefaultCacheDependendencies()
        {
            Guid instanceGuid = InstanceGUID;
            if (instanceGuid != Guid.Empty)
            {
                // Default cache dependency is based on instance GUID of the web part
                return "webpartinstance|" + instanceGuid.ToString().ToLowerInvariant();
            }
            else
            {
                return null;
            }
        }


        /// <summary>
        /// Gets the cache dependency for the control.
        /// </summary>
        public virtual CMSCacheDependency GetCacheDependency()
        {
            return CacheHelper.GetCacheDependency(CacheDependencies);
        }


        /// <summary>
        /// Gets web part identifier.
        /// </summary>
        public string GetIdentifier()
        {
            return (InstanceGUID == Guid.Empty) ? ID : InstanceGUID.ToString();
        }


        /// <summary>
        /// Ensures that the tracked Evals are logged to the debug
        /// </summary>
        private void LogEvals()
        {
            // Log the evals within this web part
            if (mLoggedEvals != null)
            {
                // Log message only if the columns property is not set as instructed
                string usedColumns = String.Join(",", mLoggedEvals);
                string columns = GetStringValue("Columns", "");

                if (!usedColumns.Equals(columns, StringComparison.InvariantCultureIgnoreCase))
                {
                    // Prepare the web part information
                    string name = "'" + WebPartID + "'";
                    string title = WebPartTitle;
                    if (!String.IsNullOrEmpty(title))
                    {
                        name += " [" + title + "]";
                    }

                    // Log only if there is more columns in data source available than used
                    int count = mLoggedEvals.Count;
                    if (mLoggedColsCount > count)
                    {
                        title = ResHelper.GetStringFormat("WebPart.Evals", name, count, mLoggedColsCount);

                        // Log the information to the debug
                        SqlDebug.LogInformation(title, usedColumns);
                    }
                }
            }
        }

        #endregion


        #region "Rendering methods"

        /// <summary>
        /// Renders the control.
        /// </summary>
        protected override void Render(HtmlTextWriter writer)
        {
            bool outputFilter = EnableOutputFilter;
            bool resolveMacros = ContextResolver.SkippedResolver(RESOLVER_RENDER);

            if (outputFilter || resolveMacros)
            {
                // Render through the string
                string code = RenderToString(this);

                // Fix the HTML code
                if (outputFilter)
                {
                    FixXHTMLSettings settings = new FixXHTMLSettings
                    {
                        Attributes = OutputFixAttributes,
                        HTML5 = OutputFixHTML5,
                        Javascript = OutputFixJavascript,
                        LowerCase = OutputFixLowerCase,
                        ResolveUrl = OutputResolveURLs,
                        SelfClose = OutputFixSelfClose,
                        TableToDiv = OutputConvertTablesToDivs
                    };

                    code = HTMLHelper.FixXHTML(code, settings, null);
                }

                // Resolve the remaining macros by Render resolver
                if (resolveMacros)
                {
                    ContextResolver.ResolverName = RESOLVER_RENDER;

                    code = ContextResolver.ResolveMacros(code);
                }

                writer.Write(code);
            }
            else
            {
                // Render normally
                RenderInternal(this, writer);
            }
        }


        internal void BaseRender(HtmlTextWriter writer)
        {
            base.Render(writer);
        }

        #endregion


        #region "Web part properties methods"

        /// <summary>
        /// Resolves the macros within current WebPart context.
        /// </summary>
        /// <param name="inputText">Input text to resolve</param>
        /// <param name="settings">Macro context object with specific options</param>
        public virtual string ResolveMacros(string inputText, MacroSettings settings = null)
        {
            // Check whether macros are enabled
            if (DisableMacros)
            {
                return MacroSecurityProcessor.RemoveSecurityParameters(inputText, false, null);
            }

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
        /// Gets the string value of the web part
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <param name="defaultValue">Default value</param>
        public virtual string GetStringValue(string propertyName, string defaultValue)
        {
            object value = GetValue(propertyName);
            if (!StandAlone && (value == null))
            {
                // Ensure empty string for value that is not defined by the portal engine
                value = "";
            }

            return ValidationHelper.GetString(value, defaultValue);
        }


        /// <summary>
        /// Returns the value of the given web part property property.
        /// </summary>
        /// <param name="propertyName">Property name</param>
        public virtual object GetValue(string propertyName)
        {
            return GetValueInternal(propertyName, true);
        }


        /// <summary>
        /// Returns the value of the given web part property property. Used as OnGetValue handler for macro resolver.
        /// </summary>
        /// <param name="propertyName">Property name</param>
        private object GetValueEventHandler(string propertyName)
        {
            return GetValueInternal(propertyName, false);
        }


        /// <summary>
        /// Returns the value of the given web part property property.
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <param name="resolveMacros">If true, macros are resolved</param>
        private object GetValueInternal(string propertyName, bool resolveMacros)
        {
            object result;

            if (String.IsNullOrEmpty(propertyName))
            {
                return null;
            }

            propertyName = propertyName.ToLowerInvariant();

            // If no part instance, get local value
            if (PartInstance == null)
            {
                result = mLocalProperties[propertyName];
            }
            // Get the bound value
            else
            {
                // Get local value first
                object localValue = mLocalProperties[propertyName];
                result = localValue ?? PartInstance.GetValue(propertyName);
            }

            // Localize the value
            if (result is string)
            {
                // If widget and not resolve properties not loaded yet
                if ((PartInstance != null) && PartInstance.IsWidget)
                {
                    if (!widgetNotResolvePropertiesLoaded)
                    {
                        WidgetInfo wi = WidgetInfoProvider.GetWidgetInfo(PartInstance.WebPartType);
                        if (wi != null)
                        {
                            string notResolveProperties = String.Empty;

                            // Get properties from default values 
                            switch (ParentZone.WidgetZoneType)
                            {
                                case WidgetZoneTypeEnum.Editor:
                                    notResolveProperties = GetNotResolvingProperties(wi.WidgetFields, "Container", "ContainerTitle", "ContainerCSSClass", "ContainerCustomContent");
                                    break;

                                case WidgetZoneTypeEnum.Group:
                                case WidgetZoneTypeEnum.User:
                                case WidgetZoneTypeEnum.Dashboard:
                                    notResolveProperties = GetNotResolvingProperties(wi.WidgetFields, "WidgetTitle");
                                    break;
                            }

                            NotResolveProperties += wi.WidgetPublicFileds + notResolveProperties;
                        }
                        widgetNotResolvePropertiesLoaded = true;
                    }

                    // Localize value
                    result = ResHelper.LocalizeString(result as string);
                }

                string stringResult = (string)result;

                // If allowed to be resolved, resolve the macros
                string pname = ";" + propertyName + ";";

                bool resolve = !(NotResolveProperties.IndexOf(pname, StringComparison.InvariantCultureIgnoreCase) >= 0 || 
                    (propertyName.Contains("transformation") && stringResult.StartsWith("[", StringComparison.Ordinal) && stringResult.EndsWith("]", StringComparison.Ordinal)));

                bool isSqlProperty = (mSQLProperties.IndexOf(pname, StringComparison.InvariantCultureIgnoreCase) >= 0);
                if (resolveMacros && resolve)
                {
                    // Resolve the macros using web part resolver
                    MacroSettings settings = new MacroSettings();
                    settings.Culture = Thread.CurrentThread.CurrentCulture.ToString();
                    settings.AvoidInjection = isSqlProperty;

                    result = ResolveMacros(stringResult, settings);
                }

                // Check SQL properties for malicious code
                if (isSqlProperty)
                {
                    QueryScopeEnum scope;
                    switch (pname)
                    {
                        case ";wherecondition;":
                            scope = QueryScopeEnum.Where;
                            break;
                        case ";columns;":
                            scope = QueryScopeEnum.Columns;
                            break;
                        case ";orderby;":
                            scope = QueryScopeEnum.OrderBy;
                            break;
                        default:
                            scope = QueryScopeEnum.None;
                            break;
                    }

                    if ((scope != QueryScopeEnum.None) && !SqlSecurityHelper.CheckQuery(result as string, scope))
                    {
                        throw new InvalidOperationException($"Invalid SQL query in property \"{propertyName}\".");
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// Returns list of properties which are not defined in widget fields (;field1;field2;...).
        /// </summary>
        /// <param name="widgetFields">Widget fields  (saved in widget properties definition)</param>
        /// <param name="fields">Default fields (saved in persistent XML file)</param>
        private string GetNotResolvingProperties(List<string> widgetFields, params string[] fields)
        {
            // Widget fields
            List<string> iFields = widgetFields;
            // Initialize result
            string result = ";";

            // Loop thru all fields
            foreach (string field in fields)
            {
                // If field is not defined in fields list add field to the result list
                if (!iFields.Contains(field?.ToLowerInvariant()))
                {
                    result += field + ";";
                }
            }

            return result;
        }


        /// <summary>
        /// Sets the property value of the control, setting the value affects only local property value.
        /// </summary>
        /// <param name="propertyName">Property name to set</param>
        /// <param name="value">New property value</param>
        public virtual bool SetValue(string propertyName, object value)
        {
            mLocalProperties[propertyName.ToLowerInvariant()] = value;
            return true;
        }


        /// <summary>
        /// Gets the current viewmode.
        /// </summary>
        /// <returns></returns>
        private ViewModeEnum GetCurrentViewmode()
        {
            if (StandAlone || (PageCycle <= PageCycleEnum.Created))
            {
                // Not yet initialized, get ViewMode from context
                return PortalContext.ViewMode;
            }
            else
            {
                // Get ViewMode from the page manager
                return PageManager.ViewMode;
            }
        }

        #endregion


        #region "Layout methods"

        /// <summary>
        /// Gets the script for horizontal resizer.
        /// </summary>
        /// <param name="elementId">Element to resize</param>
        /// <param name="widthPropertyName">Width property name</param>
        /// <param name="heightPropertyName">Height property name</param>
        /// <param name="inverted">If true, the resizer should act inversely on X axis</param>
        /// <param name="infoElementId">Id of the element to be overlaid by the information</param>
        public virtual string GetBothResizerScript(string elementId, string widthPropertyName, string heightPropertyName, bool inverted = false, string infoElementId = null)
        {
            return GetHorizontalResizerScript(elementId, widthPropertyName, inverted, infoElementId) + " " + GetVerticalResizerScript(elementId, heightPropertyName, infoElementId);
        }


        /// <summary>
        /// Gets the script for horizontal resizer.
        /// </summary>
        /// <param name="elementId">Element to resize</param>
        /// <param name="widthPropertyName">Width property name</param>
        /// <param name="inverted">If true, the resizer should act inversely</param>
        /// <param name="infoElementId">Id of the element to be overlaid by the information</param>
        /// <param name="callback">Callback method when width is set</param>
        public virtual string GetHorizontalResizerScript(string elementId, string widthPropertyName, bool inverted = false, string infoElementId = null, string callback = null)
        {
            mNeedsLayoutScript = true;

            StringBuilder sb = new StringBuilder();

            sb.Append("InitHorizontalResizer(event, this, '", ShortClientID, "', '", elementId, "', '", widthPropertyName, "', ", inverted.ToString().ToLowerInvariant(), ", ", (infoElementId == null ? "null" : "'" + infoElementId + "'"), ", ", (callback ?? "null"), ");");

            return sb.ToString();
        }


        /// <summary>
        /// Gets the script for vertical resizer.
        /// </summary>
        /// <param name="elementId">Element to resize</param>
        /// <param name="heightPropertyName">Height property name</param>
        /// <param name="infoElementId">Id of the element to be overlaid by the information</param>
        /// <param name="callback">Callback method when the height changes</param>
        public virtual string GetVerticalResizerScript(string elementId, string heightPropertyName, string infoElementId = null, string callback = null)
        {
            mNeedsLayoutScript = true;

            StringBuilder sb = new StringBuilder();

            sb.Append("InitVerticalResizer(event, this, '", ShortClientID, "', '", elementId, "', '", heightPropertyName, "', ", (infoElementId == null ? "null" : "'" + infoElementId + "'"), ", ", (callback ?? "null"), ");");

            return sb.ToString();
        }

        #endregion


        #region "ITimeZoneManager Members"

        /// <summary>
        /// Returns time zone type.
        /// </summary>
        public TimeZoneTypeEnum TimeZoneType => ValidationHelper.GetString(GetValue("TimeZoneType"), String.Empty).ToEnum<TimeZoneTypeEnum>();


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


        #region "IDataControl members"

        /// <summary>
        /// Logs the evaluation of the given column to the debug
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="colsCount">Number of available columns in the database</param>
        public void LogEval(string columnName, int colsCount)
        {
            // Ensure the table of evals
            if (mLoggedEvals == null)
            {
                mLoggedEvals = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            }

            mLoggedEvals.Add(columnName);

            if (colsCount > mLoggedColsCount)
            {
                mLoggedColsCount = colsCount;
            }
        }

        #endregion
    }
}
