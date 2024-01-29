using System;

using CMS.Base.Web.UI;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.PortalEngine.Web.UI;
using CMS.PortalEngine;

namespace CMS.UIControls
{
    /// <summary>
    /// Base class for administration master pages.
    /// </summary>
    public abstract class CMSMasterPage : AbstractMasterPage, ICMSMasterPage
    {
        #region "Public methods"

        /// <summary>
        /// Sets the RTL culture to the body class if RTL language.
        /// </summary>
        public void SetRTL()
        {
            if (CultureHelper.IsUICultureRTL())
            {
                BodyClass += " RTL";
                BodyClass = BodyClass.Trim();
            }
        }


        /// <summary>
        /// Sets the browser class to the body class.
        /// </summary>
        public void SetBrowserClass()
        {
            BodyClass = EnsureBodyClass(BodyClass);
        }


        /// <summary>
        /// Hide page title if page is used in tab mode.
        /// </summary>
        public void HidePageTitle()
        {
            if (TabMode)
            {
                if (Title != null)
                {
                    Title.Visible = false;
                }
            }
        }


        /// <summary>
        /// Init event handler.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            SetRTL();
            SetBrowserClass();
            HidePageTitle();
        }


        /// <summary>
        /// PreRender event handler.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            PortalScriptHelper.RegisterBootstrapScript(PortalContext.ViewMode, Page);
        }

        #endregion


        #region "Static methods"

        /// <summary>
        /// Sets the browser class to the body class.
        /// </summary>
        /// <param name="bodyClass">The body class.</param>
        /// <param name="generateCultureClass">if set to true generate culture class.</param>
        internal static string EnsureBodyClass(string bodyClass, bool generateCultureClass = true)
        {
            // Add browser type
            string browserClass = BrowserHelper.GetBrowserClass();
            if (!String.IsNullOrEmpty(browserClass))
            {
                bodyClass = string.Format("{0} {1}", bodyClass, browserClass).Trim();
            }

            if (generateCultureClass)
            {
                // Add culture type
                string cultureClass = DocumentContext.GetUICultureClass();
                if (!String.IsNullOrEmpty(cultureClass))
                {
                    bodyClass = string.Format("{0} {1}", bodyClass, cultureClass).Trim();
                }
            }
            // Add bootstrap
            PortalUIHelper.EnsureBootstrapBodyClass(ref bodyClass, PortalContext.ViewMode, PageContext.CurrentPage);

            return bodyClass;
        }

        #endregion
    }
}
