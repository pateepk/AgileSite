using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;

using Kentico.Web.Mvc;

namespace Kentico.Components.Web.Mvc.Dialogs
{
    /// <summary>
    /// Modal dialog methods for extension point <see cref="HtmlHelperExtensions.Kentico(HtmlHelper)"/>.
    /// </summary>
    public static class HtmlHelperModalDialogsExtensions
    {
        /// <summary>
        /// Renders necessary script tag for builder's modal dialogs.
        /// </summary>
        /// <param name="instance">HtmlHelper extension point.</param>
        /// <returns>Returns HTML string containing the script required by builder's modal dialogs.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="instance"/> is <c>null</c>.</exception>
        public static IHtmlString ModalDialogScript(this ExtensionPoint<HtmlHelper> instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            return Scripts.Render("~/Kentico/Scripts/modal-dialog.js");
        }
    }
}
