using CMS.DataEngine;

namespace Kentico.Content.Web.Mvc
{
    /// <summary>
    /// Provides interface for checking page permissions for Page builder feature.
    /// </summary>
    internal interface IPageSecurityChecker
    {
        /// <summary>
        /// Checks page permissions.
        /// </summary>
        /// <param name="permission">Permission to check.</param>
        /// <returns><c>True</c> if current user is granted with given permissions.</returns>
        bool Check(PermissionsEnum permission);
    }
}