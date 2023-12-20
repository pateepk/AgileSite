using System;
using System.Text;
using System.Web.UI;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;
using CMS.Membership;
using CMS.PortalEngine;
using CMS.PortalEngine.Web.UI;
using CMS.SiteProvider;

namespace CMS.UIControls
{
    /// <summary>
    /// Base page for the administration pages to apply global settings to the pages.
    /// </summary>
    public class CMSDeskPage : CMSPage
    {
        #region "Variables"

        private bool mRegisterSplitScrollSync = true;
        private bool mRequireSite = true;
        private bool mCheckDocPermissions = true;
        private string mMode;
        private string mDevice;
        private string mAction;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets current document.
        /// </summary>
        public TreeNode Node
        {
            get
            {
                return DocumentManager.Node;
            }
        }


        /// <summary>
        /// Gets node ID of current document.
        /// </summary>
        public override int NodeID
        {
            get
            {
                if ((base.NodeID <= 0) && (Node != null))
                {
                    return Node.NodeID;
                }

                return base.NodeID;
            }
        }


        /// <summary>
        /// Gets document ID of current document.
        /// </summary>
        public override int DocumentID
        {
            get
            {
                if ((base.DocumentID <= 0) && (Node != null))
                {
                    return Node.DocumentID;
                }
                return base.DocumentID;
            }
        }


        /// <summary>
        /// Culture to consider as preferred.
        /// </summary>
        public override string CultureCode
        {
            get
            {
                if (Node != null)
                {
                    return Node.DocumentCulture;
                }
                return base.CultureCode;
            }
        }


        /// <summary>
        /// Mode query parameter value.
        /// </summary>
        protected string Mode
        {
            get
            {
                return mMode ?? (mMode = QueryHelper.GetString("mode", null));
            }
        }


        /// <summary>
        /// Device query parameter value.
        /// </summary>
        protected string Device
        {
            get
            {
                return mDevice ?? (mDevice = QueryHelper.GetString("device", null));
            }
        }


        /// <summary>
        /// Action query parameter value.
        /// </summary>
        protected string Action
        {
            get
            {
                return mAction ?? (mAction = QueryHelper.GetString("action", string.Empty).ToLowerCSafe());
            }
        }


        /// <summary>
        /// If true, current site is required.
        /// </summary>
        public bool RequireSite
        {
            get
            {
                return mRequireSite;
            }
            set
            {
                mRequireSite = value;
            }
        }


        /// <summary>
        /// Indicates if document permissions should be checked during page load.
        /// </summary>
        public bool CheckDocPermissions
        {
            get
            {
                return mCheckDocPermissions;
            }
            set
            {
                mCheckDocPermissions = value;
            }
        }


        /// <summary>
        /// Indicates if split mode (side by side comparison) is enabled for this page. Default value is false. 
        /// </summary>
        protected bool EnableSplitMode
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if scrollbar synchronization script is registered. Default value is true.
        /// </summary>
        protected bool RegisterSplitScrollSync
        {
            get
            {
                return mRegisterSplitScrollSync;
            }
            set
            {
                mRegisterSplitScrollSync = value;
            }
        }


        /// <summary>
        /// Identifies if the page is used for products UI
        /// </summary>
        protected virtual bool IsProductsUI
        {
            get
            {
                return false;
            }
        }

        #endregion


        #region "Events"

        /// <summary>
        /// The event is raised everytime the permission check for community group is needed.
        /// </summary>
        public static event Func<CheckGroupPermissionArgs, bool> OnCheckGroupPermissions;
        
        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the <see cref="CMSDeskPage"/> class.
        /// </summary>
        public CMSDeskPage()
        {
            Load += BasePage_Load;
            PreInit += CMSDeskPage_PreInit;
            Init += CMSDeskPage_Init;
        }

        #endregion


        #region "Page events"

        /// <summary>
        /// Handles the PreInit event of the CMSDeskPage control.
        /// </summary>
        private void CMSDeskPage_PreInit(object sender, EventArgs e)
        {
            CheckAdministrationInterface();

            // Session management logging
            if (DatabaseHelper.IsDatabaseAvailable && SessionManager.OnlineUsersEnabled && !RequestHelper.IsPostBack())
            {
                SessionManager.UpdateCurrentSession(null);
            }
        }


        /// <summary>
        /// Handles the Init event of the CMSDeskPage control.
        /// </summary>
        protected void CMSDeskPage_Init(object sender, EventArgs e)
        {
            // Check document permissions
            if (CheckDocPermissions)
            {
                CheckDocumentPermissions();
            }

            // Register dialog scripts for dialog mode
            if (RequiresDialog)
            {
                DocumentManager.SetRefreshFlag = true;
                DocumentManager.SetRedirectPageFlag = QueryHelper.GetBoolean("reloadnewpage", false);

                RegisterModalPageScripts();
            }
        }


