using System;

namespace CMS.UIControls
{
    /// <summary>
    /// Checks specified permissions and UI elements for given resource and performs redirect if checks fails. Works only with pages inherited from <see cref="CMSPage"/> class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class SecurityAttribute : AbstractSecurityAttribute, ICMSSecurityAttribute
    {
        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public SecurityAttribute()
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="resource">Resource name to check</param>
        public SecurityAttribute(string resource)
        {
            Resource = resource;
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="resource">Resource name to check</param>
        /// <param name="permission">Permission name to check</param>
        /// <param name="uiElements">UI elements to check separated by ';'</param>
        public SecurityAttribute(string resource, string permission, string uiElements)
        {
            Resource = resource;
            Permission = permission;
            UIElements = uiElements;
        }

        #endregion


        #region "Methods"
        
        /// <summary>
        /// Does the security check and performs redirect if user has insufficient permissions.
        /// </summary>
        /// <param name="page">Page from which is check performed</param>
        public void Check(CMSPage page)
        {
            if (GlobalAdministrator && page.CheckGlobalAdministrator())
            {
                return;
            }

            // Check resource
            if (ResourceSite)
            {
                page.CheckResourceSite(Resource, page.CurrentSiteName);
            }

            // Check permissions
            if (!string.IsNullOrEmpty(Permission))
            {
                page.CheckPermissions(Resource, Permission);
            }

            // Check UI elements
            if (!String.IsNullOrEmpty(UIElements))
            {
                page.CheckUIElements(Resource, UIElements.Split(';'));
            }

            page.CheckEditor();
        }
        
        #endregion
    }
}