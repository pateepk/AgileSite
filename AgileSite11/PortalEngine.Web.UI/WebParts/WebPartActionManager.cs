using System;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Threading;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.EventLog;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.Localization;
using CMS.Membership;
using CMS.Modules;
using CMS.Search;
using CMS.SiteProvider;
using CMS.Synchronization;

using MessageTypeEnum = CMS.Base.Web.UI.MessageTypeEnum;
using TreeNode = CMS.DocumentEngine.TreeNode;

namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// Web part/Widget action manager ensures basic actions e.g: Add, remove, move etc...
    /// </summary>
    internal class WebPartActionManager : CMSWebControl, INamingContainer, IPostBackEventHandler, ICallbackEventHandler
    {
        #region "Variables"

        protected string callbackResult = "";
        protected string keepContentChangedScript = "if (window.CMSContentManager) { CMSContentManager.storeContentChangedStatus(); };";

        #endregion


        #region "Hidden fields for postback parameters"

        /// <summary>
        /// New top position
        /// </summary>
        protected string mNewPositionTop = null;

        /// <summary>
        /// New left position
        /// </summary>
        protected string mNewPositionLeft = null;


        /// <summary>
        /// Selected web part hidden field.
        /// </summary>
        protected HiddenField hidWebPart = null;

        /// <summary>
        /// Selected web part hidden field.
        /// </summary>
        protected HiddenField hidVariant = null;

        /// <summary>
        /// Selected web part hidden field.
        /// </summary>
        protected HiddenField hidIsLayoutZone = null;

        /// <summary>
        /// Selected zone hidden field.
        /// </summary>
        protected HiddenField hidZone = null;

        /// <summary>
        /// Selected zone type hidden field.
        /// </summary>
        protected HiddenField hidZoneType = null;

        /// <summary>
        /// Selected zone variant hidden field.
        /// </summary>
        protected HiddenField hidZoneVariantId = null;

        /// <summary>
        /// Selected zone variant hidden field. Used when moving web parts between the variants of the same zone.
        /// </summary>
        protected HiddenField hidSourceZoneVariantId = null;


        /// <summary>
        /// Selected alias path hidden field.
        /// </summary>
        protected HiddenField hidAliasPath = null;

        /// <summary>
        /// Selected template ID hidden field.
        /// </summary>
        protected HiddenField hidTemplate = null;

        /// <summary>
        /// Selected web part GUID.
        /// </summary>
        protected HiddenField hidGuid = null;

        /// <summary>
        /// Target zone hidden field.
        /// </summary>
        protected HiddenField hidTargetZone = null;

        /// <summary>
        /// Target zone variant hidden field.
        /// </summary>
        protected HiddenField hidTargetZoneVariantId = null;

        /// <summary>
        /// Target position hidden field.
        /// </summary>
        protected HiddenField hidTargetPosition = null;

        #endregion


        #region "Constants"

        // Path to the web part directory
        private const string PORTALENGINE_UI_WEBPARTSPATH = "~/CMSModules/PortalEngine/UI/Webparts/";
        // Callback result unauthorized value.
        private const string UNAUTHORIZED = "U";
        // Callback result refresh page value.
        private const string REFRESH = "R";
        // Callback result error value.
        private const string ERROR = "E";
        // Callback result error and refresh page value.
        private const string ERROR_REFRESHPAGE = "ER";
        // Callback result rename CKEditor textarea IDs.
        private const string UPDATE_WIDGET_INPUT_ELEMENT_IDENTIFIERS = "UPDATE_IDS";
        // Callback result separator.
        private const string SEPARATOR = "<#>";

        #endregion


        #region "Properties"

        /// <summary>
        /// Current page info
        /// </summary>
        public PageInfo CurrentPageInfo
        {
            get
            {
                return PortalManager.CurrentPageInfo;
            }
        }

        /// <summary>
        /// View mode
        /// </summary>
        public ViewModeEnum ViewMode
        {
            get
            {
                return PortalManager.ViewMode;
            }

        }


        /// <summary>
        /// Current messages placeholder
        /// </summary>
        public MessagesPlaceHolder CurrentMessagesPlaceholder
        {
            get
            {
                return PortalManager.CurrentMessagesPlaceholder;
            }
        }


        /// <summary>
        /// Portal manager
        /// </summary>
        public CMSPortalManager PortalManager
        {
            get;
            private set;
        }


        /// <summary>
        /// Indicates whether web part management is required
        /// </summary>
        public WebPartManagementEnum RequiresWebPartManagement
        {
            get
            {
                return PortalManager.RequiresWebPartManagement;
            }
        }


        /// <summary>
        /// TreProvider object
        /// </summary>
        public TreeProvider TreeProvider
        {
            get
            {
                return PortalManager.TreeProvider;
            }
        }


        /// <summary>
        /// Site name
        /// </summary>
        public string SiteName
        {
            get
            {
                return PortalManager.SiteName;
            }
        }


        /// <summary>
        /// Current placeholder
        /// </summary>
        public CMSPagePlaceholder CurrentPlaceholder
        {
            get
            {
                return PortalManager.CurrentPlaceholder;
            }
        }

        #endregion


        #region "Methods"

        #region "Constructor"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="manager"></param>
        public WebPartActionManager(CMSPortalManager manager)
        {
            PortalManager = manager;
        }

        #endregion


        #region "Page events"

        /// <summary>
        /// Child control creation.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            // Check for the Design Mode in Visual Studio
            if (Context == null)
            {
                return;
            }


            // Web part management components
            CreateParameterFields(ViewMode);
        }


        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (ViewMode.IsOneOf(ViewModeEnum.Edit, ViewModeEnum.EditDisabled, ViewModeEnum.EditLive) && (PortalManager != null) && (PortalManager.DocumentManager != null))
            {
                PortalManager.DocumentManager.OnAfterAction += DocumentManager_OnAfterAction;

                ScriptHelper.RegisterJQuery(Page);
                ScriptHelper.RegisterScriptFile(Page, "jquery/jquery-url.js");

                // Ensure that the persistent javascript data will be stored in the form action url
                ScriptHelper.RegisterOnSubmitStatement(Page, typeof(string), "StoreDataOnSubmit", keepContentChangedScript);
            }
        }


        /// <summary>
        /// Raises the System.Web.UI.Control.PreRender event.
        /// </summary>
        /// <param name="e">An System.EventArgs object that contains the event data.</param>
        protected override void OnPreRender(EventArgs e)
        {
            ViewModeEnum viewMode = ViewMode;

            // Ensure CSS stylesheets
            switch (RequiresWebPartManagement)
            {
                case WebPartManagementEnum.Widgets:
                    // Do not register widgets.css for dashboard page
                    CssRegistration.RegisterWidgetsMode(Page);
                    break;

                case WebPartManagementEnum.None:
                    break;

                default:
                    CssRegistration.RegisterDesignMode(Page);
                    break;
            }

            // Prepare the scripts
            StringBuilder sb = new StringBuilder();
            bool registerScript = false;

            if (viewMode.IsOneOf(ViewModeEnum.Edit, ViewModeEnum.EditLive, ViewModeEnum.EditDisabled))
            {
                registerScript = true;

                // Register the complete page script
                ScriptHelper.RegisterCompletePageScript(Page);
            }

            // Web part management scripts
            if ((RequiresWebPartManagement != WebPartManagementEnum.None) || (ViewMode.IsEdit() && PortalContext.ContentPersonalizationVariantsEnabled))
            {
                // Set dashboard name parameter if is required
                string dashboardNameParameter = String.Empty;
                if (!String.IsNullOrEmpty(PortalContext.DashboardName))
                {
                    dashboardNameParameter = "&dashboard=" + PortalContext.DashboardName;
                }

                // Set dashboard site name if is required
                if (!String.IsNullOrEmpty(PortalContext.DashboardSiteName))
                {
                    dashboardNameParameter += "&sitename=" + PortalContext.DashboardSiteName;
                }

                registerScript = true;

                sb.Append(
@"
var webPartsPath = '", ResolveUrl(PORTALENGINE_UI_WEBPARTSPATH), @"';

function getDashBoardParameter() { return '" + ScriptHelper.GetString(dashboardNameParameter, false) + @"' }
function getCulture() { return '" + ScriptHelper.GetString(LocalizationContext.PreferredCultureCode, false) + @"' }
function setWebPart(val) { SetVal('", hidWebPart.ClientID, @"', val); }
function setVariant(val) { SetVal('", hidVariant.ClientID, @"', val); }
function setZone(val) { SetVal('", hidZone.ClientID, @"', val); setZoneVariantId(getZoneVariantId(val)); }
function setZoneType(val) { SetVal('", hidZoneType.ClientID, @"', val); }
function setIsLayoutZone(val) { SetVal('", hidIsLayoutZone.ClientID, @"', val); }
function setZoneVariantId(val) { SetVal('", hidZoneVariantId.ClientID, @"', val) }
function setSourceZoneVariantId(val) { SetVal('", hidSourceZoneVariantId.ClientID, @"', val) }
function setAliasPath(val) { SetVal('", hidAliasPath.ClientID, @"', val) }
function setTemplate(val) { SetVal('", hidTemplate.ClientID, @"', val); }
function setGuid(val) { SetVal('", hidGuid.ClientID, @"', val) }
function setTargetZone(val) { SetVal('", hidTargetZone.ClientID, @"', val); setTargetZoneVariantId(getZoneVariantId(val)); }
function setTargetZoneVariantId(val) { SetVal('", hidTargetZoneVariantId.ClientID, @"', val) }
function setTargetPosition(val) { SetVal('", hidTargetPosition.ClientID, @"', val); }

function getAliasPath() {return Get('", hidAliasPath.ClientID, @"').value; }
function getTemplate() {return Get('", hidTemplate.ClientID, @"').value; }
function getZoneType() {return Get('", hidZoneType.ClientID, @"').value; }
function getZone() {return Get('", hidZone.ClientID, @"').value; }
function getIsLayoutZone() {return Get('", hidIsLayoutZone.ClientID, @"').value; }
function getTargetZone() {return Get('", hidTargetZone.ClientID, @"').value; }

");

                string culture = MembershipContext.AuthenticatedUser.PreferredUICultureCode;

                // Design mode only functions
                if (RequiresWebPartManagement == WebPartManagementEnum.All)
                {
                    // Register the dialog script
                    ScriptHelper.RegisterDialogScript(Page);

                    PageTemplateInfo pti = CurrentPageInfo.UsedPageTemplateInfo;
                    bool isUI = ((pti != null) && (pti.PageTemplateType == PageTemplateTypeEnum.UI));

                    sb.Append(
                        (isUI ? "window.isUI = true;" : null),
@"
var editTemplateUrl = '", ResolveUrl("~/CMSModules/PortalEngine/UI/PageTemplates/PageTemplate_Edit.aspx"), @"';
var confirmRemoveAll = ", ScriptHelper.GetString(ResHelper.GetString("PortalManager.ConfirmRemoveAllWebParts", culture)), @";
var confirmRemove = ", ScriptHelper.GetString(ResHelper.GetString("PortalManager.ConfirmRemoveWebPart", culture)), @";
"
                    );
                }

                bool isLiveSite = (viewMode.IsLiveSite());
                sb.Append(
@"
var widgetSelectorUrl = '", (isLiveSite ? ApplicationUrlHelper.ResolveDialogUrl("~/CMSModules/Widgets/LiveDialogs/WidgetSelector.aspx") : ResolveUrl("~/CMSModules/Widgets/Dialogs/WidgetSelector.aspx")), @"';
var widgetPropertiesUrl = '", (isLiveSite ? ApplicationUrlHelper.ResolveDialogUrl("~/CMSModules/Widgets/LiveDialogs/WidgetProperties.aspx") : ResolveUrl("~/CMSModules/Widgets/Dialogs/WidgetProperties.aspx")), @"';",
                        // Page needs full page reload in Design mode/Dashboard. This will ensure initial loading of controls when adding widgets/web parts. Edit mode needs postback for keeping the unsaved edited data (editable text web parts).
@"var usePostbackRefresh = ", (viewMode.IsOneOf(ViewModeEnum.LiveSite, ViewModeEnum.Edit, ViewModeEnum.EditLive) ? "true" : "false"), @";
var confirmRemoveAllWidgets = ", ScriptHelper.GetString(ResHelper.GetString("PortalManager.ConfirmRemoveAllWidgets", culture)), @";
var confirmRemoveWidget = ", ScriptHelper.GetString(ResHelper.GetString("PortalManager.ConfirmRemoveWidget", culture)), @";
var maximizeWidget = ", ScriptHelper.GetString(ResHelper.GetString("Widgets.Maximize", culture)), @";
var minimizeWidget = ", ScriptHelper.GetString(ResHelper.GetString("Widgets.Minimize", culture)), @";
var cannotModifyWidgets = ", ScriptHelper.GetString(ResHelper.GetString((AuthenticationHelper.IsAuthenticated() ? "Widgets.CannotModifyUser" : "Widgets.NotAuthenticated"), culture)), @";
"
                );

                // Register linked scripts
                ScriptHelper.RegisterCompletePageScript(Page);
            }

            if (registerScript)
            {
                ScriptHelper.RegisterScriptFile(Page, "DesignMode/PortalManager.js");
            }

            // Prepare ScrenLock script
            string screenLockScript = @"if (window.top.HideScreenLockWarningAndSync) { window.top.HideScreenLockWarningAndSync(" + SecurityHelper.GetSecondsToShowScreenLockAction(SiteContext.CurrentSiteName) + @"); }";

            sb.Append(@"
function PM_Postback(param) { ", screenLockScript, " if(window.CMSContentManager) { CMSContentManager.allowSubmit = true; }; ", Page.ClientScript.GetPostBackEventReference(this, "#P#").Replace("'#P#'", "param"), @"; }
function PM_Callback(param, callback, ctx) { ", screenLockScript, keepContentChangedScript, Page.ClientScript.GetCallbackEventReference(this, "param", "#C#", "ctx", true).Replace("#C#", "callback"), @"; }");

            // Register the script
            ScriptHelper.RegisterClientScriptBlock(this, typeof(string), "PageManagement", ScriptHelper.GetScript(sb.ToString()));

            base.OnPreRender(e);
        }


        /// <summary>
        /// Render event
        /// </summary>
        /// <param name="writer">HtmlTextWriter</param>
        protected override void Render(HtmlTextWriter writer)
        {
            // Do not render any code
            base.RenderChildren(writer);
        }


        /// <summary>
        /// Handles the OnAfterAction event of the DocumentManager control.
        /// </summary>
        protected void DocumentManager_OnAfterAction(object sender, DocumentManagerEventArgs e)
        {
            if (ViewMode.IsOneOf(ViewModeEnum.Edit, ViewModeEnum.EditLive))
            {
                if (e.ActionName == ComponentEvents.SAVE)
                {
                    string resetChangesScript = @"
$cmsj(document).ready(function () {
    CMSContentManager.changed(false);
});";

                    // Reset the "content changed" flag after the document save action
                    ScriptHelper.RegisterStartupScript(this, typeof(string), "resetChangedStatus", resetChangesScript, true);
                }
            }
        }

        #endregion


        #region "PostBack event"

        /// <summary>
        /// Raises the post back event.
        /// </summary>
        /// <param name="eventArgument">Event argument</param>
        public void RaisePostBackEvent(string eventArgument)
        {
            SecurityHelper.LogScreenLockAction();

            try
            {
                if (PortalContext.IsDesignMode(ViewMode, false))
                {
                    // Design mode
                    if (CheckRefreshPage())
                    {
                        CMSPortalManager.RefreshPage();
                        return;
                    }

                    // Perform the action
                    switch (eventArgument.ToLowerCSafe())
                    {
                        case "movewebpart":
                            {
                                mNewPositionLeft = "0";
                                mNewPositionTop = "0";

                                MoveWebPart(true);
                            }
                            break;

                        case "removewebpart":
                            WebPartAction(WebPartActionEnum.Remove);
                            break;

                        case "removeallwebparts":
                            RemoveAllWebParts();
                            break;

                        case "moveallwebparts":
                            MoveAllWebParts();
                            break;

                        case "movewebpartup":
                            WebPartAction(WebPartActionEnum.Up);
                            break;

                        case "movewebpartdown":
                            WebPartAction(WebPartActionEnum.Down);
                            break;

                        case "clonewebpart":
                            CloneWebPart();
                            break;

                        case "clonetemplate":
                            CloneTemplate();
                            break;

                        case "addwebpartwithoutdialog":
                            AddWebPartWithoutDialog();
                            break;

                        case "addwidgetwithoutdialog":
                            AddWidgetWithoutDialog();
                            break;
                    }
                }
                else if ((RequiresWebPartManagement != WebPartManagementEnum.None) && (ViewMode != ViewModeEnum.EditDisabled))
                {
                    // Perform the action
                    switch (eventArgument.ToLowerCSafe())
                    {
                        case "removewebpart":
                            WebPartAction(WebPartActionEnum.Remove);
                            break;

                        case "addwidgetwithoutdialog":
                            AddWidgetWithoutDialog();
                            break;

                        case "removeallwebparts":
                            RemoveAllWebParts();
                            break;
                    }
                }
            }
            catch (ThreadAbortException)
            {
                // Ignore thread abort exception caused by response.redirect
            }
            catch (Exception ex)
            {
                // Log the exception
                EventLogProvider.LogException("Design", eventArgument.ToUpperCSafe(), ex);

                // Show the error message in case of error
                if (CurrentMessagesPlaceholder != null)
                {
                    CurrentMessagesPlaceholder.ShowMessage(MessageTypeEnum.Error, ex.Message);
                }
            }
        }


        /// <summary>
        /// Returns true if the page should be refreshed
        /// </summary>
        private bool CheckRefreshPage()
        {
            var pt = PortalManager.PageTemplate;

            return (pt != null) && SynchronizationHelper.IsCheckedOutByOtherUser(pt) && (pt.PageTemplateType != PageTemplateTypeEnum.Dashboard);
        }

        #endregion


        #region "CallBack event"

        /// <summary>
        /// Prepares the callback result.
        /// </summary>
        public string GetCallbackResult()
        {
            return callbackResult;
        }


        /// <summary>
        /// Raises the callback event.
        /// </summary>
        public void RaiseCallbackEvent(string eventArgument)
        {
            SecurityHelper.LogScreenLockAction();

            string[] argumentParts = eventArgument.Split('\n');

            if (PortalContext.IsDesignMode(ViewMode, false) && CheckRefreshPage())
            {
                switch (argumentParts[0].ToLowerCSafe())
                {
                    case "movewebpartasync":
                        callbackResult = REFRESH;
                        break;

                    default:
                        callbackResult = "RefreshPage(true);";
                        break;
                }
                return;
            }

            if (RequiresWebPartManagement != WebPartManagementEnum.None)
            {
                try
                {
                    if (argumentParts.Length > 4)
                    {
                        hidZone.Value = argumentParts[1];
                        hidWebPart.Value = argumentParts[2];
                        hidAliasPath.Value = argumentParts[3];
                        hidGuid.Value = argumentParts[4];
                    }

                    // Perform the action
                    switch (argumentParts[0].ToLowerCSafe())
                    {
                        // Copy web part
                        case "copywebpart":
                            {
                                hidZoneVariantId.Value = argumentParts[5];
                                if (CopyWebPart())
                                {
                                    callbackResult = "ok";
                                }
                            }
                            break;

                        // Paste web part
                        case "pastewebpart":
                            {
                                hidZoneVariantId.Value = argumentParts[5];
                                hidIsLayoutZone.Value = argumentParts[6];
                                hidZoneType.Value = argumentParts[7];
                                if (PasteWebPart())
                                {
                                    callbackResult = "ok";
                                }
                            }
                            break;

                        // Move the web part somewhere else
                        case "movewebpartasync":
                            {
                                hidTargetZone.Value = argumentParts[5];
                                hidTargetPosition.Value = argumentParts[6];
                                hidZoneVariantId.Value = argumentParts[7];
                                hidTargetZoneVariantId.Value = argumentParts[8];

                                if (argumentParts.Length >= 11)
                                {
                                    // Set new position of the web part
                                    mNewPositionLeft = argumentParts[9];
                                    mNewPositionTop = argumentParts[10];
                                }

                                MoveWebPart(false);
                            }
                            break;

                        // Set the web part property
                        case "setwebpartproperty":
                            {
                                hidVariant.Value = argumentParts[5];

                                string propertyName = argumentParts[6];
                                string value = argumentParts[7].Replace("[\\n]", "\r\n");
                                int line = ValidationHelper.GetInteger(argumentParts[8], 0);

                                SetWebPartProperty(propertyName, value, line);
                            }
                            break;

                        // Set the web part property
                        case "addtowebpartproperty":
                            {
                                hidVariant.Value = argumentParts[5];
                                string propertyName = argumentParts[6];
                                string value = argumentParts[7];
                                bool refresh = ValidationHelper.GetBoolean(argumentParts[8], false);

                                AddToWebPartProperty(propertyName, value);

                                if (refresh)
                                {
                                    callbackResult = "RefreshPage(true);";
                                }
                            }
                            break;

                        // Minimize the widget
                        case "minimizewidget":
                            ChangeWidgetWindowState(WidgetStateEnum.Minimize);
                            break;

                        // Maximize the widget
                        case "maximizewidget":
                            ChangeWidgetWindowState(WidgetStateEnum.Maximize);
                            break;

                        // Move web part up
                        case "movewebpartup":
                            WebPartAction(WebPartActionEnum.Up, false);
                            break;

                        // Move web part down
                        case "movewebpartdown":
                            WebPartAction(WebPartActionEnum.Down, false);
                            break;

                        // Move web part to the top
                        case "movewebparttop":
                            WebPartAction(WebPartActionEnum.Top, false);
                            break;

                        // Move web part to the bottom
                        case "movewebpartbottom":
                            WebPartAction(WebPartActionEnum.Bottom, false);
                            break;

                        case "addwidgetwithoutdialog":
                            hidWebPart.Value = argumentParts[1];
                            hidTargetZone.Value = argumentParts[2];
                            hidIsLayoutZone.Value = argumentParts[3];
                            hidZoneType.Value = argumentParts[4];

                            AddWidgetWithoutDialog();
                            break;

                        case "removewebpart":
                            hidZone.Value = argumentParts[1];
                            hidWebPart.Value = argumentParts[2];
                            hidAliasPath.Value = argumentParts[3];
                            hidGuid.Value = argumentParts[4];

                            WebPartAction(WebPartActionEnum.Remove, false);
                            break;

                        case "removeallwebparts":
                            hidZone.Value = argumentParts[1];
                            hidAliasPath.Value = argumentParts[2];

                            RemoveAllWebParts(false);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    callbackResult = ERROR + SEPARATOR + ex.Message;

                    EventLogProvider.LogException("Design", "CALLBACK", ex);
                }
            }
            else
            {
                callbackResult = UNAUTHORIZED;
            }
        }

        #endregion


        #region "Control methods"

        /// <summary>
        /// Creates the hidden fields and required components for the web part management
        /// </summary>
        /// <param name="viewMode">View mode</param>
        private void CreateParameterFields(ViewModeEnum viewMode)
        {
            // Web part management components
            if ((RequiresWebPartManagement != WebPartManagementEnum.None) || (viewMode.IsEdit(true) && PortalContext.ContentPersonalizationVariantsEnabled))
            {
                // Always maintain scroll position in design mode
                Page.MaintainScrollPositionOnPostBack = true;

                // Add the hidden fields
                EnsureHiddenField(ref hidWebPart, "hidWebPart");
                EnsureHiddenField(ref hidVariant, "hidVariant");
                EnsureHiddenField(ref hidIsLayoutZone, "hidIsLayoutZone");
                EnsureHiddenField(ref hidZone, "hidZone");
                EnsureHiddenField(ref hidZoneType, "hidZoneType");
                EnsureHiddenField(ref hidZoneVariantId, "hidZoneVariantId");
                EnsureHiddenField(ref hidSourceZoneVariantId, "hidSourceZoneVariantId");
                EnsureHiddenField(ref hidAliasPath, "hidAliasPath");
                EnsureHiddenField(ref hidTemplate, "hidTemplate");
                EnsureHiddenField(ref hidGuid, "hidGuid");
                EnsureHiddenField(ref hidTargetZone, "hidTargetZone");
                EnsureHiddenField(ref hidTargetZoneVariantId, "hidTargetZoneVariantId");
                EnsureHiddenField(ref hidTargetPosition, "hidTargetPosition");

                string culture = MembershipContext.AuthenticatedUser.PreferredUICultureCode;
            }
        }


        /// <summary>
        /// Register hidden field to the controls collection
        /// </summary>
        /// <param name="ctrl">Control</param>
        /// <param name="id">Control id</param>
        private void EnsureHiddenField(ref HiddenField ctrl, string id)
        {
            ctrl = new HiddenField();

            ctrl.ID = id;
            ctrl.EnableViewState = false;

            Controls.Add(ctrl);
        }

        #endregion


        #region "Actions"

        /// <summary>
        /// Adds the widget without the properties dialog.
        /// </summary>
        private void AddWidgetWithoutDialog()
        {
            // Get the widget info
            int widgetId = ValidationHelper.GetInteger(hidWebPart.Value, 0);

            int position = 0;
            ParsePosition(ref position);


            string zoneId = hidTargetZone.Value;
            bool isLayoutZone = ValidationHelper.GetBoolean(hidIsLayoutZone.Value, false);
            WidgetZoneTypeEnum zoneType = hidZoneType.Value.ToEnum<WidgetZoneTypeEnum>();

            // Get the corresponding widget info from the database
            WidgetInfo wi = WidgetInfoProvider.GetWidgetInfo(widgetId);
            if (wi != null)
            {
                // Get the parent web part info
                WebPartInfo wpi = WebPartInfoProvider.GetWebPartInfo(wi.WidgetWebPartID);
                if (wpi != null)
                {
                    // Get the correct template instance
                    PageInfo pi = CurrentPageInfo;
                    if (pi != null)
                    {
                        // Load the version data of the current workflow step
                        pi.LoadVersion();
                        // Clear cached instances after the versioned data is loaded
                        pi.DocumentTemplateInstance = null;
                        pi.TemplateInstance = null;

                        PageTemplateInfo pti = pi.UsedPageTemplateInfo;
                        PageTemplateInstance templateInstance = CMSPortalManager.GetTemplateInstanceForEditing(pi);

                        // Get the zone to which it inserts
                        WebPartZoneInstance zone = templateInstance.EnsureZone(zoneId);
                        if ((zoneType == WidgetZoneTypeEnum.None) && (zone != null))
                        {
                            zoneType = zone.WidgetZoneType;
                        }

                        // Set the zone type for layout zones
                        if (zone.WidgetZoneType == WidgetZoneTypeEnum.None)
                        {
                            zone.WidgetZoneType = zoneType;
                        }

                        // Check security
                        var currentUser = MembershipContext.AuthenticatedUser;
                        bool allowed = true;

                        switch (zoneType)
                        {
                            // Group zone => Only group widgets and group admin
                            case WidgetZoneTypeEnum.Group:
                                // Should always be, only group widget are allowed in group zone
                                if (!wi.WidgetForGroup || (!currentUser.IsGroupAdministrator(CurrentPageInfo.NodeGroupID) && ((PortalContext.ViewMode != ViewModeEnum.Design) || ((PortalContext.ViewMode == ViewModeEnum.Design) && (!currentUser.IsAuthorizedPerResource("CMS.Design", "Design"))))))
                                {
                                    allowed = false;
                                }
                                break;

                            // Widget must be allowed for editor zones
                            case WidgetZoneTypeEnum.Editor:
                                if (!wi.WidgetForEditor)
                                {
                                    allowed = false;
                                }
                                break;

                            // Widget must be allowed for user zones
                            case WidgetZoneTypeEnum.User:
                                if (!wi.WidgetForUser)
                                {
                                    allowed = false;
                                }
                                break;

                            // Widget must be allowed for dashboard zones
                            case WidgetZoneTypeEnum.Dashboard:
                                if (!wi.WidgetForDashboard)
                                {
                                    allowed = false;
                                }
                                break;
                        }

                        // Check security
                        if ((zoneType != WidgetZoneTypeEnum.Group) && !WidgetRoleInfoProvider.IsWidgetAllowed(wi, currentUser.UserID, AuthenticationHelper.IsAuthenticated()))
                        {
                            allowed = false;
                        }

                        if (allowed)
                        {
                            // Merge the parent web part properties definition with the widget properties definition
                            string widgetProperties = FormHelper.MergeFormDefinitions(wpi.WebPartProperties, wi.WidgetProperties);

                            // Create the FormInfo for the current widget properties definition
                            FormInfo fi = PortalFormHelper.GetWidgetFormInfo(wi.WidgetName, zoneType, widgetProperties, true, wi.WidgetDefaultValues);

                            if (fi != null)
                            {
                                // Data row with required columns
                                DataRow dr = null;

                                // Apply changed values
                                dr = fi.GetDataRow();

                                fi.LoadDefaultValues(dr, FormResolveTypeEnum.Visible);

                                // Add web part to the currently selected zone under currently selected page
                                WebPartInstance widgetInstance = PortalHelper.AddNewWidget(widgetId, zoneId, zoneType, isLayoutZone, templateInstance, dr);

                                // Apply the position
                                ApplyPosition(widgetInstance);

                                if (widgetInstance != null)
                                {
                                    // Save the changes  
                                    CMSPortalManager.SaveTemplateChanges(CurrentPageInfo, templateInstance, zoneType, PortalContext.ViewMode, TreeProvider);
                                }
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Parses the web part position
        /// </summary>
        /// <param name="position">Order of the web part</param>
        private void ParsePosition(ref int position)
        {
            string[] items = hidTargetPosition.Value.Split('|');

            position = ValidationHelper.GetInteger(items[0], 0);

            // Left, top position
            if (items.Length >= 3)
            {
                mNewPositionLeft = items[1];
                mNewPositionTop = items[2];
            }
        }


        /// <summary>
        /// Adds the web part without the properties dialog.
        /// </summary>
        private void AddWebPartWithoutDialog()
        {
            // Get the web part info
            int webPartId = ValidationHelper.GetInteger(hidWebPart.Value, 0);

            int position = 0;
            ParsePosition(ref position);

            string zoneId = hidTargetZone.Value;
            int zoneVariantId = ValidationHelper.GetInteger(hidTargetZoneVariantId.Value, 0);
            bool isLayoutZone = ValidationHelper.GetBoolean(hidIsLayoutZone.Value, false);

            if ((webPartId > 0) && !string.IsNullOrEmpty(zoneId))
            {
                // Get the page info
                PageInfo pi = CurrentPageInfo;
                if (pi != null)
                {
                    // Web part instance hasn't created yet
                    WebPartInfo wpi = WebPartInfoProvider.GetWebPartInfo(webPartId);

                    if (wpi != null)
                    {
                        // Get template instance
                        PageTemplateInstance pti = CMSPortalManager.GetTemplateInstanceForEditing(pi);

                        // Add Web part
                        var props = PortalFormHelper.GetDefaultWebPartProperties(wpi);

                        var webPartInstance = PortalHelper.AddNewWebPart(webPartId, zoneId, isLayoutZone, zoneVariantId, position, pti, props);

                        // Set default layout
                        if (wpi.WebPartParentID > 0)
                        {
                            WebPartLayoutInfo wpli = WebPartLayoutInfoProvider.GetDefaultLayout(wpi.WebPartID);
                            if (wpli != null)
                            {
                                webPartInstance.SetValue("WebPartLayout", wpli.WebPartLayoutCodeName);
                            }
                        }

                        // Apply the position
                        ApplyPosition(webPartInstance);

                        if (zoneVariantId == 0)
                        {
                            // Save the changes  
                            CMSPortalManager.SaveTemplateChanges(pi, pti, WidgetZoneTypeEnum.None, ViewMode, TreeProvider);
                        }
                        else
                        {
                            if ((webPartInstance != null) && (webPartInstance.ParentZone != null))
                            {
                                // Save changes to the web part variant
                                VariantHelper.SaveWebPartVariantChanges(webPartInstance, 0, zoneVariantId, webPartInstance.ParentZone.VariantMode, null);
                            }
                        }

                        // Refresh the page
                        URLHelper.Redirect(RequestContext.CurrentURL);
                    }
                }
            }
        }


        /// <summary>
        /// Clones current template as an ad-hoc template.
        /// </summary>
        private void CloneTemplate()
        {
            // Clone the template
            string aliasPath = ValidationHelper.GetString(hidAliasPath.Value, "");
            if (aliasPath != "")
            {
                string preferredCultureCode = LocalizationContext.PreferredCultureCode;
                SiteInfo currentSite = SiteContext.CurrentSite;

                // Get template source page info
                PageInfo pi = PageInfoProvider.GetPageInfo(SiteName, aliasPath, preferredCultureCode, null, currentSite.CombineWithDefaultCulture);
                if (pi != null)
                {
                    PageInfo sourcePageInfo = PageInfoProvider.GetTemplateSourcePageInfo(pi, preferredCultureCode, currentSite.CombineWithDefaultCulture);
                    if (sourcePageInfo != null)
                    {
                        // Get the document
                        TreeNode node = TreeProvider.SelectSingleNode(SiteName, pi.NodeAliasPath, preferredCultureCode, currentSite.CombineWithDefaultCulture, null, false);
                        if (node != null)
                        {

                            string colName = sourcePageInfo.GetUsedPageTemplateIdColumn();
                            int templateId = sourcePageInfo.GetUsedPageTemplateId();

                            PageTemplateInfo pti = PageTemplateInfoProvider.GetPageTemplateInfo(templateId);
                            if (pti != null)
                            {
                                if (pti.IsPortal && pti.IsReusable)
                                {
                                    string displayName = "Ad-hoc: " + pi.GetDocumentName();

                                    // Clone the template
                                    PageTemplateInfo newInfo = PageTemplateInfoProvider.CloneTemplateAsAdHoc(pti, displayName, pi.NodeSiteID, node.NodeGUID);

                                    newInfo.Description = String.Format(ResHelper.GetString("PageTemplate.AdHocDescription"), node.DocumentNamePath);

                                    PageTemplateInfoProvider.SetPageTemplateInfo(newInfo);

                                    // Update the document
                                    node.SetValue(colName, newInfo.PageTemplateId);
                                    using (new CMSActionContext { LogEvents = false })
                                    {
                                        node.Update();
                                    }

                                    // Update search index for node
                                    if (DocumentHelper.IsSearchTaskCreationAllowed(node))
                                    {
                                        SearchTaskInfoProvider.CreateTask(SearchTaskTypeEnum.Update, TreeNode.OBJECT_TYPE, SearchFieldsConstants.ID, node.GetSearchID(), node.DocumentID);
                                    }

                                    // Add new template to site
                                    PageTemplateInfoProvider.AddPageTemplateToSite(newInfo.PageTemplateId, currentSite.SiteID);

                                    // Update page infos
                                    pi.UsedPageTemplateInfo = newInfo;
                                    pi.SetPageTemplateId(newInfo.PageTemplateId);

                                    pi.UsedPageTemplateInfo = newInfo;

                                    CMSPortalManager.RefreshPage();
                                }
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Returns true if user can change the content of the specified zone.
        /// </summary>
        /// <param name="zone">WebPartZoneInstance</param>
        /// <param name="pi">PageInfo</param>
        protected bool WebPartActionAllowed(WebPartZoneInstance zone, PageInfo pi)
        {
            // In design mode or for layout zones, allow all actions
            if (PortalContext.IsDesignMode(ViewMode) || zone.LayoutZone)
            {
                return true;
            }

            switch (zone.WidgetZoneType)
            {
                // Editor zone
                case WidgetZoneTypeEnum.Editor:
                    return MembershipContext.AuthenticatedUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Editor, SiteContext.CurrentSiteName);

                // Group zone
                case WidgetZoneTypeEnum.Group:
                    return MembershipContext.AuthenticatedUser.IsGroupAdministrator(pi.NodeGroupID);

                // User zone
                case WidgetZoneTypeEnum.User:
                case WidgetZoneTypeEnum.Dashboard:
                    return AuthenticationHelper.IsAuthenticated();

                default:
                    return false;
            }
        }


        /// <summary>
        /// Copy web part(s) to the clipboard
        /// </summary>
        /// <returns>Returns true if web pat was added to the clipboard</returns>
        protected bool CopyWebPart()
        {
            string webPartControlId = ValidationHelper.GetString(hidWebPart.Value, "");
            string zoneId = ValidationHelper.GetString(hidZone.Value, "");
            string aliasPath = ValidationHelper.GetString(hidAliasPath.Value, "");
            Guid instanceGuid = ValidationHelper.GetGuid(hidGuid.Value, Guid.Empty);
            int zoneVariantId = ValidationHelper.GetInteger(hidZoneVariantId.Value, 0);

            // Add web part to the currently selected zone under currently selected page
            if (zoneId != "")
            {
                // Get source data
                PageInfo pi = PortalManager.GetPageInfoForEditing(aliasPath);
                if (pi != null)
                {
                    // Get template instance
                    PageTemplateInstance pti = CMSPortalManager.GetTemplateInstanceForEditing(pi);
                    if (pti != null)
                    {
                        WebPartZoneInstance zoneInstance = pti.GetZone(zoneId, zoneVariantId);
                        if (zoneInstance == null)
                        {
                            zoneInstance = pti.EnsureZone(zoneId);
                        }

                        if (zoneInstance != null)
                        {
                            // Copy zone web parts
                            if (String.IsNullOrEmpty(webPartControlId) && (instanceGuid == Guid.Empty))
                            {
                                if (zoneInstance.WebParts.Count > 0)
                                {
                                    WebPartClipBoardItem item = new WebPartClipBoardItem(zoneInstance, pti);
                                    WebPartClipBoardManager.SetClipBoardItem(item);
                                    return true;
                                }
                            }
                            // Copy single web part
                            else
                            {
                                // Get web part
                                WebPartInstance webPart = pti.GetWebPart(instanceGuid, zoneVariantId, 0) ?? pti.GetWebPart(webPartControlId);
                                if (webPart != null)
                                {
                                    WebPartClipBoardItem item = new WebPartClipBoardItem(webPart, pti);
                                    WebPartClipBoardManager.SetClipBoardItem(item);
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }


        /// <summary>
        /// Returns true
        /// </summary>
        /// <returns>Returns true if </returns>
        protected bool PasteWebPart()
        {
            string webPartControlId = ValidationHelper.GetString(hidWebPart.Value, "");
            string zoneId = ValidationHelper.GetString(hidZone.Value, "");
            string aliasPath = ValidationHelper.GetString(hidAliasPath.Value, "");
            Guid instanceGuid = ValidationHelper.GetGuid(hidGuid.Value, Guid.Empty);
            int zoneVariantId = ValidationHelper.GetInteger(hidZoneVariantId.Value, 0);
            bool isLayout = ValidationHelper.GetBoolean(hidIsLayoutZone.Value, false);
            WidgetZoneTypeEnum zoneType = hidZoneType.Value.ToEnum<WidgetZoneTypeEnum>();

            // Add web part to the currently selected zone under currently selected page
            if (!String.IsNullOrEmpty(zoneId))
            {
                // Get source data
                PageInfo pi = PortalManager.GetPageInfoForEditing(aliasPath);
                if (pi != null)
                {
                    // Get template instance
                    PageTemplateInstance pti = CMSPortalManager.GetTemplateInstanceForEditing(pi);
                    if (pti != null)
                    {
                        // Get the zone
                        WebPartZoneInstance zoneInstance = pti.GetZone(zoneId, zoneVariantId);
                        if (zoneInstance == null)
                        {
                            zoneInstance = pti.EnsureZone(zoneId);
                            zoneInstance.LayoutZone = isLayout;
                            zoneInstance.WidgetZoneType = zoneType;
                        }

                        if (zoneInstance != null)
                        {
                            // Check security
                            if (WebPartActionAllowed(zoneInstance, pi))
                            {
                                // Indicates whether at least one item was in clipboard
                                bool itemFound = false;

                                int position = -1;

                                // Try get position
                                WebPartInstance targetWebPart = pti.GetWebPart(instanceGuid, zoneVariantId, 0) ?? pti.GetWebPart(webPartControlId);
                                if (targetWebPart != null)
                                {
                                    position = zoneInstance.WebParts.FindIndex((WebPartInstance p) => (p.InstanceGUID == targetWebPart.InstanceGUID)) + 1;
                                }

                                // Try get from clipboard
                                WebPartClipBoardItem item = WebPartClipBoardManager.GetClipBoardItem(zoneInstance, pti);

                                if (item != null)
                                {
                                    // Loop thru all clipboard items
                                    foreach (WebPartInstance webPart in item.GetItems())
                                    {
                                        itemFound = true;

                                        // Ensure web parts
                                        zoneInstance.EnsureWebPartInstanceIdentificators(webPart);

                                        if (zoneVariantId > 0)
                                        {
                                            // Paste the web part
                                            var newWp = zoneInstance.AddWebPart(webPart, position);
                                            newWp.ClearValues("PositionTop", "PositionLeft");
                                        }
                                        else
                                        {
                                            // Relocate the web part
                                            var newWp = pti.AddWebPart(zoneId, webPart, position);

                                            newWp.ClearValues("PositionTop", "PositionLeft");

                                            // Remove variants of the cloned web part
                                            if (webPart.HasVariants)
                                            {
                                                // Get cloned web parts
                                                WebPartInstance clonedWebPart = zoneInstance.WebParts[zoneInstance.WebParts.Count - 1] as WebPartInstance;
                                                if (clonedWebPart != null)
                                                {
                                                    clonedWebPart.PartInstanceVariants = null;
                                                }
                                            }
                                        }

                                        if (position > -1)
                                        {
                                            position++;
                                        }
                                    }

                                    if (itemFound)
                                    {
                                        // Save the changes
                                        PortalManager.SaveZoneChanges(pi, pti, zoneInstance, false);
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }


        /// <summary>
        /// Returns zone found by zoneId. Search through all placeholders. If no zone is found, reutrns null.
        /// </summary>
        /// <param name="zoneId">Zone ID page placeholder should contains.</param>
        private CMSWebPartZone GetZone(String zoneId)
        {
            string aliasPath = DocumentContext.CurrentAliasPath;
            var placeholders = PortalManager.FindAllPlaceholders(aliasPath);
            return placeholders.Select(plc => plc.FindZone(zoneId)).FirstOrDefault(wpz => wpz != null);
        }


        /// <summary>
        /// Moves the web part to the specific location.
        /// </summary>
        /// <param name="reloadControls">If true, controls are reloaded after moving the web part</param>
        protected void MoveWebPart(bool reloadControls)
        {
            string webPartControlId = ValidationHelper.GetString(hidWebPart.Value, "");
            string zoneId = ValidationHelper.GetString(hidZone.Value, "");
            int zoneVariantId = ValidationHelper.GetInteger(hidSourceZoneVariantId.Value, -1);
            if (zoneVariantId == -1)
            {
                zoneVariantId = ValidationHelper.GetInteger(hidZoneVariantId.Value, -1);
            }
            string aliasPath = ValidationHelper.GetString(hidAliasPath.Value, "");
            Guid instanceGuid = ValidationHelper.GetGuid(hidGuid.Value, Guid.Empty);
            string targetZoneId = ValidationHelper.GetString(hidTargetZone.Value, "");
            int targetZoneVariantId = ValidationHelper.GetInteger(hidTargetZoneVariantId.Value, 0);
            int targetPosition = ValidationHelper.GetInteger(hidTargetPosition.Value, -1);

            // Move the web part
            if ((webPartControlId != "") && (zoneId != "") && (targetZoneId != ""))
            {
                // Check whether the target zone is layout zone
                bool layoutZone = false;
                CMSWebPartZone zoneControl = GetZone(targetZoneId);

                if (zoneControl != null)
                {
                    layoutZone = zoneControl.LayoutZone;
                }

                // Get source data
                PageInfo pi = PortalManager.GetPageInfoForEditing(aliasPath);
                if (pi != null)
                {
                    // Get template instance
                    PageTemplateInstance pti = CMSPortalManager.GetTemplateInstanceForEditing(pi);
                    if (pti != null)
                    {
                        // Get web part
                        WebPartInstance webPart = pti.GetWebPart(instanceGuid, zoneVariantId, 0) ?? pti.GetWebPart(webPartControlId);

                        // Get the zone
                        WebPartZoneInstance sourceZoneInstance = pti.GetZone(zoneId, zoneVariantId);

                        // Get the target zone
                        WebPartZoneInstance targetZoneInstance = pti.GetZone(targetZoneId);

                        // If the target zone is a layout zone and is not loaded yet (which happens when the layout zone has no web parts), load the layout zone
                        if (targetZoneInstance == null)
                        {
                            targetZoneInstance = pti.EnsureZone(targetZoneId);

                            // Try to get the layout web part id which contains the target zone
                            int sepIndex = targetZoneId.LastIndexOf('_');
                            if (sepIndex >= 0)
                            {
                                string layoutWebPartId = targetZoneId.Substring(0, sepIndex);

                                // Create the target layout zone
                                WebPartInstance layoutWebPart = pti.GetWebPart(layoutWebPartId);
                                if (layoutWebPart != null)
                                {
                                    targetZoneInstance.LayoutZone = true;
                                }
                            }
                        }

                        using (var h = WebPartEvents.MoveWebPart.StartEvent(new MoveWebPartsArgs()
                        {
                            Zone = sourceZoneInstance,
                            TargetZone = targetZoneInstance,
                            TemplateInfo = pi.UsedPageTemplateInfo,
                            WebPartInstance = webPart
                        }))
                        {
                            if (h.CanContinue())
                            {
                                // Abort moving the web part when the web part contains variants and the target zone contains also variants
                                if ((webPart != null) && webPart.HasVariants && targetZoneInstance.HasVariants)
                                {
                                    if (reloadControls)
                                    {
                                        ScriptHelper.RegisterStartupScript(this, typeof(string), "WebPartsActionReloadControls", ScriptHelper.GetScript("alert('" + ResHelper.GetString("webpartzone.webpartswithvariantsnotallowed") + "'); RefreshPage();"));
                                    }
                                    else
                                    {
                                        callbackResult = ERROR_REFRESHPAGE + SEPARATOR + ResHelper.GetString("webpartzone.webpartswithvariantsnotallowed");
                                    }

                                    return;
                                }

                                if (targetZoneVariantId > 0)
                                {
                                    // Get the target zone variant
                                    targetZoneInstance = pti.GetZone(targetZoneId, targetZoneVariantId);
                                }

                                if ((sourceZoneInstance != null) && (webPart != null))
                                {
                                    // Check security
                                    if (WebPartActionAllowed(sourceZoneInstance, pi))
                                    {
                                        // Ensure the layout zone flag of the target zone
                                        if (layoutZone)
                                        {
                                            targetZoneInstance.LayoutZone = true;
                                            targetZoneInstance.WidgetZoneType = sourceZoneInstance.WidgetZoneType;
                                        }


                                        #region "Relocate the web part"

                                        // Check if the web part can be moved
                                        if (targetPosition >= 0)
                                        {
                                            // Move the web part
                                            pti.MoveWebPart(webPart, sourceZoneInstance, targetZoneInstance, targetZoneId, targetPosition);
                                        }

                                        // Apply the position (location) in free layout
                                        ApplyPosition(webPart);

                                        // Save the changes
                                        PortalManager.SaveZoneChanges(pi, pti, sourceZoneInstance, false);

                                        // Save the only the zone which has not been saved already
                                        if (targetZoneVariantId != zoneVariantId)
                                        {
                                            // Save the changes
                                            PortalManager.SaveZoneChanges(pi, pti, targetZoneInstance, false);
                                        }

                                        h.FinishEvent();

                                        // Ensure that the text area and input element ID and NAME parameters are updated when moving widgets between zones
                                        if (sourceZoneInstance.WidgetZoneType == WidgetZoneTypeEnum.Editor)
                                        {
                                            CMSWebPartZone sourceZoneControl = CurrentPlaceholder.FindZone(zoneId);
                                            CMSWebPartZone targetZoneControl = CurrentPlaceholder.FindZone(targetZoneId);
                                            if ((sourceZoneControl != null)
                                                && (targetZoneControl != null))
                                            {
                                                callbackResult = UPDATE_WIDGET_INPUT_ELEMENT_IDENTIFIERS
                                                    + SEPARATOR + sourceZoneControl.UniqueID
                                                    + SEPARATOR + targetZoneControl.UniqueID;
                                            }
                                        }

                                        #endregion

                                    }
                                }
                            }

                            // Reload the contents
                            if (reloadControls)
                            {
                                CMSPortalManager.RefreshPage();
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Applies the relative position to the web part
        /// </summary>
        /// <param name="webPart">Web part instance</param>
        private void ApplyPosition(WebPartInstance webPart)
        {
            // Set new position
            if (mNewPositionLeft != null)
            {
                webPart.SetValue("PositionLeft", mNewPositionLeft);
            }
            if (mNewPositionTop != null)
            {
                webPart.SetValue("PositionTop", mNewPositionTop);
            }
        }


        /// <summary>
        /// Sets the web part property.
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <param name="value">Property value</param>
        /// <param name="line">Line of the web part property to edit</param>
        protected void SetWebPartProperty(string propertyName, string value, int line)
        {
            string webPartControlId = ValidationHelper.GetString(hidWebPart.Value, "");
            string zoneId = ValidationHelper.GetString(hidZone.Value, "");
            string aliasPath = ValidationHelper.GetString(hidAliasPath.Value, "");
            Guid instanceGuid = ValidationHelper.GetGuid(hidGuid.Value, Guid.Empty);
            int variantId = ValidationHelper.GetInteger(hidVariant.Value, 0);

            // Move the web part
            if ((webPartControlId != "") && (zoneId != ""))
            {
                // Get source data
                PageInfo pi = PortalManager.GetPageInfoForEditing(aliasPath);
                if (pi != null)
                {
                    // Get template instance
                    PageTemplateInstance pti = CMSPortalManager.GetTemplateInstanceForEditing(pi);
                    if (pti != null)
                    {
                        // Get web part
                        WebPartInstance webPart = pti.GetWebPart(instanceGuid) ?? pti.GetWebPart(webPartControlId);
                        if (webPart != null)
                        {
                            // Get the zone
                            WebPartZoneInstance zone = webPart.ParentZone;
                            if (zone != null)
                            {
                                // Check security
                                if (WebPartActionAllowed(zone, pi))
                                {
                                    // Get the variant if variant specified
                                    if (variantId > 0)
                                    {
                                        webPart = webPart.FindVariant(variantId);
                                    }

                                    if (webPart != null)
                                    {
                                        string newValue = value;

                                        if (line > 0)
                                        {
                                            // Parse the lines and inject the new line
                                            newValue = ValidationHelper.GetString(webPart.GetValue(propertyName), "");
                                            newValue = TextHelper.SetLine(newValue, value, line);
                                        }

                                        // Set the value
                                        webPart.SetValue(propertyName, newValue);

                                        // Save the changes
                                        CMSPortalManager.SaveTemplateChanges(pi, pti, zone.WidgetZoneType, ViewMode, TreeProvider);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Sets the web part property.
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <param name="value">Property value</param>
        protected void AddToWebPartProperty(string propertyName, string value)
        {
            string webPartControlId = ValidationHelper.GetString(hidWebPart.Value, "");
            string zoneId = ValidationHelper.GetString(hidZone.Value, "");
            string aliasPath = ValidationHelper.GetString(hidAliasPath.Value, "");
            Guid instanceGuid = ValidationHelper.GetGuid(hidGuid.Value, Guid.Empty);
            int variantId = ValidationHelper.GetInteger(hidVariant.Value, 0);

            // Move the web part
            if ((webPartControlId != "") && (zoneId != ""))
            {
                // Get source data
                PageInfo pi = PortalManager.GetPageInfoForEditing(aliasPath);
                if (pi != null)
                {
                    // Get template instance
                    PageTemplateInstance pti = CMSPortalManager.GetTemplateInstanceForEditing(pi);
                    if (pti != null)
                    {
                        // Get web part
                        WebPartInstance webPart = pti.GetWebPart(instanceGuid) ?? pti.GetWebPart(webPartControlId);
                        if (webPart != null)
                        {
                            // Get the zone
                            WebPartZoneInstance zone = webPart.ParentZone;
                            if (zone != null)
                            {
                                // Check security
                                if (WebPartActionAllowed(zone, pi))
                                {
                                    // Get the variant if variant specified
                                    if (variantId > 0)
                                    {
                                        webPart = webPart.FindVariant(variantId);
                                    }

                                    if (webPart != null)
                                    {
                                        int newValue = ValidationHelper.GetInteger(webPart.GetValue(propertyName), 0);
                                        newValue += ValidationHelper.GetInteger(value, 0);

                                        // Set the value
                                        webPart.SetValue(propertyName, newValue);

                                        // Save the changes
                                        CMSPortalManager.SaveTemplateChanges(pi, pti, zone.WidgetZoneType, ViewMode, TreeProvider);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Remove all web parts event handler.
        /// </summary>
        private void RemoveAllWebParts(bool refresh = true)
        {
            string zoneId = ValidationHelper.GetString(hidZone.Value, "");
            int zoneVariantId = ValidationHelper.GetInteger(hidZoneVariantId.Value, 0);
            string aliasPath = ValidationHelper.GetString(hidAliasPath.Value, "");

            // Add web part to the currently selected zone under currently selected page
            if (zoneId != "")
            {
                // Get the correct page info
                PageInfo pi = PortalManager.GetPageInfoForEditing(aliasPath);
                if (pi != null)
                {
                    // Get template instance
                    PageTemplateInstance pti = CMSPortalManager.GetTemplateInstanceForEditing(pi);
                    if (pti != null)
                    {
                        // Get web part zone instance
                        WebPartZoneInstance zone = pti.GetZone(zoneId, zoneVariantId);

                        if (zone != null)
                        {
                            zone.RemoveAllWebParts();

                            // Save the changes
                            PortalManager.SaveZoneChanges(pi, pti, zone, refresh);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Moves all the web parts in the given zone to the specific location.
        /// </summary>
        protected void MoveAllWebParts()
        {
            string zoneId = ValidationHelper.GetString(hidZone.Value, "");
            string aliasPath = ValidationHelper.GetString(hidAliasPath.Value, "");
            string targetZoneId = ValidationHelper.GetString(hidTargetZone.Value, "");
            int zoneVariantId = ValidationHelper.GetInteger(hidSourceZoneVariantId.Value, -1);
            if (zoneVariantId == -1)
            {
                zoneVariantId = ValidationHelper.GetInteger(hidZoneVariantId.Value, -1);
            }
            int targetZoneVariantId = ValidationHelper.GetInteger(hidTargetZoneVariantId.Value, 0);

            // Move the web part
            if ((zoneId != "") && (targetZoneId != ""))
            {
                // Get source data
                PageInfo pi = PortalManager.GetPageInfoForEditing(aliasPath);
                if (pi != null)
                {
                    // Get template instance
                    PageTemplateInstance pti = CMSPortalManager.GetTemplateInstanceForEditing(pi);
                    if (pti != null)
                    {
                        // Get the zones
                        WebPartZoneInstance zone = pti.GetZone(zoneId, zoneVariantId);
                        WebPartZoneInstance targetZone = pti.GetZone(targetZoneId, targetZoneVariantId);

                        // If the target zone is a layout zone and is not loaded yet (which happens when the layout zone has no web parts), load the layout zone
                        if (targetZone == null)
                        {
                            targetZone = pti.EnsureZone(targetZoneId);

                            // Try to get the layout web part id which contains the target zone
                            int sepIndex = targetZoneId.LastIndexOf('_');
                            if (sepIndex >= 0)
                            {
                                string layoutWebPartId = targetZoneId.Substring(0, sepIndex);

                                // Create the target layout zone
                                WebPartInstance layoutWebPart = pti.GetWebPart(layoutWebPartId);
                                if (layoutWebPart != null)
                                {
                                    targetZone.LayoutZone = true;
                                }
                            }
                        }

                        if ((zone != null) && (targetZone != null))
                        {
                            // Abort moving the web parts if any of the web parts contains variants and also the target zone contains variants
                            if (zone.WebPartsContainVariants)
                            {
                                WebPartZoneInstance targetOriginalZone = targetZone;
                                if (targetZoneVariantId > 0)
                                {
                                    // Get the original target zone
                                    targetOriginalZone = pti.GetZone(targetZoneId, 0);
                                }

                                // Show the alert message
                                if ((targetOriginalZone != null) && targetOriginalZone.HasVariants)
                                {
                                    ScriptHelper.RegisterStartupScript(this, typeof(string), "targetOriginalZone", ScriptHelper.GetScript("alert('" + ResHelper.GetString("webpartzone.webpartswithvariantsnotallowed") + "'); RefreshPage();"));
                                    return;
                                }
                            }
                        }

                        using (var h = WebPartEvents.MoveAllWebParts.StartEvent(new MoveWebPartsArgs()
                        {
                            Zone = zone,
                            TargetZone = targetZone,
                            TemplateInfo = pi.UsedPageTemplateInfo
                        }))
                        {
                            if (h.CanContinue())
                            {
                                if ((zoneId != targetZoneId) || (zoneVariantId != targetZoneVariantId))
                                {
                                    // Relocate all web parts
                                    pti.MoveAllWebParts(zone, targetZone);

                                    // Save the changes
                                    PortalManager.SaveZoneChanges(pi, pti, zone, false);

                                    // Save the only the zone which has not been saved already
                                    if (targetZoneVariantId != zoneVariantId)
                                    {
                                        // Save the changes
                                        PortalManager.SaveZoneChanges(pi, pti, targetZone, false);
                                    }

                                    h.FinishEvent();
                                }
                            }

                            CMSPortalManager.RefreshPage();
                        }
                    }
                }
            }
        }


        private void WebPartAction(WebPartActionEnum action, bool refresh = true)
        {
            string webPartControlId = ValidationHelper.GetString(hidWebPart.Value, "");
            string zoneId = ValidationHelper.GetString(hidZone.Value, "");
            int zoneVariantId = ValidationHelper.GetInteger(hidZoneVariantId.Value, 0);
            string aliasPath = ValidationHelper.GetString(hidAliasPath.Value, "");
            Guid instanceGuid = ValidationHelper.GetGuid(hidGuid.Value, Guid.Empty);

            // Add web part to the currently selected zone under currently selected page
            if ((webPartControlId != "") && (zoneId != ""))
            {
                // Get source data
                PageInfo pi = PortalManager.GetPageInfoForEditing(aliasPath);
                if (pi != null)
                {
                    // Get template instance
                    PageTemplateInstance pti = CMSPortalManager.GetTemplateInstanceForEditing(pi);
                    if (pti != null)
                    {
                        // Get web part
                        WebPartInstance webPart = pti.GetWebPart(instanceGuid, zoneVariantId, 0) ?? pti.GetWebPart(webPartControlId);

                        // Get the zone
                        WebPartZoneInstance zoneInstance = pti.GetZone(zoneId, zoneVariantId);

                        if ((zoneInstance != null) && (webPart != null))
                        {
                            // Check security
                            if (WebPartActionAllowed(zoneInstance, pi))
                            {
                                switch (action)
                                {
                                    // Up
                                    case WebPartActionEnum.Up:
                                        pti.MoveWebPartUp(webPart, false);
                                        break;

                                    // Down
                                    case WebPartActionEnum.Down:
                                        pti.MoveWebPartDown(webPart, false);
                                        break;

                                    // Top
                                    case WebPartActionEnum.Top:
                                        pti.MoveWebPartUp(webPart, true);
                                        break;

                                    // Bottom
                                    case WebPartActionEnum.Bottom:
                                        pti.MoveWebPartDown(webPart, true);
                                        break;

                                    // Remove
                                    case WebPartActionEnum.Remove:
                                        zoneInstance.RemoveWebPart(webPart);
                                        break;
                                }

                                // Save the changes
                                PortalManager.SaveZoneChanges(pi, pti, zoneInstance, refresh);
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Changes widget window state
        /// </summary>
        /// <param name="requiredState">Required window state (maximize/minimize)</param>
        private void ChangeWidgetWindowState(WidgetStateEnum requiredState)
        {
            string webPartControlId = ValidationHelper.GetString(hidWebPart.Value, "");
            string aliasPath = ValidationHelper.GetString(hidAliasPath.Value, "");
            Guid instanceGuid = ValidationHelper.GetGuid(hidGuid.Value, Guid.Empty);

            // Maximize the widget
            if (webPartControlId != "")
            {
                // Get source data
                PageInfo pi = PortalManager.GetPageInfoForEditing(aliasPath);
                if (pi != null)
                {
                    // Get template instance
                    PageTemplateInstance pti = CMSPortalManager.GetTemplateInstanceForEditing(pi);
                    if (pti != null)
                    {
                        // Get web part
                        WebPartInstance webPart = pti.GetWebPart(instanceGuid) ?? pti.GetWebPart(webPartControlId);
                        if (webPart != null)
                        {
                            switch (requiredState)
                            {
                                case WidgetStateEnum.Normal:
                                case WidgetStateEnum.Maximize:
                                    webPart.Minimized = false;
                                    break;

                                case WidgetStateEnum.Minimize:
                                    webPart.Minimized = true;
                                    break;
                            }

                            // Save the changes
                            CMSPortalManager.SaveTemplateChanges(pi, pti, (ViewMode == ViewModeEnum.DashboardWidgets) ? WidgetZoneTypeEnum.Dashboard : WidgetZoneTypeEnum.User, ViewMode, TreeProvider);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Clone web part event handler.
        /// </summary>
        private void CloneWebPart()
        {
            string webPartControlId = ValidationHelper.GetString(hidWebPart.Value, "");
            string zoneId = ValidationHelper.GetString(hidZone.Value, "");
            string aliasPath = ValidationHelper.GetString(hidAliasPath.Value, "");
            Guid instanceGuid = ValidationHelper.GetGuid(hidGuid.Value, Guid.Empty);
            int zoneVariantId = ValidationHelper.GetInteger(hidZoneVariantId.Value, 0);

            // Add web part to the currently selected zone under currently selected page
            if ((webPartControlId != "") && (zoneId != ""))
            {
                // Get source data
                PageInfo pi = PortalManager.GetPageInfoForEditing(aliasPath);
                if (pi != null)
                {
                    // Get template instance
                    PageTemplateInstance pti = CMSPortalManager.GetTemplateInstanceForEditing(pi);
                    if (pti != null)
                    {
                        // Get web part
                        WebPartInstance webPart = pti.GetWebPart(instanceGuid, zoneVariantId, 0) ?? pti.GetWebPart(webPartControlId);

                        // Get the zone
                        WebPartZoneInstance zoneInstance = pti.GetZone(zoneId, zoneVariantId);

                        if ((zoneInstance != null) && (webPart != null))
                        {
                            // Check security
                            if (WebPartActionAllowed(zoneInstance, pi))
                            {
                                if (zoneVariantId > 0)
                                {
                                    // Clone the web part
                                    var newWp = zoneInstance.CloneWebPart(webPart.ControlID);
                                    newWp.ClearValues("PositionTop", "PositionLeft");
                                }
                                else
                                {
                                    // Relocate the web part
                                    var newWp = pti.CloneWebPart(webPart);

                                    newWp.ClearValues("PositionTop", "PositionLeft");

                                    // Remove variants of the cloned web part
                                    if (webPart.HasVariants)
                                    {
                                        // Get cloned web parts
                                        WebPartInstance clonedWebPart = zoneInstance.WebParts[zoneInstance.WebParts.Count - 1] as WebPartInstance;
                                        if (clonedWebPart != null)
                                        {
                                            clonedWebPart.PartInstanceVariants = null;
                                        }
                                    }
                                }

                                // Save the changes
                                PortalManager.SaveZoneChanges(pi, pti, zoneInstance, true);
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #endregion
    }
}
