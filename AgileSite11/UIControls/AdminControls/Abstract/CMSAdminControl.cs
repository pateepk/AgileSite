using System;
using System.Drawing;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.DocumentEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.Membership;

using TreeNode = CMS.DocumentEngine.TreeNode;

namespace CMS.UIControls
{
    /// <summary>
    /// Administration interface control - Base control used for the standard administration interface.
    /// </summary>
    public abstract class CMSAdminControl : CMSUserControl
    {
        #region "Constants"

        // Path to the error control
        private const string ERROR_CONTROL_PATH = "~/CMSAdminControls/UI/System/ErrorMessage.ascx";

        /// <summary>
        /// Represents the 'Modify' type of default module permission, used for the 'Manage' permission representation as well.
        /// </summary>
        public const string PERMISSION_MODIFY = "modify";

        /// <summary>
        /// Represents the 'GlobalModify' type of default module permission for global objects, used for the 'Manage' permission representation as well.
        /// </summary>
        public const string PERMISSION_GLOBALMODIFY = "globalmodify";

        /// <summary>
        /// Represents the 'Read' type of default module permission.
        /// </summary>
        public const string PERMISSION_READ = "read";

        /// <summary>
        /// Represents the 'GlobalRead' type of default module permission for global objects.
        /// </summary>
        public const string PERMISSION_GLOBALREAD = "globalread";

        /// <summary>
        /// Represents the 'Delete' type of default module permission.
        /// </summary>
        public const string PERMISSION_DELETE = "delete";

        /// <summary>
        /// Represents the 'Manage' type of default module permission.
        /// </summary>
        public const string PERMISSION_MANAGE = "manage";

        #endregion


        #region "Delegates"

        /// <summary>
        /// Delegate of event fired when permissions should be checked.
        /// </summary>
        /// <param name="permissionType">Type of a permission to check</param>
        /// <param name="sender">Sender</param>
        public delegate void CheckPermissionsEventHandler(string permissionType, CMSAdminControl sender);


        /// <summary>
        /// Delegate of event fired when permissions should be checked.
        /// </summary>
        /// <param name="permissionType">Type of a permission to check</param>
        /// <param name="modulePermissionType">Name of the module permission</param>
        /// <param name="sender">Sender</param>
        public delegate void CheckPermissionsExtendedEventHandler(string permissionType, string modulePermissionType, CMSAdminControl sender);


        /// <summary>
        /// Delegate of event fired when action is not allowed.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="permissionType">Type of a permission not allowed</param>
        public delegate void NotAllowedEventHandler(string permissionType, CMSAdminControl sender);

        #endregion


        #region "Events"

        /// <summary>
        /// On not allowed event.
        /// </summary>
        public event NotAllowedEventHandler OnNotAllowed;

        /// <summary>
        /// On check permissions event.
        /// </summary>
        public event CheckPermissionsEventHandler OnCheckPermissions;

        /// <summary>
        /// On check permissions event.
        /// </summary>
        public event CheckPermissionsExtendedEventHandler OnCheckPermissionsExtended;

        #endregion


        #region "Variables"

        private bool mRedirectToAccessDeniedPage = true;

        #endregion


        #region "Properties"

        /// <summary>
        /// Information message.
        /// </summary>
        public string InfoMessage
        {
            get;
            set;
        }


        /// <summary>
        /// Error message.
        /// </summary>
        public string ErrorMessage
        {
            get;
            set;
        }


