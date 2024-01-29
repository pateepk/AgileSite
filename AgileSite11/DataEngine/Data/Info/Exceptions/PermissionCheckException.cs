using System;

namespace CMS.DataEngine
{
    /// <summary>
    /// Base class for the exceptions raised in permissions check.
    /// </summary>
    public class PermissionCheckException : Exception
    {
        /// <summary>
        /// Name of the module the permission of which failed.
        /// </summary>
        public string ModuleName
        {
            get;
            set;
        }


        /// <summary>
        /// Name of the permission failed.
        /// </summary>
        public string PermissionFailed
        {
            get;
            set;
        }


        /// <summary>
        /// Name of the site where the permission failed.
        /// </summary>
        public string SiteName
        {
            get;
            set;
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="moduleName">Name of the module the permission of which failed</param>
        /// <param name="permissionName">Name of the permission failed</param>
        /// <param name="siteName">Name of the site where the permission failed</param>
        public PermissionCheckException(string moduleName, string permissionName, string siteName)
        {
            ModuleName = moduleName;
            PermissionFailed = permissionName;
            SiteName = siteName;
        }
    }
}