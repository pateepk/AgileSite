using System;
using System.Collections;

using CMS.Core;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Base;
using CMS.SiteProvider;
using CMS.Membership;
using CMS.DataEngine;
using CMS.Modules;

namespace CMS.PortalEngine
{
    /// <summary>
    /// Portal context methods.
    /// </summary>
    public class PortalContext : AbstractContext<PortalContext>
    {
        #region "Variables"

        private CssStylesheetInfo mCurrentSiteStylesheet;

        private string mDashboardName;
        private string mDashboardSiteName;
        private ViewModeEnum? mRequestViewMode;

        private CurrentComponentsList mCurrentComponents;

        private bool? mMVTVariantsEnabled;
        private bool mEditableControlsHidden;
        private bool mContentPersonalizationVariantsEnabled;
        private bool? mContentPersonalizationEnabled;

        private EditModeButtonEnum mEditDeleteButtonsMode = EditModeButtonEnum.None;

        private bool? mCurrentUserIsDesigner;

        private IPageManager mCurrentPageManager;
        private ArrayList mCurrentEditableControls;

        private string mCurrentCompiledValue;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the current compiled value of css pre-processor
        /// </summary>
        internal static string CurrentCompiledValue
        {
            get
            {
                return Current.mCurrentCompiledValue;

            }
            set
            {
                Current.mCurrentCompiledValue = value;
            }
        }


        /// <summary>
        /// Gets the list of editable controls on current page.
        /// </summary>
        public static ArrayList CurrentEditableControls
        {
            get
            {
                var c = Current;

                return c.mCurrentEditableControls ?? (c.mCurrentEditableControls = new ArrayList());
            }
        }


        /// <summary>
        /// Gets or sets the current page manager
        /// </summary>
        public static IPageManager CurrentPageManager
        {
            get
            {
                return Current.mCurrentPageManager;
            }
            set
            {
                var c = Current;
                if (c.mCurrentPageManager != null)
                {
                    throw new Exception("Only one page manager (CMSPageManager or CMSPortalManager) is allowed on the page.");
                }

                c.mCurrentPageManager = value;
            }
        }


        /// <summary>
        /// Gets or sets the edit/delete buttons mode
        /// </summary>
        internal static EditModeButtonEnum EditDeleteButtonsMode
        {
            get
            {
                return Current.mEditDeleteButtonsMode;
            }
            set
            {
                Current.mEditDeleteButtonsMode = value;
            }
        }


        /// <summary>
        /// Returns current ViewMode.
        /// </summary>
        [RegisterColumn]
        public static ViewModeEnum ViewMode
        {
            get
            {
                return GetViewMode();
            }
            set
            {
                SetViewMode(value);
            }
        }


        /// <summary>
        /// Gets or sets the current dashboard name.
        /// </summary>
        [RegisterColumn]
        public static string DashboardName
        {
            get
            {
                return Current.mDashboardName;
            }
            set
            {
                Current.mDashboardName = value;
            }
        }


        /// <summary>
        /// Gets or sets the current dashboard site name.
        /// </summary>
        [RegisterColumn]
        public static string DashboardSiteName
        {
            get
            {
                return Current.mDashboardSiteName;
            }
            set
            {
                Current.mDashboardSiteName = value;
            }
        }


        /// <summary>
        /// List of web part containers used by current page. Hashtable [containerName.ToLowerCSafe() -> WebPartContainerInfo]
        /// </summary>
        [RegisterProperty]
        public static CurrentComponentsList CurrentComponents
        {
            get
            {
                var c = Current;
                return c.mCurrentComponents ?? (c.mCurrentComponents = new CurrentComponentsList());
            }
        }


