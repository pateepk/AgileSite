using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.Core;
using CMS.DataEngine;
using CMS.Membership;
using CMS.SiteProvider;
using CMS.Modules;
using CMS.UIControls;

namespace CMS.Chat.Web.UI
{
    /// <summary>
    /// Base page for all Chat pages in CMS Desk Tools. 
    /// </summary>
    [CheckLicence(FeatureEnum.Chat)]
    [Security(Resource = ModuleName.CHAT, UIElements = "Chat")]
    [Security(Resource = ModuleName.CHAT, ResourceSite = true)]
    public abstract class CMSChatPage : CMSDeskPage
    {
        /// <summary>
        /// "Cache" for permissions check.
        /// </summary>
        private Dictionary<string, bool> permissionsCache = new Dictionary<string, bool>();


        /// <summary>
        /// True if user has read global permission for resource ResourceName on current site.
        /// </summary>
        protected bool ReadGlobalAllowed
        {
            get
            {
                return IsAuthorizedPerResource(CMSAdminControl.PERMISSION_GLOBALREAD);
            }
        }


        /// <summary>
        /// True if user has read permission for resource ResourceName on current site.
        /// </summary>
        protected bool ReadAllowed
        {
            get
            {
                return IsAuthorizedPerResource(CMSAdminControl.PERMISSION_READ);
            }
        }


        /// <summary>
        /// True if user has modify global permission for resource ResourceName on current site.
        /// </summary>
        protected bool ModifyGlobalAllowed
        {
            get
            {
                return IsAuthorizedPerResource(CMSAdminControl.PERMISSION_GLOBALMODIFY);
            }
        }


        /// <summary>
        /// True if user has modify permission for resource ResourceName on current site.
        /// </summary>
        protected bool ModifyAllowed
        {
            get
            {
                return IsAuthorizedPerResource(CMSAdminControl.PERMISSION_MODIFY);
            }
        }


        /// <summary>
        /// Resource name
        /// </summary>
        protected string ResourceName
        {
            get
            {
                return ModuleName.CHAT;
            }
        }


        /// <summary>
        /// Checks if user has modify permission based on siteID. If siteID is null, then modifyglobal permission is checked. 
        /// Otherwise modify is checked and siteID has to be current site.
        /// </summary>
        /// <param name="siteID">SiteID to check permission for.</param>
        public void CheckModifyPermission(int? siteID)
        {
            // Global
            if ((siteID == null) || (siteID <= 0))
            {
                CheckModifyGlobalPermission();
            }
            else
            {
                CheckModifyPermission();

                // If not global, only current site can be selected
                if (siteID.Value != SiteContext.CurrentSiteID)
                {
                    RedirectToInformation(GetString("chat.error.siteidisnotcurrentsite"));
                }
            }
        }


        /// <summary>
        /// Returns true if user has modify permission based on siteID (if siteID is null, then modifyglobal is checked, otherwise modify).
        /// </summary>
        /// <param name="siteID">Site ID to check permission for</param>
        public bool HasUserModifyPermission(int? siteID = null)
        {
            // Global
            if ((siteID == null) || (siteID <= 0))
            {
                return ModifyGlobalAllowed;
            }

            if (!ModifyAllowed || (siteID.Value != SiteContext.CurrentSiteID))
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// Checks any read permission (this is the only necessary requirement for viewing pages).
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            CheckDocPermissions = false;
            CheckAnyReadPermission();

            base.OnInit(e);
        }

      
        /// <summary>
        /// Checks if user has at least one read permission (read or readglobal).
        /// 
        /// Redirects to access denied if user does not have any permission.
        /// </summary>
        protected void CheckAnyReadPermission()
        {
            CheckAnyPermission(CMSAdminControl.PERMISSION_READ, CMSAdminControl.PERMISSION_GLOBALREAD);
        }


        /// <summary>
        /// Checks if user has read permission based on siteID. If siteID is null, then readglobal permission is checked. 
        /// Otherwise read is checked and siteID has to be current site.
        /// </summary>
        /// <param name="siteID">SiteID to check permission for.</param>
        protected void CheckReadPermission(int? siteID)
        {
            // Global
            if ((siteID == null) || (siteID <= 0))
            {
                CheckReadGlobalPermission();
            }
            else
            {
                CheckReadPermission();

                // If not global, only current site can be selected
                if (siteID.Value != SiteContext.CurrentSiteID)
                {
                    RedirectToInformation(GetString("chat.error.siteidisnotcurrentsite"));
                }
            }
        }
        
       
        /// <summary>
        /// Checks if user has permission passed in parameter for resource ResourceName.
        /// </summary>
        /// <param name="permissionName">Code name of the permission</param>
        /// <returns>True if user has permission; otherwise false</returns>
        private bool IsAuthorizedPerResource(string permissionName)
        {
            bool result;

            if (permissionsCache.TryGetValue(permissionName, out result))
            {
                return result;
            }

            return permissionsCache[permissionName] = MembershipContext.AuthenticatedUser.IsAuthorizedPerResource(ResourceName, permissionName);
        }


        /// <summary>
        /// Checks if user has at least one of the passed permission on current site.
        /// </summary>
        /// <param name="permissionNames">Array of permission names</param>
        private void CheckAnyPermission(params string[] permissionNames)
        {
            List<string> notAllowedPermissions = permissionNames.Where(permissionName => !IsAuthorizedPerResource(permissionName)).ToList();

            if (notAllowedPermissions.Count == permissionNames.Length)
            {
                RedirectToAccessDenied(ResourceName, notAllowedPermissions[0]);
            }
        }


        private void CheckModifyPermission()
        {
            CheckPermission(CMSAdminControl.PERMISSION_MODIFY);
        }


        private void CheckModifyGlobalPermission()
        {
            CheckPermission(CMSAdminControl.PERMISSION_GLOBALMODIFY);
        }


        private void CheckReadPermission()
        {
            CheckPermission(CMSAdminControl.PERMISSION_READ);
        }


        private void CheckReadGlobalPermission()
        {
            CheckPermission(CMSAdminControl.PERMISSION_GLOBALREAD);
        }


        /// <summary>
        /// Checks if user has permission passed in the parameter for resource ResourceName on current site.
        /// 
        /// User is redirected to access denied page if he doesn't have permission.
        /// </summary>
        /// <param name="permissionName">Code name of the permission</param>
        private void CheckPermission(string permissionName)
        {
            if (!IsAuthorizedPerResource(permissionName))
            {
                RedirectToAccessDenied(ResourceName, permissionName);
            }
        }

    }
}
