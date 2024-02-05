using System;
using System.Linq;
using System.Web.UI;

using CMS.Base.Web.UI;
using CMS.Helpers;

namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// Class for managing portal scripts.
    /// </summary>
    public class PortalScriptHelper
    {
        /// <summary>
        /// Register application storage script. The current application is stored in browser storage for later use.
        /// </summary>
        /// <param name="page">Current page</param>
        public static void RegisterApplicationStorageScript(Page page)
        {
            const string SCRIPT = @"
if (typeof (Storage) !== 'undefined' && window.location.hash) {
    sessionStorage.cmsLatestApp = window.location.hash.substring(1);
}
";
            ScriptHelper.RegisterClientScriptBlock(page, typeof(string), "RegisterApplicationStorageScript", SCRIPT, true);
        }


        /// <summary>
        /// Registers reload script for administration interface with possibility to specify target URL
        /// </summary>
        /// <param name="page">Current page</param>
        /// <param name="url">Required target URL (optional)</param>
        public static void RegisterAdminRedirectScript(Page page, string url = null)
        {
            string script;

            // Reload current
            if (String.IsNullOrEmpty(url))
            {
                script = "parent.location.reload();";
            }
            // Specific URL
            else
            {
                script = "var adminRedirectUrl = " + ScriptHelper.GetString(url) + ";";

                // Workaround for safari bug
                if (BrowserHelper.IsSafari())
                {
                    Guid guid = Guid.NewGuid();
                    url = URLHelper.UpdateParameterInUrl(url, "appId", guid.ToString());
                    script = "var adminRedirectUrl = " + ScriptHelper.GetString(url) + ";";
                    script += "adminRedirectUrl = adminRedirectUrl.replace('" + guid + "', parent.location.hash.substring(1));";
                }

                script += "parent.location = adminRedirectUrl + parent.location.hash;";
            }

            ScriptHelper.RegisterStartupScript(page, typeof(Page), "RegisterAdminRedirectScript", ScriptHelper.GetScript(script));
        }


        /// <summary>
        /// Ensures the bootstrap JavaScript files for specified page if it is allowed for view mode.
        /// </summary>
        /// <param name="viewMode">The view mode.</param>
        /// <param name="page">The current page.</param>
        public static void RegisterBootstrapScript(ViewModeEnum viewMode, Page page)
        {
            // Skip addition for live sites
            if ((viewMode == ViewModeEnum.UI) || (page is IAdminPage))
            {
                // Register Bootstrap JavaScript
                ScriptHelper.RegisterBootstrapScripts(page);
            }
        }
    }
}
