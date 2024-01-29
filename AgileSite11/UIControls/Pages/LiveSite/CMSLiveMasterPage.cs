using System;

using CMS.Base.Web.UI;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.PortalEngine.Web.UI;
using CMS.PortalEngine;

namespace CMS.UIControls
{
    /// <summary>
    /// Base class for live site master pages.
    /// </summary>
    public abstract class CMSLiveMasterPage : AbstractMasterPage, ICMSMasterPage
    {
        #region "Public methods"

        /// <summary>
        /// Sets the RTL culture to the body class if RTL language.
        /// </summary>
        public void SetRTL()
        {
            if (CultureHelper.IsPreferredCultureRTL())
            {
                BodyClass += " RTL";
                BodyClass = BodyClass.Trim();
            }
        }


        /// <summary>
        /// Sets the dialog body class.
        /// </summary>
        public void SetDialogClass()
        {
            if (!BodyClass.Contains("LiveSiteDialog"))
            {
                BodyClass += " LiveSiteDialog";
                BodyClass = BodyClass.Trim();
            }
        }


        /// <summary>
        /// Sets the browser class to the body class.
        /// </summary>
        public void SetBrowserClass()
        {
            // Add browser type
            string browserClass = BrowserHelper.GetBrowserClass();
            if (!string.IsNullOrEmpty(browserClass))
            {
                BodyClass += " " + browserClass;
                BodyClass = BodyClass.Trim();
            }

            // Add culture type
            string cultureClass = DocumentContext.GetCultureClass();
            if (!string.IsNullOrEmpty(cultureClass))
            {
                BodyClass += " " + cultureClass;
                BodyClass = BodyClass.Trim();
            }

            // Add bootstrap wrapping class if it is nesscessary
            string bodyClass = BodyClass;
            PortalUIHelper.EnsureBootstrapBodyClass(ref bodyClass, PortalContext.ViewMode, PageContext.CurrentPage);
            BodyClass = bodyClass;
        }


        /// <summary>
        /// Init event handler.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            SetRTL();
            SetBrowserClass();
            SetDialogClass();
        }

        #endregion
    }
}
