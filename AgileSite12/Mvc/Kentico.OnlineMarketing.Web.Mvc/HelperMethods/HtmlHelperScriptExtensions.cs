using System;
using System.Text;
using System.Web;
using System.Web.Mvc;

using Kentico.Web.Mvc;

namespace Kentico.OnlineMarketing.Web.Mvc
{
    internal static class HtmlHelperScriptExtensions
    {
        /// <summary>
        /// Renders script tag for each input source.
        /// </summary>
        /// <param name="instance">HtmlHelper extension point.</param>
        /// <param name="scriptSources">Script sources.</param>        
        internal static IHtmlString RenderScriptsTag(this ExtensionPoint<HtmlHelper> instance, params string[] scriptSources)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            var stringBuilder = new StringBuilder();

            foreach (var scriptSource in scriptSources)
            {
                var scriptUrl = HttpUtility.UrlPathEncode(scriptSource);
                stringBuilder.Append(string.Format(@"<script type=""text/javascript"" src=""{0}"" async></script>", scriptUrl));                
            }
            
            return new HtmlString(stringBuilder.ToString());
        }
    }
}
