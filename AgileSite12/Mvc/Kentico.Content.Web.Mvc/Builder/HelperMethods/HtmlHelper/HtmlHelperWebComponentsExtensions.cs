using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;

using CMS.Base;
using CMS.Helpers;

using Kentico.Web.Mvc;

namespace Kentico.Builder.Web.Mvc.Internal
{
    /// <summary>
    /// Web components methods for extension point <see cref="HtmlHelperExtensions.Kentico(HtmlHelper)"/>.
    /// </summary>
    public static class HtmlHelperWebComponentsExtensions
    {
        private const string WEB_COMPONENTS_SCRIPT_FILE_NAME = "components.js";


        /// <summary>
        /// Renders necessary script tag for web components.
        /// </summary>
        /// <param name="instance">HtmlHelper extension point.</param>
        /// <returns>Returns HTML string containing the script tag required to use web components.</returns>
        /// <remarks>Usable for iframes where is not included builder script.</remarks>
        /// <exception cref="ArgumentNullException"><paramref name="instance"/> is <c>null</c>.</exception>
        public static IHtmlString WebComponentsScript(this ExtensionPoint<HtmlHelper> instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            var componentsScriptPath = $"~/Kentico/Scripts/builders/{WEB_COMPONENTS_SCRIPT_FILE_NAME}";

#if DEBUG
            var debugBuilderScripts = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSDebugBuilderScripts"], false);

            // Use script provided by the Webpack DevServer
            if (debugBuilderScripts)
            {
                componentsScriptPath = $"http://localhost:3000/dist/{WEB_COMPONENTS_SCRIPT_FILE_NAME}";
            }
#endif
            return Scripts.Render(componentsScriptPath);
        }

        
        /// <summary>
        /// Renders necessary style tag for web components.
        /// </summary>
        /// <param name="instance">HtmlHelper extension point.</param>
        /// <returns>Returns HTML string containing the style tag required to use web components.</returns>
        /// <remarks>Usable for iframes where is not included builder styles.</remarks>
        /// <exception cref="ArgumentNullException"><paramref name="instance"/> is <c>null</c>.</exception>
        public static IHtmlString WebComponentsStyle(this ExtensionPoint<HtmlHelper> instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            return Styles.Render("~/Kentico/Scripts/builders/web-components.css");
        }
    }
}