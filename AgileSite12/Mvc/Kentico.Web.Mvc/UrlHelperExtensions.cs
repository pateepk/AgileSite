using System;
using System.Web.Mvc;

namespace Kentico.Web.Mvc
{
    /// <summary>
    /// Provides methods to build URLs to Kentico content.
    /// </summary>
    public static class UrlHelperExtensions
    {
        /// <summary>
        /// Returns an object that provides methods to build URLs to Kentico content.
        /// </summary>
        /// <param name="target">An instance of the <see cref="UrlHelper"/> class.</param>
        /// <returns>An object that provides methods to build URLs to Kentico content.</returns>
        public static ExtensionPoint<UrlHelper> Kentico(this UrlHelper target)
        {
            return new ExtensionPoint<UrlHelper>(target);
        }
    }
}
