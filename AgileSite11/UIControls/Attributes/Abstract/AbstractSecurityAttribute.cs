using System;

using CMS.Base;
using CMS.DataEngine;
using CMS.Membership;
using CMS.Modules;
using CMS.SiteProvider;

namespace CMS.UIControls
{
    /// <summary>
    /// Abstract class for the security attributes.
    /// </summary>
    public abstract class AbstractSecurityAttribute : AbstractAttribute
    {
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
        /// If true, the resource assignment to the site should be checked.
        /// </summary>
        public bool ResourceSite
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
        /// UI elements to be checked separated by semicolon ;.
        /// </summary>
        public string UIElements
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the security check is allowed, if false, it automatically returns false.
        /// </summary>
        public bool IsAllowed
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

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor.
        /// </summary>
        public AbstractSecurityAttribute()
        {
            IsAllowed = true;
        }


        /// <summary>
        /// Checks the security of the UI elements, returns true if the security check succeeded.
        /// </summary>
        public bool CheckUIElements()
        {
            if (String.IsNullOrEmpty(UIElements))
            {
                return true;
            }

            string[] elems = UIElements.Split(';');

            // Check for the resource items
            return (String.IsNullOrEmpty(Resource) || (elems.Length == 0) || CurrentUser.IsAuthorizedPerUIElement(Resource, elems, CurrentSiteName));
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
            return (!GlobalAdministrator || CurrentUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin));
        }


        /// <summary>
        /// Checks the security of resource, returns true if the module is assigned to the current site.
        /// </summary>
        public bool CheckResourceSite()
        {
            return (String.IsNullOrEmpty(Resource) || !ResourceSite || ResourceSiteInfoProvider.IsResourceOnSite(Resource, CurrentSiteName));
        }


        /// <summary>
        /// Checks if resource is loaded, returns false if resource/module is separated.
        /// </summary>
        public bool CheckModuleLoaded()
        {
            return (String.IsNullOrEmpty(Resource) || ModuleManager.IsModuleLoaded(Resource));
        }


        /// <summary>
        /// Checks the security, returns true if the security check succeeded.
        /// </summary>
        public bool CheckSecurity()
        {
            // Check the global flag
            if (!IsAllowed)
            {
                return false;
            }

            // Check separabilty
            if (!CheckModuleLoaded())
            {
                return false;
            }

            // If global admin check done and passed, automatically return true
            if (GlobalAdministrator && CheckGlobalAdministrator())
            {
                return true;
            }

            return CheckResourceSite() && CheckEditor() && CheckPermissions() && CheckUIElements();
        }

        #endregion
    }
}