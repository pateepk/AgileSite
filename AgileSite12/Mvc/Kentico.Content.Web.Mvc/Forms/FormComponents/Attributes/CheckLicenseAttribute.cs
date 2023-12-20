using System;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Used to perform license checks, so that it is possible to hide features that are not available in the current license.
    /// </summary>
    internal abstract class CheckLicenseAttribute : Attribute
    {
        /// <summary>
        /// Performs license check.
        /// </summary>
        public abstract bool HasValidLicense();
    }
}
