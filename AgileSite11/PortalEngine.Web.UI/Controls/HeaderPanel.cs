using System;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.DeviceProfiles;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Membership;

using TreeNode = CMS.DocumentEngine.TreeNode;

namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// Portal manager header panel.
    /// </summary>
    internal class HeaderPanel : Panel, IExtendableHeaderPanel
    {
        #region "Variables"

        internal ObjectEditMenu mObjectEditMenu;
        internal EditMenu mEditMenu;
        private CMSPanel pnlMenu;

        private const string CONTENT_CONTROLS_FOLDER = "~/CMSModules/Content/Controls/";

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets the page template of the current page.
        /// </summary>
        private PageTemplateInfo PageTemplate
        {
            get
            {
                return PortalManager.PageTemplate;
            }
        }

        /// <summary>
        /// Current user
        /// </summary>
        private CurrentUserInfo CurrentUser
        {
            get
            {
                return PortalManager.CurrentUser;
            }
        }


        /// <summary>
        /// Gets or sets the current portal manager
        /// </summary>
        private CMSPortalManager PortalManager
        {
            get;
            set;
        }


        /// <summary>
        /// Gets the current node.
        /// </summary>
        public TreeNode CurrentNode
        {
            get
            {
                return PortalManager.CurrentNode;
            }
        }


        #endregion


        #region "Context methods"

        /// <summary>
        /// OnInit event handler
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            EnsureChildControls();
        }


        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based
        /// implementation to create any child controls they contain in preparation for
        /// posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            // Keep current view mode
            ViewModeEnum viewMode = PortalManager.ViewMode;

            // Info panel
            CreateInfoPanel(viewMode);


            // Indicates whether the current page template contains any editor zones
            bool containsEditorZones = false;
            bool addEditMenu = false;

            PageInfo pi = PortalManager.CurrentPageInfo;
            var showPanel = PortalManager.ShowPanel;

            switch (viewMode)
            {
                case ViewModeEnum.Design:
                case ViewModeEnum.DesignWebPart:
                case ViewModeEnum.DesignDisabled:
                    if (showPanel)
                    {
                        PageTemplateInfo pageTemplate = PageTemplate;

                        // Add the object edit panel if required, dont apply for webpart design (layout webpart default content tab)
                        if ((viewMode != ViewModeEnum.DesignWebPart) &&
                            // Don't add only for device profiles and UI template
                            (PortalManager.UseObjectLocking || (((pageTemplate == null) || (pageTemplate.PageTemplateType != PageTemplateTypeEnum.UI)) && DeviceProfileInfoProvider.IsDeviceProfilesEnabled(PortalManager.SiteName))))
                        {
                            CreateObjectEditPanel();
                        }

                        // Add the web part toolbar if required
                        var pt = PageTemplate;
                        if ((pt == null) || pt.Generalized.IsCheckedOutByUser(CurrentUser) || !PortalManager.UseObjectLocking)
                        {
                            CreateWebPartToolbar(pi);
                        }
                    }
                    break;


                case ViewModeEnum.Edit:
                case ViewModeEnum.EditDisabled:
                    pi.LoadVersion(PortalManager.CurrentNode);
                    addEditMenu = showPanel;
                    if (showPanel)
                    {
                        CreateVariantControls(pi, ref containsEditorZones);
                    }
                    break;


                case ViewModeEnum.Preview:
                    // Preview mode - do not show for preview mode -> this mode displays its own edit panel
                    break;
                    
                case ViewModeEnum.EditLive:
                case ViewModeEnum.LiveSite:
                    // Do not show on-site edit panel when management panel should be shown
                    if (!showPanel)
                    {
                        // Add edit toolbar in the on-site edit mode and in the live site mode
                        CreateOnSiteEditToolbar(PortalContext.ViewMode);
                    }

                    // Do not show object edit panel in the on-site editing mode, but show it when forced
                    if (showPanel)
                    {
                        addEditMenu = true;

                        // Make sure management panel is always across whole width of a page
                        CssRegistration.RegisterCssBlock(Page, "BodyMargin", ".cms-bootstrap .CMSHeaderDiv { left: 0 !important; right: 0 !important; }");
                    }
                    break;
            }

            // Add the edit menu if required
            if (addEditMenu)
            {
                // Indicates whether this page is viewed via object preview
                bool isPreview = !String.IsNullOrEmpty(QueryHelper.GetString("previewobjectname", String.Empty));
                CreateEditMenu(isPreview, viewMode);
            }

            // Add variant slider if needed
            CreateVariantSlider(pi, containsEditorZones, viewMode);

            if (addEditMenu)
            {
                // Add CKToolbar
                CreateCKToolbar(viewMode);
            }
            else if (viewMode != ViewModeEnum.LiveSite)
            {
                PortalManager.CreateMessagesPlaceholder();
            }
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="manager">Current portal manager</param>
        public HeaderPanel(CMSPortalManager manager)
        {
            PortalManager = manager;
        }


        /// <summary>
        /// Renders the HTML opening tag of the <see cref="T:System.Web.UI.WebControls.Panel" /> control to the specified writer.
        /// </summary>
        public override void RenderBeginTag(HtmlTextWriter writer)
        {
            if (!PortalManager.ViewMode.IsLiveSite() || PortalManager.ShowPanel)
            {
                // Wrap the control into the bootstrap container
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "cms-bootstrap");
                writer.RenderBeginTag(HtmlTextWriterTag.Div);
            }

            base.RenderBeginTag(writer);
        }


        /// <summary>
        /// Renders the HTML closing tag of the <see cref="T:System.Web.UI.WebControls.Panel" /> control into the specified writer.
        /// </summary>
        public override void RenderEndTag(HtmlTextWriter writer)
        {
            base.RenderEndTag(writer);

            if (!PortalManager.ViewMode.IsLiveSite() || PortalManager.ShowPanel)
            {
                // Close the bootstrap container tag
                writer.RenderEndTag();
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Adds additional controls to panel.
        /// </summary>
        /// <param name="control">Control to add</param>
        public void AddAdditionalControl(Control control)
        {
            if (control != null)
            {
                Controls.Add(control);
            }
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Creates a placeholders for the CK editor toolbar
        /// </summary>
        /// <param name="viewMode">View mode</param>
        private void CreateCKToolbar(ViewModeEnum viewMode)
        {
            // Add CKToolbar for edit mode
            if (viewMode.IsEdit(true))
            {
                Controls.Add(new LiteralControl("<div id=\"CKToolbar\" ></div>"));
                if (Page is ICMSPage)
                {
                    try
                    {
                        ((ICMSPage)Page).FooterContainer.Controls.Add(new LiteralControl("<div id=\"CMSFooterDiv\"><div id=\"CKFooter\"></div></div>"));
                    }
                    catch
                    {
                    }
                }
            }
        }


        /// <summary>
        /// Adds the edit menu control
        /// </summary>
        /// <param name="isPreview">Flag whether the preview mode is forced</param>
        /// <param name="viewMode">View mode</param>
        private void CreateEditMenu(bool isPreview, ViewModeEnum viewMode)
        {
            EditMenu editMenu = Page.LoadUserControl(CONTENT_CONTROLS_FOLDER + "EditMenu.ascx") as EditMenu;
            if ((editMenu != null) && !isPreview)
            {
                bool preview = viewMode.IsPreview();

                editMenu.ID = "menuElem";
                editMenu.ShortID = "m";
                editMenu.ShowProperties = false;
                editMenu.ShowSpellCheck = true;
                editMenu.ShowSave = !preview;
                editMenu.ShowCheckOut = !preview;
                editMenu.ShowCheckIn = !preview;
                editMenu.ShowUndoCheckOut = !preview;
                editMenu.ShowApplyWorkflow = !preview;
                editMenu.NodeID = PortalManager.CurrentPageInfo.NodeID;
                editMenu.CultureCode = PortalManager.CurrentPageInfo.DocumentCulture;
                editMenu.UseSmallIcons = true;
                editMenu.IsLiveSite = false;
                editMenu.ActionsValidationGroup = "cmsContentEditGroup";

                CreateMenuPanel();

                pnlMenu.Controls.Add(editMenu);

                mEditMenu = editMenu;

                // Do not generate hide script for preview link
                if (!VirtualContext.IsPreviewLinkInitialized)
                {
                    string script =
                        @"
    if (window['isCmsDesk'] && !window.isCmsDesk){
        var infoElem = document.getElementById('" + pnlMenu.ClientID + @"'); 
        if (infoElem){
            infoElem.style.display = 'none'; 
        }
    }
    ";
                    Controls.Add(new LiteralControl(ScriptHelper.GetScript(script))
                    {
                        EnableViewState = false
                    });
                }
                var extensionTarget = editMenu as IExtensibleEditMenu;
                extensionTarget.InitializeExtenders("Content");
            }

            PortalManager.CreateMessagesPlaceholder();

            // Document manager
            PortalManager.DocumentManager.LocalMessagesPlaceHolder = PortalManager.CurrentMessagesPlaceholder;
        }


        /// <summary>
        /// Creates the on-site editing toolbar
        /// </summary>
        /// <param name="viewMode">View mode</param>
        private void CreateOnSiteEditToolbar(ViewModeEnum viewMode)
        {
            if (PortalHelper.IsOnSiteEditingEnabled(PortalManager.SiteName) && AuthenticationHelper.IsAuthenticated())
            {
                if ((viewMode.IsEditLive()) || ((viewMode.IsLiveSite()) && (SettingsKeyInfoProvider.GetBoolValue(PortalManager.SiteName + ".CMSOnSiteEditButton"))))
                {
                    // Do not show the toolbar button when the page is not found
                    if ((viewMode.IsLiveSite())
                        && (HttpContext.Current != null) && (HttpContext.Current.Response.StatusCode == 404))
                    {
                        return;
                    }

                    CMSAbstractPortalUserControl editToolbar = (CMSAbstractPortalUserControl)Page.LoadUserControl("~/CMSModules/PortalEngine/Controls/OnsiteEdit/EditToolbar.ascx");
                    editToolbar.PortalManager = PortalManager;
                    editToolbar.ID = "editToolbar";
                    editToolbar.ShortID = "et";

                    Controls.Add(editToolbar);
                }
            }
        }


        /// <summary>
        /// Creates the controls for content personalization and widget variants
        /// </summary>
        /// <param name="pi">Page info</param>
        /// <param name="containsEditorZones">Flag whether the page template contains editor zones</param>
        private void CreateVariantControls(PageInfo pi, ref bool containsEditorZones)
        {
            if ((PortalContext.MVTVariantsEnabled || PortalContext.ContentPersonalizationEnabled) && (pi.TemplateInstance != null))
            {
                // Find if any of the zones is a Editor zone
                foreach (WebPartZoneInstance zone in PortalManager.CurrentPageInfo.TemplateInstance.WebPartZones)
                {
                    if (zone.WidgetZoneType == WidgetZoneTypeEnum.Editor)
                    {
                        containsEditorZones = true;
                        break;
                    }
                }

                // Include MVT/CP scripts only if there are any editor zones
                if (containsEditorZones)
                {
                    PortalManager.CreateWidgetVariantMenu();
                }
            }

            // Add the content personalization menu
            if (PortalContext.ContentPersonalizationVariantsEnabled)
            {
                PortalManager.CreatePersonalizationMenu();
            }
        }


        /// <summary>
        /// Creates the web part toolbar control
        /// </summary>
        /// <param name="pi">The page info object</param>
        private void CreateWebPartToolbar(PageInfo pi)
        {
            var currentUser = CurrentUser;

            // Check the permissions for shared templates
            PageTemplateInfo pti = pi.UsedPageTemplateInfo;
            if ((pti != null) && pti.IsReusable && !currentUser.IsAuthorizedPerUIElement("CMS.Design", "Design.ModifySharedTemplates"))
            {
                return;
            }

            if (!currentUser.IsAuthorizedPerResource("CMS.Design", "Design") || !currentUser.IsAuthorizedPerResource("CMS.Content", "Modify"))
            {
                return;
            }

            CMSAbstractPortalUserControl mWebPartToolbarControl = (CMSAbstractPortalUserControl)Page.LoadUserControl(CONTENT_CONTROLS_FOLDER + "WebPartToolbar.ascx");
            mWebPartToolbarControl.PortalManager = PortalManager;
            mWebPartToolbarControl.ID = "webPartToolbar";
            mWebPartToolbarControl.ShortID = "wpt";

            Controls.Add(mWebPartToolbarControl);
        }


        /// <summary>
        /// Ensures menu panel
        /// </summary>
        private void CreateMenuPanel()
        {
            if (pnlMenu == null)
            {
                pnlMenu = new CMSPanel
                {
                    ID = "pnlMenu",
                    ShortID = "pMN",
                    CssClass = CultureHelper.IsUICultureRTL() ? "RTL" : ""
                };
                Controls.Add(pnlMenu);
            }
        }


        /// <summary>
        /// Creates and adds the object edit panel control.
        /// </summary>
        private void CreateObjectEditPanel()
        {
            PortalManager.UIContext.EditedObject = PageTemplate;

            CreateMenuPanel();

            // Init object edit panel
            var objectEditPanel = Page.LoadUserControl("~/CMSModules/Objects/Controls/Locking/ObjectEditPanel.ascx") as IObjectEditPanel;
            objectEditPanel.PreviewMode = true;

            var abstractControl = (AbstractUserControl)objectEditPanel;
            abstractControl.IsLiveSite = false;
            abstractControl.ID = "objectEditPanel";
            pnlMenu.Controls.Add(abstractControl);

            mObjectEditMenu = ((IObjectEditPanel)abstractControl).AbstractObjectEditMenu as ObjectEditMenu;

            // Init object edit menu
            objectEditPanel.AbstractObjectEditMenu.ShowSave = false;
            ((AbstractUserControl)objectEditPanel.AbstractObjectManager).ID = "objEditMenu";

            // Init object manager
            objectEditPanel.AbstractObjectManager.ShowPanel = true;
            objectEditPanel.AbstractObjectManager.RenderScript = true;
            objectEditPanel.AbstractObjectManager.RegisterEvents = true;
            objectEditPanel.AbstractObjectManager.OnAfterAction += (sender, args) => RefreshPage();

            ((AbstractUserControl)objectEditPanel.AbstractObjectManager).ID = "objManager";
            var extendableMenu = objectEditPanel.AbstractObjectEditMenu as IExtensibleEditMenu;
            extendableMenu.InitializeExtenders("Content");
        }


        /// <summary>
        /// Refreshes current page by redirecting it to the same URL.
        /// </summary>
        internal static void RefreshPage()
        {
            string url = RequestContext.CurrentURL;
            url = URLHelper.RemoveParameterFromUrl(url, "safemode");
            URLHelper.Redirect(url);
        }


        /// <summary>
        /// Creates info panel 
        /// </summary>
        /// <param name="viewMode">View mode</param>
        private void CreateInfoPanel(ViewModeEnum viewMode)
        {
            if (viewMode == ViewModeEnum.LiveSite && !PortalManager.ShowPanel)
            {
                return;
            }

            PortalInfoPanel portalInfoPanel = new PortalInfoPanel(PortalManager);
            portalInfoPanel.ID = "pnlPreviewInfo";
            Controls.Add(portalInfoPanel);
        }


        /// <summary>
        /// Creates the variant slider
        /// </summary>
        /// <param name="pi">Page info</param>
        /// <param name="containsEditorZones">Flag whether the page contains some editor zones</param>
        /// <param name="viewMode">View mode</param>
        private void CreateVariantSlider(PageInfo pi, bool containsEditorZones, ViewModeEnum viewMode)
        {
            // Add variant slider if needed
            switch (viewMode)
            {
                case ViewModeEnum.Design:
                case ViewModeEnum.Preview:
                case ViewModeEnum.Edit:
                case ViewModeEnum.EditDisabled:
                case ViewModeEnum.EditNotCurrent:

                    // Do not show the combination panel for Preview URL page
                    if (VirtualContext.IsPreviewLinkInitialized)
                    {
                        return;
                    }

                    // Add variant slider
                    if ((PortalContext.MVTVariantsEnabled || PortalContext.ContentPersonalizationEnabled)
                        && (CurrentUser.IsAuthorizedPerResource("cms.mvtest", "Read")
                            || CurrentUser.IsAuthorizedPerResource("cms.contentpersonalization", "Read")))
                    {

                        // Do not render the MVT/CP scripts when in edit mode and no editor zones are present or no MVT test exists for this page
                        bool renderSupportScripts = !(PortalManager.ViewMode.IsOneOf(ViewModeEnum.Edit, ViewModeEnum.EditDisabled, ViewModeEnum.EditNotCurrent)
                                                      && !containsEditorZones && !PortalContext.ContentPersonalizationVariantsEnabled && !PortalContext.MVTVariantsEnabled);

                        // Include MVT/CP scripts only if there are any editor zones
                        if (renderSupportScripts)
                        {
                            // Register the js file for the variant slider
                            ScriptHelper.RegisterJQueryCookie(Page);
                            ScriptHelper.RegisterScriptFile(Page, "DesignMode/variants.js");

                            if (PortalContext.MVTVariantsEnabled)
                            {
                                // MVT combinations panel
                                CMSAbstractPortalUserControl mMVTCombinationsControl = (CMSAbstractPortalUserControl)Page.LoadUserControl("~/CMSModules/OnlineMarketing/Controls/Content/CombinationPanel.ascx");
                                mMVTCombinationsControl.PortalManager = PortalManager;
                                mMVTCombinationsControl.ID = "MVTCombinationPanel";
                                mMVTCombinationsControl.ShortID = "mvtCP";
                                Controls.Add(mMVTCombinationsControl);

                                // Register MVT support scripts
                                ScriptHelper.RegisterScriptFile(Page, "DesignMode/mvt.js");
                            }

                            int templateId = 0;

                            // Get the template ID
                            PageTemplateInfo pti = pi.UsedPageTemplateInfo;
                            if (pti != null)
                            {
                                templateId = pti.PageTemplateId;
                            }

                            string cookieName = CookieName.GetVariantSliderPositionsCookieName(templateId);

                            HiddenField hidCPVariantSliderPositions = new HiddenField();
                            hidCPVariantSliderPositions.ID = "hidVariantSliderPositions";
                            hidCPVariantSliderPositions.EnableViewState = false;
                            hidCPVariantSliderPositions.Value = HttpUtility.UrlDecode(CookieHelper.GetValue(cookieName));
                            Controls.Add(hidCPVariantSliderPositions);

                            StringBuilder sb = new StringBuilder();
                            sb.Append(
@"
function GetCPVariantSliderPositionElem() {
    return document.getElementById('", hidCPVariantSliderPositions.ClientID, @"');
}

function SaveSlidersConfiguration() {
    // Save Content personalization
    if (cpVariantSliderPositionElem.value.length > 0) {
        $cmsj.cookie('", cookieName, @"', cpVariantSliderPositionElem.value, { expires: 7, path: '/' });
    }
    else {
        $cmsj.cookie('", cookieName, @"', null,  { path: '/' });
    }
    // Save MVT
    if (window.SaveCombinationPanelSelection) {
        SaveCombinationPanelSelection();
    }
}");

                            const string OM_DIALOGS_FOLDER = "~/CMSModules/OnlineMarketing/Dialogs/";

                            if (PortalContext.ContentPersonalizationEnabled)
                            {
                                sb.Append(
@"
function AddPersonalizationVariant(zoneId, webPartName, aliasPath, instanceGuid, templateId, variantType, itemCode) {
    var url = '", ResolveUrl(OM_DIALOGS_FOLDER + "ContentPersonalizationVariantEdit.aspx?nodeid="), PortalManager.CurrentPageInfo.NodeID, @"&zoneid=' + zoneId + '&webpartid=' + webPartName + '&instanceguid=' + instanceGuid + '&aliaspath=' + aliasPath + '&templateid=' + templateId + '&varianttype=' + variantType;
    modalDialog(url, 'editpersonalizationvariant', 750, 450);
}

function ListPersonalizationVariants(zoneId, webPartName, aliasPath, instanceGuid, templateId, variantType, itemCode) {
    var url = '", ResolveUrl(OM_DIALOGS_FOLDER + "ContentPersonalizationVariantList.aspx?nodeid="), PortalManager.CurrentPageInfo.NodeID, @"&zoneid=' + zoneId + '&webpartid=' + webPartName + '&instanceguid=' + instanceGuid + '&aliaspath=' + aliasPath + '&templateid=' + templateId + '&varianttype=' + variantType;
    modalDialog(url, 'editpersonalizationvariant', 750, 540);
}"
                                );
                            }

                            // Register the script
                            ScriptHelper.RegisterClientScriptBlock(this, typeof(string), "VariantsScript", ScriptHelper.GetScript(sb.ToString()));


                            // Register the EditVariant dialog function
                            if ((PortalContext.MVTVariantsEnabled || PortalContext.ContentPersonalizationEnabled) && DocumentContext.CurrentPageInfo != null)
                            {
                                StringBuilder editVariantDialog = new StringBuilder();
                                editVariantDialog.Append(
@"
function GetMVTAddVariantDialog() {
    return '", ResolveUrl(OM_DIALOGS_FOLDER + "MVTVariantEdit.aspx?nodeid="), DocumentContext.CurrentPageInfo.NodeID, @"';
}

function GetMVTListVariantsDialog() {
    return '", ResolveUrl(OM_DIALOGS_FOLDER + "MVTVariantList.aspx?nodeid="), DocumentContext.CurrentPageInfo.NodeID, @"';
}

function OnAddWidgetVariant(query) {
    modalDialog('", ResolveUrl("~/CMSModules/Widgets/Dialogs/WidgetProperties.aspx"), @"' + query + '&isnewvariant=true', 'configurewebpart', 900, 600);
}"
                                );

                                ScriptHelper.RegisterStartupScript(this, typeof(string), "editVariantDialog", ScriptHelper.GetScript(editVariantDialog.ToString()));
                            }
                        }
                    }
                    break;
            }
        }

        #endregion
    }
}
