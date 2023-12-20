using System;
using System.Collections.Generic;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;
using CMS.Membership;
using CMS.Modules;
using CMS.PortalEngine;
using CMS.PortalEngine.Web.UI;
using CMS.SiteProvider;
using CMS.Synchronization;

namespace CMS.UIControls
{
    /// <summary>
    /// Abstract class for web parts used in UI
    /// </summary>
    public abstract class CMSAbstractUIWebpart : CMSAbstractWebPart, IUIElementControl
    {
        #region "Variables"

        private bool? mDisplayFooter = null;

        #endregion


        #region "Constants"

        private const int TITLE_HEIGHT = 48;
        private const int BREADCRUMBS_HEIGHT = 44;

        #endregion


        #region "Properties"

        /// <summary>
        /// Element's name
        /// </summary>
        public String ElementName
        {
            get
            {
                String val = GetStringContextValue("ElementName");
                if ((val == String.Empty) && (UIContext.UIElement != null))
                {
                    val = UIContext.UIElement.ElementName;
                }

                return val;
            }
            set
            {
                SetValue("ElementName", value);
            }
        }


        /// <summary>
        /// UI element's resource name
        /// </summary>
        public String ResourceName
        {
            get
            {
                String val = GetStringContextValue("ResourceName");
                if ((val == String.Empty) && (UIContext.UIElement != null))
                {
                    ResourceInfo ri = ResourceInfoProvider.GetResourceInfo(UIContext.UIElement.ElementResourceID);
                    if (ri != null)
                    {
                        val = ri.ResourceName;
                    }
                }

                return val;
            }
            set
            {
                SetValue("ResourceName", value);
            }
        }


        /// <summary>
        /// Web part's object type. If not set, use default context's object type.
        /// </summary>
        public String ObjectType
        {
            get
            {
                String ot = ValidationHelper.GetString(GetValue("ObjectType"), String.Empty);
                if (ot == String.Empty)
                {
                    ot = UIContextHelper.GetObjectType(UIContext);
                }

                return ot;
            }
            set
            {
                SetValue("ObjectType", value);
            }
        }


        /// <summary>
        /// Text of title
        /// </summary>
        public String TitleText
        {
            get
            {
                String val = ValidationHelper.GetString(GetValue("TitleText"), String.Empty);
                if (val == String.Empty)
                {
                    val = UIContextHelper.GetTitleText(UIContext);
                }

                return val;
            }
            set
            {
                SetValue("TitleText", value);
            }
        }


        /// <summary>
        /// Indicates whether the control should display breadcrumbs
        /// </summary>
        public virtual bool DisplayBreadCrumbs
        {
            get
            {
                return UIContext.DisplayBreadcrumbs;
            }
        }


        /// <summary>
        /// Gets object type of (created)edited object (if any)
        /// </summary>
        public int ObjectID
        {
            get
            {
                return UIContext.ObjectID;
            }
        }


        /// <summary>
        /// Indicates whether this instance is used in a dialog.
        /// </summary>        
        public bool IsDialog
        {
            get
            {
                return UIContext.IsDialog;
            }
        }


        /// <summary>
        /// Gets the root element ID.
        /// Root UI element holds the very top UIElement which is open in a dialog.
        /// </summary>
        public int RootElementID
        {
            get
            {
                int rootElementId = UIContext.RootElementID;
                if ((rootElementId == 0)
                    && IsDialog
                    && (UIContext.UIElement != null))
                {
                    rootElementId = UIContext.UIElement.ElementID;
                }

                return rootElementId;
            }
        }


