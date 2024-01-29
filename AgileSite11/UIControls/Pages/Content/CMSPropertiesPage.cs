using System;

using CMS.Base.Web.UI;
using CMS.Helpers;
using CMS.Membership;
using CMS.PortalEngine;
using CMS.WorkflowEngine;

namespace CMS.UIControls
{
    /// <summary>
    /// Base page for the properties pages to apply global settings to the pages.
    /// </summary>
    public abstract class CMSPropertiesPage : CMSContentPage
    {
        #region "Constants & variables"

        /// <summary>
        /// General tab.
        /// </summary>
        protected const String TAB_GENERAL = "general";

        /// <summary>
        /// URLs tab.
        /// </summary>
        protected const String TAB_URLS = "urls";

        /// <summary>
        /// Template tab
        /// </summary>
        protected const String TAB_TEMPLATE = "template";

        /// <summary>
        /// Metadata tab
        /// </summary>
        protected const String TAB_METADATA = "metadata";

        /// <summary>
        /// Categories tab
        /// </summary>
        protected const String TAB_CATEGORIES = "categories";

        /// <summary>
        /// Menu tab
        /// </summary>
        protected const String TAB_MENU = "menu";

        /// <summary>
        /// Workflow tab
        /// </summary>
        protected const String TAB_WORKFLOW = "workflow";

        /// <summary>
        /// Versions tab
        /// </summary>
        protected const String TAB_VERSIONS = "versions";

        /// <summary>
        /// Related documents tab
        /// </summary>
        protected const String TAB_RELATEDDOCS = "relateddocs";

        /// <summary>
        /// Linked documents tab
        /// </summary>
        protected const String TAB_LINKEDDOCS = "linkeddocs";

        /// <summary>
        /// Security tab
        /// </summary>
        protected const String TAB_SECURITY = "security";

        /// <summary>
        /// Attachments tab
        /// </summary>
        protected const String TAB_ATTACHMENTS = "attachments";

        /// <summary>
        /// Languages tab
        /// </summary>
        protected const String TAB_LANGUAGES = "languages";

        /// <summary>
        /// Variants tab
        /// </summary>
        protected const String TAB_VARIANTS = "variants";


        private bool? mShowContentOnlyProperties;

        #endregion


        #region "Properties"


        /// <summary>
        /// Indicates if only properties relevant to content only pages should be shown.
        /// </summary>
        protected bool ShowContentOnlyProperties
        {
            get
            {
                if (mShowContentOnlyProperties == null)
                {
                    mShowContentOnlyProperties = Node.NodeIsContentOnly || Node.Site.SiteIsContentOnly;
                }

                return mShowContentOnlyProperties.Value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Init event handler
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // Set dialog mode for on site editing
            if (PortalContext.ViewMode.IsEditLive())
            {
                RequiresDialog = true;
                DocumentManager.OnSaveData += DocumentManager_OnSaveData;
            }
            else
            {
                // Force view mode to protect displaying the page content within live site view mode (Temporary fix - Pages application cannot be opened in multiple tabs yet)
                if (PortalContext.ViewMode.IsLiveSite())
                {
                    PortalContext.ViewMode = ViewModeEnum.Edit;
                }
                DocumentManager.OnAfterAction += DocumentManager_OnAfterAction;
            }

            CheckPropertiesSecurity();
        }


        /// <summary>
        /// Checks security for properties page.
        /// </summary>
        public static void CheckPropertiesSecurity()
        {
            // Check permissions for CMS Desk -> Content -> Properties tab
            var user = MembershipContext.AuthenticatedUser;
            if (!user.IsAuthorizedPerUIElement("CMS.Content", "Properties"))
            {
                RedirectToUIElementAccessDenied("CMS.Content", "Properties");
            }
        }


        /// <summary>
        /// Ensures base page reload after data save
        /// </summary>
        private void DocumentManager_OnSaveData(object sender, DocumentManagerEventArgs e)
        {
            string script = string.Empty;

            if (RequiresDialog)
            {
                script = "if ((parent != null) && (parent.parent != null) && (parent.parent.parent != null)) { parent.parent.parent.refreshPageOnClose = true; }";
            }
            else
            {
                script = "if ((this.parent != null) && (this.parent.parent != null)) { this.parent.parent.frames['header'].reloadPage = true; }";
            }

            ControlsHelper.RegisterClientScriptBlock(this, Page, typeof(string), "PropertiesDialogRefreshValue", ScriptHelper.GetScript(script));
        }


        /// <summary>
        /// Gets current property tab.
        /// </summary>
        protected String GetPropertyTab()
        {
            return ValidationHelper.GetString(ContextHelper.GetItem(CookieName.PropertyTab, true, false, true), TAB_GENERAL);
        }


        /// <summary>
        /// Sets current property tab.
        /// </summary>
        /// <param name="value">Value to store</param>
        protected void SetPropertyTab(String value)
        {
            ContextHelper.Add(CookieName.PropertyTab, value, true, false, true, DateTime.Now.AddDays(1));
        }


        private void DocumentManager_OnAfterAction(object sender, DocumentManagerEventArgs e)
        {
            WorkflowInfo workflow = e.Workflow;
            WorkflowStepInfo nextStep = e.CurrentStep;
            WorkflowStepInfo originalStep = e.OriginalStep;

            // If workflow state changed
            if ((workflow != null) && ((originalStep == null) || (nextStep == null)))
            {
                // Refresh left menu
                string script = "if (parent.RefreshLeftMenu) { parent.RefreshLeftMenu(); }";
                ScriptHelper.RegisterStartupScript(this, typeof(string), script.GetHashCode().ToString(), ScriptHelper.GetScript(script));
            }

            if (e.ActionName == ComponentEvents.SAVE)
            {
                // Reset the "content changed" flag after the document save action
                ScriptHelper.RegisterStartupScript(this, typeof(string), "loadDataFromUrl", "CMSContentManager.changed(false);", true);
            }
        }

        #endregion
    }
}
