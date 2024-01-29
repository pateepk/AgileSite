using System;
using System.ComponentModel;

using CMS.Base;
using CMS.Membership;
using CMS.SiteProvider;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Security check for the UI form.
    /// </summary>
    public class SecurityCheck : Component
    {
        #region "Variables"

        /// <summary>
        /// Result of the security check.
        /// </summary>
        protected bool? mIsAllowed = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// If true, the global admin check should be performed.
        /// </summary>
        public bool GlobalAdministrator
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the editor should be checked.
        /// </summary>
        public bool Editor
        {
            get;
            set;
        }


        /// <summary>
        /// Resource name to be checked.
        /// </summary>
        public string Resource
        {
            get;
            set;
        }


        /// <summary>
        /// Permission name to be checked.
        /// </summary>
        public string Permission
        {
            get;
            set;
        }


        /// <summary>
        /// UI elements to be checked.
        /// </summary>
        public string[] UIElements
        {
            get;
            set;
        }


        /// <summary>
        /// Current user.
        /// </summary>
        protected CurrentUserInfo CurrentUser
        {
            get
            {
                return MembershipContext.AuthenticatedUser;
            }
        }


        /// <summary>
        /// Current site name.
        /// </summary>
        protected string CurrentSiteName
        {
            get
            {
                return SiteContext.CurrentSiteName;
            }
        }


        /// <summary>
        /// Result of the security check.
        /// </summary>
        public bool IsAllowed
        {
            get
            {
                if (mIsAllowed == null)
                {
                    mIsAllowed = CheckSecurity();
                }

                return mIsAllowed.Value;
            }
            set
            {
                mIsAllowed = value;
            }
        }


        /// <summary>
        /// If true, the form is disabled if security check does not succeed.
        /// </summary>
        public bool DisableForm
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Checks the security of the UI elements, returns true if the security check succeeded.
        /// </summary>
        public bool CheckUIElements()
        {
            // Check for the resource items
            return (String.IsNullOrEmpty(Resource) || (UIElements == null) || CurrentUser.IsAuthorizedPerUIElement(Resource, UIElements, CurrentSiteName));
        }


        /// <summary>
        /// Checks the security of the permissions, returns true if the security check succeeded.
        /// </summary>
        public bool CheckPermissions()
        {
            // Check for the resource items
            return (String.IsNullOrEmpty(Resource) || String.IsNullOrEmpty(Permission) || CurrentUser.IsAuthorizedPerResource(Resource, Permission, CurrentSiteName));
        }


        /// <summary>
        /// Checks the security of the editor, returns true if the security check succeeded.
        /// </summary>
        public bool CheckEditor()
        {
            // Check for the editor
            return (!Editor || CurrentUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Editor, CurrentSiteName));
        }


        /// <summary>
        /// Checks the security of the global admin, returns true if the security check succeeded.
        /// </summary>
        public bool CheckGlobalAdministrator()
        {
            return (!GlobalAdministrator || CurrentUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.GlobalAdmin));
        }


        /// <summary>
        /// Checks the security, returns true if the security check succeeded.
        /// </summary>
        public bool CheckSecurity()
        {
            // If global admin check done and passed, automatically return true
            if (GlobalAdministrator && CheckGlobalAdministrator())
            {
                return true;
            }

            return (CheckEditor() && CheckPermissions() && CheckUIElements());
        }

        #endregion
    }
}