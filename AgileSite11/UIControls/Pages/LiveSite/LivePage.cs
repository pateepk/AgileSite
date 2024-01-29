using System;

namespace CMS.UIControls
{
    /// <summary>
    /// Base page for the live site pages to apply global settings to the pages.
    /// </summary>
    public abstract class LivePage : CMSPage
    {
        /// <summary>
        /// Constructor
        /// </summary>
        protected LivePage()
        {

            RegisterPageLoadedScript = false;

            Load += BasePage_Load;
            PreInit += LivePage_PreInit;
        }


        /// <summary>
        /// PreInit event handler
        /// </summary>
        private void LivePage_PreInit(object sender, EventArgs e)
        {
            SetLiveCulture();
        }


        /// <summary>
        /// PageLoad event handle
        /// </summary>
        protected void BasePage_Load(object sender, EventArgs e)
        {
            SetLiveRTL();
            SetBrowserClass();
            AddNoCacheTag();
        }
    }
}