        /// <summary>
        /// Gets or sets a value indicating whether the footer pane should be displayed.
        /// </summary>
        protected bool DisplayFooter
        {
            get
            {
                if (!mDisplayFooter.HasValue)
                {
                    // Display the footer pane automatically only for the top dialog iframe
                    mDisplayFooter = (UIContext.IsRootDialog);
                }

                return mDisplayFooter.Value;
            }
            set
            {
                mDisplayFooter = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns if element is authorized per modifying object
        /// </summary>
        /// <param name="editedObject">Edited object. If null context's object is used.</param>
        protected bool CheckEditPermissions(BaseInfo editedObject = null)
        {
            BaseInfo bi = editedObject ?? UIContext.EditedObject as BaseInfo;
            return (bi == null) || bi.CheckPermissions(PermissionsEnum.Modify, SiteContext.CurrentSiteName, CurrentUser);
        }


        /// <summary>
        /// Returns if user is allowed to read content based on content's object type
        /// </summary>
        /// <param name="editedObject">Edited object. If null context's object is used.</param>
        protected bool CheckViewPermissions(BaseInfo editedObject = null)
        {
            String elementType = UIContextHelper.GetObjectType(UIContext);
            BaseInfo bi = editedObject ?? ModuleManager.GetObject(elementType);

            if (bi != null)
            {
                bool result = bi.CheckPermissions(PermissionsEnum.Read, SiteContext.CurrentSiteName, CurrentUser);

                if (!result)
                {
                    var infoWebpart = new WebPartAccessDenied();
                    infoWebpart.ObjectType = bi.TypeInfo.ObjectType;
                    infoWebpart.PartInstance = PartInstance;
                    infoWebpart.PermissionName = "view";

                    Controls.Add(infoWebpart);
                }
                return result;
            }

            return true;
        }


        /// <summary>
        /// Check permissions for UI elements
        /// </summary>
        protected void CheckUIPermissions()
        {
            if (!CurrentUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin))
            {
                if (ViewMode == ViewModeEnum.UI)
                {
                    UIElementInfo ui = UIContext.UIElement;

                    // Dont check permission for cms (top root)
                    while ((ui != null) && (ui.ElementName.ToLowerCSafe() != "cms"))
                    {
                        if (!MembershipContext.AuthenticatedUser.IsAuthorizedPerUIElement(ui.ElementResourceID, ui.ElementName))
                        {
                            CMSPage.RedirectToUIElementAccessDenied(ResourceName, ui.ElementName);
                        }

                        ui = UIElementInfoProvider.GetUIElementInfo(ui.ElementParentID);
                    }
                }
            }
        }


        /// <summary>
        /// Initializes the page controls
        /// </summary>
        /// <param name="headerPane">Header pane</param>
        protected void SetHeaderControls(UILayoutPane headerPane)
        {
            var pt = ControlsHelper.GetChildControl<PageTitle>(headerPane);
            if (pt != null)
            {
                // Set page title to the page
                var page = Page as CMSPage;
                if ((page != null) && (page.CurrentMaster != null))
                {
                    page.PageTitle = pt;
                }
            }
        }


        /// <summary>
        /// Handles displaying page title and breadcrumbs 
        /// </summary>
        /// <param name="paneTitle">UI layout pane with title</param>
        /// <param name="hidePane">Indicates whether hide pane if title not to be shown</param>
        protected void ManagePaneTitle(UILayoutPane paneTitle, bool hidePane)
        {
            bool display = false;

            var ui = UIElementInfoProvider.GetUIElementInfo(ResourceName, ElementName);
            var pt = ControlsHelper.GetChildControl<PageTitle>(paneTitle);

            if ((ui != null) && (pt != null))
            {
                bool displayTitle = UIContext.DisplayTitle;

                // Always set page title for main breadcrumb
                SetTitle(pt, TitleText);

                // If not special title pane, increase pane size for title
                if ((!hidePane) && displayTitle && (paneTitle.Size != "auto"))
                {
                    int size = ValidationHelper.GetInteger(paneTitle.Size, 0);

                    // Lower dialog size
                    paneTitle.Size = (size + TITLE_HEIGHT).ToString();
                }

                // Hide title panel if display title not set
                if (!displayTitle)
                {
                    pt.HideTitle = true;
                }

                display = displayTitle;

                // Breadcrumbs - display breadcrumbs when required. Disable breadcrumbs only in a dialog for the root UI element to avoid browsing up in the UI hierarchy.
                if (DisplayBreadCrumbs)
                {
                    display = IsDialog;
                    SetBreadcrumbs(pt);

                    if (paneTitle.Size != "auto")
                    {
                        int size = ValidationHelper.GetInteger(paneTitle.Size, 0);
                        paneTitle.Size = (size + BREADCRUMBS_HEIGHT).ToString();
                    }
                }

                // Page title control behaves differently in dialog mode
                pt.IsDialog = IsDialog;
            }

            if (hidePane && !display)
            {
                paneTitle.RenderPane = false;
            }

            // Always visible to display main breadcrumb
            if (pt != null)
            {
                pt.Visible = true;
            }
        }


        /// <summary>
        /// Displays all information to messages placeholder based by element's properties
        /// </summary>
        protected void ManageTexts()
        {
            // Information
            String infoText = ValidationHelper.GetString(UIContext["informationtext"], String.Empty);
            if ((infoText != String.Empty) && !RequestHelper.IsPostBack())
            {
                ShowInformation(ContextResolver.ResolveMacros(infoText));

                // Information text already shown, delete it for prevent multiple assign.
                UIContext["informationtext"] = String.Empty;
            }
        }


        /// <summary>
        /// Returns string property value. If value is empty, try to get it from UI context.
        /// </summary>
        /// <param name="name">Property name</param>
        /// <param name="defaultValue">Default value for no items found</param>
        /// <param name="useSecure">If true, secure mode is used for getting property (value is not get from query string when not found in collection)</param>
        /// <param name="avoidInjection">If true, the resolving of the macros should avoid SQL injection (escapes the apostrophes in output).</param> 
        protected String GetStringContextValue(String name, String defaultValue = "", bool useSecure = false, bool avoidInjection = false)
        {
            String value = ValidationHelper.GetString(GetValue(name), String.Empty);
            if (String.IsNullOrEmpty(value))
            {
                value = ValidationHelper.GetString(UIContext.Data.GetKeyValue(name, !useSecure, avoidInjection), defaultValue);
            }

            return value;
        }


        /// <summary>
        /// Returns int property value. If value is empty, try to get it from UI context.
        /// </summary>
        /// <param name="name">Property name</param>
        /// <param name="defaultValue">Default value for empty properties</param>
        /// <param name="useSecure">If true, secure mode is used for getting property (value is not get from query string when not found in collection)</param>
        protected int GetIntContextValue(String name, int defaultValue, bool useSecure = false)
        {
            String value = ValidationHelper.GetString(GetValue(name), String.Empty);
            return (value == String.Empty) ? ValidationHelper.GetInteger((useSecure ? UIContext.Secure[name] : UIContext[name]), defaultValue) :
                ValidationHelper.GetInteger(value, defaultValue);
        }


        /// <summary>
        /// Returns bool property value. If property is not set, returns value from the UI context with the same name.
        /// </summary>
        /// <param name="name">Property name or UIContext key</param>
        /// <param name="defaultValue">Default value for empty properties</param>
        /// <param name="useSecure">If true, secure mode is used for getting property (value is not get from query string when not found in collection)</param>
        protected bool GetBoolContextValue(string name, bool defaultValue, bool useSecure = false)
        {
            string webPartPropertyValue = ValidationHelper.GetString(GetValue(name), String.Empty);

            if (webPartPropertyValue == String.Empty)
            {
                return ValidationHelper.GetBoolean((useSecure ? UIContext.Secure[name] : UIContext[name]), defaultValue);
            }

            return ValidationHelper.GetBoolean(webPartPropertyValue, defaultValue);
        }


        /// <summary>
        /// Sets breadcrumbs based on UI context data
        /// </summary>
        /// <param name="titleControl">Page title object</param>
        public void SetBreadcrumbs(PageTitle titleControl)
        {
            if (PortalContext.ViewMode.IsDesign(true))
            {
                return;
            }

            UIElementInfo currentElement = UIContext.UIElement;
            UIElementInfo parentElement = UIElementInfoProvider.GetUIElementInfo(currentElement.ElementParentID);
            BaseInfo bi = UIContext.EditedObject as BaseInfo;
            
            var name = String.Empty;

            // If parent element properties contains title text, use it
            if (parentElement.ElementProperties.Contains("<titletext>"))
            {
                XmlData dt = new XmlData();
                dt.LoadData(parentElement.ElementProperties);
                name = ValidationHelper.GetString(dt["titletext"], String.Empty);
            }

            if (String.IsNullOrEmpty(name))
            {
                name = UIElementInfoProvider.GetElementCaption(parentElement, false);
            }

            BreadcrumbItem bcItem = new BreadcrumbItem();
            bcItem.Text = name;

            if (DisplayBreadcrumbs())
            {
                String url = UIContextHelper.GetElementUrl(parentElement, UIContext);

                // Trim object ID
                url = URLHelper.RemoveParameterFromUrl(url, "objectID");

                // Trim object parameter ID  (tabs)
                String parameterId = UIContext["ObjectParameterID"].ToString(String.Empty);
                if (!String.IsNullOrEmpty(parameterId))
                {
                    url = URLHelper.RemoveParameterFromUrl(url, parameterId);
                }

                // Do not use 'Saved' indicator in the breadcrumbs
                url = URLHelper.RemoveParameterFromUrl(url, "saved");

                // Add target for first item
                bool useParentTarget = UIContextHelper.ElementIsLayout(parentElement, false);
                String target = useParentTarget ? "_parent" : "";

                if (target != String.Empty)
                {
                    bcItem.Target = target;

                    // For dialog, if breadcrumb targets root element set displaytitle 'true'
                    if (IsDialog && (UIContext.RootElementID == parentElement.ElementID))
                    {
                        url = URLHelper.UpdateParameterInUrl(url, "displaytitle", "true");
                    }
                    else
                        // If parent title is defined, replace current 'displaytitle' it with actual parent title
                        if (UIContext["parenttitle"] != null)
                        {
                            String parentTitle = UIContext["parenttitle"].ToString();
                            url = URLHelper.UpdateParameterInUrl(url, "displaytitle", parentTitle);
                        }
                }

                // Append has for dialog
                url = UIContextHelper.AppendDialogHash(UIContext, url);

                bcItem.RedirectUrl = url;
            }

            titleControl.Breadcrumbs.AddBreadcrumb(bcItem);

            bcItem = new BreadcrumbItem();
            bcItem.Text = UIContextHelper.GetObjectBreadcrumbsText(UIContext, bi);
            titleControl.Breadcrumbs.AddBreadcrumb(bcItem);
        }


        /// <summary>
        /// Indicates whether the breadcrumbs should be displayed according to the given UIContext.
        /// </summary>
        public bool DisplayBreadcrumbs()
        {
            bool displayBreadcrumbs = UIContext.DisplayBreadcrumbs;
            if (displayBreadcrumbs && IsDialog)
            {
                // UI page in a dialog => do not display breadcrumbs for the top dialog UI page
                return (UIContext.RootElementID != UIContext.UIElement.ElementID);
            }

            // Standard UI page
            return displayBreadcrumbs;
        }


        /// <summary>
        /// Get site ID either from context or use current site ID
        /// </summary>
        /// <param name="bi">Base info to set</param>
        protected int GetSiteID(BaseInfo bi)
        {
            // Get site ID from Context
            int siteID = ValidationHelper.GetInteger(UIContext["SiteID"], 0);

            // If no SiteID set via context and object does not support global objects - use current site ID
            if ((siteID == 0) && !bi.Generalized.TypeInfo.SupportsGlobalObjects)
            {
                siteID = SiteContext.CurrentSiteID;
            }

            return siteID;
        }


        /// <summary>
        /// Registers the breadcrumbs script
        /// </summary>
        protected void RegisterBreadcrumbsScript()
        {
            string script =
                @"
function refreshBreadcrumbs(newName) {
    $cmsj('.TitleBreadCrumbLast').html(newName);
    cmsrequire(['CMS/Application', 'CMS/EventHub'], function (application, hub) {
        var breadcrumbs = application.getData('breadcrumbs');
        if (breadcrumbs) {
            // Encode given name
            breadcrumbs.data[breadcrumbs.data.length - 1].text = $cmsj('<div/>').text(newName).html();
            hub.publish('PageLoaded');
        }
    });
};
";

            ScriptHelper.RegisterClientScriptBlock(this, typeof(string), "ReloadAndSetTab", ScriptHelper.GetScript(script));
        }


        /// <summary>
        /// Sets title based on UI context data
        /// </summary>
        /// <param name="pt">Page title object</param>
        /// <param name="titleText">Text of title</param>
        public void SetTitle(PageTitle pt, string titleText)
        {
            pt.TitleText = HTMLHelper.HTMLEncode(ResHelper.LocalizeString(titleText));
        }


        /// <summary>
        /// Adds version tab to collection.
        /// </summary>
        /// <param name="context">Controls context</param>
        /// <param name="tabs">Tabs items collection</param>
        protected void ManageVersionTab(UIContext context, List<UITabItem> tabs)
        {
            if (PortalContext.ViewMode == ViewModeEnum.UI)
            {
                var bi = context.EditedObject as BaseInfo;

                if ((bi != null) && ObjectVersionManager.DisplayVersionsTab(bi))
                {
                    var tab = new UITabItem();
                    tab.Text = ResHelper.GetString("properties.versions");
                    tab.Index = tabs.Count;
                    String url = UrlResolver.ResolveUrl(String.Format("~/CMSModules/AdminControls/Pages/ObjectVersions.aspx?objecttype={0}&objectid={1}", UIContextHelper.GetObjectType(context), ObjectID));
                    tab.RedirectUrl = URLHelper.AddParameterToUrl(url, "hash", QueryHelper.GetHash(url));

                    tabs.Add(tab);
                }
            }
        }

        #endregion
    }
}