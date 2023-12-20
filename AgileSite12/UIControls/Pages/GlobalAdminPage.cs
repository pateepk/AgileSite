using System;

namespace CMS.UIControls
{
    /// <summary>
    /// Base page class for the pages which should be available for global administrators only.
    /// </summary>
    [Security(GlobalAdministrator = true)]
    public abstract class GlobalAdminPage : CMSPage
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public GlobalAdminPage()
        {
            Load += BasePage_Load;
            PreInit += GlobalAdminPage_PreInit;
        }


        private void GlobalAdminPage_PreInit(object sender, EventArgs e)
        {
            CheckAdministrationInterface();
        }


        /// <summary>
        /// Page load event
        /// </summary>
        protected void BasePage_Load(object sender, EventArgs e)
        {
            RedirectToSecured();
            SetRTL();
            SetBrowserClass();
            AddNoCacheTag();

            CheckGlobalAdministrator();
        }
    }
}