        /// <summary>
        /// PageLoad event handler
        /// </summary>
        protected void BasePage_Load(object sender, EventArgs e)
        {
            RedirectToSecured();
            SetRTL();
            SetBrowserClass();
            AddNoCacheTag();

            if (RequireSite)
            {
                CheckSite();
            }

            CheckEditor();

            // Enable split mode only in administration
            if (EnableSplitMode && (PortalContext.ViewMode != ViewModeEnum.EditLive))
            {
                EnsureSplitMode();
            }

            ScriptHelper.RegisterClientScriptBlock(this, typeof(string), "InfoScript", ScriptHelper.GetScript("function IsCMSDesk() { return true; }"));
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Ensures split mode.
        /// </summary>
        protected void EnsureSplitMode()
        {
            // When split mode is enabled and requested
            if (PortalUIHelper.DisplaySplitMode)
            {
                // For split view, display dialog for new culture version
                if (Node == null)
                {
                    RedirectToNewCultureVersionPage();
                }

                // Check if scrollbar sync script has to be registered
                if (RegisterSplitScrollSync)
                {
                    // Register java-script synchronization script for split mode
                    RegisterSplitModeSync(true, false);
                }
            }
        }


        /// <summary>
        /// Redirects to new document language version page.
        /// </summary>
        protected virtual void RedirectToNewCultureVersionPage()
        {
            // Redirect
            URLHelper.Redirect(DocumentUIHelper.GetNewCultureVersionPageUrl());
        }


        /// <summary>
        /// Adds script for selecting of ecommerce menu button.
        /// </summary>
        /// <param name="elementName">UI Element code name (button)</param>
        /// <param name="url">URL to redirect to</param>
        /// <param name="menuFrame">Frame with menu</param>
        public void AddMenuButtonSelectScript(string elementName, string url, string menuFrame)
        {
            AddMenuButtonSelectScript(this, elementName, url, menuFrame);
        }


        /// <summary>
        /// Adds script for selecting of unimenu button to given Control (probably Page).
        /// </summary>
        /// <param name="control">Control (page) to assign script to</param>
        /// <param name="elementName">UI Element code name (button)</param>
        /// <param name="url">URL to redirect to</param>
        /// <param name="menuFrame">Frame with menu</param>
        public static void AddMenuButtonSelectScript(Control control, string elementName, string url, string menuFrame)
        {
            var sb = new StringBuilder();

            sb.Append(
@"
function UM_SelectMenuItem(w)
{
    if (w != null)
    {
        var f = w.frames['" + menuFrame + @"'];
        if (f != null) {
            if (f.SelectItem != null) {
                f.SelectItem('" + elementName + @"', '" + ScriptHelper.GetString(url, false) + @"', false);
            }
        }
    }
}

var wp = window.parent;
if (wp != null)
{
    UM_SelectMenuItem(wp);
    UM_SelectMenuItem(wp.parent);
}
"
                );

            ScriptHelper.RegisterClientScriptBlock(control, typeof(string), "UniMenuButtonSelectScript", ScriptHelper.GetScript(sb.ToString()));
        }


        /// <summary>
        /// Checks document permissions regarding the document manager mode and optionally redirects to access denied page
        /// </summary>
        protected void CheckDocumentPermissions()
        {
            DocumentUIHelper.CheckDocumentPermissions(DocumentManager);
        }


        /// <summary>
        /// Checks currently edited document permissions and optionally redirects to access denied page
        /// </summary>
        /// <param name="permission">Permission to check</param>
        protected void CheckDocumentPermissions(PermissionsEnum permission)
        {
            DocumentUIHelper.CheckDocumentPermissions(permission, Node);
        }


        /// <summary>
        /// Check whether user is group administrator or has manage permission. If <paramref name="autoRedirect"/> is omitted, 
        /// a redirection to Access denied page is done if the user has insufficient permissions.
        /// </summary>
        /// <param name="groupId">Community group ID</param>
        /// <param name="permissionName">Permission name</param>
        /// <param name="autoRedirect">Indicates whether user should be redirected to the access denied page</param>
        public static bool CheckGroupPermissions(int groupId, string permissionName, bool autoRedirect = true)
        {
            if (RaiseOnCheckGroupPermissions(groupId, permissionName, SiteContext.CurrentSiteID))
            {
                return true;
            }

            if (autoRedirect)
            {
                RedirectToAccessDenied("CMS.Groups", permissionName);
            }

            return false;
        }


        /// <summary>
        /// Ensures current document breadcrumbs (non-clickable)
        /// </summary>
        /// <param name="breadcrumbs">Breadcrumbs control</param>
        /// <param name="node">Current document. If not provided, action text is used instead.</param>
        /// <param name="action">Name of the action</param>
        protected void EnsureDocumentBreadcrumbs(Breadcrumbs breadcrumbs, TreeNode node = null, string action = null)
        {
            if (IsProductsUI)
            {
                var url = UIContextHelper.GetElementUrl(ModuleName.ECOMMERCE, "Products");
                var suffix = (Action == "newculture") ? "" : "com.productsection";

                DocumentUIHelper.EnsureDocumentBreadcrumbs(breadcrumbs, node, action, url, suffix);
            }
            else
            {
                DocumentUIHelper.EnsureDocumentBreadcrumbs(breadcrumbs, node, action, "");
            }
        }


        private static bool RaiseOnCheckGroupPermissions(int groupId, string permissionName, int siteId)
        {
            bool? result = OnCheckGroupPermissions?.Invoke(new CheckGroupPermissionArgs(MembershipContext.AuthenticatedUser, groupId, permissionName, siteId));

            return result ?? false;
        }

        #endregion
    }
}