using System;
using System.Web.Mvc;

namespace Kentico.Web.Mvc
{
    /// <summary>
    /// Provides methods to render HTML fragments.
    /// </summary>
    public static class HtmlHelperExtensions
    {
        /// <summary>
        /// Returns an object that provides methods to render HTML fragments.
        /// </summary>
        /// <param name="target">An instance of the <see cref="System.Web.Mvc.HtmlHelper"/> class.</param>
        /// <returns>An object that provides methods to render HTML fragments.</returns>
        public static ExtensionPoint<HtmlHelper> Kentico(this HtmlHelper target)
        {
            return new ExtensionPoint<HtmlHelper>(target);
        }
    }
}
