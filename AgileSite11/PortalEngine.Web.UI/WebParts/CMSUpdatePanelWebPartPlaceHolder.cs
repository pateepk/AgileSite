using System;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using CMS.Helpers;
using CMS.IO;

namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// Placeholder used for update panel
    /// </summary>
    internal class CMSUpdatePanelWebPartPlaceHolder : PlaceHolder, INamingContainer
    {
        /// <summary>
        /// Web part instance
        /// </summary>
        public CMSAbstractWebPart WebPart
        {
            get;
            set;
        }


        /// <summary>
        /// Render
        /// </summary>
        protected override void Render(HtmlTextWriter writer)
        {
            if (WebPart != null)
            {
                bool outputFilter = WebPart.EnableOutputFilter;
                bool resolveMacros = WebPart.ContextResolver.SkippedResolver(CMSAbstractWebPart.RESOLVER_RENDER);

                if (outputFilter || resolveMacros)
                {
                    StringBuilder sb = new StringBuilder();
                    StringWriter tw = new StringWriter(sb);
                    HtmlTextWriter hw = new HtmlTextWriter(tw);

                    base.Render(hw);

                    // Render through the string
                    string code = sb.ToString();

                    // Fix the HTML code
                    if (outputFilter)
                    {
                        FixXHTMLSettings settings = new FixXHTMLSettings()
                        {
                            Attributes = WebPart.OutputFixAttributes,
                            HTML5 = WebPart.OutputFixHTML5,
                            Javascript = WebPart.OutputFixJavascript,
                            LowerCase = WebPart.OutputFixLowerCase,
                            ResolveUrl = WebPart.OutputResolveURLs,
                            SelfClose = WebPart.OutputFixSelfClose,
                            TableToDiv = WebPart.OutputConvertTablesToDivs
                        };

                        code = HTMLHelper.FixXHTML(code, settings, null);
                    }

                    // Resolve the remaining macros by Render resolver
                    if (resolveMacros)
                    {
                        WebPart.ContextResolver.ResolverName = CMSAbstractWebPart.RESOLVER_RENDER;
                        code = WebPart.ContextResolver.ResolveMacros(code);
                    }

                    writer.Write(code);
                    return;
                }
            }

            base.Render(writer);
        }

    }
}
