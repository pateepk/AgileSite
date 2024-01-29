using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.DeviceProfiles;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.SiteProvider;
using CMS.WorkflowEngine;

using TreeNode = CMS.DocumentEngine.TreeNode;

namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// Portal control functions.
    /// </summary>
    public static class PortalHelper
    {
        #region "Variables"

        /// <summary>
        /// If true, the orphaned web part zones will be shown in design mode.
        /// </summary>
        private static bool? mShowOrphanedWebPartZones;

        /// <summary>
        /// If true, the orphaned web part zones from layouts will be shown in design mode.
        /// </summary>
        private static bool? mShowOrphanedLayoutWebPartZones;

        /// <summary>
        /// If true, the web part ID CSS class is rendered for web parts.
        /// </summary>
        private static bool? mRenderWebPartIDCssClass;

        /// <summary>
        /// Default on-site edit URL
        /// </summary>
        private static string mOnSiteEditRelativeURL = "~/CMSEdit/";

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the on-site edit relative URL. Access to this URL switch user to the editing mode if is authorized to this mode.
        /// </summary>
        public static string OnSiteEditRelativeURL
        {
            get
            {
                return mOnSiteEditRelativeURL;
            }
            set
            {
                mOnSiteEditRelativeURL = value;
            }

        }


        /// <summary>
        /// If true, the orphaned web part zones will be shown in design mode.
        /// </summary>
        public static bool ShowOrphanedWebPartZones
        {
            get
            {
                if (mShowOrphanedWebPartZones == null)
                {
                    mShowOrphanedWebPartZones = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSShowOrphanedWebPartZones"], true);
                }

                return mShowOrphanedWebPartZones.Value;
            }
            set
            {
                mShowOrphanedWebPartZones = value;
            }
        }


        /// <summary>
        /// If true, the orphaned web part zones from layout web parts will be shown in design mode.
        /// </summary>
        public static bool ShowOrphanedLayoutWebPartZones
        {
            get
            {
                if (mShowOrphanedLayoutWebPartZones == null)
                {
                    mShowOrphanedLayoutWebPartZones = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSShowOrphanedLayoutWebPartZones"], false);
                }

                return mShowOrphanedLayoutWebPartZones.Value;
            }
            set
            {
                mShowOrphanedLayoutWebPartZones = value;
            }
        }


        /// <summary>
        /// If true, the web part ID CSS class is rendered for web parts.
        /// </summary>
        public static bool RenderWebPartIDCssClass
        {
            get
            {
                if (mRenderWebPartIDCssClass == null)
                {
                    mRenderWebPartIDCssClass = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSRenderWebPartIDCssClass"], false);
                }

                return mRenderWebPartIDCssClass.Value;
            }
            set
            {
                mRenderWebPartIDCssClass = value;
            }
        }


        /// <summary>
        /// If true, safe mode is required (only for new design mode).
        /// </summary>
        public static bool SafeMode
        {
            get
            {
                return ValidationHelper.GetBoolean(HttpContext.Current.Request.QueryString["safemode"], false);
            }
        }


        /// <summary>
        /// If true, web part content should be displayed in design mode.
        /// </summary>
        public static bool DisplayContentInDesignMode
        {
            get
            {
                return ValidationHelper.GetBoolean(ContextHelper.GetItem(CookieName.DisplayContentInDesignMode, true, false, true), true);
            }
            set
            {
                ContextHelper.Add(CookieName.DisplayContentInDesignMode, (value ? "1" : "0"), true, false, true, DateTime.Now.AddYears(1));
            }
        }


        /// <summary>
        /// If true, web part content should be displayed in UI element design mode.
        /// </summary>
        public static bool DisplayContentInUIElementDesignMode
        {
            get
            {
                return ValidationHelper.GetBoolean(ContextHelper.GetItem(CookieName.DisplayContentInUIElementDesignMode, true, false, true), true);
            }
            set
            {
                ContextHelper.Add(CookieName.DisplayContentInUIElementDesignMode, (value ? "1" : "0"), true, false, true, DateTime.Now.AddYears(1));
            }
        }


        /// <summary>
        /// Determines whether any device profile is being used at the moment and its dimensions are valid.
        /// </summary>
        public static bool DeviceProfileActive
        {
            get
            {
                var profile = DeviceContext.CurrentDeviceProfile;

                return (profile != null) && (profile.ProfilePreviewWidth > 0) && (profile.ProfilePreviewHeight > 0);
            }
        }


        /// <summary>
        /// Indicates whether the device rotate button should be available.
        /// </summary>
        public static bool ShowDevicePreviewRotator
        {
            get
            {
                return (PortalContext.ViewMode == ViewModeEnum.Preview) && DeviceProfileActive;
            }
        }


        /// <summary>
        /// Indicates whether device selection button should be available.
        /// </summary>
        public static bool ShowDeviceSelection
        {
            get
            {
                // Hide the device selection in the preview mode and when a device profile is selected
                // => Device selection is displayed by another control in this mode which also displays selected device layout => do not display by portal manager
                bool forceHide = (PortalContext.ViewMode == ViewModeEnum.Preview) && DeviceProfileActive;
                bool showSelection = QueryHelper.GetBoolean("showdevicesselection", !VirtualContext.IsPreviewLinkInitialized);

                return showSelection && !forceHide;
            }
        }


        /// <summary>
        /// Gets the default web part font icon CSS class.
        /// </summary>
        public static string DefaultPageTemplateIconClass
        {
            get
            {
                return "icon-layout";
            }
        }


        /// <summary>
        /// Gets the default web part font icon CSS class.
        /// </summary>
        public static string DefaultPageLayoutIconClass
        {
            get
            {
                return "icon-layout";
            }
        }


        /// <summary>
        /// Gets the default web part font icon CSS class.
        /// </summary>
        public static string DefaultWebPartIconClass
        {
            get
            {
                return "icon-w-webpart-default";
            }
        }


        /// <summary>
        /// Gets the default widget font icon CSS class.
        /// </summary>
        public static string DefaultWidgetIconClass
        {
            get
            {
                return "icon-w-widget-default";
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets the HTML attributes for the disabled form element
        /// </summary>
        public static string GetDisabledFormElementAttributes()
        {
            return " onclick=\"alert('" + ScriptHelper.GetString(ResHelper.GetString("PortalHelper.DisabledElement"), false) + "'); return false;\" class=\"dont-check-changes\"";
        }


        /// <summary>
        /// Registers the layouts script
        /// </summary>
        /// <param name="page">Page for which register the script</param>
        public static void RegisterLayoutsScript(Page page)
        {
            ScriptHelper.RegisterJQuery(page);
            ScriptHelper.RegisterScriptFile(page, "DesignMode/layouts.js");
        }


        /// <summary>
        /// Gets the value that indicates whether on site editing is enabled for specific site name
        /// </summary>
        public static bool IsOnSiteEditingEnabled(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSAllowOnSiteEditing");
        }


        /// <summary>
        /// Finds the Portal manager within the controls structure.
        /// </summary>
        /// <param name="sender">Control that requests to find the manager</param>
        /// <param name="searchUp">If true, parent controls are searched</param>
        public static CMSPortalManager FindManager(Control sender, bool searchUp = true)
        {
            // If found or no control given, return the original value
            if ((sender == null) || (sender is CMSPortalManager))
            {
                return (CMSPortalManager)sender;
            }
            // Search up or down the tree
            if (searchUp)
            {
                // If no parent found, switch to search down
                if (sender.Parent == null)
                {
                    return FindManager(sender, false);
                }

                // Search up the tree
                Control parent = sender.Parent;
                if (parent is CMSPortalManager)
                {
                    return (CMSPortalManager)parent;
                }

                if (parent is ICMSPortalControl)
                {
                    // If parent is portal control, let the parent do the searching or return the manager known
                    return ((ICMSPortalControl)parent).PortalManager;
                }

                return FindManager(parent);
            }

            // Search down the tree
            foreach (Control child in sender.Controls)
            {
                // Check if the control is manager
                if (child is CMSPortalManager)
                {
                    return (CMSPortalManager)child;
                }

                // Search the inner controls
                CMSPortalManager result = FindManager(child, false);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }


        /// <summary>
        /// Find page manager within the given page.
        /// </summary>
        /// <param name="page">Page where to seek for Page manager</param>
        public static IPageManager FindPageManager(Page page)
        {
            if (page == null)
            {
                return null;
            }

            if (page.Master != null)
            {
                return FindPageManager(page.Master);
            }

            return FindPageManager((Control)page);
        }


        /// <summary>
        /// Tries to find the page manager within the controls structure.
        /// </summary>
        public static IPageManager FindPageManager(Control sender)
        {
            // If clear, return the result
            if (sender == null)
            {
                return null;
            }
            if (sender is IPageManager)
            {
                return (IPageManager)sender;
            }

            // Try to find the manager in child controls
            foreach (Control child in sender.Controls)
            {
                // If PageManager, return
                if (child is IPageManager)
                {
                    return (IPageManager)child;
                }
                else
                {
                    // Search child control
                    IPageManager childManager = FindPageManager(child);
                    if (childManager != null)
                    {
                        return childManager;
                    }
                }
            }
            return null;
        }


        /// <summary>
        /// Finds the parent page placeholder within the controls structure.
        /// </summary>
        /// <param name="sender">Control that requests to find the placeholder</param>
        public static CMSPagePlaceholder FindParentPlaceholder(Control sender)
        {
            // Is sender null, unable to find parent
            if (sender == null)
            {
                return null;
            }
            // Find the parent
            Control parent = sender.Parent;
            while (parent != null)
            {
                // If found, return
                if (parent is CMSPagePlaceholder)
                {
                    return (CMSPagePlaceholder)parent;
                }
                else
                {
                    parent = parent.Parent;
                }
            }
            return null;
        }


        /// <summary>
        /// Gets the web part control instance from given control.
        /// </summary>
        /// <param name="control">Initial control</param>
        public static CMSAbstractWebPart GetWebPartControl(Control control)
        {
            // Go inside the partial caching control
            if (control is PartialCachingControl)
            {
                control = ((PartialCachingControl)control).CachedControl;
            }

            // Validate abstract web part
            if ((control != null) && !(control is CMSAbstractWebPart))
            {
                control = null;
            }

            return (CMSAbstractWebPart)control;
        }


        /// <summary>
        /// Gets the view mode type with dependence on current node workflow step
        /// </summary>
        /// <param name="node">Current node</param>
        /// <param name="manager">Current document manager</param>
        /// <param name="viewMode">Current view mode</param>
        public static ViewModeEnum GetWorkflowViewMode(TreeNode node, ICMSDocumentManager manager, ViewModeEnum viewMode)
        {
            // Setup the workflow information
            WorkflowInfo wi = manager.Workflow;
            if (wi != null)
            {
                // Get current step info, do not update document
                WorkflowStepInfo si = manager.Step;

                if (si != null)
                {
                    bool useCheckInCheckOut = wi.UseCheckInCheckOut(node.NodeSiteName);
                    bool canApprove = manager.WorkflowManager.CheckStepPermissions(node, WorkflowActionEnum.Approve);

                    // Check the edit status (switch to EditDisabled mode if not checked out)
                    if (!node.IsCheckedOut)
                    {
                        // Check-in, Check-out
                        if (useCheckInCheckOut && canApprove)
                        {
                            if (viewMode.IsEdit())
                            {
                                viewMode = ViewModeEnum.EditDisabled;
                            }
                        }
                    }
                    else
                    {
                        // If checked out by current user, add the check-in button
                        int checkedOutBy = node.DocumentCheckedOutByUserID;
                        if (checkedOutBy != MembershipContext.AuthenticatedUser.UserID)
                        {
                            if (viewMode.IsEdit())
                            {
                                viewMode = ViewModeEnum.EditDisabled;
                            }
                        }
                    }

                    if (!canApprove)
                    {
                        if (viewMode.IsEdit())
                        {
                            viewMode = ViewModeEnum.EditDisabled;
                        }
                    }
                }
            }

            return viewMode;
        }


        /// <summary>
        /// Gets the javascript code which ensures adding a new inline widget to the page.
        /// </summary>
        /// <param name="widgetInfo">The widget info object</param>
        /// <param name="row">The widget data</param>
        /// <param name="fields">The list of inline widget properties which will be saved in the text editor</param>
        /// <param name="additionalFieldsNames">Names of fields that are required to be saved within widget properties.</param>
        public static string GetAddInlineWidgetScript(WidgetInfo widgetInfo, DataRow row, List<FormFieldInfo> fields, IEnumerable<string> additionalFieldsNames)
        {
            if (fields == null)
            {
                return String.Empty;
            }
            StringBuilder sb = new StringBuilder();
            sb.Append("var widgetObj = new Object(); \n");

            // Name of the widget is first argument
            sb.Append(String.Format("widgetObj['name']='{0}';", HttpUtility.UrlEncode(widgetInfo.WidgetName)));

            foreach (FormFieldInfo ffi in fields)
            {
                if (row.Table.Columns.Contains(ffi.Name))
                {
                    // Store only visible or explicitly specified fields, invisible won't be resolved anyway
                    if (ffi.Visible || additionalFieldsNames.Contains(ffi.Name))
                    {
                        String value = Convert.ToString(row[ffi.Name], CultureHelper.EnglishCulture);
                        value = value.Replace("%", "%25");

                        sb.Append(String.Format("widgetObj['{0}'] = {1}; \n", ffi.Name, ScriptHelper.GetString(HttpUtility.UrlEncode(value))));
                    }
                }
            }

            // Add image GUID
            DataSet ds = MetaFileInfoProvider.GetMetaFiles("MetaFileObjectID = " + widgetInfo.WidgetID + "  AND MetaFileObjectType = 'cms.widget'", String.Empty, "MetafileGuid", 0);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                Guid guid = ValidationHelper.GetGuid(ds.Tables[0].Rows[0]["MetafileGuid"], Guid.Empty);
                sb.Append("widgetObj['image_guid'] = '" + guid.ToString() + "'; \n");
            }

            // Add localized display name
            string displayName = ResHelper.LocalizeString(widgetInfo.WidgetDisplayName);
            sb.Append("widgetObj['widget_displayname'] = " + ScriptHelper.GetString(HttpUtility.UrlEncode(displayName.Replace("%", "%25"))) + "; \n");

            // Create javascript for save
            sb.Append(
@"widgetObj['cms_type'] = 'widget';
if (typeof(InsertSelectedItem) !== 'undefined') {
    InsertSelectedItem(widgetObj);
}
else {
    if ((wopener != null) && (typeof(wopener.CKEDITOR) !== 'undefined')) {
        wopener.CMSPlugin.insert(widgetObj);
    }
    else if (CKEDITOR != null) {
        CMSPlugin.insert(widgetObj);
    }
}");

            return sb.ToString();
        }


        /// <summary>
        /// Adds the new web part to the page and returns its instance.
        /// </summary>
        /// <param name="webpartId">The web part id</param>
        /// <param name="zoneId">The zone id</param>
        /// <param name="isLayoutZone">Indicates whether the zone is a layout zone</param>
        /// <param name="zoneVariantId">The zone variant id</param>
        /// <param name="position">The position of the web part in the zone</param>
        /// <param name="templateInstance">The template instance</param>
        /// <param name="row">The data of the new web part</param>
        public static WebPartInstance AddNewWebPart(int webpartId, string zoneId, bool isLayoutZone, int zoneVariantId, int position, PageTemplateInstance templateInstance, DataRow row = null)
        {
            WebPartInstance webPartInstance = null;

            // Add web part to the currently selected zone under currently selected page
            if ((webpartId > 0)
                && !String.IsNullOrEmpty(zoneId)
                && (templateInstance != null))
            {
                // Get the web part by code name
                WebPartInfo wi = WebPartInfoProvider.GetWebPartInfo(webpartId);
                if (wi != null)
                {
                    if (isLayoutZone)
                    {
                        // Ensure that the layout zone is created before inserting the new web part
                        WebPartZoneInstance zone = templateInstance.EnsureZone(zoneId);
                        zone.LayoutZone = true;
                    }

                    bool fromDefaultConfig = false;

                    // Try to use default web part configuration
                    webPartInstance = wi.DefaultConfiguration.GetWebPart("webpart");
                    if ((webPartInstance != null) && (row != null))
                    {
                        webPartInstance = webPartInstance.Clone(true);
                        webPartInstance.ControlID = wi.WebPartName;
                        webPartInstance.InstanceGUID = Guid.NewGuid();

                        webPartInstance = templateInstance.AddWebPart(zoneId, webPartInstance, position);

                        fromDefaultConfig = true;
                    }
                    else
                    {
                        // Add the web part
                        if (zoneVariantId == 0)
                        {
                            webPartInstance = templateInstance.AddWebPart(zoneId, webpartId, position);
                        }
                        else
                        {
                            // Add the new web part to the zone variant
                            WebPartZoneInstance wpzi = templateInstance.EnsureZone(zoneId);

                            // Load the zone variants if not loaded yet
                            if (wpzi.ZoneInstanceVariants == null)
                            {
                                wpzi.LoadVariants(false, VariantModeEnum.None);
                            }

                            // Find the correct zone variant
                            wpzi = wpzi.ZoneInstanceVariants.Find(z => z.VariantID.Equals(zoneVariantId));
                            if (wpzi != null)
                            {
                                webPartInstance = wpzi.AddWebPart(webpartId, position);
                            }
                        }
                    }

                    if (webPartInstance != null)
                    {
                        if (fromDefaultConfig)
                        {
                            var ids = new StringSafeDictionary<string>();
                            var insertedWebParts = new HashSet<Guid>() { webPartInstance.InstanceGUID };
                            var webParts = new List<WebPartInstance>();

                            // Add the nested zones of the inserted web part
                            if (wi.DefaultConfiguration == templateInstance)
                            {
                                // Ensure that the target is not the same as source
                                wi.DefaultConfiguration = wi.DefaultConfiguration.Clone();
                            }

                            AddNestedZones(wi.DefaultConfiguration, WebPartInfo.DEFAULT_CONFIG_WEBPARTNAME, templateInstance, webPartInstance.ControlID, webParts, ids, insertedWebParts);

                            // Synchronize ID changes
                            foreach (var partInstance in webParts)
                            {
                                TranslateWebPartID(partInstance, "DataSourceName", ids);
                                TranslateWebPartID(partInstance, "FilterName", ids);
                            }
                        }
                        else
                        {

                            if (row == null)
                            {
                                row = PortalFormHelper.GetDefaultWebPartProperties(wi);
                            }

                            webPartInstance.LoadProperties(row);
                        }

                        // Add web part to user's last recently used (except UI web parts)
                        if ((WebPartTypeEnum)wi.WebPartType != WebPartTypeEnum.UI)
                        {
                            MembershipContext.AuthenticatedUser.UserSettings.UpdateRecentlyUsedWebPart(wi.WebPartName);
                        }
                    }
                }
            }

            return webPartInstance;
        }


        /// <summary>
        /// Translate the web part ID in the specified property to a new ID based on the translation table
        /// </summary>
        /// <param name="partInstance">Web part instance</param>
        /// <param name="propertyName">Property name</param>
        /// <param name="ids">ID translation table</param>
        private static void TranslateWebPartID(WebPartInstance partInstance, string propertyName, StringSafeDictionary<string> ids)
        {
            var dataSourceId = ValidationHelper.GetString(partInstance.GetValue(propertyName), "");
            if (!String.IsNullOrEmpty(dataSourceId))
            {
                var newDataSourceId = ids[dataSourceId];
                if (!String.IsNullOrEmpty(newDataSourceId))
                {
                    partInstance.SetValue(propertyName, newDataSourceId);
                }
            }
        }


        /// <summary>
        /// Adds the nested zones to the 
        /// </summary>
        /// <param name="source">Source template instance</param>
        /// <param name="sourceWebPartId">Source web part ID</param>
        /// <param name="target">Target template instance</param>
        /// <param name="targetWebPartId">Target web part ID</param>
        /// <param name="webParts"></param>
        /// <param name="ids">Table of translations from original web part ID to new web part ID</param>
        /// <param name="insertedWebParts"></param>
        private static void AddNestedZones(PageTemplateInstance source, string sourceWebPartId, PageTemplateInstance target, string targetWebPartId, List<WebPartInstance> webParts, StringSafeDictionary<string> ids, HashSet<Guid> insertedWebParts)
        {
            // Load related nested zones
            var zones = source.GetNestedZones(sourceWebPartId);
            if (zones != null)
            {
                foreach (var webPartZoneInstance in zones)
                {
                    var nestedZoneId = targetWebPartId + webPartZoneInstance.ZoneID.Substring(sourceWebPartId.Length);

                    // Only add zone in case it doesn't yet exist
                    if (!target.ZoneExists(nestedZoneId))
                    {
                        // Process the web part zone
                        var zone = webPartZoneInstance.Clone(false);
                        zone.ZoneID = nestedZoneId;
                        zone.ParentTemplateInstance = null;

                        zone = target.AddZone(zone);

                        // Process nested web parts
                        foreach (var partInstance in webPartZoneInstance.WebParts)
                        {
                            // Make sure that web part was not yet added (to avoid recursion)
                            if (!insertedWebParts.Contains(partInstance.InstanceGUID))
                            {
                                var webPart = partInstance.Clone(true);

                                webPart.InstanceGUID = Guid.NewGuid();

                                webPart = zone.AddWebPart(webPart);

                                // Recursively add nested web part zones for the inner web part
                                AddNestedZones(source, partInstance.ControlID, target, webPart.ControlID, webParts, ids, insertedWebParts);

                                // Register web part within collection
                                webParts.Add(webPart);
                                insertedWebParts.Add(partInstance.InstanceGUID);

                                ids[partInstance.ControlID] = webPart.ControlID;
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Adds the new widget to the page and returns its instance.
        /// </summary>
        /// <param name="widgetId">The widget id</param>
        /// <param name="zoneId">The zone id</param>
        /// <param name="zoneType">Type of the widget zone</param>
        /// <param name="isLayoutZone">Indicates whether the zone is a layout zone</param>
        /// <param name="templateInstance">The template instance</param>
        /// <param name="row">The data of the new widget</param>
        public static WebPartInstance AddNewWidget(int widgetId, string zoneId, WidgetZoneTypeEnum zoneType, bool isLayoutZone, PageTemplateInstance templateInstance, DataRow row = null)
        {
            WebPartInstance widgetInstance = null;

            // Add web part to the currently selected zone under currently selected page
            if ((widgetId > 0) && !String.IsNullOrEmpty(zoneId))
            {
                WidgetInfo wi = WidgetInfoProvider.GetWidgetInfo(widgetId);

                if (wi != null)
                {
                    // Ensure layout zone flag
                    if (isLayoutZone)
                    {
                        WebPartZoneInstance zone = templateInstance.EnsureZone(zoneId);
                        zone.LayoutZone = true;
                        zone.WidgetZoneType = zoneType;

                        // Ensure the layout zone flag in the original page template instance
                        WebPartZoneInstance zoneInstance = templateInstance.GetZone(zoneId);
                        if (zoneInstance != null)
                        {
                            zoneInstance.LayoutZone = true;
                            zone.WidgetZoneType = zoneType;
                        }
                    }

                    // Add the widget
                    WebPartInstance newWidget = templateInstance.AddWidget(zoneId, widgetId);
                    if (newWidget != null)
                    {
                        // Prepare the form info to get the default properties
                        WebPartInfo wpi = WebPartInfoProvider.GetWebPartInfo(wi.WidgetWebPartID);
                        string widgetProperties = FormHelper.MergeFormDefinitions(wpi.WebPartProperties, wi.WidgetProperties);
                        FormInfo fi = PortalFormHelper.GetWidgetFormInfo(wi.WidgetName, zoneType, widgetProperties, true);

                        // Data row with required columns
                        if (row == null)
                        {
                            // Apply changed values
                            row = fi.GetDataRow();

                            // Get data rows with required columns
                            if ((!String.IsNullOrEmpty(wi.WidgetDefaultValues)) && (CMSString.Compare(wi.WidgetDefaultValues, "<form></form>", true) != 0))
                            {
                                fi.LoadDefaultValues(row, wi.WidgetDefaultValues);
                            }
                        }

                        newWidget.LoadProperties(row);

                        // Add web part to user's last recently used
                        MembershipContext.AuthenticatedUser.UserSettings.UpdateRecentlyUsedWidget(wi.WidgetName);

                        widgetInstance = newWidget;
                    }
                }
            }

            return widgetInstance;
        }


        /// <summary>
        /// Combines widget info with default XML system properties.
        /// </summary>
        /// <param name="fi">Widget form info</param>
        /// <param name="wi">Widget info object</param>
        public static DataRow CombineWithDefaultValues(FormInfo fi, WidgetInfo wi)
        {
            if ((!String.IsNullOrEmpty(wi.WidgetDefaultValues)) && (CMSString.Compare(wi.WidgetDefaultValues, "<form></form>", true) != 0))
            {
                // Apply changed values
                DataRow dr = fi.GetDataRow();
                fi.LoadDefaultValues(dr, wi.WidgetDefaultValues);

                return dr;
            }

            return fi.GetDataRow();
        }


        /// <summary>
        /// Gets the web part / widget / page template icon HTML.
        /// </summary>
        /// <param name="thumbnailGuid">The icon thumbnail GUID</param>
        /// <param name="iconClass">The icon css class</param>
        /// <param name="thumbnailMaxSideSize">Max side size of the generated thumbnail</param>
        public static string GetIconHtml(Guid thumbnailGuid, string iconClass, int thumbnailMaxSideSize = 64)
        {
            string iconHtml;

            if (thumbnailGuid != Guid.Empty)
            {
                // Display metafile
                string imgQueryParams = HTMLHelper.EncodeForHtmlAttribute($"?maxsidesize={thumbnailMaxSideSize}&fileguid={thumbnailGuid}");
                iconHtml = $"<img alt=\"\" src=\"{UrlResolver.ResolveUrl("~/CMSPages/GetMetaFile.aspx")}{imgQueryParams}\" />";
            }
            else
            {
                // Display font icon
                iconHtml = $"<i class=\"{HTMLHelper.EncodeForHtmlAttribute(iconClass)}\" aria-hidden=\"true\"></i>";
            }

            return iconHtml;
        }


        /// <summary>
        /// Returns path to the default editor css file for the specified site.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static string GetHtmlEditorAreaCss(string siteName)
        {
            string cssFilePath = String.Empty;

            SiteInfo si = SiteInfoProvider.GetSiteInfo(siteName);
            if (si == null)
            {
                return cssFilePath;
            }

            var stylesheetId = si.SiteDefaultEditorStylesheet > 0 ? si.SiteDefaultEditorStylesheet : si.SiteDefaultStylesheetID;
            var cssInfo = CssStylesheetInfoProvider.GetCssStylesheetInfo(stylesheetId);
            if (cssInfo != null)
            {
                cssFilePath = CssLinkHelper.GetStylesheetUrl(cssInfo.StylesheetName);
            }

            return cssFilePath;
        }


        /// <summary> 
        /// Links component style sheets to the page. Styles of the following objects are included: web parts, page templates, page layouts, web part layouts, web part containers, transformations and device layouts.
        /// </summary>
        /// <param name="page">Represents page object which style sheets are included to.</param>
        public static void AddComponentsCSS(Page page)
        {
            if (!SettingsKeyInfoProvider.GetBoolValue("CMSAllowComponentsCSS"))
            {
                return;
            }

            var settings = new CMSCssSettings();
            var components = PortalContext.CurrentComponents;

            settings.Containers.AddRange(GetContainerSettings(components.Containers));
            settings.Transformations.AddRange(GetTransformationSettings(components.Transformations));
            settings.Layouts.AddRange(GetLayoutSettings(components.Layouts));
            settings.Templates.AddRange(GetTemplateSettings(components.Templates));
            settings.WebPartLayouts.AddRange(GetWebPartLayoutSettings(components.WebPartLayouts));
            settings.WebParts.AddRange(GetWebPartSettings(components.WebParts));
            settings.DeviceLayouts.AddRange(GetDeviceLayoutSettings(components.DeviceLayouts));

            // Register the CSSes within request
            settings.RegisterCSS(page);
        }


        private static IEnumerable<int> GetDeviceLayoutSettings(Dictionary<string, PageTemplateDeviceLayoutInfo> layouts)
        {
            if (layouts == null)
            {
                return Enumerable.Empty<int>();
            }

            return layouts.Values
                           .Where(layout => (layout != null) && !String.IsNullOrWhiteSpace(layout.LayoutCSS))
                           .Select(layout => layout.TemplateDeviceLayoutID);
        }


        private static IEnumerable<int> GetWebPartSettings(Dictionary<string, WebPartInfo> webParts)
        {
            if (webParts == null)
            {
                return Enumerable.Empty<int>();
            }

            return webParts.Values
                           .Where(webPart => (webPart != null) && !String.IsNullOrWhiteSpace(webPart.WebPartCSS))
                           .Select(webPart => webPart.WebPartID);
        }


        private static IEnumerable<int> GetWebPartLayoutSettings(Dictionary<string, WebPartLayoutInfo> webpartLayouts)
        {
            if (webpartLayouts == null)
            {
                return Enumerable.Empty<int>();
            }

            return webpartLayouts.Values
                                 .Where(layout => (layout != null) && !String.IsNullOrWhiteSpace(layout.WebPartLayoutCSS))
                                 .Select(layout => layout.WebPartLayoutID);
        }


        private static IEnumerable<int> GetTemplateSettings(Dictionary<string, PageTemplateInfo> templates)
        {
            if (templates == null)
            {
                return Enumerable.Empty<int>();
            }

            return templates.Values
                            .Where(template => (template != null) && !String.IsNullOrWhiteSpace(template.PageTemplateCSS))
                            .Select(template => template.PageTemplateId);
        }


        private static IEnumerable<int> GetLayoutSettings(Dictionary<string, LayoutInfo> layouts)
        {
            if (layouts == null)
            {
                return Enumerable.Empty<int>();
            }

            return layouts.Values
                          .Where(layout => (layout != null) && !String.IsNullOrWhiteSpace(layout.LayoutCSS))
                          .Select(layout => layout.LayoutId);
        }


        private static IEnumerable<int> GetContainerSettings(Dictionary<string, WebPartContainerInfo> containers)
        {
            if (containers == null)
            {
                return Enumerable.Empty<int>();
            }

            return containers.Values
                             .Where(container => (container != null) && !String.IsNullOrWhiteSpace(container.ContainerCSS))
                             .Select(container => container.ContainerID);
        }


        private static IEnumerable<int> GetTransformationSettings(Dictionary<string, TransformationInfo> transformations)
        {
            if (transformations == null)
            {
                return Enumerable.Empty<int>();
            }

            return transformations.Values
                             .Where(transformation => (transformation != null) && !String.IsNullOrWhiteSpace(transformation.TransformationCSS))
                             .Select(transformation => transformation.TransformationID);
        }

        #endregion


        #region "Controls collecting"

        /// <summary>
        /// Returns the list of all the WebParts within the given control (searches only for the level 0 placeholders, does not search recursively in the placeholders).
        /// </summary>
        /// <param name="parent">Parent control</param>
        public static List<CMSAbstractWebPart> CollectWebParts(Control parent)
        {
            if (parent == null)
            {
                return null;
            }
            else
            {
                List<CMSAbstractWebPart> result = null;

                // Process the child controls
                foreach (Control child in parent.Controls)
                {
                    // If WebPart, add to result
                    if (child is CMSAbstractWebPart)
                    {
                        if (result == null)
                        {
                            result = new List<CMSAbstractWebPart>();
                        }

                        result.Add((CMSAbstractWebPart)child);
                    }
                    else if (child is CMSPagePlaceholder)
                    {
                        // If placeholder, do nothing, do not search within the inner placeholders
                    }
                    else
                    {
                        // Else try to find within the child controls
                        List<CMSAbstractWebPart> childWebParts = CollectWebParts(child);
                        if (childWebParts != null)
                        {
                            if (result == null)
                            {
                                result = new List<CMSAbstractWebPart>();
                            }

                            result.AddRange(childWebParts);
                        }
                    }
                }
                return result;
            }
        }


        /// <summary>
        /// Returns the list of all controls matching the given control type
        /// </summary>
        /// <param name="parent">Parent control</param>
        /// <param name="collectChild">If true, also child controls under controls of the given type are collected</param>
        public static List<ControlType> CollectControlsOfType<ControlType>(Control parent, bool collectChild = false)
            where ControlType : Control
        {
            // Search for specific controls of the specified type
            // 'Repeater' = do not search in controls supporting data binding
            return CollectControlsOfType<ControlType, Repeater>(parent, collectChild);
        }


        /// <summary>
        /// Returns the list of all controls matching the given control type
        /// </summary>
        /// <param name="parent">Parent control</param>
        /// <param name="collectChild">If true, also child controls under controls of the given type are collected</param>
        internal static List<ControlType> CollectControlsOfType<ControlType, SkipControlType>(Control parent, bool collectChild = false)
            where ControlType : Control
            where SkipControlType : Control
        {
            if (parent == null)
            {
                return null;
            }
            else
            {
                List<ControlType> result = null;

                // Repeater type indicates that all the data binding controls should be skipped (Repeater here represents just a default value)
                bool skipDataBindingControls = (typeof(SkipControlType) == typeof(Repeater));

                // Process the child controls
                foreach (Control child in parent.Controls)
                {
                    if (skipDataBindingControls)
                    {
                        // Do not search in the controls which support data binding.
                        // We don't want to access their 'Controls' property yet, otherwise it would initialize loading of the control.
                        if ((child is Repeater)
                            || (child is BaseDataList)
                            || (child is BaseDataBoundControl))
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if (child is SkipControlType)
                        {
                            // Do not search in the specific control type
                            continue;
                        }
                    }

                    // If placeholder, add to the result and do not search within the placeholder
                    if (child is ControlType)
                    {
                        if (result == null)
                        {
                            result = new List<ControlType>();
                        }

                        result.Add((ControlType)child);
                        if (collectChild)
                        {
                            List<ControlType> childControls = CollectControlsOfType<ControlType, SkipControlType>(child, true);
                            if (childControls != null)
                            {
                                result.AddRange(childControls);
                            }
                        }
                    }
                    else
                    {
                        // Else try to find within the child controls
                        List<ControlType> childControls = CollectControlsOfType<ControlType, SkipControlType>(child, collectChild);
                        if (childControls != null)
                        {
                            if (result == null)
                            {
                                result = new List<ControlType>();
                            }

                            result.AddRange(childControls);
                        }
                    }
                }

                return result;
            }
        }


        /// <summary>
        /// Returns the hashtable of all the CMSPagePlaceholders within the given control (searches only for the level 0 placeholders, does not search recursively in the placeholders).
        /// </summary>
        /// <param name="parent">Parent control</param>
        /// <param name="collectChild">If true, also child placeholders are collected</param>
        public static List<CMSPagePlaceholder> CollectPlaceholders(Control parent, bool collectChild = false)
        {
            return CollectControlsOfType<CMSPagePlaceholder>(parent, collectChild);
        }


        /// <summary>
        /// Returns the list of all the WebPartZones within the given control (searches only for the level 0 placeholders, does not search recursively in the placeholders).
        /// </summary>
        /// <param name="parent">Parent control</param>
        public static List<CMSWebPartZone> CollectWebPartZones(Control parent)
        {
            if (parent == null)
            {
                return null;
            }
            else
            {
                List<CMSWebPartZone> result = null;

                // Process the child controls
                foreach (Control child in parent.Controls)
                {
                    // If WebPartZone, add to result
                    if (child is CMSWebPartZone)
                    {
                        if (result == null)
                        {
                            result = new List<CMSWebPartZone>();
                        }

                        result.Add((CMSWebPartZone)child);
                    }
                    else if ((child is CMSPagePlaceholder) || (child is CMSAbstractWebPart))
                    {
                        // If placeholder or webpart, do nothing, do not search within the inner placeholders
                    }
                    else if (child is CMSUpdatePanel)
                    {
                        CMSUpdatePanel panel = (CMSUpdatePanel)child;

                        // Update panel controls
                        List<CMSWebPartZone> childWebPartZones = CollectWebPartZones(panel.ContentTemplateContainer);
                        if (childWebPartZones != null)
                        {
                            if (result == null)
                            {
                                result = new List<CMSWebPartZone>();
                            }

                            result.AddRange(childWebPartZones);
                        }
                    }
                    else if (child is CMSConditionalLayout)
                    {
                        CMSConditionalLayout layout = (CMSConditionalLayout)child;

                        // Conditional layout, collect zones only if condition matched
                        if (layout.Visible)
                        {
                            if (result == null)
                            {
                                result = new List<CMSWebPartZone>();
                            }

                            result.AddRange(layout.WebPartZones);
                        }
                    }
                    else
                    {
                        // Else try to find within the child controls
                        List<CMSWebPartZone> childWebPartZones = CollectWebPartZones(child);
                        if (childWebPartZones != null)
                        {
                            if (result == null)
                            {
                                result = new List<CMSWebPartZone>();
                            }

                            result.AddRange(childWebPartZones);
                        }
                    }
                }

                return result;
            }
        }


        /// <summary>
        /// Returns the list of all the Editable controls within the given control.
        /// </summary>
        /// <param name="parent">Parent control</param>
        /// <param name="recursive">If true, the editable controls are searched inside of the editable controls</param>
        /// <param name="excludePlaceholder">If true, placeholder control is excluded from lookup</param>
        public static List<ICMSEditableControl> CollectEditableControls(Control parent, bool recursive, bool excludePlaceholder)
        {
            if (parent == null)
            {
                return null;
            }
            else
            {
                List<ICMSEditableControl> result = null;

                // Process the child controls
                foreach (Control child in parent.Controls)
                {
                    if ((child is CMSPagePlaceholder) && excludePlaceholder && !((CMSPagePlaceholder)child).UsingDefaultPage)
                    {
                        // Excluded placeholder, do not continue further
                    }
                    else if (child is ICMSEditableControl)
                    {
                        if (result == null)
                        {
                            result = new List<ICMSEditableControl>();
                        }

                        // If WebPart, add to result
                        result.Add((ICMSEditableControl)child);

                        // Recursive search
                        if (recursive)
                        {
                            // Try to find within the child controls                            
                            List<ICMSEditableControl> childRegions = CollectEditableControls(child, true, excludePlaceholder);
                            if (childRegions != null)
                            {
                                result.AddRange(childRegions);
                            }
                        }
                    }
                    else
                    {
                        // Else try to find within the child controls
                        List<ICMSEditableControl> childRegions = CollectEditableControls(child, recursive, excludePlaceholder);
                        if (childRegions != null)
                        {
                            if (result == null)
                            {
                                result = new List<ICMSEditableControl>();
                            }

                            result.AddRange(childRegions);
                        }
                    }
                }

                return result;
            }
        }


        /// <summary>
        /// Ensures the Script manager on the page.
        /// </summary>
        /// <param name="page">Page</param>
        [Obsolete("Use ControlsHelper.EnsureScriptManager(Page) instead.")]
        public static ScriptManager EnsureScriptManager(Page page)
        {
            return ControlsHelper.EnsureScriptManager(page);
        }

        #endregion


        #region "Portal to ASPX conversion"

        /// <summary>
        /// Combines the register code blocks and avoids duplicated declarations.
        /// </summary>
        /// <param name="originalCode">Original code</param>
        /// <param name="newCode">Code to add</param>
        public static string CombineRegisterCode(string originalCode, string newCode)
        {
            if (newCode == null)
            {
                return originalCode;
            }

            string[] codeLines = newCode.Split('\r', '\n');
            foreach (string codeLine in codeLines)
            {
                var trimmedCodeLine = codeLine.Trim();
                if (!string.IsNullOrEmpty(trimmedCodeLine) && (originalCode.IndexOf(trimmedCodeLine, StringComparison.InvariantCultureIgnoreCase) < 0))
                {
                    originalCode += trimmedCodeLine + "\r\n";
                }
            }

            return originalCode;
        }


        /// <summary>
        /// Gets the page template ASPX code.
        /// </summary>
        /// <param name="template">Page template</param>
        /// <param name="registerCode">Returning additional code that must be registered so the page template content could run properly</param>
        public static string GetPageTemplateASPXCode(PageTemplateInfo template, out string registerCode)
        {
            registerCode = "";
            if ((template == null) || !template.IsPortal)
            {
                return null;
            }

            string code;
            LayoutTypeEnum layoutType;

            // Get the base layout
            LayoutInfo li = LayoutInfoProvider.GetLayoutInfo(template.LayoutID);
            if (li != null)
            {
                code = li.LayoutCode;
                layoutType = li.LayoutType;
            }
            else
            {
                code = template.PageTemplateLayout;
                layoutType = template.PageTemplateLayoutType;
            }

            // Convert zones to the proper ASPX code
            foreach (WebPartZoneInstance zone in template.WebPartZones)
            {
                string zoneRegisterCode;
                string zoneCode = GetWebPartZoneASPXCode(zone, out zoneRegisterCode);

                registerCode = CombineRegisterCode(registerCode, zoneRegisterCode);

                Regex zoneRegEx;
                if (layoutType == LayoutTypeEnum.Ascx)
                {
                    zoneRegEx = RegexHelper.GetRegex("<((cc1)|(cms)):CMSWebPartZone [^>]*ID=\"" + zone.ZoneID + "\"[^>]*/>", true);
                }
                else
                {
                    // HTML layout type
                    zoneRegEx = RegexHelper.GetRegex("\\{\\^WebPartZone\\|\\(id\\)" + zone.ZoneID + "\\^\\}", true);
                }
                code = zoneRegEx.Replace(code, zoneCode);
            }

            // Remove all extra zone tags
            if (!String.IsNullOrEmpty(code))
            {
                if (layoutType == LayoutTypeEnum.Ascx)
                {
                    code = Regex.Replace(code, "</?((cc1)|(cms)):CMSWebPartZone[^>]*>", "");
                }
                else
                {
                    // HTML layout type
                    code = Regex.Replace(code, "\\{\\^WebPartZone\\|\\(id\\)[^\\^]*\\^\\}", "");
                }
            }

            return code;
        }


        /// <summary>
        /// Gets the web part zone ASPX code.
        /// </summary>
        /// <param name="zone">Web part zone instance</param>
        /// <param name="registerCode">Returning additional code that must be registered so the web part zone content could run properly</param>
        public static string GetWebPartZoneASPXCode(WebPartZoneInstance zone, out string registerCode)
        {
            registerCode = "";
            if (zone == null)
            {
                return null;
            }

            string code = "";

            // Add the code for all the web parts
            foreach (WebPartInstance webPart in zone.WebParts)
            {
                string partRegisterCode;
                string partCode = GetWebPartASPXCode(webPart, out partRegisterCode);

                registerCode = CombineRegisterCode(registerCode, partRegisterCode);

                code += partCode;
            }

            return code;
        }


        /// <summary>
        /// Gets the ASPX code for the web part.
        /// </summary>
        /// <param name="webPart">Web part instance</param>
        /// <param name="registerCode">Returning additional code that must be registered so the web part could run properly</param>
        public static string GetWebPartASPXCode(WebPartInstance webPart, out string registerCode)
        {
            registerCode = "";
            if (webPart == null)
            {
                return null;
            }

            if (String.IsNullOrEmpty(webPart.WebPartType))
            {
                return null;
            }

            string code;

            switch (webPart.WebPartType.ToLowerInvariant())
            {
                // Static Text / HTML
                case "statictext":
                case "statichtml":
                    code = ValidationHelper.GetString(webPart.GetValue("Text"), "");
                    break;

                // Page placeholder - just mark the position of inner content
                case "pageplaceholder":
                    code = "<%--CONTENT--%>";
                    break;

                // Standard web parts
                default:
                    // Get web part information
                    WebPartInfo wi = WebPartInfoProvider.GetWebPartInfo(webPart.WebPartType);
                    if (wi == null)
                    {
                        return null;
                    }

                    // Prepare base code
                    switch (webPart.WebPartType.ToLowerInvariant())
                    {
                        case "editabletext":
                            // Convert editable text to DocumentEngine.Web.UI.EditableRegion
                            registerCode += "<%@ Register Assembly=\"CMS.DocumentEngine.Web.UI\" Namespace=\"CMS.DocumentEngine.Web.UI\" TagPrefix=\"cc1\" %>\r\n";
                            code = "<cc1:CMSEditableRegion runat=\"server\" ID=\"" + webPart.ControlID + "\"";
                            break;

                        case "editableimage":
                            // Convert editable image to DocumentEngine.Web.UI.EditableImage
                            registerCode += "<%@ Register Assembly=\"CMS.DocumentEngine.Web.UI\" Namespace=\"CMS.DocumentEngine.Web.UI\" TagPrefix=\"cc1\" %>\r\n";
                            code = "<cc1:CMSEditableImage runat=\"server\" ID=\"" + webPart.ControlID + "\"";
                            break;

                        default:
                            // Standard web parts
                            string tagName = webPart.WebPartType.Replace(".", "");
                            string path = WebPartInfoProvider.GetWebPartUrl(wi, false);

                            registerCode += "<%@ Register Src=\"" + path + "\" TagName=\"" + tagName + "\" TagPrefix=\"uc1\" %>\r\n";
                            code = "<uc1:" + tagName + " runat=\"server\" ID=\"" + webPart.ControlID + "\"";
                            break;
                    }

                    // Prepare the default data
                    FormInfo pfi = new FormInfo(wi.WebPartProperties);
                    DataRow defaultData = pfi.GetDataRow();
                    pfi.LoadDefaultValues(defaultData);

                    // Add properties
                    foreach (DataColumn dc in defaultData.Table.Columns)
                    {
                        string value = ValidationHelper.GetString(webPart.GetValue(dc.ColumnName), string.Empty);
                        string defaultValue = ValidationHelper.GetString(defaultData[dc.ColumnName], string.Empty);
                        bool useProperty;

                        // Check the default value
                        switch (dc.DataType.Name.ToLowerInvariant())
                        {
                            case "boolean":
                                useProperty = (value != string.Empty) && !value.Equals(defaultValue, StringComparison.OrdinalIgnoreCase);
                                break;

                            default:
                                useProperty = value != defaultValue;
                                break;
                        }

                        // Special column treatment
                        switch (dc.ColumnName.ToLowerInvariant())
                        {
                            case "webpartcontrolid":
                            case "contentbefore":
                            case "contentafter":
                            case "containertitle":
                            case "container":
                                useProperty = false;
                                break;

                            case "visible":
                                useProperty = !value.Equals("true", StringComparison.OrdinalIgnoreCase);
                                break;

                            case "hideonsubpages":
                                useProperty = !value.Equals("false", StringComparison.OrdinalIgnoreCase);
                                break;
                        }

                        // Add the property string
                        if (useProperty)
                        {
                            code += " " + dc.ColumnName + "=\"" + HttpUtility.HtmlEncode(value) + "\"";
                        }
                    }

                    code += " />\r\n";

                    break;
            }

            // Add content before / after
            code = ValidationHelper.GetString(webPart.GetValue("ContentBefore"), "") + code + ValidationHelper.GetString(webPart.GetValue("ContentAfter"), "");

            // Add container
            string containerName = ValidationHelper.GetString(webPart.GetValue("Container"), "");
            if (containerName != "")
            {
                registerCode += "<%@ Register Assembly=\"CMS.PortalEngine.Web.UI\" Namespace=\"CMS.PortalEngine.Web.UI\" TagPrefix=\"cc2\" %>\r\n";

                // Init title property
                string containerTitle = ValidationHelper.GetString(webPart.GetValue("ContainerTitle"), "");
                if (containerTitle != "")
                {
                    containerTitle = " ContainerTitle=\"" + containerTitle + "\"";
                }

                code = "<cc2:WebPartContainer ID=\"wpc" + webPart.ControlID + "\" runat=\"server\" ContainerName=\"" + containerName + "\"" + containerTitle + ">\r\n" + code + "\r\n</cc2:WebPartContainer>\r\n";
            }

            return code;
        }

        #endregion


        /// <summary>
        /// Registers the editable control.
        /// </summary>
        /// <param name="control">Control to register</param>
        public static void RegisterEditableControl(ICMSEditableControl control)
        {
            PortalContext.CurrentEditableControls.Add(control);
        }


        /// <summary>
        /// Returns the list of all the Editable controls within the given control.
        /// </summary>
        /// <param name="parent">Parent control</param>
        public static List<ICMSEditableControl> CollectEditableControls(Control parent)
        {
            return CollectEditableControls(parent, true);
        }


        /// <summary>
        /// Returns the list of all the Editable controls within the given control.
        /// </summary>
        /// <param name="parent">Parent control</param>
        /// <param name="recursive">If true, the editable controls are searched inside of the editable controls</param>
        public static List<ICMSEditableControl> CollectEditableControls(Control parent, bool recursive)
        {
            var result = new List<ICMSEditableControl>();
            if (parent == null)
            {
                return result;
            }
            else
            {
                // Process the child controls
                foreach (Control child in parent.Controls)
                {
                    // If WebPart, add to result
                    if (child is ICMSEditableControl)
                    {
                        result.Add((ICMSEditableControl)child);

                        // Recursive search
                        if (recursive)
                        {
                            // Try to find within the child controls                            
                            var childRegions = CollectEditableControls(child, true);
                            if (childRegions != null)
                            {
                                result.AddRange(childRegions);
                            }
                        }
                    }
                    else
                    {
                        // Else try to find within the child controls
                        var childRegions = CollectEditableControls(child, recursive);
                        if (childRegions != null)
                        {
                            result.AddRange(childRegions);
                        }
                    }
                }
                return result;
            }
        }


        /// <summary>
        /// Localizes a given <paramref name="resourceString"/> according to current <see cref="PortalContext.ViewMode"/>.
        /// Use this method to ensure correct localization of UI components in various view modes. The target culture is determined using <see cref="GetUILocalizationCulture"/> method.
        /// </summary>
        /// <param name="resourceString">Resource string</param>
        public static string LocalizeStringForUI(string resourceString)
        {
            return ResHelper.GetString(resourceString, GetUILocalizationCulture());
        }


        /// <summary>
        /// Returns a code of culture which should be used for localization while taking into account current <see cref="PortalContext.ViewMode"/>. 
        /// Only in case of <see cref="ViewModeEnum.LiveSite"/>, <see cref="ViewModeEnum.UserWidgets"/> or <see cref="ViewModeEnum.UserWidgetsDisabled"/> the culture of content is returned, 
        /// otherwise context of user interface is assumed and current user's preferred UI culture is returned.
        /// </summary>
        public static string GetUILocalizationCulture()
        {
            if (PortalContext.ViewMode.IsOneOf(ViewModeEnum.LiveSite, ViewModeEnum.UserWidgets, ViewModeEnum.UserWidgetsDisabled))
            {
                return CultureHelper.GetPreferredCulture();
            }

            return MembershipContext.AuthenticatedUser.PreferredUICultureCode;
        }
    }
}
