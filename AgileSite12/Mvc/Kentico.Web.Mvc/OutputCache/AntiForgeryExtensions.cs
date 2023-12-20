using System.Web;
using System.Web.Mvc;

namespace Kentico.Web.Mvc
{
    /// <summary>
    /// Extension methods for AntiForgery support for pages with output cache.
    /// </summary>
    public static class AntiForgeryExtensions
    {
        /// <summary>
        /// Generates a hidden form field (anti-forgery token) that is validated when the form is submitted.
        /// </summary>
        /// <param name="htmlHelper"><see cref="HtmlHelper"/> instance.</param>
        /// <remarks>Ensures that AntiForgery token is correctly generated on pages with output cache.</remarks>
        public static IHtmlString AntiForgeryToken(this ExtensionPoint<HtmlHelper> htmlHelper)
        {
            var helper = AntiForgeryHelper.Instance;
            helper.EnsureResponeTokens(isCachedRequest: false);
            return MvcHtmlString.Create(AntiForgeryHelper.INPUT_PLACEHOLDER);
        }
    }
}