        /// <summary>
        /// Indicates whether to show the MVT variant slider and render all the web part/zone variants. 
        /// This value will be true only when there are defined any multivariate tests for the current document.
        /// </summary>
        public static bool MVTVariantsEnabled
        {
            get
            {
                var c = Current;

                bool? enabled = c.mMVTVariantsEnabled;
                if (enabled == null)
                {
                    // Disabled by default
                    var args = new PortalEngineEventArgs
                    {
                        Enabled = false
                    };

                    PortalEngineEvents.MVTVariantsEnabled.StartEvent(args);

                    enabled = args.Enabled;

                    c.mMVTVariantsEnabled = enabled;
                }

                return enabled.Value;
            }
            set
            {
                Current.mMVTVariantsEnabled = value;
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates whether current page may contain some hidden editable controls
        /// </summary>
        public static bool EditableControlsHidden
        {
            get
            {
                return Current.mEditableControlsHidden;
            }
            set
            {
                Current.mEditableControlsHidden = value;
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates whether current page has at least one personalization variant
        /// </summary>
        public static bool ContentPersonalizationVariantsEnabled
        {
            get
            {
                return Current.mContentPersonalizationVariantsEnabled;
            }
            set
            {
                Current.mContentPersonalizationVariantsEnabled = value;
            }
        }


        /// <summary>
        /// Indicates whether to show the Content personalization variant slider and render all the web part/zone variants.
        /// </summary>
        public static bool ContentPersonalizationEnabled
        {
            get
            {
                var c = Current;
                if (c.mContentPersonalizationEnabled != null)
                {
                    return c.mContentPersonalizationEnabled.Value;
                }

                c.mContentPersonalizationEnabled = SettingsKeyInfoProvider.GetBoolValue(SiteContext.CurrentSiteName + ".CMSContentPersonalizationEnabled")
                                                    && ModuleEntryManager.IsModuleLoaded(ModuleName.ONLINEMARKETING)
                                                    && LicenseKeyInfoProvider.IsFeatureAvailable(RequestContext.CurrentDomain, FeatureEnum.ContentPersonalization)
                                                    && ResourceSiteInfoProvider.IsResourceOnSite("cms.contentpersonalization", SiteContext.CurrentSiteName);

                return c.mContentPersonalizationEnabled.Value;
            }
        }


        /// <summary>
        /// Returns the current site stylesheet info.
        /// </summary>
        [RegisterProperty]
        public static CssStylesheetInfo CurrentSiteStylesheet
        {
            get
            {
                var c = Current;
                if (c.mCurrentSiteStylesheet != null)
                {
                    return c.mCurrentSiteStylesheet;
                }

                var site = SiteContext.CurrentSite;
                if (site != null)
                {
                    c.mCurrentSiteStylesheet = CssStylesheetInfoProvider.GetCssStylesheetInfo(site.SiteDefaultStylesheetID);
                }

                return c.mCurrentSiteStylesheet;
            }
            set
            {
                Current.mCurrentSiteStylesheet = value;
            }
        }


        /// <summary>
        /// Returns current site stylesheet name.
        /// </summary>
        [RegisterColumn]
        public static string CurrentSiteStylesheetName
        {
            get
            {
                var stylesheet = CurrentSiteStylesheet;
                return stylesheet != null ? stylesheet.StylesheetName : null;
            }
        }


        /// <summary>
        /// Indicates whether the current user has designer rights and can access web part properties (UI element)
        /// </summary>
        public static bool CurrentUserIsDesigner
        {
            get
            {
                var c = Current;
                if (c.mCurrentUserIsDesigner != null)
                {
                    return c.mCurrentUserIsDesigner.Value;
                }

                var cui = MembershipContext.AuthenticatedUser;
                c.mCurrentUserIsDesigner = cui.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin) 
                    || (cui.IsAuthorizedPerResource("cms.design", "Design") 
                    && cui.IsAuthorizedPerUIElement("CMS.Design", "Design.WebPartProperties"));

                return c.mCurrentUserIsDesigner.Value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Ensures the portal mode.
        /// </summary>
        public static ViewModeEnum EnsurePortalMode(ViewModeEnum currentMode)
        {
            // If not known, get from context
            if (currentMode == ViewModeEnum.Unknown)
            {
                currentMode = ViewMode;
            }

            if (!IsPortalMode(currentMode))
            {
                // Not a portal mode - Change mode to live site
                currentMode = ViewModeEnum.LiveSite;
                SetViewMode(currentMode);
            }

            return currentMode;
        }


        /// <summary>
        /// Returns true if the given mode is mode displaying the document.
        /// </summary>
        public static bool IsPortalMode(ViewModeEnum mode)
        {
            switch (mode)
            {
                // Portal mode
                case ViewModeEnum.LiveSite:
                case ViewModeEnum.Edit:
                case ViewModeEnum.EditLive:
                case ViewModeEnum.Design:
                case ViewModeEnum.Preview:
                    return true;

                default:
                    return false;
            }
        }


        /// <summary>
        /// Gets current view mode.
        /// </summary>
        public static ViewModeEnum GetViewMode()
        {
            // Always return live site mode for non-authenticated user
            if (!RequestContext.IsUserAuthenticated)
            {
                return ViewModeEnum.LiveSite;
            }

            // Try to get from virtual context first
            string virtualViewMode = (string)VirtualContext.GetItem(VirtualContext.PARAM_VIEWMODE);
            if (virtualViewMode != null)
            {
                return ViewModeCode.GetPageEnumFromString(virtualViewMode);
            }

            // Try to get from the request
            var mode = Current.mRequestViewMode;
            if (mode == null)
            {
                var request = CMSHttpContext.Current.Request;

                // Querystring value
                string viewmode = request.QueryString["viewmode"];
                if (!String.IsNullOrEmpty(viewmode))
                {
                    mode = ViewModeCode.ToEnum(ValidationHelper.GetInteger(viewmode, (int)ViewModeEnum.Unknown));
                }

                // Postback value
                string vmode = request.Form["vmode"];
                if (!String.IsNullOrEmpty(vmode))
                {
                    mode = ViewModeCode.ToEnum(ValidationHelper.GetInteger(vmode, (int)ViewModeEnum.Unknown));
                }

                // Cookie value
                if ((mode == null) || (mode.Value == ViewModeEnum.Unknown))
                {
                    // Get from the cookie
                    mode = ViewModeCode.ToEnum(ValidationHelper.GetInteger(CookieHelper.GetValue(CookieName.ViewMode), (int)ViewModeEnum.LiveSite));
                }

                // Save value
                SetViewMode((ViewModeEnum)mode);
            }

            return (ViewModeEnum)mode;
        }


        /// <summary>
        /// Sets current request view mode.
        /// </summary>
        /// <param name="value">New view mode</param>
        public static void SetRequestViewMode(ViewModeEnum value)
        {
            Current.mRequestViewMode = value;
        }


        /// <summary>
        /// Sets current view mode.
        /// </summary>
        /// <param name="value">New view mode</param>
        public static void SetViewMode(ViewModeEnum value)
        {
            // Save to the request
            SetRequestViewMode(value);

            // If view mode is wanted only for request, don't store to cookies
            if (!VirtualContext.ItemIsSet(VirtualContext.PARAM_VIEWMODE) && (ObjectLifeTimeFunctions.GetCurrentObjectLifeTime(CookieName.ViewMode) == ObjectLifeTimeEnum.Cookies))
            {
                // Save to the cookie
                int code = ViewModeCode.FromEnum(value);
                CookieHelper.SetValue(CookieName.ViewMode, code.ToString(), DateTime.Now.AddDays(1));
            }
        }


        /// <summary>
        /// Updates the current view mode
        /// </summary>
        /// <param name="baseMode">Current base mode (edit / preview / live)</param>
        public static ViewModeEnum UpdateViewMode(ViewModeEnum baseMode)
        {
            // Get current mode
            ViewModeEnum currentMode = ViewMode;
            CurrentUserInfo currentUser = MembershipContext.AuthenticatedUser;

            ViewModeEnum newMode = currentMode;

            // If mode within query string, set mode
            string mode = QueryHelper.GetString("mode", string.Empty).ToLowerCSafe();
            if (mode != String.Empty)
            {
                ViewModeEnum modeEnum = ViewModeCode.FromString(mode);

                // Check design permission
                if (ViewModeCode.IsSubsetOfEditMode(modeEnum) &&
                   ((modeEnum != ViewModeEnum.Design) ||
                   ((modeEnum == ViewModeEnum.Design) && (currentUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin) || currentUser.IsAuthorizedPerResource("CMS.Design", "Design")))))
                {
                    newMode = modeEnum;
                }
            }
            else
            {
                // Switch to the base mode if wrong mode
                if (baseMode.IsEdit() && currentMode.IsOneOf(ViewModeEnum.LiveSite, ViewModeEnum.Preview))
                {
                    newMode = ViewModeEnum.Edit;
                }
            }

            if (currentMode != newMode)
            {
                ViewMode = newMode;
            }

            return newMode;
        }


        /// <summary>
        /// Returns true if the given mode is considered design mode (with web parts active)
        /// </summary>
        /// <param name="mode">View mode to check</param>
        /// <param name="allowDisabled">If true (default), the disabled design view mode is detected as design mode</param>
        public static bool IsDesignMode(ViewModeEnum mode, bool allowDisabled = true)
        {
            switch (mode)
            {
                case ViewModeEnum.Design:
                case ViewModeEnum.DesignWebPart:
                    return true;

                case ViewModeEnum.DesignDisabled:
                    return allowDisabled;
            }

            return false;
        }


        /// <summary>
        /// Gets the unique identifier for editor widgets per document.
        /// </summary>
        /// <param name="documentId">The document id.</param>
        private static string GetEditorWidgetsKey(int documentId)
        {
            return "Wi" + documentId;
        }


        /// <summary>
        /// Gets modified editor widgets from a temporary interlayer.
        /// </summary>
        /// <param name="documentId">The document id.</param>
        public static string GetEditorWidgets(int documentId)
        {
            return SessionHelper.GetValue(GetEditorWidgetsKey(documentId)) as string;
        }


        /// <summary>
        /// Saves the modified editor widgets into a temporary interlayer.
        /// </summary>
        /// <param name="documentId">The document id.</param>
        /// <param name="widgets">The widgets.</param>
        public static void SaveEditorWidgets(int documentId, string widgets)
        {
            SessionHelper.SetValue(GetEditorWidgetsKey(documentId), widgets);
        }


        /// <summary>
        /// Clears the temporary interlayer which holds modified editor widgets.
        /// </summary>
        /// <param name="documentId">The document id.</param>
        public static void ClearEditorWidgets(int documentId)
        {
            SessionHelper.Remove(GetEditorWidgetsKey(documentId));
        }

        #endregion
    }
}