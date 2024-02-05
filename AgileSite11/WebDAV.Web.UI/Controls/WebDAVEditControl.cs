using CMS.Base.Web.UI;
using CMS.Helpers;
using CMS.SiteProvider;
using CMS.UIControls;

namespace CMS.WebDAV.Web.UI
{
    /// <summary>
    /// Base control for WebDAV editing of files
    /// </summary>
    public abstract class WebDAVEditControl : ExternalEditControl
    {
        /// <summary>
        /// Checks the enabled status of the control
        /// </summary>
        protected override void CheckEnable()
        {
            base.CheckEnable();

            if (Enabled)
            {
                // Register the script
                ScriptHelper.RegisterScriptFile(Page, WebDAVWebUIModule.WEBDAV_JS);
            }
        }


        /// <summary>
        /// Gets the editing action script
        /// </summary>
        protected override string GetEditScript()
        {
            // Prepare javascript parameters
            string errorMessageOpenDocument = ScriptHelper.GetString(ResHelper.GetString("webdav.sharepoint.opendocuments.error"));
            string errorMessageEditDocument = ScriptHelper.GetString(ResHelper.GetString("webdav.editdocument.error"));

            // Get domain name for other site than current
            string domainName = null;
            if (SiteName != SiteContext.CurrentSiteName)
            {
                domainName = (SiteInfo != null) ? SiteInfo.DomainName : null;
            }

            // Get absolute URL
            string absoluteURL = ScriptHelper.GetString(URLHelper.GetAbsoluteUrl(GetUrl(), domainName));

            // Prepare 'onclick' script
            return "editDocumentWithProgID(" + absoluteURL + ", " + errorMessageOpenDocument + ", " + errorMessageEditDocument + "); ";
        }
    }
}
