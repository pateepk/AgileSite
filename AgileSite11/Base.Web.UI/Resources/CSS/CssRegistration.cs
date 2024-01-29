using System;
using System.Web.UI;

using CMS.Helpers;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Provides methods for CSS stylesheet registration.
    /// </summary>
    public class CssRegistration
    {
        private const string cssBlockPrefix = "CSSBlock_";


        /// <summary>
        /// Adds a style block containing specified CSS rules to the page's header. If registering the same block multiple times then a given block is registered only once.
        /// </summary>
        /// <param name="page">The page that is registering the CSS style block</param>
        /// <param name="key">Unique key of the style block</param>
        /// <param name="style">Style declaration that contains CSS rules</param>
        public static void RegisterCssBlock(Page page, string key, string style)
        {
            if (String.IsNullOrEmpty(style))
            {
                return;
            }

            // Register CSS block only if it hasn't been already registered
            if (WebLanguagesContext.CurrentCSSBlocks.ContainsKey(key))
            {
                return;
            }

            if (RequestHelper.IsAsyncPostback())
            {
                // Register function to register CSS
                const string addCssBlock = @"function AddCSSBlock(block){try{var s=document.createElement('style'),r=document.createTextNode(block);s.setAttribute('type','text/css');if(s.styleSheet){s.styleSheet.cssText=r.nodeValue;}else{s.appendChild(r);}document.getElementsByTagName('head')[0].appendChild(s);}catch(e){}}";
                ScriptHelper.RegisterStartupScript(page, page.GetType(), "LinkCSSBlock", addCssBlock, true);

                // Register the CSS itself
                string addCssLinkScript = string.Format("AddCSSBlock({0});", ScriptHelper.GetString(style));
                ScriptHelper.RegisterStartupScript(page, page.GetType(), cssBlockPrefix + key, addCssLinkScript, true);
            }
            else
            {
                page.AddToHeader(CssHelper.GetStyle(style));
            }

            WebLanguagesContext.CurrentCSSBlocks.Add(key, null);
        }


        /// <summary>
        /// Adds a link to the custom theme CSS stylesheet to the page header. Supports asynchronous requests.
        /// </summary>
        /// <param name="page">The page that is registering the CSS link</param>
        /// <param name="theme">Stylesheet theme</param>
        /// <param name="fileName">Name of the stylesheet file</param>
        public static bool RegisterCssLink(Page page, string theme, string fileName)
        {
            // Try to get the URL
            string url = CssLinkHelper.GetThemeCssUrl(theme, fileName);
            if (String.IsNullOrEmpty(url))
            {
                return false;
            }

            // Register the link
            url = CssLinkHelper.GetPhysicalCssUrl(url);
            return AddExternalCssFromListToPageHeader(page, url);
        }


        /// <summary>
        /// Adds a link to the external CSS stylesheet to the page header. Supports asynchronous requests.
        /// </summary>
        /// <param name="page">The page that is registering the CSS link</param>
        /// <param name="url">URL of the external stylesheet</param>        
        public static bool RegisterCssLink(Page page, string url)
        {
            if ((page == null) || string.IsNullOrEmpty(url))
            {
                return false;
            }

            return AddExternalCssToPageHeader(page, CssLinkHelper.GetPhysicalCssUrl(url));
        }


        /// <summary>
        /// Registers the design mode CSS.
        /// </summary>
        /// <param name="page">Page</param>
        public static void RegisterDesignMode(Page page)
        {
            if (page != null)
            {
                RegisterCssLink(page, page.Theme, "DesignMode.css");
            }
        }


        /// <summary>
        /// Registers the bootstrap CSS.
        /// </summary>
        /// <param name="page">Page</param>
        public static void RegisterBootstrap(Page page)
        {
            if (page != null)
            {
                RegisterCssLink(page, page.Theme, "bootstrap.css");
            }
        }


        /// <summary>
        /// Registers the widgets mode CSS.
        /// </summary>
        /// <param name="page">Page</param>
        public static void RegisterWidgetsMode(Page page)
        {
            if (page != null)
            {
                // Register the main widgets theme
                if (!RegisterCssLink(page, page.Theme, "Widgets.css"))
                {
                    RegisterCssLink(page, "Design", "Widgets.css");
                }

                // Development mode support
                if (SystemContext.DevelopmentMode)
                {
                    RegisterCssLink(page, "Test/" + SystemContext.MachineName, "Widgets.css");
                }
            }
        }


        /// <summary>
        /// Adds a link to the external CSS stylesheet to the page header. Supports asynchronous requests.
        /// </summary>
        /// <param name="page">The page that is registering the CSS link</param>
        /// <param name="url">URL of the external stylesheet</param>
        internal static bool AddExternalCssToPageHeader(Page page, string url)
        {
            if (String.IsNullOrEmpty(url))
            {
                return false;
            }

            // Register only when not already present
            url = UrlResolver.ResolveUrl(url);
            string key = CssLinkHelper.CSS_LINK_PREFIX + url;

            if (WebLanguagesContext.CurrentCSSLinks[key] != null)
            {
                return true;
            }

            if (RequestHelper.IsAsyncPostback())
            {
                // Register function to register CSS
                const string addCssLink = @"
function AddCSSLink(url) {
    var links = document.getElementsByTagName('link');
    for (var i = 0; i < links.length; i++) {
        var l = links[i];
        if (l && l.attributes['href'] && (l.attributes['href'].value == url)) {
            return;
        }
    }    
    var link = document.createElement('link');
    link.href = url; link.rel = 'stylesheet'; link.type = 'text/css';
    document.getElementsByTagName('head')[0].appendChild(link);
}";

                ScriptHelper.RegisterClientScriptBlock(page, page.GetType(), "LinkCSS", addCssLink, true);

                // Register the CSS itself
                string addCssLinkScript = string.Format("AddCSSLink('{0}');", url);
                ScriptHelper.RegisterClientScriptBlock(page, page.GetType(), key, addCssLinkScript, true);
            }
            else
            {
                // Create a link element which references external stylesheet
                string tag = CssLinkHelper.GetCssFileLink(url);

                // Add the link to the page header
                page.AddToHeader(tag);
            }

            WebLanguagesContext.CurrentCSSLinks[key] = true;

            return true;
        }


        /// <summary>
        /// Adds a link to the external CSS stylesheet to the page header. Supports asynchronous requests.
        /// </summary>
        /// <param name="page">The page that is registering the CSS link</param>
        /// <param name="urls">URL of the external stylesheets</param>
        private static bool AddExternalCssFromListToPageHeader(Page page, params string[] urls)
        {
            bool result = false;

            // Register all links
            foreach (string url in urls)
            {
                result = result || AddExternalCssToPageHeader(page, url);
            }

            return result;
        }
    }
}