        /// <summary>
        /// Redirect to access denied if the access is not allowed.
        /// </summary>
        public bool RedirectToAccessDeniedPage
        {
            get
            {
                return mRedirectToAccessDeniedPage;
            }
            set
            {
                mRedirectToAccessDeniedPage = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Clears control.
        /// </summary>
        public virtual void ClearForm()
        {
        }


        /// <summary>
        /// Reloads control data without force reload.
        /// </summary>
        public virtual void ReloadData()
        {
            ReloadData(false);
        }


        /// <summary>
        /// Reloads control data.
        /// </summary>
        /// <param name="forceReload">Indicates if force reload should be used</param>
        public virtual void ReloadData(bool forceReload)
        {
        }


        /// <summary>
        /// Raises the OnNotAllowed event.
        /// </summary>
        /// <param name="permissionType">Type of the permission not allowed</param>
        public void RaiseOnNotAllowed(string permissionType)
        {
            if (OnNotAllowed != null)
            {
                OnNotAllowed(permissionType, this);
            }
        }


        /// <summary>
        /// Raises the OnCheckPermissions event, returns true when event was fired.
        /// </summary>
        /// <param name="permissionType">Type of the permission to check</param>
        /// <param name="sender">Sender admin control</param>
        public bool RaiseOnCheckPermissions(string permissionType, CMSAdminControl sender)
        {
            if (OnCheckPermissions != null)
            {
                OnCheckPermissions(permissionType, sender);
                return true;
            }

            return false;
        }


        /// <summary>
        /// Raises the OnCheckPermissions event, returns true when event was fired.
        /// </summary>
        /// <param name="permissionType">Type of the permission to check</param>
        /// <param name="modulePermissionType">Name of the module permission</param> 
        /// <param name="sender">Sender (CMSAdminControl)</param>
        public bool RaiseOnCheckPermissions(string permissionType, string modulePermissionType, CMSAdminControl sender)
        {
            if (OnCheckPermissionsExtended != null)
            {
                OnCheckPermissionsExtended(permissionType, modulePermissionType, sender);
                return true;
            }

            return false;
        }


        /// <summary>
        /// Check permissions for specified resource and permission type, return false is user doesn't have permissions.
        /// </summary>
        /// <param name="resourceName">Resource name</param>
        /// <param name="permissionType">Permission name</param>
        public bool CheckPermissions(string resourceName, string permissionType)
        {
            if (OnCheckPermissions != null)
            {
                OnCheckPermissions(permissionType, this);
                return !StopProcessing;
            }
            else
            {
                if (!MembershipContext.AuthenticatedUser.IsAuthorizedPerResource(resourceName, permissionType))
                {
                    AccessDenied(resourceName, permissionType);
                }
            }
            return true;
        }


        /// <summary>
        /// Check permissions for specified resource and permission type, return false is user doesn't have permissions.
        /// </summary>
        /// <param name="resourceName">Resource name</param>
        /// <param name="permissionType">Permission name</param>
        /// <param name="modulePermissionType">Name of the module permission</param>
        public bool CheckPermissions(string resourceName, string permissionType, string modulePermissionType)
        {
            if (OnCheckPermissionsExtended != null)
            {
                OnCheckPermissionsExtended(permissionType, modulePermissionType, this);
                return !StopProcessing;
            }
            else
            {
                if (OnCheckPermissions != null)
                {
                    OnCheckPermissions(modulePermissionType, this);
                    return !StopProcessing;
                }
                else
                {
                    if (!MembershipContext.AuthenticatedUser.IsAuthorizedPerResource(resourceName, modulePermissionType))
                    {
                        AccessDenied(resourceName, modulePermissionType);
                    }
                }
            }
            return true;
        }


        /// <summary>
        /// Check permissions for specified resource and permission type, return false is user doesn't have permissions.
        /// </summary>
        /// <param name="resourceName">Resource name</param>
        /// <param name="permissionType">Permission name</param>
        /// <param name="groupId">Group identifier</param>
        public bool CheckPermissions(string resourceName, string permissionType, int groupId)
        {
            if (OnCheckPermissions != null)
            {
                OnCheckPermissions(permissionType, this);
                return !StopProcessing;
            }
            else
            {
                if (!MembershipContext.AuthenticatedUser.IsGroupAdministrator(groupId))
                {
                    if (!MembershipContext.AuthenticatedUser.IsAuthorizedPerResource(resourceName, permissionType))
                    {
                        AccessDenied(resourceName, permissionType);
                    }
                }
            }
            return true;
        }

        #endregion


        #region "Access denied methods"

        /// <summary>
        /// Display error message for live site.
        /// </summary>
        /// <param name="title">Title of the error</param>
        /// <param name="message">Error message</param>
        private void DisplayErrorMessage(string title, string message)
        {
            try
            {
                // Load error control
                ErrorMessageControl errorControl = (ErrorMessageControl)LoadUserControl(ERROR_CONTROL_PATH);
                if (errorControl != null)
                {
                    // Hide all other controls
                    foreach (Control ctrl in Controls)
                    {
                        ctrl.Visible = false;
                    }

                    errorControl.ID = "errorMessageControl";
                    errorControl.ErrorTitle = HTMLHelper.HTMLEncode(title);
                    errorControl.ErrorMessage = HTMLHelper.HTMLEncode(message);

                    Controls.Add(errorControl);
                }
            }
            catch (Exception ex)
            {
                Label lblError = new Label();
                lblError.ForeColor = Color.Red;
                lblError.Text = "[CMSAdminControl.DisplayErrorMessage]: Error loading error control (" + ERROR_CONTROL_PATH + ").";

                // Try to log error
                try
                {
                    EventLogProvider.LogException("LoadControl", "ERRORLOAD", ex);
                }
                catch
                {
                }

                Controls.Add(lblError);
            }
        }


        /// <summary>
        /// Ensure access denied actions due to settings.
        /// </summary>
        /// <param name="message">Error message</param>
        public void AccessDenied(string message)
        {
            if (RedirectToAccessDeniedPage)
            {
                RedirectToAccessDenied(message);
            }
            else
            {
                // Display error message
                string title = ResHelper.GetString("CMSDesk.AccessDenied");
                DisplayErrorMessage(title, message);

                // Raise not allowed event
                RaiseOnNotAllowed(message);
            }
        }


        /// <summary>
        /// Ensure access denied actions due to settings.
        /// </summary>
        /// <param name="resourceName">Resource name that failed</param>
        /// <param name="permissionName">Permission name that failed</param>
        public void AccessDenied(string resourceName, string permissionName)
        {
            if (RedirectToAccessDeniedPage)
            {
                RedirectToAccessDenied(resourceName, permissionName);
            }
            else
            {
                // Display error message
                string title = null;
                string message = null;
                if (permissionName == null)
                {
                    title = String.Format(ResHelper.GetString("general.accessdeniedonresource"), resourceName);
                    message = ResHelper.GetString("CMSMessages.AccessDenied");
                }
                else
                {
                    title = String.Format(ResHelper.GetString("general.accessdeniedonpermissionname"), permissionName);
                    message = String.Format(ResHelper.GetString("CMSMessages.AccessDeniedResource"), resourceName);
                }
                DisplayErrorMessage(title, message);

                // Raise not allowed event
                RaiseOnNotAllowed(message);
            }
        }


        /// <summary>
        /// Ensure access denied actions due to settings.
        /// </summary>
        /// <param name="nodeId">Node ID that failed</param>
        /// <param name="permissionName">Permission name that failed</param>
        public void AccessDenied(int nodeId, string permissionName)
        {
            if (RedirectToAccessDeniedPage)
            {
                RedirectToAccessDenied(nodeId, permissionName);
            }
            else
            {
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);
                TreeNode node = tree.SelectSingleNode(nodeId);
                if (node != null)
                {
                    // Display error message
                    string title = String.Format(ResHelper.GetString("general.accessdeniedonnode"), node.GetDocumentName());
                    string message = ResHelper.GetString("CMSMessages.AccessDenied");
                    DisplayErrorMessage(title, message);

                    // Raise not allowed event
                    RaiseOnNotAllowed(message);
                }
            }
        }

        #endregion
    }
}