using System;
using System.Web.Mvc;

namespace Kentico.Web.Mvc
{
    /// <summary>
    /// Provides methods to render HTML fragments.
    /// </summary>
    public static class AjaxHelperExtensions
    {
        /// <summary>
        /// Represents support for rendering HTML in AJAX scenarios within a view.
        /// </summary>
        /// <param name="target">An instance of the <see cref="AjaxHelper"/> class.</param>
        /// <returns>An object that provides methods for rendering HTML in AJAX scenarios within a view..</returns>
        public static ExtensionPoint<AjaxHelper> Kentico(this AjaxHelper target)
        {
            return new ExtensionPoint<AjaxHelper>(target);
        }
    }
}
